﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace MFAYubiCryptHarddrive
{
    class Crypter
    {
            ///<summary>
            /// Steve Lydford - 12/05/2008.
            ///
            /// Encrypts a file using Rijndael algorithm.
            ///</summary>
            ///<param name="inputFile"></param>
            ///<param name="outputFile"></param>
            public static void EncryptFile(string inputFile, string outputFile, string encryptionKey)
            {

                try
                {
                    string password = encryptionKey;
                    UnicodeEncoding UE = new UnicodeEncoding();
                    byte[] key = UE.GetBytes(password);

                    string cryptFile = outputFile;
                    FileStream fsCrypt = new FileStream(cryptFile, FileMode.Create);

                    RijndaelManaged RMCrypto = new RijndaelManaged();

                    CryptoStream cs = new CryptoStream(fsCrypt,
                        RMCrypto.CreateEncryptor(key, key),
                        CryptoStreamMode.Write);

                    FileStream fsIn = new FileStream(inputFile, FileMode.Open);

                    int data;
                    while ((data = fsIn.ReadByte()) != -1)
                        cs.WriteByte((byte)data);


                    fsIn.Close();
                    cs.Close();
                    fsCrypt.Close();
                }
                catch
                {
                    Console.WriteLine("Encryption failed!", "Error");
                }
            }
            ///<summary>
            /// Steve Lydford - 12/05/2008.
            ///
            /// Decrypts a file using Rijndael algorithm.
            ///</summary>
            ///<param name="inputFile"></param>
            ///<param name="outputFile"></param>
            public static void DecryptFile(string inputFile, string outputFile, string decryptionKey)
            {

                {
                    string password = decryptionKey; // Your Key Here

                    UnicodeEncoding UE = new UnicodeEncoding();
                    byte[] key = UE.GetBytes(password);

                    FileStream fsCrypt = new FileStream(inputFile, FileMode.Open);

                    RijndaelManaged RMCrypto = new RijndaelManaged();

                    CryptoStream cs = new CryptoStream(fsCrypt,
                        RMCrypto.CreateDecryptor(key, key),
                        CryptoStreamMode.Read);

                    FileStream fsOut = new FileStream(outputFile, FileMode.Create);

                    int data;
                    while ((data = cs.ReadByte()) != -1)
                        fsOut.WriteByte((byte)data);

                    fsOut.Close();
                    cs.Close();
                    fsCrypt.Close();

                }
            }
        }

    
}
