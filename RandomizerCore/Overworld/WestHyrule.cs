using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NLog;

namespace Z2Randomizer.RandomizerCore.Overworld;

public sealed class WestHyrule : World
{
    private readonly new Logger logger = LogManager.GetCurrentClassLogger();

    public Location northPalace;
    public Location locationAtMido;
    public Location bagu;
    public Location mirrorTable;
    public Location midoChurch;
    public Location locationAtRuto;
    public Location medicineCave;
    public Location trophyCave;
    public Location locationAtPalace1;
    public Location locationAtPalace2;
    public Location locationAtPalace3;
    public Location magicContainerCave;
    public Location grassTile;
    public Location heartContainerCave;
    public Location locationAtSariaNorth;
    public Location locationAtSariaSouth;
    public Location locationAtRauru;

    private Location bridge1;
    private Location bridge2;
    public Location pbagCave;
	
    private Location jumpCave;
    private Location heartCave;
    private Location fairyCave;

    private Location parapaCave1;
    private Location parapaCave2;
    private Location jumpCave2;
    private Location fairyCave2;
    private int bridgeCount;

    private int calderaCenterX, calderaCenterY;

    private const int CALDERA_DEAD_ZONE_X = 7;
    private const int CALDERA_DEAD_ZONE_Y = 7;

    private Dictionary<Location, Location> bridgeConn;
    private Dictionary<Location, Location> cityConn;
    private Dictionary<Location, Location> caveConn;
    private Dictionary<Location, Location> graveConn;

    private static int debug = 0;

    private List<Location> lostWoods;

    private readonly SortedDictionary<int, Terrain> terrains = new SortedDictionary<int, Terrain>
        {
            { 0x462F, Terrain.PALACE},
            { 0x4630, Terrain.CAVE },
            { 0x4631, Terrain.FOREST},
            { 0x4632, Terrain.CAVE },
            { 0x4633, Terrain.FOREST },
            { 0x4634, Terrain.GRASS },
            { 0x4635, Terrain.FOREST },
            { 0x4636, Terrain.ROAD },
            { 0x4637, Terrain.SWAMP },
            { 0x4638, Terrain.GRAVE }, //09: Grave red jar
            { 0x4639, Terrain.CAVE },
            { 0x463A, Terrain.CAVE },
            { 0x463B, Terrain.CAVE },
            { 0x463C, Terrain.CAVE },
            { 0x463D, Terrain.CAVE },
            { 0x463E, Terrain.CAVE },
            { 0x463F, Terrain.CAVE },
            { 0x4640, Terrain.GRAVE }, //17: Fairy cave drop
            { 0x4641, Terrain.CAVE },
            { 0x4642, Terrain.BRIDGE },
            { 0x4643, Terrain.BRIDGE },
            { 0x4644, Terrain.BRIDGE },
            { 0x4645, Terrain.BRIDGE },
            { 0x4646, Terrain.FOREST },
            { 0x4647, Terrain.SWAMP },
            { 0x4648, Terrain.FOREST },
            { 0x4649, Terrain.FOREST },
            { 0x464A, Terrain.FOREST },
            { 0x464B, Terrain.FOREST },
            { 0x464C, Terrain.FOREST },
            { 0x464D, Terrain.ROAD },
            //{ 0x464E, terrain.desert },
            { 0x464F, Terrain.DESERT },
            //{ 0x4658, terrain.bridge },
            //{ 0x4659, terrain.cave },
            //{ 0x465A, terrain.cave },
            { 0x465B, Terrain.GRAVE }, //44: King's tomb
            { 0x465C, Terrain.TOWN },
            { 0x465E, Terrain.TOWN },
            { 0x465F, Terrain.TOWN },
            { 0x4660, Terrain.TOWN },
            { 0x4661, Terrain.FOREST },
            { 0x4662, Terrain.TOWN },
            { 0x4663, Terrain.PALACE },
            { 0x4664, Terrain.PALACE },
            { 0x4665, Terrain.PALACE }
    };

    private const int MAP_ADDR = 0x7480;

