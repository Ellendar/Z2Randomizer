using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace Z2Randomizer.RandomizerCore.Overworld;

public abstract partial class World
{
    protected internal enum ConnectorKind
    {
        Bridge = Location.CONNECTOR_BRIDGE_ID,
        Raft = Location.CONNECTOR_RAFT_ID,
        Cave1 = Location.CONNECTOR_CAVE1_ID,
        Cave2 = Location.CONNECTOR_CAVE2_ID
    }

    public Location? raft;
    public Location? bridge;
    public Location? cave1;
    public Location? cave2;

    private List<ContinentConnectorReservation> continentConnectorReservations = [];
    private bool continentConnectorsFinalized;

    public Location LoadRaft(ROM rom, Continent connectedContinent)
        => LoadConnector(rom, ConnectorKind.Raft, connectedContinent);

    public Location LoadBridge(ROM rom, Continent connectedContinent)
        => LoadConnector(rom, ConnectorKind.Bridge, connectedContinent);

    public Location LoadCave1(ROM rom, Continent connectedContinent)
        => LoadConnector(rom, ConnectorKind.Cave1, connectedContinent);

    public Location LoadCave2(ROM rom, Continent connectedContinent)
        => LoadConnector(rom, ConnectorKind.Cave2, connectedContinent);

    /// <summary>
    /// Loads a connector into the existing location indexes and named field, then
    /// sets its destination metadata. Bridges override ROM passthrough flags; other
    /// connectors retain them. Only an out-of-bounds position consumes RNG to pick
    /// a replacement. The returned instance is the one registered in this world.
    /// </summary>
    private Location LoadConnector(ROM rom, ConnectorKind kind, Continent destination)
    {
        var id = LocationIDUtils.FromIndex(continentId, (int)kind);
        var loaded = rom.LoadLocation(id, ConnectorTerrain(kind), kind == ConnectorKind.Bridge ? false : null);
        AddLocation(loaded);
        var location = GetLocation(id);
        Debug.Assert(loaded == location);
        SetConnector(kind, location);
        location.ConnectedContinent = destination;
        location.IsExternalWorld = true;
        location.Map = (int)kind;
        if (kind == ConnectorKind.Raft) location.TerrainType = Terrain.BRIDGE;
        bool isCave = kind is ConnectorKind.Cave1 or ConnectorKind.Cave2;
        if (isCave) location.CanShuffle = true;
        if (!WithinMapBounds(location.Pos))
        {
            location.Pos = IntVector2.Random(RNG, MapColumns, MapRows);
        }
        return location;
    }

    private void SetConnector(ConnectorKind kind, Location location)
    {
        switch (kind)
        {
            case ConnectorKind.Raft: raft = location; break;
            case ConnectorKind.Bridge: bridge = location; break;
            case ConnectorKind.Cave1: cave1 = location; break;
            case ConnectorKind.Cave2: cave2 = location; break;
            default: throw new ArgumentOutOfRangeException(nameof(kind));
        }
    }

    private static Terrain ConnectorTerrain(ConnectorKind kind)
    {
        bool isCave = kind is ConnectorKind.Cave1 or ConnectorKind.Cave2;
        return isCave ? Terrain.CAVE : Terrain.BRIDGE;
    }

    public bool HasConnections()
    {
        return raft != null || bridge != null || cave1 != null || cave2 != null;
    }

    public List<Location> GetContinentConnections()
    {
        return new List<Location>() { cave1!, cave2!, bridge!, raft! }.Where(i => i != null).ToList();
    }

    /// <summary>
    /// Seeds the existing biome oceans before location placement. Keep their shape
    /// contribution to growth; the later reservation makes a landing deliberate.
    /// </summary>
    private protected (Direction Raft, Direction Bridge) PrepareConnectorShores(RandomizerProperties props, BiomeTerrainProfile profile)
    {
        var policy = ContinentConnectorOrientationPolicy.Resolve(continentId, biome, profile.IsHorizontal, props.ContinentConnections);
        var occupied = AllLocations.Where(l => l.IsEnabled() && WithinMapBounds(l.Pos, 0)).Select(l => l.Pos).ToArray();
        var planner = new ContinentConnectorShorePlanner(policy, RNG, map, walkableTerrains, occupied, biome);
        var raftDirection = PrepareRaftShore(planner, profile.PreplacedWater);
        var bridgeDirection = PrepareBridgeShore(planner, profile.PreplacedWater, raftDirection);
        return (raftDirection, bridgeDirection);
    }

    private Direction PrepareRaftShore(ContinentConnectorShorePlanner planner, Terrain preplacedWater)
    {
        if (raft is null) return Direction.NONE;
        var direction = planner.SelectRaftEdge();
        DrawOcean(direction, preplacedWater);
        return direction;
    }

