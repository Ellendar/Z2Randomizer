using Z2Randomizer.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Z2Randomizer.Core;

namespace Z2Randomizer.Core.Overworld;

class DeathMountain : World
{

    private readonly SortedDictionary<int, Terrain> terrains = new SortedDictionary<int, Terrain>
        {
            { 0x610C, Terrain.CAVE },
            { 0x610D, Terrain.CAVE },
            { 0x610E, Terrain.CAVE },
            { 0x610F, Terrain.CAVE },
            { 0x6110, Terrain.CAVE },
            { 0x6111, Terrain.CAVE },
            { 0x6112, Terrain.CAVE },
            { 0x6113, Terrain.CAVE },
            { 0x6114, Terrain.CAVE },
            { 0x6115, Terrain.CAVE },
            { 0x6116, Terrain.CAVE },
            { 0x6117, Terrain.CAVE },
            { 0x6118, Terrain.CAVE },
            { 0x6119, Terrain.CAVE },
            { 0x611A, Terrain.CAVE },
            { 0x611B, Terrain.CAVE },
            { 0x611C, Terrain.CAVE },
            { 0x611D, Terrain.CAVE },
            { 0x611E, Terrain.CAVE },
            { 0x611F, Terrain.CAVE },
            { 0x6120, Terrain.CAVE },
            { 0x6121, Terrain.CAVE },
            { 0x6122, Terrain.CAVE },
            { 0x6123, Terrain.CAVE },
            { 0x6124, Terrain.CAVE },
            { 0x6125, Terrain.CAVE },
            { 0x6126, Terrain.CAVE },
            { 0x6127, Terrain.CAVE },
            { 0x6128, Terrain.CAVE },
            { 0x6129, Terrain.CAVE },
            { 0x612A, Terrain.CAVE },
            { 0x612B, Terrain.CAVE },
            { 0x612C, Terrain.CAVE },
            { 0x612D, Terrain.CAVE },
            { 0x612E, Terrain.CAVE },
            { 0x612F, Terrain.CAVE },
            { 0x6130, Terrain.CAVE },
            { 0x6136, Terrain.CAVE },
            { 0x6137, Terrain.CAVE },
            { 0x6144, Terrain.CAVE }
    };

    public Dictionary<Location, List<Location>> connectionsDM;
    public Location hammerCave;
    public Location magicCave;

    private const int MAP_ADDR = 0x7a00;

