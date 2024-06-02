using System.ComponentModel.DataAnnotations;
using RandomizerCore;
using RandomizerCore.Overworld;

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
    public int TimeBuildingWest { get; set; }
    public int TimeBuildingEast { get; set; }
    public int TimeBuildingDM { get; set; }
    public int TimeBuildingMI { get; set; }

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
        Seed = hyrule.SeedHash;

        //Palace Generation Attempts
        P1GenerationAttempts = hyrule.palaces[0].Generations;
        P2GenerationAttempts = hyrule.palaces[1].Generations;
        P3GenerationAttempts = hyrule.palaces[2].Generations;
        P4GenerationAttempts = hyrule.palaces[3].Generations;
        P5GenerationAttempts = hyrule.palaces[4].Generations;
        P6GenerationAttempts = hyrule.palaces[5].Generations;
        GPGenerationAttempts = hyrule.palaces[6].Generations;

        //Overall progress timestamps
        TimeBuildingDM = hyrule.timeSpentBuildingDM;
        TimeBuildingEast = hyrule.timeSpentBuildingEast;
        TimeBuildingMI = hyrule.timeSpentBuildingMI;
        TimeBuildingWest = hyrule.timeSpentBuildingWest;

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
