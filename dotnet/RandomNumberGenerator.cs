using System;
using System.Collections;
using System.Collections.Generic;

namespace dotnet
{
    public class RandomNumberGenerator
    {
        public List<long> RandomNumbers { get; set; }
        public RandomNumberGenerator(int arrayLength)
        {
            RandomNumbers = new List<long>();
            Random r = new Random();
            for (int i = 0; i < arrayLength; i++)
            {
                int rInt = r.Next(1000000, 9999999);
                RandomNumbers.Add(rInt);
            }
        }
    }
}