    public DeathMountain(Hyrule hy)
        : base(hy)
    {
        LoadLocations(0x610C, 37, terrains, Continent.DM);
        //loadLocations(0x6136, 2, terrains, continent.dm);
        LoadLocations(0x6144, 1, terrains, Continent.DM);

        //isHorizontal = hy.Props.DmIsHorizontal;
        isHorizontal = true;

        hammerCave = GetLocationByMem(0x6128);
        magicCave = GetLocationByMem(0x6144);

        //reachableAreas = new HashSet<string>();
        connectionsDM = new Dictionary<Location, List<Location>>
        {
            { GetLocationByMem(0x610C), new List<Location>() { GetLocationByMem(0x610D) } },
            { GetLocationByMem(0x610D), new List<Location>() { GetLocationByMem(0x610C) } },
            { GetLocationByMem(0x610E), new List<Location>() { GetLocationByMem(0x610F) } },
            { GetLocationByMem(0x610F), new List<Location>() { GetLocationByMem(0x610E) } },
            { GetLocationByMem(0x6110), new List<Location>() { GetLocationByMem(0x6111) } },
            { GetLocationByMem(0x6111), new List<Location>() { GetLocationByMem(0x6110) } },
            { GetLocationByMem(0x6112), new List<Location>() { GetLocationByMem(0x6113) } },
            { GetLocationByMem(0x6113), new List<Location>() { GetLocationByMem(0x6112) } },
            { GetLocationByMem(0x6114), new List<Location>() { GetLocationByMem(0x6115) } },
            { GetLocationByMem(0x6115), new List<Location>() { GetLocationByMem(0x6114) } },
            { GetLocationByMem(0x6116), new List<Location>() { GetLocationByMem(0x6117) } },
            { GetLocationByMem(0x6117), new List<Location>() { GetLocationByMem(0x6116) } },
            { GetLocationByMem(0x6118), new List<Location>() { GetLocationByMem(0x6119) } },
            { GetLocationByMem(0x6119), new List<Location>() { GetLocationByMem(0x6118) } },
            { GetLocationByMem(0x611A), new List<Location>() { GetLocationByMem(0x611B) } },
            { GetLocationByMem(0x611B), new List<Location>() { GetLocationByMem(0x611A) } },
            { GetLocationByMem(0x611C), new List<Location>() { GetLocationByMem(0x611D) } },
            { GetLocationByMem(0x611D), new List<Location>() { GetLocationByMem(0x611C) } },
            { GetLocationByMem(0x611E), new List<Location>() { GetLocationByMem(0x611F) } },
            { GetLocationByMem(0x611F), new List<Location>() { GetLocationByMem(0x611E) } },
            { GetLocationByMem(0x6120), new List<Location>() { GetLocationByMem(0x6121) } },
            { GetLocationByMem(0x6121), new List<Location>() { GetLocationByMem(0x6120) } },
            { GetLocationByMem(0x6122), new List<Location>() { GetLocationByMem(0x6123) } },
            { GetLocationByMem(0x6123), new List<Location>() { GetLocationByMem(0x6122) } },
            { GetLocationByMem(0x6124), new List<Location>() { GetLocationByMem(0x6125) } },
            { GetLocationByMem(0x6125), new List<Location>() { GetLocationByMem(0x6124) } },
            { GetLocationByMem(0x6126), new List<Location>() { GetLocationByMem(0x6127) } },
            { GetLocationByMem(0x6127), new List<Location>() { GetLocationByMem(0x6126) } },

            { GetLocationByMem(0x6129), new List<Location>() { GetLocationByMem(0x612A), GetLocationByMem(0x612B), GetLocationByMem(0x612C) } },
            { GetLocationByMem(0x612A), new List<Location>() { GetLocationByMem(0x6129), GetLocationByMem(0x612B), GetLocationByMem(0x612C) } },
            { GetLocationByMem(0x612B), new List<Location>() { GetLocationByMem(0x612A), GetLocationByMem(0x6129), GetLocationByMem(0x612C) } },
            { GetLocationByMem(0x612C), new List<Location>() { GetLocationByMem(0x612A), GetLocationByMem(0x612B), GetLocationByMem(0x6129) } },

            { GetLocationByMem(0x612D), new List<Location>() { GetLocationByMem(0x612E), GetLocationByMem(0x612F), GetLocationByMem(0x6130) } },
            { GetLocationByMem(0x612E), new List<Location>() { GetLocationByMem(0x612D), GetLocationByMem(0x612F), GetLocationByMem(0x6130) } },
            { GetLocationByMem(0x612F), new List<Location>() { GetLocationByMem(0x612E), GetLocationByMem(0x612D), GetLocationByMem(0x6130) } },
            { GetLocationByMem(0x6130), new List<Location>() { GetLocationByMem(0x612E), GetLocationByMem(0x612F), GetLocationByMem(0x612D) } }
        };

        enemies = new List<int> { 0x03, 0x04, 0x05, 0x11, 0x12, 0x14, 0x15, 0x16, 0x17, 0x18, 0x19, 0x1A, 0x1B, 0x1C, 0x1F, 0x20 };
        flyingEnemies = new List<int> { 0x06, 0x07, 0x0A, 0x0D, 0x0E };
        generators = new List<int> { 0x0B, 0x0C, 0x0F, 0x1D };
        smallEnemies = new List<int> { 0x03, 0x04, 0x05, 0x11, 0x12, 0x1C, 0x1F };
        largeEnemies = new List<int> { 0x20, 0x14, 0x15, 0x16, 0x17, 0x18, 0x19, 0x1A, 0x1B };
        enemyAddr = 0x48B0;
        enemyPtr = 0x608E;

        overworldMaps = new List<int>();
        MAP_ROWS = 45;
        MAP_COLS = 64;

        baseAddr = 0x610C;
        VANILLA_MAP_ADDR = 0x665c;

        if (hy.Props.DmBiome == Biome.ISLANDS)
        {
            this.biome = Biome.ISLANDS;
        }
        else if (hy.Props.DmBiome == Biome.CANYON || hy.Props.DmBiome == Biome.DRY_CANYON)
        {
            this.biome = Biome.CANYON;
            //MAP_ROWS = 75;
        }
        else if(hy.Props.DmBiome == Biome.CALDERA)
        {
            this.biome = Biome.CALDERA;
        }
        else if(hy.Props.DmBiome == Biome.MOUNTAINOUS)
        {
            this.biome = Biome.MOUNTAINOUS;
        }
        else if(hy.Props.DmBiome == Biome.VANILLA)
        {
            this.biome = Biome.VANILLA;
        }
        else if(hy.Props.DmBiome == Biome.VANILLA_SHUFFLE)
        {
            this.biome = Biome.VANILLA_SHUFFLE;
        }
        else
        {
            this.biome = Biome.VANILLALIKE;
        }
        walkableTerrains = new List<Terrain>() { Terrain.DESERT, Terrain.FOREST, Terrain.GRAVE };
        randomTerrains = new List<Terrain>() { Terrain.DESERT, Terrain.FOREST, Terrain.GRAVE, Terrain.MOUNTAIN, Terrain.WALKABLEWATER, Terrain.WATER };
    
        
    }

