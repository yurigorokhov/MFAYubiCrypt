
// AsyncSampleDlg.cpp : implementation file
//

#include "stdafx.h"
#include "AsyncSample.h"
#include "AsyncSampleDlg.h"

#ifdef _DEBUG
#define new DEBUG_NEW
#endif

// CAboutDlg dialog used for App About

class CAboutDlg : public CDialog
{
public:
	CAboutDlg();

// Dialog Data
	enum { IDD = IDD_ABOUTBOX };

	protected:
	virtual void DoDataExchange(CDataExchange* pDX);    // DDX/DDV support

// Implementation
protected:
	DECLARE_MESSAGE_MAP()
};

CAboutDlg::CAboutDlg() : CDialog(CAboutDlg::IDD)
{
}

void CAboutDlg::DoDataExchange(CDataExchange* pDX)
{
	CDialog::DoDataExchange(pDX);
}

BEGIN_MESSAGE_MAP(CAboutDlg, CDialog)
END_MESSAGE_MAP()


// CAsyncSampleDlg dialog




CAsyncSampleDlg::CAsyncSampleDlg(CWnd* pParent /*=NULL*/)
	: CDialog(CAsyncSampleDlg::IDD, pParent)
{
	m_hIcon = AfxGetApp()->LoadIcon(IDR_MAINFRAME);

    m_pending = false;
}

void CAsyncSampleDlg::DoDataExchange(CDataExchange* pDX)
{
    CDialog::DoDataExchange(pDX);
    DDX_Control(pDX, IDC_PROGRESS, m_prog);
}

BEGIN_MESSAGE_MAP(CAsyncSampleDlg, CDialog)
	ON_WM_SYSCOMMAND()
	ON_WM_PAINT()
	ON_WM_QUERYDRAGICON()
	//}}AFX_MSG_MAP
    ON_BN_CLICKED(IDOK, &CAsyncSampleDlg::OnBnClickedOk)
    ON_BN_CLICKED(IDC_HMAC, &CAsyncSampleDlg::OnBnClickedHmac)
    ON_WM_TIMER()
END_MESSAGE_MAP()


// CAsyncSampleDlg message handlers

BOOL CAsyncSampleDlg::OnInitDialog()
{
	CDialog::OnInitDialog();

	// Add "About..." menu item to system menu.

	// IDM_ABOUTBOX must be in the system command range.
	ASSERT((IDM_ABOUTBOX & 0xFFF0) == IDM_ABOUTBOX);
	ASSERT(IDM_ABOUTBOX < 0xF000);

	CMenu* pSysMenu = GetSystemMenu(FALSE);
	if (pSysMenu != NULL)
	{
		BOOL bNameValid;
		CString strAboutMenu;
		bNameValid = strAboutMenu.LoadString(IDS_ABOUTBOX);
		ASSERT(bNameValid);
		if (!strAboutMenu.IsEmpty())
		{
			pSysMenu->AppendMenu(MF_SEPARATOR);
			pSysMenu->AppendMenu(MF_STRING, IDM_ABOUTBOX, strAboutMenu);
		}
	}

	// Set the icon for this dialog.  The framework does this automatically
	//  when the application's main window is not a dialog
	SetIcon(m_hIcon, TRUE);			// Set big icon
	SetIcon(m_hIcon, FALSE);		// Set small icon

	// TODO: Add extra initialization here

    m_prog.SetRange(0, 15);

    // Setup a timer to poll the key every 250 ms

    SetTimer(1, 250, 0);

	return TRUE;  // return TRUE  unless you set the focus to a control
}

void CAsyncSampleDlg::OnSysCommand(UINT nID, LPARAM lParam)
{
	if ((nID & 0xFFF0) == IDM_ABOUTBOX)
	{
		CAboutDlg dlgAbout;
		dlgAbout.DoModal();
	}
	else
	{
		CDialog::OnSysCommand(nID, lParam);
	}
}

// If you add a minimize button to your dialog, you will need the code below
//  to draw the icon.  For MFC applications using the document/view model,
//  this is automatically done for you by the framework.

