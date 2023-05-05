using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections;
using System.Runtime.InteropServices;
using NLog;
using System.Threading.Channels;
using System.Diagnostics;
using Z2Randomizer.Core;
//using System.Runtime.InteropServices.WindowsRuntime;

namespace Z2Randomizer.Core.Overworld;

public abstract class World
{
    protected readonly Logger logger = LogManager.GetCurrentClassLogger();
    protected SortedDictionary<String, List<Location>> areasByLocation;
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
    protected List<int> spawners;
    protected List<int> smallEnemies;
    protected List<int> largeEnemies;
    protected int enemyPtr;
    protected List<int> overworldMaps;
    protected SortedDictionary<Tuple<int, int>, Location> locsByCoords;
    protected Hyrule hyrule;
    protected Terrain[,] map;
    private const int overworldXOffset = 0x3F;
    private const int overworldMapOffset = 0x7E;
    private const int overworldWorldOffset = 0xBD;
    private List<int> visitedEnemies;
    protected int MAP_ROWS;
    protected int MAP_COLS;
    protected int bytesWritten;
    protected List<Terrain> randomTerrains;
    protected List<Terrain> walkableTerrains;
    protected bool[,] visitation;
    protected const int MAP_SIZE_BYTES = 1408;
    protected List<Location> unimportantLocs;
    protected Biome biome;
    protected bool isHorizontal;
    protected int VANILLA_MAP_ADDR;
    protected SortedDictionary<Tuple<int, int>, string> section;
    public Location raft;
    public Location bridge;
    public Location cave1;
    public Location cave2;
    //private bool allreached;

    protected int baseAddr;

    private const int MAXIMUM_BRIDGE_LENGTH = 10;
    private const int MINIMUM_BRIDGE_LENGTH = 2;

    protected abstract List<Location> GetPathingStarts();
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

    public World(Hyrule parent)
    {
        hyrule = parent;
        connections = new Dictionary<Location, Location>();
        Locations = new Dictionary<Terrain, List<Location>>();
        foreach(Terrain Terrain in Enum.GetValues(typeof(Terrain)))
        {
            Locations.Add(Terrain, new List<Location>());
        }
        //Locations = new List<Location>[11] { Towns, Caves, Palaces, Bridges, Deserts, Grasses, Forests, Swamps, Graves, Roads, Lavas };
        AllLocations = new List<Location>();
        locsByCoords = new SortedDictionary<Tuple<int, int>, Location>();
        //reachableAreas = new HashSet<string>();
        visitedEnemies = new List<int>();
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
        //This only swaps elements in a list a number of times equal to the number of elements in the list.
        //This is an extremely biased shuffle (see https://blog.codinghorror.com/the-danger-of-naivete/)
        //ALSO, it has a bias towards things in their vanilla locations by just not shuffling when one of
        //the locations can't shuffle instead of ignoring such locations in the shuffling
        //TODO: Replace this with knuth-fisher-yates, which will break seed compatibility
        /*for (int i = 0; i < locationsToShuffle.Count; i++)
        {
            int s = hyrule.RNG.Next(i, locationsToShuffle.Count);
            Location sl = locationsToShuffle[s];
            if (sl.CanShuffle && locationsToShuffle[i].CanShuffle)
            {
                Swap(locationsToShuffle[i], locationsToShuffle[s]);
            }
        }*/

        List<Location> shufflableLocations = locationsToShuffle.Where(i => i.CanShuffle).ToList();
        for (int i = shufflableLocations.Count() - 1; i > 0; i--)
        {
            int n = hyrule.RNG.Next(i + 1);
            Swap(shufflableLocations[i], shufflableLocations[n]);
        }
    }


    protected void Swap(Location l1, Location l2)
    {
        int tempX = l1.Xpos;
        int tempY = l1.Ypos;
        int tempPass = l1.PassThrough;
        l1.Xpos = l2.Xpos;
        l1.Ypos = l2.Ypos;
        l1.PassThrough = l2.PassThrough;
        l2.Xpos = tempX;
        l2.Ypos = tempY;
        l2.PassThrough = tempPass;
    }

    public void ShuffleEnemies(int addr, bool isOver)
    {
        if (isOver)
        {
            addr = addr + hyrule.ROMData.GetByte(addr);
        }
        if (!visitedEnemies.Contains(addr) && addr != 0x95A4)
        {
            int numBytes = hyrule.ROMData.GetByte(addr);
            for (int j = addr + 2; j < addr + numBytes; j = j + 2)
            {
                int enemy = hyrule.ROMData.GetByte(j) & 0x3F;
                int highPart = hyrule.ROMData.GetByte(j) & 0xC0;
                if (hyrule.Props.MixPalaceEnemies)
                {
                    if (enemies.Contains(enemy))
                    {
                        int swap = enemies[hyrule.RNG.Next(0, enemies.Count)];
                        hyrule.ROMData.Put(j, (Byte)(swap + highPart));
                        if ((smallEnemies.Contains(enemy) && largeEnemies.Contains(swap) && swap != 0x20))
                        {
                            int ypos = hyrule.ROMData.GetByte(j - 1) & 0xF0;
                            int xpos = hyrule.ROMData.GetByte(j - 1) & 0x0F;
                            ypos = ypos - 32;
                            hyrule.ROMData.Put(j - 1, (Byte)(ypos + xpos));
                        }
                        else if (swap == 0x20 && swap != enemy)
                        {
                            int ypos = hyrule.ROMData.GetByte(j - 1) & 0xF0;
                            int xpos = hyrule.ROMData.GetByte(j - 1) & 0x0F;
                            ypos = ypos - 48;
                            hyrule.ROMData.Put(j - 1, (Byte)(ypos + xpos));
                        }
                        else if (enemy == 0x1F && swap != enemy)
                        {
                            int ypos = hyrule.ROMData.GetByte(j - 1) & 0xF0;
                            int xpos = hyrule.ROMData.GetByte(j - 1) & 0x0F;
                            ypos = ypos - 16;
                            hyrule.ROMData.Put(j - 1, (Byte)(ypos + xpos));
                        }
                    }
                }
                else
                {

                    if (largeEnemies.Contains(enemy))
                    {
                        int swap = hyrule.RNG.Next(0, largeEnemies.Count);
                        if (largeEnemies[swap] == 0x20 && largeEnemies[swap] != enemy)
                        {
                            int ypos = hyrule.ROMData.GetByte(j - 1) & 0xF0;
                            int xpos = hyrule.ROMData.GetByte(j - 1) & 0x0F;
                            ypos = ypos - 48;
                            hyrule.ROMData.Put(j - 1, (Byte)(ypos + xpos));
                        }
                        hyrule.ROMData.Put(j, (Byte)(largeEnemies[swap] + highPart));
                    }

                    if (smallEnemies.Contains(enemy))
                    {
                        int swap = hyrule.RNG.Next(0, smallEnemies.Count);
                        hyrule.ROMData.Put(j, (Byte)(smallEnemies[swap] + highPart));
                    }
                }

                if (flyingEnemies.Contains(enemy))
                {
                    int swap = hyrule.RNG.Next(0, flyingEnemies.Count);
                    hyrule.ROMData.Put(j, (Byte)(flyingEnemies[swap] + highPart));

                    if (flyingEnemies[swap] == 0x07 || flyingEnemies[swap] == 0x0a || flyingEnemies[swap] == 0x0d || flyingEnemies[swap] == 0x0e)
                    {
                        int ypos = 0x00;
                        int xpos = hyrule.ROMData.GetByte(j - 1) & 0x0F;
                        hyrule.ROMData.Put(j - 1, (Byte)(ypos + xpos));
                    }
                }

                if (spawners.Contains(enemy))
                {
                    int swap = hyrule.RNG.Next(0, spawners.Count);
                    hyrule.ROMData.Put(j, (Byte)(spawners[swap] + highPart));
                }

                if (enemy == 33)
                {
                    int swap = hyrule.RNG.Next(0, spawners.Count + 1);
                    if (swap != spawners.Count)
                    {
                        hyrule.ROMData.Put(j, (Byte)(spawners[swap] + highPart));
                    }
                }
            }
            visitedEnemies.Add(addr);
        }
    }

