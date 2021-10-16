using System.Collections.Generic;

namespace Markov.KeyGen
{
    public class KeyGenToStringConverter<TKey, TValue> : IKeyGen<string, TValue>
    {
        private IKeyGen<TKey, TValue> Kg { get; }
        public KeyGenToStringConverter(IKeyGen<TKey, TValue> kg)
        {
            Kg = kg;
        }
        public string Generate(IEnumerable<TValue> input)
        {
            return Kg.Generate(input).ToString();
        }
    }
}
