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

namespace akarnokd_misc_dotnet.ix
{
    public static class Ix
    {
        public static IEnumerable<T> Just<T>(T value)
        {
            return new IxJust<T>(value);
            //yield return value;
        }

        public static IEnumerable<int> Characters(string s)
        {
            int end = s.Length;
            for (int i = 0; i < end; i++)
            {
                yield return s[i];
            }
        }

        public static IEnumerable<R> Map<T, R>(this IEnumerable<T> source, Func<T, R> mapper)
        {
            foreach (var t in source)
            {
                yield return mapper(t);
            }
        }

        public static IEnumerable<T> Filter<T>(this IEnumerable<T> source, Func<T, bool> predicate)
        {
            foreach (var t in source)
            {
                if (predicate(t))
                {
                    yield return t;
                }
            }
        }

        public static IEnumerable<T> Reduce<T>(this IEnumerable<T> source, Func<T, T, T> reducer)
        {

            bool hasValue = false;
            T value = default(T);

            foreach (var t in source)
            {
                if (!hasValue)
                {
                    hasValue = true;
                    value = t;
                }
                else
                {
                    value = reducer(value, t);
                }
            }

            if (hasValue)
            {
                yield return value;
            }
        }

        public static IEnumerable<R> Collect<T, R>(this IEnumerable<T> source, Func<R> initialFactory, Action<R, T> collector)
        {
            R value = initialFactory();

            foreach (var t in source)
            {
                collector(value, t);
            }

            yield return value;
        }

        public static T First<T>(this IEnumerable<T> source)
        {
            using (IEnumerator<T> en = source.GetEnumerator())
            {
                if (!en.MoveNext())
                {
                    throw new IndexOutOfRangeException();
                }
                return en.Current;
            }
        }

        public static IEnumerable<T> Concat<T>(params IEnumerable<T>[] sources)
        {
            foreach (var source in sources)
            {
                foreach (var t in source)
                {
                    yield return t;
                }
            }
        }

        public static IEnumerable<T> Take<T>(this IEnumerable<T> source, int n)
        {
            foreach (var t in source)
            {
                if (n == 0)
                {
                    yield break;
                }

                n--;
                yield return t;
            }
        }

        public static IEnumerable<T> Skip<T>(this IEnumerable<T> source, int n)
        {
            foreach (var t in source)
            {
                if (n == 0)
                {
                    yield return t;
                } else
                {
                    n--;
                }

            }
        }

        public static IEnumerable<R> FlatMap<T, R>(this IEnumerable<T> source, Func<T, IEnumerable<R>> mapper)
        {
            foreach (var t in source)
            {
                foreach (var u in mapper(t))
                {
                    yield return u;
                }
            }
        }

        public static IEnumerable<KeyValuePair<K, V>> AsEnumerable<K, V>(this IDictionary<K, V> source)
        {
            return source;
            
            /*    
            foreach (var k in source.Keys)
            {
                yield return new KeyValuePair<K, V>(k, source[k]);
            }
            */
        }
    }
}
