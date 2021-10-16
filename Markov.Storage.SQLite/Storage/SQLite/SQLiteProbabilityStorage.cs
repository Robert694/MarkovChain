using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SQLite;
using System.Linq;
using Markov.Random;
using Newtonsoft.Json;

namespace Markov.Storage.SQLite
{
    public class SQLiteProbabilityStorage<TValue> : IStorage<string, TValue>
    {
        public IRandom Random;
        private string TableName { get; }
        private string ConnectString { get; }
        public SQLiteProbabilityStorage(string location, string tablename = "default", bool compress = true) : this(new DefaultRandom(), location, tablename, compress) { }
        public SQLiteProbabilityStorage(IRandom rng, string location, string tablename = "default", bool compress = true)
        {
            Random = rng;
            TableName = tablename;
            ConnectString = $"Data Source={location};Version=3;Pooling=True;Compress={compress};journal_mode=WAL;";
            connection = GetConnection();
            CreateTable(TableName);
        }

        private SQLiteConnection connection;

        private SQLiteConnection GetConnection()
        {
            var connection = new SQLiteConnection(ConnectString);
            connection.Open();
            return connection;
        }

        private void CreateTable(string table)
        {
            using SQLiteCommand cmd = new SQLiteCommand($"CREATE TABLE IF NOT EXISTS '{table}' (Id TEXT UNIQUE, Value TEXT);", connection);
            cmd.ExecuteNonQuery();
        }

        public bool Contains(string key)
        {
            using SQLiteCommand cmd = new SQLiteCommand($"SELECT EXISTS(SELECT 1 FROM '{TableName}' WHERE Id=@id LIMIT 1);", connection);
            cmd.Parameters.Add(new SQLiteParameter("@id", DbType.String) { Value = key });
            return cmd.ExecuteNonQuery() == 1;
        }

        public bool Get(string key, out TValue value)
        {
            Dictionary<TValue, DataValues> entry = FetchEntry(key);
            if (entry != null)
            {
                value = GetValueOverride != null ? GetValueOverride(entry) : GetValue(entry);
                return true;
            }

            value = default;
            return false;
        }

        public void Set(string key, TValue value)
        {
            Dictionary<TValue, DataValues> entry = FetchEntry(key);
            if (entry != null)
            {
                if (!entry.ContainsKey(value))
                {
                    entry.Add(value, new DataValues());
                    InsertOrUpdateEntry(key, entry);
                }
                else
                {
                    entry[value].Count++;
                }
            }
            else
            {
                entry = new Dictionary<TValue, DataValues> {{value, new DataValues()}};
            }
            UpdateWeights(entry);
            InsertOrUpdateEntry(key, entry);
        }

        private Dictionary<TValue, DataValues> FetchEntry(string key)
        {
            using SQLiteCommand cmd = new SQLiteCommand($"SELECT Value FROM '{TableName}' WHERE Id=@id;", connection);
            cmd.Parameters.Add(new SQLiteParameter("@id", DbType.String) { Value = key });
            SQLiteDataReader reader = cmd.ExecuteReader();
            if (reader.HasRows)
            {
                reader.Read();
                var value = reader.GetString(0);
                return JsonConvert.DeserializeObject<Dictionary<TValue, DataValues>>(value);
            }
            return null;
        }


        private void InsertOrUpdateEntry(string key, Dictionary<TValue, DataValues> values)
        {
            string json = JsonConvert.SerializeObject(values);
            using SQLiteCommand cmd = new SQLiteCommand($"INSERT OR IGNORE INTO '{TableName}' (Id, Value) VALUES(@id,@value); UPDATE '{TableName}' SET Value=@value WHERE Id = @id;", connection);
            cmd.Parameters.Add(new SQLiteParameter("@id", DbType.String) { Value = key });
            cmd.Parameters.Add(new SQLiteParameter("@value", DbType.String) { Value = json });
            cmd.ExecuteNonQuery();
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

        public class DataValues
        {
            public uint Count { get; set; } = 1;
            public double Weight { get; set; } = 0;
        }
    }
}
