using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using Z2Randomizer.RandomizerCore.Enemy;

namespace Z2Randomizer.RandomizerCore.Overworld;

sealed class MazeIsland : World
{
    public static readonly int[] OverworldEnemies = [03, 04, 05, 0x11, 0x12, 0x14, 0x16, 0x18, 0x19, 0x1A, 0x1B, 0x1C];
    public static readonly int[] OverworldFlyingEnemies = [0x06, 0x07, 0x0A, 0x0D, 0x0E, 0x15];
    public static readonly int[] OverworldGenerators = [0x0B, 0x0F, 0x17];
    public static readonly int[] OverworldSmallEnemies = [0x03, 0x04, 0x05, 0x11, 0x12, 0x16];
    public static readonly int[] OverworldLargeEnemies = [0x14, 0x18, 0x19, 0x1A, 0x1B, 0x1C];

    private readonly SortedDictionary<LocationID, Terrain> terrains = new()
    {
        { LocationID.MI_TRAP1, Terrain.ROAD },
        { LocationID.MI_TRAP2, Terrain.ROAD },
        { LocationID.MI_MAGIC_CONTAINER_DROP, Terrain.ROAD },
        { LocationID.MI_CONNECTOR_BRIDGE, Terrain.BRIDGE },
        { LocationID.MI_PALACE4, Terrain.PALACE },
        { LocationID.MI_CHILD_DROP, Terrain.ROAD },
        { LocationID.MI_TRAP3, Terrain.ROAD },
        { LocationID.MI_TRAP4, Terrain.ROAD },
        { LocationID.MI_TRAP5, Terrain.ROAD },
        { LocationID.MI_TRAP6, Terrain.ROAD },
        { LocationID.MI_TRAP7, Terrain.ROAD },
    };

    public Location childDrop;
    public Location magicContainerDrop;
    public Location locationAtPalace4;

    private const int MAP_ADDR = 0xba00;


    public MazeIsland(RandomizerProperties props, Random r, ROM rom) : base(r)
    {
        List<Location> trapLocations = [
            .. rom.LoadLocations(LocationID.MI_TRAP1, 2, terrains),
            .. rom.LoadLocations(LocationID.MI_TRAP3, 5, terrains),
        ];

        List < Location> locations =
        [
            .. rom.LoadLocations(LocationID.MI_MAGIC_CONTAINER_DROP, 1, terrains),
            .. rom.LoadLocations(LocationID.MI_PALACE4, 1, terrains),
            .. rom.LoadLocations(LocationID.MI_CHILD_DROP, 1, terrains),
            .. trapLocations,
        ];
        locations.ForEach(AddLocation);

        walkableTerrains = [Terrain.MOUNTAIN];

        sideviewPtrTable = 0xA010;
        sideviewBank = 2;
        enemyPtr = 0xA08E;
        groupedEnemies = Enemies.GroupedEastEnemies;
        overworldEncounterMaps = [];
        nonEncounterMaps = [
            34, // MAZE_ISLAND_FORCED_BATTLE_2
            35, // MAZE_ISLAND_FORCED_BATTLE_1
            36, // MAZE_ISLAND_MAGIC
            37, // MAZE_ISLAND_CHILD
            47, // MAZE_ISLAND_FORCED_BATTLE_3
            48, // MAZE_ISLAND_FORCED_BATTLE_7
            49, // MAZE_ISLAND_FORCED_BATTLE_4
            50, // MAZE_ISLAND_FORCED_BATTLE_5
            51, // MAZE_ISLAND_FORCED_BATTLE_6
        ];

        childDrop = GetLocation(LocationID.MI_CHILD_DROP);
        magicContainerDrop = GetLocation(LocationID.MI_MAGIC_CONTAINER_DROP);
        locationAtPalace4 = GetLocation(LocationID.MI_PALACE4);

        continentId = Continent.MAZE;
        baseAddr = RomMap.ContinentLocationBases[continentId];
        VANILLA_MAP_ADDR = 0xa65c;

        biome = props.MazeBiome;
        if (biome.UsesVanillaMap())
        {
            MapRows = 75;
            MapColumns = 64;
        }
        else
        {
            var meta = props.MazeSize.GetMeta();
            MapColumns = meta.Height;
            MapRows = meta.Width;

            // TODO: use metadata for num caves to remove
            var trapTilesToRemove = props.MazeSize switch
            {
                MazeSizeOption.LARGE => 0,
                MazeSizeOption.MEDIUM => 1,
                MazeSizeOption.SMALL => 2,
                _ => throw new NotImplementedException(),
            };

            for (int i = 0; i < trapTilesToRemove; i++)
            {
                var j = r.Next(trapLocations.Count);
                var removeLoc = trapLocations[j];
                RemoveLocations([removeLoc]);
                trapLocations.Remove(removeLoc);
            }
        }
        //SetVanillaCollectables(props.ReplaceFireWithDash);
    }

