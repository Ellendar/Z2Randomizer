using System.Reflection;
using Z2Randomizer.RandomizerCore;
using Z2Randomizer.RandomizerCore.Overworld;

namespace Z2Randomizer.Tests;

// Independent observations of the game's map format, not a second encoder or item solver.
internal static class OverworldSemanticAssertions
{
    internal static void Require(bool valid, string code, object? detail = null)
    {
        if (!valid) throw new InvalidOperationException($"{code}: {detail}");
    }

    internal static (byte[,] Tiles, int Length) Decode(ReadOnlySpan<byte> region, int rows,
        IntVector2? hiddenKasuto = null, IntVector2? threeEyeRock = null)
    {
        Require(rows > 0, "dimensions", rows);
        var tiles = new byte[rows, 64];
        int offset = 0;
        for (int y = 0; y < rows; y++)
        {
            int x = 0;
            while (x < 64)
            {
                Require(offset < region.Length, "map-capacity-or-truncation", $"row {y}, byte {offset}");
                byte token = region[offset++];
                int count = (token >> 4) + 1;
                Require(x + count <= 64, "map-row-overshoot", $"({x},{y})");
                if (hiddenKasuto is { } target && target.Y == y && target.X >= x && target.X < x + count)
                    Require(token == 0x06, "hammer-reveal-token", $"({target.X},{y}): 0x{token:X2}");
                if (threeEyeRock is { } rock && rock.Y == y && rock.X >= x && rock.X < x + count)
                    Require(count == 1, "flute-reveal-token", $"({rock.X},{y}): 0x{token:X2}");
                for (int end = x + count; x < end; x++) tiles[y, x] = (byte)(token & 15);
            }
        }
        return (tiles, offset);
    }

    internal static byte GameTerrain(Terrain terrain) => terrain switch
    {
        Terrain.PREPLACED_WATER => 12,
        Terrain.PREPLACED_WATER_WALKABLE => 13,
        _ => (byte)terrain
    };

    internal static IEnumerable<ContinentConnectorReservation> ContinentReservations(World world)
        => (IEnumerable<ContinentConnectorReservation>)typeof(World)
            .GetField("continentConnectorReservations", BindingFlags.Instance | BindingFlags.NonPublic)!
            .GetValue(world)!;

    internal static int MapMatches(World world, ReadOnlySpan<byte> region, IntVector2? hiddenKasuto = null,
        IntVector2? hiddenPalace = null, Terrain? threeEyeRockUnrevealedTerrain = null)
    {
        Require(world.MapRows > 0 && world.MapColumns is > 0 and <= 64 &&
            world.map.GetLength(0) >= world.MapRows && world.map.GetLength(1) >= world.MapColumns,
            "dimensions");
        var (tiles, length) = Decode(region, world.MapRows, hiddenKasuto, threeEyeRock: hiddenPalace);
        bool mountainPadding = world.map[0, world.MapColumns - 1] == Terrain.MOUNTAIN
            && !ContinentReservations(world).Any(reservation => reservation.Edge == Direction.EAST);
        byte padding = mountainPadding ? (byte)11 : (byte)12;
        for (int y = 0; y < world.MapRows; y++)
            for (int x = 0; x < 64; x++)
            {
                if (x < world.MapColumns)
                {
                    Terrain terrain = world.map[y, x];
                    Require(Enum.IsDefined(terrain) && terrain != Terrain.NONE, "unseeded-or-invalid-terrain", $"({x},{y})={terrain}");
                }
                var position = new IntVector2(x, y);
                byte expected = hiddenKasuto == position ? (byte)Terrain.FOREST
                    : hiddenPalace == position ? GameTerrain(threeEyeRockUnrevealedTerrain ?? world.map[position])
                    : x < world.MapColumns ? GameTerrain(world.map[y, x]) : padding;
                Require(tiles[y, x] == expected, "map-terrain-mismatch", $"({x},{y}): {tiles[y, x]} != {expected}");
            }
        return length;
    }

    internal static bool Inside(World world, IntVector2 pos) =>
        pos.X >= 0 && pos.X < world.MapColumns && pos.Y >= 0 && pos.Y < world.MapRows;

    internal static void Placements(World world, IEnumerable<Location> required)
    {
        foreach (var location in required)
            Require(location.YRaw != 0 && Inside(world, location.Pos), "required-disabled-or-outside", location.ID);
        foreach (var location in world.AllLocations.Where(l => l.YRaw != 0))
            Require(Inside(world, location.Pos), "active-location-outside", location.ID);
    }

    internal static void RequiredReachable(IEnumerable<Location> required)
    {
        foreach (var location in required) Require(location.Reachable, "required-unreachable", location.ID);
    }

    internal static void ContinentConnector(World world, Location? location, bool raft)
    {
        if (location is null) return;
        Require(location.YRaw != 0 && Inside(world, location.Pos), "connector-outside", location.ID);
        Require(world.map[location.Pos] == Terrain.BRIDGE, "connector-tile", location.ID);
        var neighbors = IntVector2.CARDINALS.Select(d => location.Pos + d).Where(p => Inside(world, p)).ToArray();
        Require(neighbors.Any(p => world.map[p].IsWalkable(true) && !world.map[p].IsWater() && world.map[p] != Terrain.BRIDGE),
            "connector-approach", location.ID);
        if (raft)
        {
            Require(neighbors.Any(p => world.map[p].IsWater()), "raft-water", location.ID);
            return;
        }
        var pending = new Queue<IntVector2>();
        var seen = new HashSet<IntVector2>();
        pending.Enqueue(location.Pos);
        while (pending.TryDequeue(out var pos))
        {
            if (!seen.Add(pos)) continue;
            if (pos.X == 0 || pos.Y == 0 || pos.X == world.MapColumns - 1 || pos.Y == world.MapRows - 1) return;
            foreach (var next in IntVector2.CARDINALS.Select(d => pos + d))
                if (Inside(world, next) && world.map[next] == Terrain.BRIDGE) pending.Enqueue(next);
        }
        Require(false, "bridge-shore", location.ID);
    }

    internal static void OutwardConnectorCave(World world, Location? cave)
    {
        if (cave is null) return;
        bool FacesEdge(IntVector2 outward)
        {
            var approach = cave.Pos - outward;
            if (!Inside(world, approach) || !world.map[approach].IsWalkable(true) || world.map[approach].IsWater()) return false;
            var side = outward.Perpendicular();
            if (new[] { cave.Pos - side, cave.Pos + side }.Any(p => !Inside(world, p) || world.map[p] != Terrain.MOUNTAIN))
                return false;
            var behind = cave.Pos + outward;
            if (!Inside(world, behind) || world.map[behind] != Terrain.MOUNTAIN) return false;
            int distance = 0;
            for (var p = behind; Inside(world, p); p += outward)
            {
                if (world.map[p] != Terrain.MOUNTAIN && !world.map[p].IsWater()) return false;
                distance++;
            }
            return distance <= 8;
        }

        Require(cave.IsEnabled() && Inside(world, cave.Pos) && world.map[cave.Pos] == Terrain.CAVE
            && IntVector2.CARDINALS.Any(FacesEdge), "connector-cave-outward", cave.ID);
    }

    internal static void HiddenColumns(Location? palace, Location? kasuto, bool bothEnabled)
    {
        if (bothEnabled) Require(palace != null && kasuto != null && palace.Xpos != kasuto.Xpos, "hidden-columns");
    }

}
