using System;
using System.Collections.Generic;

namespace Z2Randomizer.RandomizerCore.Overworld;

public sealed partial class EastHyrule
{
    /// <summary>Paints a lava opening with a mountain rim and four inset corners, then the palace tile.</summary>
    /// <remarks>The caller supplies a center whose full 9x9 footprint fits in the map.</remarks>
    internal static void DrawValleyOfDeathOpening(OverworldMap map, IntVector2 center)
    {
        for (int y = -4; y <= 4; y++)
        {
            for (int x = -4; x <= 4; x++)
            {
                bool mountain = Math.Abs(x) == 4 || Math.Abs(y) == 4 || Math.Abs(x) == 3 && Math.Abs(y) == 3;
                map[center + new IntVector2(x, y)] = mountain ? Terrain.MOUNTAIN : Terrain.LAVA;
            }
        }
        map[center] = Terrain.PALACE;
    }

    /// <summary>
    /// Walks the routes from one palace opening. Cave selection, trap order and the
    /// second Volcano route share state for this build only. Failure leaves partial
    /// edits for the enclosing terrain candidate to handle.
    /// </summary>
    private sealed class ValleyOfDeathRouteBuilder
    {
        private readonly EastHyrule world;
        private readonly IntVector2 palacePosition;
        private readonly IReadOnlyList<Location> traps;
        private readonly bool usesMountainOpening;
        private readonly bool horizontalPath;
        private readonly int routeLength;
        private readonly int routeCount;
        private readonly IntVector2 forwardDirection;
        private readonly IntVector2 sideDirection;
        private readonly (Location Entrance, Location Exit) secondRouteCaves;

        private (Location Entrance, Location Exit) activeCaves;
        private IntVector2 currentPosition;
        private IntVector2 direction;
        private bool cavePlaced;
        private int placedTraps;
        // Keep duplicates: fallback trap sampling has historically used this ordered list.
        private readonly List<IntVector2> lavaPositions = [];

        internal ValleyOfDeathRouteBuilder(EastHyrule world, IntVector2 palacePosition, IReadOnlyList<Location> traps)
        {
            this.world = world;
            this.palacePosition = palacePosition;
            this.traps = traps;
            bool isCanyon = world.biome is Biome.CANYON or Biome.DRY_CANYON or Biome.COASTAL_ISLANDS;
            usesMountainOpening = isCanyon || world.biome == Biome.VOLCANO;
            horizontalPath = world.isHorizontal ^ isCanyon;
            routeLength = usesMountainOpening ? 20 : world.RNG.Next(5, 16);

            // At the vertical midpoint, canyon routes head north; other routes head south.
            direction = horizontalPath
                ? (palacePosition.X > world.MapColumns / 2 ? IntVector2.WEST : IntVector2.EAST)
                : ((isCanyon ? palacePosition.Y >= world.MapRows / 2 : palacePosition.Y > world.MapRows / 2)
                    ? IntVector2.NORTH : IntVector2.SOUTH);
            currentPosition = palacePosition + 4 * direction;

            world.canyonShort = world.RNG.NextDouble() > .5;
            var shortCaves = (world.GetLocation(LocationID.EAST_CAVE_VOD_PASSTHROUGH2_START),
                world.GetLocation(LocationID.EAST_CAVE_VOD_PASSTHROUGH2_END));
            var longCaves = (world.GetLocation(LocationID.EAST_CAVE_VOD_PASSTHROUGH1_END),
                world.GetLocation(LocationID.EAST_CAVE_VOD_PASSTHROUGH1_START));
            (activeCaves, secondRouteCaves) = world.canyonShort ? (shortCaves, longCaves) : (longCaves, shortCaves);

            routeCount = world.biome == Biome.VOLCANO ? world.RNG.Next(1, 3) : 1;
            forwardDirection = horizontalPath ? IntVector2.EAST : IntVector2.SOUTH;
            sideDirection = horizontalPath ? IntVector2.SOUTH : IntVector2.EAST;
        }

