using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace Z2Randomizer.RandomizerCore.Overworld;

/*
    Globally unique identifiers for all points of interest.

    xx.. .... - Continent number
    ..xx xxxx - Index into continent's location table
*/
public enum LocationID
{
    WEST_NORTH_PALACE = 0,                       //  0: (23, 52)
    WEST_CAVE_TROPHY,                            //  1: (29, 32)
    WEST_MINOR_FOREST_AT_START,                  //  2: (37, 42)
    WEST_CAVE_MAGIC_CONTAINER,                   //  3: (16, 60)
    WEST_MINOR_FOREST_BY_SARIA,                  //  4: (20, 86)
    WEST_GRASS,                                  //  5: (62, 64)
    WEST_BAGU_WOODS1,                            //  6: (21, 77)
    WEST_TRAP_ROAD,                              //  7: (61, 57)
    WEST_MINOR_SWAMP1,                           //  8: ( 8, 71)
    WEST_MINOR_GRAVE1,                           //  9: (48, 92)
    WEST_CAVE_PARAPA_NORTH,                      // 10: (48, 41)
    WEST_CAVE_PARAPA_SOUTH,                      // 11: (55, 46)
    WEST_CAVE_JUMP_NORTH,                        // 12: ( 1, 58)
    WEST_CAVE_JUMP_SOUTH,                        // 13: ( 3, 62)
    WEST_CAVE_PBAG,                              // 14: (38, 62)
    WEST_CAVE_MEDICINE,                          // 15: ( 9, 69)
    WEST_CAVE_HEART_CONTAINER,                   // 16: (54, 62)
    WEST_FAIRY_CAVE_DROP,                        // 17: (50, 96)
    WEST_FAIRY_CAVE_EXIT,                        // 18: (59,102)
    WEST_BRIDGE_NORTH_OF_SARIA,                  // 19: (16, 82)
    WEST_BRIDGE_EAST_OF_SARIA,                   // 20: (26, 87)
    WEST_BRIDGE_AFTER_DM_WEST,                   // 21: (26, 97)
    WEST_BRIDGE_AFTER_DM_EAST,                   // 22: (34, 97)
    WEST_MINOR_FOREST_BY_JUMP_CAVE,              // 23: ( 7, 64)
    WEST_MINOR_SWAMP2,                           // 24: (17, 67)
    WEST_MINOR_FOREST_EAST_OF_SARIA,             // 25: (33, 87)
    WEST_BAGU_WOODS2,                            // 26: (20, 76)
    WEST_BAGU_WOODS3,                            // 27: (17, 77)
    WEST_BAGU_WOODS4,                            // 28: (19, 78)
    WEST_BAGU_WOODS5,                            // 29: (23, 77)
    WEST_MINOR_ROAD,                             // 30: (37, 68)
    WEST_MINOR_DESERT = 32,                      // 32: (38,102)
    WEST_RAFT_TO_EAST = 41,                      // 41: (61, 77)
    WEST_DM_ENTRANCE,                            // 42: (10, 95)
    WEST_DM_EXIT,                                // 43: (21, 96)
    WEST_KINGS_TOMB,                             // 44: (50, 88)
    WEST_TOWN_RAURO,                             // 45: (46, 54)
    WEST_TOWN_RUTO = 47,                         // 47: ( 2, 36)
    WEST_TOWN_SARIA_SOUTH,                       // 48: ( 8, 91)
    WEST_TOWN_SARIA_NORTH,                       // 49: ( 8, 89)
    WEST_BAGU_HOUSE,                             // 50: (21, 76)
    WEST_TOWN_MIDO,                              // 51: (60, 75)
    WEST_PALACE1,                                // 52: (62, 32)
    WEST_PALACE2,                                // 53: (11, 64)
    WEST_PALACE3,                                // 54: (57, 98)

