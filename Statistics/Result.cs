﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Z2Randomizer.Overworld;

namespace Z2Randomizer.Statistics;

class Result
{
    [Key]
    public int Id { get; set; }
    [Required]
    public string Flags { get; set; }
    [Required]
    public int Seed { get; set; }
    public int GenerationTime { get; set; }

    //Palace Generation
    public int P1GenerationAttempts { get; set; }
    public int P2GenerationAttempts { get; set; }
    public int P3GenerationAttempts { get; set; }
    public int P4GenerationAttempts { get; set; }
    public int P5GenerationAttempts { get; set; }
    public int P6GenerationAttempts { get; set; }
    public int GPGenerationAttempts { get; set; }

    //Overall progress timestamps
    public int ProgressStartTo2 { get; set; }
    public int Progress2To3 { get; set; }
    public int Progress3To4 { get; set; }
    public int Progress4To5 { get; set; }
    public int Progress5To6 { get; set; }
    public int Progress6To8 { get; set; }
    public int Progress8To9 { get; set; }

    public int ProgressStartToGenerateStartingValues { get; set; }
    public int ProgressStartingValuesToGenerateEnemies { get; set; }
    public int ProgressGenerateEnemiesToProcessOverworld { get; set; }

    public int TotalReachabilityOverworldAttempts { get; set; }
    public int TotalContinentConnectionOverworldAttempts { get; set; }
    public int TotalWestGenerationAttempts { get; set; }
    public int TotalEastGenerationAttempts { get; set; }
    public int TotalMazeIslandGenerationAttempts { get; set; }
    public int TotalDeathMountainGenerationAttempts { get; set; }
    public int IsEverythingReachableFailures { get; set; }

    //West Contient failure causes
    public int WestFailedOnLocationPlacement { get; set; }
    public int WestFailedOnBaguPlacement { get; set; }
    public int WestTerrainGrowthAttempts { get; set; }
    public int WestFailedOnRaftPlacement { get; set; }
    public int WestFailedOnBridgePlacement { get; set; }
    public int WestFailedOnIslandConnection { get; set; }
    public int WestFailedOnMakeCaldera { get; set; }
    public int WestFailedOnConnectIslands { get; set; }



    //Spell costs
    //XP thresholds
    //Which items are required (is this cleanly saved?)
    public Result()
    {

    }

    public Result(Hyrule hyrule)
    {
        Flags = hyrule.Flags;
        Seed = hyrule.Seed;

        //Palace Generation Attempts
        P1GenerationAttempts = hyrule.palaces[0].Generations;
        P2GenerationAttempts = hyrule.palaces[1].Generations;
        P3GenerationAttempts = hyrule.palaces[2].Generations;
        P4GenerationAttempts = hyrule.palaces[3].Generations;
        P5GenerationAttempts = hyrule.palaces[4].Generations;
        P6GenerationAttempts = hyrule.palaces[5].Generations;
        GPGenerationAttempts = hyrule.palaces[6].Generations;

        //Overall progress timestamps
        ProgressStartTo2 = (int)hyrule.updateProgress2Timestamp.Subtract(hyrule.startTime).TotalMilliseconds;
        Progress2To3 = (int)hyrule.updateProgress3Timestamp.Subtract(hyrule.updateProgress2Timestamp).TotalMilliseconds;
        Progress3To4 = (int)hyrule.updateProgress4Timestamp.Subtract(hyrule.updateProgress3Timestamp).TotalMilliseconds;
        Progress4To5 = (int)hyrule.updateProgress5Timestamp.Subtract(hyrule.updateProgress4Timestamp).TotalMilliseconds;
        Progress5To6 = (int)hyrule.updateProgress6Timestamp.Subtract(hyrule.updateProgress5Timestamp).TotalMilliseconds;
        Progress6To8 = (int)hyrule.updateProgress8Timestamp.Subtract(hyrule.updateProgress6Timestamp).TotalMilliseconds;
        Progress8To9 = (int)hyrule.updateProgress9Timestamp.Subtract(hyrule.updateProgress8Timestamp).TotalMilliseconds;

        ProgressStartToGenerateStartingValues = (int)hyrule.startRandomizeStartingValuesTimestamp.Subtract(hyrule.startTime).TotalMilliseconds;
        ProgressStartingValuesToGenerateEnemies = (int)hyrule.startRandomizeEnemiesTimestamp.Subtract(hyrule.startRandomizeStartingValuesTimestamp).TotalMilliseconds;
        ProgressGenerateEnemiesToProcessOverworld = (int)hyrule.firstProcessOverworldTimestamp.Subtract(hyrule.startRandomizeEnemiesTimestamp).TotalMilliseconds;

        TotalReachabilityOverworldAttempts = hyrule.totalReachabilityOverworldAttempts;
        TotalContinentConnectionOverworldAttempts = hyrule.totalContinentConnectionOverworldAttempts;
        TotalWestGenerationAttempts = hyrule.totalWestGenerationAttempts;
        TotalEastGenerationAttempts = hyrule.totalEastGenerationAttempts;
        TotalMazeIslandGenerationAttempts = hyrule.totalMazeIslandGenerationAttempts;
        TotalDeathMountainGenerationAttempts = hyrule.totalDeathMountainGenerationAttempts;
        IsEverythingReachableFailures = hyrule.isEverythingReachableFailures;

        //West Contient failure causes
        WestFailedOnLocationPlacement = WestHyrule.failedOnPlaceLocations;
        WestFailedOnBaguPlacement = WestHyrule.failedOnBaguPlacement;
        //WestTerrainGrowthAttempts = WestHyrule.terrainGrowthAttempts;
        WestFailedOnRaftPlacement = WestHyrule.failedOnRaftPlacement;
        WestFailedOnBridgePlacement = WestHyrule.failedOnBridgePlacement;
        WestFailedOnIslandConnection = WestHyrule.failedOnIslandConnection;
        WestFailedOnMakeCaldera = WestHyrule.failedOnMakeCaldera;
        WestFailedOnConnectIslands = WestHyrule.failedOnConnectIslands;
    }
}
