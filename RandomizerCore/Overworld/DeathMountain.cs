using System;
using System.Collections.Generic;
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

    public Dictionary<Location, List<Location>> connectionsDM;
    public Location hammerCave;
    public Location specRock;

    private const int MAP_ADDR = 0x7a00;

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

        MAP_ROWS = 45;
        MAP_COLS = 64;

        baseAddr = RomMap.DM_CAVE1A_TILE_LOCATION;
        continentId = Continent.DM;
        VANILLA_MAP_ADDR = 0x665c;

        biome = props.DmBiome switch
        {
            Biome.DRY_CANYON => Biome.CANYON,
            _ => props.DmBiome
        };

        walkableTerrains = new List<Terrain>() { Terrain.DESERT, Terrain.FOREST, Terrain.GRAVE };
        randomTerrainFilter = new List<Terrain>() { Terrain.DESERT, Terrain.FOREST, Terrain.GRAVE, Terrain.MOUNTAIN, Terrain.WALKABLEWATER, Terrain.WATER };

        climate = Climates.Create(props.DmClimate);
        climate.SeedTerrainCount = Math.Min(climate.SeedTerrainCount, biome.SeedTerrainLimit());
        SetVanillaCollectables(props.ReplaceFireWithDash);
    }

    public override bool Terraform(RandomizerProperties props, ROM rom)
    {
        Terrain water = Terrain.WATER;
        if (props.CanWalkOnWaterWithBoots)
        {
            water = Terrain.WALKABLEWATER;
        }
        walkableTerrains = new List<Terrain>() { Terrain.DESERT, Terrain.FOREST, Terrain.GRAVE };
        randomTerrainFilter = new List<Terrain>() { Terrain.DESERT, Terrain.FOREST, Terrain.GRAVE, Terrain.MOUNTAIN, water };

        foreach (Location location in AllLocations)
        {
            location.CanShuffle = true;
        }
        if (biome == Biome.VANILLA || biome == Biome.VANILLA_SHUFFLE)
        {
            MAP_ROWS = 75;
            MAP_COLS = 64;
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
                    for (int i = 0; i < MAP_ROWS; i++)
                    {
                        for (int j = 0; j < 29; j++)
                        {
                            map[i, j] = Terrain.NONE;
                        }
                        for (int j = 29; j < MAP_COLS; j++)
                        {
                            map[i, j] = water;
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
                    riverT = water;
                    for (int i = 0; i < MAP_COLS; i++)
                    {
                        map[0, i] = water;
                        map[MAP_ROWS - 1, i] = water;
                    }

                    for (int i = 0; i < MAP_ROWS; i++)
                    {
                        map[i, 0] = water;
                        map[i, MAP_COLS - 1] = water;
                    }

                    int cols = RNG.Next(2, 4);
                    int rows = RNG.Next(2, 4);
                    List<int> pickedC = new List<int>();
                    List<int> pickedR = new List<int>();

                    while (cols > 0)
                    {
                        int col = RNG.Next(1, MAP_COLS);
                        if (!pickedC.Contains(col))
                        {
                            for (int i = 0; i < MAP_ROWS; i++)
                            {
                                if (map[i, col] == Terrain.NONE)
                                {
                                    map[i, col] = water;
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
                                    map[row, i] = water;
                                }
                                else if (map[row, i] == water)
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
                    riverT = water;

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
                if (biome == Biome.CANYON || biome == Biome.DRY_CANYON)
                {
                    raftDirection = props.DmIsHorizontal ? DirectionExtensions.RandomHorizontal(RNG) : DirectionExtensions.RandomVertical(RNG);
                }
                if (raft != null)
                {
                    if (biome != Biome.CANYON && biome != Biome.DRY_CANYON && biome != Biome.CALDERA)
                    {
                        MAP_COLS = 29;
                    }
                    DrawOcean(raftDirection, props.CanWalkOnWaterWithBoots);
                    MAP_COLS = 64;
                }

                Direction bridgeDirection;
                do
                {
                    if (biome != Biome.CANYON && biome != Biome.DRY_CANYON)
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
                    if (biome != Biome.CANYON && biome != Biome.DRY_CANYON && biome != Biome.CALDERA)
                    {
                        MAP_COLS = 29;
                    }
                    DrawOcean(bridgeDirection, props.CanWalkOnWaterWithBoots);
                    MAP_COLS = 64;
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
                        } while ((map[y, x] != Terrain.NONE || map[y - 1, x] != Terrain.NONE || map[y + 1, x] != Terrain.NONE || map[y + 1, x + 1] != Terrain.NONE || map[y, x + 1] != Terrain.NONE || map[y - 1, x + 1] != Terrain.NONE || map[y + 1, x - 1] != Terrain.NONE || map[y, x - 1] != Terrain.NONE || map[y - 1, x - 1] != Terrain.NONE) && tries < 100);
                        if (tries >= 100)
                        {
                            return false;
                        }

                        map[y, x] = location.TerrainType;
                        location.Xpos = x;
                        location.Y = y;
                        if (location.TerrainType == Terrain.CAVE)
                        {

                            Direction direction = (Direction)RNG.Next(4);

                            //Terrain s = walkableTerrains[RNG.Next(walkableTerrains.Count)];
                            Terrain s = climate.GetRandomTerrain(RNG, walkableTerrains);
                            if (biome == Biome.VANILLALIKE)
                            {
                                s = Terrain.ROAD;
                            }
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
                                    tries++;
                                } while ((x < 5 || y < 5 || x > MAP_COLS - 5 || y > MAP_ROWS - 5 || map[y, x] != Terrain.NONE || map[y - 1, x] != Terrain.NONE || map[y + 1, x] != Terrain.NONE || map[y + 1, x + 1] != Terrain.NONE || map[y, x + 1] != Terrain.NONE || map[y - 1, x + 1] != Terrain.NONE || map[y + 1, x - 1] != Terrain.NONE || map[y, x - 1] != Terrain.NONE || map[y - 1, x - 1] != Terrain.NONE) && tries < 100);
                                if (tries >= 100)
                                {
                                    return false;
                                }

                                while ((direction == Direction.NORTH && y < 15) || (direction == Direction.EAST && x > MAP_COLS - 15) || (direction == Direction.SOUTH && y > MAP_ROWS - 15) || (direction == Direction.WEST && x < 15))
                                {
                                    direction = (Direction)RNG.Next(4);
                                }
                                if (connectionsDM[location].Count == 1)
                                {
                                    int otherx = 0;
                                    int othery = 0;
                                    tries = 0;
                                    bool crossing = true;
                                    do
                                    {
                                        int range = 7;
                                        int offset = 3;
                                        if (biome == Biome.ISLANDS)
                                        {
                                            range = 7;
                                            offset = 5;
                                        }
                                        crossing = true;
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
                                        tries++;

                                        if (tries >= 100)
                                        {
                                            return false;
                                        }
                                    } while (!crossing || otherx <= 1 || otherx >= MAP_COLS - 1 || othery <= 1 || othery >= MAP_ROWS - 1 || map[othery, otherx] != Terrain.NONE || map[othery - 1, otherx] != Terrain.NONE || map[othery + 1, otherx] != Terrain.NONE || map[othery + 1, otherx + 1] != Terrain.NONE || map[othery, otherx + 1] != Terrain.NONE || map[othery - 1, otherx + 1] != Terrain.NONE || map[othery + 1, otherx - 1] != Terrain.NONE || map[othery, otherx - 1] != Terrain.NONE || map[othery - 1, otherx - 1] != Terrain.NONE);
                                    if (tries >= 100)
                                    {
                                        return false;
                                    }

                                    List<Location> l2 = connectionsDM[location];
                                    location.CanShuffle = false;
                                    location.Xpos = x;
                                    location.Y = y;
                                    l2[0].CanShuffle = false;
                                    l2[0].Xpos = otherx;
                                    l2[0].Y = othery;
                                    PlaceCave(x, y, direction, s);
                                    PlaceCave(otherx, othery, direction.Reverse(), s);
                                }
                                else //4-way caves
                                {
                                    int otherx = 0;
                                    int othery = 0;
                                    tries = 0;
                                    bool crossing = true;
                                    do
                                    {
                                        int range = 7;
                                        int offset = 3;
                                        if (biome == Biome.ISLANDS)
                                        {
                                            range = 7;
                                            offset = 5;
                                        }
                                        crossing = true;
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
                                        tries++;

                                        if (tries >= 100)
                                        {
                                            return false;
                                        }
                                    } while (!crossing || otherx <= 1 || otherx >= MAP_COLS - 1 || othery <= 1 || othery >= MAP_ROWS - 1 || map[othery, otherx] != Terrain.NONE || map[othery - 1, otherx] != Terrain.NONE || map[othery + 1, otherx] != Terrain.NONE || map[othery + 1, otherx + 1] != Terrain.NONE || map[othery, otherx + 1] != Terrain.NONE || map[othery - 1, otherx + 1] != Terrain.NONE || map[othery + 1, otherx - 1] != Terrain.NONE || map[othery, otherx - 1] != Terrain.NONE || map[othery - 1, otherx - 1] != Terrain.NONE); if (tries >= 100)
                                    {
                                        return false;
                                    }

                                    List<Location> caveExits = connectionsDM[location];
                                    location.CanShuffle = false;
                                    location.Xpos = x;
                                    location.Y = y;
                                    caveExits[0].CanShuffle = false;
                                    caveExits[0].Xpos = otherx;
                                    caveExits[0].Y = othery;
                                    PlaceCave(x, y, direction, s);
                                    PlaceCave(otherx, othery, direction.Reverse(), s);
                                    int newx = 0;
                                    int newy = 0;
                                    tries = 0;
                                    do
                                    {
                                        newx = x + RNG.Next(7) - 3;
                                        newy = y + RNG.Next(7) - 3;
                                        tries++;
                                    } while (newx > 2 && newx < MAP_COLS - 2 && newy > 2 && newy < MAP_ROWS - 2 && (map[newy, newx] != Terrain.NONE || map[newy - 1, newx] != Terrain.NONE || map[newy + 1, newx] != Terrain.NONE || map[newy + 1, newx + 1] != Terrain.NONE || map[newy, newx + 1] != Terrain.NONE || map[newy - 1, newx + 1] != Terrain.NONE || map[newy + 1, newx - 1] != Terrain.NONE || map[newy, newx - 1] != Terrain.NONE || map[newy - 1, newx - 1] != Terrain.NONE) && tries < 100);
                                    if (tries >= 100)
                                    {
                                        return false;
                                    }
                                    caveExits[1].Xpos = newx;
                                    caveExits[1].Y = newy;
                                    caveExits[1].CanShuffle = false;
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
                                        crossing = true;

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
                                        tries++;

                                        if (tries >= 100)
                                        {
                                            return false;
                                        }
                                    } while (!crossing || otherx <= 1 || otherx >= MAP_COLS - 1 || othery <= 1 || othery >= MAP_ROWS - 1 || map[othery, otherx] != Terrain.NONE || map[othery - 1, otherx] != Terrain.NONE || map[othery + 1, otherx] != Terrain.NONE || map[othery + 1, otherx + 1] != Terrain.NONE || map[othery, otherx + 1] != Terrain.NONE || map[othery - 1, otherx + 1] != Terrain.NONE || map[othery + 1, otherx - 1] != Terrain.NONE || map[othery, otherx - 1] != Terrain.NONE || map[othery - 1, otherx - 1] != Terrain.NONE); if (tries >= 100)
                                    {
                                        return false;
                                    }

                                    location.CanShuffle = false;
                                    caveExits[2].CanShuffle = false;
                                    caveExits[2].Xpos = otherx;
                                    caveExits[2].Y = othery;
                                    PlaceCave(otherx, othery, direction.Reverse(), s);
                                }
                            }
                            else
                            {
                                PlaceCave(x, y, direction, s);
                            }
                            
                        }
                    }
                }

                if (biome == Biome.VANILLALIKE)
                {
                    PlaceRandomTerrain(climate, 5);
                }
                randomTerrainFilter.Add(Terrain.ROAD);
                //Debug.WriteLine(GetMapDebug());
                Climate growthClimate = climate.CloneWithInvertedDistances();
                float dmOpennessFactor = biome switch
                {
                    Biome.CANYON => (float)(RNG.NextDouble() * .75 + 1),
                    _ => 1f
                };
                growthClimate.ApplyDeathMountainSafety(randomTerrainFilter, dmOpennessFactor);
                if (!GrowTerrain(growthClimate))
                {
                    return false;
                }
                //Debug.WriteLine(GetMapDebug());
                if (biome == Biome.CALDERA)
                {
                    bool f = MakeCaldera(props.CanWalkOnWaterWithBoots);
                    if (!f)
                    {
                        return false;
                    }
                }
                walkableTerrains.Add(Terrain.ROAD);
                if (raft != null)
                {
                    if (biome != Biome.CALDERA && biome != Biome.CANYON && biome != Biome.DRY_CANYON)
                    {
                        MAP_COLS = 29;
                    }
                    bool r = DrawRaft(raftDirection);
                    MAP_COLS = 64;
                    if (!r)
                    {
                        return false;
                    }
                }

                if (bridge != null)
                {
                    if (biome != Biome.CALDERA && biome != Biome.CANYON && biome != Biome.DRY_CANYON)
                    {
                        MAP_COLS = 29;
                    }
                    bool b = DrawBridge(bridgeDirection);
                    MAP_COLS = 64;
                    if (!b)
                    {
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
                    return false;
                }

                //check bytes and adjust
                bytesWritten = WriteMapToRom(rom, false, MAP_ADDR, MAP_SIZE_BYTES, 0, 0, props.HiddenPalace, props.HiddenKasuto);
            }
        }
        if(!ValidateBasicRouting())
        {
            return false;
        }

        WriteMapToRom(rom, true, MAP_ADDR, MAP_SIZE_BYTES, 0, 0, props.HiddenPalace, props.HiddenKasuto);
        

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
    private bool MakeCaldera(bool canWalkOnWaterWithBoots)
    {
        Terrain water = Terrain.WATER;
        if (canWalkOnWaterWithBoots)
        {
            water = Terrain.WALKABLEWATER;
        }
        int centerx, centery;

        bool placeable;
        do
        {
            if (isHorizontal)
            {
                centerx = RNG.Next(27, 37);
                centery = RNG.Next(17, 27);
            }
            else
            {
                centerx = RNG.Next(21, 41);
                centery = RNG.Next(17, 27);
            }
            placeable = true;
            for (int i = centery - 7; i < centery + 8; i++)
            {
                for (int j = centerx - 7; j < centerx + 8; j++)
                {
                    if (map[i, j] != Terrain.MOUNTAIN)
                    {
                        placeable = false;
                    }
                }
            }
        } while (!placeable);

        int startx = centerx - 5;
        int starty = centery;
        int deltax = 1;
        int deltay = 0;
        if (!isHorizontal)
        {
            startx = centerx;
            starty = centery - 5;
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

        Location cave1l, cave1r, cave2l, cave2r;

        do
        {
            int cavenum1 = RNG.Next(connectionsDM.Keys.Count);
            cave1l = connectionsDM.Keys.ToList()[cavenum1];
            cave1r = connectionsDM[cave1l][0];
        } while (connectionsDM[cave1l].Count != 1 || connectionsDM[cave1r].Count != 1);

        do
        {
            int cavenum1 = RNG.Next(connectionsDM.Keys.Count);
            cave2l = connectionsDM.Keys.ToList()[cavenum1];
            cave2r = connectionsDM[cave2l][0];
        } while (connectionsDM[cave2l].Count != 1 || cave1l == cave2l || cave1l == cave2r);
        cave1l.CanShuffle = false;
        cave1r.CanShuffle = false;
        cave2l.CanShuffle = false;
        cave2r.CanShuffle = false;
        map[cave1l.Y, cave1l.Xpos] = Terrain.MOUNTAIN;
        map[cave2l.Y, cave2l.Xpos] = Terrain.MOUNTAIN;

        map[cave1r.Y, cave1r.Xpos] = Terrain.MOUNTAIN;

        map[cave2r.Y, cave2r.Xpos] = Terrain.MOUNTAIN;


        int caveType = RNG.Next(2);
        if (isHorizontal)
        {
            bool f = HorizontalCave(caveType, centerx, centery, cave1l, cave1r);
            if (!f)
            {
                return false;
            }

            if (caveType == 0)
            {
                caveType = 1;
            }
            else
            {
                caveType = 0;
            }
            f = HorizontalCave(caveType, centerx, centery, cave2l, cave2r);
            if (!f)
            {
                return false;
            }
        }
        else
        {
            bool f = VerticalCave(caveType, centerx, centery, cave1l, cave1r);
            if (!f)
            {
                return false;
            }
            if (caveType == 0)
            {
                caveType = 1;
            }
            else
            {
                caveType = 0;
            }
            f = VerticalCave(caveType, centerx, centery, cave2l, cave2r);
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
    public override void UpdateVisit(Dictionary<Collectable, bool> itemGet)
    {
        UpdateReachable(itemGet);

        foreach (Location location in AllLocations)
        {
            if (visitation[location.Y, location.Xpos])
            {
                location.Reachable = true;
                if (connectionsDM.Keys.Contains(location))
                {
                    foreach(Location l3 in connectionsDM[location])
                    { 
                        l3.Reachable = true;
                        visitation[l3.Y, l3.Xpos] = true;
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
