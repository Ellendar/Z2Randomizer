﻿using Newtonsoft.Json;
using System.Diagnostics;
using RandomizerCore.Sidescroll;

namespace Z2Randomizer.Tests;

[TestClass]
public class RoomSerializationTests
{
    [TestMethod]
    public void TestSerialization()
    {
        string roomJson = "{\"name\": \"testName\", \"enabled\": true, \"group\": \"palace1vanilla\", \"map\": 4, \"connections\": \"0F000214\", \"enemies\": \"050F0B00CB\", \"sideviewData\": \"3A600E08D208420022C8420022C8420022C8420022C8420022C8420022C84200D40E07F1F050B071D708420022C8420022C8420022C84200D20E\", \"bitmask\": \"0F\", \"isFairyBlocked\": false, \"isGloveBlocked\": false, \"isDownstabBlocked\": false, \"isUpstabBlocked\": false, \"isJumpBlocked\": false, \"hasItem\": false, \"hasBoss\": false, \"hasDrop\": false, \"elevatorScreen\": 2, \"memoryAddress\": \"01073B\", \"IsUpDownReversed\": false, \"isDropZone\": false}";

        Room room = new Room(roomJson);
        String serialized = room.Serialize();
        Debug.WriteLine(serialized);
        room = new Room(serialized);
    }

    [TestMethod]
    public void TestDeserialization()
    {
        //string roomJson = "{\"name\": \"testName\", \"enabled\": true, \"group\": \"palace1vanilla\", \"map\": 4, \"connections\": \"0F000214\", \"enemies\": \"050F0B00CB\", \"sideviewData\": \"3A600E08D208420022C8420022C8420022C8420022C8420022C8420022C84200D40E07F1F050B071D708420022C8420022C8420022C84200D20E\", \"bitmask\": \"0F\", \"isFairyBlocked\": false, \"isGloveBlocked\": false, \"isDownstabBlocked\": false, \"isUpstabBlocked\": false, \"isJumpBlocked\": false, \"hasItem\": false, \"hasBoss\": false, \"hasDrop\": false, \"elevatorScreen\": 2, \"memoryAddress\": \"01073B\", \"IsUpDownReversed\": false, \"isDropZone\": false}";
        string roomJson = """
            {
            "bitmask": "0F",
            "connections": "0F000214",
            "elevatorScreen": 2,
            "enabled": true,
            "enemies": "050F0B00CB",
            "group": "VANILLA", 
            "author": "vanilla",
            "hasBoss": false,
            "hasDrop": false,
            "hasItem": false,
            "isThunderBirdRoom": false,
            "isEntrance": false,
            "palaceNumber": null,
            "linkedRoomName": null,
            "isDropZone": false,
            "isEntrance": false,
            "IsUpDownReversed": false,
            "map": 4,
            "memoryAddress": 67387,
            "name": "testName",
            "requirements":
                [
                ],
            "sideviewData": "3A600E08D208420022C8420022C8420022C8420022C8420022C8420022C84200D40E07F1F050B071D708420022C8420022C8420022C84200D20E"
            }
            """;

        Room room = new Room(roomJson);
        Assert.AreEqual("testName", room.Name);
        Assert.AreEqual(0x1073B, room.ConnectionStartAddress);
    }

    [TestMethod]
    public void TestBulkDeserialization()
    {
        Dictionary<RoomGroup, List<Room>> roomsByGroup = new();
        string roomsJson = File.ReadAllText("PalaceRooms.json");
        dynamic rooms = JsonConvert.DeserializeObject(roomsJson);
        foreach (var obj in rooms)
        {
            Room room = new Room(obj.ToString());
            if (!roomsByGroup.ContainsKey(room.Group))
            {
                roomsByGroup.Add(room.Group, new List<Room>());
            }
            roomsByGroup[room.Group].Add(room);
        }
    }

    public class StandardByteArrayEqualityComparer : IEqualityComparer<byte[]>
    {
        public bool Equals(byte[] x, byte[] y)
        {
            if (x.Length != y.Length)
            {
                return false;
            }
            for (int i = 0; i < x.Length; i++)
            {
                if (x[i] != y[i])
                {
                    return false;
                }
            }
            return true;
        }

        public int GetHashCode(byte[] room)
        {
            int result = 17;
            for (int i = 0; i < room.Length; i++)
            {
                unchecked
                {
                    result = result * 23 + room[i];
                }
            }
            return result;
        }
    }

    [TestMethod] 
    public void RoomsContainsNoDuplcateRooms()
    {
        //TODO: REIMPLEMENT THIS
        /*
        bool useCustomRooms = false;
        List<Room> rooms = new List<Room>();
        rooms.AddRange(PalaceRooms.Palace1Vanilla(useCustomRooms));
        rooms.AddRange(PalaceRooms.Palace2Vanilla(useCustomRooms));
        rooms.AddRange(PalaceRooms.Palace3Vanilla(useCustomRooms));
        rooms.AddRange(PalaceRooms.Palace4Vanilla(useCustomRooms));
        rooms.AddRange(PalaceRooms.Palace5Vanilla(useCustomRooms));
        rooms.AddRange(PalaceRooms.Palace6Vanilla(useCustomRooms));
        rooms.AddRange(PalaceRooms.RoomJamGTM(useCustomRooms));
        rooms.AddRange(PalaceRooms.DMInPalaces(useCustomRooms));
        rooms.AddRange(PalaceRooms.WinterSolstice(useCustomRooms));
        rooms.AddRange(PalaceRooms.MaxRoomJam(useCustomRooms));
        rooms.AddRange(PalaceRooms.DusterRoomJam(useCustomRooms));
        rooms.AddRange(PalaceRooms.AaronRoomJam(useCustomRooms));
        rooms.AddRange(PalaceRooms.KnightcrawlerRoomJam(useCustomRooms));
        rooms.AddRange(PalaceRooms.TriforceOfCourage(useCustomRooms));
        rooms.AddRange(PalaceRooms.BenthicKing(useCustomRooms));
        rooms.AddRange(PalaceRooms.EasternShadow(useCustomRooms));
        rooms.AddRange(PalaceRooms.EunosRooms(useCustomRooms));

        for (int i = 0; i < rooms.Count; i++)
        {
            for (int j = 0; j < rooms.Count; j++)
            {
                if(i != j)
                {
                    Assert.IsFalse(Util.byteArrayEqualityComparer.Equals(rooms[i].SideView, rooms[j].SideView)
                        && Util.byteArrayEqualityComparer.Equals(rooms[i].Enemies, rooms[j].Enemies)
                        && !(rooms[i].Group.EndsWith("vanilla") && rooms[j].Group.EndsWith("vanilla"))
                        , "Room# " + i + " (" + Util.ByteArrayToHexString(rooms[i].SideView) + "/" + Util.ByteArrayToHexString(rooms[i].Enemies) + ") " +
                        " is a duplicate of Room# " + j + " (" + Util.ByteArrayToHexString(rooms[j].SideView) + "/" + Util.ByteArrayToHexString(rooms[j].Enemies) + ")");
                }
            }
        }
        */
    }
}
