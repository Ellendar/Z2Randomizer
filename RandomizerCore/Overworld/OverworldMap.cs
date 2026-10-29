using System;

namespace Z2Randomizer.RandomizerCore.Overworld;

public class OverworldMap
{
    private readonly Terrain[,] inner;

    public int LogicalRows => inner.GetLength(0);
    public int LogicalColumns => inner.GetLength(1);

    public OverworldMap(int mapRows, int mapColumns)
    {
        inner = new Terrain[mapRows, mapColumns];
    }

    /// <summary>Creates a map filled with the requested terrain, including NONE for unseeded generation.</summary>
    public OverworldMap(int mapRows, int mapColumns, Terrain initialTerrain) : this(mapRows, mapColumns)
    {
        for (int y = 0; y < mapRows; y++)
        {
            for (int x = 0; x < mapColumns; x++)
            {
                inner[y, x] = initialTerrain;
            }
        }
    }

    public OverworldMap(Terrain[,] map)
    {
        inner = map;
    }

    public Terrain this[int y, int x]
    {
        get => inner[y, x];

        set
        {
            // if (y == 27 && x == 20) Debugger.Break();
            inner[y, x] = value;
        }
    }

    public Terrain this[IntVector2 pos]
    {
        get => inner[pos.Y, pos.X];

        set
        {
            inner[pos.Y, pos.X] = value;
        }
    }

    internal OverworldMap Clone() => new((Terrain[,])inner.Clone());

    /// <summary>Checks whether a position belongs to the playable area.</summary>
    internal bool Contains(IntVector2 position) => position.IsWithinBounds(LogicalColumns, LogicalRows);

    /// <summary>
    /// Tests the center and its eight neighbors. Returns false if the full 3x3
    /// area does not fit in this map; it never clips the area at an edge.
    /// </summary>
    public bool AllTerrainIn3x3Equals(IntVector2 center, Terrain terrain)
        => AllTerrainInSquareEquals(center, terrain, radius: 1);

    /// <summary>Checks an odd-width square, returning false unless the whole area fits in the map.</summary>
    public bool AllTerrainInSquareEquals(IntVector2 center, Terrain terrain, int radius)
    {
        if (radius < 0) throw new ArgumentOutOfRangeException(nameof(radius));
        if (!center.IsWithinBounds(inner.GetLength(1), inner.GetLength(0), radius))
            return false;

        for (int y = center.Y - radius; y <= center.Y + radius; y++)
            for (int x = center.X - radius; x <= center.X + radius; x++)
                if (inner[y, x] != terrain) return false;
        return true;
    }

    public int GetLength(int dimension)
    {
        return inner.GetLength(dimension);
    }
}
