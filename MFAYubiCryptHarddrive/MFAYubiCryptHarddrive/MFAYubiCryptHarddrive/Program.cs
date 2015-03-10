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
            var encryptionIds = new List<Tuple<string, string>>();

            while (true)
            {
                Console.WriteLine("Select an action to perform:");
                Console.WriteLine("1. Setup a new file with encryption.");
                Console.WriteLine("2. Unlock an encrypted file.");

                var command = Console.ReadKey();
                var consoleKey = command.KeyChar.ToString();

                if (consoleKey.Equals("1"))
                {
                    encryptionIds.Add(SetupFileWithEncryption());
                }
                else if (consoleKey.Equals("2"))
                {
                    DecryptFile(encryptionIds);
                }


                Console.WriteLine("\n\n");
            }
        }

        private static void DecryptFile(List<Tuple<string, string>> encryptionIds)
        {
            var currentDir = Directory.GetCurrentDirectory();
            var files = Directory
                .EnumerateFiles(currentDir, "*", SearchOption.AllDirectories)
                .Select(Path.GetFileName);

            Console.WriteLine("Current Dir is: " + currentDir);
            Console.WriteLine("Files are: \n " + files.Aggregate(((current, next) => current + "\n " + next)));

            var fileToDecrypt = Console.ReadLine();

            Tuple<string, string> encryptionTuple = null;
            foreach (var encryptionId in encryptionIds)
            {
                if (encryptionId.Item1.Equals(fileToDecrypt))
                {
                    encryptionTuple = encryptionId;
                }
            }

            if (encryptionTuple == null)
            {
                throw new Exception("File requested was not on the list of known Ids.");
            }


            string url = "http://ec2-52-0-229-227.compute-1.amazonaws.com:8888/api/decrypt/";

            var postString = encryptionTuple.Item2;

            string htmlResult;

            using (WebClient wc = new WebClient())
            {
                try
                {
                    htmlResult = wc.UploadString(url + postString, "POST", "");
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Bad return value from server. Do the specified encryptionId exist on the server?");
                    throw;
                }
            }

            var requestIdReturn = Newtonsoft.Json.JsonConvert.DeserializeObject<DecryptionReturn>(htmlResult);

            Console.WriteLine("Request now posted to server. Awaiting acceptance from the quorum. Press any key to quire the server for status.");

            

            htmlResult = "";

            while (htmlResult == "")
            {
                Console.ReadKey();

                Console.WriteLine("Quering the server for status.");

                using (WebClient wc = new WebClient())
                {
                    try
                    {
                        htmlResult = wc.DownloadString(url + requestIdReturn.RequestId);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(
                            "Bad return value from server. Do the specified requestId exist on the server?");
                        throw;
                    }
                }

                if (htmlResult == "") { Console.WriteLine("Quorum has not responded. Try again later.");}
            }

            string decryptedFile;

            using (StreamReader sr = new StreamReader(currentDir + "/" + fileToDecrypt))
            {
                String fileContent = sr.ReadToEnd();
                decryptedFile = Cryptography.Decrypt(fileContent, htmlResult);
            }

            using (StreamWriter sw = new StreamWriter(currentDir + "/" + fileToDecrypt))
            {
                sw.Write(decryptedFile);
            }

            Console.WriteLine("decryptedFile: " + decryptedFile);


        }

        private static Tuple<string,string> SetupFileWithEncryption()
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

            return Tuple.Create(fileToEncrypt, encryptionSetupReturn.EncryptionId);
        }
    }

    class EncryptionSetupReturn
    {
        public string EncryptionId;
        public string ShaKeys;

    }

    class DecryptionReturn
    {
        public string RequestId;
    }

}
