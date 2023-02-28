using System;
using System.Collections.Generic;
using LiteDB;
using Markov.Random;
using Newtonsoft.Json;

namespace Markov.Storage.LiteDB
{
    public class LiteDbRandomStorage<TValue> : IStorage<string, TValue>
    {
        public IRandom Random;
        private string TableName { get; }
        public LiteDbRandomStorage(string location, string tablename = "default") : this(new DefaultRandom(), location, tablename) { }
        public LiteDbRandomStorage(IRandom rng, string location, string tablename = "default")
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
                var hs = JsonConvert.DeserializeObject<HashSet<TValue>>(data.Next);
                value = new List<TValue>(hs)[Random.Random(0, hs.Count)];
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
                var hs = JsonConvert.DeserializeObject<HashSet<TValue>>(table.Next);
                if (hs.Contains(value))//Already exists in table
                {

                }
                else//Doesnt exist in table
                {
                    hs.Add(value);
                    table.Next = JsonConvert.SerializeObject(hs);
                    Collection.Update(table);
                }
            }
            else//Table doesnt exist
            {
                //Collection.Insert(new Data{ Key = key, Next = new HashSet<TValue>() {value}});
                Collection.Insert(new Data { Key = key, Next = JsonConvert.SerializeObject(new HashSet<TValue>() { value }) });
            }
        }

        public class Data
        {
            [BsonId]
            public string Key { get; set; }
            //public HashSet<TValue> Next { get; set; }
            public string Next { get; set; }
        }
    }
}
