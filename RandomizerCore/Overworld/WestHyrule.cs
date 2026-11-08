using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NLog;
using Z2Randomizer.RandomizerCore.Enemy;
using Z2Randomizer.RandomizerCore.Sidescroll.Town;

namespace Z2Randomizer.RandomizerCore.Overworld;

public sealed class WestHyrule : World
{
    private readonly new Logger logger = LogManager.GetCurrentClassLogger();

    public Location northPalace;
    public Location rauru;
    public Location ruto;
    public Location sariaNorth;
    public Location sariaSouth;
    public Location mido;
    public Location bagu;
    public Location medicineCave;
    public Location trophyCave;
    public Location locationAtPalace1;
    public Location locationAtPalace2;
    public Location locationAtPalace3;
    public Location magicContainerCave;
    public Location grassTile;
    public Location heartContainerCave;

    private Location bridge1;
    private Location bridge2;
    public Location pbagCave;
    public Location kingsTomb;

    private Location jumpCave;
    private Location fairyCave;

    private Location parapaCave1;
    private Location parapaCave2;
    private Location jumpCave2;
    private Location fairyCave2;

    private int calderaCenterX, calderaCenterY;

    private const int CALDERA_DEAD_ZONE_X = 7;
    private const int CALDERA_DEAD_ZONE_Y = 7;

    private List<(Location Left, Location Right)> caveConnections;

    private List<Location> lostWoods;



