using Leaf.xNet;
using System;
using System.IO;
using System.Threading;

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
                HttpResponse res = req.Get($"https://discordapp.com/api/v6/entitlements/gift-codes/{code}");

                switch (res.StatusCode)
                {
                    case HttpStatusCode.InternalServerError:
                    case HttpStatusCode.TooManyRequests:
                    case HttpStatusCode.Forbidden:
                    case HttpStatusCode.BadRequest:
                        {
                            if (Program.verbose)
                            {
                                Console.ForegroundColor = ConsoleColor.Yellow;
                                Console.WriteLine($"[-] {code}: {res.StatusCode}");
                            }
                            throw new HttpException();
                        }

                    case HttpStatusCode.NotFound:
                        {
                            Interlocked.Increment(ref Program.ch);
                            if (Program.verbose)
                            {
                                Console.ForegroundColor = ConsoleColor.Red;
                                Console.WriteLine($"[-] {code}");
                            }
                            return;
                        }

                    default:
                        {
                            Interlocked.Increment(ref Program.ch);
                            Interlocked.Increment(ref Program.goods);
                            Console.ForegroundColor = ConsoleColor.Green;
                            if (!Program.verbose)
                                Console.SetCursorPosition(0, 5 + Program.goods);

                            string wump = getWumpCode(code);

                            Console.WriteLine($"[+] {wump} (StatusCode: {(int)res.StatusCode})"); //I don't know status code of valid nitro gift

                            lock (fileLocker)
                            {
                                File.AppendAllText("good.txt", wump + $" (StatusCode: {(int)res.StatusCode})\r\n");
                            }

                            if (Program.emailnotification)
                                Program.SendEmail(wump, "Hey! I founded valid code!");
                            return;
                        }
                }
            }
        }
    }
}
