using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using SD.Tools.BCLExtensions.CollectionsRelated;
using Z2Randomizer.RandomizerCore.Enemy;

namespace Z2Randomizer.RandomizerCore.Overworld;

sealed class DeathMountain : World
{
    //Other connections are all 1:1 but DM has 4-way connectors.
    //Eventually this should go away and connections should just be a one-to-many
    public Dictionary<Location, List<Location>> connectionsDM = new();
    public Location hammerCave;
    public Location specRock;

    public DeathMountain(RandomizerProperties props, Random r, ROM rom) : base(r, RomMap.DM_MAP_OFFSET)
    {
        List<Location> locations =
        [
            .. rom.LoadLocations(LocationID.DM_CAVE1A, 37),
            .. rom.LoadLocations(LocationID.DM_SPEC_ROCK, 1),
        ];
        locations.ForEach(AddLocation);

        isHorizontal = props.DmIsHorizontal;
        hammerCave = GetLocation(LocationID.DM_HAMMER_CAVE);
        specRock = GetLocation(LocationID.DM_SPEC_ROCK);

        CreateConnections(null, connectionsDM);

        sideviewPtrTable = 0x6010;
        sideviewBank = 1;
        enemyPtr = 0x608E;
        groupedEnemies = Enemies.GroupedWestEnemies;
        overworldEncounterMaps = [
            29,     // Desert    - in vanilla 29-30 use the same table
            34, 35, // Grass     - not used in Vanilla
            39,     // Forest    - in vanilla 39-40 use the same table
            47, 48, // Swamp     - not used in Vanilla
            52,     // Graveyard - in vanilla 52-53 use the same table
            57,     // Road      - in vanilla 57-58 use the same table
        ];
        overworldEncounterMapDuplicate = [
            30, 40, 53, 58,
        ];
        nonEncounterMaps = [
            1, 2, 3, 4, 5, 6, 7, 8, // Caves
            9, 10, 11, 12, 13, 14,  // Caves
            15, 16, 17,             // Hammer Cave floor 1
            18, 19, 20, 21,         // Hammer Cave floor 2
            22, 23, 24, 25,         // Caves with elevators
            26,                     // Spectacle Rock
        ];
        // Re-used DM cave sideviews
        int[][] duplicateSideviewCaves = [
            [1, 12],
            [4, 13],
            [5, 8],
            [6, 10],
            [7, 14],
            // [23, 24], // top part of 4-way
        ];

        continentId = Continent.DM;
        baseAddr = RomMap.DM_LOCATIONS_BASE;
        VANILLA_MAP_ADDR = 0x665c;

        biome = props.DmBiome switch
        {
            Biome.DRY_CANYON => Biome.CANYON,
            _ => props.DmBiome
        };
        if (biome.UsesVanillaMap())
        {
            MapColumns = 64;
            MapRows = 75;
            northSouthEncounterSeparator = 45;
        }
        else
        {
            var meta = props.DmSize.GetMeta();
            MapColumns = meta.Width;
            MapRows = meta.Height;
            northSouthEncounterSeparator = MapRows / 2;

            if (biome == Biome.CALDERA) // Caldera won't work with less
            {
                MapColumns = Math.Max(MapColumns, 35);
                MapRows = Math.Max(MapRows, 35);
            }

            // TODO: use metadata for num caves to remove
            int connectorPairsToRemove = props.DmSize switch
            {
                DmSizeOption.LARGE => 0,
                DmSizeOption.MEDIUM => 0,
                DmSizeOption.SMALL => 9,
                DmSizeOption.TINY => 13,
                _ => throw new NotImplementedException(),
            };

            int removedCavePairs = 0;
            if (props.DmSize != DmSizeOption.LARGE)
            {
                // remove duplicate (by sideview) caves
                foreach (int[] dupeMaps in duplicateSideviewCaves)
                {
                    int keepIndex = r.Next(dupeMaps.Length);
                    for (int index = 0; index < dupeMaps.Length; index++)
                    {
                        if (index != keepIndex)
                        {
                            var map = dupeMaps[index];
                            Location entrance = connectionsDM.Keys.FirstOrDefault(loc => loc.Map == map)!;
                            Debug.Assert(entrance != null);
                            List<Location> otherSideEntrance = connectionsDM[entrance];
                            Debug.Assert(otherSideEntrance.Count == 1);
                            RemoveLocationsDM([entrance, .. otherSideEntrance]);
                            removedCavePairs++;
                        }
                    }
                }
            }

            int maxElevatorConnectorsToRemove = 1;
            int elevatorConnectorsRemoved = 0;
            for (; removedCavePairs < connectorPairsToRemove; removedCavePairs++)
            {
                KeyValuePair<Location, List<Location>> removeLocPair;
                do
                {
                    var j = r.Next(connectionsDM.Count);
                    removeLocPair = connectionsDM.ElementAt(j);
                } while (removeLocPair.Value.Count > 1 && elevatorConnectorsRemoved++ >= maxElevatorConnectorsToRemove);
                RemoveLocationsDM([removeLocPair.Key, .. removeLocPair.Value]);
            }
        }

        walkableTerrains = new List<Terrain>() { Terrain.DESERT, Terrain.FOREST, Terrain.GRAVE };
        randomTerrainFilter = new List<Terrain>() { Terrain.DESERT, Terrain.FOREST, Terrain.GRAVE, Terrain.MOUNTAIN, Terrain.WALKABLEWATER, Terrain.WATER };

        climate = Climates.CreateForWorld(continentId, props.DmClimate, biome, props.CanWalkOnWaterWithBoots);
    }

