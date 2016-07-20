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
    sealed class OxFromEnumerable<T> : IObservableX<T>
    {
        readonly IEnumerable<T> source;

        public OxFromEnumerable(IEnumerable<T> source)
        {
            this.source = source;
        }

        public void Subscribe(IObserverX<T> observer)
        {
            var bd = new BooleanDisposable();

            observer.OnSubscribe(bd);

            try
            {
                foreach (var t in source)
                {
                    if (bd.IsDisposed)
                    {
                        return;
                    }
                    observer.OnNext(t);
                }
            }
            catch (Exception ex)
            {
                if (bd.IsDisposed)
                {
                    return;
                }
                observer.OnError(ex);
                return;
            }

            if (bd.IsDisposed)
            {
                return;
            }
            observer.OnComplete();
        }
    }
}
