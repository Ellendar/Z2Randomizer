using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using SD.Tools.BCLExtensions.CollectionsRelated;
using Z2Randomizer.RandomizerCore.Enemy;

namespace Z2Randomizer.RandomizerCore.Overworld;

sealed class DeathMountain : World
{
    private readonly SortedDictionary<int, Terrain> terrains = new SortedDictionary<int, Terrain>
        {
            { RomMap.DM_CAVE1A_TILE_LOCATION, Terrain.CAVE },
            { RomMap.DM_CAVE1B_TILE_LOCATION, Terrain.CAVE },
            { RomMap.DM_CAVE2A_TILE_LOCATION, Terrain.CAVE },
            { RomMap.DM_CAVE2B_TILE_LOCATION, Terrain.CAVE },
            { RomMap.DM_CAVE3A_TILE_LOCATION, Terrain.CAVE },
            { RomMap.DM_CAVE3B_TILE_LOCATION, Terrain.CAVE },
            { RomMap.DM_CAVE4A_TILE_LOCATION, Terrain.CAVE },
            { RomMap.DM_CAVE4B_TILE_LOCATION, Terrain.CAVE },
            { RomMap.DM_CAVE5A_TILE_LOCATION, Terrain.CAVE },
            { RomMap.DM_CAVE5B_TILE_LOCATION, Terrain.CAVE },
            { RomMap.DM_CAVE6A_TILE_LOCATION, Terrain.CAVE },
            { RomMap.DM_CAVE6B_TILE_LOCATION, Terrain.CAVE },
            { RomMap.DM_CAVE7A_TILE_LOCATION, Terrain.CAVE },
            { RomMap.DM_CAVE7B_TILE_LOCATION, Terrain.CAVE },
            { RomMap.DM_CAVE8A_TILE_LOCATION, Terrain.CAVE },
            { RomMap.DM_CAVE8B_TILE_LOCATION, Terrain.CAVE },
            { RomMap.DM_CAVE9A_TILE_LOCATION, Terrain.CAVE },
            { RomMap.DM_CAVE9B_TILE_LOCATION, Terrain.CAVE },
            { RomMap.DM_CAVE10A_TILE_LOCATION, Terrain.CAVE },
            { RomMap.DM_CAVE10B_TILE_LOCATION, Terrain.CAVE },
            { RomMap.DM_CAVE11A_TILE_LOCATION, Terrain.CAVE },
            { RomMap.DM_CAVE11B_TILE_LOCATION, Terrain.CAVE },
            { RomMap.DM_CAVE12A_TILE_LOCATION, Terrain.CAVE },
            { RomMap.DM_CAVE12B_TILE_LOCATION, Terrain.CAVE },
            { RomMap.DM_CAVE13A_TILE_LOCATION, Terrain.CAVE },
            { RomMap.DM_CAVE13B_TILE_LOCATION, Terrain.CAVE },
            { RomMap.DM_CAVE14A_TILE_LOCATION, Terrain.CAVE },
            { RomMap.DM_CAVE14B_TILE_LOCATION, Terrain.CAVE },
            { RomMap.DM_HAMMER_CAVE_TILE_LOCATION, Terrain.CAVE },
            { RomMap.DM_CAVE4WAY1A_TILE_LOCATION, Terrain.CAVE },
            { RomMap.DM_CAVE4WAY1B_TILE_LOCATION, Terrain.CAVE },
            { RomMap.DM_CAVE4WAY1C_TILE_LOCATION, Terrain.CAVE },
            { RomMap.DM_CAVE4WAY1D_TILE_LOCATION, Terrain.CAVE },
            { RomMap.DM_CAVE4WAY2A_TILE_LOCATION, Terrain.CAVE },
            { RomMap.DM_CAVE4WAY2B_TILE_LOCATION, Terrain.CAVE },
            { RomMap.DM_CAVE4WAY2C_TILE_LOCATION, Terrain.CAVE },
            { RomMap.DM_CAVE4WAY2D_TILE_LOCATION, Terrain.CAVE },
            { RomMap.DM_CONTINENT_CONNECTOR1_TILE_LOCATION, Terrain.CAVE },
            { RomMap.DM_CONTINENT_CONNECTOR2_TILE_LOCATION, Terrain.CAVE },
            { RomMap.DM_SPEC_ROCK_TILE_LOCATION, Terrain.CAVE }
    };

    //Other connections are all 1:1 but DM has 4-way connectors.
    //Eventually this should go away and connections should just be a one-to-many
    public Dictionary<Location, List<Location>> connectionsDM;
    public Location hammerCave;
    public Location specRock;

    private const int MAP_ADDR = 0x7a00;

    int[][] duplicateSideviewCaves;

