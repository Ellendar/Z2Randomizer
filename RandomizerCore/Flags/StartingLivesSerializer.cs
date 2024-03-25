using System;
using Z2Randomizer.Core.Overworld;

namespace RandomizerCore.Flags;

public class StartingLivesSerializer : IFlagSerializer
{
    public object Deserialize(int option)
    {
        return option switch
        {
            0 => 1,
            1 => 2,
            2 => 3,
            3 => 4,
            4 => 5,
            5 => 8,
            6 => 16,
            7 => null,
            _ => throw new Exception("Invalid starting lives index")
        };
    }

    public int GetLimit()
    {
        return 8;
    }

    public int Serialize(object lives)
    {
        if(lives == null)
        {
            return 7;
        }
        return (int)lives switch
        {
            1 => 0,
            2 => 1,
            3 => 2,
            4 => 3,
            5 => 4,
            8 => 5,
            16 => 6,
            _ => throw new ArgumentException("Unrecognized starting lives option")
        };
    }
}