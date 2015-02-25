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

int _tmain(int argc, _TCHAR* argv[])
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
    BYTE targetSlot = YKLIB_FIRST_SLOT;

    memset(&status, 0, sizeof(status));

    for (;;) {
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

        ch = tolower(_getch());
       
        switch (ch) {

            case 0x1b:
                return 1;

                // Enumerate all ports with Yubikeys present and return count

            case 'a':
                printf("Number of Yubikeys found %d", yk.enumPorts());
                break;

                // Print out port names from previous enumeration

            case 'b':
                for (i = 0; ; i++) {
                    if (!yk.getPortName(i, buf, sizeof(buf) / sizeof(buf[0]))) break;
                    wprintf(L"\n%2d - %s", i, buf); 
                }
                break;

                // Open one key. If none or more than one key is attached, the call will fail

            case 'c':
                printRetcode(yk.openKey());
                break;

                // Open a specific key based on index from a previous enumeration           

            case 'd':
                printRetcode(yk.openKey((unsigned short) 0));
                break;

                // Close an open Yubikey

            case 'e':
                printRetcode(yk.closeKey());
                break;

                // Read out the status block from an opened Yubikey

            case 'f':
                if ((rc = yk.readStatus(&status)) == YKLIB_OK) {
                    printf("Version %d.%d.%d Touch %d", status.versionMajor, status.versionMinor, status.versionBuild, status.touchLevel);
                    printf("\nSlot configuration status:\nSlot 0 - ");
                    printRetcode(yk.isSlotConfigured(YKLIB_FIRST_SLOT, &status));
                    printf(" Slot 1 - ");
                    printRetcode(yk.isSlotConfigured(YKLIB_SECOND_SLOT, &status));
                } else
                    printRetcode(rc);
                break;

                // Write a configuration record, Yubico OTP or challenge-response

            case 'g':
            case 'h':
                // Clear unused fields

                memset(&config, 0, sizeof(config));

                // Fill in desired settings in the config structure here

                if (ch == 'g') {

                    // Yubico OTP
                    
                    // 2 bytes public id 0x4711 == fibb modhex

                    config.fixedSize = 2;
                    config.fixed[0] = 0x47;
                    config.fixed[1] = 0x11;

                    // Set private ID here

                    memcpy(config.uid, "\x1\x2\x3\x4\x5\x6", sizeof(config.uid));

                    // Set AES-128 key here

                    memset(config.key, 0x55, sizeof(config.key));

                    // Set flags tktFlags, cfgFlags and extFlags to values from ykdef.h

                    config.tktFlags = TKTFLAG_APPEND_CR;
                    config.extFlags = EXTFLAG_SERIAL_API_VISIBLE | EXTFLAG_SERIAL_BTN_VISIBLE;
                } else {

                    // HMAC-SHA1 for Challenge-response, using a NIST key

                    yk.setKey160(&config, nistKey);

                    // Set flags tktFlags, cfgFlags and extFlags to values from ykdef.h

                    config.tktFlags = TKTFLAG_CHAL_RESP;
                    config.cfgFlags = CFGFLAG_CHAL_HMAC | CFGFLAG_HMAC_LT64 | CFGFLAG_CHAL_BTN_TRIG;
                    config.extFlags = EXTFLAG_SERIAL_API_VISIBLE;
                }

                // Perform configuration write

                timerStart();
                rc = yk.writeConfigBegin(targetSlot, &config);
                printRetcode(rc);
                printf(" twrite=%lu ms ", timerQuery());

                // If write was initiated successful, wait for completion

                if (rc == YKLIB_OK) {
                    rc = yk.waitForCompletion(YKLIB_MAX_WRITE_WAIT);
                    printRetcode(rc);
                    printf(" total=%lu ms", timerQuery());
                }
                break;

                // Kill a configuration in an opened Yubikey

            case 'i':

                // Initialize write (no data = kill)

                rc = yk.writeConfigBegin(targetSlot);

                // If successfully initiated, wait for completion

                if (rc == YKLIB_PROCESSING) rc = yk.waitForCompletion(YKLIB_MAX_WRITE_WAIT);
                printRetcode(rc);
                break;

                // Read out the factory programmed serial number from an opened Yubikey

            case 'j':

                // Setup serial number read

                timerStart(); 
                rc = yk.readSerialBegin();
                printf(" twrite=%lu ms ", timerQuery());
                if (rc == YKLIB_OK) {

                    // Wait for response completion

                    rc = yk.waitForCompletion(YKLIB_MAX_SERIAL_WAIT, buffer, sizeof(DWORD));
                    printf(" total=%lu ms ", timerQuery());

                    if (rc == YKLIB_OK) {
                        printf("\nSerial number read = %02x%02x%02x%02x", buffer[0], buffer[1], buffer[2], buffer[3]);
                    } else
                        printRetcode(rc);
                } else
                    printRetcode(rc);

                break;

                // Perform a challenge-response in Yubico OTP mode

            case 'k':
                memcpy(buffer, "\x1\x2\x3\x4\x5\x6", 6);
                timerStart();
                printRetcode(rc = yk.writeChallengeBegin(targetSlot, YKLIB_CHAL_OTP, buffer, UID_SIZE));
                printf(" twrite=%lu ms ", timerQuery());

                if (rc == YKLIB_OK) {

                    // Wait for response completion

                    rc = yk.waitForCompletion(YKLIB_MAX_CHAL_WAIT, buffer, sizeof(TICKET));
                    printf(" total=%lu ms ", timerQuery());

                    if (rc == YKLIB_OK) {
                        printf("\nYubico OTP = ");
                        for (i = 0; i < sizeof(TICKET); i++) printf("%02x", buffer[i]);
                    } else
                        printRetcode(rc);
                } else
                    printRetcode(rc);

                break;

                // Perform a challenge-response in HMAC-SHA1 mode

            case 'l':
                timerStart();
                printRetcode(rc = yk.writeChallengeBegin(targetSlot, YKLIB_CHAL_HMAC, (BYTE *) nistChal, sizeof(nistChal)));
                printf(" twrite=%lu ms ", timerQuery());

                if (rc == YKLIB_OK) {

                    // Wait for response completion

                    rc = yk.waitForCompletion(YKLIB_MAX_CHAL_WAIT, buffer, SHA1_DIGEST_SIZE);
                    printf(" total=%lu ms ", timerQuery());

                    if (rc == YKLIB_OK) {
                        printf("\nHMAC-SHA1 = ");
                        for (i = 0; i < SHA1_DIGEST_SIZE; i++) printf("%02x", buffer[i]);
                        if (!memcmp(buffer, nistResp, sizeof(nistResp))) printf(" = NIST response");
                    } else
                        printRetcode(rc);
                } else
                    printRetcode(rc);

                break;

                // Perform a challenge-response in HMAC-SHA1 mode and allow wait for button to be pressed

            case 'm':
                memcpy(buffer, "Sample #2", 9);
                printRetcode(rc = yk.writeChallengeBegin(targetSlot, YKLIB_CHAL_HMAC, buffer, 9));
                
                if (rc == YKLIB_OK) {

                    // Wait for response completion. This may take a while if a button acknowledgement is requested

                    printf("\nPress any key to abort pending request\n");

                    for (;;) {
                        rc = yk.waitForCompletion(YKLIB_MAX_CHAL_WAIT, buffer, SHA1_DIGEST_SIZE, &timerVal);
                        if (rc != YKLIB_TIMER_WAIT) break;
                        printf("%d \r", timerVal);

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
                        printf("\nHMAC-SHA1 = ");
                        for (i = 0; i < SHA1_DIGEST_SIZE; i++) printf("%02x", buffer[i]);
                        if (!memcmp(buffer, nistResp, sizeof(nistResp))) printf(" = NIST response");
                    } else
                        printRetcode(rc);
                } else
                    printRetcode(rc);

                break;

                // Write NDEF URI record

            case 'n':

                // Initialize write (no data = kill)

                rc = yk.writeNDEFBegin(targetSlot, YKLIB_NDEF_URI, "http://demo.yubico.com/php-yubico/one_factor.php?key=");

                // If successfully initiated, wait for completion

                if (rc == YKLIB_PROCESSING) rc = yk.waitForCompletion(YKLIB_MAX_WRITE_WAIT);
                printRetcode(rc);
                break;

                // Abort a pending request (if any)

            case 'o':
                printRetcode(yk.abortPendingRequest());
                break;

            case 'p':
                if (targetSlot == YKLIB_FIRST_SLOT) {
                    printf("Target is now YKLIB_SECOND_SLOT");
                    targetSlot = YKLIB_SECOND_SLOT;
                } else {
                    printf("Target is now YKLIB_FIRST_SLOT");
                    targetSlot = YKLIB_FIRST_SLOT;
                }
                break;

            case 'q':
                printf("\nPress any key to quit... ");
                while (!_kbhit()) {
                    rc = yk.readStatus(&status);
                    if (rc != YKLIB_OK) {
                        printf("\nTerminated with retcode ");
                        printRetcode(rc);
                        break;
                    }
                    printf("%d ", status.touchLevel);
                    Sleep(250);
                }
                break;

        }
    }

	return 0;
}

