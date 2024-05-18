using System;

namespace RandomizerCore;

public class ImpossibleException : Exception
{
    public ImpossibleException() : base()
    {
        
    }

    public ImpossibleException(string message) : base(message)
    {

    }
}
