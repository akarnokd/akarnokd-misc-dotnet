using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Reactive.Streams;
using Reactor.Core;
using System.Threading;
using Reactor.Core.flow;

namespace akarnokd_misc_dotnet.observablex
{
    public interface IObservableX<out T>
    {
        void Subscribe(IObserverX<T> observer);
    }

    public interface IObserverX<in T>
    {
        void OnSubscribe(IDisposable d);

        void OnNext(T t);

        void OnError(Exception e);

        void OnComplete();
    }

    
}
