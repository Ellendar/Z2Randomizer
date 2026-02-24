using System;
using System.Collections.Generic;
using Z2Randomizer.RandomizerCore;

namespace Z2Randomizer.RandomizerCore.Overworld;
public static class Climates
{
    private static readonly Climate CLASSIC = new(
        "Classic",
        new Dictionary<Terrain, float>
        {
            { Terrain.DESERT, 1 },
            { Terrain.GRASS, 1 },
            { Terrain.FOREST, 1 },
            { Terrain.SWAMP, 1 },
            { Terrain.GRAVE, 1 },
            { Terrain.LAVA, 1 },
            { Terrain.WALKABLEWATER, 1 },
            { Terrain.PREPLACED_WATER_WALKABLE, 1 },
            { Terrain.PREPLACED_WATER, 1 },
            { Terrain.WATER, 1 },
            { Terrain.MOUNTAIN, 1 },
            { Terrain.ROAD, 1 }
        },
        new TableWeightedRandom<Terrain>([
            ( Terrain.DESERT, 1 ),
            ( Terrain.GRASS, 1 ),
            ( Terrain.FOREST, 1 ),
            ( Terrain.SWAMP, 1 ),
            ( Terrain.GRAVE, 1 ),
            ( Terrain.LAVA, 1 ),
            ( Terrain.WALKABLEWATER, 1 ),
            ( Terrain.WATER, 1 ),
            ( Terrain.MOUNTAIN, 1 ),
            ( Terrain.ROAD, 1 ),
        ]),
        30
    );

    private static readonly Climate VANILLA_WEIGHTED_WEST = new(
        "Vanilla-Weighted West",
        new Dictionary<Terrain, float>
        {
            { Terrain.DESERT, 1 },
            { Terrain.GRASS, 1 },
            { Terrain.FOREST, 1 },
            { Terrain.SWAMP, 1 },
            { Terrain.GRAVE, 1 },
            { Terrain.LAVA, 1 },
            { Terrain.WALKABLEWATER, 1 },
            { Terrain.PREPLACED_WATER_WALKABLE, 1 },
            { Terrain.PREPLACED_WATER, 1 },
            { Terrain.WATER, 1 },
            { Terrain.MOUNTAIN, 1 },
            { Terrain.ROAD, 1 }
        },
        new LinearWeightedRandom<Terrain>([
            ( Terrain.DESERT, 615 ),
            ( Terrain.GRASS, 647 ),
            ( Terrain.FOREST, 359 ),
            ( Terrain.SWAMP, 300 ),
            ( Terrain.GRAVE, 256 ),
            ( Terrain.LAVA, 0 ),
            ( Terrain.WALKABLEWATER, 712 ),
            ( Terrain.WATER, 712 ),
            ( Terrain.MOUNTAIN, 1546 ),
            ( Terrain.ROAD, 328 ),
        ]),
        30
    );

    private static readonly Climate VANILLA_WEIGHTED_EAST = new(
        "Vanilla-Weighted East",
        new Dictionary<Terrain, float>
        {
            { Terrain.DESERT, 1 },
            { Terrain.GRASS, 1 },
            { Terrain.FOREST, 1 },
            { Terrain.SWAMP, 1 },
            { Terrain.GRAVE, 1 },
            { Terrain.LAVA, 1 },
            { Terrain.WALKABLEWATER, 1 },
            { Terrain.PREPLACED_WATER_WALKABLE, 1 },
            { Terrain.PREPLACED_WATER, 1 },
            { Terrain.WATER, 1 },
            { Terrain.MOUNTAIN, 1 },
            { Terrain.ROAD, 1 }
        },
        new LinearWeightedRandom<Terrain>([
            ( Terrain.DESERT, 542 ),
            ( Terrain.GRASS, 401 ),
            ( Terrain.FOREST, 556 ),
            ( Terrain.SWAMP, 90 ),
            ( Terrain.GRAVE, 45 ),
            ( Terrain.LAVA, 131 ),
            ( Terrain.WALKABLEWATER, 1359 ), /* 350 walkable */
            ( Terrain.WATER, 1359 ),
            ( Terrain.MOUNTAIN, 1514 ),
            ( Terrain.ROAD, 128 ),
        ]),
        30
    );

    private static readonly Climate CHAOS = new(
        "Chaos",
        //Coefficients
        new Dictionary<Terrain, float>
        {
            { Terrain.DESERT, 1 },
            { Terrain.GRASS, 1 },
            { Terrain.FOREST, 1 },
            { Terrain.SWAMP, 1 },
            { Terrain.GRAVE, 1 },
            { Terrain.LAVA, 1 },
            { Terrain.WALKABLEWATER, 1 },
            { Terrain.PREPLACED_WATER_WALKABLE, 1 },
            { Terrain.PREPLACED_WATER, 1 },
            { Terrain.WATER, 1 },
            { Terrain.MOUNTAIN, 1 },
            { Terrain.ROAD, .2f }
        },
        //weights
        new TableWeightedRandom<Terrain>([
            ( Terrain.DESERT, 5 ),
            ( Terrain.GRASS, 5 ),
            ( Terrain.FOREST, 5 ),
            ( Terrain.SWAMP, 5 ),
            ( Terrain.GRAVE, 5 ),
            ( Terrain.LAVA, 5 ),
            ( Terrain.WALKABLEWATER, 1 ),
            ( Terrain.WATER, 1 ),
            ( Terrain.MOUNTAIN, 5 ),
            ( Terrain.ROAD, 5 ),
        ]),
        300
    );

