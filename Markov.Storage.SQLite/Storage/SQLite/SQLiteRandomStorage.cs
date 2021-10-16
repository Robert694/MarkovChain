using System.Collections.Generic;
using System.Data;
using System.Data.SQLite;
using Markov.Random;
using Newtonsoft.Json;

namespace Markov.Storage.SQLite
{
    public class SQLiteRandomStorage<TValue> : IStorage<string, TValue>
    {
        public IRandom Random;
        private string TableName { get; }
        private string ConnectString { get; }
        public SQLiteRandomStorage(string location, string tablename = "default", bool compress = true) : this(new DefaultRandom(), location, tablename, compress) { }
        public SQLiteRandomStorage(IRandom rng, string location, string tablename = "default", bool compress = true)
        {
            Random = rng;
            TableName = tablename;
            ConnectString = $"Data Source={location};Version=3;Pooling=True;Compress={compress};journal_mode=WAL;";
            connection = new SQLiteConnection(ConnectString);
            connection.Open();
            CreateTable(TableName);
        }

        private SQLiteConnection connection;

        private void CreateTable(string table)
        {
            using (SQLiteCommand cmd = new SQLiteCommand($"CREATE TABLE IF NOT EXISTS '{table}' (Id TEXT UNIQUE, Value TEXT);", connection))
            {
                cmd.ExecuteNonQuery();
            }
        }

        public bool Contains(string key)
        {
            using (SQLiteCommand cmd = new SQLiteCommand($"SELECT EXISTS(SELECT 1 FROM '{TableName}' WHERE Id=@id LIMIT 1);", connection))
            {
                cmd.Parameters.Add(new SQLiteParameter("@id", DbType.String) { Value = key });
                return cmd.ExecuteNonQuery() == 1;
            }
        }

        public bool Get(string key, out TValue value)
        {
            HashSet<TValue> entry = FetchEntry(key);
            if (entry != null)
            {
                value = new List<TValue>(entry)[Random.Random(entry.Count)];
                return true;
            }

            value = default;
            return false;
        }

        public void Set(string key, TValue value)
        {
            HashSet<TValue> entry = FetchEntry(key);
            if (entry != null)
            {
                if (!entry.Contains(value))
                {
                    entry.Add(value);
                    InsertOrUpdateEntry(key, entry);
                }
            }
            else
            {
                InsertOrUpdateEntry(key, new HashSet<TValue>(){value});
            }
        }

        private HashSet<TValue> FetchEntry(string key)
        {
            using (SQLiteCommand cmd = new SQLiteCommand($"SELECT Value FROM '{TableName}' WHERE Id=@id;", connection))
            {
                cmd.Parameters.Add(new SQLiteParameter("@id", DbType.String) { Value = key });
                SQLiteDataReader reader = cmd.ExecuteReader();
                if (reader.HasRows)
                {
                    reader.Read();
                    return JsonConvert.DeserializeObject<HashSet<TValue>>(reader.GetString(0));
                }
                return null;
            }
        }


        private void InsertOrUpdateEntry(string key, HashSet<TValue> values)
        {
            string json = JsonConvert.SerializeObject(values);
            using (SQLiteCommand cmd = new SQLiteCommand($"INSERT OR IGNORE INTO '{TableName}' (Id, Value) VALUES(@id,@value); UPDATE '{TableName}' SET Value=@value WHERE Id = @id;", connection))
            {
                cmd.Parameters.Add(new SQLiteParameter("@id", DbType.String) { Value = key });
                cmd.Parameters.Add(new SQLiteParameter("@value", DbType.String) { Value = json });
                cmd.ExecuteNonQuery();
            }
        }
    }
}
