using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using NLog;
using Z2Randomizer.RandomizerCore.Enemy;

namespace Z2Randomizer.RandomizerCore.Overworld;

//6A31 - address in memory of kasuto y coord;
//6A35 - address in memory of palace 6 y coord
public sealed class EastHyrule : World
{
    //private int bridgeCount;

    private static readonly new Logger logger = LogManager.GetCurrentClassLogger();

    private readonly SortedDictionary<int, Terrain> terrains = new SortedDictionary<int, Terrain>
    {
        { RomMap.EAST_MINOR_FOREST_TILE_BY_NABOORU_LOCATION, Terrain.FOREST },
        { RomMap.EAST_MINOR_FOREST_TILE_BY_P6_LOCATION, Terrain.FOREST },
        { RomMap.EAST_TRAP_ROAD_TILE_LOCATION1, Terrain.ROAD },
        { RomMap.EAST_TRAP_ROAD_TILE_LOCATION2, Terrain.ROAD },
        { RomMap.EAST_TRAP_ROAD_TILE_LOCATION3, Terrain.ROAD },
        { RomMap.EAST_TRAP_ROAD_TILE_TO_VOD_LOCATION, Terrain.ROAD },
        { RomMap.EAST_BRIDGE_TILE_TO_P6_LOCATION, Terrain.BRIDGE },
        { RomMap.EAST_BRIDGE_TILE_TO_KASUTO_LOCATION, Terrain.BRIDGE },
        { RomMap.EAST_TRAP_DESERT_TILE_LOCATION1, Terrain.DESERT },
        { RomMap.EAST_TRAP_DESERT_TILE_LOCATION2, Terrain.DESERT },
        { RomMap.EAST_WATER_TILE_LOCATION, Terrain.WALKABLEWATER },
        { RomMap.EAST_CAVE_NABOORU_PASSTHROUGH_SOUTH_LOCATION, Terrain.CAVE },
        { RomMap.EAST_CAVE_NABOORU_PASSTHROUGH_NORTH_LOCATION, Terrain.CAVE },
        { RomMap.EAST_CAVE_PBAG1_LOCATION, Terrain.CAVE },
        { RomMap.EAST_CAVE_PBAG2_LOCATION, Terrain.CAVE },
        { RomMap.EAST_CAVE_NEW_KASUTO_PASSTHROUGH_WEST_LOCATION, Terrain.CAVE },
        { RomMap.EAST_CAVE_NEW_KASUTO_PASSTHROUGH_EAST_LOCATION, Terrain.CAVE },
        { RomMap.EAST_CAVE_VOD_PASSTHROUGH2_START_LOCATION, Terrain.CAVE },
        { RomMap.EAST_CAVE_VOD_PASSTHROUGH2_END_LOCATION, Terrain.CAVE },
        { RomMap.EAST_CAVE_VOD_PASSTHROUGH1_END_LOCATION, Terrain.CAVE },
        { RomMap.EAST_CAVE_VOD_PASSTHROUGH1_START_LOCATION, Terrain.CAVE },
        { RomMap.EAST_MINOR_SWAMP_TILE_LOCATION, Terrain.SWAMP },
        { RomMap.EAST_BUGGED_MINOR_LAVA_TILE_LOCATION, Terrain.LAVA },
        { RomMap.EAST_MINOR_DESERT_TILE_LOCATION1, Terrain.DESERT },
        { RomMap.EAST_MINOR_DESERT_TILE_LOCATION2, Terrain.DESERT },
        { RomMap.EAST_MINOR_DESERT_TILE_LOCATION3, Terrain.DESERT },
        { RomMap.EAST_DESERT_TILE_LOCATION, Terrain.DESERT },
        { RomMap.EAST_MINOR_FOREST_TILE_LOCATION2, Terrain.FOREST },
        { RomMap.EAST_MINOR_LAVA_TILE_LOCATION1, Terrain.LAVA },
        { RomMap.EAST_MINOR_LAVA_TILE_LOCATION2, Terrain.LAVA },
        { RomMap.EAST_TRAP_LAVA_TILE_LOCATION1, Terrain.LAVA },
        { RomMap.EAST_TRAP_LAVA_TILE_LOCATION2, Terrain.LAVA },
        { RomMap.EAST_TRAP_LAVA_TILE_LOCATION3, Terrain.LAVA },
        { RomMap.EAST_BRIDGE_TILE_TO_MI_LOCATION, Terrain.BRIDGE },
        { RomMap.EAST_RAFT_TILE_TO_WEST_LOCATION, Terrain.BRIDGE },
        { RomMap.EAST_TOWN_OF_NABOORU_TILE_LOCATION, Terrain.TOWN },
        { RomMap.EAST_TOWN_OF_DARUNIA_TILE_LOCATION, Terrain.TOWN },
        { RomMap.EAST_TOWN_OF_NEW_KASUTO_TILE_LOCATION, Terrain.TOWN },
        { RomMap.EAST_TOWN_OF_OLD_KASUTO_TILE_LOCATION, Terrain.TOWN },
        { RomMap.EAST_PALACE5_TILE_LOCATION, Terrain.PALACE },
        { RomMap.EAST_PALACE6_TILE_LOCATION, Terrain.PALACE },
        { RomMap.EAST_GREAT_PALACE_TILE_LOCATION, Terrain.PALACE },
    };

    public Location locationAtPalace5;
    public Location locationAtPalace6;
    public Location fountain;
    public Location waterTile;
    public Location desertTile;
    public Location townAtDarunia;
    public Location daruniaRoof;
    public Location townAtNewKasuto;
    public Location spellTower;
    public Location newKasutoBasement;
    public Location townAtNabooru;
    public Location townAtOldKasuto;
    public Location locationAtGP;
    public Location pbagCave1;
    public Location pbagCave2;
    private bool canyonShort;
    //I'm not sure precisely what these are supposed to track, but they're never initialized so apparently nothing
    //private Location vodcave1;
    //private Location vodcave2;
    public Location hiddenPalaceLocation;
    public Location hiddenKasutoLocation;
    public (int, int) hiddenPalaceCoords;

    private const int MAP_ADDR = 0xb480;


