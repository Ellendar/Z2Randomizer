using System;

namespace RandomizerCore;

public enum Town
{
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
    public static int VanillaTownOrder(this Town town)
    {
        return town switch
        {
            Town.RAURU => 1,
            Town.RUTO => 2,
            Town.SARIA_NORTH => 3,
            Town.MIDO_WEST => 4,
            Town.NABOORU => 5,
            Town.DARUNIA_WEST => 6,
            Town.NEW_KASUTO => 7,
            Town.OLD_KASUTO => 8,
            _ => 0
        };
    }

    public static bool IsWizardTown(this Town town)
    {
        return town switch
        {
            Town.RAURU => true,
            Town.RUTO => true,
            Town.SARIA_NORTH => true,
            Town.MIDO_WEST => true,
            Town.NABOORU => true,
            Town.DARUNIA_WEST => true,
            Town.NEW_KASUTO => true,
            Town.OLD_KASUTO => true,
            Town.MIDO_CHURCH => true,
            Town.DARUNIA_ROOF => true,
            _ => false
        };
    }

    public static bool AppearsOnMap(this Town town)
    {
        return town switch
        {
            Town.RAURU => true,
            Town.RUTO => true,
            Town.SARIA_NORTH => true,
            Town.SARIA_SOUTH => true,
            Town.MIDO_WEST => true,
            Town.MIDO_CHURCH => false,
            Town.NABOORU => true,
            Town.DARUNIA_WEST => true,
            Town.DARUNIA_ROOF => false,
            Town.NEW_KASUTO => true,
            Town.OLD_KASUTO => true,
            Town.SARIA_TABLE => false,
            Town.BAGU => true,
            Town.NABOORU_FOUNTAIN => false,
            _ => throw new Exception("Unrecognized town: " + town.ToString())
        };
    }

    public static Town GetMasterTown(this Town town)
    {
        return town switch
        {
            Town.SARIA_TABLE => Town.SARIA_NORTH,
            Town.DARUNIA_ROOF => Town.DARUNIA_WEST,
            Town.NABOORU_FOUNTAIN => Town.NABOORU,
            Town.MIDO_CHURCH => Town.MIDO_WEST,
            _ => town
        };
    }

    public static bool IsUnderConsiderationForReachable(this Town town)
    {
        return town switch
        {
            Town.RAURU => true,
            Town.RUTO => true,
            Town.SARIA_NORTH => true,
            Town.SARIA_SOUTH => true,
            Town.MIDO_WEST => true,
            Town.MIDO_CHURCH => false,
            Town.NABOORU => true,
            Town.DARUNIA_WEST => true,
            Town.DARUNIA_ROOF => false,
            Town.NEW_KASUTO => true,
            Town.OLD_KASUTO => true,
            Town.SARIA_TABLE => false,
            Town.BAGU => true,
            Town.NABOORU_FOUNTAIN => false,
            _ => throw new Exception("Unrecognized town: " + town.ToString())
        };
    }
}
