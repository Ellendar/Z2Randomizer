using System;
using System.Collections.Frozen;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using NLog;
using Z2Randomizer.RandomizerCore.Diagnostics;
using Z2Randomizer.RandomizerCore.Enemy;
using Z2Randomizer.RandomizerCore.Sidescroll.Town;

namespace Z2Randomizer.RandomizerCore.Overworld;

public abstract partial class World
{
    protected readonly Logger logger = LogManager.GetCurrentClassLogger();

    public Dictionary<Location, Location> connections;
    public int sideviewPtrTable { get; protected set; }
    public int sideviewBank { get; protected set; }
    public int enemyPtr { get; protected set; }
    public GroupedEnemiesBase groupedEnemies { get; protected set; }
    public IReadOnlyList<int> overworldEncounterMaps { get; protected set; }
    public IReadOnlyList<int> overworldEncounterMapDuplicate { get; protected set; } = [];
    public IReadOnlyList<int> nonEncounterMaps { get; protected set; }
    public OverworldMap map;
    public const int MAP_ROWS_FULL = 75;
    public const int MAP_COLS_FULL = WorldMapCodec.StoredColumns;
    private const int MAX_LOCATION_PLACEMENT_ATTEMPTS = 5000;
    public int MapRows { get; init; }
    public int MapColumns { get; init; }
    protected List<Terrain> randomTerrainFilter;
    protected List<Terrain> walkableTerrains;
    internal int MapRomOffset { get; }
    internal int MapRomCapacity { get; }
    protected List<Location> unimportantLocs;
    public Biome biome { get; init; }
    protected bool isHorizontal;
    protected int VANILLA_MAP_ADDR;
    public int baseAddr;
    public Continent continentId;
    protected Climate climate;
    private GeneratedLocationPlan? generatedLocationPlan;
    /// south encounter version will be used when Link's Y >= this
    public int northSouthEncounterSeparator { get; init; }

    protected Random RNG;

    protected abstract List<Location> GetPathingStarts();
    public abstract bool Terraform(RandomizerProperties props, ROM rom);
    public abstract string GetName();

    //DEBUG / STATS
    public static int failedOnPlaceLocations = 0;
    public static int failedOnBaguPlacement = 0;
    public static int TerrainGrowthAttempts = 0;
    public static int failedOnRaftPlacement = 0;
    public static int failedOnBridgePlacement = 0;
    public static int failedOnConnectorConflict = 0;
    public static int failedOnIslandConnection = 0;
    public static int failedOnMakeCaldera = 0;
    public static int failedOnConnectIslands = 0;
    public static int PlaceCaveCount = 0;


    public List<Location> AllLocations { get; }
    public List<Location> RemovedLocations { get; }
    public Dictionary<Terrain, List<Location>> Locations { get; set; }
    protected Location? startLocation { get; set; }

    // subset of walkable terrains that we would expect to be next to a trap tile
    protected static readonly FrozenSet<Terrain> TRAP_PATH_VALID_TERRAIN = [Terrain.DESERT, Terrain.GRASS, Terrain.ROAD, Terrain.LAVA];
    protected static readonly FrozenSet<Terrain> TRAP_PATH_BLOCKING_TERRAIN = [Terrain.MOUNTAIN, Terrain.WATER];

    //The analyzer doesn't understand the design pattern of an inaccessable constructor as a forced inheritance anchor
    //maybe that's just a javaism it doesn't like. In either case, I don't care.
#pragma warning disable CS8618
    protected World(Random r, int mapRomOffset, int mapRomCapacity = RomMap.OVERWORLD_MAP_CAPACITY)
#pragma warning restore CS8618 
    {
        RNG = r;
        MapRomOffset = mapRomOffset;
        MapRomCapacity = mapRomCapacity;

        connections = new Dictionary<Location, Location>();
        Locations = new Dictionary<Terrain, List<Location>>();
        foreach (Terrain Terrain in Enum.GetValues(typeof(Terrain)))
        {
            Locations.Add(Terrain, new List<Location>());
        }
        AllLocations = [];
        RemovedLocations = [];
        unimportantLocs = [];
        AllReached = false;
    }

    public static void ResetStats()
    {
        failedOnPlaceLocations = 0;
        failedOnBaguPlacement = 0;
        TerrainGrowthAttempts = 0;
        failedOnRaftPlacement = 0;
        failedOnBridgePlacement = 0;
        failedOnConnectorConflict = 0;
        failedOnIslandConnection = 0;
        failedOnMakeCaldera = 0;
        failedOnConnectIslands = 0;
    }

    public void AddLocation(Location location)
    {
        if (location.TerrainType == Terrain.WALKABLEWATER)
        {
            Locations[Terrain.SWAMP].Add(location);
        }
        else
        {
            Locations[location.TerrainType].Add(location);
        }
        AllLocations.Add(location);
    }

    protected void ShuffleLocations(List<Location> locationsToShuffle)
    {
        List<Location> shufflableLocations = locationsToShuffle.Where(i => i.CanShuffle && i.AppearsOnMap).ToList();
        for (int i = shufflableLocations.Count - 1; i > 0; i--)
        {
            int n = RNG.Next(i + 1);
            Swap(shufflableLocations[i], shufflableLocations[n]);
        }
    }


    protected void Swap(Location l1, Location l2)
    {
        (l2.Xpos, l1.Xpos) = (l1.Xpos, l2.Xpos);
        (l2.Y, l1.Y) = (l1.Y, l2.Y);
        (l2.IsPassthrough, l1.IsPassthrough) = (l1.IsPassthrough, l2.IsPassthrough);
    }

