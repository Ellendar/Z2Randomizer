using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using NLog;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace RandomizerCore.Sidescroll;

public class ReconstructedPalaceGenerator(CancellationToken ct) : PalaceGenerator
{
    private static readonly Logger logger = LogManager.GetCurrentClassLogger();

    internal override Palace GeneratePalace(RandomizerProperties props, RoomPool rooms, Random r, int roomCount, int palaceNumber)
    {
        int tries = 0;
        int innertries = 0;
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

        Palace palace;
        do // while (tries >= PALACE_SHUFFLE_ATTEMPT_LIMIT);
        {
            RoomPool roomPool = new(rooms);
            if (ct.IsCancellationRequested)
            {
                return new(palaceNumber);
            }

            tries = 0;
            innertries = 0;
            int roomPlacementFailures = 0;
            do //while (roomPlacementFailures > ROOM_PLACEMENT_FAILURE_LIMIT || palace.AllRooms.Any(i => i.CountOpenExits() > 0));

            {
                palace = new(palaceNumber);
                palace.Entrance = new(roomPool.Entrances[r.Next(roomPool.Entrances.Count)])
                {
                    IsRoot = true,
                    PalaceGroup = palaceGroup
                };
                palace.AllRooms.Add(palace.Entrance);

                palace.BossRoom = new(roomPool.BossRooms[r.Next(roomPool.BossRooms.Count)]);
                palace.BossRoom.Enemies = (byte[])roomPool.VanillaBossRoom.Enemies.Clone();
                palace.BossRoom.NewEnemies = palace.BossRoom.Enemies;
                palace.BossRoom.PalaceGroup = palaceGroup;
                palace.AllRooms.Add(palace.BossRoom);

                if (palaceNumber < 7) //Not GP
                {
                    if(roomPool.ItemRoomsByDirection.Values.Sum(i => i.Count) == 0)
                    {
                        throw new Exception("No item rooms for generated palace");
                    }
                    Direction itemRoomDirection;
                    Room? itemRoom = null;
                    while (itemRoom == null)
                    {
                        itemRoomDirection = DirectionExtensions.RandomItemRoomOrientation(r);
                        if (!roomPool.ItemRoomsByDirection.ContainsKey(itemRoomDirection))
                        {
                            continue;
                        }
                        itemRoom = new(roomPool.ItemRoomsByDirection[itemRoomDirection].ElementAt(r.Next(roomPool.ItemRoomsByDirection[itemRoomDirection].Count)));
                    }
                    palace.ItemRoom = itemRoom;
                    palace.ItemRoom.PalaceGroup = palaceGroup;
                    palace.AllRooms.Add(palace.ItemRoom);

                    if (props.BossRoomConnect)
                    {
                        palace.BossRoom.HasRightExit = true;
                    }

                    if (palace.ItemRoom.LinkedRoomName != null)
                    {
                        Room segmentedItemRoom1, segmentedItemRoom2;
                        segmentedItemRoom1 = palace.ItemRoom;
                        segmentedItemRoom2 = new(roomPool.LinkedRooms[segmentedItemRoom1.LinkedRoomName]);
                        segmentedItemRoom2.PalaceGroup = palaceGroup;
                        //segmentedItemRoom2.SetItem((Item)palaceNumber);
                        segmentedItemRoom2.LinkedRoom = segmentedItemRoom1;
                        segmentedItemRoom1.LinkedRoom = segmentedItemRoom2;
                        palace.AllRooms.Add(segmentedItemRoom2);
                    }
                }
                else //GP
                {
                    //thunderbird?
                    if (!props.RemoveTbird)
                    {
                        palace.TbirdRoom = new(roomPool.TbirdRooms[r.Next(roomPool.TbirdRooms.Count)]);
                        palace.TbirdRoom.PalaceGroup = 3;
                        palace.AllRooms.Add(palace.TbirdRoom);
                    }
                }

                //add rooms
                roomPlacementFailures = 0;
                while (palace.AllRooms.Count < roomCount)
                {
                    if (roomPool.NormalRooms.Count == 0)
                    {
                        throw new Exception("Palace room pool was empty");
                    }
                    int roomIndex = r.Next(roomPool.NormalRooms.Count);
                    Room roomToAdd = new(roomPool.NormalRooms[roomIndex]);

                    roomToAdd.PalaceGroup = palaceGroup;
                    bool added = true;
                    if (roomToAdd.HasDrop && !roomPool.NormalRooms.Any(i => i.IsDropZone && i != roomToAdd))
                    {
                        //Debug.WriteLine(palace.AllRooms.Count + " - 0");
                        added = false;
                    }
                    if (props.NoDuplicateRoomsBySideview)
                    {
                        if (palace.AllRooms.Any(i => byteArrayEqualityComparer.Equals(i.SideView, roomToAdd.SideView)))
                        {
                            Room test = palace.AllRooms.First(i => byteArrayEqualityComparer.Equals(i.SideView, roomToAdd.SideView));
                            added = false;
                        }
                    }
                    if (added)
                    {
                        added = AddRoom(palace, roomToAdd, props.BlockersAnywhere);
                        if (added && roomToAdd.LinkedRoomName != null)
                        {
                            Room linkedRoom = new(roomPool.LinkedRooms[roomToAdd.LinkedRoomName]);
                            linkedRoom.LinkedRoom = roomToAdd;
                            roomToAdd.LinkedRoom = linkedRoom;
                            AddRoom(palace, linkedRoom, props.BlockersAnywhere);
                        }
                    }
                    if (added)
                    {
                        if (props.NoDuplicateRooms)
                        {
                            roomPool.NormalRooms.RemoveAt(roomIndex);
                        }
                        if (roomToAdd.LinkedRoom?.HasDrop ?? false)
                        {
                            roomToAdd = roomToAdd.LinkedRoom;
                        }
                        if (roomToAdd.HasDrop)
                        {
                            int numDrops = r.Next(Math.Min(3, roomCount - palace.AllRooms.Count), Math.Min(6, roomCount - palace.AllRooms.Count));
                            numDrops = Math.Min(numDrops, roomPool.NormalRooms.Count(i => i.IsDropZone) + 1);
                            bool continueDropping = true;
                            int j = 0;
                            int dropPlacementFailures = 0;
                            while (j < numDrops && continueDropping)
                            {
                                List<Room> possibleDropZones = roomPool.NormalRooms.Where(i => i.IsDropZone).ToList();
                                if (possibleDropZones.Count == 0)
                                {
                                    logger.Debug("Exhausted all available drop zones");
                                    return null;
                                }
                                Room dropZoneRoom = new(possibleDropZones[r.Next(0, possibleDropZones.Count)]);
                                bool added2 = AddRoom(palace, dropZoneRoom, props.BlockersAnywhere);
                                if (added2)
                                {
                                    if (props.NoDuplicateRooms)
                                    {
                                        roomPool.NormalRooms.Remove(dropZoneRoom);
                                    }
                                    continueDropping = dropZoneRoom.HasDrop;
                                    if (dropZoneRoom.LinkedRoomName != null)
                                    {
                                        Room linkedRoom = new(roomPool.LinkedRooms[dropZoneRoom.LinkedRoomName]);
                                        if (AddRoom(palace, linkedRoom, props.BlockersAnywhere))
                                        {
                                            linkedRoom.LinkedRoom = dropZoneRoom;
                                            dropZoneRoom.LinkedRoom = linkedRoom;
                                            //If the drop zone isn't a drop, but is linked to a room that is a drop, keep dropping from the linked room
                                            if (!continueDropping && linkedRoom.HasDrop)
                                            {
                                                continueDropping = true;
                                            }
                                        }
                                    }
                                    j++;
                                }
                                else if (++dropPlacementFailures > DROP_PLACEMENT_FAILURE_LIMIT)
                                {
                                    logger.Trace("Drop placement failure limit exceeded.");
                                    break;
                                }
                            }
                        }
                    }
                    else if (++roomPlacementFailures >= ROOM_PLACEMENT_FAILURE_LIMIT)
                    {
                        break;
                    }

                    List <Room> openRooms = palace.AllRooms.Where(i => i.IsOpen()).ToList();

                    if (openRooms.Count >= roomCount - palace.AllRooms.Count) //consolidate
                    {
                        Consolidate(openRooms);
                    }
                }

                innertries++;
            } while (roomPlacementFailures >= ROOM_PLACEMENT_FAILURE_LIMIT
                || palace.AllRooms.Any(i => i.CountOpenExits() > 0)
                );

            if (roomPlacementFailures != ROOM_PLACEMENT_FAILURE_LIMIT)
            {
                int count = 0;
                bool reachable = false;
                do
                {
                    palace.ResetRooms();
                    count++;
                    palace.ShuffleRooms(r);
                    reachable = palace.AllReachable();
                    tries++;
                    logger.Debug("Palace room shuffle attempt #" + tries);
                }
                while (
                (!reachable || (palaceNumber == 7 && props.RequireTbird && !palace.RequiresThunderbird()) || palace.HasDeadEnd())
                && (tries < ROOM_SHUFFLE_ATTEMPT_LIMIT)
                    );
            }
        } while (tries >= ROOM_SHUFFLE_ATTEMPT_LIMIT);
        palace.Generations += tries;
        palace.IsValid = true;
        return palace;
    }

