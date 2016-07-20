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
using System.Collections;

namespace akarnokd_misc_dotnet.ix
{
    sealed class IxJust<T> : IEnumerable<T>
    {
        readonly T value;

        public IxJust(T value)
        {
            this.value = value;
        }

        public IEnumerator<T> GetEnumerator()
        {
            return new JustEnumerator(value);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return new JustEnumerator(value);
        }

        sealed class JustEnumerator : IEnumerator<T>
        {
            readonly T value;

            bool once;

            public JustEnumerator(T value)
            {
                this.value = value;
            }

            public T Current
            {
                get
                {
                    return value;
                }
            }

            object IEnumerator.Current
            {
                get
                {
                    return value;
                }
            }

            public void Dispose()
            {
                // ignored
            }

            public bool MoveNext()
            {
                if (!once)
                {
                    once = true;
                    return true;
                }
                return false;
            }

            public void Reset()
            {
                throw new NotImplementedException();
            }
        }
    }
}
