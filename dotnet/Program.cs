using System;
using System.Linq;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using ProtoBuf;

namespace dotnet
{
    internal static class Program
    {
        private static readonly Dictionary<int, Index> Indices = new Dictionary<int, Index>();
        private static List<long> _numbers;
        private static int _indexPointer = 0;

        private static void Main(string[] args)
        {
            ReadFromFiles();

            for (var i = 0; i < 10; i++)
            {
                var n = _numbers[i];
                if (i == 0)
                {
                    Console.WriteLine($"Printing sample numbers");
                }

                Console.WriteLine($"{n.ToString()}");
            }

            Console.WriteLine($"Generating Index {DateTime.Now}");
            for (var i = 0; i < _numbers.Count; i++)
            {
                var n = _numbers[i];
                Tokenize(n.ToString(), Indices, i);
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

                if (!fl.Contains("numbers")) continue;
                using (var file = File.OpenRead(fl))
                {
                    _numbers = Serializer.Deserialize<List<long>>(file);
                }
            }
        }

        private static void EnterValue()
        {
            Console.WriteLine($"type any number and press enter / or x to exit");

            var a = Console.ReadLine();

            if (a == "x") return;
            var start = DateTime.Now.Ticks;
            var searchResult = NumberSearch(a);
            Console.WriteLine($"it took {new TimeSpan(DateTime.Now.Ticks - start).TotalMilliseconds} ms to search");
            Console.WriteLine(searchResult);
            EnterValue();
        }

        private static void Tokenize(string n, Dictionary<int, Index> Indices, int id, int level = 1)
        {
            if (level == 1)
            {
                if (_indexPointer % 1000 == 0)
                {
                    Console.Write("\r building index {0}/{1}", _indexPointer, _numbers.Count);
                }

                _indexPointer++;
            }

            var charArray = n.ToCharArray();
            var nextLevel = level + 1;
            for (var i = 0; i < charArray.Length; i++)
            {
                var key = Convert.ToInt32(charArray[i].ToString());
                if (!Indices.ContainsKey(key))
                {
                    var newIndex = new Index();
                    Indices.Add(key, newIndex);
                }
                
                if (charArray.Length - i < 4) continue;
                var nextStep = n.Substring(i + 1);
                var nextKey = Convert.ToInt32(charArray[i + 1].ToString());
                Index nextIndex = null;

                    if (Indices[key].Lookup.ContainsKey(nextKey))
                    {
                        nextIndex = Indices[key].Lookup[nextKey];
                    }
                    else
                    {
                        nextIndex = new Index();
                        Indices[key].Lookup.Add(nextKey, nextIndex);
                    }

                PopulateNextLevel(nextStep, nextIndex, id, nextLevel);
            }
        }

        private static void PopulateNextLevel(string sub, Index index, Int32 id, int level)
        {
            if (sub.ToCharArray().Length == 0) return;
            var key = Convert.ToInt32(sub[0].ToString());

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

            var current = Indices[Convert.ToInt32(search.Substring(0, 1))];
            var matches = new List<int>();
            var result = new List<Index>();

            foreach (var t in charArray)
            {
                var key = t.ToString();
                if (!current.Lookup.ContainsKey(Convert.ToInt32(key)))
                {
                    current = null;
                    break;
                }
                
                current = current.Lookup[Convert.ToInt32(key)];
            }

            if (current != null)
            {
                result.Add(current);
            }
            else
            {
                return "no matches found";
            }

            if (matches.Count == 0) matches = result.SelectMany(x => x.Matches).ToList().Distinct().ToList();

            if (matches.Count == 0) return "no matches found";
            var getNumbers = matches.Select(m => _numbers[m]).ToList();
            return string.Join(",", getNumbers);
        }
    }
}