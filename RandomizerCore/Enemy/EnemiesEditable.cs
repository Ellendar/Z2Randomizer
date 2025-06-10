using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using Z2Randomizer.RandomizerCore.Sidescroll;

namespace Z2Randomizer.RandomizerCore.Enemy;

public class EnemiesEditable<T> where T : Enum
{
    private byte[] Header;

    public List<Enemy<T>> Enemies { get; set; }

    /// <summary>
    /// Create an EnemiesEditable from an array of bytes for the enemies of a room.
    /// </summary>
    /// <param name="bytes"><see cref="Room.Enemies"/> goes here.</param>
    public EnemiesEditable(byte[] bytes)
    {
        if (bytes.Length < 1) { throw new ArgumentException("Enemies data has no header."); }
        if (bytes.Length != bytes[0]) { throw new ArgumentException("Enemies data length byte is incorrect."); }
        Header = bytes[0..1];
        Enemies = new List<Enemy<T>>();
        int i = 1; // start after header
        while (i < bytes.Length)
        {
            if (i + 1 == bytes.Length) { throw new ArgumentException("Enemies data contains incomplete enemy bytes data."); }
            byte[] objectBytes = bytes[i..(i + 2)];
            Enemy<T> o = new(objectBytes);
            Enemies.Add(o);
            i += 2;
        }
    }

    /// <summary>
    /// Rebuild the enemies bytes with our updated list of enemies.
    /// </summary>
    /// <returns>The final bytes that represent our enemies list.</returns>
    public byte[] Finalize()
    {
        // should enemies be sorted?
        Enemies.Sort((a, b) =>
        {
            if (a.X != b.X)
            {
                return a.X.CompareTo(b.X);
            }
            else
            {
                return a.Y.CompareTo(b.Y);
            }
        });
        byte[] bytes = [
            .. Header,
            .. Enemies.SelectMany(o => o.Bytes)
        ];
        bytes[0] = (byte)bytes.Length;
        return bytes;
    }

    public void Mirror()
    {
        foreach (var enemy in Enemies)
        {
            enemy.X = 63 - enemy.X;
        }
    }

    public string DebugString()
    {
        StringBuilder sb = new StringBuilder("");
        var headerBytes = Convert.ToHexString(Header);
        sb.AppendLine($"{headerBytes}");
        foreach (var c in Enemies)
        {
            sb.AppendLine(c.DebugString());
        }
        return sb.ToString();
    }
}

public class Enemy<T> where T : Enum
{
    /// <summary>
    /// The bytes making up the enemy definition
    /// </summary>
    public byte[] Bytes { get; }

    /// <summary>
    /// Create from an array of bytes making up the enemy definition.
    /// </summary>
    /// <param name="data">The map command bytes. Must be 2 bytes long.</param>
    public Enemy(byte[] data)
    {
        Debug.Assert(data != null, "data cannot be null.");
        Debug.Assert(data.Length == 2, "data array must be 2 long.");
        Bytes = new byte[data.Length];
        Array.Copy(data, Bytes, data.Length);
    }

    /// <summary>
    /// Access the y position of the enemy, the first 4 bits.
    /// </summary>
    public int Y
    {
        get {
            int rawY = (Bytes[0] & 0xF0) >> 4;
            return rawY == 0 ? 1 : rawY + 2;
        }
        set {
            int rawY = value < 3 ? 0 : value - 2;
            Bytes[0] = (byte)(rawY << 4 | Bytes[0] & 0x0F);
        }
    }

    /// <summary>
    /// Access the x position of the enemy, which is determined by
    /// the first 2 bits of the second byte (page number), and
    /// the second 4 bits of the first byte.
    /// </summary>
    public int X
    {
        get => (Bytes[1] & 0b11000000) >> 2 | Bytes[0] & 0x0F;
        set {
            Bytes[1] = (byte)((value & 0b110000) << 2 | Bytes[1] & 0b00111111);
            Bytes[0] = (byte)(Bytes[0] & 0xF0 | value & 0x0F);
        }
    }

    public int Page
    {
        get => (Bytes[1] & 0b11000000) >> 6;
        set { Bytes[1] = (byte)((value & 0b11) << 6 | Bytes[1] & 0b00111111); }
    }

    /// <summary>
    /// Access the enemy ID as its enum representation
    /// </summary>
    public T Id
    {
        get => (T)Enum.ToObject(typeof(T), Bytes[1] & 0b00111111);
        set { Bytes[1] = (byte)(Bytes[1] & 0b11000000 | Convert.ToByte(value) & 0b00111111); }
    }

    /// <summary>
    /// Access the enemy ID as a byte
    /// </summary>
    public byte IdByte
    {
        get => (byte)(Bytes[1] & 0b00111111);
        set { Bytes[1] = (byte)(Bytes[1] & 0b11000000 | value & 0b00111111); }
    }

