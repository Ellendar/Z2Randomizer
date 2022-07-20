using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections;
using System.Windows.Forms;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.WindowsRuntime;

namespace Z2Randomizer
{
    abstract class World
    {
        protected enum biome { vanilla, vanillaShuffle, vanillalike, islands, canyon, mountainous, volcano, caldera };
        protected enum direction { north, south, east, west };
        protected SortedDictionary<String, List<Location>> areasByLocation;
        private List<Location> caves;
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
        private List<Location>[] locations;
        private List<Location> allLocations;
        public Dictionary<Location, Location> connections;
        protected HashSet<String> reachableAreas;
        protected int enemyAddr;
        protected List<int> enemies;
        protected List<int> flyingEnemies;
        protected List<int> generators;
        protected List<int> shorties;
        protected List<int> tallGuys;
        protected int enemyPtr;
        protected List<int> overworldMaps;
        protected SortedDictionary<Tuple<int, int>, Location> locsByCoords;
        protected Hyrule hy;
        protected terrain[,] map;
        private const int overworldXOff = 0x3F;
        private const int overworldMapOff = 0x7E;
        private const int overworldWorldOff = 0xBD;
        private List<int> visitedEnemies;
        protected int MAP_ROWS;
        protected int MAP_COLS;
        protected int bcount;
        protected List<terrain> randomTerrains;
        protected List<terrain> walkable;
        protected bool[,] v;
        protected const int MAP_SIZE_BYTES = 1408;
        protected List<Location> unimportantLocs;
        protected biome bio;
        protected bool horizontal;
        protected int VANILLA_MAP_ADDR;
        protected SortedDictionary<Tuple<int, int>, string> section;
        public Location raft;
        public Location bridge;
        public Location cave1;
        public Location cave2;
        private Boolean allreached;

        protected int baseAddr;


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

        public List<Location> AllLocations
        {
            get
            {
                return allLocations;
            }
        }

        public List<Location> Palaces
        {
            get
            {
                return palaces;
            }

            set
            {
                palaces = value;
            }
        }

        public List<Location> Towns
        {
            get
            {
                return towns;
            }

            set
            {
                towns = value;
            }
        }

        internal List<Location> Caves
        {
            get
            {
                return caves;
            }

            set
            {
                caves = value;
            }
        }

        internal List<Location> Grasses
        {
            get
            {
                return grasses;
            }

            set
            {
                grasses = value;
            }
        }

        internal List<Location> Swamps
        {
            get
            {
                return swamps;
            }

            set
            {
                swamps = value;
            }
        }

        internal List<Location> Bridges
        {
            get
            {
                return bridges;
            }

            set
            {
                bridges = value;
            }
        }

        internal List<Location> Deserts
        {
            get
            {
                return deserts;
            }

            set
            {
                deserts = value;
            }
        }

        internal List<Location> Forests
        {
            get
            {
                return forests;
            }

            set
            {
                forests = value;
            }
        }

        internal List<Location> Graves
        {
            get
            {
                return graves;
            }

            set
            {
                graves = value;
            }
        }

        internal List<Location> Lavas
        {
            get
            {
                return lavas;
            }

            set
            {
                lavas = value;
            }
        }

        internal List<Location> Roads
        {
            get
            {
                return roads;
            }

            set
            {
                roads = value;
            }
        }

        public bool Allreached { get => allreached; set => allreached = value; }

        public World(Hyrule parent)
        {
            hy = parent;
            caves = new List<Location>();
            towns = new List<Location>();
            palaces = new List<Location>();
            grasses = new List<Location>();
            swamps = new List<Location>();
            bridges = new List<Location>();
            deserts = new List<Location>();
            forests = new List<Location>();
            graves = new List<Location>();
            lavas = new List<Location>();
            roads = new List<Location>();
            connections = new Dictionary<Location, Location>();
            locations = new List<Location>[11] { towns, caves, palaces, bridges, deserts, grasses, forests, swamps, graves, roads, lavas };
            allLocations = new List<Location>();
            locsByCoords = new SortedDictionary<Tuple<int, int>, Location>();
            reachableAreas = new HashSet<string>();
            visitedEnemies = new List<int>();
            unimportantLocs = new List<Location>();
            areasByLocation = new SortedDictionary<string, List<Location>>();
            allreached = false;
        }

        public void addLocation(Location l)
        {
            if (l.TerrainType == terrain.walkablewater)
            {
                locations[(int)terrain.swamp].Add(l);
            }
            else
            {
                locations[(int)l.TerrainType].Add(l);
            }
            allLocations.Add(l);
            //locsByCoords.Add(l.Coords, l);
        }

        protected void shuffleLocations(List<Location> westLocs)
        {
            for (int i = 0; i < westLocs.Count; i++)
            {

                int s = hy.R.Next(i, westLocs.Count);
                Location sl = westLocs[s];
                if (sl.CanShuffle && westLocs[i].CanShuffle)
                {
                    swap(westLocs[i], westLocs[s]);
                }
            }
        }


        protected void swap(Location l1, Location l2)
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

        public void shuffleEnemies(int addr, bool isOver)
        {
            if (isOver)
            {
                addr = addr + hy.ROMData.getByte(addr);
            }
            if (!visitedEnemies.Contains(addr) && addr != 0x95A4)
            {
                int numBytes = hy.ROMData.getByte(addr);
                for (int j = addr + 2; j < addr + numBytes; j = j + 2)
                {
                    int enemy = hy.ROMData.getByte(j) & 0x3F;
                    int highPart = hy.ROMData.getByte(j) & 0xC0;
                    if (hy.Props.mixEnemies)
                    {
                        if (enemies.Contains(enemy))
                        {
                            int swap = enemies[hy.R.Next(0, enemies.Count)];
                            hy.ROMData.put(j, (Byte)(swap + highPart));
                            if ((shorties.Contains(enemy) && tallGuys.Contains(swap) && swap != 0x20))
                            {
                                int ypos = hy.ROMData.getByte(j - 1) & 0xF0;
                                int xpos = hy.ROMData.getByte(j - 1) & 0x0F;
                                ypos = ypos - 32;
                                hy.ROMData.put(j - 1, (Byte)(ypos + xpos));
                            }
                            else if (swap == 0x20 && swap != enemy)
                            {
                                int ypos = hy.ROMData.getByte(j - 1) & 0xF0;
                                int xpos = hy.ROMData.getByte(j - 1) & 0x0F;
                                ypos = ypos - 48;
                                hy.ROMData.put(j - 1, (Byte)(ypos + xpos));
                            }
                            else if (enemy == 0x1F && swap != enemy)
                            {
                                int ypos = hy.ROMData.getByte(j - 1) & 0xF0;
                                int xpos = hy.ROMData.getByte(j - 1) & 0x0F;
                                ypos = ypos - 16;
                                hy.ROMData.put(j - 1, (Byte)(ypos + xpos));
                            }
                        }
                    }
                    else
                    {

                        if (tallGuys.Contains(enemy))
                        {
                            int swap = hy.R.Next(0, tallGuys.Count);
                            if (tallGuys[swap] == 0x20 && tallGuys[swap] != enemy)
                            {
                                int ypos = hy.ROMData.getByte(j - 1) & 0xF0;
                                int xpos = hy.ROMData.getByte(j - 1) & 0x0F;
                                ypos = ypos - 48;
                                hy.ROMData.put(j - 1, (Byte)(ypos + xpos));
                            }
                            hy.ROMData.put(j, (Byte)(tallGuys[swap] + highPart));
                        }

                        if (shorties.Contains(enemy))
                        {
                            int swap = hy.R.Next(0, shorties.Count);
                            hy.ROMData.put(j, (Byte)(shorties[swap] + highPart));
                        }
                    }

                    if (flyingEnemies.Contains(enemy))
                    {
                        int swap = hy.R.Next(0, flyingEnemies.Count);
                        hy.ROMData.put(j, (Byte)(flyingEnemies[swap] + highPart));

                        if (flyingEnemies[swap] == 0x07 || flyingEnemies[swap] == 0x0a || flyingEnemies[swap] == 0x0d || flyingEnemies[swap] == 0x0e)
                        {
                            int ypos = 0x00;
                            int xpos = hy.ROMData.getByte(j - 1) & 0x0F;
                            hy.ROMData.put(j - 1, (Byte)(ypos + xpos));
                        }
                    }

                    if (generators.Contains(enemy))
                    {
                        int swap = hy.R.Next(0, generators.Count);
                        hy.ROMData.put(j, (Byte)(generators[swap] + highPart));
                    }

                    if (enemy == 33)
                    {
                        int swap = hy.R.Next(0, generators.Count + 1);
                        if (swap != generators.Count)
                        {
                            hy.ROMData.put(j, (Byte)(generators[swap] + highPart));
                        }
                    }
                }
                visitedEnemies.Add(addr);
            }
        }

