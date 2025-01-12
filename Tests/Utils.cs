using System.Diagnostics;
using System.Text;
using System.Text.RegularExpressions;
using DynamicData;
using McMaster.Extensions.CommandLineUtils;
using RandomizerCore;
using RandomizerCore.Sidescroll;
using SD.Tools.Algorithmia.GeneralDataStructures;

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

    [TestMethod]
    public void GenerateRoomStats()
    {
        //For each of GP / Not GP
        //Pull all the rooms
        //Categorize the rooms by their shape
        //Report the number of rooms of each shape / dropness ordered by number and a separate report of 0s

        PalaceRooms palaceRooms = new(Util.ReadAllTextFromFile("PalaceRooms.json"), false);

        List<Room> normalRooms =
        [
            .. palaceRooms.NormalPalaceRoomsByGroup(RoomGroup.VANILLA),
            .. palaceRooms.NormalPalaceRoomsByGroup(RoomGroup.V4_0),
            .. palaceRooms.NormalPalaceRoomsByGroup(RoomGroup.V4_4),
        ];

        List<Room> gpRooms =
        [
            .. palaceRooms.GpRoomsByGroup(RoomGroup.VANILLA),
            .. palaceRooms.GpRoomsByGroup(RoomGroup.V4_0),
            .. palaceRooms.GpRoomsByGroup(RoomGroup.V4_4),
        ];

        //This copypasta is lazy as fuck, I don't care.

        //Normal, NON-DROP
        Debug.WriteLine("Normal, Non-DropZone");
        MultiValueDictionary<RoomExitType, Room> categorizedRooms = [];
        foreach (Room room in normalRooms.Where(i => !i.IsDropZone))
        {
            categorizedRooms.Add(room.CategorizeExits(), room);
        }

        List<RoomExitType> missingExitTypes =
            Enum.GetValues(typeof(RoomExitType))
            .Cast<RoomExitType>().Where(i => !categorizedRooms.ContainsKey(i))
            .Where(i => i != RoomExitType.NO_ESCAPE)
            .ToList();

        List<(RoomExitType, int)> counts = [];
        foreach(RoomExitType key in categorizedRooms.Keys)
        {
            counts.Add((key, categorizedRooms[key].Count));
        }
        counts.OrderByDescending(i => i.Item2).ToList().ForEach(i => Debug.WriteLine(i.Item1 + " : " + i.Item2));
        missingExitTypes.ForEach(i => Debug.WriteLine(i + " : 0"));

        //Normal, DROP
        Debug.WriteLine("\nNormal, DropZone");
        categorizedRooms.Clear();
        foreach (Room room in normalRooms.Where(i => i.IsDropZone))
        {
            categorizedRooms.Add(room.CategorizeExits(), room);
        }

        missingExitTypes =
            Enum.GetValues(typeof(RoomExitType))
            .Cast<RoomExitType>().Where(i => !categorizedRooms.ContainsKey(i))
            .Where(i => i != RoomExitType.NO_ESCAPE)
            .ToList();

        counts.Clear();
        foreach (RoomExitType key in categorizedRooms.Keys)
        {
            counts.Add((key, categorizedRooms[key].Count));
        }
        counts.OrderByDescending(i => i.Item2).ToList().ForEach(i => Debug.WriteLine(i.Item1 + " : " + i.Item2));
        missingExitTypes.ForEach(i => Debug.WriteLine(i + " : 0"));

        //GP, NON-DROP
        Debug.WriteLine("\nGP, Non-DropZone");
        categorizedRooms.Clear();
        foreach (Room room in gpRooms.Where(i => !i.IsDropZone))
        {
            categorizedRooms.Add(room.CategorizeExits(), room);
        }

        missingExitTypes =
            Enum.GetValues(typeof(RoomExitType))
            .Cast<RoomExitType>().Where(i => !categorizedRooms.ContainsKey(i))
            .Where(i => i != RoomExitType.NO_ESCAPE)
            .ToList();

        counts.Clear();
        foreach (RoomExitType key in categorizedRooms.Keys)
        {
            counts.Add((key, categorizedRooms[key].Count));
        }
        counts.OrderByDescending(i => i.Item2).ToList().ForEach(i => Debug.WriteLine(i.Item1 + " : " + i.Item2));
        missingExitTypes.ForEach(i => Debug.WriteLine(i + " : 0"));

        //GP, DROP
        Debug.WriteLine("\nGP, DropZone");
        categorizedRooms.Clear();
        foreach (Room room in gpRooms.Where(i => i.IsDropZone))
        {
            categorizedRooms.Add(room.CategorizeExits(), room);
        }

        missingExitTypes =
            Enum.GetValues(typeof(RoomExitType))
            .Cast<RoomExitType>().Where(i => !categorizedRooms.ContainsKey(i))
            .Where(i => i != RoomExitType.NO_ESCAPE)
            .ToList();

        counts.Clear();
        foreach (RoomExitType key in categorizedRooms.Keys)
        {
            counts.Add((key, categorizedRooms[key].Count));
        }
        counts.OrderByDescending(i => i.Item2).ToList().ForEach(i => Debug.WriteLine(i.Item1 + " : " + i.Item2));
        missingExitTypes.ForEach(i => Debug.WriteLine(i + " : 0"));
    }
}
