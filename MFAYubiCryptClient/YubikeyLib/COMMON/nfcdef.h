/*************************************************************************
**                                                                      **
**      N F C D E F   Yubico NFC/NDEF definitions                       **
**                                                                      **
**      Copyright 2011 - Yubico AB                                      **
**                                                                      **
**      Date   / Sig / Rev  / History                                   **
**      111001 / J E / 0.00 / Main                                      **
**                                                                      **
*************************************************************************/

#ifndef __NFCDEF_H_INCLUDED__
#define __NFCDEF_H_INCLUDED__

#define ISO_CT          0x88    // ISO14443 Cascade Tag
#define YUBICO_ICC      0x27    // ‘27’ Cypak AB (SE) 
#define ULTRALIGHT_INT  0x48    // Ultralight interal byte value

#define NDEF_MAGIC      0xe1    // Capability container NDEF magic number
#define NDEF_VERSION    0x10    // Currently supported NDEF version
#define NDEF_UNPROT     0x00    // Tag is unprotected
#define NDEF_WPROT      0x0f    // Tag is write protected
#define NDEF_BLOCK_SIZE 0x08    // Block size

#define NDEF_TNF_EMPTY  0xd0    // Empty
#define NDEF_TNF_KNOWN  0xd1    // Well-known type (SR format)

#define NDEF_URI_TYPE   'U'     // URI type
#define NDEF_TEXT_TYPE  'T'     // Text type

#define NDEF_TLV_NDEF   0x03    // NDEF TLV
#define NDEF_TLV_TERM   0xfe    // NDEF terminating TLV

// NDEF URI identifiers

#define NDEF_URI_ID0    ""
#define NDEF_URI_ID1    "http://www."
#define NDEF_URI_ID2    "https://www."
#define NDEF_URI_ID3    "http://"
#define NDEF_URI_ID4    "https://"
#define NDEF_URI_ID5    "tel:"
#define NDEF_URI_ID6    "mailto:"
#define NDEF_URI_ID7    "ftp://anonymous:anonymous@"
#define NDEF_URI_ID8    "ftp://ftp."
#define NDEF_URI_ID9    "ftps://"

// Mifare Ultraligt page 0..3 configuration

typedef struct {
    unsigned char sn0, sn1, sn2, bcc0;
    unsigned char sn3, sn4, sn5, sn6;
    unsigned char bcc1, intn, lock0, lock1;
    unsigned char otp[4];
} UL_CONFIG;

// NDEF TLV

typedef struct {
    unsigned char type;           // TLV type
    unsigned char len;            // TLV len
    unsigned char recFmt;         // Record format specifier (= NDEF_TNF_KNOWN)
    unsigned char recTypeLen;     // Length of recort type (= 1)
    unsigned char recLen;         // Payload length
    unsigned char recType;        // NDEF record type specifier
    unsigned char data [1];       // Payload 
} NDEF_TLV;

#endif