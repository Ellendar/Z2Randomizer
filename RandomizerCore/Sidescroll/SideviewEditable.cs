using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace Z2Randomizer.RandomizerCore.Sidescroll;

//TODO:Restructure this as a Sideview class that understands how the format actually works
//has properties that describe things about it, and can answer questions about the net result of the format
//then replace all instances of the Sideview byte[] with references to that class.
//This will allow us to ask questions of it like "does this room contain a key door"
//or "can you get from the left to right side of this room without fairy"
//these will be very useful for the future (as well as just being easier to read)

/// <summary>
/// Holds a list of <see cref="SideviewMapCommand"/>s in order to allow programmatically editing a room. 
/// 
/// <see cref="Finalize"/> must be called after editing to retrieve the updated bytes for the room.
/// </summary>
public class SideviewEditable<T> where T : Enum
{
    private byte[] Header;
    public List<SideviewMapCommand<T>> Commands { get; set; } 

    /// <summary>
    /// Create a SideViewEditable from an array of bytes for a side view.
    /// </summary>
    /// <param name="bytes"><see cref="Room.SideView"/> goes here.</param>
    public SideviewEditable(byte[] bytes)
    {
        if(bytes.Length < 4) { throw new ArgumentException("Sideview data has no header."); }
        Header = bytes[0..4];
        if (bytes.Length != Length) { throw new ArgumentException("Sideview data has bad length in header."); }
        var bgMap = BackgroundMap;
        // (bg maps in the overworld are never solid)
        if (bgMap != 0 && this is SideviewEditable<PalaceObject>) {
            byte[] bgMapBytes = [];
            switch (bgMap)
            {
                case 1: // from 0x1062f
                    bgMapBytes = [
                        0x1A, 0x60, 0x0F, 0x08, 0xD6, 0x08, 0xA2, 0x45,
                        0x84, 0xC1, 0xD4, 0x0E, 0xD4, 0x08, 0xE3, 0x00,
                        0xA8, 0x77, 0x94, 0x73, 0x50, 0x21, 0x30, 0x09,
                        0xD2, 0x0C
                    ];
                    break;
                case 2: // from 0x1210c
                    bgMapBytes = [
                        0x18, 0x60, 0x0E, 0x10, 0xE1, 0x00, 0xD0, 0x00,
                        0x5F, 0x09, 0x70, 0x20, 0xD1, 0x0E, 0xE3, 0x00,
                        0xD0, 0x00, 0xA2, 0x45, 0x81, 0xC1, 0xD7, 0x0F
                    ];
                    break;
                case 3: // from 0x12173
                    bgMapBytes = [
                        0x18, 0x60, 0x00, 0x10, 0xDD, 0x0E, 0xDC, 0x00,
                        0x6A, 0x02, 0xD1, 0x0E, 0x03, 0xF1, 0xB0, 0x71,
                        0xF0, 0x50, 0x84, 0x05, 0xD1, 0x00, 0x60, 0x01
                    ];
                    break;
                default:
                    throw new NotImplementedException();
            }
            byte[] xSkip = [0xE0, 0]; // insert after bgmap to set x cursor back to 0
            bytes = [(byte)(bytes[0] + bgMapBytes[0] - 2), ..bgMapBytes[1..], ..xSkip, ..bytes[4..]];
            Header = bytes[0..4];
            if (bytes.Length != Length) { throw new ArgumentException("Sideview background map has bad length in header."); }
            BackgroundMap = 0;
        }
        Commands = new List<SideviewMapCommand<T>>();
        int i = 4; // start after header
        int xcursor = 0;
        while (i < bytes.Length)
        {
            byte[] objectBytes;
            byte firstByte = bytes[i++];
            if (i == bytes.Length) { throw new ArgumentException("Sideview data contains incomplete map command."); }
            byte secondByte = bytes[i++];
            int ypos = (firstByte & 0xF0) >> 4;
            if (secondByte == 15 && ypos < 13) // 3 byte object found
            {
                if (i == bytes.Length) { throw new ArgumentException("Sideview data contains incomplete map command."); }
                byte thirdByte = bytes[i++];
                objectBytes = [firstByte, secondByte, thirdByte];
            }
            else
            {
                objectBytes = [firstByte, secondByte];
            }
            if (ypos == 14) // "x skip" command resets the cursor to the start of a page
            {
                xcursor = 16 * (firstByte & 0x0F);
            }
            else
            {
                xcursor += firstByte & 0x0F;
            }
            SideviewMapCommand<T> o = new(objectBytes);
            o.AbsX = xcursor;
            Commands.Add(o);
        }
    }

