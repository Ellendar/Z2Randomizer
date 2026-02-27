using FtRandoLib.Importer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Z2Randomizer.RandomizerCore.Sidescroll;

public class RandomWalkCoordinatePalaceGenerator : ShapeFirstCoordinatePalaceGenerator
{
    private const float DROP_CHANCE = .06f;
    private static readonly ItemRoomSelectionStrategy itemRoomSelectionStrategy = new ByShapeItemRoomSelectionStrategy();


    private static readonly TableWeightedRandom<int> _weightedRandomDirection = new([
        (0, 1),  // left
        (1, 1),  // down
        (2, 1),  // up
        (3, 1),  // right
    ]);

    protected virtual IWeightedSampler<int> GetDirectionWeights(int palaceNumber)
    {
        return _weightedRandomDirection;
    }

    protected override async Task<Dictionary<Coord, RoomExitType>> GetPalaceShape(RandomizerProperties props, Palace palace, RoomPool roomPool, Random r, int roomCount)
    {
        Dictionary<Coord, RoomExitType> walkGraph = [];
        List<Coord> openCoords = [];
        Room entrance = new(roomPool.Entrances[r.Next(roomPool.Entrances.Count)])
        {
            IsRoot = true,
            // PalaceGroup = palaceGroup
        };
        openCoords.AddRange(entrance.GetOpenExitCoords());
        palace.AllRooms.Add(entrance);
        palace.Entrance = entrance;

        walkGraph[Coord.Origin] = entrance.CategorizeExits();

        var currentCoord = Coord.Origin;

        var weightedRandomDirection = GetDirectionWeights(palace.Number);

        //Create graph
        while (walkGraph.Count < roomCount)
        {
            await Task.Yield();
            int direction = weightedRandomDirection.Next(r);
            Coord nextCoord = direction switch
            {
                0 => currentCoord with { X = currentCoord.X - 1 }, //left
                1 => currentCoord with { Y = currentCoord.Y - 1 }, //down
                2 => currentCoord with { Y = currentCoord.Y + 1 }, //up
                3 => currentCoord with { X = currentCoord.X + 1 }, //right
                _ => throw new ImpossibleException()
            };
            if (nextCoord == Coord.Uninitialized
                || (currentCoord == Coord.Uninitialized && nextCoord == new Coord(-1, 0)) //can't ever go left from an entrance.
                || (currentCoord == Coord.Uninitialized && nextCoord == new Coord(1, 0) && !entrance.HasRightExit)
                || (currentCoord == Coord.Uninitialized && nextCoord == new Coord(0, 1) && !entrance.HasUpExit)
                || (currentCoord == Coord.Uninitialized && nextCoord == new Coord(0, -1) && !entrance.HasDownExit)
            )
            {
                continue;
            }

            walkGraph.TryAdd(nextCoord, RoomExitType.NO_ESCAPE);

            switch (direction)
            {
                case 0: //Left
                    walkGraph[nextCoord] = walkGraph[nextCoord].AddRight();
                    walkGraph[currentCoord] = walkGraph[currentCoord].AddLeft();
                    break;
                case 1: //Down
                    walkGraph[nextCoord] = walkGraph[nextCoord].AddUp();
                    walkGraph[currentCoord] = walkGraph[currentCoord].AddDown();
                    break;
                case 2: //Up
                    walkGraph[nextCoord] = walkGraph[nextCoord].AddDown();
                    walkGraph[currentCoord] = walkGraph[currentCoord].AddUp();
                    break;
                case 3: //Right
                    walkGraph[nextCoord] = walkGraph[nextCoord].AddLeft();
                    walkGraph[currentCoord] = walkGraph[currentCoord].AddRight();
                    break;
            }
            currentCoord = nextCoord;
        }

        //Dropify graph
        foreach (Coord coord in walkGraph.Keys.OrderByDescending(i => i.Y).ThenBy(i => i.X))
        {
            await Task.Yield();
            if (!walkGraph.TryGetValue(coord, out RoomExitType exitType))
            {
                throw new ImpossibleException("Walk graph coordinate was explicitly missing");
            }
            int x = coord.X;
            int y = coord.Y;

            RoomExitType? downExitType = null;
            if (walkGraph.ContainsKey(new Coord(x, y - 1)))
            {
                downExitType = walkGraph[new Coord(x, y - 1)];
            }
            else
            {
                continue;
            }
            double dropChance = DROP_CHANCE;
            //if we dropped into this room
            if (walkGraph.TryGetValue(new Coord(x, y + 1), out RoomExitType upRoomType) && upRoomType.ContainsDrop())
            {
                //If There are no drop -> elevator conversion rooms, so if we have to keep going down, it needs to be a drop.
                if (exitType.ContainsDown() && roomPool.GetNormalRoomsForExitType(RoomExitType.DROP_STUB).Any(i => i.IsDropZone))
                {
                    dropChance = 1f;
                }
            }
            //if the path doesn't go down, or the room below doesn't exist, or the room below only goes up
            if (!exitType.ContainsDown() || downExitType == null || downExitType == RoomExitType.DEADEND_EXIT_UP)
            {
                dropChance = 0f;
            }

            if (r.NextDouble() < dropChance)
            {
                walkGraph[coord] = exitType.ConvertToDrop();
                walkGraph[new Coord(x, y - 1)] = walkGraph[new Coord(x, y - 1)].RemoveUp();
            }
        }

        //Debug.WriteLine(GetLayoutDebug(walkGraph, false));

        //If dropification created a room with no entrance, change it
        foreach (KeyValuePair<Coord, RoomExitType> item in walkGraph.Where(i => i.Value == RoomExitType.DROP_STUB))
        {
            if (!walkGraph.ContainsKey(new Coord(item.Key.X, item.Key.Y + 1)))
            {
                walkGraph[item.Key] = RoomExitType.DEADEND_EXIT_DOWN;
                RoomExitType downRoomType = walkGraph[new Coord(item.Key.X, item.Key.Y - 1)];
                walkGraph[new Coord(item.Key.X, item.Key.Y - 1)] = downRoomType.AddUp();
            }
        }

        //If dropification created a pit, convert it to an elevator.
        //This should never happen, but it's a good safety
        foreach (KeyValuePair<Coord, RoomExitType> item in walkGraph.Where(i => i.Value == RoomExitType.NO_ESCAPE))
        {
            walkGraph[item.Key] = RoomExitType.DEADEND_EXIT_UP;
            RoomExitType upRoomType = walkGraph[new Coord(item.Key.X, item.Key.Y + 1)];
            walkGraph[new Coord(item.Key.X, item.Key.Y + 1)] = upRoomType.ConvertFromDropToDown();
        }

        //Debug.WriteLine(GetLayoutDebug(walkGraph, false));

        return await Task.FromResult(walkGraph);
    }

    protected override ItemRoomSelectionStrategy GetItemRoomSelectionStrategy()
    {
        return itemRoomSelectionStrategy;
    }
}
