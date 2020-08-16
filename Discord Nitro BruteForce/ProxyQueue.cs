using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

namespace Discord_Nitro_BruteForce
{
    class ProxyQueue
    {
        ConcurrentQueue<string> proxies;
        string[] plist;

        object locker = new object();

        public int Length { get; private set; }

        public ProxyQueue(IEnumerable<string> pr)
        {
            plist = pr.ToArray();
            Length = plist.Length;
            proxies = new ConcurrentQueue<string>(plist);
        }

        public string Next()
        {
            if(proxies.Count == 0)
            {
                lock (locker)
                {
                    if (proxies.Count == 0) //in order to exclude repeated re-creation of the queue by other threads
                        proxies = new ConcurrentQueue<string>(plist);
                }
            }

            string res;
            if (proxies.TryDequeue(out res))
                return res;
            else
                return Next();
        }
    }
}
