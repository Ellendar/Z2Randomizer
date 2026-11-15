using System;
using System.Collections.Generic;
using System.Linq;

namespace Z2Randomizer.RandomizerCore.Overworld;

/// <summary>
/// Regions used to choose required cave connections before shuffling vanilla West/East
/// locations. Coordinates are from the vanilla ROM (Y starts at 30), not generated maps.
/// </summary>
internal static class VanillaShuffleAreas
{
    private static readonly SortedDictionary<(int YRaw, int X), string> WestAreas = new()
    {
        { (0x34, 0x17), "north" },
        { (0x20, 0x1D), "north" },
        { (0x2A, 0x25), "north" },
        { (0x3C, 0x10), "north" },
        { (0x56, 0x14), "mid" },
        { (0x40, 0x3E), "parapa" },
        { (0x4D, 0x15), "mid" },
        { (0x39, 0x3D), "parapa" },
        { (0x47, 0x08), "mid" },
        { (0x5C, 0x30), "grave" },
        { (0x29, 0x30), "parapa" },
        { (0x2E, 0x37), "north" },
        { (0x3A, 0x01), "north" },
        { (0x3E, 0x03), "mid" },
        { (0x3E, 0x26), "mid" },
        { (0x45, 0x09), "hammer0" },
        { (0x3E, 0x36), "hammer" },
        { (0x60, 0x32), "grave" },
        { (0x66, 0x3B), "island" },
        { (0x52, 0x10), "mid" },
        { (0x57, 0x1A), "mid" },
        { (0x61, 0x1A), "dmexit" },
        { (0x61, 0x22), "grave" },
        { (0x40, 0x07), "mid" },
        { (0x43, 0x11), "mid" },
        { (0x57, 0x21), "mid" },
        { (0x4C, 0x14), "mid" },
        { (0x4D, 0x11), "mid" },
        { (0x4E, 0x13), "mid" },
        { (0x4D, 0x17), "mid" },
        { (0x44, 0x25), "mid" },
        { (0x66, 0x26), "grave" },
        { (0x4D, 0x3D), "grave" },
        { (0x5F, 0x0A), "lifesouth" },
        { (0x60, 0x15), "dmexit" },
        { (0x58, 0x32), "grave" },
        { (0x36, 0x2E), "north" },
        { (0x24, 0x02), "north" },
        { (0x5B, 0x08), "lifesouth" },
        { (0x59, 0x08), "mid" },
        { (0x4C, 0x15), "mid" },
        { (0x4B, 0x3C), "grave" },
        { (0x20, 0x3E), "parapa" },
        { (0x40, 0x0B), "mid" },
        { (0x62, 0x39), "island" },
    };

    private static readonly SortedDictionary<(int YRaw, int X), string> EastAreas = new()
    {
        { (0x3A, 0x0A), "mid2" },
        { (0x5B, 0x36), "south" },
        { (0x4C, 0x15), "south" },
        { (0x51, 0x11), "south" },
        { (0x54, 0x13), "south" },
        { (0x60, 0x18), "south" },
        { (0x5D, 0x23), "south" },
        { (0x64, 0x25), "south" },
        { (0x24, 0x09), "north2" },
        { (0x26, 0x0A), "north2" },
        { (0x38, 0x3F), "boots1" },
        { (0x34, 0x18), "mid2" },
        { (0x30, 0x1B), "north2" },
        { (0x47, 0x19), "mid2" },
        { (0x4E, 0x1F), "south" },
        { (0x4E, 0x31), "south" },
        { (0x4E, 0x39), "kasuto" },
        { (0x4B, 0x02), "vod" },
        { (0x4B, 0x04), "gp" },
        { (0x4D, 0x06), "vod" },
        { (0x4D, 0x0A), "south" },
        { (0x51, 0x1A), "south" },
        { (0x40, 0x35), "hammer2" },
        { (0x38, 0x22), "mid2" },
        { (0x2C, 0x30), "north2" },
        { (0x63, 0x39), "south" },
        { (0x44, 0x0D), "mid2" },
        { (0x5B, 0x04), "south" },
        { (0x63, 0x1B), "south" },
        { (0x53, 0x03), "vod" },
        { (0x56, 0x08), "south" },
        { (0x63, 0x08), "south" },
        { (0x28, 0x34), "north2" },
        { (0x34, 0x07), "mid2" },
        { (0x3C, 0x17), "mid2" },
        { (0x21, 0x03), "north2" },
        { (0x51, 0x3D), "kasuto" },
        { (0x63, 0x22), "south" },
        { (0x3C, 0x3E), "boots" },
        { (0x66, 0x2D), "south" },
        { (0x49, 0x04), "gp" },
    };

    internal static Dictionary<string, List<Location>> GroupLocations(Continent continent, IReadOnlyList<Location> locations)
    {
        string[] names = continent switch
        {
            Continent.WEST => ["north", "mid", "parapa", "grave", "lifesouth", "island", "hammer", "hammer0", "dmexit"],
            Continent.EAST => ["north2", "mid2", "south", "vod", "kasuto", "gp", "boots", "boots1", "hammer2"],
            _ => throw new ArgumentOutOfRangeException(nameof(continent), continent, "Only West and East use vanilla shuffle areas.")
        };
        var areas = names.ToDictionary(name => name, _ => new List<Location>());
        foreach (var location in locations)
        {
            // Preserve location order and the existing first-location choice where
            // multiple location records share a coordinate, including duplicate entries.
            var firstAtPosition = locations.First(other => other.Pos == location.Pos);
            areas[GetArea(continent, location.CoordsY30Offset)].Add(firstAtPosition);
        }
        return areas;
    }

    private static string GetArea(Continent continent, (int YRaw, int X) coordinates) => continent switch
    {
        Continent.WEST => WestAreas[coordinates],
        Continent.EAST => EastAreas[coordinates],
        _ => throw new ArgumentOutOfRangeException(nameof(continent), continent, "Only West and East use vanilla shuffle areas.")
    };
}
