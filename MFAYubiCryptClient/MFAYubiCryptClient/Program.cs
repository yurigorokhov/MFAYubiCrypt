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
           string Url = "http://ec2-52-0-229-227.compute-1.amazonaws.com:8888/api/challenge/123";

           using (WebClient wc = new WebClient())
           {

               var jsonString =
                   wc.DownloadString(Url);

                var jsonChallenge = Newtonsoft.Json.JsonConvert.DeserializeObject<ChallengeObj>(jsonString);

                if(jsonChallenge != null)
                {
                    string response = YubiKey.GetResponse(jsonChallenge.Challenge);

                    if (!(response == ""))
                    {
                       // wc.Headers[HttpRequestHeader.ContentType] = "application/x-www-form-urlencoded";
                        string HtmlResult = wc.UploadString(Url, response);
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