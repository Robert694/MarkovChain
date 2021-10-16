using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using Markov;
using Markov.KeyGen;
using Markov.Storage;
using Markov.Trainer;

namespace MarkovTest
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.Write("Order: ");
            int order = int.Parse(Console.ReadLine());

            Console.Write("MaxLength: ");
            int max = int.Parse(Console.ReadLine());

            Chain<string, string> m = new Chain<string, string>(
                new DefaultKeyGenMD5<string>(), 
                new DefaultProbabilityStorage<string, string>(),//new SQLiteProbabilityStorage<string>("sqldata.db") 
                new DefaultTrainer<string, string>(),
                order);

            using (var sr = new StreamReader(File.OpenRead("input.txt")))
            {
                while (!sr.EndOfStream)
                {
                    string line = sr.ReadLine();
            
                    IEnumerable<string> data = SplitChars(line + "\r");
                    m.Trainer.Train(data);
                }
            }

            var options = new GenerationOptionsBase() { MaximumLength = max };

            while (true)
            {
                var data = m.Trainer.Generate(options).TakeWhile(v => v != "\r");
                Console.WriteLine(string.Join("", data));
                Console.WriteLine(new string('-', Console.BufferWidth));
                Console.ReadKey();
            }
        }

        public static IEnumerable<string> SplitChars(string input)
        {
            TextElementEnumerator si = StringInfo.GetTextElementEnumerator(input);

            while (si.MoveNext())
            {
                yield return si.GetTextElement();
            }
        }
    }
}
