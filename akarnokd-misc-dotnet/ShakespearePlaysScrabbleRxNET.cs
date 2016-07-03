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
using System.Reactive.Linq;

namespace akarnokd_misc_dotnet
{
    class ShakespearePlaysScrabbleRxNET : ShakespearePlaysScrabble
    {


        static IObservable<int> chars(string s)
        {
            return Observable.Create<int>(o =>
            {
                for (int i = 0; i < s.Length; i++)
                {
                    o.OnNext(s[i]);
                }
                o.OnCompleted();
                return () => { };
            });
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
                .Aggregate((a, b) => a + b)
                ;

            Func<string, IObservable<bool>> checkBlanks = word =>
                nBlanks(word).Select(v => v <= 2);

            Func<string, IObservable<int>> score2 = word =>
                histoOfLetters(word)
                .SelectMany(map => map.AsEnumerable())
                .Select(letterScore)
                .Aggregate((a, b) => a + b);

            Func<string, IObservable<int>> first3 = word =>
                chars(word).Take(3);

            Func<string, IObservable<int>> last3 = word =>
                chars(word).Skip(3);

            Func<string, IObservable<int>> toBeMaxed = word =>
                Observable.Concat(first3(word), last3(word));

            Func<string, IObservable<int>> bonusForDoubleLetter = word =>
                toBeMaxed(word)
                .Select(scoreOfALetter)
                .Aggregate((a, b) => Math.Max(a, b));

            Func<string, IObservable<int>> score3 = word =>
                Observable.Concat(
                    score2(word),
                    score2(word),
                    bonusForDoubleLetter(word),
                    bonusForDoubleLetter(word),
                    Observable.Return(word.Length == 7 ? 50 : 0)
                )
                .Aggregate((a, b) => a + b);

            Func<Func<string, IObservable<int>>, IObservable<SortedDictionary<int, IList<string>>>> buildHistoOnScore = score =>
                Observable.ToObservable(shakespeareWords.AsEnumerable())
                .Where(word => scrabbleWords.Contains(word))
                .Where(word => {
                    return checkBlanks(word).ToEnumerable().First();
                })
                .Aggregate<string, SortedDictionary<int, IList<string>>>(
                    null,
                    (map, word) => {
                        if (map == null)
                        {
                            map = new SortedDictionary<int, IList<string>>(IntReverse);
                        }

                        int key = score(word).ToEnumerable().First();
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
                .ToEnumerable().First();

            return finalList2;
        }
    }
}