    public DeathMountain(RandomizerProperties props, Random r, ROM rom) : base(r)
    {
        List<Location> locations =
        [
            .. rom.LoadLocations(RomMap.DM_CAVE1A_TILE_LOCATION, 37, terrains, Continent.DM),
            // loadLocations(RomMap.DM_CONTINENT_CONNECTOR1_TILE_LOCATION, 2, terrains, Continent.DM);
            .. rom.LoadLocations(RomMap.DM_SPEC_ROCK_TILE_LOCATION, 1, terrains, Continent.DM),
        ];
        locations.ForEach(AddLocation);

        isHorizontal = props.DmIsHorizontal;
        hammerCave = GetLocationByMem(RomMap.DM_HAMMER_CAVE_TILE_LOCATION);
        specRock = GetLocationByMem(RomMap.DM_SPEC_ROCK_TILE_LOCATION);

        //reachableAreas = new HashSet<string>();
        connectionsDM = new Dictionary<Location, List<Location>>
        {
            { GetLocationByMem(RomMap.DM_CAVE1A_TILE_LOCATION), new List<Location>() { GetLocationByMem(RomMap.DM_CAVE1B_TILE_LOCATION) } },
            { GetLocationByMem(RomMap.DM_CAVE1B_TILE_LOCATION), new List<Location>() { GetLocationByMem(RomMap.DM_CAVE1A_TILE_LOCATION) } },
            { GetLocationByMem(RomMap.DM_CAVE2A_TILE_LOCATION), new List<Location>() { GetLocationByMem(RomMap.DM_CAVE2B_TILE_LOCATION) } },
            { GetLocationByMem(RomMap.DM_CAVE2B_TILE_LOCATION), new List<Location>() { GetLocationByMem(RomMap.DM_CAVE2A_TILE_LOCATION) } },
            { GetLocationByMem(RomMap.DM_CAVE3A_TILE_LOCATION), new List<Location>() { GetLocationByMem(RomMap.DM_CAVE3B_TILE_LOCATION) } },
            { GetLocationByMem(RomMap.DM_CAVE3B_TILE_LOCATION), new List<Location>() { GetLocationByMem(RomMap.DM_CAVE3A_TILE_LOCATION) } },
            { GetLocationByMem(RomMap.DM_CAVE4A_TILE_LOCATION), new List<Location>() { GetLocationByMem(RomMap.DM_CAVE4B_TILE_LOCATION) } },
            { GetLocationByMem(RomMap.DM_CAVE4B_TILE_LOCATION), new List<Location>() { GetLocationByMem(RomMap.DM_CAVE4A_TILE_LOCATION) } },
            { GetLocationByMem(RomMap.DM_CAVE5A_TILE_LOCATION), new List<Location>() { GetLocationByMem(RomMap.DM_CAVE5B_TILE_LOCATION) } },
            { GetLocationByMem(RomMap.DM_CAVE5B_TILE_LOCATION), new List<Location>() { GetLocationByMem(RomMap.DM_CAVE5A_TILE_LOCATION) } },
            { GetLocationByMem(RomMap.DM_CAVE6A_TILE_LOCATION), new List<Location>() { GetLocationByMem(RomMap.DM_CAVE6B_TILE_LOCATION) } },
            { GetLocationByMem(RomMap.DM_CAVE6B_TILE_LOCATION), new List<Location>() { GetLocationByMem(RomMap.DM_CAVE6A_TILE_LOCATION) } },
            { GetLocationByMem(RomMap.DM_CAVE7A_TILE_LOCATION), new List<Location>() { GetLocationByMem(RomMap.DM_CAVE7B_TILE_LOCATION) } },
            { GetLocationByMem(RomMap.DM_CAVE7B_TILE_LOCATION), new List<Location>() { GetLocationByMem(RomMap.DM_CAVE7A_TILE_LOCATION) } },
            { GetLocationByMem(RomMap.DM_CAVE8A_TILE_LOCATION), new List<Location>() { GetLocationByMem(RomMap.DM_CAVE8B_TILE_LOCATION) } },
            { GetLocationByMem(RomMap.DM_CAVE8B_TILE_LOCATION), new List<Location>() { GetLocationByMem(RomMap.DM_CAVE8A_TILE_LOCATION) } },
            { GetLocationByMem(RomMap.DM_CAVE9A_TILE_LOCATION), new List<Location>() { GetLocationByMem(RomMap.DM_CAVE9B_TILE_LOCATION) } },
            { GetLocationByMem(RomMap.DM_CAVE9B_TILE_LOCATION), new List<Location>() { GetLocationByMem(RomMap.DM_CAVE9A_TILE_LOCATION) } },
            { GetLocationByMem(RomMap.DM_CAVE10A_TILE_LOCATION), new List<Location>() { GetLocationByMem(RomMap.DM_CAVE10B_TILE_LOCATION) } },
            { GetLocationByMem(RomMap.DM_CAVE10B_TILE_LOCATION), new List<Location>() { GetLocationByMem(RomMap.DM_CAVE10A_TILE_LOCATION) } },
            { GetLocationByMem(RomMap.DM_CAVE11A_TILE_LOCATION), new List<Location>() { GetLocationByMem(RomMap.DM_CAVE11B_TILE_LOCATION) } },
            { GetLocationByMem(RomMap.DM_CAVE11B_TILE_LOCATION), new List<Location>() { GetLocationByMem(RomMap.DM_CAVE11A_TILE_LOCATION) } },
            { GetLocationByMem(RomMap.DM_CAVE12A_TILE_LOCATION), new List<Location>() { GetLocationByMem(RomMap.DM_CAVE12B_TILE_LOCATION) } },
            { GetLocationByMem(RomMap.DM_CAVE12B_TILE_LOCATION), new List<Location>() { GetLocationByMem(RomMap.DM_CAVE12A_TILE_LOCATION) } },
            { GetLocationByMem(RomMap.DM_CAVE13A_TILE_LOCATION), new List<Location>() { GetLocationByMem(RomMap.DM_CAVE13B_TILE_LOCATION) } },
            { GetLocationByMem(RomMap.DM_CAVE13B_TILE_LOCATION), new List<Location>() { GetLocationByMem(RomMap.DM_CAVE13A_TILE_LOCATION) } },
            { GetLocationByMem(RomMap.DM_CAVE14A_TILE_LOCATION), new List<Location>() { GetLocationByMem(RomMap.DM_CAVE14B_TILE_LOCATION) } },
            { GetLocationByMem(RomMap.DM_CAVE14B_TILE_LOCATION), new List<Location>() { GetLocationByMem(RomMap.DM_CAVE14A_TILE_LOCATION) } },

            { GetLocationByMem(RomMap.DM_CAVE4WAY1A_TILE_LOCATION), new List<Location>() { GetLocationByMem(RomMap.DM_CAVE4WAY1B_TILE_LOCATION), GetLocationByMem(RomMap.DM_CAVE4WAY1C_TILE_LOCATION), GetLocationByMem(RomMap.DM_CAVE4WAY1D_TILE_LOCATION) } },
            { GetLocationByMem(RomMap.DM_CAVE4WAY1B_TILE_LOCATION), new List<Location>() { GetLocationByMem(RomMap.DM_CAVE4WAY1A_TILE_LOCATION), GetLocationByMem(RomMap.DM_CAVE4WAY1C_TILE_LOCATION), GetLocationByMem(RomMap.DM_CAVE4WAY1D_TILE_LOCATION) } },
            { GetLocationByMem(RomMap.DM_CAVE4WAY1C_TILE_LOCATION), new List<Location>() { GetLocationByMem(RomMap.DM_CAVE4WAY1B_TILE_LOCATION), GetLocationByMem(RomMap.DM_CAVE4WAY1A_TILE_LOCATION), GetLocationByMem(RomMap.DM_CAVE4WAY1D_TILE_LOCATION) } },
            { GetLocationByMem(RomMap.DM_CAVE4WAY1D_TILE_LOCATION), new List<Location>() { GetLocationByMem(RomMap.DM_CAVE4WAY1B_TILE_LOCATION), GetLocationByMem(RomMap.DM_CAVE4WAY1C_TILE_LOCATION), GetLocationByMem(RomMap.DM_CAVE4WAY1A_TILE_LOCATION) } },

            { GetLocationByMem(RomMap.DM_CAVE4WAY2A_TILE_LOCATION), new List<Location>() { GetLocationByMem(RomMap.DM_CAVE4WAY2B_TILE_LOCATION), GetLocationByMem(RomMap.DM_CAVE4WAY2C_TILE_LOCATION), GetLocationByMem(RomMap.DM_CAVE4WAY2D_TILE_LOCATION) } },
            { GetLocationByMem(RomMap.DM_CAVE4WAY2B_TILE_LOCATION), new List<Location>() { GetLocationByMem(RomMap.DM_CAVE4WAY2A_TILE_LOCATION), GetLocationByMem(RomMap.DM_CAVE4WAY2C_TILE_LOCATION), GetLocationByMem(RomMap.DM_CAVE4WAY2D_TILE_LOCATION) } },
            { GetLocationByMem(RomMap.DM_CAVE4WAY2C_TILE_LOCATION), new List<Location>() { GetLocationByMem(RomMap.DM_CAVE4WAY2B_TILE_LOCATION), GetLocationByMem(RomMap.DM_CAVE4WAY2A_TILE_LOCATION), GetLocationByMem(RomMap.DM_CAVE4WAY2D_TILE_LOCATION) } },
            { GetLocationByMem(RomMap.DM_CAVE4WAY2D_TILE_LOCATION), new List<Location>() { GetLocationByMem(RomMap.DM_CAVE4WAY2B_TILE_LOCATION), GetLocationByMem(RomMap.DM_CAVE4WAY2C_TILE_LOCATION), GetLocationByMem(RomMap.DM_CAVE4WAY2A_TILE_LOCATION) } }
        };

        sideviewPtrTable = 0x6010;
        sideviewBank = 1;
        enemyPtr = 0x608E;
        groupedEnemies = Enemies.GroupedWestEnemies;
        overworldEncounterMaps = [
            29,     // Desert    - in vanilla 29-30 use the same table
            34, 35, // Grass     - not used in Vanilla
            39,     // Forest    - in vanilla 39-40 use the same table
            47, 48, // Swamp     - not used in Vanilla
            52,     // Graveyard - in vanilla 52-53 use the same table
            57,     // Road      - in vanilla 57-58 use the same table
        ];
        overworldEncounterMapDuplicate = [
            30, 40, 53, 58,
        ];
        nonEncounterMaps = [
            1, 2, 3, 4, 5, 6, 7, 8, // Caves
            9, 10, 11, 12, 13, 14,  // Caves
            15, 16, 17,             // Hammer Cave floor 1
            18, 19, 20, 21,         // Hammer Cave floor 2
            22, 23, 24, 25,         // Caves with elevators
            26,                     // Spectacle Rock
        ];
        // Re-used DM cave sideviews
        duplicateSideviewCaves = [
            [1, 12],
            [4, 13],
            [5, 8],
            [6, 10],
            [7, 14],
            // [23, 24], // top part of 4-way
        ];

        baseAddr = RomMap.DM_CAVE1A_TILE_LOCATION;
        continentId = Continent.DM;
        VANILLA_MAP_ADDR = 0x665c;

        biome = props.DmBiome switch
        {
            Biome.DRY_CANYON => Biome.CANYON,
            _ => props.DmBiome
        };
        if (biome == Biome.VANILLA || biome == Biome.VANILLA_SHUFFLE)
        {
            MAP_COLS = 64;
            MAP_ROWS = 75;
            northSouthEncounterSeparator = 45;
        }
        else
        {
            var meta = props.DmSize.GetMeta();
            MAP_COLS = meta.Width;
            MAP_ROWS = meta.Height;
            northSouthEncounterSeparator = MAP_ROWS / 2;

            if (biome == Biome.CALDERA) // Caldera won't work with less
            {
                MAP_COLS = Math.Max(MAP_COLS, 35);
                MAP_ROWS = Math.Max(MAP_ROWS, 35);
            }

            // TODO: use metadata for num caves to remove
            int connectorPairsToRemove = props.DmSize switch
            {
                DmSizeOption.LARGE => 0,
                DmSizeOption.MEDIUM => 0,
                DmSizeOption.SMALL => 9,
                DmSizeOption.TINY => 13,
                _ => throw new NotImplementedException(),
            };

            int removedCavePairs = 0;
            if (props.DmSize != DmSizeOption.LARGE)
            {
                // remove duplicate (by sideview) caves
                foreach (int[] dupeMaps in duplicateSideviewCaves)
                {
                    int keepIndex = r.Next(dupeMaps.Length);
                    for (int index = 0; index < dupeMaps.Length; index++)
                    {
                        if (index != keepIndex)
                        {
                            var map = dupeMaps[index];
                            Location entrance = connectionsDM.Keys.FirstOrDefault(loc => loc.Map == map)!;
                            Debug.Assert(entrance != null);
                            List<Location> otherSideEntrance = connectionsDM[entrance];
                            Debug.Assert(otherSideEntrance.Count == 1);
                            RemoveLocationsDM([entrance, .. otherSideEntrance]);
                            removedCavePairs++;
                        }
                    }
                }
            }

            int maxElevatorConnectorsToRemove = 1;
            int elevatorConnectorsRemoved = 0;
            for (; removedCavePairs < connectorPairsToRemove; removedCavePairs++)
            {
                KeyValuePair<Location, List<Location>> removeLocPair;
                do
                {
                    var j = r.Next(connectionsDM.Count);
                    removeLocPair = connectionsDM.ElementAt(j);
                } while (removeLocPair.Value.Count > 1 && elevatorConnectorsRemoved++ >= maxElevatorConnectorsToRemove);
                RemoveLocationsDM([removeLocPair.Key, .. removeLocPair.Value]);
            }
        }

        walkableTerrains = new List<Terrain>() { Terrain.DESERT, Terrain.FOREST, Terrain.GRAVE };
        randomTerrainFilter = new List<Terrain>() { Terrain.DESERT, Terrain.FOREST, Terrain.GRAVE, Terrain.MOUNTAIN, Terrain.WALKABLEWATER, Terrain.WATER };

        climate = Climates.Create(props.DmClimate);
        climate.SeedTerrainCount = Math.Min(climate.SeedTerrainCount, biome.SeedTerrainLimit());
        SetVanillaCollectables(props.ReplaceFireWithDash);
    }