    public WestHyrule(RandomizerProperties props, Random r, ROM rom) : base(r)
    {
        isHorizontal = props.WestIsHorizontal;
        List<Location> locations =
        [
            .. rom.LoadLocations(0x4639, 4, terrains, Continent.WEST),
            .. rom.LoadLocations(0x4640, 2, terrains, Continent.WEST),
            .. rom.LoadLocations(0x462F, 10, terrains, Continent.WEST),
            .. rom.LoadLocations(0x463D, 3, terrains, Continent.WEST),
            .. rom.LoadLocations(0x4642, 12, terrains, Continent.WEST),
            .. rom.LoadLocations(0x464F, 1, terrains, Continent.WEST),
            .. rom.LoadLocations(0x465B, 2, terrains, Continent.WEST),
            .. rom.LoadLocations(0x465E, 8, terrains, Continent.WEST),
        ];
        locations.ForEach(AddLocation);

        northPalace = GetLocationByMem(0x462f); //0x462f
        jumpCave = GetLocationByMem(0x463b); //0x463b
        jumpCave.NeedJump = true;
        medicineCave = GetLocationByMem(0x463e); //0x463e
        heartCave = GetLocationByMem(0x463f); //0x463f
        fairyCave = GetLocationByMem(0x4640); //0x4640
        fairyCave.NeedFairy = true;
        bagu = GetLocationByMem(0x4661); //0x4661
        bagu.ActualTown = Town.BAGU;
        bagu.Collectable = Collectable.BAGUS_NOTE;

        locationAtRauru = GetLocationByMem(0x465C);
        locationAtRauru.Collectable = Collectable.SHIELD_SPELL;
        locationAtRuto = GetLocationByMem(0x465e); //0x465e
        locationAtRuto.Collectable = Collectable.JUMP_SPELL;
        locationAtSariaNorth = GetLocationByMem(0x4660); //0x00004660
        locationAtSariaSouth = GetLocationByMem(0x465f); //0x0000465f
        locationAtSariaNorth.NeedBagu = true;
        locationAtSariaSouth.NeedBagu = true;
        locationAtSariaNorth.Collectable = Collectable.FAIRY_SPELL;
        locationAtMido = GetLocationByMem(0x4662); //0x00004662
        locationAtMido.Collectable = Collectable.LIFE_SPELL;

        trophyCave = GetLocationByMem(0x4630); //0x00004630
        //raft = GetLocationByMem(0x4658);

        locationAtPalace1 = GetLocationByMem(0x4663);
        locationAtPalace1.PalaceNumber = 1;
        locationAtPalace2 = GetLocationByMem(0x4664);
        locationAtPalace2.PalaceNumber = 2;
        locationAtPalace3 = GetLocationByMem(0x4665);
        locationAtPalace3.PalaceNumber = 3;

        magicContainerCave = GetLocationByMem(0x4632);
        grassTile = GetLocationByMem(0x463F);
        heartContainerCave = GetLocationByMem(0x4634);
        pbagCave = GetLocationByMem(0x463D);

        parapaCave1 = GetLocationByMem(0x4639); //0x4639
        parapaCave2 = GetLocationByMem(0x463a); //0x463a
        jumpCave2 = GetLocationByMem(0x463c); //0x463c
        fairyCave2 = GetLocationByMem(0x4641); //0x4641
        bridge1 = GetLocationByMem(0x4644); //0x4644
        bridge2 = GetLocationByMem(0x4645); //0x4645

        //Fake locations that dont correspond to anywhere on the map, but still hold logic and items
        mirrorTable = new Location(locationAtSariaNorth);
        mirrorTable.Collectable = Collectable.MIRROR;
        mirrorTable.ActualTown = Town.SARIA_TABLE;
        mirrorTable.Name = "Saria Mirror Table";
        mirrorTable.CanShuffle = false;
        locationAtSariaNorth.Children.Add(mirrorTable);
        AddLocation(mirrorTable);

        midoChurch = new Location(locationAtMido);
        midoChurch.Collectable = Collectable.DOWNSTAB;
        midoChurch.ActualTown = Town.MIDO_CHURCH;
        midoChurch.Name = "Mido Church";
        midoChurch.CanShuffle = false;
        locationAtMido.Children.Add(midoChurch);
        AddLocation(midoChurch);

        if (props.SaneCaves)
        {
            fairyCave.TerrainType = Terrain.CAVE;
        }

        caveConn = [];
        bridgeConn = [];
        cityConn = [];
        graveConn = [];

        //connections.Add(hammerEnter, hammerExit);
        //connections.Add(hammerExit, hammerEnter);
        //caveConn.Add(hammerEnter, hammerExit);
        //caveConn.Add(hammerExit, hammerEnter);
        connections.Add(parapaCave1, parapaCave2);
        connections.Add(parapaCave2, parapaCave1);
        caveConn.Add(parapaCave1, parapaCave2);
        caveConn.Add(parapaCave2, parapaCave1);
        connections.Add(jumpCave, jumpCave2);
        connections.Add(jumpCave2, jumpCave);
        caveConn.Add(jumpCave, jumpCave2);
        caveConn.Add(jumpCave2, jumpCave);
        connections.Add(fairyCave, fairyCave2);
        connections.Add(fairyCave2, fairyCave);
        caveConn.Add(fairyCave2, fairyCave);
        graveConn.Add(fairyCave, fairyCave2);
        connections.Add(locationAtSariaNorth, locationAtSariaSouth);
        connections.Add(locationAtSariaSouth, locationAtSariaNorth);
        cityConn.Add(locationAtSariaSouth, locationAtSariaNorth);
        cityConn.Add(locationAtSariaNorth, locationAtSariaSouth);
        connections.Add(bridge1, bridge2);
        connections.Add(bridge2, bridge1);
        bridgeConn.Add(bridge1, bridge2);
        bridgeConn.Add(bridge2, bridge1);

        sideviewPtrTable = 0x4533;
        sideviewBank = 1;

        enemyAddr = 0x48B0;
        enemyPtr = 0x45B1;

        overworldMaps = [29, 30, 34, 35, 39, 40, 48, 53, 58];

        MAP_ROWS = 75;
        MAP_COLS = 64;
        baseAddr = 0x462F;
        VANILLA_MAP_ADDR = 0x506C;

        walkableTerrains = new List<Terrain>() { Terrain.DESERT, Terrain.GRASS, Terrain.FOREST, Terrain.SWAMP, Terrain.GRAVE };
        randomTerrainFilter = new List<Terrain> { Terrain.DESERT, Terrain.GRASS, Terrain.FOREST, Terrain.SWAMP, Terrain.GRAVE, Terrain.MOUNTAIN };
        if (props.HideLessImportantLocations)
        {
            unimportantLocs.Add(GetLocationByMem(0x4631));
            unimportantLocs.Add(GetLocationByMem(0x4633));
            unimportantLocs.Add(GetLocationByMem(0x4635));
            unimportantLocs.Add(GetLocationByMem(0x4637));
            unimportantLocs.Add(GetLocationByMem(0x4638));
            unimportantLocs.Add(GetLocationByMem(0x4646));
            unimportantLocs.Add(GetLocationByMem(0x4647));
            unimportantLocs.Add(GetLocationByMem(0x4648));
            unimportantLocs.Add(GetLocationByMem(0x4649));
            unimportantLocs.Add(GetLocationByMem(0x464A));
            unimportantLocs.Add(GetLocationByMem(0x464B));
            unimportantLocs.Add(GetLocationByMem(0x464C));
            unimportantLocs.Add(GetLocationByMem(0x464D));
            unimportantLocs.Add(GetLocationByMem(0x464F));
            if(!props.HelpfulHints)
            {
                unimportantLocs.Add(GetLocationByMem(0x465B));
            }
        }
        biome = props.WestBiome;

        //Climate filtering
        climate = props.Climate.Clone();
        climate.SeedTerrainCount = Math.Min(climate.SeedTerrainCount, biome.SeedTerrainLimit());
        climate.DisallowTerrain(props.CanWalkOnWaterWithBoots ? Terrain.WATER : Terrain.WALKABLEWATER);
        //climate.DisallowTerrain(Terrain.LAVA);

        section = new SortedDictionary<(int, int), string>{
            { (0x34, 0x17), "north" },
            { (0x20, 0x1D), "north" },
            { (0x2A, 0x25), "north" },
            { (0x3C, 0x10), "north" },
            { (0x56, 0x14), "mid" },
            { (0x40, 0x3E), "parapa" },
            { (0x4D, 0x15), "mid" },
            { (0x39, 0x3D), "parapa" },
            { (0x47, 0x08), "mid" },
            { (0x5C, 0x30), "grave" },
            { (0x29, 0x30), "parapa" },
            { (0x2E, 0x37), "north" },
            { (0x3A, 0x01), "north" },
            { (0x3E, 0x03), "mid" },
            { (0x3E, 0x26), "mid" },
            { (0x45, 0x09), "hammer0" },
            { (0x3E, 0x36), "hammer" },
            { (0x60, 0x32), "grave" },
            { (0x66, 0x3B), "island" },
            { (0x52, 0x10), "mid" },
            { (0x57, 0x1A), "mid" },
            { (0x61, 0x1A), "dmexit" },
            { (0x61, 0x22), "grave" },
            { (0x40, 0x07), "mid" },
            { (0x43, 0x11), "mid" },
            { (0x57, 0x21), "mid" },
            { (0x4C, 0x14), "mid" },
            { (0x4D, 0x11), "mid" },
            { (0x4E, 0x13), "mid" },
            { (0x4D, 0x17), "mid" },
            { (0x44, 0x25), "mid" },
            { (0x66, 0x26), "grave" },
            { (0x4D, 0x3D), "grave" },
            { (0x5F, 0x0A), "lifesouth" },
            { (0x60, 0x15), "dmexit" },
            { (0x58, 0x32), "grave" },
            { (0x36, 0x2E), "north" },
            { (0x24, 0x02), "north" },
            { (0x5B, 0x08), "lifesouth" },
            { (0x59, 0x08), "mid" },
            { (0x4C, 0x15), "mid" },
            { (0x4B, 0x3C), "grave" },
            { (0x20, 0x3E), "parapa" },
            { (0x40, 0x0B), "mid" },
            { (0x62, 0x39), "island" }
        };
        lostWoods = [
            GetLocationByMem(0x4649), 
            GetLocationByMem(0x464A), 
            GetLocationByMem(0x464B), 
            GetLocationByMem(0x464C), 
            GetLocationByMem(0x4635)
        ];
        SetVanillaCollectables(props.ReplaceFireWithDash);
    }