        protected void chooseConn(String section, Dictionary<Location, Location> co, bool changeType)
        {
            if (co.Count > 0)
            {
                int start = hy.R.Next(areasByLocation[section].Count);
                Location s = areasByLocation[section][start];
                int conn = hy.R.Next(co.Count);
                Location c = co.Keys.ElementAt(conn);
                int count = 0;
                while ((!c.CanShuffle || !s.CanShuffle || (!changeType && (c.TerrainType != s.TerrainType))) && count < co.Count)
                {
                    start = hy.R.Next(areasByLocation[section].Count);
                    s = areasByLocation[section][start];
                    conn = hy.R.Next(co.Count);
                    c = co.Keys.ElementAt(conn);
                    count++;
                }
                swap(s, c);
                c.CanShuffle = false;
            }
        }

        protected void loadLocations(int startAddr, int locNum, SortedDictionary<int, terrain> terrains, continent c)
        {
            for (int i = 0; i < locNum; i++)
            {
                Byte[] bytes = new Byte[4] { hy.ROMData.getByte(startAddr + i), hy.ROMData.getByte(startAddr + overworldXOff + i), hy.ROMData.getByte(startAddr + overworldMapOff + i), hy.ROMData.getByte(startAddr + overworldWorldOff + i) };
                addLocation(new Location(bytes, terrains[startAddr + i], startAddr + i, c));
            }
        }

        protected Location getLocationByMap(int map, int world)
        {
            Location l = null;
            foreach (Location loc in allLocations)
            {
                if (loc.LocationBytes[2] == map && loc.World == world)
                {
                    l = loc;
                    break;
                }
            }
            return l;
        }

