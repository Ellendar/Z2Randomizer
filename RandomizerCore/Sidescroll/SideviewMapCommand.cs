using System;
using System.Diagnostics;

namespace RandomizerCore.Sidescroll;

/// <summary>
/// Holds and manipulates the bytes for a side view map command.
/// Map commands can be things like: defining floors, walls,
/// decorative objects - everything that make up a palace room.
/// </summary>
public class SideviewMapCommand
{
    /// <summary>
    /// The bytes making up the map command. The array must be 2 or 3 long.
    /// </summary>
    public byte[] Bytes { get; }

    /// <summary>
    /// The absolute X position of the map command. In the game data
    /// only 4 bits are used to represent a relative x position (from
    /// the previous map command). To be able to manipulate commands
    /// efficiently, we keep track of the absolute positions.
    /// </summary>
    public int AbsX { get; set; }

    /// <summary>
    /// Create from an array of bytes making up a single map command.
    /// </summary>
    /// <param name="data">The map command bytes. Must be 2 or 3 long.</param>
    public SideviewMapCommand(byte[] data)
    {
        Debug.Assert(data != null, "data cannot be null.");
        Debug.Assert(data.Length == 2 || data.Length == 3, "data array must be 2 or 3 long.");
        Bytes = new byte[data.Length];
        Array.Copy(data, Bytes, data.Length);
    }

    public SideviewMapCommand(int x, int y, int id)
    {
        Bytes = new byte[2];
        AbsX = x;
        RelX = x;
        Y = y;
        Id = id;
    }

    public SideviewMapCommand(int x, int y, int id, int extra)
    {
        Debug.Assert(id == 15 && y < 13, "Illegal 3 byte map command");
        Bytes = new byte[3];
        AbsX = x;
        RelX = x;
        Y = y;
        Id = id;
        Extra = extra;
    }

    /// <summary>
    /// Access the y position of the map command, the first 4 bits.
    /// </summary>
    public int Y
    {
        get { return (Bytes[0] & 0xF0) >> 4; }
        set { Bytes[0] = (byte)((value << 4) + (Bytes[0] & 0x0F)); }
    }

    /// <summary>
    /// Access the relative x position of the map command, the second 4 bits.
    /// </summary>
    public int RelX {
        get { return (Bytes[0] & 0x0F); }
        set { Bytes[0] = (byte)((Bytes[0] & 0xF0) + (value & 0x0F)); }
    }

    /// <summary>
    /// Access the second byte of the map command.
    /// </summary>
    public int Id
    {
        get { return Bytes[1]; }
        set { Bytes[1] = (byte)value; }
    }

    /// <summary>
    /// Access the third byte of the map command. This only exists
    /// for special cases like items that you can pick up.
    /// </summary>
    public int Extra
    {
        get
        {
            Debug.Assert(Bytes.Length > 2, "This map command does not have the extra byte.");
            return Bytes[2];
        }
        set
        {
            Debug.Assert(Bytes.Length > 2, "This map command does not have the extra byte.");
            Bytes[2] = (byte)value;
        }
    }
}