        internal bool TryBuild()
        {
            for (int route = 0; route < routeCount; route++)
            {
                if (!TryWalkRoute(route))
                    return false;
                if (!usesMountainOpening)
                    OpenRouteEntrance();

                // A second Volcano route starts on the other side of the palace.
                currentPosition = palacePosition - 4 * direction;
                direction = -direction;
                cavePlaced = false;
            }

            PlaceRemainingTraps();
            return true;
        }

        private bool TryWalkRoute(int route)
        {
            int trapsRemaining = routeCount == 2 ? (route == 0 ? 2 : 1) : 3;
            int minAdjust = -1;
            int maxAdjust = 2;
            int tilesPlaced = 0;

            while (world.WithinMapBounds(currentPosition, 1))
            {
                if (usesMountainOpening)
                {
                    if (world.map[currentPosition] != Terrain.MOUNTAIN)
                        break;
                }
                else
                {
                    if (tilesPlaced >= routeLength)
                        break;
                }

                tilesPlaced++;
                PaintLava(currentPosition);
                int adjust = RollAdjustment(minAdjust, maxAdjust);
                if (!TryDrawRouteSegment(adjust))
                    return false;
                currentPosition += adjust * sideDirection;

                bool shouldPlaceTrap = (cavePlaced && adjust == 0) || System.Math.Abs(adjust) > 1;
                if (shouldPlaceTrap && trapsRemaining > 0)
                {
                    if (TryPlaceRouteTrap(adjust))
                    {
                        trapsRemaining--;
                        // Steer away from a trap just placed on a lateral segment.
                        switch (adjust)
                        {
                            case > 0: (minAdjust, maxAdjust) = (0, 4); break;
                            case < 0: (minAdjust, maxAdjust) = (-3, 1); break;
                        }
                    }
                }
                else if (adjust == 0 && !cavePlaced)
                {
                    if (!TryPlaceCavePair(route))
                        return false;
                }
                else
                {
                    (minAdjust, maxAdjust) = (-3, 4);
                }

                AdvanceOrRetreat();
            }
            return true;
        }

        private void PaintLava(IntVector2 position)
        {
            world.map[position] = Terrain.LAVA;
            lavaPositions.Add(position);
        }

        private int RollAdjustment(int minimum, int maximum)
        {
            int adjust;
            do { adjust = world.RNG.Next(minimum, maximum); }
            // Keep the end of the sideways turn inside the map border.
            while (!world.WithinMapBounds(currentPosition + adjust * sideDirection, 1));
            return adjust;
        }

        private bool TryDrawRouteSegment(int adjust)
        {
            if (adjust != 0)
                return TryDrawPerpendicularSegment(adjust * sideDirection);

            if (world.map[currentPosition] != Terrain.CAVE)
            {
                if (!usesMountainOpening)
                {
                    world.map[currentPosition - sideDirection] = Terrain.MOUNTAIN;
                    world.map[currentPosition + sideDirection] = Terrain.MOUNTAIN;
                }
                if (world.GetLocationAt(currentPosition + direction) != null)
                    return false;
            }
            return true;
        }

        /// <summary>Paints a turn with mountain walls, rejecting occupied non-shuffleable tiles.</summary>
        private bool TryDrawPerpendicularSegment(IntVector2 movement)
        {
            IntVector2 step = movement.Normalize();
            int steps = movement.ManhattanLength;
            if (!usesMountainOpening) world.map[currentPosition - step] = Terrain.MOUNTAIN;

            for (int i = 0; i <= steps; i++)
            {
                IntVector2 position = currentPosition + i * step;
                if (!world.WithinMapBounds(position))
                    break;
                PaintLava(position);
                if (!usesMountainOpening)
                {
                    foreach (var wall in new[] { position - forwardDirection, position + forwardDirection })
                    {
                        var terrain = world.map[wall];
                        if (terrain != Terrain.LAVA && terrain != Terrain.CAVE)
                            world.map[wall] = Terrain.MOUNTAIN;
                    }
                }
                if (world.GetLocationAt(position) is Location location && !location.CanShuffle)
                    return false;
            }

            if (!usesMountainOpening)
            {
                var cap = currentPosition + (steps + 1) * step;
                if (world.WithinMapBounds(cap))
                    world.map[cap] = Terrain.MOUNTAIN;
            }
            return true;
        }

