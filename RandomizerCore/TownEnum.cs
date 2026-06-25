/*
using System;

namespace Z2Randomizer.RandomizerCore;

public enum TownEnum
{
    INVALID = 0,
    RAURU = 1,
    RUTO = 2,
    SARIA_NORTH = 3,
    SARIA_SOUTH = 4,
    MIDO_WEST = 5,
    MIDO_CHURCH = 6,
    NABOORU = 7,
    DARUNIA_WEST = 8,
    DARUNIA_ROOF = 9,
    NEW_KASUTO = 10,
    OLD_KASUTO = 11,
    SARIA_TABLE = 12,
    BAGU = 13,
    NABOORU_FOUNTAIN = 14,
}

public static class TownExtensions
{
    public const int SPELL_GET_START_ADDRESS = 0x17AF7;
    public static int VanillaTownOrder(this TownEnum town)
    {
        return town switch
        {
            TownEnum.INVALID => 99,
            TownEnum.RAURU => 1,
            TownEnum.RUTO => 2,
            TownEnum.SARIA_NORTH => 3,
            TownEnum.MIDO_WEST => 4,
            TownEnum.NABOORU => 5,
            TownEnum.DARUNIA_WEST => 6,
            TownEnum.NEW_KASUTO => 7,
            TownEnum.OLD_KASUTO => 8,
            _ => 0
        };
    }

    public static bool IsWizardTown(this TownEnum town)
    {
        return town switch
        {
            TownEnum.RAURU => true,
            TownEnum.RUTO => true,
            TownEnum.SARIA_NORTH => true,
            TownEnum.MIDO_WEST => true,
            TownEnum.NABOORU => true,
            TownEnum.DARUNIA_WEST => true,
            TownEnum.NEW_KASUTO => true,
            TownEnum.OLD_KASUTO => true,
            TownEnum.MIDO_CHURCH => true,
            TownEnum.DARUNIA_ROOF => true,
            _ => false
        };
    }

    public static string HintName(this TownEnum town)
    {
        return town switch
        {
            TownEnum.RAURU => "RAURU",
            TownEnum.RUTO => "RUTO",
            TownEnum.SARIA_NORTH => "SARIA",
            TownEnum.MIDO_WEST => "MIDO",
            TownEnum.NABOORU => "NABOORU",
            TownEnum.DARUNIA_WEST => "DARUNIA",
            TownEnum.NEW_KASUTO => "NEW KASUTO",
            TownEnum.OLD_KASUTO => "OLD KASUTO",
            TownEnum.MIDO_CHURCH => "MIDO",
            TownEnum.DARUNIA_ROOF => "DARUNIA",
            _ => "UNKNOWN"
        };
    }

    public static bool AppearsOnMap(this TownEnum town)
    {
        return town switch
        {
            TownEnum.RAURU => true,
            TownEnum.RUTO => true,
            TownEnum.SARIA_NORTH => true,
            TownEnum.SARIA_SOUTH => true,
            TownEnum.MIDO_WEST => true,
            TownEnum.MIDO_CHURCH => false,
            TownEnum.NABOORU => true,
            TownEnum.DARUNIA_WEST => true,
            TownEnum.DARUNIA_ROOF => false,
            TownEnum.NEW_KASUTO => true,
            TownEnum.OLD_KASUTO => true,
            TownEnum.SARIA_TABLE => false,
            TownEnum.BAGU => true,
            TownEnum.NABOORU_FOUNTAIN => false,
            _ => throw new Exception("Unrecognized town: " + town.ToString())
        };
    }

    public static TownEnum GetMasterTown(this TownEnum town)
    {
        return town switch
        {
            TownEnum.SARIA_TABLE => TownEnum.SARIA_NORTH,
            TownEnum.DARUNIA_ROOF => TownEnum.DARUNIA_WEST,
            TownEnum.NABOORU_FOUNTAIN => TownEnum.NABOORU,
            TownEnum.MIDO_CHURCH => TownEnum.MIDO_WEST,
            _ => town
        };
    }

    public static bool IsUnderConsiderationForReachable(this TownEnum town)
    {
        return town switch
        {
            TownEnum.RAURU => true,
            TownEnum.RUTO => true,
            TownEnum.SARIA_NORTH => true,
            TownEnum.SARIA_SOUTH => true,
            TownEnum.MIDO_WEST => true,
            TownEnum.MIDO_CHURCH => false,
            TownEnum.NABOORU => true,
            TownEnum.DARUNIA_WEST => true,
            TownEnum.DARUNIA_ROOF => false,
            TownEnum.NEW_KASUTO => true,
            TownEnum.OLD_KASUTO => true,
            TownEnum.SARIA_TABLE => false,
            TownEnum.BAGU => true,
            TownEnum.NABOORU_FOUNTAIN => false,
            _ => throw new Exception("Unrecognized town: " + town.ToString())
        };
    }
}
*/