// See https://aka.ms/new-console-template for more information

using RandomizerCore.Sidescroll;
using System.Text;
using System.Text.Json;
using Z2Randomizer.Core;
using Z2Randomizer.Core.Sidescroll;

ROM ROMData = new ROM(args[0]);
int[] connAddr = [0x1072B, 0x12208, 0x1472B];
int[] side = [0x10533, 0x12010, 0x14533];
int[] enemy = [0x105b1, 0x1208E, 0x145b1];
int[] bit = [0x17ba5, 0x17bc5, 0x17be5];
StringBuilder sb = new StringBuilder("[");
for (int j = 0; j < 3; j++)
{
    for (int i = 0; i < 63; i++)
    {
        int addr = connAddr[j] + i * 4;
        byte[] connectBytes = new byte[4];
        for (int k = 0; k < 4; k++)
        {
            connectBytes[k] = ROMData.GetByte(addr + k);

        }
        Room r;
        int sideViewPtr = (ROMData.GetByte(side[j] + i * 2) + (ROMData.GetByte(side[j] + 1 + i * 2) << 8)) + 0x8010;
        if (j == 2)
        {
            sideViewPtr = (ROMData.GetByte(side[j] + i * 2) + (ROMData.GetByte(side[j] + 1 + i * 2) << 8)) + 0xC010;
        }
        int sideViewLength = ROMData.GetByte(sideViewPtr);
        byte[] sideView = ROMData.GetBytes(sideViewPtr, sideViewPtr + sideViewLength);

        int enemyPtr = ROMData.GetByte(enemy[j] + i * 2) + (ROMData.GetByte(enemy[j] + 1 + i * 2) << 8) + 0x98b0;
        if (j == 2)
        {
            enemyPtr = ROMData.GetByte(enemy[j] + i * 2) + (ROMData.GetByte(enemy[j] + 1 + i * 2) << 8) + 0xd8b0;
        }

        int enemyLength = ROMData.GetByte(enemyPtr);
        byte[] enemies = ROMData.GetBytes(enemyPtr, enemyPtr + enemyLength);

        byte bitmask = ROMData.GetByte(bit[j] + i / 2);

        if (i % 2 == 0)
        {
            bitmask = (byte)(bitmask & 0xF0);
            bitmask = (byte)(bitmask >> 4);
        }
        else
        {
            bitmask = (byte)(bitmask & 0x0F);
        }

        bool hasItem = false;
        int currentX = 0;
        int elevatorScreen = -1;
        for (int sideviewIndex = 4; sideviewIndex < sideView.Length; sideviewIndex += 2)
        {
            int xAdvance = sideView[sideviewIndex] & 0x0F;
            int yPos = sideView[sideviewIndex] & 0xF0;
            yPos >>= 4;
            //item
            if (yPos < 13 && sideView[sideviewIndex + 1] == 0x0F)
            {
                int collectableIndex = sideView[sideviewIndex + 2];
                sideviewIndex++;
                if (collectableIndex <= 0x07)
                {
                    hasItem = true;
                }
            }
            //elevator
            else if (sideView[sideviewIndex + 1] == 0x50)
            {
               elevatorScreen = currentX >> 4;
            }
            currentX += xAdvance;
        }

        r = new Room
        {
            itemGetBits = bitmask,
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
            PalaceGroup = j,
            LinkedRoomName = null,
            IsDropZone = false,
            IsEntrance = false,
            isUpDownReversed = false,
            Map = i,
            ConnectionStartAddress = sideViewPtr,
            Name = "",
            Requirements = new Requirements(),
            SideView = sideView,
        };
        r.PalaceGroup = (j + 1);
        sb.Append(r.Serialize() + ",");
    }
}
sb[sb.Length - 1] = ']';
var jsonElement = JsonSerializer.Deserialize<JsonElement>(sb.ToString());
string prettyPrintedJson = JsonSerializer.Serialize(jsonElement, new JsonSerializerOptions() { WriteIndented = true, });
File.WriteAllText("Rooms.dump", prettyPrintedJson);