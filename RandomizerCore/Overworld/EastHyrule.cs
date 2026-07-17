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

    private readonly SortedDictionary<LocationID, Terrain> terrains = new()
    {
        { LocationID.EAST_MINOR_FOREST_BY_NABOORU, Terrain.FOREST },
        { LocationID.EAST_MINOR_FOREST_BY_P6, Terrain.FOREST },
        { LocationID.EAST_TRAP_ROAD1, Terrain.ROAD },
        { LocationID.EAST_TRAP_ROAD2, Terrain.ROAD },
        { LocationID.EAST_TRAP_ROAD3, Terrain.ROAD },
        { LocationID.EAST_TRAP_ROAD_TO_VOD, Terrain.ROAD },
        { LocationID.EAST_BRIDGE_TO_P6, Terrain.BRIDGE },
        { LocationID.EAST_BRIDGE_TO_KASUTO, Terrain.BRIDGE },
        { LocationID.EAST_TRAP_DESERT1, Terrain.DESERT },
        { LocationID.EAST_TRAP_DESERT2, Terrain.DESERT },
        { LocationID.EAST_WATER, Terrain.WALKABLEWATER },
        { LocationID.EAST_CAVE_NABOORU_PASSTHROUGH_SOUTH, Terrain.CAVE },
        { LocationID.EAST_CAVE_NABOORU_PASSTHROUGH_NORTH, Terrain.CAVE },
        { LocationID.EAST_CAVE_PBAG1, Terrain.CAVE },
        { LocationID.EAST_CAVE_PBAG2, Terrain.CAVE },
        { LocationID.EAST_CAVE_NEW_KASUTO_PASSTHROUGH_WEST, Terrain.CAVE },
        { LocationID.EAST_CAVE_NEW_KASUTO_PASSTHROUGH_EAST, Terrain.CAVE },
        { LocationID.EAST_CAVE_VOD_PASSTHROUGH2_START, Terrain.CAVE },
        { LocationID.EAST_CAVE_VOD_PASSTHROUGH2_END, Terrain.CAVE },
        { LocationID.EAST_CAVE_VOD_PASSTHROUGH1_END, Terrain.CAVE },
        { LocationID.EAST_CAVE_VOD_PASSTHROUGH1_START, Terrain.CAVE },
        { LocationID.EAST_MINOR_SWAMP, Terrain.SWAMP },
        { LocationID.EAST_BUGGED_MINOR_LAVA, Terrain.LAVA },
        { LocationID.EAST_MINOR_DESERT1, Terrain.DESERT },
        { LocationID.EAST_MINOR_DESERT2, Terrain.DESERT },
        { LocationID.EAST_MINOR_DESERT3, Terrain.DESERT },
        { LocationID.EAST_DESERT, Terrain.DESERT },
        { LocationID.EAST_MINOR_FOREST2, Terrain.FOREST },
        { LocationID.EAST_MINOR_LAVA1, Terrain.LAVA },
        { LocationID.EAST_MINOR_LAVA2, Terrain.LAVA },
        { LocationID.EAST_TRAP_LAVA1, Terrain.LAVA },
        { LocationID.EAST_TRAP_LAVA2, Terrain.LAVA },
        { LocationID.EAST_TRAP_LAVA3, Terrain.LAVA },
        { LocationID.EAST_BRIDGE_TO_MI, Terrain.BRIDGE },
        { LocationID.EAST_RAFT_TO_WEST, Terrain.BRIDGE },
        { LocationID.EAST_TOWN_NABOORU, Terrain.TOWN },
        { LocationID.EAST_TOWN_DARUNIA, Terrain.TOWN },
        { LocationID.EAST_TOWN_NEW_KASUTO, Terrain.TOWN },
        { LocationID.EAST_TOWN_OLD_KASUTO, Terrain.TOWN },
        { LocationID.EAST_PALACE5, Terrain.PALACE },
        { LocationID.EAST_PALACE6, Terrain.PALACE },
        { LocationID.EAST_GREAT_PALACE, Terrain.PALACE },
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
        continentId = Continent.EAST;
        baseAddr = RomMap.ContinentLocationBases[continentId];

        List<Location> passthroughCaveLocations = [
            .. rom.LoadLocations(LocationID.EAST_CAVE_NABOORU_PASSTHROUGH_SOUTH, 2, terrains),
            .. rom.LoadLocations(LocationID.EAST_CAVE_NEW_KASUTO_PASSTHROUGH_WEST, 2, terrains),
        ];
        List<Location> passthroughVodLocations = [
            .. rom.LoadLocations(LocationID.EAST_CAVE_VOD_PASSTHROUGH2_START, 4, terrains),
        ];
        roadTrapLocations = [
            .. rom.LoadLocations(LocationID.EAST_TRAP_ROAD1, 4, terrains),
        ];
        List<Location> desertTrapLocations = [
            .. rom.LoadLocations(LocationID.EAST_TRAP_DESERT1, 2, terrains),
        ];

        List<Location> locations =
        [
            .. passthroughCaveLocations,
            .. passthroughVodLocations,
            .. roadTrapLocations,
            .. desertTrapLocations,
            .. rom.LoadLocations(LocationID.EAST_MINOR_FOREST_BY_NABOORU, 2, terrains),
            .. rom.LoadLocations(LocationID.EAST_BRIDGE_TO_P6, 2, terrains),
            .. rom.LoadLocations(LocationID.EAST_WATER, 1, terrains),
            .. rom.LoadLocations(LocationID.EAST_MINOR_SWAMP, 1, terrains),
            .. rom.LoadLocations(LocationID.EAST_CAVE_PBAG1, 2, terrains),
            .. rom.LoadLocations(LocationID.EAST_MINOR_DESERT1, 10, terrains),
            .. rom.LoadLocations(LocationID.EAST_TOWN_NABOORU, 1, terrains),
            .. rom.LoadLocations(LocationID.EAST_TOWN_DARUNIA, 1, terrains),
            .. rom.LoadLocations(LocationID.EAST_TOWN_NEW_KASUTO, 1, terrains),
            .. rom.LoadLocations(LocationID.EAST_TOWN_OLD_KASUTO, 4, terrains),
        ];
        locations.ForEach(AddLocation);

        valleyOfDeathLocations = [
            GetLocation(LocationID.EAST_CAVE_VOD_PASSTHROUGH1_END),
            GetLocation(LocationID.EAST_CAVE_VOD_PASSTHROUGH1_START),
            GetLocation(LocationID.EAST_CAVE_VOD_PASSTHROUGH2_END),
            GetLocation(LocationID.EAST_CAVE_VOD_PASSTHROUGH2_START),
            GetLocation(LocationID.EAST_TRAP_LAVA1),
            GetLocation(LocationID.EAST_TRAP_LAVA2),
            GetLocation(LocationID.EAST_TRAP_LAVA3),
        ];

        CreateConnections();

        //Palaces
        locationAtPalace5 = GetLocation(LocationID.EAST_PALACE5);
        locationAtPalace5.PalaceNumber = 5;
        locationAtPalace5.CollectableRequirements = DEFAULT_PALACE_REQUIREMENTS;

        locationAtPalace6 = GetLocation(LocationID.EAST_PALACE6);
        locationAtPalace6.PalaceNumber = 6;
        locationAtPalace6.CollectableRequirements = DEFAULT_PALACE_REQUIREMENTS;

        locationAtGP = GetLocation(LocationID.EAST_GREAT_PALACE);
        locationAtGP.PalaceNumber = 7;
        locationAtGP.CollectableRequirements = DEFAULT_PALACE_REQUIREMENTS; // the same as this gets shuffled with regular palaces
        locationAtGP.VanillaCollectable = Collectable.DO_NOT_USE;
        locationAtGP.Collectables = [];

        //Towns
        townAtNabooru = GetLocation(LocationID.EAST_TOWN_NABOORU);
        townAtNabooru.VanillaCollectable = props.ReplaceFireWithDash ? Collectable.DASH_SPELL : Collectable.FIRE_SPELL;
        townAtNabooru.Collectables = props.ReplaceFireWithDash ? [Collectable.DASH_SPELL] : [Collectable.FIRE_SPELL];
        townAtNabooru.CollectableRequirements = props.DisableMagicRecs ?
            new Requirements([RequirementType.WATER])
            : new Requirements([], [[RequirementType.WATER, RequirementType.FIVE_CONTAINERS]]);

        townAtDarunia = GetLocation(LocationID.EAST_TOWN_DARUNIA);
        townAtDarunia.VanillaCollectable = Collectable.REFLECT_SPELL;
        townAtDarunia.Collectables = [Collectable.REFLECT_SPELL];
        townAtDarunia.CollectableRequirements = props.DisableMagicRecs ?
            new Requirements([RequirementType.CHILD])
            : new Requirements([], [[RequirementType.CHILD, RequirementType.SIX_CONTAINERS]]);

        townAtNewKasuto = GetLocation(LocationID.EAST_TOWN_NEW_KASUTO);
        townAtNewKasuto.VanillaCollectable = Collectable.SPELL_SPELL;
        townAtNewKasuto.Collectables = [Collectable.SPELL_SPELL];
        townAtNewKasuto.CollectableRequirements = props.DisableMagicRecs ? Requirements.NONE : new Requirements([RequirementType.SEVEN_CONTAINERS]);

        townAtOldKasuto = GetLocation(LocationID.EAST_TOWN_OLD_KASUTO);
        townAtOldKasuto.VanillaCollectable = Collectable.THUNDER_SPELL;
        townAtOldKasuto.Collectables = [Collectable.THUNDER_SPELL];
        townAtOldKasuto.CollectableRequirements = props.DisableMagicRecs ? Requirements.NONE : new Requirements([RequirementType.EIGHT_CONTAINERS]);

        waterTile = GetLocation(LocationID.EAST_WATER);
        waterTile.AccessRequirements = new Requirements([RequirementType.BOOTS]);
        desertTile = GetLocation(LocationID.EAST_DESERT);

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

        pbagCave1 = GetLocation(LocationID.EAST_CAVE_PBAG1);
        pbagCave2 = GetLocation(LocationID.EAST_CAVE_PBAG2);
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
        if (biome.UsesVanillaMap() || props.EastSize == OverworldSizeOption.LARGE)
        {
            MapColumns = 64;
            MapRows = 75;
            northSouthEncounterSeparator = 46;
        }
        else
        {
            var meta = props.EastSize.GetMeta();
            MapColumns = meta.Width;
            MapRows = meta.Height;
            northSouthEncounterSeparator = MapRows / 2;
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
        townAtNewKasuto.IsExternalWorld = true;
        locationAtPalace6.IsExternalWorld = true;
        hiddenPalaceLocation = locationAtPalace6;
        hiddenKasutoLocation = townAtNewKasuto;

        //Climate filtering
        climate = Climates.Create(continentId, props.EastClimate);
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
            location.IsPassthrough = location.WasPassthrough;
            location.ResetCoords();
            location.AccessRequirements = location.AccessRequirements.Without([RequirementType.HAMMER, RequirementType.FLUTE]);
            if (location != raft && location != bridge && location != cave1 && location != cave2)
            {
                location.TerrainType = terrains[location.ID];
            }
        }
        if (props.LessImportantLocationsOption != LessImportantLocationsOption.ISOLATE)
        {
            unimportantLocs =
            [
                GetLocation(LocationID.EAST_MINOR_FOREST_BY_NABOORU),
                GetLocation(LocationID.EAST_MINOR_FOREST_BY_P6),
                GetLocation(LocationID.EAST_MINOR_SWAMP),
                GetLocation(LocationID.EAST_MINOR_DESERT1),
                GetLocation(LocationID.EAST_MINOR_DESERT2),
                GetLocation(LocationID.EAST_MINOR_DESERT3),
                GetLocation(LocationID.EAST_MINOR_FOREST2),
                GetLocation(LocationID.EAST_MINOR_LAVA1),
                GetLocation(LocationID.EAST_MINOR_LAVA2)
            ];
        }

        if (biome.UsesVanillaMap())
        {
            Debug.Assert(MapRows == 75);
            Debug.Assert(MapColumns == 64);
            map = new OverworldMap(rom.ReadVanillaMap(rom, VANILLA_MAP_ADDR, MapRows, MapColumns));

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
                    areasByLocation[section[location.CoordsY30Offset]].Add(GetLocationAt(location.Pos)!);
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
                if (!props.LegacyVanillaShuffledLocations)
                {
                    foreach (Location location in AllLocations)
                    {
                        map[location.Y, location.Xpos] = location.TerrainType;
                    }
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
                        location.TerrainType = terrains[location.ID];
                    }
                }
                if (props.LessImportantLocationsOption != LessImportantLocationsOption.ISOLATE)
                {
                    unimportantLocs = new List<Location>
                    {
                        GetLocation(LocationID.EAST_MINOR_FOREST_BY_NABOORU),
                        GetLocation(LocationID.EAST_MINOR_FOREST_BY_P6),
                        GetLocation(LocationID.EAST_MINOR_SWAMP),
                        GetLocation(LocationID.EAST_MINOR_DESERT1),
                        GetLocation(LocationID.EAST_MINOR_DESERT2),
                        GetLocation(LocationID.EAST_MINOR_DESERT3),
                        GetLocation(LocationID.EAST_MINOR_FOREST2),
                        GetLocation(LocationID.EAST_MINOR_LAVA1),
                        GetLocation(LocationID.EAST_MINOR_LAVA2)
                    };
                }

                map = new OverworldMap(MapRows, MapColumns);

                for (int i = 0; i < MapRows; i++)
                {
                    for (int j = 0; j < MapColumns; j++)
                    {
                        map[i, j] = Terrain.NONE;
                    }
                }

                if (biome == Biome.ISLANDS)
                {
                    riverTerrain = preplacedWater;
                    for (int i = 0; i < MapColumns; i++)
                    {
                        map[0, i] = preplacedWater;
                        map[MapRows - 1, i] = preplacedWater;
                    }

                    for (int i = 0; i < MapRows; i++)
                    {
                        map[i, 0] = preplacedWater;
                        map[i, MapColumns - 1] = preplacedWater;
                    }
                    MakeValleyOfDeath();
                    int cols = RNG.Next(2, 4);
                    int rows = RNG.Next(2, 4);
                    List<int> pickedC = [];
                    List<int> pickedR = [];

                    while (cols > 0)
                    {
                        int col = RNG.Next(1, MapColumns - 1);
                        if (!pickedC.Contains(col))
                        {
                            for (int i = 0; i < MapRows; i++)
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
                        int row = RNG.Next(1, MapRows - 1);
                        if (!pickedR.Contains(row))
                        {
                            for (int i = 0; i < MapColumns; i++)
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
                    for (int i = 0; i < MapColumns; i++)
                    {
                        map[0, i] = Terrain.MOUNTAIN;
                        map[MapRows - 1, i] = Terrain.MOUNTAIN;
                    }

                    for (int i = 0; i < MapRows; i++)
                    {
                        map[i, 0] = Terrain.MOUNTAIN;
                        map[i, MapColumns - 1] = Terrain.MOUNTAIN;
                    }
                    MakeValleyOfDeath();

                    int cols = RNG.Next(2, 4);
                    int rows = RNG.Next(2, 4);
                    List<int> pickedC = new List<int>();
                    List<int> pickedR = new List<int>();

                    while (cols > 0)
                    {
                        int col = RNG.Next(10, MapColumns - 11);
                        if (!pickedC.Contains(col))
                        {
                            for (int i = 0; i < MapRows; i++)
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
                        int row = RNG.Next(10, MapRows - 11);
                        if (!pickedR.Contains(row))
                        {
                            for (int i = 0; i < MapColumns; i++)
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
                townAtNewKasuto, spellTower, !props.LegacyVanillaShuffledLocations);
            hiddenPalaceLocation.AccessRequirements = hiddenPalaceLocation.AccessRequirements.WithHardRequirement(RequirementType.FLUTE);
            hiddenPalaceLocation.Children.ForEach(i => i.AccessRequirements = i.AccessRequirements.WithHardRequirement(RequirementType.FLUTE));
        }
        if (props.HiddenKasuto)
        {
            rom.UpdateKasuto(hiddenKasutoLocation, townAtNewKasuto, spellTower, biome,
                baseAddr, terrains[hiddenKasutoLocation.ID], !props.LegacyVanillaShuffledLocations);
            hiddenKasutoLocation.AccessRequirements = hiddenPalaceLocation.AccessRequirements.WithHardRequirement(RequirementType.HAMMER);
            hiddenKasutoLocation.Children.ForEach(i => i.AccessRequirements = i.AccessRequirements.WithHardRequirement(RequirementType.HAMMER));
        }

        WriteMapToRom(rom, true, MAP_ADDR, MAP_SIZE_BYTES, hiddenPalaceLocation.Y, hiddenPalaceLocation.Xpos, props.HiddenPalace, props.HiddenKasuto);


        visitation = new bool[MapRows, MapColumns];
        for (int i = 0; i < MapRows; i++)
        {
            for (int j = 0; j < MapColumns; j++)
            {
                visitation[i, j] = false;
            }
        }


        return true;
    }

    public bool MakeValleyOfDeath()
    {
        bool isCanyon = biome is Biome.CANYON or Biome.DRY_CANYON;
        bool isCalderaLike = biome is Biome.CANYON or Biome.DRY_CANYON or Biome.VOLCANO;
        bool horizontalPath = isHorizontal ^ isCanyon;

        // VoD passthrough locations
        List<Location> passthroughLocations = [
            GetLocation(LocationID.EAST_TRAP_LAVA1),
            GetLocation(LocationID.EAST_TRAP_LAVA2),
            GetLocation(LocationID.EAST_TRAP_LAVA3),
        ];
        //Clean them up in case of data leakage (this is not hypothetical)
        passthroughLocations.ForEach(i => i.YRaw = 0);

        // Pick a spot for the center of the GP hole
        int xmin, xmax, ymin, ymax;
        if (biome == Biome.VOLCANO)
        {
            int mapCenterX = MapColumns / 2; // 32
            int mapCenterY = MapRows / 2; // 37
            xmin = mapCenterX - 11;
            xmax = mapCenterX + 9;
            ymin = Math.Max(5, mapCenterY - 15);
            ymax = Math.Min(mapCenterY + 15, MapRows - 6);
        }
        else
        {
            xmin = 5;
            ymin = 5;
            xmax = MapColumns - 6;
            ymax = MapColumns - 6;
        }
        IntVector2 palacePos = new(RNG.Next(xmin, xmax), RNG.Next(ymin, ymax));

        // Ensure there is enough unallocated space to draw the whole opening
        if (isCalderaLike)
        {
            int tries = 0;
            var offsets =
                Enumerable.Range(-4, 9)
                .SelectMany(dy => Enumerable.Range(-4, 9)
                .Select(dx => new IntVector2(dx, dy)));

            bool allMountains;
            do
            {
                if (tries++ == 1000) { return false; }
                palacePos = IntVector2.Random(RNG, xmin, xmax, ymin, ymax);
                allMountains = offsets.All(offset => map[palacePos + offset] == Terrain.MOUNTAIN);
            } while (!allMountains);
        }

        // VoD center
        for (int y = -3; y <= 3; y++)
        {
            for (int x = -3; x <= 3; x++)
            {
                map[palacePos + new IntVector2(x, y)] = Terrain.LAVA;
            }
        }
        List<IntVector2> vodBorderPositions = [
            .. Enumerable.Range(0, 9).Select(i => palacePos + new IntVector2(-4, -4) + i * IntVector2.EAST),
            .. Enumerable.Range(1, 7).Select(i => palacePos + new IntVector2(4, -4) + i * IntVector2.SOUTH),
            .. Enumerable.Range(0, 9).Select(i => palacePos + new IntVector2(-4, 4) + i * IntVector2.EAST),
            .. Enumerable.Range(1, 7).Select(i => palacePos + new IntVector2(-4, -4) + i * IntVector2.SOUTH),
            .. new IntVector2[] { new(-3, -3), new(3, -3), new(-3, 3), new(3, 3) }.Select(p => palacePos + p)
        ];
        foreach (var borderPos in vodBorderPositions)
        {
            map[borderPos] = Terrain.MOUNTAIN;
        }
        map[palacePos] = Terrain.PALACE;
        locationAtGP.Pos = palacePos;
        locationAtGP.CanShuffle = false;

        int length = isCalderaLike ? 20 : RNG.Next(5, 16);


        // Initial delta direction.
        // Non-canyon: horizontalPath uses X axis, !horizontalPath uses Y axis with >
        // Canyon: horizontalPath uses X axis,     !horizontalPath uses Y axis with >=
        IntVector2 delta = horizontalPath
            ? (palacePos.X > MapColumns / 2 ? IntVector2.WEST : IntVector2.EAST)
            : ((isCanyon ? palacePos.Y >= MapRows / 2 : palacePos.Y > MapRows / 2) // TODO: verify that these can be the same
                ? IntVector2.NORTH : IntVector2.SOUTH);

        IntVector2 currentPos = palacePos + 4 * delta;
        bool cavePlaced = false;
        Location? vodcave1, vodcave2, vodcave3, vodcave4;
        canyonShort = RNG.NextDouble() > .5;
        if (canyonShort)
        {
            vodcave1 = GetLocation(LocationID.EAST_CAVE_VOD_PASSTHROUGH2_START);
            vodcave2 = GetLocation(LocationID.EAST_CAVE_VOD_PASSTHROUGH2_END);
            vodcave3 = GetLocation(LocationID.EAST_CAVE_VOD_PASSTHROUGH1_END);
            vodcave4 = GetLocation(LocationID.EAST_CAVE_VOD_PASSTHROUGH1_START);
        }
        else
        {
            vodcave1 = GetLocation(LocationID.EAST_CAVE_VOD_PASSTHROUGH1_END);
            vodcave2 = GetLocation(LocationID.EAST_CAVE_VOD_PASSTHROUGH1_START);
            vodcave3 = GetLocation(LocationID.EAST_CAVE_VOD_PASSTHROUGH2_START);
            vodcave4 = GetLocation(LocationID.EAST_CAVE_VOD_PASSTHROUGH2_END);
        }

        int traps = 0;
        int vodRoutes = biome == Biome.VOLCANO ? RNG.Next(1, 3) : 1;
        IntVector2 forwardDir = horizontalPath ? IntVector2.EAST : IntVector2.SOUTH;
        IntVector2 sideDir = horizontalPath ? IntVector2.SOUTH : IntVector2.EAST;

        void TerraformVodCave(IntVector2 p, IntVector2 delta)
        {
            map[p] = Terrain.CAVE;
            map[p + delta] = Terrain.MOUNTAIN;
            map[p - sideDir] = Terrain.MOUNTAIN;
            map[p + sideDir] = Terrain.MOUNTAIN;
        }

        void TerraformVodCavePair(IntVector2 first, IntVector2 second, IntVector2 delta)
        {
            if (WithinMapBounds(vodcave1.Pos))
            {
                map[vodcave1.Pos] = Terrain.MOUNTAIN;
            }
            if (WithinMapBounds(vodcave2.Pos))
            {
                map[vodcave2.Pos] = Terrain.MOUNTAIN;
            }
            TerraformVodCave(first, delta);
            vodcave1.Pos = first;
            vodcave1.CanShuffle = false;
            TerraformVodCave(second, -delta);
            vodcave2.Pos = second;
            vodcave2.CanShuffle = false;
        }

        // Rolls a zig-zag offset that stays within map bounds (1-tile margin).
        int RollAdjust(IntVector2 pos, IntVector2 d, int minA, int maxA)
        {
            int adj;
            do
            {
                adj = RNG.Next(minA, maxA);
            }
            while (!WithinMapBounds(pos + adj * d, 1));
            return adj;
        }

        // Draws a perpendicular lava segment (when zig-zagging), bordered by mountains.
        // Returns false if a non-shuffleable location blocks the segment.
        bool DrawPerpendicularLavaSegment(IntVector2 movement)
        {
            IntVector2 step = movement.Normalize();
            int steps = movement.ManhattanLength;

            if (!isCalderaLike)
            {
                map[currentPos - step] = Terrain.MOUNTAIN;
            }

            for (int i = 0; i <= steps; i++)
            {
                IntVector2 pos = currentPos + i * step;
                if (!WithinMapBounds(pos)) { break; }

                map[pos] = Terrain.LAVA;

                if (!isCalderaLike)
                {
                    foreach (IntVector2 wallPos in new[] { pos - forwardDir, pos + forwardDir })
                    {
                        var terrain = map[wallPos];
                        if (terrain != Terrain.LAVA && terrain != Terrain.CAVE)
                        {
                            map[wallPos] = Terrain.MOUNTAIN;
                        }
                    }
                }

                if (GetLocationAt(pos) is Location loc && !loc.CanShuffle) { return false; }
            }

            if (!isCalderaLike)
            {
                var capPos = currentPos + (steps + 1) * step;
                if (WithinMapBounds(capPos))
                {
                    map[capPos] = Terrain.MOUNTAIN;
                }
            }

            return true;
        }

        // tries to place a enemy trap
        bool TryPlaceTrap(IntVector2 trapPos, ref int trapsToPlaceRemaining, ref int trapIndex)
        {
            var trap = passthroughLocations[trapIndex];
            if (ValidTrapTilePosition(trapPos) != null)
            {
                trap.Pos = trapPos;
                trap.CanShuffle = false;
                trapsToPlaceRemaining--;
                trapIndex++;
                return true;
            }
            return false;
        }

        // given a successful trap placement, narrows the zig-zag range to steer away.
        // Returns null when adjust == 0 (no range change needed).
        (int, int)? AdjustRangeAfterTrap(int adjust)
        {
            return adjust switch
            {
                > 0 => (0, 4),
                < 0 => (-3, 1),
                _ => null
            };
        }

        for (int k = 0; k < vodRoutes; k++)
        {
            int trapsPlaced = vodRoutes == 2 ? (k == 0 ? 2 : 1) : 3;
            int minAdjust = -1;
            int maxAdjust = 2;
            int tilesPlaced = 0;

            while (WithinMapBounds(currentPos, 1))
            {
                if (isCalderaLike)
                {
                    if (map[currentPos] != Terrain.MOUNTAIN) { break; }
                }
                else
                {
                    if (tilesPlaced >= length) { break; }
                }

                tilesPlaced++;
                map[currentPos] = Terrain.LAVA;

                // roll potential zig-zag offset
                int adjust = RollAdjust(currentPos, delta, minAdjust, maxAdjust);

                if (adjust != 0)
                {
                    IntVector2 movement = adjust * sideDir;
                    if (!DrawPerpendicularLavaSegment(movement))
                    {
                        return false;
                    }
                }
                else // moving straight forward
                {
                    if (map[currentPos] != Terrain.CAVE)
                    {
                        if (!isCalderaLike)
                        {
                            map[currentPos - sideDir] = Terrain.MOUNTAIN;
                            map[currentPos + sideDir] = Terrain.MOUNTAIN;
                        }
                        map[currentPos] = Terrain.LAVA;
                        if (GetLocationAt(currentPos + delta) != null)
                        {
                            return false;
                        }
                    }
                }

                currentPos += adjust * sideDir;

                // try to place trap locations
                bool shouldPlaceTrap = (cavePlaced && adjust == 0) || Math.Abs(adjust) > 1;
                if (shouldPlaceTrap && trapsPlaced > 0)
                {
                    var rotate = isCanyon ^ isHorizontal;
                    IntVector2 trapDir = rotate ? -sideDir : -forwardDir;
                    if (adjust < 0)
                    {
                        // not sure why negative adjusts would not rotate like positive adjustment,
                        // but it was the old behavior. (probably always fails placement?)
                        trapDir = sideDir;
                    }
                    //new behavior: if multiple steps along the zig-zag path would work as trap tiles, pick one at random
                    var trapStep = adjust switch
                    {
                        0 => 0,
                        >3 => RNG.Next(adjust - 3) + 1,
                        _ => 1,
                    };
                    IntVector2 trapTilePos = currentPos + trapStep * trapDir;
                    if (TryPlaceTrap(trapTilePos, ref trapsPlaced, ref traps))
                    {
                        if (AdjustRangeAfterTrap(adjust) is ValueTuple<int, int> newRange)
                        {
                            (minAdjust, maxAdjust) = newRange;
                        }
                    }
                }
                else if (adjust == 0 && !cavePlaced)
                {
                    // place cave pair, then set pos to cave exit position
                    if (k != 0)
                    {
                        vodcave1 = vodcave3;
                        vodcave2 = vodcave4;
                    }

                    if (RNG.NextDouble() > .5 && vodRoutes != 2 && biome == Biome.VOLCANO)
                    {
                        delta = -delta;
                    }

                    int caveOffset;
                    if (horizontalPath)
                    {
                        caveOffset = currentPos.Y > MapRows / 2 ? RNG.Next(-9, -4) : RNG.Next(5, 10);
                    }
                    else
                    {
                        caveOffset = currentPos.X > MapColumns / 2 ? RNG.Next(-9, -4) : RNG.Next(5, 10);
                    }
                    IntVector2 cave2Pos = currentPos + caveOffset * sideDir;

                    if (isCalderaLike && map[cave2Pos] != Terrain.MOUNTAIN)
                    {
                        return false;
                    }

                    TerraformVodCavePair(currentPos, cave2Pos, delta);
                    currentPos = cave2Pos;
                    cavePlaced = true;
                }
                else
                {
                    (minAdjust, maxAdjust) = (-3, 4);
                }

                // advance forward (or retreat if an existing location blocks the path)
                if (GetLocationAt(currentPos + delta) != null)
                {
                    map[currentPos] = Terrain.MOUNTAIN;
                    currentPos -= delta;
                }
                else
                {
                    currentPos += delta;
                }
            }

            // finally, open up VoD entrance
            if (!isCalderaLike)
            {
                // since this code is changing the terrain *after* the
                // trap tiles have been validated, we must check again
                // to avoid adding unwanted entrances to trap tiles
                List<IntVector2> newLavaTilePositions = [
                    currentPos,
                    currentPos + sideDir,
                    currentPos - sideDir,
                    currentPos + forwardDir,
                    currentPos + forwardDir + sideDir,
                    currentPos + forwardDir - sideDir
                ];
                foreach (var pos in newLavaTilePositions)
                {
                    if (!WithinMapBounds(pos)) { continue; }
                    bool illegalTrapTileEntrance = false;
                    foreach (var loc in LocationsOrthogonalTo(pos))
                    {
                        var posBehind = pos + 2 * (loc.Pos - pos);
                        if (!WithinMapBounds(posBehind))
                        {
                            illegalTrapTileEntrance = true;
                            break;
                        }
                        var terrainBehind = map[posBehind];
                        if (!terrainBehind.IsWalkable())
                        {
                            illegalTrapTileEntrance = true;
                            break;
                        }
                    }
                    if (!illegalTrapTileEntrance)
                    {
                        map[pos] = Terrain.LAVA;
                    }
                }
            }

            // set to opposite side of palace for 2nd VoD entrance
            currentPos = palacePos - 4 * delta;
            delta = -delta;
            minAdjust = -1;
            maxAdjust = 2;
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
                || (!biome.UsesVanillaMap() && hiddenKasutoLocation.TerrainType == Terrain.LAVA && hiddenKasutoLocation.IsPassthrough))
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
        int xpos = RNG.Next(6, MapColumns - 6);
        int ypos = RNG.Next(6, MapRows - 6);
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
                || (!biome.UsesVanillaMap()
                    && hiddenPalaceLocation.TerrainType == Terrain.LAVA
                    && hiddenPalaceLocation.IsPassthrough))
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
            xpos = RNG.Next(6, MapColumns - 6);
            ypos = RNG.Next(6, MapRows - 6);
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

        /*readonly*/ int mountStartY1 = RNG.Next(MapColumns / 3 - 10, MapColumns / 3 + 10);
        /*readonly*/ int mountEndY1 = RNG.Next(MapColumns / 3 - 10, MapColumns / 3 + 10);
        /*readonly*/ int endMountMargin1 = RNG.Next(2, 8);
        /*readonly*/ int endMountEndRight1 = MapColumns - endMountMargin1;
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

        /*readonly*/ int mountStartY2 = RNG.Next(MapColumns * 2 / 3 - 10, MapColumns * 2 / 3 + 10);
        map[mountStartY2, 0] = Terrain.MOUNTAIN;

        /*readonly*/ int mountEndY2 = RNG.Next(MapColumns * 2 / 3 - 10, MapColumns * 2 / 3 + 10);
        /*readonly*/ int endMountMargin2 = RNG.Next(2, 8);
        /*readonly*/ int endMountEndRight2 = MapColumns - endMountMargin2;

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
                            Location roadEnc = GetLocation(LocationID.EAST_TRAP_ROAD1);
                            roadEnc.Xpos = x;
                            roadEnc.Y = y;
                            roadEnc.CanShuffle = false;
                            roadEncounters++;
                        }
                        else if (roadEncounters == 1)
                        {
                            Location roadEnc = GetLocation(LocationID.EAST_TRAP_ROAD2);
                            roadEnc.Xpos = x;
                            roadEnc.Y = y;
                            roadEnc.CanShuffle = false;
                            roadEncounters++;
                        }
                        else if (roadEncounters == 2)
                        {
                            Location roadEnc = GetLocation(LocationID.EAST_TRAP_ROAD3);
                            roadEnc.Xpos = x;
                            roadEnc.Y = y;
                            roadEnc.CanShuffle = false;
                            roadEncounters++;
                        }
                        else if (roadEncounters == 3)
                        {
                            Location roadEnc = GetLocation(LocationID.EAST_TRAP_ROAD_TO_VOD);
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

    protected override bool IsReserved(IntVector2 pos)
    {
        if ((locationAtGP.Pos - pos).Abs().MinComponent() < 4)
        {
            return true;
        }
        return false;
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

        bool[,] visitedCoordinates = new bool[MapRows, MapColumns];
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
            if (x < MapColumns - 1 && map[y, x + 1].IsWalkable())
            {
                pendingCoordinates.Add((y, x + 1));
            }
            if (y > 0 && map[y - 1, x].IsWalkable())
            {
                pendingCoordinates.Add((y - 1, x));
            }
            if (y < MapRows - 1 && map[y + 1, x].IsWalkable())
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

    public override void DisableDisallowedPassthroughs()
    {
        foreach (Location location in Locations[Terrain.CAVE])
        {
            location.IsPassthrough = false;
        }
        foreach (Location location in Locations[Terrain.TOWN])
        {
            location.IsPassthrough = false;
        }
        foreach (Location location in Locations[Terrain.PALACE])
        {
            location.IsPassthrough = false;
        }
        if (raft != null)
        {
            raft.IsPassthrough = false;
        }
        if (bridge != null)
        {
            bridge.IsPassthrough = false;
        }
        //Issue #2: Desert tile passthrough causes the wrong screen to load, making the item unobtainable.
        desertTile.IsPassthrough = false;
        desertTile.MapPage = 1;

        Location? desert = GetLocation(LocationID.EAST_MINOR_DESERT1);
        Location? swamp = GetLocation(LocationID.EAST_MINOR_SWAMP);
        if (desert == null || swamp == null)
        {
            throw new ImpossibleException("Unable to find desert/swamp passthrough on east.");
        }

        if (desert.IsPassthrough)
        {
            desert.AccessRequirements = desert.AccessRequirements.WithHardRequirement(RequirementType.JUMP);
        }
        else
        {
            desert.AccessRequirements = desert.AccessRequirements.Without(RequirementType.JUMP);
        }

        if (swamp.IsPassthrough)
        {
            swamp.AccessRequirements = swamp.AccessRequirements.WithHardRequirement(RequirementType.FAIRY);
        }
        else
        {
            swamp.AccessRequirements = swamp.AccessRequirements.Without(RequirementType.FAIRY);
        }
    }
}