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
    abstract class BaseObserverX<T, R> : IObserverX<T>, IDisposable
    {
        internal readonly IObserverX<R> actual;

        internal IDisposable d;

        public BaseObserverX(IObserverX<R> actual)
        {
            this.actual = actual;
        }

        public virtual void OnSubscribe(IDisposable d)
        {
            this.d = d;

            actual.OnSubscribe(this);
        }

        public abstract void OnNext(T v);

        public virtual void OnError(Exception ex)
        {
            actual.OnError(ex);
        }

        public void Fail(Exception ex)
        {
            d.Dispose();
            OnError(ex);
        }

        public virtual void OnComplete()
        {
            actual.OnComplete();
        }

        public virtual void Dispose()
        {
            d.Dispose();
        }
    }
}
