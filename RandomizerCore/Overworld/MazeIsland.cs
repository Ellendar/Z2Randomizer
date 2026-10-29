using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
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


    public Location childDrop;
    public Location magicContainerDrop;
    public Location locationAtPalace4;


    public MazeIsland(RandomizerProperties props, Random r, ROM rom) : base(r, RomMap.MI_MAP_OFFSET)
    {
        List<Location> trapLocations = [
            .. rom.LoadLocations(LocationID.MI_TRAP1, 2),
            .. rom.LoadLocations(LocationID.MI_TRAP3, 5),
        ];

        List<Location> locations =
        [
            .. rom.LoadLocations(LocationID.MI_MAGIC_CONTAINER_DROP, 1),
            .. rom.LoadLocations(LocationID.MI_PALACE4, 1),
            .. rom.LoadLocations(LocationID.MI_CHILD_DROP, 1),
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
        baseAddr = RomMap.MI_LOCATIONS_BASE;
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

            // TODO: use metadata for num traps to remove
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
    }

    public override bool Terraform(RandomizerProperties props, ROM rom)
    {
        using var candidate = BeginTerrainCandidate(rom);
        EncodedWorldMap? encodedMap;
        if (biome.UsesVanillaMap())
        {
            LoadAndShuffleVanillaMap(rom, props.LegacyVanillaShuffledLocations);
            encodedMap = EncodeTerrainMap();
        }
        else // Generate biome terrain
        {
            if (!TryGenerateMazeMap(out encodedMap))
            {
                return false;
            }
        }

        if (!TryStageEncodedTerrainMap(encodedMap)) return false;
        ClearUnusedLocationYBytes(Continent.MAZE, TerrainWrites.Put);
        InitializeVisitation();

        candidate.Commit();
        return true;
    }

    private protected override void RepaintVanillaShuffleTiles()
        => RepaintLocationTiles(markPassthrough: true);

    /// <summary>Resets placement and builds one maze whose encoding fits the ROM budget.</summary>
    private bool TryGenerateMazeMap([NotNullWhen(true)] out EncodedWorldMap? encodedMap)
    {
        encodedMap = null;
        PrepareTerraformLocations();
        // Tracks maze carving progress; it remains local to this attempt.
        bool[,] relaxed = new bool[MapRows, MapColumns];
        InitializeMazeMap(relaxed);

        //choose starting Y position
        IntVector2 mazeStart = new(2, RNG.Next(2, MapRows - 1));
        if (mazeStart.Y % 2 == 1)
        {
            mazeStart += IntVector2.NORTH;
        }

        //generate maze
        IntVector2 current = mazeStart;
        Stack<IntVector2> stack = new();
        while (MoreToVisit(relaxed))
        {
            var neighbors = GetPositionsTwoTilesAway(current, relaxed).ToArray();
            RNG.Shuffle(neighbors);
            if (neighbors.Length > 0)
            {
                var next = neighbors[RNG.Next(neighbors.Length)];
                stack.Push(next);
                var delta = (next - current) / 2; // neighbors are all 2 tiles away
                map[current + delta] = Terrain.ROAD;
                current = next;
                relaxed[current.Y, current.X] = true;
            }
            else if (stack.Count > 0)
            {
                current = stack.Pop();
            }
        }

        if (!TryPlaceMazePalace(out var palacePosition)) return false;
        if (!TryDrawMazeRiver(mazeStart, palacePosition)) return false;
        if (!TryPlaceMazeConnectors()) return false;
        if (!TryPlaceMazeEncounters()) return false;
        if (!TryRepairAndValidateCaveExits()) return false;

        // Current supported sizes always fit; reject safely if future dimensions exceed the budget.
        var candidate = EncodeTerrainMap();
        if (candidate.Length > MapRomCapacity)
        {
            return false;
        }

        encodedMap = candidate;
        return true;
    }

    /// <summary>
    /// Builds the uncarved island grid and initializes carving progress.
    /// The playable border is walkable water. The encoder adds deep water beyond it.
    /// </summary>
    private void InitializeMazeMap(bool[,] relaxed)
    {
        map = new OverworldMap(MapRows, MapColumns);
        // create walkable water border
        for (int x = 0; x < MapColumns; x++)
        {
            IntVector2 top = new(x, 0);
            map[top] = Terrain.WALKABLEWATER;
            relaxed[top.Y, top.X] = true;
            IntVector2 bottom = new(x, MapRows - 1);
            map[bottom] = Terrain.WALKABLEWATER;
            relaxed[bottom.Y, bottom.X] = true;
        }
        for (int y = 0; y < MapRows; y++)
        {
            IntVector2 left = new(0, y);
            map[left] = Terrain.WALKABLEWATER;
            relaxed[left.Y, left.X] = true;
            IntVector2 right = new(MapColumns - 1, y);
            map[right] = Terrain.WALKABLEWATER;
            relaxed[right.Y, right.X] = true;
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
                IntVector2 position = new(x, y);
                Terrain terrain = map[position];
                if (terrain != Terrain.MOUNTAIN && terrain != Terrain.WATER && terrain != Terrain.WALKABLEWATER)
                {
                    map[position] = Terrain.ROAD;
                    relaxed[position.Y, position.X] = false;
                }
                else
                {
                    relaxed[position.Y, position.X] = true;
                }
            }
        }
    }

    /// <summary>Places Palace 4 away from existing locations and opens its eight neighboring tiles.</summary>
    private bool TryPlaceMazePalace(out IntVector2 palacePosition)
    {
        List<IntVector2> candidates = [];
        for (int y = 3; y < MapRows - 4; y++)
            for (int x = 3; x < MapColumns - 4; x++)
            {
                // Preserve the weights from rounding each sampled even coordinate upward.
                // Several raw coordinate pairs can therefore represent the same odd center.
                var position = new IntVector2(x % 2 == 0 ? x + 1 : x, y % 2 == 0 ? y + 1 : y);
                if (!LocationsIn3x3Area(position).Any()) candidates.Add(position);
            }
        palacePosition = default;
        if (candidates.Count == 0) return false;
        palacePosition = candidates[RNG.Next(candidates.Count)];
        locationAtPalace4.Pos = palacePosition;
        map[palacePosition] = Terrain.PALACE;
        foreach (var direction in IntVector2.DIRECTIONS) map[palacePosition + direction] = Terrain.ROAD;
        return true;
    }

    /// <summary>Selects a complete river plan avoiding the carving start row and palace opening before painting.</summary>
    private bool TryDrawMazeRiver(IntVector2 mazeStart, IntVector2 palacePosition)
    {
        List<int> startRows = [];
        List<int> endRows = [];
        for (int sample = 0; sample < (MapRows - 5) / 2; sample++)
        {
            int row = sample * 2 + 3;
            if (Math.Abs(palacePosition.Y - row) < 2) continue;
            endRows.Add(row);
            if (row != mazeStart.Y) startRows.Add(row);
        }
        List<int> pivotColumns = [];
        for (int sample = 1; sample < (MapColumns - 2) / 2; sample++)
        {
            int column = sample * 2 + 1;
            if (Math.Abs(palacePosition.X - column) >= 2) pivotColumns.Add(column);
        }
        // Every allowed start belongs to endRows, so each has endRows.Count - 1 partners.
        if (startRows.Count == 0 || endRows.Count < 2 || pivotColumns.Count == 0) return false;
        int startY = startRows[RNG.Next(startRows.Count)];
        endRows.Remove(startY);
        var start = new IntVector2(1, startY);
        var end = new IntVector2(MapColumns - 2, endRows[RNG.Next(endRows.Count)]);
        int pivot = pivotColumns[RNG.Next(pivotColumns.Count)];
        // The three equally likely opening combinations exclude closing both ends.
        int openings = RNG.Next(1, 4);
        DrawRiver(start, end, pivot, (openings & 1) != 0, (openings & 2) != 0);
        return true;
    }

    /// <summary>Places caves in finished-maze dead ends, then paints present raft/bridge landings on distinct edges.</summary>
    private bool TryPlaceMazeConnectors()
    {
        if (!TryPlaceMazeCaves()) return false;
        List<Direction> availableEdges = [Direction.NORTH, Direction.EAST, Direction.SOUTH, Direction.WEST];
        if (raft != null)
        {
            var direction = availableEdges[RNG.Next(availableEdges.Count)];
            if (!TryChooseConnectorPosition(direction, out var position)) return false;
            raft.Pos = position;
            map[position] = Terrain.BRIDGE;
            availableEdges.Remove(direction);
        }
        if (bridge != null)
        {
            var direction = availableEdges[RNG.Next(availableEdges.Count)];
            if (!TryChooseConnectorPosition(direction, out var position)) return false;
            bridge.Pos = position;
            map[position] = Terrain.BRIDGE;
            map[position + direction.ToIntVector2()] = Terrain.BRIDGE;
        }
        return true;
    }

    /// <summary>Plans all loaded caves on unused ROAD dead ends, each with exactly one ROAD approach.</summary>
    private bool TryPlaceMazeCaves()
    {
        if (cave1 == null && cave2 == null) return true;
        List<IntVector2> available = [];
        // Carved maze cells occupy even coordinates; odd cells are the connecting passages.
        for (int y = 2; y < MapRows - 2; y += 2)
            for (int x = 2; x < MapColumns - 2; x += 2)
            {
                var position = new IntVector2(x, y);
                if (map[position] != Terrain.ROAD || LocationsIn3x3Area(position).Any()) continue;
                var neighbors = IntVector2.CARDINALS.Select(d => map[position + d]).ToArray();
                if (neighbors.Count(t => t == Terrain.ROAD) == 1
                    && neighbors.All(t => t is Terrain.ROAD or Terrain.MOUNTAIN)) available.Add(position);
            }

        List<(Location Location, IntVector2 Position)> planned = [];
        foreach (var cave in new[] { cave1, cave2 })
        {
            if (cave == null) continue;
            if (available.Count == 0) return false;
            var position = available[RNG.Next(available.Count)];
            planned.Add((cave, position));
            available.RemoveAll(p => Math.Abs(p.X - position.X) <= 1 && Math.Abs(p.Y - position.Y) <= 1);
        }
        foreach (var (cave, position) in planned)
        {
            cave.Pos = position;
            cave.CanShuffle = false;
            map[position] = Terrain.CAVE;
        }
        return true;
    }

    /// <summary>Chooses a mountain landing on the requested edge with an existing ROAD approach inward.</summary>
    private bool TryChooseConnectorPosition(Direction direction, out IntVector2 position)
    {
        var outward = direction.ToIntVector2();
        int span = direction.IsHorizontal() ? MapRows : MapColumns;
        List<IntVector2> candidates = [];
        for (int offset = 2; offset < span - 2; offset++)
        {
            var candidate = direction switch
            {
                Direction.NORTH => new IntVector2(offset, 1),
                Direction.SOUTH => new IntVector2(offset, MapRows - 2),
                Direction.WEST => new IntVector2(1, offset),
                Direction.EAST => new IntVector2(MapColumns - 2, offset),
                _ => throw new ArgumentException("Invalid direction: " + direction)
            };
            if (map[candidate] == Terrain.MOUNTAIN && map[candidate - outward] == Terrain.ROAD)
                candidates.Add(candidate);
        }
        position = default;
        if (candidates.Count == 0) return false;
        position = candidates[RNG.Next(candidates.Count)];
        return true;
    }

    /// <summary>Places ROAD records in their existing order, rejecting an empty drop/trap pool without moving that record.</summary>
    private bool TryPlaceMazeEncounters()
    {
        foreach (Location location in AllLocations)
        {
            if (location.TerrainType != Terrain.ROAD) continue;
            bool isDrop = location == magicContainerDrop || location == childDrop;
            List<IntVector2> candidates = [];
            for (int y = 2; y < MapRows - 2; y++)
                for (int x = 2; x < MapColumns - 2; x++)
                {
                    var position = new IntVector2(x, y);
                    bool valid = isDrop ? ValidMazeDropPosition(position) : ValidTrapTilePosition(position) != null;
                    if (valid) candidates.Add(position);
                }
            if (candidates.Count == 0) return false;
            location.Pos = candidates[RNG.Next(candidates.Count)];
        }
        return true;
    }

    /// <summary>Checks maze-carving progress, independently of gameplay visitation.</summary>
    private bool MoreToVisit(bool[,] relaxed)
    {
        for (int y = 0; y < relaxed.GetLength(0); y++)
        {
            for (int x = 1; x < relaxed.GetLength(1) - 1; x++)
            {
                if (relaxed[y, x] == false)
                {
                    return true;
                }
            }
        }
        return false;
    }

    private List<IntVector2> GetPositionsTwoTilesAway(IntVector2 current, bool[,] relaxed)
    {
        List<IntVector2> neighbors = [];

        foreach (IntVector2 dir in IntVector2.CARDINALS)
        {
            IntVector2 candidate = current + 2 * dir;
            if (!WithinMapBounds(candidate, 2))
            {
                continue;
            }
            if (!relaxed[candidate.Y, candidate.X])
            {
                neighbors.Add(candidate);
            }
        }

        return neighbors;
    }

    private void DrawRiver(IntVector2 startPos, IntVector2 endPos, int xPivot, bool openWest, bool openEast)
    {
        Terrain terrain = Terrain.WALKABLEWATER;
        IntVector2 pos = startPos;

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
        VisitStart(); // in case we support Maze Island start later
        ExpandMazeVisitation(requireables.Contains(RequirementType.BOOTS));
        MarkReachableMazeLocations(requireables);
    }

    /// <summary>Expands existing entrance/tile visitation in row order until no further tiles are reached.</summary>
    private void ExpandMazeVisitation(bool hasBoots)
    {
        bool changed = true;
        while (changed)
        {
            changed = false;
            for (int y = 0; y < MapRows; y++)
            {
                for (int x = 0; x < MapColumns; x++)
                {
                    if (visitation[y, x]) continue;
                    Terrain terrain = map[y, x];
                    bool passable = terrain is Terrain.ROAD or Terrain.PALACE or Terrain.BRIDGE or Terrain.CAVE
                        || (terrain == Terrain.WALKABLEWATER && hasBoots);
                    if (!passable || !HasVisitedMazeNeighbor(new(x, y))) continue;
                    visitation[y, x] = true;
                    changed = true;
                }
            }
        }
    }

    private bool HasVisitedMazeNeighbor(IntVector2 position)
    {
        foreach (var direction in IntVector2.CARDINALS)
        {
            var neighbor = position + direction;
            if (WithinMapBounds(neighbor) && visitation[neighbor.Y, neighbor.X]) return true;
        }
        return false;
    }

    /// <summary>Requirements gate location marking, not the terrain beyond a location; reached flags accumulate.</summary>
    private void MarkReachableMazeLocations(IReadOnlySet<RequirementType> requireables)
    {
        foreach (Location location in AllLocations)
        {
            if (location.IsEnabled() && visitation[location.Y, location.Xpos] && location.AccessRequirements.AreSatisfiedBy(requireables))
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
        requiredLocations.UnionWith(GetContinentConnections());
        return requiredLocations.Where(i => i != null);
    }

    public override string GenerateSpoiler()
    {
        StringBuilder sb = new();
        sb.AppendLine("MAZE ISLAND: ");
        AppendOverworldItemSpoilerLine(sb, magicContainerDrop);
        AppendOverworldItemSpoilerLine(sb, childDrop);

        AppendPalaceSpoilerLine(sb, locationAtPalace4);

        sb.AppendLine();
        return sb.ToString();
    }

    public override void DisableDisallowedPassthroughs()
    {
        base.DisableDisallowedPassthroughs();
        if (bridge != null)
        {
            bridge.IsPassthrough = false;
        }
        magicContainerDrop.IsPassthrough = false;
        childDrop.IsPassthrough = false;
    }

    public override void ResetCollectables(RandomizerProperties props)
    {
        ResetOverworldCollectable(childDrop, Collectable.CHILD, props.ShuffleOverworldItems);
        ResetOverworldCollectable(magicContainerDrop, Collectable.MAGIC_CONTAINER, props.ShuffleOverworldItems);

        ResetPalaceCollectables(locationAtPalace4, props);
    }
}
