using FtRandoLib.Importer;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace Z2Randomizer.RandomizerCore.Sidescroll;

public class TowerCoordinatePalaceGenerator : ShapeFirstCoordinatePalaceGenerator
{
    private const double SINGLE_WALL_CHANCE = 0;
    private const double SEGMENTED_CHANCE = 1;
    private const double DROP_CHANCE = 0;
    private const double REDUNDANT_ELEVATOR_CHANCE = 0;
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
        int baseRoomsPerLayer, numberOfLayers, layersWithBonusFloors;

        do
        {
            baseRoomsPerLayer = Convert.ToInt32(Math.Sqrt(roomCount)) + r.Next(5) - 2;
            numberOfLayers = (roomCount - 2) / baseRoomsPerLayer;
            layersWithBonusFloors = roomCount - 2 - (baseRoomsPerLayer * numberOfLayers);
        } while (
            //There are more bonus rooms than floors to add them in
            layersWithBonusFloors > numberOfLayers
            //Adding bonus rooms would exceed the maximum number of rooms per floor
            || ((layersWithBonusFloors > 0) && baseRoomsPerLayer > Convert.ToInt32(Math.Sqrt(roomCount)) + 2)
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
                shape[new Coord(rightPivot, y)] = shape[new Coord(leftPivot, y)].RemoveRight();
                shape[new Coord((rightPivot + 1) % roomsPerLayer[y], y)] = shape[new Coord((rightPivot + 1) % roomsPerLayer[y], y)].RemoveLeft();

                //analyze which Xs are in which segements
                //for each segment
                //if that segment is connected to both layers above/below either by connection or by a connection not being possible
                    //do nothing
                //if the segment is connected to 0 layers
                    //while there are 0 connected layers
                        //chance to connect each layer (maybe this chance is per full connection type above/below (i.e. drop chain / big elevator / stub / etc)
                //if both segments are connected to exactly 1 layer
                    //connect one or both segments to the unconnected layer
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

        Debug.WriteLine(GetLayoutDebug(shape));
        return await Task.FromResult(shape);

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
}
