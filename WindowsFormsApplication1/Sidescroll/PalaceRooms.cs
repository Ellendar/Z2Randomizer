using Newtonsoft.Json;
using SD.Tools.Algorithmia.GeneralDataStructures;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Z2Randomizer.Sidescroll;

public class PalaceRooms
{
    private static Dictionary<string, List<Room>> roomsByGroup = new Dictionary<string, List<Room>>();
    public static int Test = 0;

    static PalaceRooms()
    {
        Test = 1;
        string roomsJson = File.ReadAllText("PalaceRooms.json");
        dynamic rooms = JsonConvert.DeserializeObject(roomsJson);
        foreach (var obj in rooms)
        {
            Room room = new Room(obj.ToString());
            if (room.Enabled)
            {
                if (!roomsByGroup.ContainsKey(room.Group))
                {
                    roomsByGroup.Add(room.Group, new List<Room>());
                }
                roomsByGroup[room.Group].Add(room);
            }
        }
    }

    /*
    public static List<Room> Palaces { 
        get {
            return roomsByGroup["palace1vanilla"]
                .Union(roomsByGroup["palace2vanilla"])
                .Union(roomsByGroup["palace3vanilla"])
                .Union(roomsByGroup["palace4vanilla"])
                .Union(roomsByGroup["palace5vanilla"])
                .Union(roomsByGroup["palace6vanilla"])
                .Union(roomsByGroup["palace7vanilla"])
                .ToList();
        } 
    }
    */
    public static List<Room> PalaceRoomsByNumber(int palaceNum) => palaceNum switch
    {
        1 => roomsByGroup["palace1vanilla"],
        2 => roomsByGroup["palace2vanilla"],
        3 => roomsByGroup["palace3vanilla"],
        4 => roomsByGroup["palace4vanilla"],
        5 => roomsByGroup["palace5vanilla"],
        6 => roomsByGroup["palace6vanilla"],
        7 => roomsByGroup["palace7vanilla"],
        _ => throw new ArgumentException("Invalid palace number: " + palaceNum)
    };
   
    //public static List<List<Room>> PalacesByNumber = new List<List<Room>> { };
    public static Room Thunderbird { get { return roomsByGroup["thunderbird"].First(); } }
    public static List<Room> TBirdRooms { get { return roomsByGroup["tbirdRooms"]; } }
    public static List<Room> BossRooms { get { return roomsByGroup["bossrooms"]; } }
    public static List<Room> ItemRooms { get { return roomsByGroup["itemRooms"]; } }
    public static List<Room> Entrances { get { return roomsByGroup["entrances"]; } }
    public static Room MaxBonusItemRoom { get { return roomsByGroup["maxBonusItemRoom"].First(); } }
    public static List<Room> AaronRoomJam { get { return roomsByGroup["aaronRoomJam"]; } }
    public static List<Room> BenthicKing { get { return roomsByGroup["benthicKing"]; } }
    public static List<Room> DMInPalaces { get { return roomsByGroup["dmInPalaces"]; } }
    public static List<Room> DusterRoomJam { get { return roomsByGroup["dusterRoomJam"]; } }
    public static List<Room> EasternShadow { get { return roomsByGroup["easternShadow"]; } }
    public static List<Room> EonRoomJam { get { return roomsByGroup["eonRoomjam"]; } }
    public static List<Room> EunosGpRooms { get { return roomsByGroup["eunosGpRooms"]; } }
    public static List<Room> EunosRooms { get { return roomsByGroup["eunosRooms"]; } }
    public static List<Room> FlippedGP { get { return roomsByGroup["flippedGp"]; } }
    public static List<Room> GTMNewGpRooms { get { return roomsByGroup["gtmNewgpRooms"]; } }
    public static List<Room> KnightcrawlerRoomJam { get { return roomsByGroup["knightCrawlerRoomJam"]; } }
    public static List<Room> Link7777RoomJam { get { return roomsByGroup["link7777RoomJam"]; } }
    public static List<Room> MaxRoomJam { get { return roomsByGroup["maxRoomJam"]; } }
    public static List<Room> Palace1Vanilla { get { return roomsByGroup["palace1vanilla"]; } }
    public static List<Room> Palace2Vanilla { get { return roomsByGroup["palace2vanilla"]; } }
    public static List<Room> Palace3Vanilla { get { return roomsByGroup["palace3vanilla"]; } }
    public static List<Room> Palace4Vanilla { get { return roomsByGroup["palace4vanilla"]; } }
    public static List<Room> Palace5Vanilla { get { return roomsByGroup["palace5vanilla"]; } }
    public static List<Room> Palace6Vanilla { get { return roomsByGroup["palace6vanilla"]; } }
    public static List<Room> Palace7Vanilla { get { return roomsByGroup["palace7vanilla"]; } }
    public static List<Room> RoomJamGTM { get { return roomsByGroup["roomJamGTM"]; } }
    public static List<Room> TriforceOfCourage { get { return roomsByGroup["triforceOfCourage"]; } }
    public static List<Room> TriforceOfCourageGP { get { return roomsByGroup["triforceOfCourageGP"]; } }
    public static List<Room> WinterSolstice { get { return roomsByGroup["winterSolstice"]; } }
    public static List<Room> WinterSolsticeGP { get { return roomsByGroup["winterSolsticeGP"]; } }
    public static List<Room> GTMOldGPRooms { get { return roomsByGroup["gtmOldgpRooms"]; } }
    public static List<Room> NewP6BossRooms { get { return roomsByGroup["newp6BossRooms"]; } }
    public static List<Room> NewBossRooms { get { return roomsByGroup["newbossrooms"]; } }
    public static List<Room> LeftOpenItemRooms { get { return roomsByGroup["leftOpenItemRooms"]; } }
    public static List<Room> RightOpenItemRooms { get { return roomsByGroup["rightOpenItemRooms"]; } }
    public static List<Room> UpOpenItemRooms { get { return roomsByGroup["upOpenItemRooms"]; } }
    public static List<Room> DownOpenItemRooms { get { return roomsByGroup["downOpenItemRooms"]; } }
    public static List<Room> ThroughItemRooms { get { return roomsByGroup["throughItemRooms"]; } }
    public static List<Room> DarkLinkRooms { get { return roomsByGroup["darkLinkRooms"]; } }
}
