using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using NLog;

namespace Z2Randomizer.RandomizerCore.Sidescroll;

public abstract class CoordinatePalaceGenerator() : PalaceGenerator
{
    private static readonly RoomExitType[] PRIORITY_ROOM_SHAPES = [RoomExitType.DROP_STUB, RoomExitType.DROP_T];
    protected static readonly Logger logger = LogManager.GetCurrentClassLogger();
    protected static bool AddSpecialRoomsByReplacement(Palace palace, RoomPool roomPool, Random r, RandomizerProperties props)
    {
        //Debug.WriteLine(palace.GetLayoutDebug(PalaceStyle.SEQUENTIAL));
        palace.ItemRooms ??= [];

        int itemRoomTotal = palace.Number < 7 ? props.PalaceItemRoomCounts[palace.Number - 1] : 0;

        //ItemRoom
        if (palace.Number < 7)
        {
            if (roomPool.ItemRoomsByDirection.Values.Sum(i => i.Count) == 0)
            {
                throw new Exception("No item rooms for generated palace");
            }

            int itemRoomNumber = 0;

            List<RoomExitType> possibleItemRoomExitTypes = [];
            foreach(RoomExitType shape in PRIORITY_ROOM_SHAPES)
            {
                if(roomPool.ItemRoomsByShape.ContainsKey(shape))
                {
                    possibleItemRoomExitTypes.Add(shape);
                }
            }
            List<RoomExitType> additionalItemRoomExitShapes = roomPool.ItemRoomsByShape.Keys.Where(i => !PRIORITY_ROOM_SHAPES.Contains(i)).ToList();
            additionalItemRoomExitShapes.FisherYatesShuffle(r);
            possibleItemRoomExitTypes.AddRange(additionalItemRoomExitShapes);

            while (itemRoomNumber < itemRoomTotal)
            {
                if(possibleItemRoomExitTypes.Count == 0)
                {
                    return false;
                }

                RoomExitType itemRoomExitType = possibleItemRoomExitTypes[0];
                List<Room> itemRoomCandidates = roomPool.ItemRoomsByShape[itemRoomExitType].ToList();
                itemRoomCandidates.FisherYatesShuffle(r);

                bool itemRoomPlaced = false;

                foreach (Room itemRoomCandidate in itemRoomCandidates)
                {
                    if (itemRoomPlaced)
                    {
                        break;
                    }
                    List<Room> itemRoomReplacementCandidates =
                        palace.AllRooms.Where(i => i.IsNormalRoom() && i.CategorizeExits() == itemRoomExitType).ToList();

                    itemRoomReplacementCandidates.FisherYatesShuffle(r);
                    foreach (Room itemRoomReplacementRoom in itemRoomReplacementCandidates)
                    {
                        Room? upRoom = palace.AllRooms.FirstOrDefault(
                            i => i.coords == itemRoomReplacementRoom.coords with { Y = itemRoomReplacementRoom.coords.Y + 1 });
                        if (itemRoomReplacementRoom != null &&
                            (upRoom == null || !upRoom.HasDownExit || upRoom.HasDrop == itemRoomCandidate.IsDropZone))
                        {
                            palace.ItemRooms.Add(new(itemRoomCandidate));
                            if (itemRoomCandidate.LinkedRoomName != null)
                            {
                                Room linkedRoom = roomPool.LinkedRooms[itemRoomCandidate.LinkedRoomName];
                                Room newLinkedRoom = new();
                                palace.ItemRooms[itemRoomNumber] = palace.ItemRooms[itemRoomNumber].Merge(linkedRoom);
                            }
                            palace.ReplaceRoom(itemRoomReplacementRoom, palace.ItemRooms[itemRoomNumber]);
                            itemRoomPlaced = true;
                            itemRoomNumber++;
                            break;
                        }
                    }
                }
                if(itemRoomPlaced)
                {
                    // shuffle item room shape priority if we have more item rooms to place
                    if (itemRoomNumber < itemRoomTotal)
                    {
                        possibleItemRoomExitTypes.FisherYatesShuffle(r);
                    }
                }
                else
                {
                    possibleItemRoomExitTypes.Remove(itemRoomExitType);
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
                        i => i.coords == tbirdRoomReplacementRoom.coords with { Y = tbirdRoomReplacementRoom.coords.Y + 1 });
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
            if (palace.Number < 7 && props.BossRoomsExitToPalace[palace.Number - 1])
            {
                bossRoomExitType = bossRoomExitType.AddRight();
            }
            List<Room> bossRoomReplacementCandidates =
                palace.AllRooms.Where(i => i.CategorizeExits() == bossRoomExitType && i.IsNormalRoom()).ToList();

            bossRoomReplacementCandidates.FisherYatesShuffle(r);
            foreach (Room bossRoomReplacementRoom in bossRoomReplacementCandidates)
            {
                Room? upRoom = palace.AllRooms.FirstOrDefault(
                    i => i.coords == bossRoomReplacementRoom.coords with { Y = bossRoomReplacementRoom.coords.Y + 1 });
                if (bossRoomReplacementRoom != null &&
                    (upRoom == null || !upRoom.HasDownExit || upRoom.HasDrop == bossRoomCandidate.IsDropZone))
                {
                    palace.BossRoom = new(bossRoomCandidate);
                    palace.BossRoom.Enemies = (byte[])roomPool.VanillaBossRoom.Enemies.Clone();
                    if (palace.Number < 7 && props.BossRoomsExitToPalace[palace.Number - 1])
                    {
                        palace.BossRoom.HasRightExit = true;
                        palace.BossRoom.AdjustContinuingBossRoom();
                    }
                    palace.ReplaceRoom(bossRoomReplacementRoom, palace.BossRoom);
                    break;
                }
            }
        }

        if (palace.BossRoom == null
            || palace.Entrance == null
            || (palace.Number == 7 && palace.TbirdRoom == null)
            || palace.ItemRooms.Count != itemRoomTotal)
        {
            logger.Debug("Failed to place critical room in palace");
            return false;
        }

        UnmergeMergedRooms(palace, roomPool);

        //TODO: This is REALLY late to abandon ship on the whole palace, but 
        //refactoring the boss placement to do the check properly without a million stupid room swaps
        //was not a thing I felt like figuring out.
        //Maybe we use ShuffleRooms()?
        //So for now we suffer lesser performance (but still way better than Reconstructed so do we care?)
        if (!palace.AllReachable()
            || (palace.Number == 7 && props.RequireTbird && !palace.RequiresThunderbird())
            || (palace.Number == 7 && !palace.BossRoomMinDistance(props.DarkLinkMinDistance))
        )
        {
            //Debug.WriteLine(palace.GetLayoutDebug(PalaceStyle.SEQUENTIAL));
            return false;
        }

        return true;
    } 

    //Linked rooms were linked ahead of time for the purposes of palace generation, but need to be unlinked for
    //logical calculation
    private static void UnmergeMergedRooms(Palace palace, RoomPool roomPool)
    {

        List<(Room, Room, Room)> replacements = [];
        //only the primary linked rooms
        foreach (Room primaryRoom in palace.AllRooms.Where(i => i.LinkedRoomName != null && i.Enabled && i.LinkedRoom == null)) 
        {
            Room secondaryRoom = new(roomPool.LinkedRooms[primaryRoom.LinkedRoomName!]);
            Room newPrimaryRoom = new(roomPool.LinkedRooms[primaryRoom.Name]);

            newPrimaryRoom.coords = primaryRoom.coords;
            newPrimaryRoom.LinkedRoom = secondaryRoom;
            secondaryRoom.LinkedRoom = newPrimaryRoom;

            replacements.Add((primaryRoom, newPrimaryRoom, secondaryRoom));
            if(primaryRoom.Up != null)
            {
                if(newPrimaryRoom.HasUpExit)
                {
                    newPrimaryRoom.Up = primaryRoom.Up;
                    primaryRoom.Up.Down = newPrimaryRoom;
                }
                else if (secondaryRoom.HasUpExit)
                {
                    secondaryRoom.Up = primaryRoom.Up;
                    primaryRoom.Up.Down = secondaryRoom;
                }
            }
            if (primaryRoom.Down != null)
            {
                if (newPrimaryRoom.HasDownExit)
                {
                    newPrimaryRoom.Down = primaryRoom.Down;
                    primaryRoom.Down.Up = newPrimaryRoom;
                }
                else if (secondaryRoom.HasDownExit)
                {
                    secondaryRoom.Down = primaryRoom.Down;
                    primaryRoom.Down.Up = secondaryRoom;
                }
            }
            if (primaryRoom.Left != null)
            {
                if (newPrimaryRoom.HasLeftExit)
                {
                    newPrimaryRoom.Left = primaryRoom.Left;
                    primaryRoom.Left.Right = newPrimaryRoom;
                }
                else if (secondaryRoom.HasLeftExit)
                {
                    secondaryRoom.Left = primaryRoom.Left;
                    primaryRoom.Left.Right = secondaryRoom;
                }
            }
            if (primaryRoom.Right != null)
            {
                if (newPrimaryRoom.HasRightExit)
                {
                    newPrimaryRoom.Right = primaryRoom.Right;
                    primaryRoom.Right.Left = newPrimaryRoom;
                }
                else if (secondaryRoom.HasRightExit)
                {
                    secondaryRoom.Right = primaryRoom.Right;
                    primaryRoom.Right.Left = secondaryRoom;
                }
            }
            if(primaryRoom.HasItem)
            {
                int index = palace.ItemRooms.IndexOf(primaryRoom);
                palace.ItemRooms[index] = newPrimaryRoom;
            }
            if (primaryRoom.IsBossRoom)
            {
                palace.BossRoom = newPrimaryRoom;
            }
            if (primaryRoom.IsThunderBirdRoom)
            {
                palace.TbirdRoom = newPrimaryRoom;
            }
        }
        
        foreach((Room, Room, Room) replacement in replacements)
        {
            palace.AllRooms.Remove(replacement.Item1);
            palace.AllRooms.Add(replacement.Item2);
            palace.AllRooms.Add(replacement.Item3);
        }
    }
}
