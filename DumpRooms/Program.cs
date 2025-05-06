// See https://aka.ms/new-console-template for more information
#pragma warning disable IL2026 // Members annotated with 'RequiresUnreferencedCodeAttribute' require dynamic access otherwise can break functionality when trimming application code

using System.Text;
using System.Text.Json;
using Z2Randomizer.RandomizerCore;
using Z2Randomizer.RandomizerCore.Sidescroll;

ROM ROMData = new ROM(args[0], true);

int[] connAddr = [0x1072B, 0x12208, 0x1472B];
int[] side = [0x10533, 0x12010, 0x14533];
int[] enemy = [0x105b1, 0x1208E, 0x145b1];
int[] bit = [0x17ba5, 0x17bc5, 0x17be5];
StringBuilder sb = new StringBuilder("[");
for (int palaceGroup = 0; palaceGroup < 3; palaceGroup++)
{
    for (int map = 0; map < 63; map++)
    {
        int addr = connAddr[palaceGroup] + map * 4;
        byte[] connectBytes = new byte[4];
        for (int k = 0; k < 4; k++)
        {
            connectBytes[k] = ROMData.GetByte(addr + k);

        }
        Room r;
        int sideViewPtr = (ROMData.GetByte(side[palaceGroup] + map * 2) + (ROMData.GetByte(side[palaceGroup] + 1 + map * 2) << 8)) + 0x8010;
        if (palaceGroup == 2)
        {
            sideViewPtr = (ROMData.GetByte(side[palaceGroup] + map * 2) + (ROMData.GetByte(side[palaceGroup] + 1 + map * 2) << 8)) + 0xC010;
        }
        int sideViewLength = ROMData.GetByte(sideViewPtr);
        byte[] sideView = ROMData.GetBytes(sideViewPtr, sideViewLength);

        int enemyPtr = ROMData.GetByte(enemy[palaceGroup] + map * 2) + (ROMData.GetByte(enemy[palaceGroup] + 1 + map * 2) << 8) + 0x98b0;
        if (palaceGroup == 2)
        {
            enemyPtr = ROMData.GetByte(enemy[palaceGroup] + map * 2) + (ROMData.GetByte(enemy[palaceGroup] + 1 + map * 2) << 8) + 0xd8b0;
        }

        int enemyLength = ROMData.GetByte(enemyPtr);
        byte[] enemies = ROMData.GetBytes(enemyPtr, enemyLength);

        byte bitmask = ROMData.GetByte(bit[palaceGroup] + map / 2);

        if (map % 2 == 0)
        {
            bitmask = (byte)(bitmask & 0xF0);
            bitmask = (byte)(bitmask >> 4);
        }
        else
        {
            bitmask = (byte)(bitmask & 0x0F);
        }

        bool hasItem = false;
        int elevatorScreen = -1;
        try
        {
            SideviewEditable<PalaceObject> sideviewEditable = new(sideView);
            hasItem = sideviewEditable.HasItem();
            var elevator = sideviewEditable.Find(o => o.IsElevator());
            if (elevator != null) { elevatorScreen = elevator.AbsX / 16; }
        }
        catch (Exception e)
        {
            Console.WriteLine("Malformed room breaks parsing. Group: " + palaceGroup + " Map: " + map);
        }

        r = new Room
        {
            ItemGetBits = [bitmask],
            Connections = connectBytes,
            ElevatorScreen = elevatorScreen,
            Enabled = true,
            Enemies = enemies,
            Group = RoomGroup.V4_4,
            Author = "",
            HasBoss = false,
            IsBossRoom = false,
            HasDrop = false,
            HasItem = hasItem,
            IsThunderBirdRoom = false,
            PalaceNumber = null,
            // PalaceGroup = (PalaceGrouping)palaceGroup,
            LinkedRoomName = null,
            IsDropZone = false,
            IsEntrance = false,
            //IsUpDownReversed = false,
            Map = (byte)map,
            ConnectionStartAddress = sideViewPtr,
            Name = "",
            Requirements = new Requirements(),
            SideView = sideView,
        };
        // r.PalaceGroup = (PalaceGrouping)palaceGroup;
        sb.Append(r.Serialize() + ",");
    }
}
sb[sb.Length - 1] = ']';
var jsonElement = JsonSerializer.Deserialize<JsonElement>(sb.ToString());
JsonSerializerOptions options = new JsonSerializerOptions() { WriteIndented = true, };
string prettyPrintedJson = JsonSerializer.Serialize(jsonElement, options);
File.WriteAllText("Rooms.dump", prettyPrintedJson);

#pragma warning restore IL2026 // Members annotated with 'RequiresUnreferencedCodeAttribute' require dynamic access otherwise can break functionality when trimming application code
