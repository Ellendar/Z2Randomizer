using NLog;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Z2Randomizer.Core.Overworld;

//6A31 - address in memory of kasuto y coord;
//6A35 - address in memory of palace 6 y coord
public class EastHyrule : World
{
    private int bridgeCount;

    private readonly new Logger logger = LogManager.GetCurrentClassLogger();

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
        { RomMap.VANILLA_DESERT_TILE_LOCATION, Terrain.DESERT },
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

    public Location locationAtPalace5;
    public Location locationAtPalace6;
    public Location waterTile;
    public Location desertTile;
    public Location darunia;
    public Location newKasuto;
    public Location newKasuto2;
    public Location nabooru;
    public Location oldKasuto;
    public Location locationAtGP;
    public Location pbagCave1;
    public Location pbagCave2;
    public Location hiddenPalaceCallSpot;
    private bool canyonShort;
    private Location vodcave1;
    private Location vodcave2;
    public Location hiddenPalaceLocation;
    public Location hiddenKasutoLocation;

    private const int MAP_ADDR = 0xb480;
    public static int debug = 0;


    public EastHyrule(Hyrule hy)
        : base(hy)
    {
        baseAddr = 0x862F;
        LoadLocations(0x863E, 6, terrains, Continent.EAST);
        LoadLocations(0x863A, 2, terrains, Continent.EAST);

        LoadLocations(0x862F, 11, terrains, Continent.EAST);
        LoadLocations(0x8644, 1, terrains, Continent.EAST);
        LoadLocations(0x863C, 2, terrains, Continent.EAST);
        LoadLocations(0x8646, 10, terrains, Continent.EAST);
        //loadLocations(0x8657, 2, terrains, continent.east);
        LoadLocations(0x865C, 1, terrains, Continent.EAST);
        LoadLocations(0x865E, 1, terrains, Continent.EAST);
        LoadLocations(0x8660, 1, terrains, Continent.EAST);
        LoadLocations(0x8662, 4, terrains, Continent.EAST);

        //reachableAreas = new HashSet<string>();

        connections.Add(GetLocationByMem(0x863A), GetLocationByMem(0x863B));
        connections.Add(GetLocationByMem(0x863B), GetLocationByMem(0x863A));
        connections.Add(GetLocationByMem(0x863E), GetLocationByMem(0x863F));
        connections.Add(GetLocationByMem(0x863F), GetLocationByMem(0x863E));
        connections.Add(GetLocationByMem(0x8640), GetLocationByMem(0x8641));
        connections.Add(GetLocationByMem(0x8641), GetLocationByMem(0x8640));
        connections.Add(GetLocationByMem(0x8642), GetLocationByMem(0x8643));
        connections.Add(GetLocationByMem(0x8643), GetLocationByMem(0x8642));

        locationAtPalace6 = GetLocationByMem(0x8664);
        locationAtPalace6.PalNum = 6;
        darunia = GetLocationByMem(0x865E);
        locationAtPalace5 = GetLocationByMap(0x23, 0x0E);
        locationAtPalace5.PalNum = 5;

        newKasuto = GetLocationByMem(0x8660);
        newKasuto2 = new Location(newKasuto.LocationBytes, newKasuto.TerrainType, newKasuto.MemAddress, Continent.EAST);
        waterTile = GetLocationByMem(0x8639);
        waterTile.NeedBoots = true;
        desertTile = GetLocationByMem(RomMap.VANILLA_DESERT_TILE_LOCATION);

        if (locationAtPalace5 == null)
        {
            locationAtPalace5 = GetLocationByMem(0x8657);
            locationAtPalace5.PalNum = 5;
        }

        hiddenPalaceCallSpot = new Location();
        hiddenPalaceCallSpot.Xpos = 0;
        hiddenPalaceCallSpot.Ypos = 0;

        enemyAddr = 0x88B0;
        enemies = new List<int> { 03, 04, 05, 0x11, 0x12, 0x14, 0x16, 0x18, 0x19, 0x1A, 0x1B, 0x1C };
        flyingEnemies = new List<int> { 0x06, 0x07, 0x0A, 0x0D, 0x0E, 0x15 };
        generators = new List<int> { 0x0B, 0x0F, 0x17 };
        smallEnemies = new List<int> { 0x03, 0x04, 0x05, 0x11, 0x12, 0x16 };
        largeEnemies = new List<int> { 0x14, 0x18, 0x19, 0x1A, 0x1B, 0x1C };
        enemyPtr = 0x85B1;
        nabooru = GetLocationByMem(0x865C);
        oldKasuto = GetLocationByMem(0x8662);
        locationAtGP = GetLocationByMem(0x8665);
        locationAtGP.PalNum = 7;
        locationAtGP.Item = Item.DO_NOT_USE;
        pbagCave1 = GetLocationByMem(0x863C);
        pbagCave2 = GetLocationByMem(0x863D);
        VANILLA_MAP_ADDR = 0x9056;

        overworldMaps = new List<int> { 0x22, 0x1D, 0x27, 0x35, 0x30, 0x1E, 0x28, 0x3C };

        MAP_ROWS = 75;
        MAP_COLS = 64;

        walkableTerrains = new List<Terrain>() { Terrain.DESERT, Terrain.GRASS, Terrain.FOREST, Terrain.SWAMP, Terrain.GRAVE };
        randomTerrains = new List<Terrain> { Terrain.DESERT, Terrain.GRASS, Terrain.FOREST, Terrain.SWAMP, Terrain.GRAVE, Terrain.MOUNTAIN, Terrain.WALKABLEWATER };
        

        if (hy.Props.EastBiome == Biome.ISLANDS)
        {
            this.biome = Biome.ISLANDS;
        }
        else if (hy.Props.EastBiome == Biome.CANYON || hy.Props.EastBiome == Biome.DRY_CANYON)
        {
            this.biome = Biome.CANYON;
        }
        else if (hy.Props.EastBiome == Biome.VOLCANO)
        {
            this.biome = Biome.VOLCANO;
        }
        else if (hy.Props.EastBiome == Biome.MOUNTAINOUS)
        {
            this.biome = Biome.MOUNTAINOUS;
        }
        else if (hy.Props.EastBiome == Biome.VANILLA)
        {
            this.biome = Biome.VANILLA;
        }
        else if (hy.Props.EastBiome == Biome.VANILLA_SHUFFLE)
        {
            this.biome = Biome.VANILLA_SHUFFLE;
        }
        else
        {
            this.biome = Biome.VANILLALIKE;
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
        locationAtPalace6.ExternalWorld = 128;
        hiddenPalaceLocation = locationAtPalace6;
        hiddenKasutoLocation = newKasuto;
    }

    public bool Terraform()
    {
        foreach (Location location in AllLocations)
        {
            location.CanShuffle = true;
            location.NeedHammer = false;
            location.NeedRecorder = false;
            if (location != raft && location != bridge && location != cave1 && location != cave2)
            {
                location.TerrainType = terrains[location.MemAddress];
            }
        }
        if (hyrule.Props.HideLocs)
        {
            unimportantLocs = new List<Location>
            {
                GetLocationByMem(0x862F),
                GetLocationByMem(0x8630),
                GetLocationByMem(0x8644),
                GetLocationByMem(0x8646),
                GetLocationByMem(0x8647),
                GetLocationByMem(0x8648),
                GetLocationByMem(0x864A),
                GetLocationByMem(0x864B),
                GetLocationByMem(0x864C)
            };

        }
        if (this.biome == Biome.VANILLA || this.biome == Biome.VANILLA_SHUFFLE)
        {
            MAP_ROWS = 75;
            MAP_COLS = 64;
            ReadVanillaMap();

           

            if (this.biome == Biome.VANILLA_SHUFFLE)
            {
                areasByLocation = new SortedDictionary<string, List<Location>>
                {
                    { "north2", new List<Location>() },
                    { "mid2", new List<Location>() },
                    { "south", new List<Location>() },
                    { "vod", new List<Location>() },
                    { "kasuto", new List<Location>() },
                    { "gp", new List<Location>() },
                    { "boots", new List<Location>() },
                    { "boots1", new List<Location>() },
                    { "hammer2", new List<Location>() }
                };
                //areasByLocation.Add("horn", new List<Location>());
                foreach (Location location in AllLocations)
                {
                    areasByLocation[section[location.Coords]].Add(GetLocationByCoords(location.Coords));
                }

                ChooseConn("kasuto", connections, true);
                ChooseConn("vod", connections, true);
                ChooseConn("gp", connections, true);

                if(!hyrule.Props.ShuffleHidden)
                {
                    newKasuto.CanShuffle = false;
                    locationAtPalace6.CanShuffle = false;
                }
                ShuffleLocations(AllLocations);
                if (hyrule.Props.VanillaShuffleUsesActualTerrain)
                {
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
                raft.PassThrough = 0;
                bridge.PassThrough = 0;
                //Issue #2: Desert tile passthrough causes the wrong screen to load, making the item unobtainable.
                desertTile.PassThrough = 0;
                debug++;

                desertTile.MapPage = 64;
                desertTile.UpdateBytes();
                Location desert = GetLocationByMem(0x8646);
                Location swamp = GetLocationByMem(0x8644);
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
            hiddenKasutoLocation = GetLocationByCoords(Tuple.Create(81, 61));
            hiddenPalaceLocation = GetLocationByCoords(Tuple.Create(102, 45));

            if (hyrule.Props.HiddenKasuto)
            {
                
                if(connections.ContainsKey(hiddenKasutoLocation) || hiddenKasutoLocation == raft || hiddenKasutoLocation == bridge)
                {
                    return false;
                }
            }
            if (hyrule.Props.HiddenPalace)
            {
                if (connections.ContainsKey(hiddenPalaceLocation) || hiddenPalaceLocation == raft || hiddenPalaceLocation == bridge)
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
            if (hyrule.Props.CanWalkOnWaterWithBoots)
            {
                water = Terrain.WALKABLEWATER;
            }

            bytesWritten = 2000;
            this.locationAtGP.CanShuffle = false;
            Terrain riverTerrain = Terrain.MOUNTAIN;
            while (bytesWritten > MAP_SIZE_BYTES)
            {
                map = new Terrain[MAP_ROWS, MAP_COLS];

                for (int i = 0; i < MAP_ROWS; i++)
                {
                    for (int j = 0; j < MAP_COLS; j++)
                    {
                        map[i, j] = Terrain.NONE;
                    }
                }

                if (this.biome == Biome.ISLANDS)
                {
                    riverTerrain = water;
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
                    int cols = hyrule.RNG.Next(2, 4);
                    int rows = hyrule.RNG.Next(2, 4);
                    List<int> pickedC = new List<int>();
                    List<int> pickedR = new List<int>();

                    while (cols > 0)
                    {
                        int col = hyrule.RNG.Next(1, MAP_COLS - 1);
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
                        int row = hyrule.RNG.Next(1, MAP_ROWS - 1);
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
                    walkableTerrains = new List<Terrain>() { Terrain.LAVA, Terrain.DESERT, Terrain.GRASS, Terrain.FOREST, Terrain.SWAMP, Terrain.GRAVE };
                    randomTerrains = new List<Terrain> { Terrain.LAVA, Terrain.DESERT, Terrain.GRASS, Terrain.FOREST, Terrain.SWAMP, Terrain.GRAVE, Terrain.MOUNTAIN, water };




                }
                else if (this.biome == Biome.CANYON)
                {
                    isHorizontal = hyrule.RNG.NextDouble() > 0.5;
                    riverTerrain = water;
                    if (hyrule.Props.EastBiome == Biome.DRY_CANYON)
                    {
                        riverTerrain = Terrain.DESERT;
                    }
                    //riverT = terrain.lava;
                    walkableTerrains = new List<Terrain>() { Terrain.DESERT, Terrain.GRASS, Terrain.FOREST, Terrain.GRAVE, Terrain.MOUNTAIN };
                    randomTerrains = new List<Terrain> { Terrain.DESERT, Terrain.GRASS, Terrain.FOREST, Terrain.GRAVE, Terrain.MOUNTAIN, water };


                    DrawCanyon(riverTerrain);
                    this.walkableTerrains.Remove(Terrain.MOUNTAIN);

                    locationAtGP.CanShuffle = false;


                }
                else if (this.biome == Biome.VOLCANO)
                {
                    isHorizontal = hyrule.RNG.NextDouble() > .5;

                    DrawCenterMountain();



                    walkableTerrains = new List<Terrain>() { Terrain.LAVA, Terrain.DESERT, Terrain.GRASS, Terrain.FOREST, Terrain.SWAMP, Terrain.GRAVE };
                    randomTerrains = new List<Terrain> { Terrain.LAVA, Terrain.DESERT, Terrain.GRASS, Terrain.FOREST, Terrain.SWAMP, Terrain.GRAVE, Terrain.MOUNTAIN, water};


                }
                else if (this.biome == Biome.MOUNTAINOUS)
                {
                    riverTerrain = Terrain.MOUNTAIN;
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
                    MakeVolcano();

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
                    walkableTerrains = new List<Terrain>() { Terrain.DESERT, Terrain.GRASS, Terrain.FOREST, Terrain.SWAMP, Terrain.GRAVE };
                    randomTerrains = new List<Terrain> { Terrain.DESERT, Terrain.GRASS, Terrain.FOREST, Terrain.SWAMP, Terrain.GRAVE, Terrain.MOUNTAIN, water };
                }
                else
                {
                    walkableTerrains = new List<Terrain>() { Terrain.DESERT, Terrain.GRASS, Terrain.FOREST, Terrain.SWAMP, Terrain.GRAVE };
                    randomTerrains = new List<Terrain> { Terrain.DESERT, Terrain.GRASS, Terrain.FOREST, Terrain.SWAMP, Terrain.GRAVE, Terrain.MOUNTAIN, water };
                    MakeVolcano();


                    DrawMountains();
                    DrawRiver(new List<Location>() { GetLocationByMem(0x8635), GetLocationByMem(0x8636) });
                }


                if (hyrule.Props.HiddenKasuto)
                {
                    DrawHiddenKasuto();
                }
                if (hyrule.Props.HiddenPalace)
                {
                    bool hp = DrawHiddenPalace();
                    if (!hp)
                    {
                        return false;
                    }
                }
                Direction raftDirection = Direction.WEST;
                if (hyrule.Props.ContinentConnections != ContinentConnectionType.NORMAL && this.biome != Biome.CANYON)
                {
                    raftDirection = (Direction)hyrule.RNG.Next(4);
                }
                else if (this.biome == Biome.CANYON || this.biome == Biome.VOLCANO)
                {
                    raftDirection = isHorizontal ? DirectionExtensions.RandomHorizontal(hyrule.RNG) : DirectionExtensions.RandomVertical(hyrule.RNG);

                }
                if (raft != null)
                {
                    DrawOcean(raftDirection);
                }


                Direction bridgeDirection = Direction.EAST;
                do
                {
                    if (this.biome != Biome.CANYON && this.biome != Biome.VOLCANO)
                    {
                        bridgeDirection = (Direction)hyrule.RNG.Next(4);
                    }
                    else
                    {
                        bridgeDirection = (Direction)hyrule.RNG.Next(2);
                        if (isHorizontal)
                        {
                            bridgeDirection += 2;
                        }
                    }
                } while (bridgeDirection == raftDirection);

                if (bridge != null)
                {
                    DrawOcean(bridgeDirection);
                }

                bool b = PlaceLocations(riverTerrain);
                if (!b)
                {
                    return false;
                }

                //WHAT?
                if (hyrule.Props.HideLocs)
                {
                    PlaceRandomTerrain(50);
                }
                else
                {
                    PlaceRandomTerrain(25);

                }
                randomTerrains.Add(Terrain.LAVA);
                if (!GrowTerrain())
                {
                    return false;
                }
                randomTerrains.Remove(Terrain.LAVA);
                if (raft != null)
                {
                    bool r = DrawRaft(false, raftDirection);
                    if (!r)
                    {
                        return false;
                    }
                }

                if (bridge != null)
                {
                    bool b2 = DrawRaft(true, bridgeDirection);
                    if (!b2)
                    {
                        return false;
                    }
                }

                if (this.biome == Biome.VOLCANO || this.biome == Biome.CANYON)
                {
                    bool f = MakeVolcano();
                    if (!f)
                    {
                        return false;
                    }
                }
                PlaceHiddenLocations();
                if (this.biome == Biome.VANILLALIKE)
                {
                    ConnectIslands(4, false, Terrain.MOUNTAIN, false, false, true);

                    ConnectIslands(3, false, water, true, false, false);

                }
                if (this.biome == Biome.ISLANDS)
                {
                    ConnectIslands(100, false, riverTerrain, true, false, true);
                }
                if (this.biome == Biome.MOUNTAINOUS)
                {
                    ConnectIslands(20, false, riverTerrain, true, false, true);
                }
                if (this.biome == Biome.CANYON)
                {
                    ConnectIslands(15, false, riverTerrain, true, false, true);

                }

                foreach (Location location in AllLocations)
                {
                    if (location.CanShuffle)
                    {
                        location.Ypos = 0;
                        location.CanShuffle = false;
                    }
                }
                WriteMapToRom(false, MAP_ADDR, MAP_SIZE_BYTES, hiddenPalaceLocation.Ypos - 30, hiddenPalaceLocation.Xpos);
                //logger.Debug("East:" + bytesWritten);
            }
            
        }
        if (hyrule.Props.HiddenPalace)
        {
            UpdateHPspot();
        }
        if (hyrule.Props.HiddenKasuto)
        {
            UpdateKasuto();
        }
        WriteMapToRom(true, MAP_ADDR, MAP_SIZE_BYTES, hiddenPalaceLocation.Ypos - 30, hiddenPalaceLocation.Xpos);


        visitation = new bool[MAP_ROWS, MAP_COLS];
        for (int i = 0; i < MAP_ROWS; i++)
        {
            for (int j = 0; j < MAP_COLS; j++)
            {
                visitation[i, j] = false;
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
        if (this.biome != Biome.VOLCANO)
        {
            xmin = 5;
            ymin = 5;
            xmax = MAP_COLS - 6;
            ymax = MAP_COLS - 6;
        }
        int palacex = hyrule.RNG.Next(xmin, xmax);
        int palacey = hyrule.RNG.Next(ymin, ymax);
        if (this.biome == Biome.VOLCANO || this.biome == Biome.CANYON)
        {
            bool placeable = false;
            do
            {
                palacex = hyrule.RNG.Next(xmin, xmax);
                palacey = hyrule.RNG.Next(ymin, ymax);
                placeable = true;
                for (int i = palacey - 4; i < palacey + 5; i++)
                {
                    for (int j = palacex - 4; j < palacex + 5; j++)
                    {
                        if (map[i, j] != Terrain.MOUNTAIN)
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
                    map[palacey - 3 + i, palacex - 3 + j] = Terrain.MOUNTAIN;
                }
                if (i == 0)
                {
                    map[palacey - 4, palacex - 3 + j] = Terrain.MOUNTAIN;
                }
                if (i == 6)
                {
                    map[palacey + 4, palacex - 3 + j] = Terrain.MOUNTAIN;
                }
                if (j == 0)
                {
                    map[palacey - 3 + i, palacex - 4] = Terrain.MOUNTAIN;
                }
                if (j == 6)
                {
                    map[palacey - 3 + i, palacex + 4] = Terrain.MOUNTAIN;
                }
            }
        }
        map[palacey, palacex] = Terrain.PALACE;
        locationAtGP.Xpos = palacex;
        locationAtGP.Ypos = palacey + 30;
        locationAtGP.CanShuffle = false;

        int length = 20;
        if (this.biome != Biome.CANYON && this.biome != Biome.VOLCANO)
        {
            this.isHorizontal = hyrule.RNG.NextDouble() > .5;
            length = hyrule.RNG.Next(5, 16);
        }
        int deltax = 1;
        int deltay = 0;
        int starty = palacey;
        int startx = palacex + 4;
        if (this.biome != Biome.CANYON)
        {
            if (palacex > MAP_COLS / 2)
            {
                deltax = -1;
                startx = palacex - 4;
            }
            if (!isHorizontal)
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
            if (isHorizontal)
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
        this.canyonShort = hyrule.RNG.NextDouble() > .5;
        if (canyonShort)
        {
            vodcave1 = GetLocationByMem(0x8640);
            vodcave2 = GetLocationByMem(0x8641);
            vodcave3 = GetLocationByMem(0x8642);
            vodcave4 = GetLocationByMem(0x8643);
        }
        else
        {
            vodcave1 = GetLocationByMem(0x8642);
            vodcave2 = GetLocationByMem(0x8643);
            vodcave3 = GetLocationByMem(0x8640);
            vodcave4 = GetLocationByMem(0x8641);
        }

        int forced = 0;
        int vodRoutes = hyrule.RNG.Next(1, 3);
        bool horizontalPath = (isHorizontal && this.biome != Biome.CANYON) || (!isHorizontal && this.biome == Biome.CANYON);
        if (this.biome != Biome.VOLCANO)
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
            while (startx > 1 && startx < MAP_COLS - 1 && starty > 1 && starty < MAP_ROWS - 1 && (((this.biome == Biome.VOLCANO || this.biome == Biome.CANYON) && map[starty, startx] == Terrain.MOUNTAIN) || ((this.biome != Biome.VOLCANO && this.biome != Biome.CANYON) && c < length)))
            {
                c++;
                map[starty, startx] = Terrain.LAVA;
                int adjust = hyrule.RNG.Next(minadjust, maxadjust);
                while ((deltax != 0 && (starty + adjust < 1 || starty + adjust > MAP_ROWS - 2)) || (deltay != 0 && (startx + adjust < 1 || startx + adjust > MAP_COLS - 2)))
                {
                    adjust = hyrule.RNG.Next(minadjust, maxadjust);

                }
                if (adjust > 0)
                {
                    if (this.biome != Biome.VOLCANO && this.biome != Biome.CANYON)
                    {
                        if (deltax != 0)
                        {
                            map[starty - 1, startx] = Terrain.MOUNTAIN;

                        }
                        else
                        {
                            map[starty, startx - 1] = Terrain.MOUNTAIN;

                        }
                    }
                    for (int i = 0; i <= adjust; i++)
                    {
                        if (horizontalPath)
                        {
                            map[starty + i, startx] = Terrain.LAVA;
                            if (this.biome != Biome.VOLCANO && this.biome != Biome.CANYON)
                            {
                                if (map[starty + i, startx - 1] != Terrain.LAVA && map[starty + i, startx - 1] != Terrain.CAVE)
                                {
                                    map[starty + i, startx - 1] = Terrain.MOUNTAIN;
                                }
                                if (map[starty + i, startx + 1] != Terrain.LAVA && map[starty + i, startx + 1] != Terrain.CAVE)
                                {
                                    map[starty + i, startx + 1] = Terrain.MOUNTAIN;
                                }
                            }
                            Location location = GetLocationByCoords(Tuple.Create(starty + i + 30, startx));
                            if (location != null && !location.CanShuffle)
                            {
                                return false;
                            }
                        }
                        else
                        {
                            map[starty, startx + i] = Terrain.LAVA;
                            if (this.biome != Biome.VOLCANO && this.biome != Biome.CANYON)
                            {
                                if (map[starty - 1, startx + i] != Terrain.LAVA && map[starty - 1, startx + i] != Terrain.CAVE)
                                {
                                    map[starty - 1, startx + i] = Terrain.MOUNTAIN;
                                }
                                if (map[starty + 1, startx + i] != Terrain.LAVA && map[starty + 1, startx + i] != Terrain.CAVE)
                                {
                                    map[starty + 1, startx + i] = Terrain.MOUNTAIN;
                                }
                            }
                            Location location = GetLocationByCoords(Tuple.Create(starty + 30, startx + i));
                            if (location != null && !location.CanShuffle)
                            {
                                return false;
                            }
                        }
                    }
                    if (this.biome != Biome.VOLCANO && this.biome != Biome.CANYON)
                    {
                        if (deltax != 0)
                        {
                            map[starty + adjust + 1, startx] = Terrain.MOUNTAIN;

                        }
                        else
                        {
                            map[starty, startx + adjust + 1] = Terrain.MOUNTAIN;

                        }
                    }
                }
                else if (adjust < 0)
                {
                    if (this.biome != Biome.VOLCANO && this.biome != Biome.CANYON)
                    {
                        if (deltax != 0)
                        {
                            map[starty + 1, startx] = Terrain.MOUNTAIN;

                        }
                        else
                        {
                            map[starty, startx + 1] = Terrain.MOUNTAIN;
                        }
                    }
                    if (horizontalPath)
                    {
                        for (int i = 0; i <= Math.Abs(adjust); i++)
                        {
                            map[starty - i, startx] = Terrain.LAVA;
                            if (this.biome != Biome.VOLCANO && this.biome != Biome.CANYON)
                            {
                                if (map[starty - i, startx - 1] != Terrain.LAVA && map[starty - i, startx - 1] != Terrain.CAVE)
                                {
                                    map[starty - i, startx - 1] = Terrain.MOUNTAIN;
                                }
                                if (map[starty - i, startx + 1] != Terrain.LAVA && map[starty - i, startx + 1] != Terrain.CAVE)
                                {
                                    map[starty - i, startx + 1] = Terrain.MOUNTAIN;
                                }
                            }
                            Location l = GetLocationByCoords(Tuple.Create(starty - i + 30, startx));
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
                            if (this.biome != Biome.VOLCANO && this.biome != Biome.CANYON)
                            {
                                if (map[starty - 1, startx - i] != Terrain.LAVA && map[starty - 1, startx - i] != Terrain.CAVE)
                                {
                                    map[starty - 1, startx - i] = Terrain.MOUNTAIN;
                                }
                                if (map[starty + 1, startx - i] != Terrain.LAVA && map[starty + 1, startx - i] != Terrain.CAVE)
                                {
                                    map[starty + 1, startx - i] = Terrain.MOUNTAIN;
                                }
                            }
                            Location l = GetLocationByCoords(Tuple.Create(starty + 30, startx - i));
                            if (l != null && !l.CanShuffle)
                            {
                                return false;
                            }
                        }
                    }
                    if (this.biome != Biome.VOLCANO && this.biome != Biome.CANYON)
                    {
                        if (deltax != 0)
                        {
                            map[starty + adjust - 1, startx] = Terrain.MOUNTAIN;

                        }
                        else
                        {
                            map[starty, startx + adjust - 1] = Terrain.MOUNTAIN;
                        }
                    }
                }
                else
                {
                    if (this.biome != Biome.VOLCANO && this.biome != Biome.CANYON)
                    {
                        if (deltax != 0)
                        {
                            map[starty - 1, startx] = Terrain.MOUNTAIN;
                            map[starty + 1, startx] = Terrain.MOUNTAIN;
                        }
                        else
                        {
                            map[starty, startx - 1] = Terrain.MOUNTAIN;
                            map[starty, startx + 1] = Terrain.MOUNTAIN;
                        }
                    }
                    if (map[starty, startx] != Terrain.CAVE)
                    {
                        map[starty, startx] = Terrain.LAVA;
                        if (GetLocationByCoords(Tuple.Create(starty + 30 + deltay, startx + deltax)) != null)
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
                    Location f = GetLocationByMem(0x864D);
                    if (forced == 1)
                    {
                        f = GetLocationByMem(0x864E);
                    }
                    else if (forced == 2)
                    {
                        f = GetLocationByMem(0x864F);
                    }

                    if (adjust == 0)
                    {
                        if (horizontalPath)
                        {
                            if (GetLocationByCoords(Tuple.Create(starty + 30, startx - 1)) == null && GetLocationByCoords(Tuple.Create(starty + 30, startx + 1)) == null)
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
                            if (GetLocationByCoords(Tuple.Create(starty + 30 - 1, startx)) == null && GetLocationByCoords(Tuple.Create(starty + 30 + 1, startx)) == null)
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
                        if ((isHorizontal && this.biome != Biome.CANYON) || (!isHorizontal && this.biome == Biome.CANYON))
                        {
                            if (map[starty - 1, startx - 1] == Terrain.MOUNTAIN && map[starty - 1, startx + 1] == Terrain.MOUNTAIN)
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
                            if (map[starty - 1, startx - 1] == Terrain.MOUNTAIN && map[starty + 1, startx - 1] == Terrain.MOUNTAIN)
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
                            if (map[starty + 1, startx - 1] == Terrain.MOUNTAIN && map[starty + 1, startx + 1] == Terrain.MOUNTAIN)
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
                            if (map[starty - 1, startx + 1] == Terrain.MOUNTAIN && map[starty + 1, startx + 1] == Terrain.MOUNTAIN)
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
                    map[vodcave1.Ypos - 30, vodcave1.Xpos] = Terrain.MOUNTAIN;
                    map[starty, startx] = Terrain.CAVE;
                    map[starty + deltay, startx + deltax] = Terrain.MOUNTAIN;
                    if (deltax != 0)
                    {
                        map[starty + 1, startx] = Terrain.MOUNTAIN;
                        map[starty - 1, startx] = Terrain.MOUNTAIN;
                    }
                    else
                    {
                        map[starty, startx + 1] = Terrain.MOUNTAIN;
                        map[starty, startx - 1] = Terrain.MOUNTAIN;
                    }
                    vodcave1.Xpos = startx;
                    vodcave1.Ypos = starty + 30;


                    if (hyrule.RNG.NextDouble() > .5 && vodRoutes != 2 && this.biome == Biome.VOLCANO)
                    {
                        if (isHorizontal)
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
                            starty += hyrule.RNG.Next(-9, -4);
                        }
                        else
                        {
                            starty += hyrule.RNG.Next(5, 10);
                        }
                    }
                    else
                    {
                        if (startx > MAP_COLS / 2)
                        {
                            startx += hyrule.RNG.Next(-9, -4);
                        }
                        else
                        {
                            startx += hyrule.RNG.Next(5, 10);
                        }
                    }
                    if (map[starty, startx] != Terrain.MOUNTAIN && (this.biome == Biome.VOLCANO || this.biome == Biome.CANYON))
                    {
                        return false;
                    }
                    map[vodcave2.Ypos - 30, vodcave2.Xpos] = Terrain.MOUNTAIN;
                    map[starty - deltay, startx - deltax] = Terrain.MOUNTAIN;
                    map[starty, startx] = Terrain.CAVE;
                    if (deltax != 0)
                    {
                        map[starty + 1, startx] = Terrain.MOUNTAIN;
                        map[starty - 1, startx] = Terrain.MOUNTAIN;
                    }
                    else
                    {
                        map[starty, startx + 1] = Terrain.MOUNTAIN;
                        map[starty, startx - 1] = Terrain.MOUNTAIN;
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
                    if (GetLocationByCoords(Tuple.Create(starty + 30, startx + deltax)) != null)
                    {
                        map[starty, startx] = Terrain.MOUNTAIN;
                        startx -= deltax;
                    }
                    else
                    {
                        startx += deltax;
                    }
                }
                else
                {
                    if (GetLocationByCoords(Tuple.Create(starty + 30 + deltay, startx)) != null)
                    {
                        map[starty, startx] = Terrain.MOUNTAIN;
                        starty -= deltay;
                    }
                    else
                    {
                        starty += deltay;
                    }
                }

            }

            if (this.biome != Biome.VOLCANO && this.biome != Biome.CANYON)
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
        hyrule.ROMData.Put(0x1df79, (byte)(hiddenKasutoLocation.Ypos + hiddenKasutoLocation.ExternalWorld));
        hyrule.ROMData.Put(0x1dfac, (byte)(hiddenKasutoLocation.Ypos - 30));
        hyrule.ROMData.Put(0x1dfb2, (byte)(hiddenKasutoLocation.Xpos + 1));
        hyrule.ROMData.Put(0x1ccd4, (byte)(hiddenKasutoLocation.Xpos + hiddenKasutoLocation.Secondpartofcave));
        hyrule.ROMData.Put(0x1ccdb, (byte)(hiddenKasutoLocation.Ypos));
        int connection = hiddenKasutoLocation.MemAddress - baseAddr;
        hyrule.ROMData.Put(0x1df77, (byte)connection);
        hiddenKasutoLocation.NeedHammer = true;
        if (hiddenKasutoLocation == newKasuto || hiddenKasutoLocation == newKasuto2)
        {
            newKasuto.NeedHammer = true;
            newKasuto2.NeedHammer = true;
        }
        if (hyrule.Props.VanillaShuffleUsesActualTerrain || this.biome != Biome.VANILLA_SHUFFLE)
        {
            Terrain t = terrains[hiddenKasutoLocation.MemAddress];
            hyrule.ROMData.Put(0x1df75, (byte)t);
            if (t == Terrain.PALACE)
            {
                hyrule.ROMData.Put(0x1dfb6, 0x60);
                hyrule.ROMData.Put(0x1dfbb, 0x61);

                hyrule.ROMData.Put(0x1dfc0, 0x62);

                hyrule.ROMData.Put(0x1dfc5, 0x63);
            }
            else if (t == Terrain.SWAMP)
            {
                hyrule.ROMData.Put(0x1dfb6, 0x6F);
                hyrule.ROMData.Put(0x1dfbb, 0x6F);

                hyrule.ROMData.Put(0x1dfc0, 0x6F);

                hyrule.ROMData.Put(0x1dfc5, 0x6F);
            }
            else if (t == Terrain.LAVA || t == Terrain.WALKABLEWATER)
            {
                hyrule.ROMData.Put(0x1dfb6, 0x6E);
                hyrule.ROMData.Put(0x1dfbb, 0x6E);

                hyrule.ROMData.Put(0x1dfc0, 0x6E);

                hyrule.ROMData.Put(0x1dfc5, 0x6E);
            }
            else if (t == Terrain.FOREST)
            {
                hyrule.ROMData.Put(0x1dfb6, 0x68);
                hyrule.ROMData.Put(0x1dfbb, 0x69);

                hyrule.ROMData.Put(0x1dfc0, 0x6A);

                hyrule.ROMData.Put(0x1dfc5, 0x6B);
            }
            else if (t == Terrain.GRAVE)
            {
                hyrule.ROMData.Put(0x1dfb6, 0x70);
                hyrule.ROMData.Put(0x1dfbb, 0x71);

                hyrule.ROMData.Put(0x1dfc0, 0x7F);

                hyrule.ROMData.Put(0x1dfc5, 0x7F);
            }
            else if (t == Terrain.ROAD)
            {
                hyrule.ROMData.Put(0x1dfb6, 0xFE);
                hyrule.ROMData.Put(0x1dfbb, 0xFE);

                hyrule.ROMData.Put(0x1dfc0, 0xFE);

                hyrule.ROMData.Put(0x1dfc5, 0xFE);
            }
            else if (t == Terrain.BRIDGE)
            {
                hyrule.ROMData.Put(0x1dfb6, 0x5A);
                hyrule.ROMData.Put(0x1dfbb, 0x5B);

                hyrule.ROMData.Put(0x1dfc0, 0x5A);

                hyrule.ROMData.Put(0x1dfc5, 0x5B);
            }
            else if (t == Terrain.CAVE)
            {
                hyrule.ROMData.Put(0x1dfb6, 0x72);
                hyrule.ROMData.Put(0x1dfbb, 0x73);

                hyrule.ROMData.Put(0x1dfc0, 0x72);

                hyrule.ROMData.Put(0x1dfc5, 0x73);
            }
            else if (t == Terrain.DESERT)
            {
                hyrule.ROMData.Put(0x1dfb6, 0x6C);
                hyrule.ROMData.Put(0x1dfbb, 0x6C);

                hyrule.ROMData.Put(0x1dfc0, 0x6C);

                hyrule.ROMData.Put(0x1dfc5, 0x6C);
            }
            else if (t == Terrain.TOWN)
            {
                hyrule.ROMData.Put(0x1dfb6, 0x5C);
                hyrule.ROMData.Put(0x1dfbb, 0x5D);

                hyrule.ROMData.Put(0x1dfc0, 0x5E);

                hyrule.ROMData.Put(0x1dfc5, 0x5F);
            }
        }
    }

    private void DrawHiddenKasuto()
    {
        if (hyrule.Props.ShuffleHidden)
        {
            hiddenKasutoLocation = AllLocations[hyrule.RNG.Next(AllLocations.Count)];
            while (hiddenKasutoLocation == null 
                || hiddenKasutoLocation == raft 
                || hiddenKasutoLocation == bridge 
                || hiddenKasutoLocation == cave1 
                || hiddenKasutoLocation == cave2 
                || connections.ContainsKey(hiddenKasutoLocation) 
                || !hiddenKasutoLocation.CanShuffle 
                || ((this.biome != Biome.VANILLA && this.biome != Biome.VANILLA_SHUFFLE) && hiddenKasutoLocation.TerrainType == Terrain.LAVA && hiddenKasutoLocation.PassThrough !=0))
            {
                hiddenKasutoLocation = AllLocations[hyrule.RNG.Next(AllLocations.Count)];
            }
        }
        else
        {
            hiddenKasutoLocation = newKasuto;
        }
        hiddenKasutoLocation.TerrainType = Terrain.FOREST;
        hiddenKasutoLocation.NeedHammer = true;
        unimportantLocs.Remove(hiddenKasutoLocation);
        //hkLoc.CanShuffle = false;
        //map[hkLoc.Ypos - 30, hkLoc.Xpos] = terrain.forest;
    }

    private bool DrawHiddenPalace()
    {
        bool done = false;
        int xpos = hyrule.RNG.Next(6, MAP_COLS - 6);
        int ypos = hyrule.RNG.Next(6, MAP_ROWS - 6);
        if (hyrule.Props.ShuffleHidden)
        {
            hiddenPalaceLocation = AllLocations[hyrule.RNG.Next(AllLocations.Count)];
            while(hiddenPalaceLocation == null || hiddenPalaceLocation == raft || hiddenPalaceLocation == bridge || hiddenPalaceLocation == cave1 || hiddenPalaceLocation == cave2 || connections.ContainsKey(hiddenPalaceLocation) || !hiddenPalaceLocation.CanShuffle || hiddenPalaceLocation == hiddenKasutoLocation || ((this.biome != Biome.VANILLA && this.biome != Biome.VANILLA_SHUFFLE) && hiddenPalaceLocation.TerrainType == Terrain.LAVA && hiddenPalaceLocation.PassThrough != 0))
            {
                hiddenPalaceLocation = AllLocations[hyrule.RNG.Next(AllLocations.Count)];
            }
        }
        else
        {
            hiddenPalaceLocation = locationAtPalace6;
        }
        int tries = 0;
        while (!done && tries < 1000)
        {
            xpos = hyrule.RNG.Next(6, MAP_COLS - 6);
            ypos = hyrule.RNG.Next(6, MAP_ROWS - 6);
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
        Terrain t = walkableTerrains[hyrule.RNG.Next(walkableTerrains.Count())];
        while (t == Terrain.FOREST)
        {
            t = walkableTerrains[hyrule.RNG.Next(walkableTerrains.Count())];
        }
        //t = terrain.desert;
        for (int i = ypos - 3; i < ypos + 4; i++)
        {
            for (int j = xpos - 3; j < xpos + 4; j++)
            {
                if ((i == ypos - 2 && j == xpos) || (i == ypos && j == xpos - 2) || (i == ypos && j == xpos + 2))
                {
                    map[i, j] = Terrain.MOUNTAIN;
                }
                else
                {
                    map[i, j] = t;
                }
            }
        }
        //map[hpLoc.Ypos - 30, hpLoc.Xpos] = map[hpLoc.Ypos - 29, hpLoc.Xpos];
        hiddenPalaceLocation.Xpos = xpos;
        hiddenPalaceLocation.Ypos = ypos + 2 + 30;
        hiddenPalaceCallSpot.Xpos = xpos;
        hiddenPalaceCallSpot.Ypos = ypos + 30;
        hyrule.ROMData.Put(0x1df70, (byte)t);
        hiddenPalaceLocation.CanShuffle = false;
        return true;
    }

    public new void UpdateAllReached()
    {
        if (!AllReached)
        {
            base.UpdateAllReached();
            if (!hiddenPalaceLocation.Reachable || !hiddenKasutoLocation.Reachable || !newKasuto2.Reachable)
            {
                AllReached = false;
            }
        }
    }

    public void UpdateHPspot()
    {

        if (this.biome != Biome.VANILLA && this.biome != Biome.VANILLA_SHUFFLE)
        {
            hyrule.ROMData.Put(0x8382, (byte)hiddenPalaceCallSpot.Ypos);
            hyrule.ROMData.Put(0x8388, (byte)hiddenPalaceCallSpot.Xpos);
        }
        int pos = hiddenPalaceLocation.Ypos;

        hyrule.ROMData.Put(0x1df78, (byte)(pos + hiddenPalaceLocation.ExternalWorld));
        hyrule.ROMData.Put(0x1df84, 0xff);
        hyrule.ROMData.Put(0x1ccc0, (byte)pos);
        int connection = hiddenPalaceLocation.MemAddress - baseAddr;
        hyrule.ROMData.Put(0x1df76, (byte)connection);
        hiddenPalaceLocation.NeedRecorder = true;
        if (hiddenPalaceLocation == newKasuto || hiddenPalaceLocation == newKasuto2)
        {
            newKasuto.NeedRecorder = true;
            newKasuto2.NeedRecorder = true;
        }
        if (hyrule.Props.VanillaShuffleUsesActualTerrain || this.biome != Biome.VANILLA_SHUFFLE)
        {
            hyrule.ROMData.Put(0x1df74, (byte)hiddenPalaceLocation.TerrainType);
            if (hiddenPalaceLocation.TerrainType == Terrain.PALACE)
            {
                hyrule.ROMData.Put(0x1df7d, 0x60);
                hyrule.ROMData.Put(0x1df82, 0x61);

                hyrule.ROMData.Put(0x1df7e, 0x62);

                hyrule.ROMData.Put(0x1df83, 0x63);
            }
            else if (hiddenPalaceLocation.TerrainType == Terrain.SWAMP)
            {
                hyrule.ROMData.Put(0x1df7d, 0x6F);
                hyrule.ROMData.Put(0x1df82, 0x6F);

                hyrule.ROMData.Put(0x1df7e, 0x6F);

                hyrule.ROMData.Put(0x1df83, 0x6F);
            }
            else if (hiddenPalaceLocation.TerrainType == Terrain.LAVA || hiddenPalaceLocation.TerrainType == Terrain.WALKABLEWATER)
            {
                hyrule.ROMData.Put(0x1df7d, 0x6E);
                hyrule.ROMData.Put(0x1df82, 0x6E);

                hyrule.ROMData.Put(0x1df7e, 0x6E);

                hyrule.ROMData.Put(0x1df83, 0x6E);
            }
            else if (hiddenPalaceLocation.TerrainType == Terrain.FOREST)
            {
                hyrule.ROMData.Put(0x1df7d, 0x68);
                hyrule.ROMData.Put(0x1df82, 0x69);

                hyrule.ROMData.Put(0x1df7e, 0x6A);

                hyrule.ROMData.Put(0x1df83, 0x6B);
            }
            else if (hiddenPalaceLocation.TerrainType == Terrain.GRAVE)
            {
                hyrule.ROMData.Put(0x1df7d, 0x70);
                hyrule.ROMData.Put(0x1df82, 0x71);

                hyrule.ROMData.Put(0x1df7e, 0x7F);

                hyrule.ROMData.Put(0x1df83, 0x7F);
            }
            else if (hiddenPalaceLocation.TerrainType == Terrain.ROAD)
            {
                hyrule.ROMData.Put(0x1df7d, 0xFE);
                hyrule.ROMData.Put(0x1df82, 0xFE);

                hyrule.ROMData.Put(0x1df7e, 0xFE);

                hyrule.ROMData.Put(0x1df83, 0xFE);
            }
            else if (hiddenPalaceLocation.TerrainType == Terrain.BRIDGE)
            {
                hyrule.ROMData.Put(0x1df7d, 0x5A);
                hyrule.ROMData.Put(0x1df82, 0x5B);

                hyrule.ROMData.Put(0x1df7e, 0x5A);

                hyrule.ROMData.Put(0x1df83, 0x5B);
            }
            else if (hiddenPalaceLocation.TerrainType == Terrain.CAVE)
            {
                hyrule.ROMData.Put(0x1df7d, 0x72);
                hyrule.ROMData.Put(0x1df82, 0x73);

                hyrule.ROMData.Put(0x1df7e, 0x72);

                hyrule.ROMData.Put(0x1df83, 0x73);
            }
            else if (hiddenPalaceLocation.TerrainType == Terrain.DESERT)
            {
                hyrule.ROMData.Put(0x1df7d, 0x6C);
                hyrule.ROMData.Put(0x1df82, 0x6C);

                hyrule.ROMData.Put(0x1df7e, 0x6C);

                hyrule.ROMData.Put(0x1df83, 0x6C);
            }
            else if (hiddenPalaceLocation.TerrainType == Terrain.TOWN)
            {
                hyrule.ROMData.Put(0x1df7d, 0x5C);
                hyrule.ROMData.Put(0x1df82, 0x5D);

                hyrule.ROMData.Put(0x1df7e, 0x5E);

                hyrule.ROMData.Put(0x1df83, 0x5F);
            }
        }
       


        int ppu_addr1 = 0x2000 + 2 * (32 * (hiddenPalaceLocation.Ypos % 15) + (hiddenPalaceLocation.Xpos % 16)) + 2048 * (hiddenPalaceLocation.Ypos % 30 / 15);
        int ppu_addr2 = ppu_addr1 + 32;
        int ppu1low = ppu_addr1 & 0x00ff;
        int ppu1high = (ppu_addr1 >> 8) & 0xff;
        int ppu2low = ppu_addr2 & 0x00ff;
        int ppu2high = (ppu_addr2 >> 8) & 0xff;
        hyrule.ROMData.Put(0x1df7a, (byte)ppu1high);
        hyrule.ROMData.Put(0x1df7b, (byte)ppu1low);
        hyrule.ROMData.Put(0x1df7f, (byte)ppu2high);
        hyrule.ROMData.Put(0x1df80, (byte)ppu2low);

    }

    public void UpdateVisit()
    {
        UpdateReachable();

        foreach (Location location in AllLocations)
        {
            if (location.Ypos > 30 && visitation[location.Ypos - 30, location.Xpos])
            {
                if ((!location.NeedRecorder || (location.NeedRecorder && hyrule.itemGet[Item.FLUTE]) ) 
                    && (!location.NeedHammer || (location.NeedHammer && hyrule.itemGet[Item.HAMMER]) )
                    && (!location.NeedBoots || (location.NeedBoots && hyrule.itemGet[Item.BOOTS])))
                {
                    location.Reachable = true;
                    if (connections.Keys.Contains(location))
                    {
                        Location connectedLocation = connections[location];

                        if ((location.NeedBagu && (hyrule.westHyrule.bagu.Reachable || hyrule.SpellGet[Spell.FAIRY])))
                        {
                            connectedLocation.Reachable = true;
                            visitation[connectedLocation.Ypos - 30, connectedLocation.Xpos] = true;
                        }

                        if (location.NeedFairy && hyrule.SpellGet[Spell.FAIRY])
                        {
                            connectedLocation.Reachable = true;
                            visitation[connectedLocation.Ypos - 30, connectedLocation.Xpos] = true;
                        }

                        if (location.NeedJump && (hyrule.SpellGet[Spell.JUMP] || hyrule.SpellGet[Spell.FAIRY]))
                        {
                            connectedLocation.Reachable = true;
                            visitation[connectedLocation.Ypos - 30, connectedLocation.Xpos] = true;
                        }

                        if (!location.NeedFairy && !location.NeedBagu && !location.NeedJump)
                        {
                            connectedLocation.Reachable = true;
                            visitation[connectedLocation.Ypos - 30, connectedLocation.Xpos] = true;
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
        int mounty = hyrule.RNG.Next(MAP_COLS / 3 - 10, MAP_COLS / 3 + 10);
        map[mounty, 0] = Terrain.MOUNTAIN;
        bool placedSpider = false;


        int endmounty = hyrule.RNG.Next(MAP_COLS / 3 - 10, MAP_COLS / 3 + 10);
        int endmountx = hyrule.RNG.Next(2, 8);
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
                    map[y2, x2] = Terrain.MOUNTAIN;
                }
                else if (map[y2, x2] == Terrain.ROAD)
                {
                    if (!placedSpider)
                    {
                        map[y2, x2] = Terrain.SPIDER;
                        placedSpider = true;
                    }
                    else if (map[y2, x2 + 1] == Terrain.NONE && (((y2 > 0 && map[y2 - 1, x2] == Terrain.ROAD) && (y2 < MAP_ROWS - 1 && map[y2 + 1, x2] == Terrain.ROAD)) || ((x2 > 0 && map[y2, x2 - 0] == Terrain.ROAD) && (x2 < MAP_COLS - 1 && map[y2, x2 + 1] == Terrain.ROAD))))
                    {
                        if (roadEncounters == 0)
                        {
                            Location roadEnc = GetLocationByMem(0x8631);
                            roadEnc.Xpos = x2;
                            roadEnc.Ypos = y2 + 30;
                            roadEnc.CanShuffle = false;
                            roadEncounters++;
                        }
                        else if (roadEncounters == 1)
                        {
                            Location roadEnc = GetLocationByMem(0x8632);
                            roadEnc.Xpos = x2;
                            roadEnc.Ypos = y2 + 30;
                            roadEnc.CanShuffle = false;
                            roadEncounters++;
                        }
                        else if (roadEncounters == 2)
                        {
                            Location roadEnc = GetLocationByMem(0x8633);
                            roadEnc.Xpos = x2;
                            roadEnc.Ypos = y2 + 30;
                            roadEnc.CanShuffle = false;
                            roadEncounters++;
                        }
                        else if (roadEncounters == 3)
                        {
                            Location roadEnc = GetLocationByMem(0x8634);
                            roadEnc.Xpos = x2;
                            roadEnc.Ypos = y2 + 30;
                            roadEnc.CanShuffle = false;
                            roadEncounters++;
                        }
                    }
                }
            }
        }

        mounty = hyrule.RNG.Next(MAP_COLS * 2 / 3 - 10, MAP_COLS * 2 / 3 + 10);
        map[mounty, 0] = Terrain.MOUNTAIN;

        endmounty = hyrule.RNG.Next(MAP_COLS * 2 / 3 - 10, MAP_COLS * 2 / 3 + 10);
        endmountx = hyrule.RNG.Next(2, 8);
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
                    map[y2, x2] = Terrain.MOUNTAIN;
                }
                else if (map[y2, x2] == Terrain.ROAD)
                {
                    if (!placedSpider)
                    {
                        map[y2, x2] = Terrain.SPIDER;
                        placedSpider = true;
                    }
                    else if (map[y2, x2 + 1] == Terrain.NONE && (((y2 > 0 && map[y2 - 1, x2] == Terrain.ROAD) && (y2 < MAP_ROWS - 1 && map[y2 + 1, x2] == Terrain.ROAD)) || ((x2 > 0 && map[y2, x2 - 0] == Terrain.ROAD) && (x2 < MAP_COLS - 1 && map[y2, x2 + 1] == Terrain.ROAD))))
                    {
                        if (roadEncounters == 0)
                        {
                            Location roadEnc = GetLocationByMem(0x8631);
                            roadEnc.Xpos = x2;
                            roadEnc.Ypos = y2 + 30;
                            roadEnc.CanShuffle = false;
                            roadEncounters++;
                        }
                        else if (roadEncounters == 1)
                        {
                            Location roadEnc = GetLocationByMem(0x8632);
                            roadEnc.Xpos = x2;
                            roadEnc.Ypos = y2 + 30;
                            roadEnc.CanShuffle = false;
                            roadEncounters++;
                        }
                        else if (roadEncounters == 2)
                        {
                            Location roadEnc = GetLocationByMem(0x8633);
                            roadEnc.Xpos = x2;
                            roadEnc.Ypos = y2 + 30;
                            roadEnc.CanShuffle = false;
                            roadEncounters++;
                        }
                        else if (roadEncounters == 3)
                        {
                            Location roadEnc = GetLocationByMem(0x8634);
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
    protected override List<Location> GetPathingStarts()
    {
        /*return new List<Location>
        {
            palace5, palace6, waterTile, desertTile, darunia,
            newKasuto, newKasuto2, nabooru, oldKasuto, gp,
            pbagCave1, pbagCave2, hiddenPalaceCallSpot, hiddenPalaceLocation, hiddenKasutoLocation
        };*/
        return connections.Keys.Where(i => i.Reachable)
            .Union(GetContinentConnections().Where(i => i.Reachable))
            .ToList();
    }

    public override string GetName()
    {
        return "East";
    }
}
