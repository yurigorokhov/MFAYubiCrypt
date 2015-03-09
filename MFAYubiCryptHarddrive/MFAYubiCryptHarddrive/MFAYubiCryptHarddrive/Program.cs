using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace MFAYubiCryptHarddrive
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            Console.WriteLine("Select an action to perform:");
            Console.WriteLine("1. Setup a new file with encryption.");
            Console.WriteLine("2. Unlock an encrypted file.");

            var command = Console.ReadKey();
            var consoleKey = command.KeyChar.ToString();

            if (consoleKey.Equals("1"))
            {
                SetupFileWithEncryption();
            }


            Console.ReadLine();
        }

        private static void SetupFileWithEncryption()
        {
            Console.WriteLine("Choose users to participate in encryption. List users seperated by comma: ");

            string users = Console.ReadLine();

            EncryptionSetupReturn encryptionSetupReturn;
            
            // Setup an encryption with the server
            using (WebClient wc = new WebClient())
            {
                Console.WriteLine("Connecting with server and grant from participators.");

                string url = "http://ec2-52-0-229-227.compute-1.amazonaws.com:8888/api/setup";

                var postString = "?users=" + users;

                string htmlResult;

                try
                {
                    htmlResult = wc.UploadString(url + postString, "POST", "");
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Bad return value from server. Do the specified users exist on the server?");
                    throw;
                }
                

                encryptionSetupReturn =
                    Newtonsoft.Json.JsonConvert.DeserializeObject<EncryptionSetupReturn>(htmlResult);

                Console.WriteLine(
                    encryptionSetupReturn != null
                        ? "Successfully received response from server including encryptionId and SHA of all participants keys."
                        : "Bad return value from server. Do the specified users exist on the server?");
            }

            var currentDir = Directory.GetCurrentDirectory();
            var files = Directory
                .EnumerateFiles(currentDir, "*", SearchOption.AllDirectories)
                .Select(Path.GetFileName);

            Console.WriteLine("Ready to encrypt file. Please specifiy file to encrypt: ");
            Console.WriteLine("Current Dir is: " + currentDir);
            Console.WriteLine("Files are: \n " + files.Aggregate(((current, next) => current + "\n " + next)));

            var fileToEncrypt = Console.ReadLine();

            String encryptedString;

            using (StreamReader sr = new StreamReader(currentDir + "/" + fileToEncrypt))
            {
                String fileContent = sr.ReadToEnd();
                encryptedString = Cryptography.Encrypt(fileContent, encryptionSetupReturn.ShaKeys);
            }

            using (StreamWriter sw = new StreamWriter(currentDir + "/" + fileToEncrypt))
            {
                sw.Write(encryptedString);
            }

            //Checking that the content can be decrypted
            var decryptedString = Cryptography.Decrypt(encryptedString, encryptionSetupReturn.ShaKeys);

            Console.WriteLine("decryptedString: " + decryptedString);
        }
    }

    class EncryptionSetupReturn
    {
        public string EncryptionId;
        public string ShaKeys;

    }

}
