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
            Console.WriteLine("Press s to setup user. Any other key to ask the server if there is a challenge to solve");
            var keyPressed = Console.ReadKey();

           string url = "http://ec2-52-0-229-227.compute-1.amazonaws.com:8888/api/challenge/";

           using (WebClient wc = new WebClient())
           {
               if (keyPressed.KeyChar.ToString() == "s")
               {

                   string secret = HexStringToString("31 6e e2 dd 9d 5d c3 46 94 2c 9f f3 4a 66 29 86 29 86 12 55");
                   //Setup
                   string url2 = "http://ec2-52-0-229-227.compute-1.amazonaws.com:8888/api/users";


                   var htmlResult = wc.UploadString(url2 + "?username=lars3&secret="+ secret, "POST", "");
               }

               if (keyPressed.KeyChar.ToString() == "c")
               {
                   string response = YubiKey.GetResponse("7483261694db9df00ee9dc2c49d1c5980242b5e26febbb632963115f6ea5aefe");

                   //  Response shall be f38211d9c4b5a79fdd407b8ecb076bc2e75dd4c6
               }

               var userId = "14";

               var jsonString =
                   wc.DownloadString(url + userId);

                var jsonChallenge = Newtonsoft.Json.JsonConvert.DeserializeObject<ChallengeObj>(jsonString);

                if(jsonChallenge != null)
                {
                    Console.WriteLine("Challenge successfully received. Please press yubikey to provide response and send back to server.");

                    string response = YubiKey.GetResponse(jsonChallenge.Challenge);

                    if (response != "")
                    {
                       // wc.Headers[HttpRequestHeader.ContentType] = "application/x-www-form-urlencoded";
                        var htmlResult = wc.UploadString(url + jsonChallenge.Id, "POST", response);
                    }
                    else
                    {
                        Console.WriteLine("Response from Yubikey failed. Make sure the Yubikey is correctly inserted.");
                    }
                }
            }

            Console.ReadKey();
        }

        public static string HexStringToString(String s)
        {
            string hexValues = s;
            string finalString = "";
            string[] hexValuesSplit = hexValues.Split(' ');
            foreach (String hex in hexValuesSplit)
            {
                // Convert the number expressed in base-16 to an integer. 
                int value = Convert.ToInt32(hex, 16);
                // Get the character corresponding to the integral value. 
                string stringValue = Char.ConvertFromUtf32(value);
                finalString += stringValue;
                char charValue = (char)value;
                Console.WriteLine("hexadecimal value = {0}, int value = {1}, char value = {2} or {3}",
                                    hex, value, stringValue, charValue);

                
            }

            return finalString;
        }


    }

    class ChallengeObj
    {
        public string Id;
        public string Challenge;

    }



}