    public WestHyrule(RandomizerProperties props, Random r, ROM rom) : base(r, RomMap.WEST_MAP_OFFSET)
    {
        isHorizontal = props.WestIsHorizontal;
        List<Location> loadedLocations =
        [
            .. rom.LoadLocations(LocationID.WEST_NORTH_PALACE, 10),
            .. rom.LoadLocations(LocationID.WEST_CAVE_PARAPA_NORTH, 4),
            .. rom.LoadLocations(LocationID.WEST_CAVE_PBAG, 3),
            .. rom.LoadLocations(LocationID.WEST_FAIRY_CAVE_DROP, 2),
            .. rom.LoadLocations(LocationID.WEST_BRIDGE_NORTH_OF_SARIA, 12),
            .. rom.LoadLocations(LocationID.WEST_MINOR_DESERT, 1),
            .. rom.LoadLocations(LocationID.WEST_KINGS_TOMB, 2),
            .. rom.LoadLocations(LocationID.WEST_TOWN_RUTO, 8),
        ];
        loadedLocations.ForEach(AddLocation);

        northPalace = GetLocation(LocationID.WEST_NORTH_PALACE); //0x462f
        medicineCave = GetLocation(LocationID.WEST_CAVE_MEDICINE); //0x463e
        trophyCave = GetLocation(LocationID.WEST_CAVE_TROPHY); //0x00004630
        magicContainerCave = GetLocation(LocationID.WEST_CAVE_MAGIC_CONTAINER);
        grassTile = GetLocation(LocationID.WEST_GRASS);
        heartContainerCave = GetLocation(LocationID.WEST_CAVE_HEART_CONTAINER); //0x463f
        pbagCave = GetLocation(LocationID.WEST_CAVE_PBAG);
        kingsTomb = GetLocation(LocationID.WEST_KINGS_TOMB);

        //Towns
        bagu = GetLocation(LocationID.WEST_BAGU_HOUSE); //0x4661
        bagu.Town = Towns.LoadVanillaTown(rom, TownType.BAGU, props.DisableMagicRecs);

        rauru = GetLocation(LocationID.WEST_TOWN_RAURU);
        rauru.Town = Towns.LoadVanillaTown(rom, TownType.RAURU, props.DisableMagicRecs);

        ruto = GetLocation(LocationID.WEST_TOWN_RUTO); //0x465e
        ruto.Town = Towns.LoadVanillaTown(rom, TownType.RUTO, props.DisableMagicRecs);

        sariaNorth = GetLocation(LocationID.WEST_TOWN_SARIA_NORTH); //0x00004660
        Town sariaTown = Towns.LoadVanillaTown(rom, TownType.SARIA, props.DisableMagicRecs);
        sariaNorth.Town = sariaTown;

        sariaSouth = GetLocation(LocationID.WEST_TOWN_SARIA_SOUTH); //0x0000465f
        sariaSouth.Town = sariaTown;

        mido = GetLocation(LocationID.WEST_TOWN_MIDO); //0x00004662
        mido.Town = Towns.LoadVanillaTown(rom, TownType.MIDO, props.DisableMagicRecs);

        //Palaces
        locationAtPalace1 = GetLocation(LocationID.WEST_PALACE1);
        locationAtPalace2 = GetLocation(LocationID.WEST_PALACE2);
        locationAtPalace3 = GetLocation(LocationID.WEST_PALACE3);

        //Connectors
        fairyCave = GetLocation(LocationID.WEST_FAIRY_CAVE_DROP); //0x4640
        fairyCave.AccessRequirements = new Requirements([RequirementType.FAIRY]);
        jumpCave = GetLocation(LocationID.WEST_CAVE_JUMP_NORTH); //0x463b
        jumpCave.AccessRequirements = new Requirements([RequirementType.JUMP, RequirementType.FAIRY]);

        parapaCave1 = GetLocation(LocationID.WEST_CAVE_PARAPA_NORTH); //0x4639
        parapaCave2 = GetLocation(LocationID.WEST_CAVE_PARAPA_SOUTH); //0x463a

        //These are the "high sides" so they don't have connection requirements to get to the other side
        jumpCave2 = GetLocation(LocationID.WEST_CAVE_JUMP_SOUTH); //0x463c
        fairyCave2 = GetLocation(LocationID.WEST_FAIRY_CAVE_EXIT); //0x4641

        bridge1 = GetLocation(LocationID.WEST_BRIDGE_AFTER_DM_WEST); //0x4644
        bridge2 = GetLocation(LocationID.WEST_BRIDGE_AFTER_DM_EAST); //0x4645

        caveConnections = []; // left/right (north/south for vertical calderas)
        caveConnections.Add((parapaCave1, parapaCave2));
        caveConnections.Add((jumpCave, jumpCave2));

        if (props.SaneCaves)
        {
            fairyCave.TerrainType = Terrain.CAVE;
            caveConnections.Add((fairyCave, fairyCave2));
        }

        CreateConnections(new()
        {
            LocationID.WEST_KINGS_TOMB,
            LocationID.WEST_BAGU_HOUSE,
        });

        sideviewPtrTable = 0x4533;
        sideviewBank = 1;
        enemyPtr = 0x45B1;
        groupedEnemies = Enemies.GroupedWestEnemies;
        overworldEncounterMaps = [
            29, 30, // Desert
            34, 35, // Grass
            39, 40, // Forest
            47, 48, // Swamp     - in vanilla 47-48 use the same table
            52, 53, // Graveyard - in vanilla 52-53 use the same table
            57,     // Road      - in vanilla 57-58 use the same table
        ];
        overworldEncounterMapDuplicate = [
            58,
        ];
        nonEncounterMaps = [
            00,             // NORTH_PALACE (start of game)
            01, 02, 04, 05, // Bridges
            06,             // BUBBLE_CLIFF
            07,             // Parapa passthrough
            09, 10, 11,     // Jump Cave passthrough
            12, 13,         // PILLAR_PBAG_CAVE
            14, 15,         // MEDICINE_CAVE
            16, 17,         // HEART_CONTAINER_CAVE
            18, 19,         // Fairy Cave passthrough
            20,             // SARIA_FAIRY
            21, 22, 23,     // LOST_WOODS
            33,             // TROPHY_CAVE
            38,             // GRASS_TILE
            43,             // FOREST_50P
            44,             // LOST_WOODS_1
            45,             // MAGIC_CAVE
            46,             // FOREST_100P
            51,             // EX_LIFE_SWAMP_1
            56,             // RED_JAR_CEM
        ];

        continentId = Continent.WEST;
        baseAddr = RomMap.WEST_LOCATIONS_BASE;
        VANILLA_MAP_ADDR = 0x506C;

        walkableTerrains = [Terrain.DESERT, Terrain.GRASS, Terrain.FOREST, Terrain.SWAMP, Terrain.GRAVE];
        randomTerrainFilter = [Terrain.DESERT, Terrain.GRASS, Terrain.FOREST, Terrain.SWAMP, Terrain.GRAVE, Terrain.MOUNTAIN];
        if (props.LessImportantLocationsOption != LessImportantLocationsOption.ISOLATE)
        {
            unimportantLocs.Add(GetLocation(LocationID.WEST_MINOR_FOREST_AT_START));
            unimportantLocs.Add(GetLocation(LocationID.WEST_MINOR_FOREST_BY_SARIA));
            unimportantLocs.Add(GetLocation(LocationID.WEST_BAGU_WOODS1));
            unimportantLocs.Add(GetLocation(LocationID.WEST_MINOR_SWAMP1));
            unimportantLocs.Add(GetLocation(LocationID.WEST_MINOR_GRAVE1));
            unimportantLocs.Add(GetLocation(LocationID.WEST_MINOR_FOREST_BY_JUMP_CAVE));
            unimportantLocs.Add(GetLocation(LocationID.WEST_MINOR_SWAMP2));
            unimportantLocs.Add(GetLocation(LocationID.WEST_MINOR_FOREST_EAST_OF_SARIA));
            unimportantLocs.Add(GetLocation(LocationID.WEST_BAGU_WOODS2));
            unimportantLocs.Add(GetLocation(LocationID.WEST_BAGU_WOODS3));
            unimportantLocs.Add(GetLocation(LocationID.WEST_BAGU_WOODS4));
            unimportantLocs.Add(GetLocation(LocationID.WEST_BAGU_WOODS5));
            unimportantLocs.Add(GetLocation(LocationID.WEST_MINOR_ROAD));
            unimportantLocs.Add(GetLocation(LocationID.WEST_MINOR_DESERT));
        }

        if (props.StartingLocation != StartingLocation.NORTH_PALACE)
        {
            // North Palace is useless if we don't start there
            RemoveLocations([northPalace]);
        }

        biome = props.WestBiome;
        if (biome.UsesVanillaMap() || props.WestSize == OverworldSizeOption.LARGE)
        {
            MapColumns = 64;
            MapRows = 75;
            northSouthEncounterSeparator = 30;
        }
        else
        {
            var meta = props.WestSize.GetMeta();
            MapColumns = meta.Width;
            MapRows = meta.Height;
            northSouthEncounterSeparator = MapRows / 2;
            // TODO: use metadata for num passthrough caves to remove for small continents
            // RemoveLocations([fairyCave, fairyCave2]);
            // caveConnections.Remove((fairyCave, fairyCave2));
        }

        climate = Climates.CreateForWorld(continentId, props.WestClimate, biome, props.CanWalkOnWaterWithBoots);

        lostWoods = [
            GetLocation(LocationID.WEST_BAGU_WOODS2),
            GetLocation(LocationID.WEST_BAGU_WOODS3),
            GetLocation(LocationID.WEST_BAGU_WOODS4),
            GetLocation(LocationID.WEST_BAGU_WOODS5),
            GetLocation(LocationID.WEST_BAGU_WOODS1)
        ];
    }