        protected void readVanillaMap()
        {
            int addr = VANILLA_MAP_ADDR;
            int i = 0;
            int j = 0;
            map = new terrain[MAP_ROWS, MAP_COLS];
            while(i < MAP_ROWS)
            {
                j = 0;
                while(j < MAP_COLS)
                {
                    byte data = hy.ROMData.getByte(addr);
                    int count = (data & 0xF0) >> 4;
                    count++;
                    terrain t = (terrain)(data & 0x0F);
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

        public void allReachable()
        {
            if(allreached)
            {
                return;
            }
            else
            {
                allreached = true;
                foreach(Location l in AllLocations)
                {
                    if(l.TerrainType == terrain.palace || l.TerrainType == terrain.town || l.item != items.donotuse)
                    {
                        if(!l.Reachable)
                        {
                            allreached = false;
                        }
                    }
                }
            }
        }

        protected Location getLocationByCoords(Tuple<int, int> coords)
        {
            Location l = null;
            foreach (Location loc in allLocations)
            {
                if (loc.Coords.Equals(coords))
                {
                    l = loc;
                    break;
                }
            }
            if (l == null)
            {
                //Console.Write(coords);
            }
            return l;
        }

        protected Location getLocationByMem(int mem)
        {
            Location l = null;
            foreach (Location loc in allLocations)
            {
                if (loc.MemAddress == mem)
                {
                    l = loc;
                    break;
                }
            }
            return l;
        }

        public void shuffleE()
        {
            for (int i = enemyPtr; i < enemyPtr + 126; i = i + 2)
            {
                int low = hy.ROMData.getByte(i);
                int high = hy.ROMData.getByte(i + 1);
                high = high << 8;
                high = high & 0x0FFF;
                int addr = high + low + enemyAddr;
                shuffleEnemies(high + low + enemyAddr, false);
            }

            foreach (int i in overworldMaps)
            {
                int ptrAddr = enemyPtr + i * 2;
                int low = hy.ROMData.getByte(ptrAddr);
                int high = hy.ROMData.getByte(ptrAddr + 1);
                high = high << 8;
                high = high & 0x0FFF;
                int addr = high + low + enemyAddr;
                shuffleEnemies(high + low + enemyAddr, true);
            }
        }

        protected Boolean placeLocations(terrain riverT)
        {
            foreach (Location l in AllLocations)
            {
                if ((l.TerrainType != terrain.bridge && l.CanShuffle && !unimportantLocs.Contains(l) && l.PassThrough == 0) || l.Needhammer)
                {
                    int x = 0;
                    int y = 0;
                    do
                    {
                        x = hy.R.Next(MAP_COLS - 2) + 1;
                        y = hy.R.Next(MAP_ROWS - 2) + 1;
                    } while (map[y, x] != terrain.none || map[y - 1, x] != terrain.none || map[y + 1, x] != terrain.none || map[y + 1, x + 1] != terrain.none || map[y, x + 1] != terrain.none || map[y - 1, x + 1] != terrain.none || map[y + 1, x - 1] != terrain.none || map[y, x - 1] != terrain.none || map[y - 1, x - 1] != terrain.none);

                    map[y, x] = l.TerrainType;
                    if (l.TerrainType == terrain.cave)
                    {
                        int dir = hy.R.Next(4);
                        terrain s = walkable[hy.R.Next(walkable.Count)];
                        if (!hy.Props.saneCaves || !connections.ContainsKey(l))
                        {
                            placeCave(x, y, dir, s);
                        }
                        else
                        {
                            if((l.HorizontalPos == 0 || l.FallInHole != 0) && l.ForceEnterRight == 0)
                            {
                                if(dir == 0)
                                {
                                    dir = 1;
                                }

                                if(dir == 3)
                                {
                                    dir = 2;
                                }
                            }
                            else
                            {
                                if (dir == 1)
                                {
                                    dir = 0;
                                }

                                if (dir == 2)
                                {
                                    dir = 3;
                                }
                            }
                            map[y, x] = terrain.none;
                            do
                            {
                                x = hy.R.Next(MAP_COLS - 2) + 1;
                                y = hy.R.Next(MAP_ROWS - 2) + 1;
                            } while (x < 5 || y < 5 || x > MAP_COLS - 5 || y > MAP_ROWS - 5 || map[y, x] != terrain.none || map[y - 1, x] != terrain.none || map[y + 1, x] != terrain.none || map[y + 1, x + 1] != terrain.none || map[y, x + 1] != terrain.none || map[y - 1, x + 1] != terrain.none || map[y + 1, x - 1] != terrain.none || map[y, x - 1] != terrain.none || map[y - 1, x - 1] != terrain.none);

                            while ((dir == 0 && y < 15) || (dir == 1 && x > MAP_COLS - 15) || (dir == 2 && y > MAP_ROWS - 15) || (dir == 3 && x < 15))
                            {
                                dir = hy.R.Next(4);
                            }
                            int otherdir = (dir + 2) % 4;
                            int otherx = 0;
                            int othery = 0;
                            int tries = 0;
                            Boolean crossing = true;
                            do
                            {
                                int range = 12;
                                int offset = 6;
                                if(bio == biome.islands)
                                {
                                    range = 10;
                                    offset = 10;
                                }
                                else if(bio == biome.volcano)
                                {
                                    range = 10;
                                    offset = 20;
                                }
                                crossing = true;
                                if(dir == 0) //south
                                {
                                    otherx = x + (hy.R.Next(7) - 3);
                                    othery = y - (hy.R.Next(range) + offset);
                                } 
                                else if(dir == 1) //west
                                {
                                    otherx = x + (hy.R.Next(range) + offset);
                                    othery = y + (hy.R.Next(7) - 3);
                                }
                                else if(dir == 2) //north
                                {
                                    otherx = x + (hy.R.Next(7) - 3);
                                    othery = y + (hy.R.Next(range) + offset);
                                }
                                else //east
                                {
                                    otherx = x - (hy.R.Next(range) + offset);
                                    othery = y + (hy.R.Next(7) - 3);
                                }
                                tries++;
                                if(this.bio != biome.volcano)
                                {
                                    if(!crossingWater(x, otherx, y, othery, riverT))
                                    {
                                        crossing = false;
                                    }
                                }
                                if (tries >= 100)
                                {
                                    return false;
                                }
                            } while (!crossing || otherx <= 1 || otherx >= MAP_COLS - 1 || othery <= 1 || othery >= MAP_ROWS - 1 || map[othery, otherx] != terrain.none || map[othery - 1, otherx] != terrain.none || map[othery + 1, otherx] != terrain.none || map[othery + 1, otherx + 1] != terrain.none || map[othery, otherx + 1] != terrain.none || map[othery - 1, otherx + 1] != terrain.none || map[othery + 1, otherx - 1] != terrain.none || map[othery, otherx - 1] != terrain.none || map[othery - 1, otherx - 1] != terrain.none);
                            
                            Location l2 = connections[l];
                            l.CanShuffle = false;
                            l2.CanShuffle = false;
                            l2.Xpos = otherx;
                            l2.Ypos = othery + 30;
                            placeCave(x, y, dir, walkable[hy.R.Next(walkable.Count)]);
                            placeCave(otherx, othery, otherdir, walkable[hy.R.Next(walkable.Count)]);
                        }
                    }
                    else if (l.TerrainType == terrain.palace)
                    {
                        terrain s = walkable[hy.R.Next(walkable.Count)];
                        map[y + 1, x] = s;
                        map[y + 1, x + 1] = s;
                        map[y + 1, x - 1] = s;
                        map[y, x - 1] = s;
                        map[y, x + 1] = s;
                        map[y - 1, x - 1] = s;
                        map[y - 1, x] = s;
                        map[y - 1, x + 1] = s;
                    }
                    else if(l.TerrainType != terrain.town || l.townNum != 9) //don't place newkasuto2
                    {
                        terrain t = terrain.none;
                        do
                        {
                            t = walkable[hy.R.Next(walkable.Count)];
                        } while (t == l.TerrainType);
                        map[y + 1, x] = t;
                        map[y + 1, x + 1] = t;
                        map[y + 1, x - 1] = t;
                        map[y, x - 1] = t;
                        map[y, x + 1] = t;
                        map[y - 1, x - 1] = t;
                        map[y - 1, x] = t;
                        map[y - 1, x + 1] = t;
                    }
                    l.Xpos = x;
                    l.Ypos = y + 30;
                    l.CanShuffle = false;
                }
            }
            return true;
        }

        protected bool crossingWater(int x, int otherx, int y, int othery, terrain riverT)
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

        protected void placeCave(int x, int y, int dir, terrain s)
        {
            map[y, x] = terrain.cave;
            if (dir == 0) //south
            {
                map[y + 1, x] = s;
                map[y + 1, x + 1] = s;
                map[y + 1, x - 1] = s;
                map[y, x - 1] = terrain.mountain;
                map[y, x + 1] = terrain.mountain;
                map[y - 1, x - 1] = terrain.mountain;
                map[y - 1, x] = terrain.mountain;
                map[y - 1, x + 1] = terrain.mountain;
            }
            else if (dir == 1) //west
            {
                map[y + 1, x] = terrain.mountain;
                map[y + 1, x + 1] = terrain.mountain;
                map[y + 1, x - 1] = s;
                map[y, x - 1] = s;
                map[y, x + 1] = terrain.mountain;
                map[y - 1, x - 1] = s;
                map[y - 1, x] = terrain.mountain;
                map[y - 1, x + 1] = terrain.mountain;
            }
            else if (dir == 2) //north
            {
                map[y + 1, x] = terrain.mountain;
                map[y + 1, x + 1] = terrain.mountain;
                map[y + 1, x - 1] = terrain.mountain;
                map[y, x - 1] = terrain.mountain;
                map[y, x + 1] = terrain.mountain;
                map[y - 1, x - 1] = s;
                map[y - 1, x] = s;
                map[y - 1, x + 1] = s;
            }
            else if (dir == 3) //east
            {
                map[y + 1, x] = terrain.mountain;
                map[y + 1, x + 1] = s;
                map[y + 1, x - 1] = terrain.mountain;
                map[y, x - 1] = terrain.mountain;
                map[y, x + 1] = s;
                map[y - 1, x - 1] = terrain.mountain;
                map[y - 1, x] = terrain.mountain;
                map[y - 1, x + 1] = s;
            }
        }

        public void placeHiddenLocations()
        {
            foreach(Location l in unimportantLocs)
            {
                if (l.CanShuffle)
                {
                    int tries = 0;
                    int x = 0;
                    int y = 0;
                    do
                    {
                        x = hy.R.Next(MAP_COLS);
                        y = hy.R.Next(MAP_ROWS);
                        tries++;
                    } while ((map[y, x] != l.TerrainType || getLocationByCoords(Tuple.Create(y + 30, x)) != null) && tries < 2000);

                    if (tries < 2000)
                    {
                        l.Xpos = x;
                        l.Ypos = y + 30;
                    }
                    else
                    {
                        l.Xpos = 0;
                        l.Ypos = 0;
                    }
                    l.CanShuffle = false;
                }
            }
        }

        protected bool connectIslands(int numBridges, Boolean placeTown, terrain riverT, bool riverDevil, bool placeLongBridge, bool placeDarunia)
        {
            int[,] mass = makeBlobs();
            Dictionary<int, List<int>> connectMass = new Dictionary<int, List<int>>();
            int bridges = numBridges;
            int terrainCycle = 0;
            int tries = 0;
            while (bridges > 0 && tries < 2000)
            {
                tries++;
                int x = hy.R.Next(MAP_COLS - 2) + 1;
                int y = hy.R.Next(MAP_ROWS - 2) + 1;
                int waterDir = nextToWater(x, y, riverT);
                int waterTries = 0;
                while (waterDir == -1 && waterTries < 2000)// || (this.bio == biome.canyon && (waterDir == 0 || waterDir == 1)))
                {
                    x = hy.R.Next(MAP_COLS - 2) + 1;
                    y = hy.R.Next(MAP_ROWS - 2) + 1;
                    waterDir = nextToWater(x, y, riverT);
                    waterTries++;
                }
                if(waterTries >= 2000)
                {
                    return false;
                }
                int deltaX = 0;
                int deltaY = 0;
                int length = 0;
                if(singleTile(y, x))
                {
                    length = 100;
                }

                int startMass = mass[y, x];

                if (waterDir == 0)
                {
                    deltaY = 1;
                }
                else if (waterDir == 1)
                {
                    deltaY = -1;
                }
                else if (waterDir == 2)
                {
                    deltaX = 1;
                }
                else if (waterDir == 3)
                {
                    deltaX = -1;
                }
               
                if(getLocationByCoords(Tuple.Create(y + 30, x)) != null || getLocationByCoords(Tuple.Create(y + 30, x + 1)) != null || getLocationByCoords(Tuple.Create(y + 30, x - 1)) != null || getLocationByCoords(Tuple.Create(y + 31, x)) != null || getLocationByCoords(Tuple.Create(y + 29, x)) != null)
                {
                    length = 100;
                }

                x += deltaX;
                y += deltaY;
                
                while (x > 0 && x < MAP_COLS && y > 0 && y < MAP_ROWS && map[y, x] == riverT)
                {
                    if(x + 1 < MAP_COLS && getLocationByCoords(Tuple.Create(y + 30, x + 1)) != null)
                    {
                        length = 100;
                    }
                    if (x - 1 > 0 && getLocationByCoords(Tuple.Create(y + 30, x - 1)) != null)
                    {
                        length = 100;
                    }

                    if (y + 1 < MAP_ROWS && getLocationByCoords(Tuple.Create(y + 31, x)) != null)
                    {
                        length = 100;
                    }
                    if (y - 1 > 0 && getLocationByCoords(Tuple.Create(y + 29, x)) != null)
                    {
                        length = 100;
                    }
                    int water = 0;
                    if (x + 1 < MAP_COLS && (map[y, x + 1] == riverT || map[y, x + 1] == terrain.mountain))
                    {
                        water++;
                    }

                    if (x - 1 >= 0 && (map[y, x - 1] == riverT || map[y, x - 1] == terrain.mountain))
                    {
                        water++;
                    }

                    if (y + 1 < MAP_ROWS && (map[y + 1, x] == riverT || map[y + 1, x] == terrain.mountain))
                    {
                        water++;
                    }

                    if (y - 1 >= 0 && (map[y - 1, x] == riverT || map[y - 1, x] == terrain.mountain))
                    {
                        water++;
                    }
                    if(deltaX != 0)
                    {
                        if(y - 1 > 0)
                        {
                            if(map[y - 1, x] != riverT && map[y - 1, x] != terrain.mountain)
                            {
                                length = 100;
                            }
                        }
                        if (y + 1 < MAP_ROWS)
                        {
                            if (map[y + 1, x] != riverT && map[y + 1, x] != terrain.mountain)
                            {
                                length = 100;
                            }
                        }
                    }

                    if (deltaY != 0)
                    {
                        if (x - 1 > 0)
                        {
                            if (map[y, x - 1] != riverT && map[y, x - 1] != terrain.mountain)
                            {
                                length = 100;
                            }
                        }
                        if (x + 1 < MAP_ROWS)
                        {
                            if (map[y, x + 1] != riverT && map[y, x + 1] != terrain.mountain)
                            {
                                length = 100;
                            }
                        }
                    }

                    if (water <= 2)
                    {
                        length = 100;
                    }
                    x += deltaX;
                    y += deltaY;
                    length++;
                    
                }
                if (singleTile(y, x))
                {
                    length = 100;
                }
                
                int endMass = 0;
                if(y > 0 && x > 0 && y < MAP_ROWS - 1 && x < MAP_COLS - 1)
                {
                    if (getLocationByCoords(Tuple.Create(y + 30, x)) != null || getLocationByCoords(Tuple.Create(y + 30, x + 1)) != null || getLocationByCoords(Tuple.Create(y + 30, x - 1)) != null || getLocationByCoords(Tuple.Create(y + 31, x)) != null || getLocationByCoords(Tuple.Create(y + 29, x)) != null)
                    {
                        length = 100;
                    }
                    endMass = mass[y, x];
                }
                
                if((riverT != terrain.desert && this.bio != biome.caldera && this.bio != biome.volcano) && (startMass == 0 || endMass == 0 || endMass == startMass || (connectMass.ContainsKey(startMass) && connectMass[startMass].Contains(endMass))))
                {
                    length = 100;
                }
                if (((placeTown && length < 10) || (length < 10 && length > 2)) && x > 0 && x < MAP_COLS - 1 && y > 0 && y < MAP_ROWS - 1 && walkable.Contains(map[y, x]) && map[y, x] != riverT)
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

                    terrain t = map[y, x];


                    if (placeTown)
                    {
                        
                        map[y, x] = terrain.town;
                        Location l = getLocationByMem(0x465F);
                        l.Ypos = y + 30;
                        l.Xpos = x;
                        x -= deltaX;
                        y -= deltaY;
                        while (map[y, x] == riverT)
                        {
                            x -= deltaX;
                            y -= deltaY;
                            //if(map[y + deltaY, x + deltaX] != terrain.town && riverT != terrain.walkablewater)
                            //{
                            //    map[y + deltaY, x + deltaX] = terrain.walkablewater;
                            //}
                        }
                        map[y, x] = terrain.town;
                        l = getLocationByMem(0x4660);
                        l.Ypos = y + 30;
                        l.Xpos = x;
                        placeTown = false;
                    }
                    else if(placeLongBridge)
                    {
                        Location bridge1 = getLocationByMap(0x04, 0);
                        Location bridge2 = getLocationByMap(0xC5, 0);
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
                        
                        while (map[y, x] == riverT)
                        {
                            map[y, x] = terrain.bridge;
                            x -= deltaX;
                            y -= deltaY;
                            //if(map[y + deltaY, x + deltaX] != terrain.town && riverT != terrain.walkablewater)
                            //{
                            //    map[y + deltaY, x + deltaX] = terrain.walkablewater;
                            //}
                            
                        }
                        x += deltaX;
                        y += deltaY;
                        map[y, x] = terrain.bridge;
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
                        Location bridge1 = getLocationByMem(0x8638);
                        Location bridge2 = getLocationByMem(0x8637);
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

                            while (map[y, x] == riverT)
                            {
                                map[y, x] = terrain.desert;
                                x -= deltaX;
                                y -= deltaY;
                                //if(map[y + deltaY, x + deltaX] != terrain.town && riverT != terrain.walkablewater)
                                //{
                                //    map[y + deltaY, x + deltaX] = terrain.walkablewater;
                                //}

                            }
                            x += deltaX;
                            y += deltaY;
                            map[y, x] = terrain.desert;
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
                        if (terrainCycle == 2)
                        {
                            
                            x += deltaX;
                            y += deltaY;
                            map[y, x] = terrain.road;
                            x -= deltaX;
                            y -= deltaY;
                            while (map[y, x] == riverT)
                            {
                                map[y, x] = terrain.walkablewater;
                                x -= deltaX;
                                y -= deltaY;

                                //if(map[y + deltaY, x + deltaX] != terrain.town && riverT != terrain.walkablewater)
                                //{
                                //    map[y + deltaY, x + deltaX] = terrain.walkablewater;
                                //}
                            }
                            map[y, x] = terrain.road;
                            
                        }
                        else {
                            while (map[y, x] == riverT)
                            {

                                if (this.bio != biome.mountainous && this.bio != biome.vanillalike)
                                {
                                    if (terrainCycle == 0)
                                    {
                                        map[y, x] = terrain.road;

                                        if (curr == length / 2)
                                        {
                                            bool placed = false;
                                            if (riverDevil)
                                            {
                                                map[y, x] = terrain.spider;
                                                riverDevil = false;
                                                placed = true;
                                            }
                                            foreach (Location l in Roads)
                                            {
                                                if (l.CanShuffle && !placed)
                                                {
                                                    l.Ypos = y + 30;
                                                    l.Xpos = x;
                                                    l.CanShuffle = false;
                                                    break;
                                                }
                                            }

                                        }
                                    }
                                    else if (terrainCycle == 1)
                                    {
                                        if (riverT == terrain.water || riverT == terrain.walkablewater || riverT == terrain.mountain || (riverT != terrain.walkablewater && curr != length / 2 + 1))
                                        {
                                            map[y, x] = terrain.bridge;
                                        }
                                        bool placed = false;
                                        if (curr == length / 2)
                                        {
                                            if (riverDevil)
                                            {
                                                map[y, x] = terrain.spider;
                                                riverDevil = false;
                                                placed = true;
                                            }
                                            foreach (Location l in Bridges)
                                            {
                                                if (l.CanShuffle && !placed)
                                                {
                                                    l.Ypos = y + 30;
                                                    l.Xpos = x;
                                                    l.CanShuffle = false;
                                                    break;
                                                }
                                            }
                                        }
                                    }
                                }
                                else
                                {
                                    if (this.bio == biome.vanillalike)
                                    {
                                        if (riverT == terrain.walkablewater || riverT == terrain.water)
                                        {
                                            t = terrain.bridge;
                                        }
                                        else
                                        {
                                            t = terrain.road;
                                        }
                                    }

                                    map[y, x] = t;
                                    bool placed = false;
                                    if (curr == length / 2)
                                    {
                                        List<Location> locs = locations[(int)t];
                                        if (riverDevil)
                                        {
                                            map[y, x] = terrain.spider;
                                            riverDevil = false;
                                            placed = true;
                                        }
                                        foreach (Location l in locs)
                                        {
                                            if (l.CanShuffle && !placed)
                                            {
                                                l.Ypos = y + 30;
                                                l.Xpos = x;
                                                l.CanShuffle = false;
                                                break;
                                            }
                                        }

                                    }
                                }
                                curr++;
                                x -= deltaX;
                                y -= deltaY;
                            }
                        }
                        terrainCycle++;
                        if (riverT != terrain.mountain && riverT != terrain.desert && !hy.Props.bootsWater)
                        {
                            terrainCycle = terrainCycle % 3;
                        }
                        else
                        {
                            terrainCycle = terrainCycle % 2;
                        }
                        
                    }
                    
                    bridges--;
                    
                }
            }
            return !placeTown;
        }

        private int[,] makeBlobs()
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
                    if(mass[i,j] == 0 && walkable.Contains(map[i,j]))
                    {
                        visit(i, j, massNo, mass);
                        massNo++;
                    }
                }
            }
            return mass;
        }

