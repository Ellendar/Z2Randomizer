using System;

namespace Z2Randomizer.RandomizerCore.Flags;

internal class LimitAttribute : Attribute
{
    public int Limit { get; }
    public LimitAttribute(int limit) 
    {
        Limit = limit;
    }
}
