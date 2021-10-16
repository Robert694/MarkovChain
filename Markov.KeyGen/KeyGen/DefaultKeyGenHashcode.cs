using System.Collections.Generic;

namespace Markov.KeyGen
{
    public class DefaultKeyGenHashcode<TValue> : IKeyGen<int, TValue>
    {
        public int Generate(IEnumerable<TValue> input)
        {
            return string.Join("|", input).GetHashCode();
        }
    }
}
