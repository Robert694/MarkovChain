using System.Collections.Generic;

namespace Markov.KeyGen
{
    public class DefaultKeyGen<TValue> : IKeyGen<string, TValue>
    {
        public string Generate(IEnumerable<TValue> input)
        {
            return string.Join("|", input);
        }
    }
}