    protected void RemoveLocations(ICollection<Location> locationsToRemove)
    {
        foreach (var loc in locationsToRemove)
        {
            loc.Disable();

            AllLocations.Remove(loc);
            RemovedLocations.Add(loc);
            foreach (List<Location> ls in Locations.Values)
            {
                ls.Remove(loc);
            }
            connections.Remove(loc);
        }
    }

    /// <summary>
    /// Fill out the connections map based on the location tables in ROM, so they don't need to be hard-coded. Uses the EntranceNumber to find all entrances leading to a common location.
    /// </summary>
    /// <param name="ignoreLocs">Entrances that should not be considered. This is used to filter out cases where multiple logically distinct locations share a single 4-page map such as King's Tomb and Mido.</param>
    /// <param name="allConns">A map supporting more than 2 entrances per location, if necessary (DM).</param>
    protected void CreateConnections(
        HashSet<LocationID>? ignoreLocs = null,
        Dictionary<Location, List<Location>>? allConns = null)
    {
        // Group all entrances by location
        SortedDictionary<LocationID, HashSet<int>> locsEntrances = new();
        foreach (var loc in AllLocations
            .Where(loc => ignoreLocs == null || !ignoreLocs.Contains(loc.ID)))
        {
            var baseLid = loc.ID - loc.EntranceNumber;
            if (!locsEntrances.TryGetValue(baseLid, out var entrs))
            {
                entrs = new();
                locsEntrances[baseLid] = entrs;
            }

            entrs.Add(loc.EntranceNumber);
        }

        // Add locations with more than one entrance to the maps
        foreach (var (lid, entrs)
            in locsEntrances.Where(kv => kv.Value.Count > 1))
        {
            var allEntrs = entrs.Select(i => GetLocation(lid + i)).ToList();
            allEntrs.Sort((a, b) => a.ID.CompareTo(b.ID));

            for (int i = 0; i < entrs.Count; i++)
            {
                var entrsList = allEntrs.ToList();
                entrsList.RemoveAt(i);

                if (entrsList.Count == 1)
                    connections[allEntrs[i]] = entrsList[0];
                if (allConns != null)
                    allConns[allEntrs[i]] = entrsList;
            }
        }
    }

    /// <summary>Moves a local connection entrance into the given vanilla shuffle area.</summary>
    private protected void PlaceVanillaShuffleConnection(IReadOnlyList<Location> area)
    {
        if (connections.Count == 0) return;

        Location site = area[RNG.Next(area.Count)];
        Location entrance = connections.Keys.ElementAt(RNG.Next(connections.Count));
        for (int retry = 0; retry < connections.Count && (!entrance.CanShuffle || !site.CanShuffle); retry++)
        {
            site = area[RNG.Next(area.Count)];
            entrance = connections.Keys.ElementAt(RNG.Next(connections.Count));
        }

        // Vanilla Shuffle uses the final pair even when the retries run out.
        Swap(site, entrance);
        entrance.CanShuffle = false;
    }

    public Location GetLocation(LocationID locId)
    {
        return AllLocations.FirstOrDefault(i => i.ID == locId)
            ?? throw new Exception($"Failed to find Location with ID {locId}");
    }

    /// <summary>
    /// Returns the (first) location at the specified position,
    /// or <c>null</c> if no location exists there.
    /// </summary>
    protected Location? GetLocationAt(IntVector2 pos)
    {
        return AllLocations.FirstOrDefault(i => i.Pos == pos);
    }

    /// <summary>
    /// Returns any locations found within the 3x3 area centered on
    /// <paramref name="center"/>, including diagonal neighbors and the
    /// center position itself.
    /// </summary>
    protected IEnumerable<Location> LocationsIn3x3Area(IntVector2 center)
    {
        return AllLocations.Where(i => i.Pos.X >= center.X - 1 && i.Pos.X <= center.X + 1 &&
                                       i.Pos.Y >= center.Y - 1 && i.Pos.Y <= center.Y + 1);
    }

    /// <summary>
    /// Returns any locations found at <paramref name="center"/> or one
    /// tile away in a cardinal direction. (Diagonal positions are not included.)
    ///
    /// TLDR; Finds any location within a "plus sign" formation.
    /// </summary>
    protected IEnumerable<Location> LocationsAtOrOrthogonalTo(IntVector2 center)
    {
        return AllLocations.Where(i => (i.Pos - center).ManhattanLength <= 1);
    }

    /// <summary>
    /// Returns any locations found exactly one tile away from <paramref name="center"/>
    /// in any cardinal direction. (Diagonal positions are not included.)
    /// </summary>
    protected IEnumerable<Location> LocationsOrthogonalTo(IntVector2 center)
    {
        return AllLocations.Where(i => (i.Pos - center).ManhattanLength == 1);
    }

