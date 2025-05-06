using System;
using System.Diagnostics;

namespace Z2Randomizer.RandomizerCore.Sidescroll;

/// <summary>
/// Holds and manipulates the bytes for a side view map command.
/// Map commands can be things like: defining floors, walls,
/// decorative objects - everything that make up a palace room.
/// </summary>
public class SideviewMapCommand<T> where T : Enum
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

    public int Page
    {
        get => AbsX / 16;
    }

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

    public SideviewMapCommand(int x, int y, T id)
    {
        Bytes = new byte[2];
        AbsX = x;
        RelX = x;
        Y = y;
        Id = id;
    }

    public static SideviewMapCommand<T> CreateXSkip(int page)
    {
        byte[] bytes = [(byte)(0xE0 + page), 0];
        return new SideviewMapCommand<T>(bytes);
    }

    public static SideviewMapCommand<T> CreateNewFloor(int x, int param)
    {
        byte[] bytes = [(byte)0xD0, (byte)param];
        var obj = new SideviewMapCommand<T>(bytes);
        obj.RelX = x;
        obj.AbsX = x;
        return obj;
    }

    public static SideviewMapCommand<T> CreateCollectable(int x, int y, Collectable extra)
    {
        Debug.Assert(y < 13, "Collectable command must have y < 13");
        byte[] bytes = new byte[3];
        var obj = new SideviewMapCommand<T>(bytes);
        obj.AbsX = x;
        obj.RelX = x;
        obj.Y = y;
        obj.Bytes[1] = 0x0F;
        obj.Extra = extra;
        return obj;
    }

    /// <summary>
    /// Access the y position of the map command, the first 4 bits.
    /// </summary>
    public int Y
    {
        get => (Bytes[0] & 0xF0) >> 4;
        set { Bytes[0] = (byte)((value << 4) | (Bytes[0] & 0x0F)); }
    }

    /// <summary>
    /// Access the relative x position of the map command, the second 4 bits.
    /// </summary>
    public int RelX {
        get => Bytes[0] & 0x0F;
        set { Bytes[0] = (byte)((Bytes[0] & 0xF0) | (value & 0x0F)); }
    }

    /// <summary>
    /// Access the second byte of the map command.
    /// </summary>
    public T Id
    {
        get => (T)Enum.ToObject(typeof(T), (Bytes[1] < 0x10) ? Bytes[1] : (Bytes[1] & 0xF0));
        set { Bytes[1] = Convert.ToByte(value); }
    }

    /// <summary>
    /// Access the second 4 bits of the 2nd byte. Typically it's the length of platforms.
    /// </summary>
    public int Param
    {
        get
        {
            Debug.Assert(HasParam(), "This map command does not have a parameter.");
            return Bytes[1] & 0x0F;
        }
        set
        {
            Debug.Assert(HasParam(), "This map command does not have a parameter.");
            Bytes[1] = (byte)((Bytes[1] & 0xF0) + (value & 0x0F));
        }
    }

    /// <summary>
    /// Access the third byte of the map command. This only exists
    /// for special cases like items that you can pick up.
    /// </summary>
    public Collectable Extra
    {
        get
        {
            Debug.Assert(HasExtra(), "This map command does not have the extra byte.");
            return (Collectable)Bytes[2];
        }
        set
        {
            Debug.Assert(HasExtra(), "This map command does not have the extra byte.");
            Bytes[2] = (byte)value;
        }
    }

    public bool HasParam()
    {
        return (Bytes[1] & 0xF0) > 0;
    }

    public bool HasExtra()
    {
        return Bytes.Length > 2;
    }

    public bool IsItem()
    {
        return Bytes[1] == 0x0F && Y < 13 && HasExtra();
    }

    public bool IsNewFloor()
    {
        return Y == 13;
    }

    public bool IsXSkip()
    {
        return Y == 14;
    }

    public bool IsLava()
    {
        switch (this)
        {
            case SideviewMapCommand<PalaceObject>:
            case SideviewMapCommand<GreatPalaceObject>:
                return Y == 15 && (Bytes[1] & 0xF0) == 0x10;
            default:
                throw new NotImplementedException();
        }
    }

    public bool IsElevator()
    {
        return Y == 15 && (Bytes[1] & 0xF0) == 0x50;
    }

    public int Width
    {
        get
        {
            if (Y < 13)
            {
                switch (this)
                {
                    case SideviewMapCommand<PalaceObject>:
                        return PalaceObjectExtensions.Width((this as SideviewMapCommand<PalaceObject>)!);
                    case SideviewMapCommand<GreatPalaceObject>:
                        return GreatPalaceObjectExtensions.Width((this as SideviewMapCommand<GreatPalaceObject>)!);
                    default:
                        throw new NotImplementedException();
                }
            }
            else if (Y < 15)
            {
                return 0;
            }
            else
            {
                if (IsElevator())
                {
                    return 2;
                }
                else if (IsLava())
                {
                    return 1 + Param;
                }
            }
            throw new NotImplementedException();
        }
    }

    public int Height
    {
        get
        {
            if (Y < 13)
            {
                switch (this)
                {
                    case SideviewMapCommand<PalaceObject>:
                        return PalaceObjectExtensions.Height((this as SideviewMapCommand<PalaceObject>)!);
                    case SideviewMapCommand<GreatPalaceObject>:
                        return GreatPalaceObjectExtensions.Height((this as SideviewMapCommand<GreatPalaceObject>)!);
                    default:
                        throw new NotImplementedException();
                }
            }
            else if (Y < 15)
            {
                return 0;
            }
            else
            {
                if (IsElevator())
                {
                    return 13;
                }
                else if (IsLava())
                {
                    return 13;
                }
            }
            throw new NotImplementedException();
        }
    }

    public bool IsSolid
    {
        get
        {
            if (Y < 13)
            {
                switch (this)
                {
                    case SideviewMapCommand<PalaceObject>:
                        return PalaceObjectExtensions.IsSolid((this as SideviewMapCommand<PalaceObject>)!);
                    case SideviewMapCommand<GreatPalaceObject>:
                        return GreatPalaceObjectExtensions.IsSolid((this as SideviewMapCommand<GreatPalaceObject>)!);
                    default:
                        throw new NotImplementedException();
                }
            }
            else
            {
                return false;
            }
        }
    }

    public bool IsBreakable
    {
        get
        {
            if (Y < 13)
            {
                switch (this)
                {
                    case SideviewMapCommand<PalaceObject>:
                        return PalaceObjectExtensions.IsBreakable((this as SideviewMapCommand<PalaceObject>)!);
                    case SideviewMapCommand<GreatPalaceObject>:
                        return GreatPalaceObjectExtensions.IsBreakable((this as SideviewMapCommand<GreatPalaceObject>)!);
                    default:
                        throw new NotImplementedException();
                }
            }
            else
            {
                return false;
            }
        }
    }

    public bool IsPit
    {
        get
        {
            if (Y < 13)
            {
                switch (this)
                {
                    case SideviewMapCommand<PalaceObject>:
                        return PalaceObjectExtensions.IsPit((this as SideviewMapCommand<PalaceObject>)!);
                    case SideviewMapCommand<GreatPalaceObject>:
                        return GreatPalaceObjectExtensions.IsPit((this as SideviewMapCommand<GreatPalaceObject>)!);
                    default:
                        throw new NotImplementedException();
                }
            }
            else
            {
                return false;
            }
        }
    }

    public bool IsFloorSolidAt(int y)
    {
        Debug.Assert(IsNewFloor());
        var floorByte = Bytes[1];
        if ((floorByte & 0xf) == 0xf) { return true; } // xf = complete wall, always
        if ((floorByte & 0b10000000) == 0b10000000) // 8th bit == 1 means open sky
        {
            // the 2nd row is normally solid when the "floor" grows from the top,
            // but open sky overrides that row specifically
            // (Z2Edit is incorrect when growing from the top with no ceiling)
            if (y < 2) { return false; }
        }
        else
        {
            if (y == 0) { return true; }
        }
        if (y > 10) { return true; } // bottom 2 rows are always floor
        if ((floorByte & 0b1000) == 0) // 4th bit branches logic
        {
            return y > 10 - (floorByte & 0b0111);
        }
        else
        {
            return y < (floorByte & 0b0111) + 2;
        }
    }

    public bool Intersects(int x1, int x2, int y1, int y2)
    {
        if ((Y + Height <= y1) || Y > y2) { return false; }
        if ((AbsX + Width) <= x1 || AbsX > x2) { return false; }
        return true;
    }

    public String DebugString()
    {
        var bytes = Convert.ToHexString(Bytes);
        var idString = Enum.GetName(typeof(T), Id);
        if (IsNewFloor())
        {
            return $"{bytes,-6}  {AbsX,2}     {"NewFloor",-26}{Bytes[1]}";
        }
        if (IsXSkip())
        {
            return $"{bytes,-6}         XSkip";
        }
        if (IsLava()) { idString = "Lava"; }
        if (IsElevator()) { idString = "Elevator"; }
        var param = HasParam() ? $"{Param}" : "";
        if (HasExtra())
        {
            param = Enum.GetName(typeof(Collectable), Extra);
        }
        return $"{bytes,-6}  {AbsX,2},{Y,2}  {idString,-26}{param}";
    }
}
