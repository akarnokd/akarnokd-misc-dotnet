using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace akarnokd_misc_dotnet.syncobservable
{
    internal sealed class SyncObservableCharacters : ISyncObservable<int>
    {
        readonly string str;

        public SyncObservableCharacters(string str)
        {
            this.str = str;
        }

        public void Subscribe(ISyncObserver<int> observer)
        {
            var d = new BasicSyncDisposable();

            observer.OnSubscribe(d);

            var str = this.str;
            var n = str.Length;

            for (int i = 0; i < n; i++)
            {
                if (d.IsDisposed)
                {
                    return;
                }
                observer.OnNext(str[i]);
            }
            if (d.IsDisposed)
            {
                return;
            }
            observer.OnCompleted();
        }
    }
}
