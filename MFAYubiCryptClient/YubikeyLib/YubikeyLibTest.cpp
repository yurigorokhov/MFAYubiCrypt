/*************************************************************************
**                                                                      **
**      Y K L I B T E S T  -  YkLib simple test application             **
**                                                                      **
**      Copyright 2011 - Yubico AB                                      **
**                                                                      **
**      Date   / Sig / Rev  / History                                   **
**      110329 / J E / 0.00 / Main                                      **
**      111121 / J E / 0.10 / Added challenge-response config write     **
**      131107 / J E / 3.00 / Added NDEF test functions                 **
**                                                                      **
*************************************************************************/

#include "stdafx.h"
#include <yklib.h>
#include <ctype.h>
#include <conio.h>
#include  <string.h>

// NIST test vectors for HMAC-SHA1

static const BYTE nistKey[] = {
    0x30,0x31,0x32,0x33,0x34,0x35,0x36,0x37,0x38,0x39,0x3a,0x3b,0x3c,0x3d,0x3e,0x3f,0x40,0x41,0x42,0x43
};

static const BYTE nistChal[] = {
    0x53,0x61,0x6d,0x70,0x6c,0x65,0x20,0x23,0x32
};

static BYTE nistResp[] = {
    0x09,0x22,0xd3,0x40,0x5f,0xaa,0x3d,0x19,0x4f,0x82,0xa4,0x58,0x30,0x73,0x7d,0x5c,0xc6,0xc7,0x5d,0x24
};

// Start precision timer (for testing purposes only)

static LARGE_INTEGER tstart;

void timerStart(void)
{
    QueryPerformanceCounter(&tstart);                
}

// Return elapsed time in ms (for tesing purposes only)

DWORD timerQuery(void)
{
    LARGE_INTEGER tval, freq;

    QueryPerformanceCounter(&tval);                
    QueryPerformanceFrequency(&freq);

    return (DWORD) ((1000 * (tval.QuadPart - tstart.QuadPart)) / freq.QuadPart);
}

// Print out return code in clear text

void printRetcode(YKLIB_RC rc)
{
    switch (rc) {
        case YKLIB_OK:                  printf("YKLIB_OK "); break;
        case YKLIB_FAILURE:             printf("YKLIB_FAILURE "); break;
        case YKLIB_NOT_OPENED:          printf("YKLIB_NOT_OPENED "); break;
        case YKLIB_INVALID_PARAMETER:   printf("YKLIB_INVALID_PARAMETER "); break;
        case YKLIB_NO_DEVICE:           printf("YKLIB_NO_DEVICE "); break;
        case YKLIB_MORE_THAN_ONE:       printf("YKLIB_MORE_THAN_ONE "); break;
        case YKLIB_WRITE_ERROR:         printf("YKLIB_WRITE_ERROR "); break;
        case YKLIB_INVALID_RESPONSE:    printf("YKLIB_INVALID_RESPONSE "); break;
        case YKLIB_NOT_COMPLETED:       printf("YKLIB_NOT_COMPLETED "); break;
        case YKLIB_NOT_CONFIGURED:      printf("YKLIB_NOT_CONFIGURED "); break;
        case YKLIB_NOT_READY:           printf("YKLIB_NOT_READY "); break;
        case YKLIB_PROCESSING:          printf("YKLIB_PROCESSING "); break;
        case YKLIB_TIMER_WAIT:          printf("YKLIB_TIMER_WAIT "); break;
        case YKLIB_UNSUPPORTED_FEATURE: printf("YKLIB_UNSUPPORTED_FEATURE "); break;
        default:                        printf("Unknown rc=%d ", rc); break;
    }
}

int _tmain(int argc, char *argv[])
{
    CYkLib yk;
    BYTE ch;
    WORD timerVal;
    BYTE buffer[128];
    int i;
    wchar_t buf[MAX_PATH];
    YKLIB_RC rc;
    STATUS status;
    CONFIG config;
    BYTE targetSlot = YKLIB_SECOND_SLOT;

    memset(&status, 0, sizeof(status));

    //for (;;) {
		/*
        printf("\n\n");

        printf("a - Enumerate ports with Yubikeys present\n");
        printf("b - Get port names [from last enumeration]\n");
        printf("c - Open one key\n");
        printf("d - Open key from enumeration list\n");
        printf("e - Close key\n");
        printf("f - Read status\n");
        printf("g - Write Yubico OTP configuration\n");
        printf("h - Write challenge-response configuration\n");
        printf("i - KillConfig\n");
        printf("j - Read serial number\n"); 
        printf("k - Challenge-response - Yubico OTP\n");
        printf("l - Challenge-response - HMAC-SHA1\n");
        printf("m - Challenge-response - HMAC-SHA1 - allow button wait\n");
        printf("n - Write NDEF record\n");
        printf("o - Abort pending request\n");
        printf("p - Toggle target slot # from %d\n", targetSlot);
        printf("q - Poll readStatus\n");

        printf("\nSelect a..o > ");
		*/
	
/*	int countLen = 0;
	for (char* it = argv[1]; *it; ++it) {
		//*it;
		printf("%c", it);
		countLen++;
	}
*/
/*	printf("Len: %i", strlen(argv[1]));

	for (i = 0; i <= 16; i = i + 2){
		printf("%c", argv[1][i]);

	}
*/		//printf("%s", argv[1]);
	int size_of_input = 10;//sizeof(argv[1]);
//	printf("\n Size of input: %d", size_of_input);

	yk.openKey();

				memcpy(buffer, argv[1], size_of_input);

				int count = 0;
				for (i = 0; i < size_of_input; i = i + 1){
					buffer[i] = argv[1][count];
					count = count + 2;
				}

/*				printf("\n Input: ");
				for (i = 0; i < size_of_input; i = i + 1){
					printf("%c", buffer[i]);

				}

				printf("\n");
				*/
				printRetcode(rc = yk.writeChallengeBegin(targetSlot, YKLIB_CHAL_HMAC, buffer, size_of_input));
                
                if (rc == YKLIB_OK) {

                    // Wait for response completion. This may take a while if a button acknowledgement is requested

                    //printf("\nPress any key to abort pending request\n");

                    for (;;) {
                        rc = yk.waitForCompletion(YKLIB_MAX_CHAL_WAIT, buffer, SHA1_DIGEST_SIZE, &timerVal);
                        if (rc != YKLIB_TIMER_WAIT) break;
                        //printf("%d \r", timerVal);

                        // Throttling can be added here if desired

                        if (_kbhit()) {
                            _getch();
                            printf("\nAborting ");
                            printRetcode(yk.abortPendingRequest());
                            rc = YKLIB_NOT_COMPLETED;
                            break;
                        }
                    }
                        
                    if (rc == YKLIB_OK) {
                        //printf("\nHMAC-SHA1 = ");
						printf("\n");
                        for (i = 0; i < SHA1_DIGEST_SIZE; i++) printf("%02x", buffer[i]);
						//printf("\n");
						if (!memcmp(buffer, nistResp, sizeof(nistResp))) printf("");// = NIST response");
                    } else
                        printRetcode(rc);
                } else
                    printRetcode(rc);

            
    

	return 0;
}

