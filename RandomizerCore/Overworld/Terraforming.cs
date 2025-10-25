using System;
using System.Collections.Generic;

namespace Z2Randomizer.RandomizerCore.Overworld;

public class Terraforming
{
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
