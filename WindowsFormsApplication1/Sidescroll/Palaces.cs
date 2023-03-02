using NLog;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Z2Randomizer.Sidescroll;

public class Palaces
{
    private static readonly Logger logger = LogManager.GetCurrentClassLogger();

    private const int PALACE_SHUFFLE_ATTEMPT_LIMIT = 100;

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
        List<Palace> palaces = new List<Palace>();
        List<Room> roomPool = new List<Room>();
        Dictionary<byte[], List<Room>> sideviews = new Dictionary<byte[], List<Room>>(new Util.StandardByteArrayEqualityComparer());
        Dictionary<byte[], List<Room>> sideviewsgp = new Dictionary<byte[], List<Room>>(new Util.StandardByteArrayEqualityComparer());
        int enemyBytes = 0;
        int enemyBytesGP = 0;
        int mapNo = 0;
        int mapNoGp = 0;
        if (props.palaceStyle == PalaceStyle.RECONSTRUCTED)
        {
            roomPool.Clear();
            roomPool.AddRange(PalaceRooms.Palace1Vanilla);
            roomPool.AddRange(PalaceRooms.Palace2Vanilla);
            roomPool.AddRange(PalaceRooms.Palace3Vanilla);
            roomPool.AddRange(PalaceRooms.Palace4Vanilla);
            roomPool.AddRange(PalaceRooms.Palace5Vanilla);
            roomPool.AddRange(PalaceRooms.Palace6Vanilla);
            if (props.customRooms)
            {
                roomPool.AddRange(PalaceRooms.RoomJamGTM);
                roomPool.AddRange(PalaceRooms.DMInPalaces);
                roomPool.AddRange(PalaceRooms.WinterSolstice);
                roomPool.AddRange(PalaceRooms.MaxRoomJam);
                roomPool.AddRange(PalaceRooms.DusterRoomJam);
                roomPool.AddRange(PalaceRooms.AaronRoomJam);
                roomPool.AddRange(PalaceRooms.KnightcrawlerRoomJam);
                roomPool.AddRange(PalaceRooms.TriforceOfCourage);
                roomPool.AddRange(PalaceRooms.BenthicKing);
                roomPool.AddRange(PalaceRooms.EasternShadow);
                roomPool.AddRange(PalaceRooms.EunosRooms);
            }
            int[] sizes = new int[7];

            sizes[0] = r.Next(10, 17);
            sizes[1] = r.Next(16, 25);
            sizes[2] = r.Next(11, 18);
            sizes[3] = r.Next(16, 25);
            sizes[4] = r.Next(23, 63 - sizes[0] - sizes[1]);
            sizes[5] = r.Next(22, 63 - sizes[2] - sizes[3]);

            if (props.shortenGP)
            {
                sizes[6] = r.Next(27, 41);
            }
            else
            {
                sizes[6] = r.Next(54, 60);
            }

            for (int i = 1; i < 8; i++)
            {

                Palace p;// = new Palace(i, palaceAddr[i], palaceConnectionLocs[i], this.ROMData);
                int tries = 0;

                do
                {
                    if (worker != null && worker.CancellationPending)
                    {
                        return null;
                    }

                    tries = 0;
                    bool done = false;
                    do
                    {
                        mapNo = i switch
                        {
                            1 => 0,
                            2 => palaces[0].AllRooms.Count,
                            3 => 0,
                            4 => palaces[2].AllRooms.Count,
                            5 => palaces[0].AllRooms.Count + palaces[1].AllRooms.Count,
                            6 => mapNo = palaces[2].AllRooms.Count + palaces[3].AllRooms.Count,
                            _ => 0
                        };

                        if (i == 7)
                        {
                            mapNoGp = 0;
                        }

                        p = new Palace(i, palaceAddr[i], palaceConnectionLocs[i]);
                        p.Root = PalaceRooms.Entrances[i - 1].DeepCopy();

                        p.BossRoom = SelectBossRoom(i, r);

                        p.AllRooms.Add(p.Root);

                        p.AllRooms.Add(p.BossRoom);
                        if (i < 7) //Not GP
                        {
                            p.ItemRoom = SelectItemRoom(r);
                            if ((i == 1 || i == 2 || i == 5) && p.ItemRoom.HasBoss)
                            {
                                p.ItemRoom.Enemies[1] = 0x6C;
                            }

                            p.AllRooms.Add(p.ItemRoom);

                            p.Root.Newmap = mapNo;
                            IncrementMapNo(ref mapNo, ref mapNoGp, i);
                            p.BossRoom.Newmap = mapNo;
                            if (props.bossRoomConnect)
                            {
                                p.BossRoom.RightByte = 0x69;
                            }
                            IncrementMapNo(ref mapNo, ref mapNoGp, i);
                            p.ItemRoom.Newmap = mapNo;
                            p.ItemRoom.SetItem((Item)i);
                            IncrementMapNo(ref mapNo, ref mapNoGp, i);
                            if (p.ItemRoom.Map == 69)
                            {
                                Room extra = PalaceRooms.MaxBonusItemRoom.DeepCopy();
                                extra.Newmap = p.ItemRoom.Newmap;
                                extra.SetItem((Item)i);
                                p.AllRooms.Add(extra);
                                p.SortRoom(extra);
                                p.SetOpenRoom(extra);
                            }
                            p.SortRoom(p.Root);
                            p.SortRoom(p.BossRoom);
                            p.SortRoom(p.ItemRoom);
                            p.SetOpenRoom(p.Root);
                        }
                        else //GP
                        {
                            p.Root.Newmap = mapNoGp;
                            IncrementMapNo(ref mapNo, ref mapNoGp, i);
                            p.BossRoom.Newmap = mapNoGp;
                            IncrementMapNo(ref mapNo, ref mapNoGp, i);
                            p.SortRoom(p.Root);
                            p.SortRoom(p.BossRoom);
                            //thunderbird?
                            if (!props.removeTbird)
                            {
                                p.Tbird = PalaceRooms.TBirdRooms[r.Next(PalaceRooms.TBirdRooms.Count)].DeepCopy();
                                p.Tbird.Newmap = mapNoGp;
                                IncrementMapNo(ref mapNo, ref mapNoGp, i);
                                p.SortRoom(p.Tbird);
                                p.AllRooms.Add(p.Tbird);
                            }
                            p.SetOpenRoom(p.Root);

                        }


                        p.MaxRooms = sizes[i - 1];
                        //add rooms
                        if (i == 7)
                        {
                            roomPool.Clear();
                            roomPool.AddRange(PalaceRooms.Palace7Vanilla);
                            if (props.customRooms)
                            {
                                roomPool.AddRange(PalaceRooms.Link7777RoomJam);
                                roomPool.AddRange(PalaceRooms.GTMNewGpRooms);
                                roomPool.AddRange(PalaceRooms.GTMOldGPRooms);
                                roomPool.AddRange(PalaceRooms.WinterSolsticeGP);
                                roomPool.AddRange(PalaceRooms.EonRoomJam);
                                roomPool.AddRange(PalaceRooms.TriforceOfCourageGP);
                                roomPool.AddRange(PalaceRooms.EunosGpRooms);
                                roomPool.AddRange(PalaceRooms.FlippedGP);

                            }
                        }
                        bool dropped = false;
                        while (p.AllRooms.Count < p.MaxRooms)
                        {
                            Room addThis = roomPool[r.Next(roomPool.Count)].DeepCopy();
                            if (i < 7)
                            {
                                addThis.Newmap = mapNo;
                            }
                            else
                            {
                                addThis.Newmap = mapNoGp;
                            }
                            bool added = p.AddRoom(addThis, props.blockersAnywhere);
                            if (added)
                            {
                                IncrementMapNo(ref mapNo, ref mapNoGp, i);
                                if (addThis.HasDrop && !dropped)
                                {
                                    int numDrops = r.Next(Math.Min(3, p.MaxRooms - p.AllRooms.Count), Math.Min(6, p.MaxRooms - p.AllRooms.Count));
                                    bool lastDrop = true;
                                    int j = 0;
                                    while (j < numDrops && lastDrop)
                                    {
                                        Room room = roomPool[r.Next(roomPool.Count)].DeepCopy();
                                        while (!room.IsDropZone)
                                        {
                                            room = roomPool[r.Next(roomPool.Count)].DeepCopy();
                                        }
                                        if (i < 7)
                                        {
                                            room.Newmap = mapNo;
                                        }
                                        else
                                        {
                                            room.Newmap = mapNoGp;
                                        }
                                        bool added2 = p.AddRoom(room, props.blockersAnywhere);
                                        if (added2)
                                        {
                                            IncrementMapNo(ref mapNo, ref mapNoGp, i);
                                            lastDrop = room.HasDrop;
                                            j++;
                                        }
                                    }
                                }
                            }

                            if (p.GetOpenRooms() >= p.MaxRooms - p.AllRooms.Count) //consolidate
                            {
                                p.Consolidate();
                            }

                        }
                        done = true;
                        foreach (Room room in p.AllRooms)
                        {
                            if (room.CountOpenExits() > 0)
                            {
                                done = false;
                            }
                        }

                    } while (!done);

                    p.ShuffleRooms(r);
                    bool reachable = p.AllReachable();
                    while ((!reachable || (i == 7 && (props.requireTbird && !p.RequiresThunderbird())) || p.HasDeadEnd()) && (tries < PALACE_SHUFFLE_ATTEMPT_LIMIT))
                    {
                        p.ResetRooms();
                        p.ShuffleRooms(r);
                        reachable = p.AllReachable();
                        tries++;
                        logger.Debug("Palace room shuffle attempt #" + tries);
                    }
                } while (tries >= PALACE_SHUFFLE_ATTEMPT_LIMIT);
                p.Generations += tries;
                palaces.Add(p);
                foreach (Room room in p.AllRooms)
                {
                    if (i != 7)
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

                    else
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
                }
            }
        }
        else
        {
            for (int i = 1; i < 8; i++)
            {
                int check2 = r.Next(10);

                Palace p = new Palace(i, palaceAddr[i], palaceConnectionLocs[i]);
                //p.dumpMaps();

                p.Root = PalaceRooms.Entrances[i - 1].DeepCopy();
                p.BossRoom = PalaceRooms.BossRooms[i - 1].DeepCopy();
                p.AllRooms.Add(p.Root);
                if (i != 7)
                {
                    p.ItemRoom = PalaceRooms.ItemRooms[i - 1].DeepCopy();
                    p.AllRooms.Add(p.ItemRoom);
                }
                p.AllRooms.Add(p.BossRoom);
                if (i == 7)
                {
                    Room bird = PalaceRooms.Thunderbird.DeepCopy();
                    p.AllRooms.Add(bird);
                    p.Tbird = bird;

                }
                foreach (Room v in PalaceRooms.PalaceRoomsByNumber(i))
                {
                    p.AllRooms.Add(v.DeepCopy());
                }
                bool removeTbird = (i == 7 && props.removeTbird);
                p.CreateTree(removeTbird);

                if (i == 7 && props.shortenGP)
                {
                    p.Shorten(r);
                }
                if (props.palaceStyle == PalaceStyle.SHUFFLED)
                {
                    p.ShuffleRooms(r);
                }
                while (!p.AllReachable() || (i == 7 && (props.requireTbird && !p.RequiresThunderbird())) || p.HasDeadEnd())
                {
                    p.ResetRooms();
                    if (props.palaceStyle == PalaceStyle.SHUFFLED)
                    {
                        p.ShuffleRooms(r);
                    }
                }
                palaces.Add(p);
            }
        }
        int check = r.Next(10);

