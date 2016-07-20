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
    static class OxHelper
    {
        static readonly IDisposable DISPOSED = new Disposed();

        public static bool Dispose(ref IDisposable field)
        {
            var d = Volatile.Read(ref field);
            if (d != DISPOSED)
            {
                d = Interlocked.Exchange(ref field, DISPOSED);
                if (d != DISPOSED)
                {
                    d?.Dispose();
                    return true;
                }
            }
            return false;
        }

        public static bool IsDisposed(IDisposable d)
        {
            return d == DISPOSED;
        }

        public static bool SetOnce(ref IDisposable field, IDisposable d)
        {
            if (d == null)
            {
                throw new ArgumentNullException();
            }
            var curr = Interlocked.CompareExchange(ref field, d, null);
            if (curr == DISPOSED)
            {
                d.Dispose();
                return false;
            }
            return true;
        }

        public static bool Replace(ref IDisposable field, IDisposable d)
        {
            var c = Volatile.Read(ref field);
            for (;;)
            {
                if (c == DISPOSED)
                {
                    d?.Dispose();
                    return false;
                }
                var curr = Interlocked.CompareExchange(ref field, d, c);
                if (curr == c)
                {
                    return true;
                }
                c = curr;
            }
        }
    }

    sealed class Disposed : IDisposable
    {
        public void Dispose()
        {
            // Deliberately no-op.
        }
    }
}
