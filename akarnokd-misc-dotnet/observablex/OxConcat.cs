using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Reactive.Streams;
using Reactor.Core;
using System.Threading;
using Reactor.Core.flow;
using Reactor.Core.subscriber;
using Reactor.Core.subscription;
using Reactor.Core.util;

namespace akarnokd_misc_dotnet.observablex
{
    sealed class OxConcat<T> : IObservableX<T>
    {
        readonly IObservableX<T>[] sources;

        public OxConcat(IObservableX<T>[] sources)
        {
            this.sources = sources;
        }

        public void Subscribe(IObserverX<T> observer)
        {
            var parent = new ConcatObserver(observer, sources);
            observer.OnSubscribe(parent);
            parent.OnComplete();
        }

        sealed class ConcatObserver : IObserverX<T>, IDisposable
        {
            readonly IObserverX<T> actual;

            readonly IObservableX<T>[] sources;

            IDisposable d;

            int wip;

            int index;

            bool disposed;

            bool active;

            public ConcatObserver(IObserverX<T> actual, IObservableX<T>[] sources)
            {
                this.actual = actual;
                this.sources = sources;
            }

            public void Dispose()
            {
                Volatile.Write(ref disposed, true);
                OxHelper.Dispose(ref d);
            }

            public void OnSubscribe(IDisposable d)
            {
                OxHelper.Replace(ref this.d, d);
            }

            public void OnNext(T t)
            {
                actual.OnNext(t);
            }

            public void OnError(Exception e)
            {
                actual.OnError(e);
            }

            public void OnComplete()
            {
                Volatile.Write(ref active, false);
                if (Interlocked.Increment(ref wip) != 1)
                {
                    return;
                }

                for (;;)
                {
                    if (Volatile.Read(ref disposed))
                    {
                        return;
                    }

                    if (!Volatile.Read(ref active))
                    {
                        int idx = index;
                        if (idx == sources.Length)
                        {
                            actual.OnComplete();
                            return;
                        }
                        index = idx + 1;

                        Volatile.Write(ref active, true);
                        sources[idx].Subscribe(this);
                    }

                    if (Interlocked.Decrement(ref wip) == 0)
                    {
                        break;
                    }
                }
            }
        }
    }
}
