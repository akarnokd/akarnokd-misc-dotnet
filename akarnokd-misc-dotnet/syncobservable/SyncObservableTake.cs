using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace akarnokd_misc_dotnet.syncobservable
{
    internal sealed class SyncObservableTake<T> : ISyncObservable<T>
    {
        readonly ISyncObservable<T> source;

        readonly long n;

        public SyncObservableTake(ISyncObservable<T> source, long n)
        {
            this.source = source;
            this.n = n;
        }

        public void Subscribe(ISyncObserver<T> observer)
        {
            source.Subscribe(new TakeObserver(observer, n));
        }

        sealed class TakeObserver : ISyncObserver<T>, IDisposable
        {
            readonly ISyncObserver<T> downstream;

            long remaining;

            IDisposable upstream;

            public TakeObserver(ISyncObserver<T> downstream, long remaining)
            {
                this.downstream = downstream;
                this.remaining = remaining;
            }

            public void Dispose()
            {
                upstream.Dispose();
            }

            public void OnCompleted()
            {
                if (remaining > 0)
                {
                    downstream.OnCompleted();
                }
            }

            public void OnError(Exception error)
            {
                if (remaining > 0)
                {
                    downstream.OnError(error);
                }
            }

            public void OnNext(T item)
            {
                var r = remaining;
                if (r > 0)
                {
                    remaining = --r;
                    downstream.OnNext(item);
                    if (r == 0)
                    {
                        remaining = 0;
                        upstream.Dispose();
                        downstream.OnCompleted();
                    }
                }
            }

            public void OnSubscribe(IDisposable d)
            {
                upstream = d;
                downstream.OnSubscribe(this);
                if (remaining == 0)
                {
                    d.Dispose();
                    downstream.OnCompleted();
                }
            }
        }
    }
}
