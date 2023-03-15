using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
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
        byte[] hash = hasher.ComputeHash(Encoding.UTF8.GetBytes(roomsJson));
        Debug.WriteLine(Convert.ToBase64String(hash));
    }
}
