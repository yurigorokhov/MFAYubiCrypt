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
            string path = Directory.GetCurrentDirectory();

            string filename = Path.Combine(path, "YkLibTest.exe");

            Console.WriteLine(filename);

            var startInfo = new ProcessStartInfo
            {
                FileName = filename,
                Arguments = "Challenge",
                UseShellExecute = false,
                RedirectStandardOutput = true,
                //RedirectStandardError = true,
                CreateNoWindow = true
            };

            var proc = new Process
            {
                StartInfo = startInfo
            };

            proc.Start();
            //proc.StandardInput.Write("c");

            while (!proc.StandardOutput.EndOfStream)
            {

            //proc.StandardInput.WriteLine("c");

                string line = proc.StandardOutput.ReadLine();
                Console.WriteLine(line);
            }

            Console.ReadKey();
        
        }
    }
}