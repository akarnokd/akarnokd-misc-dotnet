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
using System.Collections;

namespace akarnokd_misc_dotnet.ix
{
    sealed class IxCharacters : IEnumerable<int>
    {
        readonly string s;

        public IxCharacters(string s)
        {
            this.s = s;
        }

        public IEnumerator<int> GetEnumerator()
        {
            return new CharactersEnumerator(s);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return new CharactersEnumerator(s);
        }

        sealed class CharactersEnumerator : IEnumerator<int>
        {
            readonly string s;

            int index;

            public CharactersEnumerator(string s)
            {
                this.s = s;
            }

            public int Current
            {
                get
                {
                    return s[index - 1];
                }
            }

            object IEnumerator.Current
            {
                get
                {
                    return s[index];
                }
            }

            public void Dispose()
            {
                // ignored
            }

            public bool MoveNext()
            {
                return index++ != s.Length;
            }

            public void Reset()
            {
                throw new NotImplementedException();
            }
        }
    }
}
