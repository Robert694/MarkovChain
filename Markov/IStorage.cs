namespace Markov
{
    public interface IStorage<in TKey, TValue>
    {
        bool Get(TKey key, out TValue value);
        void Set(TKey key, TValue value);
        bool Contains(TKey key);
    }
}
