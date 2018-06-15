using akarnokd_misc_dotnet.schedulers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Reflection;

namespace akarnokd_misc_dotnet
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine(Environment.Version);
            Console.WriteLine(GetNetCoreVersion());

            ScrabbleBenchmarks();

            Console.WriteLine("Done... Press ENTER to quit");
            Console.ReadLine();
        }

        public static string GetNetCoreVersion()
        {
            var assembly = typeof(System.Runtime.GCSettings).GetTypeInfo().Assembly;
            var assemblyPath = assembly.CodeBase.Split(new[] { '/', '\\' }, StringSplitOptions.RemoveEmptyEntries);
            int netCoreAppIndex = Array.IndexOf(assemblyPath, "Microsoft.NETCore.App");
            if (netCoreAppIndex > 0 && netCoreAppIndex < assemblyPath.Length - 2)
                return assemblyPath[netCoreAppIndex + 1];
            return null;
        }

        static void mpsclTest()
        {
            string str = null;

            for (int i = 1; i < 10; i++)
            {
                var q = new MpscLinkedArrayQueue<string>(i);

                Console.WriteLine("Empty: " + q.IsEmpty());

                for (int j = 0; j <= i; j++)
                {
                    q.Enqueue(j.ToString());
                    Console.WriteLine("Enqueue: " + j + ", Empty: " + q.IsEmpty());
                }

                for (int j = 0; j <= i + 1; j++)
                {
                    Console.WriteLine("Dequeue: " + q.TryDequeue(out str) + " " + str + ", Empty: " + q.IsEmpty());
                }
            }
        }

        static void threadWorkerTest()
        {
            var tw = new ThreadWorker();

            for (int i = 0; i < 1000000; i++)
            {
                int j = i;
                tw.Schedule(() => {
                    if (j % 100000 == 0)
                    {
                        Console.WriteLine(j);
                    }
                });
            }

            Thread.Sleep(2000);

            for (int i = 0; i < 1000000; i++)
            {
                int j = i;
                tw.Schedule(() => {
                    if (j % 100000 == 0)
                    {
                        Console.WriteLine(j);
                    }
                });
            }

            Thread.Sleep(2000);

        }

        static void benchmarkHolder()
        {
            BenchmarkHeader();

            ScrabbleBenchmarks();

            //ReactorBenchmarks();

            //RxBenchmarks();

            //RxFastRangeBenchmarks();

        }

        static void BenchmarkHeader()
        {
            Console.WriteLine("Benchmark (lib) (count) Mode Cnt Score Error Unit");
        }

        static void ReactorBenchmarks()
        {
            int[] count = { 1, 10, 100, 1000, 10000, 100000, 1000000 };

            Benchmarking.Benchmark(5, c => ReactorCoreBenchmarks.Range(c), 
                "Range", "Reactor", count);

            Benchmarking.Benchmark(5, c => ReactorCoreBenchmarks.RangeAsync(c),
                "RangeAsync", "Reactor", count);

            Benchmarking.Benchmark(5, c => ReactorCoreBenchmarks.RangePipeline(c),
                "RangePipeline", "Reactor", count);

            Benchmarking.Benchmark(5, c => ReactorCoreBenchmarks.FlatMapJust(c),
                "FlatMapJust", "Reactor", count);

            Benchmarking.Benchmark(5, c => ReactorCoreBenchmarks.FlatMapRange(c),
                "FlatMapRange", "Reactor", count);

            Benchmarking.Benchmark(5, c => ReactorCoreBenchmarks.FlatMapXRange(c),
                "FlatMapXRange", "Reactor", count);

            Benchmarking.Benchmark(5, c => ReactorCoreBenchmarks.ConcatMapJust(c),
                "ConcatMapJust", "Reactor", count);

            Benchmarking.Benchmark(5, c => ReactorCoreBenchmarks.FlatMapRange(c),
                "ConcatMapRange", "Reactor", count);

            Benchmarking.Benchmark(5, c => ReactorCoreBenchmarks.FlatMapXRange(c),
                "ConcatMapXRange", "Reactor", count);

        }

        static void RxBenchmarks()
        {
            int[] count = { 1, 10, 100, 1000, 10000, 100000, 1000000 };

            Benchmarking.Benchmark(5, c => RxNETBenchmarks.Range(c),
                "Range", "Rx.NET", count);

            Benchmarking.Benchmark(5, c => RxNETBenchmarks.RangeAsync(c),
                "RangeAsync", "Rx.NET", count);

            Benchmarking.Benchmark(5, c => RxNETBenchmarks.RangePipeline(c),
                "RangePipeline", "Rx.NET", count);

            Benchmarking.Benchmark(5, c => RxNETBenchmarks.FlatMapJust(c),
                "FlatMapJust", "Rx.NET", count);

            Benchmarking.Benchmark(5, c => RxNETBenchmarks.FlatMapRange(c),
                "FlatMapRange", "Rx.NET", count);

            Benchmarking.Benchmark(5, c => RxNETBenchmarks.FlatMapXRange(c),
                "FlatMapXRange", "Rx.NET", count);

            Benchmarking.Benchmark(5, c => RxNETBenchmarks.ConcatMapJust(c),
                "ConcatMapJust", "Rx.NET", count);

            Benchmarking.Benchmark(5, c => RxNETBenchmarks.FlatMapRange(c),
                "ConcatMapRange", "Rx.NET", count);

            Benchmarking.Benchmark(5, c => RxNETBenchmarks.FlatMapXRange(c),
                "ConcatMapXRange", "Rx.NET", count);

        }

        static void RxFastRangeBenchmarks()
        {
            int[] count = { 1, 10, 100, 1000, 10000, 100000, 1000000 };

            Benchmarking.Benchmark(5, c => RxNETBenchmarksFastRange.Range(c),
                "Range", "RxFr.NET", count);

            Benchmarking.Benchmark(5, c => RxNETBenchmarksFastRange.RangeAsync(c),
                "RangeAsync", "RxFr.NET", count);

            Benchmarking.Benchmark(5, c => RxNETBenchmarksFastRange.RangePipeline(c),
                "RangePipeline", "RxFr.NET", count);

            Benchmarking.Benchmark(5, c => RxNETBenchmarksFastRange.FlatMapJust(c),
                "FlatMapJust", "RxFr.NET", count);

            Benchmarking.Benchmark(5, c => RxNETBenchmarksFastRange.FlatMapRange(c),
                "FlatMapRange", "RxFr.NET", count);

            Benchmarking.Benchmark(5, c => RxNETBenchmarksFastRange.FlatMapXRange(c),
                "FlatMapXRange", "RxFr.NET", count);

            Benchmarking.Benchmark(5, c => RxNETBenchmarksFastRange.ConcatMapJust(c),
                "ConcatMapJust", "RxFr.NET", count);

            Benchmarking.Benchmark(5, c => RxNETBenchmarksFastRange.FlatMapRange(c),
                "ConcatMapRange", "RxFr.NET", count);

            Benchmarking.Benchmark(5, c => RxNETBenchmarksFastRange.FlatMapXRange(c),
                "ConcatMapXRange", "RxFr.NET", count);

        }

        static void ScrabbleBenchmarks()
        {
            ShakespearePlaysScrabble.Init();
            /*
            Console.WriteLine("ShakespearePlaysScrabbleReactive4NET");
            PrintResults(ShakespearePlaysScrabbleReactive4NET.Run());

            Console.WriteLine("ShakespearePlaysScrabbleReactorCore");
            PrintResults(ShakespearePlaysScrabbleReactorCore.Run());
            */
            Console.WriteLine("ShakespearePlaysScrabbleRxNET");
            PrintResults(ShakespearePlaysScrabbleRxNET.Run());
            /*
            Console.WriteLine("ShakespearePlaysScrabbleIxNET");
            PrintResults(ShakespearePlaysScrabbleIxNET.Run());

            Console.WriteLine("ShakespearePlaysScrabbleOx");
            PrintResults(ShakespearePlaysScrabbleOx.Run());

            Console.WriteLine("ShakespearePlaysScrabbleIx");
            PrintResults(ShakespearePlaysScrabbleIx.Run());

            Console.WriteLine("ShakespearePlaysScrabbleObservableSource");
            PrintResults(ShakespearePlaysScrabbleObservableSource.Run());

            Console.WriteLine("ShakespearePlaysScrabbleForLoop");
            PrintResults(ShakespearePlaysScrabbleForLoop.Run());

            Console.WriteLine("ShakespearePlaysScrabbleSyncObservable");
            PrintResults(ShakespearePlaysScrabbleSyncObservable.Run());
            */

            Console.WriteLine("ShakespearePlaysScrabbleUniRx");
            PrintResults(ShakespearePlaysScrabbleUniRx.Run());

            Console.WriteLine("Benchmarking...");

            //Benchmarking.Benchmark(5, () => ShakespearePlaysScrabbleReactive4NET.Run(), "ShakespearePlaysScrabbleReactive4NET");

            Benchmarking.Benchmark(5, () => ShakespearePlaysScrabbleRxNET.Run(), "ShakespearePlaysScrabbleRxNET");

            //Benchmarking.Benchmark(5, () => ShakespearePlaysScrabbleIxNET.Run(), "ShakespearePlaysScrabbleIxNET");

            //Benchmarking.Benchmark(5, () => ShakespearePlaysScrabbleReactorCore.Run(), "ShakespearePlaysScrabbleReactorCore");

            //Benchmarking.Benchmark(5, () => ShakespearePlaysScrabbleOx.Run(), "ShakespearePlaysScrabbleOx");

            //Benchmarking.Benchmark(5, () => ShakespearePlaysScrabbleObservableSource.Run(), "ShakespearePlaysScrabbleObservableSource");

            //Benchmarking.Benchmark(5, () => ShakespearePlaysScrabbleIx.Run(), "ShakespearePlaysScrabbleIx");

            //Benchmarking.Benchmark(5, () => ShakespearePlaysScrabbleForLoop.Run(), "ShakespearePlaysScrabbleForLoop");

            //Benchmarking.Benchmark(5, () => ShakespearePlaysScrabbleSyncObservable.Run(), "ShakespearePlaysScrabbleSyncObservable");

            Benchmarking.Benchmark(5, () => ShakespearePlaysScrabbleUniRx.Run(), "ShakespearePlaysScrabbleUniRx");
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

    }
}
