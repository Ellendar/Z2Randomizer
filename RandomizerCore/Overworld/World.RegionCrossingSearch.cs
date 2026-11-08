using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Z2Randomizer.RandomizerCore.Overworld;

public abstract partial class World
{
    /// <summary>Finds and measures possible crossings for one placement call.</summary>
    /// <remarks>
    /// At the start, the search lists starting tiles and directions into the terrain
    /// to cross, and assigns an ID to each connected area of walkable terrain (in `regionLabels`).
    /// Each measurement reads the current terrain and location positions, including
    /// changes from earlier crossings in this call. Where the crossing rules require
    /// it, the search uses the original area IDs to avoid connecting an area to
    /// itself or reusing a pair already accepted for placement. The caller paints
    /// the crossing and moves any locations.
    /// </remarks>
    private sealed class RegionCrossingSearch
    {
        private const int MINIMUM_BRIDGE_LENGTH = 2;

        private static readonly Dictionary<Biome, int> MAXIMUM_BRIDGE_LENGTH = new()
        {
            { Biome.ISLANDS, 10 },
            { Biome.COASTAL_ISLANDS, 15 },
            { Biome.VOLCANO, 10 },
            { Biome.DRY_CANYON, 10 },
            { Biome.VANILLALIKE, 10 },
            { Biome.CALDERA, 10 },
            { Biome.CANYON, 10 },
            { Biome.MOUNTAINOUS, 15 }
        };

        private static readonly Dictionary<Biome, int> MAXIMUM_BRIDGE_ATTEMPTS = new()
        {
            { Biome.ISLANDS, 4000 },
            { Biome.COASTAL_ISLANDS, 4000 },
            { Biome.VOLCANO, 3000 },
            { Biome.DRY_CANYON, 2000 },
            { Biome.VANILLALIKE, 2000 },
            { Biome.CALDERA, 3000 },
            { Biome.CANYON, 2000 },
            { Biome.MOUNTAINOUS, 4000 }
        };

        private readonly World world;
        private readonly RegionCrossingOptions options;
        private readonly int maximumLength;
        private readonly int[,] regionLabels;
        private readonly Terrain[] crossingTerrains;
        private readonly Terrain[] edgeCrossingTerrains;
        private readonly List<(int, int)> connectedRegions = [];
        private readonly List<(IntVector2 Pos, Direction)> shoreCandidates = [];

        internal int MaximumAttempts { get; }
        internal int RemainingCandidates => shoreCandidates.Count;
        internal Terrain[] CrossingTerrains => crossingTerrains;

        internal RegionCrossingSearch(World world, RegionCrossingOptions options)
        {
            this.world = world;
            this.options = options;
            var biome = options.Biome;
            var riverTerrain = options.CrossingTerrain;
            bool canWalkOnWater = options.CanWalkOnWaterWithBoots;
            maximumLength = MAXIMUM_BRIDGE_LENGTH[biome];
            // Preserve the extra length allowance for Great Lakes or Boots that cannot cross water.
            if (!canWalkOnWater || world.climate.Name == Climates.Create(world.continentId, ClimateEnum.GREAT_LAKES).Name)
            {
                maximumLength = (int)(maximumLength * 1.5);
            }
            MaximumAttempts = MAXIMUM_BRIDGE_ATTEMPTS[biome];
            MaximumAttempts *= canWalkOnWater ? 1 : 2;
            regionLabels = LabelLandRegions();

            crossingTerrains = riverTerrain.IsWater()
                ? [Terrain.WATER, Terrain.PREPLACED_WATER, Terrain.WALKABLEWATER, Terrain.PREPLACED_WATER_WALKABLE]
                : [riverTerrain];
            edgeCrossingTerrains = [.. crossingTerrains, .. world.walkableTerrains];

            // Enumerate shores once in row/direction order; discard each sampled shore after its attempt.
            for (int y = 1; y < world.MapRows - 1; y++)
            {
                for (int x = 1; x < world.MapColumns - 1; x++)
                {
                    List<Direction> waterDirections = world.NextToWaterDirections(x, y, crossingTerrains);
                    if (AllowsStartingShore(x, y))
                    {
                        foreach (Direction direction in waterDirections)
                        {
                            shoreCandidates.Add((new IntVector2(x, y), direction));
                        }
                    }
                }
            }
        }

