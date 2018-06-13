using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace akarnokd_misc_dotnet.syncobservable
{
    internal sealed class SyncObservableJust<T> : ISyncObservable<T>
    {
        readonly T value;

        public SyncObservableJust(T value)
        {
            this.value = value;
        }

        public void Subscribe(ISyncObserver<T> observer)
        {
            var d = new BasicSyncDisposable();
            observer.OnSubscribe(d);
            if (d.IsDisposed)
            {
                return;
            }
            observer.OnNext(value);
            if (d.IsDisposed)
            {
                return;
            }
            observer.OnCompleted();
        }
    }
}
