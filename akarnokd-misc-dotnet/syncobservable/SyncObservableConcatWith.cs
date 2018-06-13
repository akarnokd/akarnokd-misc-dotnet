using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace akarnokd_misc_dotnet.syncobservable
{
    internal sealed class SyncObservableConcatWith<T> : ISyncObservable<T>
    {
        readonly ISyncObservable<T> first;

        readonly ISyncObservable<T> second;

        public SyncObservableConcatWith(ISyncObservable<T> first, ISyncObservable<T> second)
        {
            this.first = first;
            this.second = second;
        }

        public void Subscribe(ISyncObserver<T> observer)
        {
            var parent = new ConcatCoordinator(observer, second);
            observer.OnSubscribe(parent);
            parent.Next(first);
        }

        sealed class ConcatCoordinator : ISyncObserver<T>, IDisposable
        {
            readonly ISyncObserver<T> downstream;

            ISyncObservable<T> next;

            IDisposable upstream;

            int wip;

            public ConcatCoordinator(ISyncObserver<T> downstream, ISyncObservable<T> next)
            {
                this.downstream = downstream;
                this.next = next;
            }

            public void Dispose()
            {
                BasicSyncDisposable.Dispose(ref upstream);
            }

            public void OnCompleted()
            {
                if (next != null)
                {
                    Next(null);
                }
                else
                {
                    downstream.OnCompleted();
                }
            }

            internal void Next(ISyncObservable<T> source)
            {
                if (wip++ == 0)
                {
                    for (; ; )
                    {
                        if (source == null)
                        {
                            var src = next;
                            next = null;
                            source.Subscribe(this);
                        }
                        else
                        {
                            var src = source;
                            source = null;
                            source.Subscribe(this);
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