        for (int i = 0; i < 6; i++)
        {
            palaces[i].UpdateBlocks();
        }

        if (palaces[1].NeedGlove && !props.shufflePalaceItems && (props.palaceStyle == PalaceStyle.SHUFFLED || props.palaceStyle == PalaceStyle.RECONSTRUCTED))
        {
            return new List<Palace>();
        }

        if (!(props.palaceStyle == PalaceStyle.SHUFFLED || props.palaceStyle == PalaceStyle.RECONSTRUCTED))
        {
            palaces[1].NeedGlove = false;
        }
        //update pointers
        if (props.palaceStyle == PalaceStyle.RECONSTRUCTED)
        {
            Dictionary<int, int> freeSpace = SetupFreeSpace(true, 0);
            if (enemyBytes > 0x400 || enemyBytesGP > 681)
            {
                return new List<Palace>();
            }
            int enemyPtr = 0x108b0;
            int enemyPtrgp = 0x148B0;
            foreach (byte[] sv in sideviews.Keys)
            {
                int addr = FindFreeSpace(freeSpace, sv);
                if (addr == -1) //not enough space
                {
                    return new List<Palace>();
                }
                ROMData.Put(addr, sv);
                if (ROMData.GetByte(addr + sv.Length) >= 0xD0)
                {
                    ROMData.Put(addr + sv.Length, 0x00);
                }
                List<Room> rooms = sideviews[sv];
                foreach (Room room in rooms)
                {
                    if (room.Newmap == 45)
                    {
                        logger.Trace("here");
                    }
                    int palSet = 1;
                    if (palaces[2].AllRooms.Contains(room) || palaces[3].AllRooms.Contains(room) || palaces[5].AllRooms.Contains(room))
                    {
                        palSet = 2;
                    }
                    room.WriteSideViewPtr(addr, palSet, ROMData);
                    room.UpdateEnemies(enemyPtr, palSet, ROMData);
                    enemyPtr += room.Enemies.Length;
                    room.UpdateBitmask(palSet, ROMData);
                    bool entrance = false;
                    foreach (Palace p in palaces)
                    {
                        if (p.Root == room || p.BossRoom == room)
                        {
                            entrance = true;
                        }
                    }
                    room.UpdateConnectors(palSet, ROMData, entrance);
                }

            }
            freeSpace = SetupFreeSpace(false, enemyBytesGP);
            foreach (byte[] sv in sideviewsgp.Keys)
            {
                int addr = FindFreeSpace(freeSpace, sv);
                if (addr == -1) //not enough space
                {
                    return new List<Palace>();
                }
                ROMData.Put(addr, sv);
                if (ROMData.GetByte(addr + sv.Length) >= 0xD0)
                {
                    ROMData.Put(addr + sv.Length, 0x00);
                }
                List<Room> rooms = sideviewsgp[sv];
                foreach (Room room in rooms)
                {


                    room.WriteSideViewPtr(addr, 3, ROMData);
                    room.UpdateEnemies(enemyPtrgp, 3, ROMData);
                    enemyPtrgp += room.Enemies.Length;
                    room.UpdateBitmask(3, ROMData);
                    room.UpdateConnectors(3, ROMData, room == palaces[6].Root);
                }

            }

        }
        else
        {
            foreach (Palace palace in palaces)
            {
                palace.UpdateRom(ROMData);
            }
        }