    public string DebugString()
    {
        var bytes = Convert.ToHexString(Bytes);
        var idString = $"{Id}";
        return $"{bytes,-4}  {X,2},{Y,2}  {idString}";
    }

    /// <summary>
    /// Is this a shufflable small enemy? EnemiesPalace125 and EnemiesPalace346 IDs may be mixed.
    /// </summary>
    public bool IsShufflableSmall()
    {
        switch (this)
        {
            case Enemy<EnemiesWest>:
                return Enemies.WestSmallEnemies.Any(e => e.Equals(Id));
            case Enemy<EnemiesEast>:
                return Enemies.EastSmallEnemies.Any(e => e.Equals(Id));
            case Enemy<EnemiesPalace125>:
                return Enemies.StandardPalaceSmallEnemies.Contains(IdByte);
            case Enemy<EnemiesPalace346>:
                return Enemies.StandardPalaceSmallEnemies.Contains(IdByte);
            case Enemy<EnemiesGreatPalace>:
                return Enemies.GPSmallEnemies.Any(e => e.Equals(Id));
            default:
                throw new NotImplementedException();
        }
    }

    /// <summary>
    /// Is this a shufflable large enemy? EnemiesPalace125 and EnemiesPalace346 IDs may be mixed.
    /// </summary>
    public bool IsShufflableLarge()
    {
        switch (this)
        {
            case Enemy<EnemiesWest>:
                return Enemies.WestLargeEnemies.Any(e => e.Equals(Id));
            case Enemy<EnemiesEast>:
                return Enemies.EastLargeEnemies.Any(e => e.Equals(Id));
            case Enemy<EnemiesPalace125>:
                return Enemies.StandardPalaceLargeEnemies.Contains(IdByte);
            case Enemy<EnemiesPalace346>:
                return Enemies.StandardPalaceLargeEnemies.Contains(IdByte);
            case Enemy<EnemiesGreatPalace>:
                return Enemies.GPLargeEnemies.Any(e => e.Equals(Id));
            default:
                throw new NotImplementedException();
        }
    }

    /// <summary>
    /// Is this a shufflable regular large or small enemy? EnemiesPalace125 and EnemiesPalace346 IDs may be mixed.
    /// </summary>
    public bool IsShufflableSmallOrLarge()
    {
        switch (this)
        {
            case Enemy<EnemiesWest>:
                return Enemies.WestGroundEnemies.Any(e => e.Equals(Id));
            case Enemy<EnemiesEast>:
                return Enemies.EastGroundEnemies.Any(e => e.Equals(Id));
            case Enemy<EnemiesPalace125>:
                return Enemies.StandardPalaceGroundEnemies.Contains(IdByte);
            case Enemy<EnemiesPalace346>:
                return Enemies.StandardPalaceGroundEnemies.Contains(IdByte);
            case Enemy<EnemiesGreatPalace>:
                return Enemies.GPGroundEnemies.Any(e => e.Equals(Id));
            default:
                throw new NotImplementedException();
        }
    }

    /// <summary>
    /// Is this a shufflable flying enemy? EnemiesPalace125 and EnemiesPalace346 IDs may be mixed.
    /// </summary>
    public bool IsShufflableFlying()
    {
        switch (this)
        {
            case Enemy<EnemiesWest>:
                return Enemies.WestFlyingEnemies.Any(e => e.Equals(Id));
            case Enemy<EnemiesEast>:
                return Enemies.EastFlyingEnemies.Any(e => e.Equals(Id));
            case Enemy<EnemiesPalace125>:
                return Enemies.StandardPalaceFlyingEnemies.Contains(IdByte);
            case Enemy<EnemiesPalace346>:
                return Enemies.StandardPalaceFlyingEnemies.Contains(IdByte);
            case Enemy<EnemiesGreatPalace>:
                return Enemies.GPFlyingEnemies.Any(e => e.Equals(Id));
            default:
                throw new NotImplementedException();
        }
    }

    /// <summary>
    /// Is this a shufflable enemy generator? EnemiesPalace125 and EnemiesPalace346 IDs may be mixed.
    /// </summary>
    public bool IsShufflableGenerator()
    {
        switch (this)
        {
            case Enemy<EnemiesWest>:
                return Enemies.WestGenerators.Any(e => e.Equals(Id));
            case Enemy<EnemiesEast>:
                return Enemies.EastGenerators.Any(e => e.Equals(Id));
            case Enemy<EnemiesPalace125>:
                return Enemies.StandardPalaceGenerators.Contains(IdByte);
            case Enemy<EnemiesPalace346>:
                return Enemies.StandardPalaceGenerators.Contains(IdByte);
            case Enemy<EnemiesGreatPalace>:
                return Enemies.GPGenerators.Any(e => e.Equals(Id));
            default:
                throw new NotImplementedException();
        }
    }
}
