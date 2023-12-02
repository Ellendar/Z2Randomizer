using System;
using Z2Randomizer.Core.Overworld;

namespace RandomizerCore.Flags;

public class ClimateFlagSerializer : IFlagSerializer
{
    public Climate Deserialize(int option)
    {
        return option switch
        {
            0 => Climates.Classic,
            1 => Climates.Chaos,
            2 => Climates.Wetlands,
            _ => throw new ArgumentException("Unrecognized climate index in ClimateFlagSerializer")
        };
    }

    public int GetLimit()
    {
        return 3;
    }

    public int Serialize(Object climate)
    {
        if(climate == null)
        {
            return 0;
        }
        return ((Climate)climate).Name switch
        {
            "Classic" => 0,
            "Chaos" => 1,
            "Wetlands" => 2,
            _ => throw new ArgumentException("Unrecognized climate type in ClimateFlagSerializer")
        };
    }
}