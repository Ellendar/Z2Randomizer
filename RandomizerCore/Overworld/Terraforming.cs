using System;
using System.Collections.Frozen;
using System.Collections.Generic;
using System.Diagnostics;

namespace Z2Randomizer.RandomizerCore.Overworld;

public class Terraforming
{
    /// used for GrowTerrain optimization
    private readonly struct PlacedTerrainPreCalc
    {
        public readonly double X;
        public readonly double Y;
        public readonly Terrain Terrain;
        public readonly double CoefSquared;
        public PlacedTerrainPreCalc(int x, int y, Terrain terrain, double coefSquared)
        {
            X = x;
            Y = y;
            Terrain = terrain;
            CoefSquared = coefSquared;
        }
    }

    /// Pre-build an array to loop over, since this used MAP_ROWS * MAP_COLS times
    private static PlacedTerrainPreCalc[] GrowTerrainGetPlacedTerrains(Terrain[,] map, int MAP_COLS, int MAP_ROWS, Climate climate, FrozenSet<Terrain> randomTerrains)
    {
        List<(int, int)> placedCoords = new();
        for (int y = 0; y < MAP_ROWS; y++)
        {
            for (int x = 0; x < MAP_COLS; x++)
            {
                Terrain t = map[y, x];
                if (t != Terrain.NONE && randomTerrains.Contains(t))
                {
                    placedCoords.Add((y, x));
                }
            }
        }
        Debug.Assert(placedCoords.Count > 0);
        PlacedTerrainPreCalc[] placedTerrains = new PlacedTerrainPreCalc[placedCoords.Count];
        for (int i = 0; i < placedCoords.Count; i++)
        {
            var (py, px) = placedCoords[i];
            Terrain t = map[py, px];
            double c = climate.DistanceCoefficients[(int)t];
            placedTerrains[i] = new PlacedTerrainPreCalc(px, py, t, c * c);
        }
        return placedTerrains;
    }

    public static bool GrowTerrain(Random r, ref Terrain[,] map, int MAP_COLS, int MAP_ROWS, Climate climate, IEnumerable<Terrain> randomTerrainFilter)
    {
        var randomTerrains = climate.RandomTerrains(randomTerrainFilter).ToFrozenSet();
        Terrain[,] mapCopy = new Terrain[MAP_ROWS, MAP_COLS];
        PlacedTerrainPreCalc[] placedTerrains = GrowTerrainGetPlacedTerrains(map, MAP_COLS, MAP_ROWS, climate, randomTerrains);
        const double EPSILON = 1e-9;
        double distance, minDistance;
        List<Terrain> choices = new();
        for (int y = 0; y < MAP_ROWS; y++)
        {
            for (int x = 0; x < MAP_COLS; x++)
            {
                var existingT = map[y, x];
                if (existingT != Terrain.NONE)
                {
                    mapCopy[y, x] = existingT;
                }
                else
                {
                    choices.Clear();
                    minDistance = double.MaxValue;

                    for (int i = 0; i < placedTerrains.Length; i++)
                    {
                        ref readonly var p = ref placedTerrains[i];
                        double dx = p.X - x;
                        double dy = p.Y - y;
                        Terrain t = p.Terrain;

                        // optimize further by skipping square root, because the
                        // minimum distance will also be the minimum distance squared
                        distance = p.CoefSquared * (dx * dx + dy * dy);
                        if (distance > minDistance) // most likely case first
                        {
                            continue;
                        }
                        else if (distance + EPSILON < minDistance)
                        {
                            choices.Clear();
                            choices.Add(t);
                            minDistance = distance;
                        }
                        else
                        {
                            choices.Add(t);
                        }
                    }
                    Debug.Assert(choices.Count > 0);
                    mapCopy[y, x] = choices[r.Next(choices.Count)];
                }
            }
        }
        map = mapCopy; // no need to clone as we created this array at the start of the method
        return true;
    }

    public static bool DrawRaft(Random r, Terrain[,] map, Location? raft, List<Terrain> walkableTerrains, Direction direction)
    {
        if (raft == null) { throw new Exception("Unable to draw unloaded raft"); }
        int raftx = 0;
        int rafty = 0;
        GetAwayFromEdgeDeltas(direction, out int deltax, out int deltay);

        int tries = 0;
        int length = 0;

        do
        {
            length = 0;
            tries++;
            if (tries > 100)
            {
                return false;
            }

            if (!FindWaterTileAtEdge(r, map, direction, out raftx, out rafty, 100))
            {
                return false;
            }

            length = MeasureWaterPath(map, raftx, rafty, deltax, deltay, out raftx, out rafty);
        }
        while (!IsValidEndTile(map, walkableTerrains, raftx, rafty, length, false));

        rafty -= deltay;
        raftx -= deltax;

        PlaceRaft(map, raft, raftx, rafty);

        return true;
    }