        private void visit(int y, int x, int massNo, int[,] mass)
        {
            mass[y, x] = massNo;
            if(y - 1 > 0 && mass[y - 1, x] == 0 && walkable.Contains(map[y - 1, x]))
            {
                visit(y - 1, x, massNo, mass);
            }
            if (y + 1 < MAP_ROWS && mass[y + 1, x] == 0 && walkable.Contains(map[y + 1, x]))
            {
                visit(y + 1, x, massNo, mass);
            }
            if (x - 1 > 0 && mass[y, x - 1] == 0 && walkable.Contains(map[y, x - 1]))
            {
                visit(y, x - 1, massNo, mass);
            }
            if (x + 1 < MAP_COLS && mass[y, x + 1] == 0 && walkable.Contains(map[y, x + 1]))
            {
                visit(y, x + 1, massNo, mass);
            }
        }


    

        protected int nextToWater(int x, int y, terrain riverT)
        {
            if(walkable.Contains(map[y, x]) && map[y, x] != riverT)
            {
                if(map[y + 1, x] == riverT)
                {
                    return 0;
                }

                if (map[y - 1, x] == riverT)
                {
                    return 1;
                }

                if (map[y, x + 1] == riverT)
                {
                    return 2;
                }

                if (map[y, x - 1] == riverT)
                {
                    return 3;
                }
            }
            return -1;
        }

