using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace akarnokd_misc_dotnet.syncobservable
{
    internal sealed class SyncObservableConcatMapEnumerable<T, R> : ISyncObservable<R>
    {
        readonly ISyncObservable<T> source;

        readonly Func<T, IEnumerable<R>> mapper;

        public SyncObservableConcatMapEnumerable(ISyncObservable<T> source, Func<T, IEnumerable<R>> mapper)
        {
            this.source = source;
            this.mapper = mapper;
        }

        public void Subscribe(ISyncObserver<R> observer)
        {
            source.Subscribe(new ConcatMapObserver(observer, mapper));
        }

        sealed class ConcatMapObserver : ISyncObserver<T>, IDisposable
        {
            readonly ISyncObserver<R> downstream;

            readonly Func<T, IEnumerable<R>> mapper;

            IDisposable upstream;

            bool done;

            bool disposed;

            public ConcatMapObserver(ISyncObserver<R> downstream, Func<T, IEnumerable<R>> mapper)
            {
                this.downstream = downstream;
                this.mapper = mapper;
            }

            public void Dispose()
            {
                disposed = true;
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
                var enumerator = default(IEnumerator<R>);
                try
                {
                    enumerator = mapper(item).GetEnumerator() ?? throw new NullReferenceException();
                }
                catch (Exception ex)
                {
                    done = true;
                    downstream.OnError(ex);
                    return;
                }

                for (; ; )
                {
                    if (disposed)
                    {
                        enumerator.Dispose();
                        return;
                    }

                    var b = false;
                    var v = default(R);
                    try
                    {
                        b = enumerator.MoveNext();
                        if (b)
                        {
                            v = enumerator.Current;
                        }
                    }
                    catch (Exception ex)
                    {
                        enumerator.Dispose();
                        done = true;
                        downstream.OnError(ex);
                        return;
                    }

                    if (b)
                    {
                        downstream.OnNext(v);
                    }
                    else
                    {
                        enumerator.Dispose();
                        return;
                    }
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
