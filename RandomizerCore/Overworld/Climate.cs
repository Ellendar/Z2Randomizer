using Microsoft.VisualBasic.FileIO;
using NLog.Filters;
using RandomizerCore.Flags;
using System;
using System.Collections.Generic;
using System.Linq;
using Z2Randomizer.Core;

namespace Z2Randomizer.Core.Overworld;
public class Climate
{
    private readonly Dictionary<Terrain, float> DistanceCoefficients;
    private readonly Dictionary<Terrain, int> TerrainWeights;
    private readonly Dictionary<Terrain, int> TerrainPercentages;
    public int SeedTerrainCount { get; set; }
    private List<Terrain> terrainWeightTable;
    public string Name { get; set; }

    public Climate(string name, Dictionary<Terrain, float> distanceCoefficients, Dictionary<Terrain, int> terrainWeights, int seedTerrainCount)
    {
        Name = name;
        DistanceCoefficients = distanceCoefficients;
        TerrainWeights = terrainWeights;
        SeedTerrainCount = seedTerrainCount;
        //This is a lazy, not very efficient lookup method, but it has very fast lookups, and since terrain generation
        //is one of the most expensive operations and we're going to do tens of thousands of lookups easily,
        //this is probably good for now.
        terrainWeightTable = new();
        foreach (Terrain terrain in TerrainWeights.Keys)
        {
            for (int i = 0; i < TerrainWeights[terrain]; i++)
            {
                terrainWeightTable.Add(terrain);
            }
        }
    }

    public float GetDistanceCoefficient(Terrain terrain)
    {
        if (!DistanceCoefficients.TryGetValue(terrain, out float value))
        {
            return 0;
        }
        //Returning the reciprocal of the distance makes the sizes of terrains scale proportionately with the coefficient's value
        //I feel this is the most intuitive way to represent this, but maybe I am wrong.
        return 1f / value;
    }

    public Terrain GetRandomTerrain(Random r, IEnumerable<Terrain> whitelist)
    {
        Terrain result;
        do
        {
            result = terrainWeightTable[r.Next(terrainWeightTable.Count)];
        }
        while (whitelist == null || !whitelist.Contains(result));

        return result;
    }

    public IEnumerable<Terrain> RandomTerrains(IEnumerable<Terrain> filter)
    {
        if (filter == null)
        {
            return terrainWeightTable.Distinct();
        }
        return filter.Intersect(terrainWeightTable.Distinct());

    }

    public void DisallowTerrain(Terrain terrain)
    {
        for (int i = 0; i < TerrainWeights[terrain]; i++)
        {
            terrainWeightTable.RemoveAll(i => i == terrain);
        }
    }

    public Climate Clone()
    {
        return new(Name, 
            new Dictionary<Terrain, float>(DistanceCoefficients), 
            new Dictionary<Terrain, int>(TerrainWeights), 
            SeedTerrainCount);
    }

    /// <summary>
    /// If death mountain has distance coefficients that are too small relative to the growth of mountains,
    /// there isn't ever enough open space to walk around between caves and it never properly generates.
    /// In order to resolve that, if the aggregate weighted terrain growth factor is less than that of mountain
    /// (times the factor provided in constraintLimitFactor), increase the growth factor of each terrain type
    /// by the relative difference between mountain terrain growth and that terrain growth.
    /// </summary>
    /// <param name="walkableTerrains">Types of terrains that can be randomly generated that need to be scaled.</param>
    /// <param name="constraintLimitFactor">The relative permissible factor between walkable and mountain terrain growths</param>
    public void ApplyDeathMountainSafety(IEnumerable<Terrain> walkableTerrains, float constraintLimitFactor = 1f)
    {
        float totalRandomGrowthFactors = 0f;
        float totalWalkableTerrainWeight = 0f;
        foreach (Terrain terrain in walkableTerrains)
        {
            totalRandomGrowthFactors += TerrainWeights[terrain] * DistanceCoefficients[terrain];
            totalWalkableTerrainWeight += TerrainWeights[terrain];
        }
        float aggregateWalkableTerrainGrowth = totalRandomGrowthFactors / totalWalkableTerrainWeight;
        float mountainTerrainGrowth = DistanceCoefficients[Terrain.MOUNTAIN];

        if (mountainTerrainGrowth * constraintLimitFactor > aggregateWalkableTerrainGrowth)
        {
            foreach (Terrain terrain in walkableTerrains)
            {
                DistanceCoefficients[terrain] *= ((mountainTerrainGrowth * constraintLimitFactor) / aggregateWalkableTerrainGrowth);
            }
        }
    }
}