    protected void ChooseConn(String section, Dictionary<Location, Location> co, bool changeType)
    {
        if (co.Count > 0)
        {
            int start = hyrule.RNG.Next(areasByLocation[section].Count);
            Location s = areasByLocation[section][start];
            int conn = hyrule.RNG.Next(co.Count);
            Location c = co.Keys.ElementAt(conn);
            int count = 0;
            while ((!c.CanShuffle || !s.CanShuffle || (!changeType && (c.TerrainType != s.TerrainType))) && count < co.Count)
            {
                start = hyrule.RNG.Next(areasByLocation[section].Count);
                s = areasByLocation[section][start];
                conn = hyrule.RNG.Next(co.Count);
                c = co.Keys.ElementAt(conn);
                count++;
            }
            Swap(s, c);
            c.CanShuffle = false;
        }
    }

    protected void LoadLocations(int startAddr, int locNum, SortedDictionary<int, Terrain> Terrains, Continent continent)
    {
        for (int i = 0; i < locNum; i++)
        {
            byte[] bytes = new Byte[4] { hyrule.ROMData.GetByte(startAddr + i), hyrule.ROMData.GetByte(startAddr + overworldXOffset + i), hyrule.ROMData.GetByte(startAddr + overworldMapOffset + i), hyrule.ROMData.GetByte(startAddr + overworldWorldOffset + i) };
            AddLocation(new Location(bytes, Terrains[startAddr + i], startAddr + i, continent));
        }
    }

    protected Location GetLocationByMap(int map, int world)
    {
        Location l = null;
        foreach (Location loc in AllLocations)
        {
            if (loc.LocationBytes[2] == map && loc.World == world)
            {
                l = loc;
                break;
            }
        }
        return l;
    }

    protected void ReadVanillaMap()
    {
        int addr = VANILLA_MAP_ADDR;
        int i = 0;
        int j = 0;
        map = new Terrain[MAP_ROWS, MAP_COLS];
        while(i < MAP_ROWS)
        {
            j = 0;
            while(j < MAP_COLS)
            {
                byte data = hyrule.ROMData.GetByte(addr);
                int count = (data & 0xF0) >> 4;
                count++;
                Terrain t = (Terrain)(data & 0x0F);
                for(int k = 0; k < count; k++)
                {
                    map[i, j + k] = t;
                }
                j += count;
                addr++;
            }
            i++;
        }
    }

    public void UpdateAllReached()
    {
        if(AllReached)
        {
            return;
        }
        else
        {
            AllReached = true;
            foreach(Location location in AllLocations)
            {
                if(location.TerrainType == Terrain.PALACE || location.TerrainType == Terrain.TOWN || location.item != Item.DO_NOT_USE)
                {
                    if(!location.Reachable)
                    {
                        AllReached = false;
                    }
                }
            }
        }
    }

    protected Location GetLocationByCoords(Tuple<int, int> coords)
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

    public void ShuffleOverworldEnemies()
    {
        for (int i = enemyPtr; i < enemyPtr + 126; i = i + 2)
        {
            int low = hyrule.ROMData.GetByte(i);
            int high = hyrule.ROMData.GetByte(i + 1);
            high = high << 8;
            high = high & 0x0FFF;
            int addr = high + low + enemyAddr;
            ShuffleEnemies(high + low + enemyAddr, false);
        }

        foreach (int i in overworldMaps)
        {
            int ptrAddr = enemyPtr + i * 2;
            int low = hyrule.ROMData.GetByte(ptrAddr);
            int high = hyrule.ROMData.GetByte(ptrAddr + 1);
            high = high << 8;
            high = high & 0x0FFF;
            int addr = high + low + enemyAddr;
            ShuffleEnemies(high + low + enemyAddr, true);
        }
    }

