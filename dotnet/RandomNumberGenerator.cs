using System;
using System.Collections;
using System.Collections.Generic;
using ProtoBuf;

namespace dotnet
{
    [ProtoContract]
    public class RandomNumberGenerator
    {
        [ProtoMember(1)]
        public List<long> RandomNumbers { get; set; }
        public RandomNumberGenerator(int arrayLength)
        {
            RandomNumbers = new List<long>();
            Random r = new Random();
            var i = 0;
            while (i < arrayLength)
            {
                long rInt = r.Next(1000000, 9999999);
                var exists = RandomNumbers.Exists(x => x == rInt);
                if (!exists)
                {
                    RandomNumbers.Add(rInt);
                    i++;
                    Console.Write("\r{0}", i);
                }
            }
        }
    }
}