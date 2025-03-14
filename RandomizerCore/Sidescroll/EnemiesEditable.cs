using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace RandomizerCore.Sidescroll;

public class EnemiesEditable
{
    private byte[] Header;

    public List<Enemy> Enemies { get; set; }

    /// <summary>
    /// Create an EnemiesEditable from an array of bytes for the enemies of a room.
    /// </summary>
    /// <param name="bytes"><see cref="Room.Enemies"/> goes here.</param>
    public EnemiesEditable(byte[] bytes)
    {
        if (bytes.Length < 1) { throw new ArgumentException("Enemies data has no header."); }
        if (bytes.Length != bytes[0]) { throw new ArgumentException("Enemies data length byte is incorrect."); }
        Header = bytes[0..1];
        Enemies = new List<Enemy>();
        int i = 1; // start after header
        while (i < bytes.Length)
        {
            if (i + 1 == bytes.Length) { throw new ArgumentException("Enemies data contains incomplete enemy bytes data."); }
            byte[] objectBytes = bytes[i..(i + 2)];
            Enemy o = new(objectBytes);
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

    public String DebugString()
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

public class Enemy
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
        get { return (Bytes[0] & 0xF0) >> 4; }
        set { Bytes[0] = (byte)((value << 4) + (Bytes[0] & 0x0F)); }
    }

    /// <summary>
    /// Access the x position of the enemy, which is determined by
    /// the first 2 bits of the second byte (page number), and
    /// the second 4 bits of the first byte.
    /// </summary>
    public int X
    {
        get { return ((Bytes[1] & 0b11000000) >> 2) + (Bytes[0] & 0x0F); }
        set {
            Bytes[1] = (byte)(((value & 0xF0) << 2) + (Bytes[1] & 0b00111111));
            Bytes[0] = (byte)((Bytes[0] & 0xF0) + (value & 0x0F));
        }
    }

    /// <summary>
    /// Access the enemy number
    /// </summary>
    public int Id
    {
        get { return Bytes[1] & 0b00111111; }
        set { Bytes[1] = (byte)((Bytes[1] & 0b11000000) + (value & 0b00111111)); }
    }

    public String DebugString()
    {
        var bytes = Convert.ToHexString(Bytes);
        var idString = $"Id";
        return $"{bytes,-4}  {X,2},{Y,2}  {idString}";
    }
}
