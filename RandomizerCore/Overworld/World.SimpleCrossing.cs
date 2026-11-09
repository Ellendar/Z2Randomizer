using System;
using System.Linq;

namespace Z2Randomizer.RandomizerCore.Overworld;

public abstract partial class World
{
    /// <summary>
    /// Paints terrain crossings that may place one encounter <see cref="Location"/> or a blocker
    /// at the midpoint. A blocker takes precedence over the encounter.
    /// Terrain-cycle progress and pending blockers are shared by simple crossings within one request.
    /// </summary>
    private sealed class SimpleCrossingPainter
    {
        private readonly World world;
        private readonly RegionCrossingOptions options;
        private readonly Terrain[] crossingTerrains;
        private bool pendingRiverDevil;
        private bool pendingRock;
        private int terrainCycle;

        internal SimpleCrossingPainter(World world, RegionCrossingOptions options, Terrain[] crossingTerrains)
        {
            this.world = world;
            this.options = options;
            this.crossingTerrains = crossingTerrains;
            pendingRiverDevil = options.RiverDevilBlocker;
            pendingRock = options.RockBlocker;
        }

        internal bool TryPaint(RegionCrossingSpan span, Random r)
        {
            if (terrainCycle == 2)
            {
                PaintWalkableWater(span);
            }
            else if (!TryPaintLandCrossing(span, r))
            {
                return false;
            }

            terrainCycle++;
            if (options.CrossingTerrain is not (Terrain.MOUNTAIN or Terrain.DESERT) && !options.CanWalkOnWaterWithBoots)
                terrainCycle %= 3;
            else
                terrainCycle %= 2;

            return true;
        }

        private bool TryPaintLandCrossing(RegionCrossingSpan span, Random r)
        {
            Terrain terrain = ChooseLandTerrain(span);
            MountainLane[] lanes = [];
            if (options.ShapesMountainPassages)
            {
                lanes = PlanMountainLanes(span, r);
                if (span.Length >= MinimumLengthRequiringWidePassage && lanes.Length == 0)
                {
                    return false;
                }
            }

            var position = span.End - span.Forward;
            int tileIndex = 0;
            while (crossingTerrains.Contains(world.map[position]))
            {
                if (ShouldPaintLandTile(tileIndex, span.Length))
                {
                    world.map[position] = terrain;
                }
                foreach (var lane in lanes)
                {
                    if (tileIndex >= lane.FirstTile && tileIndex <= lane.LastTile)
                    {
                        world.map[position + lane.Side] = terrain;
                    }
                }
                if (tileIndex == span.Length / 2)
                {
                    PlaceMidpoint(position, terrain);
                }
                tileIndex++;
                position -= span.Forward;
            }
            return true;
        }

        private MountainLane[] PlanMountainLanes(RegionCrossingSpan span, Random r)
        {
            if (pendingRiverDevil || pendingRock)
            {
                // The blocker forms the waist. Let each half open into its bank where it fits.
                int midpoint = span.Length / 2;
                IntVector2 blockerLaneSide = r.Next(2) == 0 ? span.Side : -span.Side;
                var beforeBlocker = world.PlanMountainFunnel(span, 0, midpoint - 1, openAtFirstTile: true, blockerLaneSide);
                var afterBlocker = world.PlanMountainFunnel(span, midpoint + 1, span.Length - 1, openAtFirstTile: false, blockerLaneSide);
                return [.. beforeBlocker, .. afterBlocker];
            }

            int neckLength = MountainNeckLength(span.Length);
            int lastTile = span.Length - 1;
            int preferredShape = r.Next(5) switch
            {
                < 2 => 0,
                < 4 => 1,
                _ => 2
            };
            IntVector2 preferredSide = r.Next(2) == 0 ? span.Side : -span.Side;

            for (int offset = 0; offset < 3; offset++)
            {
                int shape = (preferredShape + offset) % 3;
                MountainLane[] lanes = shape switch
                {
                    0 => world.PlanMountainFunnel(span, 0, lastTile - neckLength, openAtFirstTile: true, preferredSide),
                    1 => world.PlanMountainFunnel(span, neckLength, lastTile, openAtFirstTile: false, preferredSide),
                    _ => world.PlanMountainDoubleNeck(span, neckLength, lastTile - neckLength, preferredSide)
                };
                if (lanes.Length > 0)
                {
                    return lanes;
                }
            }
            return [];
        }

        private Terrain ChooseLandTerrain(RegionCrossingSpan span) => options.Biome switch
        {
            // Vanilla-like distinguishes preplaced water from these final water values.
            Biome.VANILLALIKE => options.CrossingTerrain is Terrain.WATER or Terrain.WALKABLEWATER ? Terrain.BRIDGE : Terrain.ROAD,
            Biome.MOUNTAINOUS => world.map[span.End],

            Biome.VANILLA or
            Biome.VANILLA_SHUFFLE or
            Biome.ISLANDS or
            Biome.CANYON or
            Biome.DRY_CANYON or
            Biome.VOLCANO or
            Biome.CALDERA
                => terrainCycle == 0 ? Terrain.ROAD : Terrain.BRIDGE,

            _ => throw new ArgumentOutOfRangeException(nameof(options.Biome))
        };

        private bool ShouldPaintLandTile(int tileIndex, int length)
        {
            if (options.Biome is Biome.MOUNTAINOUS or Biome.VANILLALIKE || terrainCycle == 0) return true;
            // Retain the untouched tile beside the midpoint in non-water bridge spans.
            return options.CrossingTerrain.IsWater()
                || options.CrossingTerrain == Terrain.MOUNTAIN
                || (options.CrossingTerrain != Terrain.WALKABLEWATER && tileIndex != length / 2 + 1);
        }

        private void PlaceMidpoint(IntVector2 position, Terrain terrain)
        {
            if (TryPlaceBlocker(position)) return;

            foreach (var location in world.Locations[terrain])
            {
                if (!location.CanShuffle) continue;

                location.Pos = position;
                location.CanShuffle = false;
                break;
            }
        }

        private bool TryPlaceBlocker(IntVector2 position)
        {
            if (pendingRiverDevil)
            {
                world.map[position] = Terrain.RIVER_DEVIL;
                pendingRiverDevil = false;
                return true;
            }
            if (pendingRock)
            {
                world.map[position] = Terrain.ROCK;
                pendingRock = false;
                return true;
            }
            return false;
        }

        private void PaintWalkableWater(RegionCrossingSpan span)
        {
            var position = span.End - span.Forward;
            world.map[span.End] = Terrain.ROAD;
            while (crossingTerrains.Contains(world.map[position]))
            {
                world.map[position] = Terrain.WALKABLEWATER;
                position -= span.Forward;
            }
            world.map[position] = Terrain.ROAD;
        }
    }
}
