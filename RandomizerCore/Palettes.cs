using System;
using System.Collections.Generic;
using System.Linq;
using Z2Randomizer.RandomizerCore.Sidescroll.Palace;

namespace Z2Randomizer.RandomizerCore;

public static class Palettes
{
    public static int ORANGE_PALETTE_ADDR = ROM.RomHdrSize + 0x100a2;

    public static readonly Dictionary<Terrain, int> TERRAIN_PALETTE_ADDRS = new()
    {
        { Terrain.TOWN, 0x1c463 },
        { Terrain.CAVE, 0x1c45f },
        { Terrain.PALACE, 0x1c463 },
        { Terrain.BRIDGE, 0x1c45f },
        { Terrain.DESERT, 0x1c467 },
        { Terrain.GRASS, 0x1c45b },
        { Terrain.FOREST, 0x1c45b },
        { Terrain.SWAMP, 0x1c45b },
        { Terrain.GRAVE, 0x1c45f },
        { Terrain.ROAD, 0x1c45f },
        { Terrain.LAVA, 0x1c45f },
        { Terrain.MOUNTAIN, 0x1c45f },
        { Terrain.WATER, 0x100aa },
        { Terrain.PREPLACED_WATER, 0x100aa },
        { Terrain.WALKABLEWATER, 0x1c467 },
        { Terrain.PREPLACED_WATER_WALKABLE, 0x1c467 },
        { Terrain.ROCK, 0x1c45f },
        { Terrain.RIVER_DEVIL, 0x1c45f },
    };

    //None of these methods should have a reference to the ROM and write their own output.
    //All of these should output their results, and then those results should be written to the output state.
    public static void ShufflePalacePalettes(ROM ROMData, Random r)
    {
        List<int[]> brickList = new List<int[]>();
        List<int[]> curtainList = new List<int[]>();
        List<int> bRows = new List<int>();
        List<int> binRows = new List<int>();
        for (int i = 0; i < 7; i++)
        {
            int brickRow = r.Next(PalaceColors.bricks.GetLength(0));
            int curtainRow = r.Next(PalaceColors.curtains.GetLength(0));

            int[] bricks = new int[3];
            int[] curtains = new int[3];
            for (int j = 0; j < 3; j++)
            {
                bricks[j] = PalaceColors.bricks[brickRow, j];
                curtains[j] = PalaceColors.curtains[curtainRow, j];
            }

            brickList.Add(bricks);
            curtainList.Add(curtains);

            bRows.Add(r.Next(7));
            binRows.Add(r.Next(7));
        }

        ROMData.WritePalacePalettes(brickList, curtainList, bRows, binRows);
    }

    /// all addresses whose colors are randomized together for one palette table
    public sealed record PaletteTableAddresses(
        List<int> LifeBgColorAddr,
        List<int> OrangeSpriteColorAddr,
        List<int> RedSpriteColorAddr,
        List<int> BlueSpriteColorAddr,
        List<int> StabGuyColorAddr);

    /// the colors of one palette table, as written by <see cref="SetPaletteTableColors"/>
    /// and read by <see cref="ReadPaletteTableColors"/>.
    /// Orange.light is usually null to use the white default
    /// The life bar background tiles always match Orange.dark. 
    public sealed record PaletteTableColors(
        (byte dark, byte middle, byte? light) Orange,
        (byte dark, byte middle, byte? light) Red,
        (byte dark, byte middle, byte? light) Blue,
        (byte dark, byte middle, byte? light)? StabGuy);

