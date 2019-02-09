using System;
using System.Linq;
using System.Collections.Generic;
using System.Globalization;

namespace dotnet
{
    class Program
    {
        public static Index Index = new Index();
        public static int Size = 10000;
        public static List<long> Numbers = new RandomNumberGenerator(Size).RandomNumbers;
        public static int IndexPointer = 0;

        static void Main(string[] args)
        {
            Console.WriteLine($"Generating Index {DateTime.Now}");
            for (var i = 0; i < Numbers.Count; i++)
            {
                var n = Numbers[i];
                Tokenize(n, Index, i);
            }

            Console.WriteLine($"Index completed {DateTime.Now}");

            EnterValue();
        }

        private static void EnterValue()
        {
            Console.WriteLine($"type any number and press enter / or x to exit");

            var a = Console.ReadLine();

            if (a == "x")
            {
            }
            else
            {
                var start  = DateTime.Now.Ticks;
                var searcResult = NumberSearch(a);
                Console.WriteLine($"it took {new TimeSpan(DateTime.Now.Ticks - start).TotalMilliseconds} ms to search");
                Console.WriteLine(searcResult);

                EnterValue();
            }
        }

        private static void Tokenize(long n, Index index, Int32 id, int level = 1)
        {
            var chararray = n.ToString().ToCharArray();
            var nextStep = n.ToString().Substring(1);
            var nextLevel = level + 1;
            for (int i = 0; i < chararray.Length-1; i++)
            {
                var charA = chararray[i];
                var charB = chararray[i + 1];
                var key = $"{charA}{charB}";
                if (!index.Lookup.ContainsKey(key))
                {
                    var newIndex = new Index();
                    index.Lookup.Add(key, newIndex);
                    var exists = newIndex.Matches.Exists(x => x == id);
                    if (!exists)
                    {
                        newIndex.Matches.Add(id);
                    }
                    Tokenize(Convert.ToInt32(nextStep), newIndex, id, nextLevel);
                }
                else
                {
                    Index existingIndex = (Index)index.Lookup[key];
                    existingIndex.Matches.Add(id);
                    Tokenize(Convert.ToInt32(nextStep), existingIndex, id, nextLevel);
                }
            }

            if (level == 1)
            {
                Console.Write("\r building index {0}/{1}", IndexPointer, Size);
                IndexPointer++;
            }
        }

        private static string NumberSearch(string search)
        {
            var chararray = search.ToCharArray();
            var tokens = new List<string>();

            for (int i = 0; i < chararray.Length - 1; i++)
            {
                var charA = chararray[i];
                var charB = chararray[i + 1];
                tokens.Add($"{charA}{charB}");
            }

            var result = Index;
            foreach (var t in tokens)
            {
                if (result.Lookup.ContainsKey(t))
                {
                    result = (Index) result.Lookup[t];
                }
                else
                {
                    result = null;
                    break;
                }
            }

            if (result == null) return "no matches found";
            var getNumbers = result.Matches.Select(m => Numbers[m]).ToList();
            return String.Join(",", getNumbers);
        }
    }
}
