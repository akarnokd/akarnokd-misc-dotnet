using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace akarnokd_misc_dotnet
{
    class Program
    {
        static void Main(string[] args)
        {

            ReactorBenchmarks();

            RxBenchmarks();

            Console.WriteLine("Done... Press ENTER to quit");
            Console.ReadLine();
        }

        static void ReactorBenchmarks()
        {
            int[] count = { 1, 10, 100, 1000, 10000, 100000, 1000000 };

            Benchmarking.Benchmark(5, c => ReactorCoreBenchmarks.Range(c), 
                "ReactorCoreBenchmarks.Range", count);

            Benchmarking.Benchmark(5, c => ReactorCoreBenchmarks.RangeAsync(c),
                "ReactorCoreBenchmarks.RangeAsync", count);

            Benchmarking.Benchmark(5, c => ReactorCoreBenchmarks.RangePipeline(c),
                "ReactorCoreBenchmarks.RangePipeline", count);

            Benchmarking.Benchmark(5, c => ReactorCoreBenchmarks.FlatMapJust(c),
                "ReactorCoreBenchmarks.FlatMapJust", count);

            Benchmarking.Benchmark(5, c => ReactorCoreBenchmarks.FlatMapRange(c),
                "ReactorCoreBenchmarks.FlatMapRange", count);

            Benchmarking.Benchmark(5, c => ReactorCoreBenchmarks.FlatMapXRange(c),
                "ReactorCoreBenchmarks.FlatMapXRange", count);

            Benchmarking.Benchmark(5, c => ReactorCoreBenchmarks.ConcatMapJust(c),
                "ReactorCoreBenchmarks.ConcatMapJust", count);

            Benchmarking.Benchmark(5, c => ReactorCoreBenchmarks.FlatMapRange(c),
                "ReactorCoreBenchmarks.ConcatMapRange", count);

            Benchmarking.Benchmark(5, c => ReactorCoreBenchmarks.FlatMapXRange(c),
                "ReactorCoreBenchmarks.ConcatMapXRange", count);

        }

        static void RxBenchmarks()
        {
            int[] count = { 1, 10, 100, 1000, 10000, 100000, 1000000 };

            Benchmarking.Benchmark(5, c => RxNETBenchmarks.Range(c),
                "RxNETBenchmarks.Range", count);

            Benchmarking.Benchmark(5, c => RxNETBenchmarks.RangeAsync(c),
                "RxNETBenchmarks.RangeAsync", count);

            Benchmarking.Benchmark(5, c => RxNETBenchmarks.RangePipeline(c),
                "RxNETBenchmarks.RangePipeline", count);

            Benchmarking.Benchmark(5, c => RxNETBenchmarks.FlatMapJust(c),
                "RxNETBenchmarks.FlatMapJust", count);

            Benchmarking.Benchmark(5, c => RxNETBenchmarks.FlatMapRange(c),
                "RxNETBenchmarks.FlatMapRange", count);

            Benchmarking.Benchmark(5, c => RxNETBenchmarks.FlatMapXRange(c),
                "RxNETBenchmarks.FlatMapXRange", count);

            Benchmarking.Benchmark(5, c => RxNETBenchmarks.ConcatMapJust(c),
                "RxNETBenchmarks.ConcatMapJust", count);

            Benchmarking.Benchmark(5, c => RxNETBenchmarks.FlatMapRange(c),
                "RxNETBenchmarks.ConcatMapRange", count);

            Benchmarking.Benchmark(5, c => RxNETBenchmarks.FlatMapXRange(c),
                "RxNETBenchmarks.ConcatMapXRange", count);

        }

        static void ScrabbleBenchmarks()
        {
            ShakespearePlaysScrabble.Init();

            PrintResults(ShakespearePlaysScrabbleReactorCore.Run());

            PrintResults(ShakespearePlaysScrabbleRxNET.Run());

            PrintResults(ShakespearePlaysScrabbleIxNET.Run());

            Console.WriteLine("Benchmarking...");

            Benchmarking.Benchmark(5, () => ShakespearePlaysScrabbleRxNET.Run(), "ShakespearePlaysScrabbleRxNET");

            Benchmarking.Benchmark(5, () => ShakespearePlaysScrabbleIxNET.Run(), "ShakespearePlaysScrabbleIxNET");

            Benchmarking.Benchmark(5, () => ShakespearePlaysScrabbleReactorCore.Run(), "ShakespearePlaysScrabbleReactorCore");
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
