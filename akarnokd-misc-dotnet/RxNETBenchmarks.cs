using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Reactive.Streams;
using System.Threading;
using System.Reactive.Linq;
using System.Reactive.Concurrency;

namespace akarnokd_misc_dotnet
{
    internal static class RxNETBenchmarks
    {
        internal static object Range(int c)
        {
            return Observable.Range(1, c).Last();
        }

        internal static object RangeAsync(int c)
        {
            return Observable.Range(1, c).ObserveOn(DefaultScheduler.Instance).Last();
        }

        internal static object RangePipeline(int c)
        {
            return Observable.Range(1, c, DefaultScheduler.Instance)
                .ObserveOn(DefaultScheduler.Instance).Last();
        }

        internal static object FlatMapJust(int c)
        {
            return Observable.Range(1, c).SelectMany(v => Observable.Return(v)).Last();
        }

        internal static object FlatMapRange(int c)
        {
            return Observable.Range(1, c).SelectMany(v => Observable.Range(v, 2)).Last();
        }

        internal static object FlatMapXRange(int c)
        {
            int d = 1000000 / c;
            return Observable.Range(1, c).SelectMany(v => Observable.Range(v, d)).Last();
        }

        internal static object ConcatMapJust(int c)
        {
            return Observable.Concat(Observable.Range(1, c).Select(v => Observable.Return(v))).Last();
        }

        internal static object ConcatMapRange(int c)
        {
            return Observable.Concat(Observable.Range(1, c).Select(v => Observable.Range(v, 2))).Last();
        }

        internal static object ConcatMapXRange(int c)
        {
            int d = 1000000 / c;
            return Observable.Concat(Observable.Range(1, c).Select(v => Observable.Range(v, d))).Last();
        }
    }
}
