using Flurl.Http;
using System;
using System.Net;
using System.Threading;

namespace IPer
{
    public class Program
    {
        private static long CurrentIP = 0;
        private static string ContentToFind = "<html>";
        private static TimeSpan Timeout = new TimeSpan(0, 0, 0, 1, 500);
        private static Semaphore locker;

        private static void Main(string[] args)
        {
            locker = new Semaphore(1, 1);
            Console.Write("Next IP: ");
            SetNext(Console.ReadLine());
            Console.WriteLine($"Searches: \"{ContentToFind}\"");
            Thread[] threads = new Thread[8];
            for (int i = 0; i < threads.Length; i++)
                (threads[i] = new Thread(F)).Start();

            Console.WriteLine("Press any key to exit");
            Console.ReadKey();
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine("Ended on: " + ToAddr(CurrentIP));
            Console.ForegroundColor = ConsoleColor.Gray;
        }

        private static async void F()
        {
            while (CurrentIP < uint.MaxValue - 1)
            {
                string ip = GetNextIp();
                try
                {
                    string content = await $"http://{ip}:80/".WithTimeout(Timeout).GetStringAsync();
                    if (content.Contains(ContentToFind))
                        WriteFoundIP($"Found {ContentToFind} on ip: " + ip);
                }
                catch (Exception e)
                {
                }
                if (CurrentIP % 10000 == 0)
                    Console.WriteLine("Check " + ip);
            }
        }

        private static void SetNext(string ip)
        {
            CurrentIP = ToInt(ip) - 1;
        }

        private static void WriteFoundIP(string text)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine(text);
            Console.ForegroundColor = ConsoleColor.Gray;
        }

        private static string GetNextIp()
        {
            long result;
            locker.WaitOne();
            result = ++CurrentIP;
            locker.Release();
            return ToAddr(result);
        }

        static long ToInt(string address)
        {
            return IPAddress.NetworkToHostOrder((int)IPAddress.Parse(address).Address);
        }

        static string ToAddr(long address)
        {
            return IPAddress.Parse(address.ToString()).ToString();
        }
    }
}
