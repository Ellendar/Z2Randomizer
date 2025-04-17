using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.DirectoryServices.ActiveDirectory;
using System.Linq;
using System.Text;
using NLog;
using RandomizerCore;
using Z2Randomizer.Core.Sidescroll;
//using System.Runtime.InteropServices.WindowsRuntime;

namespace Z2Randomizer.Core.Overworld;

public abstract class World
{
    protected readonly Logger logger = LogManager.GetCurrentClassLogger();
    protected SortedDictionary<string, List<Location>> areasByLocation;
    /*private List<Location> caves;
    private List<Location> towns;
    private List<Location> palaces;
    private List<Location> grasses;
    private List<Location> swamps;
    private List<Location> bridges;
    private List<Location> deserts;
    private List<Location> forests;
    private List<Location> graves;
    private List<Location> lavas;
    private List<Location> roads;
    private List<Location> allLocations;
    */
    public Dictionary<Location, Location> connections;
    //protected HashSet<String> reachableAreas;
    protected int enemyAddr;
    protected List<int> enemies;
    protected List<int> flyingEnemies;
    protected List<int> generators;
    protected List<int> smallEnemies;
    protected List<int> largeEnemies;
    protected int enemyPtr;
    protected List<int> overworldMaps;
    protected SortedDictionary<(int, int), Location> locsByCoords;
    //protected Hyrule hyrule;
    protected Terrain[,] map;
    //private List<int> visitedEnemies;
    protected int MAP_ROWS;
    protected int MAP_COLS;
    //protected int bytesWritten;
    protected List<Terrain> randomTerrainFilter;
    protected List<Terrain> walkableTerrains;
    protected bool[,] visitation;
    protected const int MAP_SIZE_BYTES = 1400;
    protected List<Location> unimportantLocs;
    protected Biome biome;
    protected bool isHorizontal;
    protected int VANILLA_MAP_ADDR;
    protected SortedDictionary<(int, int), string> section;
    public Location raft;
    public Location bridge;
    public Location cave1;
    public Location cave2;
    //private bool allreached;

    public int baseAddr;
    protected Climate climate;

    protected Random RNG;

    //private const int MAXIMUM_BRIDGE_LENGTH = 10;
    private const int MINIMUM_BRIDGE_LENGTH = 2;

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
    public Dictionary<Terrain, List<Location>> Locations { get; set; }

    public bool AllReached { get; set; }

    public World(Random r)
    {
        RNG = r;

        connections = new Dictionary<Location, Location>();
        Locations = new Dictionary<Terrain, List<Location>>();
        foreach (Terrain Terrain in Enum.GetValues(typeof(Terrain)))
        {
            Locations.Add(Terrain, new List<Location>());
        }
        //Locations = new List<Location>[11] { Towns, Caves, Palaces, Bridges, Deserts, Grasses, Forests, Swamps, Graves, Roads, Lavas };
        AllLocations = new List<Location>();
        locsByCoords = new SortedDictionary<(int, int), Location>();
        //reachableAreas = new HashSet<string>();
        unimportantLocs = new List<Location>();
        areasByLocation = new SortedDictionary<string, List<Location>>();
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
        //locsByCoords.Add(l.Coords, l);
    }

    protected void ShuffleLocations(List<Location> locationsToShuffle)
    {
        List<Location> shufflableLocations = locationsToShuffle.Where(i => i.CanShuffle && i.AppearsOnMap).ToList();
        for (int i = shufflableLocations.Count() - 1; i > 0; i--)
        {
            int n = RNG.Next(i + 1);
            Swap(shufflableLocations[i], shufflableLocations[n]);
        }
    }


    protected static void Swap(Location l1, Location l2)
    {
        (l2.Xpos, l1.Xpos) = (l1.Xpos, l2.Xpos);
        (l2.Ypos, l1.Ypos) = (l1.Ypos, l2.Ypos);
        (l2.PassThrough, l1.PassThrough) = (l1.PassThrough, l2.PassThrough);
    }

    //TODO: This should work like the new palace enemy shuffle. Shuffle should set what enemies are where into
    //enemies arrays for the different encouter types, and then there should be a separate write step.
    public void ShuffleEnemies(ROM romData, int addr, bool generatorsAlwaysMatch, bool mixLargeAndSmallEnemies)
    {
        int? firstGenerator = null;
        if (addr != 0x95A4)
        {
            int numBytes = romData.GetByte(addr);
            for (int j = addr + 2; j < addr + numBytes; j += 2)
            {
                int enemy = romData.GetByte(j) & 0x3F;
                int highPart = romData.GetByte(j) & 0xC0;
                if (mixLargeAndSmallEnemies)
                {
                    if (enemies.Contains(enemy))
                    {
                        int swap = enemies[RNG.Next(0, enemies.Count)];
                        romData.Put(j, (byte)(swap + highPart));
                        if (smallEnemies.Contains(enemy) && largeEnemies.Contains(swap) && swap != 0x20)
                        {
                            int ypos = romData.GetByte(j - 1) & 0xF0;
                            int xpos = romData.GetByte(j - 1) & 0x0F;
                            ypos -= 32;
                            romData.Put(j - 1, (byte)(ypos + xpos));
                        }
                        else if (swap == 0x20 && swap != enemy)
                        {
                            int ypos = romData.GetByte(j - 1) & 0xF0;
                            int xpos = romData.GetByte(j - 1) & 0x0F;
                            ypos -= 48;
                            romData.Put(j - 1, (byte)(ypos + xpos));
                        }
                        else if (enemy == 0x1F && swap != enemy)
                        {
                            int ypos = romData.GetByte(j - 1) & 0xF0;
                            int xpos = romData.GetByte(j - 1) & 0x0F;
                            ypos -= 16;
                            romData.Put(j - 1, (byte)(ypos + xpos));
                        }
                    }
                }
                else
                {

                    if (largeEnemies.Contains(enemy))
                    {
                        int swap = RNG.Next(0, largeEnemies.Count);
                        if (largeEnemies[swap] == 0x20 && largeEnemies[swap] != enemy)
                        {
                            int ypos = romData.GetByte(j - 1) & 0xF0;
                            int xpos = romData.GetByte(j - 1) & 0x0F;
                            ypos -= 48;
                            romData.Put(j - 1, (byte)(ypos + xpos));
                        }
                        romData.Put(j, (byte)(largeEnemies[swap] + highPart));
                    }

                    if (smallEnemies.Contains(enemy))
                    {
                        int swap = RNG.Next(0, smallEnemies.Count);
                        romData.Put(j, (byte)(smallEnemies[swap] + highPart));
                    }
                }

                if (flyingEnemies.Contains(enemy))
                {
                    int swap = RNG.Next(0, flyingEnemies.Count);
                    romData.Put(j, (byte)(flyingEnemies[swap] + highPart));

                    if (flyingEnemies[swap] == 0x07 || flyingEnemies[swap] == 0x0a || flyingEnemies[swap] == 0x0d || flyingEnemies[swap] == 0x0e)
                    {
                        int ypos = 0x00;
                        int xpos = romData.GetByte(j - 1) & 0x0F;
                        romData.Put(j - 1, (byte)(ypos + xpos));
                    }
                }

                if (generators.Contains(enemy))
                {
                    int swap = RNG.Next(0, generators.Count);
                    firstGenerator ??= generators[swap];
                    if (generatorsAlwaysMatch)
                    {
                        romData.Put(j, (byte)(firstGenerator + highPart));
                    }
                    else
                    {
                        romData.Put(j, (byte)(generators[swap] + highPart));
                    }
                }

                //Moblin generators can become things, but things can't become moblin generators.
                //Why? I assume it causes some kind of issue, but I've never investigated.
                if (enemy == 0x21)
                {
                    int swap = RNG.Next(0, generators.Count + 1);
                    firstGenerator ??= swap == generators.Count ? 0x21 : generators[swap];
                    if (generatorsAlwaysMatch)
                    {
                        romData.Put(j, (byte)(firstGenerator + highPart));
                    }
                    else if (swap != generators.Count)
                    {
                        romData.Put(j, (byte)(generators[swap] + highPart));
                    }
                }
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
                if (location.TerrainType == Terrain.PALACE || location.TerrainType == Terrain.TOWN || location.Item != Item.DO_NOT_USE)
                {
                    if (!location.Reachable)
                    {
                        AllReached = false;
                    }
                }
            }
        }
    }

    protected Location GetLocationByCoords((int, int)coords)
    {
        Location location = null;
        foreach (Location loc in AllLocations)
        {
            if (loc.Coords.Equals(coords))
            {
                location = loc;
                break;
            }
        }
        if (location == null)
        {
            //Console.Write(coords);
        }
        return location;
    }

    protected Location GetLocationByMem(int mem)
    {
        Location location = null;
        foreach (Location loc in AllLocations)
        {
            if (loc.MemAddress == mem)
            {
                location = loc;
                break;
            }
        }
        return location;
    }

    public void ShuffleOverworldEnemies(ROM romData, bool generatorsAlwaysMatch, bool mixLargeAndSmallEnemies)
    {
        List<int> shuffledEncounters = new List<int>();
        //0x4581
        for (int i = enemyPtr; i < enemyPtr + 126; i += 2)
        {
            int low = romData.GetByte(i);
            int high = romData.GetByte(i + 1);
            high <<= 8;
            high &= 0x0FFF;
            int addr = high + low + enemyAddr;
            if(shuffledEncounters.Contains(addr))
            {
                logger.Debug("Duplicate encounter");
            }
            else
            {
                shuffledEncounters.Add(addr);
                ShuffleEnemies(romData, addr, generatorsAlwaysMatch, mixLargeAndSmallEnemies);
            }
        }
        //{ 0x22, 0x1D, 0x27, 0x30, 0x23, 0x3A, 0x1E, 0x35, 0x28 };
        foreach (int map in overworldMaps)
        {
            int ptrAddr = enemyPtr + map * 2;
            int low = romData.GetByte(ptrAddr);
            int high = romData.GetByte(ptrAddr + 1);
            high <<= 8;
            high &= 0x0FFF;
            int addr = high + low + enemyAddr;
            addr += romData.GetByte(addr);
            if (shuffledEncounters.Contains(addr))
            {
                logger.Debug("Duplicate encounter");
            }
            else
            {
                shuffledEncounters.Add(addr);
                ShuffleEnemies(romData, addr, generatorsAlwaysMatch, mixLargeAndSmallEnemies);
            }
        }
    }