    private static readonly Climate WETLANDS = new(
        "Wetlands",
        //Size
        new Dictionary<Terrain, float>
        {
            { Terrain.DESERT, 1 },
            { Terrain.GRASS, 1 },
            { Terrain.FOREST, 1 },
            { Terrain.SWAMP, 2.5f },
            { Terrain.GRAVE, 1 },
            { Terrain.LAVA, 1 },
            { Terrain.WALKABLEWATER, 1 },
            { Terrain.PREPLACED_WATER_WALKABLE, 1 },
            { Terrain.PREPLACED_WATER, 1 },
            { Terrain.WATER, 1 },
            { Terrain.MOUNTAIN, 1 },
            { Terrain.ROAD, 1 }
        },
        //Frequency
        new TableWeightedRandom<Terrain>([
            ( Terrain.DESERT, 0 ),
            ( Terrain.GRASS, 2 ),
            ( Terrain.FOREST, 2 ),
            ( Terrain.SWAMP, 2 ),
            ( Terrain.GRAVE, 2 ),
            ( Terrain.LAVA, 1 ),
            ( Terrain.WALKABLEWATER, 3 ),
            ( Terrain.WATER, 3 ),
            ( Terrain.MOUNTAIN, 2 ),
            ( Terrain.ROAD, 3 ),
        ]),
        30
    );

    private static readonly Climate SCRUBLAND = new(
        "Scrubland",
        //Size
        new Dictionary<Terrain, float>
        {
            { Terrain.DESERT, 1 },
            { Terrain.GRASS, 1 },
            { Terrain.FOREST, .5f },
            { Terrain.SWAMP, .25f },
            { Terrain.GRAVE, .35f },
            { Terrain.LAVA, .5f },
            { Terrain.WALKABLEWATER, .5f },
            { Terrain.PREPLACED_WATER_WALKABLE, 1 },
            { Terrain.PREPLACED_WATER, 1 },
            { Terrain.WATER, .25f },
            { Terrain.MOUNTAIN, 1 },
            { Terrain.ROAD, 1 }
        },
        //Frequency
        new TableWeightedRandom<Terrain>([
            ( Terrain.DESERT, 2 ),
            ( Terrain.GRASS, 3 ),
            ( Terrain.FOREST, 1 ),
            ( Terrain.SWAMP, 0 ),
            ( Terrain.GRAVE, 4 ),
            ( Terrain.LAVA, 1 ),
            ( Terrain.WALKABLEWATER, 1 ),
            ( Terrain.WATER, 1 ),
            ( Terrain.MOUNTAIN, 1 ),
            ( Terrain.ROAD, 2 ),
        ]),
        30
    );

    private static readonly Climate GREAT_LAKES = new(
        "GreatLakes",
        //Size
        new Dictionary<Terrain, float>
        {
            { Terrain.DESERT, .5f },
            { Terrain.GRASS, 1 },
            { Terrain.FOREST, 1.5f },
            { Terrain.SWAMP, .75f },
            { Terrain.GRAVE, 1 },
            { Terrain.LAVA, 1 },
            { Terrain.WALKABLEWATER, 5 },
            { Terrain.PREPLACED_WATER_WALKABLE, 1.5f },
            { Terrain.PREPLACED_WATER, 1.5f },
            { Terrain.WATER, 5 },
            { Terrain.MOUNTAIN, .75f },
            { Terrain.ROAD, 1 }
        },
        //Frequency
        new TableWeightedRandom<Terrain>([
            ( Terrain.DESERT, 0 ),
            ( Terrain.GRASS, 2 ),
            ( Terrain.FOREST, 2 ),
            ( Terrain.SWAMP, 2 ),
            ( Terrain.GRAVE, 2 ),
            ( Terrain.LAVA, 2 ),
            ( Terrain.WALKABLEWATER, 2 ),
            ( Terrain.WATER, 2 ),
            ( Terrain.MOUNTAIN, 2 ),
            ( Terrain.ROAD, 3 ),
        ]),
        30
    );
    
    public static Climate Create(ClimateEnum climate)
    {
        switch (climate)
        {
            case ClimateEnum.CLASSIC:
                return CLASSIC.CloneWithInvertedDistances();
            case ClimateEnum.VANILLA_WEIGHTED_WEST:
                return VANILLA_WEIGHTED_WEST.Clone();
            case ClimateEnum.VANILLA_WEIGHTED_EAST:
                return VANILLA_WEIGHTED_EAST.Clone();
            case ClimateEnum.CHAOS:
                return CHAOS.CloneWithInvertedDistances();
            case ClimateEnum.WETLANDS:
                return WETLANDS.CloneWithInvertedDistances();
            case ClimateEnum.GREAT_LAKES:
                return GREAT_LAKES.CloneWithInvertedDistances();
            case ClimateEnum.SCRUBLAND:
                return SCRUBLAND.CloneWithInvertedDistances();
            default:
                throw new NotImplementedException();
        }
    }
}
