﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Z2Randomizer
{
    class DeathMountain : World
    {

        private readonly SortedDictionary<int, terrain> terrains = new SortedDictionary<int, terrain>
            {
                { 0x610C, terrain.cave },
                { 0x610D, terrain.cave },
                { 0x610E, terrain.cave },
                { 0x610F, terrain.cave },
                { 0x6110, terrain.cave },
                { 0x6111, terrain.cave },
                { 0x6112, terrain.cave },
                { 0x6113, terrain.cave },
                { 0x6114, terrain.cave },
                { 0x6115, terrain.cave },
                { 0x6116, terrain.cave },
                { 0x6117, terrain.cave },
                { 0x6118, terrain.cave },
                { 0x6119, terrain.cave },
                { 0x611A, terrain.cave },
                { 0x611B, terrain.cave },
                { 0x611C, terrain.cave },
                { 0x611D, terrain.cave },
                { 0x611E, terrain.cave },
                { 0x611F, terrain.cave },
                { 0x6120, terrain.cave },
                { 0x6121, terrain.cave },
                { 0x6122, terrain.cave },
                { 0x6123, terrain.cave },
                { 0x6124, terrain.cave },
                { 0x6125, terrain.cave },
                { 0x6126, terrain.cave },
                { 0x6127, terrain.cave },
                { 0x6128, terrain.cave },
                { 0x6129, terrain.cave },
                { 0x612A, terrain.cave },
                { 0x612B, terrain.cave },
                { 0x612C, terrain.cave },
                { 0x612D, terrain.cave },
                { 0x612E, terrain.cave },
                { 0x612F, terrain.cave },
                { 0x6130, terrain.cave },
                { 0x6136, terrain.cave },
                { 0x6137, terrain.cave },
                { 0x6144, terrain.cave }
        };

        public Dictionary<Location, List<Location>> connectionsDM;
        public Location hammerCave;
        public Location magicCave;

        private const int MAP_ADDR = 0x7a00;

        public DeathMountain(Hyrule hy)
            : base(hy)
        {
            loadLocations(0x610C, 37, terrains, continent.dm);
            //loadLocations(0x6136, 2, terrains, continent.dm);
            loadLocations(0x6144, 1, terrains, continent.dm);

            hammerCave = getLocationByMem(0x6128);
            magicCave = getLocationByMem(0x6144);

            reachableAreas = new HashSet<string>();
            connectionsDM = new Dictionary<Location, List<Location>>();
            connectionsDM.Add(getLocationByMem(0x610C), new List<Location>() { getLocationByMem(0x610D) });
            connectionsDM.Add(getLocationByMem(0x610D), new List<Location>() { getLocationByMem(0x610C) });
            connectionsDM.Add(getLocationByMem(0x610E), new List<Location>() { getLocationByMem(0x610F) });
            connectionsDM.Add(getLocationByMem(0x610F), new List<Location>() { getLocationByMem(0x610E) });
            connectionsDM.Add(getLocationByMem(0x6110), new List<Location>() { getLocationByMem(0x6111) });
            connectionsDM.Add(getLocationByMem(0x6111), new List<Location>() { getLocationByMem(0x6110) });
            connectionsDM.Add(getLocationByMem(0x6112), new List<Location>() { getLocationByMem(0x6113) });
            connectionsDM.Add(getLocationByMem(0x6113), new List<Location>() { getLocationByMem(0x6112) });
            connectionsDM.Add(getLocationByMem(0x6114), new List<Location>() { getLocationByMem(0x6115) });
            connectionsDM.Add(getLocationByMem(0x6115), new List<Location>() { getLocationByMem(0x6114) });
            connectionsDM.Add(getLocationByMem(0x6116), new List<Location>() { getLocationByMem(0x6117) });
            connectionsDM.Add(getLocationByMem(0x6117), new List<Location>() { getLocationByMem(0x6116) });
            connectionsDM.Add(getLocationByMem(0x6118), new List<Location>() { getLocationByMem(0x6119) });
            connectionsDM.Add(getLocationByMem(0x6119), new List<Location>() { getLocationByMem(0x6118) });
            connectionsDM.Add(getLocationByMem(0x611A), new List<Location>() { getLocationByMem(0x611B) });
            connectionsDM.Add(getLocationByMem(0x611B), new List<Location>() { getLocationByMem(0x611A) });
            connectionsDM.Add(getLocationByMem(0x611C), new List<Location>() { getLocationByMem(0x611D) });
            connectionsDM.Add(getLocationByMem(0x611D), new List<Location>() { getLocationByMem(0x611C) });
            connectionsDM.Add(getLocationByMem(0x611E), new List<Location>() { getLocationByMem(0x611F) });
            connectionsDM.Add(getLocationByMem(0x611F), new List<Location>() { getLocationByMem(0x611E) });
            connectionsDM.Add(getLocationByMem(0x6120), new List<Location>() { getLocationByMem(0x6121) });
            connectionsDM.Add(getLocationByMem(0x6121), new List<Location>() { getLocationByMem(0x6120) });
            connectionsDM.Add(getLocationByMem(0x6122), new List<Location>() { getLocationByMem(0x6123) });
            connectionsDM.Add(getLocationByMem(0x6123), new List<Location>() { getLocationByMem(0x6122) });
            connectionsDM.Add(getLocationByMem(0x6124), new List<Location>() { getLocationByMem(0x6125) });
            connectionsDM.Add(getLocationByMem(0x6125), new List<Location>() { getLocationByMem(0x6124) });
            connectionsDM.Add(getLocationByMem(0x6126), new List<Location>() { getLocationByMem(0x6127) });
            connectionsDM.Add(getLocationByMem(0x6127), new List<Location>() { getLocationByMem(0x6126) });
            connectionsDM.Add(getLocationByMem(0x6129), new List<Location>() { getLocationByMem(0x612A), getLocationByMem(0x612B), getLocationByMem(0x612C) });
            connectionsDM.Add(getLocationByMem(0x612D), new List<Location>() { getLocationByMem(0x612E), getLocationByMem(0x612F), getLocationByMem(0x6130) });
            connectionsDM.Add(getLocationByMem(0x612E), new List<Location>() { getLocationByMem(0x612D), getLocationByMem(0x612F), getLocationByMem(0x6130) });
            connectionsDM.Add(getLocationByMem(0x612F), new List<Location>() { getLocationByMem(0x612E), getLocationByMem(0x612D), getLocationByMem(0x6130) });
            connectionsDM.Add(getLocationByMem(0x6130), new List<Location>() { getLocationByMem(0x612E), getLocationByMem(0x612F), getLocationByMem(0x612D) });

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
                this.bio = biome.islands;
            }
            else if (hy.Props.dmBiome.Equals("Canyon") || hy.Props.dmBiome.Equals("CanyonD"))
            {
                this.bio = biome.canyon;
                //MAP_ROWS = 75;
            }
            else if(hy.Props.dmBiome.Equals("Caldera"))
            {
                this.bio = biome.caldera;
            }
            else if(hy.Props.dmBiome.Equals("Mountainous"))
            {
                this.bio = biome.mountainous;
            }
            else if(hy.Props.dmBiome.Equals("Vanilla"))
            {
                this.bio = biome.vanilla;
            }
            else if(hy.Props.dmBiome.Equals("Vanilla (shuffled)"))
            {
                this.bio = biome.vanillaShuffle;
            }
            else
            {
                this.bio = biome.vanillalike;
            }
            walkable = new List<terrain>() { terrain.desert, terrain.forest, terrain.grave };
            randomTerrains = new List<terrain>() { terrain.desert, terrain.forest, terrain.grave, terrain.mountain, terrain.walkablewater, terrain.water };
        
            
        }

        public bool terraform()
        {
            terrain water = terrain.water;
            if (hy.Props.bootsWater)
            {
                water = terrain.walkablewater;
            }
            walkable = new List<terrain>() { terrain.desert, terrain.forest, terrain.grave };
            randomTerrains = new List<terrain>() { terrain.desert, terrain.forest, terrain.grave, terrain.mountain, water };

            foreach (Location l in AllLocations)
            {
                l.CanShuffle = true;
            }
            if (this.bio == biome.vanilla || this.bio == biome.vanillaShuffle)
            {
                MAP_ROWS = 75;
                MAP_COLS = 64;
                readVanillaMap();
                if (this.bio == biome.vanillaShuffle)
                {
                    shuffleLocations(AllLocations);
                    if (hy.Props.vanillaOriginal)
                    {
                        magicCave.TerrainType = terrain.rock;
                        foreach (Location l in AllLocations)
                        {
                            map[l.Ypos - 30, l.Xpos] = l.TerrainType;
                        }
                    }
                    foreach (Location l in Caves)
                    {
                        l.PassThrough = 0;
                    }
                    foreach (Location l in Towns)
                    {
                        l.PassThrough = 0;
                    }
                    foreach (Location l in Palaces)
                    {
                        l.PassThrough = 0;
                    }
                }
            }
            else
            {


                bcount = 2000;
                bool horizontal = false;
                while (bcount > MAP_SIZE_BYTES)
                {
                    map = new terrain[MAP_ROWS, MAP_COLS];
                    terrain riverT = terrain.mountain;
                    if (this.bio != biome.canyon && this.bio != biome.caldera && this.bio != biome.islands)
                    {
                        for (int i = 0; i < MAP_ROWS; i++)
                        {
                            for (int j = 0; j < 29; j++)
                            {
                                map[i, j] = terrain.none;
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
                                map[i, j] = terrain.none;
                            }
                        }
                    }

                    if (this.bio == biome.islands)
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

                        int cols = hy.R.Next(2, 4);
                        int rows = hy.R.Next(2, 4);
                        List<int> pickedC = new List<int>();
                        List<int> pickedR = new List<int>();

                        while (cols > 0)
                        {
                            int col = hy.R.Next(1, MAP_COLS);
                            if (!pickedC.Contains(col))
                            {
                                for (int i = 0; i < MAP_ROWS; i++)
                                {
                                    if (map[i, col] == terrain.none)
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
                            int row = hy.R.Next(5, MAP_ROWS - 6);
                            if (!pickedR.Contains(row))
                            {
                                for (int i = 0; i < MAP_COLS; i++)
                                {
                                    if (map[row, i] == terrain.none)
                                    {
                                        map[row, i] = water;
                                    }
                                    else if (map[row, i] == water)
                                    {
                                        int adjust = hy.R.Next(-3, 4);
                                        while (row + adjust < 1 || row + adjust > MAP_ROWS - 2)
                                        {
                                            adjust = hy.R.Next(-3, 4);
                                        }
                                        row += adjust;
                                    }
                                }
                                pickedR.Add(row);
                                rows--;
                            }
                        }



                    }
                    else if (this.bio == biome.canyon)
                    {
                        riverT = water;
                        horizontal = hy.R.NextDouble() > 0.5;

                        if (hy.Props.westBiome.Equals("CanyonD"))
                        {
                            riverT = terrain.desert;
                        }

                        this.walkable.Clear();
                        this.walkable.Add(terrain.grass);
                        this.walkable.Add(terrain.grave);
                        this.walkable.Add(terrain.forest);
                        this.walkable.Add(terrain.desert);
                        this.walkable.Add(terrain.mountain);

                        this.randomTerrains.Remove(terrain.swamp);
                        drawCanyon(riverT);
                        this.walkable.Remove(terrain.mountain);

                        this.randomTerrains.Remove(terrain.swamp);
                        //this.randomTerrains.Add(terrain.lava);

                    }
                    else if (this.bio == biome.caldera)
                    {
                        this.horizontal = hy.R.NextDouble() > .5;
                        drawCenterMountain();
                    }
                    else if (this.bio == biome.mountainous)
                    {
                        riverT = terrain.mountain;
                        for (int i = 0; i < MAP_COLS; i++)
                        {
                            map[0, i] = terrain.mountain;
                            map[MAP_ROWS - 1, i] = terrain.mountain;
                        }

                        for (int i = 0; i < MAP_ROWS; i++)
                        {
                            map[i, 0] = terrain.mountain;
                            map[i, MAP_COLS - 1] = terrain.mountain;
                        }


                        int cols = hy.R.Next(2, 4);
                        int rows = hy.R.Next(2, 4);
                        List<int> pickedC = new List<int>();
                        List<int> pickedR = new List<int>();

                        while (cols > 0)
                        {
                            int col = hy.R.Next(10, MAP_COLS - 11);
                            if (!pickedC.Contains(col))
                            {
                                for (int i = 0; i < MAP_ROWS; i++)
                                {
                                    if (map[i, col] == terrain.none)
                                    {
                                        map[i, col] = terrain.mountain;
                                    }
                                }
                                pickedC.Add(col);
                                cols--;
                            }
                        }

                        while (rows > 0)
                        {
                            int row = hy.R.Next(10, MAP_ROWS - 11);
                            if (!pickedR.Contains(row))
                            {
                                for (int i = 0; i < MAP_COLS; i++)
                                {
                                    if (map[row, i] == terrain.none)
                                    {
                                        map[row, i] = terrain.mountain;
                                    }
                                }
                                pickedR.Add(row);
                                rows--;
                            }
                        }
                    }

                    direction rDir = (direction)hy.R.Next(4);
                    if (this.bio == biome.canyon)
                    {
                        rDir = (direction)hy.R.Next(2);
                        if (horizontal)
                        {
                            rDir += 2;
                        }
                    }
                    if (raft != null)
                    {
                        if (this.bio != biome.canyon && this.bio != biome.caldera)
                        {
                            MAP_COLS = 29;
                        }
                        drawOcean(rDir);
                        MAP_COLS = 64;
                    }




                    direction bDir = (direction)hy.R.Next(4);
                    do
                    {
                        if (this.bio != biome.canyon)
                        {
                            bDir = (direction)hy.R.Next(4);
                        }
                        else
                        {
                            bDir = (direction)hy.R.Next(2);
                            if (horizontal)
                            {
                                bDir += 2;
                            }
                        }
                    } while (bDir == rDir);
                    if (bridge != null)
                    {
                        if (this.bio != biome.canyon && this.bio != biome.caldera)
                        {
                            MAP_COLS = 29;
                        }
                        drawOcean(bDir);
                        MAP_COLS = 64;
                    }
                    int x = 0;
                    int y = 0;
                    foreach (Location l in AllLocations)
                    {
                        if (l.TerrainType != terrain.bridge && l != magicCave && l.CanShuffle)
                        {
                            int tries = 0;
                            do
                            {
                                x = hy.R.Next(MAP_COLS - 2) + 1;
                                y = hy.R.Next(MAP_ROWS - 2) + 1;
                                tries++;
                            } while ((map[y, x] != terrain.none || map[y - 1, x] != terrain.none || map[y + 1, x] != terrain.none || map[y + 1, x + 1] != terrain.none || map[y, x + 1] != terrain.none || map[y - 1, x + 1] != terrain.none || map[y + 1, x - 1] != terrain.none || map[y, x - 1] != terrain.none || map[y - 1, x - 1] != terrain.none) && tries < 100);
                            if (tries >= 100)
                            {
                                return false;
                            }

                            map[y, x] = l.TerrainType;
                            l.Xpos = x;
                            l.Ypos = y + 30;
                            if (l.TerrainType == terrain.cave)
                            {

                                int dir = hy.R.Next(4);

                                terrain s = walkable[hy.R.Next(walkable.Count)];
                                if (this.bio == biome.vanillalike)
                                {
                                    s = terrain.road;
                                }
                                if (!hy.Props.saneCaves || !connectionsDM.ContainsKey(l))
                                {
                                    placeCave(x, y, dir, s);
                                }
                                else
                                {
                                    if ((l.HorizontalPos == 0 || l.FallInHole != 0) && l.ForceEnterRight == 0)
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
                                    map[y, x] = terrain.none;

                                    tries = 0;
                                    do
                                    {
                                        x = hy.R.Next(MAP_COLS - 2) + 1;
                                        y = hy.R.Next(MAP_ROWS - 2) + 1;
                                        tries++;
                                    } while ((x < 5 || y < 5 || x > MAP_COLS - 5 || y > MAP_ROWS - 5 || map[y, x] != terrain.none || map[y - 1, x] != terrain.none || map[y + 1, x] != terrain.none || map[y + 1, x + 1] != terrain.none || map[y, x + 1] != terrain.none || map[y - 1, x + 1] != terrain.none || map[y + 1, x - 1] != terrain.none || map[y, x - 1] != terrain.none || map[y - 1, x - 1] != terrain.none) && tries < 100);
                                    if (tries >= 100)
                                    {
                                        return false;
                                    }

                                    while ((dir == 0 && y < 15) || (dir == 1 && x > MAP_COLS - 15) || (dir == 2 && y > MAP_ROWS - 15) || (dir == 3 && x < 15))
                                    {
                                        dir = hy.R.Next(4);
                                    }
                                    int otherdir = (dir + 2) % 4;
                                    if (connectionsDM[l].Count == 1)
                                    {
                                        int otherx = 0;
                                        int othery = 0;
                                        tries = 0;
                                        Boolean crossing = true;
                                        do
                                        {
                                            int range = 7;
                                            int offset = 3;
                                            if (bio == biome.islands)
                                            {
                                                range = 7;
                                                offset = 5;
                                            }
                                            crossing = true;
                                            if (dir == 0) //south
                                            {
                                                otherx = x + (hy.R.Next(7) - 3);
                                                othery = y - (hy.R.Next(range) + offset);
                                            }
                                            else if (dir == 1) //west
                                            {
                                                otherx = x + (hy.R.Next(range) + offset);
                                                othery = y + (hy.R.Next(7) - 3);
                                            }
                                            else if (dir == 2) //north
                                            {
                                                otherx = x + (hy.R.Next(7) - 3);
                                                othery = y + (hy.R.Next(range) + offset);
                                            }
                                            else //east
                                            {
                                                otherx = x - (hy.R.Next(range) + offset);
                                                othery = y + (hy.R.Next(7) - 3);
                                            }
                                            tries++;

                                            if (tries >= 100)
                                            {
                                                return false;
                                            }
                                        } while (!crossing || otherx <= 1 || otherx >= MAP_COLS - 1 || othery <= 1 || othery >= MAP_ROWS - 1 || map[othery, otherx] != terrain.none || map[othery - 1, otherx] != terrain.none || map[othery + 1, otherx] != terrain.none || map[othery + 1, otherx + 1] != terrain.none || map[othery, otherx + 1] != terrain.none || map[othery - 1, otherx + 1] != terrain.none || map[othery + 1, otherx - 1] != terrain.none || map[othery, otherx - 1] != terrain.none || map[othery - 1, otherx - 1] != terrain.none);
                                        if (tries >= 100)
                                        {
                                            return false;
                                        }

                                        List<Location> l2 = connectionsDM[l];
                                        l.CanShuffle = false;
                                        l.Xpos = x;
                                        l.Ypos = y + 30;
                                        l2[0].CanShuffle = false;
                                        l2[0].Xpos = otherx;
                                        l2[0].Ypos = othery + 30;
                                        placeCave(x, y, dir, s);
                                        placeCave(otherx, othery, otherdir, s);
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
                                            if (bio == biome.islands)
                                            {
                                                range = 7;
                                                offset = 5;
                                            }
                                            crossing = true;
                                            if (dir == 0) //south
                                            {
                                                otherx = x + (hy.R.Next(7) - 3);
                                                othery = y - (hy.R.Next(range) + offset);
                                            }
                                            else if (dir == 1) //west
                                            {
                                                otherx = x + (hy.R.Next(range) + offset);
                                                othery = y + (hy.R.Next(7) - 3);
                                            }
                                            else if (dir == 2) //north
                                            {
                                                otherx = x + (hy.R.Next(7) - 3);
                                                othery = y + (hy.R.Next(range) + offset);
                                            }
                                            else //east
                                            {
                                                otherx = x - (hy.R.Next(range) + offset);
                                                othery = y + (hy.R.Next(7) - 3);
                                            }
                                            tries++;

                                            if (tries >= 100)
                                            {
                                                return false;
                                            }
                                        } while (!crossing || otherx <= 1 || otherx >= MAP_COLS - 1 || othery <= 1 || othery >= MAP_ROWS - 1 || map[othery, otherx] != terrain.none || map[othery - 1, otherx] != terrain.none || map[othery + 1, otherx] != terrain.none || map[othery + 1, otherx + 1] != terrain.none || map[othery, otherx + 1] != terrain.none || map[othery - 1, otherx + 1] != terrain.none || map[othery + 1, otherx - 1] != terrain.none || map[othery, otherx - 1] != terrain.none || map[othery - 1, otherx - 1] != terrain.none); if (tries >= 100)
                                        {
                                            return false;
                                        }

                                        List<Location> l2 = connectionsDM[l];
                                        l.CanShuffle = false;
                                        l.Xpos = x;
                                        l.Ypos = y + 30;
                                        l2[0].CanShuffle = false;
                                        l2[0].Xpos = otherx;
                                        l2[0].Ypos = othery + 30;
                                        placeCave(x, y, dir, s);
                                        placeCave(otherx, othery, otherdir, s);
                                        int newx = 0;
                                        int newy = 0;
                                        tries = 0;
                                        do
                                        {
                                            newx = x + hy.R.Next(7) - 3;
                                            newy = y + hy.R.Next(7) - 3;
                                            tries++;
                                        } while (newx > 2 && newx < MAP_COLS - 2 && newy > 2 && newy < MAP_ROWS - 2 && (map[newy, newx] != terrain.none || map[newy - 1, newx] != terrain.none || map[newy + 1, newx] != terrain.none || map[newy + 1, newx + 1] != terrain.none || map[newy, newx + 1] != terrain.none || map[newy - 1, newx + 1] != terrain.none || map[newy + 1, newx - 1] != terrain.none || map[newy, newx - 1] != terrain.none || map[newy - 1, newx - 1] != terrain.none) && tries < 100);
                                        if (tries >= 100)
                                        {
                                            return false;
                                        }
                                        l2[1].Xpos = newx;
                                        l2[1].Ypos = newy + 30;
                                        l2[1].CanShuffle = false;
                                        placeCave(newx, newy, dir, s);
                                        y = newy;
                                        x = newx;

                                        tries = 0;
                                        do
                                        {
                                            int range = 7;
                                            int offset = 3;
                                            if (bio == biome.islands)
                                            {
                                                range = 7;
                                                offset = 5;
                                            }
                                            crossing = true;
                                            if (dir == 0) //south
                                            {
                                                otherx = x + (hy.R.Next(7) - 3);
                                                othery = y - (hy.R.Next(range) + offset);
                                            }
                                            else if (dir == 1) //west
                                            {
                                                otherx = x + (hy.R.Next(range) + offset);
                                                othery = y + (hy.R.Next(7) - 3);
                                            }
                                            else if (dir == 2) //north
                                            {
                                                otherx = x + (hy.R.Next(7) - 3);
                                                othery = y + (hy.R.Next(range) + offset);
                                            }
                                            else //east
                                            {
                                                otherx = x - (hy.R.Next(range) + offset);
                                                othery = y + (hy.R.Next(7) - 3);
                                            }
                                            tries++;

                                            if (tries >= 100)
                                            {
                                                return false;
                                            }
                                        } while (!crossing || otherx <= 1 || otherx >= MAP_COLS - 1 || othery <= 1 || othery >= MAP_ROWS - 1 || map[othery, otherx] != terrain.none || map[othery - 1, otherx] != terrain.none || map[othery + 1, otherx] != terrain.none || map[othery + 1, otherx + 1] != terrain.none || map[othery, otherx + 1] != terrain.none || map[othery - 1, otherx + 1] != terrain.none || map[othery + 1, otherx - 1] != terrain.none || map[othery, otherx - 1] != terrain.none || map[othery - 1, otherx - 1] != terrain.none); if (tries >= 100)
                                        {
                                            return false;
                                        }

                                        l.CanShuffle = false;
                                        l2[2].CanShuffle = false;
                                        l2[2].Xpos = otherx;
                                        l2[2].Ypos = othery + 30;
                                        placeCave(otherx, othery, otherdir, s);
                                    }

                                }
                            }
                        }
                    }


                    if (this.bio == biome.vanillalike)
                    {
                        placeRandomTerrain(5);
                    }
                    randomTerrains.Add(terrain.road);
                    if (!growTerrain())
                    {
                        return false;
                    }
                    if (this.bio == biome.caldera)
                    {
                        bool f = makeCaldera();
                        if (!f)
                        {
                            return false;
                        }
                    }
                    walkable.Add(terrain.road);
                    if (raft != null)
                    {
                        if (this.bio != biome.caldera && this.bio != biome.canyon)
                        {
                            MAP_COLS = 29;
                        }
                        Boolean r = drawRaft(false, rDir);
                        MAP_COLS = 64;
                        if (!r)
                        {
                            return false;
                        }
                    }

                    if (bridge != null)
                    {
                        if (this.bio != biome.caldera && this.bio != biome.canyon)
                        {
                            MAP_COLS = 29;
                        }
                        Boolean b = drawRaft(true, bDir);
                        MAP_COLS = 64;
                        if (!b)
                        {
                            return false;
                        }
                    }


                    do
                    {
                        x = hy.R.Next(MAP_COLS - 2) + 1;
                        y = hy.R.Next(MAP_ROWS - 2) + 1;
                    } while (!walkable.Contains(map[y, x]) || map[y + 1, x] == terrain.cave || map[y - 1, x] == terrain.cave || map[y, x + 1] == terrain.cave || map[y, x - 1] == terrain.cave);

                    map[y, x] = terrain.rock;
                    magicCave.Ypos = y + 30;
                    magicCave.Xpos = x;


                    if (this.bio == biome.canyon || this.bio == biome.islands)
                    {
                        connectIslands(25, false, riverT, false, false, false);
                    }

                    //check bytes and adjust
                    writeBytes(false, MAP_ADDR, MAP_SIZE_BYTES, 0, 0);
                }
            }
            writeBytes(true, MAP_ADDR, MAP_SIZE_BYTES, 0, 0);
            

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
                    hy.ROMData.put(i, 0x00);
                }
            }
            return true;
        }
        private bool makeCaldera()
        {
            terrain water = terrain.water;
            if (hy.Props.bootsWater)
            {
                water = terrain.walkablewater;
            }
            int centerx = hy.R.Next(21, 41);
            int centery = hy.R.Next(17, 27);
            if (horizontal)
            {
                centerx = hy.R.Next(27, 37);
                centery = hy.R.Next(17, 27);
            }

            bool placeable = false;
            do
            {
                if (horizontal)
                {
                    centerx = hy.R.Next(27, 37);
                    centery = hy.R.Next(17, 27);
                }
                else
                {
                    centerx = hy.R.Next(21, 41);
                    centery = hy.R.Next(17, 27);
                }
                placeable = true;
                for (int i = centery - 7; i < centery + 8; i++)
                {
                    for (int j = centerx - 7; j < centerx + 8; j++)
                    {
                        if (map[i, j] != terrain.mountain)
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
                int lake = hy.R.Next(7, 11);
                if (i == 0 || i == 9)
                {
                    lake = hy.R.Next(3, 6);
                }
                if (horizontal)
                {
                    for (int j = 0; j < lake / 2; j++)
                    {
                        map[starty + j, startx] = water;
                        if (i == 0)
                        {
                            map[starty + j, startx - 1] = terrain.forest;
                        }
                        if (i == 9)
                        {
                            map[starty + j, startx + 1] = terrain.forest;
                        }

                    }
                    int top = starty + lake / 2;
                    while (map[top, startx - 1] == terrain.mountain)
                    {
                        map[top, startx - 1] = terrain.forest;
                        top--;
                    }
                    top = starty + lake / 2;
                    while (map[top, startx - 1] != terrain.mountain)
                    {
                        map[top, startx] = terrain.forest;
                        top++;
                    }

                    for (int j = 0; j < lake - (lake / 2); j++)
                    {
                        map[starty - j, startx] = water;
                        if (i == 0)
                        {
                            map[starty - j, startx - 1] = terrain.forest;
                        }
                        if (i == 9)
                        {
                            map[starty - j, startx + 1] = terrain.forest;
                        }

                    }
                    top = starty - (lake - (lake / 2));
                    while (map[top, startx - 1] == terrain.mountain)
                    {
                        map[top, startx - 1] = terrain.forest;
                        top++;
                    }
                    top = starty - (lake - (lake / 2));
                    while (map[top, startx - 1] != terrain.mountain)
                    {
                        map[top, startx] = terrain.forest;
                        top--;
                    }

                    //map[starty + lake / 2, startx] = terrain.forest;
                    // map[starty - (lake - (lake / 2)), startx] = terrain.forest;
                    if (i == 0)
                    {
                        map[starty + lake / 2, startx + 1] = terrain.forest;
                        map[starty - (lake - (lake / 2)), startx - 1] = terrain.forest;
                    }
                    if (i == 9)
                    {
                        map[starty + lake / 2, startx + 1] = terrain.forest;
                        map[starty - (lake - (lake / 2)), startx + 1] = terrain.forest;
                    }

                }
                else
                {
                    for (int j = 0; j < lake / 2; j++)
                    {
                        map[starty, startx + j] = water;
                        if (i == 0)
                        {
                            map[starty - 1, startx + j] = terrain.forest;
                        }
                        if (i == 9)
                        {
                            map[starty + 1, startx + j] = terrain.forest;
                        }
                    }
                    int top = startx + lake / 2;
                    while (map[starty - 1, top] == terrain.mountain && i != 0)
                    {
                        map[starty - 1, top] = terrain.forest;
                        top--;
                    }
                    top = startx + lake / 2;
                    while (map[starty - 1, top] != terrain.mountain && i != 0)
                    {
                        map[starty, top] = terrain.forest;
                        top++;
                    }

                    for (int j = 0; j < lake - (lake / 2); j++)
                    {
                        map[starty, startx - j] = water;
                        if (i == 0)
                        {
                            map[starty - 1, startx - j] = terrain.forest;
                        }
                        if (i == 9)
                        {
                            map[starty + 1, startx - j] = terrain.forest;
                        }
                    }
                    top = startx - (lake - (lake / 2));
                    while (map[starty - 1, top] == terrain.mountain && i != 0)
                    {
                        map[starty - 1, top] = terrain.forest;
                        top++;
                    }
                    top = startx - (lake - (lake / 2));
                    while (map[starty - 1, top] != terrain.mountain && i != 0)
                    {
                        map[starty, top] = terrain.forest;
                        top--;
                    }
                    //map[starty, startx + lake / 2] = terrain.forest;
                    //map[starty, startx - (lake - (lake / 2))] = terrain.forest;
                    if (i == 0)
                    {
                        map[starty - 1, startx + lake / 2] = terrain.forest;
                        map[starty - 1, startx - (lake - (lake / 2))] = terrain.forest;
                    }
                    if (i == 9)
                    {
                        map[starty + 1, startx + lake / 2] = terrain.forest;
                        map[starty + 1, startx - (lake - (lake / 2))] = terrain.forest;
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
                int cavenum1 = hy.R.Next(connectionsDM.Keys.Count);
                cave1l = connectionsDM.Keys.ToList()[cavenum1];
                cave1r = connectionsDM[cave1l][0];
            } while (connectionsDM[cave1l].Count != 1 || connectionsDM[cave1r].Count != 1);

            do
            {
                int cavenum1 = hy.R.Next(connectionsDM.Keys.Count);
                cave2l = connectionsDM.Keys.ToList()[cavenum1];
                cave2r = connectionsDM[cave2l][0];
            } while (connectionsDM[cave2l].Count != 1 || cave1l == cave2l || cave1l == cave2r);
            cave1l.CanShuffle = false;
            cave1r.CanShuffle = false;
            cave2l.CanShuffle = false;
            cave2r.CanShuffle = false;
            map[cave1l.Ypos - 30, cave1l.Xpos] = terrain.mountain;
            map[cave2l.Ypos - 30, cave2l.Xpos] = terrain.mountain;

            map[cave1r.Ypos - 30, cave1r.Xpos] = terrain.mountain;

            map[cave2r.Ypos - 30, cave2r.Xpos] = terrain.mountain;


            int caveDir = hy.R.Next(2);
            if (horizontal)
            {
                bool f = horizontalCave(caveDir, centerx, centery, cave1l, cave1r);
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
                f = horizontalCave(caveDir, centerx, centery, cave2l, cave2r);
                if (!f)
                {
                    return false;
                }
            }
            else
            {
                bool f = verticalCave(caveDir, centerx, centery, cave1l, cave1r);
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
                f = verticalCave(caveDir, centerx, centery, cave2l, cave2r);
                if (!f)
                {
                    return false;
                }
                
            }
            return true;
        }
        public void updateVisit()
        {
            updateReachable();

            foreach (Location l in AllLocations)
            {
                if (v[l.Ypos - 30, l.Xpos])
                {
                    l.Reachable = true;
                    if (connectionsDM.Keys.Contains(l))
                    {
                        List<Location> l2 = connectionsDM[l];

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
