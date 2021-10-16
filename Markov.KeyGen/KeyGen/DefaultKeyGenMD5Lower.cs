using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;

namespace Markov.KeyGen
{
    public class DefaultKeyGenMD5Lower<TValue> : IKeyGen<string, TValue>
    {
        public string Generate(IEnumerable<TValue> input)
        {
            return Hash(string.Join("|", input).ToLower());
        }

        private readonly MD5 _hash = MD5.Create();
        private readonly object lck = new object();
        private string Hash(string input)
        {
            lock (lck)
            {
                return BitConverter.ToString(_hash.ComputeHash(Encoding.UTF8.GetBytes(input))).Replace("-", string.Empty);
            }
        }
    }
}
