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

namespace akarnokd_misc_dotnet.observablex
{
    public static class Ox
    {
        public static IObservableX<int> Characters(string s)
        {
            return new OxFromString(s);
        }

        public static IObservableX<T> Just<T>(T value)
        {
            return new OxJust<T>(value);
        }

        public static IObservableX<R> Map<T, R>(this IObservableX<T> source, Func<T, R> mapper)
        {
            return new OxMap<T, R>(source, mapper);
        }

        public static IObservableX<T> Filter<T>(this IObservableX<T> source, Func<T, bool> predicate)
        {
            return new OxFilter<T>(source, predicate);
        }

        public static IObservableX<R> Reduce<T, R>(this IObservableX<T> source, Func<R> initialFactory, Func<R, T, R> reducer)
        {
            return new OxReduce<T, R>(source, initialFactory, reducer);
        }

        public static IObservableX<T> Reduce<T>(this IObservableX<T> source, Func<T, T, T> reducer)
        {
            return new OxReduce<T>(source, reducer);
        }

        public static IObservableX<int> Sum(this IObservableX<int> source)
        {
            return Reduce(source, (a, b) => a + b);
        }

        public static IObservableX<int> Max(this IObservableX<int> source)
        {
            return Reduce(source, (a, b) => Math.Max(a, b));
        }

        public static IObservableX<R> Collect<T, R>(this IObservableX<T> source, Func<R> initialFactory, Action<R, T> collector)
        {
            return new OxCollect<T, R>(source, initialFactory, collector);
        }

        public static IObservableX<T> Take<T>(this IObservableX<T> source, long n)
        {
            return new OxTake<T>(source, n);
        }

        public static IObservableX<T> Skip<T>(this IObservableX<T> source, long n)
        {
            return new OxSkip<T>(source, n);
        }

        public static IObservableX<R> FlatMap<T, R>(this IObservableX<T> source, Func<T, IEnumerable<R>> mapper)
        {
            return new OxFlattenEnumerable<T, R>(source, mapper);
        }

        public static IObservableX<T> Concat<T>(params IObservableX<T>[] sources)
        {
            return new OxConcat<T>(sources);
        }

        public static T Block<T>(this IObservableX<T> source)
        {
            BlockingObserverX<T> o = new BlockingObserverX<T>();
            source.Subscribe(o);
            return o.Get();
        }

        public static IObservableX<T> From<T>(IEnumerable<T> source)
        {
            return new OxFromEnumerable<T>(source);
        }
    }
}
