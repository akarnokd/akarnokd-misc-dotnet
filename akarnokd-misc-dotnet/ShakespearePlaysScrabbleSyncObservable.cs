using System;
using System.Collections.Generic;
using System.Linq;
using akarnokd_misc_dotnet.syncobservable;
using BenchmarkDotNet.Attributes;

namespace akarnokd_misc_dotnet
{
    [MemoryDiagnoser]
    public class ShakespearePlaysScrabbleSyncObservable : ShakespearePlaysScrabble
    {
        [Benchmark]
        public object ObservableSync()
        {
            return Run();
        }

        static ISyncObservable<int> chars(string s)
        {
            return SyncObservable.Characters(s);
        }

        internal static IList<KeyValuePair<int, IList<string>>> Run()
        {
            Func<int, int> scoreOfALetter = letter => letterScores[letter - 'a'];

            Func<KeyValuePair<int, MutableInt>, int> letterScore = entry =>
                letterScores[entry.Key - 'a']
                * Math.Min(entry.Value.value, scrabbleAvailableLetters[entry.Key - 'a']);

            Func<string, ISyncObservable<int>> toIntegerFlux = str => chars(str);

            Func<string, ISyncObservable<Dictionary<int, MutableInt>>> histoOfLetters =
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

            Func<string, ISyncObservable<long>> nBlanks = word =>
                histoOfLetters(word)
                .ConcatMap(map => map.AsEnumerable())
                .Map(blank)
                .Reduce((a, b) => a + b)
                ;

            Func<string, ISyncObservable<bool>> checkBlanks = word =>
                nBlanks(word).Map(v => v <= 2);

            Func<string, ISyncObservable<int>> score2 = word =>
                histoOfLetters(word)
                .ConcatMap(map => map.AsEnumerable())
                .Map(letterScore)
                .Reduce((a, b) => a + b);

            Func<string, ISyncObservable<int>> first3 = word =>
                chars(word).Take(3);

            Func<string, ISyncObservable<int>> last3 = word =>
                chars(word).Skip(3);

            Func<string, ISyncObservable<int>> toBeMaxed = word =>
                SyncObservable.Concat(first3(word), last3(word));

            Func<string, ISyncObservable<int>> bonusForDoubleLetter = word =>
                toBeMaxed(word)
                .Map(scoreOfALetter)
                .Reduce((a, b) => Math.Max(a, b));

            Func<string, ISyncObservable<int>> score3 = word =>
                SyncObservable.Concat(
                    score2(word).Map(v => v * 2),
                    bonusForDoubleLetter(word).Map(v => v * 2),
                    SyncObservable.Just(word.Length == 7 ? 50 : 0)
                )
                .Reduce((a, b) => a + b);

            Func<Func<string, ISyncObservable<int>>, ISyncObservable<SortedDictionary<int, IList<string>>>> buildHistoOnScore = score =>
                SyncObservable.FromEnumerable(shakespeareWords.AsEnumerable())
                .Filter(word => scrabbleWords.Contains(word))
                .Filter(word => checkBlanks(word).BlockingFirst())
                .Collect(
                    () => new SortedDictionary<int, IList<string>>(IntReverse),
                    (map, word) => {
                        int key = score(word).BlockingFirst();
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
                .ConcatMap(map => map.AsEnumerable())
                .Take(3)
                .Collect(
                    () => new List<KeyValuePair<int, IList<string>>>(),
                    (list, entry) =>
                    {
                        list.Add(entry);
                    }
                )
                .BlockingFirst();

            return finalList2;
        }
    }
}
