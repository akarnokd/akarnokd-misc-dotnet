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
    sealed class OxMap<T, R> : IObservableX<R>
    {
        readonly IObservableX<T> source;

        readonly Func<T, R> mapper;

        public OxMap(IObservableX<T> source, Func<T, R> mapper)
        {
            this.source = source;
            this.mapper = mapper;
        }

        public void Subscribe(IObserverX<R> observer)
        {
            source.Subscribe(new MapObserver(observer, mapper));
        }

        sealed class MapObserver : BaseObserverX<T, R>
        {
            readonly Func<T, R> mapper;

            public MapObserver(IObserverX<R> actual, Func<T, R> mapper) : base(actual)
            {
                this.mapper = mapper;
            }

            public override void OnNext(T t)
            {
                R v;

                try
                {
                    v = mapper(t);
                }
                catch (Exception ex)
                {
                    Fail(ex);
                    return;
                }

                actual.OnNext(v);
            }
        }
    }
}
