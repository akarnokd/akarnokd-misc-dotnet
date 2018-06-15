using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace akarnokd_misc_dotnet.syncobservable
{
    internal sealed class SyncObservableFilter<T> : ISyncObservable<T>
    {
        readonly ISyncObservable<T> source;

        readonly Func<T, bool> predicate;

        public SyncObservableFilter(ISyncObservable<T> source, Func<T, bool> predicate)
        {
            this.source = source;
            this.predicate = predicate;
        }

        public void Subscribe(ISyncObserver<T> observer)
        {
            source.Subscribe(new FilterObserver(observer, predicate));
        }

        sealed class FilterObserver : ISyncObserver<T>, IDisposable
        {
            readonly ISyncObserver<T> downstream;

            readonly Func<T, bool> predicate;

            IDisposable upstream;

            bool done;

            public FilterObserver(ISyncObserver<T> downstream, Func<T, bool> predicate)
            {
                this.downstream = downstream;
                this.predicate = predicate;
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

                var r = false;
                try
                {
                    r = predicate(item);
                }
                catch (Exception ex)
                {
                    upstream.Dispose();
                    done = true;
                    downstream.OnError(ex);
                    return;
                }

                if (r)
                {
                    downstream.OnNext(item);
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
