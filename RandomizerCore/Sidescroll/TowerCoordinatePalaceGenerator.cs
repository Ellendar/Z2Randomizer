using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace Z2Randomizer.RandomizerCore.Sidescroll;

public class TowerCoordinatePalaceGenerator : ShapeFirstCoordinatePalaceGenerator
{
    private static readonly ItemRoomSelectionStrategy itemRoomSelectionStrategy = new RandomItemRoomSelectionStrategy();

    private const int MINIMUM_ROOMS_PER_FLOOR = 3;

    private const double SINGLE_WALL_CHANCE = .3;
    private const double SEGMENTED_CHANCE = .2;
    private const double DROP_CHANCE = .1;
    private const double REDUNDANT_ELEVATOR_CHANCE = .1;

    private TableWeightedRandom<SegmentConnectionType> SegmentConnectionOptionWeights = new([
        (SegmentConnectionType.ELEVATOR_UP, 10),
        (SegmentConnectionType.ELEVATOR_DOWN, 10),
        (SegmentConnectionType.BOTH_ELEVATORS, 6),
        (SegmentConnectionType.DROP_COLUMN, 2),
        (SegmentConnectionType.DROP_INTO, 3),
        (SegmentConnectionType.DROP_OUT, 3),
        (SegmentConnectionType.DROP_TO_ELEVATOR, 2),
    ]);