    private protected override Action CaptureTerrainScratchState()
    {
        var center = (calderaCenterX, calderaCenterY);
        return () => (calderaCenterX, calderaCenterY) = center;
    }

    public override bool Terraform(RandomizerProperties props, ROM rom)
    {
        using var candidate = BeginTerrainCandidate(rom);
        PrepareTerraformLocations();
        if (biome.UsesVanillaMap())
        {
            LoadAndShuffleVanillaMap(props, rom);
        }
        else // Generate biome terrain
        {
            var profile = ResolveTerrainProfile(props);
            if (!TryGenerateTerrainWithinBudget(props, profile, TryGenerateTerrainAttempt))
            {
                return false;
            }
        }
        if (!TryValidateAndStageMap()) return false;
        candidate.Commit();
        return true;
    }

    /// <summary>Loads vanilla terrain and applies the existing shuffle and repainting rules in order.</summary>
    private void LoadAndShuffleVanillaMap(RandomizerProperties props, ROM rom)
    {
        LoadVanillaTerrain(rom);
        if (biome == Biome.VANILLA_SHUFFLE)
        {
            var areas = VanillaShuffleAreas.GroupLocations(continentId, AllLocations);
            PlaceVanillaShuffleConnection(areas["parapa"]);
            PlaceVanillaShuffleConnection(areas["lifesouth"]);
            PlaceVanillaShuffleConnection(areas["island"]);
            PlaceVanillaShuffleConnection(areas["dmexit"]);

            ShuffleVanillaLocations(props.LegacyVanillaShuffledLocations);
        }
    }

