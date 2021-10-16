using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Markov.Random;
using Microsoft.Extensions.Caching.Memory;
using Newtonsoft.Json;

namespace Markov.Storage.File
{
    public class FileProbabilityStorage<TKey, TValue> : IStorage<TKey, TValue>
    {
        public IRandom Random;
        public string Directory { get; }
        public string Extension { get; }
        public bool UseCache { get; }
        public MemoryCache Cache { get; }
        public TimeSpan CacheDuration { get; set; }
        public FileProbabilityStorage(string directory, string extension = default, bool useCache = true) : this(new DefaultRandom(), directory, extension, useCache) { }
        public FileProbabilityStorage(IRandom rng, string directory, string extension = default, bool useCache = true)
        {
            Random = rng;
            Directory = directory;
            Extension = extension;
            UseCache = useCache;
            if (!System.IO.Directory.Exists(Directory))
            {
                System.IO.Directory.CreateDirectory(Directory);
            }
            if (UseCache)
            {
                CacheDuration = TimeSpan.FromMinutes(5);
                Cache = new MemoryCache(new MemoryCacheOptions());
            }
        }

        private string GetPath(TKey key)
        {
            return Path.Combine(Directory, $"{key}{Extension}");
        }

        public bool Contains(TKey key)
        {
            return System.IO.File.Exists(GetPath(key));
        }

        public bool Get(TKey key, out TValue value)
        {
            if (GetNode(key, out Dictionary<TValue,DataValues> node))
            {
                value = GetValue(node);
                return true;
            }
            value = default;
            return false;
        }

        public void Set(TKey key, TValue value)
        {
            string path = GetPath(key);
            if (!GetNode(key, out Dictionary<TValue,DataValues> hs))
            {
                hs = new Dictionary<TValue,DataValues>();
            }
            if (hs.ContainsKey(value))//Value already exists in table
            {
                hs[value].Count++;
            }
            else//Add Value to table
            {
                hs.Add(value, new DataValues());
            }
            UpdateWeights(hs);
            SaveNode(path, key, hs);
        }

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

        public bool GetNode(TKey key, out Dictionary<TValue,DataValues> node)
        {
            string path = GetPath(key);
            if (UseCache)
            {
                node = Cache.GetOrCreate(key, entry =>
                {
                    entry.SlidingExpiration = CacheDuration;
                    return GetNode(path);
                });
                return node != null;
            }
            node = GetNode(path);
            return node != null;
        }

        public static Dictionary<TValue,DataValues> GetNode(string path)
        {
            return System.IO.File.Exists(path) ? JsonConvert.DeserializeObject<Dictionary<TValue,DataValues>>(System.IO.File.ReadAllText(path)) : null;
        }

        private void SaveNode(string path, TKey key, Dictionary<TValue,DataValues> hs)
        {
            System.IO.File.WriteAllText(path, JsonConvert.SerializeObject(hs));
            if (UseCache)//Perform caching if caching is enabled
            {
                Cache.Set(key, hs, new MemoryCacheEntryOptions() { SlidingExpiration = CacheDuration });
            }
        }

        public class DataValues
        {
            public uint Count { get; set; } = 1;
            public double Weight { get; set; } = 0;
        }
    }
}
