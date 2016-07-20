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
    sealed class OxReduce<T, R> : IObservableX<R>
    {
        readonly IObservableX<T> source;

        readonly Func<R> initialFactory;

        readonly Func<R, T, R> reducer;

        public OxReduce(IObservableX<T> source, Func<R> initialFactory, Func<R, T, R> reducer)
        {
            this.source = source;
            this.initialFactory = initialFactory;
            this.reducer = reducer;
        }

        public void Subscribe(IObserverX<R> observer)
        {
            source.Subscribe(new ReduceObserver(observer, initialFactory(), reducer));
        }

        sealed class ReduceObserver : BaseObserverX<T, R>
        {
            readonly Func<R, T, R> reducer;

            R value;

            public ReduceObserver(IObserverX<R> actual, R initial, Func<R, T, R> reducer) : base(actual)
            {
                this.reducer = reducer;
                this.value = initial;
            }

            public override void OnNext(T t)
            {
                try
                {
                    value = reducer(value, t);
                }
                catch (Exception ex)
                {
                    Fail(ex);
                }
            }

            public override void OnComplete()
            {
                actual.OnNext(value);
                actual.OnComplete();
            }
        }
    }

    sealed class OxReduce<T> : IObservableX<T>
    {
        readonly IObservableX<T> source;

        readonly Func<T, T, T> reducer;

        public OxReduce(IObservableX<T> source, Func<T, T, T> reducer)
        {
            this.source = source;
            this.reducer = reducer;
        }

        public void Subscribe(IObserverX<T> observer)
        {
            source.Subscribe(new ReduceObserver(observer, reducer));
        }

        sealed class ReduceObserver : BaseObserverX<T, T>
        {
            readonly Func<T, T, T> reducer;

            T value;
            bool hasValue;

            public ReduceObserver(IObserverX<T> actual, Func<T, T, T> reducer) : base(actual)
            {
                this.reducer = reducer;
            }

            public override void OnNext(T t)
            {
                if (!hasValue)
                {
                    hasValue = true;
                    value = t;
                }
                else
                {
                    try
                    {
                        value = reducer(value, t);
                    }
                    catch (Exception ex)
                    {
                        Fail(ex);
                    }
                }
            }

            public override void OnComplete()
            {
                if (hasValue)
                {
                    actual.OnNext(value);
                }
                actual.OnComplete();
            }
        }
    }
}
