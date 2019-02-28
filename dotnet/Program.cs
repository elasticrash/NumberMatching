using System;
using System.Linq;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using ProtoBuf;

namespace dotnet
{
    class Program
    {
        public static Dictionary<string, Index> Indices = new Dictionary<string, Index>();
        public static List<long> Numbers;
        public static int IndexPointer = 0;

        static void Main(string[] args)
        {

            ReadFromFiles();

            for (var i = 0; i < 10; i++)
            {
                var n = Numbers[i];
                if (i == 0)
                {
                    Console.WriteLine($"Printing sample numbers");
                }
                Console.WriteLine($"{n.ToString()}");
            }

            Console.WriteLine($"Generating Index {DateTime.Now}");
            for (var i = 0; i < Numbers.Count; i++)
            {
                var n = Numbers[i];
                var keyName = n.ToString().Substring(0, 1);
                var index = Indices.ContainsKey(keyName) ? Indices[keyName] : new Index();
                if (!Indices.ContainsKey(keyName))
                {
                    Indices[keyName] = index;
                }
                Tokenize(n.ToString(), index, i);
            }

            Console.WriteLine($"Index completed {DateTime.Now}");

            EnterValue();
        }


        private static void ReadFromFiles()
        {
            var files = from f in Directory.EnumerateFiles("./")
                        where f.EndsWith(".bin")
                        select f;

            foreach (var fl in files)
            {
                Console.WriteLine($"Processing file {fl} {DateTime.Now}");

                if (fl.Contains("numbers"))
                {
                    using (var file = File.OpenRead(fl))
                    {
                        Numbers = Serializer.Deserialize<List<long>>(file);
                    }
                }
            }
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
                var start = DateTime.Now.Ticks;
                var searchResult = NumberSearch(a);
                Console.WriteLine($"it took {new TimeSpan(DateTime.Now.Ticks - start).TotalMilliseconds} ms to search");
                Console.WriteLine(searchResult);

                EnterValue();
            }
        }

        private static void Tokenize(string n, Index index, Int32 id, int level = 1)
        {
            if (level == 1)
            {
                if (IndexPointer % 1000 == 0)
                {
                    Console.Write("\r building index {0}/{1}", IndexPointer, Numbers.Count);
                }

                IndexPointer++;
            }

            var charArray = n.ToCharArray();
            if (charArray.Length == 0) return;
            var nextLevel = level + 1;
            for (var i = 0; i < charArray.Length; i++)
            {
                var key = charArray[i].ToString();
                if (!index.Lookup.ContainsKey(key))
                {
                    var newIndex = new Index();
                    index.Lookup.Add(key, newIndex);
                    newIndex.Matches.Add(id);
                }
                var previousIndex = index.Lookup[key];
                var nextStep = n.Substring(i+1);

                PopulateNextLevel(nextStep, previousIndex, id, nextLevel);
            }
        }

        private static void PopulateNextLevel(string sub, Index index, Int32 id, int level)
        {
            if (sub.ToCharArray().Length == 0) return;
            var key = sub[0].ToString();
            
            if (!index.Lookup.ContainsKey(key))
            {
                var newIndex = new Index();
                index.Lookup.Add(key, newIndex);
                if (level > 3)
                {
                    newIndex.Matches.Add(id);
                }
            }
            else
            {
                Index existingIndex = (Index) index.Lookup[key];
                var exists = existingIndex.Matches.Exists(x => x == id);
                if (!exists)
                {
                    if (level > 3)
                    {
                        existingIndex.Matches.Add(id);
                    }
                }
            }
            
            var previousIndex = index.Lookup[key];
            var nextStep = sub.Substring(1);
            var nextLevel = level + 1;
            

            PopulateNextLevel(nextStep, previousIndex, id, nextLevel);
        }

        private static string NumberSearch(string search)
        {

            var charArray = search.ToCharArray();
            if (charArray.Length < 4) return "you need at least 4 characters to do a search";
            var tokens = charArray.Select(a => $"{a.ToString()}").ToList();

            var current = Indices[search.Substring(0,1)];
            var matches = new List<int>();
            var result = new List<Index>();

            foreach (var t in charArray)
            {
                if (!current.Lookup.ContainsKey(t.ToString()))
                {
                    current = null;
                    break;
                };
                current = current.Lookup[t.ToString()];
            }

            if (current != null)
            {
                result.Add(current);
            }
            else
            {
                return "no matches found";
            }
            
            if (matches.Count == 0) matches = result.SelectMany(x=>x.Matches).ToList().Distinct().ToList();

            if (matches.Count == 0) return "no matches found";
            var getNumbers = matches.Select(m => Numbers[m]).ToList();
            return String.Join(",", getNumbers);
        }
    }
}