    /// <summary>Initializes map, biome reservations, and terrain for one map-budget attempt.</summary>
    private Terrain PrepareGeneratedMap(RandomizerProperties props, BiomeTerrainProfile profile)
    {
        sariaSouth.ReserveForTerrainGeneration();
        sariaNorth.ReserveForTerrainGeneration();
        InitializeGeneratedMap();
        Terrain riverTerrain = profile.RiverTerrain;

        switch (biome)
        {
            case Biome.ISLANDS:
            case Biome.MOUNTAINOUS:
                FillMapBorder(profile.StripeTerrain);
                DrawTerrainStripes(profile.StripeTerrain, profile.NearEdgeMargin, profile.FarEdgeMargin);
                break;
            case Biome.CANYON:
            case Biome.DRY_CANYON:
                if (biome == Biome.DRY_CANYON)
                {
                    bridge1.Disable();
                    bridge2.Disable();
                }
                DrawCanyon(riverTerrain, profile.Climate, profile.CanyonBankTerrains, profile.IsHorizontal);
                break;
            case Biome.COASTAL_ISLANDS:
                DrawCoastalIslands(riverTerrain, profile.Climate, profile.CanyonBankTerrains, profile.IsHorizontal);
                break;
            case Biome.CALDERA:
                DrawCenterMountain(profile.IsHorizontal);
                locationAtPalace3.ReserveForTerrainGeneration();
                break;
            case Biome.VANILLALIKE:
                // Keep this biome's unused road encounter out of later crossing placement.
                GetLocation(LocationID.WEST_TRAP_ROAD).Disable(); // Legacy behavior of no road bubble tiles
                DrawMountains();
                DrawRiver(props.CanWalkOnWaterWithBoots);
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(biome), biome, "Expected a supported generated West biome.");
        }
        return riverTerrain;
    }

    /// <summary>Places terrain, connectors and blockers, disables unplaced roads/bridges, then measures size.</summary>
    private bool TryGenerateTerrainAttempt(RandomizerProperties props, BiomeTerrainProfile profile, out int encodedSize)
    {
        encodedSize = 0;
        using var attempt = BeginGeneratedTerrainAttempt(profile);
        if (props.BagusWoods)
        {
            foreach (var location in lostWoods) unimportantLocs.Remove(location);
        }
        Terrain riverTerrain = PrepareGeneratedMap(props, profile);
        var (raftDirection, bridgeDirection) = PrepareConnectorShores(props, profile);
        if (biome == Biome.CALDERA)
        {
            bool g = MakeCaldera(profile.Water);
            if (!g)
            {
                failedOnMakeCaldera++;
                return false;
            }
        }

        if (props.BagusWoods)
        {
            bool f = PlaceBagu();
            if (!f)
            {
                failedOnBaguPlacement++;
                return false;
            }
        }

        PlaceRandomTerrain(profile.Climate, profile.SeedTerrains);

        bool b = PlaceLocations(riverTerrain, props.SaneCaves);
        if (!b)
        {
            failedOnPlaceLocations++;
            return false;
        }
        if (!TryReserveContinentConnectors(profile, raftDirection, bridgeDirection)) return false;
        if (!GrowTerrain(profile.Climate))
        {
            return false;
        }

        if (!TryFinalizeContinentConnectors()) return false;

        if (!TryConnectGeneratedRegions(props, riverTerrain, profile.PreplacedWater))
        {
            return false;
        }

        foreach (Location location in Locations[Terrain.ROAD])
        {
            if (location.CanShuffle)
            {
                location.Disable();
            }
        }

        foreach (Location location in Locations[Terrain.BRIDGE])
        {
            if (location.CanShuffle)
            {
                location.Disable();
            }
        }

        //check bytes and adjust
        encodedSize = EncodeTerrainMap().Length;
        logger.Debug("West:" + encodedSize);

        return true;
    }

    /// <summary>Connects biome regions around cave blockers and hidden locations, retaining West's failure counters.</summary>
    private bool TryConnectGeneratedRegions(RandomizerProperties props, Terrain riverTerrain, Terrain preplacedWater)
    {
        if (biome == Biome.CALDERA)
        {
            var crossings = PlaceRegionCrossings(new RegionCrossingOptions
            {
                MaximumCrossings = 1,
                CrossingTerrain = preplacedWater,
                Biome = biome,
                CanWalkOnWaterWithBoots = props.CanWalkOnWaterWithBoots,
                Features = CrossingFeatures.Saria,
                DeadZone = new(
                    MinimumX: calderaCenterX - CALDERA_DEAD_ZONE_X,
                    MaximumX: calderaCenterX + CALDERA_DEAD_ZONE_X,
                    MinimumY: calderaCenterY - CALDERA_DEAD_ZONE_Y,
                    MaximumY: calderaCenterY + CALDERA_DEAD_ZONE_Y)
            });
            if ((crossings.PlacedFeatures & CrossingFeatures.Saria) == 0)
            {
                failedOnIslandConnection++;
                return false;
            }
        }

        BlockCaves(props.BoulderBlockConnections, Terrain.ROCK);
        PlaceHiddenLocations(props.LessImportantLocationsOption);

        int maximumCrossings;
        Terrain crossingTerrain = riverTerrain;
        CrossingFeatures features = CrossingFeatures.Saria | CrossingFeatures.LongBridge;
        switch (biome)
        {
            case Biome.CANYON:
            case Biome.DRY_CANYON:
                maximumCrossings = 100;
                break;
            case Biome.ISLANDS:
            case Biome.COASTAL_ISLANDS:
                maximumCrossings = 25;
                break;
            case Biome.MOUNTAINOUS:
                walkableTerrains.Add(Terrain.ROAD);
                maximumCrossings = 15;
                features = CrossingFeatures.Saria;
                break;
            case Biome.VANILLALIKE:
                // Mountain passages are optional. The water request determines acceptance.
                _ = PlaceRegionCrossings(new RegionCrossingOptions
                {
                    MaximumCrossings = 2,
                    CrossingTerrain = Terrain.MOUNTAIN,
                    Biome = biome,
                    CanWalkOnWaterWithBoots = props.CanWalkOnWaterWithBoots
                });
                maximumCrossings = 4;
                crossingTerrain = preplacedWater;
                break;
            case Biome.CALDERA:
                return true;
            default:
                throw new ArgumentOutOfRangeException(nameof(biome), biome, "Expected a supported generated West biome.");
        }

        var requiredCrossings = PlaceRegionCrossings(new RegionCrossingOptions
        {
            MaximumCrossings = maximumCrossings,
            CrossingTerrain = crossingTerrain,
            Biome = biome,
            CanWalkOnWaterWithBoots = props.CanWalkOnWaterWithBoots,
            Features = features
        });
        if ((requiredCrossings.PlacedFeatures & CrossingFeatures.Saria) == 0
            || requiredCrossings.Termination == RegionCrossingTermination.LongBridgeEntranceBlocked)
        {
            failedOnConnectIslands++;
            return false;
        }
        return true;
    }

    /// <summary>Checks routing before cave repairs, stages terrain and separator, then resets visitation.</summary>
    private bool TryValidateAndStageMap()
    {
        if (!ValidateBasicRouting())
        {
            logger.LogDebug("Terrain routing failed");
            return false;
        }

        if (!TryRepairAndValidateCaveExits()) return false;
        if (!TryStageTerrainMap()) return false;
        TerrainWrites.Put(RomMap.NORTH_SOUTH_SEPARATOR_WEST, (byte)(northSouthEncounterSeparator + 30));

        InitializeVisitation();

        return true;
    }

    /// <summary>Plans Bagu and five distinct woods tiles before publishing any terrain or location changes.</summary>
    private bool PlaceBagu()
    {
        if (!TryPlanBaguPlacement(out var position, out var woodsPositions)) return false;

        bagu.Pos = position;
        bagu.CanShuffle = false;
        map[position] = Terrain.FOREST;
        PlaceBaguWoods(woodsPositions);
        return true;
    }

    /// <summary>Tries at most 1,000 sites, requiring room for all five woods before accepting a site.</summary>
    /// <remarks>Failure advances RNG but leaves terrain, coordinates and scheduling flags unchanged.</remarks>
    private bool TryPlanBaguPlacement(out IntVector2 baguPosition, out IntVector2[] woodsPositions)
    {
        baguPosition = IntVector2.ZERO;
        woodsPositions = [];
        for (int sample = 0; sample < 1000; sample++)
        {
            // Retain Y-before-X sampling for the proposed Bagu site.
            int y = RNG.Next(6, MapRows - 7);
            var position = new IntVector2(RNG.Next(6, MapColumns - 7), y);
            if (!IsEmptyBaguTile(position)) continue;

            var availableWoods = FindAvailableBaguWoodsSites(position);
            if (availableWoods.Count < lostWoods.Count) continue;
            availableWoods.FisherYatesShuffle(RNG);
            baguPosition = position;
            woodsPositions = availableWoods.Take(lostWoods.Count).ToArray();
            return true;
        }
        return false;
    }

    private bool IsEmptyBaguTile(IntVector2 position) => map[position] == Terrain.NONE && GetLocationAt(position) == null;

    /// <summary>Enumerates distinct unused tiles within three tiles of Bagu, excluding Bagu's own tile.</summary>
    private List<IntVector2> FindAvailableBaguWoodsSites(IntVector2 baguPosition)
    {
        List<IntVector2> positions = [];
        for (int y = -3; y <= 3; y++)
            for (int x = -3; x <= 3; x++)
            {
                var position = baguPosition + new IntVector2(x, y);
                if (position != baguPosition && IsEmptyBaguTile(position)) positions.Add(position);
            }
        return positions;
    }

    /// <summary>Publishes the complete, checked woods plan in the existing record order.</summary>
    private void PlaceBaguWoods(IReadOnlyList<IntVector2> positions)
    {
        for (int i = 0; i < lostWoods.Count; i++)
        {
            var woodsLocation = lostWoods[i];
            woodsLocation.Pos = positions[i];
            map[positions[i]] = Terrain.FOREST;
            woodsLocation.CanShuffle = false;
        }
    }

    private bool MakeCaldera(Terrain water)
    {
        if (!TryChooseCalderaCenter()) return false;
        CalderaTerrain.DrawLake(map, new(calderaCenterX, calderaCenterY), isHorizontal, water, RNG);
        return TryPlaceCalderaLocations();
    }

    // West chooses before ordinary location placement and terrain growth, so
    // the 15x15 mountain check confines the lake to the seeded mountain shape.
    private bool TryChooseCalderaCenter()
    {
        var mapCenter = new IntVector2(MapColumns / 2, MapRows / 2);
        var minimum = isHorizontal
            ? new IntVector2(mapCenter.X - 5, Math.Max(7, mapCenter.Y - 15))
            : new IntVector2(Math.Max(7, mapCenter.X - 11), mapCenter.Y - 3);
        var maximum = isHorizontal
            ? new IntVector2(mapCenter.X + 5, Math.Min(mapCenter.Y + 15, MapRows - 8))
            : new IntVector2(Math.Min(mapCenter.X + 9, MapColumns - 8), mapCenter.Y + 5);
        bool found = CalderaTerrain.TryChooseCenter(map, RNG, minimum, maximum, out var center);
        // Keep the last sample as before; candidate rollback restores the prior center on rejection.
        (calderaCenterX, calderaCenterY) = (center.X, center.Y);
        return found;
    }

    private bool TryPlaceCalderaLocations()
    {
        // One pair gives the lake a single entrance; two make it a through route.
        int pairCount = RNG.Next(2) + 1;
        int primaryIndex = RNG.Next(caveConnections.Count);
        var primaryPair = caveConnections[primaryIndex];
        ClearCalderaCavePair(primaryPair);

        (Location Left, Location Right)? secondaryPair = null;
        if (pairCount == 2)
        {
            int secondaryIndex;
            do
            {
                secondaryIndex = RNG.Next(caveConnections.Count);
            } while (secondaryIndex == primaryIndex);
            secondaryPair = caveConnections[secondaryIndex];
            ClearCalderaCavePair(secondaryPair.Value);
        }

        var outward = isHorizontal ? DirectionExtensions.RandomHorizontal(RNG) : DirectionExtensions.RandomVertical(RNG);
        if (!TryPlaceCalderaConnection(primaryPair, outward)) return false;
        if (secondaryPair is { } pair)
        {
            if (!TryPlaceCalderaConnection(pair, outward.Reverse())) return false;
        }
        PlaceCalderaPalace(pairCount, outward);
        return true;
    }

    /// <summary>Clears selected caves' old tiles where they fit the current map dimensions.</summary>
    private void ClearCalderaCavePair((Location Left, Location Right) pair)
    {
        foreach (var cave in new[] { pair.Left, pair.Right })
        {
            if (WithinMapBounds(cave.Pos, 0)) map[cave.Pos] = Terrain.MOUNTAIN;
        }
    }

    /// <summary>Places a passage, reserves its caves and paints forest inside and sampled terrain outside.</summary>
    private bool TryPlaceCalderaConnection((Location Left, Location Right) pair, Direction outward)
    {
        if (!TryPlaceCalderaCavePair(new(calderaCenterX, calderaCenterY), outward, pair)) return false;
        pair.Left.CanShuffle = false;
        pair.Right.CanShuffle = false;
        Terrain exitTerrain = climate.GetRandomTerrain(RNG, randomTerrainFilter);

        var axis = isHorizontal ? IntVector2.EAST : IntVector2.SOUTH;
        bool towardLeft = outward is Direction.WEST or Direction.NORTH;
        CalderaTerrain.PaintCaveApproach(map, pair.Left.Pos, -axis,
            towardLeft ? exitTerrain : Terrain.FOREST);
        CalderaTerrain.PaintCaveApproach(map, pair.Right.Pos, axis,
            towardLeft ? Terrain.FOREST : exitTerrain);
        return true;
    }

    /// <summary>
    /// Places Palace 3 in the mountain bank opposite a single passage, or on a
    /// perpendicular bank when passages occupy both ends of the lake.
    /// </summary>
    private void PlaceCalderaPalace(int pairCount, Direction primaryOutward)
    {
        var center = new IntVector2(calderaCenterX, calderaCenterY);
        var axis = isHorizontal ? IntVector2.EAST : IntVector2.SOUTH;
        var across = isHorizontal ? IntVector2.SOUTH : IntVector2.EAST;
        IntVector2 direction, offsetAxis;
        if (pairCount == 1)
        {
            direction = -primaryOutward.ToIntVector2();
            offsetAxis = across;
        }
        else
        {
            direction = RNG.Next(2) == 0 ? across : -across;
            offsetAxis = axis;
        }
        var position = center + RNG.Next(-2, 3) * offsetAxis;
        while (map[position] != Terrain.MOUNTAIN) position += direction;
        map[position] = Terrain.PALACE;
        locationAtPalace3.Pos = position;
        map[position + direction] = Terrain.MOUNTAIN;
    }

    /// <summary>Attempts zero to two cave blockers; a partially filled quota is allowed.</summary>
    private void BlockCaves(bool connectionsCanBeBlocked, Terrain blockerTerrain)
    {
        int blockersRemaining = RNG.Next(3);
        if (blockersRemaining == 0) return;
        foreach (var cave in GetShuffledCaveBlockerCandidates(connectionsCanBeBlocked))
        {
            if (blockersRemaining == 0) break;
            if (!TryPlaceCaveBlocker(cave, blockerTerrain)) continue;
            blockersRemaining--;
        }
    }

    /// <summary>Seeds West's mountain band before rivers and locations are placed.</summary>
    private void DrawMountains()
    {
        var start = new IntVector2(0, RNG.Next((int)(MapRows * .30), (int)(MapRows * .56)));
        int endY = RNG.Next((int)(MapRows * .30), (int)(MapRows * .56));
        var end = new IntVector2(MapColumns - RNG.Next(2, 8 * MapColumns / 75), endY);
        map[start] = Terrain.MOUNTAIN;
        foreach (var position in TerrainLayout.MountainBandInterior(start, end))
        {
            map[position] = Terrain.MOUNTAIN;
        }
    }

    protected override List<Location> GetPathingStarts()
    {
        return connections.Keys.Where(i => i.Reachable)
            .Union(startLocation != null ? [startLocation] : [])
            .Union(GetContinentConnections().Where(i => i.Reachable))
            .ToList();
    }

    public override string GetName()
    {
        return "West";
    }

    public override IEnumerable<Location> RequiredLocations(bool hiddenPalace, bool hiddenKasuto)
    {
        HashSet<Location> requiredLocations =
        [
            rauru,
            mido,
            ruto,
            sariaNorth,
            sariaSouth,
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
        ];

        IncludeEnabledLocalConnections(requiredLocations);
        return requiredLocations.Where(i => i != null);
    }

    public override string GenerateSpoiler()
    {
        StringBuilder sb = new();
        sb.AppendLine("WEST: ");
        sb.AppendLine("Rauru: \n" + rauru.Town!.GenerateSpoiler());
        sb.AppendLine("Ruto: \n" + ruto.Town!.GenerateSpoiler());
        sb.AppendLine("Saria: \n" + sariaNorth.Town!.GenerateSpoiler());
        sb.AppendLine("Mido: \n" + mido.Town!.GenerateSpoiler());
        sb.AppendLine("Bagu: \n" + bagu.Town!.GenerateSpoiler());
        sb.AppendLine("Overworld:");
        AppendOverworldItemSpoilerLine(sb, magicContainerCave);
        AppendOverworldItemSpoilerLine(sb, trophyCave);
        AppendOverworldItemSpoilerLine(sb, grassTile);
        AppendOverworldItemSpoilerLine(sb, heartContainerCave);
        AppendOverworldItemSpoilerLine(sb, pbagCave);
        AppendOverworldItemSpoilerLine(sb, medicineCave);

        AppendPalaceSpoilerLine(sb, locationAtPalace1);
        AppendPalaceSpoilerLine(sb, locationAtPalace2);
        AppendPalaceSpoilerLine(sb, locationAtPalace3);

        sb.AppendLine();
        return sb.ToString();
    }

    public override void DisableDisallowedPassthroughs()
    {
        base.DisableDisallowedPassthroughs();
        if (raft != null)
        {
            raft.IsPassthrough = false;
        }
        bridge1.IsPassthrough = false;
        bridge2.IsPassthrough = false;

        fairyCave.IsPassthrough = false; //fairy cave
    }

    public override void ResetCollectables(RandomizerProperties props)
    {
        ResetTownCollectable(rauru.Town!.GetWizard()!, Collectable.SHIELD_SPELL, props.IncludeSpellsInShuffle);

        Collectable rutoSpell = props.MarioMode ? Collectable.TANOOKI : Collectable.JUMP_SPELL;
        ResetTownCollectable(ruto.Town!.GetWizard()!, rutoSpell, props.IncludeSpellsInShuffle);

        ResetTownCollectable(sariaNorth.Town!.GetWizard()!, Collectable.LIFE_SPELL, props.IncludeSpellsInShuffle);
        ResetTownCollectable(sariaNorth.Town!.GetTownMap(VanillaTownMap.SARIA_TABLE)!, Collectable.MIRROR, props.IncludeQuestItemsInShuffle);

        ResetTownCollectable(mido.Town!.GetWizard()!, Collectable.FAIRY_SPELL, props.IncludeSpellsInShuffle);
        ResetTownCollectable(mido.Town!.GetTownMap(VanillaTownMap.MIDO_TRAINER)!, Collectable.DOWNSTAB, props.IncludeSwordTechsInShuffle);

        ResetTownCollectable(bagu.Town!.GetTownMap(VanillaTownMap.BAGU_INDOORS)!, Collectable.BAGUS_NOTE, props.IncludeQuestItemsInShuffle);

        ResetOverworldCollectable(trophyCave, Collectable.TROPHY, props.ShuffleOverworldItems);
        ResetOverworldCollectable(medicineCave, Collectable.MEDICINE, props.ShuffleOverworldItems);
        ResetOverworldCollectable(heartContainerCave, Collectable.HEART_CONTAINER, props.ShuffleOverworldItems);
        ResetOverworldCollectable(magicContainerCave, Collectable.MAGIC_CONTAINER, props.ShuffleOverworldItems);
        ResetOverworldCollectable(pbagCave, Collectable.LARGE_BAG, props.ShuffleOverworldItems && props.PbagItemShuffle);
        ResetOverworldCollectable(grassTile, Collectable.HEART_CONTAINER, props.ShuffleOverworldItems);

        ResetPalaceCollectables(locationAtPalace1, props);
        ResetPalaceCollectables(locationAtPalace2, props);
        ResetPalaceCollectables(locationAtPalace3, props);
    }
}
