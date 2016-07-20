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
    sealed class BooleanDisposable : IDisposable
    {
        bool disposed;

        public bool IsDisposed { get { return Volatile.Read(ref disposed); } }

        public void Dispose()
        {
            Volatile.Write(ref disposed, true);
        }
    }
}
