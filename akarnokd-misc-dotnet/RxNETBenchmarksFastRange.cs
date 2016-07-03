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
    internal static class RxNETBenchmarksFastRange
    {
        sealed class RangeObservable : IObservable<int>, IDisposable
        {
            readonly int start;
            readonly int count;

            internal RangeObservable(int start, int count)
            {
                this.start = start;
                this.count = count;
            }

            public IDisposable Subscribe(IObserver<int> observer)
            {
                int e = start + count;
                for (int i = start; i != e; i++)
                {
                    observer.OnNext(i);
                }
                observer.OnCompleted();
                return this;
            }

            public void Dispose()
            {
                // ignored
            }
        }

        static IObservable<int> Range(int start, int count)
        {
            return new RangeObservable(start, count);
        }

        internal static object Range(int c)
        {
            return Range(1, c).Last();
        }

        internal static object RangeAsync(int c)
        {
            return Range(1, c).ObserveOn(DefaultScheduler.Instance).Last();
        }

        internal static object RangePipeline(int c)
        {
            return Range(1, c)
                .SubscribeOn(DefaultScheduler.Instance)
                .ObserveOn(DefaultScheduler.Instance).Last();
        }

        internal static object FlatMapJust(int c)
        {
            return Range(1, c).SelectMany(v => Observable.Return(v)).Last();
        }

        internal static object FlatMapRange(int c)
        {
            return Range(1, c).SelectMany(v => Range(v, 2)).Last();
        }

        internal static object FlatMapXRange(int c)
        {
            int d = 1000000 / c;
            return Range(1, c).SelectMany(v => Range(v, d)).Last();
        }

        internal static object ConcatMapJust(int c)
        {
            return Observable.Concat(Range(1, c).Select(v => Observable.Return(v))).Last();
        }

        internal static object ConcatMapRange(int c)
        {
            return Observable.Concat(Range(1, c).Select(v => Range(v, 2))).Last();
        }

        internal static object ConcatMapXRange(int c)
        {
            int d = 1000000 / c;
            return Observable.Concat(Range(1, c).Select(v => Range(v, d))).Last();
        }
    }
}