    private Direction PrepareBridgeShore(ContinentConnectorShorePlanner planner, Terrain preplacedWater, Direction raftDirection)
    {
        if (bridge is null) return Direction.NONE;
        var direction = planner.SelectBridgeEdge(raftDirection);
        DrawOcean(direction, preplacedWater);
        return direction;
    }

    /// <summary>
    /// Reserves viable approaches on the chosen edges before growth. Existing
    /// features are constraints; failure leaves the map and locations alone.
    /// Missing connectors consume no RNG.
    /// </summary>
    private protected bool TryReserveContinentConnectors(BiomeTerrainProfile profile, Direction raftDirection, Direction bridgeDirection)
    {
        List<ContinentConnectorReservation> planned = [];
        var occupied = AllLocations.Where(l => l.IsEnabled() && WithinMapBounds(l.Pos, 0)).Select(l => l.Pos).ToArray();
        foreach (var (kind, location, edge) in new[] { (ConnectorKind.Raft, raft, raftDirection), (ConnectorKind.Bridge, bridge, bridgeDirection) })
        {
            if (location is null) continue;
            var reservation = ContinentConnectorReservation.Plan(RNG, map, biome, kind, edge, walkableTerrains, occupied, planned);
            if (reservation is null)
            {
                if (kind == ConnectorKind.Raft) failedOnRaftPlacement++;
                else failedOnBridgePlacement++;
                logger.LogDebug($"No viable {kind} approach on the {edge} edge");
                return false;
            }
            planned.Add(reservation);
        }
        foreach (var reservation in planned)
        {
            var land = reservation.ExistingApproachTerrain(map) ?? profile.Climate.GetRandomTerrain(RNG, walkableTerrains);
            reservation.Seed(map, profile.PreplacedWater, land);
        }
        continentConnectorReservations = planned;
        return true;
    }

    /// <summary>Publishes the reserved landing coordinates without searching the grown map again.</summary>
    protected bool TryFinalizeContinentConnectors()
    {
        if (!ContinentConnectorReservationsIntact()) return false;
        foreach (var reservation in continentConnectorReservations)
            reservation.Finalize(map, reservation.Kind == ConnectorKind.Raft ? raft! : bridge!);
        continentConnectorsFinalized = true;
        return true;
    }

    protected bool IsContinentConnectorReserved(IntVector2 position)
        => continentConnectorReservations.Any(r => r.Contains(position));

    private bool TouchesContinentConnectorReservation(IntVector2 position)
        => continentConnectorReservations.Count > 0
        && (IsContinentConnectorReserved(position) || IntVector2.CARDINALS.Any(d => IsContinentConnectorReserved(position + d)));

    private bool ContinentConnectorReservationsIntact()
    {
        foreach (var reservation in continentConnectorReservations)
        {
            var connector = reservation.Kind == ConnectorKind.Raft ? raft : bridge;
            if (connector is null || !reservation.IsIntact(map, continentConnectorsFinalized)
                || continentConnectorsFinalized && connector.Pos != reservation.Landing
                || AllLocations.Any(l => l != connector && l.IsEnabled() && reservation.Contains(l.Pos)))
            {
                failedOnConnectorConflict++;
                logger.LogDebug($"Reserved {reservation.Kind} approach was overwritten");
                return false;
            }
        }
        return true;
    }

    public static void PlaceRaft(OverworldMap map, Location raft, int x, int y)
    {
        map[y, x] = Terrain.BRIDGE;
        raft.Xpos = x;
        raft.Y = y;
        raft.CanShuffle = false;
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

    /// <summary>
    /// Places loaded continent connectors, raft before bridge, stopping at the first
    /// failure. Missing connectors consume no RNG. Partial placement is retained;
    /// failures are logged here; callers own counters. Cave connectors are placed with caves.
    /// </summary>
    protected bool TryPlaceConnectors(Direction raftDirection, Direction bridgeDirection, out ConnectorKind? failedConnector)
    {
        failedConnector = null;
        if (raft != null && !DrawRaft(raftDirection))
        {
            failedConnector = ConnectorKind.Raft;
        }
        if (failedConnector == null && bridge != null && !DrawBridge(bridgeDirection))
        {
            failedConnector = ConnectorKind.Bridge;
        }
        if (failedConnector != null)
        {
            logger.LogDebug($"Placing {failedConnector} connector failed");
            return false;
        }
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

    /// Get (dx,dy) vector pointing away from the edge for `direction`.
    protected static void GetAwayFromEdgeDeltas(Direction direction, out int dx, out int dy)
    {
        (dx, dy) = direction switch
        {
            Direction.WEST => (1, 0),
            Direction.EAST => (-1, 0),
            Direction.NORTH => (0, 1),
            Direction.SOUTH => (0, -1),
            _ => throw new ArgumentOutOfRangeException(nameof(direction))
        };
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
}