    private void RemoveLocationsDM(ICollection<Location> locations)
    {
        RemoveLocations(locations);
        foreach (var loc in locations)
        {
            connectionsDM.Remove(loc);
        }
    }

    public override bool Terraform(RandomizerProperties props, ROM rom)
    {
        using var candidate = BeginTerrainCandidate(rom);
        var profile = ResolveTerrainProfile(props);
        bool usesVanillaMap = biome.UsesVanillaMap();
        // Generated attempts copy their profile in BeginGeneratedTerrainAttempt.
        if (usesVanillaMap)
        {
            ApplyTerrainProfile(profile);
        }
        PrepareTerraformLocations();
        if (usesVanillaMap)
        {
            LoadAndShuffleVanillaMap(rom, props.LegacyVanillaShuffledLocations);
        }
        else // Generate biome terrain
        {
            if (!TryGenerateTerrainWithinBudget(props, profile, TryGenerateTerrainAttempt))
            {
                return false;
            }
        }
        if (!TryValidateAndStageMap()) return false;
        candidate.Commit();
        return true;
    }

    private protected override void RepaintVanillaShuffleTiles()
    {
        specRock.TerrainType = Terrain.ROCK;
        RepaintLocationTiles();
    }

    /// <summary>Places terrain, caves and connectors, validates cave exits, then measures size; the enclosing candidate owns publication.</summary>
    private bool TryGenerateTerrainAttempt(RandomizerProperties props, BiomeTerrainProfile profile, out int encodedSize)
    {
        encodedSize = 0;
        using var attempt = BeginGeneratedTerrainAttempt(profile);
        Terrain riverT = PrepareGeneratedMap(profile);

        var (raftDirection, bridgeDirection) = PrepareConnectorShores(props, profile);

        if (!TryPlaceEdgeConnectorCaves(props))
        {
            return false;
        }
        if (!TryPlaceGeneratedLocations(props))
        {
            return false;
        }

        if (biome == Biome.VANILLALIKE)
        {
            // Every candidate starts from the profile; ROAD is added only after seeding.
            PlaceRandomTerrain(profile.Climate, profile.SeedTerrains, seedCountMaximum: 5);
        }

        var growthTerrains = profile.SeedTerrains.Append(Terrain.ROAD);

        if (!TryReserveContinentConnectors(profile, raftDirection, bridgeDirection)) return false;

        var growthClimate = TerrainGrowth.CreateDeathMountainClimate(profile.Climate, growthTerrains, biome, RNG);
        GrowTerrain(growthClimate);

        if (biome == Biome.CALDERA)
        {
            bool f = MakeCaldera(props.CanWalkOnWaterWithBoots);
            if (!f)
            {
                logger.LogDebug("MakeCaldera failed");
                return false;
            }
        }

        walkableTerrains.Add(Terrain.ROAD);

        if (!TryFinalizeContinentConnectors()) return false;


        if (!TryPlaceSpectacleRock()) return false;

        if (biome == Biome.CANYON || biome == Biome.DRY_CANYON
            || biome == Biome.ISLANDS || biome == Biome.COASTAL_ISLANDS)
        {
            _ = PlaceRegionCrossings(new RegionCrossingOptions
            {
                MaximumCrossings = 25,
                CrossingTerrain = riverT,
                Biome = biome,
                CanWalkOnWaterWithBoots = props.CanWalkOnWaterWithBoots
            });
        }

        if (!TryRepairAndValidateCaveExits())
        {
            return false;
        }
        if (!ValidateEdgeConnectorCaves(props))
        {
            return false;
        }

        //check bytes and adjust
        encodedSize = EncodeTerrainMap().Length;

        return true;
    }