    protected bool PlaceLocations(Terrain riverTerrain, bool saneCaves)
    {
        return PlaceLocations(riverTerrain, saneCaves, null, -1);
    }
    protected bool PlaceLocations(Terrain riverTerrain, bool saneCaves, Location hiddenKasutoLocation, int hiddenPalaceX)
    {
        //return true;
        int i = 0;
        foreach (Location location in AllLocations)
        {
            i++;
            if ((location.TerrainType != Terrain.BRIDGE
                    && location.CanShuffle
                    && !unimportantLocs.Contains(location)
                    && location.PassThrough == 0)
                || location.NeedHammer)
            {
                int x, y;
                //Place the location in a spot that is not adjacent to any other location
                do
                {
                    x = RNG.Next(MAP_COLS - 2) + 1;
                    y = RNG.Next(MAP_ROWS - 2) + 1;
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
                    List<Direction> caveDirections = new() { Direction.NORTH, Direction.SOUTH, Direction.EAST, Direction.WEST };
                    Direction direction = caveDirections[RNG.Next(4)];
                    Terrain entranceTerrain = climate.GetRandomTerrain(RNG, walkableTerrains);

                    if (saneCaves && connections.ContainsKey(location))
                    {
                        PlaceCaveCount++;
                        map[y, x] = Terrain.NONE;
                        while (!PlaceSaneCave(direction, riverTerrain, location))
                        {
                            caveDirections.Remove(direction);
                            if (caveDirections.Count == 0)
                            {
                                //Debug.WriteLine(GetMapDebug());
                                return false;
                            }
                            direction = caveDirections[RNG.Next(caveDirections.Count)];
                        }
                    }
                    else
                    {
                        PlaceCaveCount++;
                        PlaceCave(x, y, direction, entranceTerrain);
                        location.Xpos = x;
                        location.Ypos = y + 30;
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
                    location.Ypos = y + 30;
                    location.CanShuffle = false;
                }
                else if (location.TerrainType != Terrain.TOWN || location.ActualTown.AppearsOnMap())
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
                    location.Ypos = y + 30;
                    location.CanShuffle = false;
                }
            }
        }
        return true;
    }

