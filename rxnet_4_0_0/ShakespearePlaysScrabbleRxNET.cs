using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Threading;
using System.Reactive.Linq;
using System.Reactive.Disposables;
using BenchmarkDotNet.Attributes;

namespace rxnet_3
{
    [MemoryDiagnoser]
    public class ShakespearePlaysScrabbleRxNET : ShakespearePlaysScrabble
    {
        [Benchmark]
        public object RxNET()
        {
            return Run();
        }

        static IObservable<int> chars(string s)
        {
            return new CharObservable(s);
        }

        sealed class CharObservable : IObservable<int>
        {
            readonly string str;

            public CharObservable(string str)
            {
                this.str = str;
            }

            public IDisposable Subscribe(IObserver<int> observer)
            {
                for (int i = 0; i < str.Length; i++)
                {
                    observer.OnNext((int)str[i]);
                }
                observer.OnCompleted();
                return Disposable.Empty;
            }
        }

        internal static IList<KeyValuePair<int, IList<string>>> Run()
        {
            Func<int, int> scoreOfALetter = letter => letterScores[letter - 'a'];

            Func<KeyValuePair<int, MutableInt>, int> letterScore = entry =>
                letterScores[entry.Key - 'a']
                * Math.Min(entry.Value.value, scrabbleAvailableLetters[entry.Key - 'a']);

            Func<string, IObservable<int>> toIntegerFlux = str => chars(str);

            Func<string, IObservable<Dictionary<int, MutableInt>>> histoOfLetters =
                word => toIntegerFlux(word)
                        .Aggregate<int, Dictionary<int, MutableInt>>(
                            null,
                            (m, value) =>
                            {
                                if (m == null)
                                {
                                    m = new Dictionary<int, MutableInt>();
                                }

                                MutableInt mi;

                                if (!m.TryGetValue(value, out mi))
                                { 
                                    mi = new MutableInt();
                                    m.Add(value, mi);
                                }

                                mi.value++;
                                return m;
                            }
                        );

            Func<KeyValuePair<int, MutableInt>, long> blank = entry =>
                Math.Max(0L, entry.Value.value - scrabbleAvailableLetters[entry.Key - 'a']);

            Func<string, IObservable<long>> nBlanks = word =>
                histoOfLetters(word)
                .SelectMany(map => map.AsEnumerable())
                .Select(blank)
                .Sum()
                ;

            Func<string, IObservable<bool>> checkBlanks = word =>
                nBlanks(word).Select(v => v <= 2);

            Func<string, IObservable<int>> score2 = word =>
                histoOfLetters(word)
                .SelectMany(map => map.AsEnumerable())
                .Select(letterScore)
                .Sum();

            Func<string, IObservable<int>> first3 = word =>
                chars(word).Take(3);

            Func<string, IObservable<int>> last3 = word =>
                chars(word).Skip(3);

            Func<string, IObservable<int>> toBeMaxed = word =>
                Observable.Concat(first3(word), last3(word));

            Func<string, IObservable<int>> bonusForDoubleLetter = word =>
                toBeMaxed(word)
                .Select(scoreOfALetter)
                .Max();

            Func<string, IObservable<int>> score3 = word =>
                Observable.Concat(
                    score2(word).Select(v => v * 2),
                    bonusForDoubleLetter(word).Select(v => v * 2),
                    Observable.Return(word.Length == 7 ? 50 : 0)
                )
                .Sum();

#pragma warning disable CS0618 // Type or member is obsolete
            Func<Func<string, IObservable<int>>, IObservable<SortedDictionary<int, IList<string>>>> buildHistoOnScore = score =>
                Observable.ToObservable(shakespeareWords.AsEnumerable())
                .Where(word => scrabbleWords.Contains(word))
                .Where(word => {
                    return checkBlanks(word).First();
                })
                .Aggregate<string, SortedDictionary<int, IList<string>>>(
                    null,
                    (map, word) => {
                        if (map == null)
                        {
                            map = new SortedDictionary<int, IList<string>>(IntReverse);
                        }

                        int key = score(word).First();
                        IList<string> list;
                        if (!map.TryGetValue(key, out list))
                        {
                            list = new List<string>();
                            map.Add(key, list);
                        }
                        list.Add(word);
                        return map;
                    }
                )
                ;

            IList<KeyValuePair<int, IList<string>>> finalList2 =
                buildHistoOnScore(score3)
                .SelectMany(map => map.AsEnumerable())
                .Take(3)
                .Aggregate< KeyValuePair<int, IList<string>>, List<KeyValuePair<int, IList<string>>>>(
                    null,
                    (list, entry) =>
                    {
                        if (list == null)
                        {
                            list = new List<KeyValuePair<int, IList<string>>>();
                        }
                        list.Add(entry);
                        return list;
                    }
                )
                .First();
#pragma warning restore CS0618 // Type or member is obsolete

            return finalList2;
        }
    }
    /*
    internal static class RxNET
    {
        internal static T FirstBlocking<T>(this IObservable<T> source)
        {
            var parent = new FirstBlockingObserver<T>();

            parent.SetDisposable(source.Subscribe(parent));

            return parent.Get();
        }

        sealed class FirstBlockingObserver<T> : CountdownEvent, IObserver<T>
        {
            int once;

            T value;
            bool hasValue;
            Exception error;

            IDisposable upstream;

            public FirstBlockingObserver() : base(1)
            {

            }

            internal void SetDisposable(IDisposable d)
            {
                DisposableHelper.Replace(ref upstream, d);
            }

            void Unblock()
            {
                if (Interlocked.CompareExchange(ref once, 1, 0) == 0)
                {
                    Signal();
                }
            }

            public void OnCompleted()
            {
                Unblock();
            }

            public void OnError(Exception error)
            {
                value = default(T);
                this.error = error;
            }

            public void OnNext(T value)
            {
                if (!hasValue)
                {
                    DisposableHelper.Dispose(ref upstream);
                    hasValue = true;
                    this.value = value;
                    Unblock();
                }
            }

            internal T Get()
            {
                if (CurrentCount != 0)
                {
                    Wait();
                }

                var ex = error;
                if (ex != null)
                {
                    throw ex;
                }
                if (hasValue)
                {
                    return value;
                }
                throw new IndexOutOfRangeException();
            }
        }

        internal static readonly IDisposable Empty = new EmptyDisposable();

        sealed class EmptyDisposable : IDisposable
        {
            public void Dispose()
            {
                // no op
            }
        }

        internal static IObservable<R> Reduce<T, R>(this IObservable<T> source, R initialValue, Func<R, T, R> reducer)
        {
            return new ObservableReduce<T, R>(source, initialValue, reducer);
        }

        sealed class ObservableReduce<T, R> : IObservable<R>
        {
            readonly IObservable<T> source;

            readonly R initialValue;

            readonly Func<R, T, R> reducer;

            public ObservableReduce(IObservable<T> source, R initialValue, Func<R, T, R> reducer)
            {
                this.source = source;
                this.initialValue = initialValue;
                this.reducer = reducer;
            }

            public IDisposable Subscribe(IObserver<R> observer)
            {
                var parent = new ReduceObserver(observer, initialValue, reducer);
                var d = source.Subscribe(parent);
                parent.SetUpstream(d);
                return parent;
            }

            sealed class ReduceObserver : IObserver<T>, IDisposable
            {
                readonly IObserver<R> downstream;

                readonly Func<R, T, R> reducer;

                R value;

                IDisposable upstream;

                bool done;

                public ReduceObserver(IObserver<R> downstream, R value, Func<R, T, R> reducer)
                {
                    this.downstream = downstream;
                    this.value = value;
                    this.reducer = reducer;
                }

                public void Dispose()
                {
                    DisposableHelper.Dispose(ref upstream);
                }

                internal void SetUpstream(IDisposable d)
                {
                    DisposableHelper.Replace(ref upstream, d);
                }

                public void OnCompleted()
                {
                    if (done)
                    {
                        return;
                    }
                    downstream.OnNext(value);
                    downstream.OnCompleted();
                }

                public void OnError(Exception error)
                {
                    if (done)
                    {
                        return;
                    }
                    value = default;
                    downstream.OnError(error);
                }

                public void OnNext(T item)
                {
                    if (done)
                    {
                        return;
                    }
                    try
                    {
                        value = reducer(value, item);
                    }
                    catch (Exception ex)
                    {
                        Dispose();
                        done = true;
                        downstream.OnError(ex);
                    }
                }
            }
        }
    }
*/

}
