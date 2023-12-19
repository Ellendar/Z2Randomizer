using System;
using System.Collections.Generic;
using System.Linq;
using Z2Randomizer.Core;

namespace Z2Randomizer.Core.Overworld;
public class Climates
{
    public static readonly Climate Classic = new(
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
                { Terrain.WATER, 1 },
                { Terrain.MOUNTAIN, 1 },
                { Terrain.ROAD, 1 }
            },
        new Dictionary<Terrain, int> 
            {
                { Terrain.DESERT, 1 },
                { Terrain.GRASS, 1 },
                { Terrain.FOREST, 1 },
                { Terrain.SWAMP, 1 },
                { Terrain.GRAVE, 1 },
                { Terrain.LAVA, 1 },
                { Terrain.WALKABLEWATER, 1 },
                { Terrain.WATER, 1 },
                { Terrain.MOUNTAIN, 1 },
                { Terrain.ROAD, 1 }
            }, 
        30 
    );

    public static readonly Climate Chaos = new(
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
                { Terrain.WATER, 1 },
                { Terrain.MOUNTAIN, 1 },
                { Terrain.ROAD, .2f }
           },
       //weights
       new Dictionary<Terrain, int>
           {
                { Terrain.DESERT, 5 },
                { Terrain.GRASS, 5 },
                { Terrain.FOREST, 5 },
                { Terrain.SWAMP, 5 },
                { Terrain.GRAVE, 5 },
                { Terrain.LAVA, 5 },
                { Terrain.WALKABLEWATER, 1 },
                { Terrain.WATER, 1 },
                { Terrain.MOUNTAIN, 5 },
                { Terrain.ROAD, 5 }
           },
       300
   );

    public static readonly Climate Wetlands = new(
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
                { Terrain.WATER, 1 },
                { Terrain.MOUNTAIN, 1 },
                { Terrain.ROAD, 1 }
        },
        //Frequency
        new Dictionary<Terrain, int>
        {
                { Terrain.DESERT, 0 },
                { Terrain.GRASS, 2 },
                { Terrain.FOREST, 2 },
                { Terrain.SWAMP, 2 },
                { Terrain.GRAVE, 2 },
                { Terrain.LAVA, 1 },
                { Terrain.WALKABLEWATER, 3 },
                { Terrain.WATER, 3 },
                { Terrain.MOUNTAIN, 2 },
                { Terrain.ROAD, 3 }
        },
        30
    );

    public static readonly Climate Scrubland = new(
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
                { Terrain.WATER, .25f },
                { Terrain.MOUNTAIN, 1 },
                { Terrain.ROAD, 1 }
        },
        //Frequency
        new Dictionary<Terrain, int>
        {
                { Terrain.DESERT, 2 },
                { Terrain.GRASS, 3 },
                { Terrain.FOREST, 1 },
                { Terrain.SWAMP, 0 },
                { Terrain.GRAVE, 4 },
                { Terrain.LAVA, 1 },
                { Terrain.WALKABLEWATER, 1 },
                { Terrain.WATER, 1 },
                { Terrain.MOUNTAIN, 1 },
                { Terrain.ROAD, 2 }
        },
        30
    );

    public static readonly Climate GreatLakes = new(
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
                { Terrain.WATER, 5 },
                { Terrain.MOUNTAIN, .75f },
                { Terrain.ROAD, 1 }
        },
        //Frequency
        new Dictionary<Terrain, int>
        {
                { Terrain.DESERT, 0 },
                { Terrain.GRASS, 2 },
                { Terrain.FOREST, 2 },
                { Terrain.SWAMP, 2 },
                { Terrain.GRAVE, 2 },
                { Terrain.LAVA, 2 },
                { Terrain.WALKABLEWATER, 2 },
                { Terrain.WATER, 2 },
                { Terrain.MOUNTAIN, 2 },
                { Terrain.ROAD, 3 }
        },
        30
    );

}


//Terrain.DESERT, Terrain.GRASS, Terrain.FOREST, Terrain.SWAMP, Terrain.GRAVE, Terrain.MOUNTAIN