void CAsyncSampleDlg::OnPaint()
{
	if (IsIconic())
	{
		CPaintDC dc(this); // device context for painting

		SendMessage(WM_ICONERASEBKGND, reinterpret_cast<WPARAM>(dc.GetSafeHdc()), 0);

		// Center icon in client rectangle
		int cxIcon = GetSystemMetrics(SM_CXICON);
		int cyIcon = GetSystemMetrics(SM_CYICON);
		CRect rect;
		GetClientRect(&rect);
		int x = (rect.Width() - cxIcon + 1) / 2;
		int y = (rect.Height() - cyIcon + 1) / 2;

		// Draw the icon
		dc.DrawIcon(x, y, m_hIcon);
	}
	else
	{
		CDialog::OnPaint();
	}
}

// The system calls this function to obtain the cursor to display while the user drags
//  the minimized window.
HCURSOR CAsyncSampleDlg::OnQueryDragIcon()
{
	return static_cast<HCURSOR>(m_hIcon);
}


void CAsyncSampleDlg::OnBnClickedOk()
{
}

void CAsyncSampleDlg::OnBnClickedHmac()
{
    // Check if in progress already

    if (m_pending) {
        if (AfxMessageBox(_T("Busy. Abort ?"), MB_YESNO) == IDYES) {

            // If a request is pending, it may be aborted with the abortPendingRequest

            m_yk.abortPendingRequest();
            m_pending = false;
        }
        return;
    }
        
    // Open one key (zero or more than one will fail here)

    if (m_yk.openKey() != YKLIB_OK) {
        AfxMessageBox(_T("Failed to open key"));
        return;
    }

    // Prepare the HMAC-SHA1 challenge here

    BYTE chalBuf[SHA1_MAX_BLOCK_SIZE];
    BYTE chalLength;

    chalLength = 9;
    memcpy(chalBuf, "Sample #2", chalLength);

    // Initiate HMAC-SHA1 operation now

    if (m_yk.writeChallengeBegin(YKLIB_FIRST_SLOT, YKLIB_CHAL_HMAC, chalBuf, chalLength) != YKLIB_OK) {
        AfxMessageBox(_T("HMAC call failed"));
        return;
    }

    m_pending = true;
}

void CAsyncSampleDlg::OnTimer(UINT_PTR nIDEvent)
{
    // If an operation is pending, check if it has completed

    if (m_pending) {

        // We now wait for a response with the HMAC-SHA1 digest

        BYTE respBuf[SHA1_DIGEST_SIZE];
        unsigned short timer;

        switch (m_yk.waitForCompletion(YKLIB_NO_WAIT, respBuf, sizeof(respBuf), &timer)) {
            
            case YKLIB_OK:      // The operation is completed and the result is valid
                {
                    // Display the response (now present in respBuf)

                    TCHAR buf[128];
                    int i;

                    for (i = 0; i < sizeof(respBuf); i++) wsprintf(buf + 2 * i, _T("%02x"), respBuf[i]);
                    SetDlgItemText(IDC_RESULT, buf);
                    m_pending = false;
                }
                break;

            case YKLIB_PROCESSING:  // Still processing or waiting for the result
                break;

            case YKLIB_TIMER_WAIT:  // A given number of seconds remain 
                SetDlgItemInt(IDC_RESULT, timer);
                m_prog.SetPos(timer);
                break;

            case YKLIB_INVALID_RESPONSE:  // Invalid or no response
                m_pending = false;
                SetDlgItemText(IDC_RESULT, _T("YKLIB_INVALID_RESPONSE"));
                break;

            default:                // A non-recoverable error has occures
                m_pending = false;
                SetDlgItemText(IDC_RESULT, _T("Failed"));
                break;
        }
    } else {

        // No HMAC operation is pending - check if one and only one key is present

        switch (m_yk.enumPorts()) {
            case 0:     SetDlgItemText(IDC_INSERTED, _T("No key present")); break;
            case 1:     SetDlgItemText(IDC_INSERTED, _T("One key present")); break;
            default:    SetDlgItemText(IDC_INSERTED, _T("More than one key present")); break;
        }
    }
}
