using NLog;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Speech.Synthesis;
using System.Threading.Tasks;
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

    public static async Task<List<Palace>> CreatePalaces(Random r, RandomizerProperties props, ROM ROMData, bool raftIsRequired)
    {
        List<Palace> palaces = new List<Palace>();
        List<Room> roomPool = new List<Room>();
        List<Room> gpRoomPool = new List<Room>();
        Dictionary<byte[], List<Room>> sideviews = new Dictionary<byte[], List<Room>>(new Util.StandardByteArrayEqualityComparer());
        Dictionary<byte[], List<Room>> sideviewsgp = new Dictionary<byte[], List<Room>>(new Util.StandardByteArrayEqualityComparer());
        int enemyBytes = 0;
        int enemyBytesGP = 0;
        int mapNo = 0;
        int mapNoGp = 0;
        if (props.PalaceStyle == PalaceStyle.RECONSTRUCTED)
        {
            roomPool.Clear();
            roomPool.AddRange(props.Rooms.Palace1Vanilla(props.UseCustomRooms));
            roomPool.AddRange(props.Rooms.Palace2Vanilla(props.UseCustomRooms));
            roomPool.AddRange(props.Rooms.Palace3Vanilla(props.UseCustomRooms));
            roomPool.AddRange(props.Rooms.Palace4Vanilla(props.UseCustomRooms));
            roomPool.AddRange(props.Rooms.Palace5Vanilla(props.UseCustomRooms));
            roomPool.AddRange(props.Rooms.Palace6Vanilla(props.UseCustomRooms));
            if (props.UseCommunityRooms)
            {
                roomPool.AddRange(props.Rooms.RoomJamGTM(props.UseCustomRooms));
                roomPool.AddRange(props.Rooms.DMInPalaces(props.UseCustomRooms));
                roomPool.AddRange(props.Rooms.WinterSolstice(props.UseCustomRooms));
                roomPool.AddRange(props.Rooms.MaxRoomJam(props.UseCustomRooms));
                roomPool.AddRange(props.Rooms.DusterRoomJam(props.UseCustomRooms));
                roomPool.AddRange(props.Rooms.AaronRoomJam(props.UseCustomRooms));
                roomPool.AddRange(props.Rooms.KnightcrawlerRoomJam(props.UseCustomRooms));
                roomPool.AddRange(props.Rooms.TriforceOfCourage(props.UseCustomRooms));
                roomPool.AddRange(props.Rooms.BenthicKing(props.UseCustomRooms));
                roomPool.AddRange(props.Rooms.EasternShadow(props.UseCustomRooms));
                roomPool.AddRange(props.Rooms.EunosRooms(props.UseCustomRooms));
            }

            gpRoomPool.AddRange(props.Rooms.Palace7Vanilla(props.UseCustomRooms));
            if (props.UseCommunityRooms)
            {
                gpRoomPool.AddRange(props.Rooms.Link7777RoomJam(props.UseCustomRooms));
                gpRoomPool.AddRange(props.Rooms.GTMNewGpRooms(props.UseCustomRooms));
                gpRoomPool.AddRange(props.Rooms.GTMOldGPRooms(props.UseCustomRooms));
                gpRoomPool.AddRange(props.Rooms.WinterSolsticeGP(props.UseCustomRooms));
                gpRoomPool.AddRange(props.Rooms.EonRoomJam(props.UseCustomRooms));
                gpRoomPool.AddRange(props.Rooms.TriforceOfCourageGP(props.UseCustomRooms));
                gpRoomPool.AddRange(props.Rooms.EunosGpRooms(props.UseCustomRooms));
                gpRoomPool.AddRange(props.Rooms.FlippedGP(props.UseCustomRooms));

            }
            int[] sizes = new int[7];

            sizes[0] = r.Next(10, 17);
            sizes[1] = r.Next(16, 25);
            sizes[2] = r.Next(11, 18);
            sizes[3] = r.Next(16, 25);
            sizes[4] = r.Next(23, 63 - sizes[0] - sizes[1]);
            sizes[5] = r.Next(22, 63 - sizes[2] - sizes[3]);

            if (props.ShortenGP)
            {
                sizes[6] = r.Next(27, 41);
            }
            else
            {
                sizes[6] = r.Next(54, 60);
            }

            for (int currentPalace = 1; currentPalace < 8; currentPalace++)
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
                Room segmentedItemRoom1, segmentedItemRoom2;

                do // while (tries >= PALACE_SHUFFLE_ATTEMPT_LIMIT);
                {
                    segmentedItemRoom1 = null;
                    segmentedItemRoom2 = null;
                    //if (worker != null && worker.CancellationPending)
                    //{
                    //    return null;
                    //}
                    tries = 0;
                    innertries = 0;
                    int roomPlacementFailures = 0;
                    //bool done = false;
                    do //while (roomPlacementFailures > ROOM_PLACEMENT_FAILURE_LIMIT || palace.AllRooms.Any(i => i.CountOpenExits() > 0));

                    {
                        palace = await Task.Run(() =>
                        {
                            List<Room> palaceRoomPool = new List<Room>(currentPalace == 7 ? gpRoomPool : roomPool);
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

                            var palace = new Palace(props.Rooms, currentPalace, palaceAddr[currentPalace], palaceConnectionLocs[currentPalace], props.UseCustomRooms);
                            palace.Root = props.Rooms.Entrances(props.UseCustomRooms)[currentPalace - 1].DeepCopy();
                            palace.Root.IsRoot = true;
                            palace.Root.PalaceGroup = palaceGroup;
                            palace.AllRooms.Add(palace.Root);

                            palace.BossRoom = SelectBossRoom(props.Rooms, currentPalace, r, props.UseCustomRooms, props.UseCommunityRooms);
                            palace.BossRoom.PalaceGroup = palaceGroup;
                            palace.AllRooms.Add(palace.BossRoom);

                            if (currentPalace < 7) //Not GP
                            {
                                palace.ItemRoom = GenerateItemRoom(props.Rooms, r, props.UseCustomRooms, props.UseCommunityRooms);
                                palace.ItemRoom.PalaceGroup = palaceGroup;
                                //#76: Not sure if this is still needed. If the item room is a boss item room, and it's in palace group 1,
                                //move the boss up 1 tile. I fixed the underlying broken room, but for now, let's keep this.
                                if (palaceGroup == 1 && palace.ItemRoom.HasBoss)
                                {
                                    palace.ItemRoom.NewEnemies[1] = 0x6C;
                                }
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
                                //This magic number is an awful way to indicate this is the segmented item room. Update this to just use a boolean flag.
                                if (palace.ItemRoom.Map == 69)
                                {
                                    segmentedItemRoom1 = palace.ItemRoom;
                                    segmentedItemRoom2 = props.Rooms.MaxBonusItemRoom(props.UseCustomRooms).DeepCopy();
                                    segmentedItemRoom2.NewMap = palace.ItemRoom.NewMap;
                                    segmentedItemRoom2.PalaceGroup = palaceGroup;
                                    segmentedItemRoom2.SetItem((Item)currentPalace);
                                    palace.AllRooms.Add(segmentedItemRoom2);
                                    //palace.SortRoom(segmentedItemRoom2);
                                    palace.SetOpenRoom(segmentedItemRoom2);
                                }
                                //palace.SortRoom(palace.Root);
                                //palace.SortRoom(palace.BossRoom);
                                //palace.SortRoom(palace.ItemRoom);
                                palace.SetOpenRoom(palace.Root);
                            }
                            else //GP
                            {
                                palace.Root.NewMap = mapNoGp;
                                IncrementMapNo(ref mapNo, ref mapNoGp, currentPalace);
                                palace.BossRoom.NewMap = mapNoGp;
                                IncrementMapNo(ref mapNo, ref mapNoGp, currentPalace);
                                //palace.SortRoom(palace.Root);
                                //palace.SortRoom(palace.BossRoom);
                                //thunderbird?
                                if (!props.RemoveTbird)
                                {
                                    if (props.UseCommunityRooms)
                                    {
                                        palace.Tbird = props.Rooms.TBirdRooms(props.UseCustomRooms)[r.Next(props.Rooms.TBirdRooms(props.UseCustomRooms).Count)].DeepCopy();
                                    }
                                    else
                                    {
                                        palace.Tbird = props.Rooms.Thunderbird(props.UseCustomRooms).DeepCopy();
                                    }
                                    palace.Tbird.NewMap = mapNoGp;
                                    palace.Tbird.PalaceGroup = 3;
                                    IncrementMapNo(ref mapNo, ref mapNoGp, currentPalace);
                                    //palace.SortRoom(palace.Tbird);
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
                                    return null;
                                }
                                int roomIndex = r.Next(palaceRoomPool.Count);
                                Room roomToAdd = palaceRoomPool[roomIndex].DeepCopy();

                                roomToAdd.PalaceGroup = palaceGroup;
                                if (currentPalace < 7)
                                {
                                    roomToAdd.NewMap = mapNo;
                                }
                                else
                                {
                                    roomToAdd.NewMap = mapNoGp;
                                }
                                bool added;
                                if (roomToAdd.HasDrop && palaceRoomPool.Count(i => i.IsDropZone && i != roomToAdd) == 0)
                                {
                                    //Debug.WriteLine(palace.AllRooms.Count + " - 0");
                                    added = false;
                                }
                                else
                                {
                                    added = palace.AddRoom(roomToAdd, props.BlockersAnywhere);
                                    /*
                                    if (currentPalace == 7 && roomToAdd.HasDrop)
                                    {
                                        Debug.WriteLine(palace.AllRooms.Count + " - " + palaceRoomPool.Count(i => i.IsDropZone && i != roomToAdd));
                                    }
                                    */
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
                                            Room dropZoneRoom = possibleDropZones[r.Next(0, possibleDropZones.Count)].DeepCopy();

                                            if (currentPalace < 7)
                                            {
                                                dropZoneRoom.NewMap = mapNo;
                                            }
                                            else
                                            {
                                                dropZoneRoom.NewMap = mapNoGp;
                                            }
                                            bool added2 = palace.AddRoom(dropZoneRoom, props.BlockersAnywhere);
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
                            return palace;
                        });
                        // Palace is set to null when the randomizer runs out of palace rooms to place
                        if (palace == null)
                        {
                            return null;
                        }
                        innertries++;
                    } while (roomPlacementFailures < ROOM_PLACEMENT_FAILURE_LIMIT
                        && palace.AllRooms.Any(i => i.CountOpenExits() > 0)
                      );

                    if(roomPlacementFailures != ROOM_PLACEMENT_FAILURE_LIMIT)
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

                //Unify the parts of the segmented item room back together.
                if (segmentedItemRoom1 != null)
                {
                    segmentedItemRoom2.Down = segmentedItemRoom1.Down;
                    segmentedItemRoom2.DownByte = segmentedItemRoom1.DownByte;
                    palace.AllRooms.Where(i => i.Up == segmentedItemRoom1).ToList().ForEach(i => i.Up = segmentedItemRoom2);
                    palace.AllRooms.Remove(segmentedItemRoom1);
                }

            }
        }
        //Not reconstructed
        else
        {
            for (int currentPalace = 1; currentPalace < 8; currentPalace++)
            {
                Palace palace = new Palace(props.Rooms, currentPalace, palaceAddr[currentPalace], palaceConnectionLocs[currentPalace], props.UseCustomRooms);
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

                palace.Root = props.Rooms.Entrances(props.UseCustomRooms)[currentPalace - 1].DeepCopy();
                palace.Root.PalaceGroup = palaceGroup;
                palace.BossRoom = props.Rooms.BossRooms(props.UseCustomRooms)[currentPalace - 1].DeepCopy();
                palace.BossRoom.PalaceGroup = palaceGroup;
                palace.AllRooms.Add(palace.Root);
                if (currentPalace != 7)
                {
                    palace.ItemRoom = props.Rooms.ItemRooms(props.UseCustomRooms)[currentPalace - 1].DeepCopy();
                    palace.ItemRoom.PalaceGroup = palaceGroup;
                    palace.AllRooms.Add(palace.ItemRoom);
                }
                palace.AllRooms.Add(palace.BossRoom);
                if (currentPalace == 7)
                {
                    Room bird = props.Rooms.Thunderbird(props.UseCustomRooms).DeepCopy();
                    bird.PalaceGroup = palaceGroup;
                    palace.AllRooms.Add(bird);
                    palace.Tbird = bird;

                }
                foreach (Room v in props.Rooms.PalaceRoomsByNumber(currentPalace, props.UseCustomRooms))
                {
                    Room room = v.DeepCopy();
                    room.PalaceGroup = palaceGroup;
                    palace.AllRooms.Add(room);
                    
                }
                bool removeTbird = (currentPalace == 7 && props.RemoveTbird);
                palace.CreateTree(removeTbird);

                if (currentPalace == 7 && props.ShortenGP)
                {
                    palace.Shorten(r);
                }
                if (props.PalaceStyle == PalaceStyle.SHUFFLED)
                {
                    palace.ShuffleRooms(r);
                }
                while (!palace.AllReachable() || (currentPalace == 7 && (props.RequireTbird && !palace.RequiresThunderbird())) || palace.HasDeadEnd())
                {
                    palace.ResetRooms();
                    if (props.PalaceStyle == PalaceStyle.SHUFFLED)
                    {
                        palace.ShuffleRooms(r);
                    }
                }
                palaces.Add(palace);
            }
        }

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

        //update pointers
        if (props.PalaceStyle == PalaceStyle.RECONSTRUCTED)
        {
            Dictionary<int, int> freeSpace = SetupFreeSpace(true, 0);
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
                    room.UpdateEnemies(enemyAddr, ROMData, props.PalaceStyle);
                    enemyAddr += room.NewEnemies.Length;
                    /*
                    bool entrance = false;
                    foreach (Palace p in palaces)
                    {
                        if (p.Root == room || p.BossRoom == room)
                        {
                            entrance = true;
                        }
                    }
                    */
                    //room.UpdateConnectors(ROMData, entrance);
                    room.UpdateConnectors();
                }
            }
            //GP Reconstructed
            enemyAddr = Enemies.GPEnemyAddr;
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
                    room.UpdateEnemies(enemyAddr, ROMData, props.PalaceStyle);
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
                    room.UpdateEnemies(enemyAddr, ROMData, props.PalaceStyle);
                    enemyAddr += room.Enemies.Length;
                    enemiesLength += room.Enemies.Length;
                }

                foreach (Room room in palaces[2].AllRooms.Union(palaces[3].AllRooms).Union(palaces[5].AllRooms).OrderBy(i => i.NewMap == 0 ? i.Map : i.NewMap))
                {
                    room.UpdateEnemies(enemyAddr, ROMData, props.PalaceStyle);
                    enemyAddr += room.Enemies.Length;
                    enemiesLength += room.Enemies.Length;
                }

                enemyAddr = Enemies.GPEnemyAddr;
                foreach (Room room in palaces[6].AllRooms)
                {
                    room.UpdateEnemies(enemyAddr, ROMData, props.PalaceStyle);
                    enemyAddr += room.Enemies.Length;
                }
            }
        }

        if (props.ShuffleSmallItems || props.ExtraKeys)
        {
            palaces[0].ShuffleSmallItems(4, true, r, props.ShuffleSmallItems, props.ExtraKeys, props.PalaceStyle == PalaceStyle.RECONSTRUCTED, ROMData);
            palaces[1].ShuffleSmallItems(4, true, r, props.ShuffleSmallItems, props.ExtraKeys, props.PalaceStyle == PalaceStyle.RECONSTRUCTED, ROMData);
            palaces[2].ShuffleSmallItems(4, false, r, props.ShuffleSmallItems, props.ExtraKeys, props.PalaceStyle == PalaceStyle.RECONSTRUCTED, ROMData);
            palaces[3].ShuffleSmallItems(4, false, r, props.ShuffleSmallItems, props.ExtraKeys, props.PalaceStyle == PalaceStyle.RECONSTRUCTED, ROMData);
            palaces[4].ShuffleSmallItems(4, true, r, props.ShuffleSmallItems, props.ExtraKeys, props.PalaceStyle == PalaceStyle.RECONSTRUCTED, ROMData);
            palaces[5].ShuffleSmallItems(4, false, r, props.ShuffleSmallItems, props.ExtraKeys, props.PalaceStyle == PalaceStyle.RECONSTRUCTED, ROMData);
            palaces[6].ShuffleSmallItems(5, true, r, props.ShuffleSmallItems, props.ExtraKeys, props.PalaceStyle == PalaceStyle.RECONSTRUCTED, ROMData);
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
    private static Room SelectBossRoom(PalaceRooms palaceRooms, int palaceNumber, Random r, bool useCustomRooms, bool useCommunityRooms)
    {
        if(useCommunityRooms)
        {
            if (palaceNumber == 7)
            {
                return palaceRooms.DarkLinkRooms(useCustomRooms)[r.Next(palaceRooms.DarkLinkRooms(useCustomRooms).Count)].DeepCopy();
            }
            if (palaceNumber == 6)
            {
                return palaceRooms.NewP6BossRooms(useCustomRooms)[r.Next(palaceRooms.NewP6BossRooms(useCustomRooms).Count)].DeepCopy();
            }
            Room room = palaceRooms.NewBossRooms(useCustomRooms)[r.Next(palaceRooms.NewBossRooms(useCustomRooms).Count)].DeepCopy();
            room.Enemies = palaceRooms.BossRooms(useCustomRooms)[palaceNumber - 1].Enemies;
            return room;
        }
        else
        {
            return palaceNumber switch
            {
                1 => palaceRooms.BossRooms(useCustomRooms).Where(i => i.Map == 13).First().DeepCopy(),
                2 => palaceRooms.BossRooms(useCustomRooms).Where(i => i.Map == 34).First().DeepCopy(),
                3 => palaceRooms.BossRooms(useCustomRooms).Where(i => i.Map == 14).First().DeepCopy(),
                4 => palaceRooms.BossRooms(useCustomRooms).Where(i => i.Map == 28).First().DeepCopy(),
                5 => palaceRooms.BossRooms(useCustomRooms).Where(i => i.Map == 41).First().DeepCopy(),
                6 => palaceRooms.BossRooms(useCustomRooms).Where(i => i.Map == 58).First().DeepCopy(),
                7 => palaceRooms.BossRooms(useCustomRooms).Where(i => i.Map == 54).First().DeepCopy(),
                _ => throw new ImpossibleException("Unable to find vanilla boss room")
            };
        }
    }
    public static Room GenerateItemRoom(PalaceRooms palaceRooms, Random r, bool useCustomRooms, bool useCommunityRooms)
    {
        if(!useCommunityRooms)
        {
            return palaceRooms.ItemRooms(useCustomRooms)[r.Next(palaceRooms.ItemRooms(useCustomRooms).Count)].DeepCopy();
        }
        return r.Next(5) switch
        {
            //left
            0 => palaceRooms.LeftOpenItemRooms(useCustomRooms)[r.Next(palaceRooms.LeftOpenItemRooms(useCustomRooms).Count)].DeepCopy(),
            //right
            1 => palaceRooms.RightOpenItemRooms(useCustomRooms)[r.Next(palaceRooms.RightOpenItemRooms(useCustomRooms).Count)].DeepCopy(),
            //up
            2 => palaceRooms.UpOpenItemRooms(useCustomRooms)[r.Next(palaceRooms.UpOpenItemRooms(useCustomRooms).Count)].DeepCopy(),
            //down
            3 => palaceRooms.DownOpenItemRooms(useCustomRooms)[r.Next(palaceRooms.DownOpenItemRooms(useCustomRooms).Count)].DeepCopy(),
            //Through
            4 => palaceRooms.ThroughItemRooms(useCustomRooms)[r.Next(palaceRooms.ThroughItemRooms(useCustomRooms).Count)].DeepCopy(),
            _ => throw new Exception("Invalid item room direction selection."),
        };
    }

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
