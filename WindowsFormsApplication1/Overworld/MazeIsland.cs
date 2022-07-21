using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Z2Randomizer
{
    class MazeIsland : World
    {
        private readonly SortedDictionary<int, Terrain> terrains = new SortedDictionary<int, Terrain>
        {
            { 0xA131, Terrain.ROAD },
                { 0xA132, Terrain.ROAD },
                { 0xA133, Terrain.ROAD },
                { 0xA134, Terrain.BRIDGE },
                { 0xA140, Terrain.PALACE },
                { 0xA143, Terrain.ROAD },
                { 0xA145, Terrain.ROAD },
                { 0xA146, Terrain.ROAD },
                { 0xA147, Terrain.ROAD },
                { 0xA148, Terrain.ROAD },
                { 0xA149, Terrain.ROAD }
        };

        public Location kid;
        public Location magic;
        public Location palace4;

        private const int MAP_ADDR = 0xba00;


        public MazeIsland(Hyrule hy)
            : base(hy)
        {
            LoadLocations(0xA131, 3, terrains, Continent.MAZE);
            LoadLocations(0xA140, 1, terrains, Continent.MAZE);
            LoadLocations(0xA143, 1, terrains, Continent.MAZE);
            LoadLocations(0xA145, 5, terrains, Continent.MAZE);
            walkable = new List<Terrain>();
            walkable.Add(Terrain.MOUNAIN);
            enemyAddr = 0x88B0;
            enemies = new List<int> { 03, 04, 05, 0x11, 0x12, 0x14, 0x16, 0x18, 0x19, 0x1A, 0x1B, 0x1C };
            flyingEnemies = new List<int> { 0x06, 0x07, 0x0A, 0x0D, 0x0E, 0x15 };
            generators = new List<int> { 0x0B, 0x0F, 0x17 };
            shorties = new List<int> { 0x03, 0x04, 0x05, 0x11, 0x12, 0x16 };
            tallGuys = new List<int> { 0x14, 0x18, 0x19, 0x1A, 0x1B, 0x1C };
            enemyPtr = 0xA08E;
            overworldMaps = new List<int>();

            kid = GetLocationByMem(0xA143);
            magic = GetLocationByMem(0xA133);
            palace4 = GetLocationByMem(0xA140);
            palace4.PalNum = 4;
            palace4.World = palace4.World | 0x03;
            MAP_ROWS = 23;
            MAP_COLS = 23;

            baseAddr = 0xA10c;
            VANILLA_MAP_ADDR = 0xa65c;
            if(hy.Props.mazeBiome.Equals("Vanilla"))
            {
                this.bio = Biome.vanilla;
            }
            else if(hy.Props.mazeBiome.Equals("Vanilla (shuffled)"))
            {
                this.bio = Biome.vanillaShuffle;
            }
            else
            {
                this.bio = Biome.vanillalike;
            }
    }

        public Boolean terraform()
        {
            if (this.bio == Biome.vanilla || this.bio == Biome.vanillaShuffle)
            {
                MAP_ROWS = 75;
                MAP_COLS = 64;
                ReadVanillaMap();
                if (this.bio == Biome.vanillaShuffle)
                {
                    ShuffleLocations(AllLocations);
                    if (hy.Props.vanillaOriginal)
                    {
                        foreach (Location l in AllLocations)
                        {
                            map[l.Ypos - 30, l.Xpos] = l.TerrainType;
                            l.PassThrough = 64;
                        }
                    }
                    foreach (Location l in Locations[Terrain.CAVE])
                    {
                        l.PassThrough = 0;
                    }
                    foreach (Location l in Locations[Terrain.TOWN])
                    {
                        l.PassThrough = 0;
                    }
                    foreach (Location l in Locations[Terrain.PALACE])
                    {
                        l.PassThrough = 0;
                    }
                    bridge.PassThrough = 0;
                    magic.PassThrough = 0;
                    kid.PassThrough = 0;
                }
            }
            else
            {
                MAP_ROWS = 23;
                MAP_COLS = 23;
                bcount = 2000;
                foreach (Location l in AllLocations)
                {
                    l.CanShuffle = true;
                }
                while (bcount > MAP_SIZE_BYTES)
                {

                    map = new Terrain[MAP_ROWS, 64];
                    bool[,] visited = new bool[MAP_ROWS, MAP_COLS];

                    for (int i = 0; i < MAP_COLS; i++)
                    {
                        map[0, i] = Terrain.WALKABLEWATER;
                        map[MAP_ROWS - 1, i] = Terrain.WALKABLEWATER;
                        visited[MAP_ROWS - 1, i] = true;

                        visited[0, i] = true;

                    }

                    for (int i = 0; i < MAP_ROWS; i++)
                    {
                        map[i, 0] = Terrain.WALKABLEWATER;
                        visited[i, 0] = true;
                        map[i, MAP_COLS - 1] = Terrain.WALKABLEWATER;
                        visited[i, MAP_COLS - 1] = true;
                    }
                    for (int i = 1; i < MAP_ROWS - 1; i += 2)
                    {

                        for (int j = 1; j < MAP_COLS - 1; j++)
                        {
                            map[i, j] = Terrain.MOUNAIN;
                        }
                    }

                    for (int i = 0; i < MAP_ROWS; i++)
                    {
                        for (int j = MAP_COLS; j < 64; j++)
                        {
                            map[i, j] = Terrain.WATER;
                        }
                    }

                    for (int j = 1; j < MAP_COLS; j += 2)
                    {
                        for (int i = 1; i < MAP_ROWS - 1; i++)
                        {
                            {
                                map[i, j] = Terrain.MOUNAIN;
                            }
                        }
                    }

                    for (int i = 1; i < MAP_ROWS; i++)
                    {
                        for (int j = 1; j < MAP_COLS; j++)
                        {
                            if (map[i, j] != Terrain.MOUNAIN && map[i, j] != Terrain.WATER && map[i, j] != Terrain.WALKABLEWATER)
                            {
                                map[i, j] = Terrain.ROAD;
                                visited[i, j] = false;
                            }
                            else
                            {
                                visited[i, j] = true;
                            }
                        }
                    }
                    //choose starting position
                    int starty = hy.RNG.Next(2, MAP_ROWS);
                    if (starty == 0)
                    {
                        starty++;
                    }
                    else if (starty % 2 == 1)
                    {
                        starty--;
                    }



                    //generate maze
                    int currx = 2;
                    int curry = starty;
                    Stack<Tuple<int, int>> s = new Stack<Tuple<int, int>>();
                    bool canPlaceCave = true;
                    while (moreToVisit(visited))
                    {
                        List<Tuple<int, int>> n = getListOfNeighbors(currx, curry, visited);
                        if (n.Count > 0)
                        {
                            canPlaceCave = true;
                            Tuple<int, int> next = n[hy.RNG.Next(n.Count)];
                            s.Push(next);
                            if (next.Item1 > currx)
                            {
                                map[curry, currx + 1] = Terrain.ROAD;
                            }
                            else if (next.Item1 < currx)
                            {
                                map[curry, currx - 1] = Terrain.ROAD;
                            }
                            else if (next.Item2 > curry)
                            {
                                map[curry + 1, currx] = Terrain.ROAD;
                            }
                            else
                            {
                                map[curry - 1, currx] = Terrain.ROAD;
                            }
                            currx = next.Item1;
                            curry = next.Item2;
                            visited[curry, currx] = true;
                        }
                        else if (s.Count > 0)
                        {
                            if (cave1 != null && cave1.CanShuffle && GetLocationByCoords(Tuple.Create(curry + 30, currx)) == null)
                            {
                                map[curry, currx] = Terrain.CAVE;
                                cave1.Ypos = curry + 30;
                                cave1.Xpos = currx;
                                cave1.CanShuffle = false;
                                canPlaceCave = false;
                                sealDeadEnd(curry, currx);
                            }
                            else if (cave2 != null && cave2.CanShuffle && GetLocationByCoords(Tuple.Create(curry + 30, currx)) == null && canPlaceCave)
                            {
                                map[curry, currx] = Terrain.CAVE;
                                cave2.Ypos = curry + 30;
                                cave2.Xpos = currx;
                                cave2.CanShuffle = false;
                                sealDeadEnd(curry, currx);

                            }
                            Tuple<int, int> n2 = s.Pop();
                            currx = n2.Item1;
                            curry = n2.Item2;
                        }
                    }

                    //place palace 4

                    Boolean canPlace = false;

                    int p4x = hy.RNG.Next(15) + 3;
                    int p4y = hy.RNG.Next(MAP_ROWS - 6) + 3;
                    while (!canPlace)
                    {
                        p4x = hy.RNG.Next(15) + 3;
                        p4y = hy.RNG.Next(MAP_ROWS - 6) + 3;
                        canPlace = true;
                        if (map[p4y, p4x] != Terrain.ROAD)
                        {
                            canPlace = false;
                        }

                        for (int i = -1; i < 2; i++)
                        {
                            for (int j = -1; j < 2; j++)
                            {
                                if (GetLocationByCoords(Tuple.Create(p4y + i + 30, p4x + j)) != null)
                                {
                                    canPlace = false;
                                }
                            }
                        }
                    }
                    palace4.Xpos = p4x;
                    palace4.Ypos = p4y + 30;
                    map[p4y, p4x] = Terrain.PALACE;
                    map[p4y + 1, p4x] = Terrain.ROAD;
                    map[p4y - 1, p4x] = Terrain.ROAD;
                    map[p4y, p4x + 1] = Terrain.ROAD;
                    map[p4y, p4x - 1] = Terrain.ROAD;
                    map[p4y + 1, p4x + 1] = Terrain.ROAD;
                    map[p4y - 1, p4x - 1] = Terrain.ROAD;
                    map[p4y - 1, p4x + 1] = Terrain.ROAD;
                    map[p4y + 1, p4x - 1] = Terrain.ROAD;

                    //draw a river
                    int riverstart = starty;
                    while (riverstart == starty)
                    {
                        riverstart = hy.RNG.Next(10) * 2 + 1;
                    }

                    int riverend = hy.RNG.Next(10) * 2 + 1;

                    Location rs = new Location();
                    rs.Xpos = 1;
                    rs.Ypos = riverstart + 30;

                    Location re = new Location();
                    re.Xpos = 21;
                    re.Ypos = riverend + 30;
                    drawLine(rs, re, Terrain.WALKABLEWATER);

                    Direction rDir = Direction.east;
                    if (raft != null)
                    {

                        rDir = (Direction)hy.RNG.Next(4);

                        int bx = 0;
                        int by = 0;
                        if (rDir == Direction.north)
                        {

                            bx = hy.RNG.Next(2, MAP_COLS - 1);
                            while (by != 2)
                            {
                                bx = hy.RNG.Next(2, MAP_COLS - 1);
                                by = 0;
                                while (by < MAP_ROWS && map[by, bx] != Terrain.ROAD)
                                {
                                    by++;
                                }
                            }
                            by--;
                            map[by, bx] = Terrain.BRIDGE;
                            this.raft.Ypos = by + 30;
                            this.raft.Xpos = bx;
                            by--;

                            while (by >= 0)
                            {
                                if (map[by, bx] != Terrain.PALACE && map[by, bx] != Terrain.CAVE)
                                {
                                    map[by, bx] = Terrain.WALKABLEWATER;

                                }
                                by--;
                            }
                        }
                        else if (rDir == Direction.south)
                        {
                            by = MAP_ROWS - 1;
                            bx = hy.RNG.Next(2, MAP_COLS - 1);
                            while (by != MAP_ROWS - 3)
                            {
                                by = MAP_ROWS - 1;
                                bx = hy.RNG.Next(2, MAP_COLS - 1);
                                while (by > 0 && map[by, bx] != Terrain.ROAD)
                                {
                                    by--;
                                }
                            }
                            by++;
                            map[by, bx] = Terrain.BRIDGE;
                            this.raft.Ypos = by + 30;
                            this.raft.Xpos = bx;
                            by++;

                            while (by < MAP_ROWS)
                            {
                                if (map[by, bx] != Terrain.PALACE && map[by, bx] != Terrain.CAVE)
                                {
                                    map[by, bx] = Terrain.WALKABLEWATER;

                                }
                                by++;
                            }
                        }
                        else if (rDir == Direction.west)
                        {
                            while (bx != 2)
                            {
                                bx = 0;
                                by = hy.RNG.Next(2, MAP_ROWS - 2);
                                while (bx < MAP_COLS && map[by, bx] != Terrain.ROAD)
                                {
                                    bx++;
                                }
                            }

                            bx--;
                            map[by, bx] = Terrain.BRIDGE;
                            this.raft.Ypos = by + 30;
                            this.raft.Xpos = bx;
                            bx--;

                            while (bx >= 0)
                            {
                                if (map[by, bx] != Terrain.PALACE && map[by, bx] != Terrain.CAVE)
                                {
                                    map[by, bx] = Terrain.WALKABLEWATER;

                                }
                                bx--;
                            }
                        }
                        else
                        {
                            while (bx != MAP_COLS - 3)
                            {
                                bx = MAP_COLS - 1;
                                by = hy.RNG.Next(2, MAP_ROWS - 2);
                                while (bx > 0 && map[by, bx] != Terrain.ROAD)
                                {
                                    bx--;
                                }
                            }
                            bx++;
                            map[by, bx] = Terrain.BRIDGE;
                            this.raft.Ypos = by + 30;
                            this.raft.Xpos = bx;
                            bx++;
                            while (bx < MAP_COLS)
                            {
                                if (map[by, bx] != Terrain.PALACE && map[by, bx] != Terrain.CAVE)
                                {
                                    map[by, bx] = Terrain.WALKABLEWATER;

                                }
                                bx++;
                            }
                        }
                    }


                    Direction bDir = Direction.west;
                    while (bDir == rDir)
                    {
                        bDir = (Direction)hy.RNG.Next(4);
                    }
                    if (bridge != null)
                    {
                        int bx = 0;
                        int by = 0;
                        if (bDir == Direction.north)
                        {
                            bx = hy.RNG.Next(2, MAP_COLS - 1);
                            while (by < MAP_ROWS && map[by, bx] != Terrain.ROAD)
                            {
                                by++;
                            }
                            
                            by--;

                            map[by, bx] = Terrain.BRIDGE;
                            this.bridge.Ypos = by + 30;
                            this.bridge.Xpos = bx;
                            while (by >= 0)
                            {
                                if (map[by, bx] != Terrain.PALACE && map[by, bx] != Terrain.CAVE)
                                {
                                    map[by, bx] = Terrain.BRIDGE;

                                }
                                by--;
                            }
                        }
                        else if (bDir == Direction.south)
                        {
                            by = MAP_ROWS - 1;
                            bx = hy.RNG.Next(2, MAP_COLS - 1);
                            while (by > 0 && map[by, bx] != Terrain.ROAD)
                            {
                                by--;
                            }

                            by++;
                            map[by, bx] = Terrain.BRIDGE;
                            this.bridge.Ypos = by + 30;
                            this.bridge.Xpos = bx;

                            while (by < MAP_ROWS)
                            {
                                if (map[by, bx] != Terrain.PALACE && map[by, bx] != Terrain.CAVE)
                                {
                                    map[by, bx] = Terrain.BRIDGE;

                                }
                                by++;
                            }
                        }
                        else if (bDir == Direction.west)
                        {
                            by = hy.RNG.Next(2, MAP_ROWS - 2);
                            while(by == riverend || by == riverstart)
                            {
                                by = hy.RNG.Next(2, MAP_ROWS - 2);

                            }
                            while (bx < MAP_COLS && map[by, bx] != Terrain.ROAD)
                            {
                                bx++;
                            }

                            bx--;
                            map[by, bx] = Terrain.BRIDGE;
                            this.bridge.Ypos = by + 30;
                            this.bridge.Xpos = bx;

                            while (bx >= 0)
                            {
                                if (map[by, bx] != Terrain.PALACE && map[by, bx] != Terrain.CAVE)
                                {
                                    map[by, bx] = Terrain.BRIDGE;

                                }
                                bx--;
                            }
                        }
                        else
                        {
                            bx = MAP_COLS + 3;
                            by = hy.RNG.Next(2, MAP_ROWS - 2);
                            while (by == riverend || by == riverstart)
                            {
                                by = hy.RNG.Next(2, MAP_ROWS - 2);

                            }
                            while (bx > 0 && map[by, bx] != Terrain.ROAD)
                            {
                                bx--;
                            }
                            bx++;
                            map[by, bx] = Terrain.BRIDGE;
                            this.bridge.Ypos = by + 30;
                            this.bridge.Xpos = bx;

                            while (bx < MAP_COLS)
                            {
                                if (map[by, bx] != Terrain.PALACE && map[by, bx] != Terrain.CAVE)
                                {
                                    map[by, bx] = Terrain.BRIDGE;

                                }
                                bx++;
                            }
                        }
                    }


                    foreach (Location l in AllLocations)
                    {
                        if (l.TerrainType == Terrain.ROAD)
                        {
                            int x = 0;
                            int y = 0;
                            if (l != magic && l != kid)
                            {
                                do
                                {
                                    x = hy.RNG.Next(19) + 2;
                                    y = hy.RNG.Next(MAP_ROWS - 4) + 2;
                                } while (map[y, x] != Terrain.ROAD || !((map[y, x + 1] == Terrain.MOUNAIN && map[y, x - 1] == Terrain.MOUNAIN) || (map[y + 1, x] == Terrain.MOUNAIN && map[y - 1, x] == Terrain.MOUNAIN)) || GetLocationByCoords(new Tuple<int, int>(y + 30, x + 1)) != null || GetLocationByCoords(new Tuple<int, int>(y + 30, x - 1)) != null || GetLocationByCoords(new Tuple<int, int>(y + 31, x)) != null || GetLocationByCoords(new Tuple<int, int>(y + 29, x)) != null || GetLocationByCoords(new Tuple<int, int>(y + 30, x)) != null);
                            }
                            else
                            {
                                do
                                {
                                    x = hy.RNG.Next(19) + 2;
                                    y = hy.RNG.Next(MAP_ROWS - 4) + 2;
                                } while (map[y, x] != Terrain.ROAD || GetLocationByCoords(new Tuple<int, int>(y + 30, x + 1)) != null || GetLocationByCoords(new Tuple<int, int>(y + 30, x - 1)) != null || GetLocationByCoords(new Tuple<int, int>(y + 31, x)) != null || GetLocationByCoords(new Tuple<int, int>(y + 29, x)) != null || GetLocationByCoords(new Tuple<int, int>(y + 30, x)) != null);
                            }

                            l.Xpos = x;
                            l.Ypos = y + 30;
                        }
                    }

                    //check bytes and adjust
                    MAP_COLS = 64;
                    WriteBytes(false, MAP_ADDR, MAP_SIZE_BYTES, 0, 0);
                    MAP_COLS = 23;
                    
                }
            }
            MAP_COLS = 64;
            WriteBytes(true, MAP_ADDR, MAP_SIZE_BYTES, 0, 0);
            for (int i = 0xA10C; i < 0xA149; i++)
            {
                if(!terrains.Keys.Contains(i))
                {
                    hy.ROMData.Put(i, 0x00);
                }
            }

            v = new bool[MAP_ROWS, MAP_COLS];
            for (int i = 0; i < MAP_ROWS; i++)
            {
                for (int j = 0; j < MAP_COLS; j++)
                {
                    v[i, j] = false;
                }
            }
            return true;
        }

        private void sealDeadEnd(int curry, int currx)
        {
            bool foundRoad = false;
            if(map[curry+1, currx] == Terrain.ROAD)
            {
                foundRoad = true;
            }
            
            if(map[curry-1, currx] == Terrain.ROAD)
            {
                if(foundRoad)
                {
                    map[curry - 1, currx] = Terrain.MOUNAIN;
                }
                else
                {
                    foundRoad = true;
                }
            }

            if (map[curry, currx - 1] == Terrain.ROAD)
            {
                if (foundRoad)
                {
                    map[curry, currx - 1] = Terrain.MOUNAIN;
                }
                else
                {
                    foundRoad = true;
                }
            }

            if (map[curry, currx+1 ] == Terrain.ROAD)
            {
                if (foundRoad)
                {
                    map[curry, currx+1] = Terrain.MOUNAIN;
                }
                else
                {
                    foundRoad = true;
                }
            }
        }

        private bool moreToVisit(bool[,] v)
        {
            for (int i = 0; i < v.GetLength(0); i++)
            {
                for (int j = 1; j < v.GetLength(1) - 1; j++)
                {
                    if (v[i, j] == false)
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        private List<Tuple<int, int>> getListOfNeighbors(int currx, int curry, bool[,] v)
        {
            List<Tuple<int, int>> x = new List<Tuple<int, int>>();

            if (currx - 2 > 1 && v[curry, currx - 2] == false)
            {
                x.Add(new Tuple<int, int>(currx - 2, curry));
            }

            if (currx + 2 < MAP_COLS && v[curry, currx + 2] == false)
            {
                x.Add(new Tuple<int, int>(currx + 2, curry));
            }

            if (curry - 2 > 1 && v[curry - 2, currx] == false)
            {
                x.Add(new Tuple<int, int>(currx, curry - 2));
            }

            if (curry + 2 < MAP_ROWS && v[curry + 2, currx] == false)
            {
                x.Add(new Tuple<int, int>(currx, curry + 2));
            }
            return x;
        }

        private void drawLine(Location to, Location from, Terrain t)
        {
            int x = from.Xpos;
            int y = from.Ypos - 30;
            while (x != to.Xpos)
            {
                if (x == 21 || (hy.RNG.NextDouble() > .5 && x != to.Xpos))
                {
                    int diff = to.Xpos - x;
                    int move = (hy.RNG.Next(Math.Abs(diff / 2)) + 1) * 2;

     
                    while (Math.Abs(move) > 0 && !(x == to.Xpos && y == to.Ypos - 30))
                    {
                        for (int i = 0; i < 2; i++)
                        {
                            if ((x != to.Xpos || y != (to.Ypos - 30)) && GetLocationByCoords(new Tuple<int, int>(y + 30, x)) == null)
                            {
                                if(map[y, x] == Terrain.MOUNAIN)
                                {
                                    map[y, x] = t;
                                }
                                else if (map[y, x] == Terrain.ROAD && ((diff > 0 && (map[y, x + 1] == Terrain.MOUNAIN)) || (diff < 0 && map[y, x - 1] == Terrain.MOUNAIN)))
                                {
                                    map[y, x] = Terrain.BRIDGE;
                                }
                                else if (map[y, x] != Terrain.PALACE && map[y, x] != Terrain.BRIDGE && map[y, x] != Terrain.CAVE)
                                {
                                    map[y, x] = t;
                                }

                            }
                            if (diff > 0 && x < MAP_COLS - 1)
                            {
                                x++;
                                
                            }
                            else if (x > 0)
                            {
                                x--;
                                
                            }
                            
                            move--;
                        }
                    }
                }
                else if(y != to.Ypos - 30)
                {
                    int diff = to.Ypos - 30 - y;
                    int move = (hy.RNG.Next(Math.Abs(diff / 2)) + 1) * 2;
                    while (Math.Abs(move) > 0 && !(x == to.Xpos && y == to.Ypos - 30))
                    {
                        for (int i = 0; i < 2; i++)
                        {
                            if ((x != to.Xpos || y != (to.Ypos - 30)) && GetLocationByCoords(new Tuple<int, int>(y + 30, x)) == null)
                            {
                                if (map[y, x] == Terrain.MOUNAIN)
                                {
                                    map[y, x] = t;
                                }
                                else if(map[y, x] == Terrain.ROAD && ((diff > 0 && (map[y + 1, x] == Terrain.MOUNAIN)) || (diff < 0 && map[y - 1, x] == Terrain.MOUNAIN)))
                                {
                                    map[y, x] = Terrain.BRIDGE;
                                }
                                else if (map[y, x] != Terrain.PALACE && map[y, x] != Terrain.BRIDGE && map[y, x] != Terrain.CAVE)
                                {
                                    map[y, x] = t;
                                }
                            }
                            if (diff > 0 && y < MAP_ROWS - 1)
                            {
                                y++;
                                
                            }
                            else if (y > 0)
                            {
                                y--;
                                
                            }
                            move--;
                        }
                    }
                }
            }
        }
        public void updateVisit()
        {
            bool changed = true;
            while (changed)
            {
                changed = false;
                for (int i = 0; i < MAP_ROWS; i++)
                {
                    for (int j = 0; j < MAP_COLS; j++)
                    {
                        if (!v[i,j] && ((map[i, j] == Terrain.WALKABLEWATER && hy.itemGet[(int)Items.boots]) || map[i,j] == Terrain.ROAD || map[i,j] == Terrain.PALACE || map[i, j] == Terrain.BRIDGE))
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

            foreach (Location l in AllLocations)
            {
                if (v[l.Ypos - 30, l.Xpos])
                {
                    l.Reachable = true;
                }
            }
        }
    }
}