    protected override byte[] RandomizeEnemies(byte[] sideviewBytes, byte[] enemyBytes, bool mixLargeAndSmallEnemies, bool generatorsAlwaysMatch)
    {
        var groundEnemies = Enemies.WestGroundEnemies;
        var flyingEnemies = Enemies.WestFlyingEnemies;
        var generators = Enemies.WestGenerators;
        var smallEnemies = Enemies.WestSmallEnemies;
        var largeEnemies = Enemies.WestLargeEnemies;
        var ee = new Sidescroll.EnemiesEditable<EnemiesWest>(enemyBytes);
        RandomizeEnemiesInner(sideviewBytes, ee, mixLargeAndSmallEnemies, generatorsAlwaysMatch, RNG, groundEnemies, smallEnemies, largeEnemies, flyingEnemies, generators);
        return ee.Finalize();
    }

    public override bool Terraform(RandomizerProperties props, ROM rom)
    {
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
                areasByLocation = new SortedDictionary<string, List<Location>>
                {
                    { "north", new List<Location>() },
                    { "mid", new List<Location>() },
                    { "parapa", new List<Location>() },
                    { "grave", new List<Location>() },
                    { "lifesouth", new List<Location>() },
                    { "island", new List<Location>() },
                    { "hammer", new List<Location>() },
                    { "hammer0", new List<Location>() },
                    { "dmexit", new List<Location>() }
                };
                foreach (Location location in AllLocations)
                {
                    areasByLocation[section[location.Coords]].Add(GetLocationByCoords(location.Coords)!);
                }
                ChooseConn("parapa", connections, true);
                ChooseConn("lifesouth", connections, true);
                ChooseConn("island", connections, true);
                ChooseConn("dmexit", connections, true);

                ShuffleLocations(AllLocations);
                if (props.VanillaShuffleUsesActualTerrain)
                {
                    foreach (Location location in AllLocations)
                    {
                        map[location.Ypos - 30, location.Xpos] = location.TerrainType;
                    }
                }
                foreach(Location location in Locations[Terrain.CAVE])
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
                if(raft != null)
                {
                    raft.PassThrough = 0;
                }
                bridge1.PassThrough = 0;
                bridge2.PassThrough = 0;

                fairyCave.PassThrough = 0; //fairy cave

            }
        }
        else //Not vanilla
        {
            Terrain fillerWater = props.CanWalkOnWaterWithBoots ? Terrain.WALKABLEWATER : Terrain.WATER;

            int bytesWritten = 2000;

            if(props.BagusWoods)
            {
                bagu.CanShuffle = false;
                foreach(Location location in lostWoods)
                {
                    location.CanShuffle = false;
                    unimportantLocs.Remove(location);
                }
            }
            while (bytesWritten > MAP_SIZE_BYTES)
            {
                AllLocations.ForEach(i => i.CanShuffle = true);
                Terrain riverTerrain = Terrain.MOUNTAIN;
                locationAtSariaSouth.CanShuffle = false;
                locationAtSariaNorth.CanShuffle = false;

                map = new Terrain[MAP_ROWS, MAP_COLS];

                //blank the whole map to start
                for (int i = 0; i < MAP_ROWS; i++)
                {
                    for (int j = 0; j < MAP_COLS; j++)
                    {
                        map[i, j] = Terrain.NONE;
                    }
                }

                //Biome specifics
                switch(biome)
                {
                    case Biome.ISLANDS:
                        riverTerrain = fillerWater;

                        //Fill the edges with water
                        for (int i = 0; i < MAP_COLS; i++)
                        {
                            map[0, i] = fillerWater;
                            map[MAP_ROWS - 1, i] = fillerWater;
                        }
                        for (int i = 0; i < MAP_ROWS; i++)
                        {
                            map[i, 0] = fillerWater;
                            map[i, MAP_COLS - 1] = fillerWater;
                        }


                        //Stripe 1-3 rows/columns with water to establish rough island borders
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
                                        map[i, col] = fillerWater;
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
                                        map[row, i] = fillerWater;
                                    }
                                }
                                pickedR.Add(row);
                                rows--;
                            }
                        }
                        locationAtSariaSouth.CanShuffle = false;
                        locationAtSariaNorth.CanShuffle = false;
                        walkableTerrains = new List<Terrain>() { Terrain.DESERT, Terrain.GRASS, Terrain.FOREST, Terrain.SWAMP, Terrain.GRAVE };
                        randomTerrainFilter = new List<Terrain> { Terrain.DESERT, Terrain.GRASS, Terrain.FOREST, Terrain.SWAMP, Terrain.GRAVE, Terrain.MOUNTAIN, fillerWater };
                        break;

                    case Biome.CANYON:
                    case Biome.DRY_CANYON:
                        riverTerrain = fillerWater;
                        if (biome == Biome.DRY_CANYON)
                        {
                            riverTerrain = Terrain.DESERT;
                            bridge1.CanShuffle = false;
                            bridge1.Ypos = 0;
                            bridge2.CanShuffle = false;
                            bridge2.Ypos = 0;
                        }
                        walkableTerrains = new List<Terrain>() { Terrain.DESERT, Terrain.GRASS, Terrain.FOREST,  Terrain.GRAVE, Terrain.MOUNTAIN };
                        randomTerrainFilter = new List<Terrain> { Terrain.DESERT, Terrain.GRASS, Terrain.FOREST,  Terrain.GRAVE, Terrain.MOUNTAIN, fillerWater };

                        DrawCanyon(riverTerrain);
                        walkableTerrains.Remove(Terrain.MOUNTAIN);
                        //randomTerrainFilter.Add(Terrain.lava);
                        break;
                    case Biome.CALDERA:
                        DrawCenterMountain();
                        locationAtPalace3.CanShuffle = false;
                        walkableTerrains = new List<Terrain>() { Terrain.DESERT, Terrain.GRASS, Terrain.FOREST, Terrain.SWAMP, Terrain.GRAVE };
                        randomTerrainFilter = new List<Terrain> { Terrain.DESERT, Terrain.GRASS, Terrain.FOREST, Terrain.SWAMP, Terrain.GRAVE, Terrain.MOUNTAIN, fillerWater };
                        break;

                    case Biome.MOUNTAINOUS:
                        walkableTerrains = new List<Terrain>() { Terrain.DESERT, Terrain.GRASS, Terrain.FOREST, Terrain.SWAMP, Terrain.GRAVE };
                        randomTerrainFilter = new List<Terrain> { Terrain.DESERT, Terrain.GRASS, Terrain.FOREST, Terrain.SWAMP, Terrain.GRAVE, Terrain.MOUNTAIN, fillerWater };

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


                        cols = RNG.Next(2, 4);
                        rows = RNG.Next(2, 4);
                        pickedC = new List<int>();
                        pickedR = new List<int>();

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
                        locationAtSariaSouth.CanShuffle = false;
                        locationAtSariaNorth.CanShuffle = false;
                        break;
                    default:
                        walkableTerrains = new List<Terrain>() { Terrain.DESERT, Terrain.GRASS, Terrain.FOREST, Terrain.SWAMP, Terrain.GRAVE };
                        randomTerrainFilter = new List<Terrain> { Terrain.DESERT, Terrain.GRASS, Terrain.FOREST, Terrain.SWAMP, Terrain.GRAVE, Terrain.MOUNTAIN, fillerWater };
                        //drawRoad();
                        DrawMountains();
                        //drawBridge();
                        DrawRiver(props.CanWalkOnWaterWithBoots);
                        break;
                }

                Direction raftDirection = Direction.EAST;
                if (props.ContinentConnections != ContinentConnectionType.NORMAL && biome != Biome.CANYON && biome != Biome.DRY_CANYON)
                {
                    raftDirection = (Direction)RNG.Next(4);
                }
                else if (biome == Biome.CANYON || biome == Biome.DRY_CANYON || biome == Biome.CALDERA)
                {
                    raftDirection = isHorizontal ? DirectionExtensions.RandomHorizontal(RNG) : DirectionExtensions.RandomVertical(RNG);
                }
                if (raft != null)
                {
                    DrawOcean(raftDirection, props.CanWalkOnWaterWithBoots);
                }


                Direction bridgeDirection = (Direction)RNG.Next(4);
                do
                {
                    if (biome != Biome.CANYON && biome != Biome.DRY_CANYON && biome != Biome.CALDERA)
                    {
                        bridgeDirection = (Direction)RNG.Next(4);
                    }
                    else
                    {
                        bridgeDirection = isHorizontal ? DirectionExtensions.RandomHorizontal(RNG) : DirectionExtensions.RandomVertical(RNG);
                    }
                } while (bridgeDirection == raftDirection);
                if (bridge != null)
                {
                    DrawOcean(bridgeDirection, props.CanWalkOnWaterWithBoots);
                }

                if (biome == Biome.CALDERA)
                {
                    bool g = MakeCaldera(props.CanWalkOnWaterWithBoots, props.SaneCaves);
                    if (!g)
                    {
                        failedOnMakeCaldera++;
                        return false;
                    }
                }

                if (props.BagusWoods)
                {
                    debug++;
                    bool f = PlaceBagu();
                    if (!f)
                    {
                        failedOnBaguPlacement++;
                        return false;
                    }
                }

                PlaceRandomTerrain(climate);

                bool b = PlaceLocations(riverTerrain, props.SaneCaves);
                if (!b)
                {
                    failedOnPlaceLocations++;
                    return false;
                }

                if (!GrowTerrain(climate))
                {
                    return false;
                }

                //This is ass-backwards. We should be placing the raft (and the seed water associated with it) between
                //seed terrain and grow. Instead we're just randomly hoping there is water on the edge, and if there isn't
                //we're regenerating the whole expensive-ass operation. Making this change carries consequences across all
                //continents so it's a later thing.
                if (raft != null)
                {
                    if (!DrawRaft(false, raftDirection))
                    {
                        failedOnRaftPlacement++;
                        return false;
                    }
                }

                if (bridge != null)
                {
                    if (!DrawRaft(true, bridgeDirection))
                    {
                        failedOnBridgePlacement++;
                        return false;
                    }
                }

                if (biome == Biome.CALDERA)
                {
                    //debug++;
                    //Debug.WriteLine(debug);
                    bool f = ConnectIslands(1, true, fillerWater, false, false, false, false, props.CanWalkOnWaterWithBoots, biome,
                        calderaCenterX - CALDERA_DEAD_ZONE_X, calderaCenterX + CALDERA_DEAD_ZONE_X, 
                        calderaCenterY - CALDERA_DEAD_ZONE_Y, calderaCenterY + CALDERA_DEAD_ZONE_Y);
                    if (!f)
                    {
                        failedOnIslandConnection++;
                        return false;
                    }
                }

                BlockCaves(props.BoulderBlockConnections);
                //Debug.WriteLine(GetMapDebug());
                PlaceHiddenLocations();


                if (biome == Biome.CANYON || biome == Biome.DRY_CANYON)
                {
                    //Debug.WriteLine(GetMapDebug());
                    bool f = ConnectIslands(100, true, riverTerrain, false, false, true, false, props.CanWalkOnWaterWithBoots, biome);
                    if (!f)
                    {
                        //Debug.WriteLine(GetMapDebug());
                        failedOnConnectIslands++;
                        return false;
                    }
                }
                if (biome == Biome.ISLANDS)
                {
                    bool f = ConnectIslands(25, true, riverTerrain, false, false, true, false, props.CanWalkOnWaterWithBoots, biome);
                    if (!f)
                    {
                        failedOnConnectIslands++;
                        return false;
                    }
                }
                if (biome == Biome.MOUNTAINOUS)
                {
                    walkableTerrains.Add(Terrain.ROAD);

                    bool h = ConnectIslands(15, true, riverTerrain, false, false, false, false, props.CanWalkOnWaterWithBoots, biome);
                    if (!h)
                    {
                        failedOnConnectIslands++;
                        return false;
                    }
                }
                if (biome == Biome.VANILLALIKE)
                {
                    riverTerrain = fillerWater;
                    //2 bridges over the mountains
                    ConnectIslands(2, false, Terrain.MOUNTAIN, false, false, false, false, props.CanWalkOnWaterWithBoots, biome);
                    //4 bridges including saria and the double bridge across arbitrary water
                    bool f = ConnectIslands(4, true, riverTerrain, false, false, true, false, props.CanWalkOnWaterWithBoots, biome);
                    if (!f)
                    {
                        failedOnConnectIslands++;
                        return false;
                    }
                }

                foreach (Location location in Locations[Terrain.ROAD])
                {
                    if (location.CanShuffle)
                    {
                        location.Ypos = 0;
                        location.CanShuffle = false;
                    }
                }

                foreach (Location location in Locations[Terrain.BRIDGE])
                {
                    if (location.CanShuffle)
                    {
                        location.Ypos = 0;
                        location.CanShuffle = false;
                    }
                }

                //check bytes and adjust
                bytesWritten = WriteMapToRom(rom, false, MAP_ADDR, MAP_SIZE_BYTES, 0, 0, props.HiddenPalace, props.HiddenKasuto);
                logger.Debug("West:" + bytesWritten);
            }
        }

        if (!ValidateBasicRouting())
        {
            return false;
        }

        if (!ValidateCaves())
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

        visitation[northPalace.Ypos - 30, northPalace.Xpos] = true;
        return true;
    }

    public void SetStart()
    {
        visitation[northPalace.Ypos - 30, northPalace.Xpos] = true;
        northPalace.Reachable = true;
    }

    private bool PlaceBagu()
    {
        bagu.CanShuffle = true;
        lostWoods.ForEach(i => i.CanShuffle = true);
        int y = RNG.Next(6, MAP_ROWS - 7);
        int x = RNG.Next(6, MAP_COLS - 7);
        int tries = 0;
        while((map[y, x] != Terrain.NONE || GetLocationByCoords((y + 30, x)) != null) && tries < 1000)
        {
            y = RNG.Next(6, MAP_ROWS - 7);
            x = RNG.Next(6, MAP_COLS - 7);
        }
        if(tries >= 1000)
        {
            return false;
        }
        bagu.Ypos = y + 30;
        bagu.Xpos = x;
        bagu.CanShuffle = false;
        map[y, x] = Terrain.FOREST;

        int placed = 0;
        tries = 0;
        while(placed < 5 && tries < 3000)
        {
            int newx = RNG.Next(x - 3, x + 4);
            int newy = RNG.Next(y - 3, y + 4);
            while((map[newy, newx] != Terrain.NONE || GetLocationByCoords((newy + 30, newx)) != null) && tries < 100)
            {
                newx = RNG.Next(x - 3, x + 4);
                newy = RNG.Next(y - 3, y + 4);
                tries++;
            }
            lostWoods[placed].Ypos = newy + 30;
            lostWoods[placed].Xpos = newx;
            map[newy, newx] = Terrain.FOREST;
            lostWoods[placed].CanShuffle = false;
            placed++;
        }
        if(tries >= 3000 && placed < 3)
        {
            return false;
        }
        for(int i = placed; i < lostWoods.Count; i++)
        {
            lostWoods[placed].Ypos = 0;
        }
        return true;
    }
    private bool MakeCaldera(bool canWalkOnWaterWithBoots, bool useSaneCaves)
    {
        Terrain water = Terrain.WATER;
        if(canWalkOnWaterWithBoots)
        {
            water = Terrain.WALKABLEWATER;
        }

        bool placeable;
        do
        {
            if (isHorizontal)
            {
                calderaCenterX = RNG.Next(27, 37);
                calderaCenterY = RNG.Next(22, 52);
            }
            else
            {
                calderaCenterX = RNG.Next(21, 41);
                calderaCenterY = RNG.Next(32, 42);
            }
            placeable = true;
            for (int i = calderaCenterY - 7; i < calderaCenterY + 8; i++)
            {
                for (int j = calderaCenterX - 7; j < calderaCenterX + 8; j++)
                {
                    if (map[i, j] != Terrain.MOUNTAIN)
                    {
                        placeable = false;
                    }
                }
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
        for(int i = 0; i < 10; i++)
        {
            int lake = RNG.Next(7, 11);
            if(i == 0 || i == 9)
            {
                lake = RNG.Next(3, 6);
            }
            if (isHorizontal)
            {
                for(int j = 0; j < lake / 2; j++)
                {
                    map[starty + j, startx] = water;
                    if(i == 0)
                    {
                        map[starty + j, startx - 1] = Terrain.FOREST;
                    }
                    if(i == 9)
                    {
                        map[starty + j, startx + 1] = Terrain.FOREST;
                    }
                    
                }
                int top = starty + lake / 2;
                while(map[top, startx - 1] == Terrain.MOUNTAIN)
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
                //map[starty - (lake - (lake / 2)), startx] = terrain.forest;
                if (i == 0)
                {
                    map[starty + lake / 2, startx + 1] = Terrain.FOREST;
                    map[starty - (lake - (lake / 2)), startx - 1] = Terrain.FOREST;
                }
                if (i == 9)
                {
                    map[starty + lake / 2, startx + 1] = Terrain.FOREST;
                    map[starty - (lake - (lake / 2) ), startx + 1] = Terrain.FOREST;
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
        int caveCount = RNG.Next(2) + 1;
        Location cave1l, cave1r;
        Location? cave2l = null, cave2r = null;
        int availableConnectorCount = 2;
        if(useSaneCaves)
        {
            availableConnectorCount++;
        }
        int cavenum1 = RNG.Next(availableConnectorCount);
        if(cavenum1 == 0)
        {
            cave1l = jumpCave;//jump cave
            cave1r = jumpCave2;
        }
        else if (cavenum1 == 1)
        {
            cave1l = parapaCave1; //parapa
            cave1r = parapaCave2;
        }
        else
        {
            cave1l = fairyCave; //fairy cave
            cave1r = fairyCave2;
        }
        map[cave1l.Ypos - 30, cave1l.Xpos] = Terrain.MOUNTAIN;
        map[cave1r.Ypos - 30, cave1r.Xpos] = Terrain.MOUNTAIN;
        if (caveCount > 1)
        {
            int cavenum2 = RNG.Next(availableConnectorCount);
            while(cavenum2 == cavenum1)
            {
                cavenum2 = RNG.Next(availableConnectorCount);
            }
            if (cavenum2 == 0)
            {
                cave2l = jumpCave;//jump cave
                cave2r = jumpCave2;
            }
            else if (cavenum2 == 1)
            {
                cave2l = parapaCave1; //parapa
                cave2r = parapaCave2;
            }
            else
            {
                cave2l = fairyCave; //fairy cave
                cave2r = fairyCave2;
            }
            map[cave2l.Ypos - 30, cave2l.Xpos] = Terrain.MOUNTAIN;
            map[cave2r.Ypos - 30, cave2r.Xpos] = Terrain.MOUNTAIN;
        }
        int caveOrientation = RNG.Next(2);
        if (isHorizontal)
        {
            bool f = HorizontalCave(caveOrientation, calderaCenterX, calderaCenterY, cave1l, cave1r);
            if(!f)
            {
                return false;
            }
            cave1l.CanShuffle = false;
            cave1r.CanShuffle = false;
            Terrain caveExitTerrain = climate.GetRandomTerrain(RNG, randomTerrainFilter);
            map[cave1l.Ypos - 30, cave1l.Xpos - 1] = caveOrientation == 0 ? caveExitTerrain : Terrain.FOREST;
            map[cave1l.Ypos - 29, cave1l.Xpos - 1] = caveOrientation == 0 ? caveExitTerrain : Terrain.FOREST;
            map[cave1l.Ypos - 31, cave1l.Xpos - 1] = caveOrientation == 0 ? caveExitTerrain : Terrain.FOREST;
            map[cave1r.Ypos - 30, cave1r.Xpos + 1] = caveOrientation == 0 ? Terrain.FOREST : caveExitTerrain;
            map[cave1r.Ypos - 29, cave1r.Xpos + 1] = caveOrientation == 0 ? Terrain.FOREST : caveExitTerrain;
            map[cave1r.Ypos - 31, cave1r.Xpos + 1] = caveOrientation == 0 ? Terrain.FOREST : caveExitTerrain;

            if (caveCount > 1)
            {
                if (cave2l == null || cave2r == null)
                {
                    throw new Exception("Failed to find connection caves while constructing caldera");
                }
                if (caveOrientation == 0)
                {
                    caveOrientation = 1;
                }
                else
                {
                    caveOrientation = 0;
                }
                f = HorizontalCave(caveOrientation, calderaCenterX, calderaCenterY, cave2l, cave2r);
                if (!f)
                {
                    return false;
                }
                cave2l.CanShuffle = false;
                cave2r.CanShuffle = false;
                caveExitTerrain = climate.GetRandomTerrain(RNG, randomTerrainFilter);
                map[cave2l.Ypos - 30, cave2l.Xpos - 1] = caveOrientation == 0 ? caveExitTerrain : Terrain.FOREST;
                map[cave2l.Ypos - 29, cave2l.Xpos - 1] = caveOrientation == 0 ? caveExitTerrain : Terrain.FOREST;
                map[cave2l.Ypos - 31, cave2l.Xpos - 1] = caveOrientation == 0 ? caveExitTerrain : Terrain.FOREST;
                map[cave2r.Ypos - 30, cave2r.Xpos + 1] = caveOrientation == 0 ? Terrain.FOREST : caveExitTerrain;
                map[cave2r.Ypos - 29, cave2r.Xpos + 1] = caveOrientation == 0 ? Terrain.FOREST : caveExitTerrain;
                map[cave2r.Ypos - 31, cave2r.Xpos + 1] = caveOrientation == 0 ? Terrain.FOREST : caveExitTerrain;
            }
            
            if(caveCount == 1)
            {
                int delta = -1;
                if(caveOrientation == 0) //palace goes right
                {
                    delta = 1;
                }
                int palacex = calderaCenterX;
                int palacey = RNG.Next(calderaCenterY - 2, calderaCenterY + 3);
                while (map[palacey, palacex] != Terrain.MOUNTAIN)
                {
                    palacex += delta;
                }
                map[palacey, palacex] = Terrain.PALACE;
                locationAtPalace3.Ypos = palacey + 30;
                locationAtPalace3.Xpos = palacex;
                map[palacey, palacex + delta] = Terrain.MOUNTAIN;

            }
            else
            {
                int palaceType = RNG.Next(2);
                int delta = -1;
                if(palaceType == 0)
                {
                    delta = 1;
                }
                int palacex = RNG.Next(calderaCenterX - 2, calderaCenterX + 3);
                int palacey = calderaCenterY;
                while (map[palacey, palacex] != Terrain.MOUNTAIN)
                {
                    palacey += delta;
                }
                map[palacey, palacex] = Terrain.PALACE;
                locationAtPalace3.Ypos = palacey + 30;
                locationAtPalace3.Xpos = palacex;
                map[palacey + delta, palacex] = Terrain.MOUNTAIN;

            }

        }
        else //Vertical
        {
            bool f = VerticalCave(caveOrientation, calderaCenterX, calderaCenterY, cave1l, cave1r);
            if (!f)
            {
                return false;
            }
            cave1l.CanShuffle = false;
            cave1r.CanShuffle = false;
            Terrain caveExitTerrain = climate.GetRandomTerrain(RNG, randomTerrainFilter);

            map[cave1l.Ypos - 31, cave1l.Xpos] = caveOrientation == 0 ? caveExitTerrain : Terrain.FOREST;
            map[cave1l.Ypos - 31, cave1l.Xpos + 1] = caveOrientation == 0 ? caveExitTerrain : Terrain.FOREST;
            map[cave1l.Ypos - 31, cave1l.Xpos - 1] = caveOrientation == 0 ? caveExitTerrain : Terrain.FOREST;
            map[cave1r.Ypos - 29, cave1r.Xpos] = caveOrientation == 0 ? Terrain.FOREST : caveExitTerrain;
            map[cave1r.Ypos - 29, cave1r.Xpos + 1] = caveOrientation == 0 ? Terrain.FOREST : caveExitTerrain;
            map[cave1r.Ypos - 29, cave1r.Xpos - 1] = caveOrientation == 0 ? Terrain.FOREST : caveExitTerrain;

            if (caveCount > 1)
            {
                if (cave2l == null || cave2r == null)
                {
                    throw new Exception("Failed to find connection caves while constructing caldera");
                }
                if (caveOrientation == 0)
                {
                    caveOrientation = 1;
                }
                else
                {
                    caveOrientation = 0;
                }
                f = VerticalCave(caveOrientation, calderaCenterX, calderaCenterY, cave2l, cave2r);
                if (!f)
                {
                    return false;
                }
                cave2l.CanShuffle = false;
                cave2r.CanShuffle = false;
                caveExitTerrain = climate.GetRandomTerrain(RNG, randomTerrainFilter);
                map[cave2l.Ypos - 31, cave2l.Xpos] = caveOrientation == 0 ? caveExitTerrain : Terrain.FOREST;
                map[cave2l.Ypos - 31, cave2l.Xpos + 1] = caveOrientation == 0 ? caveExitTerrain : Terrain.FOREST;
                map[cave2l.Ypos - 31, cave2l.Xpos - 1] = caveOrientation == 0 ? caveExitTerrain : Terrain.FOREST;
                map[cave2r.Ypos - 29, cave2r.Xpos] = caveOrientation == 0 ? Terrain.FOREST : caveExitTerrain;
                map[cave2r.Ypos - 29, cave2r.Xpos + 1] = caveOrientation == 0 ? Terrain.FOREST : caveExitTerrain;
                map[cave2r.Ypos - 29, cave2r.Xpos - 1] = caveOrientation == 0 ? Terrain.FOREST : caveExitTerrain;
            }

            if (caveCount == 1)
            {
                int delta = -1;
                if (caveOrientation == 0) //palace goes down
                {
                    delta = 1;
                }
                int palacex = RNG.Next(calderaCenterX - 2, calderaCenterX + 3);
                int palacey = calderaCenterY;
                while (map[palacey, palacex] != Terrain.MOUNTAIN)
                {
                    palacey += delta;
                }
                map[palacey, palacex] = Terrain.PALACE;
                locationAtPalace3.Ypos = palacey + 30;
                locationAtPalace3.Xpos = palacex;
                map[palacey + delta, palacex] = Terrain.MOUNTAIN;


            }
            else
            {
                int palaceType = RNG.Next(2);
                int delta = -1;
                if (palaceType == 0)
                {
                    delta = 1;
                }
                int palacex = calderaCenterX;
                int palacey = RNG.Next(calderaCenterY - 2, calderaCenterY + 3);
                while (map[palacey, palacex] != Terrain.MOUNTAIN)
                {
                    palacex += delta;
                }
                map[palacey, palacex] = Terrain.PALACE;
                locationAtPalace3.Ypos = palacey + 30;
                locationAtPalace3.Xpos = palacex;
                map[palacey, palacex + delta] = Terrain.MOUNTAIN;

            }
        }
        return true;
    }
    private void BlockCaves(bool boulderBlockConnections)
    {
        int rockNum = RNG.Next(3);
        int cavePicked = 0;
        while (rockNum > 0)
        {
            List<Location> Caves = Locations[Terrain.CAVE];
            Location cave = Caves[RNG.Next(Caves.Count)];
            int caveConn = 0;
            if(caveConn != 0 && connections.ContainsKey(GetLocationByMem(cavePicked)))
            {
                caveConn = connections[GetLocationByMem(cavePicked)].MemAddress;
            }
            //Item caves
            if (boulderBlockConnections && cave.MemAddress != cavePicked && cave.MemAddress != caveConn)
            {
                if (map[cave.Ypos - 30, cave.Xpos - 1] != Terrain.MOUNTAIN && cave.Xpos + 2 < MAP_COLS && GetLocationByCoords((cave.Ypos - 30, cave.Xpos + 2)) == null)
                {
                    map[cave.Ypos - 30, cave.Xpos - 1] = Terrain.ROCK;
                    map[cave.Ypos - 30, cave.Xpos] = Terrain.ROAD;
                    map[cave.Ypos - 30, cave.Xpos + 1] = Terrain.CAVE;
                    if (cave.Xpos + 2 < MAP_COLS)
                    {
                        map[cave.Ypos - 30, cave.Xpos + 2] = Terrain.MOUNTAIN;
                    }
                    cave.Xpos++;
                    rockNum--;
                    cavePicked = cave.MemAddress;
                }
                else if (map[cave.Ypos - 30, cave.Xpos + 1] != Terrain.MOUNTAIN && cave.Xpos - 2 > 0 && GetLocationByCoords((cave.Ypos - 30, cave.Xpos - 2)) == null)
                {
                    map[cave.Ypos - 30, cave.Xpos + 1] = Terrain.ROCK;
                    map[cave.Ypos - 30, cave.Xpos] = Terrain.ROAD;
                    map[cave.Ypos - 30, cave.Xpos - 1] = Terrain.CAVE;
                    if (cave.Xpos - 2 >= 0)
                    {
                        map[cave.Ypos - 30, cave.Xpos - 2] = Terrain.MOUNTAIN;
                    }
                    cave.Xpos--;
                    rockNum--;
                    cavePicked = cave.MemAddress;
                }
                else if (map[cave.Ypos - 29, cave.Xpos] != Terrain.MOUNTAIN && cave.Ypos - 32 < MAP_COLS && GetLocationByCoords((cave.Ypos - 32, cave.Xpos)) == null)
                {
                    map[cave.Ypos - 29, cave.Xpos] = Terrain.ROCK;
                    map[cave.Ypos - 30, cave.Xpos] = Terrain.ROAD;
                    map[cave.Ypos - 31, cave.Xpos] = Terrain.CAVE;
                    if (cave.Ypos - 32 >= 0)
                    {
                        map[cave.Ypos - 32, cave.Xpos] = Terrain.MOUNTAIN;
                    }
                    cave.Ypos--;
                    rockNum--;
                    cavePicked = cave.MemAddress;
                }
                else if (map[cave.Ypos - 31, cave.Xpos] != Terrain.MOUNTAIN && cave.Ypos - 28 < MAP_COLS && GetLocationByCoords((cave.Ypos - 28, cave.Xpos)) == null)
                {
                    map[cave.Ypos - 31, cave.Xpos] = Terrain.ROCK;
                    map[cave.Ypos - 30, cave.Xpos] = Terrain.ROAD;
                    map[cave.Ypos - 29, cave.Xpos] = Terrain.CAVE;
                    if (cave.Ypos - 28 < MAP_ROWS)
                    {
                        map[cave.Ypos - 28, cave.Xpos] = Terrain.MOUNTAIN;
                    }
                    cave.Ypos++;
                    rockNum--;
                    cavePicked = cave.MemAddress;
                }
            }
            //Connector caves
            else if (!connections.Keys.Contains(cave) && cave != cave1 && cave != cave2 && cave.MemAddress != cavePicked)
            {
                if (map[cave.Ypos - 30, cave.Xpos - 1] != Terrain.MOUNTAIN)
                {
                    map[cave.Ypos - 30, cave.Xpos - 1] = Terrain.ROCK;
                    rockNum--;
                    cavePicked = cave.MemAddress;
                }
                else if (map[cave.Ypos - 30, cave.Xpos + 1] != Terrain.MOUNTAIN)
                {
                    map[cave.Ypos - 30, cave.Xpos + 1] = Terrain.ROCK;
                    rockNum--;
                    cavePicked = cave.MemAddress;
                }
                else if (map[cave.Ypos - 29, cave.Xpos] != Terrain.MOUNTAIN)
                {
                    map[cave.Ypos - 29, cave.Xpos] = Terrain.ROCK;
                    rockNum--;
                    cavePicked = cave.MemAddress;
                }
                else if (map[cave.Ypos - 31, cave.Xpos] != Terrain.MOUNTAIN)
                {
                    map[cave.Ypos - 31, cave.Xpos] = Terrain.ROCK;
                    rockNum--;
                    cavePicked = cave.MemAddress;
                }
            }
        }
    }

    private void DrawMountains()
    {
        //create some mountains
        int mounty = RNG.Next(22, 42);
        map[mounty, 0] = Terrain.MOUNTAIN;
        bool placedRoad = false;


        int endmounty = RNG.Next(22, 42);
        int endmountx = RNG.Next(2, 8);
        int x2 = 0;
        int y2 = mounty;
        int placedRocks = 0;
        while (x2 != (MAP_COLS - endmountx) || y2 != endmounty)
        {
            if (Math.Abs(x2 - (MAP_COLS - endmountx)) >= Math.Abs(y2 - endmounty))
            {
                if (x2 > MAP_COLS - endmountx && x2 > 0)
                {
                    x2--;
                }
                else if (x2 < MAP_COLS - 1)
                {
                    x2++;
                }
            }
            else
            {
                if (y2 > endmounty && y2 > 0)
                {
                    y2--;
                }
                else if (y2 < MAP_ROWS - 1)
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
                else
                {
                    if (!placedRoad && map[y2, x2 + 1] != Terrain.ROAD)
                    {
                        if (RNG.NextDouble() > .5 && (x2 > 0 && map[y2, x2 - 1] != Terrain.ROCK) && (x2 < MAP_COLS - 1 && map[y2, x2 + 1] != Terrain.ROCK) && (((y2 > 0 && map[y2 - 1, x2] == Terrain.ROAD) && (y2 < MAP_ROWS - 1 && map[y2 + 1, x2] == Terrain.ROAD)) || ((x2 > 0 && map[y2, x2 - 0] == Terrain.ROAD) && (x2 < MAP_COLS - 1 && map[y2, x2 + 1] == Terrain.ROAD))))
                        {
                            Location roadEnc = GetLocationByMem(0x4636);
                            roadEnc.Xpos = x2;
                            roadEnc.Ypos = y2 + 30;
                            roadEnc.CanShuffle = false;
                            roadEnc.Reachable = true;
                            placedRoad = true;
                        }
                        else if (placedRocks < 1)
                        {
                            Location roadEnc = GetLocationByMem(0x4636);
                            if ((roadEnc.Ypos - 30 != y2 && roadEnc.Xpos - 1 != x2) && (roadEnc.Ypos - 30 + 1 != y2 && roadEnc.Xpos != x2) && (roadEnc.Ypos - 30 - 1 != y2 && roadEnc.Xpos != x2) && (roadEnc.Ypos - 30 != y2 && roadEnc.Xpos + 1 != x2))
                            {
                                map[y2, x2] = Terrain.ROCK;
                                placedRocks++;
                            }
                        }
                    }
                    else if (placedRocks < 1)
                    {

                        map[y2, x2] = Terrain.ROCK;
                        placedRocks++;
                    }
                }
            }
        }

        if (!placedRoad)
        {
            Location roadEnc = GetLocationByMem(0x4636);
            roadEnc.Xpos = 0;
            roadEnc.Ypos = 0;
            roadEnc.CanShuffle = false;
        }
    }


    public override void UpdateVisit(Dictionary<Collectable, bool> itemGet)
    {
        visitation[northPalace.Ypos - 30, northPalace.Xpos] = true;
        UpdateReachable(itemGet);

        foreach (Location location in AllLocations)
        {
            if (location.Ypos > 30)
            {
                if (visitation[location.Ypos - 30, location.Xpos])
                {
                    location.Reachable = true;
                    if (connections.Keys.Contains(location))
                    {
                        Location l2 = connections[location];
                        if (
                            location.NeedBagu 
                            && (itemGet[Collectable.BAGUS_NOTE]
                                || itemGet[Collectable.FAIRY_SPELL]
                                || (itemGet.ContainsKey(Collectable.DASH_SPELL) && itemGet[Collectable.DASH_SPELL] && itemGet[Collectable.JUMP_SPELL])))
                        {
                            l2.Reachable = true;
                            visitation[l2.Ypos - 30, l2.Xpos] = true;
                        }

                        if (location.NeedFairy && itemGet[Collectable.FAIRY_SPELL])
                        {
                            l2.Reachable = true;
                            visitation[l2.Ypos - 30, l2.Xpos] = true;
                        }

                        if (location.NeedJump && (itemGet[Collectable.JUMP_SPELL] || itemGet[Collectable.FAIRY_SPELL]))
                        {
                            l2.Reachable = true;
                            visitation[l2.Ypos - 30, l2.Xpos] = true;
                        }

                        if (!location.NeedFairy && !location.NeedBagu && !location.NeedJump)
                        {
                            l2.Reachable = true;
                            visitation[l2.Ypos - 30, l2.Xpos] = true;
                        }
                    }
                }
            }
        }
        Location actualSariaNorth = AllLocations.First(i => i.ActualTown == Town.SARIA_NORTH);
        Location actualSariaSouth = AllLocations.First(i => i.ActualTown == Town.SARIA_SOUTH);
        if(actualSariaNorth.Reachable && 
            (itemGet[Collectable.FAIRY_SPELL] ||
            itemGet[Collectable.BAGUS_NOTE] ||
            (itemGet.ContainsKey(Collectable.DASH_SPELL) && itemGet[Collectable.DASH_SPELL] && itemGet[Collectable.JUMP_SPELL]))
        )    
        {
            actualSariaSouth.Reachable = true;
        }
        if (actualSariaSouth.Reachable &&
            (itemGet[Collectable.FAIRY_SPELL] ||
            itemGet[Collectable.BAGUS_NOTE] ||
            (itemGet.ContainsKey(Collectable.DASH_SPELL) && itemGet[Collectable.DASH_SPELL] && itemGet[Collectable.JUMP_SPELL]))
)
        {
            actualSariaNorth.Reachable = true;
        }
    }

    protected override List<Location> GetPathingStarts()
    {
        return connections.Keys.Where(i => i.Reachable)
            .Union(new List<Location>() { northPalace })
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
        foreach (Location location in GetContinentConnections())
        {
            pendingCoordinates.Add((location.Ypos - 30, location.Xpos));
        }
        pendingCoordinates.Add((northPalace.Ypos - 30, northPalace.Xpos));
        int y, x;
        do
        {
            (int, int) coordinate = pendingCoordinates.First();
            y = coordinate.Item1;
            x = coordinate.Item2;
            pendingCoordinates.Remove(coordinate);
            if (visitedCoordinates[y, x])
            {
                continue;
            }
            visitedCoordinates[y, x] = true;
            //if there is a location at this coordinate
            Location? here = unreachedLocations.FirstOrDefault(location => location.Ypos - 30 == y && location.Xpos == x);
            if (here != null)
            {
                //it's reachable
                unreachedLocations.Remove(here);
                //if it's a connection cave, add the exit(s) to the pending locations
                if (connections.ContainsKey(here))
                {
                    pendingCoordinates.Add((connections[here].Ypos - 30, connections[here].Xpos));
                }
            }

            //for each adjacent direction, if it's not off the map, and it's potentially walkable terrain, crawl it
            if (x > 0 && map[y, x - 1].IsWalkable())
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
        return "West";
    }

    public override IEnumerable<Location> RequiredLocations(bool hiddenPalace, bool hiddenKasuto)
    {
        HashSet<Location> requiredLocations = new()
        {
            northPalace,
            locationAtRauru,
            locationAtMido,
            locationAtRuto,
            locationAtSariaNorth,
            locationAtSariaSouth,
            bagu,

            medicineCave,
            trophyCave,
            heartContainerCave,
            pbagCave,
            magicContainerCave,
            grassTile,

            locationAtPalace1,
            locationAtPalace2,
            locationAtPalace3,
        };

        foreach (Location location in connections.Keys)
        {
            if (!requiredLocations.Contains(location) && location.Ypos != 0)
            {
                requiredLocations.Add(location);
            }
        }
        return requiredLocations.Where(i => i != null);
    }

    protected override void SetVanillaCollectables(bool useFire)
    {
        locationAtPalace1.VanillaCollectable = Collectable.CANDLE;
        locationAtPalace2.VanillaCollectable = Collectable.GLOVE;
        locationAtPalace3.VanillaCollectable = Collectable.RAFT;

        locationAtRauru.VanillaCollectable = Collectable.SHIELD_SPELL;
        locationAtRuto.VanillaCollectable = Collectable.JUMP_SPELL;
        locationAtSariaNorth.VanillaCollectable = Collectable.LIFE_SPELL;
        locationAtMido.VanillaCollectable = Collectable.FAIRY_SPELL;
        midoChurch.VanillaCollectable = Collectable.DOWNSTAB;
        bagu.VanillaCollectable = Collectable.BAGUS_NOTE;
        mirrorTable.VanillaCollectable = Collectable.MIRROR;

        grassTile.VanillaCollectable = Collectable.HEART_CONTAINER;
        pbagCave.VanillaCollectable = Collectable.LARGE_BAG;
        heartContainerCave.VanillaCollectable = Collectable.HEART_CONTAINER;
        magicContainerCave.VanillaCollectable = Collectable.MAGIC_CONTAINER;
        trophyCave.VanillaCollectable = Collectable.TROPHY;
        medicineCave.VanillaCollectable = Collectable.MEDICINE;
    }

    public override string GenerateSpoiler()
    {
        StringBuilder sb = new();
        sb.AppendLine("WEST: ");
        sb.AppendLine("\tRauru: " + AllLocations.First(i => i.ActualTown == Town.RAURU).Collectable.EnglishText());
        sb.AppendLine("\tRuto: " + AllLocations.First(i => i.ActualTown == Town.RUTO).Collectable.EnglishText());
        sb.AppendLine("\tMirror Table: " + mirrorTable.Collectable.EnglishText());
        sb.AppendLine("\tSaria: " + AllLocations.First(i => i.ActualTown == Town.SARIA_NORTH).Collectable.EnglishText());
        sb.AppendLine("\tDownstab Guy: " + midoChurch.Collectable.EnglishText());
        sb.AppendLine("\tMido: " + AllLocations.First(i => i.ActualTown == Town.MIDO_WEST).Collectable.EnglishText());
        sb.AppendLine("\tBagu: " + bagu.Collectable.EnglishText());

        sb.AppendLine("\tMagic Container Cave: " + magicContainerCave.Collectable.EnglishText());
        sb.AppendLine("\tTrophy Cave: " + trophyCave.Collectable.EnglishText());
        sb.AppendLine("\tGrass Tile: " + grassTile.Collectable.EnglishText());
        sb.AppendLine("\tHeart Container Cave: " + heartContainerCave.Collectable.EnglishText());
        sb.AppendLine("\tPillar Pbag Cave: " + pbagCave.Collectable.EnglishText());
        sb.AppendLine("\tMedicine Cave: " + medicineCave.Collectable.EnglishText());

        sb.AppendLine("\tPalace 1 (" + locationAtPalace1.PalaceNumber + "): " + locationAtPalace1.Collectable.EnglishText());
        sb.AppendLine("\tPalace 2 (" + locationAtPalace2.PalaceNumber + "): " + locationAtPalace2.Collectable.EnglishText());
        sb.AppendLine("\tPalace 3 (" + locationAtPalace3.PalaceNumber + "): " + locationAtPalace3.Collectable.EnglishText());
        sb.AppendLine();
        return sb.ToString();
    }
}