    public override bool Terraform(RandomizerProperties props, ROM rom)
    {
        if (biome.UsesVanillaMap())
        {
            Debug.Assert(MapRows == 75);
            Debug.Assert(MapColumns == 64);
            map = new OverworldMap(rom.ReadVanillaMap(rom, VANILLA_MAP_ADDR, MapRows, MapColumns));
            if (biome == Biome.VANILLA_SHUFFLE)
            {
                ShuffleLocations(AllLocations);
                if (!props.LegacyVanillaShuffledLocations)
                {
                    foreach (Location location in AllLocations)
                    {
                        map[location.Y, location.Xpos] = location.TerrainType;
                        location.IsPassthrough = true;
                    }
                }
            }
        }
        else
        {
            int bytesWritten = 2000;
            foreach (Location location in AllLocations)
            {
                location.CanShuffle = true;
                location.IsPassthrough = location.WasPassthrough;
                location.ResetCoords();
            }
            while (bytesWritten > MAP_SIZE_BYTES)
            {
                map = new OverworldMap(MapRows, 64);
                bool[,] visited = new bool[MapRows, MapColumns];
                List<Location> placedLocations = new();

                // create walkable water border
                for (int x = 0; x < MapColumns; x++)
                {
                    IntVector2 top = new(x, 0);
                    map[top] = Terrain.WALKABLEWATER;
                    visited[top.Y, top.X] = true;
                    IntVector2 bottom = new(x, MapRows - 1);
                    map[bottom] = Terrain.WALKABLEWATER;
                    visited[bottom.Y, bottom.X] = true;
                }
                for (int y = 0; y < MapRows; y++)
                {
                    IntVector2 left = new(0, y);
                    map[left] = Terrain.WALKABLEWATER;
                    visited[left.Y, left.X] = true;
                    IntVector2 right = new(MapColumns - 1, y);
                    map[right] = Terrain.WALKABLEWATER;
                    visited[right.Y, left.X] = true;
                }

                // fill non-walkable water to the right of the island water border
                for (int y = 0; y < MapRows; y++)
                    {
                    for (int x = MapColumns; x < 64; x++)
                    {
                        map[y, x] = Terrain.WATER;
                    }
                }

                // fill every other row with mountain
                for (int y = 1; y < MapRows - 1; y += 2)
                {
                    for (int x = 1; x < MapColumns - 1; x++)
                    {
                        map[y, x] = Terrain.MOUNTAIN;
                    }
                }

                // fill every other column with mountain
                for (int x = 1; x < MapColumns; x += 2)
                {
                    for (int y = 1; y < MapRows - 1; y++)
                    {
                        map[y, x] = Terrain.MOUNTAIN;
                        }
                    }

                for (int y = 1; y < MapRows; y++)
                {
                    for (int x = 1; x < MapColumns; x++)
                    {
                        if (map[y, x] != Terrain.MOUNTAIN && map[y, x] != Terrain.WATER && map[y, x] != Terrain.WALKABLEWATER)
                        {
                            map[y, x] = Terrain.ROAD;
                            visited[y, x] = false;
                        }
                        else
                        {
                            visited[y, x] = true;
                        }
                    }
                }

                //choose starting Y position
                int starty = RNG.Next(2, MapRows - 1);
                if (starty % 2 == 1)
                {
                    starty--;
                }

                //generate maze
                IntVector2 current = new(2, starty);
                Stack<IntVector2> stack = new();
                bool canPlaceCave = true;
                while (MoreToVisit(visited))
                {
                    var neighbors = GetPositionsTwoTilesAway(current, visited, RNG).ToArray();
                    RNG.Shuffle(neighbors);
                    if (neighbors.Length > 0)
                    {
                        canPlaceCave = true;
                        var next = neighbors[RNG.Next(neighbors.Length)];
                        stack.Push(next);
                        var delta = (next - current) / 2; // neighbors are all 2 tiles away
                        map[current + delta] = Terrain.ROAD;
                        current = next;
                        visited[current.Y, current.X] = true;
                    }
                    else if (stack.Count > 0)
                    {
                        if (cave1 != null && cave1.CanShuffle && GetLocationAt(current) == null)
                        {
                            map[current] = Terrain.CAVE;
                            cave1.Pos = current;
                            cave1.CanShuffle = false;
                            canPlaceCave = false;
                            SealDeadEnd(current, RNG);
                            placedLocations.Add(cave1);
                        }
                        else if (cave2 != null && cave2.CanShuffle && GetLocationAt(current) == null && canPlaceCave)
                        {
                            map[current] = Terrain.CAVE;
                            cave2.Pos = current;
                            cave2.CanShuffle = false;
                            SealDeadEnd(current, RNG);
                            placedLocations.Add(cave2);
                        }
                        current = stack.Pop();
                    }
                }

                //place palace 4
                bool canPlace = false;
                IntVector2 palace4Pos;
                do
                {
                    palace4Pos = IntVector2.Random(RNG, 3, MapColumns - 4, 3, MapRows - 4);
                    if (palace4Pos.X % 2 == 0) { palace4Pos += IntVector2.EAST; }
                    if (palace4Pos.Y % 2 == 0) { palace4Pos += IntVector2.SOUTH; }
                    canPlace = true;
                    if (LocationsIn3x3Area(palace4Pos).Any())
                    {
                        canPlace = false;
                    }
                } while (!canPlace);
                locationAtPalace4.Pos = palace4Pos;
                map[palace4Pos] = Terrain.PALACE;
                foreach (var dir in IntVector2.DIRECTIONS)
                    {
                    map[palace4Pos + dir] = Terrain.ROAD;
                            }
                placedLocations.Add(locationAtPalace4);

                //draw a river
                bool openWest, openEast;
                do
                {
                    openWest = RNG.Next(2) == 1;
                    openEast = RNG.Next(2) == 1;
                } while (!openWest && !openEast);

                int riverEndX = MapColumns - 2;
                int riverPivotX, riverStartY, riverEndY;
                do
                {
                    riverStartY = RNG.Next((MapRows - 5) / 2) * 2 + 3;
                } while (riverStartY == starty || Math.Abs(palace4Pos.Y - riverStartY) < 2);

                do
                    {
                    riverEndY = RNG.Next((MapRows - 5) / 2) * 2 + 3;
                } while (Math.Abs(riverStartY - riverEndY) < 2 || Math.Abs(palace4Pos.Y - riverEndY) < 2);

                do
                        {
                    riverPivotX = RNG.Next(1, riverEndX / 2) * 2 + 1; //3,5,7,9,11,13,15,17,19
                } while (Math.Abs(palace4Pos.X - riverPivotX) < 2);

                Debug.Assert(riverEndX % 2 == 1); // even number loops forever
                Debug.Assert(riverEndY % 2 == 1);
                DrawRiver(riverStartY, 1, riverEndY, riverEndX, riverPivotX, openWest, openEast);

                //Pick raft & bridge edges
                Direction raftDirEnum = (Direction)RNG.Next(4);
                Direction bridgeDirEnum;
                do
                    {
                    bridgeDirEnum = (Direction)RNG.Next(4);
                } while (bridgeDirEnum == raftDirEnum);

                //Place raft
                if (raft != null)
                        {
                    IntVector2 raftDirVec = raftDirEnum.ToIntVector2();
                    IntVector2 raftPos, nextToRaft;
                    do
                            {
                        raftPos = raftDirEnum switch
                    {
                            Direction.NORTH => new IntVector2(RNG.Next(2, MapColumns - 2), 1),
                            Direction.SOUTH => new IntVector2(RNG.Next(2, MapColumns - 2), MapRows - 2),
                            Direction.WEST => new IntVector2(1, RNG.Next(2, MapRows - 2)),
                            Direction.EAST => new IntVector2(MapColumns - 2, RNG.Next(2, MapRows - 2)),
                            _ => throw new ArgumentException("Invalid direction: " + raftDirEnum)
                        };
                        nextToRaft = raftPos - raftDirVec;
                    } while (map[raftPos] is not Terrain.MOUNTAIN || map[nextToRaft] is not Terrain.ROAD);

                    map[raftPos] = Terrain.BRIDGE;
                    raft.Pos = raftPos;
                            }

                //Place bridge
                if (bridge != null)
                {
                    IntVector2 bridgeDirVec = bridgeDirEnum.ToIntVector2();
                    IntVector2 bridgePos, nextToBridge;

                    do
                    {
                        bridgePos = bridgeDirEnum switch
                        {
                            Direction.NORTH => new IntVector2(RNG.Next(2, MapColumns - 2), 1),
                            Direction.SOUTH => new IntVector2(RNG.Next(2, MapColumns - 2), MapRows - 2),
                            Direction.WEST => new IntVector2(1, RNG.Next(2, MapRows - 2)),
                            Direction.EAST => new IntVector2(MapColumns - 2, RNG.Next(2, MapRows - 2)),
                            _ => throw new ArgumentException("Invalid direction: " + bridgeDirEnum)
                        };
                        nextToBridge = bridgePos - bridgeDirVec;
                    } while (map[bridgePos] is not Terrain.MOUNTAIN || map[nextToBridge] is not Terrain.ROAD);

                    IntVector2 waterByBridge = bridgePos + bridgeDirVec;
                    map[bridgePos] = Terrain.BRIDGE;
                    bridge.Pos = bridgePos;
                    map[waterByBridge] = Terrain.BRIDGE;
                }

                foreach (Location location in AllLocations)
                {
                    if (location.TerrainType == Terrain.ROAD)
                    {
                        while (true)
                        {
                            var pos = IntVector2.Random(RNG, 2, MapColumns - 2, 2, MapRows - 2);
                            if (ValidMazeDropPosition(pos))
                            {
                                location.Pos = pos;
                                break;
                            }
                        }
                    }
                }

                if (!ValidateCaves())
                {
                    return false;
                }

                //check bytes and adjust
                bytesWritten = WriteMapToRom(rom, false, MAP_ADDR, MAP_SIZE_BYTES, 0, 0, props.HiddenPalace, props.HiddenKasuto);
                
            }
        }
        WriteMapToRom(rom, true, MAP_ADDR, MAP_SIZE_BYTES, 0, 0, props.HiddenPalace, props.HiddenKasuto);
        foreach (var lid in LocationIDUtils.Enumerate(Continent.MAZE))
        {
            if(!terrains.Keys.Contains(lid))
            {
                rom.Put(lid, 0, 0x00);
            }
        }

        visitation = new bool[MapRows, MapColumns];
        for (int i = 0; i < MapRows; i++)
        {
            for (int j = 0; j < MapColumns; j++)
            {
                visitation[i, j] = false;
            }
        }
        return true;
    }

