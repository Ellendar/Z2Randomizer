using Newtonsoft.Json;
using System.Diagnostics;
using System.Dynamic;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using RandomizerCore;
using Z2Randomizer.Core.Sidescroll;

namespace Z2Randomizer.Tests;

[TestClass]
public class Utils
{
    [TestMethod]
    public void ConvertRoomsData()
    {
        /*

        string roomsJson = File.ReadAllText("PalaceRooms.json");
        dynamic rooms = JsonConvert.DeserializeObject(roomsJson);
        List<Room> newRooms = new List<Room>();
        foreach (var obj in rooms)
        {
            Room room = new Room(obj.ToString());
            newRooms.Add(room);
            List <RequirementType> blockers = new List<RequirementType>();
            if(room.IsFairyBlocked)
            {
                blockers.Add(RequirementType.JUMP);
                blockers.Add(RequirementType.FAIRY);
            }
            else if (room.IsJumpBlocked)
            {
                blockers.Add(RequirementType.JUMP);
            }
            if (room.IsGloveBlocked)
            {
                blockers.Add(RequirementType.GLOVE);
            }
            if (room.IsDownstabBlocked)
            {
                blockers.Add(RequirementType.DOWNSTAB);
            }
            if (room.IsUpstabBlocked)
            {
                blockers.Add(RequirementType.UPSTAB);
            }
            room.Requirements = new Requirements(blockers);
        }

        StringBuilder finalJson = new();
        finalJson.Append("[");
        for(int i = 0; i < newRooms.Count; i++)
        {
            finalJson.Append(newRooms[i].Serialize());
            if(i != newRooms.Count -1)
            {
                finalJson.Append(",");
            }
        }
        finalJson.Append("]");

        Debug.WriteLine(finalJson.ToString());
        */
    }

    [TestMethod]
    public void GenerateRoomsMD5()
    {
        string roomsJson = File.ReadAllText("PalaceRooms.json");

        byte[] hash = MD5Hash.ComputeHash(Encoding.UTF8.GetBytes(Regex.Replace(roomsJson, @"[\n\r\f]", "")));
        Debug.WriteLine(Convert.ToBase64String(hash));
    }
}
