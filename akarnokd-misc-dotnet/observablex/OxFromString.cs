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
    sealed class OxFromString : IObservableX<int>
    {
        readonly string s;

        public OxFromString(string s)
        {
            this.s = s;
        }

        public void Subscribe(IObserverX<int> observer)
        {
            var bd = new BooleanDisposable();

            observer.OnSubscribe(bd);

            string s = this.s;
            int end = s.Length;

            for (int i = 0; i < end; i++)
            {
                if (bd.IsDisposed)
                {
                    return;
                }

                observer.OnNext(s[i]);
            }

            if (bd.IsDisposed)
            {
                return;
            }
            observer.OnComplete();
        }
    }
}