    public bool AddRoom(Palace palace, Room room, bool blockersAnywhere)
    {
        bool placed;
        room.PalaceGroup = palace.GetPalaceGroup();
        int netDeadEnds = palace.AllRooms.Count(i => i.IsDeadEnd);

        if (netDeadEnds > 3 && room.IsDeadEnd)
        {
            return false;
        }

        if (netDeadEnds < -3 && room.CountOpenExits() > 2)
        {
            return false;
        }

        if (!blockersAnywhere)
        {
            RequirementType[] allowedBlockers = Palaces.ALLOWED_BLOCKERS_BY_PALACE[palace.Number - 1];
            if (!room.IsTraversable(allowedBlockers))
            {
                return false;
            }
            if ((palace.Number == 1 || palace.Number == 2 || palace.Number == 5 || palace.Number == 7) && room.HasBoss)
            {
                return false;
            }
        }

        List<Room> openRooms = palace.AllRooms.Where(i => i.IsOpen()).ToList();

        if (openRooms.Count == 0)
        {
            openRooms.Add(room);
            ProcessRoom(palace, room, openRooms);
            return true;
        }
        foreach (Room open in openRooms)
        {
            placed = AttachToOpen(room, open, openRooms);

            if (placed)
            {
                ProcessRoom(palace, room, openRooms);
                return true;
            }

        }
        return false;
    }

