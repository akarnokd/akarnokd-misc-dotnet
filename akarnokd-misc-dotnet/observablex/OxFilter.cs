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
    sealed class OxFilter<T> : IObservableX<T>
    {
        readonly IObservableX<T> source;

        readonly Func<T, bool> predicate;

        public OxFilter(IObservableX<T> source, Func<T, bool> predicate)
        {
            this.source = source;
            this.predicate = predicate;
        }

        public void Subscribe(IObserverX<T> observer)
        {
            source.Subscribe(new FilterObserver(observer, predicate));
        }

        sealed class FilterObserver : BaseObserverX<T, T>
        {
            readonly Func<T, bool> predicate;

            public FilterObserver(IObserverX<T> actual, Func<T, bool> predicate) : base(actual)
            {
                this.predicate = predicate;
            }

            public override void OnNext(T t)
            {
                bool v;

                try
                {
                    v = predicate(t);
                }
                catch (Exception ex)
                {
                    d.Dispose();
                    actual.OnError(ex);
                    return;
                }

                if (v)
                {
                    actual.OnNext(t);
                }
            }
        }
    }
}
