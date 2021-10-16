using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using Markov.Random;

namespace Markov.Storage
{
    public class DefaultRandomStorage<TKey, TValue> : IStorage<TKey, TValue>
    {
        public IRandom Random;
        public DefaultRandomStorage() : this(new DefaultRandom()) { }
        public DefaultRandomStorage(IRandom rng)
        {
            Random = rng;
        }

        public ConcurrentDictionary<TKey, NodeSet<TValue>> Store = new ConcurrentDictionary<TKey, NodeSet<TValue>>();

        public bool Contains(TKey key)
        {
            return Store.ContainsKey(key);
        }

        public bool Get(TKey key, out TValue value)
        {
            bool exists = Store.TryGetValue(key, out NodeSet<TValue> node);
            if (exists)
            {
                value = new List<TValue>(node.Next)[Random.Random(node.Next.Count)];
            }
            else
            {
                value = default;
            }
            return exists;
        }

        public void Set(TKey key, TValue value)
        {
            if (Store.ContainsKey(key))//Table already exists
            {
                if (Store[key].Next.Contains(value))//Key in table exists - just increment
                {

                }
                else//Key in table doesn't exist - add
                {
                    Store[key].Next.Add(value);
                }
            }
            else//Table doesn't exist - Create table and add key
            {
                Store.TryAdd(key, new NodeSet<TValue>()
                {
                    Next = new HashSet<TValue> { value }
                });
            }
        }

        public class NodeSet<T>
        {
            public HashSet<T> Next = new HashSet<T>();
        }
    }
}
