using System;
using System.Collections.Generic;
using System.Text;

namespace Z2Randomizer.RandomizerCore.Sidescroll.Town;

public enum TownType
{
    INVALID = 0,
    RAURU = 1,
    RUTO = 2,
    SARIA = 3,
    MIDO = 4,
    NABOORU = 5,
    DARUNIA = 6,
    NEW_KASUTO = 7,
    OLD_KASUTO = 8,
    BAGU = 9,
}

public static class TownExtensions
{
    public const int SPELL_GET_START_ADDRESS = 0x17AF7;
    public static int VanillaTownOrder(this TownType town)
    {
        return town switch
        {
            TownType.INVALID => 99,
            TownType.RAURU => 1,
            TownType.RUTO => 2,
            TownType.SARIA => 3,
            TownType.MIDO => 4,
            TownType.NABOORU => 5,
            TownType.DARUNIA => 6,
            TownType.NEW_KASUTO => 7,
            TownType.OLD_KASUTO => 8,
            _ => 0
        };
    }

    public static string HintName(this TownType town)
    {
        return town switch
        {
            TownType.RAURU => "RAURU",
            TownType.RUTO => "RUTO",
            TownType.SARIA => "SARIA",
            TownType.MIDO => "MIDO",
            TownType.NABOORU => "NABOORU",
            TownType.DARUNIA => "DARUNIA",
            TownType.NEW_KASUTO => "NEW KASUTO",
            TownType.OLD_KASUTO => "OLD KASUTO",
            _ => "UNKNOWN"
        };
    }
}

