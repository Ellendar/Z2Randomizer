using System;

namespace Z2Randomizer.RandomizerCore.Flags;

internal class MinimumAttribute : Attribute
{
    public int Minimum { get; }
    public MinimumAttribute(int minimum) 
    {
        Minimum = minimum;
    }
}
