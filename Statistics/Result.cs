using FtRandoLib.Importer;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using Z2Randomizer.RandomizerCore;
using Z2Randomizer.RandomizerCore.Overworld;
using Z2Randomizer.RandomizerCore.Sidescroll;

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
    public int WestFailedOnConnectIslands { get; set; } = 0;

    public float P1WidthRatio { get; set; }
    public int P1Size { get; set; }
    public int P1MinDistanceToBoss { get; set; }
    public float P2WidthRatio { get; set; }
    public int P2Size { get; set; }
    public int P2MinDistanceToBoss { get; set; }
    public float P3WidthRatio { get; set; }
    public int P3Size { get; set; }
    public int P3MinDistanceToBoss { get; set; }
    public float P4WidthRatio { get; set; }
    public int P4Size { get; set; }
    public int P4MinDistanceToBoss { get; set; }
    public float P5WidthRatio { get; set; }
    public int P5Size { get; set; }
    public int P5MinDistanceToBoss { get; set; }
    public float P6WidthRatio { get; set; }
    public int P6Size { get; set; }
    public int P6MinDistanceToBoss { get; set; }
    public float GPWidthRatio { get; set; }
    public int GPSize { get; set; }
    public int GPMinDistanceToBoss { get; set; }

#pragma warning disable CS8618 
    public Result()
#pragma warning restore CS8618 
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

        // ProgressStartToGenerateStartingValues = (int)hyrule.startRandomizeStartingValuesTimestamp.Subtract(hyrule.startTime).TotalMilliseconds;
        // ProgressStartingValuesToGenerateEnemies = (int)hyrule.startRandomizeEnemiesTimestamp.Subtract(hyrule.startRandomizeStartingValuesTimestamp).TotalMilliseconds;
        // ProgressGenerateEnemiesToProcessOverworld = (int)hyrule.firstProcessOverworldTimestamp.Subtract(hyrule.startRandomizeEnemiesTimestamp).TotalMilliseconds;

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

        float width, height;
        //Palace stats
        P1Size = hyrule.palaces[0].AllRooms.Count;
        width = hyrule.palaces[0].AllRooms.Max(i => i.coords.X) - hyrule.palaces[0].AllRooms.Min(i => i.coords.X);
        height = hyrule.palaces[0].AllRooms.Max(i => i.coords.Y) - hyrule.palaces[0].AllRooms.Min(i => i.coords.Y);
        P1WidthRatio = width / height;
        P1MinDistanceToBoss = StepCountToBossRoom(hyrule.palaces[0]);

        P2Size = hyrule.palaces[1].AllRooms.Count;
        width = hyrule.palaces[1].AllRooms.Max(i => i.coords.X) - hyrule.palaces[1].AllRooms.Min(i => i.coords.X);
        height = hyrule.palaces[1].AllRooms.Max(i => i.coords.Y) - hyrule.palaces[1].AllRooms.Min(i => i.coords.Y);
        P2WidthRatio = width / height;
        P2MinDistanceToBoss = StepCountToBossRoom(hyrule.palaces[1]);

        P3Size = hyrule.palaces[2].AllRooms.Count;
        width = hyrule.palaces[2].AllRooms.Max(i => i.coords.X) - hyrule.palaces[2].AllRooms.Min(i => i.coords.X);
        height = hyrule.palaces[2].AllRooms.Max(i => i.coords.Y) - hyrule.palaces[2].AllRooms.Min(i => i.coords.Y);
        P3WidthRatio = width / height;
        P3MinDistanceToBoss = StepCountToBossRoom(hyrule.palaces[2]);

        P4Size = hyrule.palaces[3].AllRooms.Count;
        width = hyrule.palaces[3].AllRooms.Max(i => i.coords.X) - hyrule.palaces[3].AllRooms.Min(i => i.coords.X);
        height = hyrule.palaces[3].AllRooms.Max(i => i.coords.Y) - hyrule.palaces[3].AllRooms.Min(i => i.coords.Y);
        P4WidthRatio = width / height;
        P4MinDistanceToBoss = StepCountToBossRoom(hyrule.palaces[3]);

        P5Size = hyrule.palaces[4].AllRooms.Count;
        width = hyrule.palaces[4].AllRooms.Max(i => i.coords.X) - hyrule.palaces[4].AllRooms.Min(i => i.coords.X);
        height = hyrule.palaces[4].AllRooms.Max(i => i.coords.Y) - hyrule.palaces[4].AllRooms.Min(i => i.coords.Y);
        P5WidthRatio = width / height;
        P5MinDistanceToBoss = StepCountToBossRoom(hyrule.palaces[4]);

        P6Size = hyrule.palaces[5].AllRooms.Count;
        width = hyrule.palaces[5].AllRooms.Max(i => i.coords.X) - hyrule.palaces[5].AllRooms.Min(i => i.coords.X);
        height = hyrule.palaces[5].AllRooms.Max(i => i.coords.Y) - hyrule.palaces[5].AllRooms.Min(i => i.coords.Y);
        P6WidthRatio = width / height;
        P6MinDistanceToBoss = StepCountToBossRoom(hyrule.palaces[5]);

        GPSize = hyrule.palaces[6].AllRooms.Count;
        width = hyrule.palaces[6].AllRooms.Max(i => i.coords.X) - hyrule.palaces[6].AllRooms.Min(i => i.coords.X);
        height = hyrule.palaces[6].AllRooms.Max(i => i.coords.Y) - hyrule.palaces[6].AllRooms.Min(i => i.coords.Y);
        GPWidthRatio = width / height;
        GPMinDistanceToBoss = StepCountToBossRoom(hyrule.palaces[6]);

    }

    //Proably the boss room min distance calculation should generate and int and then just compare so we don't have to duplicate this
    //but it is _slightly_ less efficient so fuck it
    public static int StepCountToBossRoom(Palace palace)
    {
        if (palace.Entrance == null) { throw new Exception("Palace Entrance is missing"); }
        HashSet<Room> reachedRooms = [];
        Queue<(Room, int)> roomsToCheck = [];
        roomsToCheck.Enqueue((palace.Entrance, 0));
        while (roomsToCheck.Count > 0)
        {
            var (room, stepsToRoom) = roomsToCheck.Dequeue();
            if (room.IsBossRoom)
            {
                return stepsToRoom;
            }

            // This will return false if the room is already added
            if (!reachedRooms.Add(room)) { continue; }

            int stepsToNextRoom = stepsToRoom + 1;
            if (room.Left != null) { roomsToCheck.Enqueue((room.Left, stepsToNextRoom)); }
            if (room.Right != null) { roomsToCheck.Enqueue((room.Right, stepsToNextRoom)); }
            if (room.Up != null) { roomsToCheck.Enqueue((room.Up, stepsToNextRoom)); }
            if (room.Down != null) { roomsToCheck.Enqueue((room.Down, stepsToNextRoom)); }
        }
        return int.MaxValue; // Boss room not found??
    }


}
