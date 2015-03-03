using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MFAYubiCryptHarddrive
{
    class Program
    {
        static void Main(string[] args)
        {
            string currentDir = Directory.GetCurrentDirectory();

            Console.WriteLine("Current Dir is: " + currentDir);

            Crypter.EncryptFile(currentDir + "/testfile.txt", currentDir + "/testfileEncrypted.txt", @"myKey123");
            Crypter.DecryptFile(currentDir + "/testfileEncrypted.txt", currentDir + "/testfileDecrypted.txt", @"myKey123");

            Console.ReadLine();
        }
    }
}