        if (props.shuffleSmallItems || props.extraKeys)
        {
            palaces[0].ShuffleSmallItems(4, true, r, props.shuffleSmallItems, props.extraKeys, props.palaceStyle == PalaceStyle.RECONSTRUCTED, ROMData);
            palaces[1].ShuffleSmallItems(4, true, r, props.shuffleSmallItems, props.extraKeys, props.palaceStyle == PalaceStyle.RECONSTRUCTED, ROMData);
            palaces[2].ShuffleSmallItems(4, false, r, props.shuffleSmallItems, props.extraKeys, props.palaceStyle == PalaceStyle.RECONSTRUCTED, ROMData);
            palaces[3].ShuffleSmallItems(4, false, r, props.shuffleSmallItems, props.extraKeys, props.palaceStyle == PalaceStyle.RECONSTRUCTED, ROMData);
            palaces[4].ShuffleSmallItems(4, true, r, props.shuffleSmallItems, props.extraKeys, props.palaceStyle == PalaceStyle.RECONSTRUCTED, ROMData);
            palaces[5].ShuffleSmallItems(4, false, r, props.shuffleSmallItems, props.extraKeys, props.palaceStyle == PalaceStyle.RECONSTRUCTED, ROMData);
            palaces[6].ShuffleSmallItems(5, true, r, props.shuffleSmallItems, props.extraKeys, props.palaceStyle == PalaceStyle.RECONSTRUCTED, ROMData);
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
    private static Room SelectBossRoom(int pal, Random r)
    {
        if (pal == 7)
        {
            return PalaceRooms.DarkLinkRooms[r.Next(PalaceRooms.DarkLinkRooms.Count)].DeepCopy();
        }
        if (pal == 6)
        {
            return PalaceRooms.NewP6BossRooms[r.Next(PalaceRooms.NewP6BossRooms.Count)].DeepCopy();
        }
        Room room = PalaceRooms.NewBossRooms[r.Next(PalaceRooms.NewBossRooms.Count)].DeepCopy();
        room.Enemies = PalaceRooms.BossRooms[pal - 1].Enemies;
        return room;
    }
    public static Room SelectItemRoom(Random r)
    {
        return r.Next(5) switch
        {
            //left
            0 => PalaceRooms.LeftOpenItemRooms[r.Next(PalaceRooms.LeftOpenItemRooms.Count)].DeepCopy(),
            //right
            1 => PalaceRooms.RightOpenItemRooms[r.Next(PalaceRooms.RightOpenItemRooms.Count)].DeepCopy(),
            //up
            2 => PalaceRooms.UpOpenItemRooms[r.Next(PalaceRooms.UpOpenItemRooms.Count)].DeepCopy(),
            //down
            3 => PalaceRooms.DownOpenItemRooms[r.Next(PalaceRooms.DownOpenItemRooms.Count)].DeepCopy(),
            //Through
            4 => PalaceRooms.ThroughItemRooms[r.Next(PalaceRooms.ThroughItemRooms.Count)].DeepCopy(),
            _ => throw new Exception("Invalid item room direction selection."),
        };
    }

    private static Dictionary<int, int> SetupFreeSpace(bool bank4, int enemyData)
    {
        Dictionary<int, int> freeSpace = new Dictionary<int, int>();
        if (bank4)
        {
            freeSpace.Add(0x103EC, 148);
            freeSpace.Add(0x10649, 226);
            freeSpace.Add(0x10827, 89);
            freeSpace.Add(0x10cb0, 1888);
            freeSpace.Add(0x11ef0, 288);
            freeSpace.Add(0x12124, 79);
            freeSpace.Add(0x1218b, 125);
            freeSpace.Add(0x12304, 1548);
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
            if (freeSpace[addr] > sv.Length)
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
