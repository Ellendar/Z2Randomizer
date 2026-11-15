using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NLog;
using Z2Randomizer.RandomizerCore.Enemy;
using Z2Randomizer.RandomizerCore.Sidescroll.Palace;
using Z2Randomizer.RandomizerCore.Sidescroll.Town;

namespace Z2Randomizer.RandomizerCore.Overworld;

//6A31 - address in memory of kasuto y coord;
//6A35 - address in memory of palace 6 y coord
public sealed partial class EastHyrule : World
{
#pragma warning disable CS0414
#pragma warning restore CS0414
    private static readonly new Logger logger = LogManager.GetCurrentClassLogger();

    public Location locationAtPalace5;
    public Location locationAtPalace6;
    public Location waterTile;
    public Location desertTile;
    public Location darunia;
    public Location newKasuto;
    public Location nabooru;
    public Location oldKasuto;
    public Location locationAtGP;
    public Location pbagCave1;
    public Location pbagCave2;
    private bool canyonShort;

    /// <summary>The location selected for the Flute reveal at Three-Eye Rock.</summary>
    public Location? threeEyeRock;

    /// <summary>The location selected for the Hammer reveal at the hidden forest tile.</summary>
    public Location? hiddenForestTile;

    private Terrain? threeEyeRockUnrevealedTerrain;
    private readonly List<Location> valleyOfDeathLocations;

