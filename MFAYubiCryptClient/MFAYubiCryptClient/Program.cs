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
            var jsonString =
                new WebClient().DownloadString("http://ec2-52-0-229-227.compute-1.amazonaws.com:8888/api/challenge/123");

            var jsonChallenge = Newtonsoft.Json.JsonConvert.DeserializeObject<ChallengeObj>(jsonString);

            if(jsonChallenge != null)
            {
                string response = YubiKey.GetResponse(jsonChallenge.Challenge);
            }

            //TODO: Send the string back to the server.

            Console.ReadKey();
        }
    }

    class ChallengeObj
    {
        public int Id;
        public string Challenge;

    }

}