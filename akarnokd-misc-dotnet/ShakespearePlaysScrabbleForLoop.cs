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
    class ShakespearePlaysScrabbleForLoop : ShakespearePlaysScrabble
    {
        [Benchmark]
        public object ForLoops()
        {
            return Run();
        }

        internal static IList<KeyValuePair<int, IList<string>>> Run()
        {
            SortedDictionary<int, IList<String>> treemap = new SortedDictionary<int, IList<String>>(IntReverse);

            foreach (var word in shakespeareWords)
            {
                if (scrabbleWords.Contains(word))
                {
                    Dictionary<int, MutableInt> wordHistogram = new Dictionary<int, MutableInt>();
                    for (int k = 0; k < word.Length; k++)
                    {
                        if (!wordHistogram.TryGetValue((int)word[k], out var newValue))
                        {
                            newValue = new MutableInt();
                            wordHistogram.Add((int)word[k], newValue);
                        }
                        newValue.value++;
                    }

                    long sum = 0L;
                    foreach (var entry in wordHistogram)
                    {
                        sum += Math.Max(0L, entry.Value.value -
                                    scrabbleAvailableLetters[entry.Key - 'a']);
                    }
                    bool b = sum <= 2L;

                    if (b)
                    {
                        // redo the histogram?!
                        //                    wordHistogram = new HashMap<>();
                        //                    for (int i = 0; i < word.length(); i++) {
                        //                        MutableLong newValue = wordHistogram.get((int)word.charAt(i)) ;
                        //                        if (newValue == null) {
                        //                            newValue = new MutableLong();
                        //                            wordHistogram.put((int)word.charAt(i), newValue);
                        //                        }
                        //                        newValue.incAndSet();
                        //                    }

                        int sum2 = 0;
                        foreach (var entry in wordHistogram)
                        {
                            sum2 += letterScores[entry.Key - 'a'] *
                                    Math.Min(
                                    (int)entry.Value.value,
                                    scrabbleAvailableLetters[entry.Key - 'a']
                                );
                        }
                        int max2 = 0;
                        for (int j = 0; j < 3 && j < word.Length; j++)
                        {
                            max2 = Math.Max(max2, letterScores[word[j] - 'a']);
                        }

                        for (int j = 3; j < word.Length; j++)
                        {
                            max2 = Math.Max(max2, letterScores[word[j] - 'a']);
                        }

                        sum2 += max2;
                        sum2 = 2 * sum2 + (word.Length == 7 ? 50 : 0);

                        var key = sum2;

                        if (!treemap.TryGetValue(key, out var listInner))
                        {
                            listInner = new List<string>();
                            treemap.Add(key, listInner);
                        }
                        listInner.Add(word);
                    }
                }
            }

            var list = new List<KeyValuePair<int, IList<string>>>();

            int i = 4;
            foreach (var e in treemap)
            {
                if (--i == 0)
                {
                    break;
                }
                list.Add(e);
            }

            return list;
        }
    }
}
