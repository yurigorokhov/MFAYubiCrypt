using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MFAYubiCryptClient
{
    class Program
    {
        static void Main(string[] args)
        {
            string response = YubiKey.GetResponse("Challenge");


            Console.ReadKey();
        }
    }
}