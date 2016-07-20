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
    sealed class BlockingObserverX<T> : IObserverX<T>, IDisposable
    {
        readonly CountdownEvent latch;

        IDisposable d;

        T value;
        Exception error;

        public BlockingObserverX()
        {
            this.latch = new CountdownEvent(1);
        }

        public void OnComplete()
        {
            if (latch.CurrentCount != 0)
            {
                latch.Signal();
            }
        }

        public void OnError(Exception e)
        {
            error = e;
            latch.Signal();
        }

        public void OnNext(T t)
        {
            value = t;
        }

        public void OnSubscribe(IDisposable d)
        {
            OxHelper.SetOnce(ref this.d, d);
        }

        public void Dispose()
        {
            OxHelper.Dispose(ref d);
        }

        public T Get()
        {
            if (latch.CurrentCount != 0)
            {
                latch.Wait();
            }
            var ex = error;
            if (ex != null)
            {
                throw ex;
            }
            return value;
        }
    }
}
