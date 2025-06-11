using System;

namespace Z2Randomizer.RandomizerCore;

public class UserFacingException : Exception
{
    public readonly string Heading;

    public UserFacingException() : base()
    {
        Heading = "";
    }

    public UserFacingException(string heading, string body) : base(body)
    {
        this.Heading = heading;
    }
}
