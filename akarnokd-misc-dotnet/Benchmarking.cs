using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Reactive.Streams;
using Reactor.Core;
using System.Threading;
using Reactor.Core.flow;
using Reactor.Core.subscriber;
using Reactor.Core.subscription;
using Reactor.Core.util;

namespace akarnokd_misc_dotnet
{
    static class Benchmarking
    {
        static object field;

        internal static void Benchmark(int seconds, Func<object> action, string name)
        {
            Console.Write(name);
            Console.Write(": ");

            var start = DateTimeOffset.UtcNow;
            var end = start.AddSeconds(seconds);

            long op = 0;

            while (DateTimeOffset.UtcNow < end)
            {
                Volatile.Write(ref field, action());
                op++;
            }

            var diff = (DateTimeOffset.UtcNow - start).TotalMilliseconds;

            Console.Write("Count = ");
            Console.Write(op);
            Console.Write(", Speed = ");
            Console.Write(((long)(diff * 1000 / op) / 1000d));
            Console.WriteLine(" ms/op");
        }

        internal static void Benchmark(int seconds, Func<int, object> action, string name, params int[] count)
        {
            foreach (var c in count)
            {
                Console.Write(name);
                Console.Write("(");
                Console.Write(c);
                Console.Write("): ");

                var start = DateTimeOffset.UtcNow;
                var end = start.AddSeconds(seconds);

                try
                {
                    long op = 0;

                    while (DateTimeOffset.UtcNow < end)
                    {
                        Volatile.Write(ref field, action(c));
                        op++;
                    }

                    var diff = (DateTimeOffset.UtcNow - start).TotalMilliseconds;

                    Console.Write("Count = ");
                    Console.Write(op);
                    Console.Write(", Speed = ");
                    Console.Write(((long)(diff * 1000 / op) / 1000d));
                    Console.WriteLine(" ms/op");
                }
                catch (Exception ex)
                {
                    Console.Write(name);
                    Console.Write("(");
                    Console.Write(c);
                    Console.Write("): ");
                    Console.WriteLine(ex.ToString());
                }
            }
        }
    }
}
