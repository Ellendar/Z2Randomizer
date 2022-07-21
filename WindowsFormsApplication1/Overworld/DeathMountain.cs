using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Z2Randomizer
{
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

            hammerCave = GetLocationByMem(0x6128);
            magicCave = GetLocationByMem(0x6144);

            reachableAreas = new HashSet<string>();
            connectionsDM = new Dictionary<Location, List<Location>>();
            connectionsDM.Add(GetLocationByMem(0x610C), new List<Location>() { GetLocationByMem(0x610D) });
            connectionsDM.Add(GetLocationByMem(0x610D), new List<Location>() { GetLocationByMem(0x610C) });
            connectionsDM.Add(GetLocationByMem(0x610E), new List<Location>() { GetLocationByMem(0x610F) });
            connectionsDM.Add(GetLocationByMem(0x610F), new List<Location>() { GetLocationByMem(0x610E) });
            connectionsDM.Add(GetLocationByMem(0x6110), new List<Location>() { GetLocationByMem(0x6111) });
            connectionsDM.Add(GetLocationByMem(0x6111), new List<Location>() { GetLocationByMem(0x6110) });
            connectionsDM.Add(GetLocationByMem(0x6112), new List<Location>() { GetLocationByMem(0x6113) });
            connectionsDM.Add(GetLocationByMem(0x6113), new List<Location>() { GetLocationByMem(0x6112) });
            connectionsDM.Add(GetLocationByMem(0x6114), new List<Location>() { GetLocationByMem(0x6115) });
            connectionsDM.Add(GetLocationByMem(0x6115), new List<Location>() { GetLocationByMem(0x6114) });
            connectionsDM.Add(GetLocationByMem(0x6116), new List<Location>() { GetLocationByMem(0x6117) });
            connectionsDM.Add(GetLocationByMem(0x6117), new List<Location>() { GetLocationByMem(0x6116) });
            connectionsDM.Add(GetLocationByMem(0x6118), new List<Location>() { GetLocationByMem(0x6119) });
            connectionsDM.Add(GetLocationByMem(0x6119), new List<Location>() { GetLocationByMem(0x6118) });
            connectionsDM.Add(GetLocationByMem(0x611A), new List<Location>() { GetLocationByMem(0x611B) });
            connectionsDM.Add(GetLocationByMem(0x611B), new List<Location>() { GetLocationByMem(0x611A) });
            connectionsDM.Add(GetLocationByMem(0x611C), new List<Location>() { GetLocationByMem(0x611D) });
            connectionsDM.Add(GetLocationByMem(0x611D), new List<Location>() { GetLocationByMem(0x611C) });
            connectionsDM.Add(GetLocationByMem(0x611E), new List<Location>() { GetLocationByMem(0x611F) });
            connectionsDM.Add(GetLocationByMem(0x611F), new List<Location>() { GetLocationByMem(0x611E) });
            connectionsDM.Add(GetLocationByMem(0x6120), new List<Location>() { GetLocationByMem(0x6121) });
            connectionsDM.Add(GetLocationByMem(0x6121), new List<Location>() { GetLocationByMem(0x6120) });
            connectionsDM.Add(GetLocationByMem(0x6122), new List<Location>() { GetLocationByMem(0x6123) });
            connectionsDM.Add(GetLocationByMem(0x6123), new List<Location>() { GetLocationByMem(0x6122) });
            connectionsDM.Add(GetLocationByMem(0x6124), new List<Location>() { GetLocationByMem(0x6125) });
            connectionsDM.Add(GetLocationByMem(0x6125), new List<Location>() { GetLocationByMem(0x6124) });
            connectionsDM.Add(GetLocationByMem(0x6126), new List<Location>() { GetLocationByMem(0x6127) });
            connectionsDM.Add(GetLocationByMem(0x6127), new List<Location>() { GetLocationByMem(0x6126) });
            connectionsDM.Add(GetLocationByMem(0x6129), new List<Location>() { GetLocationByMem(0x612A), GetLocationByMem(0x612B), GetLocationByMem(0x612C) });
            connectionsDM.Add(GetLocationByMem(0x612D), new List<Location>() { GetLocationByMem(0x612E), GetLocationByMem(0x612F), GetLocationByMem(0x6130) });
            connectionsDM.Add(GetLocationByMem(0x612E), new List<Location>() { GetLocationByMem(0x612D), GetLocationByMem(0x612F), GetLocationByMem(0x6130) });
            connectionsDM.Add(GetLocationByMem(0x612F), new List<Location>() { GetLocationByMem(0x612E), GetLocationByMem(0x612D), GetLocationByMem(0x6130) });
            connectionsDM.Add(GetLocationByMem(0x6130), new List<Location>() { GetLocationByMem(0x612E), GetLocationByMem(0x612F), GetLocationByMem(0x612D) });

            enemies = new List<int> { 3, 4, 5, 17, 18, 20, 21, 22, 23, 24, 25, 26, 27, 28, 31, 32 };
            flyingEnemies = new List<int> { 0x06, 0x07, 0x0A, 0x0D, 0x0E };
            generators = new List<int> { 11, 12, 15, 29 };
            shorties = new List<int> { 3, 4, 5, 17, 18, 0x1C, 0x1F };
            tallGuys = new List<int> { 0x20, 20, 21, 22, 23, 24, 25, 26, 27 };
            enemyAddr = 0x48B0;
            enemyPtr = 0x608E;

            overworldMaps = new List<int>();
            MAP_ROWS = 45;
            MAP_COLS = 64;

            baseAddr = 0x610C;
            VANILLA_MAP_ADDR = 0x665c;

            if (hy.Props.dmBiome.Equals("Islands"))
            {
                this.biome = Biome.islands;
            }
            else if (hy.Props.dmBiome.Equals("Canyon") || hy.Props.dmBiome.Equals("CanyonD"))
            {
                this.biome = Biome.canyon;
                //MAP_ROWS = 75;
            }
            else if(hy.Props.dmBiome.Equals("Caldera"))
            {
                this.biome = Biome.caldera;
            }
            else if(hy.Props.dmBiome.Equals("Mountainous"))
            {
                this.biome = Biome.mountainous;
            }
            else if(hy.Props.dmBiome.Equals("Vanilla"))
            {
                this.biome = Biome.vanilla;
            }
            else if(hy.Props.dmBiome.Equals("Vanilla (shuffled)"))
            {
                this.biome = Biome.vanillaShuffle;
            }
            else
            {
                this.biome = Biome.vanillalike;
            }
            walkable = new List<Terrain>() { Terrain.DESERT, Terrain.FOREST, Terrain.GRAVE };
            randomTerrains = new List<Terrain>() { Terrain.DESERT, Terrain.FOREST, Terrain.GRAVE, Terrain.MOUNAIN, Terrain.WALKABLEWATER, Terrain.WATER };
        
            
        }

        public bool Terraform()
        {
            Terrain water = Terrain.WATER;
            if (hy.Props.bootsWater)
            {
                water = Terrain.WALKABLEWATER;
            }
            walkable = new List<Terrain>() { Terrain.DESERT, Terrain.FOREST, Terrain.GRAVE };
            randomTerrains = new List<Terrain>() { Terrain.DESERT, Terrain.FOREST, Terrain.GRAVE, Terrain.MOUNAIN, water };

            foreach (Location location in AllLocations)
            {
                location.CanShuffle = true;
            }
            if (this.biome == Biome.vanilla || this.biome == Biome.vanillaShuffle)
            {
                MAP_ROWS = 75;
                MAP_COLS = 64;
                ReadVanillaMap();
                if (this.biome == Biome.vanillaShuffle)
                {
                    ShuffleLocations(AllLocations);
                    if (hy.Props.vanillaOriginal)
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


                bcount = 2000;
                bool horizontal = false;
                while (bcount > MAP_SIZE_BYTES)
                {
                    map = new Terrain[MAP_ROWS, MAP_COLS];
                    Terrain riverT = Terrain.MOUNAIN;
                    if (this.biome != Biome.canyon && this.biome != Biome.caldera && this.biome != Biome.islands)
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

                    if (this.biome == Biome.islands)
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

                        int cols = hy.RNG.Next(2, 4);
                        int rows = hy.RNG.Next(2, 4);
                        List<int> pickedC = new List<int>();
                        List<int> pickedR = new List<int>();

                        while (cols > 0)
                        {
                            int col = hy.RNG.Next(1, MAP_COLS);
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
                            int row = hy.RNG.Next(5, MAP_ROWS - 6);
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
                                        int adjust = hy.RNG.Next(-3, 4);
                                        while (row + adjust < 1 || row + adjust > MAP_ROWS - 2)
                                        {
                                            adjust = hy.RNG.Next(-3, 4);
                                        }
                                        row += adjust;
                                    }
                                }
                                pickedR.Add(row);
                                rows--;
                            }
                        }



                    }
                    else if (this.biome == Biome.canyon)
                    {
                        riverT = water;
                        horizontal = hy.RNG.NextDouble() > 0.5;

                        if (hy.Props.westBiome.Equals("CanyonD"))
                        {
                            riverT = Terrain.DESERT;
                        }

                        this.walkable.Clear();
                        this.walkable.Add(Terrain.GRASS);
                        this.walkable.Add(Terrain.GRAVE);
                        this.walkable.Add(Terrain.FOREST);
                        this.walkable.Add(Terrain.DESERT);
                        this.walkable.Add(Terrain.MOUNAIN);

                        this.randomTerrains.Remove(Terrain.SWAMP);
                        DrawCanyon(riverT);
                        this.walkable.Remove(Terrain.MOUNAIN);

                        this.randomTerrains.Remove(Terrain.SWAMP);
                        //this.randomTerrains.Add(terrain.lava);

                    }
                    else if (this.biome == Biome.caldera)
                    {
                        this.horizontal = hy.RNG.NextDouble() > .5;
                        DrawCenterMountain();
                    }
                    else if (this.biome == Biome.mountainous)
                    {
                        riverT = Terrain.MOUNAIN;
                        for (int i = 0; i < MAP_COLS; i++)
                        {
                            map[0, i] = Terrain.MOUNAIN;
                            map[MAP_ROWS - 1, i] = Terrain.MOUNAIN;
                        }

                        for (int i = 0; i < MAP_ROWS; i++)
                        {
                            map[i, 0] = Terrain.MOUNAIN;
                            map[i, MAP_COLS - 1] = Terrain.MOUNAIN;
                        }


                        int cols = hy.RNG.Next(2, 4);
                        int rows = hy.RNG.Next(2, 4);
                        List<int> pickedC = new List<int>();
                        List<int> pickedR = new List<int>();

                        while (cols > 0)
                        {
                            int col = hy.RNG.Next(10, MAP_COLS - 11);
                            if (!pickedC.Contains(col))
                            {
                                for (int i = 0; i < MAP_ROWS; i++)
                                {
                                    if (map[i, col] == Terrain.NONE)
                                    {
                                        map[i, col] = Terrain.MOUNAIN;
                                    }
                                }
                                pickedC.Add(col);
                                cols--;
                            }
                        }

                        while (rows > 0)
                        {
                            int row = hy.RNG.Next(10, MAP_ROWS - 11);
                            if (!pickedR.Contains(row))
                            {
                                for (int i = 0; i < MAP_COLS; i++)
                                {
                                    if (map[row, i] == Terrain.NONE)
                                    {
                                        map[row, i] = Terrain.MOUNAIN;
                                    }
                                }
                                pickedR.Add(row);
                                rows--;
                            }
                        }
                    }

                    Direction rDir = (Direction)hy.RNG.Next(4);
                    if (this.biome == Biome.canyon)
                    {
                        rDir = (Direction)hy.RNG.Next(2);
                        if (horizontal)
                        {
                            rDir += 2;
                        }
                    }
                    if (raft != null)
                    {
                        if (this.biome != Biome.canyon && this.biome != Biome.caldera)
                        {
                            MAP_COLS = 29;
                        }
                        DrawOcean(rDir);
                        MAP_COLS = 64;
                    }




                    Direction bDir = (Direction)hy.RNG.Next(4);
                    do
                    {
                        if (this.biome != Biome.canyon)
                        {
                            bDir = (Direction)hy.RNG.Next(4);
                        }
                        else
                        {
                            bDir = (Direction)hy.RNG.Next(2);
                            if (horizontal)
                            {
                                bDir += 2;
                            }
                        }
                    } while (bDir == rDir);
                    if (bridge != null)
                    {
                        if (this.biome != Biome.canyon && this.biome != Biome.caldera)
                        {
                            MAP_COLS = 29;
                        }
                        DrawOcean(bDir);
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
                                x = hy.RNG.Next(MAP_COLS - 2) + 1;
                                y = hy.RNG.Next(MAP_ROWS - 2) + 1;
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

                                int dir = hy.RNG.Next(4);

                                Terrain s = walkable[hy.RNG.Next(walkable.Count)];
                                if (this.biome == Biome.vanillalike)
                                {
                                    s = Terrain.ROAD;
                                }
                                if (!hy.Props.saneCaves || !connectionsDM.ContainsKey(location))
                                {
                                    PlaceCave(x, y, dir, s);
                                }
                                else
                                {
                                    if ((location.HorizontalPos == 0 || location.FallInHole != 0) && location.ForceEnterRight == 0)
                                    {
                                        if (dir == 0)
                                        {
                                            dir = 1;
                                        }

                                        if (dir == 3)
                                        {
                                            dir = 2;
                                        }
                                    }
                                    else
                                    {
                                        if (dir == 1)
                                        {
                                            dir = 0;
                                        }

                                        if (dir == 2)
                                        {
                                            dir = 3;
                                        }
                                    }
                                    map[y, x] = Terrain.NONE;

                                    tries = 0;
                                    do
                                    {
                                        x = hy.RNG.Next(MAP_COLS - 2) + 1;
                                        y = hy.RNG.Next(MAP_ROWS - 2) + 1;
                                        tries++;
                                    } while ((x < 5 || y < 5 || x > MAP_COLS - 5 || y > MAP_ROWS - 5 || map[y, x] != Terrain.NONE || map[y - 1, x] != Terrain.NONE || map[y + 1, x] != Terrain.NONE || map[y + 1, x + 1] != Terrain.NONE || map[y, x + 1] != Terrain.NONE || map[y - 1, x + 1] != Terrain.NONE || map[y + 1, x - 1] != Terrain.NONE || map[y, x - 1] != Terrain.NONE || map[y - 1, x - 1] != Terrain.NONE) && tries < 100);
                                    if (tries >= 100)
                                    {
                                        return false;
                                    }

                                    while ((dir == 0 && y < 15) || (dir == 1 && x > MAP_COLS - 15) || (dir == 2 && y > MAP_ROWS - 15) || (dir == 3 && x < 15))
                                    {
                                        dir = hy.RNG.Next(4);
                                    }
                                    int otherdir = (dir + 2) % 4;
                                    if (connectionsDM[location].Count == 1)
                                    {
                                        int otherx = 0;
                                        int othery = 0;
                                        tries = 0;
                                        Boolean crossing = true;
                                        do
                                        {
                                            int range = 7;
                                            int offset = 3;
                                            if (biome == Biome.islands)
                                            {
                                                range = 7;
                                                offset = 5;
                                            }
                                            crossing = true;
                                            if (dir == 0) //south
                                            {
                                                otherx = x + (hy.RNG.Next(7) - 3);
                                                othery = y - (hy.RNG.Next(range) + offset);
                                            }
                                            else if (dir == 1) //west
                                            {
                                                otherx = x + (hy.RNG.Next(range) + offset);
                                                othery = y + (hy.RNG.Next(7) - 3);
                                            }
                                            else if (dir == 2) //north
                                            {
                                                otherx = x + (hy.RNG.Next(7) - 3);
                                                othery = y + (hy.RNG.Next(range) + offset);
                                            }
                                            else //east
                                            {
                                                otherx = x - (hy.RNG.Next(range) + offset);
                                                othery = y + (hy.RNG.Next(7) - 3);
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
                                        PlaceCave(x, y, dir, s);
                                        PlaceCave(otherx, othery, otherdir, s);
                                    }
                                    else
                                    {
                                        int otherx = 0;
                                        int othery = 0;
                                        tries = 0;
                                        Boolean crossing = true;
                                        do
                                        {
                                            int range = 7;
                                            int offset = 3;
                                            if (biome == Biome.islands)
                                            {
                                                range = 7;
                                                offset = 5;
                                            }
                                            crossing = true;
                                            if (dir == 0) //south
                                            {
                                                otherx = x + (hy.RNG.Next(7) - 3);
                                                othery = y - (hy.RNG.Next(range) + offset);
                                            }
                                            else if (dir == 1) //west
                                            {
                                                otherx = x + (hy.RNG.Next(range) + offset);
                                                othery = y + (hy.RNG.Next(7) - 3);
                                            }
                                            else if (dir == 2) //north
                                            {
                                                otherx = x + (hy.RNG.Next(7) - 3);
                                                othery = y + (hy.RNG.Next(range) + offset);
                                            }
                                            else //east
                                            {
                                                otherx = x - (hy.RNG.Next(range) + offset);
                                                othery = y + (hy.RNG.Next(7) - 3);
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
                                        PlaceCave(x, y, dir, s);
                                        PlaceCave(otherx, othery, otherdir, s);
                                        int newx = 0;
                                        int newy = 0;
                                        tries = 0;
                                        do
                                        {
                                            newx = x + hy.RNG.Next(7) - 3;
                                            newy = y + hy.RNG.Next(7) - 3;
                                            tries++;
                                        } while (newx > 2 && newx < MAP_COLS - 2 && newy > 2 && newy < MAP_ROWS - 2 && (map[newy, newx] != Terrain.NONE || map[newy - 1, newx] != Terrain.NONE || map[newy + 1, newx] != Terrain.NONE || map[newy + 1, newx + 1] != Terrain.NONE || map[newy, newx + 1] != Terrain.NONE || map[newy - 1, newx + 1] != Terrain.NONE || map[newy + 1, newx - 1] != Terrain.NONE || map[newy, newx - 1] != Terrain.NONE || map[newy - 1, newx - 1] != Terrain.NONE) && tries < 100);
                                        if (tries >= 100)
                                        {
                                            return false;
                                        }
                                        l2[1].Xpos = newx;
                                        l2[1].Ypos = newy + 30;
                                        l2[1].CanShuffle = false;
                                        PlaceCave(newx, newy, dir, s);
                                        y = newy;
                                        x = newx;

                                        tries = 0;
                                        do
                                        {
                                            int range = 7;
                                            int offset = 3;
                                            if (biome == Biome.islands)
                                            {
                                                range = 7;
                                                offset = 5;
                                            }
                                            crossing = true;
                                            if (dir == 0) //south
                                            {
                                                otherx = x + (hy.RNG.Next(7) - 3);
                                                othery = y - (hy.RNG.Next(range) + offset);
                                            }
                                            else if (dir == 1) //west
                                            {
                                                otherx = x + (hy.RNG.Next(range) + offset);
                                                othery = y + (hy.RNG.Next(7) - 3);
                                            }
                                            else if (dir == 2) //north
                                            {
                                                otherx = x + (hy.RNG.Next(7) - 3);
                                                othery = y + (hy.RNG.Next(range) + offset);
                                            }
                                            else //east
                                            {
                                                otherx = x - (hy.RNG.Next(range) + offset);
                                                othery = y + (hy.RNG.Next(7) - 3);
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
                                        PlaceCave(otherx, othery, otherdir, s);
                                    }

                                }
                            }
                        }
                    }


                    if (this.biome == Biome.vanillalike)
                    {
                        PlaceRandomTerrain(5);
                    }
                    randomTerrains.Add(Terrain.ROAD);
                    if (!GrowTerrain())
                    {
                        return false;
                    }
                    if (this.biome == Biome.caldera)
                    {
                        bool f = MakeCaldera();
                        if (!f)
                        {
                            return false;
                        }
                    }
                    walkable.Add(Terrain.ROAD);
                    if (raft != null)
                    {
                        if (this.biome != Biome.caldera && this.biome != Biome.canyon)
                        {
                            MAP_COLS = 29;
                        }
                        Boolean r = DrawRaft(false, rDir);
                        MAP_COLS = 64;
                        if (!r)
                        {
                            return false;
                        }
                    }

                    if (bridge != null)
                    {
                        if (this.biome != Biome.caldera && this.biome != Biome.canyon)
                        {
                            MAP_COLS = 29;
                        }
                        Boolean b = DrawRaft(true, bDir);
                        MAP_COLS = 64;
                        if (!b)
                        {
                            return false;
                        }
                    }


                    do
                    {
                        x = hy.RNG.Next(MAP_COLS - 2) + 1;
                        y = hy.RNG.Next(MAP_ROWS - 2) + 1;
                    } while (!walkable.Contains(map[y, x]) || map[y + 1, x] == Terrain.CAVE || map[y - 1, x] == Terrain.CAVE || map[y, x + 1] == Terrain.CAVE || map[y, x - 1] == Terrain.CAVE);

                    map[y, x] = Terrain.ROCK;
                    magicCave.Ypos = y + 30;
                    magicCave.Xpos = x;


                    if (this.biome == Biome.canyon || this.biome == Biome.islands)
                    {
                        ConnectIslands(25, false, riverT, false, false, false);
                    }

                    //check bytes and adjust
                    WriteBytes(false, MAP_ADDR, MAP_SIZE_BYTES, 0, 0);
                }
            }
            WriteBytes(true, MAP_ADDR, MAP_SIZE_BYTES, 0, 0);
            

            v = new bool[MAP_ROWS, MAP_COLS];
            for (int i = 0; i < MAP_ROWS; i++)
            {
                for (int j = 0; j < MAP_COLS; j++)
                {
                    v[i, j] = false;
                }
            }

            for (int i = 0x610C; i < 0x614B; i++)
            {
                if (!terrains.Keys.Contains(i))
                {
                    hy.ROMData.Put(i, 0x00);
                }
            }
            return true;
        }
        private bool MakeCaldera()
        {
            Terrain water = Terrain.WATER;
            if (hy.Props.bootsWater)
            {
                water = Terrain.WALKABLEWATER;
            }
            int centerx = hy.RNG.Next(21, 41);
            int centery = hy.RNG.Next(17, 27);
            if (horizontal)
            {
                centerx = hy.RNG.Next(27, 37);
                centery = hy.RNG.Next(17, 27);
            }

            bool placeable = false;
            do
            {
                if (horizontal)
                {
                    centerx = hy.RNG.Next(27, 37);
                    centery = hy.RNG.Next(17, 27);
                }
                else
                {
                    centerx = hy.RNG.Next(21, 41);
                    centery = hy.RNG.Next(17, 27);
                }
                placeable = true;
                for (int i = centery - 7; i < centery + 8; i++)
                {
                    for (int j = centerx - 7; j < centerx + 8; j++)
                    {
                        if (map[i, j] != Terrain.MOUNAIN)
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
            if (!horizontal)
            {
                startx = centerx;
                starty = centery - 5;
                deltax = 0;
                deltay = 1;
            }
            for (int i = 0; i < 10; i++)
            {
                int lake = hy.RNG.Next(7, 11);
                if (i == 0 || i == 9)
                {
                    lake = hy.RNG.Next(3, 6);
                }
                if (horizontal)
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
                    while (map[top, startx - 1] == Terrain.MOUNAIN)
                    {
                        map[top, startx - 1] = Terrain.FOREST;
                        top--;
                    }
                    top = starty + lake / 2;
                    while (map[top, startx - 1] != Terrain.MOUNAIN)
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
                    while (map[top, startx - 1] == Terrain.MOUNAIN)
                    {
                        map[top, startx - 1] = Terrain.FOREST;
                        top++;
                    }
                    top = starty - (lake - (lake / 2));
                    while (map[top, startx - 1] != Terrain.MOUNAIN)
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
                    while (map[starty - 1, top] == Terrain.MOUNAIN && i != 0)
                    {
                        map[starty - 1, top] = Terrain.FOREST;
                        top--;
                    }
                    top = startx + lake / 2;
                    while (map[starty - 1, top] != Terrain.MOUNAIN && i != 0)
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
                    while (map[starty - 1, top] == Terrain.MOUNAIN && i != 0)
                    {
                        map[starty - 1, top] = Terrain.FOREST;
                        top++;
                    }
                    top = startx - (lake - (lake / 2));
                    while (map[starty - 1, top] != Terrain.MOUNAIN && i != 0)
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
                int cavenum1 = hy.RNG.Next(connectionsDM.Keys.Count);
                cave1l = connectionsDM.Keys.ToList()[cavenum1];
                cave1r = connectionsDM[cave1l][0];
            } while (connectionsDM[cave1l].Count != 1 || connectionsDM[cave1r].Count != 1);

            do
            {
                int cavenum1 = hy.RNG.Next(connectionsDM.Keys.Count);
                cave2l = connectionsDM.Keys.ToList()[cavenum1];
                cave2r = connectionsDM[cave2l][0];
            } while (connectionsDM[cave2l].Count != 1 || cave1l == cave2l || cave1l == cave2r);
            cave1l.CanShuffle = false;
            cave1r.CanShuffle = false;
            cave2l.CanShuffle = false;
            cave2r.CanShuffle = false;
            map[cave1l.Ypos - 30, cave1l.Xpos] = Terrain.MOUNAIN;
            map[cave2l.Ypos - 30, cave2l.Xpos] = Terrain.MOUNAIN;

            map[cave1r.Ypos - 30, cave1r.Xpos] = Terrain.MOUNAIN;

            map[cave2r.Ypos - 30, cave2r.Xpos] = Terrain.MOUNAIN;


            int caveDir = hy.RNG.Next(2);
            if (horizontal)
            {
                bool f = HorizontalCave(caveDir, centerx, centery, cave1l, cave1r);
                if (!f)
                {
                    return false;
                }

                if (caveDir == 0)
                {
                    caveDir = 1;
                }
                else
                {
                    caveDir = 0;
                }
                f = HorizontalCave(caveDir, centerx, centery, cave2l, cave2r);
                if (!f)
                {
                    return false;
                }
            }
            else
            {
                bool f = VerticalCave(caveDir, centerx, centery, cave1l, cave1r);
                if (!f)
                {
                    return false;
                }
                if (caveDir == 0)
                {
                    caveDir = 1;
                }
                else
                {
                    caveDir = 0;
                }
                f = VerticalCave(caveDir, centerx, centery, cave2l, cave2r);
                if (!f)
                {
                    return false;
                }
                
            }
            return true;
        }
        public void UpdateVisit()
        {
            UpdateReachable();

            foreach (Location location in AllLocations)
            {
                if (v[location.Ypos - 30, location.Xpos])
                {
                    location.Reachable = true;
                    if (connectionsDM.Keys.Contains(location))
                    {
                        List<Location> l2 = connectionsDM[location];

                        foreach(Location l3 in l2)
                        { 
                            l3.Reachable = true;
                            v[l3.Ypos - 30, l3.Xpos] = true;
                        }
                    }
                }
            }
        }
    }
}
