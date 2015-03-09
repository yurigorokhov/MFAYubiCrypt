using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace MFAYubiCryptHarddrive
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            string currentDir = Directory.GetCurrentDirectory();

            Console.WriteLine("Current Dir is: " + currentDir);

            var userList = new List<string> { "a564e5a1-3965-4136-836b-dbc80b0bf0d7", "4a971f77-4f34-4118-9aa1-d0c788526fb4" };

            var transmitObj = new TransmitObj();
            transmitObj.Users = userList;

            // Setup an encryption with the server
            using (WebClient wc = new WebClient())
            {
                string url = "http://ec2-52-0-229-227.compute-1.amazonaws.com:8888/api/setup";

                var jsonUserList = Newtonsoft.Json.JsonConvert.SerializeObject(transmitObj);

                var htmlResult = wc.UploadString(url, jsonUserList);
            }

            Crypter.EncryptFile(currentDir + "/testfile.txt", currentDir + "/testfileEncrypted.txt", @"myKey123");
            Crypter.DecryptFile(currentDir + "/testfileEncrypted.txt", currentDir + "/testfileDecrypted.txt",
                @"myKey123");

            Console.ReadLine();
        }
    }

    class TransmitObj
    {
        public List<string> Users;
    }
}