    public EastHyrule(RandomizerProperties props, Random r, ROM rom) : base(r)
    {
        logger.Trace("Initializing EastHyrule");
        isHorizontal = props.EastIsHorizontal;
        baseAddr = RomMap.EAST_MINOR_FOREST_TILE_BY_NABOORU_LOCATION;
        List<Location> locations =
        [
            .. rom.LoadLocations(RomMap.EAST_CAVE_NEW_KASUTO_PASSTHROUGH_WEST_LOCATION, 6, terrains, Continent.EAST),
            .. rom.LoadLocations(RomMap.EAST_CAVE_NABOORU_PASSTHROUGH_SOUTH_LOCATION, 2, terrains, Continent.EAST),
            .. rom.LoadLocations(RomMap.EAST_MINOR_FOREST_TILE_BY_NABOORU_LOCATION, 11, terrains, Continent.EAST),
            .. rom.LoadLocations(RomMap.EAST_MINOR_SWAMP_TILE_LOCATION, 1, terrains, Continent.EAST),
            .. rom.LoadLocations(RomMap.EAST_CAVE_PBAG1_LOCATION, 2, terrains, Continent.EAST),
            .. rom.LoadLocations(RomMap.EAST_MINOR_DESERT_TILE_LOCATION1, 10, terrains, Continent.EAST),
            .. rom.LoadLocations(RomMap.EAST_TOWN_OF_NABOORU_TILE_LOCATION, 1, terrains, Continent.EAST),
            .. rom.LoadLocations(RomMap.EAST_TOWN_OF_DARUNIA_TILE_LOCATION, 1, terrains, Continent.EAST),
            .. rom.LoadLocations(RomMap.EAST_TOWN_OF_NEW_KASUTO_TILE_LOCATION, 1, terrains, Continent.EAST),
            .. rom.LoadLocations(RomMap.EAST_TOWN_OF_OLD_KASUTO_TILE_LOCATION, 4, terrains, Continent.EAST),
        ];
        locations.ForEach(AddLocation);

        //reachableAreas = new HashSet<string>();

        connections.Add(GetLocationByMem(RomMap.EAST_CAVE_NABOORU_PASSTHROUGH_SOUTH_LOCATION), GetLocationByMem(RomMap.EAST_CAVE_NABOORU_PASSTHROUGH_NORTH_LOCATION));
        connections.Add(GetLocationByMem(RomMap.EAST_CAVE_NABOORU_PASSTHROUGH_NORTH_LOCATION), GetLocationByMem(RomMap.EAST_CAVE_NABOORU_PASSTHROUGH_SOUTH_LOCATION));

        connections.Add(GetLocationByMem(RomMap.EAST_CAVE_NEW_KASUTO_PASSTHROUGH_WEST_LOCATION), GetLocationByMem(RomMap.EAST_CAVE_NEW_KASUTO_PASSTHROUGH_EAST_LOCATION));
        connections.Add(GetLocationByMem(RomMap.EAST_CAVE_NEW_KASUTO_PASSTHROUGH_EAST_LOCATION), GetLocationByMem(RomMap.EAST_CAVE_NEW_KASUTO_PASSTHROUGH_WEST_LOCATION));

        //valley of death 19-20
        connections.Add(GetLocationByMem(RomMap.EAST_CAVE_VOD_PASSTHROUGH2_START_LOCATION), GetLocationByMem(RomMap.EAST_CAVE_VOD_PASSTHROUGH2_END_LOCATION));
        connections.Add(GetLocationByMem(RomMap.EAST_CAVE_VOD_PASSTHROUGH2_END_LOCATION), GetLocationByMem(RomMap.EAST_CAVE_VOD_PASSTHROUGH2_START_LOCATION));

        //valley of death 17-18
        connections.Add(GetLocationByMem(RomMap.EAST_CAVE_VOD_PASSTHROUGH1_END_LOCATION), GetLocationByMem(RomMap.EAST_CAVE_VOD_PASSTHROUGH1_START_LOCATION));
        connections.Add(GetLocationByMem(RomMap.EAST_CAVE_VOD_PASSTHROUGH1_START_LOCATION), GetLocationByMem(RomMap.EAST_CAVE_VOD_PASSTHROUGH1_END_LOCATION));

        locationAtPalace6 = GetLocationByMem(RomMap.EAST_PALACE6_TILE_LOCATION);
        locationAtPalace6.PalaceNumber = 6;
        townAtDarunia = GetLocationByMem(RomMap.EAST_TOWN_OF_DARUNIA_TILE_LOCATION);
        locationAtPalace5 = GetLocationByMem(RomMap.EAST_PALACE5_TILE_LOCATION);
        locationAtPalace5.PalaceNumber = 5;

        townAtNabooru = GetLocationByMem(RomMap.EAST_TOWN_OF_NABOORU_TILE_LOCATION);
        townAtNabooru.VanillaCollectable = props.ReplaceFireWithDash ? Collectable.DASH_SPELL : Collectable.FIRE_SPELL;
        townAtNabooru.Collectables = props.ReplaceFireWithDash ? [Collectable.DASH_SPELL] : [Collectable.FIRE_SPELL];
        townAtDarunia = GetLocationByMem(RomMap.EAST_TOWN_OF_DARUNIA_TILE_LOCATION);
        townAtDarunia.VanillaCollectable = Collectable.REFLECT_SPELL;
        townAtDarunia.Collectables = [Collectable.REFLECT_SPELL];

        townAtNewKasuto = GetLocationByMem(RomMap.EAST_TOWN_OF_NEW_KASUTO_TILE_LOCATION);
        townAtNewKasuto.VanillaCollectable = Collectable.SPELL_SPELL;
        townAtNewKasuto.Collectables = [Collectable.SPELL_SPELL];

        townAtOldKasuto = GetLocationByMem(RomMap.EAST_TOWN_OF_OLD_KASUTO_TILE_LOCATION);
        townAtOldKasuto.VanillaCollectable = Collectable.THUNDER_SPELL;
        townAtOldKasuto.Collectables = [Collectable.THUNDER_SPELL];

        waterTile = GetLocationByMem(RomMap.EAST_WATER_TILE_LOCATION);
        waterTile.NeedBoots = true;
        desertTile = GetLocationByMem(RomMap.EAST_DESERT_TILE_LOCATION);

        Debug.Assert(locationAtPalace5 != null); // checking if this can be removed
        if (locationAtPalace5 == null)
        {
            locationAtPalace5 = GetLocationByMem(0x8657);
            locationAtPalace5.PalaceNumber = 5;
        }

        hiddenPalaceCoords = (0, 0);

        sideviewPtrTable = 0x8533;
        sideviewBank = 2;
        enemyPtr = 0x85B1;
        groupedEnemies = Enemies.GroupedEastEnemies;
        overworldEncounterMaps = [
            29, 30, // Desert
            34, 35, // Grass
            39, 40, // Forest
            47, 48, // Swamp     - in vanilla 47-48 use the same table
            52, 53, // Graveyard - in vanilla 52-53 use the same table
            57,     // Road      - in vanilla 57-58 use the same table
            59, 60, // Lava      - in vanilla 59-60 use the same table
        ];
        overworldEncounterMapDuplicate = [
            58,
        ];
        nonEncounterMaps = [
            00, 01,         // Bridges
            02, 03, 04, 05, // Wilson Fence
            06,             // Darunia trap tile
            07,             // WATER_TILE
            08,             // Nabooru Cave passthrough
            09, 10,         // SUNKEN_PBAG_CAVE
            11, 12,         // RISEN_PBAG_CAVE
            13, 14,         // NEW_KASUTO_CAVE
            16, 17,         // DEATH_VALLEY_CAVE_1
            19, 20, 22,     // DEATH_VALLEY_CAVE_2
            23,             // Darunia trap tile
            24,             // Valley of Death passthrough
            25,             // Valley of Death passthrough
            26,             // Valley of Death passthrough
            33,             // Desert Hills 500 P-Bag
            38,             // Grass Hills 200 P-Bag
            44,             // Stonehenge 500 P-bag
            45,             // FIRE_TOWN_FAIRY
            46,             // DESERT_TILE
            51,             // OLD_KASUTO_SWAMP_LIFE
            62,             // DEATH_VALLEY_RED_JAR
        ];

        locationAtGP = GetLocationByMem(RomMap.EAST_GREAT_PALACE_TILE_LOCATION);
        locationAtGP.PalaceNumber = 7;
        locationAtGP.VanillaCollectable = Collectable.DO_NOT_USE;
        locationAtGP.Collectables = [];
        pbagCave1 = GetLocationByMem(RomMap.EAST_CAVE_PBAG1_LOCATION);
        pbagCave2 = GetLocationByMem(RomMap.EAST_CAVE_PBAG2_LOCATION);
        VANILLA_MAP_ADDR = 0x9056;

        //Fake locations that dont correspond to anywhere on the map, but still hold logic and items
        spellTower = new Location(townAtNewKasuto);
        spellTower.VanillaCollectable = Collectable.MAGIC_KEY;
        spellTower.Collectables = [Collectable.MAGIC_KEY];
        spellTower.Name = "Spell Tower";
        spellTower.CanShuffle = false;
        spellTower.ActualTown = null;
        townAtNewKasuto.Children.Add(spellTower);
        AddLocation(spellTower);

        newKasutoBasement = new Location(townAtNewKasuto);
        newKasutoBasement.VanillaCollectable = Collectable.MAGIC_CONTAINER;
        newKasutoBasement.Collectables = [Collectable.MAGIC_CONTAINER];
        newKasutoBasement.Name = "Granny's basement";
        newKasutoBasement.CanShuffle = false;
        newKasutoBasement.ActualTown = null;
        townAtNewKasuto.Children.Add(newKasutoBasement);
        AddLocation(newKasutoBasement);

        fountain = new Location(townAtNabooru);
        fountain.VanillaCollectable = Collectable.WATER;
        fountain.Collectables = [Collectable.WATER];
        fountain.Name = "Water Fountain";
        fountain.ActualTown = Town.NABOORU_FOUNTAIN;
        fountain.CanShuffle = false;
        townAtNabooru.Children.Add(fountain);
        AddLocation(fountain);

        daruniaRoof = new Location(townAtDarunia);
        daruniaRoof.VanillaCollectable = Collectable.UPSTAB;
        daruniaRoof.Collectables = [Collectable.UPSTAB];
        daruniaRoof.Name = "Darunia Roof";
        daruniaRoof.ActualTown = Town.DARUNIA_ROOF;
        daruniaRoof.CanShuffle = false;
        townAtDarunia.Children.Add(daruniaRoof);
        AddLocation(daruniaRoof);

        MAP_ROWS = 75;
        MAP_COLS = 64;

        walkableTerrains = [Terrain.DESERT, Terrain.GRASS, Terrain.FOREST, Terrain.SWAMP, Terrain.GRAVE];
        randomTerrainFilter = [Terrain.DESERT, Terrain.GRASS, Terrain.FOREST, Terrain.SWAMP, Terrain.GRAVE, Terrain.MOUNTAIN, Terrain.WALKABLEWATER];

        biome = props.EastBiome;
        section = new SortedDictionary<(int, int), string>()
        {
            { (0x3A, 0x0A), "mid2" },
            { (0x5B, 0x36), "south" },
            { (0x4C, 0x15), "south" },
            { (0x51, 0x11), "south" },
            { (0x54, 0x13), "south" },
            { (0x60, 0x18), "south" },
            { (0x5D, 0x23), "south" },
            { (0x64, 0x25), "south" },
            { (0x24, 0x09), "north2" },
            { (0x26, 0x0A), "north2" },
            { (0x38, 0x3F), "boots1" },
            { (0x34, 0x18), "mid2" },
            { (0x30, 0x1B), "north2" },
            { (0x47, 0x19), "mid2" },
            { (0x4E, 0x1F), "south" },
            { (0x4E, 0x31), "south" },
            { (0x4E, 0x39), "kasuto" },
            { (0x4B, 0x02), "vod" },
            { (0x4B, 0x04), "gp" },
            { (0x4D, 0x06), "vod" },
            { (0x4D, 0x0A), "south" },
            { (0x51, 0x1A), "south" },
            { (0x40, 0x35), "hammer2" },
            { (0x38, 0x22), "mid2" },
            { (0x2C, 0x30), "north2" },
            { (0x63, 0x39), "south" },
            { (0x44, 0x0D), "mid2" },
            { (0x5B, 0x04), "south" },
            { (0x63, 0x1B), "south" },
            { (0x53, 0x03), "vod" },
            { (0x56, 0x08), "south" },
            { (0x63, 0x08), "south" },
            { (0x28, 0x34), "north2" },
            { (0x34, 0x07), "mid2" },
            { (0x3C, 0x17), "mid2" },
            { (0x21, 0x03), "north2" },
            { (0x51, 0x3D), "kasuto" },
            { (0x63, 0x22), "south" },
            { (0x3C, 0x3E), "boots" },
            { (0x66, 0x2D), "south" },
            { (0x49, 0x04), "gp" }
        };
        townAtNewKasuto.ExternalWorld = 128;
        locationAtPalace6.ExternalWorld = 128;
        hiddenPalaceLocation = locationAtPalace6;
        hiddenKasutoLocation = townAtNewKasuto;

        //Climate filtering
        climate = props.Climate.Clone();
        climate.SeedTerrainCount = Math.Min(climate.SeedTerrainCount, biome.SeedTerrainLimit());
        climate.DisallowTerrain(props.CanWalkOnWaterWithBoots ? Terrain.WATER : Terrain.WALKABLEWATER);
        //climate.DisallowTerrain(Terrain.LAVA);
        SetVanillaCollectables(props.ReplaceFireWithDash);
    }

