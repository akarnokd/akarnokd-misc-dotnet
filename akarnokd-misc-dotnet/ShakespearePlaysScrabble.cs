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
using System.IO;
using BenchmarkDotNet.Attributes;

namespace akarnokd_misc_dotnet
{
    public class ShakespearePlaysScrabble
    {
        static internal readonly int[] letterScores = {
    // a, b, c, d, e, f, g, h, i, j, k, l, m, n, o, p,  q, r, s, t, u, v, w, x, y,  z
       1, 3, 3, 2, 1, 4, 2, 4, 1, 8, 5, 1, 3, 1, 1, 3, 10, 1, 1, 1, 1, 4, 4, 8, 4, 10};

        static internal readonly int[] scrabbleAvailableLetters = {
     // a, b, c, d,  e, f, g, h, i, j, k, l, m, n, o, p, q, r, s, t, u, v, w, x, y, z
        9, 2, 2, 1, 12, 2, 3, 2, 9, 1, 1, 4, 2, 6, 8, 2, 1, 6, 4, 6, 4, 2, 2, 1, 2, 1};

        static internal readonly HashSet<string> scrabbleWords = new HashSet<string>();
        static internal readonly HashSet<string> shakespeareWords = new HashSet<string>();

        [GlobalSetup]
        public void Setup()
        {
            Init();
        }

        static internal void Init()
        {
            Console.Write("// Reading text files...");
            var dir = Directory.GetCurrentDirectory();
            var idx = dir.IndexOf("akarnokd-misc-dotnet");
            var path = dir.Substring(0, idx) + "akarnokd-misc-dotnet\\akarnokd-misc-dotnet\\";
            using (StreamReader stream = new StreamReader(path + "files\\ospd.txt"))
            {
                string line;

                while ((line = stream.ReadLine()) != null)
                {
                    scrabbleWords.Add(line.ToLowerInvariant());
                }
            }

            using (StreamReader stream = new StreamReader(path + "files\\words.shakespeare.txt"))
            {
                string line;

                while ((line = stream.ReadLine()) != null)
                {
                    shakespeareWords.Add(line.ToLowerInvariant());
                }
            }
            Console.WriteLine("// Done!");
        }

        internal sealed class MutableInt
        {
            internal int value;
        }

        internal static readonly IntReverseComparer IntReverse = new IntReverseComparer();

        internal sealed class IntReverseComparer : IComparer<int>
        {

            public int Compare(int x, int y)
            {
                return x < y ? 1 : (x > y ? -1 : 0); 
            }
        }

    }
}