    private void SealDeadEnd(IntVector2 pos, Random r)
    {
        var cardinalOrder = IntVector2.CARDINALS.ToArray();
        r.Shuffle(cardinalOrder);
        bool foundRoad = false;
        foreach (IntVector2 dir in cardinalOrder)
        {
            IntVector2 neighbor = pos + dir;
        
            if (map[neighbor] != Terrain.ROAD)
        {
                continue;
            }
            if (foundRoad)
            {
                map[neighbor] = Terrain.MOUNTAIN;
            }
            else
            {
                foundRoad = true;
            }
        }
            }

    private bool MoreToVisit(bool[,] v)
    {
        for (int y = 0; y < v.GetLength(0); y++)
        {
            for (int x = 1; x < v.GetLength(1) - 1; x++)
            {
                if (v[y, x] == false)
                {
                    return true;
                }
            }
        }
        return false;
    }

    private List<IntVector2> GetPositionsTwoTilesAway(IntVector2 current, bool[,] visited, Random r)
    {
        List<IntVector2> neighbors = [];

        foreach (IntVector2 dir in IntVector2.CARDINALS)
        {
            IntVector2 candidate = current + 2 * dir;
            if (!WithinMapBounds(candidate, 2))
        {
                continue;
        }
            if (!visited[candidate.Y, candidate.X])
        {
                neighbors.Add(candidate);
        }
        }

        return neighbors;
        }

