using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using akarnokd.reactive_extensions;

namespace akarnokd_misc_dotnet
{
    class ShakespearePlaysScrabbleAsyncEnum : ShakespearePlaysScrabble
    {
        static IAsyncEnumerable<int> chars(string s)
        {
            return AsyncEnumerable.Range(0, s.Length).Map(i => (int)s[i]);
        }

        internal static IList<KeyValuePair<int, IList<string>>> Run()
        {
            Func<int, int> scoreOfALetter = letter => letterScores[letter - 'a'];

            Func<KeyValuePair<int, MutableInt>, int> letterScore = entry =>
                letterScores[entry.Key - 'a']
                * Math.Min(entry.Value.value, scrabbleAvailableLetters[entry.Key - 'a']);

            Func<string, IAsyncEnumerable<int>> toIntegerFlux = str => chars(str);

            Func<string, IAsyncEnumerable<Dictionary<int, MutableInt>>> histoOfLetters =
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

            Func<string, IAsyncEnumerable<long>> nBlanks = word =>
                histoOfLetters(word)
                .ConcatMap(map => map.AsEnumerable())
                .Map(blank)
                .Reduce((a, b) => a + b)
                ;

            Func<string, IAsyncEnumerable<bool>> checkBlanks = word =>
                nBlanks(word).Map(v => v <= 2);

            Func<string, IAsyncEnumerable<int>> score2 = word =>
                histoOfLetters(word)
                .ConcatMap(map => map.AsEnumerable())
                .Map(letterScore)
                .Reduce((a, b) => a + b);

            Func<string, IAsyncEnumerable<int>> first3 = word =>
                chars(word).Take(3);

            Func<string, IAsyncEnumerable<int>> last3 = word =>
                chars(word).Skip(3);

            Func<string, IAsyncEnumerable<int>> toBeMaxed = word =>
                AsyncEnumerable.Concat(first3(word), last3(word));

            Func<string, IAsyncEnumerable<int>> bonusForDoubleLetter = word =>
                toBeMaxed(word)
                .Map(scoreOfALetter)
                .Reduce((a, b) => Math.Max(a, b));

            Func<string, IAsyncEnumerable<int>> score3 = word =>
                AsyncEnumerable.Concat(
                    score2(word).Map(v => v * 2),
                    bonusForDoubleLetter(word).Map(v => v * 2),
                    AsyncEnumerable.Just(word.Length == 7 ? 50 : 0)
                )
                .Reduce((a, b) => a + b);

            Func<Func<string, IAsyncEnumerable<int>>, IAsyncEnumerable<SortedDictionary<int, IList<string>>>> buildHistoOnScore = score =>
                AsyncEnumerable.FromEnumerable(shakespeareWords.AsEnumerable())
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
