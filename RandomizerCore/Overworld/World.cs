using System;
using System.Collections.Frozen;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using NLog;
using Z2Randomizer.RandomizerCore.Enemy;

namespace Z2Randomizer.RandomizerCore.Overworld;

public abstract class World
{
    protected readonly Logger logger = LogManager.GetCurrentClassLogger();
    protected SortedDictionary<string, List<Location>> areasByLocation;

    public Dictionary<Location, Location> connections;
    public int sideviewPtrTable { get; protected set; }
    public int sideviewBank { get; protected set; }
    public int enemyPtr { get; protected set; }
    public GroupedEnemiesBase groupedEnemies { get; protected set; }
    public IReadOnlyList<int> overworldEncounterMaps { get; protected set; }
    public IReadOnlyList<int> overworldEncounterMapDuplicate { get; protected set; } = [];
    public IReadOnlyList<int> nonEncounterMaps { get; protected set; }
    protected SortedDictionary<(int, int), Location> locsByCoords;
    public OverworldMap map;
    public const int MAP_ROWS_FULL = 75;
    public const int MAP_COLS_FULL = 64;
    private const int MAX_LOCATION_PLACEMENT_ATTEMPTS = 5000;
    public int MapRows { get; init; }
    public int MapColumns { get; init; }
    protected List<Terrain> randomTerrainFilter;
    protected List<Terrain> walkableTerrains;
    protected bool[,] visitation;
    protected const int MAP_SIZE_BYTES = 1400;
    protected List<Location> unimportantLocs;
    public Biome biome { get; init; }
    protected bool isHorizontal;
    protected int VANILLA_MAP_ADDR;
    protected SortedDictionary<(int, int), string> section;
    public Location? raft;
    public Location? bridge;
    public Location? cave1;
    public Location? cave2;

    public int baseAddr;
    public Continent continentId;
    protected Climate climate;
    /// south encounter version will be used when Link's Y >= this
    public int northSouthEncounterSeparator { get; init; }

    protected Random RNG;

    private const int MINIMUM_BRIDGE_LENGTH = 2;

    public static readonly Requirements DEFAULT_PALACE_REQUIREMENTS = new Requirements([RequirementType.FAIRY, RequirementType.KEY]);

    private static readonly Dictionary<Biome, int> MAXIMUM_BRIDGE_LENGTH = new()
    {
        { Biome.ISLANDS, 10 },
        { Biome.VOLCANO, 10 },
        { Biome.DRY_CANYON, 10 },
        { Biome.VANILLALIKE, 10 },
        { Biome.CALDERA, 10 },
        { Biome.CANYON, 10 },
        { Biome.MOUNTAINOUS, 15 }
    };

    private static readonly Dictionary<Biome, int> MAXIMUM_BRIDGE_ATTEMPTS = new()
    {
        { Biome.ISLANDS, 4000 },
        { Biome.VOLCANO, 3000 },
        { Biome.DRY_CANYON, 2000 },
        { Biome.VANILLALIKE, 2000 },
        { Biome.CALDERA, 3000 },
        { Biome.CANYON, 2000 },
        { Biome.MOUNTAINOUS, 4000 }
    };


    protected abstract List<Location> GetPathingStarts();
    public abstract bool Terraform(RandomizerProperties props, ROM rom);
    /// <summary>
    /// Some kinds of locations aren't allowed to be passthroughs (even if their base location is a passthrough)
    /// because that kind of location can't pass through. Previously, this was handled in Terraform, but now that we 
    /// attempt to terraform multiple times per instatiation of that world, a late failure could cause passthroughness
    /// to carry from attempt to attempt, so now after terraforming is done (and successful) we can unpassthrough them
    /// </summary>
    public abstract void DisableDisallowedPassthroughs();
    public abstract string GetName();


    /*
     * Maps
     * bridge: 40
     * raft: 41
     * cave1: 42
     * cave2: 43
     * 
     * West:
     * bridge: 0x4657
     * 
     * East:
     * cave1: 0x8659
     * cave2: 0x865a 
     * 
     * Dm:
     * raft: 0x6135
     * 
     * Maze:
     * raft: 0xa135
     */

    //DEBUG / STATS
    public static int failedOnPlaceLocations = 0;
    public static int failedOnBaguPlacement = 0;
    public static int TerrainGrowthAttempts = 0;
    public static int failedOnRaftPlacement = 0;
    public static int failedOnBridgePlacement = 0;
    public static int failedOnIslandConnection = 0;
    public static int failedOnMakeCaldera = 0;
    public static int failedOnConnectIslands = 0;
    public static int PlaceCaveCount = 0;


    public List<Location> AllLocations { get; }
    public List<Location> RemovedLocations { get; }
    public Dictionary<Terrain, List<Location>> Locations { get; set; }

    public bool AllReached { get; set; }

    //The analyzer doesn't understand the design pattern of an inaccessable constructor as a forced inheritance anchor
    //maybe that's just a javaism it doesn't like. In either case, I don't care.
#pragma warning disable CS8618
    protected World(Random r)
#pragma warning restore CS8618 
    {
        RNG = r;

        connections = new Dictionary<Location, Location>();
        Locations = new Dictionary<Terrain, List<Location>>();
        foreach (Terrain Terrain in Enum.GetValues(typeof(Terrain)))
        {
            Locations.Add(Terrain, new List<Location>());
        }
        AllLocations = [];
        RemovedLocations = [];
        locsByCoords = [];
        unimportantLocs = [];
        areasByLocation = [];
        AllReached = false;
    }

    public static void ResetStats()
    {
        failedOnPlaceLocations = 0;
        failedOnBaguPlacement = 0;
        TerrainGrowthAttempts = 0;
        failedOnRaftPlacement = 0;
        failedOnBridgePlacement = 0;
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

        foreach (Location child in l1.Children)
        {
            child.Xpos = l1.Xpos;
            child.Y = l1.Y;
            child.IsPassthrough = l1.IsPassthrough;
        }

        foreach (Location child in l2.Children)
        {
            child.Xpos = l2.Xpos;
            child.Y = l2.Y;
            child.IsPassthrough = l2.IsPassthrough;
        }
    }