    private void DrawRiver(int fromY, int fromX, int toY, int toX, int xPivot, bool openWest, bool openEast)
    {
        Terrain terrain = Terrain.WALKABLEWATER;
        IntVector2 startPos = new(fromX, fromY);
        IntVector2 pos = startPos;
        IntVector2 endPos = new(toX, toY);

        IntVector2 horizontalStep = endPos.X > pos.X ? IntVector2.EAST : IntVector2.WEST;
        IntVector2 verticalStep = endPos.Y > pos.Y ? IntVector2.SOUTH : IntVector2.NORTH;

        while (pos.X != endPos.X)
                {
            if (pos.X == xPivot && pos.Y != endPos.Y)
                    {
                PaintRiverTile(pos, terrain, horizontalStep);
                pos += verticalStep;
                            }
            else
                            {
                PaintRiverTile(pos, terrain, verticalStep);
                pos += horizontalStep;
                            }

            map[startPos] = openWest ? Terrain.WALKABLEWATER : Terrain.MOUNTAIN;
            map[endPos] = openEast ? Terrain.WALKABLEWATER : Terrain.MOUNTAIN;
                        }
                        }

    private void PaintRiverTile(IntVector2 pos, Terrain terrain, IntVector2 perpendicular)
            {
        if (GetLocationAt(pos) != null)
                {
            return;
                            }
        Terrain current = map[pos];
        if (current == Terrain.MOUNTAIN)
                            {
            map[pos] = terrain;
                            }
        else if (current == Terrain.ROAD)
                            {
            IntVector2 leftOf = pos - perpendicular;
            IntVector2 rightOf = pos + perpendicular;
            if (map[leftOf] != Terrain.MOUNTAIN && map[rightOf] != Terrain.MOUNTAIN)
                        {
                map[pos] = Terrain.BRIDGE;
                        }
            else
                        {
                map[pos] = terrain;
                        }
                    }
        else if (current != Terrain.PALACE && current != Terrain.BRIDGE && current != Terrain.CAVE)
            {
            map[pos] = terrain;
            }
        }
    public override void UpdateVisit(IReadOnlySet<RequirementType> requireables)
    {
        bool changed = true;
        while (changed)
        {
            changed = false;
            for (int i = 0; i < MapRows; i++)
            {
                for (int j = 0; j < MapColumns; j++)
                {
                    if (!visitation[i, j]
                    && (
                        (map[i, j] == Terrain.WALKABLEWATER && requireables.Contains(RequirementType.BOOTS))
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

                        if (i + 1 < MapRows)
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

                        if (j + 1 < MapColumns)
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

        //DebugVisitation();
        foreach (Location location in AllLocations)
        {
            if (visitation[location.Y, location.Xpos])
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

    /*
    protected override void SetVanillaCollectables(bool useDash)
    {
        locationAtPalace4.VanillaCollectable = Collectable.BOOTS;
        childDrop.VanillaCollectable = Collectable.CHILD;
        magicContainerDrop.VanillaCollectable = Collectable.MAGIC_CONTAINER;
    }
    */

    public override string GenerateSpoiler()
    {
        StringBuilder sb = new();
        sb.AppendLine("MAZE ISLAND: ");
        sb.AppendLine("\tMagic Container Drop: " + magicContainerDrop.GetAllCollectables()[0].EnglishText());
        sb.AppendLine("\tChild Drop: " + childDrop.GetAllCollectables()[0].EnglishText());

        sb.Append("\tPalace 4 (" + locationAtPalace4.Palace!.Number + "): ");
        List<Collectable> palaceCollectables = locationAtPalace4.GetAllCollectables();
        sb.AppendLine(palaceCollectables.Count == 0 ? "No Items" : string.Join(", ", palaceCollectables.Select(c => c.EnglishText())));

        sb.AppendLine();
        return sb.ToString();
    }

    public override void DisableDisallowedPassthroughs()
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
        if (bridge != null)
        {
            bridge.IsPassthrough = false;
        }
        magicContainerDrop.IsPassthrough = false;
        childDrop.IsPassthrough = false;
    }

    public override void ResetCollectables(RandomizerProperties props)
    {
        childDrop.SetCollectables([Collectable.CHILD]);
        childDrop.CollectablesAreShufflable = props.ShuffleOverworldItems;
        magicContainerDrop.SetCollectables([Collectable.MAGIC_CONTAINER]);
        magicContainerDrop.CollectablesAreShufflable = props.ShuffleOverworldItems;

        locationAtPalace4.SetCollectables(locationAtPalace4.Palace!
            .GetVanillaCollectables(props.PalaceItemRoomCounts[locationAtPalace4.Palace!.Number - 1]));
    }
}
