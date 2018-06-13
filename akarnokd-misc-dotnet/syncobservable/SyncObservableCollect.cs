using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace akarnokd_misc_dotnet.syncobservable
{
    internal sealed class SyncObservableCollect<T, C> : ISyncObservable<C>
    {
        readonly ISyncObservable<T> source;

        readonly Func<C> collectionSupplier;

        readonly Action<C, T> collector;

        public SyncObservableCollect(ISyncObservable<T> source, Func<C> collectionSupplier, Action<C, T> collector)
        {
            this.source = source;
            this.collectionSupplier = collectionSupplier;
            this.collector = collector;
        }

        public void Subscribe(ISyncObserver<C> observer)
        {
            var coll = default(C);
            try
            {
                coll = collectionSupplier();
            }
            catch (Exception ex)
            {
                observer.OnSubscribe(BasicSyncDisposable.Empty);
                observer.OnError(ex);
                return;
            }

            source.Subscribe(new ReduceObserver(observer, coll, collector));
        }

        sealed class ReduceObserver : ISyncObserver<T>, IDisposable
        {
            readonly ISyncObserver<C> downstream;

            readonly Action<C, T> collector;

            C collection;
            bool done;

            IDisposable upstream;

            public ReduceObserver(ISyncObserver<C> downstream, C collection, Action<C, T> collector)
            {
                this.downstream = downstream;
                this.collection = collection;
                this.collector = collector;
            }

            public void Dispose()
            {
                upstream.Dispose();
            }

            public void OnCompleted()
            {
                var v = collection;
                collection = default;
                downstream.OnNext(v);
                downstream.OnCompleted();
            }

            public void OnError(Exception error)
            {
                if (done)
                {
                    return;
                }
                collection = default;
                downstream.OnError(error);
            }

            public void OnNext(T item)
            {
                if (done)
                {
                    return;
                }
                try
                {
                    collector(collection, item);
                }
                catch (Exception ex)
                {
                    upstream.Dispose();
                    done = true;
                    downstream.OnError(ex);
                }
            }

            public void OnSubscribe(IDisposable d)
            {
                upstream = d;
                downstream.OnSubscribe(this);
            }
        }
    }
}