    DM_CAVE1A = 0x40 + 0,                        //  0: ( 0, 42)
    DM_CAVE1B,                                   //  1: ( 4, 41)
    DM_CAVE2A,                                   //  2: ( 9, 40)
    DM_CAVE2B,                                   //  3: (11, 42)
    DM_CAVE3A,                                   //  4: (16, 43)
    DM_CAVE3B,                                   //  5: (14, 39)
    DM_CAVE4A,                                   //  6: ( 3, 45)
    DM_CAVE4B,                                   //  7: ( 7, 45)
    DM_CAVE5A,                                   //  8: ( 3, 48)
    DM_CAVE5B,                                   //  9: ( 5, 47)
    DM_CAVE6A,                                   // 10: (14, 47)
    DM_CAVE6B,                                   // 11: (16, 48)
    DM_CAVE7A,                                   // 12: ( 4, 51)
    DM_CAVE7B,                                   // 13: ( 5, 54)
    DM_CAVE8A,                                   // 14: (20, 48)
    DM_CAVE8B,                                   // 15: (20, 50)
    DM_CAVE9A,                                   // 16: (22, 52)
    DM_CAVE9B,                                   // 17: (20, 54)
    DM_CAVE10A,                                  // 18: ( 3, 59)
    DM_CAVE10B,                                  // 19: ( 7, 57)
    DM_CAVE11A,                                  // 20: (15, 58)
    DM_CAVE11B,                                  // 21: (18, 57)
    DM_CAVE12A,                                  // 22: (13, 60)
    DM_CAVE12B,                                  // 23: (17, 60)
    DM_CAVE13A,                                  // 24: (16, 65)
    DM_CAVE13B,                                  // 25: (18, 63)
    DM_CAVE14A,                                  // 26: (22, 63)
    DM_CAVE14B,                                  // 27: (24, 60)
    DM_HAMMER_CAVE,                              // 28: (10, 64)
    DM_CAVE4WAY1A,                               // 29: (11, 54)
    DM_CAVE4WAY1B,                               // 30: (14, 54)
    DM_CAVE4WAY1C,                               // 31: ( 9, 47)
    DM_CAVE4WAY1D,                               // 32: (11, 48)
    DM_CAVE4WAY2A,                               // 33: ( 8, 52)
    DM_CAVE4WAY2B,                               // 34: (10, 51)
    DM_CAVE4WAY2C,                               // 35: (18, 40)
    DM_CAVE4WAY2D,                               // 36: (18, 44)
    DM_CONTINENT_CONNECTOR1 = 0x40 + 42,         // 42: ( 7, 37)
    DM_CONTINENT_CONNECTOR2,                     // 43: (23, 37)
    DM_SPEC_ROCK = 0x40 + 56,                    // 56: ( 8, 64)

    EAST_MINOR_FOREST_BY_NABOORU = 0x80 + 0,     //  0: (10, 58)
    EAST_MINOR_FOREST_BY_P6,                     //  1: (54, 91)
    EAST_TRAP_ROAD1,                             //  2: (21, 76)
    EAST_TRAP_ROAD2,                             //  3: (17, 81)
    EAST_TRAP_ROAD3,                             //  4: (19, 84)
    EAST_TRAP_ROAD_TO_VOD,                       //  5: (24, 96)
    EAST_BRIDGE_TO_P6,                           //  6: (35, 93)
    EAST_BRIDGE_TO_KASUTO,                       //  7: (37,100)
    EAST_TRAP_DESERT1,                           //  8: ( 9, 36)
    EAST_TRAP_DESERT2,                           //  9: (10, 38)
    EAST_WATER,                                  // 10: (63, 56)
    EAST_CAVE_NABOORU_PASSTHROUGH_SOUTH,         // 11: (24, 52)
    EAST_CAVE_NABOORU_PASSTHROUGH_NORTH,         // 12: (27, 48)
    EAST_CAVE_PBAG1,                             // 13: (25, 71)
    EAST_CAVE_PBAG2,                             // 14: (31, 78)
    EAST_CAVE_NEW_KASUTO_PASSTHROUGH_WEST,       // 15: (49, 78)
    EAST_CAVE_NEW_KASUTO_PASSTHROUGH_EAST,       // 16: (57, 78)
    EAST_CAVE_VOD_PASSTHROUGH2_START,            // 17: ( 2, 75)
    EAST_CAVE_VOD_PASSTHROUGH2_END,              // 18: ( 4, 75)
    EAST_CAVE_VOD_PASSTHROUGH1_END,              // 19: ( 6, 77)
    EAST_CAVE_VOD_PASSTHROUGH1_START,            // 20: (10, 77)
    EAST_MINOR_SWAMP,                            // 21: (26, 81)
    EAST_BUGGED_MINOR_LAVA,                      // 22: ( 4, 91)
    EAST_MINOR_DESERT1,                          // 23: (53, 64)
    EAST_MINOR_DESERT2,                          // 24: (34, 56)
    EAST_MINOR_DESERT3,                          // 25: (48, 44)
    EAST_DESERT,                                 // 26: (57, 99)
    EAST_MINOR_FOREST2,                          // 27: (13, 68)
    EAST_MINOR_LAVA1,                            // 28: ( 4, 91)
    EAST_MINOR_LAVA2,                            // 29: (27, 99)
    EAST_TRAP_LAVA1,                             // 30: ( 3, 83)
    EAST_TRAP_LAVA2,                             // 31: ( 8, 86)
    EAST_TRAP_LAVA3,                             // 32: ( 8, 99)
    EAST_BRIDGE_TO_MI = 0x80 + 40,               // 40: (52, 40)
    EAST_RAFT_TO_WEST,                           // 41: ( 7, 52)
    EAST_TOWN_NABOORU = 0x80 + 45,               // 45: (23, 60)
    EAST_TOWN_DARUNIA = 0x80 + 47,               // 47: ( 3, 33)
    EAST_TOWN_NEW_KASUTO = 0x80 + 49,            // 49: (61,  0)
    EAST_TOWN_OLD_KASUTO = 0x80 + 51,            // 51: (34, 99)
    EAST_PALACE5,                                // 52: (62, 60)
    EAST_PALACE6,                                // 53: (45,  0)
    EAST_GREAT_PALACE,                           // 54: ( 4, 73)

