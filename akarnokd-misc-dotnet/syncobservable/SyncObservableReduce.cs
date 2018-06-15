using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace akarnokd_misc_dotnet.syncobservable
{
    internal sealed class SyncObservableReduce<T> : ISyncObservable<T>
    {
        readonly ISyncObservable<T> source;

        readonly Func<T, T, T> reducer;

        public SyncObservableReduce(ISyncObservable<T> source, Func<T, T, T> reducer)
        {
            this.source = source;
            this.reducer = reducer;
        }

        public void Subscribe(ISyncObserver<T> observer)
        {
            source.Subscribe(new ReduceObserver(observer, reducer));
        }

        sealed class ReduceObserver : ISyncObserver<T>, IDisposable
        {
            readonly ISyncObserver<T> downstream;

            readonly Func<T, T, T> reducer;

            T value;
            bool hasValue;
            bool done;

            IDisposable upstream;

            public ReduceObserver(ISyncObserver<T> downstream, Func<T, T, T> reducer)
            {
                this.downstream = downstream;
                this.reducer = reducer;
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
                if (hasValue)
                {
                    var v = value;
                    value = default;
                    downstream.OnNext(v);
                }
                downstream.OnCompleted();
            }

            public void OnError(Exception error)
            {
                if (done)
                {
                    return;
                }
                value = default;
                downstream.OnError(error);
            }

            public void OnNext(T item)
            {
                if (done)
                {
                    return;
                }

                if (hasValue) {
                    try
                    {
                        value = reducer(value, item);
                    }
                    catch (Exception ex)
                    {
                        upstream.Dispose();
                        done = true;
                        downstream.OnError(ex);
                    }
                }
                else
                {
                    value = item;
                    hasValue = true;
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
