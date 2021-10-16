namespace Markov
{
    public class Chain<TKey, TValue>
    {
        public IKeyGen<TKey, TValue> KeyGen { get; } 
        public IStorage<TKey, TValue> Storage { get; }
        public ITrainer<TKey, TValue> Trainer { get; }
        public int Order { get; }

        public Chain(IKeyGen<TKey, TValue> keyGen, IStorage<TKey, TValue> storage, ITrainer<TKey, TValue> trainer, int order)
        {
            KeyGen = keyGen;
            Storage = storage;
            Trainer = trainer;
            Order = order;
            trainer.Init(KeyGen, Storage, Order);
        }
    }
}