    protected async override Task<Dictionary<Coord, RoomExitType>> GetPalaceShape(RandomizerProperties props, Palace palace, RoomPool roomPool, Random r, int roomCount)
    {
        Dictionary<Coord, RoomExitType> shape = [];
        //Entrance is always up entrance(even if up entrances are not in the pool) 
        List<Room> entranceCandidates = roomPool.Entrances.Where(i => i.CategorizeExits() == RoomExitType.DEADEND_EXIT_UP).ToList();
        Room entrance = new(entranceCandidates.Sample(r) ?? roomPool.DefaultUpEntrance)
        {
            IsRoot = true,
        };
        palace.AllRooms.Add(entrance);
        palace.Entrance = entrance;

        //How far in each ring. (Distance per ring should be within 1 of all other levels) (-2 from boss room/ entrance)
        //sqrt(numrooms) +/ -2 base rooms per layer
        int baseRoomsPerLayer, minRoomsPerLayer, maxRoomsPerLayer, numberOfLayers, layersWithBonusFloors;

        do
        {
            maxRoomsPerLayer = Convert.ToInt32(Math.Sqrt(roomCount)) + 2;
            minRoomsPerLayer = Math.Max(Convert.ToInt32(Math.Sqrt(roomCount)) - 2, MINIMUM_ROOMS_PER_FLOOR);
            baseRoomsPerLayer = r.Next(minRoomsPerLayer, maxRoomsPerLayer + 1);
            numberOfLayers = (roomCount - 2) / baseRoomsPerLayer;
            layersWithBonusFloors = roomCount - 2 - (baseRoomsPerLayer * numberOfLayers);
        } while (
            //There are more bonus rooms than floors to add them in
            layersWithBonusFloors > numberOfLayers
            //Adding bonus rooms would exceed the maximum number of rooms per floor
            || ((layersWithBonusFloors > 0) && baseRoomsPerLayer == maxRoomsPerLayer)
        );

        //layers that have more rooms are the lower layers
        int[] roomsPerLayer = new int[numberOfLayers + 1];
        Array.Fill(roomsPerLayer, baseRoomsPerLayer);
        roomsPerLayer[0] = 1;
        for (int i = 1; i < layersWithBonusFloors + 1; i++)
        {
            roomsPerLayer[i]++;
        }

        //start with rings
        for (int y = 1; y < roomsPerLayer.Length; y++)
        {
            for(int x = 0; x < roomsPerLayer[y]; x++)
            {
                shape.Add(new Coord(x, y), RoomExitType.HORIZONTAL_PASSTHROUGH);
            }
        }
        //connect the entrance
        int connectionX = r.Next(0, roomsPerLayer[1]);
        shape.Add(new Coord(connectionX, 0), RoomExitType.DEADEND_EXIT_UP);
        shape[new Coord(connectionX, 1)] = shape[new Coord(connectionX, 1)].AddDown();
        palace.Entrance.coords = new Coord(connectionX, 0);

        //for each layer add an up at a random coordinate at that layer
        for(int y = 1; y < roomsPerLayer.Length - 1; y++)
        {
            connectionX = r.Next(0, Math.Min(roomsPerLayer[y], roomsPerLayer[y+1]));
            shape[new Coord(connectionX, y)] = shape[new Coord(connectionX, y)].AddUp();
            shape[new Coord(connectionX, y+1)] = shape[new Coord(connectionX, y+1)].AddDown();
        }

        //place the boss at the top
        int lastLayerY = roomsPerLayer.Length - 1;
        connectionX = r.Next(0, roomsPerLayer[lastLayerY]);
        shape[new Coord(connectionX, lastLayerY)] = shape[new Coord(connectionX, lastLayerY)].AddUp();
        shape.Add(new Coord(connectionX, lastLayerY + 1), RoomExitType.DEADEND_EXIT_DOWN);

        List<Room> bossRoomCandidates = roomPool.BossRooms.Where(i => i.CategorizeExits() == RoomExitType.DEADEND_EXIT_DOWN).ToList();
        Room bossRoom = new(bossRoomCandidates.Sample(r) ?? roomPool.DefaultDownBossRoom);
        bossRoom.coords = new Coord(connectionX, lastLayerY + 1);
        palace.AllRooms.Add(bossRoom);
        palace.BossRoom = bossRoom;

        //x % chance(each)
        //    ring has a wall turning it into a unlooping ring
        for (int y = 1; y < roomsPerLayer.Length; y++)
        {
            if (r.NextDouble() < SINGLE_WALL_CHANCE)
            {
                int leftSideOfWallX = r.Next(0, roomsPerLayer[y]);
                int rightSideOfWallX = (leftSideOfWallX + 1) % roomsPerLayer[y];
                shape[new Coord(leftSideOfWallX, y)] = shape[new Coord(leftSideOfWallX, y)].RemoveRight();
                shape[new Coord(rightSideOfWallX, y)] = shape[new Coord(rightSideOfWallX, y)].RemoveLeft();
            }
            //OR ring has 2 walls and is segmented(ensure there is a redundant up or down in the segment)
            else if (r.NextDouble() < SEGMENTED_CHANCE)
            {
                //pick 2 rooms (they can even be the same)
                int leftPivot, rightPivot;
                do
                {
                    leftPivot = r.Next(0, roomsPerLayer[y]);
                    rightPivot = r.Next(0, roomsPerLayer[y]);
                }
                while ((rightPivot + 1 % roomsPerLayer[y]) == leftPivot);
                shape[new Coord(leftPivot, y)] = shape[new Coord(leftPivot, y)].RemoveLeft();
                int leftOfLeftPivot = leftPivot - 1;
                leftOfLeftPivot = leftOfLeftPivot < 0 ? roomsPerLayer[y] - 1 : leftOfLeftPivot;
                shape[new Coord(leftOfLeftPivot, y)] = shape[new Coord(leftOfLeftPivot, y)].RemoveRight();
                shape[new Coord(rightPivot, y)] = shape[new Coord(rightPivot, y)].RemoveRight();
                shape[new Coord((rightPivot + 1) % roomsPerLayer[y], y)] = shape[new Coord((rightPivot + 1) % roomsPerLayer[y], y)].RemoveLeft();

                //analyze which Xs are in which segements
                List<Coord>[] segments = [[new Coord(0, y)], []];
                int currentSegment = 0;
                int timesSwitched = 0;
                for(int x = 1; x < roomsPerLayer[y]; x++)
                {
                    Coord currentCoord = new Coord(x, y);
                    if (!shape[currentCoord].ContainsLeft())
                    {
                        currentSegment = (currentSegment + 1) % 2;
                        timesSwitched++;
                    }
                    segments[currentSegment].Add(currentCoord);
                }
                //More than 2 switches means we have a triple segment (or I screwed up the logic)
                Debug.Assert(timesSwitched <= 2);
                //for each segment
                bool[] segmentConnectsUp = [false, false];
                bool[] segmentConnectsDown = [false, false];
                for (int segment = 0; segment < 2; segment++)
                {
                    for (int i = 0; i < segments[segment].Count; i++)
                    {
                        if (shape[segments[segment][i]].ContainsUp())
                        {
                            segmentConnectsUp[segment] = true;
                        }
                        if (shape[segments[segment][i]].ContainsDown())
                        {
                            segmentConnectsDown[segment] = true;
                        }
                    }
                }
                for (int segment = 0; segment < 2; segment++)
                {
                    //if that segment is connected to both layers above/below either by connection or by a connection not being possible
                    if (segmentConnectsUp[segment] && segmentConnectsDown[segment])
                    {
                        //do nothing
                    }
                    //if the segment is connected to 0 layers
                    if (!segmentConnectsUp[segment] && !segmentConnectsDown[segment])
                    {
                        //pick a random coord in the segment to connect to
                        List<Coord> connectionCandidates = new(segments[segment]);
                        bool canConnectDown, canConnectUp;
                        Coord connectionCoord;
                        do
                        {
                            connectionCoord = connectionCandidates.Sample(r);
                            if (connectionCoord == Coord.Uninitialized)
                            {
                                return [];
                            }
                            canConnectDown = CoordCanConnectDown(connectionCoord);
                            canConnectUp = CoordCanConnectUp(connectionCoord, numberOfLayers, shape);
                            if (!canConnectUp && !canConnectDown)
                            {
                                connectionCandidates.Remove(connectionCoord);
                            }
                        }
                        while (!canConnectDown && !canConnectUp);

                        //chance to connect each layer
                        //(maybe this chance is per full connection type above/below (i.e. drop chain / big elevator / stub / etc)
                        SegmentConnectionType connectionType;
                        do
                        {
                            connectionType = SegmentConnectionOptionWeights.Next(r);
                        }
                        while((connectionType.RequiresUpRoom() && !canConnectUp)
                            || (connectionType.RequiresDownRoom() && !canConnectDown));

                        ApplyConnection(shape, connectionCoord, connectionType);
                    }
                }

                //Recalculate which segmets are connected to what after connections were added
                for (int segment = 0; segment < 2; segment++)
                {
                    for (int i = 0; i < segments[segment].Count; i++)
                    {
                        if (shape[segments[segment][i]].ContainsUp())
                        {
                            segmentConnectsUp[segment] = true;
                        }
                        if (shape[segments[segment][i]].ContainsDown())
                        {
                            segmentConnectsDown[segment] = true;
                        }
                    }
                }

                //if both segments are connected to exactly 1 layer, the critical path is split
                if ((segmentConnectsUp[0] ^ segmentConnectsDown[0]) && (segmentConnectsUp[1] ^ segmentConnectsDown[1]))
                {
                    //connect one or both segments to the unconnected layer
                    bool[] tryToConnectSegmentUp;
                    bool[] tryToConnectSegmentDown;
                    do
                    {
                        tryToConnectSegmentUp = [!segmentConnectsUp[0], !segmentConnectsUp[1]];
                        tryToConnectSegmentDown = [!segmentConnectsDown[0], !segmentConnectsDown[1]];
                        for (int segment = 0; segment < 2; segment++)
                        {
                            if (r.Next() % 2 == 0)
                            {
                                tryToConnectSegmentDown[segment] = false;
                            }
                            if (r.Next() % 2 == 0)
                            {
                                tryToConnectSegmentUp[segment] = false;
                            }
                        }
                    }
                    while (!(tryToConnectSegmentUp[0] || tryToConnectSegmentDown[0] || tryToConnectSegmentUp[1] || tryToConnectSegmentDown[1]));

                    for (int segment = 0; segment < 2; segment++)
                    {
                        if (tryToConnectSegmentUp[segment])
                        {
                            //pick a random coord in the segment to connect to
                            List<Coord> connectionCandidates = new(segments[segment]);
                            bool canConnectUp;
                            Coord connectionCoord;
                            do
                            {
                                connectionCoord = connectionCandidates.Sample(r);
                                if (connectionCoord == Coord.Uninitialized)
                                {
                                    break;
                                }
                                canConnectUp = CoordCanConnectUp(connectionCoord, numberOfLayers, shape);
                                if(!canConnectUp)
                                {
                                    connectionCandidates.Remove(connectionCoord);
                                }
                            }
                            while (!canConnectUp);
                            if(connectionCoord != Coord.Uninitialized)
                            {
                                ApplyConnection(shape, connectionCoord, SegmentConnectionType.ELEVATOR_UP);
                            }
                        }
                        if (tryToConnectSegmentDown[segment])
                        {
                            //pick a random coord in the segment to connect to
                            List<Coord> connectionCandidates = new(segments[segment]);
                            bool canConnectDown;
                            Coord connectionCoord;
                            do
                            {
                                connectionCoord = connectionCandidates.Sample(r);
                                if (connectionCoord == Coord.Uninitialized)
                                {
                                    break;
                                }
                                canConnectDown = CoordCanConnectDown(connectionCoord);
                                if (!canConnectDown)
                                {
                                    connectionCandidates.Remove(connectionCoord);
                                }
                            }
                            while (!canConnectDown);
                            if (connectionCoord != Coord.Uninitialized)
                            {
                                ApplyConnection(shape, connectionCoord, SegmentConnectionType.ELEVATOR_DOWN);
                            }
                        }
                    }
                }
            }

            //ring has a redundant connection to the layer up / down
            if (r.NextDouble() < REDUNDANT_ELEVATOR_CHANCE && y != 1)
            {
                List<Coord> possibleDownXs = shape.Where(i => i.Key.Y == y && !i.Value.ContainsDown() && !i.Value.ContainsDrop()).Select(i => i.Key).ToList();
                if (possibleDownXs.Count > 0)
                {
                    Coord downCoord = possibleDownXs.Sample(r);
                    shape[downCoord] = shape[downCoord].AddDown();
                    shape[new Coord(downCoord.X, downCoord.Y - 1)] = shape[new Coord(downCoord.X, downCoord.Y - 1)].AddUp();
                }
            }

            //ring has a drop
            if (r.NextDouble() < DROP_CHANCE && y != 1)
            {
                List<Coord> possibleDropXs = shape.Where(i => i.Key.Y == y && !i.Value.ContainsDown() && !i.Value.ContainsDrop()).Select(i => i.Key).ToList();
                if(possibleDropXs.Count > 0)
                {
                    Coord dropCoord = possibleDropXs.Sample(r);
                    shape[dropCoord] = shape[dropCoord].AddDrop();
                }
            }
        }

        //Debug.WriteLine(GetLayoutDebug(shape));
        return await Task.FromResult(shape);

    }

