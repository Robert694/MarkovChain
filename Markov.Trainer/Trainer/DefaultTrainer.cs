using System.Collections.Generic;
using System.Linq;

namespace Markov.Trainer
{
    public class DefaultTrainer<TKey, TValue> : ITrainer<TKey, TValue>
    {
        private IKeyGen<TKey, TValue> KeyGen { get; set; }
        private IStorage<TKey, TValue> Storage { get; set; }
        private int Order { get; set; }
        private IEnumerable<TValue> _startseq;
        public void Init(IKeyGen<TKey, TValue> keyGen, IStorage<TKey, TValue> storage, int order)
        {
            KeyGen = keyGen;
            Storage = storage;
            Order = order;
            _startseq = Enumerable.Repeat<TValue>(default, Order);
        }

        public void Train(IEnumerable<TValue> data)
        {
            Queue<TValue> tQueue = new Queue<TValue>(_startseq);
            //for (int i = 0; i < Order; i++) { tQueue.Enqueue(default); }

            foreach (var value in data)
            {
                Storage.Set(KeyGen.Generate(tQueue), value);
                tQueue.Enqueue(value);
                tQueue.Dequeue();
            }
        }

        public bool Next(IEnumerable<TValue> input, out TValue value)
        {
            return Storage.Get(KeyGen.Generate(input), out value);
        }

        public IEnumerable<TValue> Generate(GenerationOptionsBase options = null)
        {
            Queue<TValue> tQueue = new Queue<TValue>(_startseq);
            int count = 0;
            int? max = options?.MaximumLength;
            while (Next(tQueue, out TValue value))
            {
                count++;
                yield return value;
                tQueue.Enqueue(value);
                tQueue.Dequeue();
                if (count >= max) break;
            }
        }

    }
}