    public override bool Terraform(RandomizerProperties props, ROM rom)
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
        if (props.HideLessImportantLocations)
        {
            unimportantLocs =
            [
                GetLocationByMem(RomMap.EAST_MINOR_FOREST_TILE_BY_NABOORU_LOCATION),
                GetLocationByMem(RomMap.EAST_MINOR_FOREST_TILE_BY_P6_LOCATION),
                GetLocationByMem(RomMap.EAST_MINOR_SWAMP_TILE_LOCATION),
                GetLocationByMem(RomMap.EAST_MINOR_DESERT_TILE_LOCATION1),
                GetLocationByMem(RomMap.EAST_MINOR_DESERT_TILE_LOCATION2),
                GetLocationByMem(RomMap.EAST_MINOR_DESERT_TILE_LOCATION3),
                GetLocationByMem(RomMap.EAST_MINOR_FOREST_TILE_LOCATION2),
                GetLocationByMem(RomMap.EAST_MINOR_LAVA_TILE_LOCATION1),
                GetLocationByMem(RomMap.EAST_MINOR_LAVA_TILE_LOCATION2)
            ];
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
                    areasByLocation[section[location.Coords]].Add(GetLocationByCoords(location.Coords)!);
                }

                ChooseConn("kasuto", connections, true);
                ChooseConn("vod", connections, true);
                ChooseConn("gp", connections, true);

                if (!props.ShuffleHidden)
                {
                    townAtNewKasuto.CanShuffle = false;
                    locationAtPalace6.CanShuffle = false;
                }
                ShuffleLocations(AllLocations);
                if (props.VanillaShuffleUsesActualTerrain)
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
                if(raft != null)
                {
                    raft.PassThrough = 0;
                }
                if (bridge != null)
                {
                    bridge.PassThrough = 0;
                }
                //Issue #2: Desert tile passthrough causes the wrong screen to load, making the item unobtainable.
                desertTile.PassThrough = 0;

                desertTile.MapPage = 64;

                Location? desert = GetLocationByMem(RomMap.EAST_MINOR_DESERT_TILE_LOCATION1);
                Location? swamp = GetLocationByMem(RomMap.EAST_MINOR_SWAMP_TILE_LOCATION);
                if(desert == null || swamp == null)
                {
                    throw new ImpossibleException("Unable to find desert/swamp passthrough on east.");
                }

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
			
