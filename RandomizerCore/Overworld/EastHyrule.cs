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
    int debug = 0;
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
    private List<Location> roadTrapLocations;

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
    public Location hiddenPalaceLocation;
    public Location hiddenKasutoLocation;
    public (int, int) hiddenPalaceCoords;
    private const int MAP_ADDR = 0xb480;

    private readonly List<Location> valleyOfDeathLocations;

    public EastHyrule(RandomizerProperties props, Random r, ROM rom) : base(r)
    {
        logger.Trace("Initializing EastHyrule");
        isHorizontal = props.EastIsHorizontal;
        baseAddr = RomMap.EAST_MINOR_FOREST_TILE_BY_NABOORU_LOCATION;
        continentId = Continent.EAST;

        List<Location> passthroughCaveLocations = [
            .. rom.LoadLocations(RomMap.EAST_CAVE_NABOORU_PASSTHROUGH_SOUTH_LOCATION, 2, terrains, Continent.EAST),
            .. rom.LoadLocations(RomMap.EAST_CAVE_NEW_KASUTO_PASSTHROUGH_WEST_LOCATION, 2, terrains, Continent.EAST),
        ];
        List<Location> passthroughVodLocations = [
            .. rom.LoadLocations(RomMap.EAST_CAVE_VOD_PASSTHROUGH2_START_LOCATION, 4, terrains, Continent.EAST),
        ];
        roadTrapLocations = [
            .. rom.LoadLocations(RomMap.EAST_TRAP_ROAD_TILE_LOCATION1, 4, terrains, Continent.EAST),
        ];
        List<Location> desertTrapLocations = [
            .. rom.LoadLocations(RomMap.EAST_TRAP_DESERT_TILE_LOCATION1, 2, terrains, Continent.EAST),
        ];

        List<Location> locations =
        [
            .. passthroughCaveLocations,
            .. passthroughVodLocations,
            .. roadTrapLocations,
            .. desertTrapLocations,
            .. rom.LoadLocations(RomMap.EAST_MINOR_FOREST_TILE_BY_NABOORU_LOCATION, 2, terrains, Continent.EAST),
            .. rom.LoadLocations(RomMap.EAST_BRIDGE_TILE_TO_P6_LOCATION, 2, terrains, Continent.EAST),
            .. rom.LoadLocations(RomMap.EAST_WATER_TILE_LOCATION, 1, terrains, Continent.EAST),
            .. rom.LoadLocations(RomMap.EAST_MINOR_SWAMP_TILE_LOCATION, 1, terrains, Continent.EAST),
            .. rom.LoadLocations(RomMap.EAST_CAVE_PBAG1_LOCATION, 2, terrains, Continent.EAST),
            .. rom.LoadLocations(RomMap.EAST_MINOR_DESERT_TILE_LOCATION1, 10, terrains, Continent.EAST),
            .. rom.LoadLocations(RomMap.EAST_TOWN_OF_NABOORU_TILE_LOCATION, 1, terrains, Continent.EAST),
            .. rom.LoadLocations(RomMap.EAST_TOWN_OF_DARUNIA_TILE_LOCATION, 1, terrains, Continent.EAST),
            .. rom.LoadLocations(RomMap.EAST_TOWN_OF_NEW_KASUTO_TILE_LOCATION, 1, terrains, Continent.EAST),
            .. rom.LoadLocations(RomMap.EAST_TOWN_OF_OLD_KASUTO_TILE_LOCATION, 4, terrains, Continent.EAST),
        ];
        locations.ForEach(AddLocation);

        valleyOfDeathLocations = [
            GetLocationByMem(RomMap.EAST_CAVE_VOD_PASSTHROUGH1_END_LOCATION),
            GetLocationByMem(RomMap.EAST_CAVE_VOD_PASSTHROUGH1_START_LOCATION),
            GetLocationByMem(RomMap.EAST_CAVE_VOD_PASSTHROUGH2_END_LOCATION),
            GetLocationByMem(RomMap.EAST_CAVE_VOD_PASSTHROUGH2_START_LOCATION),
            GetLocationByMem(RomMap.EAST_TRAP_LAVA_TILE_LOCATION1),
            GetLocationByMem(RomMap.EAST_TRAP_LAVA_TILE_LOCATION2),
            GetLocationByMem(RomMap.EAST_TRAP_LAVA_TILE_LOCATION3),
        ];

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

        //Palaces
        locationAtPalace5 = GetLocationByMem(RomMap.EAST_PALACE5_TILE_LOCATION);
        locationAtPalace5.PalaceNumber = 5;
        locationAtPalace5.CollectableRequirements = DEFAULT_PALACE_REQUIREMENTS;

        locationAtPalace6 = GetLocationByMem(RomMap.EAST_PALACE6_TILE_LOCATION);
        locationAtPalace6.PalaceNumber = 6;
        locationAtPalace5.CollectableRequirements = DEFAULT_PALACE_REQUIREMENTS;

        locationAtGP = GetLocationByMem(RomMap.EAST_GREAT_PALACE_TILE_LOCATION);
        locationAtGP.PalaceNumber = 7;
        locationAtGP.VanillaCollectable = Collectable.DO_NOT_USE;
        locationAtGP.Collectables = [];

        //Towns
        townAtNabooru = GetLocationByMem(RomMap.EAST_TOWN_OF_NABOORU_TILE_LOCATION);
        townAtNabooru.VanillaCollectable = props.ReplaceFireWithDash ? Collectable.DASH_SPELL : Collectable.FIRE_SPELL;
        townAtNabooru.Collectables = props.ReplaceFireWithDash ? [Collectable.DASH_SPELL] : [Collectable.FIRE_SPELL];
        townAtNabooru.CollectableRequirements = props.DisableMagicRecs ?
            new Requirements([RequirementType.WATER])
            : new Requirements([], [[RequirementType.WATER, RequirementType.FIVE_CONTAINERS]]);

        townAtDarunia = GetLocationByMem(RomMap.EAST_TOWN_OF_DARUNIA_TILE_LOCATION);
        townAtDarunia.VanillaCollectable = Collectable.REFLECT_SPELL;
        townAtDarunia.Collectables = [Collectable.REFLECT_SPELL];
        townAtDarunia.CollectableRequirements = props.DisableMagicRecs ?
            new Requirements([RequirementType.CHILD])
            : new Requirements([], [[RequirementType.CHILD, RequirementType.SIX_CONTAINERS]]);

        townAtNewKasuto = GetLocationByMem(RomMap.EAST_TOWN_OF_NEW_KASUTO_TILE_LOCATION);
        townAtNewKasuto.VanillaCollectable = Collectable.SPELL_SPELL;
        townAtNewKasuto.Collectables = [Collectable.SPELL_SPELL];
        townAtNewKasuto.CollectableRequirements = props.DisableMagicRecs ? Requirements.NONE : new Requirements([RequirementType.SEVEN_CONTAINERS]);

        townAtOldKasuto = GetLocationByMem(RomMap.EAST_TOWN_OF_OLD_KASUTO_TILE_LOCATION);
        townAtOldKasuto.VanillaCollectable = Collectable.THUNDER_SPELL;
        townAtOldKasuto.Collectables = [Collectable.THUNDER_SPELL];
        townAtOldKasuto.CollectableRequirements = props.DisableMagicRecs ? Requirements.NONE : new Requirements([RequirementType.EIGHT_CONTAINERS]);

        waterTile = GetLocationByMem(RomMap.EAST_WATER_TILE_LOCATION);
        waterTile.AccessRequirements = new Requirements([RequirementType.BOOTS]);
        desertTile = GetLocationByMem(RomMap.EAST_DESERT_TILE_LOCATION);

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

        pbagCave1 = GetLocationByMem(RomMap.EAST_CAVE_PBAG1_LOCATION);
        pbagCave2 = GetLocationByMem(RomMap.EAST_CAVE_PBAG2_LOCATION);
        VANILLA_MAP_ADDR = 0x9056;

        //Fake locations that dont correspond to anywhere on the map, but still hold logic and items
        spellTower = new Location(townAtNewKasuto);
        spellTower.VanillaCollectable = Collectable.MAGIC_KEY;
        spellTower.Collectables = [Collectable.MAGIC_KEY];
        spellTower.CollectableRequirements = new Requirements([RequirementType.SPELL]);
        spellTower.Name = "Spell Tower";
        spellTower.CanShuffle = false;
        spellTower.ActualTown = null;
        townAtNewKasuto.Children.Add(spellTower);
        AddLocation(spellTower);

        newKasutoBasement = new Location(townAtNewKasuto);
        newKasutoBasement.VanillaCollectable = Collectable.MAGIC_CONTAINER;
        newKasutoBasement.Collectables = [Collectable.MAGIC_CONTAINER];
        newKasutoBasement.CollectableRequirements = props.NewKasutoBasementRequirement switch
        {
            5 => new Requirements([RequirementType.FIVE_CONTAINERS]),
            6 => new Requirements([RequirementType.SIX_CONTAINERS]),
            7 => new Requirements([RequirementType.SEVEN_CONTAINERS]),
            _ => throw new Exception($"Unsupported New Kasuto basement container count: {props.NewKasutoBasementRequirement}")
        };
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
        daruniaRoof.CollectableRequirements = new Requirements([RequirementType.FAIRY, RequirementType.JUMP]);
        daruniaRoof.Name = "Darunia Roof";
        daruniaRoof.ActualTown = Town.DARUNIA_ROOF;
        daruniaRoof.CanShuffle = false;
        townAtDarunia.Children.Add(daruniaRoof);
        AddLocation(daruniaRoof);

        walkableTerrains = [Terrain.DESERT, Terrain.GRASS, Terrain.FOREST, Terrain.SWAMP, Terrain.GRAVE];
        randomTerrainFilter = [Terrain.DESERT, Terrain.GRASS, Terrain.FOREST, Terrain.SWAMP, Terrain.GRAVE, Terrain.MOUNTAIN, Terrain.WALKABLEWATER];

        biome = props.EastBiome;
        if (biome == Biome.VANILLA || biome == Biome.VANILLA_SHUFFLE || props.EastSize == OverworldSizeOption.LARGE)
        {
            MAP_COLS = 64;
            MAP_ROWS = 75;
            northSouthEncounterSeparator = 46;
        }
        else
        {
            var meta = props.EastSize.GetMeta();
            MAP_COLS = meta.Width;
            MAP_ROWS = meta.Height;
            northSouthEncounterSeparator = MAP_ROWS / 2;
            // TODO: use metadata for num trap tiles etc. to remove for small continents
            int trapTilesToRemove = 0; // must be even
            for (int i = 0; i < trapTilesToRemove; i++)
            {
                var j = r.Next(roadTrapLocations.Count);
                var removeLoc = roadTrapLocations[j];
                RemoveLocations([removeLoc]);
                roadTrapLocations.Remove(removeLoc);
            }

            int passthroughsToRemove = 0;
            for (int i = 0; i < passthroughsToRemove; i++)
            {
                var j = r.Next(passthroughCaveLocations.Count);
                var removeLoc = passthroughCaveLocations[j];
                var removeLocConnected = connections[removeLoc];

                RemoveLocations([removeLoc, removeLocConnected]);
                passthroughCaveLocations.Remove(removeLoc);
                passthroughCaveLocations.Remove(removeLocConnected);
            }
        }

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
        climate = Climates.Create(props.EastClimate);
        climate.SeedTerrainCount = Math.Min(climate.SeedTerrainCount, biome.SeedTerrainLimit());
        climate.DisallowTerrain(props.CanWalkOnWaterWithBoots ? Terrain.WATER : Terrain.WALKABLEWATER);
        //climate.DisallowTerrain(Terrain.LAVA);
        SetVanillaCollectables(props.ReplaceFireWithDash);
    }

    public override bool Terraform(RandomizerProperties props, ROM rom)
    {
        Terrain normalWater = props.CanWalkOnWaterWithBoots ? Terrain.WALKABLEWATER : Terrain.WATER;
        Terrain preplacedWater = props.CanWalkOnWaterWithBoots ? Terrain.PREPLACED_WATER_WALKABLE : Terrain.PREPLACED_WATER;

        foreach (Location location in AllLocations)
        {
            location.CanShuffle = true;
            location.AccessRequirements = location.AccessRequirements.Without([RequirementType.HAMMER, RequirementType.FLUTE]);
            if (location != raft && location != bridge && location != cave1 && location != cave2)
            {
                location.TerrainType = terrains[location.MemAddress];
            }
        }
        if (props.LessImportantLocationsOption != LessImportantLocationsOption.ISOLATE)
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
            Debug.Assert(MAP_ROWS == 75);
            Debug.Assert(MAP_COLS == 64);
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
                    // section uses tuples with the Y+30 offset
                    areasByLocation[section[location.CoordsY30Offset]].Add(GetLocationByPos(location.Pos)!);
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

                desertTile.MapPage = 1;

                Location? desert = GetLocationByMem(RomMap.EAST_MINOR_DESERT_TILE_LOCATION1);
                Location? swamp = GetLocationByMem(RomMap.EAST_MINOR_SWAMP_TILE_LOCATION);
                if(desert == null || swamp == null)
                {
                    throw new ImpossibleException("Unable to find desert/swamp passthrough on east.");
                }

                if (desert.PassThrough != 0)
                {
                    desert.AccessRequirements = desert.AccessRequirements.WithHardRequirement(RequirementType.JUMP);
                }
                else
                {
                    desert.AccessRequirements = desert.AccessRequirements.Without(RequirementType.JUMP);
                }

                if (swamp.PassThrough != 0)
                {
                    swamp.AccessRequirements = swamp.AccessRequirements.WithHardRequirement(RequirementType.FAIRY);
                }
                else
                {
                    swamp.AccessRequirements = swamp.AccessRequirements.Without(RequirementType.FAIRY);
                }

            }
			
            //in vanilla shuffle, post location shuffling, the locations have moved, but the hidden palace spot doesn't
            //so reset the reference
            hiddenKasutoLocation = GetLocationByCoordsY30Offset((81, 61))!;
            hiddenPalaceLocation = GetLocationByCoordsY30Offset((102, 45))!;
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
            int bytesWritten = 2000;
            locationAtGP.CanShuffle = false;
            Terrain riverTerrain = Terrain.MOUNTAIN;
            while (bytesWritten > MAP_SIZE_BYTES)
            {
                //This double initialization for locations for non-vanilla maps is not well written, at some point I should clean this up
                foreach (Location location in AllLocations)
                {
                    location.CanShuffle = true;
                    location.AccessRequirements = location.AccessRequirements.Without([RequirementType.HAMMER, RequirementType.FLUTE]);
                    if (location != raft && location != bridge && location != cave1 && location != cave2)
                    {
                        location.TerrainType = terrains[location.MemAddress];
                    }
                }
                if (props.LessImportantLocationsOption != LessImportantLocationsOption.ISOLATE)
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
                    riverTerrain = preplacedWater;
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
                                    map[i, col] = preplacedWater;
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
                                    map[row, i] = preplacedWater;
                                }
                            }
                            pickedR.Add(row);
                            rows--;
                        }
                    }
                    walkableTerrains = [Terrain.LAVA, Terrain.DESERT, Terrain.GRASS, Terrain.FOREST, Terrain.SWAMP, Terrain.GRAVE];
                    randomTerrainFilter = [Terrain.LAVA, Terrain.DESERT, Terrain.GRASS, Terrain.FOREST, Terrain.SWAMP, Terrain.GRAVE, Terrain.MOUNTAIN, normalWater];

                }
                else if (biome == Biome.CANYON || biome == Biome.DRY_CANYON)
                {
                    riverTerrain = preplacedWater;
                    if (biome == Biome.DRY_CANYON)
                    {
                        riverTerrain = Terrain.DESERT;
                    }
                    //riverT = terrain.lava;
                    walkableTerrains = [Terrain.DESERT, Terrain.GRASS, Terrain.FOREST, Terrain.GRAVE, Terrain.MOUNTAIN];
                    randomTerrainFilter = [Terrain.DESERT, Terrain.GRASS, Terrain.FOREST, Terrain.GRAVE, Terrain.MOUNTAIN, normalWater];


                    DrawCanyon(riverTerrain);
                    walkableTerrains.Remove(Terrain.MOUNTAIN);

                    locationAtGP.CanShuffle = false;


                }
                else if (biome == Biome.VOLCANO)
                {
                    DrawCenterMountain();

                    walkableTerrains = new List<Terrain>() { Terrain.LAVA, Terrain.DESERT, Terrain.GRASS, Terrain.FOREST, Terrain.SWAMP, Terrain.GRAVE };
                    randomTerrainFilter = new List<Terrain> { Terrain.LAVA, Terrain.DESERT, Terrain.GRASS, Terrain.FOREST, Terrain.SWAMP, Terrain.GRAVE, Terrain.MOUNTAIN, normalWater };


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
                    walkableTerrains = [Terrain.DESERT, Terrain.GRASS, Terrain.FOREST, Terrain.SWAMP, Terrain.GRAVE];
                    randomTerrainFilter = [Terrain.DESERT, Terrain.GRASS, Terrain.FOREST, Terrain.SWAMP, Terrain.GRAVE, Terrain.MOUNTAIN, normalWater];
                }
                else
                {
                    walkableTerrains = [Terrain.DESERT, Terrain.GRASS, Terrain.FOREST, Terrain.SWAMP, Terrain.GRAVE];
                    randomTerrainFilter = [Terrain.DESERT, Terrain.GRASS, Terrain.FOREST, Terrain.SWAMP, Terrain.GRAVE, Terrain.MOUNTAIN, normalWater];
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
                //Debug.WriteLine(GetMapDebug());
                debug++;
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
                    DrawOcean(raftDirection, preplacedWater);
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
                    DrawOcean(bridgeDirection, preplacedWater);
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
                //Debug.WriteLine(GetMapDebug());
                randomTerrainFilter.Remove(Terrain.LAVA);
                if (raft != null)
                {
                    bool r = DrawRaft(raftDirection);
                    if (!r)
                    {
                        return false;
                    }
                }

                if (bridge != null)
                {
                    bool b = DrawBridge(bridgeDirection);
                    if (!b)
                    {
                        return false;
                    }
                }

                bool riverDevilBlocksCave = props.RiverDevilBlockerOption == RiverDevilBlockerOption.CAVE;
                bool rockBlock = props.EastRocks && !props.EastRockIsPath;
                PlaceHiddenLocations(props.LessImportantLocationsOption);
                BlockCaves(props.BoulderBlockConnections, riverDevilBlocksCave, rockBlock, [hiddenPalaceLocation, hiddenKasutoLocation, .. valleyOfDeathLocations]);
                bool riverDevilBlocksPath = props.RiverDevilBlockerOption == RiverDevilBlockerOption.PATH;
                rockBlock = props.EastRocks && props.EastRockIsPath;
                if (biome == Biome.VANILLALIKE)
                {
                    ConnectIslands(4, false, Terrain.MOUNTAIN, false, false, false, true, props.CanWalkOnWaterWithBoots, biome);

                    ConnectIslands(3, false, preplacedWater, riverDevilBlocksPath, rockBlock, false, false, props.CanWalkOnWaterWithBoots, biome);

                }
                if (biome == Biome.ISLANDS)
                {
                    ConnectIslands(100, false, riverTerrain, riverDevilBlocksPath, rockBlock, false, true, props.CanWalkOnWaterWithBoots, biome);
                }
                if (biome == Biome.MOUNTAINOUS)
                {
                    ConnectIslands(20, false, riverTerrain, riverDevilBlocksPath, rockBlock, false, true, props.CanWalkOnWaterWithBoots, biome);
                }
                if (biome == Biome.CANYON || biome == Biome.DRY_CANYON)
                {
                    ConnectIslands(15, false, riverTerrain, riverDevilBlocksPath, rockBlock, false, true, props.CanWalkOnWaterWithBoots, biome);

                }

                if(props.RiverDevilBlockerOption == RiverDevilBlockerOption.SIEGE)
                {
                    //Iterate the towns in a random order
                    Location[] towns = [townAtNabooru, townAtDarunia, townAtNewKasuto, townAtOldKasuto];
                    RNG.Shuffle(towns);
                    bool placed = false;
                    foreach (Location location in towns)
                    {
                        //don't pick a town behind hidden kasuto or hidden palace
                        if (location == hiddenKasutoLocation || location == hiddenPalaceLocation)
                        {
                            continue;
                        }
                        //if the town has walkable space in all 4 cardinal directions
                        //It is currently impossible to put towns on the edge of the map but don't trust that
                        if (map[location.Y + 1, location.Xpos].IsWalkable() && map[location.Y, location.Xpos - 1].IsWalkable() &&
                            map[location.Y - 1, location.Xpos].IsWalkable() && map[location.Y, location.Xpos + 1].IsWalkable())
                        {
                            //replace each cardinal adjacency with a river devil
                            map[location.Y + 1, location.Xpos] = Terrain.RIVER_DEVIL;
                            map[location.Y - 1, location.Xpos] = Terrain.RIVER_DEVIL;
                            map[location.Y, location.Xpos - 1] = Terrain.RIVER_DEVIL;
                            map[location.Y, location.Xpos + 1] = Terrain.RIVER_DEVIL;
                            placed = true;
                            break;
                        }
                    }
                    if(!placed)
                    {
                        logger.Trace("No valid candidate towns for river devil siege.");
                        return false;
                    }
                }

                foreach (Location location in AllLocations)
                {
                    if (location.CanShuffle && location.AppearsOnMap)
                    {
                        location.YRaw = 0;
                        location.CanShuffle = false;
                    }
                }
                bytesWritten = WriteMapToRom(rom, false, MAP_ADDR, MAP_SIZE_BYTES, hiddenPalaceLocation.Y, hiddenPalaceLocation.Xpos, props.HiddenPalace, props.HiddenKasuto);
                rom.Put(RomMap.NORTH_SOUTH_SEPARATOR_EAST, (byte)(northSouthEncounterSeparator + 30));
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
            hiddenPalaceLocation.AccessRequirements = hiddenPalaceLocation.AccessRequirements.WithHardRequirement(RequirementType.FLUTE);
            hiddenPalaceLocation.Children.ForEach(i => i.AccessRequirements = i.AccessRequirements.WithHardRequirement(RequirementType.FLUTE));
        }
        if (props.HiddenKasuto)
        {
            rom.UpdateKasuto(hiddenKasutoLocation, townAtNewKasuto, spellTower, biome,
                baseAddr, terrains[hiddenKasutoLocation.MemAddress], props.VanillaShuffleUsesActualTerrain);
            hiddenKasutoLocation.AccessRequirements = hiddenPalaceLocation.AccessRequirements.WithHardRequirement(RequirementType.HAMMER);
            hiddenKasutoLocation.Children.ForEach(i => i.AccessRequirements = i.AccessRequirements.WithHardRequirement(RequirementType.HAMMER));
        }

        WriteMapToRom(rom, true, MAP_ADDR, MAP_SIZE_BYTES, hiddenPalaceLocation.Y, hiddenPalaceLocation.Xpos, props.HiddenPalace, props.HiddenKasuto);


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
        //DM passthrough locations
        List<Location> passthroughLocations = [
            GetLocationByMem(RomMap.EAST_TRAP_LAVA_TILE_LOCATION1),
            GetLocationByMem(RomMap.EAST_TRAP_LAVA_TILE_LOCATION2),
            GetLocationByMem(RomMap.EAST_TRAP_LAVA_TILE_LOCATION3),
        ];
        //Clean them up in case of data leakage (this is not hypothetical)
        passthroughLocations.ForEach(i => i.YRaw = 0);

        //Pick a spot for the center of the GP hole
        int xmin, xmax, ymin, ymax;
        if (biome == Biome.VOLCANO)
        {
            int mapCenterX = MAP_COLS / 2; // 32
            int mapCenterY = MAP_ROWS / 2; // 37
            xmin = mapCenterX - 11;
            xmax = mapCenterX + 9;
            ymin = Math.Max(5, mapCenterY - 15);
            ymax = Math.Min(mapCenterY + 15, MAP_ROWS - 6);
        }
        else
        {
            xmin = 5;
            ymin = 5;
            xmax = MAP_COLS - 6;
            ymax = MAP_COLS - 6;
        }
        int palacex = RNG.Next(xmin, xmax);
        int palacey = RNG.Next(ymin, ymax);

        //Ensure there is enough unallocated space to draw the whole opening
        //Why does this happen only for caldera-shaped biomes?
        if (biome == Biome.VOLCANO || biome == Biome.CANYON || biome == Biome.DRY_CANYON)
        {
            int tries = 0;
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
                            break; // end inner for
                        }
                        if (!placeable) {
                            break; // end outer for
                        }
                    }
                }
                if (++tries == 1000)
                {
                    return false;
                }
            } while (!placeable);
        }

        //Actually draw the center of the GP pocket
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
        locationAtGP.Y = palacey;
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
                            Location? location = GetLocationByCoordsNoOffset((starty + i, startx));
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
                            Location? location = GetLocationByCoordsNoOffset((starty, startx + i));
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
                            Location? l = GetLocationByCoordsNoOffset((starty - i, startx));
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
                            Location? l = GetLocationByCoordsNoOffset((starty, startx - i));
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
                        if (GetLocationByCoordsNoOffset((starty + deltay, startx + deltax)) != null)
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
                        if (ValidTrapTilePosition(new IntVector2(startx, starty)) != null)
                        {
                                f.Xpos = startx;
                                f.Y = starty;
                                f.CanShuffle = false;
                                forcedPlaced--;
                                forced++;
                            }
                        }
                    else if (adjust > 0)
                    {
                        bool isCanyon = biome == Biome.CANYON || biome == Biome.DRY_CANYON;
                        if (isHorizontal != isCanyon) // exactly one of isHorizontal or isCanyon is true
                        {
                            if (ValidTrapTilePosition(new IntVector2(startx, starty - 1)) != null)
                            {
                                f.Xpos = startx;
                                f.Y = starty - 1;
                                f.CanShuffle = false;
                                forcedPlaced--;
                                forced++;
                            }
                        }
                        else
                        {
                            if (ValidTrapTilePosition(new IntVector2(startx - 1, starty)) != null)
                            {
                                f.Xpos = startx - 1;
                                f.Y = starty;
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
                            if (ValidTrapTilePosition(new IntVector2(startx, starty + 1)) != null)
                            {
                                f.Xpos = startx;
                                f.Y = starty + 1;
                                f.CanShuffle = false;
                                forcedPlaced--;
                                forced++;
                            }
                        }
                        else
                        {
                            if (ValidTrapTilePosition(new IntVector2(startx + 1, starty)) != null)
                            {
                                f.Xpos = startx + 1;
                                f.Y = starty;
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
                    if (vodcave1.Y < MAP_ROWS && vodcave1.Xpos < MAP_COLS)
                    {
                        map[vodcave1.Y, vodcave1.Xpos] = Terrain.MOUNTAIN;
                    }
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
                    vodcave1.Y = starty;

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
                    if (vodcave2.Y < MAP_ROWS && vodcave2.Xpos < MAP_COLS)
                    {
                        map[vodcave2.Y, vodcave2.Xpos] = Terrain.MOUNTAIN;
                    }
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
                    vodcave2.Y = starty;
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
                    if (GetLocationByCoordsNoOffset((starty, startx + deltax)) != null)
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
                    if (GetLocationByCoordsNoOffset((starty + deltay, startx)) != null)
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
        hiddenKasutoLocation.AccessRequirements = hiddenKasutoLocation.AccessRequirements.WithHardRequirement(RequirementType.HAMMER);
        unimportantLocs.Remove(hiddenKasutoLocation);
        //hkLoc.CanShuffle = false;
        //map[hkLoc.Y, hkLoc.Xpos] = terrain.forest;
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
            //#124 - Hidden palace and hidden kasuto on the same X-coordinate causes a wrong warp when leaving hidden palace
            if (hiddenKasuto && xpos == hiddenKasutoLocation.Xpos)
            {
                continue;
            }
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
        //map[hpLoc.Y, hpLoc.Xpos] = map[hpLoc.Y + 1, hpLoc.Xpos];
        hiddenPalaceLocation.Xpos = xpos;
        hiddenPalaceLocation.Y = ypos + 2;
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

    public override void UpdateVisit(List<RequirementType> requireables)
    {
        UpdateReachable(requireables);

        foreach (Location location in AllLocations)
        {
            if (location.Y > 0 && visitation[location.Y, location.Xpos])
            {
                if(location.AccessRequirements.AreSatisfiedBy(requireables))
                {
                    location.Reachable = true;
                    if (connections.ContainsKey(location) && location.ConnectionRequirements.AreSatisfiedBy(requireables))
                    {
                        Location connectedLocation = connections[location];
                        connectedLocation.Reachable = true;
                        visitation[connectedLocation.Y, connectedLocation.Xpos] = true;
                    }
                }
            }
        }
    }

    private double ComputeDistance(Location l, Location l2)
    {
        return Math.Sqrt(Math.Pow(l.Xpos - l2.Xpos, 2) + Math.Pow(l.Y - l2.Y, 2));
    }

    private void BlockCaves(bool connectionsCanBeBlocked, bool riverDevilBlocks, bool rockBlock)
    {
        BlockCaves(connectionsCanBeBlocked, riverDevilBlocks, rockBlock, []);
    }

    private void BlockCaves(bool connectionsCanBeBlocked, bool riverDevilBlocks, bool rockBlock, 
        IEnumerable<Location> disallowedLocations)
    {
        if(!riverDevilBlocks && !rockBlock) { return; }

        bool riverDevilPlaced = !riverDevilBlocks;
        bool rockPlaced = !rockBlock;

        Location? riverDevilPlacedLocation = null;

        List<Location> availableCaves = [.. Locations[Terrain.CAVE]];
        availableCaves.FisherYatesShuffle(RNG);

        foreach (var cave in availableCaves)
        {
            if (riverDevilPlaced && rockPlaced) { break; }

            if (disallowedLocations.Contains(cave)) { continue; }

            if (!connectionsCanBeBlocked)
            {
                bool isConnectorCave = !connections.ContainsKey(cave) && cave != cave1 && cave != cave2;
                if (isConnectorCave) { continue; }
            }

            if (riverDevilPlacedLocation != null &&
                int.Abs(riverDevilPlacedLocation.Xpos - cave.Xpos) < 4 &&
                int.Abs(riverDevilPlacedLocation.Y - cave.Y) < 4)
            {
                continue;
            }

            Terrain blockerTerrain = riverDevilPlaced ? Terrain.ROCK : Terrain.RIVER_DEVIL;

            var dir = ValidBlockingCavePosition(cave.Pos);
            if (dir != null)
                {
                PlaceCaveBlocker(cave, dir.Value, blockerTerrain);
                if (!riverDevilPlaced)
                    {
                    riverDevilPlaced = true;
                    riverDevilPlacedLocation = cave;
                }
                else
                {
                    rockPlaced = true;
                }
            }
        }
    }

    private void DrawMountains(bool useRiverDevil)
    {
        bool placedSpider = !useRiverDevil;

        /*readonly*/ int mountStartY1 = RNG.Next(MAP_COLS / 3 - 10, MAP_COLS / 3 + 10);
        /*readonly*/ int mountEndY1 = RNG.Next(MAP_COLS / 3 - 10, MAP_COLS / 3 + 10);
        /*readonly*/ int endMountMargin1 = RNG.Next(2, 8);
        /*readonly*/ int endMountEndRight1 = MAP_COLS - endMountMargin1;
        int x = 0;
        int y = mountStartY1;
        int roadEncounters = 0;
        map[mountStartY1, 0] = Terrain.MOUNTAIN;
        while (x != endMountEndRight1 || y != mountEndY1)
        {
            if (Math.Abs(x - endMountEndRight1) >= Math.Abs(y - mountEndY1))
            {
                if (x > endMountEndRight1)
                {
                    x--;
                }
                else
                {
                    x++;
                }
            }
            else
            {
                if (y > mountEndY1)
                {
                    y--;
                }
                else
                {
                    y++;
                }
            }
            if (x != endMountEndRight1 || y != mountEndY1)
            {
                if (map[y, x] == Terrain.NONE)
                {
                    map[y, x] = Terrain.MOUNTAIN;
                }
                else if (map[y, x] == Terrain.ROAD)
                {
                    if (!placedSpider)
                    {
                        map[y, x] = Terrain.RIVER_DEVIL;
                        placedSpider = true;
                    }
                    else if (ValidTrapTilePosition(new IntVector2(x, y)) != null)
                    {
                        if (roadEncounters == 0 && roadTrapLocations.Count > 0)
                        {
                            Location roadEnc = roadTrapLocations[0];
                            roadEnc.Xpos = x;
                            roadEnc.Y = y;
                            roadEnc.CanShuffle = false;
                            roadEncounters++;
                        }
                        else if (roadEncounters == 1 && roadTrapLocations.Count > 1)
                        {
                            Location roadEnc = roadTrapLocations[1];
                            roadEnc.Xpos = x;
                            roadEnc.Y = y;
                            roadEnc.CanShuffle = false;
                            roadEncounters++;
                        }
                        else if (roadEncounters == 2 && roadTrapLocations.Count > 2)
                        {
                            Location roadEnc = roadTrapLocations[2];
                            roadEnc.Xpos = x;
                            roadEnc.Y = y;
                            roadEnc.CanShuffle = false;
                            roadEncounters++;
                        }
                        else if (roadEncounters == 3 && roadTrapLocations.Count > 3)
                        {
                            Location roadEnc = roadTrapLocations[3];
                            roadEnc.Xpos = x;
                            roadEnc.Y = y;
                            roadEnc.CanShuffle = false;
                            roadEncounters++;
                        }
                    }
                }
            }
        }

        /*readonly*/ int mountStartY2 = RNG.Next(MAP_COLS * 2 / 3 - 10, MAP_COLS * 2 / 3 + 10);
        map[mountStartY2, 0] = Terrain.MOUNTAIN;

        /*readonly*/ int mountEndY2 = RNG.Next(MAP_COLS * 2 / 3 - 10, MAP_COLS * 2 / 3 + 10);
        /*readonly*/ int endMountMargin2 = RNG.Next(2, 8);
        /*readonly*/ int endMountEndRight2 = MAP_COLS - endMountMargin2;

        x = 0;
        y = mountStartY2;
        while (x != endMountEndRight2 || y != mountEndY2)
        {
            if (Math.Abs(x - endMountEndRight2) >= Math.Abs(y - mountEndY2))
            {
                if (x > endMountEndRight2)
                {
                    x--;
                }
                else
                {
                    x++;
                }
            }
            else
            {
                if (y > mountEndY2)
                {
                    y--;
                }
                else
                {
                    y++;
                }
            }
            if (x != endMountEndRight2 || y != mountEndY2)
            {
                if (map[y, x] == Terrain.NONE)
                {
                    map[y, x] = Terrain.MOUNTAIN;
                }
                else if (map[y, x] == Terrain.ROAD)
                {
                    if (!placedSpider)
                    {
                        map[y, x] = Terrain.RIVER_DEVIL;
                        placedSpider = true;
                    }
                    else if (ValidTrapTilePosition(new IntVector2(x, y)) != null)
                    {
                        if (roadEncounters == 0)
                        {
                            Location roadEnc = GetLocationByMem(RomMap.EAST_TRAP_ROAD_TILE_LOCATION1);
                            roadEnc.Xpos = x;
                            roadEnc.Y = y;
                            roadEnc.CanShuffle = false;
                            roadEncounters++;
                        }
                        else if (roadEncounters == 1)
                        {
                            Location roadEnc = GetLocationByMem(RomMap.EAST_TRAP_ROAD_TILE_LOCATION2);
                            roadEnc.Xpos = x;
                            roadEnc.Y = y;
                            roadEnc.CanShuffle = false;
                            roadEncounters++;
                        }
                        else if (roadEncounters == 2)
                        {
                            Location roadEnc = GetLocationByMem(RomMap.EAST_TRAP_ROAD_TILE_LOCATION3);
                            roadEnc.Xpos = x;
                            roadEnc.Y = y;
                            roadEnc.CanShuffle = false;
                            roadEncounters++;
                        }
                        else if (roadEncounters == 3)
                        {
                            Location roadEnc = GetLocationByMem(RomMap.EAST_TRAP_ROAD_TILE_TO_VOD_LOCATION);
                            roadEnc.Xpos = x;
                            roadEnc.Y = y;
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
            pendingCoordinates.Add((location.Y, location.Xpos));
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
            Location? here = unreachedLocations.FirstOrDefault(location => location.Y == y && location.Xpos == x);
            if (here != null)
            {
                //it's reachable
                unreachedLocations.Remove(here);
                //if it's a connection cave, add the exit(s) to the pending locations
                if (connections.ContainsKey(here))
                {
                    pendingCoordinates.Add((connections[here].Y, connections[here].Xpos));
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
            if (!requiredLocations.Contains(location) && location.YRaw != 0)
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