    protected bool PlaceLocations(Terrain riverTerrain)
    {
        int i = 0;
        foreach (Location location in AllLocations)
        {
            i++;
            if ((location.TerrainType != Terrain.BRIDGE && location.CanShuffle && !unimportantLocs.Contains(location) && location.PassThrough == 0) || location.NeedHammer)
            {
                int x = 0;
                int y = 0;
                //Place the location in a spot that is not adjacent to any other location
                do
                {
                    x = hyrule.RNG.Next(MAP_COLS - 2) + 1;
                    y = hyrule.RNG.Next(MAP_ROWS - 2) + 1;
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
                );

                map[y, x] = location.TerrainType;
                //Connect the cave
                if (location.TerrainType == Terrain.CAVE)
                {
                    Direction direction = (Direction)hyrule.RNG.Next(4);
                    Terrain entranceTerrain = walkableTerrains[hyrule.RNG.Next(walkableTerrains.Count)];
                    // if (!hyrule.Props.saneCaves || !connections.ContainsKey(location))
                    //Updated to avoid "if not else" anti-pattern
                    if(hyrule.Props.SaneCaves && connections.ContainsKey(location))
                    {
                        PlaceCaveCount++;
                        if(!PlaceSaneCave(x, y, direction, entranceTerrain, riverTerrain, location))
                        {
                            return false;
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
                    Terrain s = walkableTerrains[hyrule.RNG.Next(walkableTerrains.Count)];
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
                else if(location.TerrainType != Terrain.TOWN || location.TownNum != Town.NEW_KASUTO_2) //don't place newkasuto2
                {
                    Terrain t = Terrain.NONE;
                    do
                    {
                        t = walkableTerrains[hyrule.RNG.Next(walkableTerrains.Count)];
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
    /// <param name="x"></param>
    /// <param name="otherx"></param>
    /// <param name="y"></param>
    /// <param name="othery"></param>
    /// <param name="riverT"></param>
    /// <returns></returns>
    protected bool CrossingWater(int x, int otherx, int y, int othery, Terrain riverT)
    {
        int smallx = x;
        int largex = otherx;
        if(x > otherx)
        {
            smallx = otherx;
            largex = x;
        }

        int smally = y;
        int largey = othery;

        if(y > othery)
        {
            smally = othery;
            largey = y;
        }

        for(int i = smally; i < largey; i++)
        {
            for (int j = smallx; j < largex; j++)
            {
                if(i > 0 && i < MAP_ROWS && j > 0 && j < MAP_COLS && map[i, j] == riverT)
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

    protected bool PlaceSaneCave(int x, int y, Direction direction, Terrain entranceTerrain, Terrain riverTerrain, Location location)
    {
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
        map[y, x] = Terrain.NONE;
        //Place the exit cave less than 5 rows or columns from the edge of the map, on an unoccupied square 
        //That is also not adjacent to any other location.
        do
        {
            x = hyrule.RNG.Next(MAP_COLS - 2) + 1;
            y = hyrule.RNG.Next(MAP_ROWS - 2) + 1;
        } while (x < 5 || y < 5 || x > MAP_COLS - 5 || y > MAP_ROWS - 5 || map[y, x] != Terrain.NONE || map[y - 1, x] != Terrain.NONE || map[y + 1, x] != Terrain.NONE || map[y + 1, x + 1] != Terrain.NONE || map[y, x + 1] != Terrain.NONE || map[y - 1, x + 1] != Terrain.NONE || map[y + 1, x - 1] != Terrain.NONE || map[y, x - 1] != Terrain.NONE || map[y - 1, x - 1] != Terrain.NONE);

        while ((direction == Direction.NORTH && y < 15) || (direction == Direction.EAST && x > MAP_COLS - 15) || (direction == Direction.SOUTH && y > MAP_ROWS - 15) || (direction == Direction.WEST && x < 15))
        {
            direction = (Direction)hyrule.RNG.Next(4);
        }
        int otherx = 0;
        int othery = 0;
        int tries = 0;
        bool crossing = true;    
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
                offset = 20;
            }
            crossing = true;
            if (direction == Direction.NORTH)
            {
                otherx = x + (hyrule.RNG.Next(7) - 3);
                othery = y - (hyrule.RNG.Next(range) + offset);
            }
            else if (direction == Direction.EAST)
            {
                otherx = x + (hyrule.RNG.Next(range) + offset);
                othery = y + (hyrule.RNG.Next(7) - 3);
            }
            else if (direction == Direction.SOUTH)
            {
                otherx = x + (hyrule.RNG.Next(7) - 3);
                othery = y + (hyrule.RNG.Next(range) + offset);
            }
            else //west
            {
                otherx = x - (hyrule.RNG.Next(range) + offset);
                othery = y + (hyrule.RNG.Next(7) - 3);
            }
            if (this.biome != Biome.VOLCANO)
            {
                if (!CrossingWater(x, otherx, y, othery, riverTerrain))
                {
                    crossing = false;
                }
            }
            if (tries++ >= 100)
            {
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
        PlaceCave(x, y, direction, walkableTerrains[hyrule.RNG.Next(walkableTerrains.Count)]);
        PlaceCave(otherx, othery, direction.Reverse(), walkableTerrains[hyrule.RNG.Next(walkableTerrains.Count)]);
        return true;
    }

    public void PlaceHiddenLocations()
    {
        foreach(Location location in unimportantLocs)
        {
            if (location.CanShuffle)
            {
                int tries = 0;
                int x = 0;
                int y = 0;
                do
                {
                    x = hyrule.RNG.Next(MAP_COLS);
                    y = hyrule.RNG.Next(MAP_ROWS);
                    tries++;
                } while ((map[y, x] != location.TerrainType || GetLocationByCoords(Tuple.Create(y + 30, x)) != null) && tries < 2000);

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
    /// <param name="numBridges">Number of bridges to create</param>
    /// <param name="placeTown">If true, one of the bridges is the saria river crossing</param>
    /// <param name="riverTerrain">Type of terrain the rivers are crossing over. Usually this is water, but on Mountanous, the "bridges" are over mountains</param>
    /// <param name="riverDevil">If true, one of the bridges will be a road blocked by the river devil</param>
    /// <param name="placeLongBridge">If true, one of the bridges is the bridge from vanilla west connecting DM to the SE desert with encounters at both ends</param>
    /// <param name="placeDarunia">If true, one of the bridges is a desert road with the two encounters that lead to darunia in vanilla</param>
    /// <returns>False if greater than 2000 total attempts were made in placement of all of the bridges. Else true.</returns>
    protected bool ConnectIslands(int numBridges, bool placeTown, Terrain riverTerrain, bool riverDevil, bool placeLongBridge, bool placeDarunia)
    {
        int[,] mass = GetTerrainGlobs();
        Dictionary<int, List<int>> connectMass = new Dictionary<int, List<int>>();
        int bridges = numBridges;
        int TerrainCycle = 0;
        int tries = 0;
        while (bridges > 0 && tries < 2000)
        {
            tries++;
            int x = hyrule.RNG.Next(MAP_COLS - 2) + 1;
            int y = hyrule.RNG.Next(MAP_ROWS - 2) + 1;
            Direction waterDirection = NextToWater(x, y, riverTerrain);
            int waterTries = 0;
            while (waterDirection == Direction.NONE && waterTries < 2000)// || (this.bio == biome.canyon && (waterdir == Direction.NORTH || waterdir == Direction.SOUTH)))
            {
                x = hyrule.RNG.Next(MAP_COLS - 2) + 1;
                y = hyrule.RNG.Next(MAP_ROWS - 2) + 1;
                waterDirection = NextToWater(x, y, riverTerrain);
                waterTries++;
            }
            if(waterTries >= 2000)
            {
                return false;
            }
            int deltaX = waterDirection.DeltaX();
            int deltaY = waterDirection.DeltaY();
            int length = 0;
            if(IsSingleTile(y, x))
            {
                length = 100;
            }

            int startMass = mass[y, x];

           
            if(GetLocationByCoords(Tuple.Create(y + 30, x)) != null 
                || GetLocationByCoords(Tuple.Create(y + 30, x + 1)) != null 
                || GetLocationByCoords(Tuple.Create(y + 30, x - 1)) != null 
                || GetLocationByCoords(Tuple.Create(y + 31, x)) != null 
                || GetLocationByCoords(Tuple.Create(y + 29, x)) != null)
            {
                length = 100;
            }

            x += deltaX;
            y += deltaY;
            
            while (x > 0 && x < MAP_COLS && y > 0 && y < MAP_ROWS && map[y, x] == riverTerrain)
            {
                if(x + 1 < MAP_COLS && GetLocationByCoords(Tuple.Create(y + 30, x + 1)) != null)
                {
                    length = 100;
                }
                if (x - 1 > 0 && GetLocationByCoords(Tuple.Create(y + 30, x - 1)) != null)
                {
                    length = 100;
                }

                if (y + 1 < MAP_ROWS && GetLocationByCoords(Tuple.Create(y + 31, x)) != null)
                {
                    length = 100;
                }
                if (y - 1 > 0 && GetLocationByCoords(Tuple.Create(y + 29, x)) != null)
                {
                    length = 100;
                }
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
                if(deltaX != 0)
                {
                    if(y - 1 > 0)
                    {
                        if(map[y - 1, x] != riverTerrain && map[y - 1, x] != Terrain.MOUNTAIN)
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

                if (adjacentRiverTerrainDirectionCount <= 2)
                {
                    length = 100;
                }
                x += deltaX;
                y += deltaY;
                length++;
                
            }
            if (IsSingleTile(y, x))
            {
                length = 100;
            }
            
            int endMass = 0;
            if(y > 0 && x > 0 && y < MAP_ROWS - 1 && x < MAP_COLS - 1)
            {
                if (GetLocationByCoords(Tuple.Create(y + 30, x)) != null 
                    || GetLocationByCoords(Tuple.Create(y + 30, x + 1)) != null 
                    || GetLocationByCoords(Tuple.Create(y + 30, x - 1)) != null 
                    || GetLocationByCoords(Tuple.Create(y + 31, x)) != null 
                    || GetLocationByCoords(Tuple.Create(y + 29, x)) != null)
                {
                    length = 100;
                }
                endMass = mass[y, x];
            }
            
            if((riverTerrain != Terrain.DESERT && this.biome != Biome.CALDERA && this.biome != Biome.VOLCANO) 
                && (startMass == 0 
                    || endMass == 0 
                    || endMass == startMass 
                    || (connectMass.ContainsKey(startMass) && connectMass[startMass].Contains(endMass))
                )
            )
            {
                length = 100;
            }
            if (
                (placeTown && length < MAXIMUM_BRIDGE_LENGTH || (length < MAXIMUM_BRIDGE_LENGTH && length > MINIMUM_BRIDGE_LENGTH)) 
                && x > 0 
                && x < MAP_COLS - 1 
                && y > 0 
                && y < MAP_ROWS - 1 
                && walkableTerrains.Contains(map[y, x]) 
                && map[y, x] != riverTerrain)
            {
                if(!connectMass.ContainsKey(startMass))
                {
                    List<int> c = new List<int>();
                    c.Add(endMass);
                    connectMass[startMass] = c;
                }
                else
                {                        
                    connectMass[startMass].Add(endMass);
                }

                if (!connectMass.ContainsKey(endMass))
                {
                    List<int> c = new List<int>();
                    c.Add(startMass);
                    connectMass[endMass] = c;
                }
                else
                {
                    connectMass[endMass].Add(startMass);
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
                else if(placeLongBridge)
                {
                    Location bridge1 = GetLocationByMap(0x04, 0);
                    Location bridge2 = GetLocationByMap(0xC5, 0);
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
                else if(placeDarunia)
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
                    else {
                        while (map[y, x] == riverTerrain)
                        {

                            if (this.biome == Biome.MOUNTAINOUS || this.biome == Biome.VANILLALIKE)
                            {
                                if (this.biome == Biome.VANILLALIKE)
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
                                        map[y, x] = Terrain.SPIDER;
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
                                            map[y, x] = Terrain.SPIDER;
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
                                            map[y, x] = Terrain.SPIDER;
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
                    if (riverTerrain != Terrain.MOUNTAIN && riverTerrain != Terrain.DESERT && !hyrule.Props.CanWalkOnWaterWithBoots)
                    {
                        TerrainCycle = TerrainCycle % 3;
                    }
                    else
                    {
                        TerrainCycle = TerrainCycle % 2;
                    }
                    
                }
                
                bridges--;
                
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
                if(mass[i,j] == 0 && walkableTerrains.Contains(map[i,j]))
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
        if(y - 1 > 0 && mass[y - 1, x] == 0 && walkableTerrains.Contains(map[y - 1, x]))
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

    private string PrintTerrainGlobMap(int[,] mass)
    {
        char[] encoding = {'0','1','2','3','4','5','6','7','8','9','A','B','C','D','E','F','G','H','I','J','K','L','M','N','O','P','Q'};
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
        if(walkableTerrains.Contains(map[y, x]) && map[y, x] != riverTerrain)
        {
            if(map[y + 1, x] == riverTerrain)
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
    protected bool GrowTerrain()
    {
        TerrainGrowthAttempts++;
        Terrain[,] mapCopy = new Terrain[MAP_ROWS, MAP_COLS];
        List<Tuple<int, int>> placed = new List<Tuple<int, int>>();
        for(int i = 0; i < MAP_ROWS; i++)
        {
            for(int j = 0; j < MAP_COLS; j++)
            {
                if (map[i, j] != Terrain.NONE && randomTerrains.Contains(map[i, j]))
                {
                    placed.Add(new Tuple<int, int>(i, j));
                }
            }
        }

        int tx, ty;
        double distance;
        List<Terrain> choices = new List<Terrain>();
        for (int i = 0; i < MAP_ROWS; i++)
        {
            for (int j = 0; j < MAP_COLS; j++)
            {
                if (map[i, j] == Terrain.NONE)
                {
                    choices.Clear();
                    double mindistance = Int32.MaxValue;

                    foreach(Tuple<int, int> t in placed)
                    {
                        tx = t.Item1 - i;
                        ty = t.Item2 - j;
                        distance = Math.Sqrt(tx * tx + ty * ty);
                        //distance = ((tx + (tx >> 31)) ^ (tx >> 31)) + ((ty + (ty >> 31)) ^ (ty >> 31));
                        //distance = Math.Abs(tx) + Math.Abs(ty);
                        if (distance < mindistance)
                        {
                            choices = new List<Terrain>();
                            choices.Add(map[t.Item1, t.Item2]);
                            mindistance = distance;
                        }
                        else if(distance == mindistance)
                        {
                            choices.Add(map[t.Item1, t.Item2]);
                        }
                    }
                    mapCopy[i, j] = choices[hyrule.RNG.Next(choices.Count)];
                }
            }
        }

        for (int i = 0; i < MAP_ROWS; i++)
        {
            for (int j = 0; j < MAP_COLS; j++)
            {
                if (map[i, j] != Terrain.NONE)
                {
                    mapCopy[i, j] = map[i, j];
                }
            }
        }
        map = (Terrain[,])mapCopy.Clone();
        return true;
    }

    protected void PlaceRandomTerrain(int num)
    {
        //randomly place remaining Terrain
        int placed = 0;
        while (placed < num)
        {
            int x = 0;
            int y = 0;
            Terrain t = randomTerrains[hyrule.RNG.Next(randomTerrains.Count)];
            do
            {
                x = hyrule.RNG.Next(MAP_COLS);
                y = hyrule.RNG.Next(MAP_ROWS);
            } while (map[y, x] != Terrain.NONE);
            map[y, x] = t;
            placed++;
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
    protected void WriteMapToRom(bool doWrite, int loc, int total, int h1, int h2)
    {
        bytesWritten = 0; //Number of bytes written so far
        Terrain currentTerrain = map[0, 0];
        int currentTerrainCount = 0;
        for (int y = 0; y < MAP_ROWS; y++)
        {
            for (int x = 0; x < MAP_COLS; x++)
            {
                if(hyrule.Props.HiddenPalace && y == h1 && x == h2 && y != 0 && x != 0)
                {
                    currentTerrainCount--;
                    int b = currentTerrainCount * 16 + (int)currentTerrain;
                    //logger.WriteLine("Hex: {0:X}", b);
                    if (doWrite)
                    {
                        hyrule.ROMData.Put(loc, (Byte)b);
                        hyrule.ROMData.Put(loc + 1, (byte)currentTerrain);
                    }
                    currentTerrainCount = 0;
                    loc += 2;
                    bytesWritten += 2;
                    continue;
                }
                if (hyrule.Props.HiddenKasuto && y == 51 && x == 61 && y != 0 && x != 0 && (this.biome == Biome.VANILLA || this.biome == Biome.VANILLA_SHUFFLE))
                {
                    currentTerrainCount--;
                    int b = currentTerrainCount * 16 + (int)currentTerrain;
                    //logger.WriteLine("Hex: {0:X}", b);
                    if (doWrite)
                    {
                        hyrule.ROMData.Put(loc, (Byte)b);
                        hyrule.ROMData.Put(loc + 1, (byte)currentTerrain);
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
                        hyrule.ROMData.Put(loc, (Byte)b);
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
                hyrule.ROMData.Put(loc, (Byte)b2);
            }

            if (y < MAP_ROWS - 1)
            {
                currentTerrain = map[y + 1, 0];
            }
            currentTerrainCount = 0;
            loc++;
            bytesWritten++;
        }

        //Fill any remaining map space in the rom with filler. (Replace 0x0B with a constant)
        while (bytesWritten < total)
        {
            hyrule.ROMData.Put(loc, (Byte)0x0B);
            bytesWritten++;
            loc++;
        }
    }

    protected void DrawOcean(Direction direction)
    {
        Terrain water = Terrain.WATER;
        if (hyrule.Props.CanWalkOnWaterWithBoots)
        {
            water = Terrain.WALKABLEWATER;
        }
        int x;
        int y;
        int olength = 0;

        if(direction == Direction.WEST)
        {
            x = 0;
            y = hyrule.RNG.Next(MAP_ROWS);
            olength = hyrule.RNG.Next(Math.Max(y, MAP_ROWS - y));
        } 
        else if(direction == Direction.EAST)
        {
            x = MAP_COLS - 1;
            y = hyrule.RNG.Next(MAP_ROWS);
            olength = hyrule.RNG.Next(Math.Max(y, MAP_ROWS - y));
        }
        else if(direction == Direction.NORTH)
        {
            x = hyrule.RNG.Next(MAP_COLS);
            y = 0;
            olength = hyrule.RNG.Next(Math.Max(x, MAP_COLS - x));
        }
        else //south
        {
            x = hyrule.RNG.Next(MAP_COLS);
            y = MAP_ROWS - 1;
            olength = hyrule.RNG.Next(Math.Max(x, MAP_COLS - x));
        }
        //draw ocean on right side
        
        if (direction == Direction.EAST || direction == Direction.WEST)
        {
            if (y < MAP_ROWS / 2)
            {
                for (int i = 0; i < olength; i++)
                {
                    if(map[y + i, x] != Terrain.NONE && this.biome != Biome.MOUNTAINOUS)
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
                        if (map[y - i, x] != Terrain.NONE && this.biome != Biome.MOUNTAINOUS)
                        {
                            return;
                        }
                        map[y - i, x] = water;
                    }
                }
                catch(IndexOutOfRangeException e)
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
                    if (map[y, x + i] != Terrain.NONE && this.biome != Biome.MOUNTAINOUS)
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
                    if (map[y, x - 1] != Terrain.NONE && this.biome != Biome.MOUNTAINOUS)
                    {
                        return;
                    }
                    map[y, x - i] = water;
                }
            }
        }
    }
    protected void LegacyUpdateReachable()
    {
        bool needJump = false;
        Location location = GetLocationByMem(0x8646);
        int jumpBlockY = -1;
        int jumpBlockX = -1;
        if(location != null)
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
        bool hasFairySpell = hyrule.SpellGet[Spell.FAIRY];
        bool hasJumpSpell = hyrule.SpellGet[Spell.JUMP];
        bool hasHammer = hyrule.itemGet[Item.HAMMER];
        bool hasBoots = hyrule.itemGet[Item.BOOTS];
        bool hasFlute = hyrule.itemGet[Item.FLUTE];
        bool changed = true;
        while (changed)
        {
            changed = false;
            for (int y = 0; y < MAP_ROWS; y++)
            {
                for (int x = 0; x < MAP_COLS; x++)
                {
                    Terrain terrain = map[y, x];
                    if(location != null && location.TerrainType == Terrain.SWAMP)
                    {
                        needFairy = location.NeedFairy;
                    }
                    if (location != null && location.TerrainType == Terrain.DESERT)
                    {
                        needJump = location.NeedJump;
                    }
                    //If this isn't already marked as visited, and it is visitable:
                    if (!visitation[y, x]
                        //East desert jump blocker
                        && !(
                            needJump 
                            && jumpBlockY == y 
                            && jumpBlockX == x 
                            && (!hasJumpSpell && !hasFairySpell)
                        )
                        //Fairy cave is traversable
                        && !(needFairy && fairyBlockY == y && fairyBlockX == x && !hasFairySpell)
                        //This map tile is a traversable terrain type
                        && (
                            terrain == Terrain.LAVA 
                            || terrain == Terrain.BRIDGE 
                            || terrain == Terrain.CAVE 
                            || terrain == Terrain.ROAD 
                            || terrain == Terrain.PALACE 
                            || terrain == Terrain.TOWN 
                            || (terrain == Terrain.WALKABLEWATER && hasBoots)
                            || walkableTerrains.Contains(terrain) 
                            || (terrain == Terrain.ROCK && hasHammer) 
                            || (terrain == Terrain.SPIDER && hasFlute)
                        )
                    )
                    //If an adjacent tile is visited, this one must also be.
                    {
                        if (y - 1 >= 0)
                        {
                            if (visitation[y - 1, x])
                            {
                                visitation[y, x] = true;
                                changed = true;
                                continue;
                            }

                        }

                        if (y + 1 < MAP_ROWS)
                        {
                            if (visitation[y + 1, x])
                            {
                                visitation[y, x] = true;
                                changed = true;
                                continue;

                            }
                        }

                        if (x - 1 >= 0)
                        {
                            if (visitation[y, x - 1])
                            {
                                visitation[y, x] = true;
                                changed = true;
                                continue;

                            }
                        }

                        if (x + 1 < MAP_COLS)
                        {
                            if (visitation[y, x + 1])
                            {
                                visitation[y, x] = true;
                                changed = true;
                                continue;

                            }
                        }
                    }
                }
            }
        }
    }
    protected void UpdateReachable()
    {
        //Setup
        bool needJump = false;
        Location location = GetLocationByMem(0x8646);
        int jumpBlockY = -1;
        int jumpBlockX = -1;
        if (location != null)
        {
            needJump = location.NeedFairy;
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
            if(start.Ypos >= 30 && start.Xpos >= 0)
            {
                UpdateReachable(ref covered, start.Ypos - 30, start.Xpos, jumpBlockY, jumpBlockX, fairyBlockY, fairyBlockX, needJump, needFairy);
            }
        }

        /*
        StringBuilder sb = new();
        sb.AppendLine(GetName());
        for (int y = 0; y < MAP_ROWS; y++)
        {
            for (int x = 0; x < MAP_COLS; x++)
            {
                sb.Append(visitation[y,x] ? 'x' : ' ');
            }
            sb.Append('\n');
        }
        Debug.Write(sb.ToString());
        */
    }

    protected void UpdateReachable(ref bool[,] covered, int y, int x, int jumpBlockY, int jumpBlockX, int fairyBlockY, int fairyBlockX, bool needJump, bool needFairy)
    {
        try
        {
            if (covered[y, x])
            {
                return;
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
                || (terrain == Terrain.WALKABLEWATER && hyrule.itemGet[Item.BOOTS])
                || (terrain == Terrain.ROCK && hyrule.itemGet[Item.HAMMER])
                || (terrain == Terrain.SPIDER && hyrule.itemGet[Item.FLUTE]))
                //East desert jump blocker
                && !(
                    needJump
                    && jumpBlockY == y
                    && jumpBlockX == x
                    && (!hyrule.SpellGet[Spell.JUMP] && !hyrule.SpellGet[Spell.FAIRY])
                )
                //Fairy cave is traversable
                && !(needFairy && fairyBlockY == y && fairyBlockX == x && !hyrule.SpellGet[Spell.FAIRY])
            )
            {
                visitation[y, x] = true;

                if (y - 1 >= 0)
                {
                    UpdateReachable(ref covered, y - 1, x, jumpBlockY, jumpBlockX, fairyBlockY, fairyBlockX, needJump, needFairy);
                }

                if (y + 1 < MAP_ROWS)
                {
                    UpdateReachable(ref covered, y + 1, x, jumpBlockY, jumpBlockX, fairyBlockY, fairyBlockX, needJump, needFairy);
                }

                if (x - 1 >= 0)
                {
                    UpdateReachable(ref covered, y, x - 1, jumpBlockY, jumpBlockX, fairyBlockY, fairyBlockX, needJump, needFairy);
                }

                if (x + 1 < MAP_COLS)
                {
                    UpdateReachable(ref covered, y, x + 1, jumpBlockY, jumpBlockX, fairyBlockY, fairyBlockX, needJump, needFairy);
                }
            }
        }
        catch(IndexOutOfRangeException)
        {
            logger.Debug("?");
            throw;
        }
    }

    //Should the visibility calculation table even be persistent? Why is this not just in scope of the calculation itself?
    public void ResetVisitabilityState()
    {
        for(int i = 0; i < MAP_ROWS; i++)
        {
            for(int j = 0; j < MAP_COLS; j++)
            {
                visitation[i, j] = false;
            }
        }
        foreach(Location location in AllLocations)
        {
            location.CanShuffle = true;
            location.Reachable = false;
            location.itemGet = false;
        }
    }

    protected bool DrawRaft(bool bridge, Direction direction)
    {
        int raftx = 0;
        int deltax = 1;
        int deltay = 0;
        int rafty = hyrule.RNG.Next(0, MAP_ROWS);

        int tries = 0;
        int length = 0;

        do
        {
            length = 0;
            tries++;
            if(tries > 100)
            {
                return false;
            }
            if (direction == Direction.WEST)
            {
                raftx = 0;
                int rtries = 0;
                do
                {
                    rafty = hyrule.RNG.Next(0, MAP_ROWS);
                    rtries++;
                    if(rtries > 100)
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
                    raftx = hyrule.RNG.Next(0, MAP_COLS);
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
                    raftx = hyrule.RNG.Next(0, MAP_COLS);
                    rtries++;
                    if (rtries > 100)
                    {
                        return false;
                    }
                } while (map[rafty, raftx] != Terrain.WALKABLEWATER && map[rafty, raftx] != Terrain.WATER) ;
                deltax = 0;
                deltay = -1;
            }
            else
            {
                raftx = MAP_COLS - 1;
                int rtries = 0;

                do
                {
                    rafty = hyrule.RNG.Next(0, MAP_ROWS);
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
            else if(direction == Direction.WEST)
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

    private void LoadLocation(int addr, Terrain t, Continent c)
    {
        byte[] bytes = new Byte[4] { hyrule.ROMData.GetByte(addr), hyrule.ROMData.GetByte(addr + overworldXOffset), hyrule.ROMData.GetByte(addr + overworldMapOffset), hyrule.ROMData.GetByte(addr + overworldWorldOffset) };
        AddLocation(new Location(bytes, t, addr, c));
    }

    public Location LoadRaft(int world)
    {
        LoadLocation(baseAddr + 41, Terrain.BRIDGE, (Continent)world);
        raft = GetLocationByMem(baseAddr + 41);
        raft.ExternalWorld = 0x80;
        raft.World = world;
        raft.Map = 41;
        raft.TerrainType = Terrain.BRIDGE;
        return raft;
    }

    public Location LoadBridge(int world)
    {
        LoadLocation(baseAddr + 40, Terrain.BRIDGE, (Continent)world);
        bridge = GetLocationByMem(baseAddr + 40);
        bridge.ExternalWorld = 0x80;
        bridge.World = world;
        bridge.Map = 40;
        bridge.PassThrough = 0;
        return bridge;
    }

    public Location LoadCave1(int world)
    {
        LoadLocation(baseAddr + 42, Terrain.CAVE, (Continent)world);
        cave1 = GetLocationByMem(baseAddr + 42);
        cave1.ExternalWorld = 0x80;
        cave1.World = world;
        cave1.Map = 42;
        cave1.CanShuffle = true;
        return cave1;
    }

    public Location LoadCave2(int world)
    {
        LoadLocation(baseAddr + 43, Terrain.CAVE, (Continent)world);
        cave2 = GetLocationByMem(baseAddr + 43);
        cave2.ExternalWorld = 0x80;
        cave2.World = world;
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
        if(bridge != null)
        {
            visitation[bridge.Ypos - 30, bridge.Xpos] = true;
        }
    }

    public void VisitCave1()
    {
        if(cave1 != null)
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

    public void RemoveUnusedConnectors()
    {
        if(this.raft == null)
        {
            hyrule.ROMData.Put(baseAddr + 41, 0x00);
        }

        if(this.bridge == null)
        {
            hyrule.ROMData.Put(baseAddr + 40, 0x00);
        }

        if(this.cave1 == null)
        {
            hyrule.ROMData.Put(baseAddr + 42, 0x00);
        }

        if(this.cave2 == null)
        {
            hyrule.ROMData.Put(baseAddr + 43, 0x00);
        }
    }

    protected void DrawRiver(List<Location> bridges)
    {
        Terrain water = Terrain.WATER;
        if (hyrule.Props.CanWalkOnWaterWithBoots)
        {
            water = Terrain.WALKABLEWATER;
        }
        int dirr = hyrule.RNG.Next(4);
        int dirr2 = dirr;
        while (dirr == dirr2)
        {
            dirr2 = hyrule.RNG.Next(4);
        }

        int deltax = 0;
        int deltay = 0;
        int startx = 0;
        int starty = 0;
        if(dirr == 0) //north
        {
            deltay = 1;
            startx = hyrule.RNG.Next(MAP_COLS / 3, (MAP_COLS / 3) * 2);
            starty = 0;
        }
        else if(dirr == 1) //east
        {
            deltax = -1;
            startx = MAP_COLS - 1;
            starty = hyrule.RNG.Next(MAP_ROWS / 3, (MAP_ROWS / 3) * 2);
        }
        else if(dirr == 2) //south
        {
            deltay = -1;
            startx = hyrule.RNG.Next(MAP_COLS / 3, (MAP_COLS / 3) * 2);
            starty = MAP_ROWS - 1;
        }
        else //west
        {
            deltax = 1;
            startx = 0;
            starty = hyrule.RNG.Next(MAP_ROWS / 3, (MAP_ROWS / 3) * 2);
        }

        int stopping = hyrule.RNG.Next(MAP_COLS / 3, (MAP_COLS / 3) * 2);
        if(deltay != 0)
        {
            stopping = hyrule.RNG.Next(MAP_ROWS / 3, (MAP_ROWS / 3) * 2);
        }
        int curr = 0;
        while(curr < stopping)
        {
            
            if(map[starty, startx] == Terrain.NONE)
            {
                map[starty, startx] = water;
                int adjust = hyrule.RNG.Next(-1, 2);
                if ((deltax == 0 && startx == 1) || (deltay == 0 && starty == 1))
                {
                    adjust = hyrule.RNG.Next(0, 2);
                }
                else if((deltax == 0 && startx == MAP_COLS - 2) || (deltay == 0 && starty == MAP_ROWS - 2))
                {
                    adjust = hyrule.RNG.Next(-1, 1);
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
                else if(adjust > 0)
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
        while(startx > 0 && startx < MAP_COLS && starty > 0 && starty < MAP_ROWS)
        {

            if (map[starty, startx] == Terrain.NONE)
            {
                map[starty, startx] = water;
                int adjust = hyrule.RNG.Next(-1, 2);
                if ((deltax == 0 && startx == 1) || (deltay == 0 && starty == 1))
                {
                    adjust = hyrule.RNG.Next(0, 2);
                }
                else if ((deltax == 0 && startx == MAP_COLS - 2) || (deltay == 0 && starty == MAP_ROWS - 2))
                {
                    adjust = hyrule.RNG.Next(-1, 1);
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
                else if(adjust > 0)
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
        int drawLeft = hyrule.RNG.Next(0, 5);
        int drawRight = hyrule.RNG.Next(0, 5);
        Terrain tleft = walkableTerrains[hyrule.RNG.Next(walkableTerrains.Count)];
        Terrain tright = walkableTerrains[hyrule.RNG.Next(walkableTerrains.Count)];
        if (!isHorizontal)
        {
            int riverx = hyrule.RNG.Next(15, MAP_COLS - 15);
            for (int y = 0; y < MAP_ROWS; y++)
            {
                drawLeft++;
                drawRight++;
                map[y, riverx] = riverT;
                map[y, riverx + 1] = riverT;
                int adjust = hyrule.RNG.Next(-3, 4);
                int leftM = hyrule.RNG.Next(14, 17);
                if (riverx - leftM > 0)
                {
                    map[y, riverx - leftM + 3] = tleft;
                }
                if (drawLeft % 5 == 0)
                {
                    tleft = walkableTerrains[hyrule.RNG.Next(walkableTerrains.Count)];
                }
                for (int i = riverx - leftM; i >= 0; i--)
                {
                    map[y, i] = Terrain.MOUNTAIN;
                }

                int rightM = hyrule.RNG.Next(14, 17);
                
                if (riverx + rightM < MAP_COLS)
                {
                    map[y, riverx + rightM - 3] = tright;
                }
                
                if (drawRight % 5 == 0)
                {
                    tright = walkableTerrains[hyrule.RNG.Next(walkableTerrains.Count)];
                }
                for (int i = riverx + 1 + rightM; i < MAP_COLS; i++)
                {
                    map[y, i] = Terrain.MOUNTAIN;
                }
                while (riverx + adjust + 1 > MAP_COLS - 15 || riverx + adjust < 15)
                {
                    adjust = hyrule.RNG.Next(-1, 2);
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
        else
        {
            int rivery = hyrule.RNG.Next(15, MAP_ROWS - 15);
            for (int x = 0; x < MAP_COLS; x++)
            {
                drawLeft++;
                drawRight++;
                map[rivery, x] = riverT;
                map[rivery + 1, x] = riverT;
                int adjust = hyrule.RNG.Next(-3, 3);
                int leftM = hyrule.RNG.Next(14, 17);
                if (rivery - leftM > 0)
                {
                    map[rivery - leftM + 3, x] = tleft;
                }
                if (drawLeft % 5 == 0)
                {
                    tleft = walkableTerrains[hyrule.RNG.Next(walkableTerrains.Count)];
                }
                for (int i = rivery - leftM; i >= 0; i--)
                {
                    map[i, x] = Terrain.MOUNTAIN;
                }

                int rightM = hyrule.RNG.Next(14, 17);
                
                if (rivery + rightM < MAP_ROWS)
                {
                    map[rivery + rightM - 3, x] = tright;
                }
                
                if (drawRight % 5 == 0)
                {
                    tright = walkableTerrains[hyrule.RNG.Next(walkableTerrains.Count)];
                }
                for (int i = rivery + 1 + rightM; i < MAP_ROWS; i++)
                {
                    map[i, x] = Terrain.MOUNTAIN;
                }
                while (rivery + adjust + 1 > MAP_ROWS - 15 || rivery + adjust < 15)
                {
                    adjust = hyrule.RNG.Next(-1, 2);
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
    }

    public void DrawCenterMountain()
    {
        isHorizontal = hyrule.RNG.NextDouble() > 0.5;
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

    protected bool HorizontalCave(int caveType, int centerx, int centery, Location cave1l, Location cave1r)
    {
        if (caveType == 0) //first cave left
        {
            int cavey = hyrule.RNG.Next(centery - 2, centery + 3);
            int cavex = centerx;
            while (map[cavey, cavex] != Terrain.MOUNTAIN)
            {
                cavex--;
            }
            if (map[cavey + 1, cavex] != Terrain.MOUNTAIN)
            {
                cavey--;
            }
            else if(map[cavey - 1, cavex] != Terrain.MOUNTAIN)
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
            int cavey = hyrule.RNG.Next(centery - 2, centery + 3);
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
            int cavex = hyrule.RNG.Next(centerx - 2, centerx + 3);
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
            int cavey = centery;
            int cavex = hyrule.RNG.Next(centerx - 2, centerx + 3);
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
            while (cavey < MAP_ROWS && map[cavey, cavex] == Terrain.MOUNTAIN )
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
}
