using System;

namespace Z2Randomizer.RandomizerCore;

public enum Direction { 
    NORTH = 0, EAST = 1, SOUTH = 2, WEST = 3, HORIZONTAL_PASSTHROUGH = 4, VERTICAL_PASSTHROUGH = 5, NONE = 6
}

static class DirectionExtensions
{
    public static Direction Reverse(this Direction direction)
    {
        return direction switch
        {
            Direction.NORTH => Direction.SOUTH,
            Direction.SOUTH => Direction.NORTH,
            Direction.EAST => Direction.WEST,
            Direction.WEST => Direction.EAST,
            _ => throw new ArgumentException("Invalid direction: " + direction)
        };
    }

    public static int DeltaX(this Direction direction)
    {
        return direction switch
        {
            Direction.NORTH => 0,
            Direction.SOUTH => 0,
            Direction.EAST => 1,
            Direction.WEST => -1,
            _ => throw new ArgumentException("Invalid direction: " + direction)
        };
    }

    public static int DeltaY(this Direction direction)
    {
        return direction switch
        {
            Direction.NORTH => -1,
            Direction.SOUTH => 1,
            Direction.EAST => 0,
            Direction.WEST => 0,
            _ => throw new ArgumentException("Invalid direction: " + direction)
        };
    }

    public static bool IsHorizontal(this Direction direction)
    {
        return direction switch
        {
            Direction.NORTH => false,
            Direction.SOUTH => false,
            Direction.EAST => true,
            Direction.WEST => true,
            _ => throw new ArgumentException("Invalid direction: " + direction)
        };
    }

    public static bool IsVertical(this Direction direction)
    {
        return direction switch
        {
            Direction.NORTH => true,
            Direction.SOUTH => true,
            Direction.EAST => false,
            Direction.WEST => false,
            _ => throw new ArgumentException("Invalid direction: " + direction)
        };
    }

    public static Direction RandomHorizontal(Random r)
    {
        return r.Next(2) switch
        {
            0 => Direction.WEST,
            1 => Direction.EAST,
            _ => throw new ImpossibleException("Invalid random in Direction.RandomHorizontal")
        };
    }

    public static Direction RandomVertical(Random r)
    {
        return r.Next(2) switch
        {
            0 => Direction.NORTH,
            1 => Direction.SOUTH,
            _ => throw new ImpossibleException("Invalid random in Direction.Random")
        };
    }

    public static Direction RandomCardinal(Random r)
    {
        return r.Next(4) switch
        {
            0 => Direction.NORTH,
            1 => Direction.SOUTH,
            2 => Direction.EAST,
            3 => Direction.WEST,
            _ => throw new ImpossibleException("Invalid random in Direction.RandomCardinal")
        };
    }

    public static readonly Direction[] ITEM_ROOM_ORIENTATIONS = [Direction.NORTH, Direction.SOUTH, Direction.EAST, Direction.WEST];
    public static readonly Direction[] CARDINAL_DIRECTIONS = [Direction.NORTH, Direction.SOUTH, Direction.EAST, Direction.WEST];

    public static Direction RandomItemRoomOrientation(Random r)
    {
        return r.Next(4) switch
        {
            0 => Direction.NORTH,
            1 => Direction.SOUTH,
            2 => Direction.EAST,
            3 => Direction.WEST,
            //4 => Direction.VERTICAL_PASSTHROUGH,
            //5 => Direction.HORIZONTAL_PASSTHROUGH,
            _ => throw new ImpossibleException("Invalid random in Direction.RandomItemRoomOrientation")
        };
    }
}
