using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace MFAYubiCryptClient
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Press any key to ask the server if there is a challenge to solve");
            Console.ReadKey();

           string url = "http://ec2-52-0-229-227.compute-1.amazonaws.com:8888/api/challenge/123";

           using (WebClient wc = new WebClient())
           {

               var jsonString =
                   wc.DownloadString(url);

                var jsonChallenge = Newtonsoft.Json.JsonConvert.DeserializeObject<ChallengeObj>(jsonString);

                if(jsonChallenge != null)
                {
                    Console.WriteLine("Challenge successfully received. Please press yubikey to provide response and send back to server.");

                    string response = YubiKey.GetResponse(jsonChallenge.Challenge);

                    if (response != "")
                    {
                       // wc.Headers[HttpRequestHeader.ContentType] = "application/x-www-form-urlencoded";
                        var htmlResult = wc.UploadString(url, response);
                    }
                    else
                    {
                        Console.WriteLine("Response from Yubikey failed. Make sure the Yubikey is correctly inserted.");
                    }
                }
            }

            Console.ReadKey();
        }
    }

    class ChallengeObj
    {
        public int Id;
        public string Challenge;

    }

}