    /// <summary>Seeds biome geometry, preserving the eastern sea and wandering island rows.</summary>
    private Terrain PrepareGeneratedMap(BiomeTerrainProfile profile)
    {
        InitializeGeneratedMap();
        Terrain riverT = profile.RiverTerrain;
        if (profile.EasternSeaStartColumn is { } seaStart)
        {
            for (int y = 0; y < MapRows; y++)
                for (int x = seaStart; x < MapColumns; x++)
                    map[y, x] = profile.PreplacedWater;
        }

        if (biome == Biome.ISLANDS)
        {
            FillMapBorder(profile.MapBorder);

            TerrainLayout.DrawIslandStripes(map, MapRows, MapColumns, profile.StripeTerrain, RNG);
        }
        else if (biome == Biome.CANYON || biome == Biome.DRY_CANYON)
        {
            walkableTerrains = profile.CanyonBankTerrains.Where(t => t != Terrain.MOUNTAIN).ToList();
            DrawCanyon(riverT, profile.Climate, profile.CanyonBankTerrains, profile.IsHorizontal);

        }
        else if (biome == Biome.COASTAL_ISLANDS)
        {
            walkableTerrains = profile.CanyonBankTerrains.Where(t => t != Terrain.MOUNTAIN).ToList();
            DrawCoastalIslands(profile);
        }
        else if (biome == Biome.CALDERA)
        {
            DrawCenterMountain(profile.IsHorizontal);
        }
        else if (biome == Biome.MOUNTAINOUS)
        {
            FillMapBorder(profile.MapBorder);
            DrawTerrainStripes(profile.StripeTerrain, profile.NearEdgeMargin, profile.FarEdgeMargin);
        }

        return riverT;
    }

    /// <summary>Searches up to 49 empty-area candidates per location, then paints locations and expands caves.</summary>
    private bool TryPlaceGeneratedLocations(RandomizerProperties props)
    {
        const int checkedSampleBudget = 49;
        foreach (Location location in AllLocations)
        {
            if (location.TerrainType == Terrain.BRIDGE || location == specRock || !location.CanShuffle)
            {
                continue;
            }
            var search = LocationPlacementSearch.FindEmptyArea(map, MapRows, MapColumns, RNG, checkedSampleBudget);
            if (search.Position is not { } position)
            {
                logger.LogDebug($"Could not find empty 3x3 spot for location {location.Name}");
                return false;
            }

            map[position] = location.TerrainType;
            location.Pos = position;
            if (location.TerrainType == Terrain.CAVE)
            {
                if (!TerraformCaveExpansion(props, location))
                {
                    logger.LogDebug($"TerraformCaveExpansion failed for {location.Name}");
                    return false;
                }
            }
            location.CanShuffle = false;
        }
        return true;
    }

    /// <summary>Publishes Spectacle Rock only after finding an eligible interior tile.</summary>
    private bool TryPlaceSpectacleRock()
    {
        if (!TryChooseSpectacleRockPosition(out var position)) return false;
        map[position] = Terrain.ROCK;
        specRock.Pos = position;
        specRock.CanShuffle = false;
        return true;
    }

    /// <summary>
    /// Chooses uniformly from eligible interior tiles without modifying terrain or locations.
    /// Connector reservations and cardinal cave neighbors are excluded; diagonal caves remain allowed.
    /// </summary>
    private bool TryChooseSpectacleRockPosition(out IntVector2 position)
    {
        List<IntVector2> candidates = [];
        for (int y = 1; y < MapRows - 1; y++)
            for (int x = 1; x < MapColumns - 1; x++)
            {
                var candidate = new IntVector2(x, y);
                if (!IsContinentConnectorReserved(candidate)
                    && walkableTerrains.Contains(map[candidate])
                    && IntVector2.CARDINALS.All(d => map[candidate + d] != Terrain.CAVE))
                    candidates.Add(candidate);
            }
        position = default;
        if (candidates.Count == 0) return false;

        // Keep the usual sampling path: changing its RNG consumption regressed
        // matched Caldera generation. The finite pool also guarantees a fallback.
        const int sampleBudget = 128;
        for (int sample = 0; sample < sampleBudget; sample++)
        {
            var proposal = new IntVector2(RNG.Next(MapColumns - 2) + 1, RNG.Next(MapRows - 2) + 1);
            if (!candidates.Contains(proposal)) continue;
            position = proposal;
            return true;
        }
        position = candidates[RNG.Next(candidates.Count)];
        return true;
    }

