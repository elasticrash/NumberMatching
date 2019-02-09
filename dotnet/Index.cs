using System;
using System.Collections;

namespace dotnet
{
    public class Index
    {
        public ArrayList Matches { get; set; }
        public Hashtable Lookup { get; set; }
        public Index NextIndex { get; set; }
    }
}