    private void ProcessRoom(Palace palace, Room room, List<Room> openRooms)
    {
        palace.AllRooms.Add(room);

        if (palace.Number != 7 && openRooms.Count > 1 && palace.ItemRoom.CountOpenExits() > 0)
        {
            foreach (Room open2 in openRooms)
            {
                bool item = AttachToOpen(open2, palace.ItemRoom, openRooms);
                if (item)
                {
                    break;
                }
            }
        }
        if (openRooms.Count > 1 && palace.BossRoom.CountOpenExits() > 0)
        {
            foreach (Room open2 in openRooms)
            {
                bool boss = AttachToOpen(open2, palace.BossRoom, openRooms);
                if (boss)
                {
                    break;
                }
            }
        }
        if (palace.Number == 7 && openRooms.Count > 1 && palace.TbirdRoom != null && palace.TbirdRoom.CountOpenExits() > 1)
        {
            foreach (Room open2 in openRooms)
            {
                bool boss = AttachToOpen(open2, palace.TbirdRoom, openRooms);
                if (boss)
                {
                    break;
                }
            }
        }
    }

    public void Consolidate(List<Room> openRooms)
    {
        Room[] openCopy = new Room[openRooms.Count];
        openRooms.CopyTo(openCopy);
        foreach (Room r2 in openCopy)
        {
            foreach (Room r3 in openCopy)
            {
                if (r2 != r3 && openRooms.Contains(r2) && openRooms.Contains(r3))
                {
                    AttachToOpen(r2, r3, openRooms);
                }
            }
        }
    }

    /// <summary>
    /// Attach the provided room to the open room if there is a compatable pair of exits between the two rooms.
    /// Rooms attempt to use the exits in the following order (from the perspective of open):  
    /// </summary>
    /// <param name="room"></param> The room to be attached
    /// <param name="open"></param> The room onto which R is attached
    /// <returns>Whether or not the room was actually able to be attached.</returns>
    private bool AttachToOpen(Room room, Room open, List<Room> openRooms)
    {
        bool placed = false;
        //Right from open into r
        if (!placed && open.HasRightExit && open.Right == null && room.HasLeftExit && room.Left == null)
        {
            open.Right = room;
            room.Left = open;

            placed = true;
        }
        //Left open into r
        if (!placed && open.HasLeftExit && open.Left == null && room.HasRightExit && room.Right == null)
        {
            open.Left = room;

            room.Right = open;

            placed = true;
        }
        //Elevator Up from open
        if (!placed && open.HasUpExit && open.Up == null && room.HasDownExit && room.Down == null && !room.HasDrop)
        {
            open.Up = room;
            room.Down = open;

            placed = true;
        }
        //Down Elevator from open
        if (!placed && open.HasDownExit && !open.HasDrop && open.Down == null && room.HasUpExit && room.Up == null)
        {
            open.Down = room;
            room.Up = open;

            placed = true;
        }
        //Drop from open into r
        if (!placed && open.HasDownExit && open.HasDrop && open.Down == null && room.IsDropZone)
        {

            open.Down = room;
            room.IsDropZone = false;
            placed = true;
        }
        //Drop from r into open 
        if (!placed && open.IsDropZone && room.HasDrop && room.Down == null && room.HasDownExit)
        {

            room.Down = open;

            open.IsDropZone = false;
            placed = true;
        }
        //If the room doesn't have any open exits anymore, remove it from the list
        //#13: If the room doesn't have any open exits, how did it get into the 
        if (open.CountOpenExits() == 0)
        {
            openRooms.Remove(open);
        }
        //Otherwise, if the open room isn't in the open rooms list (What? How?) and the pending openings hasn't been met,
        //put the open room in openRooms where it belongs, and then for some reason mark that we successfully placed the room even though we didn't.
        else if (!openRooms.Contains(open) && (openRooms.Count < 3 || placed))
        {
            openRooms.Add(open);
            placed = true;
        }
        //If the room itself is already in the open rooms list (How?), but we filled the last exit, remove it from the open rooms list.
        if (room.CountOpenExits() == 0)
        {
            openRooms.Remove(room);
        }
        //Otherwise, if the room being added still has unmatched openings, and the maximum pending openings hasn't been met, add this room to the open rooms list
        else if (!openRooms.Contains(room) && (openRooms.Count < 3 || placed))
        {
            openRooms.Add(room);
            placed = true;
        }

        return placed;
    }
}