    protected bool PlaceLocations(Terrain riverTerrain, bool saneCaves)
    {
        return PlaceLocations(riverTerrain, saneCaves, null, null);
    }
    protected bool PlaceLocations(Terrain crossingTerrain, bool saneCaves, Location? hiddenForestTile, Location? threeEyeRock)
    {
        int remainingSamples = MAX_LOCATION_PLACEMENT_ATTEMPTS;
        if (threeEyeRock is { CanShuffle: true })
        {
            throw new ImpossibleException("Three-Eye Rock must be excluded from ordinary location placement.");
        }
        foreach (Location location in AllLocations.Where(loc => loc.AppearsOnMap))
        {
            if ((location.TerrainType != Terrain.BRIDGE
                    && location.CanShuffle
                    && !unimportantLocs.Contains(location)
                    && !location.IsPassthrough)
                || location.AccessRequirements.HasHardRequirement(RequirementType.HAMMER))
            {
                int? excludedColumn = location == hiddenForestTile ? threeEyeRock?.Xpos : null;
                var search = LocationPlacementSearch.FindEmptyArea(map, MapRows, MapColumns, RNG,
                    remainingSamples, excludedColumn);
                remainingSamples -= search.Samples;
                if (search.Position is not { } position)
                {
                    logger.LogDebug($"No empty placement area for {location.ID}");
                    return false;
                }
                int x = position.X, y = position.Y;

                map[y, x] = location.TerrainType;

                //If the location is a cave, connect it
                if (location.TerrainType == Terrain.CAVE)
                {
                    Terrain entranceTerrain = climate.GetRandomTerrain(RNG, walkableTerrains);

                    if (saneCaves && connections.ContainsKey(location))
                    {
                        PlaceCaveCount++;
                        map[y, x] = Terrain.NONE;
                        if (!PlaceSaneCave(location, crossingTerrain))
                        {
                            return false;
                        }
                    }
                    else
                    {
                        List<Direction> caveDirections = new() { Direction.NORTH, Direction.SOUTH, Direction.EAST, Direction.WEST };
                        Direction direction = caveDirections[RNG.Next(4)];
                        PlaceCaveCount++;
                        PlaceCave(new IntVector2(x, y), direction.ToIntVector2(), entranceTerrain);
                        location.Xpos = x;
                        location.Y = y;
                        location.CanShuffle = false;
                    }
                }
                else
                {
                    // Palaces accept the first sample; ordinary locations must
                    // stand out from their surrounding terrain.
                    Terrain surroundingTerrain;
                    do
                    {
                        surroundingTerrain = climate.GetRandomTerrain(RNG, walkableTerrains);
                    } while (location.TerrainType != Terrain.PALACE && surroundingTerrain == location.TerrainType);
                    TerrainLayout.PaintSurroundings(map, position, surroundingTerrain);
                    location.Pos = position;
                    location.CanShuffle = false;
                }
            }
        }
        return true;
    }

    /// <summary>
    /// Places a cave onto the map, setting the tile to the cave, and ensuring the 3 tiles facing the entrance are
    /// the passable type indicated by entranceTerrain
    /// </summary>
    /// <param name="pos">X,Y coordinate of the cave entrance</param>
    /// <param name="forward">The direction you press to enter the cave.</param>
    /// <param name="entranceTerrain">Walkable Terrain type to set in front of the entrance.</param>
    protected void PlaceCave(IntVector2 pos, IntVector2 forward, Terrain entranceTerrain)
    {
        IntVector2 side = forward.Perpendicular();

        foreach (var d in IntVector2.DIRECTIONS)
        {
            map[pos + d] = Terrain.MOUNTAIN;
        }

        map[pos - forward] = entranceTerrain;
        map[pos - forward + side] = entranceTerrain;
        map[pos - forward - side] = entranceTerrain;

        map[pos] = Terrain.CAVE;
    }

    protected bool PlaceSaneCave(Location location, Terrain crossingTerrain)
    {
        var (minimumSpacing, maximumSpacing) = biome switch
        {
            Biome.ISLANDS or Biome.MOUNTAINOUS => (10, 20),
            Biome.VOLCANO or Biome.CALDERA => (5, 20),
            Biome.VANILLA or Biome.VANILLA_SHUFFLE or Biome.VANILLALIKE or Biome.CANYON or Biome.DRY_CANYON => (6, 18),
            _ => throw new ArgumentOutOfRangeException(nameof(biome)),
        };
        Terrain? requiredCrossing = biome == Biome.VOLCANO ? null : crossingTerrain;
        var placement = SaneCavePlacement.Find(map, MapRows, MapColumns, RNG, minimumSpacing, maximumSpacing, requiredCrossing);
        if (placement is not { } pair)
        {
            logger.LogDebug($"No compatible cave pair across {requiredCrossing} for {location.ID}");
            return false;
        }

        PlaceCavePair(location, connections[location], pair,
            () => climate.GetRandomTerrain(RNG, walkableTerrains));
        return true;
    }

    /// <summary>Positions and paints a linked cave pair, then aligns its sideview entrance roles.</summary>
    /// <remarks>
    /// The chooser runs separately for each entrance after positions are set. DM
    /// supplies its one shared group terrain; West/East choose independently.
    /// </remarks>
    private protected void PlaceCavePair(Location first, Location second, SaneCavePlacement placement, Func<Terrain> chooseEntranceTerrain)
    {
        first.CanShuffle = false;
        first.Pos = placement.First;
        second.CanShuffle = false;
        second.Pos = placement.Second;
        PlaceCave(placement.First, placement.DirectionIn, chooseEntranceTerrain());
        PlaceCave(placement.Second, -placement.DirectionIn, chooseEntranceTerrain());
        AlignCavePositionsLeftToRight(placement.DirectionIn, first, second);
    }

    /// swaps the positions of two caves if the cave that is going East/South
    /// in the Overworld is not the cave that enters the sideview from the left
    protected static void AlignCavePositionsLeftToRight(IntVector2 location1Direction, Location location1, Location location2)
    {
        bool overworldGoingRight = location1Direction == IntVector2.EAST || location1Direction == IntVector2.SOUTH;
        bool sideviewGoingRight = location1.MapPage < location2.MapPage;
        if (overworldGoingRight != sideviewGoingRight)
        {
            // should swap the entrances so left-to-right aligns
            (location1.Pos, location2.Pos) = (location2.Pos, location1.Pos);
        }
    }

