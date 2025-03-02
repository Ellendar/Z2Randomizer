using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using NLog;

namespace RandomizerCore.Sidescroll;

public abstract class CoordinatePalaceGenerator() : PalaceGenerator
{

    protected static readonly Logger logger = LogManager.GetCurrentClassLogger();
    protected static bool AddSpecialRoomsByReplacement(Palace palace, RoomPool roomPool, Random r, RandomizerProperties props)
    {
        //ItemRoom
        if (palace.Number < 7)
        {
            if (roomPool.ItemRoomsByDirection.Values.Sum(i => i.Count) == 0)
            {
                throw new Exception("No item rooms for generated palace");
            }
            Direction itemRoomDirection;
            do
            {
                itemRoomDirection = DirectionExtensions.RandomItemRoomOrientation(r);
            } while (!roomPool.ItemRoomsByDirection.ContainsKey(itemRoomDirection));
            List<Room> itemRoomCandidates = roomPool.ItemRoomsByDirection[itemRoomDirection].ToList();
            itemRoomCandidates.FisherYatesShuffle(r);

            foreach (Room itemRoomCandidate in itemRoomCandidates)
            {
                if(palace.ItemRoom != null)
                {
                    break;
                }
                List<Room> itemRoomReplacementCandidates =
                    palace.AllRooms.Where(i => i.CategorizeExits() == itemRoomCandidate.CategorizeExits() && i.IsNormalRoom()).ToList();

                itemRoomReplacementCandidates.FisherYatesShuffle(r);
                foreach (Room itemRoomReplacementRoom in itemRoomReplacementCandidates)
                {
                    Room? upRoom = palace.AllRooms.FirstOrDefault(
                        i => i.coords == (itemRoomReplacementRoom.coords.Item1, itemRoomReplacementRoom.coords.Item2 + 1));
                    if (itemRoomReplacementRoom != null && 
                        (upRoom == null || !upRoom.HasDownExit || upRoom.HasDrop == itemRoomCandidate.IsDropZone))
                    {
                        palace.ItemRoom = new(itemRoomCandidate);
                        if (itemRoomCandidate.LinkedRoomName != null)
                        {
                            Room linkedRoom = roomPool.LinkedRooms[itemRoomCandidate.LinkedRoomName];
                            Room newLinkedRoom = new();
                            palace.ItemRoom = palace.ItemRoom.Merge(linkedRoom);
                        }
                        palace.ReplaceRoom(itemRoomReplacementRoom, palace.ItemRoom);
                        break;
                    }
                }
            }
        }
        //Tbird Room
        else
        {
            if (roomPool.TbirdRooms.Count == 0)
            {
                throw new Exception("No tbird rooms for generated palace");
            }
            List<Room> tbirdRoomCandidates = roomPool.TbirdRooms.ToList();
            tbirdRoomCandidates.FisherYatesShuffle(r);

            foreach (Room tbirdRoomCandidate in tbirdRoomCandidates)
            {
                if (palace.TbirdRoom != null)
                {
                    break;
                }
                List<Room> tbirdRoomReplacementCandidates =
                    palace.AllRooms.Where(i => i.CategorizeExits() == tbirdRoomCandidate.CategorizeExits() && i.IsNormalRoom()).ToList();
                tbirdRoomReplacementCandidates.FisherYatesShuffle(r);
                foreach (Room tbirdRoomReplacementRoom in tbirdRoomReplacementCandidates)
                {
                    Room? upRoom = palace.AllRooms.FirstOrDefault(
                        i => i.coords == (tbirdRoomReplacementRoom.coords.Item1, tbirdRoomReplacementRoom.coords.Item2 + 1));
                    if (tbirdRoomReplacementRoom != null &&
                        (upRoom == null || !upRoom.HasDownExit || upRoom.HasDrop == tbirdRoomCandidate.IsDropZone))
                    {
                        palace.TbirdRoom = new(tbirdRoomCandidate);
                        palace.ReplaceRoom(tbirdRoomReplacementRoom, palace.TbirdRoom);
                        break;
                    }
                }
            }
        }

        //BossRoom
        if (roomPool.BossRooms.Count == 0)
        {
            throw new Exception("No boss rooms for generated palace");
        }
        List<Room> bossRoomCandidates = roomPool.BossRooms.ToList();
        bossRoomCandidates.FisherYatesShuffle(r);

        foreach (Room bossRoomCandidate in bossRoomCandidates)
        {
            if (palace.BossRoom != null)
            {
                break;
            }
            RoomExitType bossRoomExitType = bossRoomCandidate.CategorizeExits();
            if (props.BossRoomConnect && palace.Number < 7)
            {
                bossRoomExitType = bossRoomExitType.AddRight();
            }
            List<Room> bossRoomReplacementCandidates =
                palace.AllRooms.Where(i => i.CategorizeExits() == bossRoomExitType && i.IsNormalRoom()).ToList();

            bossRoomReplacementCandidates.FisherYatesShuffle(r);
            foreach (Room bossRoomReplacementRoom in bossRoomReplacementCandidates)
            {
                Room? upRoom = palace.AllRooms.FirstOrDefault(
                    i => i.coords == (bossRoomReplacementRoom.coords.Item1, bossRoomReplacementRoom.coords.Item2 + 1));
                if (bossRoomReplacementRoom != null &&
                    (upRoom == null || !upRoom.HasDownExit || upRoom.HasDrop == bossRoomCandidate.IsDropZone))
                {
                    palace.BossRoom = new(bossRoomCandidate);
                    palace.BossRoom.Enemies = (byte[])roomPool.VanillaBossRoom.Enemies.Clone();
                    if (props.BossRoomConnect && palace.Number < 7)
                    {
                        palace.BossRoom.HasRightExit = true;
                        palace.BossRoom.ReplaceExitStatueWithCurtains();
                    }
                    palace.ReplaceRoom(bossRoomReplacementRoom, palace.BossRoom);
                    break;
                }
            }
        }

        if (palace.BossRoom == null
            || palace.Entrance == null
            || (palace.Number == 7 && palace.TbirdRoom == null)
            || (palace.Number < 7 && palace.ItemRoom == null))
        {
            logger.Debug("Failed to place critical room in palace");
            return false;
        }

        foreach(Room room in palace.AllRooms.ToList())
        {
            if(room.MergedPrimary != null)
            {
                palace.AllRooms.Remove(room);
                Room primary = room.MergedPrimary;
                Room secondary = room.MergedSecondary;
                palace.AllRooms.Add(primary);
                palace.AllRooms.Add(secondary);
                if(room == palace.ItemRoom)
                {
                    palace.ItemRoom = room.MergedPrimary;
                }
                primary.coords = room.coords;
                secondary.coords = room.coords;

                if (primary.HasLeftExit && room.Left != null)
                {
                    primary.Left = room.Left;
                }
                if (primary.HasRightExit && room.Right != null)
                {
                    primary.Right = room.Right;
                }
                if (primary.HasUpExit && room.Up != null)
                {
                    primary.Up = room.Up;
                }
                if (primary.HasDownExit && room.Down != null)
                {
                    primary.Down = room.Down;
                }

                if (secondary.HasLeftExit && room.Left != null)
                {
                    secondary.Left = room.Left;
                }
                if (secondary.HasRightExit && room.Right != null)
                {
                    secondary.Right = room.Right;
                }
                if (secondary.HasUpExit && room.Up != null)
                {
                    secondary.Up = room.Up;
                }
                if (secondary.HasDownExit && room.Down != null)
                {
                    secondary.Down = room.Down;
                }
            }
        }

        //TODO: This is REALLY late to abandon ship on the whole palace, but 
        //refactoring the boss placement to do the check properly without a million stupid room swaps
        //was not a thing I felt like figuring out.
        //Maybe we use ShuffleRooms()?
        //So for now we suffer lesser performance (but still way better than Reconstructed so do we care?)
        if (!palace.AllReachable() || (palace.Number == 7 && props.RequireTbird && !palace.RequiresThunderbird()))
        {
            return false;
        }

        return true;
    } 
}