    /// <summary>Allocates visitation before routing validation, then stages terrain/separator and clears unused records.</summary>
    private bool TryValidateAndStageMap()
    {
        visitation = new bool[MapRows, MapColumns];

        if (!ValidateBasicRouting())
        {
            logger.LogDebug("ValidateBasicRouting failed");
            return false;
        }

        if (!TryStageTerrainMap()) return false;
        TerrainWrites.Put(RomMap.NORTH_SOUTH_SEPARATOR_DM, (byte)(northSouthEncounterSeparator + 30));

        ClearVisitationTiles();

        ClearUnusedLocationYBytes(Continent.DM, TerrainWrites.Put);
        return true;
    }

    /// <summary>Expands one provisional cave into its connected group, or paints a loose entrance.</summary>
    /// <remarks>All entrances in a DM group share one terrain choice. Failure may leave the first pair placed.</remarks>
    private bool TerraformCaveExpansion(RandomizerProperties props, Location location)
    {
        Terrain entranceTerrain = biome == Biome.VANILLALIKE ? Terrain.ROAD : climate.GetRandomTerrain(RNG, walkableTerrains);
        if (!props.SaneCaves || !connectionsDM.ContainsKey(location))
        {
            IntVector2 direction = IntVector2.CARDINALS.Sample(RNG);
            PlaceCave(location.Pos, direction, entranceTerrain);
            return true;
        }

        map[location.Pos] = Terrain.NONE;
        var (minimumSpacing, maximumSpacingExclusive) = biome.GetDeathMountainCaveSpacing();
        var primary = SaneCavePlacement.Find(map, MapRows, MapColumns, RNG, minimumSpacing, maximumSpacingExclusive, null);
        if (primary is not { } primaryPair) return false;

        var exits = connectionsDM[location];
        if (exits.Count == 1)
        {
            PlaceCavePair(location, exits[0], primaryPair, () => entranceTerrain);
            return true;
        }

        // Resolve all four records before publishing the first pair, retaining
        // the graph's exit order and the existing malformed-group failure timing.
        var primaryExit = exits[0];
        var companionEntrance = exits[1];
        var companionExit = exits[2];
        PlaceCavePair(location, primaryExit, primaryPair, () => entranceTerrain);

        var companion = FindFourWayCompanionPair(primaryPair, minimumSpacing, maximumSpacingExclusive);
        if (companion is not { } companionPair) return false;
        PlaceCavePair(companionEntrance, companionExit, companionPair, () => entranceTerrain);
        return true;
    }

    /// <summary>Finds two map positions to terraform into the second pair of a 4-way cave group.</summary>
    /// <remarks>
    /// PlaceCavePair has painted the entrances and surrounding mountains for
    /// primaryPair on the map when this is called.
    ///
    /// Samples a position within three tiles on each axis of primaryPair.First,
    /// at a Manhattan distance greater than two. If its 3x3 area is NONE terrain,
    /// samples a partner along primaryPair.DirectionIn with the supplied forward
    /// spacing and up to three tiles of lateral offset, also requiring a 3x3 NONE area.
    ///
    /// PlaceCavePair uses the result to terraform the second pair and update
    /// its Location coordinates.
    /// </remarks>
    /// <returns>The sampled positions, or null when the attempt budget is exhausted.</returns>
    private SaneCavePlacement? FindFourWayCompanionPair(SaneCavePlacement primaryPair, int minimumSpacing, int maximumSpacingExclusive)
    {
        var firstLeftCavePos = primaryPair.First;
        const int maximumAttempts = 1000;
        const int maximumInnerAttempts = 3; // try a few times per successful left cave
        Func<int> rollSpacing = () => RNG.Next(minimumSpacing, maximumSpacingExclusive);
        Func<IntVector2, bool> canPlacePartner = position => map.AllTerrainIn3x3Equals(position, Terrain.NONE);
        for (int leftAttempt = 0; leftAttempt < maximumAttempts; leftAttempt++)
        {
            var newLeftPos = firstLeftCavePos + new IntVector2(RNG.Next(-3, 4), RNG.Next(-3, 4));
            if (firstLeftCavePos.ManhattanDistance(newLeftPos) <= 2) { continue; }
            if (!map.AllTerrainIn3x3Equals(newLeftPos, Terrain.NONE)) { continue; }

            var partner = SaneCavePlacement.SamplePartner(RNG, newLeftPos, primaryPair.DirectionIn, maximumInnerAttempts, rollSpacing, canPlacePartner);
            if (partner is { } newRightPos)
            {
                return new(newLeftPos, newRightPos, primaryPair.DirectionIn);
            }
        }
        return null;
    }