    protected void RemoveLocations(ICollection<Location> locationsToRemove)
    {
        foreach (var loc in locationsToRemove)
        {
            loc.Clear();

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

    protected void ChooseConn(String section, Dictionary<Location, Location> co, bool changeType)
    {
        if (co.Count > 0)
        {
            int start = RNG.Next(areasByLocation[section].Count);
            Location s = areasByLocation[section][start];
            int conn = RNG.Next(co.Count);
            Location c = co.Keys.ElementAt(conn);
            int count = 0;
            while ((!c.CanShuffle || !s.CanShuffle || (!changeType && (c.TerrainType != s.TerrainType))) && count < co.Count)
            {
                start = RNG.Next(areasByLocation[section].Count);
                s = areasByLocation[section][start];
                conn = RNG.Next(co.Count);
                c = co.Keys.ElementAt(conn);
                count++;
            }
            Swap(s, c);
            c.CanShuffle = false;
        }
    }

    public void UpdateAllReached()
    {
        if (AllReached)
        {
            return;
        }
        else
        {
            AllReached = true;
            foreach (Location location in AllLocations)
            {
                //TODO: Doing this off terrain type is a very bad idea for stuff like vanilla shuffle no actual terrain.
                if (location.TerrainType == Terrain.PALACE 
                    || location.TerrainType == Terrain.TOWN 
                    || location.Collectables.Any(i => !i.IsInternalUse()))
                {
                    if (!location.Reachable)
                    {
                        AllReached = false;
                    }
                }
            }
        }
    }

    protected Location GetLocation(LocationID lid)
    {
        return AllLocations.FirstOrDefault(i => i.ID == lid)
            ?? throw new Exception($"Failed to find Location with ID {lid}");
    }

    protected Location GetLocation(int locIdx)
        => GetLocation(LocationIDUtils.FromIndex(continentId, locIdx));

    protected Location GetLocation(Continent cont, int locIdx)
        => GetLocation(LocationIDUtils.FromIndex(cont, locIdx));

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

    protected Location? GetLocationByCoordsNoOffset((int, int) coords)
    {
        IntVector2 pos = new(coords.Item2, coords.Item1); // y,x -> x,y
        return AllLocations.FirstOrDefault(i => i.Pos == pos);
    }

    protected Location? GetLocationByCoordsY30Offset((int, int)coords)
    {
        return AllLocations.FirstOrDefault(i => i.CoordsY30Offset.Equals(coords));
    }

    protected Location GetLocationByMem(int mem)
    {
        return AllLocations.FirstOrDefault(i => i.MemAddress.Equals(mem))
            ?? throw new Exception("Failed to find Location at address: " + mem);
    }

    protected bool PlaceLocations(Terrain riverTerrain, bool saneCaves)
    {
        return PlaceLocations(riverTerrain, saneCaves, null, -1);
    }
    protected bool PlaceLocations(Terrain crossingTerrain, bool saneCaves, Location? hiddenKasutoLocation, int hiddenPalaceX)
    {
        int placementAttempt = 0;
        foreach (Location location in AllLocations.Where(loc => loc.AppearsOnMap))
        {
            if ((location.TerrainType != Terrain.BRIDGE
                    && location.CanShuffle
                    && !unimportantLocs.Contains(location)
                    && !location.IsPassthrough)
                || location.AccessRequirements.HasHardRequirement(RequirementType.HAMMER))
            {
                int x, y;
                //Place the location in a spot that is not adjacent to any other location
                do
                {
                    //It's possible there just literlly aren't enough open places to place locations (chaos does this a lot)
                    //So if we fail enough, give up and try again.
                    if (++placementAttempt > MAX_LOCATION_PLACEMENT_ATTEMPTS)
                    {
                        return false;
                    }
                    x = RNG.Next(MapColumns - 2) + 1;
                    y = RNG.Next(MapRows - 2) + 1;
                } while (
                    map[y, x] != Terrain.NONE
                    || map[y - 1, x] != Terrain.NONE
                    || map[y + 1, x] != Terrain.NONE
                    || map[y + 1, x + 1] != Terrain.NONE
                    || map[y, x + 1] != Terrain.NONE
                    || map[y - 1, x + 1] != Terrain.NONE
                    || map[y + 1, x - 1] != Terrain.NONE
                    || map[y, x - 1] != Terrain.NONE
                    || map[y - 1, x - 1] != Terrain.NONE
                    //#124
                    || (x == hiddenPalaceX && location == hiddenKasutoLocation)
                );

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
                else if (location.TerrainType == Terrain.PALACE)
                {
                    Terrain s = climate.GetRandomTerrain(RNG, walkableTerrains);
                    map[y + 1, x] = s;
                    map[y + 1, x + 1] = s;
                    map[y + 1, x - 1] = s;
                    map[y, x - 1] = s;
                    map[y, x + 1] = s;
                    map[y - 1, x - 1] = s;
                    map[y - 1, x] = s;
                    map[y - 1, x + 1] = s;
                    location.Xpos = x;
                    location.Y = y;
                    location.CanShuffle = false;
                }
                else if (location.TerrainType != Terrain.TOWN || 
                    (location.ActualTown > 0 && (location?.ActualTown?.AppearsOnMap() ?? false)))
                {
                    Terrain t;
                    do
                    {
                        t = climate.GetRandomTerrain(RNG, walkableTerrains);
                    } while (t == location.TerrainType);
                    map[y + 1, x] = t;
                    map[y + 1, x + 1] = t;
                    map[y + 1, x - 1] = t;
                    map[y, x - 1] = t;
                    map[y, x + 1] = t;
                    map[y - 1, x - 1] = t;
                    map[y - 1, x] = t;
                    map[y - 1, x + 1] = t;
                    location.Xpos = x;
                    location.Y = y;
                    location.CanShuffle = false;
                }
            }

            if(location!.TerrainType == Terrain.TOWN)
            {
                foreach (Location linkedLocation in AllLocations.Where(
                    loc => !loc.AppearsOnMap && loc.ActualTown?.GetMasterTown() == location.ActualTown))
                {
                    linkedLocation.Pos = location.Pos;
                }
            }
        }
        return true;
    }

    /// <summary>
    /// Returns true iff there is any crossingTerrain in the rectangle with corners (x1,y1) and (x2,y2)
    /// </summary>
    protected bool CrossingTerrainInArea(IntVector2 p1, IntVector2 p2, Terrain crossingTerrain)
    {
        int minX = Math.Min(p1.X, p2.X);
        int maxX = Math.Max(p1.X, p2.X);
        int minY = Math.Min(p1.Y, p2.Y);
        int maxY = Math.Max(p1.Y, p2.Y);
        for (int y = minY; y <= maxY; y++)
        {
            for (int x = minX; x <= maxX; x++)
            {
                var pos = new IntVector2(x, y);
                if (WithinMapBounds(pos, 0) && map[pos] == crossingTerrain)
                {
                    return true;
                }
            }
        }
        return false;
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

    public (IntVector2 pos, IntVector2 directionIn)? PickCavePositionAndDirection()
    {
        IntVector2 pos = IntVector2.ZERO;
        for (int tries = 0; ; tries++)
        {
            pos = IntVector2.Random(RNG, 5, MapColumns - 5, 5, MapRows - 5);
            if (AllTerrainIn3x3Equals(pos, Terrain.NONE))
            {
                break;
            }
            if (tries == 1000) { return null; }
        }

        IntVector2 minDistToEdge = new(Math.Min(MapColumns / 2 - 1, 15),
                                       Math.Min(MapRows / 2 - 1, 15));
        IntVector2 directionIn = IntVector2.ZERO;
        for (int tries = 0; ; tries++)
        {
            directionIn = IntVector2.CARDINALS.Sample(RNG);
            // check if the cave is too close to the edge in the direction it's going
            if (WithinMapBounds(pos + directionIn.ComponentMultiply(minDistToEdge)))
            {
                break;
            }
            if (tries == 1000) { return null; }
        }

        return (pos, directionIn);
    }

    protected IntVector2? PickMatchingSaneCavePosition(IntVector2 cave1pos, IntVector2 direction, Func<int> rollSpacing, Func<IntVector2, bool> additionalCheck)
    {
        IntVector2 pos;
        IntVector2 perp = direction.Perpendicular();
        for (int tries = 0; tries < 100; tries++)
        {
            var forwardSteps = rollSpacing();
            int lateralSteps = RNG.Next(-3, 4);
            pos = cave1pos + forwardSteps * direction + lateralSteps * perp;
            if (!WithinMapBounds(pos, 1) || !AllTerrainIn3x3Equals(pos, Terrain.NONE))
            {
                continue;
            }
            if (!additionalCheck(pos))
            {
                continue;
            }

            return pos;
        }

        return null;
    }

    protected bool PlaceSaneCave(Location location, Terrain crossingTerrain)
    {
        if (!(PickCavePositionAndDirection() is var (cave1pos, cave1dir)))
        {
            return false;
        }

        Func<int> rollSpacing = biome switch
        {
            Biome.ISLANDS or Biome.MOUNTAINOUS => () => RNG.Next(10, 20),
            Biome.VOLCANO or Biome.CALDERA => () => RNG.Next(5, 20),
            _ => () => RNG.Next(6, 18),
        };
        Func<IntVector2, bool> crossingCheck = biome switch
        {
            Biome.VOLCANO => (_) => true,
            _ => (IntVector2 pos) => CrossingTerrainInArea(cave1pos, pos, crossingTerrain)
        };
        if (!(PickMatchingSaneCavePosition(cave1pos, cave1dir, rollSpacing, crossingCheck) is IntVector2 cave2pos))
        {
            return false;
        }

        Location location2 = connections[location];
        location.CanShuffle = false;
        location.Pos = cave1pos;
        location2.CanShuffle = false;
        location2.Pos = cave2pos;
        PlaceCave(cave1pos, cave1dir, climate.GetRandomTerrain(RNG, walkableTerrains));
        PlaceCave(cave2pos, -cave1dir, climate.GetRandomTerrain(RNG, walkableTerrains));
        AlignCavePositionsLeftToRight(cave1dir, location, location2);
        return true;
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
            case LessImportantLocationsOption.REMOVE:
                foreach (Location location in unimportantLocs)
                {
                    if (location.CanShuffle)
                    {
                        location.Clear();
                    }
                }
                break;
            case LessImportantLocationsOption.HIDE:
                foreach (Location location in unimportantLocs)
                {
                    if (location.CanShuffle)
                    {
                        int tries = 0;
                        int x, y;
                        do
                        {
                            x = RNG.Next(MapColumns);
                            y = RNG.Next(MapRows);
                            tries++;
                        } while ((map[y, x] != location.TerrainType || GetLocationByCoordsNoOffset((y, x)) != null) && tries < 2000);

                        if (tries < 2000)
                        {
                            location.Xpos = x;
                            location.Y = y;
                        }
                        else
                        {
                            location.Xpos = 0;
                            location.YRaw = 0;
                        }
                        location.CanShuffle = false;
                    }
                }
                break;
        }
    }

    /// <summary>
    /// Creates "bridges" connecting contiguous landmasses with terrain from either connected landmass, or various kinds of special connections.
    /// </summary>
    /// <param name="maxBridges">Maximum number of bridges to create</param>
    /// <param name="placeSaria">If true, one of the bridges is the saria river crossing</param>
    /// <param name="riverTerrain">Type of terrain the rivers are crossing over. Usually this is water, but on Mountanous, the "bridges" are over mountains</param>
    /// <param name="riverDevil">If true, one of the bridges will be a road blocked by the river devil</param>
    /// <param name="placeLongBridge">If true, one of the bridges is the bridge from vanilla west connecting DM to the SE desert with encounters at both ends</param>
    /// <param name="placeDaruniaDesert">If true, one of the bridges is a desert road with the two encounters that lead to darunia in vanilla</param>
    /// <returns>False if greater than 2000 total attempts were made in placement of all of the bridges. Else true.</returns>
    protected bool ConnectIslands(int maxBridges, bool placeSaria, Terrain riverTerrain, bool riverDevil, 
        bool rockBlock, bool placeLongBridge, bool placeDaruniaDesert,
        bool canWalkOnWater, Biome biome, int deadZoneMinX = 999, int deadZoneMaxX = -1, int deadZoneMinY = 999, int deadZoneMaxY = -1)
    {
        if (!((deadZoneMinX == 999 && deadZoneMaxX == -1 && deadZoneMinY == 999 && deadZoneMaxY == -1)
            || (deadZoneMinX != 999 && deadZoneMaxX != -1 && deadZoneMinY != 999 && deadZoneMaxY != -1)))
        {
            throw new ArgumentException("ConnectIslands dead zone is improperly defined. 0 or 4 values must be specified.");
        }
        bool IsDeadZoneSafe(int deadZoneMinX, int deadZoneMaxX, int deadZoneMinY, int deadZoneMaxY, int y, int x)
            => (x < deadZoneMinX || x > deadZoneMaxX) && (y < deadZoneMinY || y > deadZoneMaxY);

        //Any of the bridge locations that are being placed need to be reset so their vanilla locations aren't avoided.
        if(placeSaria)
        {
            foreach (Location location in AllLocations.Where(i => i.ActualTown == Town.SARIA_NORTH 
                || i.ActualTown == Town.SARIA_SOUTH 
                || i.ActualTown == Town.SARIA_TABLE))
            {
                location.Xpos = 0;
                location.Y = 0;
            }
        }
        if(placeDaruniaDesert)
        {
            int addr1 = LocationID.EAST_TRAP_DESERT1.GetRomOffset(),
                addr2 = LocationID.EAST_TRAP_DESERT2.GetRomOffset();
            foreach (Location location in AllLocations.Where(i => i.MemAddress == addr1
                || i.MemAddress == addr2))
            {
                location.Xpos = 0;
                location.Y = 0;
            }
        }
        if (placeLongBridge)
        {
            int addr1 = LocationID.WEST_BRIDGE_AFTER_DM_WEST.GetRomOffset(),
                addr2 = LocationID.WEST_BRIDGE_AFTER_DM_EAST.GetRomOffset();
            foreach (Location location in AllLocations.Where(i => i.MemAddress == addr1
                || i.MemAddress == addr2))
            {
                location.Xpos = 0;
                location.Y = 0;
            }
        }
        //Debug.WriteLine(GetMapDebug());
        int maxBridgeLength = MAXIMUM_BRIDGE_LENGTH[biome];
        //Great lakes and bad boots are more likely to be incompleteable, so extend max bridge length to give them a fighting chance
        if (!canWalkOnWater || climate.Name == Climates.Create(continentId, ClimateEnum.GREAT_LAKES).Name)
        {
            maxBridgeLength = (int)(maxBridgeLength * 1.5);
        }
        int maxBridgeAttempts = MAXIMUM_BRIDGE_ATTEMPTS[biome];
        maxBridgeAttempts *= canWalkOnWater ? 1 : 2;
        int[,] globs = GetTerrainGlobs();
        List<(int, int)> globConnections = [];
        int remainingBridges = maxBridges;
        int TerrainCycle = 0;
        int tries = 0;

        Terrain[] crossingTerrains = riverTerrain.IsWater()
            ? [Terrain.WATER, Terrain.PREPLACED_WATER, Terrain.WALKABLEWATER, Terrain.PREPLACED_WATER_WALKABLE]
            : [riverTerrain];
        Terrain[] edgeCrossingTerrains = [.. crossingTerrains, .. walkableTerrains];

        //precompute next to water tiles since there are likely more fails than successes and randomly iterating the same
        //bad tiles over and over is wasteful
        List<(IntVector2 Pos, Direction)> nextToWaterTiles = [];
        for (int y = 1; y < MapRows - 1; y++) 
        {
            for (int x = 1; x < MapColumns - 1; x++)
            {
                List<Direction> waterDirections = NextToWaterDirections(x, y, crossingTerrains);
                if (IsDeadZoneSafe(deadZoneMinX, deadZoneMaxX, deadZoneMinY, deadZoneMaxY, y, x))
                {
                    foreach(Direction direction in waterDirections)
                    {
                        nextToWaterTiles.Add((new IntVector2(x, y), direction));
                    }
                }
            }
        }

        while (remainingBridges > 0 && tries < maxBridgeAttempts)
        {
            tries++;

            (IntVector2 Pos, Direction Dir)? nextToWaterTile = nextToWaterTiles.Sample(RNG);
            //All possible bridges have been evaluated.
            if (nextToWaterTile == null)
            {
                return true;
            }

            var pos = nextToWaterTile.Value.Pos;
            IntVector2 start = pos;
            IntVector2 forward = nextToWaterTile.Value.Dir.ToIntVector2();
            IntVector2 side = forward.Perpendicular();
            Direction waterDirection = nextToWaterTile.Value.Dir;

            int length = 0;
            if (IsBlockedSingleTile(pos))
            {
                length = 100;
            }
            if (LocationsAtOrOrthogonalTo(pos).Any())
            {
                length = 100;
            }
            int startMass = globs[pos.Y, pos.X];
            pos += forward;

            //iterate expanding the bridge
            while (WithinMapBounds(pos, 1) && crossingTerrains.Contains(map[pos]))
            {
                //if we are too close to a location, give up
                if (LocationsAtOrOrthogonalTo(pos).Any())
                {
                    length = 100;
                    break;
                }
                //if advancing goes off the edge of the map, give up
                if(!WithinMapBounds(pos + forward))
                {
                    length = 100;
                    break;
                }

                //count how many tiles adjacent to the bridge are the passing terrain
                int adjacentRiverTerrainDirectionCount = 0;
                //If the tile just before or just after is walkable, this is the first or last tile of the bridge
                //When that happens, the adjacent (not forward/back) directions are allowed to be any walkable terrain
                bool isFirstStep = !crossingTerrains.Contains(map[pos - forward]);
                Terrain forwardTerrain = map[pos + forward];
                bool isLastStep = !crossingTerrains.Contains(forwardTerrain);
                Terrain[] effectiveCrossingTerrain = isFirstStep || isLastStep ? edgeCrossingTerrains : crossingTerrains;

                //Check forward
                if (effectiveCrossingTerrain.Contains(forwardTerrain) || forwardTerrain == Terrain.MOUNTAIN)
                {
                    adjacentRiverTerrainDirectionCount++;
                }
                //Check perpendicular tiles. This is allowed to be any walkable terrain on the first/last tiles of the bridge
                if (WithinMapBounds(pos + side))
                {
                    var rightTerrain = map[pos + side];
                    if (effectiveCrossingTerrain.Contains(rightTerrain) || rightTerrain == Terrain.MOUNTAIN)
                    {
                        adjacentRiverTerrainDirectionCount++;
                    }
                }
                if (WithinMapBounds(pos - side))
                {
                    var leftTerrain = map[pos - side];
                    if (effectiveCrossingTerrain.Contains(leftTerrain) || leftTerrain == Terrain.MOUNTAIN)
                    {
                        adjacentRiverTerrainDirectionCount++;
                    }
                }

                //if all 3 tiles adjacent to the new tile aren't the passing terrain, give up
                if (adjacentRiverTerrainDirectionCount < 3)
                {
                    length = 100;
                }
                pos += forward;
                length++;
            }

            //no extending from single tile
            if (IsBlockedSingleTile(pos))
            {
                length = 100;
            }

            int endMass = 0;
            if (WithinMapBounds(pos, 1))
            {
                if (LocationsAtOrOrthogonalTo(pos).Any())
                {
                    length = 100;
                }
                endMass = globs[pos.Y, pos.X];
            }

            //if we're ending, it has to be on a different chunk of terrain so the bridge doesn't just cross a bay
            if (riverTerrain != Terrain.DESERT && biome != Biome.CALDERA && biome != Biome.VOLCANO
                && (startMass == 0
                    || endMass == 0
                    || endMass == startMass
                    || globConnections.Contains((startMass, endMass))
                )
            )
            {
                length = 100;
            }
            if (
                (placeSaria && length < maxBridgeLength || (length < maxBridgeLength && length > MINIMUM_BRIDGE_LENGTH))
                && WithinMapBounds(pos, 1)
                && walkableTerrains.Contains(map[pos])
                && map[pos] != riverTerrain)
            {
                Terrain terrain = map[pos];
                globConnections.Add((startMass, endMass));
                globConnections.Add((endMass, startMass));

                if (placeSaria)
                {
                    //Saria doesn't need to worry about sideways entrance since it's not a passthrough
                    map[pos] = Terrain.TOWN;
                    Location location = GetLocation(LocationID.WEST_TOWN_SARIA_SOUTH);
                    location.Pos = pos;
                    pos -= forward;
                    while (crossingTerrains.Contains(map[pos]))
                    {
                        pos -= forward;
                    }
                    map[pos] = Terrain.TOWN;
                    location = GetLocation(LocationID.WEST_TOWN_SARIA_NORTH);
                    location.Pos = pos;
                    placeSaria = false;
                }
                else if (placeLongBridge)
                {
                    Location bridge1 = GetLocation(LocationID.WEST_BRIDGE_AFTER_DM_WEST);
                    Location bridge2 = GetLocation(LocationID.WEST_BRIDGE_AFTER_DM_EAST);

                    pos -= forward;
                    if (!walkableTerrains.Contains(map[pos]))
                    {
                        map[pos] = Terrain.BRIDGE;
                    }

                    while (!crossingTerrains.Contains(map[pos + side]) &&
                           !crossingTerrains.Contains(map[pos - side]))
                    {
                        if (start.IsBehind(pos, forward))
                        {
                            logger.Warn("Unable to roll back bridge location with jagged entrance");
                            return false;
                        }
                        pos -= forward;
                    }

                    if (crossingTerrains.Contains(map[pos + forward]))
                    {
                        pos += forward;
                    }

                    if (forward.X > 0 || forward.Y > 0)
                    {
                        bridge2.Pos = pos;
                    }
                    else
                    {
                        bridge1.Pos = pos;
                    }

                    NormalizeBridgeSideTerrain(map, pos, side);

                    while (pos != start)
                    {
                        map[pos] = Terrain.BRIDGE;
                        pos -= forward;
                    }

                    if (crossingTerrains.Contains(map[pos]))
                    {
                        map[pos] = map[pos - forward];
                    }

                    pos += forward;

                    NormalizeBridgeSideTerrain(map, pos, side);
                    map[pos] = Terrain.BRIDGE;

                    if (forward.X > 0 || forward.Y > 0)
                    {
                        bridge1.Pos = pos;
                    }
                    else
                    {
                        bridge2.Pos = pos;
                    }

                    placeLongBridge = false;
                    bridge1.CanShuffle = false;
                    bridge2.CanShuffle = false;
                }
                else if (placeDaruniaDesert)
                {
                    Location bridge1 = GetLocation(LocationID.EAST_TRAP_DESERT2);
                    Location bridge2 = GetLocation(LocationID.EAST_TRAP_DESERT1);
                    if (bridge1.CanShuffle && bridge2.CanShuffle)
                    {
                        pos -= forward;
                        if (!walkableTerrains.Contains(map[pos]))
                        {
                            map[pos] = Terrain.DESERT;
                        }

                        while (!crossingTerrains.Contains(map[pos + side]) &&
                               !crossingTerrains.Contains(map[pos - side]))
                        {
                            if (start.IsBehind(pos, forward))
                            {
                                logger.Warn("Unable to roll back bridge location with jagged entrance");
                                return false;
                            }
                            pos -= forward;
                        }

                        // first bridge encounter
                        if (forward.X > 0 || forward.Y > 0)
                        {
                            bridge2.Pos = pos;
                        }
                        else
                        {
                            bridge1.Pos = pos;
                        }

                        NormalizeBridgeSideTerrain(map, pos, side);

                        while (pos != start)
                        {
                            map[pos] = Terrain.DESERT;
                            pos -= forward;
                        }

                        if (map[pos].IsWater())
                        {
                            map[pos] = map[pos - forward];
                        }

                        pos += forward;

                        NormalizeBridgeSideTerrain(map, pos, side);

                        map[pos] = Terrain.DESERT;

                        if (forward.X > 0 || forward.Y > 0)
                        {
                            bridge1.Pos = pos;
                        }
                        else
                        {
                            bridge2.Pos = pos;
                        }

                        bridge1.CanShuffle = false;
                        bridge2.CanShuffle = false;
                    }

                    placeDaruniaDesert = false;
                }
                else
                {
                    pos -= forward;
                    int curr = 0;
                    if (TerrainCycle == 2)
                    {
                        map[pos + forward] = Terrain.ROAD;
                        while (crossingTerrains.Contains(map[pos]))
                        {
                            map[pos] = Terrain.WALKABLEWATER;
                            pos -= forward;
                        }
                        map[pos] = Terrain.ROAD;
                    }
                    else
                    {
                        while (crossingTerrains.Contains(map[pos]))
                        {

                            if (biome == Biome.MOUNTAINOUS || biome == Biome.VANILLALIKE)
                            {
                                if (biome == Biome.VANILLALIKE)
                                {
                                    if (riverTerrain == Terrain.WALKABLEWATER || riverTerrain == Terrain.WATER)
                                    {
                                        terrain = Terrain.BRIDGE;
                                    }
                                    else
                                    {
                                        terrain = Terrain.ROAD;
                                    }
                                }

                                map[pos] = terrain;
                                bool placed = false;
                                if (curr == length / 2)
                                {
                                    List<Location> locs = Locations[terrain];
                                    if (riverDevil)
                                    {
                                        map[pos] = Terrain.RIVER_DEVIL;
                                        riverDevil = false;
                                        placed = true;
                                    }
                                    else if (rockBlock)
                                    {
                                        map[pos] = Terrain.ROCK;
                                        rockBlock = false;
                                        placed = true;
                                    }
                                    foreach (Location location in locs)
                                    {
                                        if (location.CanShuffle && !placed)
                                        {
                                            location.Pos = pos;
                                            location.CanShuffle = false;
                                            break;
                                        }
                                    }

                                }
                            }
                            else
                            {
                                if (TerrainCycle == 0)
                                {
                                    map[pos] = Terrain.ROAD;

                                    if (curr == length / 2)
                                    {
                                        bool placed = false;
                                        if (riverDevil)
                                        {
                                            map[pos] = Terrain.RIVER_DEVIL;
                                            riverDevil = false;
                                            placed = true;
                                        }
                                        else if (rockBlock)
                                        {
                                            map[pos] = Terrain.ROCK;
                                            rockBlock = false;
                                            placed = true;
                                        }
                                        foreach (Location location in Locations[Terrain.ROAD])
                                        {
                                            if (location.CanShuffle && !placed)
                                            {
                                                location.Pos = pos;
                                                location.CanShuffle = false;
                                                break;
                                            }
                                        }

                                    }
                                }
                                else if (TerrainCycle == 1)
                                {
                                    if (
                                        riverTerrain.IsWater()
                                        || riverTerrain == Terrain.MOUNTAIN 
                                        || (riverTerrain != Terrain.WALKABLEWATER && curr != length / 2 + 1))
                                    {
                                        map[pos] = Terrain.BRIDGE;
                                    }
                                    bool placed = false;
                                    if (curr == length / 2)
                                    {
                                        if (riverDevil)
                                        {
                                            map[pos] = Terrain.RIVER_DEVIL;
                                            riverDevil = false;
                                            placed = true;
                                        }
                                        else if (rockBlock)
                                        {
                                            map[pos] = Terrain.ROCK;
                                            rockBlock = false;
                                            placed = true;
                                        }
                                        foreach (Location location in Locations[Terrain.BRIDGE])
                                        {
                                            if (location.CanShuffle && !placed)
                                            {
                                                location.Pos = pos;
                                                location.CanShuffle = false;
                                                break;
                                            }
                                        }
                                    }
                                }
                            }
                            curr++;
                            pos -= forward;
                        }
                    }
                    TerrainCycle++;
                    if (riverTerrain != Terrain.MOUNTAIN && riverTerrain != Terrain.DESERT && !canWalkOnWater)
                    {
                        TerrainCycle %= 3;
                    }
                    else
                    {
                        TerrainCycle %= 2;
                    }

                }

                remainingBridges--;
            }

            nextToWaterTiles.Remove((start, waterDirection));

        }
        return !placeSaria;
    }

    /// <summary>
    /// Groups the map into connected sections of walkable terrain.
    /// </summary>
    /// <returns>An int[][] mapping each x-y on the map to an int corresponding to the group that tile is a member of</returns>
    private int[,] GetTerrainGlobs()
    {
        int[,] mass = new int[MapRows, MapColumns];
        for (int i = 0; i < MapRows; i++)
        {
            for (int j = 0; j < MapColumns; j++)
            {
                mass[i, j] = 0;
            }
        }
        int massNo = 1;
        for (int i = 0; i < MapRows; i++)
        {
            for (int j = 0; j < MapColumns; j++)
            {
                if (mass[i, j] == 0 && walkableTerrains.Contains(map[i, j]))
                {
                    CategorizeTerrainGlob(i, j, massNo, mass);
                    massNo++;
                }
            }
        }
        return mass;
    }

    /// <summary>
    /// Recursive step for growing the glob map
    /// </summary>
    /// <param name="y"></param>
    /// <param name="x"></param>
    /// <param name="massNo"></param>
    /// <param name="mass"></param>
    private void CategorizeTerrainGlob(int y, int x, int massNo, int[,] mass)
    {
        mass[y, x] = massNo;
        if (y - 1 > 0 && mass[y - 1, x] == 0 && walkableTerrains.Contains(map[y - 1, x]))
        {
            CategorizeTerrainGlob(y - 1, x, massNo, mass);
        }
        if (y + 1 < MapRows && mass[y + 1, x] == 0 && walkableTerrains.Contains(map[y + 1, x]))
        {
            CategorizeTerrainGlob(y + 1, x, massNo, mass);
        }
        if (x - 1 > 0 && mass[y, x - 1] == 0 && walkableTerrains.Contains(map[y, x - 1]))
        {
            CategorizeTerrainGlob(y, x - 1, massNo, mass);
        }
        if (x + 1 < MapColumns && mass[y, x + 1] == 0 && walkableTerrains.Contains(map[y, x + 1]))
        {
            CategorizeTerrainGlob(y, x + 1, massNo, mass);
        }
    }

    [System.Diagnostics.CodeAnalysis.SuppressMessage("CodeQuality", "IDE0051:Remove unused private members", Justification = "Debug Method")]
    private string PrintTerrainGlobMap(int[,] mass)
    {
        char[] encoding = { '0', '1', '2', '3', '4', '5', '6', '7', '8', '9', 'A', 'B', 'C', 'D', 'E', 'F', 'G', 'H', 'I', 'J', 'K', 'L', 'M', 'N', 'O', 'P', 'Q' };
        StringBuilder sb = new StringBuilder();
        for (int i = 0; i < MapRows; i++)
        {
            for (int j = 0; j < MapColumns; j++)
            {
                sb.Append(encoding[mass[i, j]]);
            }
            sb.Append('\n');
        }
        return sb.ToString();
    }

    private static void NormalizeBridgeSideTerrain(OverworldMap map, IntVector2 pos, IntVector2 side)
    {
        IntVector2 left = pos - side;
        IntVector2 right = pos + side;

        bool leftBlocked = map[left] == Terrain.MOUNTAIN || map[left].IsWater();
        bool rightBlocked = map[right] == Terrain.MOUNTAIN || map[right].IsWater();

        if (rightBlocked && !leftBlocked)
        {
            map[left] = map[right];
        }

        if (leftBlocked && !rightBlocked)
        {
            map[right] = map[left];
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

    /// <summary>
    /// Returns true IFF this is an isolated single tile. i.e. the
    /// terrain type one space in each of the 4 orthogonal directions
    /// is not walkable terrain.
    /// </summary>
    private bool IsBlockedSingleTile(IntVector2 pos)
    {
        return IntVector2.CARDINALS.All(d =>
        {
            var checkPos = pos + d;

            // out of bounds is effectively not walkable (blocked).
            if (!WithinMapBounds(checkPos)) { return true; }

            var checkTerrain = map[checkPos];
            return !walkableTerrains.Contains(checkTerrain);
        });
    }

    /// used for GrowTerrain optimization
    private readonly struct PlacedTerrainPreCalc
    {
        public readonly double X;
        public readonly double Y;
        public readonly Terrain Terrain;
        public readonly double CoefSquared;
        public PlacedTerrainPreCalc(int x, int y, Terrain terrain, double coefSquared)
        {
            X = x;
            Y = y;
            Terrain = terrain;
            CoefSquared = coefSquared;
        }
    }

    /// Pre-build an array to loop over, since this used MAP_ROWS * MAP_COLS times
    private PlacedTerrainPreCalc[] GrowTerrainGetPlacedTerrains(Climate climate)
    {
        List<Terrain> consideredTerrains = Enum.GetValues<Terrain>().ToList();
        List<(int, int)> placedCoords = new();
        for (int y = 0; y < MapRows; y++)
        {
            for (int x = 0; x < MapColumns; x++)
            {
                Terrain t = map[y, x];
                if (t != Terrain.NONE && consideredTerrains.Contains(t))
                {
                    placedCoords.Add((y, x));
                }
            }
        }
        Debug.Assert(placedCoords.Count > 0);
        PlacedTerrainPreCalc[] placedTerrains = new PlacedTerrainPreCalc[placedCoords.Count];
        for (int i = 0; i < placedCoords.Count; i++)
        {
            var (py, px) = placedCoords[i];
            Terrain t = map[py, px];
            double c = climate.DistanceCoefficients[(int)t];
            placedTerrains[i] = new PlacedTerrainPreCalc(px, py, t, c * c);
        }
        return placedTerrains;
    }

    protected bool GrowTerrain(Climate climate)
    {
        TerrainGrowthAttempts++;
        var mapCopy = new OverworldMap(MapRows, MapColumns);
        PlacedTerrainPreCalc[] placedTerrains = GrowTerrainGetPlacedTerrains(climate);
        const double EPSILON = 1e-9;
        double distance, minDistance;
        List<Terrain> choices = new();
        for (int y = 0; y < MapRows; y++)
        {
            for (int x = 0; x < MapColumns; x++)
            {
                var existingT = map[y, x];
                if (existingT != Terrain.NONE)
                {
                    mapCopy[y, x] = existingT;
                }
                else
                {
                    choices.Clear();
                    minDistance = double.MaxValue;

                    for (int i = 0; i < placedTerrains.Length; i++)
                    {
                        ref readonly var placedTerrain = ref placedTerrains[i];
                        double dx = placedTerrain.X - x;
                        double dy = placedTerrain.Y - y;
                        Terrain t = placedTerrain.Terrain;

                        // optimize further by skipping square root, because the
                        // minimum distance will also be the minimum distance squared
                        distance = placedTerrain.CoefSquared * (dx * dx + dy * dy);
                        if (distance > minDistance) // most likely case first
                        {
                            continue;
                        }
                        else if (distance + EPSILON < minDistance)
                        {
                            choices.Clear();
                            choices.Add(t);
                            minDistance = distance;
                        }
                        else
                        {
                            choices.Add(t);
                        }
                    }
                    Debug.Assert(choices.Count > 0);
                    mapCopy[y, x] = choices[RNG.Next(choices.Count)];
                }
            }
        }
        map = mapCopy; // no need to clone as we created this array at the start of the method
        return true;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int DistanceSquared(Location l, int x2, int y2)
    {
        int dx = l.Xpos - x2;
        int dy = l.Y - y2;
        return dx * dx + dy * dy;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int DistanceSquared(int x1, int y1, int x2, int y2)
    {
        int dx = x1 - x2;
        int dy = y1 - y2;
        return dx * dx + dy * dy;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    protected bool AllTerrainIn3x3Equals(int x, int y, Terrain t)
    {
        return AllTerrainIn3x3Equals(new IntVector2(x, y), t);
    }

    protected bool AllTerrainIn3x3Equals(IntVector2 pos, Terrain t)
    {
        return IntVector2.DIRECTIONS.All(d => map[pos + d] == t) && map[pos] == t;
    }

    protected void PlaceRandomTerrain(Climate climate, int seedCountMaximum = 500)
    {
        //If we fail to place terrain more than a certain number of times in a row, we're probably at/near the limit of the number of 
        //places terrain could be placed given the initial layout
        const int EXHAUSTION_LIMIT = 300;
        int consecutivelyPlaced = 0;
        int placed = 0;
        int x, y;
        while (placed < int.Min(climate.SeedTerrainCount, seedCountMaximum) && consecutivelyPlaced < EXHAUSTION_LIMIT)
        {
            consecutivelyPlaced = 0;
            Terrain t = climate.GetRandomTerrain(RNG, randomTerrainFilter);
            do
            {
                x = RNG.Next(MapColumns);
                y = RNG.Next(MapRows);
                consecutivelyPlaced++;
            } while (map[y, x] != Terrain.NONE && consecutivelyPlaced < EXHAUSTION_LIMIT);
            if (consecutivelyPlaced < EXHAUSTION_LIMIT)
            {
                map[y, x] = t;
                placed++;
            }
            else
            {
                logger.Trace("Exhaustion limit reached");
            }
        }
    }

    /// <summary>
    /// Writes the map data to the rom
    /// </summary>
    /// <param name="doWrite">If true, actually write the map data, otherwise just bulk fill the whole map</param>
    /// <param name="loc">Start address of the map data in the rom (usually 0xB480)</param>
    /// <param name="total">Total number of bytes to write</param>
    /// <param name="h1">For east: Hidden Palace Y - 30</param>
    /// <param name="h2">For east: Hidden palace X</param>
    protected int WriteMapToRom(ROM romData, bool doWrite, int loc, int total, int h1, int h2, bool hiddenPalace, bool hiddenKasuto)
    {
        int bytesWritten = 0; //Number of bytes written so far
        Terrain currentTerrain = map[0, 0];
        int currentTerrainCount = 0;
        Terrain edgeOfContinentTerrain = map[0, MapColumns - 1] == Terrain.MOUNTAIN ? Terrain.MOUNTAIN : Terrain.WATER;
        for (int y = 0; y < MapRows; y++)
        {
            for (int x = 0; x < MAP_COLS_FULL; x++)
            {
                //These two conditionals ABSOLUTELY should not be processed here.
                //Refactor them and remove the excess boolean parameters.
                if (hiddenPalace && y == h1 && x == h2 && y != 0 && x != 0)
                {
                    currentTerrainCount--;
                    int b = currentTerrainCount * 16 + currentTerrain.RomValue();
                    //logger.WriteLine("Hex: {0:X}", b);
                    if (doWrite)
                    {
                        romData.Put(loc, (byte)b);
                        romData.Put(loc + 1, currentTerrain.RomValue());
                    }
                    currentTerrainCount = 0;
                    loc += 2;
                    bytesWritten += 2;
                    continue;
                }
                if (hiddenKasuto && y == 51 && x == 61 && y != 0 && x != 0 && biome.UsesVanillaMap())
                {
                    currentTerrainCount--;
                    int b = currentTerrainCount * 16 + currentTerrain.RomValue();
                    //logger.WriteLine("Hex: {0:X}", b);
                    if (doWrite)
                    {
                        romData.Put(loc, (byte)b);
                        romData.Put(loc + 1, currentTerrain.RomValue());
                    }
                    currentTerrainCount = 0;
                    loc += 2;
                    bytesWritten += 2;
                    continue;
                }

                Terrain nextTerrain = x < MapColumns ? map[y, x] : edgeOfContinentTerrain;
                if (currentTerrainCount == 16 || currentTerrain != nextTerrain)
                {
                    //First 4 bits are the number of tiles to draw with that Terrain type. Last 4 are the Terrain type.
                    int b = ((currentTerrainCount - 1) << 4) | currentTerrain.RomValue();
                    //logger.WriteLine("Hex: {0:X}", b);
                    if (doWrite)
                    {
                        romData.Put(loc, (byte)b);
                    }
                    currentTerrain = nextTerrain;
                    //This is almost certainly an off by one error, but very very unlikely to matter
                    currentTerrainCount = 1;
                    loc++;
                    bytesWritten++;
                }
                else
                {
                    currentTerrainCount++;
            }
            }
            //Write the last Terrain segment for this row
            currentTerrainCount--;
            int b2 = currentTerrainCount * 16 + currentTerrain.RomValue();
            //logger.WriteLine("Hex: {0:X}", b2);
            if (doWrite)
            {
                romData.Put(loc, (byte)b2);
            }

            if (y < MapRows - 1)
            {
                currentTerrain = map[y + 1, 0];
            }
            currentTerrainCount = 0;
            loc++;
            bytesWritten++;
        }
        int nonPaddingBytesWritten = bytesWritten;

        //Fill any remaining map space in the rom with filler. (Replace 0x0B with a constant)
        while (bytesWritten < total)
        {
            romData.Put(loc, (byte)0x0B);
            bytesWritten++;
            loc++;
        }

        return nonPaddingBytesWritten;
    }

    protected bool DrawOcean(Direction direction, Terrain oceanTerrain)
    {
        int x;
        int y;
        int olength = 0;

        if (direction == Direction.WEST)
        {
            x = 0;
            y = RNG.Next(MapRows);
            olength = RNG.Next(Math.Max(y, MapRows - y));
        }
        else if (direction == Direction.EAST)
        {
            x = MapColumns - 1;
            y = RNG.Next(MapRows);
            olength = RNG.Next(Math.Max(y, MapRows - y));
        }
        else if (direction == Direction.NORTH)
        {
            x = RNG.Next(MapColumns);
            y = 0;
            olength = RNG.Next(Math.Max(x, MapColumns - x));
        }
        else //south
        {
            x = RNG.Next(MapColumns);
            y = MapRows - 1;
            olength = RNG.Next(Math.Max(x, MapColumns - x));
        }
        //draw ocean on right side

        if (direction == Direction.EAST || direction == Direction.WEST)
        {
            if (y < MapRows / 2)
            {
                for (int i = 0; i < olength; i++)
                {
                    var t = map[y + i, x];
                    if (t != Terrain.NONE && biome != Biome.MOUNTAINOUS && biome != Biome.CALDERA)
                    {
                        logger.LogDebug("DrawOcean could not add water west"); 
                        return false;
                    }
                    map[y + i, x] = oceanTerrain;
                }
            }
            else
            {
                try
                {
                    for (int i = 0; i < olength; i++)
                    {
                        var t = map[y - i, x];
                        if (t != Terrain.NONE && biome != Biome.MOUNTAINOUS && biome != Biome.CALDERA)
                        {
                            logger.LogDebug("DrawOcean could not add water east");
                            return false;
                        }
                        map[y - i, x] = oceanTerrain;
                    }
                }
                catch (IndexOutOfRangeException e)
                {
                    logger.Error(e);
                }
            }
        }
        else // north or south
        {
            if (x < MapColumns / 2)
            {
                for (int i = 0; i < olength; i++)
                {
                    var t = map[y, x + i];
                    if (t != Terrain.NONE && biome != Biome.MOUNTAINOUS && biome != Biome.CALDERA)
                    {
                        logger.LogDebug("DrawOcean could not add water north");
                        return false;
                    }
                    map[y, x + i] = oceanTerrain;
                }
            }
            else
            {
                for (int i = 0; i < olength; i++)
                {
                    var t = map[y, x - 1];
                    if (t != Terrain.NONE && biome != Biome.MOUNTAINOUS && biome != Biome.CALDERA)
                    {
                        logger.LogDebug("DrawOcean could not add water south");
                        return false;
                    }
                    map[y, x - i] = oceanTerrain;
                }
            }
        }
        return true;
    }

    protected void UpdateReachable(IReadOnlySet<RequirementType> requireables)
    {

        List<Location> starts = GetPathingStarts();

        bool[,] covered = new bool[MapRows, MapColumns];
        for (int i = 0; i < MapRows; i++)
        {
            for (int j = 0; j < MapColumns; j++)
            {
                covered[i, j] = false;
            }
        }

        //Run the initial steps
        foreach (Location start in starts)
        {
            if (start.Y >= 0 && start.Xpos >= 0)
            {
                UpdateReachable(ref covered, start.Y, start.Xpos, requireables);
            }
        }

        OnUpdateReachableTrigger();
    }

    protected virtual void OnUpdateReachableTrigger()
    {
        //This space intentionally left blank
    }

    //This signature has gotten out of control, consider a refactor
    protected void UpdateReachable(ref bool[,] covered, int start_y, int start_x, IReadOnlySet<RequirementType> requireables)
    {
        Stack<(int, int)> to_visit = new();
        // push the initial coord to the visitation stack
        to_visit.Push((start_y, start_x));
        try
        {
            while (to_visit.Count > 0)
            {
                var (y, x) = to_visit.Pop();
                // This shouldn't happen during the loop but could maybe happen from the function caller
                if (covered[y, x])
                {
                    continue;
                }
                covered[y, x] = true;
                Location? location = GetLocationByCoordsNoOffset((y, x));


                Terrain terrain = map[y, x];
                if ((terrain == Terrain.LAVA
                    || terrain == Terrain.BRIDGE
                    || terrain == Terrain.CAVE
                    || terrain == Terrain.ROAD
                    || terrain == Terrain.PALACE
                    || terrain == Terrain.TOWN
                    || walkableTerrains.Contains(terrain)
                    || (terrain == Terrain.WALKABLEWATER && requireables.Contains(RequirementType.BOOTS))
                    || (terrain == Terrain.PREPLACED_WATER_WALKABLE && requireables.Contains(RequirementType.BOOTS))
                    || (terrain == Terrain.ROCK && requireables.Contains(RequirementType.HAMMER))
                    || (terrain == Terrain.RIVER_DEVIL && requireables.Contains(RequirementType.FLUTE)))
                    && (location == null || location!.AccessRequirements.AreSatisfiedBy(requireables))
                )
                {
                    visitation[y, x] = true;
                    if (y - 1 >= 0 && !covered[y - 1, x])
                    {
                        to_visit.Push((y - 1, x));
                    }
                    if (y + 1 < MapRows && !covered[y + 1, x])
                    {
                        to_visit.Push((y + 1, x));
                    }
                    if (x - 1 >= 0 && !covered[y, x - 1])
                    {
                        to_visit.Push((y, x - 1));
                    }
                    if (x + 1 < MapColumns && !covered[y, x + 1])
                    {
                        to_visit.Push((y, x + 1));
                    }
                }
            }
        }
        catch (IndexOutOfRangeException)
        {
            logger.Debug("?");
            throw;
        }
    }

    //Should the visibility calculation table even be persistent? Why is this not just in scope of the calculation itself?
    public void ResetVisitabilityState()
    {
        for (int i = 0; i < MapRows; i++)
        {
            for (int j = 0; j < MapColumns; j++)
            {
                visitation[i, j] = false;
            }
        }
        foreach (Location location in AllLocations)
        {
            location.CanShuffle = true;
            location.Reachable = false;
        }
    }

    protected bool DrawBridge(Direction direction)
    {
        return DrawBridge(RNG, map, bridge, walkableTerrains, direction);
    }

    public static bool DrawBridge(Random r, OverworldMap map, Location? bridge, List<Terrain> walkableTerrains, Direction direction)
    {
        if (bridge == null) { throw new Exception("Unable to draw unloaded bridge"); }
        int x = 0;
        int y = 0;
        GetAwayFromEdgeDeltas(direction, out int deltax, out int deltay);

        int tries = 0;
        int length = 0;

        do
        {
            length = 0;
            tries++;
            if (tries > 100)
            {
                return false;
            }

            if (!FindWaterTileAtEdge(r, map, direction, out x, out y, 100))
            {
                return false;
            }

            length = MeasureWaterPath(map, x, y, deltax, deltay, out x, out y);
        }
        while (length <= 1 || length > 10 || !IsValidEndTile(map, walkableTerrains, x, y));

        y -= deltay;
        x -= deltax;

        PlaceBridge(map, bridge, x, y, direction);

        return true;
    }

    protected bool DrawRaft(Direction direction)
    {
        return DrawRaft(RNG, map, raft, walkableTerrains, direction);
    }

    public static bool DrawRaft(Random r, OverworldMap map, Location? raft, List<Terrain> walkableTerrains, Direction direction)
    {
        if (raft == null) { throw new Exception("Unable to draw unloaded raft"); }
        int x = 0;
        int y = 0;
        GetAwayFromEdgeDeltas(direction, out int deltax, out int deltay);

        int tries = 0;

        do
        {
            tries++;
            if (tries > 100)
            {
                return false;
            }

            if (!FindWaterTileAtEdge(r, map, direction, out x, out y, 100))
            {
                return false;
            }

            MeasureWaterPath(map, x, y, deltax, deltay, out x, out y);
        }
        while (!IsValidEndTile(map, walkableTerrains, x, y));

        y -= deltay;
        x -= deltax;

        PlaceRaft(map, raft, x, y);

        return true;
    }

    /// Tries to find a water tile at the edge of the map in `direction`.
    /// If a water tile is found, true is returned and (x,y) contains the position.
    /// Otherwise, false is returned and (x,y) is set to (0,0).
    protected static bool FindWaterTileAtEdge(Random r, OverworldMap map, Direction direction, out int x, out int y, int maxTries)
    {
        int mapRows = map.GetLength(0);
        int mapCols = map.GetLength(1);

        switch (direction)
        {
            case Direction.WEST:
            case Direction.EAST:
                x = direction == Direction.WEST ? 0 : mapCols - 1;
                for (int tries = 0; tries < maxTries; tries++)
                {
                    y = r.Next(0, mapRows);
                    if (map[y, x].IsWater()) { return true; }
                }
                break;

            case Direction.NORTH:
            case Direction.SOUTH:
                y = direction == Direction.NORTH ? 0 : mapRows - 1;
                for (int tries = 0; tries < maxTries; tries++)
                {
                    x = r.Next(0, mapCols);
                    if (map[y, x].IsWater()) { return true; }
                }
                break;

            default:
                throw new ArgumentOutOfRangeException(nameof(direction));
        }

        x = 0; y = 0;
        return false;
    }

    /// Get (dx,dy) vector pointing away from the edge for `direction`.
    protected static void GetAwayFromEdgeDeltas(Direction direction, out int dx, out int dy)
    {
        (dx, dy) = direction switch
        {
            Direction.WEST => (1, 0),
            Direction.EAST => (-1, 0),
            Direction.NORTH => (0, 1),
            Direction.SOUTH => (0, -1),
            _ => (0, 0)
        };
    }

    /// Walk from (startX,startY) in (dx,dy) direction until we are no longer on water.
    /// End position is stored in (endX,endY) and the length of the path is returned.
    protected static int MeasureWaterPath(OverworldMap map, int startX, int startY, int dx, int dy, out int endX, out int endY)
    {
        int mapRows = map.GetLength(0);
        int mapCols = map.GetLength(1);
        int length = 0;
        int x = startX;
        int y = startY;

        while (y >= 0 && y < mapRows && x >= 0 && x < mapCols && map[y, x].IsWater())
        {
            y += dy;
            x += dx;
            length++;
        }

        endX = x;
        endY = y;
        return length;
    }

    /// Check for DrawBridge & DrawRaft if (x,y) would be an acceptable placement
    protected static bool IsValidEndTile(OverworldMap map, List<Terrain> walkableTerrains, int x, int y)
    {
        int mapRows = map.GetLength(0);
        int mapCols = map.GetLength(1);

        if (x < 0 || y < 0 || x >= mapCols || y >= mapRows) { return false; }
        return walkableTerrains.Contains(map[y, x]);
    }

    public static void PlaceBridge(OverworldMap map, Location bridge, int x, int y, Direction direction)
    {
        int mapRows = map.GetLength(0);
        int mapCols = map.GetLength(1);

        map[y, x] = Terrain.BRIDGE;
        bridge.Xpos = x;
        bridge.Y = y;
        bridge.IsPassthrough = false;
        bridge.CanShuffle = false;

        if (direction == Direction.EAST)
        {
            for (int i = x + 1; i < mapCols; i++)
            {
                map[y, i] = Terrain.BRIDGE;
            }
        }
        else if (direction == Direction.WEST)
        {
            for (int i = x - 1; i >= 0; i--)
            {
                map[y, i] = Terrain.BRIDGE;
            }
        }
        else if (direction == Direction.SOUTH)
        {
            for (int i = y + 1; i < mapRows; i++)
            {
                map[i, x] = Terrain.BRIDGE;
            }
        }
        else if (direction == Direction.NORTH)
        {
            for (int i = y - 1; i >= 0; i--)
            {
                map[i, x] = Terrain.BRIDGE;
            }
        }
    }

    public static void PlaceRaft(OverworldMap map, Location raft, int x, int y)
    {
        map[y, x] = Terrain.BRIDGE;
        raft.Xpos = x;
        raft.Y = y;
        raft.CanShuffle = false;
    }

    public Location LoadRaft(ROM rom, Continent continent, Continent connectedContinent)
    {
        Debug.Assert(continent == continentId); // we can remove param if this is always true
        var newRaft = rom.LoadLocation(LocationIDUtils.FromIndex(continent, Location.CONNECTOR_RAFT_ID), Terrain.BRIDGE);
        AddLocation(newRaft);
        raft = GetLocation(continent, Location.CONNECTOR_RAFT_ID);
        Debug.Assert(newRaft == raft);
        Debug.Assert(raft.Continent == continent);
        raft.ConnectedContinent = connectedContinent;
        raft.IsExternalWorld = true;
        raft.Map = Location.CONNECTOR_RAFT_ID;
        raft.TerrainType = Terrain.BRIDGE;
        if (!WithinMapBounds(raft.Pos))
        {
            raft.Pos = IntVector2.Random(RNG, MapColumns, MapRows);
        }
        return raft;
    }

    public Location LoadBridge(ROM rom, Continent continent, Continent connectedContinent)
    {
        Debug.Assert(continent == continentId); // we can remove param if this is always true
        var newBridge = rom.LoadLocation(LocationIDUtils.FromIndex(continent, Location.CONNECTOR_BRIDGE_ID), Terrain.BRIDGE, false);
        Debug.Assert(newBridge.IsPassthrough == false);
        Debug.Assert(newBridge.WasPassthrough == false);
        AddLocation(newBridge);
        bridge = GetLocation(continent, Location.CONNECTOR_BRIDGE_ID);
        Debug.Assert(newBridge == bridge);
        Debug.Assert(bridge.Continent == continent);
        bridge.ConnectedContinent = connectedContinent;
        bridge.IsExternalWorld = true;
        bridge.Map = Location.CONNECTOR_BRIDGE_ID;
        if (!WithinMapBounds(bridge.Pos))
        {
            bridge.Pos = IntVector2.Random(RNG, MapColumns, MapRows);
        }
        return bridge;
    }

    public Location LoadCave1(ROM rom, Continent world, Continent connectedContinent)
    {
        Debug.Assert(world == continentId); // we can remove param if this is always true
        var newCave = rom.LoadLocation(LocationIDUtils.FromIndex(world, Location.CONNECTOR_CAVE1_ID), Terrain.CAVE);
        AddLocation(newCave);
        cave1 = GetLocation(world, Location.CONNECTOR_CAVE1_ID);
        Debug.Assert(newCave == cave1);
        Debug.Assert(cave1.Continent == world);
        cave1.ConnectedContinent = connectedContinent;
        cave1.IsExternalWorld = true;
        cave1.Map = Location.CONNECTOR_CAVE1_ID;
        cave1.CanShuffle = true;
        if (!WithinMapBounds(cave1.Pos))
        {
            cave1.Pos = IntVector2.Random(RNG, MapColumns, MapRows);
        }
        return cave1;
    }

    public Location LoadCave2(ROM rom, Continent world, Continent connectedContinent)
    {
        Debug.Assert(world == continentId); // we can remove param if this is always true
        var newCave = rom.LoadLocation(LocationIDUtils.FromIndex(world, Location.CONNECTOR_CAVE2_ID), Terrain.CAVE);
        AddLocation(newCave);
        cave2 = GetLocation(world, Location.CONNECTOR_CAVE2_ID);
        Debug.Assert(newCave == cave2);
        Debug.Assert(cave2.Continent == world);
        cave2.ConnectedContinent = connectedContinent;
        cave2.IsExternalWorld = true;
        cave2.Continent = world;
        cave2.Map = Location.CONNECTOR_CAVE2_ID;
        Debug.Assert(cave2.TerrainType == Terrain.CAVE);
        cave2.CanShuffle = true;
        if (!WithinMapBounds(cave2.Pos))
        {
            cave2.Pos = IntVector2.Random(RNG, MapColumns, MapRows);
        }
        return cave2;
    }

    public bool HasConnections()
    {
        return raft != null || bridge != null || cave1 != null || cave2 != null;
    }

    public void VisitRaft()
    {
        if (raft != null)
        {
            visitation[raft.Y, raft.Xpos] = true;
        }
    }

    public void VisitBridge()
    {
        if (bridge != null)
        {
            visitation[bridge.Y, bridge.Xpos] = true;
        }
    }

    public void VisitCave1()
    {
        if (cave1 != null)
        {
            visitation[cave1.Y, cave1.Xpos] = true;
        }
    }

    public void VisitCave2()
    {
        if (cave2 != null)
        {
            visitation[cave2.Y, cave2.Xpos] = true;
        }
    }

    public List<Location> GetContinentConnections()
    {
        return new List<Location>() { cave1!, cave2!, bridge!, raft! }.Where(i => i != null).ToList();
    }

    protected void DrawRiver(bool canWalkOnWaterWithBoots)
    {
        Terrain water = canWalkOnWaterWithBoots ? Terrain.WALKABLEWATER : Terrain.WATER;

        Direction direction1 = DirectionExtensions.RandomCardinal(RNG);
        Direction direction2;
        do
        {
            direction2 = DirectionExtensions.RandomCardinal(RNG);
        } while (direction1 == direction2);

        (IntVector2 Delta, IntVector2 Start) RiverFromDir(Direction direction)
        {
            int rx = RNG.Next(MapColumns / 3, (MapColumns / 3) * 2);
            int ry = RNG.Next(MapRows / 3, (MapRows / 3) * 2);
            IntVector2 start = direction switch
            {
                Direction.SOUTH => new(rx, 0),
                Direction.WEST => new(MapColumns - 1, ry),
                Direction.NORTH => new(rx, MapRows - 1),
                Direction.EAST => new(0, ry),
                _ => throw new Exception("Invalid Direction")
            };
            IntVector2 delta = direction.ToIntVector2();
            return (start, delta);
        }

        IntVector2 AdjustAndPaint(IntVector2 pos, IntVector2 delta, IntVector2 sideDir)
        {
            if (map[pos] != Terrain.NONE) { return pos; }
            map[pos] = water;
            int minAdj = (sideDir.X != 0 && pos.X == 1) || (sideDir.Y != 0 && pos.Y == 1) ? 0 : -1;
            int maxAdj = (sideDir.X != 0 && pos.X == MapColumns - 2) || (sideDir.Y != 0 && pos.Y == MapRows - 2) ? 0 : 1;
            int adjust = RNG.Next(minAdj, maxAdj + 1);
            IntVector2 adjusted = pos + adjust * sideDir;
            if (WithinMapBounds(adjusted) && !IsReserved(adjusted))
            {
                map[adjusted] = water;
            }
            return adjusted;
        }

        var (start, delta) = RiverFromDir(direction1);
        IntVector2 sideDir = delta.Perpendicular();
        int stopping = delta.Y != 0 ? RNG.Next(MapRows / 3, (MapRows / 3) * 2) : RNG.Next(MapColumns / 3, (MapColumns / 3) * 2);

        for (int i = 0; i < stopping; i++)
        {
            start = AdjustAndPaint(start, delta, sideDir);
            start += delta;
        }

        var (_, delta2) = RiverFromDir(direction2);
        delta = delta2;
        sideDir = delta.Perpendicular();

        while (WithinMapBounds(start, 0))
        {
            start = AdjustAndPaint(start, delta, sideDir);
            start += delta;
        }
    }

    public void DrawCanyon(Terrain riverT)
    {
        IntVector2 forward = isHorizontal ? IntVector2.EAST : IntVector2.SOUTH;
        IntVector2 side = isHorizontal ? IntVector2.SOUTH : IntVector2.EAST;
        int forwardLen = isHorizontal ? MapColumns : MapRows;
        int sideLen = isHorizontal ? MapRows : MapColumns;
        int minDist = Math.Min(sideLen / 2 - 1, 15);
        int drawLeft = RNG.Next(0, 5);
        int drawRight = RNG.Next(0, 5);
        Terrain tleft = climate.GetRandomTerrain(RNG, walkableTerrains);
        Terrain tright = climate.GetRandomTerrain(RNG, walkableTerrains);
        int riverSide = RNG.Next(minDist, sideLen - minDist);

        for (int f = 0; f < forwardLen; f++)
        {
            drawLeft++;
            drawRight++;
            IntVector2 basePos = f * forward;
            map[basePos + riverSide * side] = riverT;
            map[basePos + (riverSide + 1) * side] = riverT;

            int adjust = RNG.Next(-3, isHorizontal ? 3 : 4);
            int leftM = RNG.Next(14, 17);
            if (riverSide - leftM > 0)
            {
                map[basePos + (riverSide - leftM + 3) * side] = tleft;
            }
            if (drawLeft % 5 == 0)
            {
                tleft = climate.GetRandomTerrain(RNG, walkableTerrains);
            }
            for (int i = riverSide - leftM; i >= 0; i--)
            {
                map[basePos + i * side] = Terrain.MOUNTAIN;
            }
            int rightM = RNG.Next(14, 17);
            if (riverSide + rightM < sideLen)
            {
                map[basePos + (riverSide + rightM - 3) * side] = tright;
            }
            if (drawRight % 5 == 0)
            {
                tright = climate.GetRandomTerrain(RNG, walkableTerrains);
            }
            for (int i = riverSide + 1 + rightM; i < sideLen; i++)
            {
                map[basePos + i * side] = Terrain.MOUNTAIN;
            }
            while (riverSide + adjust + 1 > sideLen - minDist || riverSide + adjust < minDist)
            {
                adjust = RNG.Next(-1, 2);
            }
            int oldSide = riverSide;
            riverSide += adjust;
            for (int s = oldSide; adjust > 0 ? s < riverSide : s > riverSide; s += Math.Sign(adjust))
            {
                map[basePos + s * side] = riverT;
            }
        }
    }

    public void DrawCenterMountain()
    {
        IntVector2 forward = isHorizontal ? IntVector2.SOUTH : IntVector2.EAST;
        IntVector2 side = isHorizontal ? IntVector2.EAST : IntVector2.SOUTH;
        int forwardLen = isHorizontal ? MapRows : MapColumns;
        int sideLen = isHorizontal ? MapColumns : MapRows;
        int top = (forwardLen - 35) / 2;
        int bottom = forwardLen - top;
        int sideCenter = sideLen / 2;

        // Block out stripes outside caldera zone
        for (int f = 0; f < forwardLen; f++)
        {
            if (f < top || f > bottom)
            {
                for (int s = 0; s < sideLen; s++)
                {
                    map[f * forward + s * side] = Terrain.MOUNTAIN;
                }
            }
        }

        // Top triangle (widening)
        for (int y = 0; y < 8; y++)
        {
            int half = 3 + y;
            for (int s = sideCenter - half; s < sideCenter + half; s++)
            {
                map[(top + y) * forward + s * side] = Terrain.MOUNTAIN;
            }
        }

        // Middle rectangle
        for (int i = 0; i < 19; i++)
        {
            for (int s = sideCenter - 10; s < sideCenter + 10; s++)
            {
                map[(top + 8 + i) * forward + s * side] = Terrain.MOUNTAIN;
            }
        }

        // Bottom triangle (narrowing)
        for (int i = 0; i < 8; i++)
        {
            int half = 3 + (6 - i);
            for (int s = sideCenter - half; s < sideCenter + half; s++)
            {
                map[(top + 27 + i) * forward + s * side] = Terrain.MOUNTAIN;
            }
        }
    }

    /// <summary>
    /// Returns true iff position is within the bounds of the map. 
    /// Useful to avoid index out of bounds errors.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    protected bool WithinMapBounds(IntVector2 pos)
    {
        return pos.X >= 0 && pos.X < MapColumns
            && pos.Y >= 0 && pos.Y < MapRows;
    }

    /// <summary>
    /// Useful variant to verify that a coordinate is some margin away
    /// from the edge of the map.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    protected bool WithinMapBounds(IntVector2 pos, int margin)
    {
        return pos.X >= margin && pos.X < MapColumns - margin
            && pos.Y >= margin && pos.Y < MapRows - margin;
    }

    /// <summary>
    /// Return true iff there's a cave on the map in the 3x3 tiles centered on `pos`.
    /// Note: It's the callers responsibility that these tiles are all in bounds.
    /// </summary>
    protected bool HasAdjacentCave(IntVector2 pos)
    {
        for (int y = pos.Y - 1; y <= pos.Y + 1; y++)
        {
            for (int x = pos.X - 1; x <= pos.X + 1; x++)
            {
                if (map[y, x] == Terrain.CAVE)
                {
                    return true;
                }
            }
        }
        return false;
    }

    protected bool FindPassthroughCave(IntVector2 pos, IntVector2 dir, Location cave1, Location cave2)
    {
        var sideDir = dir.Perpendicular();

        while (true)
        {
            if (!WithinMapBounds(pos, 1))
            {
                logger.LogDebug("Start cave reached end of map");
                return false;
            }
            if (map[pos.Y, pos.X] == Terrain.MOUNTAIN)
            {
                break;
            }
            pos += dir;
        }

        IntVector2? newPos = MoveAwayIfNextToCave(pos, sideDir);
        if (newPos == null) { return false; }
        pos = newPos.Value;
        // Settled on start cave position
        var cave1pos = pos;

        int length = 0;
        while (true)
        {
            pos += dir;
            length++;
            if (!WithinMapBounds(pos, 1))
            {
                logger.LogDebug("Ending cave reached end of map");
                return false;
            }
            if (map[pos.Y, pos.X] != Terrain.MOUNTAIN)
            {
                break;
            }
            if (length > 2)
            {
                // check sideways for exits as well
                foreach (int i in new[] { -1, 1, -2, 2 })
                {
                    var sidePos = pos + i * sideDir;
                    if (!WithinMapBounds(sidePos, 1)) { continue; }
                    if (map[sidePos.Y, sidePos.X] != Terrain.MOUNTAIN &&
                        map[sidePos.Y, sidePos.X] != Terrain.CAVE)
                    {
                        pos = sidePos;
                        break;
                    }
                }
            }
        }

        if (length <= 2)
        {
            logger.LogDebug("Cave passthrough path not long enough");
            return false;
        }

        if (HasAdjacentCave(pos))
        {
            logger.LogDebug("Cave exit is too close to existing cave");
            return false;
        }

        pos -= dir;
        // settled on ending cave position
        var cave2pos = pos;

        map[cave1pos.Y, cave1pos.X] = Terrain.CAVE;
        cave1.Pos = cave1pos;
        map[cave2pos.Y, cave2pos.X] = Terrain.CAVE;
        cave2.Pos = cave2pos;

        // add mountains on all sides of our new exit
        var s1 = cave2pos - sideDir;
        map[s1.Y, s1.X] = Terrain.MOUNTAIN;
        var s2 = cave2pos + sideDir;
        map[s2.Y, s2.X] = Terrain.MOUNTAIN;
        var s3 = cave2pos - dir;
        map[s3.Y, s3.X] = Terrain.MOUNTAIN;

        return true;
    }

    private IntVector2? MoveAwayIfNextToCave(IntVector2 pos, IntVector2 sideDir)
    {
        int i = 0;
        IntVector2 tryPos = pos;
        while (true)
        {
            if (!HasAdjacentCave(tryPos))
            {
                return tryPos;
            }
            int offset = RNG.Next(-1, 2);
            tryPos = pos + offset * sideDir;
            if (++i == 6)
            {
                logger.LogDebug("Start cave too close to another cave");
                return null;
            }
        }
    }

    /// <summary>
    /// Tries to find and set positions for two caves to pass through a mountain area by carving
    /// a horizontal passthrough near a given center point. (Currently only used for Caldera.)
    /// <para />
    /// See <see cref="VerticalCave(int, int, int, Location, Location)"/> for more documentation.
    /// </summary>
    protected bool HorizontalCave(int caveDirection, int centerX, int centerY, Location caveLeft, Location caveRight)
    {
        int rndY = RNG.Next(centerY - 2, centerY + 3);
        if (caveDirection == 0)
        {
            return FindPassthroughCave(new(centerX, rndY), IntVector2.WEST, caveRight, caveLeft);
        }
        else
        {
            return FindPassthroughCave(new(centerX, rndY), IntVector2.EAST, caveLeft, caveRight);
        }
    }

    /// <summary>
    /// Tries to find and set positions for two caves to pass through a mountain area by carving
    /// a vertical passthrough near a given center point. (Currently only used for Caldera.)
    /// <para />
    ///
    /// The method randomly selects a column within a small horizontal range around the center,
    /// then searches vertically (upward or downward depending on <paramref name="caveDirection"/>)
    /// for a contiguous stretch of mountain tiles. If a sufficiently deep mountain column is
    /// found, exactly two cave tiles are placed at opposite ends of that stretch, with mountain
    /// walls preserved on both sides of each entrance.
    /// </summary>
    /// <remarks>
    /// The input coordinates of the provided <see cref="Location"/> objects are ignored.
    /// On success, their coordinates are overwritten with the final cave positions.
    /// <para />
    ///
    /// Cave placement is attempted in a column chosen randomly from
    /// <c>centerX - 2</c> through <c>centerX + 2</c>, and searches vertically toward the
    /// nearest map edge. Placement fails if the mountain run is too short, the search
    /// exceeds map bounds, or the caves would intersect or touch existing caves.
    /// </remarks>
    /// <returns>
    /// <c>true</c> if a valid vertical cave passthrough was created; otherwise <c>false</c>.
    /// </returns>
    protected bool VerticalCave(int caveDirection, int centerX, int centerY, Location caveLeft, Location caveRight)
    {
        int rndX = RNG.Next(centerX - 2, centerX + 3);
        if (caveDirection == 0)
        {
            return FindPassthroughCave(new(rndX, centerY), IntVector2.NORTH, caveRight, caveLeft);
        }
        else
        {
            return FindPassthroughCave(new(rndX, centerY), IntVector2.SOUTH, caveLeft, caveRight);
        }
    }

    // subset of walkable terrains that we would expect to be next to a trap tile
    protected static readonly FrozenSet<Terrain> TRAP_PATH_VALID_TERRAIN = [Terrain.DESERT, Terrain.GRASS, Terrain.ROAD, Terrain.LAVA, Terrain.NONE];
    protected static readonly FrozenSet<Terrain> TRAP_PATH_BLOCKING_TERRAIN = [Terrain.MOUNTAIN, Terrain.WATER];

    /// <summary>
    /// Check if the cave is possible to be blocked. It should<br/>
    /// - Be surrounded by mountains in exactly 3 directions.<br/>
    /// - There must not be any location on the tiles we will mutate
    ///   when we move the mountain back to fit the block tile.<br/>
    /// </summary>
    public IntVector2? ValidBlockingCavePosition(IntVector2 pos)
    {
        if (!WithinMapBounds(pos, 1)) { return null; }

        bool isMountain(IntVector2 pos) => map[pos.Y, pos.X] == Terrain.MOUNTAIN;
        bool checkForExistingLocation(IntVector2 pos)
        {
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

    protected virtual bool IsReserved(IntVector2 pos)
    {
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
        bool isPassable(IntVector2 pos) => TRAP_PATH_VALID_TERRAIN.Contains(map[pos.Y, pos.X]);
        bool isBlocking(IntVector2 pos) => TRAP_PATH_BLOCKING_TERRAIN.Contains(map[pos.Y, pos.X]);

        if (!WithinMapBounds(pos, 1)) { return null; }
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
                if(globs[y,x] < 10)
                {
                    debug.Append(globs[y, x]);
                }
                else if(globs[y, x] < 36)
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

    public bool ValidateCaves()
    {
        for(int y = 0; y < MapRows; y++)
        {
            for(int x = 0; x < MapColumns; x++)
            {
                if (map[y,x] == Terrain.CAVE)
                {
                    //If all 4 sides of a cave are unwalkable terrain, you can never leave the cave
                    //and you softlock since you can't turn around
                    if (
                        (y + 1 >= MapRows || !map[y + 1, x].IsWalkable())
                        && (y == 0 || !map[y - 1, x].IsWalkable())
                        && (x + 1 >= MapColumns || !map[y, x + 1].IsWalkable())
                        && (x == 0 || !map[y, x - 1].IsWalkable())
                    )
                    {
                        return false;
                    }
                    //If a cave exits into a lake, you softlock unless you have the boots, and the previous case
                    //(correctly) considers walkable water as walkable, so we need to cover that case here.
                    if (
                        y + 1 < MapRows && map[y + 1, x] == Terrain.WALKABLEWATER
                        && (y == 0 || !map[y - 1, x].IsWalkable())
                        && (x + 1 >= MapColumns || !map[y, x + 1].IsWalkable())
                        && (x == 0 || !map[y, x - 1].IsWalkable())
                    )
                    {
                        map[y + 1, x] = Terrain.ROAD;
                    }
                    if (
                        (y + 1 >= MapRows || !map[y + 1, x].IsWalkable())
                        && y > 0 && map[y - 1, x] == Terrain.WALKABLEWATER
                        && (x + 1 >= MapColumns || !map[y, x + 1].IsWalkable())
                        && (x == 0 || !map[y, x - 1].IsWalkable())
                    )
                    {
                        map[y - 1, x] = Terrain.ROAD;
                    }
                    if (
                        (y + 1 >= MapRows || !map[y + 1, x].IsWalkable())
                        && (y == 0 || !map[y - 1, x].IsWalkable())
                        && x + 1 < MapColumns && map[y, x + 1] == Terrain.WALKABLEWATER
                        && (x == 0 || !map[y, x - 1].IsWalkable())
                    )
                    {
                        map[y, x + 1] = Terrain.ROAD;
                    }
                    if (
                        (y + 1 >= MapRows || !map[y + 1, x].IsWalkable())
                        && (y == 0 || !map[y - 1, x].IsWalkable())
                        && (x + 1 >= MapColumns || !map[y, x + 1].IsWalkable())
                        && x > 0 && map[y, x - 1] == Terrain.WALKABLEWATER
                    )
                    {
                        map[y, x - 1] = Terrain.ROAD;
                    }
                }
            }
        }

        return true;
    }

    /*
    public bool PassthroughsIntersectRaftCoordinates(IEnumerable<(int, int)> raftCoordinates)
    {
        foreach (Location location in AllLocations.Where(i => i.PassThrough != 0))
        {
            if (raftCoordinates.Any(i => location.Xpos == i.Item2 && location.YRaw == i.Item1))
            {
                return true;
            }
        }
        return false;
    }
    */

    //Short term fix, right now linked locations aren't shuffled, and they are counted as reachable when
    //their parent location is reachable, but the original location remains in the location list, creating
    //a phantom reachability spot at the vanilla location.
    //What _should_ happen is there should be a unified interface for updating locations that automicatically
    //updates the coordinates, map, linked locations, etc. But that's more work than I want to do, so here's this lazy hack
    public void SynchronizeLinkedLocations()
    {
        foreach (Location parent in AllLocations)
        {
            foreach (Location child in parent.Children)
            {
                child.Xpos = parent.Xpos;
                child.Y = parent.Y;
            }
        }
    }

    /// <summary>
    /// Updates the visitation matrix and location reachability 
    /// </summary>
    public virtual void UpdateVisit(IReadOnlySet<RequirementType> requireables)
    {
        UpdateReachable(requireables);

        foreach (Location location in AllLocations)
        {
            if (location.Y > 0 && visitation[location.Y, location.Xpos])
            {
                if (location.AccessRequirements.AreSatisfiedBy(requireables))
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

    public abstract IEnumerable<Location> RequiredLocations(bool hiddenPalace, bool hiddenKasuto);

    protected abstract void SetVanillaCollectables(bool useDash);

    public abstract string GenerateSpoiler();
}