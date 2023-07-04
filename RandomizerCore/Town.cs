namespace Z2Randomizer.Core;

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
    SPELL_TOWER = 11,
    OLD_KASUTO = 12
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
}
