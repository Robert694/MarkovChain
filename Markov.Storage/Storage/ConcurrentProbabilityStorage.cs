using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using Markov.Random;

namespace Markov.Storage.Storage
{
    public class ConcurrentProbabilityStorage<TKey, TValue> : IStorage<TKey, TValue>
    {
        public IRandom Random;

        public ConcurrentProbabilityStorage() : this(new DefaultRandom())
        {
        }

        public ConcurrentProbabilityStorage(IRandom rng)
        {
            Random = rng;
        }

        public ConcurrentDictionary<TKey, ConcurrentDictionary<TValue, DataValues>> Store = new ConcurrentDictionary<TKey, ConcurrentDictionary<TValue, DataValues>>();

        public bool Contains(TKey key)
        {
            return Store.ContainsKey(key);
        }

        public bool Get(TKey key, out TValue value)
        {
            bool exists = Store.TryGetValue(key, out ConcurrentDictionary<TValue, DataValues> node);
            if (exists)
            {
                value = GetValue(node);
            }
            else
            {
                value = default;
            }

            return exists;
        }

        private TValue GetValue(ConcurrentDictionary<TValue, DataValues> node)
        {
            List<KeyValuePair<TValue, DataValues>> items = node.OrderBy(e => e.Value.Weight).ToList();
            double diceRoll = Random.Random();

            double cumulative = 0.0;
            for (int i = 0; i < items.Count; i++)
            {
                cumulative += items[i].Value.Weight;
                if (diceRoll < cumulative)
                {
                    TValue selectedElement = items[i].Key;
                    return selectedElement;
                }
            }

            return default;
        }

        private static void UpdateWeights(ConcurrentDictionary<TValue, DataValues> node)
        {
            long sum = node.Values.Sum(v => v.Count);
            foreach (var k in node.Keys)
            {
                node[k].Weight = (double)node[k].Count / sum;
            }

            uint maxCount = node.Values.Max(v => v.Count);
            if (maxCount >= 10000)
            {
                foreach (var k in node.Keys)
                {
                    node[k].Count = (uint)Math.Ceiling((double)node[k].Count / maxCount);
                }
            }
        }

        public void Set(TKey key, TValue value)
        {
            if (Store.ContainsKey(key)) //Table already exists
            {
                if (Store[key].ContainsKey(value)) //Key in table exists - just increment
                {
                    Store[key][value].Count++;
                }
                else //Key in table doesnt exist - add
                {
                    Store[key].TryAdd(value, new DataValues());
                }
            }
            else //Table doesnt exist - Create table and add key
            {
                var tv = new ConcurrentDictionary<TValue, DataValues>();
                tv.TryAdd(value, new DataValues());
                Store.TryAdd(key, tv);
            }

            UpdateWeights(Store[key]); //Updating weights
        }

        public class DataValues
        {
            public uint Count { get; set; } = 1;
            public double Weight { get; set; } = 0;
        }
    }
}