    // DM and MI tables are actually identical but duplicated in banks 1 and 2
    MI_TRAP1 = 0xc0 + 37,                        // 37: (45, 62)
    MI_TRAP2,                                    // 38: (48, 68)
    MI_MAGIC_CONTAINER_DROP,                     // 39: (41, 58)
    MI_CONNECTOR_BRIDGE,                         // 40: (40, 67)
    MI_PALACE4 = 0xc0 + 52,                      // 52: (60, 58)
    MI_CHILD_DROP = 0xc0 + 55,                   // 55: (57, 60)
    MI_TRAP3 = 0xc0 + 57,                        // 57: (48, 58)
    MI_TRAP4,                                    // 58: (51, 49)
    MI_TRAP5,                                    // 59: (46, 50)
    MI_TRAP6,                                    // 60: (48, 46)
    MI_TRAP7,                                    // 61: (50, 42)
}

public static class LocationIDUtils
{
    public const int NumLocationIndices = 0x3f;
    public const int MaxLocationIndex = NumLocationIndices - 1;

    public static readonly string[] LocationNameArray = Enum.GetNames<LocationID>();
    public static readonly LocationID[] LocationIDs = Enum.GetValues<LocationID>();
    public static readonly Dictionary<LocationID, string> LocationNames = LocationIDs
        .Zip(LocationNameArray)
        .ToDictionary(kv => kv.First, kv => kv.Second);
    public static readonly int[] LocationIDInts = LocationIDs.Select(x => (int)x).ToArray();

    /// <summary>
    /// Get a LocationID from the ROM offset of the location's location table entry.
    /// </summary>
    /// <param name="offset">Offset of the location's byte 0 in ROM.</param>
    /// <returns></returns>
    /// <exception cref="ArgumentOutOfRangeException"></exception>
    public static LocationID FromMem(int offset)
    {
        for (int i = 0; i < RomMap.ContinentLocationBaseArray.Length; i++)
        {
            int baseOffs = RomMap.ContinentLocationBaseArray[i];
            if (baseOffs > offset)
                break;

            int idx = offset - baseOffs;
            if (idx <= MaxLocationIndex)
                return FromIndex((Continent)i, idx);
        }

        throw new ArgumentOutOfRangeException(nameof(offset));
    }

    /// <summary>
    /// Get the LocationID for the specified continent and location index for that continent.
    /// </summary>
    /// <param name="cont"></param>
    /// <param name="locIdx"></param>
    /// <returns></returns>
    public static LocationID FromIndex(Continent cont, int locIdx)
    {
        Debug.Assert(locIdx >= 0);
        Debug.Assert(locIdx <= MaxLocationIndex);

        return (LocationID)((int)cont * 0x40 + locIdx);
    }

    /// <summary>
    /// Enumerate all LocationIDs in the specified range regardless of whether they are named.
    /// </summary>
    /// <param name="cont"></param>
    /// <param name="startIdx"></param>
    /// <param name="endIdx"></param>
    /// <returns></returns>
    public static IEnumerable<LocationID> Enumerate(Continent cont, int startIdx = 0, int endIdx = NumLocationIndices)
    {
        Debug.Assert(startIdx >= 0);
        Debug.Assert(endIdx <= NumLocationIndices);

        return Enumerate(FromIndex(cont, startIdx), endIdx - startIdx);
    }

    /// <summary>
    /// Enumerate all LocationIDs in the specified range regardless of whether they are named.
    /// </summary>
    /// <param name="startLid"></param>
    /// <param name="lidCount"></param>
    /// <returns></returns>
    public static IEnumerable<LocationID> Enumerate(LocationID startLid, int lidCount)
    {
        Debug.Assert(startLid.GetIndex() >= 0);
        Debug.Assert(startLid.GetIndex() + lidCount <= NumLocationIndices);

        for (int i = 0; i < lidCount; i++)
            yield return startLid + i;
    }

