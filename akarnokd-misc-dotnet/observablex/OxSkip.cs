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
    sealed class OxSkip<T> : IObservableX<T>
    {
        readonly IObservableX<T> source;

        readonly long n;

        public OxSkip(IObservableX<T> source, long n)
        {
            this.source = source;
            this.n = n;
        }

        public void Subscribe(IObserverX<T> observer)
        {
            source.Subscribe(new SkipObserver(observer, n));
        }

        sealed class SkipObserver : BaseObserverX<T, T>
        {
            long remaining;

            public SkipObserver(IObserverX<T> actual, long n) : base(actual)
            {
                remaining = n;
            }

            public override void OnNext(T t)
            {
                long r = remaining;
                if (r == 0L)
                {
                    actual.OnNext(t);
                } else
                {
                    remaining = r - 1;
                }
            }
        }
    }
}