    private void RemoveLocationsDM(ICollection<Location> locations)
    {
        RemoveLocations(locations);
        foreach (var loc in locations)
        {
            connectionsDM.Remove(loc);
        }
    }

    public override bool Terraform(RandomizerProperties props, ROM rom)
    {
        Terrain effectiveWater = Terrain.WATER, preplacedWater = Terrain.PREPLACED_WATER;
        if (props.CanWalkOnWaterWithBoots)
        {
            effectiveWater = Terrain.WALKABLEWATER;
            preplacedWater = Terrain.PREPLACED_WATER_WALKABLE;
        }
        walkableTerrains = new List<Terrain>() { Terrain.DESERT, Terrain.FOREST, Terrain.GRAVE };
        randomTerrainFilter = new List<Terrain>() { Terrain.DESERT, Terrain.FOREST, Terrain.GRAVE, Terrain.MOUNTAIN, effectiveWater };

        foreach (Location location in AllLocations)
        {
            location.CanShuffle = true;
        }
        if (biome == Biome.VANILLA || biome == Biome.VANILLA_SHUFFLE)
        {
            Debug.Assert(MAP_ROWS == 75);
            Debug.Assert(MAP_COLS == 64);
            map = rom.ReadVanillaMap(rom, VANILLA_MAP_ADDR, MAP_ROWS, MAP_COLS);
            if (biome == Biome.VANILLA_SHUFFLE)
            {
                ShuffleLocations(AllLocations);
                if (props.VanillaShuffleUsesActualTerrain)
                {
                    specRock.TerrainType = Terrain.ROCK;
                    foreach (Location location in AllLocations)
                    {
                        map[location.Y, location.Xpos] = location.TerrainType;
                    }
                }
                foreach (Location location in Locations[Terrain.CAVE])
                {
                    location.PassThrough = 0;
                }
                foreach (Location location in Locations[Terrain.TOWN])
                {
                    location.PassThrough = 0;
                }
                foreach (Location location in Locations[Terrain.PALACE])
                {
                    location.PassThrough = 0;
                }
            }
        }
        else //Not vanilla/shuffle
        {
            int bytesWritten = 2000;
            while (bytesWritten > MAP_SIZE_BYTES)
            {
                map = new Terrain[MAP_ROWS, MAP_COLS];
                Terrain riverT = Terrain.MOUNTAIN;
                if (biome != Biome.CANYON && biome != Biome.DRY_CANYON && biome != Biome.CALDERA && biome != Biome.ISLANDS)
                {
                    int colsBeforeWater = Math.Min(MAP_COLS, 29);
                    for (int i = 0; i < MAP_ROWS; i++)
                    {
                        for (int j = 0; j < colsBeforeWater; j++)
                        {
                            map[i, j] = Terrain.NONE;
                        }
                        for (int j = 29; j < MAP_COLS; j++)
                        {
                            map[i, j] = preplacedWater;
                        }
                    }
                }
                else
                {
                    for (int i = 0; i < MAP_ROWS; i++)
                    {
                        for (int j = 0; j < MAP_COLS; j++)
                        {
                            map[i, j] = Terrain.NONE;
                        }
                    }
                }

                if (biome == Biome.ISLANDS)
                {
                    riverT = preplacedWater;
                    for (int i = 0; i < MAP_COLS; i++)
                    {
                        map[0, i] = preplacedWater;
                        map[MAP_ROWS - 1, i] = preplacedWater;
                    }

                    for (int i = 0; i < MAP_ROWS; i++)
                    {
                        map[i, 0] = preplacedWater;
                        map[i, MAP_COLS - 1] = preplacedWater;
                    }

                    int cols = RNG.Next(2, 4);
                    int rows = RNG.Next(2, 4);
                    List<int> pickedC = [];
                    List<int> pickedR = [];

                    while (cols > 0)
                    {
                        int col = RNG.Next(1, MAP_COLS);
                        if (!pickedC.Contains(col))
                        {
                            for (int i = 0; i < MAP_ROWS; i++)
                            {
                                if (map[i, col] == Terrain.NONE)
                                {
                                    map[i, col] = preplacedWater;
                                }
                            }
                            pickedC.Add(col);
                            cols--;
                        }
                    }

                    while (rows > 0)
                    {
                        int row = RNG.Next(5, MAP_ROWS - 6);
                        if (!pickedR.Contains(row))
                        {
                            for (int i = 0; i < MAP_COLS; i++)
                            {
                                if (map[row, i] == Terrain.NONE)
                                {
                                    map[row, i] = preplacedWater;
                                }
                                else if (map[row, i] == preplacedWater)
                                {
                                    int adjust = RNG.Next(-3, 4);
                                    while (row + adjust < 1 || row + adjust > MAP_ROWS - 2)
                                    {
                                        adjust = RNG.Next(-3, 4);
                                    }
                                    row += adjust;
                                }
                            }
                            pickedR.Add(row);
                            rows--;
                        }
                    }



                }
                else if (biome == Biome.CANYON || biome == Biome.DRY_CANYON)
                {
                    //TODO: ??? Maybe not ???
                    riverT = preplacedWater;

                    walkableTerrains.Clear();
                    walkableTerrains.Add(Terrain.GRASS);
                    walkableTerrains.Add(Terrain.GRAVE);
                    walkableTerrains.Add(Terrain.FOREST);
                    walkableTerrains.Add(Terrain.DESERT);
                    walkableTerrains.Add(Terrain.MOUNTAIN);

                    randomTerrainFilter.Remove(Terrain.SWAMP);
                    DrawCanyon(riverT);
                    //Debug.WriteLine(GetMapDebug());
                    walkableTerrains.Remove(Terrain.MOUNTAIN);

                    randomTerrainFilter.Remove(Terrain.SWAMP);
                    //this.randomTerrains.Add(terrain.lava);

                }
                else if (biome == Biome.CALDERA)
                {
                    DrawCenterMountain();
                }
                else if (biome == Biome.MOUNTAINOUS)
                {
                    riverT = Terrain.MOUNTAIN;
                    for (int i = 0; i < MAP_COLS; i++)
                    {
                        map[0, i] = Terrain.MOUNTAIN;
                        map[MAP_ROWS - 1, i] = Terrain.MOUNTAIN;
                    }

                    for (int i = 0; i < MAP_ROWS; i++)
                    {
                        map[i, 0] = Terrain.MOUNTAIN;
                        map[i, MAP_COLS - 1] = Terrain.MOUNTAIN;
                    }


                    int cols = RNG.Next(2, 4);
                    int rows = RNG.Next(2, 4);
                    List<int> pickedC = new List<int>();
                    List<int> pickedR = new List<int>();

                    while (cols > 0)
                    {
                        int col = RNG.Next(10, MAP_COLS - 11);
                        if (!pickedC.Contains(col))
                        {
                            for (int i = 0; i < MAP_ROWS; i++)
                            {
                                if (map[i, col] == Terrain.NONE)
                                {
                                    map[i, col] = Terrain.MOUNTAIN;
                                }
                            }
                            pickedC.Add(col);
                            cols--;
                        }
                    }

                    while (rows > 0)
                    {
                        int row = RNG.Next(10, MAP_ROWS - 11);
                        if (!pickedR.Contains(row))
                        {
                            for (int i = 0; i < MAP_COLS; i++)
                            {
                                if (map[row, i] == Terrain.NONE)
                                {
                                    map[row, i] = Terrain.MOUNTAIN;
                                }
                            }
                            pickedR.Add(row);
                            rows--;
                        }
                    }
                }

                Direction raftDirection = DirectionExtensions.RandomCardinal(RNG);
                if (biome == Biome.CANYON || biome == Biome.DRY_CANYON || biome == Biome.CALDERA)
                {
                    raftDirection = props.DmIsHorizontal ? DirectionExtensions.RandomHorizontal(RNG) : DirectionExtensions.RandomVertical(RNG);
                }
                if (raft != null)
                {
                    DrawOcean(raftDirection, preplacedWater);
                }

                Direction bridgeDirection;
                do
                {
                    if (biome != Biome.CANYON && biome != Biome.DRY_CANYON && biome != Biome.CALDERA)
                    {
                        bridgeDirection = DirectionExtensions.RandomCardinal(RNG);
                    }
                    else
                    {
                        bridgeDirection = props.DmIsHorizontal ? DirectionExtensions.RandomHorizontal(RNG) : DirectionExtensions.RandomVertical(RNG);
                    }
                } while (bridgeDirection == raftDirection);
                if (bridge != null)
                {
                    DrawOcean(bridgeDirection, preplacedWater);
                }
                int x = 0;
                int y = 0;
                foreach (Location location in AllLocations)
                {
                    if (location.TerrainType != Terrain.BRIDGE && location != specRock && location.CanShuffle)
                    {
                        int tries = 0;
                        do
                        {
                            x = RNG.Next(MAP_COLS - 2) + 1;
                            y = RNG.Next(MAP_ROWS - 2) + 1;
                            tries++;
                            if (++tries >= 100)
                            {
                                logger.LogDebug($"Could not find empty 3x3 spot for location {location.Name}");
                                return false;
                            }
                        } while (!AllTerrainIn3x3Equals(x, y, Terrain.NONE));

                        map[y, x] = location.TerrainType;
                        location.Xpos = x;
                        location.Y = y;
                        if (location.TerrainType == Terrain.CAVE)
                        {
                            var f = TerraformCaveExpansion(props, ref x, ref y, location);
                            if (!f)
                            {
                                logger.LogDebug($"TerraformCaveExpansion failed for {location.Name}");
                                return false;
                            }
                        }
                    }
                }

                if (biome == Biome.VANILLALIKE)
                {
                    PlaceRandomTerrain(climate, 5);
                }
                randomTerrainFilter.Add(Terrain.ROAD);

                Climate growthClimate = climate.Clone();
                float dmOpennessFactor = biome switch
                {
                    Biome.CANYON => (float)(RNG.NextDouble() * .75 + 1),
                    Biome.ISLANDS => (float)(RNG.NextDouble() * .5 + 1),
                    _ => 1f
                };
                growthClimate.ApplyDeathMountainSafety(randomTerrainFilter, dmOpennessFactor);
                Debug.WriteLine(GetMapDebug());
                if (!GrowTerrain(growthClimate))
                {
                    logger.LogDebug("GrowTerrain failed");
                    return false;
                }
                Debug.WriteLine(GetMapDebug());
                if (biome == Biome.CALDERA)
                {
                    bool f = MakeCaldera(props.CanWalkOnWaterWithBoots);
                    if (!f)
                    {
                        logger.LogDebug("MakeCaldera failed");
                        return false;
                    }
                }
                walkableTerrains.Add(Terrain.ROAD);
                if (raft != null)
                {
                    bool r = DrawRaft(raftDirection);
                    if (!r)
                    {
                        logger.LogDebug("DrawRaft failed");
                        return false;
                    }
                }

                if (bridge != null)
                {
                    bool b = DrawBridge(bridgeDirection);
                    if (!b)
                    {
                        logger.LogDebug("DrawBridge failed");
                        return false;
                    }
                }


                do
                {
                    x = RNG.Next(MAP_COLS - 2) + 1;
                    y = RNG.Next(MAP_ROWS - 2) + 1;
                } while (!walkableTerrains.Contains(map[y, x]) || map[y + 1, x] == Terrain.CAVE || map[y - 1, x] == Terrain.CAVE || map[y, x + 1] == Terrain.CAVE || map[y, x - 1] == Terrain.CAVE);

                map[y, x] = Terrain.ROCK;
                specRock.Y = y;
                specRock.Xpos = x;


                if (biome == Biome.CANYON || biome == Biome.DRY_CANYON || biome == Biome.ISLANDS)
                {
                    ConnectIslands(25, false, riverT, false, false, false, false, props.CanWalkOnWaterWithBoots, biome);
                }

                if (!ValidateCaves())
                {
                    logger.LogDebug("ValidateCaves failed");
                    return false;
                }

                //check bytes and adjust
                bytesWritten = WriteMapToRom(rom, false, MAP_ADDR, MAP_SIZE_BYTES, 0, 0, props.HiddenPalace, props.HiddenKasuto);
            }
        }
        if(!ValidateBasicRouting())
        {
            logger.LogDebug("ValidateBasicRouting failed");
            return false;
        }

        WriteMapToRom(rom, true, MAP_ADDR, MAP_SIZE_BYTES, 0, 0, props.HiddenPalace, props.HiddenKasuto);
        rom.Put(RomMap.NORTH_SOUTH_SEPARATOR_DM, (byte)(northSouthEncounterSeparator + 30));

        visitation = new bool[MAP_ROWS, MAP_COLS];
        for (int i = 0; i < MAP_ROWS; i++)
        {
            for (int j = 0; j < MAP_COLS; j++)
            {
                visitation[i, j] = false;
            }
        }

        for (int i = RomMap.DM_CAVE1A_TILE_LOCATION; i < 0x614B; i++)
        {
            if (!terrains.Keys.Contains(i))
            {
                rom.Put(i, 0x00);
            }
        }
        return true;
    }

