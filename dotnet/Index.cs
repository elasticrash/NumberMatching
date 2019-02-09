using System;
using System.Collections;
using System.Collections.Generic;

namespace dotnet
{
    public class Index
    {
        public Index()
        {
            Matches = new List<Int32>();
            Lookup = new Hashtable();
        }

        public List<Int32> Matches { get; set; }
        public Hashtable Lookup { get; set; }
    }
}
