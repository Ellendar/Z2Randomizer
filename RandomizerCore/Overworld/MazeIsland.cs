using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RandomizerCore.Overworld;

sealed class MazeIsland : World
{
    public static readonly int[] OverworldEnemies = new int[] { 03, 04, 05, 0x11, 0x12, 0x14, 0x16, 0x18, 0x19, 0x1A, 0x1B, 0x1C };
    public static readonly int[] OverworldFlyingEnemies = new int[] { 0x06, 0x07, 0x0A, 0x0D, 0x0E, 0x15 };
    public static readonly int[] OverworldGenerators = new int[] { 0x0B, 0x0F, 0x17 };
    public static readonly int[] OverworldSmallEnemies = new int[] { 0x03, 0x04, 0x05, 0x11, 0x12, 0x16 };
    public static readonly int[] OverworldLargeEnemies = new int[] { 0x14, 0x18, 0x19, 0x1A, 0x1B, 0x1C };

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

    public Location childDrop;
    public Location magicContainerDrop;
    public Location locationAtPalace4;

    private const int MAP_ADDR = 0xba00;


    public MazeIsland(RandomizerProperties props, Random r, ROM rom) : base(r)
    {
        List<Location> locations =
        [
            .. rom.LoadLocations(0xA131, 3, terrains, Continent.MAZE),
            .. rom.LoadLocations(0xA140, 1, terrains, Continent.MAZE),
            .. rom.LoadLocations(0xA143, 1, terrains, Continent.MAZE),
            .. rom.LoadLocations(0xA145, 5, terrains, Continent.MAZE),
        ];
        locations.ForEach(AddLocation);

        walkableTerrains = [Terrain.MOUNTAIN];
        enemyAddr = 0x88B0;
        enemies = [0x03, 0x04, 0x05, 0x11, 0x12, 0x14, 0x16, 0x18, 0x19, 0x1A, 0x1B, 0x1C];
        flyingEnemies = [0x06, 0x07, 0x0A, 0x0D, 0x0E, 0x15];
        generators = [0x0B, 0x0F, 0x17];
        smallEnemies = [0x03, 0x04, 0x05, 0x11, 0x12, 0x16];
        largeEnemies = [0x14, 0x18, 0x19, 0x1A, 0x1B, 0x1C];
        enemyPtr = 0xA08E;
        overworldMaps = [];

        childDrop = GetLocationByMem(0xA143);
        magicContainerDrop = GetLocationByMem(0xA133);
        locationAtPalace4 = GetLocationByMem(0xA140);
        locationAtPalace4.PalaceNumber = 4;
        MAP_ROWS = 23;
        MAP_COLS = 23;

        baseAddr = 0xA10c;
        VANILLA_MAP_ADDR = 0xa65c;

        biome = props.MazeBiome;
        SetVanillaCollectables(props.ReplaceFireWithDash);
    }