    public byte Length { get => Header[0]; }

    public byte ObjectSet
    {
        get => (byte)((Header[1] & 0b10000000) >> 7);
        set { Header[1] = (byte)((Header[1] & 0b01111111) | (((value) << 7) & 0b10000000)); }
    }

    public byte PageCount {
        get => (byte)(((Header[1] & 0b01100000) >> 5) + 1);
        set { Header[1] = (byte)((Header[1] & 0b10011111) | (((value - 1) << 5) & 0b01100000)); }
    }

    public byte FloorHeader
    {
        get { return (byte)(Header[2] & 0b10001111); }
        set { Header[2] = (byte)((Header[2] & 0b01110000) | (value & 0b10001111)); }
    }

    public byte TilesHeader
    {
        get { return (byte)((Header[2] & 0b01110000) >> 4); }
        set { Header[2] = (byte)((Header[2] & 0b10001111) | ((value & 0b0111) << 4)); }
    }

    public byte SpritePalette
    {
        get { return (byte)((Header[3] & 0b11000000) >> 6); }
        set { Header[3] = (byte)((Header[3] & 0b00111111) | ((value & 0b11) << 6)); }
    }

    public byte BackgroundPalette
    {
        get { return (byte)((Header[3] & 0b00111000) >> 3); }
        set { Header[3] = (byte)((Header[3] & 0b11000111) | ((value & 0b111) << 3)); }
    }

    public byte BackgroundMap
    {
        get { return (byte)(Header[3] & 0b00000111); }
        set { Header[3] = (byte)((Header[3] & 0b11111000) + (value & 0b00000111)); }
    }

    public SideviewMapCommand<T>? Find(Predicate<SideviewMapCommand<T>> match)
    {
        return Commands.Find(match);
    }

    public List<SideviewMapCommand<T>> FindAll(Predicate<SideviewMapCommand<T>> match)
    {
        return Commands.FindAll(match);
    }


    public void Add(SideviewMapCommand<T> command)
    {
        Commands.Add(command);
    }

    public void Remove(SideviewMapCommand<T> item)
    {
        Commands.Remove(item);
    }

    public void RemoveAll(Predicate<SideviewMapCommand<T>> match)
    {
        Commands.RemoveAll(match);
    }

    public bool HasItem()
    {
        return Find(o => o.IsItem() && !o.Extra.IsMinorItem()) != null;
    }

    /// <summary>
    /// Rebuild the side view bytes with our updated list of map commands.
    /// 
    /// We iterate through every map command and make sure all of the
    /// relative x positions are updated to match their absolute x position.
    /// </summary>
    /// <returns>The final room side view bytes</returns>
    public byte[] Finalize()
    {
        int i = 0;
        // remove "x skip" commands before sorting
        // they are easily re-created if needed
        while (i < Commands.Count)
        {
            SideviewMapCommand<T> o = Commands[i];
            if (o.Y == 0xE)
            {
                Commands.RemoveAt(i);
            }
            else
            {
                i++;
            }
        }
        // map commands must be sorted left-to-right
        Commands.Sort((a, b) =>
            {
                if (a.AbsX != b.AbsX)
                {
                    return a.AbsX.CompareTo(b.AbsX);
                }
                else
                {
                    int aYOffset = (a.Y + 3) % 16;
                    int bYOffset = (b.Y + 3) % 16;
                    return aYOffset.CompareTo(bYOffset);
                }
            });
        i = 0;
        int xCursor = 0;
        while (i < Commands.Count)
        {
            SideviewMapCommand<T> o = Commands[i];
            if (xCursor + o.RelX != o.AbsX)
            {
                int xDiff = o.AbsX - xCursor;
                if (xDiff > 15) // create new "x skip" command
                {
                    var xSkip = SideviewMapCommand<T>.CreateXSkip(o.AbsX / 16);
                    Commands.Insert(i, xSkip);
                    i++;
                    xDiff = o.AbsX & 0xF;
                }
                o.RelX = xDiff;
            }
            xCursor = o.AbsX;
            i++;
        }
        byte[] bytes = [
            .. Header, 
            .. Commands.SelectMany(o => o.Bytes)
        ];
        bytes[0] = (byte)bytes.Length;
        return bytes;
    }

    public String DebugString()
    {
        StringBuilder sb = new StringBuilder("");
        var headerBytes = Convert.ToHexString(Header);
        var floor = SideviewMapCommand<T>.CreateNewFloor(0, FloorHeader);
        sb.AppendLine($"{headerBytes}       {"Floor",-26}{FloorHeader}");

        foreach (var c in Commands)
        {
            sb.AppendLine(c.DebugString());
        }
        return sb.ToString();
    }