    private bool MakeCaldera(bool canWalkOnWaterWithBoots)
    {
        if (!TryChooseCalderaCenter(out var center)) return false;
        var water = canWalkOnWaterWithBoots ? Terrain.WALKABLEWATER : Terrain.WATER;
        CalderaTerrain.DrawLake(map, center, isHorizontal, water, RNG);
        return TryPlaceCalderaCaves(center);
    }

    // DM chooses after cave placement and terrain growth. Its wider vertical
    // window includes additional mountain openings present at this stage.
    private bool TryChooseCalderaCenter(out IntVector2 center)
    {
        var mapCenter = new IntVector2(MapColumns / 2, MapRows / 2);
        var minimum = isHorizontal
            ? new IntVector2(mapCenter.X - 5, Math.Max(7, mapCenter.Y - 5))
            : new IntVector2(Math.Max(7, mapCenter.X - 11), mapCenter.Y - 5);
        var maximum = isHorizontal
            ? new IntVector2(mapCenter.X + 5, Math.Min(mapCenter.Y + 5, MapRows - 8))
            : new IntVector2(Math.Min(mapCenter.X + 9, MapColumns - 8), mapCenter.Y + 5);
        return CalderaTerrain.TryChooseCenter(map, RNG, minimum, maximum, out center);
    }

    private bool TryPlaceCalderaCaves(IntVector2 center)
    {
        // caves have already been shuffled by being placed on the map.
        // pick the closest ones to the caldera center to minimize having
        // overlap errors.
        var validCaves = connectionsDM
            .Where(kvp => kvp.Value.Count == 1)
            .OrderBy(kvp => DistanceSquared(kvp.Key, center.X, center.Y))
            .ToList();
        var first = validCaves[0];
        var second = validCaves.First(kvp => kvp.Key != first.Key && kvp.Key != first.Value[0]);
        var primaryPair = (Left: first.Key, Right: first.Value[0]);
        var secondaryPair = (Left: second.Key, Right: second.Value[0]);

        // Unlike West, DM reserves both pairs before either passage search.
        primaryPair.Left.CanShuffle = false;
        primaryPair.Right.CanShuffle = false;
        secondaryPair.Left.CanShuffle = false;
        secondaryPair.Right.CanShuffle = false;
        map[primaryPair.Left.Pos] = Terrain.MOUNTAIN;
        map[secondaryPair.Left.Pos] = Terrain.MOUNTAIN;
        map[primaryPair.Right.Pos] = Terrain.MOUNTAIN;
        map[secondaryPair.Right.Pos] = Terrain.MOUNTAIN;

        var outward = isHorizontal ? DirectionExtensions.RandomHorizontal(RNG) : DirectionExtensions.RandomVertical(RNG);
        if (!TryPlaceCalderaCavePair(center, outward, primaryPair)) return false;
        return TryPlaceCalderaCavePair(center, outward.Reverse(), secondaryPair);
    }

    protected override List<Location> GetPathingStarts()
    {
        return
            connectionsDM.Keys.Where(i => i.Reachable)
            .Union(GetContinentConnections().Where(i => i.Reachable))
            .ToList();
    }

    private protected override IReadOnlyList<Location> GetLocalConnectionExits(Location location)
        => connectionsDM.TryGetValue(location, out var exits) ? exits : [];

    public override string GetName()
    {
        return "Death Mountain";
    }

    public override IEnumerable<Location> RequiredLocations(bool hiddenPalace, bool hiddenKasuto)
    {
        HashSet<Location> requiredLocations = new()
        {
            hammerCave,
            specRock
        };
        requiredLocations.AddRange(connectionsDM.Keys);
        requiredLocations.AddRange(GetContinentConnections());

        return requiredLocations.Where(i => i != null);
    }

    public override string GenerateSpoiler()
    {
        StringBuilder sb = new();
        sb.AppendLine("DEATH MOUNTAIN: ");
        AppendOverworldItemSpoilerLine(sb, hammerCave);
        AppendOverworldItemSpoilerLine(sb, specRock);

        sb.AppendLine();
        return sb.ToString();
    }

    public override void ResetCollectables(RandomizerProperties props)
    {
        ResetOverworldCollectable(hammerCave, Collectable.HAMMER, props.ShuffleOverworldItems);
        ResetOverworldCollectable(specRock, Collectable.MAGIC_CONTAINER, props.ShuffleOverworldItems);
    }
}
