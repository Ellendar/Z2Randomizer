using System;

namespace Z2Randomizer.RandomizerCore;

public class ImpossibleFlagsException : Exception
{
    public ImpossibleFlagsException() : base()
    {
    }

    public ImpossibleFlagsException(string? message) : base(message)
    {
    }
}
