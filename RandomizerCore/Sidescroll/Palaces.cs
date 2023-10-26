using NLog;
using RandomizerCore.Sidescroll;
using SD.Tools.Algorithmia.GeneralDataStructures;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Speech.Synthesis;
using Z2Randomizer.Core.Overworld;

namespace Z2Randomizer.Core.Sidescroll;

public class Palaces
{
    private static readonly Logger logger = LogManager.GetCurrentClassLogger();

    private const int PALACE_SHUFFLE_ATTEMPT_LIMIT = 100;
    private const int DROP_PLACEMENT_FAILURE_LIMIT = 100;
    private const int ROOM_PLACEMENT_FAILURE_LIMIT = 100;

    private static readonly RequirementType[] VANILLA_P1_ALLOWED_BLOCKERS = new RequirementType[] { 
        RequirementType.KEY };
    private static readonly RequirementType[] VANILLA_P2_ALLOWED_BLOCKERS = new RequirementType[] { 
        RequirementType.KEY, RequirementType.JUMP, RequirementType.GLOVE };
    private static readonly RequirementType[] VANILLA_P3_ALLOWED_BLOCKERS = new RequirementType[] { 
        RequirementType.KEY, RequirementType.DOWNSTAB, RequirementType.UPSTAB, RequirementType.GLOVE};
    private static readonly RequirementType[] VANILLA_P4_ALLOWED_BLOCKERS = new RequirementType[] { 
        RequirementType.KEY, RequirementType.FAIRY, RequirementType.JUMP};
    private static readonly RequirementType[] VANILLA_P5_ALLOWED_BLOCKERS = new RequirementType[] { 
        RequirementType.KEY, RequirementType.FAIRY, RequirementType.JUMP, RequirementType.GLOVE};
    private static readonly RequirementType[] VANILLA_P6_ALLOWED_BLOCKERS = new RequirementType[] { 
        RequirementType.KEY, RequirementType.FAIRY, RequirementType.JUMP, RequirementType.GLOVE};
    private static readonly RequirementType[] VANILLA_P7_ALLOWED_BLOCKERS = new RequirementType[] { 
        RequirementType.FAIRY, RequirementType.UPSTAB, RequirementType.DOWNSTAB, RequirementType.JUMP, RequirementType.GLOVE};

    public static readonly RequirementType[][] ALLOWED_BLOCKERS_BY_PALACE = new RequirementType[][] { 
        VANILLA_P1_ALLOWED_BLOCKERS,
        VANILLA_P2_ALLOWED_BLOCKERS,
        VANILLA_P3_ALLOWED_BLOCKERS,
        VANILLA_P4_ALLOWED_BLOCKERS,
        VANILLA_P5_ALLOWED_BLOCKERS,
        VANILLA_P6_ALLOWED_BLOCKERS,
        VANILLA_P7_ALLOWED_BLOCKERS
    };


    private static readonly SortedDictionary<int, int> palaceConnectionLocs = new SortedDictionary<int, int>
    {
        {1, 0x1072B},
        {2, 0x1072B},
        {3, 0x12208},
        {4, 0x12208},
        {5, 0x1072B},
        {6, 0x12208},
        {7, 0x1472B},
    };

    private static readonly Dictionary<int, int> palaceAddr = new Dictionary<int, int>
    {
        {1, 0x4663 },
        {2, 0x4664 },
        {3, 0x4665 },
        {4, 0xA140 },
        {5, 0x8663 },
        {6, 0x8664 },
        {7, 0x8665 }
    };

