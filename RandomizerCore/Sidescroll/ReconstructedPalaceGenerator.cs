using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using NLog;

namespace RandomizerCore.Sidescroll;

public class ReconstructedPalaceGenerator(CancellationToken ct) : PalaceGenerator
{
    private static readonly Logger logger = LogManager.GetCurrentClassLogger();
    private static readonly IEqualityComparer<byte[]> byteArrayEqualityComparer = new Util.StandardByteArrayEqualityComparer();

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
                    Room itemRoom = null;
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
                        palace.SetOpenRoom(segmentedItemRoom2);
                    }
                    palace.SetOpenRoom(palace.Entrance);
                }
                else //GP
                {
                    //thunderbird?
                    if (!props.RemoveTbird)
                    {
                        palace.Tbird = new(roomPool.TbirdRooms[r.Next(roomPool.TbirdRooms.Count)]);
                        palace.Tbird.PalaceGroup = 3;
                        palace.AllRooms.Add(palace.Tbird);
                    }
                    palace.SetOpenRoom(palace.Entrance);

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
                        added = palace.AddRoom(roomToAdd, props.BlockersAnywhere);
                        if (added && roomToAdd.LinkedRoomName != null)
                        {
                            Room linkedRoom = new(roomPool.LinkedRooms[roomToAdd.LinkedRoomName]);
                            linkedRoom.LinkedRoom = roomToAdd;
                            roomToAdd.LinkedRoom = linkedRoom;
                            palace.AddRoom(linkedRoom, props.BlockersAnywhere);
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
                                bool added2 = palace.AddRoom(dropZoneRoom, props.BlockersAnywhere);
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
                                        if (palace.AddRoom(linkedRoom, props.BlockersAnywhere))
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

                    if (palace.GetOpenRooms() >= roomCount - palace.AllRooms.Count) //consolidate
                    {
                        palace.Consolidate();
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
}
