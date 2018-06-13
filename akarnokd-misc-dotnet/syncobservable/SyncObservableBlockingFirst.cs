using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace akarnokd_misc_dotnet.syncobservable
{
    internal sealed class SyncObservableBlockingFirst<T> : ISyncObserver<T>
    {

        IDisposable upstream;

        T value;
        bool hasValue;
        Exception error;

        public void OnCompleted()
        {
            // - no op
        }

        public void OnError(Exception error)
        {
            if (!hasValue)
            {
                this.error = error;
            }
        }

        public void OnNext(T item)
        {
            if (!hasValue)
            {
                hasValue = true;
                value = item;
                upstream.Dispose();
            }
        }

        public void OnSubscribe(IDisposable d)
        {
            upstream = d;
        }

        public T GetValue(out bool success)
        {
            var ex = error;
            if (ex != null)
            {
                throw ex;
            }
            if (hasValue)
            {
                success = true;
                return value;
            }
            success = false;
            return default;
        }
    }
}