    public static List<Palace> CreatePalaces(BackgroundWorker worker, Random r, RandomizerProperties props, ROM ROMData, bool raftIsRequired)
    {
        if (props.UseCustomRooms && !File.Exists("CustomRooms.json"))
        {
            throw new Exception("Couldn't find CustomRooms.json. Please create the file or disable custom rooms on the misc tab.");
        }
        List<Palace> palaces = new List<Palace>();
        List<Room> roomPool = new List<Room>();
        List<Room> gpRoomPool = new List<Room>();
        Dictionary<byte[], List<Room>> sideviews = new Dictionary<byte[], List<Room>>(new Util.StandardByteArrayEqualityComparer());
        Dictionary<byte[], List<Room>> sideviewsgp = new Dictionary<byte[], List<Room>>(new Util.StandardByteArrayEqualityComparer());
        int enemyBytes = 0;
        int enemyBytesGP = 0;
        int mapNo = 0;
        int mapNoGp = 0;
        //This is unfortunate because there is no list-backed MutliValueDictionary like apache has, so we have to deal with
        //O(n) random access.
        MultiValueDictionary<int, Room> entrancesByPalaceNumber = new();
        MultiValueDictionary<int, Room> bossRoomsByPalaceNumber = new();
        List<Room> tbirdRooms = new();
        MultiValueDictionary<Direction, Room> itemRoomsByDirection = new();
        if (props.AllowVanillaRooms)
        {
            for (int palaceNum = 1; palaceNum < 8; palaceNum++)
            {
                entrancesByPalaceNumber.AddRange(palaceNum, PalaceRooms.Entrances(RoomGroup.VANILLA, props.UseCustomRooms)
                    .Where(i => i.PalaceNumber == null || i.PalaceNumber == palaceNum ).ToList());
                bossRoomsByPalaceNumber.AddRange(palaceNum, PalaceRooms.BossRooms(RoomGroup.VANILLA, props.UseCustomRooms)
                    .Where(i => i.PalaceNumber == null || i.PalaceNumber == palaceNum).ToList());
                tbirdRooms.AddRange(PalaceRooms.TBirdRooms(RoomGroup.VANILLA, props.UseCustomRooms)
                    .Where(i => i.PalaceNumber == null || i.PalaceNumber == palaceNum).ToList());
                foreach (Direction direction in DirectionExtensions.ITEM_ROOM_ORIENTATIONS)
                {
                    itemRoomsByDirection.AddRange(direction, PalaceRooms.ItemRoomsByDirection(RoomGroup.VANILLA, direction, props.UseCustomRooms).ToList());
                }
            }  
        }

        if (props.AllowV4Rooms)
        {
            for (int palaceNum = 1; palaceNum < 8; palaceNum++)
            {
                entrancesByPalaceNumber.AddRange(palaceNum, PalaceRooms.Entrances(RoomGroup.V4_0, props.UseCustomRooms)
                    .Where(i => i.PalaceNumber == null || i.PalaceNumber == palaceNum).ToList());
                bossRoomsByPalaceNumber.AddRange(palaceNum, PalaceRooms.BossRooms(RoomGroup.V4_0, props.UseCustomRooms)
                    .Where(i => i.PalaceNumber == null || i.PalaceNumber == palaceNum).ToList());
                tbirdRooms.AddRange(PalaceRooms.TBirdRooms(RoomGroup.V4_0, props.UseCustomRooms)
                    .Where(i => i.PalaceNumber == null || i.PalaceNumber == palaceNum).ToList());
                foreach (Direction direction in DirectionExtensions.ITEM_ROOM_ORIENTATIONS)
                {
                    itemRoomsByDirection.AddRange(direction, PalaceRooms.ItemRoomsByDirection(RoomGroup.V4_0, direction, props.UseCustomRooms).ToList());
                }
            }
        }

        if (props.AllowV4_4Rooms)
        {
            for(int palaceNum = 1; palaceNum < 8; palaceNum++)
            {
                entrancesByPalaceNumber.AddRange(palaceNum, PalaceRooms.Entrances(RoomGroup.V4_4, props.UseCustomRooms)
                    .Where(i => i.PalaceNumber == null || i.PalaceNumber == palaceNum).ToList());
                bossRoomsByPalaceNumber.AddRange(palaceNum, PalaceRooms.BossRooms(RoomGroup.V4_4, props.UseCustomRooms)
                    .Where(i => i.PalaceNumber == null || i.PalaceNumber == palaceNum).ToList());
                tbirdRooms.AddRange(PalaceRooms.TBirdRooms(RoomGroup.V4_4, props.UseCustomRooms)
                    .Where(i => i.PalaceNumber == null || i.PalaceNumber == palaceNum).ToList());
                foreach (Direction direction in DirectionExtensions.ITEM_ROOM_ORIENTATIONS)
                {
                    itemRoomsByDirection.AddRange(direction, PalaceRooms.ItemRoomsByDirection(RoomGroup.V4_4, direction, props.UseCustomRooms).ToList());
                }
            }           
        }

        if (props.NormalPalaceStyle.IsReconstructed())
        {
            roomPool.Clear();
            if (props.AllowVanillaRooms)
            {
                roomPool.AddRange(PalaceRooms.NormalPalaceRoomsByGroup(RoomGroup.VANILLA, props.UseCustomRooms));
            }

            if (props.AllowV4Rooms)
            {
                roomPool.AddRange(PalaceRooms.NormalPalaceRoomsByGroup(RoomGroup.V4_0, props.UseCustomRooms));
            }

            if (props.AllowV4_4Rooms)
            {
                roomPool.AddRange(PalaceRooms.NormalPalaceRoomsByGroup(RoomGroup.V4_4, props.UseCustomRooms));
            }
        }


        if (props.GPStyle.IsReconstructed())
        {
            gpRoomPool.Clear();
            if (props.AllowVanillaRooms)
            {
                gpRoomPool.AddRange(PalaceRooms.GPRoomsByGroup(RoomGroup.VANILLA, props.UseCustomRooms));
            }

            if (props.AllowV4Rooms)
            {
                gpRoomPool.AddRange(PalaceRooms.GPRoomsByGroup(RoomGroup.V4_0, props.UseCustomRooms));
            }

            if (props.AllowV4_4Rooms)
            {
                gpRoomPool.AddRange(PalaceRooms.GPRoomsByGroup(RoomGroup.V4_4, props.UseCustomRooms));
            }
        }


        int[] sizes = new int[7];

        sizes[0] = r.Next(10, 17);
        sizes[1] = r.Next(16, 25);
        sizes[2] = r.Next(11, 18);
        sizes[3] = r.Next(16, 25);
        sizes[4] = r.Next(23, 63 - sizes[0] - sizes[1]);
        sizes[5] = r.Next(22, 63 - sizes[2] - sizes[3]);

        if (props.GPStyle == PalaceStyle.RECONSTRUCTED_SHORTENED)
        {
            sizes[6] = r.Next(27, 41);
        }
        else
        {
            sizes[6] = r.Next(54, 60);
        }

        for (int currentPalace = 1; currentPalace < 8; currentPalace++)
        {
            if (currentPalace == 7 && props.GPStyle.IsReconstructed() || currentPalace < 7 && props.NormalPalaceStyle.IsReconstructed())
            {
                Palace palace;// = new Palace(i, palaceAddr[i], palaceConnectionLocs[i], this.ROMData);
                int tries = 0;
                int innertries = 0;
                int palaceGroup = currentPalace switch
                {
                    1 => 1,
                    2 => 1,
                    3 => 2,
                    4 => 2,
                    5 => 1,
                    6 => 2,
                    7 => 3,
                    _ => throw new ImpossibleException("Invalid palace number: " + currentPalace)
                };

                do // while (tries >= PALACE_SHUFFLE_ATTEMPT_LIMIT);
                {
                    if (worker != null && worker.CancellationPending)
                    {
                        return null;
                    }

                    tries = 0;
                    innertries = 0;
                    int roomPlacementFailures = 0;
                    //bool done = false;
                    do //while (roomPlacementFailures > ROOM_PLACEMENT_FAILURE_LIMIT || palace.AllRooms.Any(i => i.CountOpenExits() > 0));

                    {
                        List<Room> palaceRoomPool = new List<Room>(currentPalace == 7 ? gpRoomPool : roomPool)
                            .Where(i => i.PalaceNumber == null || i.PalaceNumber == currentPalace).ToList();
                        mapNo = currentPalace switch
                        {
                            1 => 0,
                            2 => palaces[0].AllRooms.Count,
                            3 => 0,
                            4 => palaces[2].AllRooms.Count,
                            5 => palaces[0].AllRooms.Count + palaces[1].AllRooms.Count,
                            6 => mapNo = palaces[2].AllRooms.Count + palaces[3].AllRooms.Count,
                            _ => 0
                        };

                        if (currentPalace == 7)
                        {
                            mapNoGp = 0;
                        }

                        palace = new Palace(currentPalace, palaceAddr[currentPalace], palaceConnectionLocs[currentPalace], props.UseCustomRooms);
                        palace.Root = new(entrancesByPalaceNumber[currentPalace].ElementAt(r.Next(entrancesByPalaceNumber[currentPalace].Count)))
                        {
                            IsRoot = true,
                            PalaceGroup = palaceGroup
                        };
                        palace.AllRooms.Add(palace.Root);

                        palace.BossRoom = new(bossRoomsByPalaceNumber[currentPalace].ElementAt(r.Next(bossRoomsByPalaceNumber[currentPalace].Count)));
                        palace.BossRoom.PalaceGroup = palaceGroup;
                        palace.AllRooms.Add(palace.BossRoom);

                        if (currentPalace < 7) //Not GP
                        {
                            Direction itemRoomDirection;
                            Room itemRoom = null;
                            while (itemRoom == null)
                            {
                                itemRoomDirection = DirectionExtensions.RandomItemRoomOrientation(r);
                                if (!itemRoomsByDirection.ContainsKey(itemRoomDirection))
                                {
                                    continue;
                                }
                                itemRoom = new(itemRoomsByDirection[itemRoomDirection].ElementAt(r.Next(itemRoomsByDirection[itemRoomDirection].Count)));
                            }
                            palace.ItemRoom = itemRoom;
                            palace.ItemRoom.PalaceGroup = palaceGroup;
                            palace.AllRooms.Add(palace.ItemRoom);

                            palace.Root.NewMap = mapNo;
                            IncrementMapNo(ref mapNo, ref mapNoGp, currentPalace);
                            palace.BossRoom.NewMap = mapNo;
                            if (props.BossRoomConnect)
                            {
                                palace.BossRoom.RightByte = 0x69;
                            }
                            IncrementMapNo(ref mapNo, ref mapNoGp, currentPalace);
                            palace.ItemRoom.NewMap = mapNo;
                            palace.ItemRoom.SetItem((Item)currentPalace);
                            IncrementMapNo(ref mapNo, ref mapNoGp, currentPalace);

                            if (palace.ItemRoom.LinkedRoomName != null)
                            {
                                Room segmentedItemRoom1, segmentedItemRoom2;
                                segmentedItemRoom1 = palace.ItemRoom;
                                segmentedItemRoom2 = PalaceRooms.GetRoomByName(segmentedItemRoom1.LinkedRoomName, props.UseCustomRooms);
                                segmentedItemRoom2.NewMap = palace.ItemRoom.NewMap;
                                segmentedItemRoom2.PalaceGroup = palaceGroup;
                                segmentedItemRoom2.SetItem((Item)currentPalace);
                                palace.AllRooms.Add(segmentedItemRoom2);
                                palace.SetOpenRoom(segmentedItemRoom2);
                            }
                            palace.SetOpenRoom(palace.Root);
                        }
                        else //GP
                        {
                            palace.Root.NewMap = mapNoGp;
                            IncrementMapNo(ref mapNo, ref mapNoGp, currentPalace);
                            palace.BossRoom.NewMap = mapNoGp;
                            IncrementMapNo(ref mapNo, ref mapNoGp, currentPalace);

                            //thunderbird?
                            if (!props.RemoveTbird)
                            {

                                palace.Tbird = new(tbirdRooms[r.Next(tbirdRooms.Count)]);
                                palace.Tbird.NewMap = mapNoGp;
                                palace.Tbird.PalaceGroup = 3;
                                IncrementMapNo(ref mapNo, ref mapNoGp, currentPalace);
                                palace.AllRooms.Add(palace.Tbird);
                            }
                            palace.SetOpenRoom(palace.Root);

                        }


                        palace.MaxRooms = sizes[currentPalace - 1];

                        //add rooms
                        bool dropped = false;
                        roomPlacementFailures = 0;
                        while (palace.AllRooms.Count < palace.MaxRooms)
                        {
                            if (palaceRoomPool.Count == 0)
                            {
                                throw new Exception("Palace room pool was empty");
                            }
                            int roomIndex = r.Next(palaceRoomPool.Count);
                            Room roomToAdd = new(palaceRoomPool[roomIndex]);

                            roomToAdd.PalaceGroup = palaceGroup;
                            roomToAdd.NewMap = currentPalace < 7 ? mapNo : mapNoGp;
                            bool added;
                            if (roomToAdd.HasDrop && palaceRoomPool.Count(i => i.IsDropZone && i != roomToAdd) == 0)
                            {
                                //Debug.WriteLine(palace.AllRooms.Count + " - 0");
                                added = false;
                            }
                            else
                            {
                                added = palace.AddRoom(roomToAdd, props.BlockersAnywhere);
                                if (added && roomToAdd.LinkedRoomName != null)
                                {
                                    Room linkedRoom = PalaceRooms.GetRoomByName(roomToAdd.LinkedRoomName, props.UseCustomRooms);
                                    linkedRoom.NewMap = currentPalace < 7 ? mapNo : mapNoGp;
                                    palace.AddRoom(linkedRoom, props.BlockersAnywhere);
                                }
                            }
                            if (added)
                            {
                                if (props.NoDuplicateRooms)
                                {
                                    palaceRoomPool.RemoveAt(roomIndex);
                                }
                                IncrementMapNo(ref mapNo, ref mapNoGp, currentPalace);
                                if (roomToAdd.HasDrop && !dropped)
                                {
                                    int numDrops = r.Next(Math.Min(3, palace.MaxRooms - palace.AllRooms.Count), Math.Min(6, palace.MaxRooms - palace.AllRooms.Count));
                                    numDrops = Math.Min(numDrops, palaceRoomPool.Count(i => i.IsDropZone) + 1);
                                    bool continueDropping = true;
                                    int j = 0;
                                    int dropPlacementFailures = 0;
                                    while (j < numDrops && continueDropping)
                                    {
                                        List<Room> possibleDropZones = palaceRoomPool.Where(i => i.IsDropZone).ToList();
                                        if (possibleDropZones.Count == 0)
                                        {
                                            throw new ImpossibleException();
                                        }
                                        Room dropZoneRoom = new(possibleDropZones[r.Next(0, possibleDropZones.Count)]);
                                        dropZoneRoom.NewMap = currentPalace < 7 ? mapNo : mapNoGp;
                                        bool added2 = palace.AddRoom(dropZoneRoom, props.BlockersAnywhere);
                                        if (added2 && dropZoneRoom.LinkedRoomName != null)
                                        {
                                            Room linkedRoom = PalaceRooms.GetRoomByName(dropZoneRoom.LinkedRoomName, props.UseCustomRooms);
                                            linkedRoom.NewMap = currentPalace < 7 ? mapNo : mapNoGp;
                                            palace.AddRoom(linkedRoom, props.BlockersAnywhere);
                                            dropZoneRoom.LinkedRoom = linkedRoom;
                                        }
                                        if (added2)
                                        {
                                            if (props.NoDuplicateRooms)
                                            {
                                                palaceRoomPool.Remove(dropZoneRoom);
                                            }
                                            IncrementMapNo(ref mapNo, ref mapNoGp, currentPalace);
                                            continueDropping = dropZoneRoom.HasDrop;
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

                            if (palace.GetOpenRooms() >= palace.MaxRooms - palace.AllRooms.Count) //consolidate
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
                        palace.ShuffleRooms(r);
                        bool reachable = palace.AllReachable();
                        while (
                            (!reachable || (currentPalace == 7 && props.RequireTbird && !palace.RequiresThunderbird()) || palace.HasDeadEnd())
                            && (tries < PALACE_SHUFFLE_ATTEMPT_LIMIT)
                            )
                        {
                            palace.ResetRooms();
                            palace.ShuffleRooms(r);
                            reachable = palace.AllReachable();
                            tries++;
                            logger.Debug("Palace room shuffle attempt #" + tries);
                        }
                    }

                } while (tries >= PALACE_SHUFFLE_ATTEMPT_LIMIT);
                palace.Generations += tries;
                palaces.Add(palace);
                foreach (Room room in palace.AllRooms)
                {
                    if (currentPalace == 7)
                    {
                        enemyBytesGP += room.Enemies.Length;
                        if (sideviewsgp.ContainsKey(room.SideView))
                        {
                            sideviewsgp[room.SideView].Add(room);
                        }
                        else
                        {
                            List<Room> l = new List<Room> { room };
                            sideviewsgp.Add(room.SideView, l);
                        }
                    }
                    else
                    {
                        enemyBytes += room.Enemies.Length;
                        if (sideviews.ContainsKey(room.SideView))
                        {
                            sideviews[room.SideView].Add(room);
                        }
                        else
                        {
                            List<Room> l = new List<Room> { room };
                            sideviews.Add(room.SideView, l);
                        }
                    }
                }
            }
            //NOT RECONSTRUCTED
            else
            {
                Palace palace = new Palace(currentPalace, palaceAddr[currentPalace], palaceConnectionLocs[currentPalace], props.UseCustomRooms);
                PalaceStyle palaceStyle = currentPalace == 7 ? props.GPStyle : props.NormalPalaceStyle;
                //p.dumpMaps();
                int palaceGroup = currentPalace switch
                {
                    1 => 1,
                    2 => 1,
                    3 => 2,
                    4 => 2,
                    5 => 1,
                    6 => 2,
                    7 => 3,
                    _ => throw new ImpossibleException("Invalid palace number: " + currentPalace)
                };

                palace.Root = new(entrancesByPalaceNumber[currentPalace].First());
                palace.Root.PalaceGroup = palaceGroup;
                palace.BossRoom = PalaceRooms.VanillaBossRoom(currentPalace);
                palace.BossRoom.PalaceGroup = palaceGroup;
                palace.AllRooms.Add(palace.Root);
                if (currentPalace != 7)
                {
                    Room itemRoom = null;
                    itemRoom = new(PalaceRooms.VanillaItemRoom(currentPalace));

                    palace.ItemRoom = itemRoom;
                    palace.ItemRoom.PalaceGroup = palaceGroup;
                    palace.AllRooms.Add(palace.ItemRoom);
                }
                palace.AllRooms.Add(palace.BossRoom);
                if (currentPalace == 7)
                {
                    Room bird = new(PalaceRooms.TBirdRooms(RoomGroup.VANILLA, props.UseCustomRooms).First());
                    bird.PalaceGroup = palaceGroup;
                    palace.AllRooms.Add(bird);
                    palace.Tbird = bird;

                }
                foreach (Room v in PalaceRooms.VanillaPalaceRoomsByPalaceNumber(currentPalace, props.UseCustomRooms))
                {
                    Room room = new(v);
                    room.PalaceGroup = palaceGroup;
                    palace.AllRooms.Add(room);
                    
                    if(room.LinkedRoomName != null)
                    {
                        Room linkedRoom = new Room(PalaceRooms.GetRoomByName(room.LinkedRoomName, props.UseCustomRooms));
                        linkedRoom.PalaceGroup = palaceGroup;
                        palace.AllRooms.Add(linkedRoom);
                    }
                }
                bool removeTbird = (currentPalace == 7 && props.RemoveTbird);
                palace.CreateTree(removeTbird);

                //I broke shortened vanilla GP. I'm not sure anyone cares.
                /*
                if (currentPalace == 7 && props.ShortenGP)
                {
                    palace.Shorten(r);
                }
                */

                if (palaceStyle == PalaceStyle.SHUFFLED)
                {
                    palace.ShuffleRooms(r);
                }
                while (!palace.AllReachable() || (currentPalace == 7 && props.RequireTbird && !palace.RequiresThunderbird()) || palace.HasDeadEnd())
                {
                    palace.ResetRooms();
                    if (palaceStyle == PalaceStyle.SHUFFLED)
                    {
                        palace.ShuffleRooms(r);
                    }
                }
                palaces.Add(palace);
            }
        }

        //On second thought, we're not going to merge the rooms until we can have some sort of exit-exit
        //connection/requirement graph, otherwise clearability will be impossible to properly implement.
        /*
        //Unify the parts of segmented rooms back together
        foreach(Palace palace in palaces)
        {
            foreach(Room room in palace.AllRooms.ToList())
            {
                //If this is the primary room of a linked room pair
                if(room.LinkedRoomName != null && room.Enabled)
                {
                    Room linkedRoom = room.LinkedRoom;
                    //set each blank exit on the master room that has a counterpart in the linked room
                    if(room.HasLeftExit() && room.Left == null && linkedRoom.Left != null)
                    {
                        room.Left = linkedRoom.Left;
                    }
                    if (room.HasRightExit() && room.Right == null && linkedRoom.Right != null)
                    {
                        room.Right = linkedRoom.Right;
                    }
                    if (room.HasUpExit() && room.Up == null && linkedRoom.Up != null)
                    {
                        room.Up = linkedRoom.Up;
                    }
                    if (room.HasDownExit() && room.Down == null && linkedRoom.Down != null)
                    {
                        room.Down = linkedRoom.Down;
                    }

                    //set each room that links to the secondary room to point to the master room instead
                    palace.AllRooms.Where(i => i.Left == linkedRoom).ToList().ForEach(i => i.Left = room);
                    palace.AllRooms.Where(i => i.Right == linkedRoom).ToList().ForEach(i => i.Right = room);
                    palace.AllRooms.Where(i => i.Up == linkedRoom).ToList().ForEach(i => i.Up = room);
                    palace.AllRooms.Where(i => i.Down == linkedRoom).ToList().ForEach(i => i.Down = room);

                }
            }
        }
        */

        if (!ValidatePalaces(props, raftIsRequired, palaces))
        {
            return null;
        }

        //Randomize Enemies
        if (props.ShufflePalaceEnemies)
        {
            foreach (Palace palace in palaces)
            {
                palace.RandomizeEnemies(props, r);
            }
        }

        Dictionary<int, int> freeSpace = SetupFreeSpace(true, 0);
        //update pointers
        //if (props.NormalPalaceStyle.IsReconstructed())
        if (true)
        {
            if (enemyBytes > 0x400 || enemyBytesGP > 681)
            {
                return new List<Palace>();
            }
            //In Reconstructed, enemy pointers aren't separated between 125 and 346, they're just all in 1 big pile,
            //so we just start at the 125 pointer address
            int enemyAddr = Enemies.NormalPalaceEnemyAddr;
            foreach (byte[] sv in sideviews.Keys)
            {
                int sideViewAddr = FindFreeSpace(freeSpace, sv);
                if (sideViewAddr == -1) //not enough space
                    return new List<Palace>();
                ROMData.Put(sideViewAddr, sv);
                if (ROMData.GetByte(sideViewAddr + sv.Length) >= 0xD0)
                {
                    ROMData.Put(sideViewAddr + sv.Length, 0x00);
                }
                List<Room> rooms = sideviews[sv];
                foreach (Room room in rooms)
                {
                    room.WriteSideViewPtr(sideViewAddr, ROMData);
                    room.UpdateBitmask(ROMData);
                    room.UpdateEnemies(enemyAddr, ROMData, props.NormalPalaceStyle, props.GPStyle);
                    enemyAddr += room.NewEnemies.Length;
                    room.UpdateConnectors();
                }
            }
        }
        //if (props.GPStyle.IsReconstructed())
        if(true)
        {
            //GP Reconstructed
            int enemyAddr = Enemies.GPEnemyAddr;
            freeSpace = SetupFreeSpace(false, enemyBytesGP);
            foreach (byte[] sv in sideviewsgp.Keys)
            {
                int sideviewAddr = FindFreeSpace(freeSpace, sv);
                if (sideviewAddr == -1) //not enough space
                {
                    return new List<Palace>();
                }
                ROMData.Put(sideviewAddr, sv);
                if (ROMData.GetByte(sideviewAddr + sv.Length) >= 0xD0)
                {
                    ROMData.Put(sideviewAddr + sv.Length, 0x00);
                }
                List<Room> rooms = sideviewsgp[sv];
                foreach (Room room in rooms)
                {
                    room.WriteSideViewPtr(sideviewAddr, ROMData);
                    room.UpdateBitmask(ROMData);
                    room.UpdateEnemies(enemyAddr, ROMData, props.NormalPalaceStyle, props.GPStyle);
                    enemyAddr += room.Enemies.Length;
                    //room.UpdateConnectors(ROMData, room == palaces[6].Root);
                    room.UpdateConnectors();
                }
            }
        }
        else //Not reconstructed
        {
            if (props.ShufflePalaceEnemies) 
            {
                int enemyAddr = Enemies.NormalPalaceEnemyAddr;
                int enemiesLength = 0;
                foreach (Room room in palaces[0].AllRooms.Union(palaces[1].AllRooms).Union(palaces[4].AllRooms).OrderBy(i => i.NewMap == 0 ? i.Map : i.NewMap))
                {
                    room.UpdateEnemies(enemyAddr, ROMData, props.NormalPalaceStyle, props.GPStyle);
                    enemyAddr += room.Enemies.Length;
                    enemiesLength += room.Enemies.Length;
                }

                foreach (Room room in palaces[2].AllRooms.Union(palaces[3].AllRooms).Union(palaces[5].AllRooms).OrderBy(i => i.NewMap == 0 ? i.Map : i.NewMap))
                {
                    room.UpdateEnemies(enemyAddr, ROMData, props.NormalPalaceStyle, props.GPStyle);
                    enemyAddr += room.Enemies.Length;
                    enemiesLength += room.Enemies.Length;
                }

                enemyAddr = Enemies.GPEnemyAddr;
                foreach (Room room in palaces[6].AllRooms)
                {
                    room.UpdateEnemies(enemyAddr, ROMData, props.NormalPalaceStyle, props.GPStyle);
                    enemyAddr += room.Enemies.Length;
                }
            }
        }
        
        if (props.ShuffleSmallItems || props.ExtraKeys)
        {
            palaces[0].ShuffleSmallItems(4, true, r, props.ShuffleSmallItems, props.ExtraKeys, ROMData);
            palaces[1].ShuffleSmallItems(4, true, r, props.ShuffleSmallItems, props.ExtraKeys, ROMData);
            palaces[2].ShuffleSmallItems(4, false, r, props.ShuffleSmallItems, props.ExtraKeys, ROMData);
            palaces[3].ShuffleSmallItems(4, false, r, props.ShuffleSmallItems, props.ExtraKeys, ROMData);
            palaces[4].ShuffleSmallItems(4, true, r, props.ShuffleSmallItems, props.ExtraKeys, ROMData);
            palaces[5].ShuffleSmallItems(4, false, r, props.ShuffleSmallItems, props.ExtraKeys, ROMData);
            palaces[6].ShuffleSmallItems(5, true, r, props.ShuffleSmallItems, props.ExtraKeys, ROMData);
        }
        foreach(Palace palace in palaces)
        {
            palace.ValidateRoomConnections();
            palace.UpdateRom(ROMData);
        }

        return palaces;
    }

    private static void IncrementMapNo(ref int mapNo, ref int mapNoGp, int i)
    {
        if (i < 7)
        {
            mapNo++;
            //if (bossRooms.Contains(mapNo) && (i == 1 || i == 2 || i == 5))
            //{
            //    mapNo++;
            //}
            //else if(bossRooms2.Contains(mapNo) && (i == 3 || i == 4 || i == 6))
            //{
            //    mapNo++;
            //}
        }
        else
        {
            mapNoGp++;
            //while (bossRooms3.Contains(mapNoGp))
            //{
            //    mapNoGp++;
            //}
        }
    }
    /*
    private static Room SelectBossRoom(int palaceNumber, Random r, bool useCustomRooms, bool)
    {
        if(useCommunityRooms)
        {
            if (palaceNumber == 7)
            {
                return PalaceRooms.DarkLinkRooms(useCustomRooms)[r.Next(PalaceRooms.DarkLinkRooms(useCustomRooms).Count)].DeepCopy();
            }
            if (palaceNumber == 6)
            {
                return PalaceRooms.NewP6BossRooms(useCustomRooms)[r.Next(PalaceRooms.NewP6BossRooms(useCustomRooms).Count)].DeepCopy();
            }
            Room room = PalaceRooms.NewBossRooms(useCustomRooms)[r.Next(PalaceRooms.NewBossRooms(useCustomRooms).Count)].DeepCopy();
            room.Enemies = PalaceRooms.BossRooms(useCustomRooms)[palaceNumber - 1].Enemies;
            return room;
        }
        else
        {
            return palaceNumber switch
            {
                1 => PalaceRooms.BossRooms(useCustomRooms).Where(i => i.Map == 13).First().DeepCopy(),
                2 => PalaceRooms.BossRooms(useCustomRooms).Where(i => i.Map == 34).First().DeepCopy(),
                3 => PalaceRooms.BossRooms(useCustomRooms).Where(i => i.Map == 14).First().DeepCopy(),
                4 => PalaceRooms.BossRooms(useCustomRooms).Where(i => i.Map == 28).First().DeepCopy(),
                5 => PalaceRooms.BossRooms(useCustomRooms).Where(i => i.Map == 41).First().DeepCopy(),
                6 => PalaceRooms.BossRooms(useCustomRooms).Where(i => i.Map == 58).First().DeepCopy(),
                7 => PalaceRooms.BossRooms(useCustomRooms).Where(i => i.Map == 54).First().DeepCopy(),
                _ => throw new ImpossibleException("Unable to find vanilla boss room")
            };
        }
    }
    */

    /*
    public static Room GenerateItemRoom(Random r, bool useCustomRooms, bool useCommunityRooms)
    {
        if(!useCommunityRooms)
        {
            return PalaceRooms.ItemRooms(useCustomRooms)[r.Next(PalaceRooms.ItemRooms(useCustomRooms).Count)].DeepCopy();
        }
        return r.Next(5) switch
        {
            //left
            0 => PalaceRooms.LeftOpenItemRooms(useCustomRooms)[r.Next(PalaceRooms.LeftOpenItemRooms(useCustomRooms).Count)].DeepCopy(),
            //right
            1 => PalaceRooms.RightOpenItemRooms(useCustomRooms)[r.Next(PalaceRooms.RightOpenItemRooms(useCustomRooms).Count)].DeepCopy(),
            //up
            2 => PalaceRooms.UpOpenItemRooms(useCustomRooms)[r.Next(PalaceRooms.UpOpenItemRooms(useCustomRooms).Count)].DeepCopy(),
            //down
            3 => PalaceRooms.DownOpenItemRooms(useCustomRooms)[r.Next(PalaceRooms.DownOpenItemRooms(useCustomRooms).Count)].DeepCopy(),
            //Through
            4 => PalaceRooms.ThroughItemRooms(useCustomRooms)[r.Next(PalaceRooms.ThroughItemRooms(useCustomRooms).Count)].DeepCopy(),
            _ => throw new Exception("Invalid item room direction selection."),
        };
    }
    */

    private static Dictionary<int, int> SetupFreeSpace(bool bank4, int enemyData)
    {
        Dictionary<int, int> freeSpace = new Dictionary<int, int>();
        if (bank4)
        {
            freeSpace.Add(0x103EC, 147);
            freeSpace.Add(0x10649, 225);
            freeSpace.Add(0x10827, 88);
            freeSpace.Add(0x10cb0, 1887);
            //Bugfix for tyvarius's seed with a scuffed P3 entrance. This was 1 byte too long, which caused the sideview data to overflow
            //into the palace set 2 sideview pointer data starting at 0x12010
            freeSpace.Add(0x11ef0, 287);
            freeSpace.Add(0x12124, 78);
            freeSpace.Add(0x1218b, 124);
            freeSpace.Add(0x12304, 1547);
        }
        else
        {
            freeSpace.Add(0x1435e, 385);
            freeSpace.Add(0x1462f, 251);
            freeSpace.Add(0x14827, 137);
            //freeSpace.Add(0x148b0 + enemyData, 681 - enemyData);
            freeSpace.Add(0x153be, 82);
            freeSpace.Add(0x1655f, 177);
            //freeSpace.Add(0x17db1, 447);
            freeSpace.Add(0x1f369, 1869);
        }
        return freeSpace;
    }

    private static int FindFreeSpace(Dictionary<int, int> freeSpace, byte[] sv)
    {
        foreach (int addr in freeSpace.Keys)
        {
            if (freeSpace[addr] >= sv.Length)
            {
                int oldSize = freeSpace[addr];
                freeSpace.Remove(addr);
                freeSpace.Add(addr + sv.Length, oldSize - sv.Length);
                return addr;
            }
        }
        return -1;
    }

    private static bool ValidatePalaces(RandomizerProperties props, bool raftIsRequired, List<Palace> palaces)
    {
        return CanGetGlove(props, palaces[1])
            && CanGetRaft(props, raftIsRequired, palaces[1], palaces[2])
            && AtLeastOnePalaceCanHaveGlove(props, palaces);
    }
    private static bool AtLeastOnePalaceCanHaveGlove(RandomizerProperties props, List<Palace> palaces)
    {
        List<RequirementType> requireables = new List<RequirementType>();
        requireables.Add(RequirementType.KEY);
        requireables.Add(RequirementType.UPSTAB);
        requireables.Add(RequirementType.DOWNSTAB);
        requireables.Add(RequirementType.JUMP);
        requireables.Add(RequirementType.FAIRY);
        for(int i = 0; i < 6; i++)
        {
            //If there is at least one palace that would be clearable with everything but the glove
            //that palace could contain the glove, so we're not deadlocked.
            if (palaces[i].IsTraversable(requireables, Item.GLOVE))
            {
                return true;
            }
        }
        return false;
    }

    private static bool CanGetGlove(RandomizerProperties props, Palace palace2)
    {
        if (!props.ShufflePalaceItems)
        {
            List<RequirementType> requireables = new List<RequirementType>();
            //If shuffle overworld items is on, we assume you can get all the items / spells
            //as all progression items will eventually shuffle into spots that work
            if (props.ShuffleOverworldItems)
            {
                requireables.Add(RequirementType.KEY);
            }
            //Otherwise if it's vanilla items we can't get the magic key, because we could need glove for boots for flute to get to new kasuto
            requireables.Add(props.SwapUpAndDownStab ? RequirementType.UPSTAB : RequirementType.DOWNSTAB);
            requireables.Add(RequirementType.JUMP);
            requireables.Add(RequirementType.FAIRY);

            //If we can't clear 2 without the items available, we can never get the glove, so the palace is unbeatable
            if (!palace2.IsTraversable(requireables, Item.GLOVE))
            {
                return false;
            }
        }
        return true;
    }

    private static bool CanGetRaft(RandomizerProperties props, bool raftIsRequired, Palace palace2, Palace palace3)
    {
        //if the flagset has vanilla connections and vanilla palace items, you have to be able to get the raft
        //or it will send the logic into an uinrecoverable nosedive since the palaces can't re-generate
        if (!props.ShufflePalaceItems && raftIsRequired)
        {
            List<RequirementType> requireables = new List<RequirementType>();
            //If shuffle overworld items is on, we assume you can get all the items / spells
            //as all progression items will eventually shuffle into spots that work
            if (props.ShuffleOverworldItems)
            {
                requireables.Add(RequirementType.KEY);
                requireables.Add(RequirementType.GLOVE);
                requireables.Add(props.SwapUpAndDownStab ? RequirementType.UPSTAB : RequirementType.DOWNSTAB);
                requireables.Add(RequirementType.JUMP);
                requireables.Add(RequirementType.FAIRY);
            }
            //Otherwise we can only get the things you can get on the west normally
            else
            {
                requireables.Add(RequirementType.JUMP);
                requireables.Add(RequirementType.FAIRY);
            }
            //If we can clear P2 with this stuff, we can also get the glove
            if (palace2.IsTraversable(requireables, Item.GLOVE))
            {
                requireables.Add(RequirementType.GLOVE);
            }
            //If we can't clear 3 with all the items available on west/DM, we can never raft out, and so we're stuck forever
            //so start over
            if (!palace3.IsTraversable(requireables, Item.RAFT))
            {
                return false;
            }
        }
        return true;
    }

}
