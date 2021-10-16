using System.Collections.Generic;

namespace Markov
{
    public interface ITrainer<TKey, TValue>
    {
        void Init(IKeyGen<TKey, TValue> keyGen, IStorage<TKey, TValue> storage, int order);
        void Train(IEnumerable<TValue> data);
        bool Next(IEnumerable<TValue> input, out TValue value);
        IEnumerable<TValue> Generate(GenerationOptionsBase options = null);
    }

    //public class GenOptions : Dictionary<string, object>
    //{
    //}

    public class GenerationOptionsBase
    {
        public int? MaximumLength { get; set; }
    }
}
