using System;
using System.Collections.Generic;
//using System.Linq;

using Reactor.Core;
using akarnokd_misc_dotnet.ix;
using BenchmarkDotNet.Attributes;

namespace akarnokd_misc_dotnet
{
    [MemoryDiagnoser]
    class ShakespearePlaysScrabbleIx : ShakespearePlaysScrabble
    {
        [Benchmark]
        public object Ixx()
        {
            return Run();
        }

        static IEnumerable<int> chars(string s)
        {
            return Ix.Characters(s);
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

            Func<string, IEnumerable<long>> nBlanks = word =>
                histoOfLetters(word)
                .FlatMap(map => map.AsEnumerable())
                .Map(blank)
                .Reduce((a, b) => a + b)
                ;

            Func<string, IEnumerable<bool>> checkBlanks = word =>
                nBlanks(word).Map(v => v <= 2);

            Func<string, IEnumerable<int>> score2 = word =>
                histoOfLetters(word)
                .FlatMap(map => map.AsEnumerable())
                .Map(letterScore)
                .Reduce((a, b) => a + b);

            Func<string, IEnumerable<int>> first3 = word =>
                chars(word).Take(3);

            Func<string, IEnumerable<int>> last3 = word =>
                chars(word).Skip(3);

            Func<string, IEnumerable<int>> toBeMaxed = word =>
                Ix.Concat(first3(word), last3(word));

            Func<string, IEnumerable<int>> bonusForDoubleLetter = word =>
                toBeMaxed(word)
                .Map(scoreOfALetter)
                .Reduce((a, b) => Math.Max(a, b));

            Func<string, IEnumerable<int>> score3 = word =>
                Ix.Concat(
                    score2(word).Map(v => v * 2),
                    bonusForDoubleLetter(word).Map(v => v * 2),
                    Ix.Just(word.Length == 7 ? 50 : 0)
                )
                .Reduce((a, b) => a + b);

            Func<Func<string, IEnumerable<int>>, IEnumerable<SortedDictionary<int, IList<string>>>> buildHistoOnScore = score =>
                shakespeareWords
                .Filter(word => scrabbleWords.Contains(word))
                .Filter(word => checkBlanks(word).First())
                .Collect(
                    () => new SortedDictionary<int, IList<string>>(IntReverse),
                    (map, word) => {
                        int key = score(word).First();
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
                .First();

            return finalList2;
        }
    }
}
