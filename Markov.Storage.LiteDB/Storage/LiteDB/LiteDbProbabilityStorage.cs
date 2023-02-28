using System;
using System.Collections.Generic;
using System.Linq;
using LiteDB;
using Markov.Random;
using Newtonsoft.Json;

namespace Markov.Storage.LiteDB
{
    public class LiteDbProbabilityStorage<TValue> : IStorage<string, TValue>
    {
        public IRandom Random;
        private string TableName { get; }
        public LiteDbProbabilityStorage(string location, string tablename = "default") : this(new DefaultRandom(), location, tablename) { }
        public LiteDbProbabilityStorage(IRandom rng, string location, string tablename = "default")
        {
            Random = rng;
            TableName = tablename;
            Database = new LiteDatabase(location);
            Collection = Database.GetCollection<Data>(TableName);
        }

        public LiteDatabase Database { get; }
        public ILiteCollection<Data> Collection { get; }

        public bool Contains(string key)
        {
            return Collection.FindById(key) != null;
        }

        public bool Get(string key, out TValue value)
        {
            Data data = Collection.FindById(key);
            if (data != null)
            {
                Dictionary<TValue, DataValues> hs = JsonConvert.DeserializeObject<Dictionary<TValue,DataValues>>(data.Next);
                value = GetValueOverride != null ? GetValueOverride(hs) : GetValue(hs);
                return true;
            }
            value = default;
            return false;
        }

        public void Set(string key, TValue value)
        {
            Data table = Collection.FindById(key);
            if (table != null)//Table exists
            {
                var set = JsonConvert.DeserializeObject<Dictionary<TValue,DataValues>>(table.Next);
                if (set.ContainsKey(value))//Already exists in table
                {
                    set[value].Count++;
                }
                else//Doesnt exist in table
                {
                    set.Add(value, new DataValues());
                }
                UpdateWeights(set);
                table.Next = JsonConvert.SerializeObject(set);
                Collection.Update(table);
            }
            else//Table doesnt exist
            {
                var set = new Dictionary<TValue, DataValues>
                {
                    [value] = new DataValues()
                };
                UpdateWeights(set);
                table = new Data
                {
                    Key = key,
                    Next = JsonConvert.SerializeObject(set)
                };
            }
            Collection.Upsert(table);
        }

        private static void UpdateWeights(Dictionary<TValue, DataValues> node)
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

        public Func<Dictionary<TValue, DataValues>, TValue> GetValueOverride = null;

        private TValue GetValue(Dictionary<TValue, DataValues> node)
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

        public class Data
        {
            [BsonId]
            public string Key { get; set; }
            //public Dictionary<TValue,DataValues> Next { get; set; }
            public string Next { get; set; }
        }

        public class DataValues
        {
            public uint Count { get; set; } = 1;
            public double Weight { get; set; } = 0;
        }
    }
}
