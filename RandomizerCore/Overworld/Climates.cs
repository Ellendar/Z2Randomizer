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
                { Terrain.ROAD, 0 }
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
                { Terrain.ROAD, 1 }
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
                { Terrain.ROAD, 0 }
           },
       300
   );
}


//Terrain.DESERT, Terrain.GRASS, Terrain.FOREST, Terrain.SWAMP, Terrain.GRAVE, Terrain.MOUNTAIN