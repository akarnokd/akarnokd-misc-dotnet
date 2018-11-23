using BenchmarkDotNet.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Quinmars;
using Quinmars.AsyncObservable;

namespace akarnokd_misc_dotnet
{
    [MemoryDiagnoser]
    public class ShakespearePlaysScrabbleAsyncObservable : ShakespearePlaysScrabble
    {
        [Benchmark]
        public object AsyncObs()
        {
            return Run();
        }

        static IAsyncObservable<int> chars(string word)
        {
            return AsyncObservable.Range(0, word.Length).Select(i => (int)word[i]);
        }

        internal static IList<KeyValuePair<int, IList<string>>> Run()
        {
            Func<int, int> scoreOfALetter = letter => letterScores[letter - 'a'];

            Func<KeyValuePair<int, MutableInt>, int> letterScore = entry =>
                letterScores[entry.Key - 'a']
                * Math.Min(entry.Value.value, scrabbleAvailableLetters[entry.Key - 'a']);

            Func<string, IAsyncObservable<int>> toIntegerFlux = str => chars(str);

            Func<string, IAsyncObservable<Dictionary<int, MutableInt>>> histoOfLetters =
                word => toIntegerFlux(word)
                        .Aggregate(
                            () => new Dictionary<int, MutableInt>(),
                            (m, value) =>
                            {
                                if (!m.TryGetValue(value, out var mi))
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

            Func<string, IAsyncObservable<long>> nBlanks = word =>
                histoOfLetters(word)
                .SelectMany(map => map.AsEnumerable())
                .Select(blank)
                .Aggregate(() => 0L, (x, y) => x + y);

            Func<string, IAsyncObservable<bool>> checkBlanks = word =>
                nBlanks(word).Select(v => v <= 2);

            Func<string, IAsyncObservable<int>> score2 = word =>
                histoOfLetters(word)
                .SelectMany(map => map.AsEnumerable())
                .Select(letterScore)
                .Sum();

            Func<string, IAsyncObservable<int>> first3 = word =>
                chars(word).Take(3);

            Func<string, IAsyncObservable<int>> last3 = word =>
                chars(word).Skip(3);

            Func<string, IAsyncObservable<int>> toBeMaxed = word =>
                AsyncObservable.Concat(first3(word), last3(word));

            Func<string, IAsyncObservable<int>> bonusForDoubleLetter = word =>
                toBeMaxed(word)
                .Select(scoreOfALetter)
                .Max();

            Func<string, IAsyncObservable<int>> score3 = word =>
                AsyncObservable.Concat(
                    score2(word).Select(v => v * 2),
                    bonusForDoubleLetter(word).Select(v => v * 2),
                    AsyncObservable.Return(word.Length == 7 ? 50 : 0)
                )
                .Sum();

            Func<Func<string, IAsyncObservable<int>>, IAsyncObservable<SortedDictionary<int, IList<string>>>> buildHistoOnScore = score =>
                shakespeareWords.ToAsyncObservable()
                .Where(word => scrabbleWords.Contains(word))
                .Where(word => checkBlanks(word).FirstAsync())
                .Aggregate(
                    () => new SortedDictionary<int, IList<string>>(IntReverse),
                    (map, word) =>
                    {
                        int key = score(word).FirstAsync().Result;
                        if (!map.TryGetValue(key, out var list))
                        {
                            list = new List<string>();
                            map.Add(key, list);
                        }
                        list.Add(word);
                        return map;
                    }
                );

            IList<KeyValuePair<int, IList<string>>> finalList2 =
                buildHistoOnScore(score3)
                .SelectMany(map => map.AsEnumerable())
                .Take(3)
                .Aggregate(
                    () => new List<KeyValuePair<int, IList<string>>>(),
                    (list, entry) =>
                    {
                        list.Add(entry);
                        return list;
                    }
                )
                .FirstAsync().Result;

            return finalList2;
        }
    }
}
