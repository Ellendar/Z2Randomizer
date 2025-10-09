using System;

namespace Z2Randomizer.RandomizerCore;

public class ImpossibleException : Exception
{
    public ImpossibleException() : base()
    {
        
    }

    public ImpossibleException(string message) : base(message)
    {

    }
}
