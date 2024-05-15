using NLog;
using RandomizerCore;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Numerics;
using System.Threading;

namespace Z2Randomizer.Core.Sidescroll;

public class CartesianPalaceGenerator(CancellationToken ct) : PalaceGenerator
{
    private static readonly IEqualityComparer<byte[]> byteArrayEqualityComparer = new Util.StandardByteArrayEqualityComparer();
    internal override Palace GeneratePalace(RandomizerProperties props, RoomPool rooms, Random r, int roomCount, int palaceNumber)
    {
        Palace palace = new(palaceNumber);
        List<(int, int)> openCoords = new();
        RoomPool roomPool = new(rooms);
        int palaceGroup = palaceNumber switch
        {
            1 => 1,
            2 => 1,
            3 => 2,
            4 => 2,
            5 => 1,
            6 => 2,
            7 => 3,
            _ => throw new ImpossibleException("Invalid palace number: " + palaceNumber)
        };
        Room entrance = new(roomPool.Entrances[r.Next(roomPool.Entrances.Count)])
        {
            IsRoot = true,
            PalaceGroup = palaceGroup
        };
        openCoords.AddRange(entrance.GetOpenExitCoords());
        palace.AllRooms.Add(entrance);

        while (palace.AllRooms.Count + openCoords.Count < roomCount)
        {
            //Stalled out, try again from the start
            if(openCoords.Count == 0)
            {
                palace.IsValid = false;
                return palace;
            }
            Room newRoom = new(roomPool.NormalRooms.Sample(r));

            openCoords.FisherYatesShuffle(r);
            (int, int) bestFit = (0,0);
            int bestFitExitCount = 0;
            foreach((int, int) openCoord in openCoords) 
            {
                newRoom.coords = openCoord;
                int fitExitCount = newRoom.FitsWithLeft(palace.AllRooms.FirstOrDefault(i => i.coords == (openCoord.Item1 - 1, openCoord.Item2)))
                    + newRoom.FitsWithDown(palace.AllRooms.FirstOrDefault(i => i.coords == (openCoord.Item1, openCoord.Item2 - 1)))
                    + newRoom.FitsWithUp(palace.AllRooms.FirstOrDefault(i => i.coords == (openCoord.Item1, openCoord.Item2 + 1)))
                    + newRoom.FitsWithRight(palace.AllRooms.FirstOrDefault(i => i.coords == (openCoord.Item1 + 1, openCoord.Item2)));

                if(fitExitCount > bestFitExitCount)
                {
                    bestFitExitCount = fitExitCount;
                    bestFit = openCoord;
                }
            }

            if(bestFitExitCount > 0 && bestFit != (0, 0))
            {
                newRoom.coords = bestFit;
                List<(int, int)> newOpenCoords = newRoom.GetOpenExitCoords();
                foreach ((int, int) coord in newOpenCoords.ToList())
                {
                    if (openCoords.Contains(coord) || palace.AllRooms.Any(i => i.coords == coord))
                    {
                        newOpenCoords.Remove(coord);
                    }
                }
                //If adding this room would cause the number of open coordinates to be too large,
                //keep searching for a room that more precisely fits the available space.
                if (newOpenCoords.Count + openCoords.Count + palace.AllRooms.Count > roomCount)
                {
                    continue;
                }
                if (props.NoDuplicateRoomsBySideview)
                {
                    if (palace.AllRooms.Any(i => byteArrayEqualityComparer.Equals(i.SideView, newRoom.SideView)))
                    {
                        continue;
                    }
                }
                if (props.NoDuplicateRooms)
                {
                    roomPool.NormalRooms.Remove(newRoom);
                }
                Room left = palace.AllRooms.FirstOrDefault(i => i.coords == (bestFit.Item1 - 1, bestFit.Item2))!;
                Room down = palace.AllRooms.FirstOrDefault(i => i.coords == (bestFit.Item1, bestFit.Item2 - 1))!;
                Room up = palace.AllRooms.FirstOrDefault(i => i.coords == (bestFit.Item1, bestFit.Item2 + 1))!;
                Room right = palace.AllRooms.FirstOrDefault(i => i.coords == (bestFit.Item1 + 1, bestFit.Item2))!;
                if(newRoom.FitsWithLeft(left) > 0)
                {
                    newRoom.Left = left;
                    left.Right = newRoom;
                }
                if (newRoom.FitsWithDown(down) > 0)
                {
                    newRoom.Down = down;
                    down.Up = newRoom;
                }
                if (newRoom.FitsWithUp(up) > 0)
                {
                    newRoom.Up = up;
                    up.Down = newRoom;
                }
                if (newRoom.FitsWithRight(right) > 0)
                {
                    newRoom.Right = right;
                    right.Left = newRoom;
                }
                openCoords.AddRange(newOpenCoords);
                openCoords.Remove(bestFit);
                palace.AllRooms.Add(newRoom);

                Debug.WriteLine("Added Room at (" + newRoom.coords.Item1 + ", " + newRoom.coords.Item2 + ")");
            }
        }
        


        if (palace.AllRooms.Count + openCoords.Count > roomCount)
        {
            throw new ImpossibleException("Room count exceeds maximum room count.");
        }



        throw new NotImplementedException();
    }
}
