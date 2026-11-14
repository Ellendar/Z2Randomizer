namespace Z2Randomizer.RandomizerCore.Overworld;

/// <summary>Caldera center search, lake geometry and cave-approach painting. Continents own location selection.</summary>
internal static class CalderaTerrain
{
    /// <summary>Samples centers within the supplied bounds until a full 15x15 mountain area fits.</summary>
    /// <remarks>
    /// Maximum bounds are exclusive. Does not paint; failure returns the last
    /// sampled center.
    /// </remarks>
    internal static bool TryChooseCenter(OverworldMap map, IRandom rng, IntVector2 minimumInclusive, IntVector2 maximumExclusive, out IntVector2 center)
    {
        const int sampleLimit = 1000;
        center = IntVector2.ZERO;
        for (int sample = 0; sample < sampleLimit; sample++)
        {
            center = new(rng.Next(minimumInclusive.X, maximumExclusive.X),
                rng.Next(minimumInclusive.Y, maximumExclusive.Y));
            if (map.AllTerrainInSquareEquals(center, Terrain.MOUNTAIN, radius: 7)) return true;
        }
        return false;
    }

    /// <summary>Paints ten lake slices and forest banks inside the chosen mountain area.</summary>
    /// <remarks>
    /// Requires the chosen 15x15 mountain area. Orientation only selects axes;
    /// both orientations paint the same shape when those axes are exchanged.
    /// </remarks>
    internal static void DrawLake(OverworldMap map, IntVector2 center, bool horizontal, Terrain water, IRandom rng)
    {
        var along = horizontal ? IntVector2.EAST : IntVector2.SOUTH;
        var across = horizontal ? IntVector2.SOUTH : IntVector2.EAST;
        var slice = center - 5 * along;
        for (int i = 0; i < 10; i++, slice += along)
        {
            bool first = i == 0, last = i == 9;
            int width = rng.Next(7, 11);
            // End slices keep the existing narrower width and its second draw.
            if (first || last) width = rng.Next(3, 6);
            int positiveHalf = width / 2;
            int negativeHalf = width - positiveHalf;

            void PaintWater(IntVector2 position)
            {
                map[position] = water;
                if (first) map[position - along] = Terrain.FOREST;
                if (last) map[position + along] = Terrain.FOREST;
            }

            for (int offset = 0; offset < positiveHalf; offset++) PaintWater(slice + offset * across);
            // Only later slices have a previous lake slice to join. The next,
            // wider slice will finish the first slice's side banks.
            if (!first) PaintForestBank(map, slice + positiveHalf * across, along, across);
            for (int offset = 0; offset < negativeHalf; offset++) PaintWater(slice - offset * across);
            if (!first) PaintForestBank(map, slice - negativeHalf * across, along, -across);

            if (first || last)
            {
                // Complete the corners of the cap just beyond the lake's end.
                var cap = slice + (first ? -along : along);
                map[cap + positiveHalf * across] = Terrain.FOREST;
                map[cap - negativeHalf * across] = Terrain.FOREST;
            }
        }
    }

    /// <summary>Paints a three-tile approach just outside a cave, leaving its cave tile intact.</summary>
    /// <remarks>The caller supplies a cardinal outward vector and an entrance with room for the approach.</remarks>
    internal static void PaintCaveApproach(OverworldMap map, IntVector2 cave, IntVector2 outward, Terrain terrain)
    {
        var across = outward.X != 0 ? IntVector2.SOUTH : IntVector2.EAST;
        var approach = cave + outward;
        map[approach] = terrain;
        map[approach + across] = terrain;
        map[approach - across] = terrain;
    }

    /// <summary>Opens the previous slice's mountain bank, then fills the gap along the current slice.</summary>
    private static void PaintForestBank(OverworldMap map, IntVector2 boundary, IntVector2 along, IntVector2 outward)
    {
        var position = boundary;
        while (map[position - along] == Terrain.MOUNTAIN)
        {
            map[position - along] = Terrain.FOREST;
            position -= outward;
        }
        position = boundary;
        while (map[position - along] != Terrain.MOUNTAIN)
        {
            map[position] = Terrain.FOREST;
            position += outward;
        }
    }
}
