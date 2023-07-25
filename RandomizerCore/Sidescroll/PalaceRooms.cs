using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;

namespace Z2Randomizer.Core.Sidescroll;

public class PalaceRooms
{
    private Dictionary<string, List<Room>> roomsByGroup = new Dictionary<string, List<Room>>();
    private Dictionary<string, List<Room>> customRoomsByGroup = new Dictionary<string, List<Room>>();

    //public readonly string roomsMD5 = "qvtwG7ntEclgbAXcszrcjA==";
    private readonly string roomsSHA1 = "EAoviKhaPDNpFWGX2KOX/6WX7Ec=";

    public PalaceRooms(string? palaceRooms, string? customRooms)
    {
        if (palaceRooms != null)
        {

            SHA1 hasher = SHA1.Create();
            byte[] hash = hasher.ComputeHash(Encoding.UTF8.GetBytes(Regex.Replace(palaceRooms, @"[\n\r\f]", "")));
            string h = Convert.ToBase64String(hash);
            if (roomsSHA1 != h)
            {
                Console.WriteLine($"Expected hash: {h}");
                throw new Exception("Invalid PalaceRooms.json");
            }
            dynamic rooms = JsonConvert.DeserializeObject(palaceRooms);
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

        if (customRooms != null)
        {
            dynamic rooms = JsonConvert.DeserializeObject(customRooms);
            foreach (var obj in rooms)
            {
                Room room = new Room(obj.ToString());
                if (room.Enabled)
                {
                    if (!customRoomsByGroup.ContainsKey(room.Group))
                    {
                        customRoomsByGroup.Add(room.Group, new List<Room>());
                    }
                    customRoomsByGroup[room.Group].Add(room);
                }
            }
        }
    }

    public List<Room> PalaceRoomsByNumber(int palaceNum, bool customRooms) => palaceNum switch
    {
        1 => customRooms ? customRoomsByGroup["palace1vanilla"] : roomsByGroup["palace1vanilla"],
        2 => customRooms ? customRoomsByGroup["palace2vanilla"] : roomsByGroup["palace2vanilla"],
        3 => customRooms ? customRoomsByGroup["palace3vanilla"] : roomsByGroup["palace3vanilla"],
        4 => customRooms ? customRoomsByGroup["palace4vanilla"] : roomsByGroup["palace4vanilla"],
        5 => customRooms ? customRoomsByGroup["palace5vanilla"] : roomsByGroup["palace5vanilla"],
        6 => customRooms ? customRoomsByGroup["palace6vanilla"] : roomsByGroup["palace6vanilla"],
        7 => customRooms ? customRoomsByGroup["palace7vanilla"] : roomsByGroup["palace7vanilla"],
        _ => throw new ArgumentException("Invalid palace number: " + palaceNum)
    };

    public Room Thunderbird(bool useCustomRooms)
    { 
        return useCustomRooms ? customRoomsByGroup["thunderbird"].First() : roomsByGroup["thunderbird"].First();
    }

    public List<Room> TBirdRooms(bool useCustomRooms)
    {
        return useCustomRooms ? customRoomsByGroup["tbirdRooms"]: roomsByGroup["tbirdRooms"];
    }
    public List<Room> BossRooms(bool useCustomRooms)
    {
        return useCustomRooms ? customRoomsByGroup["bossrooms"] : roomsByGroup["bossrooms"];
    }
    public List<Room> ItemRooms(bool useCustomRooms)
    {
        return useCustomRooms ? customRoomsByGroup["itemRooms"] : roomsByGroup["itemRooms"];
    }
    public List<Room> Entrances(bool useCustomRooms)
    {
        return useCustomRooms ? customRoomsByGroup["entrances"] : roomsByGroup["entrances"];
    }
    public Room MaxBonusItemRoom(bool useCustomRooms)
    {
        return useCustomRooms ? customRoomsByGroup["maxBonusItemRoom"].First() : roomsByGroup["maxBonusItemRoom"].First();
    }
    public List<Room> AaronRoomJam(bool useCustomRooms)
    {
        return useCustomRooms ? customRoomsByGroup["aaronRoomJam"] : roomsByGroup["aaronRoomJam"];
    }
    public List<Room> BenthicKing(bool useCustomRooms)
    {
        return useCustomRooms ? customRoomsByGroup["benthicKing"] : roomsByGroup["benthicKing"];
    }
    public List<Room> DMInPalaces(bool useCustomRooms)
    {
        return useCustomRooms ? customRoomsByGroup["dmInPalaces"] : roomsByGroup["dmInPalaces"];
    }
    public List<Room> DusterRoomJam(bool useCustomRooms)
    {
        return useCustomRooms ? customRoomsByGroup["dusterRoomJam"] : roomsByGroup["dusterRoomJam"];
    }
    public List<Room> EasternShadow(bool useCustomRooms)
    {
        return useCustomRooms ? customRoomsByGroup["easternShadow"] : roomsByGroup["easternShadow"];
    }
    public List<Room> EonRoomJam(bool useCustomRooms)
    {
        return useCustomRooms ? customRoomsByGroup["eonRoomjam"] : roomsByGroup["eonRoomjam"];
    }
    public List<Room> EunosGpRooms(bool useCustomRooms)
    {
        return useCustomRooms ? customRoomsByGroup["eunosGpRooms"] : roomsByGroup["eunosGpRooms"];
    }
    public List<Room> EunosRooms(bool useCustomRooms)
    {
        return useCustomRooms ? customRoomsByGroup["eunosRooms"] : roomsByGroup["eunosRooms"];
    }
    public List<Room> FlippedGP(bool useCustomRooms)
    {
        return useCustomRooms ? customRoomsByGroup["flippedGp"] : roomsByGroup["flippedGp"];
    }
    public List<Room> GTMNewGpRooms(bool useCustomRooms)
    {
        return useCustomRooms ? customRoomsByGroup["gtmNewgpRooms"] : roomsByGroup["gtmNewgpRooms"];
    }
    public  List<Room> KnightcrawlerRoomJam(bool useCustomRooms)
    {
        return useCustomRooms ? customRoomsByGroup["knightCrawlerRoomJam"] : roomsByGroup["knightCrawlerRoomJam"];
    }
    public  List<Room> Link7777RoomJam(bool useCustomRooms)
    {
        return useCustomRooms ? customRoomsByGroup["link7777RoomJam"] : roomsByGroup["link7777RoomJam"];
    }
    public  List<Room> MaxRoomJam(bool useCustomRooms)
    {
        return useCustomRooms ? customRoomsByGroup["maxRoomJam"] : roomsByGroup["maxRoomJam"];
    }
    public  List<Room> Palace1Vanilla(bool useCustomRooms)
    {
        return useCustomRooms ? customRoomsByGroup["palace1vanilla"] : roomsByGroup["palace1vanilla"];
    }
    public  List<Room> Palace2Vanilla(bool useCustomRooms)
    {
        return useCustomRooms ? customRoomsByGroup["palace2vanilla"] : roomsByGroup["palace2vanilla"];
    }
    public  List<Room> Palace3Vanilla(bool useCustomRooms)
    {
        return useCustomRooms ? customRoomsByGroup["palace3vanilla"] : roomsByGroup["palace3vanilla"];
    }
    public  List<Room> Palace4Vanilla(bool useCustomRooms)
    {
        return useCustomRooms ? customRoomsByGroup["palace4vanilla"] : roomsByGroup["palace4vanilla"];
    }
    public  List<Room> Palace5Vanilla(bool useCustomRooms)
    {
        return useCustomRooms ? customRoomsByGroup["palace5vanilla"] : roomsByGroup["palace5vanilla"];
    }
    public  List<Room> Palace6Vanilla(bool useCustomRooms)
    {
        return useCustomRooms ? customRoomsByGroup["palace6vanilla"] : roomsByGroup["palace6vanilla"];
    }
    public  List<Room> Palace7Vanilla(bool useCustomRooms)
    {
        return useCustomRooms ? customRoomsByGroup["palace7vanilla"] : roomsByGroup["palace7vanilla"];
    }
    public  List<Room> RoomJamGTM(bool useCustomRooms)
    {
        return useCustomRooms ? customRoomsByGroup["roomJamGTM"] : roomsByGroup["roomJamGTM"];
    }
    public  List<Room> TriforceOfCourage(bool useCustomRooms)
    {
        return useCustomRooms ? customRoomsByGroup["triforceOfCourage"] : roomsByGroup["triforceOfCourage"];
    }
    public  List<Room> TriforceOfCourageGP(bool useCustomRooms)
    {
        return useCustomRooms ? customRoomsByGroup["triforceOfCourageGP"] : roomsByGroup["triforceOfCourageGP"];
    }
    public  List<Room> WinterSolstice(bool useCustomRooms)
    {
        return useCustomRooms ? customRoomsByGroup["winterSolstice"] : roomsByGroup["winterSolstice"];
    }
    public  List<Room> WinterSolsticeGP(bool useCustomRooms)
    {
        return useCustomRooms ? customRoomsByGroup["winterSolsticeGP"] : roomsByGroup["winterSolsticeGP"];
    }
    public  List<Room> GTMOldGPRooms(bool useCustomRooms)
    {
        return useCustomRooms ? customRoomsByGroup["gtmOldgpRooms"] : roomsByGroup["gtmOldgpRooms"];
    }
    public  List<Room> NewP6BossRooms(bool useCustomRooms)
    {
        return useCustomRooms ? customRoomsByGroup["newp6BossRooms"] : roomsByGroup["newp6BossRooms"];
    }
    public  List<Room> NewBossRooms(bool useCustomRooms)
    {
        return useCustomRooms ? customRoomsByGroup["newbossrooms"] : roomsByGroup["newbossrooms"];
    }
    public  List<Room> LeftOpenItemRooms(bool useCustomRooms)
    {
        return useCustomRooms ? customRoomsByGroup["leftOpenItemRooms"] : roomsByGroup["leftOpenItemRooms"];
    }
    public  List<Room> RightOpenItemRooms(bool useCustomRooms)
    {
        return useCustomRooms ? customRoomsByGroup["rightOpenItemRooms"] : roomsByGroup["rightOpenItemRooms"];
    }
    public  List<Room> UpOpenItemRooms(bool useCustomRooms)
    {
        return useCustomRooms ? customRoomsByGroup["upOpenItemRooms"] : roomsByGroup["upOpenItemRooms"];
    }
    public  List<Room> DownOpenItemRooms(bool useCustomRooms)
    {
        return useCustomRooms ? customRoomsByGroup["downOpenItemRooms"] : roomsByGroup["downOpenItemRooms"];
    }
    public  List<Room> ThroughItemRooms(bool useCustomRooms)
    {
        return useCustomRooms ? customRoomsByGroup["throughItemRooms"] : roomsByGroup["throughItemRooms"];
    }
    public  List<Room> DarkLinkRooms(bool useCustomRooms)
    {
        return useCustomRooms ? customRoomsByGroup["darkLinkRooms"] : roomsByGroup["darkLinkRooms"];
    }
}
