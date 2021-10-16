using System.Collections.Generic;

namespace Markov
{
    public interface IKeyGen<out TKey, in TValue>
    {
        TKey Generate(IEnumerable<TValue> input);
    }
}
