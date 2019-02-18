using System;
using System.Linq;
using System.Collections.Generic;
using System.IO;
using ProtoBuf;

namespace dotnet
{
    class Program
    {
        public static Index Index = new Index();
        public static List<long> Numbers;
        public static int IndexPointer = 0;

        static void Main(string[] args)
        {

            ReadFromFiles();

            Console.WriteLine($"Generating Index {DateTime.Now}");
            for (var i = 0; i < 10000; i++)
            {
                var n = Numbers[i];
                Tokenize(n.ToString(), Index, i);
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
                // else if (fl.Contains("index"))
                // {
                //     using (var file = File.OpenRead(fl))
                //     {
                //         Indices.Add(Serializer.Deserialize<Index>(file));
                //     }
                // }
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
                Console.Write("\r building index {0}/{1}", IndexPointer, Numbers.Count);
                IndexPointer++;
            }
            
            var charArray = n.ToCharArray();
            if(charArray.Length == 0) return;
            var nextStep = n.Substring(1);
            var nextLevel = level + 1;
            for (var i = 0; i < charArray.Length; i++)
            {
                var a = charArray[i];
                var key = (charArray.Length - i > 4) ? $"{a}" : n.ToString().Substring(i);
                if(charArray.Length - i < 4) return;
                //var key = a.ToString();
                if (!index.Lookup.ContainsKey(key))
                {
                    var newIndex = new Index();
                    index.Lookup.Add(key, newIndex);
                    var exists = newIndex.Matches.Exists(x => x == id);
                    if (!exists)
                    {
                        newIndex.Matches.Add(id);
                    }

                    Tokenize(nextStep, newIndex, id, nextLevel);
                }
                else
                {
                    Index existingIndex = (Index) index.Lookup[key];
                    existingIndex.Matches.Add(id);
                    Tokenize(nextStep, existingIndex, id, nextLevel);
                }
            }


        }

        private static string NumberSearch(string search)
        {
            var charArray = search.ToCharArray();
            var tokens = new List<string>();

            for (var i = 0; i < charArray.Length; i++)
            {
                var a = charArray[i];
                if (charArray.Length - i < 4) tokens.Add($"{search.ToString().Substring(i)}");
                tokens.Add($"{a.ToString()}");
            }

            var result = Index;
            var matches = new List<int>();
            foreach (var t in tokens)
            {
                if (result.Lookup.ContainsKey(t))
                {
                    result = result.Lookup[t];
                } 
//                else if (t.Length > 1){
//                    var keyList = result.Lookup.Keys.Where(k => k.StartsWith(t)).ToList();
//
//                    foreach (var res in keyList)
//                    {
//                        matches.AddRange(result.Lookup[res].Matches);
//                    }
//                }
                else
                {
                    result = null;
                    break;
                }
            }

            if (result != null && matches.Count == 0) matches = result.Matches; 

            //if (matches.Count == 0) return "no matches found";
            //var getNumbers = matches.Select(m => Numbers[m]).ToList();
            //return String.Join(",", getNumbers);
            
            if (result == null) return "no matches found";
            var getNumbers = result.Matches.Select(m => Numbers[m]).ToList();
            return String.Join(",", getNumbers);
        }
    }
}