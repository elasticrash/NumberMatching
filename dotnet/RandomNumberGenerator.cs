using System;
using System.Collections;

namespace dotnet
{
    public class RandomNumberGenerator
    {
        public ArrayList randomNumbers { get; set; }
        public RandomNumberGenerator(int arrayLength)
        {
            randomNumbers = new ArrayList<long>();

        }
    }
}