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
using Reactor.Core.scheduler;

namespace akarnokd_misc_dotnet
{
    internal static class ReactorCoreBenchmarks
    {
        internal static object Range(int c)
        {
            return Flux.Range(1, c).BlockLast();
        }

        internal static object RangeAsync(int c)
        {
            return Flux.Range(1, c).PublishOn(DefaultScheduler.Instance).BlockLast();
        }

        internal static object RangePipeline(int c)
        {
            // TODO SubscribeOn not implemented?!
            return Flux.Range(1, c)
                .PublishOn(DefaultScheduler.Instance)
                .PublishOn(DefaultScheduler.Instance).BlockLast();
        }

        internal static object FlatMapJust(int c)
        {
            return Flux.Range(1, c).FlatMap(v => Flux.Just(v)).BlockLast();
        }

        internal static object FlatMapRange(int c)
        {
            return Flux.Range(1, c).FlatMap(v => Flux.Range(v, 2)).BlockLast();
        }

        internal static object FlatMapXRange(int c)
        {
            int d = 1000000 / c;
            return Flux.Range(1, c).FlatMap(v => Flux.Range(v, d)).BlockLast();
        }

        internal static object ConcatMapJust(int c)
        {
            return Flux.Range(1, c).ConcatMap(v => Flux.Just(v)).BlockLast();
        }

        internal static object ConcatMapRange(int c)
        {
            return Flux.Range(1, c).ConcatMap(v => Flux.Range(v, 2)).BlockLast();
        }

        internal static object ConcatMapXRange(int c)
        {
            int d = 1000000 / c;
            return Flux.Range(1, c).ConcatMap(v => Flux.Range(v, d)).BlockLast();
        }
    }
}
