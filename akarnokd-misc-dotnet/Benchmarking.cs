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
            try
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

                Console.Write("# = ");
                Console.Write(op);
                Console.Write(", Spd = ");
                Console.Write(string.Format("{0:#.###}", diff / op));
                Console.Write(" ms/op, ");
                Console.Write(string.Format("{0:#.###}", op * 1000 / diff));
                Console.WriteLine(" op/s");
            }
            catch (Exception ex)
            {
                Console.Write(name);
                Console.Write(": ");
                Console.WriteLine(ex.ToString());
            }
        }

        internal static void Benchmark(int seconds, Func<int, object> action, string name, string lib, params int[] count)
        {
            foreach (var c in count)
            {
                Console.Write(name);
                Console.Write(" ");
                Console.Write(lib);
                Console.Write(" ");
                Console.Write(string.Format("{0,7}", c));
                Console.Write(" thrpt ");
                Console.Write(seconds);
                Console.Write(" ");

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

                    // score
                    Console.Write(string.Format("{0,13:#.000}", op * 1000 / diff));
                    Console.WriteLine(" 0.000 op/s");
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.ToString());
                }
            }
        }
    }
}
