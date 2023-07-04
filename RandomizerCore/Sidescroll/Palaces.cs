using NLog;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;

namespace Z2Randomizer.Core.Sidescroll;

public class Palaces
{
    private static readonly Logger logger = LogManager.GetCurrentClassLogger();

    private const int PALACE_SHUFFLE_ATTEMPT_LIMIT = 100;
    private const int DROP_PLACEMENT_FAILURE_LIMIT = 100;
    private const int ROOM_PLACEMENT_FAILURE_LIMIT = 100;

    private static readonly RequirementType[] VANILLA_P1_ALLOWED_BLOCKERS = new RequirementType[] { RequirementType.KEY };
    private static readonly RequirementType[] VANILLA_P2_ALLOWED_BLOCKERS = new RequirementType[] { RequirementType.KEY, RequirementType.JUMP, RequirementType.GLOVE };
    private static readonly RequirementType[] VANILLA_P3_ALLOWED_BLOCKERS = new RequirementType[] { RequirementType.KEY, RequirementType.DOWNSTAB, RequirementType.UPSTAB, RequirementType.GLOVE};
    private static readonly RequirementType[] VANILLA_P4_ALLOWED_BLOCKERS = new RequirementType[] { RequirementType.KEY, RequirementType.FAIRY, RequirementType.JUMP};
    private static readonly RequirementType[] VANILLA_P5_ALLOWED_BLOCKERS = new RequirementType[] { RequirementType.KEY, RequirementType.FAIRY, RequirementType.JUMP, RequirementType.GLOVE};
    private static readonly RequirementType[] VANILLA_P6_ALLOWED_BLOCKERS = new RequirementType[] { RequirementType.KEY, RequirementType.FAIRY, RequirementType.JUMP, RequirementType.GLOVE};
    private static readonly RequirementType[] VANILLA_P7_ALLOWED_BLOCKERS = new RequirementType[] { RequirementType.FAIRY, RequirementType.UPSTAB, RequirementType.DOWNSTAB, RequirementType.JUMP, RequirementType.GLOVE};

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

