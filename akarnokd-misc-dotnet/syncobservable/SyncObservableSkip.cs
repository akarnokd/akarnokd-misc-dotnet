using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace akarnokd_misc_dotnet.syncobservable
{
    internal sealed class SyncObservableSkip<T> : ISyncObservable<T>
    {
        readonly ISyncObservable<T> source;

        readonly long n;

        public SyncObservableSkip(ISyncObservable<T> source, long n)
        {
            this.source = source;
            this.n = n;
        }

        public void Subscribe(ISyncObserver<T> observer)
        {
            source.Subscribe(new SkipObserver(observer, n));
        }

        sealed class SkipObserver : ISyncObserver<T>, IDisposable
        {
            readonly ISyncObserver<T> downstream;

            long remaining;

            IDisposable upstream;

            public SkipObserver(ISyncObserver<T> downstream, long remaining)
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
                downstream.OnCompleted();
            }

            public void OnError(Exception error)
            {
                downstream.OnError(error);
            }

            public void OnNext(T item)
            {
                var r = remaining;
                if (r == 0)
                {
                    downstream.OnNext(item);
                }
                else
                {
                    remaining = r - 1;
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