        private bool singleTile(int y, int x)
        {
            int count = 0;
            if (x < MAP_COLS && x > 0)
            {
                if (y + 1 < MAP_ROWS && !walkable.Contains(map[y + 1, x]))
                {
                    count++;
                }
                if (y - 1 > 0 && !walkable.Contains(map[y - 1, x]))
                {
                    count++;
                }
            }
            if (y < MAP_ROWS && y > 0)
            {
                if (x + 1 < MAP_COLS && !walkable.Contains(map[y, x + 1]))
                {
                    count++;
                }
                if (x - 1 > 0 && !walkable.Contains(map[y, x - 1]))
                {
                    count++;
                }
            }
            return count == 4;

        }
        protected bool growTerrain()
        {
            terrain[,] mapCopy = new terrain[MAP_ROWS, MAP_COLS];
            List<Tuple<int, int>> placed = new List<Tuple<int, int>>();
            for(int i = 0; i < MAP_ROWS; i++)
            {
                for(int j = 0; j < MAP_COLS; j++)
                {
                    if (map[i, j] != terrain.none && randomTerrains.Contains(map[i, j]))
                    {
                        placed.Add(new Tuple<int, int>(i, j));
                    }
                }
            }

            for (int i = 0; i < MAP_ROWS; i++)
            {
                for (int j = 0; j < MAP_COLS; j++)
                {
                    if (map[i, j] == terrain.none)
                    {
                        List<terrain> choices = new List<terrain>();
                        double mindistance = 9999999999999999999;

                        foreach(Tuple<int, int> t in placed)
                        {
                            double tx = t.Item1 - i;
                            double ty = t.Item2 - j;
                            double distance = Math.Sqrt(tx * tx + ty * ty);
                            if (distance < mindistance)
                            {
                                choices = new List<terrain>();
                                choices.Add(map[t.Item1, t.Item2]);
                                mindistance = distance;
                            }
                            else if(distance == mindistance)
                            {
                                choices.Add(map[t.Item1, t.Item2]);
                            }
                        }
                        mapCopy[i, j] = choices[hy.R.Next(choices.Count)];
                    }
                }
            }

            for (int i = 0; i < MAP_ROWS; i++)
            {
                for (int j = 0; j < MAP_COLS; j++)
                {
                    if (map[i, j] != terrain.none)
                    {
                        mapCopy[i, j] = map[i, j];
                    }
                }
            }
            map = (terrain[,])mapCopy.Clone();
            return true;
        }

        protected void placeRandomTerrain(int num)
        {
            //randomly place remaining terrain
            int placed = 0;
            while (placed < num)
            {
                int x = 0;
                int y = 0;
                terrain t = randomTerrains[hy.R.Next(randomTerrains.Count)];
                do
                {
                    x = hy.R.Next(MAP_COLS);
                    y = hy.R.Next(MAP_ROWS);
                } while (map[y, x] != terrain.none);
                map[y, x] = t;
                placed++;
            }
        }

        protected void writeBytes(bool doWrite, int loc, int total, int h1, int h2)
        {
            bcount = 0;
            terrain curr2 = map[0, 0];
            int count2 = 0;
            for (int i = 0; i < MAP_ROWS; i++)
            {
                for (int j = 0; j < MAP_COLS; j++)
                {
                    if(hy.hiddenPalace && i == h1 && j == h2 && i != 0 && j != 0)
                    {
                        count2--;
                        int b = count2 * 16 + (int)curr2;
                        //Console.WriteLine("Hex: {0:X}", b);
                        if (doWrite)
                        {
                            hy.ROMData.put(loc, (Byte)b);
                            hy.ROMData.put(loc + 1, (byte)curr2);
                        }
                        count2 = 0;
                        loc += 2;
                        bcount += 2;
                        continue;
                    }
                    if (hy.hiddenKasuto && i == 51 && j == 61 && i != 0 && j != 0 && (this.bio == biome.vanilla || this.bio == biome.vanillaShuffle))
                    {
                        count2--;
                        int b = count2 * 16 + (int)curr2;
                        //Console.WriteLine("Hex: {0:X}", b);
                        if (doWrite)
                        {
                            hy.ROMData.put(loc, (Byte)b);
                            hy.ROMData.put(loc + 1, (byte)curr2);
                        }
                        count2 = 0;
                        loc += 2;
                        bcount += 2;
                        continue;
                    }
                    
                    if (map[i, j] == curr2 && count2 < 16)
                    {
                        count2++;
                    }
                    else
                    {
                        count2--;
                        int b = count2 * 16 + (int)curr2;
                        //Console.WriteLine("Hex: {0:X}", b);
                        if (doWrite)
                        {
                            hy.ROMData.put(loc, (Byte)b);
                        }

                        curr2 = map[i, j];
                        count2 = 1;
                        loc++;
                        bcount++;
                    }
                }
                count2--;
                int b2 = count2 * 16 + (int)curr2;
                //Console.WriteLine("Hex: {0:X}", b2);
                if (doWrite)
                {
                    hy.ROMData.put(loc, (Byte)b2);
                }

                if (i < MAP_ROWS - 1)
                {
                    curr2 = map[i + 1, 0];
                }
                count2 = 0;
                loc++;
                bcount++;
            }

            while (bcount < total)
            {
                hy.ROMData.put(loc, (Byte)0x0B);
                bcount++;
                loc++;
            }
        }