    /// <summary>
    /// Returns true iff 
    /// </summary>
    /// <param name="x1"></param>
    /// <param name="x2"></param>
    /// <param name="y1"></param>
    /// <param name="y2"></param>
    /// <param name="riverT"></param>
    /// <returns></returns>
    protected bool CrossingWater(int x1, int x2, int y1, int y2, Terrain riverTerrain)
    {
        int smallx = Math.Min(x1, x2);
        int largex = Math.Max(x1, x2);

        int smally = Math.Min(y1, y2);
        int largey = Math.Max(y1, y2);
        for (int y = smally; y < largey; y++)
        {
            for (int x = smallx; x < largex; x++)
            {
                if (y > 0 && y < MAP_ROWS && x > 0 && x < MAP_COLS && map[y, x] == riverTerrain)
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
    /// <param name="x">X coordinate of the cave entrance</param>
    /// <param name="y">Y coordinate of the cave entrance</param>
    /// <param name="direction">Direction the cave's entrance is facing. 
    ///     i.e. the direction you enter from, not the direction you press to enter it.</param>
    /// <param name="entranceTerrain">Terrain type to create for the approach.</param>
    protected void PlaceCave(int x, int y, Direction direction, Terrain entranceTerrain)
    {
        map[y, x] = Terrain.CAVE;
        if (direction == Direction.NORTH)
        {
            map[y + 1, x] = entranceTerrain;
            map[y + 1, x + 1] = entranceTerrain;
            map[y + 1, x - 1] = entranceTerrain;
            map[y, x - 1] = Terrain.MOUNTAIN;
            map[y, x + 1] = Terrain.MOUNTAIN;
            map[y - 1, x - 1] = Terrain.MOUNTAIN;
            map[y - 1, x] = Terrain.MOUNTAIN;
            map[y - 1, x + 1] = Terrain.MOUNTAIN;
        }
        else if (direction == Direction.EAST)
        {
            map[y + 1, x] = Terrain.MOUNTAIN;
            map[y + 1, x + 1] = Terrain.MOUNTAIN;
            map[y + 1, x - 1] = entranceTerrain;
            map[y, x - 1] = entranceTerrain;
            map[y, x + 1] = Terrain.MOUNTAIN;
            map[y - 1, x - 1] = entranceTerrain;
            map[y - 1, x] = Terrain.MOUNTAIN;
            map[y - 1, x + 1] = Terrain.MOUNTAIN;
        }
        else if (direction == Direction.SOUTH)
        {
            map[y + 1, x] = Terrain.MOUNTAIN;
            map[y + 1, x + 1] = Terrain.MOUNTAIN;
            map[y + 1, x - 1] = Terrain.MOUNTAIN;
            map[y, x - 1] = Terrain.MOUNTAIN;
            map[y, x + 1] = Terrain.MOUNTAIN;
            map[y - 1, x - 1] = entranceTerrain;
            map[y - 1, x] = entranceTerrain;
            map[y - 1, x + 1] = entranceTerrain;
        }
        else if (direction == Direction.WEST)
        {
            map[y + 1, x] = Terrain.MOUNTAIN;
            map[y + 1, x + 1] = entranceTerrain;
            map[y + 1, x - 1] = Terrain.MOUNTAIN;
            map[y, x - 1] = Terrain.MOUNTAIN;
            map[y, x + 1] = entranceTerrain;
            map[y - 1, x - 1] = Terrain.MOUNTAIN;
            map[y - 1, x] = Terrain.MOUNTAIN;
            map[y - 1, x + 1] = entranceTerrain;
        }
    }

    protected bool PlaceSaneCave(Direction direction, Terrain riverTerrain, Location location)
    {
        int x, y;
        if ((location.MapPage == 0 || location.FallInHole != 0) && location.ForceEnterRight == 0)
        {
            if (direction == Direction.NORTH)
            {
                direction = Direction.SOUTH;
            }

            if (direction == Direction.WEST)
            {
                direction = Direction.EAST;
            }
        }
        else
        {
            if (direction == Direction.SOUTH)
            {
                direction = Direction.NORTH;
            }

            if (direction == Direction.EAST)
            {
                direction = Direction.WEST;
            }
        }

        //Place the exit cave less than 5 rows or columns from the edge of the map, on an unoccupied square 
        //That is also not adjacent to any other location.
        do
        {
            x = RNG.Next(MAP_COLS - 2) + 1;
            y = RNG.Next(MAP_ROWS - 2) + 1;
        } while (x < 5 || y < 5 || x > MAP_COLS - 5 || y > MAP_ROWS - 5 || map[y, x] != Terrain.NONE || map[y - 1, x] != Terrain.NONE || map[y + 1, x] != Terrain.NONE || map[y + 1, x + 1] != Terrain.NONE || map[y, x + 1] != Terrain.NONE || map[y - 1, x + 1] != Terrain.NONE || map[y + 1, x - 1] != Terrain.NONE || map[y, x - 1] != Terrain.NONE || map[y - 1, x - 1] != Terrain.NONE);

        while ((direction == Direction.NORTH && y < 15) || (direction == Direction.EAST && x > MAP_COLS - 15) || (direction == Direction.SOUTH && y > MAP_ROWS - 15) || (direction == Direction.WEST && x < 15))
        {
            direction = (Direction)RNG.Next(4);
        }
        int otherx, othery;
        int tries = 0;
        bool crossing;
        do
        {
            int range = 12;
            int offset = 6;
            if (biome == Biome.ISLANDS || biome == Biome.MOUNTAINOUS)
            {
                range = 10;
                offset = 10;
            }
            else if (biome == Biome.VOLCANO || biome == Biome.CALDERA)
            {
                range = 10;
                offset = 15;
            }
            crossing = true;
            if (direction == Direction.NORTH)
            {
                otherx = x + (RNG.Next(7) - 3);
                othery = y - (RNG.Next(range) + offset);
            }
            else if (direction == Direction.EAST)
            {
                otherx = x + (RNG.Next(range) + offset);
                othery = y + (RNG.Next(7) - 3);
            }
            else if (direction == Direction.SOUTH)
            {
                otherx = x + (RNG.Next(7) - 3);
                othery = y + (RNG.Next(range) + offset);
            }
            else //west
            {
                otherx = x - (RNG.Next(range) + offset);
                othery = y + (RNG.Next(7) - 3);
            }
            if (biome != Biome.VOLCANO)
            {
                if (!CrossingWater(x, otherx, y, othery, riverTerrain))
                {
                    crossing = false;
                }
            }
            if (tries++ >= 100)
            {
                //Debug.WriteLine(GetMapDebug());
                return false;
            }
        } while (
            !crossing
            || otherx <= 1
            || otherx >= MAP_COLS - 1
            || othery <= 1
            || othery >= MAP_ROWS - 1
            || map[othery, otherx] != Terrain.NONE
            || map[othery - 1, otherx] != Terrain.NONE
            || map[othery + 1, otherx] != Terrain.NONE
            || map[othery + 1, otherx + 1] != Terrain.NONE
            || map[othery, otherx + 1] != Terrain.NONE
            || map[othery - 1, otherx + 1] != Terrain.NONE
            || map[othery + 1, otherx - 1] != Terrain.NONE
            || map[othery, otherx - 1] != Terrain.NONE
            || map[othery - 1, otherx - 1] != Terrain.NONE);

        Location location2 = connections[location];
        location.CanShuffle = false;
        location.Xpos = x;
        location.Ypos = y + 30;
        location2.CanShuffle = false;
        location2.Xpos = otherx;
        location2.Ypos = othery + 30;
        PlaceCave(x, y, direction, climate.GetRandomTerrain(RNG, walkableTerrains));
        PlaceCave(otherx, othery, direction.Reverse(), climate.GetRandomTerrain(RNG, walkableTerrains));
        return true;
    }

    public void PlaceHiddenLocations()
    {
        foreach (Location location in unimportantLocs)
        {
            if (location.CanShuffle)
            {
                int tries = 0;
                int x, y;
                do
                {
                    x = RNG.Next(MAP_COLS);
                    y = RNG.Next(MAP_ROWS);
                    tries++;
                } while ((map[y, x] != location.TerrainType || GetLocationByCoords((y + 30, x)) != null) && tries < 2000);

                if (tries < 2000)
                {
                    location.Xpos = x;
                    location.Ypos = y + 30;
                }
                else
                {
                    location.Xpos = 0;
                    location.Ypos = 0;
                }
                location.CanShuffle = false;
            }
        }
    }

    /// <summary>
    /// Creates "bridges" connecting contiguous landmasses with terrain from either connected landmass, or various kinds of special connections.
    /// </summary>
    /// <param name="maxBridges">Maximum number of bridges to create</param>
    /// <param name="placeTown">If true, one of the bridges is the saria river crossing</param>
    /// <param name="riverTerrain">Type of terrain the rivers are crossing over. Usually this is water, but on Mountanous, the "bridges" are over mountains</param>
    /// <param name="riverDevil">If true, one of the bridges will be a road blocked by the river devil</param>
    /// <param name="placeLongBridge">If true, one of the bridges is the bridge from vanilla west connecting DM to the SE desert with encounters at both ends</param>
    /// <param name="placeDarunia">If true, one of the bridges is a desert road with the two encounters that lead to darunia in vanilla</param>
    /// <returns>False if greater than 2000 total attempts were made in placement of all of the bridges. Else true.</returns>
    protected bool ConnectIslands(int maxBridges, bool placeTown, Terrain riverTerrain, bool riverDevil, bool placeLongBridge, bool placeDarunia, 
        bool canWalkOnWater, Biome biome, int? deadZoneMinX = null, int? deadZoneMaxX = null, int? deadZoneMinY = null, int? deadZoneMaxY = null)
    {
        int maxBridgeLength = MAXIMUM_BRIDGE_LENGTH[biome];
        maxBridgeLength = canWalkOnWater ? maxBridgeLength : (int)(maxBridgeLength * 1.5);
        if (!((deadZoneMinX == null && deadZoneMaxX == null && deadZoneMinY == null && deadZoneMaxY == null)
            || (deadZoneMinX != null && deadZoneMaxX != null && deadZoneMinY != null && deadZoneMaxY != null)))
        {
            throw new ArgumentException("ConnectIslands dead zone is improperly defined. 0 or 4 values must be specified.");
        }
        int maxBridgeAttempts = MAXIMUM_BRIDGE_ATTEMPTS[biome];
        maxBridgeAttempts *= canWalkOnWater ? 1 : 2;
        int[,] globs = GetTerrainGlobs();
        Dictionary<int, List<int>> massConnections = new Dictionary<int, List<int>>();
        int remainingBridges = maxBridges;
        int TerrainCycle = 0;
        int tries = 0;
        while (remainingBridges > 0 && tries < maxBridgeAttempts)
        {
            tries++;
            int x, y;
            /*
            if(deadZoneMaxX != null && x >= deadZoneMinX && x <= deadZoneMaxX && y >= deadZoneMinY && y <= deadZoneMaxY)
            {
                continue;
            }
            */
            //Find a tile adjacent to water that isn't within a dead zone to start the bridge
            Direction waterDirection;
            int waterTries = 0;
            do
            {
                x = RNG.Next(MAP_COLS - 2) + 1;
                y = RNG.Next(MAP_ROWS - 2) + 1;
                waterDirection = NextToWater(x, y, riverTerrain);
                waterTries++;
                if (waterTries >= 2000)
                {
                    return false;
                }
            } while (waterDirection == Direction.NONE
                || (deadZoneMaxX != null && x >= deadZoneMinX && x <= deadZoneMaxX && y >= deadZoneMinY && y <= deadZoneMaxY));

            int deltaX = waterDirection.DeltaX();
            int deltaY = waterDirection.DeltaY();
            int length = 0;
            if (IsSingleTile(y, x))
            {
                length = 100;
            }

            int startMass = globs[y, x];

            //if there is a location at or 1 tile adjacent to the bridge start, it's no good.
            if (GetLocationByCoords((y + 30, x)) != null
                || GetLocationByCoords((y + 30, x + 1)) != null
                || GetLocationByCoords((y + 30, x - 1)) != null
                || GetLocationByCoords((y + 31, x)) != null
                || GetLocationByCoords((y + 29, x)) != null)
            {
                length = 100;
            }

            x += deltaX;
            y += deltaY;

            //iterate expanding the bridge
            while (x > 0 && x < MAP_COLS && y > 0 && y < MAP_ROWS && map[y, x] == riverTerrain)
            {
                //if we hit the edge of the map or too close to a location, give up
                if (x + 1 < MAP_COLS && GetLocationByCoords((y + 30, x + 1)) != null)
                {
                    length = 100;
                }
                if (x - 1 > 0 && GetLocationByCoords((y + 30, x - 1)) != null)
                {
                    length = 100;
                }

                if (y + 1 < MAP_ROWS && GetLocationByCoords((y + 31, x)) != null)
                {
                    length = 100;
                }
                if (y - 1 > 0 && GetLocationByCoords((y + 29, x)) != null)
                {
                    length = 100;
                }

                //count how many tiles adjacent to the bridge are the passing terrain
                int adjacentRiverTerrainDirectionCount = 0;
                if (x + 1 < MAP_COLS && (map[y, x + 1] == riverTerrain || map[y, x + 1] == Terrain.MOUNTAIN))
                {
                    adjacentRiverTerrainDirectionCount++;
                }
                if (x - 1 >= 0 && (map[y, x - 1] == riverTerrain || map[y, x - 1] == Terrain.MOUNTAIN))
                {
                    adjacentRiverTerrainDirectionCount++;
                }
                if (y + 1 < MAP_ROWS && (map[y + 1, x] == riverTerrain || map[y + 1, x] == Terrain.MOUNTAIN))
                {
                    adjacentRiverTerrainDirectionCount++;
                }
                if (y - 1 >= 0 && (map[y - 1, x] == riverTerrain || map[y - 1, x] == Terrain.MOUNTAIN))
                {
                    adjacentRiverTerrainDirectionCount++;
                }

                if (deltaX != 0)
                {
                    if (y - 1 > 0)
                    {
                        if (map[y - 1, x] != riverTerrain && map[y - 1, x] != Terrain.MOUNTAIN)
                        {
                            length = 100;
                        }
                    }
                    if (y + 1 < MAP_ROWS)
                    {
                        if (map[y + 1, x] != riverTerrain && map[y + 1, x] != Terrain.MOUNTAIN)
                        {
                            length = 100;
                        }
                    }
                }

                if (deltaY != 0)
                {
                    if (x - 1 > 0)
                    {
                        if (map[y, x - 1] != riverTerrain && map[y, x - 1] != Terrain.MOUNTAIN)
                        {
                            length = 100;
                        }
                    }
                    if (x + 1 < MAP_ROWS)
                    {
                        if (map[y, x + 1] != riverTerrain && map[y, x + 1] != Terrain.MOUNTAIN)
                        {
                            length = 100;
                        }
                    }
                }

                //if all 3 tiles adjacent to the new tile aren't the passing terrain, give up
                if (adjacentRiverTerrainDirectionCount <= 2)
                {
                    length = 100;
                }
                x += deltaX;
                y += deltaY;
                length++;

            }

            //no extending from single tile
            if (IsSingleTile(y, x))
            {
                length = 100;
            }

            //if we're ending, it has to be on a different chunk of terrain so the bridge doesn't just cross a bay
            int endMass = 0;
            if (y > 0 && x > 0 && y < MAP_ROWS - 1 && x < MAP_COLS - 1)
            {
                if (GetLocationByCoords((y + 30, x)) != null
                    || GetLocationByCoords((y + 30, x + 1)) != null
                    || GetLocationByCoords((y + 30, x - 1)) != null
                    || GetLocationByCoords((y + 31, x)) != null
                    || GetLocationByCoords((y + 29, x)) != null)
                {
                    length = 100;
                    //Debug.WriteLine(GetGlobDebug(globs));
                }
                endMass = globs[y, x];
            }

            if ((riverTerrain != Terrain.DESERT && biome != Biome.CALDERA && biome != Biome.VOLCANO)
                && (startMass == 0
                    || endMass == 0
                    || endMass == startMass
                    || (massConnections.ContainsKey(startMass) && massConnections[startMass].Contains(endMass))
                )
            )
            {
                length = 100;
            }
            if (
                (placeTown && length < maxBridgeLength || (length < maxBridgeLength && length > MINIMUM_BRIDGE_LENGTH))
                && x > 0
                && x < MAP_COLS - 1
                && y > 0
                && y < MAP_ROWS - 1
                && walkableTerrains.Contains(map[y, x])
                && map[y, x] != riverTerrain)
            {
                if (!massConnections.ContainsKey(startMass))
                {
                    List<int> c = new List<int>();
                    c.Add(endMass);
                    massConnections[startMass] = c;
                }
                else
                {
                    massConnections[startMass].Add(endMass);
                }

                if (!massConnections.ContainsKey(endMass))
                {
                    List<int> c = new List<int>();
                    c.Add(startMass);
                    massConnections[endMass] = c;
                }
                else
                {
                    massConnections[endMass].Add(startMass);
                }

                Terrain terrain = map[y, x];


                if (placeTown)
                {

                    map[y, x] = Terrain.TOWN;
                    Location location = GetLocationByMem(0x465F);
                    location.Ypos = y + 30;
                    location.Xpos = x;
                    x -= deltaX;
                    y -= deltaY;
                    while (map[y, x] == riverTerrain)
                    {
                        x -= deltaX;
                        y -= deltaY;
                        //if(map[y + deltaY, x + deltaX] != Terrain.town && riverT != Terrain.walkablewater)
                        //{
                        //    map[y + deltaY, x + deltaX] = Terrain.walkablewater;
                        //}
                    }
                    map[y, x] = Terrain.TOWN;
                    location = GetLocationByMem(0x4660);
                    location.Ypos = y + 30;
                    location.Xpos = x;
                    placeTown = false;
                }
                else if (placeLongBridge)
                {
                    Location bridge1 = GetLocationByMem(0x4644);
                    Location bridge2 = GetLocationByMem(0x4645);
                    x -= deltaX;
                    y -= deltaY;
                    if (deltaX > 0 || deltaY > 0)
                    {
                        bridge2.Xpos = x;
                        bridge2.Ypos = y + 30;
                    }
                    else
                    {
                        bridge1.Xpos = x;
                        bridge1.Ypos = y + 30;
                    }

                    while (map[y, x] == riverTerrain)
                    {
                        map[y, x] = Terrain.BRIDGE;
                        x -= deltaX;
                        y -= deltaY;
                        //if(map[y + deltaY, x + deltaX] != Terrain.town && riverT != Terrain.walkablewater)
                        //{
                        //    map[y + deltaY, x + deltaX] = Terrain.walkablewater;
                        //}

                    }
                    x += deltaX;
                    y += deltaY;
                    map[y, x] = Terrain.BRIDGE;
                    if (deltaX > 0 || deltaY > 0)
                    {
                        bridge1.Xpos = x;
                        bridge1.Ypos = y + 30;
                    }
                    else
                    {
                        bridge2.Xpos = x;
                        bridge2.Ypos = y + 30;
                    }
                    placeLongBridge = false;
                    bridge1.CanShuffle = false;
                    bridge2.CanShuffle = false;
                }
                else if (placeDarunia)
                {
                    Location bridge1 = GetLocationByMem(0x8638);
                    Location bridge2 = GetLocationByMem(0x8637);
                    if (bridge1.CanShuffle && bridge2.CanShuffle)
                    {
                        x -= deltaX;
                        y -= deltaY;
                        if (deltaX > 0 || deltaY > 0)
                        {
                            bridge2.Xpos = x;
                            bridge2.Ypos = y + 30;
                        }
                        else
                        {
                            bridge1.Xpos = x;
                            bridge1.Ypos = y + 30;
                        }

                        while (map[y, x] == riverTerrain)
                        {
                            map[y, x] = Terrain.DESERT;
                            x -= deltaX;
                            y -= deltaY;
                            //if(map[y + deltaY, x + deltaX] != Terrain.town && riverT != Terrain.walkablewater)
                            //{
                            //    map[y + deltaY, x + deltaX] = Terrain.walkablewater;
                            //}

                        }
                        x += deltaX;
                        y += deltaY;
                        map[y, x] = Terrain.DESERT;
                        if (deltaX > 0 || deltaY > 0)
                        {
                            bridge1.Xpos = x;
                            bridge1.Ypos = y + 30;
                        }
                        else
                        {
                            bridge2.Xpos = x;
                            bridge2.Ypos = y + 30;
                        }
                        bridge1.CanShuffle = false;
                        bridge2.CanShuffle = false;
                    }
                    placeDarunia = false;

                }
                else
                {
                    x -= deltaX;
                    y -= deltaY;
                    int curr = 0;
                    if (TerrainCycle == 2)
                    {

                        x += deltaX;
                        y += deltaY;
                        map[y, x] = Terrain.ROAD;
                        x -= deltaX;
                        y -= deltaY;
                        while (map[y, x] == riverTerrain)
                        {
                            map[y, x] = Terrain.WALKABLEWATER;
                            x -= deltaX;
                            y -= deltaY;

                            //if(map[y + deltaY, x + deltaX] != Terrain.town && riverT != Terrain.walkablewater)
                            //{
                            //    map[y + deltaY, x + deltaX] = Terrain.walkablewater;
                            //}
                        }
                        map[y, x] = Terrain.ROAD;

                    }
                    else
                    {
                        while (map[y, x] == riverTerrain)
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

                                map[y, x] = terrain;
                                bool placed = false;
                                if (curr == length / 2)
                                {
                                    List<Location> locs = Locations[terrain];
                                    if (riverDevil)
                                    {
                                        map[y, x] = Terrain.RIVER_DEVIL;
                                        riverDevil = false;
                                        placed = true;
                                    }
                                    foreach (Location location in locs)
                                    {
                                        if (location.CanShuffle && !placed)
                                        {
                                            location.Ypos = y + 30;
                                            location.Xpos = x;
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
                                    map[y, x] = Terrain.ROAD;

                                    if (curr == length / 2)
                                    {
                                        bool placed = false;
                                        if (riverDevil)
                                        {
                                            map[y, x] = Terrain.RIVER_DEVIL;
                                            riverDevil = false;
                                            placed = true;
                                        }
                                        foreach (Location location in Locations[Terrain.ROAD])
                                        {
                                            if (location.CanShuffle && !placed)
                                            {
                                                location.Ypos = y + 30;
                                                location.Xpos = x;
                                                location.CanShuffle = false;
                                                break;
                                            }
                                        }

                                    }
                                }
                                else if (TerrainCycle == 1)
                                {
                                    if (riverTerrain == Terrain.WATER || riverTerrain == Terrain.WALKABLEWATER || riverTerrain == Terrain.MOUNTAIN || (riverTerrain != Terrain.WALKABLEWATER && curr != length / 2 + 1))
                                    {
                                        map[y, x] = Terrain.BRIDGE;
                                    }
                                    bool placed = false;
                                    if (curr == length / 2)
                                    {
                                        if (riverDevil)
                                        {
                                            map[y, x] = Terrain.RIVER_DEVIL;
                                            riverDevil = false;
                                            placed = true;
                                        }
                                        foreach (Location location in Locations[Terrain.BRIDGE])
                                        {
                                            if (location.CanShuffle && !placed)
                                            {
                                                location.Ypos = y + 30;
                                                location.Xpos = x;
                                                location.CanShuffle = false;
                                                break;
                                            }
                                        }
                                    }
                                }
                            }
                            curr++;
                            x -= deltaX;
                            y -= deltaY;
                        }
                    }
                    TerrainCycle++;
                    if (riverTerrain != Terrain.MOUNTAIN && riverTerrain != Terrain.DESERT && !canWalkOnWater)
                    {
                        TerrainCycle = TerrainCycle % 3;
                    }
                    else
                    {
                        TerrainCycle = TerrainCycle % 2;
                    }

                }

                remainingBridges--;

            }
        }
        return !placeTown;
    }

    /// <summary>
    /// Groups the map into connected sections of walkable terrain.
    /// </summary>
    /// <returns>An int[][] mapping each x-y on the map to an int corresponding to the group that tile is a member of</returns>
    private int[,] GetTerrainGlobs()
    {
        int[,] mass = new int[MAP_ROWS, MAP_COLS];
        for (int i = 0; i < MAP_ROWS; i++)
        {
            for (int j = 0; j < MAP_COLS; j++)
            {
                mass[i, j] = 0;
            }
        }
        int massNo = 1;
        for (int i = 0; i < MAP_ROWS; i++)
        {
            for (int j = 0; j < MAP_COLS; j++)
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
        if (y + 1 < MAP_ROWS && mass[y + 1, x] == 0 && walkableTerrains.Contains(map[y + 1, x]))
        {
            CategorizeTerrainGlob(y + 1, x, massNo, mass);
        }
        if (x - 1 > 0 && mass[y, x - 1] == 0 && walkableTerrains.Contains(map[y, x - 1]))
        {
            CategorizeTerrainGlob(y, x - 1, massNo, mass);
        }
        if (x + 1 < MAP_COLS && mass[y, x + 1] == 0 && walkableTerrains.Contains(map[y, x + 1]))
        {
            CategorizeTerrainGlob(y, x + 1, massNo, mass);
        }
    }

    [System.Diagnostics.CodeAnalysis.SuppressMessage("CodeQuality", "IDE0051:Remove unused private members", Justification = "Debug Method")]
    private string PrintTerrainGlobMap(int[,] mass)
    {
        char[] encoding = { '0', '1', '2', '3', '4', '5', '6', '7', '8', '9', 'A', 'B', 'C', 'D', 'E', 'F', 'G', 'H', 'I', 'J', 'K', 'L', 'M', 'N', 'O', 'P', 'Q' };
        StringBuilder sb = new StringBuilder();
        for (int i = 0; i < MAP_ROWS; i++)
        {
            for (int j = 0; j < MAP_COLS; j++)
            {
                sb.Append(encoding[mass[i, j]]);
            }
            sb.Append('\n');
        }
        return sb.ToString();
    }

    protected Direction NextToWater(int x, int y, Terrain riverTerrain)
    {
        if (walkableTerrains.Contains(map[y, x]) && map[y, x] != riverTerrain)
        {
            if (map[y + 1, x] == riverTerrain)
            {
                return Direction.SOUTH;
            }

            if (map[y - 1, x] == riverTerrain)
            {
                return Direction.NORTH;
            }

            if (map[y, x + 1] == riverTerrain)
            {
                return Direction.EAST;
            }

            if (map[y, x - 1] == riverTerrain)
            {
                return Direction.WEST;
            }
        }
        return Direction.NONE;
    }

    /// <summary>
    /// Returns true IFF this is an isolated single tile. i.e. The terrain type one space in each orthoginal direction is not this terrain type.
    /// </summary>
    /// <param name="y"></param>
    /// <param name="x"></param>
    /// <returns></returns>
    private bool IsSingleTile(int y, int x)
    {
        int count = 0;
        if (x < MAP_COLS && x > 0)
        {
            if (y + 1 < MAP_ROWS && !walkableTerrains.Contains(map[y + 1, x]))
            {
                count++;
            }
            if (y - 1 > 0 && !walkableTerrains.Contains(map[y - 1, x]))
            {
                count++;
            }
        }
        if (y < MAP_ROWS && y > 0)
        {
            if (x + 1 < MAP_COLS && !walkableTerrains.Contains(map[y, x + 1]))
            {
                count++;
            }
            if (x - 1 > 0 && !walkableTerrains.Contains(map[y, x - 1]))
            {
                count++;
            }
        }
        return count == 4;

    }
    protected bool GrowTerrain(Climate climate)
    {
        List<Terrain> randomTerrains = climate.RandomTerrains(randomTerrainFilter).ToList();
        TerrainGrowthAttempts++;
        Terrain[,] mapCopy = new Terrain[MAP_ROWS, MAP_COLS];
        List<(int, int)> placed = new();
        for (int y = 0; y < MAP_ROWS; y++)
        {
            for (int x = 0; x < MAP_COLS; x++)
            {
                if (map[y, x] != Terrain.NONE && randomTerrains.Contains(map[y, x]))
                {
                    placed.Add((y, x));
                }
            }
        }

        int dy, dx;
        double distance;
        List<Terrain> choices = new List<Terrain>();
        for (int y = 0; y < MAP_ROWS; y++)
        {
            for (int x = 0; x < MAP_COLS; x++)
            {
                if (map[y, x] == Terrain.NONE)
                {
                    choices.Clear();
                    double mindistance = double.MaxValue;

                    foreach ((int, int)t in placed)
                    {
                        dy = t.Item1 - y;
                        dx = t.Item2 - x;
                        distance = (climate.DistanceCoefficients[(int)map[t.Item1, t.Item2]]) * Math.Sqrt(dy * dy + dx * dx);
                        //distance = ((tx + (tx >> 31)) ^ (tx >> 31)) + ((ty + (ty >> 31)) ^ (ty >> 31));
                        //distance = Math.Abs(tx) + Math.Abs(ty);
                        if (distance < mindistance)
                        {
                            choices.Clear();
                            choices.Add(map[t.Item1, t.Item2]);
                            mindistance = distance;
                        }
                        else if (distance == mindistance)
                        {
                            choices.Add(map[t.Item1, t.Item2]);
                        }
                    }
                    mapCopy[y, x] = choices[RNG.Next(choices.Count)];
                    //mapCopy[y, x] = Terrain.ROAD;
                }
            }
        }

        //Write the locations that already had terrain before the growth back onto the copy before we clone over.
        for (int y = 0; y < MAP_ROWS; y++)
        {
            for (int x = 0; x < MAP_COLS; x++)
            {
                if (map[y, x] != Terrain.NONE)
                {
                    mapCopy[y, x] = map[y, x];
                }
            }
        }
        map = (Terrain[,])mapCopy.Clone();
        return true;
    }

    /*
    //Keeping this around for vanilla DM support, but probably this gets removed in the future
    protected void PlaceRandomTerrain(int numberOfTerrainsToPlace, Climate climate)
    {
        //randomly place remaining Terrain
        int placed = 0;
        while (placed < numberOfTerrainsToPlace)
        {
            int x = 0;
            int y = 0;
            Terrain t = climate.GetRandomTerrain(RNG, randomTerrainFilter);
            do
            {
                x = RNG.Next(MAP_COLS);
                y = RNG.Next(MAP_ROWS);
            } while (map[y, x] != Terrain.NONE);
            map[y, x] = t;
            placed++;
        }
    }
    */

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
                x = RNG.Next(MAP_COLS);
                y = RNG.Next(MAP_ROWS);
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
        for (int y = 0; y < MAP_ROWS; y++)
        {
            for (int x = 0; x < MAP_COLS; x++)
            {
                //These two conditionals ABSOLUTELY should not be processed here.
                //Refactor them and remove the excess boolean parameters.
                if (hiddenPalace && y == h1 && x == h2 && y != 0 && x != 0)
                {
                    currentTerrainCount--;
                    int b = currentTerrainCount * 16 + (int)currentTerrain;
                    //logger.WriteLine("Hex: {0:X}", b);
                    if (doWrite)
                    {
                        romData.Put(loc, (byte)b);
                        romData.Put(loc + 1, (byte)currentTerrain);
                    }
                    currentTerrainCount = 0;
                    loc += 2;
                    bytesWritten += 2;
                    continue;
                }
                if (hiddenKasuto && y == 51 && x == 61 && y != 0 && x != 0 && (biome == Biome.VANILLA || biome == Biome.VANILLA_SHUFFLE))
                {
                    currentTerrainCount--;
                    int b = currentTerrainCount * 16 + (int)currentTerrain;
                    //logger.WriteLine("Hex: {0:X}", b);
                    if (doWrite)
                    {
                        romData.Put(loc, (byte)b);
                        romData.Put(loc + 1, (byte)currentTerrain);
                    }
                    currentTerrainCount = 0;
                    loc += 2;
                    bytesWritten += 2;
                    continue;
                }

                if (map[y, x] == currentTerrain && currentTerrainCount < 16)
                {
                    currentTerrainCount++;
                }
                else
                {
                    currentTerrainCount--;
                    //First 4 bits are the number of tiles to draw with that Terrain type. Last 4 are the Terrain type.
                    int b = currentTerrainCount * 16 + (int)currentTerrain;
                    //logger.WriteLine("Hex: {0:X}", b);
                    if (doWrite)
                    {
                        romData.Put(loc, (byte)b);
                    }

                    currentTerrain = map[y, x];
                    //This is almost certainly an off by one error, but very very unlikely to matter
                    currentTerrainCount = 1;
                    loc++;
                    bytesWritten++;
                }
            }
            //Write the last Terrain segment for this row
            currentTerrainCount--;
            int b2 = currentTerrainCount * 16 + (int)currentTerrain;
            //logger.WriteLine("Hex: {0:X}", b2);
            if (doWrite)
            {
                romData.Put(loc, (byte)b2);
            }

            if (y < MAP_ROWS - 1)
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

    protected void DrawOcean(Direction direction, bool walkableWater)
    {
        Terrain water = Terrain.WATER;
        if (walkableWater)
        {
            water = Terrain.WALKABLEWATER;
        }
        int x;
        int y;
        int olength = 0;

        if (direction == Direction.WEST)
        {
            x = 0;
            y = RNG.Next(MAP_ROWS);
            olength = RNG.Next(Math.Max(y, MAP_ROWS - y));
        }
        else if (direction == Direction.EAST)
        {
            x = MAP_COLS - 1;
            y = RNG.Next(MAP_ROWS);
            olength = RNG.Next(Math.Max(y, MAP_ROWS - y));
        }
        else if (direction == Direction.NORTH)
        {
            x = RNG.Next(MAP_COLS);
            y = 0;
            olength = RNG.Next(Math.Max(x, MAP_COLS - x));
        }
        else //south
        {
            x = RNG.Next(MAP_COLS);
            y = MAP_ROWS - 1;
            olength = RNG.Next(Math.Max(x, MAP_COLS - x));
        }
        //draw ocean on right side

        if (direction == Direction.EAST || direction == Direction.WEST)
        {
            if (y < MAP_ROWS / 2)
            {
                for (int i = 0; i < olength; i++)
                {
                    if (map[y + i, x] != Terrain.NONE && biome != Biome.MOUNTAINOUS)
                    {
                        return;
                    }
                    map[y + i, x] = water;
                }
            }
            else //north or south
            {
                try
                {
                    for (int i = 0; i < olength; i++)
                    {
                        if (map[y - i, x] != Terrain.NONE && biome != Biome.MOUNTAINOUS)
                        {
                            return;
                        }
                        map[y - i, x] = water;
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
            if (x < MAP_COLS / 2)
            {
                for (int i = 0; i < olength; i++)
                {
                    if (map[y, x + i] != Terrain.NONE && biome != Biome.MOUNTAINOUS)
                    {
                        return;
                    }
                    map[y, x + i] = water;
                }
            }
            else
            {
                for (int i = 0; i < olength; i++)
                {
                    if (map[y, x - 1] != Terrain.NONE && biome != Biome.MOUNTAINOUS)
                    {
                        return;
                    }
                    map[y, x - i] = water;
                }
            }
        }
    }
  
    protected void UpdateReachable(Dictionary<Item, bool> itemGet, Dictionary<Spell, bool> spellGet)
    {
        //Setup
        bool needJump = false;
        Location location = GetLocationByMem(0x8646);
        int jumpBlockY = -1;
        int jumpBlockX = -1;
        if (location != null)
        {
            needJump = location.NeedJump;
            jumpBlockY = location.Ypos - 30;
            jumpBlockX = location.Xpos;
        }

        bool needFairy = false;
        location = GetLocationByMem(0x8644);
        int fairyBlockY = -1;
        int fairyBlockX = -1;
        if (location != null)
        {
            needFairy = location.NeedFairy;
            fairyBlockY = location.Ypos - 30;
            fairyBlockX = location.Xpos;
        }

        List<Location> starts = GetPathingStarts();

        bool[,] covered = new bool[MAP_ROWS, MAP_COLS];
        for (int i = 0; i < MAP_ROWS; i++)
        {
            for (int j = 0; j < MAP_COLS; j++)
            {
                covered[i, j] = false;
            }
        }

        //Run the initial steps
        foreach (Location start in starts)
        {
            if (start.Ypos >= 30 && start.Xpos >= 0)
            {
                UpdateReachable(ref covered, start.Ypos - 30, start.Xpos, itemGet, spellGet, jumpBlockY, jumpBlockX, fairyBlockY, fairyBlockX, needJump, needFairy);
            }
        }

        OnUpdateReachableTrigger();
    }

    protected virtual void OnUpdateReachableTrigger()
    {
        //This space intentionally left blank
    }

    //This signature has gotten out of control, consider a refactor
    protected void UpdateReachable(ref bool[,] covered, int start_y, int start_x, Dictionary<Item, bool> itemGet, Dictionary<Spell, bool> spellGet, 
        int jumpBlockY, int jumpBlockX, int fairyBlockY, int fairyBlockX, bool needJump, bool needFairy)
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

                Terrain terrain = map[y, x];
                if ((terrain == Terrain.LAVA
                    || terrain == Terrain.BRIDGE
                    || terrain == Terrain.CAVE
                    || terrain == Terrain.ROAD
                    || terrain == Terrain.PALACE
                    || terrain == Terrain.TOWN
                    || walkableTerrains.Contains(terrain)
                    || (terrain == Terrain.WALKABLEWATER && itemGet[Item.BOOTS])
                    || (terrain == Terrain.ROCK && itemGet[Item.HAMMER])
                    || (terrain == Terrain.RIVER_DEVIL && itemGet[Item.FLUTE]))
                    //East desert jump blocker
                    && !(
                        needJump
                        && jumpBlockY == y
                        && jumpBlockX == x
                        && (!spellGet[Spell.JUMP] && !spellGet[Spell.FAIRY])
                    )
                    //Fairy cave is traversable
                    && !(needFairy && fairyBlockY == y && fairyBlockX == x && !spellGet[Spell.FAIRY])
                )
                {
                    visitation[y, x] = true;
                    if (y - 1 >= 0 && !covered[y - 1, x])
                    {
                        to_visit.Push((y - 1, x));
                    }
                    if (y + 1 < MAP_ROWS && !covered[y + 1, x])
                    {
                        to_visit.Push((y + 1, x));
                    }
                    if (x - 1 >= 0 && !covered[y, x - 1])
                    {
                        to_visit.Push((y, x - 1));
                    }
                    if (x + 1 < MAP_COLS && !covered[y, x + 1])
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
        for (int i = 0; i < MAP_ROWS; i++)
        {
            for (int j = 0; j < MAP_COLS; j++)
            {
                visitation[i, j] = false;
            }
        }
        foreach (Location location in AllLocations)
        {
            location.CanShuffle = true;
            location.Reachable = false;
            location.ItemGet = false;
        }
    }

    protected bool DrawRaft(bool bridge, Direction direction)
    {
        int raftx = 0;
        int deltax = 1;
        int deltay = 0;
        int rafty = RNG.Next(0, MAP_ROWS);

        int tries = 0;
        int length = 0;
        //Debug.WriteLine(GetMapDebug());

        do
        {
            length = 0;
            tries++;
            if (tries > 100)
            {
                return false;
            }
            if (direction == Direction.WEST)
            {
                raftx = 0;
                int rtries = 0;
                do
                {
                    rafty = RNG.Next(0, MAP_ROWS);
                    rtries++;
                    if (rtries > 100)
                    {
                        return false;
                    }
                } while (map[rafty, raftx] != Terrain.WALKABLEWATER && map[rafty, raftx] != Terrain.WATER);
                deltax = 1;
            }
            else if (direction == Direction.NORTH)
            {
                rafty = 0;
                int rtries = 0;

                do
                {
                    raftx = RNG.Next(0, MAP_COLS);
                    rtries++;
                    if (rtries > 100)
                    {
                        return false;
                    }
                } while (map[rafty, raftx] != Terrain.WALKABLEWATER && map[rafty, raftx] != Terrain.WATER);
                deltax = 0;
                deltay = 1;
            }
            else if (direction == Direction.SOUTH)
            {
                rafty = MAP_ROWS - 1;
                int rtries = 0;

                do
                {
                    raftx = RNG.Next(0, MAP_COLS);
                    rtries++;
                    if (rtries > 100)
                    {
                        return false;
                    }
                } while (map[rafty, raftx] != Terrain.WALKABLEWATER && map[rafty, raftx] != Terrain.WATER);
                deltax = 0;
                deltay = -1;
            }
            else //EAST
            {
                raftx = MAP_COLS - 1;
                int rtries = 0;

                do
                {
                    rafty = RNG.Next(0, MAP_ROWS);
                    rtries++;
                    if (rtries > 100)
                    {
                        return false;
                    }
                } while (map[rafty, raftx] != Terrain.WALKABLEWATER && map[rafty, raftx] != Terrain.WATER);

                deltax = -1;
                deltay = 0;
            }
            while (rafty >= 0 && rafty < MAP_ROWS && raftx >= 0 && raftx < MAP_COLS && (map[rafty, raftx] == Terrain.WALKABLEWATER || map[rafty, raftx] == Terrain.WATER))
            {
                rafty += deltay;
                raftx += deltax;
                length++;
            }
        } while (rafty < 0 || rafty >= MAP_ROWS || raftx < 0 || raftx >= MAP_COLS || !walkableTerrains.Contains(map[rafty, raftx]) || (bridge && length > 10) || (bridge && length <= 1));


        rafty -= deltay;
        raftx -= deltax;
        if (!bridge)
        {
            map[rafty, raftx] = Terrain.BRIDGE;
            raft.Xpos = raftx;
            raft.Ypos = rafty + 30;
            raft.CanShuffle = false;
        }
        else
        {
            map[rafty, raftx] = Terrain.BRIDGE;
            this.bridge.Xpos = raftx;
            this.bridge.Ypos = rafty + 30;
            this.bridge.PassThrough = 0;
            this.bridge.CanShuffle = false;
            if (direction == Direction.EAST)
            {
                for (int i = raftx + 1; i < MAP_COLS; i++)
                {
                    map[rafty, i] = Terrain.BRIDGE;
                }
            }
            else if (direction == Direction.WEST)
            {
                for (int i = raftx - 1; i >= 0; i--)
                {
                    map[rafty, i] = Terrain.BRIDGE;
                }
            }
            else if (direction == Direction.SOUTH)
            {
                for (int i = rafty + 1; i < MAP_ROWS; i++)
                {
                    map[i, raftx] = Terrain.BRIDGE;
                }
            }
            else if (direction == Direction.NORTH)
            {
                for (int i = rafty - 1; i >= 0; i--)
                {
                    map[i, raftx] = Terrain.BRIDGE;
                }
            }
        }
        return true;

    }

    public Location LoadRaft(ROM rom, Continent continent, Continent connectedContinent)
    {
        AddLocation(rom.LoadLocation(baseAddr + 41, Terrain.BRIDGE, continent));
        raft = GetLocationByMem(baseAddr + 41);
        raft.ConnectedContinent = connectedContinent;
        raft.ExternalWorld = 0x80;
        raft.Map = 41;
        raft.TerrainType = Terrain.BRIDGE;
        return raft;
    }

    public Location LoadBridge(ROM rom, Continent continent, Continent connectedContinent)
    {
        AddLocation(rom.LoadLocation(baseAddr + 40, Terrain.BRIDGE, continent));
        bridge = GetLocationByMem(baseAddr + 40);
        bridge.ConnectedContinent = connectedContinent;
        bridge.ExternalWorld = 0x80;
        bridge.Map = 40;
        bridge.PassThrough = 0;
        return bridge;
    }

    public Location LoadCave1(ROM rom, Continent continent, Continent connectedContinent)
    {
        AddLocation(rom.LoadLocation(baseAddr + 42, Terrain.CAVE, continent));
        cave1 = GetLocationByMem(baseAddr + 42);
        cave1.ConnectedContinent = connectedContinent;
        cave1.ExternalWorld = 0x80;
        cave1.Map = 42;
        cave1.CanShuffle = true;
        return cave1;
    }

    public Location LoadCave2(ROM rom, Continent continent, Continent connectedContinent)
    {
        AddLocation(rom.LoadLocation(baseAddr + 43, Terrain.CAVE, continent));
        cave2 = GetLocationByMem(baseAddr + 43);
        cave2.ConnectedContinent = connectedContinent;
        cave2.ExternalWorld = 0x80;
        cave2.Map = 43;
        cave2.TerrainType = Terrain.CAVE;
        cave2.CanShuffle = true;
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
            visitation[raft.Ypos - 30, raft.Xpos] = true;
        }
    }

    public void VisitBridge()
    {
        if (bridge != null)
        {
            visitation[bridge.Ypos - 30, bridge.Xpos] = true;
        }
    }

    public void VisitCave1()
    {
        if (cave1 != null)
        {
            visitation[cave1.Ypos - 30, cave1.Xpos] = true;
        }
    }

    public void VisitCave2()
    {
        if (cave2 != null)
        {
            visitation[cave2.Ypos - 30, cave2.Xpos] = true;
        }
    }

    public List<Location> GetContinentConnections()
    {
        return new List<Location>() { cave1, cave2, bridge, raft }.Where(i => i != null).ToList();
    }

    protected void DrawRiver(bool canWalkOnWaterWithBoots)
    {
        Terrain water = Terrain.WATER;
        if (canWalkOnWaterWithBoots)
        {
            water = Terrain.WALKABLEWATER;
        }
        int dirr = RNG.Next(4);
        int dirr2 = dirr;
        while (dirr == dirr2)
        {
            dirr2 = RNG.Next(4);
        }

        int deltax = 0;
        int deltay = 0;
        int startx = 0;
        int starty = 0;
        if (dirr == 0) //north
        {
            deltay = 1;
            startx = RNG.Next(MAP_COLS / 3, (MAP_COLS / 3) * 2);
            starty = 0;
        }
        else if (dirr == 1) //east
        {
            deltax = -1;
            startx = MAP_COLS - 1;
            starty = RNG.Next(MAP_ROWS / 3, (MAP_ROWS / 3) * 2);
        }
        else if (dirr == 2) //south
        {
            deltay = -1;
            startx = RNG.Next(MAP_COLS / 3, (MAP_COLS / 3) * 2);
            starty = MAP_ROWS - 1;
        }
        else //west
        {
            deltax = 1;
            startx = 0;
            starty = RNG.Next(MAP_ROWS / 3, (MAP_ROWS / 3) * 2);
        }

        int stopping = RNG.Next(MAP_COLS / 3, (MAP_COLS / 3) * 2);
        if (deltay != 0)
        {
            stopping = RNG.Next(MAP_ROWS / 3, (MAP_ROWS / 3) * 2);
        }
        int curr = 0;
        while (curr < stopping)
        {

            if (map[starty, startx] == Terrain.NONE)
            {
                map[starty, startx] = water;
                int adjust = RNG.Next(-1, 2);
                if ((deltax == 0 && startx == 1) || (deltay == 0 && starty == 1))
                {
                    adjust = RNG.Next(0, 2);
                }
                else if ((deltax == 0 && startx == MAP_COLS - 2) || (deltay == 0 && starty == MAP_ROWS - 2))
                {
                    adjust = RNG.Next(-1, 1);
                }

                if (adjust < 0)
                {
                    if (deltax != 0)
                    {
                        starty--;
                    }
                    else
                    {
                        startx--;
                    }
                }
                else if (adjust > 0)
                {
                    if (deltax != 0)
                    {
                        starty++;
                    }
                    else
                    {
                        startx++;
                    }
                }
                map[starty, startx] = water;
            }

            startx += deltax;
            starty += deltay;
            curr++;
        }
        deltay = 0;
        deltax = 0;
        if (dirr2 == 0) //north
        {
            deltay = 1;
        }
        else if (dirr2 == 1) //east
        {
            deltax = -1;
        }
        else if (dirr2 == 2) //south
        {
            deltay = -1;
        }
        else //west
        {
            deltax = 1;
        }
        while (startx > 0 && startx < MAP_COLS && starty > 0 && starty < MAP_ROWS)
        {

            if (map[starty, startx] == Terrain.NONE)
            {
                map[starty, startx] = water;
                int adjust = RNG.Next(-1, 2);
                if ((deltax == 0 && startx == 1) || (deltay == 0 && starty == 1))
                {
                    adjust = RNG.Next(0, 2);
                }
                else if ((deltax == 0 && startx == MAP_COLS - 2) || (deltay == 0 && starty == MAP_ROWS - 2))
                {
                    adjust = RNG.Next(-1, 1);
                }
                if (adjust < 0)
                {
                    if (deltax != 0)
                    {
                        starty--;
                    }
                    else
                    {
                        startx--;
                    }
                }
                else if (adjust > 0)
                {
                    if (deltax != 0)
                    {
                        starty++;
                    }
                    else
                    {
                        startx++;
                    }
                }
                map[starty, startx] = water;
            }
            startx += deltax;
            starty += deltay;
        }
    }

    public void DrawCanyon(Terrain riverT)
    {
        int drawLeft = RNG.Next(0, 5);
        int drawRight = RNG.Next(0, 5);
        Terrain tleft = climate.GetRandomTerrain(RNG, walkableTerrains);
        Terrain tright = climate.GetRandomTerrain(RNG, walkableTerrains);

        if (isHorizontal)
        {

            int rivery = RNG.Next(15, MAP_ROWS - 15);
            for (int x = 0; x < MAP_COLS; x++)
            {
                drawLeft++;
                drawRight++;
                map[rivery, x] = riverT;
                map[rivery + 1, x] = riverT;
                int adjust = RNG.Next(-3, 3);
                int leftM = RNG.Next(14, 17);
                if (rivery - leftM > 0)
                {
                    map[rivery - leftM + 3, x] = tleft;
                }
                if (drawLeft % 5 == 0)
                {
                    tleft = climate.GetRandomTerrain(RNG, walkableTerrains); ;
                }
                for (int i = rivery - leftM; i >= 0; i--)
                {
                    map[i, x] = Terrain.MOUNTAIN;
                }

                int rightM = RNG.Next(14, 17);

                if (rivery + rightM < MAP_ROWS)
                {
                    map[rivery + rightM - 3, x] = tright;
                }

                if (drawRight % 5 == 0)
                {
                    tright = climate.GetRandomTerrain(RNG, walkableTerrains); ;
                }
                for (int i = rivery + 1 + rightM; i < MAP_ROWS; i++)
                {
                    map[i, x] = Terrain.MOUNTAIN;
                }
                while (rivery + adjust + 1 > MAP_ROWS - 15 || rivery + adjust < 15)
                {
                    adjust = RNG.Next(-1, 2);
                }
                if (adjust > 0)
                {
                    int curr = 0;
                    while (curr < adjust)
                    {
                        map[rivery, x] = riverT;
                        rivery++;
                        curr++;
                    }
                }
                else
                {
                    int curr = 0;
                    while (curr > adjust)
                    {
                        map[rivery, x] = riverT;
                        rivery--;
                        curr--;
                    }
                }
            }
        }
        else
        {
            int riverx = RNG.Next(15, MAP_COLS - 15);
            for (int y = 0; y < MAP_ROWS; y++)
            {
                drawLeft++;
                drawRight++;
                map[y, riverx] = riverT;
                map[y, riverx + 1] = riverT;
                int adjust = RNG.Next(-3, 4);
                int leftM = RNG.Next(14, 17);
                if (riverx - leftM > 0)
                {
                    map[y, riverx - leftM + 3] = tleft;
                }
                if (drawLeft % 5 == 0)
                {
                    tleft = climate.GetRandomTerrain(RNG, walkableTerrains); ;
                }
                for (int i = riverx - leftM; i >= 0; i--)
                {
                    map[y, i] = Terrain.MOUNTAIN;
                }

                int rightM = RNG.Next(14, 17);

                if (riverx + rightM < MAP_COLS)
                {
                    map[y, riverx + rightM - 3] = tright;
                }

                if (drawRight % 5 == 0)
                {
                    tright = climate.GetRandomTerrain(RNG, walkableTerrains);
                }
                for (int i = riverx + 1 + rightM; i < MAP_COLS; i++)
                {
                    map[y, i] = Terrain.MOUNTAIN;
                }
                while (riverx + adjust + 1 > MAP_COLS - 15 || riverx + adjust < 15)
                {
                    adjust = RNG.Next(-1, 2);
                }
                if (adjust > 0)
                {
                    int curr = 0;
                    while (curr < adjust)
                    {
                        map[y, riverx] = riverT;
                        riverx++;
                        curr++;
                    }
                }
                else
                {
                    int curr = 0;
                    while (curr > adjust)
                    {
                        map[y, riverx] = riverT;
                        riverx--;
                        curr--;
                    }
                }
            }

        }
    }

    public void DrawCenterMountain()
    {
        int top = (MAP_ROWS - 35) / 2; //20
        int bottom = MAP_ROWS - top; //55
        if (isHorizontal)
        {
            //Block out a stripe of mountains where the caldera is going to be
            for (int i = 0; i < MAP_ROWS; i++)
            {
                if (i < top || i > bottom)
                {
                    for (int j = 0; j < MAP_COLS; j++)
                    {
                        map[i, j] = Terrain.MOUNTAIN;
                    }
                }
            }

            for (int y = 0; y < 8; y++)
            {
                int xstart = MAP_COLS / 2 - (3 + y); //29 to 
                int xend = MAP_COLS / 2 + (3 + y);
                //map[20 + i, jstart - 1] = Terrain.lava;
                //map[20 + i, jend] = Terrain.lava;
                for (int x = xstart; x < xend; x++)
                {

                    map[top + y, x] = Terrain.MOUNTAIN;
                }
            }
            for (int i = 0; i < 19; i++)
            {
                //map[28 + i, MAP_COLS / 2 - 11] = Terrain.lava;
                //map[28 + i, MAP_COLS / 2 - 10 + 21] = Terrain.lava;

                for (int j = 0; j < 20; j++)
                {
                    map[top + 8 + i, MAP_COLS / 2 - 10 + j] = Terrain.MOUNTAIN;
                }
            }
            for (int i = 0; i < 8; i++)
            {
                int jstart = MAP_COLS / 2 - (3 + (6 - i));
                int jend = MAP_COLS / 2 + (3 + (6 - i));
                //map[47 + i, jstart - 1] = Terrain.lava;
                //map[47 + i, jend] = Terrain.lava;
                for (int j = jstart; j < jend; j++)
                {
                    map[top + 27 + i, j] = Terrain.MOUNTAIN;
                }
            }
        }
        else
        {
            top = (MAP_COLS - 35) / 2;
            bottom = MAP_COLS - top;
            for (int i = 0; i < MAP_COLS; i++)
            {
                if (i < top || i > bottom)
                {
                    for (int j = 0; j < MAP_ROWS; j++)
                    {
                        map[j, i] = Terrain.MOUNTAIN;
                    }
                }
            }

            for (int i = 0; i < 8; i++)
            {
                int jstart = MAP_ROWS / 2 - (3 + i);
                int jend = MAP_ROWS / 2 + (3 + i);
                //map[20 + i, jstart - 1] = Terrain.lava;
                //map[20 + i, jend] = Terrain.lava;
                for (int j = jstart; j < jend; j++)
                {

                    map[j, top + i] = Terrain.MOUNTAIN;
                }
            }
            for (int i = 0; i < 19; i++)
            {
                //map[28 + i, MAP_COLS / 2 - 11] = Terrain.lava;
                //map[28 + i, MAP_COLS / 2 - 10 + 21] = Terrain.lava;

                for (int j = 0; j < 20; j++)
                {
                    map[MAP_ROWS / 2 - 10 + j, top + 8 + i] = Terrain.MOUNTAIN;
                }
            }
            for (int i = 0; i < 8; i++)
            {
                int jstart = MAP_ROWS / 2 - (3 + (6 - i));
                int jend = MAP_ROWS / 2 + (3 + (6 - i));
                //map[47 + i, jstart - 1] = Terrain.lava;
                //map[47 + i, jend] = Terrain.lava;
                for (int j = jstart; j < jend; j++)
                {
                    map[j, top + 27 + i] = Terrain.MOUNTAIN;
                }
            }
        }
    }

    protected bool HorizontalCave(int caveOrientation, int centerx, int centery, Location cave1l, Location cave1r)
    {
        if (caveOrientation == 0) //first cave left
        {
            int cavey = RNG.Next(centery - 2, centery + 3);
            int cavex = centerx;
            while (map[cavey, cavex] != Terrain.MOUNTAIN)
            {
                cavex--;
            }
            if (map[cavey + 1, cavex] != Terrain.MOUNTAIN)
            {
                cavey--;
            }
            else if (map[cavey - 1, cavex] != Terrain.MOUNTAIN)
            {
                cavey++;
            }
            map[cavey, cavex] = Terrain.CAVE;
            cave1r.Ypos = cavey + 30;
            cave1r.Xpos = cavex;
            cavex--;
            int curr = 0;
            while (cavex > 0 && map[cavey, cavex] == Terrain.MOUNTAIN)
            {
                cavex--;
                curr++;
            }

            if (curr <= 2 || cavex <= 0)
            {
                return false;
            }
            if (map[cavey, cavex] == Terrain.CAVE)
            {
                return false;
            }
            cavex++;
            if (map[cavey + 1, cavex] == Terrain.CAVE || map[cavey - 1, cavex] == Terrain.CAVE)
            {
                return false;
            }
            map[cavey, cavex] = Terrain.CAVE;
            cave1l.Ypos = cavey + 30;
            cave1l.Xpos = cavex;
            map[cavey + 1, cavex] = Terrain.MOUNTAIN;
            map[cavey - 1, cavex] = Terrain.MOUNTAIN;
        }
        else
        {
            int cavey = RNG.Next(centery - 2, centery + 3);
            int cavex = centerx;
            while (map[cavey, cavex] != Terrain.MOUNTAIN)
            {
                cavex++;
            }
            if (map[cavey + 1, cavex] != Terrain.MOUNTAIN)
            {
                cavey--;
            }
            else if (map[cavey - 1, cavex] != Terrain.MOUNTAIN)
            {
                cavey++;
            }
            map[cavey, cavex] = Terrain.CAVE;
            cave1l.Ypos = cavey + 30;
            cave1l.Xpos = cavex;
            cavex++;
            int curr = 0;
            while (cavex < MAP_COLS && map[cavey, cavex] == Terrain.MOUNTAIN)
            {
                cavex++;
                curr++;
            }
            if (curr <= 2 || cavex >= MAP_COLS)
            {
                return false;
            }
            if (map[cavey, cavex] == Terrain.CAVE)
            {
                return false;
            }
            cavex--;
            if (map[cavey + 1, cavex] == Terrain.CAVE || map[cavey - 1, cavex] == Terrain.CAVE)
            {
                return false;
            }
            map[cavey, cavex] = Terrain.CAVE;
            cave1r.Ypos = cavey + 30;
            cave1r.Xpos = cavex;
            map[cavey + 1, cavex] = Terrain.MOUNTAIN;
            map[cavey - 1, cavex] = Terrain.MOUNTAIN;
        }
        return true;
    }

    /// <summary>
    /// Places a vertically oriented cave into/out of a location (currently only the caldera)
    /// </summary>
    /// <param name="caveType">Which order the two passthrough caves will occur in</param>
    /// <param name="centerx"></param>
    /// <param name="centery"></param>
    /// <param name="cave1l"></param>
    /// <param name="cave1r"></param>
    /// <returns></returns>
    protected bool VerticalCave(int caveType, int centerx, int centery, Location cave1l, Location cave1r)
    {
        if (caveType == 0) //first cave up
        {
            int cavey = centery;
            int cavex = RNG.Next(centerx - 2, centerx + 3);
            while (map[cavey, cavex] != Terrain.MOUNTAIN)
            {
                cavey--;
            }
            if (map[cavey, cavex + 1] != Terrain.MOUNTAIN)
            {
                cavex--;
            }
            else if (map[cavey, cavex - 1] != Terrain.MOUNTAIN)
            {
                cavex++;
            }
            map[cavey, cavex] = Terrain.CAVE;
            cave1r.Ypos = cavey + 30;
            cave1r.Xpos = cavex;
            cavey--;
            int curr = 0;
            while (cavey > 0 && map[cavey, cavex] == Terrain.MOUNTAIN)
            {
                cavey--;
                curr++;
            }
            if (curr <= 2 || cavey <= 0)
            {
                return false;
            }
            if (map[cavey, cavex] == Terrain.CAVE)
            {
                return false;
            }
            cavey++;
            if (map[cavey, cavex + 1] == Terrain.CAVE || map[cavey, cavex - 1] == Terrain.CAVE)
            {
                return false;
            }
            map[cavey, cavex] = Terrain.CAVE;
            cave1l.Ypos = cavey + 30;
            cave1l.Xpos = cavex;
            map[cavey, cavex + 1] = Terrain.MOUNTAIN;
            map[cavey, cavex - 1] = Terrain.MOUNTAIN;
        }
        else
        {
            //Debug.WriteLine(GetMapDebug());
            int cavey = centery;
            int cavex = RNG.Next(centerx - 2, centerx + 3);
            while (map[cavey, cavex] != Terrain.MOUNTAIN)
            {
                cavey++;
            }
            if (map[cavey, cavex + 1] != Terrain.MOUNTAIN)
            {
                cavex--;
            }
            else if (map[cavey, cavex - 1] != Terrain.MOUNTAIN)
            {
                cavex++;
            }
            map[cavey, cavex] = Terrain.CAVE;
            cave1l.Ypos = cavey + 30;
            cave1l.Xpos = cavex;
            cavey++;
            int curr = 0;
            while (cavey < MAP_ROWS && map[cavey, cavex] == Terrain.MOUNTAIN)
            {
                cavey++;
                curr++;
            }
            if (curr <= 2 || cavey >= MAP_ROWS)
            {
                return false;
            }
            if (map[cavey, cavex] == Terrain.CAVE)
            {
                return false;
            }
            cavey--;
            if (map[cavey, cavex + 1] == Terrain.CAVE || map[cavey, cavex - 1] == Terrain.CAVE)
            {
                return false;
            }

            map[cavey, cavex] = Terrain.CAVE;
            cave1r.Ypos = cavey + 30;
            cave1r.Xpos = cavex;
            map[cavey, cavex + 1] = Terrain.MOUNTAIN;
            map[cavey, cavex - 1] = Terrain.MOUNTAIN;
        }
        return true;
    }

    public string GetGlobDebug(int[,] globs)
    {
        StringBuilder debug = new();
        for (int y = 0; y < MAP_ROWS; y++)
        {
            for (int x = 0; x < MAP_COLS; x++)
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
        for (int y = 0; y < MAP_ROWS; y++)
        {
            for (int x = 0; x < MAP_COLS; x++)
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

    public bool ValidateCaves()
    {
        for(int y = 0; y < MAP_ROWS; y++)
        {
            for(int x = 0; x < MAP_COLS; x++)
            {
                if (map[y,x] == Terrain.CAVE)
                {
                    //If all 4 sides of a cave are unwalkable terrain, you can never leave the cave
                    //and you softlock since you can't turn around
                    if (
                        (y + 1 >= MAP_ROWS || !map[y + 1, x].IsWalkable())
                        && (y == 0 || !map[y - 1, x].IsWalkable())
                        && (x + 1 >= MAP_COLS || !map[y, x + 1].IsWalkable())
                        && (x == 0 || !map[y, x - 1].IsWalkable())
                    )
                    {
                        return false;
                    }
                    //If a cave exits into a lake, you softlock unless you have the boots, and the previous case
                    //(correctly) considers walkable water as walkable, so we need to cover that case here.
                    if (
                        y + 1 < MAP_ROWS && map[y + 1, x] == Terrain.WALKABLEWATER
                        && (y == 0 || !map[y - 1, x].IsWalkable())
                        && (x + 1 >= MAP_COLS || !map[y, x + 1].IsWalkable())
                        && (x == 0 || !map[y, x - 1].IsWalkable())
                    )
                    {
                        map[y + 1, x] = Terrain.ROAD;
                    }
                    if (
                        (y + 1 >= MAP_ROWS || !map[y + 1, x].IsWalkable())
                        && y > 0 && map[y - 1, x] == Terrain.WALKABLEWATER
                        && (x + 1 >= MAP_COLS || !map[y, x + 1].IsWalkable())
                        && (x == 0 || !map[y, x - 1].IsWalkable())
                    )
                    {
                        map[y - 1, x] = Terrain.ROAD;
                    }
                    if (
                        (y + 1 >= MAP_ROWS || !map[y + 1, x].IsWalkable())
                        && (y == 0 || !map[y - 1, x].IsWalkable())
                        && x + 1 < MAP_COLS && map[y, x + 1] == Terrain.WALKABLEWATER
                        && (x == 0 || !map[y, x - 1].IsWalkable())
                    )
                    {
                        map[y, x + 1] = Terrain.ROAD;
                    }
                    if (
                        (y + 1 >= MAP_ROWS || !map[y + 1, x].IsWalkable())
                        && (y == 0 || !map[y - 1, x].IsWalkable())
                        && (x + 1 >= MAP_COLS || !map[y, x + 1].IsWalkable())
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
    public bool PassthroughsIntersectRaftCoordinates(IEnumerable<(int, int)> raftCoordinates)
    {
        foreach (Location location in AllLocations.Where(i => i.PassThrough != 0))
        {
            if (raftCoordinates.Any(i => location.Xpos == i.Item2 && location.Ypos == i.Item1))
            {
                return true;
            }
        }
        return false;
    }

    public abstract void UpdateVisit(Dictionary<Item, bool> itemGet, Dictionary<Spell, bool> spellGet);

    public abstract IEnumerable<Location> RequiredLocations(bool hiddenPalace, bool hiddenKasuto);

}