    private bool TerraformCaveExpansion(RandomizerProperties props, ref int x, ref int y, Location location)
    {
        Direction direction = (Direction)RNG.Next(4);

        Terrain s = biome == Biome.VANILLALIKE ? Terrain.ROAD : climate.GetRandomTerrain(RNG, walkableTerrains);
        int tries;

        if (props.SaneCaves && connectionsDM.ContainsKey(location))
        {
            if ((location.MapPage == 0 || location.FallInHole != 0) && location.ForceEnterRight == 0)
            {
                if (direction == Direction.NORTH)
                {
                    direction = Direction.SOUTH;
                }

                if (direction == Direction.WEST)
                {
                    direction = Direction.EAST;
                }
            }
            else
            {
                if (direction == Direction.SOUTH)
                {
                    direction = Direction.NORTH;
                }

                if (direction == Direction.EAST)
                {
                    direction = Direction.WEST;
                }
            }
            map[y, x] = Terrain.NONE;

            tries = 0;
            do
            {
                x = RNG.Next(MAP_COLS - 2) + 1;
                y = RNG.Next(MAP_ROWS - 2) + 1;
                if (++tries >= 100)
                {
                    return false;
                }
            } while (x < 5 || x > MAP_COLS - 5
                  || y < 5 || y > MAP_ROWS - 5
                  || !AllTerrainIn3x3Equals(x, y, Terrain.NONE));

            int minDistX = Math.Min(MAP_COLS / 2 - 1, 15);
            int minDistY = Math.Min(MAP_ROWS / 2 - 1, 15);

            while ((direction == Direction.NORTH && y < minDistY)
                || (direction == Direction.EAST && x > MAP_COLS - minDistX)
                || (direction == Direction.SOUTH && y > MAP_ROWS - minDistY)
                || (direction == Direction.WEST && x < minDistX))
            {
                direction = (Direction)RNG.Next(4);
            }
            if (connectionsDM[location].Count == 1)
            {
                int otherx = 0;
                int othery = 0;
                tries = 0;
                do
                {
                    int range = 7;
                    int offset = 3;
                    if (biome == Biome.ISLANDS)
                    {
                        range = 7;
                        offset = 5;
                    }
                    if (direction == Direction.NORTH)
                    {
                        otherx = x + (RNG.Next(7) - 3);
                        othery = y - (RNG.Next(range) + offset);
                    }
                    else if (direction == Direction.EAST)
                    {
                        otherx = x + (RNG.Next(range) + offset);
                        othery = y + (RNG.Next(7) - 3);
                    }
                    else if (direction == Direction.SOUTH)
                    {
                        otherx = x + (RNG.Next(7) - 3);
                        othery = y + (RNG.Next(range) + offset);
                    }
                    else //west
                    {
                        otherx = x - (RNG.Next(range) + offset);
                        othery = y + (RNG.Next(7) - 3);
                    }
                    if (++tries >= 100)
                    {
                        return false;
                    }
                } while (otherx <= 1 || otherx >= MAP_COLS - 1
                      || othery <= 1 || othery >= MAP_ROWS - 1
                      || !AllTerrainIn3x3Equals(otherx, othery, Terrain.NONE));

                List<Location> l2 = connectionsDM[location];
                var location2 = l2[0];
                location.CanShuffle = false;
                location.Xpos = x;
                location.Y = y;
                location2.CanShuffle = false;
                location2.Xpos = otherx;
                location2.Y = othery;
                PlaceCave(x, y, direction, s);
                PlaceCave(otherx, othery, direction.Reverse(), s);
                AlignCavePositionsLeftToRight(direction, location, location2);
            }
            else //4-way caves
            {
                int otherx = 0;
                int othery = 0;
                tries = 0;
                do
                {
                    int range = 7;
                    int offset = 3;
                    if (biome == Biome.ISLANDS)
                    {
                        range = 7;
                        offset = 5;
                    }
                    if (direction == Direction.NORTH)
                    {
                        otherx = x + (RNG.Next(7) - 3);
                        othery = y - (RNG.Next(range) + offset);
                    }
                    else if (direction == Direction.EAST)
                    {
                        otherx = x + (RNG.Next(range) + offset);
                        othery = y + (RNG.Next(7) - 3);
                    }
                    else if (direction == Direction.SOUTH)
                    {
                        otherx = x + (RNG.Next(7) - 3);
                        othery = y + (RNG.Next(range) + offset);
                    }
                    else //west
                    {
                        otherx = x - (RNG.Next(range) + offset);
                        othery = y + (RNG.Next(7) - 3);
                    }
                    if (++tries >= 100)
                    {
                        return false;
                    }
                } while (otherx <= 1 || otherx >= MAP_COLS - 1
                      || othery <= 1 || othery >= MAP_ROWS - 1
                      || !AllTerrainIn3x3Equals(otherx, othery, Terrain.NONE));

                List<Location> caveExits = connectionsDM[location];
                var location2 = caveExits[0];
                var location3 = caveExits[1];
                var location4 = caveExits[2];
                location.CanShuffle = false;
                location.Xpos = x;
                location.Y = y;
                location2.CanShuffle = false;
                location2.Xpos = otherx;
                location2.Y = othery;
                PlaceCave(x, y, direction, s);
                PlaceCave(otherx, othery, direction.Reverse(), s);
                AlignCavePositionsLeftToRight(direction, location, location2);

                int newx = 0;
                int newy = 0;
                tries = 0;
                do
                {
                    newx = x + RNG.Next(7) - 3;
                    newy = y + RNG.Next(7) - 3;
                    if (++tries >= 100)
                    {
                        return false;
                    }
                } while (newx > 2 && newx < MAP_COLS - 2
                      && newy > 2 && newy < MAP_ROWS - 2
                      && !AllTerrainIn3x3Equals(newx, newy, Terrain.NONE));

                location3.CanShuffle = false;
                location3.Xpos = newx;
                location3.Y = newy;
                PlaceCave(newx, newy, direction, s);

                y = newy;
                x = newx;
                tries = 0;
                do
                {
                    int range = 7;
                    int offset = 3;
                    if (biome == Biome.ISLANDS)
                    {
                        range = 7;
                        offset = 5;
                    }

                    if (direction == Direction.NORTH)
                    {
                        otherx = x + (RNG.Next(7) - 3);
                        othery = y - (RNG.Next(range) + offset);
                    }
                    else if (direction == Direction.EAST)
                    {
                        otherx = x + (RNG.Next(range) + offset);
                        othery = y + (RNG.Next(7) - 3);
                    }
                    else if (direction == Direction.SOUTH)
                    {
                        otherx = x + (RNG.Next(7) - 3);
                        othery = y + (RNG.Next(range) + offset);
                    }
                    else //west
                    {
                        otherx = x - (RNG.Next(range) + offset);
                        othery = y + (RNG.Next(7) - 3);
                    }
                    if (++tries >= 100)
                    {
                        return false;
                    }
                } while (otherx <= 1 || otherx >= MAP_COLS - 1
                      || othery <= 1 || othery >= MAP_ROWS - 1
                      || !AllTerrainIn3x3Equals(otherx, othery, Terrain.NONE));

                location4.CanShuffle = false;
                location4.Xpos = otherx;
                location4.Y = othery;
                PlaceCave(otherx, othery, direction.Reverse(), s);
                AlignCavePositionsLeftToRight(direction, location3, location4);
            }
        }
        else
        {
            PlaceCave(x, y, direction, s);
        }
        return true;
    }

