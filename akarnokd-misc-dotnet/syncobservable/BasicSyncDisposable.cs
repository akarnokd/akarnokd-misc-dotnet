using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace akarnokd_misc_dotnet.syncobservable
{
    internal sealed class BasicSyncDisposable : IDisposable
    {
        bool disposed;

        public BasicSyncDisposable() { }

        public BasicSyncDisposable(bool dispose)
        {
            this.disposed = dispose;
        }

        public void Dispose()
        {
            disposed = true;
        }

        public bool IsDisposed => disposed;

        internal static readonly BasicSyncDisposable Disposed = new BasicSyncDisposable(true);

        internal static readonly IDisposable Empty = new EmptyDisposable();

        sealed class EmptyDisposable : IDisposable
        {
            public void Dispose()
            {
                // no-op
            }
        }

        public static bool SetOnce(ref IDisposable field, IDisposable d)
        {
            if (d == null)
            {
                throw new ArgumentNullException(nameof(d));
            }
            var current = field;
            if (current == Disposed)
            {
                d.Dispose();
                return false;
            }
            if (current != null)
            {
                throw new InvalidOperationException("IDisposable already set");
            }
            field = d;
            return true;
        }

        public static bool Replace(ref IDisposable field, IDisposable d)
        {
            var c = field;
            if (c == Disposed)
            {
                d?.Dispose();
                return false;
            }
            field = d;
            return true;
        }

        public static bool Dispose(ref IDisposable field)
        {
            var c = field;
            if (c != Disposed)
            {
                field = Disposed;
                c?.Dispose();
                return true;
            }
            return false;
        }
    }
}