    public void PlaceHiddenLocations(LessImportantLocationsOption lessImportantLocationsOption)
    {
        switch (lessImportantLocationsOption)
        {
            case LessImportantLocationsOption.ISOLATE:
                break;
            case LessImportantLocationsOption.REMOVE:
                foreach (Location location in unimportantLocs)
                {
                    if (location.CanShuffle)
                    {
                        location.Disable();
                    }
                }
                break;
            case LessImportantLocationsOption.HIDE:
                foreach (Location location in unimportantLocs)
                {
                    if (location.CanShuffle)
                    {
                        var search = LocationPlacementSearch.FindHiddenPosition(map, MapRows, MapColumns,
                            location, AllLocations, continentConnectorReservations, RNG);
                        location.Pos = search.Position ?? IntVector2.NOT_ON_MAP;
                        location.CanShuffle = false;
                    }
                }
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(lessImportantLocationsOption));
        }
    }

    protected List<Direction> NextToWaterDirections(int x, int y, Terrain[] crossingTerrains)
    {
        List<Direction> directions = [];
        if (walkableTerrains.Contains(map[y, x]) && !crossingTerrains.Contains(map[y, x]))
        {
            if (crossingTerrains.Contains(map[y + 1, x]))
            {
                directions.Add(Direction.SOUTH);
            }

            if (crossingTerrains.Contains(map[y - 1, x]))
            {
                directions.Add(Direction.NORTH);
            }

            if (crossingTerrains.Contains(map[y, x + 1]))
            {
                directions.Add(Direction.EAST);
            }

            if (crossingTerrains.Contains(map[y, x - 1]))
            {
                directions.Add(Direction.WEST);
            }
        }
        return directions;
    }

    /// <summary>Publishes shared terrain growth, retaining the World attempt counter and compatibility result.</summary>
    protected bool GrowTerrain(Climate climate)
    {
        TerrainGrowthAttempts++;
        map = TerrainGrowth.Grow(map, MapRows, MapColumns, climate.DistanceCoefficients, RNG);
        return true;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int DistanceSquared(Location l, int x2, int y2)
    {
        int dx = l.Xpos - x2;
        int dy = l.Y - y2;
        return dx * dx + dy * dy;
    }

    /// <summary>Seeds empty cells using climate weights restricted to the supplied membership filter.</summary>
    protected void PlaceRandomTerrain(Climate climate, IEnumerable<Terrain> allowedTerrains, int seedCountMaximum = 500)
    {
        var result = TerrainGrowth.Seed(map, MapRows, MapColumns, climate, allowedTerrains, RNG, seedCountMaximum);
        if (result.Exhausted) logger.Trace("Exhaustion limit reached");
    }

    /// <summary>Encodes once without changing the map, locations, or a ROM destination.</summary>
    private protected EncodedWorldMap EncodeTerrainMap(MapEncodingOptions options = default)
    {
        // An east shore route ends at the logical edge, but the ROM stores 64 columns.
        bool eastShoreConnector = continentConnectorsFinalized && continentConnectorReservations.Any(reservation => reservation.Edge == Direction.EAST);
        return new(map, options with { UseWaterPadding = eastShoreConnector });
    }

    /// <summary>Stages the final encoding for the active terrain candidate if it fits the reserved region.</summary>
    /// <param name="options">Map encoding options such as hidden tile positions.</param>
    private protected bool TryStageTerrainMap(MapEncodingOptions options = default)
    {
        if (continentConnectorReservations.Count > 0 && !continentConnectorsFinalized)
        {
            TraceTerrainRejection("connector-not-finalized");
            return false;
        }
        if (!ContinentConnectorReservationsIntact()) return false;
        return TryStageEncodedTerrainMap(EncodeTerrainMap(options));
    }

    /// <summary>Queues an existing encoding and filler if they fit the world's ROM map region.</summary>
    private protected bool TryStageEncodedTerrainMap(EncodedWorldMap encodedMap)
    {
        if (encodedMap.TryStage(TerrainWrites, MapRomOffset, MapRomCapacity)) return true;

        TraceTerrainRejection("map-too-large", $"bytes={encodedMap.Length} capacity={MapRomCapacity}");
        return false;
    }

    /// <summary>Zeros the raw Y byte for each ROM location slot without default terrain, making them not enterable in game.</summary>
    /// <remarks>Many of these slots have no Location object. Any loaded connectors in these slots
    /// retain their model coordinates for final location serialization.</remarks>
    private protected static void ClearUnusedLocationYBytes(Continent continent, Action<int, byte> writeByte)
    {
        foreach (var id in LocationIDUtils.Enumerate(continent))
        {
            if (!id.TryGetDefaultTerrain(out _))
            {
                writeByte(id.GetRomOffset(), 0);
            }
        }
    }

    /// <summary>
    /// Restores shufflability, original passthrough flags, and vanilla or blank
    /// placement coordinates. Callers own the reset scope; East additionally
    /// restores requirements and terrain in its own per-location loop.
    /// </summary>
    protected void PrepareTerraformLocations()
    {
        foreach (Location location in AllLocations)
        {
            PrepareTerraformLocation(location);
        }
    }

    private protected void PrepareTerraformLocation(Location location)
    {
        location.CanShuffle = true;
        location.IsPassthrough = location.WasPassthrough;
        location.ResetCoords(biome.UsesVanillaMap());
    }

    /// <summary>Loads this continent's vanilla terrain, retaining the full-map dimension assertions.</summary>
    protected void LoadVanillaTerrain(ROM rom)
    {
        Debug.Assert(MapRows == MAP_ROWS_FULL);
        Debug.Assert(MapColumns == MAP_COLS_FULL);
        map = new OverworldMap(rom.ReadVanillaMap(rom, VANILLA_MAP_ADDR, MapRows, MapColumns));
    }

    /// <summary>Loads vanilla terrain and shuffles its locations in Vanilla Shuffle.</summary>
    private protected void LoadAndShuffleVanillaMap(ROM rom, bool legacyVanillaShuffledLocations)
    {
        LoadVanillaTerrain(rom);
        if (biome == Biome.VANILLA_SHUFFLE)
        {
            ShuffleVanillaLocations(legacyVanillaShuffledLocations);
        }
    }

    /// <summary>Shuffles eligible locations and repaints their new tiles unless legacy shuffling is enabled.</summary>
    private protected void ShuffleVanillaLocations(bool legacyVanillaShuffledLocations)
    {
        ShuffleLocations(AllLocations);
        if (!legacyVanillaShuffledLocations)
        {
            RepaintVanillaShuffleTiles();
        }
    }

    private protected virtual void RepaintVanillaShuffleTiles() => RepaintLocationTiles();

    /// <summary>Paints location terrain at current coordinates in location order.</summary>
    protected void RepaintLocationTiles(bool markPassthrough = false)
    {
        foreach (Location location in AllLocations)
        {
            map[location.Y, location.Xpos] = location.TerrainType;
            if (markPassthrough) location.IsPassthrough = true;
        }
    }

    private protected BiomeTerrainProfile ResolveTerrainProfile(RandomizerProperties props)
        => BiomeTerrainProfile.Resolve(continentId, biome, props.CanWalkOnWaterWithBoots, climate, isHorizontal);

    private protected delegate bool GenerateTerrainAttempt(RandomizerProperties props, BiomeTerrainProfile profile, out int encodedSize);

    /// <summary>Retries oversized encodings and rejects failed placement attempts immediately.</summary>
    private protected bool TryGenerateTerrainWithinBudget(RandomizerProperties props, BiomeTerrainProfile profile,
        GenerateTerrainAttempt generateAttempt)
    {
        while (generateAttempt(props, profile, out int encodedSize))
        {
            if (encodedSize <= MapRomCapacity) return true;

            TraceTerrainRejection("encoding-retry", $"bytes={encodedSize} capacity={MapRomCapacity}");
        }

        return false;
    }

#if Z2R_DIAGNOSTICS
    private Dictionary<string, long>? terrainRejections;
#endif

    [Conditional("Z2R_DIAGNOSTICS")]
    private protected void TraceTerrainRejection(string reason, string detail = "")
    {
#if Z2R_DIAGNOSTICS
        terrainRejections ??= new();
        long count = terrainRejections.GetValueOrDefault(reason) + 1;
        terrainRejections[reason] = count;
        if (Diag.IsSample(count))
        {
            Diag.Trace("terrain.reject", $"world={continentId} biome={biome} reason={reason} count={count} {detail}".TrimEnd());
        }
#endif
    }

    /// <summary>
    /// Copies resolved membership filters into the mutable lists used by legacy
    /// placement helpers. This does not reset climate or locations.
    /// </summary>
    private protected void ApplyTerrainProfile(BiomeTerrainProfile profile)
    {
        walkableTerrains = profile.InitialPlacementTerrains.ToList();
        randomTerrainFilter = profile.SeedTerrains.ToList();
    }

    /// <summary>
    /// Restores stable location inputs and fresh terrain filters for every generated
    /// candidate, including compression retries. Disposal detaches the CanShuffle
    /// adapter. The enclosing candidate owns publication; RNG is never rewound.
    /// </summary>
    internal IDisposable BeginGeneratedTerrainAttempt(BiomeTerrainProfile profile)
    {
        generatedLocationPlan ??= new GeneratedLocationPlan(AllLocations, unimportantLocs);
        terrainCandidate?.RestartGeneratedAttempt();
        continentConnectorReservations = [];
        continentConnectorsFinalized = false;
        unimportantLocs = generatedLocationPlan.CreateMinorLocations();
        ApplyTerrainProfile(profile);
        return generatedLocationPlan.BeginAttempt();
    }

    /// <summary>Starts a logical-sized generated map with unseeded terrain, without resetting locations.</summary>
    protected void InitializeGeneratedMap()
    {
        map = new OverworldMap(MapRows, MapColumns, Terrain.NONE);
    }

    /// <summary>Paints the logical perimeter, leaving the interior and any backing-map padding intact.</summary>
    protected void FillMapBorder(Terrain terrain)
        => TerrainLayout.PaintBorder(map, MapRows, MapColumns, terrain);

    protected bool DrawOcean(Direction direction, Terrain oceanTerrain)
    {
        var result = TerrainLayout.DrawOcean(map, MapRows, MapColumns, direction, oceanTerrain,
            biome is Biome.MOUNTAINOUS or Biome.CALDERA, RNG);
        if (result == OceanPaintResult.Blocked)
        {
            logger.LogDebug($"Ocean painting blocked on the {direction} edge");
            return false;
        }
        if (result == OceanPaintResult.BackingMapBoundsFailure)
        {
            logger.Error($"Ocean painting exceeded the backing map on the {direction} edge");
        }
        // Preserve the protected adapter's legacy success after a caught bounds failure.
        return true;
    }

    /// <summary>
    /// Seeds two or three distinct columns, then rows, replacing only NONE tiles.
    /// Margins bound the selectable positions (the far bound is exclusive). Callers
    /// retain asymmetric biome bounds and place reserved features before this call.
    /// Each axis must offer at least three positions. Counts are rolled before positions
    /// and duplicate positions are retried, preserving the existing RNG sequence.
    /// Death Mountain's wandering island rows use a separate algorithm.
    /// </summary>
    protected void DrawTerrainStripes(Terrain terrain, int nearEdgeMargin, int farEdgeMargin)
        => TerrainLayout.DrawStripes(map, MapRows, MapColumns, terrain, nearEdgeMargin, farEdgeMargin, RNG);

    protected void DrawRiver(bool canWalkOnWaterWithBoots)
        => TerrainLayout.DrawRiver(map, MapRows, MapColumns,
            canWalkOnWaterWithBoots ? Terrain.WALKABLEWATER : Terrain.WATER, IsReserved, RNG);

    /// <summary>Seeds canyon banks using explicit climate and allowed bank terrain, including mountain.</summary>
    protected void DrawCanyon(Terrain riverT, Climate climate, IEnumerable<Terrain> bankTerrains, bool horizontal)
        => TerrainLayout.DrawCanyon(map, MapRows, MapColumns, riverT, climate, bankTerrains, horizontal, RNG);

    protected void DrawCenterMountain(bool horizontal)
        => TerrainLayout.DrawCentralMountain(map, MapRows, MapColumns, horizontal);

    /// <summary>
    /// Returns true iff position is within the bounds of the map. 
    /// Useful to avoid index out of bounds errors.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    protected bool WithinMapBounds(IntVector2 pos) => pos.IsWithinBounds(MapColumns, MapRows);

    /// <summary>
    /// Useful variant to verify that a coordinate is some margin away
    /// from the edge of the map.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    protected bool WithinMapBounds(IntVector2 pos, int margin) => pos.IsWithinBounds(MapColumns, MapRows, margin);

    public abstract void ResetCollectables(RandomizerProperties props);

    /// <summary>
    /// Restores a single collectable stored directly on a <see cref="Location"/> and sets its shuffle eligibility.
    /// Examples include the Hammer in Hammer Cave and the heart container on East Hyrule's desert tile.
    /// </summary>
    private protected static void ResetOverworldCollectable(Location location, Collectable collectable, bool shufflable)
    {
        location.SetCollectables([collectable]);
        location.CollectablesAreShufflable = shufflable;
    }

    /// <summary>
    /// Restores the collectable on a town map and sets its shuffle eligibility.
    /// The collectable can be an item, spell, or sword technique.
    /// </summary>
    private protected static void ResetTownCollectable(TownMap townMap, Collectable collectable, bool shufflable)
    {
        townMap.Collectable = collectable;
        townMap.CollectableIsShufflable = shufflable;
    }

    /// <summary>Restores the default collectables for the palace currently attached to the location.</summary>
    private protected static void ResetPalaceCollectables(Location location, RandomizerProperties props)
    {
        var palace = location.Palace!;
        location.SetCollectables(palace.GetVanillaCollectables(props.PalaceItemRoomCounts[palace.Number - 1]));
    }

    /// <summary>Clears passthrough on the cave, town, and palace placement buckets after Vanilla Shuffle succeeds.</summary>
    /// <remarks>
    /// Locations can change terrain during shuffling, so this uses their placement buckets.
    /// Overrides call the base implementation before applying continent-specific rules.
    /// </remarks>
    public virtual void DisableDisallowedPassthroughs()
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
    }

    /// <summary>Copies and shuffles the full cave pool, then applies the shared blocker eligibility rule.</summary>
    /// <remarks>
    /// With connection blocking disabled, the existing rule retains paired caves
    /// and the loaded continent cave connectors, excluding unpaired caves.
    /// Continents own quotas, additional exclusions and spacing between blockers.
    /// </remarks>
    protected IReadOnlyList<Location> GetShuffledCaveBlockerCandidates(bool connectionsCanBeBlocked)
    {
        List<Location> caves = [.. Locations[Terrain.CAVE]];
        caves.FisherYatesShuffle(RNG);
        if (!connectionsCanBeBlocked)
            caves.RemoveAll(cave => !connections.ContainsKey(cave) && cave != cave1 && cave != cave2);
        return caves;
    }

    /// <summary>Checks the current map and location positions, then moves the cave and paints its blocker.</summary>
    /// <returns>False without painting or moving the cave when its footprint is unavailable.</returns>
    protected bool TryPlaceCaveBlocker(Location cave, Terrain blockerTerrain)
    {
        var inward = ValidBlockingCavePosition(cave.Pos);
        if (inward == null) return false;
        PlaceCaveBlocker(cave, inward.Value, blockerTerrain);
        return true;
    }

    /// <summary>Finds the inward direction for moving a cave to make room for a blocker, or returns null.</summary>
    /// <remarks>
    /// Requires an interior cave with exactly three mountain neighbors. Protects
    /// locations and continent-connector reservations on the footprint, approach
    /// and three tiles behind the new mountain. Reads live state without painting.
    /// </remarks>
    public IntVector2? ValidBlockingCavePosition(IntVector2 pos)
    {
        if (!WithinMapBounds(pos, 1)) { return null; }

        bool isMountain(IntVector2 pos) => map[pos.Y, pos.X] == Terrain.MOUNTAIN;
        bool checkForExistingLocation(IntVector2 pos)
        {
            if (IsContinentConnectorReserved(pos)) return true;
            if (WithinMapBounds(pos))
            {
                var loc = GetLocationAt(pos);
                return loc != null;
            }
            return false;
        }

        var openDirs = IntVector2.CARDINALS
            .Where(d => !isMountain(pos - d))
            .ToList();

        // only block caves with a single path in
        if (openDirs.Count != 1) { return null; }
        var dir = openDirs[0];
        var sideways = dir.Perpendicular();

        // make sure there is nothing right in front of our new blocker
        var beforeBlockerPos = pos - 2 * dir;
        if (checkForExistingLocation(beforeBlockerPos)) { return null; }

        // make sure there is nothing where our new mountain tile will be
        var newMountainPos = pos + 2 * dir;
        if (checkForExistingLocation(newMountainPos)) { return null; }

        // check if the terraforming would destroy a 3x3 isolated tile formation
        for (int i = -1; i <= 1; i++)
        {
            var behindNewMountain = pos + 3 * dir + i * sideways;
            if (checkForExistingLocation(behindNewMountain)) { return null; }
        }

        var newCavePos = pos + dir;
        if (checkForExistingLocation(newCavePos)) { return null; }
        var newBlockPos = pos - dir;
        if (checkForExistingLocation(newBlockPos)) { return null; }

        return dir;
    }

    /// <summary>Moves a cave one tile inward, leaving road at its old position and a blocker on its approach.</summary>
    /// <remarks>
    /// The caller supplies a checked inward direction from <see cref="ValidBlockingCavePosition"/>.
    /// Paints the backing mountain when it is in bounds; retains requirements,
    /// entrance metadata and scheduling/reachability flags.
    /// </remarks>
    public void PlaceCaveBlocker(Location cave, IntVector2 dir, Terrain blockerTerrain)
    {
        var pos = cave.Pos;
        var blockPos = pos - dir;
        var roadPos = pos;
        var newCavePos = pos + dir;
        var mountainPos = pos + 2 * dir;

        map[blockPos.Y, blockPos.X] = blockerTerrain;
        map[roadPos.Y, roadPos.X] = Terrain.ROAD;
        map[newCavePos.Y, newCavePos.X] = Terrain.CAVE;
        if (WithinMapBounds(mountainPos))
        {
            map[mountainPos.Y, mountainPos.X] = Terrain.MOUNTAIN;
        }
        cave.Pos = newCavePos;
    }

    /// If this tile is reserved, in the context of a river expanding to
    /// this tile. Most of these are probably logically impossible.
    /// The point is to stop a river opening up VoD.
    /// (This method is overridden in EastHyrule)
    protected virtual bool IsReserved(IntVector2 pos, Terrain terrain)
    {
        switch (terrain)
        {
            case Terrain.TOWN:
            case Terrain.CAVE:
            case Terrain.PALACE:
            case Terrain.BRIDGE:
            case Terrain.ROCK:
                return true;
        }

        return false;
    }

    /// <summary>
    /// Check if the position is suitable as a trap tile. It should<br/>
    /// - Be the center tile of a 3-tile long path.<br/>
    /// - There must be no special locations on these 3 tiles.<br/>
    /// - The perpendicular tiles should not be passable.
    /// </summary>
    /// <returns>
    /// Returns east-pointing vector if it's valid for horizontal passthrough,
    /// or south-pointing vector if it's valid for vertical passthrough,
    /// or null.
    /// </returns>
    public IntVector2? ValidTrapTilePosition(IntVector2 pos)
    {
        bool isPassable(IntVector2 pos) => TRAP_PATH_VALID_TERRAIN.Contains(map[pos]);
        bool isBlocking(IntVector2 pos) => TRAP_PATH_BLOCKING_TERRAIN.Contains(map[pos]);

        if (!WithinMapBounds(pos, 1)) { return null; }
        if (TouchesContinentConnectorReservation(pos)) { return null; }
        if (!isPassable(pos)) { return null; }

        var dx = IntVector2.EAST;
        var dy = IntVector2.SOUTH;
        bool h1 = isPassable(pos + dx);
        bool h2 = isPassable(pos - dx);
        bool v1 = isPassable(pos + dy);
        bool v2 = isPassable(pos - dy);
        bool horizontalPath = h1 && h2 && !v1 && !v2;
        bool verticalPath = v1 && v2 && !h1 && !h2;

        if (!horizontalPath && !verticalPath) { return null; }

        var dir = horizontalPath ? dx : dy;
        var side = horizontalPath ? dy : dx;

        if (!isBlocking(pos + side) || !isBlocking(pos - side)) { return null; }

        // expensive Location check last
        for (int i = -1; i <= 1; i++)
        {
            var p = pos + i * dir;
            var l = GetLocationAt(p);
            if (l != null)
            {
                return null;
            }
        }

        return dir;
    }

    /// <summary>
    /// Check if the position is suitable as a MI drop tile. It should<br/>
    /// - Be on a walkable path.<br/>
    /// - There must be no adjacent special locations.<br/>
    /// - At most 2 adjacent tiles should be passable.
    /// </summary>
    public bool ValidMazeDropPosition(IntVector2 pos)
    {
        bool isPassable(IntVector2 pos) => TRAP_PATH_VALID_TERRAIN.Contains(map[pos.Y, pos.X]);

        if (!WithinMapBounds(pos, 1)) { return false; }
        if (!isPassable(pos)) { return false; }

        var dx = IntVector2.EAST;
        var dy = IntVector2.SOUTH;
        bool h1 = isPassable(pos + dx);
        bool h2 = isPassable(pos - dx);
        bool v1 = isPassable(pos + dy);
        bool v2 = isPassable(pos - dy);
        int passableCount = (h1 ? 1 : 0) + (h2 ? 1 : 0) + (v1 ? 1 : 0) + (v2 ? 1 : 0);

        if (passableCount == 0 || passableCount > 2)
        {
            return false;
        }

        // expensive Location check last
        foreach (var dir in IntVector2.CARDINALS.Append(new(0, 0)))
        {
            var adjacent = pos + dir;
            var l = GetLocationAt(adjacent);
            if (l != null)
            {
                return false;
            }
        }

        return true;
    }

    public string GetGlobDebug(int[,] globs)
    {
        StringBuilder debug = new();
        for (int y = 0; y < MapRows; y++)
        {
            for (int x = 0; x < MapColumns; x++)
            {
                if (globs[y, x] < 10)
                {
                    debug.Append(globs[y, x]);
                }
                else if (globs[y, x] < 36)
                {
                    debug.Append((char)(globs[y, x] + 55));
                }
                else if (globs[y, x] < 62)
                {
                    debug.Append((char)(globs[y, x] + 87));
                }
                else
                {
                    debug.Append('_');
                }
            }
            debug.Append('\n');
        }
        return debug.ToString();
    }

    public string GetMapDebug()
    {
        StringBuilder debug = new();
        for (int y = 0; y < MapRows; y++)
        {
            for (int x = 0; x < MapColumns; x++)
            {
                debug.Append(map[y, x] switch
                {
                    Terrain.TOWN => 'T',
                    Terrain.CAVE => 'C',
                    Terrain.PALACE => 'P',
                    Terrain.BRIDGE => "=",
                    Terrain.DESERT => 'D',
                    Terrain.GRASS => 'G',
                    Terrain.FOREST => 'F',
                    Terrain.SWAMP => 'S',
                    Terrain.GRAVE => 'G',
                    Terrain.ROAD => 'R',
                    Terrain.LAVA => 'L',
                    Terrain.MOUNTAIN => 'M',
                    Terrain.WATER => '-',
                    Terrain.WALKABLEWATER => 'W',
                    Terrain.PREPLACED_WATER_WALKABLE => 'K',
                    Terrain.PREPLACED_WATER => '-',
                    Terrain.ROCK => 'X',
                    Terrain.RIVER_DEVIL => 'P',
                    Terrain.NONE => ' ',
                    _ => throw new Exception("Invalid Terrain")
                });
            }
            debug.Append('\n');
        }
        return debug.ToString();
    }

    public string GetReadableMap()
    {
        StringBuilder debug = new();
        for (int y = 0; y < MapRows; y++)
        {
            for (int x = 0; x < MapColumns; x++)
            {
                debug.Append(map[y, x] switch
                {
                    Terrain.TOWN => 'T',
                    Terrain.CAVE => 'C',
                    Terrain.PALACE => 'P',
                    Terrain.BRIDGE => "=",
                    Terrain.DESERT => '_',
                    Terrain.GRASS => '_',
                    Terrain.FOREST => '_',
                    Terrain.SWAMP => '_',
                    Terrain.GRAVE => '_',
                    Terrain.ROAD => '_',
                    Terrain.LAVA => '_',
                    Terrain.MOUNTAIN => 'X',
                    Terrain.WATER => 'w',
                    Terrain.WALKABLEWATER => 'w',
                    Terrain.ROCK => 'X',
                    Terrain.RIVER_DEVIL => 'X',
                    Terrain.NONE => ' ',
                    _ => throw new Exception("Invalid Terrain")
                });
            }
            debug.Append('\n');
        }
        return debug.ToString();
    }

    /// <summary>Repairs sole water exits and logs the first remaining unsafe cave exit.</summary>
    private protected bool TryRepairAndValidateCaveExits()
    {
        var failure = RepairAndFindCaveExitFailure();
        if (failure is null) return true;

        logger.LogDebug($"Cave exit validation failed: {failure}");
        TraceTerrainRejection("cave-exit", $"failure={failure}");
        return false;
    }

    private CaveExitFailure? RepairAndFindCaveExitFailure()
    {
        CaveExitSafety.RepairSoleWaterExits(map, MapRows, MapColumns);
        return CaveExitSafety.FindFailure(map, MapRows, MapColumns);
    }

    public abstract IEnumerable<Location> RequiredLocations(bool hiddenPalace, bool hiddenKasuto);

    /// <summary>Adds the enabled entrances of local connections to the required locations.</summary>
    private protected void IncludeEnabledLocalConnections(HashSet<Location> requiredLocations)
        => requiredLocations.UnionWith(connections.Keys.Where(location => location.IsEnabled()));

    public abstract string GenerateSpoiler();

    private protected static void AppendOverworldItemSpoilerLine(StringBuilder spoiler, Location location)
    {
        spoiler.AppendLine($"\t{location.ID.GetDescription()}: {location.GetAllCollectables()[0].EnglishText()}");
    }

    private protected static void AppendPalaceSpoilerLine(StringBuilder spoiler, Location location)
    {
        var collectables = location.GetAllCollectables();
        string items = collectables.Count == 0 ? "No Items" : string.Join(", ", collectables.Select(item => item.EnglishText()));
        spoiler.AppendLine($"\t{location.ID.GetDescription()} ({PalaceSpoilerName(location.Palace!.Number)}): {items}");
    }

    private protected static string PalaceSpoilerName(int number) => number == 7 ? "Great Palace" : $"Palace {number}";

    public void SetStart(Location start) => startLocation = start;
}
