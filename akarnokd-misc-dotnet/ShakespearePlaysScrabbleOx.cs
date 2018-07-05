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
using akarnokd_misc_dotnet.observablex;
using BenchmarkDotNet.Attributes;

namespace akarnokd_misc_dotnet
{
    [MemoryDiagnoser]
    public class ShakespearePlaysScrabbleOx : ShakespearePlaysScrabble
    {
        [Benchmark]
        public object ObservableX()
        {
            return Run();
        }

        static IObservableX<int> chars(string s)
        {
            return Ox.Characters(s);
        }

        internal static IList<KeyValuePair<int, IList<string>>> Run()
        {
            Func<int, int> scoreOfALetter = letter => letterScores[letter - 'a'];

            Func<KeyValuePair<int, MutableInt>, int> letterScore = entry =>
                letterScores[entry.Key - 'a']
                * Math.Min(entry.Value.value, scrabbleAvailableLetters[entry.Key - 'a']);

            Func<string, IObservableX<int>> toIntegerFlux = str => chars(str);

            Func<string, IObservableX<Dictionary<int, MutableInt>>> histoOfLetters =
                word => toIntegerFlux(word)
                        .Collect(
                            () => new Dictionary<int, MutableInt>(),
                            (m, value) =>
                            {
                                MutableInt mi;

                                if (!m.TryGetValue(value, out mi))
                                { 
                                    mi = new MutableInt();
                                    m.Add(value, mi);
                                }

                                mi.value++;
                            }
                        );

            Func<KeyValuePair<int, MutableInt>, long> blank = entry =>
                Math.Max(0L, entry.Value.value - scrabbleAvailableLetters[entry.Key - 'a']);

            Func<string, IObservableX<long>> nBlanks = word =>
                histoOfLetters(word)
                .FlatMap(map => map.AsEnumerable())
                .Map(blank)
                .Reduce((a, b) => a + b)
                ;

            Func<string, IObservableX<bool>> checkBlanks = word =>
                nBlanks(word).Map(v => v <= 2);

            Func<string, IObservableX<int>> score2 = word =>
                histoOfLetters(word)
                .FlatMap(map => map.AsEnumerable())
                .Map(letterScore)
                .Reduce((a, b) => a + b);

            Func<string, IObservableX<int>> first3 = word =>
                chars(word).Take(3);

            Func<string, IObservableX<int>> last3 = word =>
                chars(word).Skip(3);

            Func<string, IObservableX<int>> toBeMaxed = word =>
                Ox.Concat(first3(word), last3(word));

            Func<string, IObservableX<int>> bonusForDoubleLetter = word =>
                toBeMaxed(word)
                .Map(scoreOfALetter)
                .Reduce((a, b) => Math.Max(a, b));

            Func<string, IObservableX<int>> score3 = word =>
                Ox.Concat(
                    score2(word).Map(v => v * 2),
                    bonusForDoubleLetter(word).Map(v => v * 2),
                    Ox.Just(word.Length == 7 ? 50 : 0)
                )
                .Reduce((a, b) => a + b);

            Func<Func<string, IObservableX<int>>, IObservableX<SortedDictionary<int, IList<string>>>> buildHistoOnScore = score =>
                Ox.From(shakespeareWords.AsEnumerable())
                .Filter(word => scrabbleWords.Contains(word))
                .Filter(word => checkBlanks(word).Block())
                .Collect(
                    () => new SortedDictionary<int, IList<string>>(IntReverse),
                    (map, word) => {
                        int key = score(word).Block();
                        IList<string> list;
                        if (!map.TryGetValue(key, out list))
                        {
                            list = new List<string>();
                            map.Add(key, list);
                        }
                        list.Add(word);
                    }
                )
                ;

            IList<KeyValuePair<int, IList<string>>> finalList2 =
                buildHistoOnScore(score3)
                .FlatMap(map => map.AsEnumerable())
                .Take(3)
                .Collect(
                    () => new List<KeyValuePair<int, IList<string>>>(),
                    (list, entry) =>
                    {
                        list.Add(entry);
                    }
                )
                .Block();

            return finalList2;
        }
    }
}
