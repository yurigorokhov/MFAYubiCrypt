
// AsyncSampleDlg.h : header file
//

#pragma once

#include "yklib.h"
#include "afxcmn.h"

// CAsyncSampleDlg dialog
class CAsyncSampleDlg : public CDialog
{
// Construction
public:
	CAsyncSampleDlg(CWnd* pParent = NULL);	// standard constructor

// Dialog Data
	enum { IDD = IDD_ASYNCSAMPLE_DIALOG };

	protected:
	virtual void DoDataExchange(CDataExchange* pDX);	// DDX/DDV support

    CYkLib m_yk;
    bool m_pending;

// Implementation
protected:
	HICON m_hIcon;

	// Generated message map functions
	virtual BOOL OnInitDialog();
	afx_msg void OnSysCommand(UINT nID, LPARAM lParam);
	afx_msg void OnPaint();
	afx_msg HCURSOR OnQueryDragIcon();
	DECLARE_MESSAGE_MAP()
public:
    afx_msg void OnBnClickedOk();
    afx_msg void OnBnClickedHmac();
    afx_msg void OnTimer(UINT_PTR nIDEvent);
    CProgressCtrl m_prog;
};
