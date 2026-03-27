using System;

namespace Z2Randomizer.RandomizerCore.Flags;

internal class MaximumAttribute : Attribute
{
    public int Maximum { get; }
    public MaximumAttribute(int maximum) 
    {
        Maximum = maximum;
    }
}