    public static List<Palace> CreatePalaces(BackgroundWorker worker, Random r, RandomizerProperties props, ROM ROMData)
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
        if (props.PalaceStyle == PalaceStyle.RECONSTRUCTED)
        {
            roomPool.Clear();
            roomPool.AddRange(PalaceRooms.Palace1Vanilla(props.UseCustomRooms));
            roomPool.AddRange(PalaceRooms.Palace2Vanilla(props.UseCustomRooms));
            roomPool.AddRange(PalaceRooms.Palace3Vanilla(props.UseCustomRooms));
            roomPool.AddRange(PalaceRooms.Palace4Vanilla(props.UseCustomRooms));
            roomPool.AddRange(PalaceRooms.Palace5Vanilla(props.UseCustomRooms));
            roomPool.AddRange(PalaceRooms.Palace6Vanilla(props.UseCustomRooms));
            if (props.UseCommunityRooms)
            {
                roomPool.AddRange(PalaceRooms.RoomJamGTM(props.UseCustomRooms));
                roomPool.AddRange(PalaceRooms.DMInPalaces(props.UseCustomRooms));
                roomPool.AddRange(PalaceRooms.WinterSolstice(props.UseCustomRooms));
                roomPool.AddRange(PalaceRooms.MaxRoomJam(props.UseCustomRooms));
                roomPool.AddRange(PalaceRooms.DusterRoomJam(props.UseCustomRooms));
                roomPool.AddRange(PalaceRooms.AaronRoomJam(props.UseCustomRooms));
                roomPool.AddRange(PalaceRooms.KnightcrawlerRoomJam(props.UseCustomRooms));
                roomPool.AddRange(PalaceRooms.TriforceOfCourage(props.UseCustomRooms));
                roomPool.AddRange(PalaceRooms.BenthicKing(props.UseCustomRooms));
                roomPool.AddRange(PalaceRooms.EasternShadow(props.UseCustomRooms));
                roomPool.AddRange(PalaceRooms.EunosRooms(props.UseCustomRooms));
            }

            gpRoomPool.AddRange(PalaceRooms.Palace7Vanilla(props.UseCustomRooms));
            if (props.UseCommunityRooms)
            {
                gpRoomPool.AddRange(PalaceRooms.Link7777RoomJam(props.UseCustomRooms));
                gpRoomPool.AddRange(PalaceRooms.GTMNewGpRooms(props.UseCustomRooms));
                gpRoomPool.AddRange(PalaceRooms.GTMOldGPRooms(props.UseCustomRooms));
                gpRoomPool.AddRange(PalaceRooms.WinterSolsticeGP(props.UseCustomRooms));
                gpRoomPool.AddRange(PalaceRooms.EonRoomJam(props.UseCustomRooms));
                gpRoomPool.AddRange(PalaceRooms.TriforceOfCourageGP(props.UseCustomRooms));
                gpRoomPool.AddRange(PalaceRooms.EunosGpRooms(props.UseCustomRooms));
                gpRoomPool.AddRange(PalaceRooms.FlippedGP(props.UseCustomRooms));

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

                        palace = new Palace(currentPalace, palaceAddr[currentPalace], palaceConnectionLocs[currentPalace], props.UseCustomRooms);
                        palace.Root = PalaceRooms.Entrances(props.UseCustomRooms)[currentPalace - 1].DeepCopy();
                        palace.Root.IsRoot = true;
                        palace.Root.PalaceGroup = palaceGroup;
                        palace.AllRooms.Add(palace.Root);

                        palace.BossRoom = SelectBossRoom(currentPalace, r, props.UseCustomRooms, props.UseCommunityRooms);
                        palace.BossRoom.PalaceGroup = palaceGroup;
                        palace.AllRooms.Add(palace.BossRoom);

                        if (currentPalace < 7) //Not GP
                        {
                            palace.ItemRoom = GenerateItemRoom(r, props.UseCustomRooms, props.UseCommunityRooms);
                            palace.ItemRoom.PalaceGroup = palaceGroup;
                            if (palaceGroup == 1 && palace.ItemRoom.HasBoss)
                            {
                                palace.ItemRoom.Enemies[1] = 0x6C;
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
                            //WTF is this magic number
                            if (palace.ItemRoom.Map == 69)
                            {
                                Room extra = PalaceRooms.MaxBonusItemRoom(props.UseCustomRooms).DeepCopy();
                                extra.NewMap = palace.ItemRoom.NewMap;
                                extra.PalaceGroup = palaceGroup;
                                extra.SetItem((Item)currentPalace);
                                palace.AllRooms.Add(extra);
                                palace.SortRoom(extra);
                                palace.SetOpenRoom(extra);
                            }
                            palace.SortRoom(palace.Root);
                            palace.SortRoom(palace.BossRoom);
                            palace.SortRoom(palace.ItemRoom);
                            palace.SetOpenRoom(palace.Root);
                        }
                        else //GP
                        {
                            palace.Root.NewMap = mapNoGp;
                            IncrementMapNo(ref mapNo, ref mapNoGp, currentPalace);
                            palace.BossRoom.NewMap = mapNoGp;
                            IncrementMapNo(ref mapNo, ref mapNoGp, currentPalace);
                            palace.SortRoom(palace.Root);
                            palace.SortRoom(palace.BossRoom);
                            //thunderbird?
                            if (!props.RemoveTbird)
                            {
                                if(props.UseCommunityRooms)
                                {
                                    palace.Tbird = PalaceRooms.TBirdRooms(props.UseCustomRooms)[r.Next(PalaceRooms.TBirdRooms(props.UseCustomRooms).Count)].DeepCopy();
                                }
                                else
                                {
                                    palace.Tbird = PalaceRooms.Thunderbird(props.UseCustomRooms);
                                }
                                palace.Tbird.NewMap = mapNoGp;
                                palace.Tbird.PalaceGroup = 3;
                                IncrementMapNo(ref mapNo, ref mapNoGp, currentPalace);
                                palace.SortRoom(palace.Tbird);
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
                            if(palaceRoomPool.Count == 0)
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
                        /*
                        if (currentPalace == 7)
                        {
                            Debug.WriteLine("---" + palace.AllRooms.Where(i => i.CountOpenExits() > 0).Count() + "---");
                            palace.AllRooms.Where(i => i.CountOpenExits() > 0).ToList()
                                .ForEach(i => Debug.WriteLine(Util.ByteArrayToHexString(i.SideView) + i.PrintUnsatisfiedExits()));
                        }
                        */
                    } while (roomPlacementFailures < ROOM_PLACEMENT_FAILURE_LIMIT
                        && palace.AllRooms.Any(i => i.CountOpenExits() > 0)
                        //|| (/*currentPalace == 1 && */!palace.AllRooms.Any(i => i.HasDrop))
                      );

                    if(roomPlacementFailures != ROOM_PLACEMENT_FAILURE_LIMIT)
                    {
                        palace.ShuffleRooms(r);
                        bool reachable = palace.AllReachable();
                        while ((!reachable || (currentPalace == 7 && (props.RequireTbird && !palace.RequiresThunderbird())) || palace.HasDeadEnd()) && (tries < PALACE_SHUFFLE_ATTEMPT_LIMIT))
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
        }
        //Not reconstructed
        else
        {
            for (int currentPalace = 1; currentPalace < 8; currentPalace++)
            {
                Palace palace = new Palace(currentPalace, palaceAddr[currentPalace], palaceConnectionLocs[currentPalace], props.UseCustomRooms);
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

                palace.Root = PalaceRooms.Entrances(props.UseCustomRooms)[currentPalace - 1].DeepCopy();
                palace.Root.PalaceGroup = palaceGroup;
                palace.BossRoom = PalaceRooms.BossRooms(props.UseCustomRooms)[currentPalace - 1].DeepCopy();
                palace.BossRoom.PalaceGroup = palaceGroup;
                palace.AllRooms.Add(palace.Root);
                if (currentPalace != 7)
                {
                    palace.ItemRoom = PalaceRooms.ItemRooms(props.UseCustomRooms)[currentPalace - 1].DeepCopy();
                    palace.ItemRoom.PalaceGroup = palaceGroup;
                    palace.AllRooms.Add(palace.ItemRoom);
                }
                palace.AllRooms.Add(palace.BossRoom);
                if (currentPalace == 7)
                {
                    Room bird = PalaceRooms.Thunderbird(props.UseCustomRooms).DeepCopy();
                    bird.PalaceGroup = palaceGroup;
                    palace.AllRooms.Add(bird);
                    palace.Tbird = bird;

                }
                foreach (Room v in PalaceRooms.PalaceRoomsByNumber(currentPalace, props.UseCustomRooms))
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
    private static Room SelectBossRoom(int palaceNumber, Random r, bool useCustomRooms, bool useCommunityRooms)
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
                1 => PalaceRooms.PalaceRoomsByNumber(palaceNumber, useCustomRooms).Where(i => i.Map == 13).First().DeepCopy(),
                2 => PalaceRooms.PalaceRoomsByNumber(palaceNumber, useCustomRooms).Where(i => i.Map == 34).First().DeepCopy(),
                3 => PalaceRooms.PalaceRoomsByNumber(palaceNumber, useCustomRooms).Where(i => i.Map == 14).First().DeepCopy(),
                4 => PalaceRooms.PalaceRoomsByNumber(palaceNumber, useCustomRooms).Where(i => i.Map == 28).First().DeepCopy(),
                5 => PalaceRooms.PalaceRoomsByNumber(palaceNumber, useCustomRooms).Where(i => i.Map == 41).First().DeepCopy(),
                6 => PalaceRooms.PalaceRoomsByNumber(palaceNumber, useCustomRooms).Where(i => i.Map == 58).First().DeepCopy(),
                7 => PalaceRooms.PalaceRoomsByNumber(palaceNumber, useCustomRooms).Where(i => i.Map == 54).First().DeepCopy(),
                _ => throw new ImpossibleException("Unable to find vanilla boss room")
            };
        }
    }
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

}
