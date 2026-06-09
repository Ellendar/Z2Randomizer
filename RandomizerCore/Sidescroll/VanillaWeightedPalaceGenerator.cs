using System;
using System.Collections.Generic;
using System.Linq;

namespace Z2Randomizer.RandomizerCore.Sidescroll;

public class VanillaWeightedPalaceGenerator : RandomWalkCoordinatePalaceGenerator
{
    private static readonly TableWeightedRandom<int>[] WeightedRandomDirections = [
        // weights adjusted to make output somewhat statistically similar to vanilla values
        // horizontal weights multiplied by 150%.
        new([ // Palace 1  (7x3 in vanilla)
            (0, 21),  // left
            (1, 6),  // down
            (2, 6),  // up
            (3, 21),  // right
        ]),
        new([ // Palace 2  (8x4 in vanilla)
            (0, 3),  // left
            (1, 1),  // down
            (2, 1),  // up
            (3, 3),  // right
        ]),
        new([ // Palace 3  (7x3 in vanilla)
            (0, 21),  // left
            (1, 6),  // down
            (2, 6),  // up
            (3, 21),  // right
        ]),
        new([ // Palace 4  (6x4 in vanilla)
            (0, 9),  // left
            (1, 4),  // down
            (2, 4),  // up
            (3, 9),  // right
        ]),
        new([ // Palace 5  (7x5 in vanilla)
            (0, 21),  // left
            (1, 10),  // down
            (2, 10),  // up
            (3, 21),  // right
        ]),
        new([ // Palace 6  (10x6 in vanilla)
            (0, 15),  // left
            (1, 6),  // down
            (2, 6),  // up
            (3, 15),  // right
        ]),
        new([ // Great Palace  (10x13 in vanilla)
            (0, 10),  // left
            (1, 13),  // down
            (2, 13),  // up
            (3, 10),  // right
        ]),
    ];

    // how many rooms you have to traverse to reach the boss in the vanilla palaces.
    // does not include the entrance, but does include the boss room itself
    private static readonly int[] VanillaDistanceToBosses = [
        7,  // Palace 1
        13, // Palace 2
        10, // Palace 3
        7,  // Palace 4
        12, // Palace 5
        13, // Palace 6
        24, // GP
    ];

    protected override IWeightedSampler<int> GetDirectionWeights(int palaceNumber)
    {
        return WeightedRandomDirections[palaceNumber - 1];
    }

    protected override int GetBossMinDistance(RandomizerProperties props, int palaceNumber)
    {
        if (palaceNumber == 7)
        {
            // still just use the selected value for GP
            return props.DarkLinkMinDistance;
        }
        // lets not alter the chance of items being behind the boss
        bool continuesAfter = props.BossRoomsExitToPalace[palaceNumber - 1];
        if (continuesAfter)
        {
            return 0;
        }

        double vanillaLength = Palaces.VANILLA_LENGTHS[palaceNumber - 1];
        double length = props.PalaceLengths[palaceNumber - 1];
        double lengthScale = length / vanillaLength;

        // compress the min distance a bit for longer palaces
        double a = 0.4; // compression aggressiveness
        lengthScale = lengthScale / (1.0 + a * (lengthScale - 0.5));

        double vanillaDistance = VanillaDistanceToBosses[palaceNumber - 1];
        return (int)Math.Round((double)(vanillaDistance * lengthScale));
    }