            //in vanilla shuffle, post location shuffling, the locations have moved, but the hidden palace spot doesn't
            //so reset the reference
            hiddenKasutoLocation = GetLocationByCoords((81, 61))!;
            hiddenPalaceLocation = GetLocationByCoords((102, 45))!;
            if (props.HiddenKasuto)
            {
                if (connections.ContainsKey(hiddenKasutoLocation) || hiddenKasutoLocation == raft || hiddenKasutoLocation == bridge)
                {
                    return false;
                }
            }
            if (props.HiddenPalace)
            {
                if (connections.ContainsKey(hiddenPalaceLocation) || hiddenPalaceLocation == raft || hiddenPalaceLocation == bridge)
                {
                    return false;
                }
            }
            else
            {
                if(props.ShuffleHidden)
                {
                    map[72, 45] = hiddenPalaceLocation.TerrainType;
                }
                else
                {
                    map[72, 45] = Terrain.PALACE;
                }
            }
        }
        else //Not vanilla / vanillaShuffle
        {
            Terrain fillerWater = props.CanWalkOnWaterWithBoots ? Terrain.WALKABLEWATER : Terrain.WATER;

            int bytesWritten = 2000;
            locationAtGP.CanShuffle = false;
            Terrain riverTerrain = Terrain.MOUNTAIN;
            while (bytesWritten > MAP_SIZE_BYTES)
            {
                //This double initialization for locations for non-vanilla maps is not well written, at some point I should clean this up
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
                if (props.HideLessImportantLocations)
                {
                    unimportantLocs = new List<Location>
                    {
                        GetLocationByMem(RomMap.EAST_MINOR_FOREST_TILE_BY_NABOORU_LOCATION),
                        GetLocationByMem(RomMap.EAST_MINOR_FOREST_TILE_BY_P6_LOCATION),
                        GetLocationByMem(RomMap.EAST_MINOR_SWAMP_TILE_LOCATION),
                        GetLocationByMem(RomMap.EAST_MINOR_DESERT_TILE_LOCATION1),
                        GetLocationByMem(RomMap.EAST_MINOR_DESERT_TILE_LOCATION2),
                        GetLocationByMem(RomMap.EAST_MINOR_DESERT_TILE_LOCATION3),
                        GetLocationByMem(RomMap.EAST_MINOR_FOREST_TILE_LOCATION2),
                        GetLocationByMem(RomMap.EAST_MINOR_LAVA_TILE_LOCATION1),
                        GetLocationByMem(RomMap.EAST_MINOR_LAVA_TILE_LOCATION2)
                    };
                }

                map = new Terrain[MAP_ROWS, MAP_COLS];

                for (int i = 0; i < MAP_ROWS; i++)
                {
                    for (int j = 0; j < MAP_COLS; j++)
                    {
                        map[i, j] = Terrain.NONE;
                    }
                }

                if (biome == Biome.ISLANDS)
                {
                    riverTerrain = fillerWater;
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
                    MakeValleyOfDeath();
                    int cols = RNG.Next(2, 4);
                    int rows = RNG.Next(2, 4);
                    List<int> pickedC = [];
                    List<int> pickedR = [];

                    while (cols > 0)
                    {
                        int col = RNG.Next(1, MAP_COLS - 1);
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
                        int row = RNG.Next(1, MAP_ROWS - 1);
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
                    walkableTerrains = [Terrain.LAVA, Terrain.DESERT, Terrain.GRASS, Terrain.FOREST, Terrain.SWAMP, Terrain.GRAVE];
                    randomTerrainFilter = [Terrain.LAVA, Terrain.DESERT, Terrain.GRASS, Terrain.FOREST, Terrain.SWAMP, Terrain.GRAVE, Terrain.MOUNTAIN, fillerWater];




                }
                else if (biome == Biome.CANYON || biome == Biome.DRY_CANYON)
                {
                    riverTerrain = fillerWater;
                    if (biome == Biome.DRY_CANYON)
                    {
                        riverTerrain = Terrain.DESERT;
                    }
                    //riverT = terrain.lava;
                    walkableTerrains = [Terrain.DESERT, Terrain.GRASS, Terrain.FOREST, Terrain.GRAVE, Terrain.MOUNTAIN];
                    randomTerrainFilter = [Terrain.DESERT, Terrain.GRASS, Terrain.FOREST, Terrain.GRAVE, Terrain.MOUNTAIN, fillerWater];


                    DrawCanyon(riverTerrain);
                    walkableTerrains.Remove(Terrain.MOUNTAIN);

                    locationAtGP.CanShuffle = false;


                }
                else if (biome == Biome.VOLCANO)
                {
                    DrawCenterMountain();

                    walkableTerrains = new List<Terrain>() { Terrain.LAVA, Terrain.DESERT, Terrain.GRASS, Terrain.FOREST, Terrain.SWAMP, Terrain.GRAVE };
                    randomTerrainFilter = new List<Terrain> { Terrain.LAVA, Terrain.DESERT, Terrain.GRASS, Terrain.FOREST, Terrain.SWAMP, Terrain.GRAVE, Terrain.MOUNTAIN, fillerWater };


                }
                else if (biome == Biome.MOUNTAINOUS)
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
                    MakeValleyOfDeath();

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
                    walkableTerrains = new List<Terrain>() { Terrain.DESERT, Terrain.GRASS, Terrain.FOREST, Terrain.SWAMP, Terrain.GRAVE };
                    randomTerrainFilter = new List<Terrain> { Terrain.DESERT, Terrain.GRASS, Terrain.FOREST, Terrain.SWAMP, Terrain.GRAVE, Terrain.MOUNTAIN, fillerWater };
                }
                else
                {
                    walkableTerrains = new List<Terrain>() { Terrain.DESERT, Terrain.GRASS, Terrain.FOREST, Terrain.SWAMP, Terrain.GRAVE };
                    randomTerrainFilter = new List<Terrain> { Terrain.DESERT, Terrain.GRASS, Terrain.FOREST, Terrain.SWAMP, Terrain.GRAVE, Terrain.MOUNTAIN, fillerWater };
                    MakeValleyOfDeath();


                    DrawMountains(props.RiverDevilBlockerOption == RiverDevilBlockerOption.PATH);
                    DrawRiver(props.CanWalkOnWaterWithBoots);
                }

                if (biome == Biome.VOLCANO || biome == Biome.CANYON || biome == Biome.DRY_CANYON)
                {
                    bool f = MakeValleyOfDeath();
                    if (!f)
                    {
                        return false;
                    }
                }
                if (props.HiddenPalace)
                {
                    bool hp = RandomizeHiddenPalace(rom, props.ShuffleHidden, props.HiddenKasuto);
                    if (!hp)
                    {
                        return false;
                    }
                }
                Direction raftDirection = Direction.WEST;
                if (props.ContinentConnections != ContinentConnectionType.NORMAL && biome != Biome.CANYON && biome != Biome.DRY_CANYON)
                {
                    raftDirection = (Direction)RNG.Next(4);
                }
                else if (biome == Biome.CANYON || biome == Biome.DRY_CANYON || biome == Biome.VOLCANO)
                {
                    raftDirection = isHorizontal ? DirectionExtensions.RandomHorizontal(RNG) : DirectionExtensions.RandomVertical(RNG);

                }
                if (raft != null)
                {
                    DrawOcean(raftDirection, props.CanWalkOnWaterWithBoots);
                }


                Direction bridgeDirection;
                do
                {
                    if (biome != Biome.CANYON && biome != Biome.DRY_CANYON && biome != Biome.VOLCANO)
                    {
                        bridgeDirection = (Direction)RNG.Next(4);
                    }
                    else
                    {
                        bridgeDirection = (Direction)RNG.Next(2);
                        if (isHorizontal)
                        {
                            bridgeDirection += 2;
                        }
                    }
                } while (bridgeDirection == raftDirection);

                if (bridge != null)
                {
                    DrawOcean(bridgeDirection, props.CanWalkOnWaterWithBoots);
                }

                if (props.HiddenKasuto)
                {
                    RandomizeHiddenKasuto(props.ShuffleHidden);
                }

                bool placeLocationsResult;
                if (props.HiddenKasuto && props.HiddenPalace)
                {
                    placeLocationsResult = PlaceLocations(riverTerrain, props.SaneCaves, hiddenKasutoLocation, hiddenPalaceLocation.Xpos);
                }
                else
                {
                    placeLocationsResult = PlaceLocations(riverTerrain, props.SaneCaves);
                }
                if (!placeLocationsResult)
                {
                    return false;
                }

                PlaceRandomTerrain(climate);

                randomTerrainFilter.Add(Terrain.LAVA);
                if (!GrowTerrain(climate))
                {
                    return false;
                }
                randomTerrainFilter.Remove(Terrain.LAVA);
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

                /*
                if (biome == Biome.VOLCANO || biome == Biome.CANYON)
                {
                    bool f = MakeVolcano();
                    if (!f)
                    {
                        return false;
                    }
                }
                */
                bool riverDevil = props.RiverDevilBlockerOption == RiverDevilBlockerOption.CAVE;
                bool rockBlock = props.EastRocks && !props.EastRockIsPath;
                BlockCaves(props.BoulderBlockConnections, riverDevil, rockBlock);
                PlaceHiddenLocations();
                riverDevil = props.RiverDevilBlockerOption == RiverDevilBlockerOption.PATH;
                rockBlock = props.EastRocks && props.EastRockIsPath;
                if (biome == Biome.VANILLALIKE)
                {
                    ConnectIslands(4, false, Terrain.MOUNTAIN, false, false, false, true, props.CanWalkOnWaterWithBoots, biome);

                    ConnectIslands(3, false, fillerWater, riverDevil, rockBlock, false, false, props.CanWalkOnWaterWithBoots, biome);

                }
                if (biome == Biome.ISLANDS)
                {
                    ConnectIslands(100, false, riverTerrain, riverDevil, rockBlock, false, true, props.CanWalkOnWaterWithBoots, biome);
                }
                if (biome == Biome.MOUNTAINOUS)
                {
                    ConnectIslands(20, false, riverTerrain, riverDevil, rockBlock, false, true, props.CanWalkOnWaterWithBoots, biome);
                }
                if (biome == Biome.CANYON || biome == Biome.DRY_CANYON)
                {
                    ConnectIslands(15, false, riverTerrain, riverDevil, rockBlock, false, true, props.CanWalkOnWaterWithBoots, biome);

                }

                if(props.RiverDevilBlockerOption == RiverDevilBlockerOption.SIEGE)
                {
                    //Iterate the towns in a random order
                    Location[] towns = [townAtNabooru, townAtDarunia, townAtNewKasuto, townAtOldKasuto];
                    RNG.Shuffle(towns);
                    bool placed = false;
                    foreach (Location location in towns)
                    {
                        //if the town has walkable space in all 4 cardinal directions
                        //TODO: a little bit of bounds safety.
                        //It is currently impossible to put towns on the edge of the map but don't trust that
                        if (map[location.Ypos - 29, location.Xpos].IsWalkable() && map[location.Ypos - 30, location.Xpos - 1].IsWalkable() &&
                            map[location.Ypos - 31, location.Xpos].IsWalkable() && map[location.Ypos - 30, location.Xpos + 1].IsWalkable())
                        {
                            //replace each cardinal adjacency with a river devil
                            map[location.Ypos - 29, location.Xpos] = Terrain.RIVER_DEVIL;
                            map[location.Ypos - 31, location.Xpos] = Terrain.RIVER_DEVIL;
                            map[location.Ypos - 30, location.Xpos - 1] = Terrain.RIVER_DEVIL;
                            map[location.Ypos - 30, location.Xpos + 1] = Terrain.RIVER_DEVIL;
                            placed = true;
                            break;
                        }
                    }
                    if(!placed)
                    {
                        return false;
                    }
                }

                foreach (Location location in AllLocations)
                {
                    if (location.CanShuffle && location.AppearsOnMap)
                    {
                        location.Ypos = 0;
                        location.CanShuffle = false;
                    }
                }
                bytesWritten = WriteMapToRom(rom, false, MAP_ADDR, MAP_SIZE_BYTES, hiddenPalaceLocation.Ypos - 30, hiddenPalaceLocation.Xpos, props.HiddenPalace, props.HiddenKasuto);
                //logger.Debug("East:" + bytesWritten);
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

        if (props.HiddenPalace)
        {
            rom.UpdateHiddenPalaceSpot(biome, hiddenPalaceCoords, hiddenPalaceLocation,
                townAtNewKasuto, spellTower, props.VanillaShuffleUsesActualTerrain);
        }
        if (props.HiddenKasuto)
        {
            rom.UpdateKasuto(hiddenKasutoLocation, townAtNewKasuto, spellTower, biome,
                baseAddr, terrains[hiddenKasutoLocation.MemAddress], props.VanillaShuffleUsesActualTerrain);
        }

        WriteMapToRom(rom, true, MAP_ADDR, MAP_SIZE_BYTES, hiddenPalaceLocation.Ypos - 30, hiddenPalaceLocation.Xpos, props.HiddenPalace, props.HiddenKasuto);


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

    public bool MakeValleyOfDeath()
    {
        int xmin = 21;
        int xmax = 41;
        int ymin = 22;
        int ymax = 52;
        if (biome != Biome.VOLCANO)
        {
            xmin = 5;
            ymin = 5;
            xmax = MAP_COLS - 6;
            ymax = MAP_COLS - 6;
        }
        int palacex = RNG.Next(xmin, xmax);
        int palacey = RNG.Next(ymin, ymax);

        if (biome == Biome.VOLCANO || biome == Biome.CANYON || biome == Biome.DRY_CANYON)
        {
            bool placeable;
            do
            {
                palacex = RNG.Next(xmin, xmax);
                palacey = RNG.Next(ymin, ymax);
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
        if (biome != Biome.CANYON && biome != Biome.DRY_CANYON && biome != Biome.VOLCANO)
        {
            length = RNG.Next(5, 16);
        }
        int deltax = 1;
        int deltay = 0;
        int starty = palacey;
        int startx = palacex + 4;
        if (biome != Biome.CANYON && biome != Biome.DRY_CANYON)
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
        Location? vodcave1, vodcave2, vodcave3, vodcave4;
        canyonShort = RNG.NextDouble() > .5;
        if (canyonShort)
        {
            vodcave1 = GetLocationByMem(RomMap.EAST_CAVE_VOD_PASSTHROUGH2_START_LOCATION);
            vodcave2 = GetLocationByMem(RomMap.EAST_CAVE_VOD_PASSTHROUGH2_END_LOCATION);
            vodcave3 = GetLocationByMem(RomMap.EAST_CAVE_VOD_PASSTHROUGH1_END_LOCATION);
            vodcave4 = GetLocationByMem(RomMap.EAST_CAVE_VOD_PASSTHROUGH1_START_LOCATION);
        }
        else
        {
            vodcave1 = GetLocationByMem(RomMap.EAST_CAVE_VOD_PASSTHROUGH1_END_LOCATION);
            vodcave2 = GetLocationByMem(RomMap.EAST_CAVE_VOD_PASSTHROUGH1_START_LOCATION);
            vodcave3 = GetLocationByMem(RomMap.EAST_CAVE_VOD_PASSTHROUGH2_START_LOCATION);
            vodcave4 = GetLocationByMem(RomMap.EAST_CAVE_VOD_PASSTHROUGH2_END_LOCATION);
        }

        int forced = 0;
        int vodRoutes = RNG.Next(1, 3);
        //bool horizontalPath = (isHorizontal && biome != Biome.CANYON) || (!isHorizontal && biome == Biome.CANYON);
        bool horizontalPath = isHorizontal ^ (biome == Biome.CANYON || biome == Biome.DRY_CANYON);

        if (biome != Biome.VOLCANO)
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
            while (startx > 1
                && startx < MAP_COLS - 1
                && starty > 1
                && starty < MAP_ROWS - 1
                && (((biome == Biome.VOLCANO || biome == Biome.CANYON || biome == Biome.DRY_CANYON) && map[starty, startx] == Terrain.MOUNTAIN) || (biome != Biome.VOLCANO && biome != Biome.CANYON && biome != Biome.DRY_CANYON && c < length)))
            {
                c++;
                map[starty, startx] = Terrain.LAVA;
                int adjust = RNG.Next(minadjust, maxadjust);
                while ((deltax != 0 && (starty + adjust < 1 || starty + adjust > MAP_ROWS - 2)) || (deltay != 0 && (startx + adjust < 1 || startx + adjust > MAP_COLS - 2)))
                {
                    adjust = RNG.Next(minadjust, maxadjust);

                }
                if (adjust > 0)
                {
                    if (biome != Biome.VOLCANO && biome != Biome.CANYON && biome != Biome.DRY_CANYON)
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
                            if (biome != Biome.VOLCANO && biome != Biome.CANYON && biome != Biome.DRY_CANYON)
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
                            Location? location = GetLocationByCoords((starty + i + 30, startx));
                            if (location != null && !location.CanShuffle)
                            {
                                return false;
                            }
                        }
                        else
                        {
                            map[starty, startx + i] = Terrain.LAVA;
                            if (biome != Biome.VOLCANO && biome != Biome.CANYON && biome != Biome.DRY_CANYON)
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
                            Location? location = GetLocationByCoords((starty + 30, startx + i));
                            if (location != null && !location.CanShuffle)
                            {
                                return false;
                            }
                        }
                    }
                    if (biome != Biome.VOLCANO && biome != Biome.CANYON && biome != Biome.DRY_CANYON)
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
                    if (biome != Biome.VOLCANO && biome != Biome.CANYON && biome != Biome.DRY_CANYON)
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
                            if (biome != Biome.VOLCANO && biome != Biome.CANYON && biome != Biome.DRY_CANYON)
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
                            Location? l = GetLocationByCoords((starty - i + 30, startx));
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
                            if (biome != Biome.VOLCANO && biome != Biome.CANYON && biome != Biome.DRY_CANYON)
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
                            Location? l = GetLocationByCoords((starty + 30, startx - i));
                            if (l != null && !l.CanShuffle)
                            {
                                return false;
                            }
                        }
                    }
                    if (biome != Biome.VOLCANO && biome != Biome.CANYON && biome != Biome.DRY_CANYON)
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
                    if (biome != Biome.VOLCANO && biome != Biome.CANYON && biome != Biome.DRY_CANYON)
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
                        if (GetLocationByCoords((starty + 30 + deltay, startx + deltax)) != null)
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
                    Location f = GetLocationByMem(RomMap.EAST_TRAP_LAVA_TILE_LOCATION1)!;
                    if (forced == 1)
                    {
                        f = GetLocationByMem(RomMap.EAST_TRAP_LAVA_TILE_LOCATION2)!;
                    }
                    else if (forced == 2)
                    {
                        f = GetLocationByMem(RomMap.EAST_TRAP_LAVA_TILE_LOCATION3)!;
                    }

                    if (adjust == 0)
                    {
                        if (horizontalPath)
                        {
                            if (GetLocationByCoords((starty + 30, startx - 1)) == null && GetLocationByCoords((starty + 30, startx + 1)) == null)
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
                            if (GetLocationByCoords((starty + 30 - 1, startx)) == null && GetLocationByCoords((starty + 30 + 1, startx)) == null)
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
                        //if ((isHorizontal && biome != Biome.CANYON) || (!isHorizontal && biome == Biome.CANYON))
                        if(isHorizontal ^ (biome == Biome.CANYON || biome == Biome.DRY_CANYON))
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

                    if (RNG.NextDouble() > .5 && vodRoutes != 2 && biome == Biome.VOLCANO)
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
                            starty += RNG.Next(-9, -4);
                        }
                        else
                        {
                            starty += RNG.Next(5, 10);
                        }
                    }
                    else
                    {
                        if (startx > MAP_COLS / 2)
                        {
                            startx += RNG.Next(-9, -4);
                        }
                        else
                        {
                            startx += RNG.Next(5, 10);
                        }
                    }
                    if (map[starty, startx] != Terrain.MOUNTAIN && (biome == Biome.VOLCANO || biome == Biome.CANYON || biome == Biome.DRY_CANYON))
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
                    if (GetLocationByCoords((starty + 30, startx + deltax)) != null)
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
                    if (GetLocationByCoords((starty + 30 + deltay, startx)) != null)
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

            if (biome != Biome.VOLCANO && biome != Biome.CANYON && biome != Biome.DRY_CANYON)
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

    private void RandomizeHiddenKasuto(bool shuffleHidden)
    {
        if (shuffleHidden)
        {
            hiddenKasutoLocation = AllLocations[RNG.Next(AllLocations.Count)];
            while (hiddenKasutoLocation == null
                || hiddenKasutoLocation == raft
                || hiddenKasutoLocation == bridge
                || hiddenKasutoLocation == cave1
                || hiddenKasutoLocation == cave2
                || (hiddenKasutoLocation.TerrainType == Terrain.TOWN && !hiddenKasutoLocation.AppearsOnMap) //no fake item locations
                || connections.ContainsKey(hiddenKasutoLocation)
                || !hiddenKasutoLocation.CanShuffle
                || ((biome != Biome.VANILLA && biome != Biome.VANILLA_SHUFFLE) && hiddenKasutoLocation.TerrainType == Terrain.LAVA && hiddenKasutoLocation.PassThrough != 0))
            {
                hiddenKasutoLocation = AllLocations[RNG.Next(AllLocations.Count)];
            }
        }
        else
        {
            hiddenKasutoLocation = townAtNewKasuto;
        }
        hiddenKasutoLocation.TerrainType = Terrain.FOREST;
        hiddenKasutoLocation.NeedHammer = true;
        unimportantLocs.Remove(hiddenKasutoLocation);
        //hkLoc.CanShuffle = false;
        //map[hkLoc.Ypos - 30, hkLoc.Xpos] = terrain.forest;
    }

    private bool RandomizeHiddenPalace(ROM rom, bool shuffleHidden, bool hiddenKasuto)
    {
        bool done = false;
        int xpos = RNG.Next(6, MAP_COLS - 6);
        int ypos = RNG.Next(6, MAP_ROWS - 6);
        if (shuffleHidden)
        {
            hiddenPalaceLocation = AllLocations[RNG.Next(AllLocations.Count)];

            while (hiddenPalaceLocation == null
                || hiddenPalaceLocation == raft
                || hiddenPalaceLocation == bridge
                || hiddenPalaceLocation == cave1
                || hiddenPalaceLocation == cave2
                || connections.ContainsKey(hiddenPalaceLocation)
                || !hiddenPalaceLocation.CanShuffle
                || hiddenPalaceLocation == hiddenKasutoLocation
                || (hiddenPalaceLocation.TerrainType == Terrain.TOWN && !hiddenPalaceLocation.AppearsOnMap) //no fake item locations
                || (biome != Biome.VANILLA
                    && biome != Biome.VANILLA_SHUFFLE
                    && hiddenPalaceLocation.TerrainType == Terrain.LAVA
                    && hiddenPalaceLocation.PassThrough != 0))
            {
                hiddenPalaceLocation = AllLocations[RNG.Next(AllLocations.Count)];
            }
        }
        else
        {
            hiddenPalaceLocation = locationAtPalace6;
        }
        int tries = 0;
        while (!done && tries < 1000)
        {
            xpos = RNG.Next(6, MAP_COLS - 6);
            ypos = RNG.Next(6, MAP_ROWS - 6);
            done = true;
            //#124
            if (hiddenKasuto && xpos == hiddenKasutoLocation.Xpos)
            {
                continue;
            }
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
        if(hiddenPalaceLocation == null || hiddenKasutoLocation == null)
        {
            throw new ImpossibleException("Failure in hidden location shuffle");
        }
        Terrain t = climate.GetRandomTerrain(RNG, walkableTerrains);
        while (t == Terrain.FOREST)
        {
            t = climate.GetRandomTerrain(RNG, walkableTerrains);
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
        hiddenPalaceCoords = (ypos + 30, xpos);
        //This is the only thing requiring a reference to the rom here and I have no idea what the fuck it is doing.
        rom.Put(0x1df70, (byte)t);
        hiddenPalaceLocation.CanShuffle = false;
        return true;
    }

    public new void UpdateAllReached()
    {
        if (!AllReached)
        {
            base.UpdateAllReached();
            if (!hiddenPalaceLocation.Reachable || !hiddenKasutoLocation.Reachable || !spellTower.Reachable)
            {
                AllReached = false;
            }
        }
    }

    public override void UpdateVisit(Dictionary<Collectable, bool> itemGet)
    {
        UpdateReachable(itemGet);

        foreach (Location location in AllLocations)
        {
            if (location.Ypos > 30 && visitation[location.Ypos - 30, location.Xpos])
            {
                if ((!location.NeedRecorder || (location.NeedRecorder && itemGet[Collectable.FLUTE]))
                    && (!location.NeedHammer || (location.NeedHammer && itemGet[Collectable.HAMMER]))
                    && (!location.NeedBoots || (location.NeedBoots && itemGet[Collectable.BOOTS])))
                {
                    location.Reachable = true;
                    if (connections.ContainsKey(location))
                    {
                        Location connectedLocation = connections[location];

                        if (location.NeedFairy && itemGet[Collectable.FAIRY_SPELL])
                        {
                            connectedLocation.Reachable = true;
                            visitation[connectedLocation.Ypos - 30, connectedLocation.Xpos] = true;
                        }

                        if (location.NeedJump && (itemGet[Collectable.JUMP_SPELL] || itemGet[Collectable.FAIRY_SPELL]))
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
        }
    }

    private double ComputeDistance(Location l, Location l2)
    {
        return Math.Sqrt(Math.Pow(l.Xpos - l2.Xpos, 2) + Math.Pow(l.Ypos - l2.Ypos, 2));
    }

    private void BlockCaves(bool connectionsCanBeBlocked, bool riverDevilBlocks, bool rockBlock)
    {
        //Blockers are on, but there are no block types 
        if(!riverDevilBlocks && !rockBlock)
        {
            return;
        }
        int cavePicked = 0;
        bool riverDevilPlaced = !riverDevilBlocks;
        bool rockPlaced = !rockBlock;
        while (!riverDevilPlaced || !rockPlaced)
        {
            bool placed = false;
            Terrain blockerTerrain = riverDevilPlaced ? Terrain.ROCK : Terrain.RIVER_DEVIL;
            List<Location> Caves = Locations[Terrain.CAVE];
            Location cave = Caves[RNG.Next(Caves.Count)];
            int caveConn = 0;
            if (caveConn != 0 && connections.ContainsKey(GetLocationByMem(cavePicked)))
            {
                caveConn = connections[GetLocationByMem(cavePicked)].MemAddress;
            }
            if (connectionsCanBeBlocked && cave.MemAddress != cavePicked && cave.MemAddress != caveConn)
            {
                //Von's bug from discord: When moving back the mountain, check both the space the mountain will be placed
                //and the space 1 past it so the possible border on isolated tiles is retained.
                if (map[cave.Ypos - 30, cave.Xpos - 1] != Terrain.MOUNTAIN && cave.Xpos + 2 < MAP_COLS 
                    && GetLocationByCoords((cave.Ypos - 30, cave.Xpos + 2)) == null
                    && GetLocationByCoords((cave.Ypos - 30, cave.Xpos + 3)) == null
                )
                {
                    map[cave.Ypos - 30, cave.Xpos - 1] = blockerTerrain;
                    map[cave.Ypos - 30, cave.Xpos] = Terrain.ROAD;
                    map[cave.Ypos - 30, cave.Xpos + 1] = Terrain.CAVE;
                    if (cave.Xpos + 2 < MAP_COLS)
                    {
                        map[cave.Ypos - 30, cave.Xpos + 2] = Terrain.MOUNTAIN;
                    }
                    cave.Xpos++;
                    placed = true;
                }
                else if (map[cave.Ypos - 30, cave.Xpos + 1] != Terrain.MOUNTAIN && cave.Xpos - 2 > 0 
                    && GetLocationByCoords((cave.Ypos - 30, cave.Xpos - 2)) == null
                    && GetLocationByCoords((cave.Ypos - 30, cave.Xpos - 3)) == null
                )
                {
                    map[cave.Ypos - 30, cave.Xpos + 1] = blockerTerrain;
                    map[cave.Ypos - 30, cave.Xpos] = Terrain.ROAD;
                    map[cave.Ypos - 30, cave.Xpos - 1] = Terrain.CAVE;
                    if (cave.Xpos - 2 >= 0)
                    {
                        map[cave.Ypos - 30, cave.Xpos - 2] = Terrain.MOUNTAIN;
                    }
                    cave.Xpos--;
                    placed = true;
                }
                else if (map[cave.Ypos - 29, cave.Xpos] != Terrain.MOUNTAIN && cave.Ypos - 32 < MAP_COLS 
                    && GetLocationByCoords((cave.Ypos - 32, cave.Xpos)) == null
                    && GetLocationByCoords((cave.Ypos - 33, cave.Xpos)) == null
                )
                {
                    map[cave.Ypos - 29, cave.Xpos] = blockerTerrain;
                    map[cave.Ypos - 30, cave.Xpos] = Terrain.ROAD;
                    map[cave.Ypos - 31, cave.Xpos] = Terrain.CAVE;
                    if (cave.Ypos - 32 >= 0)
                    {
                        map[cave.Ypos - 32, cave.Xpos] = Terrain.MOUNTAIN;
                    }
                    cave.Ypos--;
                    placed = true;
                }
                else if (map[cave.Ypos - 31, cave.Xpos] != Terrain.MOUNTAIN && cave.Ypos - 28 < MAP_COLS 
                    && GetLocationByCoords((cave.Ypos - 28, cave.Xpos)) == null
                    && GetLocationByCoords((cave.Ypos - 27, cave.Xpos)) == null
                )
                {
                    map[cave.Ypos - 31, cave.Xpos] = blockerTerrain;
                    map[cave.Ypos - 30, cave.Xpos] = Terrain.ROAD;
                    map[cave.Ypos - 29, cave.Xpos] = Terrain.CAVE;
                    if (cave.Ypos - 28 < MAP_ROWS)
                    {
                        map[cave.Ypos - 28, cave.Xpos] = Terrain.MOUNTAIN;
                    }
                    cave.Ypos++;
                    placed = true;
                }
                cavePicked = cave.MemAddress;
            }
            else if (!connections.Keys.Contains(cave) && cave != cave1 && cave != cave2 && cave.MemAddress != cavePicked)
            {
                if (map[cave.Ypos - 30, cave.Xpos - 1] != Terrain.MOUNTAIN)
                {
                    map[cave.Ypos - 30, cave.Xpos - 1] = blockerTerrain;
                    placed = true;
                }
                else if (map[cave.Ypos - 30, cave.Xpos + 1] != Terrain.MOUNTAIN)
                {
                    map[cave.Ypos - 30, cave.Xpos + 1] = blockerTerrain;
                    placed = true;
                }
                else if (map[cave.Ypos - 29, cave.Xpos] != Terrain.MOUNTAIN)
                {
                    map[cave.Ypos - 29, cave.Xpos] = blockerTerrain;
                    placed = true;
                }
                else if (map[cave.Ypos - 31, cave.Xpos] != Terrain.MOUNTAIN)
                {
                    map[cave.Ypos - 31, cave.Xpos] = blockerTerrain;
                    placed = true;
                }
                cavePicked = cave.MemAddress;
            }
            if(placed)
            {
                if(blockerTerrain == Terrain.RIVER_DEVIL)
                {
                    riverDevilPlaced = true;
                }
                else if (blockerTerrain == Terrain.ROCK)
                {
                    rockPlaced = true;
                }
            }
        }
    }

    private void DrawMountains(bool useRiverDevil)
    {
        //create some mountains
        int mounty = RNG.Next(MAP_COLS / 3 - 10, MAP_COLS / 3 + 10);
        map[mounty, 0] = Terrain.MOUNTAIN;
        bool placedSpider = !useRiverDevil;


        int endmounty = RNG.Next(MAP_COLS / 3 - 10, MAP_COLS / 3 + 10);
        int endmountx = RNG.Next(2, 8);
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
                        map[y2, x2] = Terrain.RIVER_DEVIL;
                        placedSpider = true;
                    }
                    else if (map[y2, x2 + 1] == Terrain.NONE && (((y2 > 0 && map[y2 - 1, x2] == Terrain.ROAD) && (y2 < MAP_ROWS - 1 && map[y2 + 1, x2] == Terrain.ROAD)) || ((x2 > 0 && map[y2, x2 - 0] == Terrain.ROAD) && (x2 < MAP_COLS - 1 && map[y2, x2 + 1] == Terrain.ROAD))))
                    {
                        if (roadEncounters == 0)
                        {
                            Location roadEnc = GetLocationByMem(RomMap.EAST_TRAP_ROAD_TILE_LOCATION1);
                            roadEnc.Xpos = x2;
                            roadEnc.Ypos = y2 + 30;
                            roadEnc.CanShuffle = false;
                            roadEncounters++;
                        }
                        else if (roadEncounters == 1)
                        {
                            Location roadEnc = GetLocationByMem(RomMap.EAST_TRAP_ROAD_TILE_LOCATION2);
                            roadEnc.Xpos = x2;
                            roadEnc.Ypos = y2 + 30;
                            roadEnc.CanShuffle = false;
                            roadEncounters++;
                        }
                        else if (roadEncounters == 2)
                        {
                            Location roadEnc = GetLocationByMem(RomMap.EAST_TRAP_ROAD_TILE_LOCATION3);
                            roadEnc.Xpos = x2;
                            roadEnc.Ypos = y2 + 30;
                            roadEnc.CanShuffle = false;
                            roadEncounters++;
                        }
                        else if (roadEncounters == 3)
                        {
                            Location roadEnc = GetLocationByMem(RomMap.EAST_TRAP_ROAD_TILE_TO_VOD_LOCATION);
                            roadEnc.Xpos = x2;
                            roadEnc.Ypos = y2 + 30;
                            roadEnc.CanShuffle = false;
                            roadEncounters++;
                        }
                    }
                }
            }
        }

        mounty = RNG.Next(MAP_COLS * 2 / 3 - 10, MAP_COLS * 2 / 3 + 10);
        map[mounty, 0] = Terrain.MOUNTAIN;

        endmounty = RNG.Next(MAP_COLS * 2 / 3 - 10, MAP_COLS * 2 / 3 + 10);
        endmountx = RNG.Next(2, 8);
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
                        map[y2, x2] = Terrain.RIVER_DEVIL;
                        placedSpider = true;
                    }
                    else if (map[y2, x2 + 1] == Terrain.NONE && (((y2 > 0 && map[y2 - 1, x2] == Terrain.ROAD) && (y2 < MAP_ROWS - 1 && map[y2 + 1, x2] == Terrain.ROAD)) || ((x2 > 0 && map[y2, x2 - 0] == Terrain.ROAD) && (x2 < MAP_COLS - 1 && map[y2, x2 + 1] == Terrain.ROAD))))
                    {
                        if (roadEncounters == 0)
                        {
                            Location roadEnc = GetLocationByMem(RomMap.EAST_TRAP_ROAD_TILE_LOCATION1);
                            roadEnc.Xpos = x2;
                            roadEnc.Ypos = y2 + 30;
                            roadEnc.CanShuffle = false;
                            roadEncounters++;
                        }
                        else if (roadEncounters == 1)
                        {
                            Location roadEnc = GetLocationByMem(RomMap.EAST_TRAP_ROAD_TILE_LOCATION2);
                            roadEnc.Xpos = x2;
                            roadEnc.Ypos = y2 + 30;
                            roadEnc.CanShuffle = false;
                            roadEncounters++;
                        }
                        else if (roadEncounters == 2)
                        {
                            Location roadEnc = GetLocationByMem(RomMap.EAST_TRAP_ROAD_TILE_LOCATION3);
                            roadEnc.Xpos = x2;
                            roadEnc.Ypos = y2 + 30;
                            roadEnc.CanShuffle = false;
                            roadEncounters++;
                        }
                        else if (roadEncounters == 3)
                        {
                            Location roadEnc = GetLocationByMem(RomMap.EAST_TRAP_ROAD_TILE_TO_VOD_LOCATION);
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

    /// <summary>
    /// Determines if all locations on this continent could be reached, assuming you had everything and could reach every entrance.
    /// If the answer is no, there's no point in bothering with the rest of a world generation.
    /// </summary>
    /// <returns></returns>
    public bool ValidateBasicRouting()
    {
        List<Location> unreachedLocations = RequiredLocations(false, false).ToList();
        //SpellTower's connection logic isn't implemented here, nor do we care since we're assuming you have everything.
        unreachedLocations.Remove(spellTower);

        bool[,] visitedCoordinates = new bool[MAP_ROWS, MAP_COLS];
        List<(int, int)> pendingCoordinates = new();
        foreach (Location location in GetContinentConnections())
        {
            pendingCoordinates.Add((location.Ypos - 30, location.Xpos));
        }
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

#if UNSAFE_DEBUG
        if(unreachedLocations.Count == 2)
        {
            //Debug.WriteLine(unreachedLocations.First().Name);
            //Debug.WriteLine(GetMapDebug());
        }
        else if (unreachedLocations.Count <= 3)
        {
            //unreachedLocations.ForEach(i => Debug.WriteLine(i.Name));
        }
#endif

        return !unreachedLocations.Any();
    }

    protected override void OnUpdateReachableTrigger()
    {
        foreach(Location parentLocation in AllLocations.Where(i => i.Children != null && i.ActualTown != null))
        {
            parentLocation.Children.ForEach(child => child.Reachable = parentLocation.Reachable);
        }
    }

    public override string GetName()
    {
        return "East";
    }

    public override IEnumerable<Location> RequiredLocations(bool hiddenPalace, bool hiddenKasuto)
    {
        HashSet<Location> requiredLocations =
        [
            locationAtPalace5,
            locationAtPalace6,
            waterTile,
            desertTile,
            townAtDarunia,
            townAtNewKasuto,
            spellTower,
            townAtNabooru,
            townAtOldKasuto,
            locationAtGP,
            pbagCave1,
            pbagCave2,
            //vodcave1,
            //vodcave2
        ];

        if(hiddenPalace)
        {
            requiredLocations.Add(hiddenPalaceLocation);
            //support for the hidden palace call spot is extremely spotty. It effectively doesn't exist in the logic
            //and is only created to hold coordinates that are later written to the rom. Ignoring it for now.
            //requiredLocations.Add(hiddenPalaceCallSpot);
        }

        if(hiddenKasuto)
        {
            requiredLocations.Add(hiddenKasutoLocation);
        }

        foreach (Location location in connections.Keys)
        {
            if (!requiredLocations.Contains(location) && location.Ypos != 0)
            {
                requiredLocations.Add(location);
            }
        }
        return requiredLocations.Where(i => i != null);
    }

    protected override void SetVanillaCollectables(bool useDash)
    {
        locationAtPalace5.VanillaCollectable = Collectable.FLUTE;
        locationAtPalace6.VanillaCollectable = Collectable.CROSS;

        townAtNabooru.VanillaCollectable = useDash ? Collectable.DASH_SPELL : Collectable.FIRE_SPELL;
        fountain.VanillaCollectable = Collectable.WATER;
        townAtDarunia.VanillaCollectable = Collectable.REFLECT_SPELL;
        daruniaRoof.VanillaCollectable = Collectable.UPSTAB;
        newKasutoBasement.VanillaCollectable = Collectable.MAGIC_CONTAINER;
        townAtNewKasuto.VanillaCollectable = Collectable.SPELL_SPELL;
        spellTower.VanillaCollectable = Collectable.MAGIC_KEY;
        townAtOldKasuto.VanillaCollectable = Collectable.THUNDER_SPELL;

        waterTile.VanillaCollectable = Collectable.HEART_CONTAINER;
        desertTile.VanillaCollectable = Collectable.HEART_CONTAINER;
        pbagCave1.VanillaCollectable = Collectable.XL_BAG;
        pbagCave2.VanillaCollectable = Collectable.XL_BAG;
    }

    public override string GenerateSpoiler()
    {
        StringBuilder sb = new();
        sb.AppendLine("EAST: ");
        sb.AppendLine("\tNabooru: " + AllLocations.First(i => i.ActualTown == Town.NABOORU).Collectables[0].EnglishText());
        sb.AppendLine("\tFountain: " + fountain.Collectables[0].EnglishText());
        sb.AppendLine("\tDarunia: " + AllLocations.First(i => i.ActualTown == Town.DARUNIA_WEST).Collectables[0].EnglishText());
        sb.AppendLine("\tUpstab Guy: " + daruniaRoof.Collectables[0].EnglishText());
        sb.AppendLine("\tNew Kasuto: " + AllLocations.First(i => i.ActualTown == Town.NEW_KASUTO).Collectables[0].EnglishText());
        sb.AppendLine("\tGranny's Basement: " + newKasutoBasement.Collectables[0].EnglishText());
        sb.AppendLine("\tSpell Tower: " + spellTower.Collectables[0].EnglishText());
        sb.AppendLine("\tOld Kasuto: " + AllLocations.First(i => i.ActualTown == Town.OLD_KASUTO).Collectables[0].EnglishText());

        sb.AppendLine("\tRisen Pbag Cave: " + pbagCave2.Collectables[0].EnglishText());
        sb.AppendLine("\tSunken Pbag Cave: " + pbagCave1.Collectables[0].EnglishText());
        sb.AppendLine("\tWater Tile: " + waterTile.Collectables[0].EnglishText());
        sb.AppendLine("\tDesert tile: " + desertTile.Collectables[0].EnglishText());

        sb.Append("\tPalace 5 (" + locationAtPalace5.PalaceNumber + "): ");
        sb.AppendLine(locationAtPalace5.Collectables.Count == 0 ? "No Items" : string.Join(", ", locationAtPalace5.Collectables.Select(c => c.EnglishText())));

        sb.Append("\tPalace 6 (" + locationAtPalace6.PalaceNumber + "): ");
        sb.AppendLine(locationAtPalace6.Collectables.Count == 0 ? "No Items" : string.Join(", ", locationAtPalace6.Collectables.Select(c => c.EnglishText())));

        sb.Append("\tPalace 7 (" + locationAtGP.PalaceNumber + "): ");
        sb.AppendLine(locationAtGP.Collectables.Count == 0 ? "No Items" : string.Join(", ", locationAtGP.Collectables.Select(c => c.EnglishText())));

        sb.AppendLine();
        return sb.ToString();
    }
}