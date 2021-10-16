using System;
using System.Collections.Generic;
using System.IO;
using Markov.Random;
using Microsoft.Extensions.Caching.Memory;
using Newtonsoft.Json;

namespace Markov.Storage.File
{
    public class FileRandomStorage<TKey, TValue> : IStorage<TKey, TValue>
    {
        public IRandom Random;
        public string Directory { get; }
        public string Extension { get; }
        public bool UseCache { get; }
        public MemoryCache Cache { get; }
        public TimeSpan CacheDuration { get; set; }
        public FileRandomStorage(string directory, string extension = default, bool useCache = true) : this(new DefaultRandom(), directory, extension, useCache) { }
        public FileRandomStorage(IRandom rng, string directory, string extension = default, bool useCache = true)
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
            if (GetNode(key, out HashSet<TValue> node))
            {
                value = new List<TValue>(node)[Random.Random(node.Count)];
                return true;
            }
            value = default;
            return false;
        }

        public void Set(TKey key, TValue value)
        {
            string path = GetPath(key);
            if (!GetNode(key, out HashSet<TValue> hs))
            {
                hs = new HashSet<TValue>();
            }
            if(hs.Contains(value))//Value already exists in table
            {
            }
            else//Add Value to table
            {
                hs.Add(value);
                SaveNode(path, key, hs);
            }
        }

        public bool GetNode(TKey key, out HashSet<TValue> node)
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

        public static HashSet<TValue> GetNode(string path)
        {
            return System.IO.File.Exists(path) ? JsonConvert.DeserializeObject<HashSet<TValue>>(System.IO.File.ReadAllText(path)) : null;
        }

        private void SaveNode(string path, TKey key, HashSet<TValue> hs)
        {
            System.IO.File.WriteAllText(path, JsonConvert.SerializeObject(hs));
            if (UseCache)//Perform caching if caching is enabled
            {
                Cache.Set(key, hs, new MemoryCacheEntryOptions(){SlidingExpiration = CacheDuration});
            }
        }
    }
}