    public bool[,] CreateSolidGrid()
    {
        int width = PageCount * 16;
        const int height = 13;
        bool[,] result = new bool[width, height];
        var floor = SideviewMapCommand<T>.CreateNewFloor(0, FloorHeader);
        List<SideviewMapCommand<T>> floors = FindAll(o => o.IsNewFloor());
        for (var x = 0; x < width; x++)
        {
            while (floors.Count > 0 && floors[0].AbsX == x)
            {
                floor = floors[0];
                floors.RemoveAt(0);
            }
            for (int y = 0; y < height; y++)
            {
                result[x, y] = floor.IsFloorSolidAt(this, y);
            }
        }
        foreach (var cmd in Commands)
        {
            if (cmd.IsSolid)
            {
                int w = Math.Min(width, cmd.AbsX + cmd.Width);
                int h = Math.Min(height, cmd.Y + cmd.Height);
                for (var x = cmd.AbsX; x < w; x++)
                {
                    for (int y = cmd.Y; y < h; y++)
                    {
                        result[x, y] = cmd.IsSolidAt(x, y);
                    }
                }
            }
            else if (cmd.IsPit)
            {
                int w = Math.Min(width, cmd.AbsX + cmd.Width);
                int h = Math.Min(height, cmd.Y + cmd.Height);
                for (var x = cmd.AbsX; x < w; x++)
                {
                    for (int y = cmd.Y; y < h; y++)
                    {
                        result[x, y] = false;
                    }
                }
            }
        }
        return result;
    }
}

public static class SolidGridHelper
{
    public static bool[,] GridUnion(bool[,] gridA, bool[,] gridB)
    {
        int widthA = gridA.GetLength(0);
        int widthB = gridB.GetLength(0);
        int widthMin = Math.Min(widthA, widthB);
        int widthMax = Math.Max(widthA, widthB);
        const int height = 13;
        var result = new bool[widthMax, height];

        for (int x = 0; x < widthMin; x++)
        {
            for (int y = 0; y < height; y++)
            {
                result[x, y] = gridA[x, y] || gridB[x, y];
            }
        }
        if (widthA != widthB)
        {
            var longerGrid = widthA > widthB ? gridA : gridB;
            for (int x = widthMin; x < widthMax; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    result[x, y] = longerGrid[x, y];
                }
            }
        }
        return result;
    }

    /// <summary>
    /// Check if solidGrid has a w x h opening at x, y.
    /// </summary>
    public static bool AreaIsOpen(bool[,] solidGrid, int x, int y, int w, int h)
    {
        var xEnd = Math.Min(x + w, solidGrid.GetLength(0));
        var yEnd = Math.Min(y + h, solidGrid.GetLength(1));
        for (int i = x; i < xEnd; i++)
        {
            for (int j = y; j < yEnd; j++)
            {
                if (solidGrid[i, j]) { return false; }
            }
        }
        return true;
    }

    /// <summary>
    /// Find the best floor y position that has a w x h opening. Optimally
    /// such that y + h will end on top of the floor.
    /// </summary>
    public static int FindFloor(bool[,] solidGrid, int x, int y, int w, int h)
    {
        Debug.Assert(y < 13);
        Debug.Assert(solidGrid.GetLength(1) == 13);

        var xEnd = Math.Min(x + w, solidGrid.GetLength(0));

        bool RowIsOpen(int j)
        {
            for (int i = x; i < xEnd; i++)
            {
                if (solidGrid[i, j])
                {
                    return false;
                }
            }
            return true;
        }

        // first try to find a floor close to the original position
        int consecutiveOpens = 0;
        int j;
        // try scanning down
        for (j = y; j < 13; j++)
        {
            if (RowIsOpen(j))
            {
                consecutiveOpens++;
            }
            else
            {
                break;
            }
        }
        if (consecutiveOpens >= h)
        {
            return j - h;
        }
        // try moving up
        for (j = y - 1; j >= 0; j--)
        {
            if (RowIsOpen(j))
            {
                consecutiveOpens++;
                if (consecutiveOpens == h)
                {
                    return j;
                }
            }
            else
            {
                break;
            }
        }

        // no luck, instead try to find the lowest floor possible
        consecutiveOpens = 0;
        for (j = 12; j >= 0; j--)
        {
            if (RowIsOpen(j))
            {
                consecutiveOpens++;
                if (consecutiveOpens == h)
                {
                    return j;
                }
            }
            else
            {
                consecutiveOpens = 0;
            }
        }

        return 0;
    }
}
