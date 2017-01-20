using Reactor.Core.util;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace akarnokd_misc_dotnet.schedulers
{
    public sealed class ThreadWorker : IDisposable
    {
        int once;

        readonly ConcurrentQueue<WorkItem> queue = new ConcurrentQueue<WorkItem>();

        int wip;

        static readonly IDisposable DISPOSED = new Disposed();

        public void Dispose()
        {
            int m = Interlocked.Exchange(ref once, -1);
            if (m != -1)
            {
                if (Volatile.Read(ref wip) == 0)
                {
                    lock (this)
                    {
                        Monitor.Pulse(this);
                    }
                }
                Clear();
            }
        }

        void Clear()
        {
            WorkItem o = default(WorkItem);
            while (queue.TryDequeue(out o)) ;

        }

        void Run()
        {
            int m = 0;
            for (;;)
            {
                WorkItem wi = default(WorkItem);

                if (Volatile.Read(ref once) == -1)
                {
                    break;
                }

                for (int i = 0; i < 64; i++)
                {
                    if (queue.TryDequeue(out wi))
                    {
                        break;
                    }
                }

                if (wi == null)
                {
                    if (m != 0)
                    {
                        m = Interlocked.Add(ref wip, -m);
                    }

                    if (m == 0)
                    {
                        lock (this)
                        {
                            if (Volatile.Read(ref once) == -1)
                            {
                                break;
                            }
                            Monitor.Wait(this, 1);
                        }
                    }
                }
                else
                {
                    m++;
                    try
                    {
                        wi.Run();
                    } catch
                    {
                        // ignored
                    }
                }
            }
        }
        
        public IDisposable Schedule(Action action)
        {
            int w = Volatile.Read(ref once);
            if (w == -1)
            {
                return DISPOSED;
            }
            if (w == 0 && Interlocked.CompareExchange(ref once, 1, 0) == 0)
            {
                Task.Factory.StartNew(Run, TaskCreationOptions.LongRunning);
            }
            var wi = new WorkItem(action);
            queue.Enqueue(wi);
            if (Interlocked.Add(ref wip, 1) == 1)
            {
                lock (this)
                {
                    Monitor.Pulse(this);
                }
            }
            if (Volatile.Read(ref once) == -1)
            {
                Clear();
                wi.Dispose();
            }
            return wi;
        }

        class WorkItem : IDisposable
        {
            readonly Action action;

            Thread thread;

            int state;
            static readonly int READY = 0;
            static readonly int RUNNING = 1;
            static readonly int FINISHED = 2;
            static readonly int CANCELLED = 4;
            static readonly int INTERRUPTING = 32;
            static readonly int INTERRUPTED = 64;


            internal WorkItem(Action action)
            {
                this.action = action;
            }

            public void Dispose()
            {
                for (;;)
                {
                    int s = lvState();
                    if ((s & (INTERRUPTING | INTERRUPTED | FINISHED | CANCELLED)) != 0)
                    {
                        break;
                    }
                    if (s == READY && casState(READY, CANCELLED))
                    {
                        break;
                    }
                    if (casState(RUNNING, INTERRUPTING))
                    {
                        Thread t = Volatile.Read(ref thread);
                        if (t != null)
                        {
                            t.Interrupt();
                        }
                        Volatile.Write(ref state, INTERRUPTED);
                    }
                }
            }

            int lvState()
            {
                return Volatile.Read(ref state);
            }

            bool casState(int expected, int next)
            {
                return Interlocked.CompareExchange(ref state, next, expected) == expected;
            }

            internal void Run()
            {
                if (lvState() == READY)
                {
                    Volatile.Write(ref thread, Thread.CurrentThread);
                    if (casState(READY, RUNNING))
                    {
                        try
                        {
                            action();
                        }
                        finally
                        {
                            if (!casState(RUNNING, FINISHED))
                            {
                                try
                                {
                                    // this should consume the interrupt indicator
                                    Thread.Sleep(Timeout.Infinite);
                                }
                                catch (ThreadInterruptedException)
                                {
                                    // ignored
                                }
                            }
                            Volatile.Write(ref thread, null);
                        }
                    }
                }
            }
        }

        internal sealed class Disposed : IDisposable
        {
            public void Dispose()
            {
                // ignored
            }
        }
    }
}
