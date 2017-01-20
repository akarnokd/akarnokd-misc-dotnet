using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace akarnokd_misc_dotnet.schedulers
{
    //[StructLayout(layoutKind: LayoutKind.Sequential, Pack = 8)]
    public sealed class MpscLinkedArrayQueue<T> where T : class
    {
        readonly int size;
        
        Node head;

        Node tail;

        public MpscLinkedArrayQueue(int size)
        {
            this.size = size;
            var n = new Node(size);
            head = n;
            Interlocked.Exchange(ref tail, n);
        }

        public void Enqueue(T item)
        {
            if (item == null)
            {
                throw new ArgumentOutOfRangeException();
            }

            var t = Volatile.Read(ref tail);

            for (;;)
            {
                int offer = Interlocked.Increment(ref t.offerIndex) - 1;

                if (offer >= size)
                {
                    var n = Volatile.Read(ref t.next);
                    if (n == null)
                    {
                        n = new Node(size, item);
                        if (Interlocked.CompareExchange(ref t.next, n, null) == null)
                        {
                            Interlocked.CompareExchange(ref tail, n, t);
                            return;
                        }
                        n = Volatile.Read(ref t.next);
                    }
                    Interlocked.CompareExchange(ref tail, n, t);
                    t = n;
                    continue;
                }

                Volatile.Write(ref t.array[offer], item);
                return;
            }

        }

        public bool TryDequeueWeak(out T item)
        {
            var h = head;
            var a = h.array;
            var offset = h.pollIndex;
            if (offset == a.Length)
            {
                var k = Volatile.Read(ref h.next);
                if (k == null)
                {
                    item = default(T);
                    return false;
                }
                h.next = null;
                item = k.array[0];
                k.array[0] = null;
                k.pollIndex = 1;
                head = k;
                return true;
            }
            var e = Volatile.Read(ref a[offset]);
            if (e == null)
            {
                item = default(T);
                return false;
            }
            item = e;
            a[offset] = null;
            h.pollIndex = offset + 1;
            return true;
        }

        public bool TryDequeue(out T item)
        {
            var h = head;
            var a = h.array;
            var offset = h.pollIndex;
            if (offset == a.Length)
            {
                var k = Volatile.Read(ref h.next);
                if (k == null)
                {
                    item = default(T);
                    return false;
                }
                h.next = null;
                item = k.array[0];
                k.array[0] = null;
                k.pollIndex = 1;
                head = k;
                return true;
            }
            for (;;)
            {
                var e = Volatile.Read(ref a[offset]);
                if (e == null)
                {
                    var i = Volatile.Read(ref h.offerIndex);
                    if (i == offset)
                    {
                        item = default(T);
                        return false;
                    }
                    continue;
                }
                item = e;
                a[offset] = null;
                h.pollIndex = offset + 1;
                return true;
            }
        }
        
        public bool IsEmpty()
        {
            var h = head;
            return h.pollIndex == Volatile.Read(ref h.offerIndex) && Volatile.Read(ref h.next) == null;
        }

        public void Clear()
        {
            var h = head;

            for (;;)
            {
                int e = Volatile.Read(ref h.offerIndex);

                for (int i = 0; i < e; i++)
                {
                    h.array[i] = null;
                }

                var n = Volatile.Read(ref h.next);
                if (n == null)
                {
                    break;
                }
                h.next = null;
                h = n;
            }
            head = h;
        }

        internal sealed class Node
        {
            internal readonly T[] array;

            internal int offerIndex;

            internal int pollIndex;

            internal Node next;

            internal Node(int size)
            {
                array = new T[size];
            }

            internal Node(int size, T first) : this(size)
            {
                offerIndex = 1;
                Volatile.Write(ref array[0], first);
            }
        }

    }
}
