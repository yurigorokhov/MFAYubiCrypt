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
            string currentDir = Directory.GetCurrentDirectory();
            EncryptionSetupReturn encryptionSetupReturn;

            Console.WriteLine("Current Dir is: " + currentDir);

            // Setup an encryption with the server
            using (WebClient wc = new WebClient())
            {
                string url = "http://ec2-52-0-229-227.compute-1.amazonaws.com:8888/api/setup";

                var postString = "?users=a564e5a1-3965-4136-836b-dbc80b0bf0d7,4a971f77-4f34-4118-9aa1-d0c788526fb4";
                
                var htmlResult = wc.UploadString(url + postString, "POST", "");

                encryptionSetupReturn =
                    Newtonsoft.Json.JsonConvert.DeserializeObject<EncryptionSetupReturn>(htmlResult);
            }

            var encryptedString = Cryptography.Encrypt("mystring", encryptionSetupReturn.ShaKeys);

            Console.WriteLine("encryptedString: " + encryptedString);

            var decryptedString = Cryptography.Decrypt(encryptedString, encryptionSetupReturn.ShaKeys);

            Console.WriteLine("decryptedString: " + decryptedString);

            Console.ReadLine();
        }
    }

    class EncryptionSetupReturn
    {
        public string EncryptionId;
        public string ShaKeys;

    }

}