    private static PaletteTableAddresses GetPaletteTableAddresses(int paletteTableAddr)
    {
        // we are not rolling the white color for magic/interface that should match the orange sprite light
        // (white looks fine with all 2 other sprite colors anyway)
        // int[] magicBgColorAddr = [paletteTableAddr + 0x01, paletteTableAddr + 0x11];
        // we ARE rolling the red sprite and matching the red tile color for the life bars to it
        // (it would limit the palette a lot if the red color has to stay red)
        PaletteTableAddresses addresses = new(
            [.. Enumerable.Range(0, 9).Select(i => paletteTableAddr + 0x10 * i + 0x03)],
            [paletteTableAddr + 0x94],
            [paletteTableAddr + 0x98],
            [paletteTableAddr + 0x9c],
            []);

        switch (paletteTableAddr)
        {
            case RomMap.PALACE_PALETTE_TABLE_MAJOR:
                addresses.OrangeSpriteColorAddr.AddRange(Enumerable.Range(0, 3).Select(i => paletteTableAddr + 0xa4 + 0x10 * i));
                // additional per-palace palettes
                addresses.LifeBgColorAddr.AddRange(Enumerable.Range(0, 6).Select(i => RomMap.PALACE_PALETTE_TABLE_ENTRANCES + 0x10 * i + 0x03));
                addresses.LifeBgColorAddr.AddRange(Enumerable.Range(0, 6).Select(i => RomMap.PALACE_PALETTE_TABLE_PER_PALACE + 0x10 * i + 0x03));
                break;
            case RomMap.GP_PALETTE_TABLE_MAJOR:
                addresses.LifeBgColorAddr.Add(0x1c48f + 0x03); // palette PPU cmd when fading to Dark Link
                addresses.LifeBgColorAddr.Add(0x1c4a3 + 0x03); // palette PPU cmd when Dark Link has been defeated
                addresses.OrangeSpriteColorAddr.Add(paletteTableAddr + 0xa4);
                addresses.OrangeSpriteColorAddr.Add(paletteTableAddr + 0xc4);
                addresses.RedSpriteColorAddr.Add(paletteTableAddr + 0xa8);
                addresses.BlueSpriteColorAddr.Add(paletteTableAddr + 0xac);
                break;
            case RomMap.TOWN_PALETTE_TABLE:
                addresses.StabGuyColorAddr.Add(paletteTableAddr + 0xac);
                addresses.OrangeSpriteColorAddr.AddRange(Enumerable.Range(0, 4).Select(i => paletteTableAddr + 0xa4 + 0x10 * i));
                break;
            case RomMap.WEST_PALETTE_TABLE or RomMap.EAST_PALETTE_TABLE:
                addresses.OrangeSpriteColorAddr.AddRange(Enumerable.Range(0, 4).Select(i => paletteTableAddr + 0xa4 + 0x10 * i));
                addresses.RedSpriteColorAddr.Add(paletteTableAddr + 0xa8);
                addresses.BlueSpriteColorAddr.Add(paletteTableAddr + 0xac);
                break;
        }

        return addresses;
    }

    /// the sideview palette tables whose sprite colors are randomized together
    private static readonly int[] SideviewPaletteTables =
    [
        RomMap.WEST_PALETTE_TABLE,
        RomMap.EAST_PALETTE_TABLE,
        RomMap.TOWN_PALETTE_TABLE,
        RomMap.PALACE_PALETTE_TABLE_MAJOR,
        RomMap.GP_PALETTE_TABLE_MAJOR,
    ];

    /// writes a color triple to every palette entry based at the given addresses
    /// (dark at +1, middle at +2, light at +3). a null color leaves the
    /// existing value untouched. +0 is not written to since it's always transparent.
    private static void WriteColorTriple(ROM ROMData, List<int> baseAddrs, (byte? dark, byte? middle, byte? light) colors)
    {
        foreach (var i in baseAddrs)
        {
            if (colors.dark is { } dark)     { ROMData.Put(i + 1, dark); }
            if (colors.middle is { } middle) { ROMData.Put(i + 2, middle); }
            if (colors.light is { } light)   { ROMData.Put(i + 3, light); }
        }
    }

    /// unrandomized helper that writes colors to one palette table. every group
    /// passed as null is left untouched. each entry stores dark at +1 and middle
    /// at +2; the light color at +3 is written for everything except the orange
    /// sprite palette, whose third slot is the fixed white magic/interface color.
    /// when orange is set, the life bar background tiles are matched to its dark color.
    public static void SetPaletteTableColors(
        ROM ROMData,
        PaletteTableAddresses addresses,
        (byte dark, byte middle, byte? light)? orange = null,
        (byte dark, byte middle, byte? light)? red = null,
        (byte dark, byte middle, byte? light)? blue = null,
        (byte dark, byte middle, byte? light)? stabGuy = null)
    {
        if (orange is { } orangeVal)
        {
            WriteColorTriple(ROMData, addresses.OrangeSpriteColorAddr, orangeVal with { light = null });
            foreach (var j in addresses.LifeBgColorAddr)
            {
                ROMData.Put(j, orangeVal.dark);
            }
        }
        if (red is { } redVal)
        {
            WriteColorTriple(ROMData, addresses.RedSpriteColorAddr, redVal);
        }
        if (blue is { } blueVal)
        {
            WriteColorTriple(ROMData, addresses.BlueSpriteColorAddr, blueVal);
        }
        if (stabGuy is { } stabGuyColors)
        {
            WriteColorTriple(ROMData, addresses.StabGuyColorAddr, stabGuyColors);
        }
    }