    public static Dictionary<Coord, int> MeasureDeadendPaths(Palace palace, Dictionary<Coord, RoomExitType> palaceShape)
    {
        List<Coord> importantCoords = [palace.Entrance!.coords, palace.BossRoom!.coords, ..palace.ItemRooms.Select(room => room.coords)];
        SortedSet<Coord> allCoords = [.. palaceShape.Keys.Order()];

        /// a map of coord -> distance  for each importantCoord
        List<Dictionary<Coord, int>> distancesFromImportantCoord = [];
        for (int a = 0; a < importantCoords.Count; a++)
        {
            Coord importantCoord = importantCoords[a];
            Dictionary<Coord, int> distances = [];
            Queue<(Coord coord, int dist)> queue = [];
            queue.Enqueue((importantCoord, 0));
            while (queue.Count > 0)
            {
                var (coord, dist) = queue.Dequeue();
                if (distances.ContainsKey(coord)) { continue; }
                distances[coord] = dist;
                var shape = palaceShape[coord];
                foreach (var neighbor in GetNeighborsAnyDirection(palaceShape, shape, coord))
                {
                    if (!distances.ContainsKey(neighbor))
                    {
                        queue.Enqueue((neighbor, dist + 1));
                    }
                }
            }
            distancesFromImportantCoord.Add(distances);
        }

        // find coords on the shortest paths between all important locations
        SortedSet<Coord> optimalPathCoords = [];
        for (int a = 0; a < importantCoords.Count; a++)
        {
            for (int b = a + 1; b < importantCoords.Count; b++)
            {
                var importantCoordA = importantCoords[a];
                var importantCoordB = importantCoords[b];

                var distFromImportantA = distancesFromImportantCoord[a];
                var distFromImportantB = distancesFromImportantCoord[b];

                if (!distFromImportantA.ContainsKey(importantCoordB))
                {
                    continue; // no path
                }
                int shortest = distFromImportantA[importantCoordB];

                foreach (var coord in allCoords)
                {
                    if (distFromImportantA.TryGetValue(coord, out int distanceA) &&
                        distFromImportantB.TryGetValue(coord, out int distanceB) &&
                        distanceA + distanceB == shortest)
                    {
                        optimalPathCoords.Add(coord);
                    }
                }
            }
        }

        // find all the distances from the optimal paths
        // basically repeat the first step, except pre-fill all the
        // optimal path coords with a cost of 0.
        Dictionary<Coord, int> distancesToOptimalPath = [];
        Queue<(Coord coord, int dist)> combinedQueue = new();
        foreach (var coord in optimalPathCoords)
        {
            combinedQueue.Enqueue((coord, 0));
        }
        while (combinedQueue.Count > 0)
        {
            var (coord, dist) = combinedQueue.Dequeue();
            if (distancesToOptimalPath.ContainsKey(coord)) { continue; }
            distancesToOptimalPath[coord] = dist;
            var shape = palaceShape[coord];
            foreach (var neighbor in GetNeighborsAnyDirection(palaceShape, shape, coord))
            {
                if (!distancesToOptimalPath.ContainsKey(neighbor))
                {
                    combinedQueue.Enqueue((neighbor, dist + 1));
                }
            }
        }
        return distancesToOptimalPath;
    }

    /// iterator over all neighboring coords from `coord` according to `exitType`
    public static IEnumerable<Coord> GetNeighborsOutgoing(RoomExitType exitType, Coord coord)
    {
        if (exitType.ContainsLeft())  { yield return coord with { X = coord.X - 1 }; }
        if (exitType.ContainsRight()) { yield return coord with { X = coord.X + 1 }; }
        if (exitType.ContainsUp())    { yield return coord with { Y = coord.Y + 1 }; }
        if (exitType.ContainsDown())  { yield return coord with { Y = coord.Y - 1 }; }
    }

    /// iterator over all neighbors, with drop rooms being included both ways
    public static IEnumerable<Coord> GetNeighborsAnyDirection(Dictionary<Coord, RoomExitType> shape, RoomExitType exitType, Coord coord)
    {
        if (exitType.ContainsLeft())  { yield return coord with { X = coord.X - 1 }; }
        if (exitType.ContainsRight()) { yield return coord with { X = coord.X + 1 }; }

        var upCoord = coord with { Y = coord.Y + 1 };
        if (exitType.ContainsUp())
        {
            yield return upCoord;
        }
        else if (shape.TryGetValue(upCoord, out var upShape) && upShape.ContainsDown())
        {
            yield return upCoord;
        }
        if (exitType.ContainsDown()) { yield return coord with { Y = coord.Y - 1 }; }
    }

    /// Vanilla Z2 only has deadend item rooms. Lets try it.
    protected override IEnumerable<RoomExitType> GetItemRoomShapes(RoomPool roomPool)
    {
        var shapesInPool = roomPool.GetItemRoomShapes();
        return RoomExitTypeExtensions.DEADENDS.Intersect(shapesInPool);
    }

    protected override bool ValidateShape(Palace palace, Dictionary<Coord, RoomExitType> palaceShape)
    {
        // GP is too different from the other palaces. ignore for now
        if (palace.Number == 7) { return true; }
        int palaceSize = palaceShape.Count;

        var shapeCounts = palaceShape.GroupBy(kvp => kvp.Value).ToDictionary(v => v.Key, v => v.Count());
        var verticalCount = shapeCounts.GetValueOrDefault(RoomExitType.VERTICAL_PASSTHROUGH, 0);
        var fourwayCount = shapeCounts.GetValueOrDefault(RoomExitType.FOUR_WAY, 0);
        // disallow too many vertical rooms
        if (verticalCount * 8 + fourwayCount * 6 > palaceSize)
        {
            return false;
        }

        var distancesToOptimalPath = MeasureDeadendPaths(palace, palaceShape);
        var coords = distancesToOptimalPath.Keys;
        double totalCost = 0.0;
        double itemCount = palace.ItemRooms.Count;
        foreach (var k in coords)
        {
            var v = distancesToOptimalPath[k];
            // the number of items influences how strict we are about deadend paths
            // (otherwise a 0 item palace would be an unfairly long linear path to the boss)
            totalCost += Math.Pow(v, (itemCount * 0.5) + 1);
        }
        // get a ratio of deadend paths/palace size
        double ratio = totalCost / palaceSize;
        return ratio <= 6.0;
    }
}
