using System.Collections.Generic;

namespace Z2Randomizer.RandomizerCore.Overworld;

public static partial class LocationIDUtils
{
    /// <summary>Terrain assigned when loading a location, independent of its current generated terrain.</summary>
    public static Terrain GetDefaultTerrain(this LocationID id)
        => id.TryGetDefaultTerrain(out var terrain) ? terrain
            : throw new KeyNotFoundException($"No default terrain is defined for location {id}.");

    /// <summary>Returns false for unused/invalid IDs and explicitly unmapped connector records.</summary>
    public static bool TryGetDefaultTerrain(this LocationID id, out Terrain terrain)
    {
        Terrain? definedTerrain = id switch
        {
            LocationID.WEST_NORTH_PALACE
                or LocationID.WEST_PALACE1
                or LocationID.WEST_PALACE2
                or LocationID.WEST_PALACE3
                or LocationID.EAST_PALACE5
                or LocationID.EAST_PALACE6
                or LocationID.EAST_GREAT_PALACE
                or LocationID.MI_PALACE4 => Terrain.PALACE,
            LocationID.WEST_CAVE_TROPHY
                or LocationID.WEST_CAVE_MAGIC_CONTAINER
                or LocationID.WEST_CAVE_PARAPA_NORTH
                or LocationID.WEST_CAVE_PARAPA_SOUTH
                or LocationID.WEST_CAVE_JUMP_NORTH
                or LocationID.WEST_CAVE_JUMP_SOUTH
                or LocationID.WEST_CAVE_PBAG
                or LocationID.WEST_CAVE_MEDICINE
                or LocationID.WEST_CAVE_HEART_CONTAINER
                or LocationID.WEST_FAIRY_CAVE_EXIT
                or LocationID.DM_CAVE1A
                or LocationID.DM_CAVE1B
                or LocationID.DM_CAVE2A
                or LocationID.DM_CAVE2B
                or LocationID.DM_CAVE3A
                or LocationID.DM_CAVE3B
                or LocationID.DM_CAVE4A
                or LocationID.DM_CAVE4B
                or LocationID.DM_CAVE5A
                or LocationID.DM_CAVE5B
                or LocationID.DM_CAVE6A
                or LocationID.DM_CAVE6B
                or LocationID.DM_CAVE7A
                or LocationID.DM_CAVE7B
                or LocationID.DM_CAVE8A
                or LocationID.DM_CAVE8B
                or LocationID.DM_CAVE9A
                or LocationID.DM_CAVE9B
                or LocationID.DM_CAVE10A
                or LocationID.DM_CAVE10B
                or LocationID.DM_CAVE11A
                or LocationID.DM_CAVE11B
                or LocationID.DM_CAVE12A
                or LocationID.DM_CAVE12B
                or LocationID.DM_CAVE13A
                or LocationID.DM_CAVE13B
                or LocationID.DM_CAVE14A
                or LocationID.DM_CAVE14B
                or LocationID.DM_HAMMER_CAVE
                or LocationID.DM_CAVE4WAY1A
                or LocationID.DM_CAVE4WAY1B
                or LocationID.DM_CAVE4WAY1C
                or LocationID.DM_CAVE4WAY1D
                or LocationID.DM_CAVE4WAY2A
                or LocationID.DM_CAVE4WAY2B
                or LocationID.DM_CAVE4WAY2C
                or LocationID.DM_CAVE4WAY2D
                or LocationID.DM_CONTINENT_CONNECTOR1
                or LocationID.DM_CONTINENT_CONNECTOR2
                or LocationID.DM_SPEC_ROCK
                or LocationID.EAST_CAVE_NABOORU_PASSTHROUGH_SOUTH
                or LocationID.EAST_CAVE_NABOORU_PASSTHROUGH_NORTH
                or LocationID.EAST_CAVE_PBAG1
                or LocationID.EAST_CAVE_PBAG2
                or LocationID.EAST_CAVE_NEW_KASUTO_PASSTHROUGH_WEST
                or LocationID.EAST_CAVE_NEW_KASUTO_PASSTHROUGH_EAST
                or LocationID.EAST_CAVE_VOD_PASSTHROUGH2_START
                or LocationID.EAST_CAVE_VOD_PASSTHROUGH2_END
                or LocationID.EAST_CAVE_VOD_PASSTHROUGH1_END
                or LocationID.EAST_CAVE_VOD_PASSTHROUGH1_START => Terrain.CAVE,
            LocationID.WEST_MINOR_FOREST_AT_START
                or LocationID.WEST_MINOR_FOREST_BY_SARIA
                or LocationID.WEST_BAGU_WOODS1
                or LocationID.WEST_MINOR_FOREST_BY_JUMP_CAVE
                or LocationID.WEST_MINOR_FOREST_EAST_OF_SARIA
                or LocationID.WEST_BAGU_WOODS2
                or LocationID.WEST_BAGU_WOODS3
                or LocationID.WEST_BAGU_WOODS4
                or LocationID.WEST_BAGU_WOODS5
                or LocationID.WEST_BAGU_HOUSE
                or LocationID.EAST_MINOR_FOREST_BY_NABOORU
                or LocationID.EAST_MINOR_FOREST_BY_P6
                or LocationID.EAST_MINOR_FOREST2 => Terrain.FOREST,
            LocationID.WEST_GRASS => Terrain.GRASS,
            LocationID.WEST_TRAP_ROAD
                or LocationID.WEST_MINOR_ROAD
                or LocationID.EAST_TRAP_ROAD1
                or LocationID.EAST_TRAP_ROAD2
                or LocationID.EAST_TRAP_ROAD3
                or LocationID.EAST_TRAP_ROAD_TO_VOD
                or LocationID.MI_TRAP1
                or LocationID.MI_TRAP2
                or LocationID.MI_MAGIC_CONTAINER_DROP
                or LocationID.MI_CHILD_DROP
                or LocationID.MI_TRAP3
                or LocationID.MI_TRAP4
                or LocationID.MI_TRAP5
                or LocationID.MI_TRAP6
                or LocationID.MI_TRAP7 => Terrain.ROAD,
            LocationID.WEST_MINOR_SWAMP1
                or LocationID.WEST_MINOR_SWAMP2
                or LocationID.EAST_MINOR_SWAMP => Terrain.SWAMP,
            LocationID.WEST_MINOR_GRAVE1
                or LocationID.WEST_FAIRY_CAVE_DROP
                or LocationID.WEST_KINGS_TOMB => Terrain.GRAVE,
            LocationID.WEST_BRIDGE_NORTH_OF_SARIA
                or LocationID.WEST_BRIDGE_EAST_OF_SARIA
                or LocationID.WEST_BRIDGE_AFTER_DM_WEST
                or LocationID.WEST_BRIDGE_AFTER_DM_EAST
                or LocationID.EAST_BRIDGE_TO_P6
                or LocationID.EAST_BRIDGE_TO_KASUTO
                or LocationID.EAST_BRIDGE_TO_MI
                or LocationID.EAST_RAFT_TO_WEST
                or LocationID.MI_CONNECTOR_BRIDGE => Terrain.BRIDGE,
            LocationID.WEST_MINOR_DESERT
                or LocationID.EAST_TRAP_DESERT1
                or LocationID.EAST_TRAP_DESERT2
                or LocationID.EAST_MINOR_DESERT1
                or LocationID.EAST_MINOR_DESERT2
                or LocationID.EAST_MINOR_DESERT3
                or LocationID.EAST_DESERT => Terrain.DESERT,
            LocationID.WEST_TOWN_RAURU
                or LocationID.WEST_TOWN_RUTO
                or LocationID.WEST_TOWN_SARIA_SOUTH
                or LocationID.WEST_TOWN_SARIA_NORTH
                or LocationID.WEST_TOWN_MIDO
                or LocationID.EAST_TOWN_NABOORU
                or LocationID.EAST_TOWN_DARUNIA
                or LocationID.EAST_TOWN_NEW_KASUTO
                or LocationID.EAST_TOWN_OLD_KASUTO => Terrain.TOWN,
            LocationID.EAST_WATER => Terrain.WALKABLEWATER,
            LocationID.EAST_BUGGED_MINOR_LAVA
                or LocationID.EAST_MINOR_LAVA1
                or LocationID.EAST_MINOR_LAVA2
                or LocationID.EAST_TRAP_LAVA1
                or LocationID.EAST_TRAP_LAVA2
                or LocationID.EAST_TRAP_LAVA3 => Terrain.LAVA,
            _ => null
        };
        terrain = definedTerrain ?? Terrain.NONE;
        return definedTerrain.HasValue;
    }
}
