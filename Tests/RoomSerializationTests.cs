using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.Linq;
using System.Security.Cryptography;
using System.Speech.Synthesis;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Z2Randomizer.Sidescroll;

namespace Tests;

[TestClass]
public class RoomSerializationTests
{
    [TestMethod]
    public void TestDeserialization()
    {
        string roomJson = "{\"name\": \"testName\", \"enabled\": true, \"group\": \"palace1vanilla\", \"map\": 4, \"connections\": \"0F000214\", \"enemies\": \"050F0B00CB\", \"sideviewData\": \"3A600E08D208420022C8420022C8420022C8420022C8420022C8420022C84200D40E07F1F050B071D708420022C8420022C8420022C84200D20E\", \"bitmask\": \"0F\", \"isFairyBlocked\": false, \"isGloveBlocked\": false, \"isDownstabBlocked\": false, \"isUpstabBlocked\": false, \"isJumpBlocked\": false, \"hasItem\": false, \"hasBoss\": false, \"hasDrop\": false, \"elevatorScreen\": 2, \"memoryAddress\": \"01073B\", \"isUpDownReversed\": false, \"isDropZone\": false}";

        Room room = new Room(roomJson);
        Assert.AreEqual("testName", room.Name);
        Assert.AreEqual(0x1073B, room.MemAddr);
    }

    [TestMethod]
    public void TestBulkDeserialization()
    {
        Dictionary<string, List<Room>> roomsByGroup = new Dictionary<string, List<Room>>();
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

    [TestMethod] 
    public void GenerateRoomsMD5()
    {
        string roomsJson = File.ReadAllText("PalaceRooms.json");

        MD5 hasher = MD5.Create();
        byte[] hash = hasher.ComputeHash(Encoding.UTF8.GetBytes(Regex.Replace(roomsJson, @"[\n\r\f]", "")));
        Debug.WriteLine(Convert.ToBase64String(hash));
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

        public int GetHashCode(byte[] obj)
        {
            int result = 17;
            for (int i = 0; i < obj.Length; i++)
            {
                unchecked
                {
                    result = result * 23 + obj[i];
                }
            }
            return result;
        }
    }
    string ByteArrayToHexString(byte[] bytes)
    {
        string hex = BitConverter.ToString(bytes);
        return hex.Replace("-", "");
    }

    [TestMethod] 
    public void RoomsContainsNoDuplcateRooms()
    {
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

        IEqualityComparer<byte[]> byteArrayEqualityComparer = new StandardByteArrayEqualityComparer();

        for (int i = 0; i < rooms.Count; i++)
        {
            for (int j = 0; j < rooms.Count; j++)
            {
                if(i != j)
                {
                    Assert.IsFalse(byteArrayEqualityComparer.Equals(rooms[i].SideView, rooms[j].SideView)
                        && byteArrayEqualityComparer.Equals(rooms[i].Enemies, rooms[j].Enemies)
                        && !(rooms[i].Group.EndsWith("vanilla") && rooms[j].Group.EndsWith("vanilla"))
                        , "Room# " + i + " (" + ByteArrayToHexString(rooms[i].SideView) + "/" + ByteArrayToHexString(rooms[i].Enemies) + ") " +
                        " is a duplicate of Room# " + j + " (" + ByteArrayToHexString(rooms[j].SideView) + "/" + ByteArrayToHexString(rooms[j].Enemies) + ")");
                }
            }
        }
    }
}