    public EastHyrule(RandomizerProperties props, Random r, ROM rom) : base(r, RomMap.EAST_MAP_OFFSET)
    {
        logger.Trace("Initializing EastHyrule");
        isHorizontal = props.EastIsHorizontal;
        continentId = Continent.EAST;
        baseAddr = RomMap.EAST_LOCATIONS_BASE;

        List<Location> passthroughCaveLocations = [
            .. rom.LoadLocations(LocationID.EAST_CAVE_NABOORU_PASSTHROUGH_SOUTH, 2),
            .. rom.LoadLocations(LocationID.EAST_CAVE_NEW_KASUTO_PASSTHROUGH_WEST, 2),
        ];
        List<Location> passthroughVodLocations = [
            .. rom.LoadLocations(LocationID.EAST_CAVE_VOD_PASSTHROUGH2_START, 4),
        ];
        List<Location> roadTrapLocations = [
            .. rom.LoadLocations(LocationID.EAST_TRAP_ROAD1, 4),
        ];
        List<Location> desertTrapLocations = [
            .. rom.LoadLocations(LocationID.EAST_TRAP_DESERT1, 2),
        ];

        List<Location> locations =
        [
            .. passthroughCaveLocations,
            .. passthroughVodLocations,
            .. roadTrapLocations,
            .. desertTrapLocations,
            .. rom.LoadLocations(LocationID.EAST_MINOR_FOREST_BY_NABOORU, 2),
            .. rom.LoadLocations(LocationID.EAST_BRIDGE_TO_P6, 2),
            .. rom.LoadLocations(LocationID.EAST_WATER, 1),
            .. rom.LoadLocations(LocationID.EAST_MINOR_SWAMP, 1),
            .. rom.LoadLocations(LocationID.EAST_CAVE_PBAG1, 2),
            .. rom.LoadLocations(LocationID.EAST_MINOR_DESERT1, 10),
            .. rom.LoadLocations(LocationID.EAST_TOWN_NABOORU, 1),
            .. rom.LoadLocations(LocationID.EAST_TOWN_DARUNIA, 1),
            .. rom.LoadLocations(LocationID.EAST_TOWN_NEW_KASUTO, 1),
            .. rom.LoadLocations(LocationID.EAST_TOWN_OLD_KASUTO, 4),
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
        locationAtPalace6 = GetLocation(LocationID.EAST_PALACE6);
        locationAtGP = GetLocation(LocationID.EAST_GREAT_PALACE);

        //Towns
        nabooru = GetLocation(LocationID.EAST_TOWN_NABOORU);
        nabooru.Town = Towns.LoadVanillaTown(rom, TownType.NABOORU, props.DisableMagicRecs);

        darunia = GetLocation(LocationID.EAST_TOWN_DARUNIA);
        darunia.Town = Towns.LoadVanillaTown(rom, TownType.DARUNIA, props.DisableMagicRecs);

        newKasuto = GetLocation(LocationID.EAST_TOWN_NEW_KASUTO);
        newKasuto.Town = Towns.LoadVanillaTown(rom, TownType.NEW_KASUTO, props.DisableMagicRecs);
        newKasuto.Town.GetTownMap(VanillaTownMap.GRANNYS_BASEMENT)!.AccessRequirements = props.NewKasutoBasementRequirement switch
        {
            5 => new Requirements([RequirementType.FIVE_CONTAINERS]),
            6 => new Requirements([RequirementType.SIX_CONTAINERS]),
            7 => new Requirements([RequirementType.SEVEN_CONTAINERS]),
            _ => throw new Exception($"Unsupported New Kasuto basement container count: {props.NewKasutoBasementRequirement}")
        };

        oldKasuto = GetLocation(LocationID.EAST_TOWN_OLD_KASUTO);
        oldKasuto.Town = Towns.LoadVanillaTown(rom, TownType.OLD_KASUTO, props.DisableMagicRecs);

        waterTile = GetLocation(LocationID.EAST_WATER);
        waterTile.AccessRequirements = new Requirements([RequirementType.BOOTS]);
        desertTile = GetLocation(LocationID.EAST_DESERT);

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

        newKasuto.IsExternalWorld = true;
        locationAtPalace6.IsExternalWorld = true;
        threeEyeRock = null;
        hiddenForestTile = null;

        climate = Climates.CreateForWorld(continentId, props.EastClimate, biome, props.CanWalkOnWaterWithBoots);
    }

    internal IReadOnlyList<HiddenLocation> HiddenLocations { get; private set; } = [];

    private protected override Action CaptureTerrainScratchState()
    {
        var state = (threeEyeRock, hiddenForestTile, canyonShort, threeEyeRockUnrevealedTerrain, HiddenLocations);
        return () => (threeEyeRock, hiddenForestTile, canyonShort, threeEyeRockUnrevealedTerrain, HiddenLocations) = state;
    }

    public override bool Terraform(RandomizerProperties props, ROM rom)
    {
        using var candidate = BeginTerrainCandidate(rom);
        PrepareTerraformAttempt(props);
        if (biome.UsesVanillaMap())
        {
            if (!LoadAndShuffleVanillaMap(props, rom)) return false;
        }
        else // Generate biome terrain
        {
            var profile = ResolveTerrainProfile(props);
            if (!TryGenerateTerrainWithinBudget(props, profile, TryGenerateTerrainAttempt))
            {
                return false;
            }
        }
        if (!TryValidateAndStageMap(props)) return false;
        candidate.Commit();
        return true;
    }

    /// <summary>Restores locations and generated requirements once per Terraform call; inner retries retain their own resets.</summary>
    private void PrepareTerraformAttempt(RandomizerProperties props)
    {

        foreach (Location location in AllLocations)
        {
            PrepareTerraformLocation(location);
            location.AccessRequirements = location.AccessRequirements.Without([RequirementType.HAMMER, RequirementType.FLUTE]);
            location.IsHidden = false;
            if (location != raft && location != bridge && location != cave1 && location != cave2)
            {
                location.TerrainType = location.ID.GetDefaultTerrain();
            }
            //XXX: FIX HERE
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


    }

    /// <summary>Loads vanilla terrain and applies the existing shuffle and repainting rules in order.</summary>
    private bool LoadAndShuffleVanillaMap(RandomizerProperties props, ROM rom)
    {
        LoadVanillaTerrain(rom);
        IntVector2 threeEyeRockVanillaPos = new(45, 72);
        IntVector2 hiddenForestVanillaPos = new(61, 51);
        threeEyeRockUnrevealedTerrain = map[threeEyeRockVanillaPos];

        if (biome == Biome.VANILLA_SHUFFLE)
        {
            var areas = VanillaShuffleAreas.GroupLocations(continentId, AllLocations);
            PlaceVanillaShuffleConnection(areas["kasuto"]);
            PlaceVanillaShuffleConnection(areas["vod"]);
            PlaceVanillaShuffleConnection(areas["gp"]);

            if (!props.ShuffleHidden)
            {
                newKasuto.CanShuffle = false;
                locationAtPalace6.CanShuffle = false;
            }
            ShuffleVanillaLocations(props.LegacyVanillaShuffledLocations);
        }

        // Reveal spots stay fixed on vanilla maps. Look up their destinations after shuffling.
        hiddenForestTile = GetLocationAt(hiddenForestVanillaPos)!;
        threeEyeRock = GetLocationAt(threeEyeRockVanillaPos)!;
        // These possibilities should probably have been excluded earlier instead of
        // failing here, but Vanilla Shuffle isn't going to change and it works so *shrug*.
        if (props.HiddenKasuto)
        {
            if (connections.ContainsKey(hiddenForestTile) || hiddenForestTile == raft || hiddenForestTile == bridge)
            {
                return false;
            }
        }
        if (props.HiddenPalace)
        {
            if (connections.ContainsKey(threeEyeRock) || threeEyeRock == raft || threeEyeRock == bridge)
            {
                return false;
            }
        }
        else
        {
            if (props.ShuffleHidden)
            {
                map[threeEyeRockVanillaPos] = threeEyeRock.TerrainType;
            }
            else
            {
                map[threeEyeRockVanillaPos] = Terrain.PALACE;
            }
        }

        return true;
    }

    /// <summary>Starts fresh placement inputs, then generates, connects and measures one candidate.</summary>
    private bool TryGenerateTerrainAttempt(RandomizerProperties props, BiomeTerrainProfile profile, out int encodedSize)
    {
        encodedSize = 0;
        using var attempt = BeginGeneratedTerrainAttempt(profile);
        Terrain riverTerrain = profile.RiverTerrain;

        if (!TryPrepareGeneratedMap(props, profile))
        {
            return false;
        }

        var (raftDirection, bridgeDirection) = PrepareConnectorShores(props, profile);

        if (!TryPlaceGeneratedLocations(props, riverTerrain))
        {
            return false;
        }

        PlaceRandomTerrain(profile.Climate, profile.SeedTerrains);

        if (!TryReserveContinentConnectors(profile, raftDirection, bridgeDirection)) return false;

        if (!GrowTerrain(profile.Climate))
        {
            return false;
        }
        if (!TryFinalizeContinentConnectors()) return false;

        ConnectGeneratedRegions(props, riverTerrain, profile.PreplacedWater);

        if (props.RiverDevilBlockerOption == RiverDevilBlockerOption.SIEGE)
        {
            if (!TryPlaceRiverDevilSiege())
            {
                return false;
            }
        }

        // CanShuffle still being true means no placement phase claimed this location.
        // Disable unused overworld entries (e.g. spare road/bridge encounters),
        // keeping them off-map and closing placement availability. AppearsOnMap
        // limits this cleanup to overworld placement participants.
        foreach (Location location in AllLocations)
        {
            if (location.CanShuffle && location.AppearsOnMap)
            {
                location.Disable();
            }
        }

        encodedSize = EncodeTerrainMap(HiddenLocationEncodingOptions()).Length;
        TerrainWrites.Put(RomMap.NORTH_SOUTH_SEPARATOR_EAST, (byte)(northSouthEncounterSeparator + 30));

        return true;
    }

    /// <summary>Seeds biome geometry and the Valley of Death with attempt-local river terrain.</summary>
    private bool TryPrepareGeneratedMap(RandomizerProperties props, BiomeTerrainProfile profile)
    {
        InitializeGeneratedMap();

        switch (biome)
        {
            case Biome.ISLANDS:
            case Biome.MOUNTAINOUS:
                FillMapBorder(profile.MapBorder);
                _ = MakeValleyOfDeath(); // allowed to fail
                DrawTerrainStripes(profile.StripeTerrain, profile.NearEdgeMargin, profile.FarEdgeMargin);
                return true;
            case Biome.CANYON:
            case Biome.DRY_CANYON:
                DrawCanyon(profile.RiverTerrain, profile.Climate, profile.CanyonBankTerrains, profile.IsHorizontal);
                locationAtGP.ReserveForTerrainGeneration();
                return MakeValleyOfDeath();
            case Biome.COASTAL_ISLANDS:
                DrawCoastalIslands(profile);
                locationAtGP.ReserveForTerrainGeneration();
                return MakeValleyOfDeath();
            case Biome.VOLCANO:
                DrawCenterMountain(profile.IsHorizontal);
                return MakeValleyOfDeath();
            case Biome.VANILLALIKE:
                _ = MakeValleyOfDeath(); // allowed to fail
                DrawMountains();
                DrawRiver(props.CanWalkOnWaterWithBoots);
                return true;
            default:
                throw new ArgumentOutOfRangeException(nameof(biome), biome, "Expected a supported generated East biome.");
        }
    }

    /// <summary>Places hidden Kasuto before the hidden palace to prevent collocated X coordinates, then ordinary locations.</summary>
    private bool TryPlaceGeneratedLocations(RandomizerProperties props, Terrain riverTerrain)
    {
        // Only targets selected by this attempt may constrain placement and encoding.
        hiddenForestTile = null;
        threeEyeRock = null;
        threeEyeRockUnrevealedTerrain = null;

        // Bug #124 has us in a chicken/egg situation: they cannot be at the same X-coord,
        // but we have to place one first. Hidden Kasuto location is pulled from the location list,
        // and Hidden Palace has code to move its location based on terrain. So we make sure to place
        // Hidden Kasuto first, let Hidden Palace check against it and set itself in a nice location,
        // and PlaceLocations will make sure we do not re-shuffle Hidden Kasuto to the same X coord.
        if (props.HiddenKasuto)
        {
            bool kasutoSelected = RandomizeHiddenKasuto(props.ShuffleHidden);
            if (!kasutoSelected)
            {
                return false;
            }
        }

        if (props.HiddenPalace)
        {
            // sets threeEyeRock
            bool hp = RandomizeHiddenPalace(props.ShuffleHidden, props.HiddenKasuto);
            if (!hp)
            {
                return false;
            }
        }

        bool placeLocationsResult;
        if (props.HiddenKasuto && props.HiddenPalace)
        {
            placeLocationsResult = PlaceLocations(riverTerrain, props.SaneCaves, hiddenForestTile, threeEyeRock);
        }
        else
        {
            placeLocationsResult = PlaceLocations(riverTerrain, props.SaneCaves);
        }
        if (!placeLocationsResult)
        {
            return false;
        }

        return true;
    }

    /// <summary>Places blockers and optional crossings; final routing retains responsibility for acceptance.</summary>
    private void ConnectGeneratedRegions(RandomizerProperties props, Terrain riverTerrain, Terrain preplacedWater)
    {
        bool riverDevilBlocksCave = props.RiverDevilBlockerOption == RiverDevilBlockerOption.CAVE;
        bool rockBlocksCave = props.EastRocks && !props.EastRockIsPath;
        PlaceHiddenLocations(props.LessImportantLocationsOption);
        BlockCaves(props.BoulderBlockConnections, riverDevilBlocksCave, rockBlocksCave, [threeEyeRock, hiddenForestTile, .. valleyOfDeathLocations]);
        bool riverDevilBlocksPath = props.RiverDevilBlockerOption == RiverDevilBlockerOption.PATH;
        bool rockBlocksPath = props.EastRocks && props.EastRockIsPath;
        // A shuffled hidden target owns its location record. The optional Darunia
        // encounter pair must not reset or move either record out from under it.
        bool placeDaruniaEncounters = !new[] { threeEyeRock, hiddenForestTile }
            .Any(l => l?.ID is LocationID.EAST_TRAP_DESERT1 or LocationID.EAST_TRAP_DESERT2);

        int maximumCrossings;
        switch (biome)
        {
            case Biome.VANILLALIKE:
                _ = PlaceRegionCrossings(new RegionCrossingOptions
                {
                    MaximumCrossings = 4,
                    CrossingTerrain = Terrain.MOUNTAIN,
                    Biome = biome,
                    CanWalkOnWaterWithBoots = props.CanWalkOnWaterWithBoots,
                    Features = placeDaruniaEncounters ? CrossingFeatures.DaruniaEncounters : CrossingFeatures.None
                });
                _ = PlaceRegionCrossings(new RegionCrossingOptions
                {
                    MaximumCrossings = 3,
                    CrossingTerrain = preplacedWater,
                    Biome = biome,
                    CanWalkOnWaterWithBoots = props.CanWalkOnWaterWithBoots,
                    RiverDevilBlocker = riverDevilBlocksPath,
                    RockBlocker = rockBlocksPath
                });
                return;
            case Biome.ISLANDS:
            case Biome.COASTAL_ISLANDS:
                maximumCrossings = 100;
                break;
            case Biome.MOUNTAINOUS:
                maximumCrossings = 20;
                break;
            case Biome.CANYON:
            case Biome.DRY_CANYON:
                maximumCrossings = 15;
                break;
            case Biome.VOLCANO:
                return;
            default:
                throw new ArgumentOutOfRangeException(nameof(biome), biome, "Expected a supported generated East biome.");
        }

        _ = PlaceRegionCrossings(new RegionCrossingOptions
        {
            MaximumCrossings = maximumCrossings,
            CrossingTerrain = riverTerrain,
            Biome = biome,
            CanWalkOnWaterWithBoots = props.CanWalkOnWaterWithBoots,
            Features = placeDaruniaEncounters ? CrossingFeatures.DaruniaEncounters : CrossingFeatures.None,
            RiverDevilBlocker = riverDevilBlocksPath,
            RockBlocker = rockBlocksPath
        });
    }

    /// <summary>Surrounds the first eligible town in shuffled order, excluding hidden-location targets.</summary>
    private bool TryPlaceRiverDevilSiege()
    {
        Location[] towns = [nabooru, darunia, newKasuto, oldKasuto];
        RNG.Shuffle(towns);
        foreach (Location location in towns)
        {
            if (location == hiddenForestTile || location == threeEyeRock)
            {
                continue;
            }
            IntVector2 position = location.Pos;
            if (IntVector2.CARDINALS.Any(d => IsContinentConnectorReserved(position + d))) continue;
            if (map[position + IntVector2.SOUTH].IsWalkable(true) && map[position + IntVector2.WEST].IsWalkable(true)
                && map[position + IntVector2.NORTH].IsWalkable(true) && map[position + IntVector2.EAST].IsWalkable(true))
            {
                map[position + IntVector2.SOUTH] = Terrain.RIVER_DEVIL;
                map[position + IntVector2.NORTH] = Terrain.RIVER_DEVIL;
                map[position + IntVector2.WEST] = Terrain.RIVER_DEVIL;
                map[position + IntVector2.EAST] = Terrain.RIVER_DEVIL;
                return true;
            }
        }
        logger.Trace("No valid candidate towns for river devil siege.");
        return false;
    }

    /// <summary>Validates routing and cave exits, records the selected reveals, and stages the terrain map.</summary>
    private bool TryValidateAndStageMap(RandomizerProperties props)
    {
        if (!ValidateBasicRouting())
        {
            return false;
        }

        if (!TryRepairAndValidateCaveExits()) return false;

        ConfigureHiddenLocations(props);

        if (!TryStageTerrainMap(HiddenLocationEncodingOptions())) return false;

        InitializeVisitation();

        return true;
    }

    /// <summary>Sets the selected locations' reveal requirements and records their ROM reveal data.</summary>
    private void ConfigureHiddenLocations(RandomizerProperties props)
    {
        List<HiddenLocation> hiddenLocations = [];
        if (props.HiddenPalace)
        {
            if (threeEyeRock is null)
            {
                throw new ImpossibleException("Failure in hidden palace shuffle");
            }
            threeEyeRock.IsHidden = true;
            threeEyeRock.AccessRequirements = threeEyeRock.AccessRequirements.WithHardRequirement(RequirementType.FLUTE);
            var unrevealedTerrain = threeEyeRockUnrevealedTerrain ?? throw new ImpossibleException("Three Eye Rock is missing its unrevealed terrain.");
            var hiddenLoc = HiddenLocation.FromLocation(threeEyeRock, HiddenLocationTrigger.Flute, unrevealedTerrain: unrevealedTerrain);
            hiddenLocations.Add(hiddenLoc);
        }
        if (props.HiddenKasuto)
        {
            if (hiddenForestTile is null)
            {
                throw new ImpossibleException("Failure in hidden Kasuto shuffle");
            }
            hiddenForestTile.IsHidden = true;
            hiddenForestTile.AccessRequirements = hiddenForestTile.AccessRequirements.WithHardRequirement(RequirementType.HAMMER);
            var revealedTerrain = hiddenForestTile.ID.GetDefaultTerrain();
            var unrevealedTerrain = Terrain.FOREST;
            var hiddenLoc = HiddenLocation.FromLocation(hiddenForestTile, HiddenLocationTrigger.Hammer, revealedTerrain, unrevealedTerrain);
            hiddenLocations.Add(hiddenLoc);
        }
        HiddenLocations = Array.AsReadOnly(hiddenLocations.ToArray());
    }

    private MapEncodingOptions HiddenLocationEncodingOptions()
        => new(ThreeEyeRockPosition: threeEyeRock?.Pos,
            HiddenForestTilePosition: hiddenForestTile?.Pos,
            ThreeEyeRockUnrevealedTerrain: threeEyeRockUnrevealedTerrain);

    /// <summary>Samples the palace opening, keeping biome-specific ranges and retry rules.</summary>
    private bool TryChooseValleyOfDeathSite(bool requiresMountainArea, out IntVector2 palacePos)
    {
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
            ymax = MapRows - 6;
        }
        palacePos = new(RNG.Next(xmin, xmax), RNG.Next(ymin, ymax));

        // Ensure there is enough unallocated space to draw the whole opening
        if (requiresMountainArea)
        {
            int tries = 0;
            bool allMountains;
            do
            {
                if (tries++ == 1000) { return false; }
                palacePos = IntVector2.Random(RNG, xmin, xmax, ymin, ymax);
                allMountains = map.AllTerrainInSquareEquals(palacePos, Terrain.MOUNTAIN, radius: 4);
            } while (!allMountains);
        }
        return true;
    }

    /// <summary>Places the palace opening, then its lava routes, cave links and trap encounters.</summary>
    /// <remarks>Failure can leave partial edits. The enclosing candidate owns rollback; callers retain their biome-specific rejection policy.</remarks>
    public bool MakeValleyOfDeath()
    {
        bool usesMountainOpening = biome is Biome.CANYON or Biome.DRY_CANYON or Biome.COASTAL_ISLANDS or Biome.VOLCANO;
        List<Location> traps =
        [
            GetLocation(LocationID.EAST_TRAP_LAVA1),
            GetLocation(LocationID.EAST_TRAP_LAVA2),
            GetLocation(LocationID.EAST_TRAP_LAVA3)
        ];
        // Clear previous trap coordinates without disabling later placement.
        foreach (var trap in traps) trap.YRaw = 0;

        if (!TryChooseValleyOfDeathSite(usesMountainOpening, out var palacePosition)) return false;
        DrawValleyOfDeathOpening(map, palacePosition);
        locationAtGP.Pos = palacePosition;
        locationAtGP.CanShuffle = false;

        var routes = new ValleyOfDeathRouteBuilder(this, palacePosition, traps);
        return routes.TryBuild();
    }

    /// <summary>Checks the shared target restrictions for East's Hammer- and Flute-hidden locations.</summary>
    private bool CanBecomeHiddenLocation(Location? location)
    {
        if (location == null || location == raft || location == bridge || location == cave1 || location == cave2) return false;
        if (connections.ContainsKey(location) || !location.CanShuffle) return false;
        if (location.TerrainType == Terrain.TOWN && !location.AppearsOnMap) return false;
        return biome.UsesVanillaMap() || location.TerrainType != Terrain.LAVA || !location.IsPassthrough;
    }

    /// <summary>Filters targets in location order and chooses uniformly; returns null when none are eligible.</summary>
    private Location? ChooseHiddenLocation(Location? excluded = null)
    {
        var eligible = AllLocations.Where(location => location != excluded && CanBecomeHiddenLocation(location)).ToArray();
        return eligible.Length == 0 ? null : eligible[RNG.Next(eligible.Length)];
    }

    /// <summary>Selects the Hammer-hidden record and adds its requirement; ordinary placement supplies its coordinates later.</summary>
    private bool RandomizeHiddenKasuto(bool shuffleHidden)
    {
        var target = shuffleHidden ? ChooseHiddenLocation() : newKasuto;
        if (target is null) return false;
        hiddenForestTile = target;
        hiddenForestTile.TerrainType = Terrain.FOREST;
        hiddenForestTile.AccessRequirements = hiddenForestTile.AccessRequirements.WithHardRequirement(RequirementType.HAMMER);
        unimportantLocs.Remove(hiddenForestTile);
        return true;
    }

    /// <summary>Selects the Flute-hidden record and places its unrevealed Three Eye Rock opening.</summary>
    private bool RandomizeHiddenPalace(bool shuffleHidden, bool hiddenKasuto)
    {
        if (hiddenKasuto && hiddenForestTile is null)
        {
            throw new ImpossibleException("If hidden Kasuto is on, RandomizeHiddenKasuto must be run before RandomizeHiddenPalace");
        }
        var target = shuffleHidden ? ChooseHiddenLocation(hiddenKasuto ? hiddenForestTile : null) : locationAtPalace6;
        if (target is null) return false;
        threeEyeRock = target;
        if (!TryChooseHiddenPalaceSite(hiddenKasuto, out var center)) return false;

        Terrain unrevealedTerrain = climate.GetRandomTerrain(RNG, walkableTerrains);
        while (unrevealedTerrain == Terrain.FOREST)
        {
            unrevealedTerrain = climate.GetRandomTerrain(RNG, walkableTerrains);
        }
        DrawThreeEyeRock(map, center, unrevealedTerrain);
        threeEyeRock.Pos = center + 2 * IntVector2.SOUTH;
        threeEyeRockUnrevealedTerrain = unrevealedTerrain;
        threeEyeRock.CanShuffle = false;
        return true;
    }

    /// <summary>Finds a fully empty 7x7 opening, optionally avoiding hidden Kasuto's column.</summary>
    /// <remarks>At most 1,000 X/Y samples, including those rejected by the column exclusion.</remarks>
    private bool TryChooseHiddenPalaceSite(bool avoidKasutoColumn, out IntVector2 center)
    {
        center = IntVector2.ZERO;
        if (MapColumns <= 12 || MapRows <= 12) return false;
        const int maximumSamples = 1000;
        for (int sample = 0; sample < maximumSamples; sample++)
        {
            center = new(RNG.Next(6, MapColumns - 6), RNG.Next(6, MapRows - 6));
            // #124: sharing Kasuto's column causes a wrong warp after leaving the palace.
            if (avoidKasutoColumn && center.X == hiddenForestTile!.Xpos) continue;
            if (map.AllTerrainInSquareEquals(center, Terrain.NONE, radius: 3)) return true;
        }
        return false;
    }

    /// <summary>Fills a 7x7 opening with its unrevealed terrain and three mountain tiles.</summary>
    /// <remarks>
    /// The caller supplies a center whose full footprint fits the map. The hidden
    /// palace stands two tiles south of the center; its tile keeps the unrevealed
    /// terrain. Location updates and reveal configuration belong to East.
    /// </remarks>
    internal static void DrawThreeEyeRock(OverworldMap map, IntVector2 center, Terrain unrevealedTerrain)
    {
        for (int y = -3; y <= 3; y++)
            for (int x = -3; x <= 3; x++)
            {
                bool mountain = (y == -2 && x == 0) || (y == 0 && (x == -2 || x == 2));
                map[center + new IntVector2(x, y)] = mountain ? Terrain.MOUNTAIN : unrevealedTerrain;
            }
    }

    /// <summary>Attempts the requested River Devil, then rock, applying caller exclusions and cave separation.</summary>
    private void BlockCaves(bool connectionsCanBeBlocked, bool riverDevilBlocks, bool rockBlock,
        IEnumerable<Location?> disallowedLocations)
    {
        if (!riverDevilBlocks && !rockBlock) { return; }

        bool riverDevilPlaced = !riverDevilBlocks;
        bool rockPlaced = !rockBlock;

        Location? riverDevilCave = null;

        foreach (var cave in GetShuffledCaveBlockerCandidates(connectionsCanBeBlocked))
        {
            if (riverDevilPlaced && rockPlaced) { break; }

            if (disallowedLocations.Contains(cave)) { continue; }

            if (riverDevilCave != null &&
                int.Abs(riverDevilCave.Xpos - cave.Xpos) < 4 &&
                int.Abs(riverDevilCave.Y - cave.Y) < 4)
            {
                continue;
            }

            Terrain blockerTerrain = riverDevilPlaced ? Terrain.ROCK : Terrain.RIVER_DEVIL;

            if (!TryPlaceCaveBlocker(cave, blockerTerrain)) continue;
            if (!riverDevilPlaced)
            {
                riverDevilPlaced = true;
                riverDevilCave = cave;
            }
            else
            {
                rockPlaced = true;
            }
        }
    }

    /// <summary>Seeds Vanilla-like East's two mountain bands around existing Valley of Death terrain.</summary>
    private void DrawMountains()
    {
        int maxMountainVariance = (int)Math.Ceiling((decimal)MapRows / 75) * 10;

        var northStart = new IntVector2(0, RNG.Next(MapRows / 3 - maxMountainVariance, MapRows / 3 + maxMountainVariance));
        int northEndY = RNG.Next(MapRows / 3 - maxMountainVariance, MapRows / 3 + maxMountainVariance);
        var northEnd = new IntVector2(MapColumns - RNG.Next(2, 8 * MapColumns / 75), northEndY);
        map[northStart] = Terrain.MOUNTAIN;
        DrawBandInterior(northStart, northEnd);

        var southStart = new IntVector2(0, RNG.Next(MapRows * 2 / 3 - maxMountainVariance, MapRows * 2 / 3 + maxMountainVariance));
        map[southStart] = Terrain.MOUNTAIN;
        int southEndY = RNG.Next(MapRows * 2 / 3 - maxMountainVariance, MapRows * 2 / 3 + maxMountainVariance);
        var southEnd = new IntVector2(MapColumns - RNG.Next(2, 8 * MapColumns / 75), southEndY);
        DrawBandInterior(southStart, southEnd);

        void DrawBandInterior(IntVector2 start, IntVector2 end)
        {
            foreach (var position in TerrainLayout.MountainBandInterior(start, end))
            {
                if (map[position] == Terrain.NONE) map[position] = Terrain.MOUNTAIN;
            }
        }
    }

    /// If this tile is reserved, in the context of a river expanding to
    /// this tile. Most of these are probably logically impossible.
    /// The point is to stop a river opening up VoD.
    protected override bool IsReserved(IntVector2 pos, Terrain terrain)
    {
        switch (terrain)
        {
            case Terrain.TOWN:
            case Terrain.CAVE:
            case Terrain.PALACE:
            case Terrain.BRIDGE:
            case Terrain.LAVA:
            case Terrain.MOUNTAIN:
            case Terrain.ROCK:
            case Terrain.RIVER_DEVIL:
                return true;
        }

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
            pbagCave1, pbagCave2, hiddenPalaceCallSpot, threeEyeRock, hiddenForestTile
        };*/
        return connections.Keys.Where(i => i.Reachable)
            .Union(GetContinentConnections().Where(i => i.Reachable))
            .ToList();
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
            darunia,
            newKasuto,
            nabooru,
            oldKasuto,
            locationAtGP,
            pbagCave1,
            pbagCave2,
            //vodcave1,
            //vodcave2
        ];