    /// <summary>
    /// Enumerate all named LocationIDs in the specified range.
    /// </summary>
    /// <param name="cont"></param>
    /// <param name="startIdx"></param>
    /// <param name="endIdx"></param>
    /// <returns></returns>
    public static IEnumerable<LocationID> EnumerateNamed(Continent cont, int startIdx = 0, int endIdx = NumLocationIndices)
    {
        Debug.Assert(startIdx >= 0);
        Debug.Assert(endIdx <= NumLocationIndices);

        int startId = (int)FromIndex(cont, startIdx),
            endId = (int)FromIndex(cont, endIdx);
        for (int i = Math.Abs(LocationIDInts.BinarySearch(startId, Comparer<int>.Default));
            i < LocationIDs.Length && LocationIDInts[i] < endId;
            i++)
            yield return LocationIDs[i];
    }

    /// <summary>
    /// Enumerate all named LocationIDs in the specified range.
    /// </summary>
    /// <param name="startLid"></param>
    /// <param name="lidCount"></param>
    /// <returns></returns>
    public static IEnumerable<LocationID> EnumerateNamed(LocationID startLid, int lidCount)
    {
        Debug.Assert(startLid.GetIndex() >= 0);
        Debug.Assert(startLid.GetIndex() + lidCount <= NumLocationIndices);

        LocationID endLid = startLid + lidCount;
        for (int i = Math.Abs(LocationIDs.BinarySearch(startLid, Comparer<LocationID>.Default));
            i < LocationIDs.Length && LocationIDs[i] < endLid;
            i++)
            yield return LocationIDs[i];
    }
}

public static class LocationIDExtensions
{
    static readonly Dictionary<LocationID, Town> townMap = new()
    {
        [LocationID.WEST_TOWN_RAURO] = Town.RAURU,
        [LocationID.WEST_TOWN_RUTO] = Town.RUTO,
        [LocationID.WEST_TOWN_SARIA_NORTH] = Town.SARIA_NORTH,
        [LocationID.WEST_TOWN_SARIA_SOUTH] = Town.SARIA_SOUTH,
        [LocationID.WEST_BAGU_HOUSE] = Town.BAGU,
        [LocationID.WEST_TOWN_MIDO] = Town.MIDO_WEST,
        [LocationID.EAST_TOWN_NABOORU] = Town.NABOORU,
        [LocationID.EAST_TOWN_DARUNIA] = Town.DARUNIA_WEST,
        [LocationID.EAST_TOWN_OLD_KASUTO] = Town.OLD_KASUTO,
        [LocationID.EAST_TOWN_NEW_KASUTO] = Town.NEW_KASUTO,
    };

    public static bool IsNamed(this LocationID lid)
        => LocationIDUtils.LocationNames.ContainsKey(lid);

    /// <summary>
    /// Get the name of the location, or null if not named.
    /// </summary>
    /// <param name="lid"></param>
    /// <returns></returns>
    public static string? GetName(this LocationID lid)
    {
        LocationIDUtils.LocationNames.TryGetValue(lid, out var name);

        return name;
    }

    public static Continent GetContinent(this LocationID lid)
        => (Continent)((int)lid / 0x40);

    /// <summary>
    /// Get the index of the location within its continent's location table.
    /// </summary>
    /// <param name="lid"></param>
    /// <returns></returns>
    public static int GetIndex(this LocationID lid)
        => (int)lid % 0x40;

    /// <summary>
    /// Gets the continent and index within its location table for the location.
    /// </summary>
    /// <param name="lid"></param>
    /// <returns></returns>
    public static (Continent, int) GetIndices(this LocationID lid)
    {
        int contIdx = Math.DivRem((int)lid, 0x40, out int locIdx);
        return ((Continent)contIdx, locIdx);
    }

    public static int GetRomOffset(this LocationID lid, int byteIdx = 0)
    {
        var (cont, locIdx) = lid.GetIndices();
        return RomMap.ContinentLocationBases[cont] + locIdx + byteIdx * 0x3f;
    }

    /// <summary>
    /// Get the town a location refers to, or null if none.
    /// </summary>
    /// <param name="lid"></param>
    /// <returns></returns>
    public static Town? GetTown(this LocationID lid)
    {
        if (townMap.TryGetValue(lid, out Town town))
            return town;

        return null;
    }
}