    private bool CoordCanConnectUp(Coord coord, int numberOfLayers, Dictionary<Coord, RoomExitType> shape)
    {
        //there has to actually be a room above. This can not be the case on the boundry between bonus
        //and non-bonus layers
        return coord.Y != numberOfLayers && shape.ContainsKey(new Coord(coord.X, coord.Y + 1));
    }

    private bool CoordCanConnectDown(Coord coord)
    {
        return coord.Y != 1;
    }

    private void ApplyConnection(Dictionary<Coord, RoomExitType> shape, Coord connectionCoord, SegmentConnectionType connectionType)
    {
        switch (connectionType)
        {
            case SegmentConnectionType.ELEVATOR_UP:
                shape[connectionCoord] = shape[connectionCoord].AddUp();
                shape[new Coord(connectionCoord.X, connectionCoord.Y + 1)] = shape[new Coord(connectionCoord.X, connectionCoord.Y + 1)].AddDown();
                break;
            case SegmentConnectionType.ELEVATOR_DOWN:
                shape[connectionCoord] = shape[connectionCoord].AddDown(overwriteDrop: true);
                shape[new Coord(connectionCoord.X, connectionCoord.Y - 1)] = shape[new Coord(connectionCoord.X, connectionCoord.Y - 1)].AddUp();
                break;
            case SegmentConnectionType.BOTH_ELEVATORS:
                shape[connectionCoord] = shape[connectionCoord].AddUp();
                shape[new Coord(connectionCoord.X, connectionCoord.Y + 1)] = shape[new Coord(connectionCoord.X, connectionCoord.Y + 1)].AddDown();
                shape[connectionCoord] = shape[connectionCoord].AddDown(overwriteDrop: true);
                shape[new Coord(connectionCoord.X, connectionCoord.Y - 1)] = shape[new Coord(connectionCoord.X, connectionCoord.Y - 1)].AddUp();
                break;
            case SegmentConnectionType.DROP_OUT:
                shape[connectionCoord] = shape[connectionCoord].AddDrop();
                break;
            case SegmentConnectionType.DROP_INTO:
                shape[new Coord(connectionCoord.X, connectionCoord.Y + 1)] = shape[new Coord(connectionCoord.X, connectionCoord.Y + 1)].AddDrop();
                break;
            case SegmentConnectionType.DROP_COLUMN:
                shape[connectionCoord] = shape[connectionCoord].AddDrop();
                shape[new Coord(connectionCoord.X, connectionCoord.Y + 1)] = shape[new Coord(connectionCoord.X, connectionCoord.Y + 1)].AddDrop();
                break;
            case SegmentConnectionType.DROP_TO_ELEVATOR:
                shape[new Coord(connectionCoord.X, connectionCoord.Y + 1)] = shape[new Coord(connectionCoord.X, connectionCoord.Y + 1)].AddDrop();
                shape[connectionCoord] = shape[connectionCoord].AddDown(overwriteDrop: true);
                shape[new Coord(connectionCoord.X, connectionCoord.Y - 1)] = shape[new Coord(connectionCoord.X, connectionCoord.Y - 1)].AddUp();
                break;
        }
    }

    protected override void ConnectNonEuclideanPaths(Palace palace)
    {
        int maxY = palace.AllRooms.Select(i => i.coords).Max(i => i.Y);
        for(int y = 1; y < maxY; y++)
        {
            Room leftmostRoom = palace.AllRooms.FirstOrDefault(i => i.coords.X == 0 && i.coords.Y == y)!;
            Debug.Assert(leftmostRoom != null);
            int maxX = palace.AllRooms.Where(i => i.coords.Y == y).Select(i => i.coords.X).Max();
            Room rightmostRoom = palace.AllRooms.FirstOrDefault(i => i.coords.X == maxX && i.coords.Y == y)!;
            if (leftmostRoom.HasLeftExit && rightmostRoom.HasRightExit)
            {
                leftmostRoom.Left = rightmostRoom;
                rightmostRoom.Right = leftmostRoom;
            }
        }
    }

    protected override ItemRoomSelectionStrategy GetItemRoomSelectionStrategy()
    {
        return itemRoomSelectionStrategy;
    }
}