        // Preserve Sample's default-tuple fallback for an empty pool. The caller
        // still runs its bounded attempts and reports the pending-Saria result.
        internal (IntVector2 Pos, Direction) Sample() => shoreCandidates.Sample(world.RNG);

        internal void Discard((IntVector2 Pos, Direction) candidate) => shoreCandidates.Remove(candidate);

        internal void RecordConnection(RegionCrossingSpan span)
        {
            connectedRegions.Add((span.StartRegion, span.EndRegion));
            connectedRegions.Add((span.EndRegion, span.StartRegion));
        }

        internal bool TryMeasure((IntVector2 Pos, Direction) candidate, bool placeSaria, out RegionCrossingSpan span)
        {
            var (pos, crossingDirection) = candidate;
            var biome = options.Biome;
            var riverTerrain = options.CrossingTerrain;
            IntVector2 start = pos;
            IntVector2 forward = crossingDirection.ToIntVector2();
            IntVector2 side = forward.Perpendicular();

            int length = 0;
            if (world.TouchesContinentConnectorReservation(pos)) length = 100;
            if (IsBlockedSingleTile(pos))
            {
                length = 100;
            }
            if (world.LocationsAtOrOrthogonalTo(pos).Any())
            {
                length = 100;
            }
            int startRegion = regionLabels[pos.Y, pos.X];
            pos += forward;

            //iterate expanding the bridge
            while (world.WithinMapBounds(pos, 1) && crossingTerrains.Contains(world.map[pos]))
            {
                //if we are too close to a location, give up
                if (world.TouchesContinentConnectorReservation(pos) || world.LocationsAtOrOrthogonalTo(pos).Any())
                {
                    length = 100;
                    break;
                }
                //if advancing goes off the edge of the map, give up
                if (!world.WithinMapBounds(pos + forward))
                {
                    length = 100;
                    break;
                }

                //count how many tiles adjacent to the bridge are the passing terrain
                int adjacentRiverTerrainDirectionCount = 0;
                //If the tile just before or just after is walkable, this is the first or last tile of the bridge
                //When that happens, the adjacent (not forward/back) directions are allowed to be any walkable terrain
                bool isFirstStep = !crossingTerrains.Contains(world.map[pos - forward]);
                Terrain forwardTerrain = world.map[pos + forward];
                bool isLastStep = !crossingTerrains.Contains(forwardTerrain);
                Terrain[] effectiveCrossingTerrain = isFirstStep || isLastStep ? edgeCrossingTerrains : crossingTerrains;

                //Check forward
                if (effectiveCrossingTerrain.Contains(forwardTerrain) || forwardTerrain == Terrain.MOUNTAIN)
                {
                    adjacentRiverTerrainDirectionCount++;
                }
                //Check perpendicular tiles. This is allowed to be any walkable terrain on the first/last tiles of the bridge
                if (world.WithinMapBounds(pos + side))
                {
                    var rightTerrain = world.map[pos + side];
                    if (effectiveCrossingTerrain.Contains(rightTerrain) || rightTerrain == Terrain.MOUNTAIN)
                    {
                        adjacentRiverTerrainDirectionCount++;
                    }
                }
                if (world.WithinMapBounds(pos - side))
                {
                    var leftTerrain = world.map[pos - side];
                    if (effectiveCrossingTerrain.Contains(leftTerrain) || leftTerrain == Terrain.MOUNTAIN)
                    {
                        adjacentRiverTerrainDirectionCount++;
                    }
                }

                //if all 3 tiles adjacent to the new tile aren't the passing terrain, give up
                if (adjacentRiverTerrainDirectionCount < 3)
                {
                    length = 100;
                }
                pos += forward;
                length++;
            }

            //no extending from single tile
            if (world.TouchesContinentConnectorReservation(pos)) length = 100;
            if (IsBlockedSingleTile(pos))
            {
                length = 100;
            }

            int endRegion = 0;
            if (world.WithinMapBounds(pos, 1))
            {
                if (world.LocationsAtOrOrthogonalTo(pos).Any())
                {
                    length = 100;
                }
                endRegion = regionLabels[pos.Y, pos.X];
            }

            //if we're ending, it has to be on a different chunk of terrain so the bridge doesn't just cross a bay
            if (riverTerrain != Terrain.DESERT && biome != Biome.CALDERA && biome != Biome.VOLCANO
                && (startRegion == 0
                    || endRegion == 0
                    || endRegion == startRegion
                    || connectedRegions.Contains((startRegion, endRegion))
                )
            )
            {
                length = 100;
            }

            span = new(start, pos, forward, length, startRegion, endRegion);
            return (placeSaria && length < maximumLength || (length < maximumLength && length > MINIMUM_BRIDGE_LENGTH))
                && world.WithinMapBounds(pos, 1)
                && world.walkableTerrains.Contains(world.map[pos])
                && world.map[pos] != riverTerrain;
        }

