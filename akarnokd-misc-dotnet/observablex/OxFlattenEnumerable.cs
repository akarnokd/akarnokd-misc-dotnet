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
using System.Collections.Concurrent;

namespace akarnokd_misc_dotnet.observablex
{
    sealed class OxFlattenEnumerable<T, R> : IObservableX<R>
    {
        readonly IObservableX<T> source;

        readonly Func<T, IEnumerable<R>> mapper;

        public OxFlattenEnumerable(IObservableX<T> source, Func<T, IEnumerable<R>> mapper)
        {
            this.source = source;
            this.mapper = mapper;
        }

        public void Subscribe(IObserverX<R> observer)
        {
            source.Subscribe(new FlattenEnumerableObserver(observer, mapper));
        }

        sealed class FlattenEnumerableObserver : BaseObserverX<T, R>
        {
            readonly Func<T, IEnumerable<R>> mapper;

            public FlattenEnumerableObserver(IObserverX<R> actual, Func<T, IEnumerable<R>> mapper) : base(actual)
            {
                this.mapper = mapper;
            }

            public override void OnNext(T v)
            {
                try
                {
                    foreach (var t in mapper(v))
                    {
                        actual.OnNext(t);
                    }
                }
                catch (Exception ex)
                {
                    Fail(ex);
                    return;
                }


            }

        }
    }
}