    private bool MakeCaldera(bool canWalkOnWaterWithBoots)
    {
        Terrain water = Terrain.WATER;
        if (canWalkOnWaterWithBoots)
        {
            water = Terrain.WALKABLEWATER;
        }
        int mapCenterX = MAP_COLS / 2; // 32
        int mapCenterY = MAP_ROWS / 2; // 22
        int calderaCenterX, calderaCenterY;

        int tries = 0;
        bool placeable;
        do
        {
            if (isHorizontal)
            {
                int minX = mapCenterX - 5;
                int maxX = mapCenterX + 5;
                calderaCenterX = RNG.Next(minX, maxX);
                int minY = Math.Max(7, mapCenterY - 5);
                int maxY = Math.Min(mapCenterY + 5, MAP_ROWS - 8);
                calderaCenterY = RNG.Next(minY, maxY);
            }
            else
            {
                int minX = Math.Max(7, mapCenterX - 11);
                int maxX = Math.Min(mapCenterX + 9, MAP_COLS - 8);
                calderaCenterX = RNG.Next(minX, maxX);
                int minY = mapCenterY - 5;
                int maxY = mapCenterY + 5;
                calderaCenterY = RNG.Next(minY, maxY);
            }
            placeable = true;
            for (int i = calderaCenterY - 7; i < calderaCenterY + 8; i++)
            {
                for (int j = calderaCenterX - 7; j < calderaCenterX + 8; j++)
                {
                    if (map[i, j] != Terrain.MOUNTAIN)
                    {
                        placeable = false;
                        break; // end inner for
                    }
                }
                if (!placeable)
                {
                    break; // end outer for
                }
            }
            if (++tries == 1000)
            {
                return false;
            }
        } while (!placeable);

        int startx = calderaCenterX - 5;
        int starty = calderaCenterY;
        int deltax = 1;
        int deltay = 0;
        if (!isHorizontal)
        {
            startx = calderaCenterX;
            starty = calderaCenterY - 5;
            deltax = 0;
            deltay = 1;
        }
        for (int i = 0; i < 10; i++)
        {
            int lake = RNG.Next(7, 11);
            if (i == 0 || i == 9)
            {
                lake = RNG.Next(3, 6);
            }
            if (isHorizontal)
            {
                for (int j = 0; j < lake / 2; j++)
                {
                    map[starty + j, startx] = water;
                    if (i == 0)
                    {
                        map[starty + j, startx - 1] = Terrain.FOREST;
                    }
                    if (i == 9)
                    {
                        map[starty + j, startx + 1] = Terrain.FOREST;
                    }

                }
                int top = starty + lake / 2;
                while (map[top, startx - 1] == Terrain.MOUNTAIN)
                {
                    map[top, startx - 1] = Terrain.FOREST;
                    top--;
                }
                top = starty + lake / 2;
                while (map[top, startx - 1] != Terrain.MOUNTAIN)
                {
                    map[top, startx] = Terrain.FOREST;
                    top++;
                }

                for (int j = 0; j < lake - (lake / 2); j++)
                {
                    map[starty - j, startx] = water;
                    if (i == 0)
                    {
                        map[starty - j, startx - 1] = Terrain.FOREST;
                    }
                    if (i == 9)
                    {
                        map[starty - j, startx + 1] = Terrain.FOREST;
                    }

                }
                top = starty - (lake - (lake / 2));
                while (map[top, startx - 1] == Terrain.MOUNTAIN)
                {
                    map[top, startx - 1] = Terrain.FOREST;
                    top++;
                }
                top = starty - (lake - (lake / 2));
                while (map[top, startx - 1] != Terrain.MOUNTAIN)
                {
                    map[top, startx] = Terrain.FOREST;
                    top--;
                }

                //map[starty + lake / 2, startx] = terrain.forest;
                // map[starty - (lake - (lake / 2)), startx] = terrain.forest;
                if (i == 0)
                {
                    map[starty + lake / 2, startx + 1] = Terrain.FOREST;
                    map[starty - (lake - (lake / 2)), startx - 1] = Terrain.FOREST;
                }
                if (i == 9)
                {
                    map[starty + lake / 2, startx + 1] = Terrain.FOREST;
                    map[starty - (lake - (lake / 2)), startx + 1] = Terrain.FOREST;
                }

            }
            else
            {
                for (int j = 0; j < lake / 2; j++)
                {
                    map[starty, startx + j] = water;
                    if (i == 0)
                    {
                        map[starty - 1, startx + j] = Terrain.FOREST;
                    }
                    if (i == 9)
                    {
                        map[starty + 1, startx + j] = Terrain.FOREST;
                    }
                }
                int top = startx + lake / 2;
                while (map[starty - 1, top] == Terrain.MOUNTAIN && i != 0)
                {
                    map[starty - 1, top] = Terrain.FOREST;
                    top--;
                }
                top = startx + lake / 2;
                while (map[starty - 1, top] != Terrain.MOUNTAIN && i != 0)
                {
                    map[starty, top] = Terrain.FOREST;
                    top++;
                }

                for (int j = 0; j < lake - (lake / 2); j++)
                {
                    map[starty, startx - j] = water;
                    if (i == 0)
                    {
                        map[starty - 1, startx - j] = Terrain.FOREST;
                    }
                    if (i == 9)
                    {
                        map[starty + 1, startx - j] = Terrain.FOREST;
                    }
                }
                top = startx - (lake - (lake / 2));
                while (map[starty - 1, top] == Terrain.MOUNTAIN && i != 0)
                {
                    map[starty - 1, top] = Terrain.FOREST;
                    top++;
                }
                top = startx - (lake - (lake / 2));
                while (map[starty - 1, top] != Terrain.MOUNTAIN && i != 0)
                {
                    map[starty, top] = Terrain.FOREST;
                    top--;
                }
                //map[starty, startx + lake / 2] = terrain.forest;
                //map[starty, startx - (lake - (lake / 2))] = terrain.forest;
                if (i == 0)
                {
                    map[starty - 1, startx + lake / 2] = Terrain.FOREST;
                    map[starty - 1, startx - (lake - (lake / 2))] = Terrain.FOREST;
                }
                if (i == 9)
                {
                    map[starty + 1, startx + lake / 2] = Terrain.FOREST;
                    map[starty + 1, startx - (lake - (lake / 2))] = Terrain.FOREST;
                }
            }
            startx += deltax;
            starty += deltay;
        }

        // caves have already been shuffled by being placed on the map.
        // pick the closest ones to the caldera center to minimize having
        // overlap errors.
        var validCaves = connectionsDM
            .Where(kvp => kvp.Value.Count == 1)
            .OrderBy(kvp => DistanceSquared(kvp.Key, calderaCenterX, calderaCenterY))
            .ToList();
        var first = validCaves[0];
        var second = validCaves.First(kvp => kvp.Key != first.Key && kvp.Key != first.Value[0]);
        Location cave1l = first.Key;
        Location cave1r = first.Value[0];
        Location cave2l = second.Key;
        Location cave2r = second.Value[0];

        cave1l.CanShuffle = false;
        cave1r.CanShuffle = false;
        cave2l.CanShuffle = false;
        cave2r.CanShuffle = false;
        map[cave1l.Y, cave1l.Xpos] = Terrain.MOUNTAIN;
        map[cave2l.Y, cave2l.Xpos] = Terrain.MOUNTAIN;
        map[cave1r.Y, cave1r.Xpos] = Terrain.MOUNTAIN;
        map[cave2r.Y, cave2r.Xpos] = Terrain.MOUNTAIN;

        int caveDirection = RNG.Next(2);
        if (isHorizontal)
        {
            bool f = HorizontalCave(caveDirection, calderaCenterX, calderaCenterY, cave1l, cave1r);
            if (!f)
            {
                return false;
            }
            caveDirection = 1 - caveDirection;
            f = HorizontalCave(caveDirection, calderaCenterX, calderaCenterY, cave2l, cave2r);
            if (!f)
            {
                return false;
            }
        }
        else
        {
            bool f = VerticalCave(caveDirection, calderaCenterX, calderaCenterY, cave1l, cave1r);
            if (!f)
            {
                return false;
            }
            caveDirection = 1 - caveDirection;
            f = VerticalCave(caveDirection, calderaCenterX, calderaCenterY, cave2l, cave2r);
            if (!f)
            {
                return false;
            }
            
        }
        return true;
    }

