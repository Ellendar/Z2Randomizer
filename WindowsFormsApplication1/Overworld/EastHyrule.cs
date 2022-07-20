﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Z2Randomizer
{
    //6A31 - address in memory of kasuto y coord;
    //6A35 - address in memory of palace 6 y coord
    class EastHyrule : World
    {
        private int bridgeCount;

        private readonly SortedDictionary<int, Terrain> terrains = new SortedDictionary<int, Terrain>
            {
                { 0x862F, Terrain.FOREST },
                { 0x8630, Terrain.FOREST },
                { 0x8631, Terrain.ROAD },
                { 0x8632, Terrain.ROAD },
                { 0x8633, Terrain.ROAD },
                { 0x8634, Terrain.ROAD },
                { 0x8635, Terrain.BRIDGE },
                { 0x8636, Terrain.BRIDGE },
                { 0x8637, Terrain.DESERT },
                { 0x8638, Terrain.DESERT },
                { 0x8639, Terrain.WALKABLEWATER },
                { 0x863A, Terrain.CAVE },
                { 0x863B, Terrain.CAVE },
                { 0x863C, Terrain.CAVE },
                { 0x863D, Terrain.CAVE },
                { 0x863E, Terrain.CAVE },
                { 0x863F, Terrain.CAVE },
                { 0x8640, Terrain.CAVE },
                { 0x8641, Terrain.CAVE },
                { 0x8642, Terrain.CAVE },
                { 0x8643, Terrain.CAVE },
                { 0x8644, Terrain.SWAMP },
                { 0x8645, Terrain.LAVA },
                { 0x8646, Terrain.DESERT },
                { 0x8647, Terrain.DESERT },
                { 0x8648, Terrain.DESERT },
                { 0x8649, Terrain.DESERT },
                { 0x864A, Terrain.FOREST },
                { 0x864B, Terrain.LAVA },
                { 0x864C, Terrain.LAVA },
                { 0x864D, Terrain.LAVA },
                { 0x864E, Terrain.LAVA },
                { 0x864F, Terrain.LAVA },
                { 0x8657, Terrain.BRIDGE },
                { 0x8658, Terrain.BRIDGE },
                { 0x865C, Terrain.TOWN },
                { 0x865E, Terrain.TOWN },
                { 0x8660, Terrain.TOWN },
                { 0x8662, Terrain.TOWN },
                { 0x8663, Terrain.PALACE },
                { 0x8664, Terrain.PALACE },
                { 0x8665, Terrain.PALACE },

            };

        public Location palace5;
        public Location palace6;
        public Location heart1;
        public Location heart2;
        public Location darunia;
        public Location newKasuto;
        public Location newKasuto2;
        public Location fireTown;
        public Location oldKasuto;
        public Location gp;
        public Location pbagCave1;
        public Location pbagCave2;
        public Location hpCallSpot;
        private bool canyonShort;
        private Location vodcave1;
        private Location vodcave2;
        public Location hpLoc;
        public Location hkLoc;

        private const int MAP_ADDR = 0xb480;


        public EastHyrule(Hyrule hy)
            : base(hy)
        {
            baseAddr = 0x862F;
            loadLocations(0x863E, 6, terrains, Continent.EAST);
            loadLocations(0x863A, 2, terrains, Continent.EAST);

            loadLocations(0x862F, 11, terrains, Continent.EAST);
            loadLocations(0x8644, 1, terrains, Continent.EAST);
            loadLocations(0x863C, 2, terrains, Continent.EAST);
            loadLocations(0x8646, 10, terrains, Continent.EAST);
            //loadLocations(0x8657, 2, terrains, continent.east);
            loadLocations(0x865C, 1, terrains, Continent.EAST);
            loadLocations(0x865E, 1, terrains, Continent.EAST);
            loadLocations(0x8660, 1, terrains, Continent.EAST);
            loadLocations(0x8662, 4, terrains, Continent.EAST);

            reachableAreas = new HashSet<string>();

            connections.Add(getLocationByMem(0x863A), getLocationByMem(0x863B));
            connections.Add(getLocationByMem(0x863B), getLocationByMem(0x863A));
            connections.Add(getLocationByMem(0x863E), getLocationByMem(0x863F));
            connections.Add(getLocationByMem(0x863F), getLocationByMem(0x863E));
            connections.Add(getLocationByMem(0x8640), getLocationByMem(0x8641));
            connections.Add(getLocationByMem(0x8641), getLocationByMem(0x8640));
            connections.Add(getLocationByMem(0x8642), getLocationByMem(0x8643));
            connections.Add(getLocationByMem(0x8643), getLocationByMem(0x8642));

            palace6 = getLocationByMem(0x8664);
            palace6.PalNum = 6;
            darunia = getLocationByMem(0x865E);
            palace5 = getLocationByMap(0x23, 0x0E);
            palace5.PalNum = 5;

            newKasuto = getLocationByMem(0x8660);
            newKasuto2 = new Location(newKasuto.LocationBytes, newKasuto.TerrainType, newKasuto.MemAddress, Continent.EAST);
            heart1 = getLocationByMem(0x8639);
            heart1.Needboots = true;
            heart2 = getLocationByMem(0x8649);
            if (palace5 == null)
            {
                palace5 = getLocationByMem(0x8657);
                palace5.PalNum = 5;
            }

            hpCallSpot = new Location();
            hpCallSpot.Xpos = 0;
            hpCallSpot.Ypos = 0;

            enemyAddr = 0x88B0;
            enemies = new List<int> { 03, 04, 05, 0x11, 0x12, 0x14, 0x16, 0x18, 0x19, 0x1A, 0x1B, 0x1C };
            flyingEnemies = new List<int> { 0x06, 0x07, 0x0A, 0x0D, 0x0E, 0x15 };
            generators = new List<int> { 0x0B, 0x0F, 0x17 };
            shorties = new List<int> { 0x03, 0x04, 0x05, 0x11, 0x12, 0x16 };
            tallGuys = new List<int> { 0x14, 0x18, 0x19, 0x1A, 0x1B, 0x1C };
            enemyPtr = 0x85B1;
            fireTown = getLocationByMem(0x865C);
            oldKasuto = getLocationByMem(0x8662);
            gp = getLocationByMem(0x8665);
            gp.PalNum = 7;
            gp.item = Items.donotuse;
            pbagCave1 = getLocationByMem(0x863C);
            pbagCave2 = getLocationByMem(0x863D);
            VANILLA_MAP_ADDR = 0x9056;

            overworldMaps = new List<int> { 0x22, 0x1D, 0x27, 0x35, 0x30, 0x1E, 0x28, 0x3C };

            MAP_ROWS = 75;
            MAP_COLS = 64;

            walkable = new List<Terrain>() { Terrain.DESERT, Terrain.GRASS, Terrain.FOREST, Terrain.SWAMP, Terrain.GRAVE };
            randomTerrains = new List<Terrain> { Terrain.DESERT, Terrain.GRASS, Terrain.FOREST, Terrain.SWAMP, Terrain.GRAVE, Terrain.MOUNAIN, Terrain.WALKABLEWATER };
            

            if (hy.Props.eastBiome.Equals("Islands"))
            {
                this.bio = biome.islands;
            }
            else if (hy.Props.eastBiome.Equals("Canyon") || hy.Props.eastBiome.Equals("CanyonD"))
            {
                this.bio = biome.canyon;
            }
            else if (hy.Props.eastBiome.Equals("Volcano"))
            {
                this.bio = biome.volcano;
            }
            else if (hy.Props.eastBiome.Equals("Mountainous"))
            {
                this.bio = biome.mountainous;
            }
            else if (hy.Props.eastBiome.Equals("Vanilla"))
            {
                this.bio = biome.vanilla;
            }
            else if (hy.Props.eastBiome.Equals("Vanilla (shuffled)"))
            {
                this.bio = biome.vanillaShuffle;
            }
            else
            {
                this.bio = biome.vanillalike;
            }
            section = new SortedDictionary<Tuple<int, int>, string>
        {
            {Tuple.Create(0x3A, 0x0A), "mid2" },
            {Tuple.Create(0x5B, 0x36), "south" },
            { Tuple.Create(0x4C, 0x15), "south" },
            { Tuple.Create(0x51, 0x11), "south" },
            { Tuple.Create(0x54, 0x13), "south" },
            { Tuple.Create(0x60, 0x18), "south" },
            { Tuple.Create(0x5D, 0x23), "south" },
            { Tuple.Create(0x64, 0x25), "south" },
            { Tuple.Create(0x24, 0x09), "north2" },
            { Tuple.Create(0x26, 0x0A), "north2" },
            { Tuple.Create(0x38, 0x3F), "boots1" },
            { Tuple.Create(0x34, 0x18), "mid2" },
            { Tuple.Create(0x30, 0x1B), "north2" },
            { Tuple.Create(0x47, 0x19), "mid2" },
            { Tuple.Create(0x4E, 0x1F), "south" },
            { Tuple.Create(0x4E, 0x31), "south" },
            { Tuple.Create(0x4E, 0x39), "kasuto" },
            { Tuple.Create(0x4B, 0x02), "vod" },
            { Tuple.Create(0x4B, 0x04), "gp" },
            { Tuple.Create(0x4D, 0x06), "vod" },
            { Tuple.Create(0x4D, 0x0A), "south" },
            { Tuple.Create(0x51, 0x1A), "south" },
            { Tuple.Create(0x40, 0x35), "hammer2" },
            { Tuple.Create(0x38, 0x22), "mid2" },
            { Tuple.Create(0x2C, 0x30), "north2" },
            { Tuple.Create(0x63, 0x39), "south" },
            { Tuple.Create(0x44, 0x0D), "mid2" },
            { Tuple.Create(0x5B, 0x04), "south" },
            { Tuple.Create(0x63, 0x1B), "south" },
            { Tuple.Create(0x53, 0x03), "vod" },
            { Tuple.Create(0x56, 0x08), "south" },
            { Tuple.Create(0x63, 0x08), "south" },
            { Tuple.Create(0x28, 0x34), "north2" },
            { Tuple.Create(0x34, 0x07), "mid2" },
            { Tuple.Create(0x3C, 0x17), "mid2" },
            { Tuple.Create(0x21, 0x03), "north2" },
            { Tuple.Create(0x51, 0x3D), "kasuto" },
            { Tuple.Create(0x63, 0x22), "south" },
            { Tuple.Create(0x3C, 0x3E), "boots" },
            { Tuple.Create(0x66, 0x2D), "south" },
            { Tuple.Create(0x49, 0x04), "gp" }
        };
            newKasuto.ExternalWorld = 128;
            palace6.ExternalWorld = 128;
            hpLoc = palace6;
            hkLoc = newKasuto;
        }

        public bool Terraform()
        {
            foreach (Location l in AllLocations)
            {
                l.CanShuffle = true;
                l.NeedHammer = false;
                l.NeedRecorder = false;
                if (l != raft && l != bridge && l != cave1 && l != cave2)
                {
                    l.TerrainType = terrains[l.MemAddress];
                }
            }
            if (hy.Props.hideLocs)
            {
                unimportantLocs = new List<Location>();
                unimportantLocs.Add(getLocationByMem(0x862F));
                unimportantLocs.Add(getLocationByMem(0x8630));
                unimportantLocs.Add(getLocationByMem(0x8644));
                unimportantLocs.Add(getLocationByMem(0x8646));
                unimportantLocs.Add(getLocationByMem(0x8647));
                unimportantLocs.Add(getLocationByMem(0x8648));
                unimportantLocs.Add(getLocationByMem(0x864A));
                unimportantLocs.Add(getLocationByMem(0x864B));
                unimportantLocs.Add(getLocationByMem(0x864C));

            }
            if (this.bio == biome.vanilla || this.bio == biome.vanillaShuffle)
            {
                MAP_ROWS = 75;
                MAP_COLS = 64;
                readVanillaMap();

               

                if (this.bio == biome.vanillaShuffle)
                {
                    areasByLocation = new SortedDictionary<string, List<Location>>();
                    areasByLocation.Add("north2", new List<Location>());
                    areasByLocation.Add("mid2", new List<Location>());
                    areasByLocation.Add("south", new List<Location>());
                    areasByLocation.Add("vod", new List<Location>());
                    areasByLocation.Add("kasuto", new List<Location>());
                    areasByLocation.Add("gp", new List<Location>());
                    areasByLocation.Add("boots", new List<Location>());
                    areasByLocation.Add("boots1", new List<Location>());
                    areasByLocation.Add("hammer2", new List<Location>());
                    //areasByLocation.Add("horn", new List<Location>());
                    foreach (Location l in AllLocations)
                    {
                        areasByLocation[section[l.Coords]].Add(getLocationByCoords(l.Coords));
                    }

                    chooseConn("kasuto", connections, true);
                    chooseConn("vod", connections, true);
                    chooseConn("gp", connections, true);

                    if(!hy.Props.shuffleHidden)
                    {
                        newKasuto.CanShuffle = false;
                        palace6.CanShuffle = false;
                    }
                    shuffleLocations(AllLocations);
                    if (hy.Props.vanillaOriginal)
                    {
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
                    raft.PassThrough = 0;
                    bridge.PassThrough = 0;
                    Location desert = getLocationByMem(0x8646);
                    Location swamp = getLocationByMem(0x8644);
                    if (desert.PassThrough != 0)
                    {
                        desert.NeedJump = true;
                    }
                    else
                    {
                        desert.NeedJump = false;
                    }

                    if (swamp.PassThrough != 0)
                    {
                        swamp.NeedFairy = true;
                    }
                    else
                    {
                        swamp.NeedFairy = false;
                    }

                }
                hkLoc = getLocationByCoords(Tuple.Create(81, 61));
                hpLoc = getLocationByCoords(Tuple.Create(102, 45));

                if (hy.hiddenKasuto)
                {
                    
                    if(connections.ContainsKey(hkLoc) || hkLoc == raft || hkLoc == bridge)
                    {
                        return false;
                    }
                }
                if (hy.hiddenPalace)
                {
                    if (connections.ContainsKey(hpLoc) || hpLoc == raft || hpLoc == bridge)
                    {
                        return false;
                    }
                }
                else
                {
                    map[72, 45] = Terrain.PALACE;
                }
            }
            else
            {
                Terrain water = Terrain.WATER;
                if (hy.Props.bootsWater)
                {
                    water = Terrain.WALKABLEWATER;
                }

                bcount = 2000;
                this.gp.CanShuffle = false;
                Terrain riverT = Terrain.MOUNAIN;
                while (bcount > MAP_SIZE_BYTES)
                {
                    map = new Terrain[MAP_ROWS, MAP_COLS];

                    for (int i = 0; i < MAP_ROWS; i++)
                    {
                        for (int j = 0; j < MAP_COLS; j++)
                        {
                            map[i, j] = Terrain.NONE;
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
                        MakeVolcano();
                        int cols = hy.R.Next(2, 4);
                        int rows = hy.R.Next(2, 4);
                        List<int> pickedC = new List<int>();
                        List<int> pickedR = new List<int>();

                        while (cols > 0)
                        {
                            int col = hy.R.Next(1, MAP_COLS - 1);
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
                            int row = hy.R.Next(1, MAP_ROWS - 1);
                            if (!pickedR.Contains(row))
                            {
                                for (int i = 0; i < MAP_COLS; i++)
                                {
                                    if (map[row, i] == Terrain.NONE)
                                    {
                                        map[row, i] = water;
                                    }
                                }
                                pickedR.Add(row);
                                rows--;
                            }
                        }
                        walkable = new List<Terrain>() { Terrain.LAVA, Terrain.DESERT, Terrain.GRASS, Terrain.FOREST, Terrain.SWAMP, Terrain.GRAVE };
                        randomTerrains = new List<Terrain> { Terrain.LAVA, Terrain.DESERT, Terrain.GRASS, Terrain.FOREST, Terrain.SWAMP, Terrain.GRAVE, Terrain.MOUNAIN, water };




                    }
                    else if (this.bio == biome.canyon)
                    {
                        horizontal = hy.R.NextDouble() > 0.5;
                        riverT = water;
                        if (hy.Props.eastBiome.Equals("CanyonD"))
                        {
                            riverT = Terrain.DESERT;
                        }
                        //riverT = terrain.lava;
                        walkable = new List<Terrain>() { Terrain.DESERT, Terrain.GRASS, Terrain.FOREST, Terrain.GRAVE, Terrain.MOUNAIN };
                        randomTerrains = new List<Terrain> { Terrain.DESERT, Terrain.GRASS, Terrain.FOREST, Terrain.GRAVE, Terrain.MOUNAIN, water };


                        drawCanyon(riverT);
                        this.walkable.Remove(Terrain.MOUNAIN);

                        gp.CanShuffle = false;


                    }
                    else if (this.bio == biome.volcano)
                    {
                        horizontal = hy.R.NextDouble() > .5;

                        drawCenterMountain();



                        walkable = new List<Terrain>() { Terrain.LAVA, Terrain.DESERT, Terrain.GRASS, Terrain.FOREST, Terrain.SWAMP, Terrain.GRAVE };
                        randomTerrains = new List<Terrain> { Terrain.LAVA, Terrain.DESERT, Terrain.GRASS, Terrain.FOREST, Terrain.SWAMP, Terrain.GRAVE, Terrain.MOUNAIN, water};


                    }
                    else if (this.bio == biome.mountainous)
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
                        MakeVolcano();

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
                            int row = hy.R.Next(10, MAP_ROWS - 11);
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
                        walkable = new List<Terrain>() { Terrain.DESERT, Terrain.GRASS, Terrain.FOREST, Terrain.SWAMP, Terrain.GRAVE };
                        randomTerrains = new List<Terrain> { Terrain.DESERT, Terrain.GRASS, Terrain.FOREST, Terrain.SWAMP, Terrain.GRAVE, Terrain.MOUNAIN, water };
                    }
                    else
                    {
                        walkable = new List<Terrain>() { Terrain.DESERT, Terrain.GRASS, Terrain.FOREST, Terrain.SWAMP, Terrain.GRAVE };
                        randomTerrains = new List<Terrain> { Terrain.DESERT, Terrain.GRASS, Terrain.FOREST, Terrain.SWAMP, Terrain.GRAVE, Terrain.MOUNAIN, water };
                        MakeVolcano();


                        DrawMountains();
                        drawRiver(new List<Location>() { getLocationByMem(0x8635), getLocationByMem(0x8636) });
                    }


                    if (hy.hiddenKasuto)
                    {
                        DrawHiddenKasuto();
                    }
                    if (hy.hiddenPalace)
                    {
                        bool hp = DrawHiddenPalace();
                        if (!hp)
                        {
                            return false;
                        }
                    }
                    direction rDir = direction.west;
                    if (!hy.Props.continentConnections.Equals("Normal") && this.bio != biome.canyon)
                    {
                        rDir = (direction)hy.R.Next(4);
                    }
                    else if (this.bio == biome.canyon || this.bio == biome.volcano)
                    {
                        rDir = (direction)hy.R.Next(2);
                        if (horizontal)
                        {
                            rDir += 2;
                        }
                    }
                    if (raft != null)
                    {
                        drawOcean(rDir);
                    }


                    direction bDir = direction.east;
                    do
                    {
                        if (this.bio != biome.canyon && this.bio != biome.volcano)
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
                        drawOcean(bDir);
                    }

                    Boolean b = placeLocations(riverT);
                    if (!b)
                    {
                        return false;
                    }


                    if (hy.Props.hideLocs)
                    {
                        placeRandomTerrain(50);
                    }
                    else
                    {
                        placeRandomTerrain(25);

                    }
                    randomTerrains.Add(Terrain.LAVA);
                    if (!growTerrain())
                    {
                        return false;
                    }
                    randomTerrains.Remove(Terrain.LAVA);
                    if (raft != null)
                    {
                        Boolean r = drawRaft(false, rDir);
                        if (!r)
                        {
                            return false;
                        }
                    }

                    if (bridge != null)
                    {
                        Boolean b2 = drawRaft(true, bDir);
                        if (!b2)
                        {
                            return false;
                        }
                    }

                    if (this.bio == biome.volcano || this.bio == biome.canyon)
                    {
                        bool f = MakeVolcano();
                        if (!f)
                        {
                            return false;
                        }
                    }
                    placeHiddenLocations();
                    if (this.bio == biome.vanillalike)
                    {
                        connectIslands(4, false, Terrain.MOUNAIN, false, false, true);

                        connectIslands(3, false, water, true, false, false);

                    }
                    if (this.bio == biome.islands)
                    {
                        connectIslands(100, false, riverT, true, false, true);
                    }
                    if (this.bio == biome.mountainous)
                    {
                        connectIslands(20, false, riverT, true, false, true);
                    }
                    if (this.bio == biome.canyon)
                    {
                        connectIslands(15, false, riverT, true, false, true);

                    }

                    foreach (Location l in AllLocations)
                    {
                        if (l.CanShuffle)
                        {
                            l.Ypos = 0;
                            l.CanShuffle = false;
                        }
                    }
                    writeBytes(false, MAP_ADDR, MAP_SIZE_BYTES, hpLoc.Ypos - 30, hpLoc.Xpos);
                    Console.WriteLine("East:" + bcount);
                }
                
            }
            if (hy.hiddenPalace)
            {
                UpdateHPspot();
            }
            if (hy.hiddenKasuto)
            {
                UpdateKasuto();
            }
            writeBytes(true, MAP_ADDR, MAP_SIZE_BYTES, hpLoc.Ypos - 30, hpLoc.Xpos);


            v = new bool[MAP_ROWS, MAP_COLS];
            for (int i = 0; i < MAP_ROWS; i++)
            {
                for (int j = 0; j < MAP_COLS; j++)
                {
                    v[i, j] = false;
                }
            }
            

            return true;
        }

        public bool MakeVolcano()
        {
            int xmin = 21;
            int xmax = 41;
            int ymin = 22;
            int ymax = 52;
            if (this.bio != biome.volcano)
            {
                xmin = 5;
                ymin = 5;
                xmax = MAP_COLS - 6;
                ymax = MAP_COLS - 6;
            }
            int palacex = hy.R.Next(xmin, xmax);
            int palacey = hy.R.Next(ymin, ymax);
            if (this.bio == biome.volcano || this.bio == biome.canyon)
            {
                bool placeable = false;
                do
                {
                    palacex = hy.R.Next(xmin, xmax);
                    palacey = hy.R.Next(ymin, ymax);
                    placeable = true;
                    for (int i = palacey - 4; i < palacey + 5; i++)
                    {
                        for (int j = palacex - 4; j < palacex + 5; j++)
                        {
                            if (map[i, j] != Terrain.MOUNAIN)
                            {
                                placeable = false;
                            }
                        }
                    }
                } while (!placeable);
            }

            for (int i = 0; i < 7; i++)
            {
                for (int j = 0; j < 7; j++)
                {
                    if (!((i == 0 && j == 0) || (i == 0 && j == 6) || (i == 6 && j == 0) || (i == 6 && j == 6) || (i == 3 && j == 3)))
                    {
                        map[palacey - 3 + i, palacex - 3 + j] = Terrain.LAVA;
                    }
                    else
                    {
                        map[palacey - 3 + i, palacex - 3 + j] = Terrain.MOUNAIN;
                    }
                    if (i == 0)
                    {
                        map[palacey - 4, palacex - 3 + j] = Terrain.MOUNAIN;
                    }
                    if (i == 6)
                    {
                        map[palacey + 4, palacex - 3 + j] = Terrain.MOUNAIN;
                    }
                    if (j == 0)
                    {
                        map[palacey - 3 + i, palacex - 4] = Terrain.MOUNAIN;
                    }
                    if (j == 6)
                    {
                        map[palacey - 3 + i, palacex + 4] = Terrain.MOUNAIN;
                    }
                }
            }
            map[palacey, palacex] = Terrain.PALACE;
            gp.Xpos = palacex;
            gp.Ypos = palacey + 30;
            gp.CanShuffle = false;

            int length = 20;
            if (this.bio != biome.canyon && this.bio != biome.volcano)
            {
                this.horizontal = hy.R.NextDouble() > .5;
                length = hy.R.Next(5, 16);
            }
            int deltax = 1;
            int deltay = 0;
            int starty = palacey;
            int startx = palacex + 4;
            if (this.bio != biome.canyon)
            {
                if (palacex > MAP_COLS / 2)
                {
                    deltax = -1;
                    startx = palacex - 4;
                }
                if (!horizontal)
                {
                    deltax = 0;
                    deltay = 1;
                    starty = palacey + 4;
                    startx = palacex;
                    if (palacey > MAP_ROWS / 2)
                    {
                        deltay = -1;
                        starty = palacey - 4;
                    }
                }
            }
            else
            {
                if (horizontal)
                {
                    if (palacey < MAP_ROWS / 2)
                    {
                        deltay = 1;
                        deltax = 0;
                        starty = palacey + 4;
                        startx = palacex;
                    }
                    else
                    {
                        deltay = -1;
                        deltax = 0;
                        starty = palacey - 4;
                        startx = palacex;
                    }
                }
                else
                {
                    if (palacex > MAP_COLS / 2)
                    {
                        deltax = -1;
                        startx = palacex - 4;
                    }
                }
            }
            bool cavePlaced = false;
            Location vodcave1, vodcave2, vodcave3, vodcave4;
            this.canyonShort = hy.R.NextDouble() > .5;
            if (canyonShort)
            {
                vodcave1 = getLocationByMem(0x8640);
                vodcave2 = getLocationByMem(0x8641);
                vodcave3 = getLocationByMem(0x8642);
                vodcave4 = getLocationByMem(0x8643);
            }
            else
            {
                vodcave1 = getLocationByMem(0x8642);
                vodcave2 = getLocationByMem(0x8643);
                vodcave3 = getLocationByMem(0x8640);
                vodcave4 = getLocationByMem(0x8641);
            }

            int forced = 0;
            int vodRoutes = hy.R.Next(1, 3);
            bool horizontalPath = (horizontal && this.bio != biome.canyon) || (!horizontal && this.bio == biome.canyon);
            if (this.bio != biome.volcano)
            {
                vodRoutes = 1;
            }
            for (int k = 0; k < vodRoutes; k++)
            {
                int forcedPlaced = 3;
                if (vodRoutes == 2)
                {
                    if (k == 0)
                    {
                        forcedPlaced = 2;
                    }
                    else
                    {
                        forcedPlaced = 1;
                    }
                }
                int minadjust = -1;
                int maxadjust = 2;
                int c = 0;
                while (startx > 1 && startx < MAP_COLS - 1 && starty > 1 && starty < MAP_ROWS - 1 && (((this.bio == biome.volcano || this.bio == biome.canyon) && map[starty, startx] == Terrain.MOUNAIN) || ((this.bio != biome.volcano && this.bio != biome.canyon) && c < length)))
                {
                    c++;
                    map[starty, startx] = Terrain.LAVA;
                    int adjust = hy.R.Next(minadjust, maxadjust);
                    while ((deltax != 0 && (starty + adjust < 1 || starty + adjust > MAP_ROWS - 2)) || (deltay != 0 && (startx + adjust < 1 || startx + adjust > MAP_COLS - 2)))
                    {
                        adjust = hy.R.Next(minadjust, maxadjust);

                    }
                    if (adjust > 0)
                    {
                        if (this.bio != biome.volcano && this.bio != biome.canyon)
                        {
                            if (deltax != 0)
                            {
                                map[starty - 1, startx] = Terrain.MOUNAIN;

                            }
                            else
                            {
                                map[starty, startx - 1] = Terrain.MOUNAIN;

                            }
                        }
                        for (int i = 0; i <= adjust; i++)
                        {
                            if (horizontalPath)
                            {
                                map[starty + i, startx] = Terrain.LAVA;
                                if (this.bio != biome.volcano && this.bio != biome.canyon)
                                {
                                    if (map[starty + i, startx - 1] != Terrain.LAVA && map[starty + i, startx - 1] != Terrain.CAVE)
                                    {
                                        map[starty + i, startx - 1] = Terrain.MOUNAIN;
                                    }
                                    if (map[starty + i, startx + 1] != Terrain.LAVA && map[starty + i, startx + 1] != Terrain.CAVE)
                                    {
                                        map[starty + i, startx + 1] = Terrain.MOUNAIN;
                                    }
                                }
                                Location l = getLocationByCoords(Tuple.Create(starty + i + 30, startx));
                                if (l != null && !l.CanShuffle)
                                {
                                    return false;
                                }
                            }
                            else
                            {
                                map[starty, startx + i] = Terrain.LAVA;
                                if (this.bio != biome.volcano && this.bio != biome.canyon)
                                {
                                    if (map[starty - 1, startx + i] != Terrain.LAVA && map[starty - 1, startx + i] != Terrain.CAVE)
                                    {
                                        map[starty - 1, startx + i] = Terrain.MOUNAIN;
                                    }
                                    if (map[starty + 1, startx + i] != Terrain.LAVA && map[starty + 1, startx + i] != Terrain.CAVE)
                                    {
                                        map[starty + 1, startx + i] = Terrain.MOUNAIN;
                                    }
                                }
                                Location l = getLocationByCoords(Tuple.Create(starty + 30, startx + i));
                                if (l != null && !l.CanShuffle)
                                {
                                    return false;
                                }
                            }
                        }
                        if (this.bio != biome.volcano && this.bio != biome.canyon)
                        {
                            if (deltax != 0)
                            {
                                map[starty + adjust + 1, startx] = Terrain.MOUNAIN;

                            }
                            else
                            {
                                map[starty, startx + adjust + 1] = Terrain.MOUNAIN;

                            }
                        }
                    }
                    else if (adjust < 0)
                    {
                        if (this.bio != biome.volcano && this.bio != biome.canyon)
                        {
                            if (deltax != 0)
                            {
                                map[starty + 1, startx] = Terrain.MOUNAIN;

                            }
                            else
                            {
                                map[starty, startx + 1] = Terrain.MOUNAIN;
                            }
                        }
                        if (horizontalPath)
                        {
                            for (int i = 0; i <= Math.Abs(adjust); i++)
                            {
                                map[starty - i, startx] = Terrain.LAVA;
                                if (this.bio != biome.volcano && this.bio != biome.canyon)
                                {
                                    if (map[starty - i, startx - 1] != Terrain.LAVA && map[starty - i, startx - 1] != Terrain.CAVE)
                                    {
                                        map[starty - i, startx - 1] = Terrain.MOUNAIN;
                                    }
                                    if (map[starty - i, startx + 1] != Terrain.LAVA && map[starty - i, startx + 1] != Terrain.CAVE)
                                    {
                                        map[starty - i, startx + 1] = Terrain.MOUNAIN;
                                    }
                                }
                                Location l = getLocationByCoords(Tuple.Create(starty - i + 30, startx));
                                if (l != null && !l.CanShuffle)
                                {
                                    return false;
                                }
                            }
                        }
                        else
                        {

                            for (int i = 0; i <= Math.Abs(adjust); i++)
                            {
                                map[starty, startx - i] = Terrain.LAVA;
                                if (this.bio != biome.volcano && this.bio != biome.canyon)
                                {
                                    if (map[starty - 1, startx - i] != Terrain.LAVA && map[starty - 1, startx - i] != Terrain.CAVE)
                                    {
                                        map[starty - 1, startx - i] = Terrain.MOUNAIN;
                                    }
                                    if (map[starty + 1, startx - i] != Terrain.LAVA && map[starty + 1, startx - i] != Terrain.CAVE)
                                    {
                                        map[starty + 1, startx - i] = Terrain.MOUNAIN;
                                    }
                                }
                                Location l = getLocationByCoords(Tuple.Create(starty + 30, startx - i));
                                if (l != null && !l.CanShuffle)
                                {
                                    return false;
                                }
                            }
                        }
                        if (this.bio != biome.volcano && this.bio != biome.canyon)
                        {
                            if (deltax != 0)
                            {
                                map[starty + adjust - 1, startx] = Terrain.MOUNAIN;

                            }
                            else
                            {
                                map[starty, startx + adjust - 1] = Terrain.MOUNAIN;
                            }
                        }
                    }
                    else
                    {
                        if (this.bio != biome.volcano && this.bio != biome.canyon)
                        {
                            if (deltax != 0)
                            {
                                map[starty - 1, startx] = Terrain.MOUNAIN;
                                map[starty + 1, startx] = Terrain.MOUNAIN;
                            }
                            else
                            {
                                map[starty, startx - 1] = Terrain.MOUNAIN;
                                map[starty, startx + 1] = Terrain.MOUNAIN;
                            }
                        }
                        if (map[starty, startx] != Terrain.CAVE)
                        {
                            map[starty, startx] = Terrain.LAVA;
                            if (getLocationByCoords(Tuple.Create(starty + 30 + deltay, startx + deltax)) != null)
                            {
                                return false;
                            }
                        }
                    }

                    if (horizontalPath)
                    {
                        starty += adjust;
                    }
                    else
                    {
                        startx += adjust;
                    }
                    if (((cavePlaced && adjust == 0) || adjust > 1 || adjust < -1) && forcedPlaced > 0)
                    {
                        Location f = getLocationByMem(0x864D);
                        if (forced == 1)
                        {
                            f = getLocationByMem(0x864E);
                        }
                        else if (forced == 2)
                        {
                            f = getLocationByMem(0x864F);
                        }

                        if (adjust == 0)
                        {
                            if (horizontalPath)
                            {
                                if (getLocationByCoords(Tuple.Create(starty + 30, startx - 1)) == null && getLocationByCoords(Tuple.Create(starty + 30, startx + 1)) == null)
                                {
                                    f.Xpos = startx;
                                    f.Ypos = starty + 30;
                                    f.CanShuffle = false;
                                    forcedPlaced--;
                                    forced++;
                                }
                            }
                            else
                            {
                                if (getLocationByCoords(Tuple.Create(starty + 30 - 1, startx)) == null && getLocationByCoords(Tuple.Create(starty + 30 + 1, startx)) == null)
                                {
                                    f.Xpos = startx;
                                    f.Ypos = starty + 30;
                                    f.CanShuffle = false;
                                    forcedPlaced--;
                                    forced++;
                                }

                            }
                        }
                        else if (adjust > 0)
                        {
                            if ((horizontal && this.bio != biome.canyon) || (!horizontal && this.bio == biome.canyon))
                            {
                                if (map[starty - 1, startx - 1] == Terrain.MOUNAIN && map[starty - 1, startx + 1] == Terrain.MOUNAIN)
                                {
                                    f.Xpos = startx;
                                    f.Ypos = starty - 1 + 30;
                                    f.CanShuffle = false;
                                    forcedPlaced--;
                                    forced++;

                                }
                            }
                            else
                            {
                                if (map[starty - 1, startx - 1] == Terrain.MOUNAIN && map[starty + 1, startx - 1] == Terrain.MOUNAIN)
                                {
                                    f.Xpos = startx - 1;
                                    f.Ypos = starty + 30;
                                    f.CanShuffle = false;
                                    forcedPlaced--;
                                    forced++;

                                }
                            }
                            minadjust = 0;
                            maxadjust = 4;
                        }
                        else if (adjust < 0)
                        {
                            if (horizontalPath)
                            {
                                if (map[starty + 1, startx - 1] == Terrain.MOUNAIN && map[starty + 1, startx + 1] == Terrain.MOUNAIN)
                                {
                                    f.Xpos = startx;
                                    f.Ypos = starty + 1 + 30;
                                    f.CanShuffle = false;
                                    forcedPlaced--;
                                    forced++;

                                }
                            }
                            else
                            {
                                if (map[starty - 1, startx + 1] == Terrain.MOUNAIN && map[starty + 1, startx + 1] == Terrain.MOUNAIN)
                                {
                                    f.Xpos = startx + 1;
                                    f.Ypos = starty + 30;
                                    f.CanShuffle = false;
                                    forcedPlaced--;
                                    forced++;

                                }
                            }
                            minadjust = -3;
                            maxadjust = 1;
                        }




                    }
                    else if (adjust == 0 && !cavePlaced)
                    {
                        if (k != 0)
                        {
                            vodcave1 = vodcave3;
                            vodcave2 = vodcave4;
                        }
                        map[vodcave1.Ypos - 30, vodcave1.Xpos] = Terrain.MOUNAIN;
                        map[starty, startx] = Terrain.CAVE;
                        map[starty + deltay, startx + deltax] = Terrain.MOUNAIN;
                        if (deltax != 0)
                        {
                            map[starty + 1, startx] = Terrain.MOUNAIN;
                            map[starty - 1, startx] = Terrain.MOUNAIN;
                        }
                        else
                        {
                            map[starty, startx + 1] = Terrain.MOUNAIN;
                            map[starty, startx - 1] = Terrain.MOUNAIN;
                        }
                        vodcave1.Xpos = startx;
                        vodcave1.Ypos = starty + 30;


                        if (hy.R.NextDouble() > .5 && vodRoutes != 2 && this.bio == biome.volcano)
                        {
                            if (horizontal)
                            {
                                deltax = -deltax;
                            }
                            else
                            {
                                deltay = -deltay;
                            }
                        }

                        if (horizontalPath)
                        {
                            if (starty > MAP_ROWS / 2)
                            {
                                starty += hy.R.Next(-9, -4);
                            }
                            else
                            {
                                starty += hy.R.Next(5, 10);
                            }
                        }
                        else
                        {
                            if (startx > MAP_COLS / 2)
                            {
                                startx += hy.R.Next(-9, -4);
                            }
                            else
                            {
                                startx += hy.R.Next(5, 10);
                            }
                        }
                        if (map[starty, startx] != Terrain.MOUNAIN && (this.bio == biome.volcano || this.bio == biome.canyon))
                        {
                            return false;
                        }
                        map[vodcave2.Ypos - 30, vodcave2.Xpos] = Terrain.MOUNAIN;
                        map[starty - deltay, startx - deltax] = Terrain.MOUNAIN;
                        map[starty, startx] = Terrain.CAVE;
                        if (deltax != 0)
                        {
                            map[starty + 1, startx] = Terrain.MOUNAIN;
                            map[starty - 1, startx] = Terrain.MOUNAIN;
                        }
                        else
                        {
                            map[starty, startx + 1] = Terrain.MOUNAIN;
                            map[starty, startx - 1] = Terrain.MOUNAIN;
                        }
                        vodcave2.Xpos = startx;
                        vodcave2.Ypos = starty + 30;
                        cavePlaced = true;
                        vodcave1.CanShuffle = false;
                        vodcave2.CanShuffle = false;
                        //startx += deltax;
                    }
                    else
                    {
                        minadjust = -3;
                        maxadjust = 4;
                    }
                    if (horizontalPath)
                    {
                        if (getLocationByCoords(Tuple.Create(starty + 30, startx + deltax)) != null)
                        {
                            map[starty, startx] = Terrain.MOUNAIN;
                            startx -= deltax;
                        }
                        else
                        {
                            startx += deltax;
                        }
                    }
                    else
                    {
                        if (getLocationByCoords(Tuple.Create(starty + 30 + deltay, startx)) != null)
                        {
                            map[starty, startx] = Terrain.MOUNAIN;
                            starty -= deltay;
                        }
                        else
                        {
                            starty += deltay;
                        }
                    }

                }

                if (this.bio != biome.volcano && this.bio != biome.canyon)
                {
                    map[starty, startx] = Terrain.LAVA;
                    if (deltax != 0)
                    {
                        map[starty + 1, startx] = Terrain.LAVA;
                        map[starty - 1, startx] = Terrain.LAVA;
                        map[starty + 1, startx + deltax] = Terrain.LAVA;
                        map[starty - 1, startx + deltax] = Terrain.LAVA;
                        map[starty, startx + deltax] = Terrain.LAVA;
                    }
                    else
                    {
                        map[starty, startx + 1] = Terrain.LAVA;
                        map[starty, startx - 1] = Terrain.LAVA;
                        map[starty + deltay, startx + 1] = Terrain.LAVA;
                        map[starty + deltay, startx - 1] = Terrain.LAVA;
                        map[starty + deltay, startx] = Terrain.LAVA;

                    }
                }
                if (horizontalPath)
                {

                    if (deltax < 0)
                    {
                        startx = palacex + 4;
                        starty = palacey;
                    }
                    else
                    {
                        startx = palacex - 4;
                        starty = palacey;
                    }
                }
                else
                {
                    if (deltay < 0)
                    {
                        startx = palacex;
                        starty = palacey + 4;
                    }
                    else
                    {
                        startx = palacex;
                        starty = palacey - 4;
                    }
                }
                deltax = -deltax;
                deltay = -deltay;
                minadjust = -1;
                maxadjust = 2;
                cavePlaced = false;
            }

            return true;
        }

        private void UpdateKasuto()
        {
            hy.ROMData.Put(0x1df79, (byte)(hkLoc.Ypos + hkLoc.ExternalWorld));
            hy.ROMData.Put(0x1dfac, (byte)(hkLoc.Ypos - 30));
            hy.ROMData.Put(0x1dfb2, (byte)(hkLoc.Xpos + 1));
            hy.ROMData.Put(0x1ccd4, (byte)(hkLoc.Xpos + hkLoc.Secondpartofcave));
            hy.ROMData.Put(0x1ccdb, (byte)(hkLoc.Ypos));
            int connection = hkLoc.MemAddress - baseAddr;
            hy.ROMData.Put(0x1df77, (byte)connection);
            hkLoc.NeedHammer = true;
            if (hkLoc == newKasuto || hkLoc == newKasuto2)
            {
                newKasuto.NeedHammer = true;
                newKasuto2.NeedHammer = true;
            }
            if (hy.Props.vanillaOriginal || this.bio != biome.vanillaShuffle)
            {
                Terrain t = terrains[hkLoc.MemAddress];
                hy.ROMData.Put(0x1df75, (byte)t);
                if (t == Terrain.PALACE)
                {
                    hy.ROMData.Put(0x1dfb6, 0x60);
                    hy.ROMData.Put(0x1dfbb, 0x61);

                    hy.ROMData.Put(0x1dfc0, 0x62);

                    hy.ROMData.Put(0x1dfc5, 0x63);
                }
                else if (t == Terrain.SWAMP)
                {
                    hy.ROMData.Put(0x1dfb6, 0x6F);
                    hy.ROMData.Put(0x1dfbb, 0x6F);

                    hy.ROMData.Put(0x1dfc0, 0x6F);

                    hy.ROMData.Put(0x1dfc5, 0x6F);
                }
                else if (t == Terrain.LAVA || t == Terrain.WALKABLEWATER)
                {
                    hy.ROMData.Put(0x1dfb6, 0x6E);
                    hy.ROMData.Put(0x1dfbb, 0x6E);

                    hy.ROMData.Put(0x1dfc0, 0x6E);

                    hy.ROMData.Put(0x1dfc5, 0x6E);
                }
                else if (t == Terrain.FOREST)
                {
                    hy.ROMData.Put(0x1dfb6, 0x68);
                    hy.ROMData.Put(0x1dfbb, 0x69);

                    hy.ROMData.Put(0x1dfc0, 0x6A);

                    hy.ROMData.Put(0x1dfc5, 0x6B);
                }
                else if (t == Terrain.GRAVE)
                {
                    hy.ROMData.Put(0x1dfb6, 0x70);
                    hy.ROMData.Put(0x1dfbb, 0x71);

                    hy.ROMData.Put(0x1dfc0, 0x7F);

                    hy.ROMData.Put(0x1dfc5, 0x7F);
                }
                else if (t == Terrain.ROAD)
                {
                    hy.ROMData.Put(0x1dfb6, 0xFE);
                    hy.ROMData.Put(0x1dfbb, 0xFE);

                    hy.ROMData.Put(0x1dfc0, 0xFE);

                    hy.ROMData.Put(0x1dfc5, 0xFE);
                }
                else if (t == Terrain.BRIDGE)
                {
                    hy.ROMData.Put(0x1dfb6, 0x5A);
                    hy.ROMData.Put(0x1dfbb, 0x5B);

                    hy.ROMData.Put(0x1dfc0, 0x5A);

                    hy.ROMData.Put(0x1dfc5, 0x5B);
                }
                else if (t == Terrain.CAVE)
                {
                    hy.ROMData.Put(0x1dfb6, 0x72);
                    hy.ROMData.Put(0x1dfbb, 0x73);

                    hy.ROMData.Put(0x1dfc0, 0x72);

                    hy.ROMData.Put(0x1dfc5, 0x73);
                }
                else if (t == Terrain.DESERT)
                {
                    hy.ROMData.Put(0x1dfb6, 0x6C);
                    hy.ROMData.Put(0x1dfbb, 0x6C);

                    hy.ROMData.Put(0x1dfc0, 0x6C);

                    hy.ROMData.Put(0x1dfc5, 0x6C);
                }
                else if (t == Terrain.TOWN)
                {
                    hy.ROMData.Put(0x1dfb6, 0x5C);
                    hy.ROMData.Put(0x1dfbb, 0x5D);

                    hy.ROMData.Put(0x1dfc0, 0x5E);

                    hy.ROMData.Put(0x1dfc5, 0x5F);
                }
            }
        }

        private void DrawHiddenKasuto()
        {
            if (hy.Props.shuffleHidden)
            {
                hkLoc = AllLocations[hy.R.Next(AllLocations.Count)];
                while (hkLoc == null || hkLoc == raft || hkLoc == bridge || hkLoc == cave1 || hkLoc == cave2 || connections.ContainsKey(hkLoc) || !hkLoc.CanShuffle || ((this.bio != biome.vanilla && this.bio != biome.vanillaShuffle) && hkLoc.TerrainType == Terrain.LAVA && hkLoc.PassThrough !=0))
                {
                    hkLoc = AllLocations[hy.R.Next(AllLocations.Count)];
                }
            }
            else
            {
                hkLoc = newKasuto;
            }
            hkLoc.TerrainType = Terrain.FOREST;
            hkLoc.NeedHammer = true;
            unimportantLocs.Remove(hkLoc);
            //hkLoc.CanShuffle = false;
            //map[hkLoc.Ypos - 30, hkLoc.Xpos] = terrain.forest;
        }

        private bool DrawHiddenPalace()
        {
            bool done = false;
            int xpos = hy.R.Next(6, MAP_COLS - 6);
            int ypos = hy.R.Next(6, MAP_ROWS - 6);
            if (hy.Props.shuffleHidden)
            {
                hpLoc = AllLocations[hy.R.Next(AllLocations.Count)];
                while(hpLoc == null || hpLoc == raft || hpLoc == bridge || hpLoc == cave1 || hpLoc == cave2 || connections.ContainsKey(hpLoc) || !hpLoc.CanShuffle || hpLoc == hkLoc || ((this.bio != biome.vanilla && this.bio != biome.vanillaShuffle) && hpLoc.TerrainType == Terrain.LAVA && hpLoc.PassThrough != 0))
                {
                    hpLoc = AllLocations[hy.R.Next(AllLocations.Count)];
                }
            }
            else
            {
                hpLoc = palace6;
            }
            int tries = 0;
            while (!done && tries < 1000)
            {
                xpos = hy.R.Next(6, MAP_COLS - 6);
                ypos = hy.R.Next(6, MAP_ROWS - 6);
                done = true;
                for (int i = ypos - 3; i < ypos + 4; i++)
                {
                    for (int j = xpos - 3; j < xpos + 4; j++)
                    {
                        if (map[i, j] != Terrain.NONE)
                        {
                            done = false;
                        }
                    }
                }
                tries++;
            }
            if (!done)
            {
                return false;
            }
            Terrain t = walkable[hy.R.Next(walkable.Count())];
            while (t == Terrain.FOREST)
            {
                t = walkable[hy.R.Next(walkable.Count())];
            }
            //t = terrain.desert;
            for (int i = ypos - 3; i < ypos + 4; i++)
            {
                for (int j = xpos - 3; j < xpos + 4; j++)
                {
                    if ((i == ypos - 2 && j == xpos) || (i == ypos && j == xpos - 2) || (i == ypos && j == xpos + 2))
                    {
                        map[i, j] = Terrain.MOUNAIN;
                    }
                    else
                    {
                        map[i, j] = t;
                    }
                }
            }
            //map[hpLoc.Ypos - 30, hpLoc.Xpos] = map[hpLoc.Ypos - 29, hpLoc.Xpos];
            hpLoc.Xpos = xpos;
            hpLoc.Ypos = ypos + 2 + 30;
            hpCallSpot.Xpos = xpos;
            hpCallSpot.Ypos = ypos + 30;
            hy.ROMData.Put(0x1df70, (byte)t);
            hpLoc.CanShuffle = false;
            return true;
        }

        public void AllReachable()
        {
            if (!Allreached)
            {
                base.allReachable();
                if (!hpLoc.Reachable || !hkLoc.Reachable || !newKasuto2.Reachable)
                {
                    Allreached = false;
                }
            }
        }

        public void UpdateHPspot()
        {

            if (this.bio != biome.vanilla && this.bio != biome.vanillaShuffle)
            {
                hy.ROMData.Put(0x8382, (byte)hpCallSpot.Ypos);
                hy.ROMData.Put(0x8388, (byte)hpCallSpot.Xpos);
            }
            int pos = hpLoc.Ypos;

            hy.ROMData.Put(0x1df78, (byte)(pos + hpLoc.ExternalWorld));
            hy.ROMData.Put(0x1df84, 0xff);
            hy.ROMData.Put(0x1ccc0, (byte)pos);
            int connection = hpLoc.MemAddress - baseAddr;
            hy.ROMData.Put(0x1df76, (byte)connection);
            hpLoc.NeedRecorder = true;
            if (hpLoc == newKasuto || hpLoc == newKasuto2)
            {
                newKasuto.NeedRecorder = true;
                newKasuto2.NeedRecorder = true;
            }
            if (hy.Props.vanillaOriginal || this.bio != biome.vanillaShuffle)
            {
                hy.ROMData.Put(0x1df74, (byte)hpLoc.TerrainType);
                if (hpLoc.TerrainType == Terrain.PALACE)
                {
                    hy.ROMData.Put(0x1df7d, 0x60);
                    hy.ROMData.Put(0x1df82, 0x61);

                    hy.ROMData.Put(0x1df7e, 0x62);

                    hy.ROMData.Put(0x1df83, 0x63);
                }
                else if (hpLoc.TerrainType == Terrain.SWAMP)
                {
                    hy.ROMData.Put(0x1df7d, 0x6F);
                    hy.ROMData.Put(0x1df82, 0x6F);

                    hy.ROMData.Put(0x1df7e, 0x6F);

                    hy.ROMData.Put(0x1df83, 0x6F);
                }
                else if (hpLoc.TerrainType == Terrain.LAVA || hpLoc.TerrainType == Terrain.WALKABLEWATER)
                {
                    hy.ROMData.Put(0x1df7d, 0x6E);
                    hy.ROMData.Put(0x1df82, 0x6E);

                    hy.ROMData.Put(0x1df7e, 0x6E);

                    hy.ROMData.Put(0x1df83, 0x6E);
                }
                else if (hpLoc.TerrainType == Terrain.FOREST)
                {
                    hy.ROMData.Put(0x1df7d, 0x68);
                    hy.ROMData.Put(0x1df82, 0x69);

                    hy.ROMData.Put(0x1df7e, 0x6A);

                    hy.ROMData.Put(0x1df83, 0x6B);
                }
                else if (hpLoc.TerrainType == Terrain.GRAVE)
                {
                    hy.ROMData.Put(0x1df7d, 0x70);
                    hy.ROMData.Put(0x1df82, 0x71);

                    hy.ROMData.Put(0x1df7e, 0x7F);

                    hy.ROMData.Put(0x1df83, 0x7F);
                }
                else if (hpLoc.TerrainType == Terrain.ROAD)
                {
                    hy.ROMData.Put(0x1df7d, 0xFE);
                    hy.ROMData.Put(0x1df82, 0xFE);

                    hy.ROMData.Put(0x1df7e, 0xFE);

                    hy.ROMData.Put(0x1df83, 0xFE);
                }
                else if (hpLoc.TerrainType == Terrain.BRIDGE)
                {
                    hy.ROMData.Put(0x1df7d, 0x5A);
                    hy.ROMData.Put(0x1df82, 0x5B);

                    hy.ROMData.Put(0x1df7e, 0x5A);

                    hy.ROMData.Put(0x1df83, 0x5B);
                }
                else if (hpLoc.TerrainType == Terrain.CAVE)
                {
                    hy.ROMData.Put(0x1df7d, 0x72);
                    hy.ROMData.Put(0x1df82, 0x73);

                    hy.ROMData.Put(0x1df7e, 0x72);

                    hy.ROMData.Put(0x1df83, 0x73);
                }
                else if (hpLoc.TerrainType == Terrain.DESERT)
                {
                    hy.ROMData.Put(0x1df7d, 0x6C);
                    hy.ROMData.Put(0x1df82, 0x6C);

                    hy.ROMData.Put(0x1df7e, 0x6C);

                    hy.ROMData.Put(0x1df83, 0x6C);
                }
                else if (hpLoc.TerrainType == Terrain.TOWN)
                {
                    hy.ROMData.Put(0x1df7d, 0x5C);
                    hy.ROMData.Put(0x1df82, 0x5D);

                    hy.ROMData.Put(0x1df7e, 0x5E);

                    hy.ROMData.Put(0x1df83, 0x5F);
                }
            }
           


            int ppu_addr1 = 0x2000 + 2 * (32 * (hpLoc.Ypos % 15) + (hpLoc.Xpos % 16)) + 2048 * (hpLoc.Ypos % 30 / 15);
            int ppu_addr2 = ppu_addr1 + 32;
            int ppu1low = ppu_addr1 & 0x00ff;
            int ppu1high = (ppu_addr1 >> 8) & 0xff;
            int ppu2low = ppu_addr2 & 0x00ff;
            int ppu2high = (ppu_addr2 >> 8) & 0xff;
            hy.ROMData.Put(0x1df7a, (byte)ppu1high);
            hy.ROMData.Put(0x1df7b, (byte)ppu1low);
            hy.ROMData.Put(0x1df7f, (byte)ppu2high);
            hy.ROMData.Put(0x1df80, (byte)ppu2low);

        }




        public void UpdateVisit()
        {
            updateReachable();

            foreach (Location l in AllLocations)
            {
                if (l.Ypos > 30)
                {
                    if (v[l.Ypos - 30, l.Xpos])
                    {
                        if ((!l.NeedRecorder || (l.NeedRecorder && hy.itemGet[(int)Items.horn]) ) && (!l.NeedHammer || (l.NeedHammer && hy.itemGet[(int)Items.hammer]) )&& (!l.Needboots || (l.Needboots && hy.itemGet[(int)Items.boots])))
                        {
                            l.Reachable = true;
                            if (connections.Keys.Contains(l))
                            {
                                Location l2 = connections[l];

                                if ((l.NeedBagu && (hy.westHyrule.bagu.Reachable || hy.SpellGet[(int)Spells.fairy])))
                                {
                                    l2.Reachable = true;
                                    v[l2.Ypos - 30, l2.Xpos] = true;
                                }

                                if (l.NeedFairy && hy.SpellGet[(int)Spells.fairy])
                                {
                                    l2.Reachable = true;
                                    v[l2.Ypos - 30, l2.Xpos] = true;
                                }

                                if (l.NeedJump && (hy.SpellGet[(int)Spells.jump] || hy.SpellGet[(int)Spells.fairy]))
                                {
                                    l2.Reachable = true;
                                    v[l2.Ypos - 30, l2.Xpos] = true;
                                }

                                if (!l.NeedFairy && !l.NeedBagu && !l.NeedJump)
                                {
                                    l2.Reachable = true;
                                    v[l2.Ypos - 30, l2.Xpos] = true;
                                }
                            }
                        }
                    }
                }
                if (newKasuto.Reachable && newKasuto.TownNum == Town.NEW_KASUTO)
                {
                    newKasuto2.Reachable = true;
                }
            }
        }

        private double ComputeDistance(Location l, Location l2)
        {
            return Math.Sqrt(Math.Pow(l.Xpos - l2.Xpos, 2) + Math.Pow(l.Ypos - l2.Ypos, 2));
        }



        private void DrawMountains()
        {
            //create some mountains
            int mounty = hy.R.Next(MAP_COLS / 3 - 10, MAP_COLS / 3 + 10);
            map[mounty, 0] = Terrain.MOUNAIN;
            bool placedSpider = false;


            int endmounty = hy.R.Next(MAP_COLS / 3 - 10, MAP_COLS / 3 + 10);
            int endmountx = hy.R.Next(2, 8);
            int x2 = 0;
            int y2 = mounty;
            int roadEncounters = 0;
            while (x2 != (MAP_COLS - endmountx) || y2 != endmounty)
            {
                if (Math.Abs(x2 - (MAP_COLS - endmountx)) >= Math.Abs(y2 - endmounty))
                {
                    if (x2 > MAP_COLS - endmountx)
                    {
                        x2--;
                    }
                    else
                    {
                        x2++;
                    }
                }
                else
                {
                    if (y2 > endmounty)
                    {
                        y2--;
                    }
                    else
                    {
                        y2++;
                    }
                }
                if (x2 != MAP_COLS - endmountx || y2 != endmounty)
                {
                    if (map[y2, x2] == Terrain.NONE)
                    {
                        map[y2, x2] = Terrain.MOUNAIN;
                    }
                    else if (map[y2, x2] == Terrain.ROAD)
                    {
                        if (!placedSpider)
                        {
                            map[y2, x2] = Terrain.RIVER_DEVIL;
                            placedSpider = true;
                        }
                        else if (map[y2, x2 + 1] == Terrain.NONE && (((y2 > 0 && map[y2 - 1, x2] == Terrain.ROAD) && (y2 < MAP_ROWS - 1 && map[y2 + 1, x2] == Terrain.ROAD)) || ((x2 > 0 && map[y2, x2 - 0] == Terrain.ROAD) && (x2 < MAP_COLS - 1 && map[y2, x2 + 1] == Terrain.ROAD))))
                        {
                            if (roadEncounters == 0)
                            {
                                Location roadEnc = getLocationByMem(0x8631);
                                roadEnc.Xpos = x2;
                                roadEnc.Ypos = y2 + 30;
                                roadEnc.CanShuffle = false;
                                roadEncounters++;
                            }
                            else if (roadEncounters == 1)
                            {
                                Location roadEnc = getLocationByMem(0x8632);
                                roadEnc.Xpos = x2;
                                roadEnc.Ypos = y2 + 30;
                                roadEnc.CanShuffle = false;
                                roadEncounters++;
                            }
                            else if (roadEncounters == 2)
                            {
                                Location roadEnc = getLocationByMem(0x8633);
                                roadEnc.Xpos = x2;
                                roadEnc.Ypos = y2 + 30;
                                roadEnc.CanShuffle = false;
                                roadEncounters++;
                            }
                            else if (roadEncounters == 3)
                            {
                                Location roadEnc = getLocationByMem(0x8634);
                                roadEnc.Xpos = x2;
                                roadEnc.Ypos = y2 + 30;
                                roadEnc.CanShuffle = false;
                                roadEncounters++;
                            }
                        }
                    }
                }
            }

            mounty = hy.R.Next(MAP_COLS * 2 / 3 - 10, MAP_COLS * 2 / 3 + 10);
            map[mounty, 0] = Terrain.MOUNAIN;

            endmounty = hy.R.Next(MAP_COLS * 2 / 3 - 10, MAP_COLS * 2 / 3 + 10);
            endmountx = hy.R.Next(2, 8);
            x2 = 0;
            y2 = mounty;
            while (x2 != (MAP_COLS - endmountx) || y2 != endmounty)
            {
                if (Math.Abs(x2 - (MAP_COLS - endmountx)) >= Math.Abs(y2 - endmounty))
                {
                    if (x2 > MAP_COLS - endmountx)
                    {
                        x2--;
                    }
                    else
                    {
                        x2++;
                    }
                }
                else
                {
                    if (y2 > endmounty)
                    {
                        y2--;
                    }
                    else
                    {
                        y2++;
                    }
                }
                if (x2 != MAP_COLS - endmountx || y2 != endmounty)
                {
                    if (map[y2, x2] == Terrain.NONE)
                    {
                        map[y2, x2] = Terrain.MOUNAIN;
                    }
                    else if (map[y2, x2] == Terrain.ROAD)
                    {
                        if (!placedSpider)
                        {
                            map[y2, x2] = Terrain.RIVER_DEVIL;
                            placedSpider = true;
                        }
                        else if (map[y2, x2 + 1] == Terrain.NONE && (((y2 > 0 && map[y2 - 1, x2] == Terrain.ROAD) && (y2 < MAP_ROWS - 1 && map[y2 + 1, x2] == Terrain.ROAD)) || ((x2 > 0 && map[y2, x2 - 0] == Terrain.ROAD) && (x2 < MAP_COLS - 1 && map[y2, x2 + 1] == Terrain.ROAD))))
                        {
                            if (roadEncounters == 0)
                            {
                                Location roadEnc = getLocationByMem(0x8631);
                                roadEnc.Xpos = x2;
                                roadEnc.Ypos = y2 + 30;
                                roadEnc.CanShuffle = false;
                                roadEncounters++;
                            }
                            else if (roadEncounters == 1)
                            {
                                Location roadEnc = getLocationByMem(0x8632);
                                roadEnc.Xpos = x2;
                                roadEnc.Ypos = y2 + 30;
                                roadEnc.CanShuffle = false;
                                roadEncounters++;
                            }
                            else if (roadEncounters == 2)
                            {
                                Location roadEnc = getLocationByMem(0x8633);
                                roadEnc.Xpos = x2;
                                roadEnc.Ypos = y2 + 30;
                                roadEnc.CanShuffle = false;
                                roadEncounters++;
                            }
                            else if (roadEncounters == 3)
                            {
                                Location roadEnc = getLocationByMem(0x8634);
                                roadEnc.Xpos = x2;
                                roadEnc.Ypos = y2 + 30;
                                roadEnc.CanShuffle = false;
                                roadEncounters++;
                            }
                        }
                    }
                }
            }

            
        }



        
    }


}