    /// reads back the colors written by <see cref="SetPaletteTableColors"/>.
    /// all addresses within a group hold identical values, so only the first
    /// entry of each group is read. the orange triple's light value is always
    /// null since no light color is stored for the orange sprite palette.
    public static PaletteTableColors ReadPaletteTableColors(ROM ROMData, int paletteTableAddr)
    {
        PaletteTableAddresses addresses = GetPaletteTableAddresses(paletteTableAddr);

        (byte dark, byte middle, byte? light) ReadGroup(List<int> baseAddrs, bool hasLight)
        {
            int addr = baseAddrs[0];
            return (
                ROMData.GetByte(addr + 1),
                ROMData.GetByte(addr + 2),
                hasLight ? (byte?)ROMData.GetByte(addr + 3) : null);
        }

        return new PaletteTableColors(
            Orange: ReadGroup(addresses.OrangeSpriteColorAddr, false),
            Red: ReadGroup(addresses.RedSpriteColorAddr, true),
            Blue: ReadGroup(addresses.BlueSpriteColorAddr, true),
            StabGuy: addresses.StabGuyColorAddr.Count > 0 ? ReadGroup(addresses.StabGuyColorAddr, true) : null);
    }

    /// forces colors across all sideview palette tables without rolling.
    /// every group passed as null is left untouched, so e.g.
    /// SetSideviewPaletteTables(ROMData, orange: (0x0a, 0x2a, 0x30))
    /// sets only the orange sprite colors (and the matching life bar tiles).
    public static void SetSideviewPaletteTables(
        ROM ROMData,
        (byte dark, byte middle, byte? light)? orange = null,
        (byte dark, byte middle, byte? light)? red = null,
        (byte dark, byte middle, byte? light)? blue = null,
        (byte dark, byte middle, byte? light)? stabGuy = null)
    {
        foreach (var paletteTableAddr in SideviewPaletteTables)
        {
            SetPaletteTableColors(ROMData, GetPaletteTableAddresses(paletteTableAddr), orange, red, blue, stabGuy);
        }
    }

    public static void RerollPaletteTable(ROM ROMData, int paletteTableAddr, Random r)
    {
        byte dark, middle, light;

        List<int> darkRangeFull = [.. Enumerable.Range(0x01, 12), .. Enumerable.Range(0x11, 13), 0x2d];
        // we make the life color range slightly narrower, to not make the HUD look too awful
        List<int> darkRangeLife = [.. Enumerable.Range(0x04, 3), .. Enumerable.Range(0x13, 5), .. Enumerable.Range(0x19, 4)];
        // brighter dark colors do not look good in towns
        List<int> darkRangeTown = [.. Enumerable.Range(0x01, 12), 0x1d];

        PaletteTableAddresses addresses = GetPaletteTableAddresses(paletteTableAddr);

        (byte dark, byte middle, byte? light)? stabGuyColors = null;
        if (paletteTableAddr == RomMap.TOWN_PALETTE_TABLE)
        {
            stabGuyColors = NES.RollMatchingColorTriple(r, darkRangeFull);
        }

        List<List<int>> tripples = [addresses.OrangeSpriteColorAddr, addresses.RedSpriteColorAddr, addresses.BlueSpriteColorAddr];
        List<int> usedColors = [];
        if (paletteTableAddr == RomMap.TOWN_PALETTE_TABLE)
        {
            usedColors.Add(0x22); // blue sky
        }

        (byte dark, byte middle, byte? light) orangeColors = default, redColors = default, blueColors = default;
        foreach (List<int> list in tripples)
        {
            bool isOrange = list == addresses.OrangeSpriteColorAddr;

            List<int> darkRange = (paletteTableAddr, isOrange) switch
            {
                (RomMap.TOWN_PALETTE_TABLE, true) => darkRangeTown.Intersect(darkRangeLife).ToList(),
                (RomMap.TOWN_PALETTE_TABLE, false) => darkRangeTown,
                (RomMap.GP_PALETTE_TABLE_MAJOR, true) => darkRangeLife.Where(x => x != 0x14 && x != 0x15).ToList(),
                (_, true) => darkRangeLife,
                (_, false) => darkRangeFull,
            };

            do
            {
                (dark, middle, light) = NES.RollMatchingColorTriple(r, darkRange);
            } while ((dark != 0x2d && usedColors.Contains(dark)) ||
                     usedColors.Contains(middle) ||
                     (light != 0x30 && usedColors.Contains(light)));

            // prevent adjacent colors from being picked again
            usedColors.AddRange(Enumerable.Range(-1, 3).Select(i => dark + i));
            usedColors.AddRange(Enumerable.Range(-1, 3).Select(i => middle + i));
            usedColors.AddRange(Enumerable.Range(-1, 3).Select(i => light + i));

            if (isOrange) { orangeColors = (dark, middle, null); }
            else if (list == addresses.RedSpriteColorAddr) { redColors = (dark, middle, light); }
            else { blueColors = (dark, middle, light); }
        }

        SetPaletteTableColors(ROMData, addresses, orangeColors, redColors, blueColors, stabGuyColors);
    }

    public static void RerollSideviewPaletteTables(ROM ROMData, Random customizationRng)
    {
        foreach (var paletteTableAddr in SideviewPaletteTables)
        {
            RerollPaletteTable(ROMData, paletteTableAddr, customizationRng);
        }
    }
}
