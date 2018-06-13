using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace akarnokd_misc_dotnet.syncobservable
{
    public interface ISyncObservable<out T>
    {
        void Subscribe(ISyncObserver<T> observer);
    }

    public interface ISyncObserver<in T>
    {
        void OnSubscribe(IDisposable d);

        void OnNext(T item);

        void OnError(Exception error);

        void OnCompleted();
    }

    public static class SyncObservable
    {

        public static ISyncObservable<int> Characters(string str)
        {
            return new SyncObservableCharacters(str);
        }

        public static ISyncObservable<R> Map<T, R>(this ISyncObservable<T> source, Func<T, R> mapper)
        {
            return new SyncObservableMap<T, R>(source, mapper);
        }

        public static ISyncObservable<T> Filter<T>(this ISyncObservable<T> source, Func<T, bool> predicate)
        {
            return new SyncObservableFilter<T>(source, predicate);
        }

        public static ISyncObservable<T> Take<T>(this ISyncObservable<T> source, long n)
        {
            return new SyncObservableTake<T>(source, n);
        }

        public static ISyncObservable<T> Skip<T>(this ISyncObservable<T> source, long n)
        {
            return new SyncObservableSkip<T>(source, n);
        }

        public static ISyncObservable<T> Reduce<T>(this ISyncObservable<T> source, Func<T, T, T> reducer)
        {
            return new SyncObservableReduce<T>(source, reducer);
        }

        public static ISyncObservable<C> Collect<T, C>(this ISyncObservable<T> source, Func<C> collectionSupplier, Action<C, T> collector)
        {
            return new SyncObservableCollect<T, C>(source, collectionSupplier, collector);
        }

        public static ISyncObservable<T> Just<T>(T value)
        {
            return new SyncObservableJust<T>(value);
        }

        public static ISyncObservable<T> Concat<T>(params ISyncObservable<T>[] sources)
        {
            return new SyncObservableConcat<T>(sources);
        }

        public static ISyncObservable<R> ConcatMap<T, R>(this ISyncObservable<T> source, Func<T, IEnumerable<R>> mapper)
        {
            return new SyncObservableConcatMapEnumerable<T, R>(source, mapper);
        }

        public static ISyncObservable<T> FromEnumerable<T>(this IEnumerable<T> source)
        {
            return new SyncObservableFromEnumerable<T>(source);
        }

        public static T BlockingFirst<T>(this ISyncObservable<T> source)
        {
            var consumer = new SyncObservableBlockingFirst<T>();
            source.Subscribe(consumer);
            var v = consumer.GetValue(out var success);
            if (!success)
            {
                throw new IndexOutOfRangeException();
            }
            return v;
        }
    }
}
