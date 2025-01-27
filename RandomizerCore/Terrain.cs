namespace RandomizerCore;

//These indexes correspond to the 4bit representation used in the rom. Do not change them.
public enum Terrain
{
    TOWN = 0,
    CAVE = 1,
    PALACE = 2,
    BRIDGE = 3,
    DESERT = 4,
    GRASS = 5,
    FOREST = 6,
    SWAMP = 7,
    GRAVE = 8,
    ROAD = 9,
    LAVA = 10,
    MOUNTAIN = 11,
    WATER = 12,
    WALKABLEWATER = 13,
    ROCK = 14,
    RIVER_DEVIL = 15,
    NONE = 16
}

static class TerrainExtensions
{
    public static bool IsWalkable(this Terrain terrain)
    {
        return terrain switch
        {
            Terrain.TOWN => true,
            Terrain.CAVE => true,
            Terrain.PALACE => true,
            Terrain.BRIDGE => true,
            Terrain.DESERT => true,
            Terrain.GRASS => true,
            Terrain.FOREST => true,
            Terrain.SWAMP => true,
            Terrain.GRAVE => true,
            Terrain.ROAD => true,
            Terrain.LAVA => true,
            Terrain.MOUNTAIN => false,
            Terrain.WATER => false,
            Terrain.WALKABLEWATER => true,
            Terrain.ROCK => true,
            Terrain.RIVER_DEVIL => true,
            Terrain.NONE => false,
            _ => throw new ImpossibleException("Unrecognized Terrain")
        };
    }
}