    public static bool DrawBridge(Random r, Terrain[,] map, Location? bridge, List<Terrain> walkableTerrains, Direction direction)
    {
        if (bridge == null) { throw new Exception("Unable to draw unloaded bridge"); }
        int raftx = 0;
        int rafty = 0;
        GetAwayFromEdgeDeltas(direction, out int deltax, out int deltay);

        int tries = 0;
        int length = 0;

        do
        {
            length = 0;
            tries++;
            if (tries > 100)
            {
                return false;
            }

            if (!FindWaterTileAtEdge(r, map, direction, out raftx, out rafty, 100))
            {
                return false;
            }

            length = MeasureWaterPath(map, raftx, rafty, deltax, deltay, out raftx, out rafty);
        }
        while (!IsValidEndTile(map, walkableTerrains, raftx, rafty, length, true));

        rafty -= deltay;
        raftx -= deltax;

        PlaceBridge(map, bridge, raftx, rafty, direction);

        return true;
    }

    public static bool FindWaterTileAtEdge(Random r, Terrain[,] map, Direction direction, out int x, out int y, int maxTries)
    {
        int mapRows = map.GetLength(0);
        int mapCols = map.GetLength(1);

        switch(direction)
        {
            case Direction.WEST:
            case Direction.EAST:
                x = direction == Direction.WEST ? 0 : mapCols - 1;
                for (int tries = 0; tries < maxTries; tries++)
                {
                    y = r.Next(0, mapRows);
                    if (map[y, x] is Terrain.WALKABLEWATER or Terrain.WATER) { return true; }
                }
                break;

            case Direction.NORTH:
            case Direction.SOUTH:
                y = direction == Direction.NORTH ? 0 : mapRows - 1;
                for (int tries = 0; tries < maxTries; tries++)
                {
                    x = r.Next(0, mapCols);
                    if (map[y, x] is Terrain.WALKABLEWATER or Terrain.WATER) { return true; }
                }
                break;

            default:
                throw new ArgumentOutOfRangeException(nameof(direction));
        }

        x = 0; y = 0;
        return false;
    }

    public static void GetAwayFromEdgeDeltas(Direction direction, out int dx, out int dy)
    {
        (dx, dy) = direction switch
        {
            Direction.WEST => (1, 0),
            Direction.EAST => (-1, 0),
            Direction.NORTH => (0, 1),
            Direction.SOUTH => (0, -1),
            _ => (0, 0)
        };
    }

    public static int MeasureWaterPath(Terrain[,] map, int startX, int startY, int dx, int dy, out int endX, out int endY)
    {
        int mapRows = map.GetLength(0);
        int mapCols = map.GetLength(1);
        int length = 0;
        int x = startX;
        int y = startY;

        while (y >= 0 && y < mapRows && x >= 0 && x < mapCols &&
               (map[y, x] == Terrain.WALKABLEWATER || map[y, x] == Terrain.WATER))
        {
            y += dy;
            x += dx;
            length++;
        }

        endX = x;
        endY = y;
        return length;
    }

    public static bool IsValidEndTile(Terrain[,] map, List<Terrain> walkableTerrains, int x, int y, int length, bool isBridge)
    {
        int mapRows = map.GetLength(0);
        int mapCols = map.GetLength(1);

        if (x < 0 || y < 0 || x >= mapCols || y >= mapRows) { return false; }
        if (!walkableTerrains.Contains(map[y, x])) { return false; }
        if (isBridge)
        {
            if (length > 10 || length <= 1) { return false; }
        }
        return true;
    }

    public static void PlaceBridge(Terrain[,] map, Location bridge, int x, int y, Direction direction)
    {
        int mapRows = map.GetLength(0);
        int mapCols = map.GetLength(1);

        map[y, x] = Terrain.BRIDGE;
        bridge.Xpos = x;
        bridge.Y = y;
        bridge.PassThrough = 0;
        bridge.CanShuffle = false;

        if (direction == Direction.EAST)
        {
            for (int i = x + 1; i < mapCols; i++)
            {
                map[y, i] = Terrain.BRIDGE;
            }
        }
        else if (direction == Direction.WEST)
        {
            for (int i = x - 1; i >= 0; i--)
            {
                map[y, i] = Terrain.BRIDGE;
            }
        }
        else if (direction == Direction.SOUTH)
        {
            for (int i = y + 1; i < mapRows; i++)
            {
                map[i, x] = Terrain.BRIDGE;
            }
        }
        else if (direction == Direction.NORTH)
        {
            for (int i = y - 1; i >= 0; i--)
            {
                map[i, x] = Terrain.BRIDGE;
            }
        }
    }

    public static void PlaceRaft(Terrain[,] map, Location raft, int x, int y)
    {
        map[y, x] = Terrain.BRIDGE;
        raft.Xpos = x;
        raft.Y = y;
        raft.CanShuffle = false;
    }
}