        if (hiddenPalace)
        {
            if (threeEyeRock is null)
            {
                throw new ImpossibleException("Failure in hidden Palace shuffle");
            }
            requiredLocations.Add(threeEyeRock);
        }

        if (hiddenKasuto)
        {
            if (hiddenForestTile is null)
            {
                throw new ImpossibleException("Failure in hidden Kasuto shuffle");
            }
            requiredLocations.Add(hiddenForestTile);
        }

        IncludeEnabledLocalConnections(requiredLocations);
        return requiredLocations.Where(i => i != null);
    }

    public override string GenerateSpoiler()
    {
        StringBuilder sb = new();
        sb.AppendLine("EAST: ");
        sb.AppendLine("Nabooru: \n" + nabooru.Town!.GenerateSpoiler());
        sb.AppendLine("Darunia: \n" + darunia.Town!.GenerateSpoiler());
        sb.AppendLine("New Kasuto: \n" + newKasuto.Town!.GenerateSpoiler());
        sb.AppendLine("Old Kasuto: \n" + oldKasuto.Town!.GenerateSpoiler());
        sb.AppendLine("Overworld:");
        AppendOverworldItemSpoilerLine(sb, pbagCave2);
        AppendOverworldItemSpoilerLine(sb, pbagCave1);
        AppendOverworldItemSpoilerLine(sb, waterTile);
        AppendOverworldItemSpoilerLine(sb, desertTile);

        if (threeEyeRock is not null)
        {
            AppendHiddenLocationSpoilerLine(sb, "Three-Eyed Rock", threeEyeRock);
        }

        if (hiddenForestTile is not null)
        {
            AppendHiddenLocationSpoilerLine(sb, "Hidden Kasuto Tile", hiddenForestTile);
        }

        AppendPalaceSpoilerLine(sb, locationAtPalace5);
        AppendPalaceSpoilerLine(sb, locationAtPalace6);
        AppendPalaceSpoilerLine(sb, locationAtGP);

        sb.AppendLine();
        return sb.ToString();
    }

    private static void AppendHiddenLocationSpoilerLine(StringBuilder spoiler, string label, Location location)
    {
        string palace = location.Palace is { } target ? $" ({PalaceSpoilerName(target.Number)})" : "";
        spoiler.AppendLine($"\t{label}: {location.ID.GetDescription()}{palace}");
    }

    public override void DisableDisallowedPassthroughs()
    {
        base.DisableDisallowedPassthroughs();
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

    public override void ResetCollectables(RandomizerProperties props)
    {
        Collectable nabooruSpell = props.ReplaceFireWithDash ? Collectable.DASH_SPELL : Collectable.FIRE_SPELL;
        ResetTownCollectable(nabooru.Town!.GetWizard()!, nabooruSpell, props.IncludeSpellsInShuffle);
        ResetTownCollectable(nabooru.Town!.GetTownMap(VanillaTownMap.NABOORU_MID)!, Collectable.WATER, props.IncludeQuestItemsInShuffle);

        ResetTownCollectable(darunia.Town!.GetWizard()!, Collectable.REFLECT_SPELL, props.IncludeSpellsInShuffle);
        ResetTownCollectable(darunia.Town!.GetTownMap(VanillaTownMap.DARUNIA_TRAINER)!, Collectable.UPSTAB, props.IncludeSwordTechsInShuffle);

        ResetTownCollectable(newKasuto.Town!.GetWizard()!, Collectable.SPELL_SPELL, props.IncludeSpellsInShuffle);
        ResetTownCollectable(newKasuto.Town!.GetTownMap(VanillaTownMap.SPELL_TOWER_INTERIOR)!, Collectable.MAGIC_KEY, props.ShuffleOverworldItems);
        ResetTownCollectable(newKasuto.Town!.GetTownMap(VanillaTownMap.GRANNYS_BASEMENT)!, Collectable.MAGIC_CONTAINER, props.ShuffleOverworldItems);

        ResetTownCollectable(oldKasuto.Town!.GetWizard()!, Collectable.THUNDER_SPELL, props.IncludeSpellsInShuffle);

        ResetOverworldCollectable(desertTile, Collectable.HEART_CONTAINER, props.ShuffleOverworldItems);
        ResetOverworldCollectable(waterTile, Collectable.HEART_CONTAINER, props.ShuffleOverworldItems);
        ResetOverworldCollectable(pbagCave1, Collectable.XL_BAG, props.ShuffleOverworldItems && props.PbagItemShuffle);
        ResetOverworldCollectable(pbagCave2, Collectable.XL_BAG, props.ShuffleOverworldItems && props.PbagItemShuffle);

        ResetPalaceCollectables(locationAtPalace5, props);
        ResetPalaceCollectables(locationAtPalace6, props);
        ResetPalaceCollectables(locationAtGP, props);
    }
}