        private bool AllowsStartingShore(int x, int y)
            => options.DeadZone is not { } zone
                || (x < zone.MinimumX || x > zone.MaximumX) && (y < zone.MinimumY || y > zone.MaximumY);

        /// <summary>
        /// Labels the connected sections of walkable terrain at the start of this request.
        /// </summary>
        /// <returns>A grid with the walkable area's ID at each tile, or zero outside those areas.</returns>
        private int[,] LabelLandRegions()
        {
            int[,] mass = new int[world.MapRows, world.MapColumns];
            int massNo = 1;
            for (int i = 0; i < world.MapRows; i++)
            {
                for (int j = 0; j < world.MapColumns; j++)
                {
                    if (mass[i, j] == 0 && world.walkableTerrains.Contains(world.map[i, j]))
                    {
                        LabelConnectedLand(i, j, massNo, mass);
                        massNo++;
                    }
                }
            }
            return mass;
        }

        /// <summary>
        /// Labels connected land in the existing recursive visit order.
        /// </summary>
        /// <param name="y"></param>
        /// <param name="x"></param>
        /// <param name="massNo"></param>
        /// <param name="mass"></param>
        private void LabelConnectedLand(int y, int x, int massNo, int[,] mass)
        {
            mass[y, x] = massNo;
            if (y - 1 > 0 && mass[y - 1, x] == 0 && world.walkableTerrains.Contains(world.map[y - 1, x]))
            {
                LabelConnectedLand(y - 1, x, massNo, mass);
            }
            if (y + 1 < world.MapRows && mass[y + 1, x] == 0 && world.walkableTerrains.Contains(world.map[y + 1, x]))
            {
                LabelConnectedLand(y + 1, x, massNo, mass);
            }
            if (x - 1 > 0 && mass[y, x - 1] == 0 && world.walkableTerrains.Contains(world.map[y, x - 1]))
            {
                LabelConnectedLand(y, x - 1, massNo, mass);
            }
            if (x + 1 < world.MapColumns && mass[y, x + 1] == 0 && world.walkableTerrains.Contains(world.map[y, x + 1]))
            {
                LabelConnectedLand(y, x + 1, massNo, mass);
            }
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("CodeQuality", "IDE0051:Remove unused private members", Justification = "Debug Method")]
        private string PrintRegionMap(int[,] mass)
        {
            char[] encoding = { '0', '1', '2', '3', '4', '5', '6', '7', '8', '9', 'A', 'B', 'C', 'D', 'E', 'F', 'G', 'H', 'I', 'J', 'K', 'L', 'M', 'N', 'O', 'P', 'Q' };
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < world.MapRows; i++)
            {
                for (int j = 0; j < world.MapColumns; j++)
                {
                    sb.Append(encoding[mass[i, j]]);
                }
                sb.Append('\n');
            }
            return sb.ToString();
        }

        /// <summary>
        /// Returns true IFF this is an isolated single tile. i.e. the
        /// terrain type one space in each of the 4 orthogonal directions
        /// is not walkable terrain.
        /// </summary>
        private bool IsBlockedSingleTile(IntVector2 pos)
        {
            return IntVector2.CARDINALS.All(d =>
            {
                var checkPos = pos + d;

                // out of bounds is effectively not walkable (blocked).
                if (!world.WithinMapBounds(checkPos)) { return true; }

                var checkTerrain = world.map[checkPos];
                return !world.walkableTerrains.Contains(checkTerrain);
            });
        }
    }
}
