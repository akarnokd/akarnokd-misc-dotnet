using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace akarnokd_misc_dotnet.syncobservable
{
    internal sealed class SyncObservableFromEnumerable<T> : ISyncObservable<T>
    {
        readonly IEnumerable<T> source;

        public SyncObservableFromEnumerable(IEnumerable<T> source)
        {
            this.source = source;
        }

        public void Subscribe(ISyncObserver<T> observer)
        {
            var parent = new BasicSyncDisposable();
            observer.OnSubscribe(parent);

            var enumerator = default(IEnumerator<T>);
            try
            {
                enumerator = source.GetEnumerator() ?? throw new NullReferenceException();
            }
            catch (Exception ex)
            {
                observer.OnError(ex);
                return;
            }

            for (; ; )
            {
                if (parent.IsDisposed)
                {
                    enumerator.Dispose();
                    return;
                }

                var b = false;
                var v = default(T);
                try
                {
                    b = enumerator.MoveNext();
                    if (b)
                    {
                        v = enumerator.Current;
                    }
                }
                catch (Exception ex)
                {
                    enumerator.Dispose();
                    observer.OnError(ex);
                    return;
                }

                if (b)
                {
                    observer.OnNext(v);
                }
                else
                {
                    enumerator.Dispose();
                    observer.OnCompleted();
                    return;
                }
            }
        }
    }
}
