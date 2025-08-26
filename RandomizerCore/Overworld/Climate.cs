using System;
using System.Collections.Generic;
using System.Linq;

namespace Z2Randomizer.RandomizerCore.Overworld;

public class Climate
{
    public float[] DistanceCoefficients { get; }
    private readonly int[] TerrainWeights;
    public int SeedTerrainCount { get; set; }
    private List<Terrain> terrainWeightTable;
    public string Name { get; set; }

    public Climate(string name, Dictionary<Terrain, float> distanceCoefficients, Dictionary<Terrain, int> terrainWeights, int seedTerrainCount, bool providedDistancesAreInverted = false)
    {
        Name = name;
        // Precompute distance coefficients into a inverse list
        DistanceCoefficients = new float[(int)Terrain.NONE];
        foreach (var pair in distanceCoefficients)
        {
            DistanceCoefficients[(int)pair.Key] = providedDistancesAreInverted ? pair.Value : (1f / pair.Value);
        }
        TerrainWeights = new int[(int)Terrain.NONE];
        foreach (var pair in terrainWeights)
        {
            TerrainWeights[(int)pair.Key] = pair.Value;
        }
        SeedTerrainCount = seedTerrainCount;
        //This is a lazy, not very efficient lookup method, but it has very fast lookups, and since terrain generation
        //is one of the most expensive operations and we're going to do tens of thousands of lookups easily,
        //this is probably good for now.
        terrainWeightTable = new();
        for (int terrain = 0; terrain < (int)Terrain.NONE; terrain++)
        {
            for (int i = 0; i < TerrainWeights[(int)terrain]; i++)
            {
                terrainWeightTable.Add((Terrain)terrain);
            }
        }
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
        for (int i = 0; i < TerrainWeights[(int)terrain]; i++)
        {
            terrainWeightTable.RemoveAll(i => i == terrain);
        }
    }

    public Climate Clone()
    {
        return new(Name,
            new Dictionary<Terrain, float>(DistanceCoefficients
                .Select((value, index) => new { value, index })
                .ToDictionary(pair => (Terrain)pair.index, pair => pair.value)),
            new Dictionary<Terrain, int>(TerrainWeights
                .Select((value, index) => new { value, index })
                .ToDictionary(pair => (Terrain)pair.index, pair => pair.value)),
            SeedTerrainCount, providedDistancesAreInverted: true);
    }

    public override bool Equals(object? obj)
    {
        if (obj is Climate climate)
            return Equals(climate);
        return false;
    }

    protected bool Equals(Climate other)
    {
        return TerrainWeights.SequenceEqual(other.TerrainWeights)
               && terrainWeightTable.SequenceEqual(other.terrainWeightTable)
               && DistanceCoefficients.SequenceEqual(other.DistanceCoefficients)
               && SeedTerrainCount == other.SeedTerrainCount
               && Name == other.Name;
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(TerrainWeights, terrainWeightTable, DistanceCoefficients, SeedTerrainCount, Name);
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
        float coefficientSum = 0f;
        foreach (Terrain terrain in walkableTerrains)
        {
            if(terrain != Terrain.ROAD)
            {
                coefficientSum += DistanceCoefficients[(int)terrain];
            }
        }
        float averageCoefficient = coefficientSum / (walkableTerrains.Count() - walkableTerrains.Count(i => i == Terrain.ROAD));
        DistanceCoefficients[(int)Terrain.ROAD] = float.Min(DistanceCoefficients[(int)Terrain.ROAD], averageCoefficient);

        float totalRandomGrowthFactors = 0f;
        float totalWalkableTerrainWeight = 0f;
        foreach (Terrain terrain in walkableTerrains)
        {
            totalRandomGrowthFactors += TerrainWeights[(int)terrain] / DistanceCoefficients[(int)terrain];
            totalWalkableTerrainWeight += TerrainWeights[(int)terrain];
        }
        float aggregateWalkableTerrainGrowth = totalRandomGrowthFactors / totalWalkableTerrainWeight;
        float mountainTerrainGrowth = 1 / DistanceCoefficients[(int)Terrain.MOUNTAIN];

        if (mountainTerrainGrowth * constraintLimitFactor > aggregateWalkableTerrainGrowth)
        {
            foreach (Terrain terrain in walkableTerrains)
            {
                DistanceCoefficients[(int)terrain] /= ((mountainTerrainGrowth * constraintLimitFactor) / aggregateWalkableTerrainGrowth);
            }
        }
    }
}