        private bool TryPlaceRouteTrap(int adjust)
        {
            IntVector2 trapDirection = horizontalPath ? -sideDirection : -forwardDirection;
            // Negative adjustments retain their distinct orientation rule.
            if (adjust < 0)
            {
                trapDirection = sideDirection;
            }
            int trapStep = adjust switch
            {
                0 => 0,
                > 3 => world.RNG.Next(adjust - 3) + 1,
                _ => 1
            };
            return TryPlaceNextTrap(currentPosition + trapStep * trapDirection);
        }

        private bool TryPlaceNextTrap(IntVector2 position)
        {
            var trap = traps[placedTraps];
            if (world.ValidTrapTilePosition(position) == null)
                return false;
            trap.Pos = position;
            trap.CanShuffle = false;
            placedTraps++;
            return true;
        }

        private bool TryPlaceCavePair(int route)
        {
            if (route != 0) activeCaves = secondRouteCaves;
            if (world.RNG.NextDouble() > .5 && routeCount != 2 && world.biome == Biome.VOLCANO)
            {
                direction = -direction;
            }

            int caveOffset;
            if (horizontalPath)
                caveOffset = currentPosition.Y > world.MapRows / 2 ? world.RNG.Next(-9, -4) : world.RNG.Next(5, 10);
            else
                caveOffset = currentPosition.X > world.MapColumns / 2 ? world.RNG.Next(-9, -4) : world.RNG.Next(5, 10);
            IntVector2 exitPosition = currentPosition + caveOffset * sideDirection;
            if (usesMountainOpening && world.map[exitPosition] != Terrain.MOUNTAIN)
                return false;

            MoveCavePair(currentPosition, exitPosition);
            currentPosition = exitPosition;
            cavePlaced = true;
            return true;
        }

        private void MoveCavePair(IntVector2 entrancePosition, IntVector2 exitPosition)
        {
            if (world.WithinMapBounds(activeCaves.Entrance.Pos))
                world.map[activeCaves.Entrance.Pos] = Terrain.MOUNTAIN;
            if (world.WithinMapBounds(activeCaves.Exit.Pos))
                world.map[activeCaves.Exit.Pos] = Terrain.MOUNTAIN;
            TerrainLayout.DrawCaveEntrance(world.map, entrancePosition, direction);
            activeCaves.Entrance.Pos = entrancePosition;
            activeCaves.Entrance.CanShuffle = false;
            TerrainLayout.DrawCaveEntrance(world.map, exitPosition, -direction);
            activeCaves.Exit.Pos = exitPosition;
            activeCaves.Exit.CanShuffle = false;
        }

        private void AdvanceOrRetreat()
        {
            if (world.GetLocationAt(currentPosition + direction) == null)
            {
                currentPosition += direction;
            }
            else
            {
                world.map[currentPosition] = Terrain.MOUNTAIN;
                currentPosition -= direction;
            }
        }

        private void OpenRouteEntrance()
        {
            IntVector2[] positions =
            [
                currentPosition,
                currentPosition + sideDirection,
                currentPosition - sideDirection,
                currentPosition + forwardDirection,
                currentPosition + forwardDirection + sideDirection,
                currentPosition + forwardDirection - sideDirection
            ];
            foreach (var position in positions)
            {
                if (CanWidenEntranceAt(position))
                {
                    world.map[position] = Terrain.LAVA;
                }
            }
        }

        private bool CanWidenEntranceAt(IntVector2 position)
        {
            if (!world.WithinMapBounds(position) || world.GetLocationAt(position) != null)
                return false;
            // Widening happens after trap placement; do not add side entrances.
            foreach (var location in world.LocationsOrthogonalTo(position))
            {
                var behind = position + 2 * (location.Pos - position);
                if (!world.WithinMapBounds(behind) || !world.map[behind].IsWalkable(true))
                    return false;
            }
            return true;
        }

        private void PlaceRemainingTraps()
        {
            while (placedTraps < traps.Count && lavaPositions.Count > 0)
            {
                int index = world.RNG.Next(lavaPositions.Count);
                var position = lavaPositions[index];
                lavaPositions.RemoveAt(index);
                TryPlaceNextTrap(position);
            }
        }
    }
}
