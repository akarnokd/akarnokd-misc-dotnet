using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace akarnokd_misc_dotnet.syncobservable
{
    internal sealed class SyncObservableConcat<T> : ISyncObservable<T>
    {
        readonly ISyncObservable<T>[] array;

        public SyncObservableConcat(ISyncObservable<T>[] array)
        {
            this.array = array;
        }

        public void Subscribe(ISyncObserver<T> observer)
        {
            var parent = new ConcatCoordinator(observer, array);
            observer.OnSubscribe(parent);
            parent.OnCompleted();
        }

        sealed class ConcatCoordinator : ISyncObserver<T>, IDisposable
        {
            readonly ISyncObserver<T> downstream;

            readonly ISyncObservable<T>[] array;

            int index;

            bool disposed;

            int wip;

            IDisposable upstream;

            public ConcatCoordinator(ISyncObserver<T> downstream, ISyncObservable<T>[] array)
            {
                this.downstream = downstream;
                this.array = array;
            }

            public void Dispose()
            {
                disposed = true;
                BasicSyncDisposable.Dispose(ref upstream);
            }

            public void OnCompleted()
            {
                if (wip++ == 0)
                {
                    for (; ; )
                    {
                        var idx = index;
                        var a = array;
                        var n = a.Length;

                        if (idx == n)
                        {
                            downstream.OnCompleted();
                        }
                        else
                        {
                            var src = a[idx];
                            if (src == null)
                            {
                                downstream.OnError(new NullReferenceException($"array[{idx}] is null"));
                            }
                            else
                            {
                                index = idx + 1;
                                a[idx].Subscribe(this);
                            }
                        }

                        if (--wip == 0)
                        {
                            break;
                        }
                    }
                }
            }

            public void OnError(Exception error)
            {
                downstream.OnError(error);
            }

            public void OnNext(T item)
            {
                downstream.OnNext(item);
            }

            public void OnSubscribe(IDisposable d)
            {
                BasicSyncDisposable.Replace(ref upstream, d);
            }
        }
    }
}