    public bool Terraform()
    {
        Terrain water = Terrain.WATER;
        if (hyrule.Props.CanWalkOnWaterWithBoots)
        {
            water = Terrain.WALKABLEWATER;
        }
        walkableTerrains = new List<Terrain>() { Terrain.DESERT, Terrain.FOREST, Terrain.GRAVE };
        randomTerrains = new List<Terrain>() { Terrain.DESERT, Terrain.FOREST, Terrain.GRAVE, Terrain.MOUNTAIN, water };

        foreach (Location location in AllLocations)
        {
            location.CanShuffle = true;
        }
        if (this.biome == Biome.VANILLA || this.biome == Biome.VANILLA_SHUFFLE)
        {
            MAP_ROWS = 75;
            MAP_COLS = 64;
            ReadVanillaMap();
            if (this.biome == Biome.VANILLA_SHUFFLE)
            {
                ShuffleLocations(AllLocations);
                if (hyrule.Props.VanillaShuffleUsesActualTerrain)
                {
                    magicCave.TerrainType = Terrain.ROCK;
                    foreach (Location location in AllLocations)
                    {
                        map[location.Ypos - 30, location.Xpos] = location.TerrainType;
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
        else
        {
            bytesWritten = 2000;
            bool horizontal = false;
            while (bytesWritten > MAP_SIZE_BYTES)
            {
                map = new Terrain[MAP_ROWS, MAP_COLS];
                Terrain riverT = Terrain.MOUNTAIN;
                if (this.biome != Biome.CANYON && this.biome != Biome.CALDERA && this.biome != Biome.ISLANDS)
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

                if (this.biome == Biome.ISLANDS)
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

                    int cols = hyrule.RNG.Next(2, 4);
                    int rows = hyrule.RNG.Next(2, 4);
                    List<int> pickedC = new List<int>();
                    List<int> pickedR = new List<int>();

                    while (cols > 0)
                    {
                        int col = hyrule.RNG.Next(1, MAP_COLS);
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
                        int row = hyrule.RNG.Next(5, MAP_ROWS - 6);
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
                                    int adjust = hyrule.RNG.Next(-3, 4);
                                    while (row + adjust < 1 || row + adjust > MAP_ROWS - 2)
                                    {
                                        adjust = hyrule.RNG.Next(-3, 4);
                                    }
                                    row += adjust;
                                }
                            }
                            pickedR.Add(row);
                            rows--;
                        }
                    }



                }
                else if (this.biome == Biome.CANYON)
                {
                    riverT = water;
                    horizontal = hyrule.RNG.NextDouble() > 0.5;

                    if (hyrule.Props.WestBiome == Biome.DRY_CANYON)
                    {
                        riverT = Terrain.DESERT;
                    }

                    this.walkableTerrains.Clear();
                    this.walkableTerrains.Add(Terrain.GRASS);
                    this.walkableTerrains.Add(Terrain.GRAVE);
                    this.walkableTerrains.Add(Terrain.FOREST);
                    this.walkableTerrains.Add(Terrain.DESERT);
                    this.walkableTerrains.Add(Terrain.MOUNTAIN);

                    this.randomTerrains.Remove(Terrain.SWAMP);
                    DrawCanyon(riverT);
                    this.walkableTerrains.Remove(Terrain.MOUNTAIN);

                    this.randomTerrains.Remove(Terrain.SWAMP);
                    //this.randomTerrains.Add(terrain.lava);

                }
                else if (this.biome == Biome.CALDERA)
                {
                    DrawCenterMountain();
                }
                else if (this.biome == Biome.MOUNTAINOUS)
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


                    int cols = hyrule.RNG.Next(2, 4);
                    int rows = hyrule.RNG.Next(2, 4);
                    List<int> pickedC = new List<int>();
                    List<int> pickedR = new List<int>();

                    while (cols > 0)
                    {
                        int col = hyrule.RNG.Next(10, MAP_COLS - 11);
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
                        int row = hyrule.RNG.Next(10, MAP_ROWS - 11);
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

                Direction raftDirection = (Direction)hyrule.RNG.Next(4);
                if (this.biome == Biome.CANYON)
                {
                    raftDirection = horizontal ? DirectionExtensions.RandomHorizontal(hyrule.RNG) : DirectionExtensions.RandomVertical(hyrule.RNG);
                }
                if (raft != null)
                {
                    if (this.biome != Biome.CANYON && this.biome != Biome.CALDERA)
                    {
                        MAP_COLS = 29;
                    }
                    DrawOcean(raftDirection);
                    MAP_COLS = 64;
                }

                Direction bridgeDirection = (Direction)hyrule.RNG.Next(4);
                do
                {
                    if (this.biome != Biome.CANYON)
                    {
                        bridgeDirection = (Direction)hyrule.RNG.Next(4);
                    }
                    else
                    {
                        bridgeDirection = horizontal ? DirectionExtensions.RandomHorizontal(hyrule.RNG) : DirectionExtensions.RandomVertical(hyrule.RNG);
                    }
                } while (bridgeDirection == raftDirection);
                if (bridge != null)
                {
                    if (this.biome != Biome.CANYON && this.biome != Biome.CALDERA)
                    {
                        MAP_COLS = 29;
                    }
                    DrawOcean(bridgeDirection);
                    MAP_COLS = 64;
                }
                int x = 0;
                int y = 0;
                foreach (Location location in AllLocations)
                {
                    if (location.TerrainType != Terrain.BRIDGE && location != magicCave && location.CanShuffle)
                    {
                        int tries = 0;
                        do
                        {
                            x = hyrule.RNG.Next(MAP_COLS - 2) + 1;
                            y = hyrule.RNG.Next(MAP_ROWS - 2) + 1;
                            tries++;
                        } while ((map[y, x] != Terrain.NONE || map[y - 1, x] != Terrain.NONE || map[y + 1, x] != Terrain.NONE || map[y + 1, x + 1] != Terrain.NONE || map[y, x + 1] != Terrain.NONE || map[y - 1, x + 1] != Terrain.NONE || map[y + 1, x - 1] != Terrain.NONE || map[y, x - 1] != Terrain.NONE || map[y - 1, x - 1] != Terrain.NONE) && tries < 100);
                        if (tries >= 100)
                        {
                            return false;
                        }

                        map[y, x] = location.TerrainType;
                        location.Xpos = x;
                        location.Ypos = y + 30;
                        if (location.TerrainType == Terrain.CAVE)
                        {

                            Direction direction = (Direction)hyrule.RNG.Next(4);

                            Terrain s = walkableTerrains[hyrule.RNG.Next(walkableTerrains.Count)];
                            if (this.biome == Biome.VANILLALIKE)
                            {
                                s = Terrain.ROAD;
                            }
                            if (!hyrule.Props.SaneCaves || !connectionsDM.ContainsKey(location))
                            {
                                PlaceCave(x, y, direction, s);
                            }
                            else
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
                                    x = hyrule.RNG.Next(MAP_COLS - 2) + 1;
                                    y = hyrule.RNG.Next(MAP_ROWS - 2) + 1;
                                    tries++;
                                } while ((x < 5 || y < 5 || x > MAP_COLS - 5 || y > MAP_ROWS - 5 || map[y, x] != Terrain.NONE || map[y - 1, x] != Terrain.NONE || map[y + 1, x] != Terrain.NONE || map[y + 1, x + 1] != Terrain.NONE || map[y, x + 1] != Terrain.NONE || map[y - 1, x + 1] != Terrain.NONE || map[y + 1, x - 1] != Terrain.NONE || map[y, x - 1] != Terrain.NONE || map[y - 1, x - 1] != Terrain.NONE) && tries < 100);
                                if (tries >= 100)
                                {
                                    return false;
                                }

                                while ((direction == Direction.NORTH && y < 15) || (direction == Direction.EAST && x > MAP_COLS - 15) || (direction == Direction.SOUTH && y > MAP_ROWS - 15) || (direction == Direction.WEST && x < 15))
                                {
                                    direction = (Direction)hyrule.RNG.Next(4);
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
                                            otherx = x + (hyrule.RNG.Next(7) - 3);
                                            othery = y - (hyrule.RNG.Next(range) + offset);
                                        }
                                        else if (direction == Direction.EAST)
                                        {
                                            otherx = x + (hyrule.RNG.Next(range) + offset);
                                            othery = y + (hyrule.RNG.Next(7) - 3);
                                        }
                                        else if (direction == Direction.SOUTH)
                                        {
                                            otherx = x + (hyrule.RNG.Next(7) - 3);
                                            othery = y + (hyrule.RNG.Next(range) + offset);
                                        }
                                        else //west
                                        {
                                            otherx = x - (hyrule.RNG.Next(range) + offset);
                                            othery = y + (hyrule.RNG.Next(7) - 3);
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
                                    location.Ypos = y + 30;
                                    l2[0].CanShuffle = false;
                                    l2[0].Xpos = otherx;
                                    l2[0].Ypos = othery + 30;
                                    PlaceCave(x, y, direction, s);
                                    PlaceCave(otherx, othery, direction.Reverse(), s);
                                }
                                else
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
                                            otherx = x + (hyrule.RNG.Next(7) - 3);
                                            othery = y - (hyrule.RNG.Next(range) + offset);
                                        }
                                        else if (direction == Direction.EAST)
                                        {
                                            otherx = x + (hyrule.RNG.Next(range) + offset);
                                            othery = y + (hyrule.RNG.Next(7) - 3);
                                        }
                                        else if (direction == Direction.SOUTH)
                                        {
                                            otherx = x + (hyrule.RNG.Next(7) - 3);
                                            othery = y + (hyrule.RNG.Next(range) + offset);
                                        }
                                        else //west
                                        {
                                            otherx = x - (hyrule.RNG.Next(range) + offset);
                                            othery = y + (hyrule.RNG.Next(7) - 3);
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

                                    List<Location> l2 = connectionsDM[location];
                                    location.CanShuffle = false;
                                    location.Xpos = x;
                                    location.Ypos = y + 30;
                                    l2[0].CanShuffle = false;
                                    l2[0].Xpos = otherx;
                                    l2[0].Ypos = othery + 30;
                                    PlaceCave(x, y, direction, s);
                                    PlaceCave(otherx, othery, direction.Reverse(), s);
                                    int newx = 0;
                                    int newy = 0;
                                    tries = 0;
                                    do
                                    {
                                        newx = x + hyrule.RNG.Next(7) - 3;
                                        newy = y + hyrule.RNG.Next(7) - 3;
                                        tries++;
                                    } while (newx > 2 && newx < MAP_COLS - 2 && newy > 2 && newy < MAP_ROWS - 2 && (map[newy, newx] != Terrain.NONE || map[newy - 1, newx] != Terrain.NONE || map[newy + 1, newx] != Terrain.NONE || map[newy + 1, newx + 1] != Terrain.NONE || map[newy, newx + 1] != Terrain.NONE || map[newy - 1, newx + 1] != Terrain.NONE || map[newy + 1, newx - 1] != Terrain.NONE || map[newy, newx - 1] != Terrain.NONE || map[newy - 1, newx - 1] != Terrain.NONE) && tries < 100);
                                    if (tries >= 100)
                                    {
                                        return false;
                                    }
                                    l2[1].Xpos = newx;
                                    l2[1].Ypos = newy + 30;
                                    l2[1].CanShuffle = false;
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
                                            otherx = x + (hyrule.RNG.Next(7) - 3);
                                            othery = y - (hyrule.RNG.Next(range) + offset);
                                        }
                                        else if (direction == Direction.EAST)
                                        {
                                            otherx = x + (hyrule.RNG.Next(range) + offset);
                                            othery = y + (hyrule.RNG.Next(7) - 3);
                                        }
                                        else if (direction == Direction.EAST)
                                        {
                                            otherx = x + (hyrule.RNG.Next(7) - 3);
                                            othery = y + (hyrule.RNG.Next(range) + offset);
                                        }
                                        else //east
                                        {
                                            otherx = x - (hyrule.RNG.Next(range) + offset);
                                            othery = y + (hyrule.RNG.Next(7) - 3);
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
                                    l2[2].CanShuffle = false;
                                    l2[2].Xpos = otherx;
                                    l2[2].Ypos = othery + 30;
                                    PlaceCave(otherx, othery, direction.Reverse(), s);
                                }

                            }
                        }
                    }
                }


                if (this.biome == Biome.VANILLALIKE)
                {
                    PlaceRandomTerrain(5);
                }
                randomTerrains.Add(Terrain.ROAD);
                if (!GrowTerrain())
                {
                    return false;
                }
                if (this.biome == Biome.CALDERA)
                {
                    bool f = MakeCaldera();
                    if (!f)
                    {
                        return false;
                    }
                }
                walkableTerrains.Add(Terrain.ROAD);
                if (raft != null)
                {
                    if (this.biome != Biome.CALDERA && this.biome != Biome.CANYON)
                    {
                        MAP_COLS = 29;
                    }
                    bool r = DrawRaft(false, raftDirection);
                    MAP_COLS = 64;
                    if (!r)
                    {
                        return false;
                    }
                }

                if (bridge != null)
                {
                    if (this.biome != Biome.CALDERA && this.biome != Biome.CANYON)
                    {
                        MAP_COLS = 29;
                    }
                    bool b = DrawRaft(true, bridgeDirection);
                    MAP_COLS = 64;
                    if (!b)
                    {
                        return false;
                    }
                }


                do
                {
                    x = hyrule.RNG.Next(MAP_COLS - 2) + 1;
                    y = hyrule.RNG.Next(MAP_ROWS - 2) + 1;
                } while (!walkableTerrains.Contains(map[y, x]) || map[y + 1, x] == Terrain.CAVE || map[y - 1, x] == Terrain.CAVE || map[y, x + 1] == Terrain.CAVE || map[y, x - 1] == Terrain.CAVE);

                map[y, x] = Terrain.ROCK;
                magicCave.Ypos = y + 30;
                magicCave.Xpos = x;


                if (this.biome == Biome.CANYON || this.biome == Biome.ISLANDS)
                {
                    ConnectIslands(25, false, riverT, false, false, false);
                }

                //check bytes and adjust
                WriteMapToRom(false, MAP_ADDR, MAP_SIZE_BYTES, 0, 0);
            }
        }
        WriteMapToRom(true, MAP_ADDR, MAP_SIZE_BYTES, 0, 0);
        

        visitation = new bool[MAP_ROWS, MAP_COLS];
        for (int i = 0; i < MAP_ROWS; i++)
        {
            for (int j = 0; j < MAP_COLS; j++)
            {
                visitation[i, j] = false;
            }
        }

        for (int i = 0x610C; i < 0x614B; i++)
        {
            if (!terrains.Keys.Contains(i))
            {
                hyrule.ROMData.Put(i, 0x00);
            }
        }
        return true;
    }
    private bool MakeCaldera()
    {
        Terrain water = Terrain.WATER;
        if (hyrule.Props.CanWalkOnWaterWithBoots)
        {
            water = Terrain.WALKABLEWATER;
        }
        int centerx = hyrule.RNG.Next(21, 41);
        int centery = hyrule.RNG.Next(17, 27);
        if (isHorizontal)
        {
            centerx = hyrule.RNG.Next(27, 37);
            centery = hyrule.RNG.Next(17, 27);
        }

        bool placeable = false;
        do
        {
            if (isHorizontal)
            {
                centerx = hyrule.RNG.Next(27, 37);
                centery = hyrule.RNG.Next(17, 27);
            }
            else
            {
                centerx = hyrule.RNG.Next(21, 41);
                centery = hyrule.RNG.Next(17, 27);
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
            int lake = hyrule.RNG.Next(7, 11);
            if (i == 0 || i == 9)
            {
                lake = hyrule.RNG.Next(3, 6);
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

        Location cave1l = new Location();
        Location cave1r = new Location();
        Location cave2l = new Location();
        Location cave2r = new Location();

        do
        {
            int cavenum1 = hyrule.RNG.Next(connectionsDM.Keys.Count);
            cave1l = connectionsDM.Keys.ToList()[cavenum1];
            cave1r = connectionsDM[cave1l][0];
        } while (connectionsDM[cave1l].Count != 1 || connectionsDM[cave1r].Count != 1);

        do
        {
            int cavenum1 = hyrule.RNG.Next(connectionsDM.Keys.Count);
            cave2l = connectionsDM.Keys.ToList()[cavenum1];
            cave2r = connectionsDM[cave2l][0];
        } while (connectionsDM[cave2l].Count != 1 || cave1l == cave2l || cave1l == cave2r);
        cave1l.CanShuffle = false;
        cave1r.CanShuffle = false;
        cave2l.CanShuffle = false;
        cave2r.CanShuffle = false;
        map[cave1l.Ypos - 30, cave1l.Xpos] = Terrain.MOUNTAIN;
        map[cave2l.Ypos - 30, cave2l.Xpos] = Terrain.MOUNTAIN;

        map[cave1r.Ypos - 30, cave1r.Xpos] = Terrain.MOUNTAIN;

        map[cave2r.Ypos - 30, cave2r.Xpos] = Terrain.MOUNTAIN;


        int caveType = hyrule.RNG.Next(2);
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
    public void UpdateVisit()
    {
        UpdateReachable();

        foreach (Location location in AllLocations)
        {
            if (visitation[location.Ypos - 30, location.Xpos])
            {
                location.Reachable = true;
                if (connectionsDM.Keys.Contains(location))
                {
                    foreach(Location l3 in connectionsDM[location])
                    { 
                        l3.Reachable = true;
                        visitation[l3.Ypos - 30, l3.Xpos] = true;
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

    public override string GetName()
    {
        return "Death Mountain";
    }
}
