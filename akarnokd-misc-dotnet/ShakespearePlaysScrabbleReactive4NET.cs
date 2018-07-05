using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BenchmarkDotNet.Attributes;
using Reactive.Streams;
using Reactive4.NET;

namespace akarnokd_misc_dotnet
{
    [MemoryDiagnoser]
    class ShakespearePlaysScrabbleReactive4NET : ShakespearePlaysScrabble
    {
        [Benchmark]
        public object Reactive4NET()
        {
            return Run();
        }

        static IFlowable<int> chars(string s)
        {
            return Flowable.Range(0, s.Length).Map(i => (int)s[i]);
        }

        internal static IList<KeyValuePair<int, IList<string>>> Run()
        {
            Func<int, int> scoreOfALetter = letter => letterScores[letter - 'a'];

            Func<KeyValuePair<int, MutableInt>, int> letterScore = entry =>
                letterScores[entry.Key - 'a']
                * Math.Min(entry.Value.value, scrabbleAvailableLetters[entry.Key - 'a']);

            Func<string, IFlowable<int>> toIntegerFlowable = str => chars(str);

            Func<string, IFlowable<Dictionary<int, MutableInt>>> histoOfLetters =
                word => toIntegerFlowable(word)
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

            Func<string, IFlowable<long>> nBlanks = word =>
                histoOfLetters(word)
                .FlatMapEnumerable(map => map.AsEnumerable())
                .Map(blank)
                .Reduce((a, b) => a + b)
                ;

            Func<string, IFlowable<bool>> checkBlanks = word =>
                nBlanks(word).Map(v => v <= 2);

            Func<string, IFlowable<int>> score2 = word =>
                histoOfLetters(word)
                .FlatMapEnumerable(map => map.AsEnumerable())
                .Map(letterScore)
                .Reduce((a, b) => a + b);

            Func<string, IFlowable<int>> first3 = word =>
                chars(word).Take(3);

            Func<string, IFlowable<int>> last3 = word =>
                chars(word).Skip(3);

            Func<string, IFlowable<int>> toBeMaxed = word =>
                Flowable.Concat(first3(word), last3(word));

            Func<string, IFlowable<int>> bonusForDoubleLetter = word =>
                toBeMaxed(word)
                .Map(scoreOfALetter)
                .Reduce((a, b) => Math.Max(a, b));

            Func<string, IFlowable<int>> score3 = word =>
                Flowable.Concat(
                    score2(word).Map(v => v * 2),
                    bonusForDoubleLetter(word).Map(v => v * 2),
                    Flowable.Just(word.Length == 7 ? 50 : 0)
                )
                .Reduce((a, b) => a + b);

            Func<Func<string, IFlowable<int>>, IFlowable<SortedDictionary<int, IList<string>>>> buildHistoOnScore = score =>
                Flowable.FromEnumerable(shakespeareWords.AsEnumerable())
                .Filter(word => scrabbleWords.Contains(word))
                .Filter(word =>
                {
                    bool b;
                    if (checkBlanks(word).BlockingFirst(out b))
                    {
                        return b;
                    }
                    return false;
                })
                .Collect(
                    () => new SortedDictionary<int, IList<string>>(IntReverse),
                    (map, word) => {
                        int key;
                        if (score(word).BlockingFirst(out key))
                        {
                            IList<string> list;
                            if (!map.TryGetValue(key, out list))
                            {
                                list = new List<string>();
                                map.Add(key, list);
                            }
                            list.Add(word);
                        }
                    }
                )
                ;

            IList<KeyValuePair<int, IList<string>>> finalList2;
                buildHistoOnScore(score3)
                .FlatMapEnumerable(map => map.AsEnumerable())
                .Take(3)
                .Collect(
                    () => new List<KeyValuePair<int, IList<string>>>(),
                    (list, entry) =>
                    {
                        list.Add(entry);
                    }
                )
                .BlockingFirst(out finalList2);

            return finalList2;
        }
    }
}