        protected void drawOcean(direction d)
        {
            terrain water = terrain.water;
            if (hy.Props.bootsWater)
            {
                water = terrain.walkablewater;
            }
            int x = 0;
            int y = 0;
            int olength = 0;

            if(d == direction.west)
            {
                y = hy.R.Next(MAP_ROWS);
                olength = hy.R.Next(Math.Max(y, MAP_ROWS - y));
            } 
            else if(d == direction.east)
            {
                x = MAP_COLS - 1;
                y = hy.R.Next(MAP_ROWS);
                olength = hy.R.Next(Math.Max(y, MAP_ROWS - y));
            }
            else if(d == direction.north)
            {
                x = hy.R.Next(MAP_COLS);
                olength = hy.R.Next(Math.Max(x, MAP_COLS - x));
            }
            else
            {
                x = hy.R.Next(MAP_COLS);
                y = MAP_ROWS - 1;
                olength = hy.R.Next(Math.Max(x, MAP_COLS - x));
            }
            //draw ocean on right side
            
            if (d == direction.east || d == direction.west)
            {
                if (y < MAP_ROWS / 2)
                {
                    for (int i = 0; i < olength; i++)
                    {
                        if(map[y + i, x] != terrain.none && this.bio != biome.mountainous)
                        {
                            return;
                        }
                        map[y + i, x] = water;
                    }
                }
                else
                {
                    for (int i = 0; i < olength; i++)
                    {
                        if (map[y - i, x] != terrain.none && this.bio != biome.mountainous)
                        {
                            return;
                        }
                        map[y - i, x] = water;
                    }
                }
            }
            else
            {
                if (x < MAP_COLS / 2)
                {
                    for (int i = 0; i < olength; i++)
                    {
                        if (map[y, x + i] != terrain.none && this.bio != biome.mountainous)
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
                        if (map[y, x - 1] != terrain.none && this.bio != biome.mountainous)
                        {
                            return;
                        }
                        map[y, x - i] = water;
                    }
                }
            }
        }
        protected void updateReachable()
        {
            bool needJump = false;
            Location l = getLocationByMem(0x8646);
            int dy = -1;
            int dx = -1;
            if(l != null)
            {
                needJump = l.Needjump;
                dy = l.Ypos - 30;
                dx = l.Xpos;
            }

            bool needFairy = false;
            l = getLocationByMem(0x8644);
            int sy = -1;
            int sx = -1;
            if (l != null)
            {
                needFairy = l.NeedFairy;
                sy = l.Ypos - 30;
                sx = l.Xpos;
            }
            bool changed = true;
            while (changed)
            {
                changed = false;
                for (int i = 0; i < MAP_ROWS; i++)
                {
                    for (int j = 0; j < MAP_COLS; j++)
                    {
                        if(l != null && l.TerrainType == terrain.swamp)
                        {
                            needFairy = l.NeedFairy;
                        }
                        if (l != null && l.TerrainType == terrain.desert)
                        {
                            needJump = l.Needjump;
                        }
                        if (!v[i, j] && !(needJump && dy == i && dx == j && (!hy.SpellGet[(int)spells.jump] && !hy.SpellGet[(int)spells.fairy])) && !(needFairy && sy == i && sx == j && !hy.SpellGet[(int)spells.fairy]) && (map[i, j] == terrain.lava || map[i, j] == terrain.bridge || map[i, j] == terrain.cave || map[i, j] == terrain.road || map[i, j] == terrain.palace || map[i, j] == terrain.town || (map[i, j] == terrain.walkablewater && hy.itemGet[(int)items.boots]) || walkable.Contains(map[i, j]) || (map[i, j] == terrain.rock && hy.itemGet[(int)items.hammer]) || (map[i, j] == terrain.spider && hy.itemGet[(int)items.horn])))
                        {
                            if (i - 1 >= 0)
                            {
                                if (v[i - 1, j])
                                {
                                    v[i, j] = true;
                                    changed = true;
                                    continue;
                                }

                            }

                            if (i + 1 < MAP_ROWS)
                            {
                                if (v[i + 1, j])
                                {
                                    v[i, j] = true;
                                    changed = true;
                                    continue;

                                }
                            }

                            if (j - 1 >= 0)
                            {
                                if (v[i, j - 1])
                                {
                                    v[i, j] = true;
                                    changed = true;
                                    continue;

                                }
                            }

                            if (j + 1 < MAP_COLS)
                            {
                                if (v[i, j + 1])
                                {
                                    v[i, j] = true;
                                    changed = true;
                                    continue;

                                }
                            }
                        }
                    }
                }
            }
        }

        public void reset()
        {
            for(int i = 0; i < MAP_ROWS; i++)
            {
                for(int j = 0; j < MAP_COLS; j++)
                {
                    v[i, j] = false;
                }
            }
            foreach(Location l in allLocations)
            {
                l.CanShuffle = true;
                l.Reachable = false;
                l.itemGet = false;
            }
        }

        protected Boolean drawRaft(bool bridge, direction d)
        {
            //up = 0
            //right = 1
            //down = 2
            //left 3
            int raftx = 0;
            int deltax = 1;
            int deltay = 0;
            int rafty = hy.R.Next(0, MAP_ROWS);

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
                if (d == direction.west)
                {
                    raftx = 0;
                    int rtries = 0;
                    do
                    {
                        rafty = hy.R.Next(0, MAP_ROWS);
                        rtries++;
                        if(rtries > 100)
                        {
                            return false;
                        }
                    } while (map[rafty, raftx] != terrain.walkablewater && map[rafty, raftx] != terrain.water);
                    deltax = 1;
                }
                else if (d == direction.north)
                {
                    rafty = 0;
                    int rtries = 0;

                    do
                    {
                        raftx = hy.R.Next(0, MAP_COLS);
                        rtries++;
                        if (rtries > 100)
                        {
                            return false;
                        }
                    } while (map[rafty, raftx] != terrain.walkablewater && map[rafty, raftx] != terrain.water);
                    deltax = 0;
                    deltay = 1;
                }
                else if (d == direction.south)
                {
                    rafty = MAP_ROWS - 1;
                    int rtries = 0;

                    do
                    { 
                        raftx = hy.R.Next(0, MAP_COLS);
                        rtries++;
                        if (rtries > 100)
                        {
                            return false;
                        }
                    } while (map[rafty, raftx] != terrain.walkablewater && map[rafty, raftx] != terrain.water) ;
                    deltax = 0;
                    deltay = -1;
                }
                else
                {
                    raftx = MAP_COLS - 1;
                    int rtries = 0;

                    do
                    {
                        rafty = hy.R.Next(0, MAP_ROWS);
                        rtries++;
                        if (rtries > 100)
                        {
                            return false;
                        }
                    } while (map[rafty, raftx] != terrain.walkablewater && map[rafty, raftx] != terrain.water);
                    
                    deltax = -1;
                    deltay = 0;
                }
                while (rafty >= 0 && rafty < MAP_ROWS && raftx >= 0 && raftx < MAP_COLS && (map[rafty, raftx] == terrain.walkablewater || map[rafty, raftx] == terrain.water))
                {
                    rafty += deltay;
                    raftx += deltax;
                    length++;
                }
            } while (rafty < 0 || rafty >= MAP_ROWS || raftx < 0 || raftx >= MAP_COLS || !walkable.Contains(map[rafty, raftx]) || (bridge && length > 10) || (bridge && length <= 1));


