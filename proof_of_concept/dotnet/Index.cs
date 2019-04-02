using System;
using System.Collections;
using System.Collections.Generic;

namespace dotnet
{
    [Serializable]
    public class Index
    {
        public Index()
        {
            Matches = new List<Int32>();
            Lookup = new Dictionary<Int32, Index>();
        }

        public List<Int32> Matches { get; set; }
        public Dictionary<Int32, Index> Lookup { get; set; }
    }
}
