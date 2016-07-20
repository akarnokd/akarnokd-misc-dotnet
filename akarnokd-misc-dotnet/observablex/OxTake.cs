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
    sealed class OxTake<T> : IObservableX<T>
    {
        readonly IObservableX<T> source;

        readonly long n;

        public OxTake(IObservableX<T> source, long n)
        {
            this.source = source;
            this.n = n;
        }

        public void Subscribe(IObserverX<T> observer)
        {
            source.Subscribe(new TakeObserver(observer, n));
        }

        sealed class TakeObserver : BaseObserverX<T, T>
        {
            long remaining;

            public TakeObserver(IObserverX<T> actual, long n) : base(actual)
            {
                remaining = n;
            }

            public override void OnNext(T t)
            {
                long r = remaining;
                if (r == 0L)
                {
                    return;
                }

                remaining = --r;
                actual.OnNext(t);
                if (r == 0)
                {
                    d.Dispose();
                    actual.OnComplete();
                }
            }
        }
    }
}
