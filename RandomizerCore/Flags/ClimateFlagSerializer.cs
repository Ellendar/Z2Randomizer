using System;
using Z2Randomizer.Core.Overworld;

namespace RandomizerCore.Flags;

public class ClimateFlagSerializer : IFlagSerializer
{
    public object Deserialize(int option)
    {
        return option switch
        {
            0 => Climates.Classic,
            1 => Climates.Chaos,
            2 => Climates.Wetlands,
            3 => Climates.GreatLakes,
            4 => Climates.Scrubland,
            _ => throw new ArgumentException("Unrecognized climate index in ClimateFlagSerializer")
        };
    }

    public int GetLimit()
    {
        return 5;
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
            "GreatLakes" => 3,
            "Scrubland" => 4,
            _ => throw new ArgumentException("Unrecognized climate type in ClimateFlagSerializer")
        };
    }
}