            rafty -= deltay;
            raftx -= deltax;
            if (!bridge)
            {
                map[rafty, raftx] = terrain.bridge;
                raft.Xpos = raftx;
                raft.Ypos = rafty + 30;
                raft.CanShuffle = false;
            }
            else
            {
                map[rafty, raftx] = terrain.bridge;
                this.bridge.Xpos = raftx;
                this.bridge.Ypos = rafty + 30;
                this.bridge.PassThrough = 0;
                this.bridge.CanShuffle = false;
                if (d == direction.east)
                {
                    for (int i = raftx + 1; i < MAP_COLS; i++)
                    {
                        map[rafty, i] = terrain.bridge;
                    }
                }
                else if(d == direction.west)
                {
                    for (int i = raftx - 1; i >= 0; i--)
                    {
                        map[rafty, i] = terrain.bridge;
                    }
                }
                else if (d == direction.south)
                {
                    for (int i = rafty + 1; i < MAP_ROWS; i++)
                    {
                        map[i, raftx] = terrain.bridge;
                    }
                }
                else if (d == direction.north)
                {
                    for (int i = rafty - 1; i >= 0; i--)
                    {
                        map[i, raftx] = terrain.bridge;
                    }
                }
            }
            return true;

        }

        private void loadLocation(int addr, terrain t, continent c)
        {
            Byte[] bytes = new Byte[4] { hy.ROMData.getByte(addr), hy.ROMData.getByte(addr + overworldXOff), hy.ROMData.getByte(addr + overworldMapOff), hy.ROMData.getByte(addr + overworldWorldOff) };
            addLocation(new Location(bytes, t, addr, c));
        }

        public void loadRaft(int world)
        {
            loadLocation(baseAddr + 41, terrain.bridge, (continent)world);
            raft = getLocationByMem(baseAddr + 41);
            raft.ExternalWorld = 0x80;
            raft.World = world;
            raft.Map = 41;
            raft.TerrainType = terrain.bridge;
        }

        public void loadBridge(int world)
        {
            loadLocation(baseAddr + 40, terrain.bridge, (continent)world);
            bridge = getLocationByMem(baseAddr + 40);
            bridge.ExternalWorld = 0x80;
            bridge.World = world;
            bridge.Map = 40;
            bridge.PassThrough = 0;
        }

        public void loadCave1(int world)
        {
            loadLocation(baseAddr + 42, terrain.cave, (continent)world);
            cave1 = getLocationByMem(baseAddr + 42);
            cave1.ExternalWorld = 0x80;
            cave1.World = world;
            cave1.Map = 42;
            cave1.CanShuffle = true;


        }

        public void loadCave2(int world)
        {
            loadLocation(baseAddr + 43, terrain.cave, (continent)world);
            cave2 = getLocationByMem(baseAddr + 43);
            cave2.ExternalWorld = 0x80;
            cave2.World = world;
            cave2.Map = 43;
            cave2.TerrainType = terrain.cave;
            cave2.CanShuffle = true;
        }

        public Boolean hasConnections()
        {
            return raft != null || bridge != null || cave1 != null || cave2 != null;
        }

        public void visitRaft()
        {
            if (raft != null)
            {
                v[raft.Ypos - 30, raft.Xpos] = true;
            }
        }

        public void visitBridge()
        {
            if(bridge != null)
            {
                v[bridge.Ypos - 30, bridge.Xpos] = true;
            }
        }

        public void visitCave1()
        {
            if(cave1 != null)
            {
                v[cave1.Ypos - 30, cave1.Xpos] = true;
            }
        }

        public void visitCave2()
        {
            if (cave2 != null)
            {
                v[cave2.Ypos - 30, cave2.Xpos] = true;
            }
        }

        public void removeUnusedConnectors()
        {
            if(this.raft == null)
            {
                hy.ROMData.put(baseAddr + 41, 0x00);
            }

            if(this.bridge == null)
            {
                hy.ROMData.put(baseAddr + 40, 0x00);
            }

            if(this.cave1 == null)
            {
                hy.ROMData.put(baseAddr + 42, 0x00);
            }

            if(this.cave2 == null)
            {
                hy.ROMData.put(baseAddr + 43, 0x00);
            }
        }

        protected void drawRiver(List<Location> bridges)
        {
            terrain water = terrain.water;
            if (hy.Props.bootsWater)
            {
                water = terrain.walkablewater;
            }
            int dirr = hy.R.Next(4);
            int dirr2 = dirr;
            while (dirr == dirr2)
            {
                dirr2 = hy.R.Next(4);
            }

            int deltax = 0;
            int deltay = 0;
            int startx = 0;
            int starty = 0;
            if(dirr == 0) //north
            {
                deltay = 1;
                startx = hy.R.Next(MAP_COLS / 3, (MAP_COLS / 3) * 2);
                starty = 0;
            }
            else if(dirr == 1) //east
            {
                deltax = -1;
                startx = MAP_COLS - 1;
                starty = hy.R.Next(MAP_ROWS / 3, (MAP_ROWS / 3) * 2);
            }
            else if(dirr == 2) //south
            {
                deltay = -1;
                startx = hy.R.Next(MAP_COLS / 3, (MAP_COLS / 3) * 2);
                starty = MAP_ROWS - 1;
            }
            else //west
            {
                deltax = 1;
                startx = 0;
                starty = hy.R.Next(MAP_ROWS / 3, (MAP_ROWS / 3) * 2);
            }

            int stopping = hy.R.Next(MAP_COLS / 3, (MAP_COLS / 3) * 2);
            if(deltay != 0)
            {
                stopping = hy.R.Next(MAP_ROWS / 3, (MAP_ROWS / 3) * 2);
            }
            int curr = 0;
            while(curr < stopping)
            {
                
                if(map[starty, startx] == terrain.none)
                {
                    map[starty, startx] = water;
                    int adjust = hy.R.Next(-1, 2);
                    if ((deltax == 0 && startx == 1) || (deltay == 0 && starty == 1))
                    {
                        adjust = hy.R.Next(0, 2);
                    }
                    else if((deltax == 0 && startx == MAP_COLS - 2) || (deltay == 0 && starty == MAP_ROWS - 2))
                    {
                        adjust = hy.R.Next(-1, 1);
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

                if (map[starty, startx] == terrain.none)
                {
                    map[starty, startx] = water;
                    int adjust = hy.R.Next(-1, 2);
                    if ((deltax == 0 && startx == 1) || (deltay == 0 && starty == 1))
                    {
                        adjust = hy.R.Next(0, 2);
                    }
                    else if ((deltax == 0 && startx == MAP_COLS - 2) || (deltay == 0 && starty == MAP_ROWS - 2))
                    {
                        adjust = hy.R.Next(-1, 1);
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

        public void drawCanyon(terrain riverT)
        {
            int drawLeft = hy.R.Next(0, 5);
            int drawRight = hy.R.Next(0, 5);
            terrain tleft = walkable[hy.R.Next(walkable.Count)];
            terrain tright = walkable[hy.R.Next(walkable.Count)];
            if (!horizontal)
            {
                int riverx = hy.R.Next(15, MAP_COLS - 15);
                for (int y = 0; y < MAP_ROWS; y++)
                {
                    drawLeft++;
                    drawRight++;
                    map[y, riverx] = riverT;
                    map[y, riverx + 1] = riverT;
                    int adjust = hy.R.Next(-3, 4);
                    int leftM = hy.R.Next(14, 17);
                    if (riverx - leftM > 0)
                    {
                        map[y, riverx - leftM + 3] = tleft;
                    }
                    if (drawLeft % 5 == 0)
                    {
                        tleft = walkable[hy.R.Next(walkable.Count)];
                    }
                    for (int i = riverx - leftM; i >= 0; i--)
                    {
                        map[y, i] = terrain.mountain;
                    }

                    int rightM = hy.R.Next(14, 17);
                    
                    if (riverx + rightM < MAP_COLS)
                    {
                        map[y, riverx + rightM - 3] = tright;
                    }
                    
                    if (drawRight % 5 == 0)
                    {
                        tright = walkable[hy.R.Next(walkable.Count)];
                    }
                    for (int i = riverx + 1 + rightM; i < MAP_COLS; i++)
                    {
                        map[y, i] = terrain.mountain;
                    }
                    while (riverx + adjust + 1 > MAP_COLS - 15 || riverx + adjust < 15)
                    {
                        adjust = hy.R.Next(-1, 2);
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
                int rivery = hy.R.Next(15, MAP_ROWS - 15);
                for (int x = 0; x < MAP_COLS; x++)
                {
                    drawLeft++;
                    drawRight++;
                    map[rivery, x] = riverT;
                    map[rivery + 1, x] = riverT;
                    int adjust = hy.R.Next(-3, 3);
                    int leftM = hy.R.Next(14, 17);
                    if (rivery - leftM > 0)
                    {
                        map[rivery - leftM + 3, x] = tleft;
                    }
                    if (drawLeft % 5 == 0)
                    {
                        tleft = walkable[hy.R.Next(walkable.Count)];
                    }
                    for (int i = rivery - leftM; i >= 0; i--)
                    {
                        map[i, x] = terrain.mountain;
                    }

                    int rightM = hy.R.Next(14, 17);
                    
                    if (rivery + rightM < MAP_ROWS)
                    {
                        map[rivery + rightM - 3, x] = tright;
                    }
                    
                    if (drawRight % 5 == 0)
                    {
                        tright = walkable[hy.R.Next(walkable.Count)];
                    }
                    for (int i = rivery + 1 + rightM; i < MAP_ROWS; i++)
                    {
                        map[i, x] = terrain.mountain;
                    }
                    while (rivery + adjust + 1 > MAP_ROWS - 15 || rivery + adjust < 15)
                    {
                        adjust = hy.R.Next(-1, 2);
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

        public void drawCenterMountain()
        {
            horizontal = hy.R.NextDouble() > 0.5;
            int top = (MAP_ROWS - 35) / 2;
            int bottom = MAP_ROWS - top;
            if (horizontal)
            {
                for (int i = 0; i < MAP_ROWS; i++)
                {
                    if (i < top || i > bottom)
                    {
                        for (int j = 0; j < MAP_COLS; j++)
                        {
                            map[i, j] = terrain.mountain;
                        }
                    }
                }

                for (int i = 0; i < 8; i++)
                {
                    int jstart = MAP_COLS / 2 - (3 + i);
                    int jend = MAP_COLS / 2 + (3 + i);
                    //map[20 + i, jstart - 1] = terrain.lava;
                    //map[20 + i, jend] = terrain.lava;
                    for (int j = jstart; j < jend; j++)
                    {

                        map[top + i, j] = terrain.mountain;
                    }
                }
                for (int i = 0; i < 19; i++)
                {
                    //map[28 + i, MAP_COLS / 2 - 11] = terrain.lava;
                    //map[28 + i, MAP_COLS / 2 - 10 + 21] = terrain.lava;

                    for (int j = 0; j < 20; j++)
                    {
                        map[top + 8 + i, MAP_COLS / 2 - 10 + j] = terrain.mountain;
                    }
                }
                for (int i = 0; i < 8; i++)
                {
                    int jstart = MAP_COLS / 2 - (3 + (6 - i));
                    int jend = MAP_COLS / 2 + (3 + (6 - i));
                    //map[47 + i, jstart - 1] = terrain.lava;
                    //map[47 + i, jend] = terrain.lava;
                    for (int j = jstart; j < jend; j++)
                    {
                        map[top + 27 + i, j] = terrain.mountain;
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
                            map[j, i] = terrain.mountain;
                        }
                    }
                }

                for (int i = 0; i < 8; i++)
                {
                    int jstart = MAP_ROWS / 2 - (3 + i);
                    int jend = MAP_ROWS / 2 + (3 + i);
                    //map[20 + i, jstart - 1] = terrain.lava;
                    //map[20 + i, jend] = terrain.lava;
                    for (int j = jstart; j < jend; j++)
                    {

                        map[j, top + i] = terrain.mountain;
                    }
                }
                for (int i = 0; i < 19; i++)
                {
                    //map[28 + i, MAP_COLS / 2 - 11] = terrain.lava;
                    //map[28 + i, MAP_COLS / 2 - 10 + 21] = terrain.lava;

                    for (int j = 0; j < 20; j++)
                    {
                        map[MAP_ROWS / 2 - 10 + j, top + 8 + i] = terrain.mountain;
                    }
                }
                for (int i = 0; i < 8; i++)
                {
                    int jstart = MAP_ROWS / 2 - (3 + (6 - i));
                    int jend = MAP_ROWS / 2 + (3 + (6 - i));
                    //map[47 + i, jstart - 1] = terrain.lava;
                    //map[47 + i, jend] = terrain.lava;
                    for (int j = jstart; j < jend; j++)
                    {
                        map[j, top + 27 + i] = terrain.mountain;
                    }
                }
            }
        }

        protected bool horizontalCave(int caveDir, int centerx, int centery, Location cave1l, Location cave1r)
        {
            if (caveDir == 0) //first cave left
            {
                int cavey = hy.R.Next(centery - 2, centery + 3);
                int cavex = centerx;
                while (map[cavey, cavex] != terrain.mountain)
                {
                    cavex--;
                }
                if (map[cavey + 1, cavex] != terrain.mountain)
                {
                    cavey--;
                }
                else if(map[cavey - 1, cavex] != terrain.mountain)
                {
                    cavey++;
                }
                map[cavey, cavex] = terrain.cave;
                cave1r.Ypos = cavey + 30;
                cave1r.Xpos = cavex;
                cavex--;
                int curr = 0;
                while (cavex > 0 && map[cavey, cavex] == terrain.mountain)
                {
                    cavex--;
                    curr++;
                }

                if (curr <= 2 || cavex <= 0)
                {
                    return false;
                }
                if (map[cavey, cavex] == terrain.cave)
                {
                    return false;
                }
                cavex++;
                if (map[cavey + 1, cavex] == terrain.cave || map[cavey - 1, cavex] == terrain.cave)
                {
                    return false;
                }
                map[cavey, cavex] = terrain.cave;
                cave1l.Ypos = cavey + 30;
                cave1l.Xpos = cavex;
                map[cavey + 1, cavex] = terrain.mountain;
                map[cavey - 1, cavex] = terrain.mountain;
            }
            else
            {
                int cavey = hy.R.Next(centery - 2, centery + 3);
                int cavex = centerx;
                while (map[cavey, cavex] != terrain.mountain)
                {
                    cavex++;
                }
                if (map[cavey + 1, cavex] != terrain.mountain)
                {
                    cavey--;
                }
                else if (map[cavey - 1, cavex] != terrain.mountain)
                {
                    cavey++;
                }
                map[cavey, cavex] = terrain.cave;
                cave1l.Ypos = cavey + 30;
                cave1l.Xpos = cavex;
                cavex++;
                int curr = 0;
                while (cavex < MAP_COLS && map[cavey, cavex] == terrain.mountain)
                {
                    cavex++;
                    curr++;
                }
                if (curr <= 2 || cavex >= MAP_COLS)
                {
                    return false;
                }
                if (map[cavey, cavex] == terrain.cave)
                {
                    return false;
                }
                cavex--;
                if (map[cavey + 1, cavex] == terrain.cave || map[cavey - 1, cavex] == terrain.cave)
                {
                    return false;
                }
                map[cavey, cavex] = terrain.cave;
                cave1r.Ypos = cavey + 30;
                cave1r.Xpos = cavex;
                map[cavey + 1, cavex] = terrain.mountain;
                map[cavey - 1, cavex] = terrain.mountain;
            }
            return true;
        }

        protected bool verticalCave(int caveDir, int centerx, int centery, Location cave1l, Location cave1r)
        {
            if (caveDir == 0) //first cave up
            {
                int cavey = centery;
                int cavex = hy.R.Next(centerx - 2, centerx + 3);
                while (map[cavey, cavex] != terrain.mountain)
                {
                    cavey--;
                }
                if (map[cavey, cavex + 1] != terrain.mountain)
                {
                    cavex--;
                }
                else if (map[cavey, cavex - 1] != terrain.mountain)
                {
                    cavex++;
                }
                map[cavey, cavex] = terrain.cave;
                cave1r.Ypos = cavey + 30;
                cave1r.Xpos = cavex;
                cavey--;
                int curr = 0;
                while (cavey > 0 && map[cavey, cavex] == terrain.mountain)
                {
                    cavey--;
                    curr++;
                }
                if (curr <= 2 || cavey <= 0)
                {
                    return false;
                }
                if (map[cavey, cavex] == terrain.cave)
                {
                    return false;
                }
                cavey++;
                if (map[cavey, cavex + 1] == terrain.cave || map[cavey, cavex - 1] == terrain.cave)
                {
                    return false;
                }
                map[cavey, cavex] = terrain.cave;
                cave1l.Ypos = cavey + 30;
                cave1l.Xpos = cavex;
                map[cavey, cavex + 1] = terrain.mountain;
                map[cavey, cavex - 1] = terrain.mountain;
            }
            else
            {
                int cavey = centery;
                int cavex = hy.R.Next(centerx - 2, centerx + 3);
                while (map[cavey, cavex] != terrain.mountain)
                {
                    cavey++;
                }
                if (map[cavey, cavex + 1] != terrain.mountain)
                {
                    cavex--;
                }
                else if (map[cavey, cavex - 1] != terrain.mountain)
                {
                    cavex++;
                }
                map[cavey, cavex] = terrain.cave;
                cave1l.Ypos = cavey + 30;
                cave1l.Xpos = cavex;
                cavey++;
                int curr = 0;
                while (cavey < MAP_ROWS && map[cavey, cavex] == terrain.mountain )
                {
                    cavey++;
                    curr++;
                }
                if (curr <= 2 || cavey >= MAP_ROWS)
                {
                    return false;
                }
                if (map[cavey, cavex] == terrain.cave)
                {
                    return false;
                }
                cavey--;
                if (map[cavey, cavex + 1] == terrain.cave || map[cavey, cavex - 1] == terrain.cave)
                {
                    return false;
                }

                map[cavey, cavex] = terrain.cave;
                cave1r.Ypos = cavey + 30;
                cave1r.Xpos = cavex;
                map[cavey, cavex + 1] = terrain.mountain;
                map[cavey, cavex - 1] = terrain.mountain;
            }
            return true;
        }
    }
}
