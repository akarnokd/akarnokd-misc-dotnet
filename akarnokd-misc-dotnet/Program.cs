using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace akarnokd_misc_dotnet
{
    class Program
    {
        static void Main(string[] args)
        {
            ShakespearePlaysScrabble.Init();

            PrintResults(ShakespearePlaysScrabbleReactorCore.Run());

            PrintResults(ShakespearePlaysScrabbleRxNET.Run());

            PrintResults(ShakespearePlaysScrabbleIxNET.Run());

            Console.WriteLine("Benchmarking...");

            Benchmark(5, () => ShakespearePlaysScrabbleRxNET.Run(), "ShakespearePlaysScrabbleRxNET");

            Benchmark(5, () => ShakespearePlaysScrabbleIxNET.Run(), "ShakespearePlaysScrabbleIxNET");

            Benchmark(5, () => ShakespearePlaysScrabbleReactorCore.Run(), "ShakespearePlaysScrabbleReactorCore");

            Console.ReadLine();
        }

        static void PrintResults(IList<KeyValuePair<int, IList<string>>> list)
        {
            foreach (var kv in list)
            {
                Console.Write(kv.Key);
                Console.Write(": ");
                foreach (var e in kv.Value)
                {
                    Console.Write(e);
                    Console.Write(", ");
                }
                Console.WriteLine();
            }
        }

        static object field;

        static void Benchmark(int seconds, Func<object> action, string name)
        {
            var start = DateTimeOffset.UtcNow;
            var end = start.AddSeconds(seconds);

            long op = 0;

            while (DateTimeOffset.UtcNow < end)
            {
                Volatile.Write(ref field, action());
                op++;
            }

            var diff = (DateTimeOffset.UtcNow - start).TotalMilliseconds;

            Console.Write(name);
            Console.Write(": ");
            Console.Write("Count = ");
            Console.Write(op);
            Console.Write(", Speed = ");
            Console.Write(((long)(diff * 1000 / op) / 1000d));
            Console.WriteLine(" ms/op");
        }
    }
}
