using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Z2Randomizer.RandomizerCore.Enemy;

namespace Z2Randomizer.RandomizerCore.Sidescroll.Palace;

public class MirrorPalaceGenerator : RandomWalkCoordinatePalaceGenerator
{
    private static readonly TableWeightedRandom<int> WeightedRandomDirections = new([
        (0, 8),  // left
        (1, 3),  // down
        (2, 3),  // up
        (3, 8),  // right
    ]);

    private static ImmutableHashSet<RoomExitType> UNMIRRORABLE_SHAPES = new HashSet<RoomExitType>([
        RoomExitType.DROP_ELEVATOR_UP,
        RoomExitType.DROP_RIGHT_T,
        RoomExitType.DROP_LEFT_T,
        RoomExitType.DROP_FOUR_WAY
    ]).ToImmutableHashSet();

    protected override IWeightedSampler<int> GetDirectionWeights(int palaceNumber)
    {
        return WeightedRandomDirections;
    }

    protected override async Task<Dictionary<Coord, RoomExitType>> GetPalaceShape(RandomizerProperties props, Palace palace, RoomPool roomPool, Random r, int roomCount)
    {
        int rightSideCount = roomCount / 2 + 1;
        Dictionary<Coord, RoomExitType> palaceShape = await base.GetPalaceShape(props, palace, roomPool, r, rightSideCount);
        var nonLeftCoords = palaceShape.Keys;
        var centerCoords = nonLeftCoords.Where(coord => coord.X == 0).Except([new(0, 0)]);
        var rightCoords = nonLeftCoords.Where(coord => coord.X > 0).ToArray();

        foreach (var coord in centerCoords)
        {
            var shape = palaceShape[coord];
            if (shape.ContainsRight())
            {
                palaceShape[coord] = shape.AddLeft();
            }
        }

        foreach (var right in rightCoords)
        {
            Coord left = new(-right.X, right.Y);
            palaceShape[left] = palaceShape[right].Mirror();
        }

        return palaceShape;
    }

    protected override bool CanExpandTo(Coord currentCoord, Coord nextCoord, Room entrance)
    {
        return !(nextCoord == Coord.Uninitialized
            || nextCoord.X < 0
            || (currentCoord == Coord.Uninitialized && nextCoord == new Coord(1, 0) && !entrance.HasRightExit)
            || (currentCoord == Coord.Uninitialized && nextCoord == new Coord(0, 1) && !entrance.HasUpExit)
            || (currentCoord == Coord.Uninitialized && nextCoord == new Coord(0, -1) && !entrance.HasDownExit)
        );
    }

    /// Only the the boss room shape and its mirror are allowed
    protected override IEnumerable<RoomExitType> GetItemRoomShapes(RoomPool roomPool, Palace palace)
    {
        Debug.Assert(palace.BossRoom != null);
        var bossRoomShape = palace.BossRoom.CategorizeExits();
        return new RoomExitType[]{ bossRoomShape.Mirror(), bossRoomShape }.Intersect(roomPool.GetItemRoomShapes());
    }

    protected override int GetBossMinDistance(RandomizerProperties props, int palaceNumber)
    {
        if (palaceNumber == 7)
        {
            // still just use the selected value for GP
            return props.DarkLinkMinDistance;
        }

        double length = props.PalaceLengths[palaceNumber - 1];

        return (int)Math.Round(length / 3);
    }


    protected override bool ValidateShape(Palace palace, Dictionary<Coord, RoomExitType> palaceShape)
    {
        // reject unmirrorable drop shapes
        if (UNMIRRORABLE_SHAPES.Intersect(palaceShape.Values).Any())
        {
            return false;
        }

        int palaceSize = palaceShape.Count;
        int centerRooms = palaceShape.Count(pair => pair.Key.X == 0);
        if (centerRooms * 3.5 > palaceSize)
        {
            return false;
        }

        var shapeCounts = palaceShape.GroupBy(kvp => kvp.Value).ToDictionary(v => v.Key, v => v.Count());
        var verticalCount = shapeCounts.GetValueOrDefault(RoomExitType.VERTICAL_PASSTHROUGH, 0);
        //var fourwayCount = shapeCounts.GetValueOrDefault(RoomExitType.FOUR_WAY, 0);
        // disallow too many vertical rooms
        if (verticalCount * 6 > palaceSize)
        {
            return false;
        }

        // pretty strong effective palace check to force boss and items to be spread out
        return VanillaWeightedPalaceGenerator.EffectivePalaceCheck(palace, palaceShape, palaceSize, 2.0);
    }

    protected override List<Room> GetNormalRoomsForExitType(RoomPool roomPool, Coord roomCoords, RoomExitType roomExitType)
    {
        var shape = roomExitType;
        if (roomCoords.X < 0)
        {
            shape = shape.Mirror();
        }
        return base.GetNormalRoomsForExitType(roomPool, roomCoords, shape);
    }

    protected override Room? SelectRoomForCoord(Palace palace, RoomPool rooms, RoomPool roomPool, Dictionary<Coord, RoomExitType> palaceShape, Coord roomCoords, bool dropZone, List<Room> shuffledRoomCandidates)
    {
        var x = roomCoords.X;
        Coord mirrorCoords = new(-x, roomCoords.Y);

        var nonLinkedCandidates = shuffledRoomCandidates.Where(room => room.LinkedRoomName == null).ToList();

        if (x == 0) // center column
        {
            return base.SelectRoomForCoord(palace, rooms, roomPool, palaceShape, roomCoords, dropZone, nonLinkedCandidates);
        }

        if (x < 0) // left-side (will be populated before right-side in the sequence)
        {
            var mirrorableCandidates = shuffledRoomCandidates.Where(room => room.CanBeMirrored()).ToList();
            Room? unmirrored = base.SelectRoomForCoord(palace, rooms, roomPool, palaceShape, roomCoords, dropZone, mirrorableCandidates);
            if (unmirrored == null) { return null; }
            // merge room pre-mirror (relying on LinkedRoomFitInShape doing its job)
            if (unmirrored.LinkedRoomName != null)
            {
                Room linkedRoom = rooms.LinkedRooms[unmirrored.LinkedRoomName];
                unmirrored = unmirrored.Merge(new Room(linkedRoom));
            }

            return unmirrored?.Mirror<PalaceObject, EnemiesPalace125>();
        }

        // right-side
        Room? mirrorRoom = palace.AllRooms.FirstOrDefault(room => room.coords == mirrorCoords);
        Debug.Assert(mirrorRoom != null);

        if (mirrorRoom != null && mirrorRoom.CanBeMirrored())
        {
            Room? unmirrored = rooms.NormalRooms.SingleOrDefault(room => room.Name == mirrorRoom.Name);
            Debug.Assert(unmirrored != null);
            // I don't think merging linked rooms here is needed
            return unmirrored;
        }
        else // left-side room is not a normal room, so just pick any (not linked) room
        {
            return base.SelectRoomForCoord(palace, rooms, roomPool, palaceShape, roomCoords, dropZone, nonLinkedCandidates);
        }
    }
}
