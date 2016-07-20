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
    sealed class OxJust<T> : IObservableX<T>
    {
        readonly T value;

        public OxJust(T value)
        {
            this.value = value;
        }

        public void Subscribe(IObserverX<T> observer)
        {
            var bd = new BooleanDisposable();
            observer.OnSubscribe(bd);

            if (bd.IsDisposed)
            {
                return;
            }
            observer.OnNext(value);
            if (bd.IsDisposed)
            {
                return;
            }
            observer.OnComplete();
        }
    }
}