    /// <summary>
    /// Updates the visitation matrix and location reachability 
    /// </summary>
    public override void UpdateVisit(List<RequirementType> requireables)
    {
        UpdateReachable(requireables);

        foreach (Location location in AllLocations)
        {
            if (location.Y > 0 && visitation[location.Y, location.Xpos])
            {
                if (location.AccessRequirements.AreSatisfiedBy(requireables))
                {
                    location.Reachable = true;
                    if (connectionsDM.ContainsKey(location) && location.ConnectionRequirements.AreSatisfiedBy(requireables))
                    {
                        foreach (Location connectedLocation in connectionsDM[location])
                        {
                            connectedLocation.Reachable = true;
                            visitation[connectedLocation.Y, connectedLocation.Xpos] = true;
                        }
                    }
                }
            }
        }
    }

    protected override List<Location> GetPathingStarts()
    {
        return 
            connectionsDM.Keys.Where(i => i.Reachable)
            .Union(GetContinentConnections().Where(i => i.Reachable))
            .ToList();
    }

    /// <summary>
    /// Determines if all locations on this continent could be reached, assuming you had everything and could reach every entrance.
    /// If the answer is no, there's no point in bothering with the rest of a world generation.
    /// </summary>
    /// <returns></returns>
    public bool ValidateBasicRouting()
    {
        List<Location> unreachedLocations = RequiredLocations(false, false).ToList();

        bool[,] visitedCoordinates = new bool[MAP_ROWS, MAP_COLS];
        List<(int, int)> pendingCoordinates = new();
        foreach(Location location in GetContinentConnections())
        {
            pendingCoordinates.Add((location.Y, location.Xpos));
        }
        int y, x;
        do
        {
            (int, int) coordinate = pendingCoordinates.First();
            y = coordinate.Item1;
            x = coordinate.Item2;
            pendingCoordinates.Remove(coordinate);
            if(visitedCoordinates[y, x])
            {
                continue;
            }
            visitedCoordinates[y, x] = true;
            //if there is a location at this coordinate
            Location? here = unreachedLocations.FirstOrDefault(location => location.Y == y && location.Xpos == x);
            if (here != null)
            {
                //it's reachable
                unreachedLocations.Remove(here);
                //if it's a connection cave, add the exit(s) to the pending locations
                if(connectionsDM.ContainsKey(here))
                {
                    connectionsDM[here].ForEach(i => pendingCoordinates.Add((i.Y, i.Xpos)));
                }
            }

            //for each adjacent direction, if it's not off the map, and it's potentially walkable terrain, crawl it
            if(x > 0 && map[y, x - 1].IsWalkable())
            {
                pendingCoordinates.Add((y, x - 1));
            }
            if (x < MAP_COLS - 1 && map[y, x + 1].IsWalkable())
            {
                pendingCoordinates.Add((y, x + 1));
            }
            if (y > 0 && map[y - 1, x].IsWalkable())
            {
                pendingCoordinates.Add((y - 1, x));
            }
            if (y < MAP_ROWS - 1 && map[y + 1, x].IsWalkable())
            {
                pendingCoordinates.Add((y + 1, x));
            }
        } while (pendingCoordinates.Count > 0);

        return !unreachedLocations.Any();
    }

    public override string GetName()
    {
        return "Death Mountain";
    }

    public override IEnumerable<Location> RequiredLocations(bool hiddenPalace, bool hiddenKasuto)
    {
        HashSet<Location> requiredLocations = new()
        {
            hammerCave,
            specRock
        };
        requiredLocations.AddRange(connectionsDM.Keys);
        requiredLocations.AddRange(GetContinentConnections());

        return requiredLocations.Where(i => i != null);
    }

    protected override void SetVanillaCollectables(bool useDash)
    {
        hammerCave.VanillaCollectable = Collectable.HAMMER;
        specRock.VanillaCollectable = Collectable.MAGIC_CONTAINER;
    }

    public override string GenerateSpoiler()
    {
        StringBuilder sb = new();
        sb.AppendLine("DEATH MOUNTAIN: ");
        sb.AppendLine("\tHammer Cave: " + hammerCave.Collectables[0].EnglishText());
        sb.AppendLine("\tSpec Rock: " + specRock.Collectables[0].EnglishText());

        sb.AppendLine();
        return sb.ToString();
    }
}
