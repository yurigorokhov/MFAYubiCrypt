using System;
using System.Diagnostics;
using System.IO;

namespace MFAYubiCryptClient
{
    class YubiKey
    {
        public static string GetResponse(string challenge)
        {
            string response = "";

            string path = Directory.GetCurrentDirectory();

            string filename = Path.Combine(path, "YkLibTest.exe");

            Console.WriteLine(filename);

            Console.WriteLine("Requesting Yubikey authentification. Please press your Yubikey within the next 15 seconds.");

            var startInfo = new ProcessStartInfo
            {
                FileName = filename,
                Arguments = challenge,
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

            
            int counter = 0;

            while (!proc.StandardOutput.EndOfStream)
            {
                string line = proc.StandardOutput.ReadLine();

                if (counter == 0)
                {
                    if (!line.Contains("YKLIB_OK"))
                        break;
                }

                if (counter == 1)
                {
                    if (line.Length == 40)
                        response = line;
                }

                counter++;

                Console.WriteLine(line);
            }

            return response;
        }
    }
}