    public override bool Terraform(RandomizerProperties props, ROM rom)
    {
        if (biome == Biome.VANILLA || biome == Biome.VANILLA_SHUFFLE)
        {
            MAP_ROWS = 75;
            MAP_COLS = 64;
            map = rom.ReadVanillaMap(rom, VANILLA_MAP_ADDR, MAP_ROWS, MAP_COLS);
            if (biome == Biome.VANILLA_SHUFFLE)
            {
                ShuffleLocations(AllLocations);
                if (props.VanillaShuffleUsesActualTerrain)
                {
                    foreach (Location location in AllLocations)
                    {
                        map[location.Ypos - 30, location.Xpos] = location.TerrainType;
                        location.PassThrough = 64;
                    }
                }
                foreach (Location location in Locations[Terrain.CAVE])
                {
                    location.PassThrough = 0;
                }
                foreach (Location location in Locations[Terrain.TOWN])
                {
                    location.PassThrough = 0;
                }
                foreach (Location location in Locations[Terrain.PALACE])
                {
                    location.PassThrough = 0;
                }
                if (bridge != null)
                {
                    bridge.PassThrough = 0;
                }
                magicContainerDrop.PassThrough = 0;
                childDrop.PassThrough = 0;
            }
        }
        else
        {
            MAP_ROWS = 23;
            MAP_COLS = 23;
            int bytesWritten = 2000;
            foreach (Location location in AllLocations)
            {
                location.CanShuffle = true;
            }
            while (bytesWritten > MAP_SIZE_BYTES)
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
                        map[i, j] = Terrain.MOUNTAIN;
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
                            map[i, j] = Terrain.MOUNTAIN;
                        }
                    }
                }

                for (int i = 1; i < MAP_ROWS; i++)
                {
                    for (int j = 1; j < MAP_COLS; j++)
                    {
                        if (map[i, j] != Terrain.MOUNTAIN && map[i, j] != Terrain.WATER && map[i, j] != Terrain.WALKABLEWATER)
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
                int starty = RNG.Next(2, MAP_ROWS);
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
                Stack<(int, int)> stack = new();
                bool canPlaceCave = true;
                while (MoreToVisit(visited))
                {
                    List<(int, int)> neighbors = GetListOfNeighbors(currx, curry, visited);
                    if (neighbors.Count > 0)
                    {
                        canPlaceCave = true;
                        (int, int)next = neighbors[RNG.Next(neighbors.Count)];
                        stack.Push(next);
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
                    else if (stack.Count > 0)
                    {
                        if (cave1 != null && cave1.CanShuffle && GetLocationByCoords((curry + 30, currx)) == null)
                        {
                            map[curry, currx] = Terrain.CAVE;
                            cave1.Ypos = curry + 30;
                            cave1.Xpos = currx;
                            cave1.CanShuffle = false;
                            canPlaceCave = false;
                            SealDeadEnd(curry, currx);
                        }
                        else if (cave2 != null && cave2.CanShuffle && GetLocationByCoords((curry + 30, currx)) == null && canPlaceCave)
                        {
                            map[curry, currx] = Terrain.CAVE;
                            cave2.Ypos = curry + 30;
                            cave2.Xpos = currx;
                            cave2.CanShuffle = false;
                            SealDeadEnd(curry, currx);

                        }
                        (int, int)n2 = stack.Pop();
                        currx = n2.Item1;
                        curry = n2.Item2;
                    }
                }

                //place palace 4

                bool canPlace = false;

                int palace4x = RNG.Next(15) + 3;
                int palace4y = RNG.Next(MAP_ROWS - 6) + 3;
                while (!canPlace)
                {
                    palace4x = RNG.Next(15) + 3;
                    palace4y = RNG.Next(MAP_ROWS - 6) + 3;
                    canPlace = true;
                    if (map[palace4y, palace4x] != Terrain.ROAD)
                    {
                        canPlace = false;
                    }

                    for (int i = -1; i < 2; i++)
                    {
                        for (int j = -1; j < 2; j++)
                        {
                            if (GetLocationByCoords((palace4y + i + 30, palace4x + j)) != null)
                            {
                                canPlace = false;
                            }
                        }
                    }
                }
                locationAtPalace4.Xpos = palace4x;
                locationAtPalace4.Ypos = palace4y + 30;
                map[palace4y, palace4x] = Terrain.PALACE;
                map[palace4y + 1, palace4x] = Terrain.ROAD;
                map[palace4y - 1, palace4x] = Terrain.ROAD;
                map[palace4y, palace4x + 1] = Terrain.ROAD;
                map[palace4y, palace4x - 1] = Terrain.ROAD;
                map[palace4y + 1, palace4x + 1] = Terrain.ROAD;
                map[palace4y - 1, palace4x - 1] = Terrain.ROAD;
                map[palace4y - 1, palace4x + 1] = Terrain.ROAD;
                map[palace4y + 1, palace4x - 1] = Terrain.ROAD;

                //draw a river
                int riverStartY = starty;
                while (riverStartY == starty)
                {
                    riverStartY = RNG.Next(10) * 2 + 1;
                }

                int riverEndY = RNG.Next(10) * 2 + 1;

                /*
                Location riverStart = new Location();
                riverStart.Xpos = 1;
                riverStart.Ypos = riverStartY + 30;

                Location riverEnd = new Location();
                riverEnd.Xpos = 21;
                riverEnd.Ypos = riverEndY + 30;
                */
                DrawLine(riverStartY, 1, riverEndY, 21, Terrain.WALKABLEWATER);

                //Place raft
                Direction raftDirection = Direction.EAST;
                if (raft != null)
                {

                    raftDirection = (Direction)RNG.Next(4);

                    int raftX = 0;
                    int raftY = 0;
                    if (raftDirection == Direction.NORTH)
                    {

                        raftX = RNG.Next(2, MAP_COLS - 1);
                        while (raftY != 2)
                        {
                            raftX = RNG.Next(2, MAP_COLS - 1);
                            raftY = 0;
                            while (raftY < MAP_ROWS && map[raftY, raftX] != Terrain.ROAD)
                            {
                                raftY++;
                            }
                        }
                        raftY--;
                        map[raftY, raftX] = Terrain.BRIDGE;
                        raft.Ypos = raftY + 30;
                        raft.Xpos = raftX;
                        raftY--;

                        while (raftY >= 0)
                        {
                            if (map[raftY, raftX] != Terrain.PALACE && map[raftY, raftX] != Terrain.CAVE)
                            {
                                map[raftY, raftX] = Terrain.WALKABLEWATER;

                            }
                            raftY--;
                        }
                    }
                    else if (raftDirection == Direction.SOUTH)
                    {
                        raftY = MAP_ROWS - 1;
                        raftX = RNG.Next(2, MAP_COLS - 1);
                        while (raftY != MAP_ROWS - 3)
                        {
                            raftY = MAP_ROWS - 1;
                            raftX = RNG.Next(2, MAP_COLS - 1);
                            while (raftY > 0 && map[raftY, raftX] != Terrain.ROAD)
                            {
                                raftY--;
                            }
                        }
                        raftY++;
                        map[raftY, raftX] = Terrain.BRIDGE;
                        raft.Ypos = raftY + 30;
                        raft.Xpos = raftX;
                        raftY++;

                        while (raftY < MAP_ROWS)
                        {
                            if (map[raftY, raftX] != Terrain.PALACE && map[raftY, raftX] != Terrain.CAVE)
                            {
                                map[raftY, raftX] = Terrain.WALKABLEWATER;

                            }
                            raftY++;
                        }
                    }
                    else if (raftDirection == Direction.WEST)
                    {
                        while (raftX != 2)
                        {
                            raftX = 0;
                            raftY = RNG.Next(2, MAP_ROWS - 2);
                            while (raftX < MAP_COLS && map[raftY, raftX] != Terrain.ROAD)
                            {
                                raftX++;
                            }
                        }

                        raftX--;
                        map[raftY, raftX] = Terrain.BRIDGE;
                        raft.Ypos = raftY + 30;
                        raft.Xpos = raftX;
                        raftX--;

                        while (raftX >= 0)
                        {
                            if (map[raftY, raftX] != Terrain.PALACE && map[raftY, raftX] != Terrain.CAVE)
                            {
                                map[raftY, raftX] = Terrain.WALKABLEWATER;

                            }
                            raftX--;
                        }
                    }
                    else
                    {
                        while (raftX != MAP_COLS - 3)
                        {
                            raftX = MAP_COLS - 1;
                            raftY = RNG.Next(2, MAP_ROWS - 2);
                            while (raftX > 0 && map[raftY, raftX] != Terrain.ROAD)
                            {
                                raftX--;
                            }
                        }
                        raftX++;
                        map[raftY, raftX] = Terrain.BRIDGE;
                        raft.Ypos = raftY + 30;
                        raft.Xpos = raftX;
                        raftX++;
                        while (raftX < MAP_COLS)
                        {
                            if (map[raftY, raftX] != Terrain.PALACE && map[raftY, raftX] != Terrain.CAVE)
                            {
                                map[raftY, raftX] = Terrain.WALKABLEWATER;

                            }
                            raftX++;
                        }
                    }
                }

                //Place bridge
                Direction bridgeDirection;
                do
                {
                    bridgeDirection = (Direction)RNG.Next(4);
                } while (bridgeDirection == raftDirection);

                //TODO: refactor this so it's not replicating the same code 4 times
                if (bridge != null)
                {
                    int bridgeX = 0;
                    int bridgeY = 0;
                    if (bridgeDirection == Direction.NORTH)
                    {
                        bridgeX = RNG.Next(2, MAP_COLS - 1);
                        while (bridgeY < MAP_ROWS && map[bridgeY, bridgeX] != Terrain.ROAD)
                        {
                            bridgeY++;
                        }
                        
                        bridgeY--;

                        map[bridgeY, bridgeX] = Terrain.BRIDGE;
                        bridge.Ypos = bridgeY + 30;
                        bridge.Xpos = bridgeX;
                        while (bridgeY >= 0)
                        {
                            if (map[bridgeY, bridgeX] == Terrain.PALACE
                                || map[bridgeY, bridgeX] == Terrain.CAVE
                                || locationAtPalace4.Xpos == bridgeX && locationAtPalace4.Ypos - 30 == bridgeY
                                || cave1 != null && cave1.Xpos == bridgeX && cave1.Ypos - 30 == bridgeY
                                || cave2 != null && cave2.Xpos == bridgeX && cave2.Ypos - 30 == bridgeY)
                            {
                                return false;
                            }
                            else
                            {
                                map[bridgeY, bridgeX] = Terrain.BRIDGE;
                            }
                            bridgeY--;
                        }
                    }
                    else if (bridgeDirection == Direction.SOUTH)
                    {
                        bridgeY = MAP_ROWS - 1;
                        bridgeX = RNG.Next(2, MAP_COLS - 1);
                        while (bridgeY > 0 && map[bridgeY, bridgeX] != Terrain.ROAD)
                        {
                            bridgeY--;
                        }

                        bridgeY++;
                        map[bridgeY, bridgeX] = Terrain.BRIDGE;
                        bridge.Ypos = bridgeY + 30;
                        bridge.Xpos = bridgeX;

                        while (bridgeY < MAP_ROWS)
                        {
                            if (map[bridgeY, bridgeX] == Terrain.PALACE
                                || map[bridgeY, bridgeX] == Terrain.CAVE
                                || locationAtPalace4.Xpos == bridgeX && locationAtPalace4.Ypos - 30 == bridgeY
                                || cave1 != null && cave1.Xpos == bridgeX && cave1.Ypos - 30 == bridgeY
                                || cave2 != null && cave2.Xpos == bridgeX && cave2.Ypos - 30 == bridgeY)
                            {
                                return false;
                            }
                            else
                            {
                                map[bridgeY, bridgeX] = Terrain.BRIDGE;
                            }
                            bridgeY++;
                        }
                    }
                    else if (bridgeDirection == Direction.WEST)
                    {
                        bridgeY = RNG.Next(2, MAP_ROWS - 2);
                        while(bridgeY == riverEndY || bridgeY == riverStartY)
                        {
                            bridgeY = RNG.Next(2, MAP_ROWS - 2);
                        }
                        while (bridgeX < MAP_COLS && map[bridgeY, bridgeX] != Terrain.ROAD)
                        {
                            bridgeX++;
                        }

                        bridgeX--;
                        map[bridgeY, bridgeX] = Terrain.BRIDGE;
                        bridge.Ypos = bridgeY + 30;
                        bridge.Xpos = bridgeX;

                        while (bridgeX >= 0)
                        {
                            if (map[bridgeY, bridgeX] == Terrain.PALACE 
                                || map[bridgeY, bridgeX] == Terrain.CAVE
                                || locationAtPalace4.Xpos == bridgeX && locationAtPalace4.Ypos - 30 == bridgeY
                                || cave1 != null && cave1.Xpos == bridgeX && cave1.Ypos - 30 == bridgeY
                                || cave2 != null && cave2.Xpos == bridgeX && cave2.Ypos - 30 == bridgeY)
                            {
                                return false;
                            }
                            else
                            {
                                map[bridgeY, bridgeX] = Terrain.BRIDGE;
                            }
                            bridgeX--;
                        }
                    }
                    else
                    {
                        bridgeX = MAP_COLS + 3;
                        bridgeY = RNG.Next(2, MAP_ROWS - 2);
                        while (bridgeY == riverEndY || bridgeY == riverStartY)
                        {
                            bridgeY = RNG.Next(2, MAP_ROWS - 2);

                        }
                        while (bridgeX > 0 && map[bridgeY, bridgeX] != Terrain.ROAD)
                        {
                            bridgeX--;
                        }
                        bridgeX++;
                        map[bridgeY, bridgeX] = Terrain.BRIDGE;
                        bridge.Ypos = bridgeY + 30;
                        bridge.Xpos = bridgeX;

                        while (bridgeX < MAP_COLS)
                        {
                            if (map[bridgeY, bridgeX] == Terrain.PALACE
                                || map[bridgeY, bridgeX] == Terrain.CAVE
                                || locationAtPalace4.Xpos == bridgeX && locationAtPalace4.Ypos - 30 == bridgeY
                                || cave1 != null && cave1.Xpos == bridgeX && cave1.Ypos - 30 == bridgeY
                                || cave2 != null && cave2.Xpos == bridgeX && cave2.Ypos - 30 == bridgeY)
                            {
                                return false;
                            }
                            else
                            {
                                map[bridgeY, bridgeX] = Terrain.BRIDGE;
                            }
                            bridgeX++;
                        }
                    }
                }


                foreach (Location location in AllLocations)
                {
                    if (location.TerrainType == Terrain.ROAD)
                    {
                        int x = 0;
                        int y = 0;
                        if (location != magicContainerDrop && location != childDrop)
                        {
                            do
                            {
                                x = RNG.Next(19) + 2;
                                y = RNG.Next(MAP_ROWS - 4) + 2;
                            } while (map[y, x] != Terrain.ROAD || !((map[y, x + 1] == Terrain.MOUNTAIN && map[y, x - 1] == Terrain.MOUNTAIN) || (map[y + 1, x] == Terrain.MOUNTAIN && map[y - 1, x] == Terrain.MOUNTAIN)) || GetLocationByCoords((y + 30, x + 1)) != null || GetLocationByCoords((y + 30, x - 1)) != null || GetLocationByCoords((y + 31, x)) != null || GetLocationByCoords((y + 29, x)) != null || GetLocationByCoords((y + 30, x)) != null);
                        }
                        else
                        {
                            do
                            {
                                x = RNG.Next(19) + 2;
                                y = RNG.Next(MAP_ROWS - 4) + 2;
                            } while (map[y, x] != Terrain.ROAD || GetLocationByCoords((y + 30, x + 1)) != null || GetLocationByCoords((y + 30, x - 1)) != null || GetLocationByCoords((y + 31, x)) != null || GetLocationByCoords((y + 29, x)) != null || GetLocationByCoords((y + 30, x)) != null);
                        }

                        location.Xpos = x;
                        location.Ypos = y + 30;
                    }
                }

                if (!ValidateCaves())
                {
                    return false;
                }

                //check bytes and adjust
                MAP_COLS = 64;
                bytesWritten = WriteMapToRom(rom, false, MAP_ADDR, MAP_SIZE_BYTES, 0, 0, props.HiddenPalace, props.HiddenKasuto);
                MAP_COLS = 23;
                
            }
        }
        MAP_COLS = 64;
        WriteMapToRom(rom, true, MAP_ADDR, MAP_SIZE_BYTES, 0, 0, props.HiddenPalace, props.HiddenKasuto);
        for (int i = 0xA10C; i < 0xA149; i++)
        {
            if(!terrains.Keys.Contains(i))
            {
                rom.Put(i, 0x00);
            }
        }

        visitation = new bool[MAP_ROWS, MAP_COLS];
        for (int i = 0; i < MAP_ROWS; i++)
        {
            for (int j = 0; j < MAP_COLS; j++)
            {
                visitation[i, j] = false;
            }
        }
        return true;
    }

    private void SealDeadEnd(int curry, int currx)
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
                map[curry - 1, currx] = Terrain.MOUNTAIN;
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
                map[curry, currx - 1] = Terrain.MOUNTAIN;
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
                map[curry, currx+1] = Terrain.MOUNTAIN;
            }
            else
            {
                foundRoad = true;
            }
        }
    }

    private bool MoreToVisit(bool[,] v)
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

    private List<(int, int)> GetListOfNeighbors(int currx, int curry, bool[,] v)
    {
        List<(int, int)> x = [];

        if (currx - 2 > 1 && v[curry, currx - 2] == false)
        {
            x.Add((currx - 2, curry));
        }

        if (currx + 2 < MAP_COLS && v[curry, currx + 2] == false)
        {
            x.Add((currx + 2, curry));
        }

        if (curry - 2 > 1 && v[curry - 2, currx] == false)
        {
            x.Add((currx, curry - 2));
        }

        if (curry + 2 < MAP_ROWS && v[curry + 2, currx] == false)
        {
            x.Add((currx, curry + 2));
        }
        return x;
    }

    private void DrawLine((int, int) from, (int, int) to, Terrain t)
    {
        DrawLine(from.Item1, from.Item2, to.Item1, to.Item2, t);
    }

    private void DrawLine(int fromY, int fromX, int toY, int toX, Terrain terrain)
    {
        while (fromX != toX)
        {
            if (fromX == 21 || (RNG.NextDouble() > .5 && fromX != toX))
            {
                int diff = toX - fromX;
                int move = (RNG.Next(Math.Abs(diff / 2)) + 1) * 2;

 
                while (Math.Abs(move) > 0 && !(fromX == toX && fromY == toY))
                {
                    for (int i = 0; i < 2; i++)
                    {
                        if ((fromX != toX || fromY != (toY)) && GetLocationByCoords((fromY, fromX)) == null)
                        {
                            if(map[fromY, fromX] == Terrain.MOUNTAIN)
                            {
                                map[fromY, fromX] = terrain;
                            }
                            else if (map[fromY, fromX] == Terrain.ROAD && ((diff > 0 && (map[fromY, fromX + 1] == Terrain.MOUNTAIN)) || (diff < 0 && map[fromY, fromX - 1] == Terrain.MOUNTAIN)))
                            {
                                map[fromY, fromX] = Terrain.BRIDGE;
                            }
                            else if (map[fromY, fromX] != Terrain.PALACE && map[fromY, fromX] != Terrain.BRIDGE && map[fromY, fromX] != Terrain.CAVE)
                            {
                                map[fromY, fromX] = terrain;
                            }

                        }
                        if (diff > 0 && fromX < MAP_COLS - 1)
                        {
                            fromX++;
                            
                        }
                        else if (fromX > 0)
                        {
                            fromX--;
                            
                        }
                        
                        move--;
                    }
                }
            }
            else if(fromY != toY)
            {
                int diff = toY - fromY;
                int move = (RNG.Next(Math.Abs(diff / 2)) + 1) * 2;
                while (Math.Abs(move) > 0 && !(fromX == toX && fromY == toY))
                {
                    for (int i = 0; i < 2; i++)
                    {
                        if ((fromX != toX || fromY != (toY)) && GetLocationByCoords((fromY, fromX)) == null)
                        {
                            if (map[fromY, fromX] == Terrain.MOUNTAIN)
                            {
                                map[fromY, fromX] = terrain;
                            }
                            else if(map[fromY, fromX] == Terrain.ROAD && ((diff > 0 && (map[fromY + 1, fromX] == Terrain.MOUNTAIN)) || (diff < 0 && map[fromY - 1, fromX] == Terrain.MOUNTAIN)))
                            {
                                map[fromY, fromX] = Terrain.BRIDGE;
                            }
                            else if (map[fromY, fromX] != Terrain.PALACE && map[fromY, fromX] != Terrain.BRIDGE && map[fromY, fromX] != Terrain.CAVE)
                            {
                                map[fromY, fromX] = terrain;
                            }
                        }
                        if (diff > 0 && fromY < MAP_ROWS - 1)
                        {
                            fromY++;
                            
                        }
                        else if (fromY > 0)
                        {
                            fromY--;
                            
                        }
                        move--;
                    }
                }
            }
        }
    }
    public override void UpdateVisit(Dictionary<Collectable, bool> itemGet)
    {
        bool changed = true;
        while (changed)
        {
            changed = false;
            for (int i = 0; i < MAP_ROWS; i++)
            {
                for (int j = 0; j < MAP_COLS; j++)
                {
                    if (!visitation[i, j]
                    && (
                        (map[i, j] == Terrain.WALKABLEWATER && itemGet[Collectable.BOOTS])
                        || map[i, j] == Terrain.ROAD || map[i, j] == Terrain.PALACE
                        || map[i, j] == Terrain.BRIDGE
                        || map[i, j] == Terrain.CAVE
                        )
                    )
                    {
                        if (i - 1 >= 0)
                        {
                            if (visitation[i - 1, j])
                            {
                                visitation[i, j] = true;
                                changed = true;
                                continue;
                            }

                        }

                        if (i + 1 < MAP_ROWS)
                        {
                            if (visitation[i + 1, j])
                            {
                                visitation[i, j] = true;
                                changed = true;
                                continue;
                            }
                        }

                        if (j - 1 >= 0)
                        {
                            if (visitation[i, j - 1])
                            {
                                visitation[i, j] = true;
                                changed = true;
                                continue;
                            }
                        }

                        if (j + 1 < MAP_COLS)
                        {
                            if (visitation[i, j + 1])
                            {
                                visitation[i, j] = true;
                                changed = true;
                                continue;
                            }
                        }
                    }
                }
            }
        }

        foreach (Location location in AllLocations)
        {
            if (visitation[location.Ypos - 30, location.Xpos])
            {
                location.Reachable = true;
            }
        }
    }

    protected override List<Location> GetPathingStarts()
    {
        throw new NotImplementedException("Maze island does not use UpdateReachable so it does not have pathing starts.");
    }

    public override string GetName()
    {
        return "Maze Island";
    }

    public override IEnumerable<Location> RequiredLocations(bool hiddenPalace, bool hiddenKasuto)
    {
        /*
            public Location childDrop;
            public Location magicContainerDrop;
            public Location locationAtPalace4;
        */
        HashSet<Location> requiredLocations = new()
        {
            childDrop,
            magicContainerDrop,
            locationAtPalace4
        };

        foreach (Location key in connections.Keys)
        {
            if (requiredLocations.TryGetValue(key, out Location? value))
            {
                requiredLocations.Add(key);
            }
        }
        return requiredLocations.Where(i => i != null);
    }

    protected override void SetVanillaCollectables(bool useDash)
    {
        locationAtPalace4.VanillaCollectable = Collectable.BOOTS;
        childDrop.VanillaCollectable = Collectable.CHILD;
        magicContainerDrop.VanillaCollectable = Collectable.MAGIC_CONTAINER;
    }

    public override string GenerateSpoiler()
    {
        StringBuilder sb = new();
        sb.AppendLine("MAZE ISLAND: ");
        sb.AppendLine("\tMagic Container Drop: " + magicContainerDrop.Collectable.EnglishText());
        sb.AppendLine("\tChild Drop: " + childDrop.Collectable.EnglishText());

        sb.AppendLine("\tPalace 4 (" + locationAtPalace4.PalaceNumber + "): " + locationAtPalace4.Collectable.EnglishText());
        sb.AppendLine();
        return sb.ToString();
    }
}
