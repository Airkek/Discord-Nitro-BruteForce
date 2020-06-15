using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using xNet;

namespace Discord_Nitro_BruteForce
{
    class Program
    {
        static Random random = new Random();
        static string goods = "";
        static int lenGoods = 0;
        static int ch = 0;
        static int err = 0;
        static bool work = false;
        static string[] proxies;
        static int proxyType;
        static bool verbose = false;
        

        [STAThread]
        static void Main(string[] args)
        {

            Thread counter = new Thread(new ThreadStart(setTitle));
            counter.Start();

            int threads;

            Console.Write("Enter count of threads: ");
            try
            {
                threads = Convert.ToInt32(Console.ReadLine().Trim());
            }
            catch
            {
                Console.WriteLine("Unknown threads count. Using 100");
                threads = 100;
            }

            while (true)
            {

                Console.WriteLine("Select proxy type:\r\n1. Http/s\r\n2. Socks4\r\n3. Socks5");
                Console.Write("Your choice: ");
                ConsoleKey k = Console.ReadKey().Key;
                Console.WriteLine();
                if (k == ConsoleKey.D1)
                {
                    Console.WriteLine("Selected Http/s type of proxy");
                    proxyType = 1;
                    break;
                }
                else if (k == ConsoleKey.D2)
                {
                    Console.WriteLine("Selected Socks4 type of proxy");
                    proxyType = 2;
                    break; 
                }
                else if (k == ConsoleKey.D3)
                {
                    Console.WriteLine("Selected Socks5 type of proxy");
                    proxyType = 3;
                    break;
                }
                Console.Clear();
            }

            Console.WriteLine("Open file with proxy list");

            OpenFileDialog dialog = new OpenFileDialog();
            dialog.Filter = "Proxy list (*.txt)|*.txt";

            if (dialog.ShowDialog() != DialogResult.OK)
            {
                return;
            }

            string proxyPath = dialog.FileName;

            proxies = File.ReadAllLines(proxyPath);
            Console.WriteLine($"Loaded {proxies.Length} proxies");

            Console.Write("Use verbose mode? (y/n): ");
            if (Console.ReadLine().ToLower().Trim() == "y")
                verbose = true;
            
            work = true;

            for (int i = 0; i < threads; i++)
            {
                Thread t = new Thread(new ThreadStart(Worker));
                t.Start();
            }
                

            Console.ReadKey();
        }


        static void Worker()
        {
            Worker("");
        }
        static void Worker(string code)
        {
            try
            {
                ProxyClient proxy = getNewProxy();

                if(code == "")
                    code = GenerateCode();
                HttpRequest request = new HttpRequest();//.Create();
                request.ConnectTimeout = 10000;
                request.Proxy = proxy;

                try
                {
                    HttpResponse response = request.Get($"https://discordapp.com/api/v6/entitlements/gift-codes/{code}");
                    String res = response.ToString();

                    ch++;
                    //Console.WriteLine(res);

                        
                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.WriteLine($"[+] {code}");
                    Console.WriteLine(res);
                    lenGoods++;
                    goods += $"{code}\r\n";
                    try
                    {
                        File.WriteAllText("good.txt", goods);

                    }
                    catch (FileNotFoundException)
                    {
                        FileStream goodsFile = File.Create("good.txt");
                        goodsFile.Close();

                        File.WriteAllText("good.txt", goods);
                    }

                    Thread.Sleep(1000);
                }
                catch (xNet.HttpException e)
                {
                    //Console.WriteLine(e.Status);
                    
                    if (e.Status == HttpExceptionStatus.ConnectFailure || e.HttpStatusCode != HttpStatusCode.NotFound)
                    {
                        err++;
                        if (verbose)
                        {
                            Console.ForegroundColor = ConsoleColor.Yellow;
                            if (e.Status == HttpExceptionStatus.ConnectFailure)
                                Console.WriteLine($"[ERR] {code}");
                            else
                                Console.WriteLine($"[RATELIMIT] {code}");
                        }
                            
                        Worker(code);
                        return;
                    }
                    ch++;
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine($"[-] {code}");
                }
            }
            catch (Exception e) 
            {
                Console.WriteLine(e);
            }

            Worker("");
        }

        static ProxyClient getNewProxy()
        {
            

            string fullP = proxies[random.Next(0, proxies.Length - 1)];
                
            switch (proxyType)
                {
                    case 1:
                            return HttpProxyClient.Parse(fullP);
                    case 2:
                            return Socks4ProxyClient.Parse(fullP);
                    case 3:
                            return Socks5ProxyClient.Parse(fullP);
                    default:
                            return HttpProxyClient.Parse(fullP);
                }
        }

        static string GenerateCode()
        {
            string code = "";
            string dict = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz1234567890";

            for(int i = 0; i < 16; i++)
            {
                code += dict[random.Next(0, dict.Length - 1)];
            }

            return code;
        }

        static void setTitle()
        {
            while (true)
            {
                string text = "idle";
                if (work)
                {
                    text = $"work Checked: {ch}, Hits: {lenGoods}";
                }

                Console.Title = $"Discord Nitro - {text}";
            }
        }
    }
}
