using System;
using System.Collections.Generic;

namespace Z2Randomizer.RandomizerCore.Overworld;

/// <summary>Hidden-location token boundaries and the east shore padding override.</summary>
internal readonly record struct MapEncodingOptions(
    IntVector2? ThreeEyeRockPosition = null,
    IntVector2? HiddenForestTilePosition = null,
    Terrain? ThreeEyeRockUnrevealedTerrain = null,
    bool UseWaterPadding = false);

/// <summary>Encodes overworld terrain without changing the map or writing to a ROM.</summary>
internal static class WorldMapCodec
{
    internal const int StoredColumns = 64;

    /// <summary>
    /// Returns run-length tokens, excluding reserved-space filler. Logical maps may
    /// be narrower than the game's stored width. Each row ends its own run, and
    /// right padding uses the top-right logical tile unless a shore needs water.
    /// </summary>
    /// <remarks>
    /// The reveal routines match and replace a whole run-length byte. Each hidden
    /// tile gets a one-tile run, even when its neighbors have the same terrain.
    /// Callers use the returned length to enforce their ROM budget.
    /// Coordinates are zero-based X/Y, not raw ROM Y coordinates with the +30 offset.
    /// </remarks>
    internal static byte[] Encode(OverworldMap map, MapEncodingOptions options = default)
    {
        var threeEyeRockPos = options.ThreeEyeRockPosition;
        if (threeEyeRockPos is { } rock && !map.Contains(rock))
        {
            throw new ArgumentOutOfRangeException(nameof(options), rock, "Three Eye Rock must be within the logical map.");
        }

        const Terrain hiddenForestTerrain = Terrain.FOREST;
        var hiddenForestPos = options.HiddenForestTilePosition;
        if (hiddenForestPos is { } forest && !map.Contains(forest))
        {
            throw new ArgumentOutOfRangeException(nameof(options), forest, "The hidden forest tile must be within the logical map.");
        }

        List<byte> bytes = [];
        Terrain currentTerrain = map[0, 0];
        int currentTerrainCount = 0;
        Terrain eastPaddingTerrain = options.UseWaterPadding || map[0, map.LogicalColumns - 1] != Terrain.MOUNTAIN
            ? Terrain.WATER
            : Terrain.MOUNTAIN;
        for (int y = 0; y < map.LogicalRows; y++)
        {
            for (int x = 0; x < StoredColumns; x++)
            {
                var pos = new IntVector2(x, y);

                bool isHiddenForestTile = hiddenForestPos == pos;
                if (isHiddenForestTile || threeEyeRockPos == pos)
                {
                    Terrain unrevealedTerrain = isHiddenForestTile
                        ? hiddenForestTerrain
                        : options.ThreeEyeRockUnrevealedTerrain ?? map[pos];
                    FlushRun();
                    bytes.Add(unrevealedTerrain.RomValue());
                    currentTerrain = unrevealedTerrain;
                    continue;
                }

                Terrain nextTerrain = x < map.LogicalColumns ? map[y, x] : eastPaddingTerrain;
                if (currentTerrainCount == 16 || currentTerrain != nextTerrain)
                {
                    FlushRun();
                    currentTerrain = nextTerrain;
                }
                currentTerrainCount++;
            }
            FlushRun();
            if (y < map.LogicalRows - 1)
            {
                currentTerrain = map[y + 1, 0];
            }
        }
        return bytes.ToArray();

        // Writes the pending run as one byte and resets its tile count.
        // The high nibble stores count minus one and the low nibble stores terrain.
        // The format has no zero-length run, so an empty run emits nothing.
        void FlushRun()
        {
            if (currentTerrainCount == 0) return;
            bytes.Add((byte)(((currentTerrainCount - 1) << 4) | currentTerrain.RomValue()));
            currentTerrainCount = 0;
        }
    }
}
