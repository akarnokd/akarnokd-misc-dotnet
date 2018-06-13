using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace akarnokd_misc_dotnet.syncobservable
{
    internal sealed class SyncObservableMap<T, R> : ISyncObservable<R>
    {
        readonly ISyncObservable<T> source;

        readonly Func<T, R> mapper;

        public SyncObservableMap(ISyncObservable<T> source, Func<T, R> mapper)
        {
            this.source = source;
            this.mapper = mapper;
        }

        public void Subscribe(ISyncObserver<R> observer)
        {
            source.Subscribe(new MapObserver(observer, mapper));
        }

        sealed class MapObserver : ISyncObserver<T>, IDisposable
        {
            readonly ISyncObserver<R> downstream;

            readonly Func<T, R> mapper;

            IDisposable upstream;

            bool done;

            public MapObserver(ISyncObserver<R> downstream, Func<T, R> mapper)
            {
                this.downstream = downstream;
                this.mapper = mapper;
            }

            public void Dispose()
            {
                upstream.Dispose();
            }

            public void OnCompleted()
            {
                if (done)
                {
                    return;
                }
                downstream.OnCompleted();
            }

            public void OnError(Exception error)
            {
                if (done)
                {
                    return;
                }
                downstream.OnError(error);
            }

            public void OnNext(T item)
            {
                if (done)
                {
                    return;
                }

                var r = default(R);
                try
                {
                    r = mapper(item);
                }
                catch (Exception ex)
                {
                    upstream.Dispose();
                    done = true;
                    downstream.OnError(ex);
                    return;
                }

                downstream.OnNext(r);
            }

            public void OnSubscribe(IDisposable d)
            {
                upstream = d;
                downstream.OnSubscribe(this);
            }
        }
    }
}
