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
    sealed class OxCollect<T, R> : IObservableX<R>
    {
        readonly IObservableX<T> source;

        readonly Func<R> initialFactory;

        readonly Action<R, T> collector;

        public OxCollect(IObservableX<T> source, Func<R> initialFactory, Action<R, T> collector)
        {
            this.source = source;
            this.initialFactory = initialFactory;
            this.collector = collector;
        }

        public void Subscribe(IObserverX<R> observer)
        {
            source.Subscribe(new CollectObserver(observer, initialFactory(), collector));
        }

        sealed class CollectObserver : BaseObserverX<T, R>
        {
            readonly Action<R, T> collector;

            R value;

            public CollectObserver(IObserverX<R> actual, R initial, Action<R, T> collector) : base(actual)
            {
                this.collector = collector;
                this.value = initial;
            }

            public override void OnNext(T t)
            {
                try
                {
                    collector(value, t);
                }
                catch (Exception ex)
                {
                    Fail(ex);
                }
            }

            public override void OnComplete()
            {
                actual.OnNext(value);
                actual.OnComplete();
            }
        }
    }
}
