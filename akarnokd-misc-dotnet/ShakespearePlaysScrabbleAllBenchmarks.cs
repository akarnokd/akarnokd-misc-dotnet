using BenchmarkDotNet.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace akarnokd_misc_dotnet
{
    [MemoryDiagnoser]
    public class ShakespearePlaysScrabbleAllBenchmarks
    {
        [GlobalSetup]
        public void Setup()
        {
            ShakespearePlaysScrabble.Init();
        }

        [Benchmark]
        public object AsyncEnum()
        {
            return ShakespearePlaysScrabbleAsyncEnum.Run();
        }

        [Benchmark]
        public object ForLoop()
        {
            return ShakespearePlaysScrabbleForLoop.Run();
        }

        [Benchmark]
        public object Ixx()
        {
            return ShakespearePlaysScrabbleIx.Run();
        }

        [Benchmark]
        public object IxNET()
        {
            return ShakespearePlaysScrabbleIxNET.Run();
        }

        [Benchmark]
        public object ObservableSrc()
        {
            return ShakespearePlaysScrabbleObservableSource.Run();
        }

        [Benchmark]
        public object ObservableX()
        {
            return ShakespearePlaysScrabbleOx.Run();
        }

        [Benchmark]
        public object Reactive4NET()
        {
            return ShakespearePlaysScrabbleReactive4NET.Run();
        }

        [Benchmark]
        public object ReactorCore()
        {
            return ShakespearePlaysScrabbleReactorCore.Run();
        }

        [Benchmark]
        public object RxNET()
        {
            return ShakespearePlaysScrabbleRxNET.Run();
        }

        [Benchmark]
        public object ObservableSync()
        {
            return ShakespearePlaysScrabbleSyncObservable.Run();
        }

        [Benchmark]
        public object UniRx()
        {
            return ShakespearePlaysScrabbleUniRx.Run();
        }
    }
}
