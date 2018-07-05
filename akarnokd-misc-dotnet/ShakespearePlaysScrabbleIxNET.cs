using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Reactive.Streams;
using System.Threading;
using Reactor.Core.flow;
using Reactor.Core.subscriber;
using Reactor.Core.subscription;
using Reactor.Core.util;
using System.Linq.Expressions;
using BenchmarkDotNet.Attributes;

namespace akarnokd_misc_dotnet
{

    [MemoryDiagnoser]
    class ShakespearePlaysScrabbleIxNET : ShakespearePlaysScrabble
    {
        [Benchmark]
        public object IxNET()
        {
            return Run();
        }

        static IEnumerable<int> chars(string s)
        {
            for (int i = 0; i < s.Length; i++)
            {
                yield return s[i];
            }
        }

        static IEnumerable<T> Return<T>(T value)
        {
            yield return value;
        }

        static IEnumerable<T> Concat<T>(params IEnumerable<T>[] sources)
        {
            foreach (var ie in sources)
            {
                foreach (var e in ie) {
                    yield return e;
                }
            }
        }

        internal static IList<KeyValuePair<int, IList<string>>> Run()
        {
            Func<int, int> scoreOfALetter = letter => letterScores[letter - 'a'];

            Func<KeyValuePair<int, MutableInt>, int> letterScore = entry =>
                letterScores[entry.Key - 'a']
                * Math.Min(entry.Value.value, scrabbleAvailableLetters[entry.Key - 'a']);

            Func<string, IEnumerable<int>> toIntegerFlux = str => chars(str);

            Func<string, IEnumerable<Dictionary<int, MutableInt>>> histoOfLetters =
                word => toIntegerFlux(word)
                        .Reduce<int, Dictionary<int, MutableInt>>(
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

            Func<string, IEnumerable<long>> nBlanks = word =>
                histoOfLetters(word)
                .SelectMany(map => map.AsEnumerable())
                .Select(blank)
                .Reduce((a, b) => a + b)
                ;

            Func<string, IEnumerable<bool>> checkBlanks = word =>
                nBlanks(word).Select(v => v <= 2);

            Func<string, IEnumerable<int>> score2 = word =>
                histoOfLetters(word)
                .SelectMany(map => map.AsEnumerable())
                .Select(letterScore)
                .Reduce((a, b) => a + b);

            Func<string, IEnumerable<int>> first3 = word =>
                chars(word).Take(3);

            Func<string, IEnumerable<int>> last3 = word =>
                chars(word).Skip(3);

            Func<string, IEnumerable<int>> toBeMaxed = word =>
                Enumerable.Concat(first3(word), last3(word));

            Func<string, IEnumerable<int>> bonusForDoubleLetter = word =>
                toBeMaxed(word)
                .Select(scoreOfALetter)
                .Reduce((a, b) => Math.Max(a, b));

            Func<string, IEnumerable<int>> score3 = word =>
                Concat(
                    score2(word).Select(v => v * 2),
                    bonusForDoubleLetter(word).Select(v => v * 2),
                    Return(word.Length == 7 ? 50 : 0)
                )
                .Reduce((a, b) => a + b);

            Func<Func<string, IEnumerable<int>>, IEnumerable<SortedDictionary<int, IList<string>>>> buildHistoOnScore = score =>
                shakespeareWords.AsEnumerable()
                .Where(word => scrabbleWords.Contains(word))
                .Where(word => {
                    return checkBlanks(word).First();
                })
                .Reduce<string, SortedDictionary<int, IList<string>>>(
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
                .Reduce<KeyValuePair<int, IList<string>>, List<KeyValuePair<int, IList<string>>>>(
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

            return finalList2;
        }
    }

    public static class Ixx
    {
        public static IEnumerable<T> Reduce<T>(this IEnumerable<T> source, Func<T, T, T> aggregator)
        {
            T accumulator = default(T);

            bool first = true;

            foreach (var e in source)
            {
                if (first)
                {
                    accumulator = e;
                    first = false;
                }
                else
                {
                    accumulator = aggregator(accumulator, e);
                }
            }

            if (first)
            {
                yield break;
            }
            yield return accumulator;
        }

        public static IEnumerable<A> Reduce<T, A>(this IEnumerable<T> source, A initial, Func<A, T, A> aggregator)
        {
            A value = initial;

            foreach (var e in source)
            {
                value = aggregator(value, e);
            }
            yield return value;
        }
    }
}
