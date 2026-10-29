namespace Z2Randomizer.RandomizerCore.Overworld;

public abstract partial class World
{
    /// <summary>
    /// Searches outward from a caldera lake through a mountain bank, sampling a
    /// parallel line within two tiles of the center. On success, places both caves.
    /// </summary>
    /// <remarks>
    /// Left/right mean west/east for a horizontal passage and north/south for a
    /// vertical one. Searching west or north encounters the right cave first.
    /// Callers own pair selection, shuffle flags and approach terrain. A failed
    /// search leaves the map and locations unchanged.
    /// Existing cave coordinates are ignored. The search rejects short mountain
    /// runs, map-margin violations and entrances too close to existing caves;
    /// it advances the seeded RNG even when no passage can be placed.
    /// </remarks>
    protected bool TryPlaceCalderaCavePair(IntVector2 center, Direction outward,
        (Location Left, Location Right) caves)
    {
        var direction = outward.ToIntVector2();
        var across = outward.IsHorizontal() ? IntVector2.SOUTH : IntVector2.EAST;
        var origin = center + RNG.Next(-2, 3) * across;
        bool towardLeft = outward is Direction.WEST or Direction.NORTH;
        var inner = towardLeft ? caves.Right : caves.Left;
        var outer = towardLeft ? caves.Left : caves.Right;
        return FindPassthroughCave(origin, direction, inner, outer);
    }

    protected bool FindPassthroughCave(IntVector2 pos, IntVector2 dir, Location cave1, Location cave2)
    {
        var sideDir = dir.Perpendicular();

        while (true)
        {
            if (!WithinMapBounds(pos, 1))
            {
                logger.LogDebug("Start cave reached end of map");
                return false;
            }
            if (map[pos.Y, pos.X] == Terrain.MOUNTAIN)
            {
                break;
            }
            pos += dir;
        }

        IntVector2? newPos = MoveAwayIfNextToCave(pos, sideDir);
        if (newPos == null) { return false; }
        pos = newPos.Value;
        // Settled on start cave position
        var cave1pos = pos;

        int length = 0;
        while (true)
        {
            pos += dir;
            length++;
            if (!WithinMapBounds(pos, 1))
            {
                logger.LogDebug("Ending cave reached end of map");
                return false;
            }
            if (map[pos.Y, pos.X] != Terrain.MOUNTAIN)
            {
                break;
            }
            if (length > 2)
            {
                // check sideways for exits as well
                foreach (int i in new[] { -1, 1, -2, 2 })
                {
                    var sidePos = pos + i * sideDir;
                    if (!WithinMapBounds(sidePos, 1)) { continue; }
                    if (map[sidePos.Y, sidePos.X] != Terrain.MOUNTAIN &&
                        map[sidePos.Y, sidePos.X] != Terrain.CAVE)
                    {
                        pos = sidePos;
                        break;
                    }
                }
            }
        }

        if (length <= 2)
        {
            logger.LogDebug("Cave passthrough path not long enough");
            return false;
        }

        if (HasAdjacentCave(pos))
        {
            logger.LogDebug("Cave exit is too close to existing cave");
            return false;
        }

        pos -= dir;
        // settled on ending cave position
        var cave2pos = pos;

        map[cave1pos.Y, cave1pos.X] = Terrain.CAVE;
        cave1.Pos = cave1pos;
        TerrainLayout.DrawCaveEntrance(map, cave2pos, -dir);
        cave2.Pos = cave2pos;

        return true;
    }

    private IntVector2? MoveAwayIfNextToCave(IntVector2 pos, IntVector2 sideDir)
    {
        int i = 0;
        IntVector2 tryPos = pos;
        while (true)
        {
            if (!HasAdjacentCave(tryPos))
            {
                return tryPos;
            }
            int offset = RNG.Next(-1, 2);
            tryPos = pos + offset * sideDir;
            if (++i == 6)
            {
                logger.LogDebug("Start cave too close to another cave");
                return null;
            }
        }
    }

    /// <summary>
    /// Return true iff there's a cave on the map in the 3x3 tiles centered on `pos`.
    /// Note: It's the callers responsibility that these tiles are all in bounds.
    /// </summary>
    protected bool HasAdjacentCave(IntVector2 pos)
    {
        for (int y = pos.Y - 1; y <= pos.Y + 1; y++)
        {
            for (int x = pos.X - 1; x <= pos.X + 1; x++)
            {
                if (map[y, x] == Terrain.CAVE)
                {
                    return true;
                }
            }
        }
        return false;
    }

    /// <summary>
    /// Horizontal caldera passage adapter: zero searches west, any other value east.
    /// See <see cref="TryPlaceCalderaCavePair"/> for placement and failure behavior.
    /// </summary>
    protected bool HorizontalCave(int caveDirection, int centerX, int centerY, Location caveLeft, Location caveRight)
        => TryPlaceCalderaCavePair(new(centerX, centerY), caveDirection == 0 ? Direction.WEST : Direction.EAST,
            (caveLeft, caveRight));

    /// <summary>
    /// Vertical caldera passage adapter: zero searches north, any other value south.
    /// Left/right denote north/south on this axis.
    /// </summary>
    protected bool VerticalCave(int caveDirection, int centerX, int centerY, Location caveLeft, Location caveRight)
        => TryPlaceCalderaCavePair(new(centerX, centerY), caveDirection == 0 ? Direction.NORTH : Direction.SOUTH,
            (caveLeft, caveRight));
}
