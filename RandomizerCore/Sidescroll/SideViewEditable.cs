using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace RandomizerCore.Sidescroll;

//TODO:Restructure this as a Sideview class that understands how the format actually works
//has properties that describe things about it, and can answer questions about the net result of the format
//then replace all instances of the Sideview byte[] with references to that class.
//This will allow us to ask questions of it like "does this room contain a key door"
//or "can you get from the left to right side of this room without fairy"
//these will be very useful for the future (as well as just being easier to read)

/// <summary>
/// Holds a list of <see cref="SideViewMapCommand"/>s in order to allow programmatically editing a room. 
/// 
/// <see cref="Finalize"/> must be called after editing to retrieve the updated bytes for the room.
/// </summary>
class SideviewEditable
{
    private byte[] header;
    public List<SideViewMapCommand> Commands { get; set; } 

    /// <summary>
    /// Create a SideViewEditable from an array of bytes for a side view.
    /// </summary>
    /// <param name="bytes"><see cref="Room.SideView"/> goes here.</param>
    public SideviewEditable(byte[] bytes)
    {
        if(bytes.Length < 4) 
        {
            throw new Exception("SideView data has no header.");
        }
        header = bytes[0..4];
        Commands = new List<SideViewMapCommand>();
        int i = 4; // start after header
        int xcursor = 0;
        while (i < bytes.Length)
        {
            byte[] objectBytes;
            byte firstByte = bytes[i++];
            Debug.Assert(i < bytes.Length, "SideView data contains incomplete map command.");
            byte secondByte = bytes[i++];
            int ypos = (firstByte & 0xF0) >> 4;
            if (secondByte == 15 && ypos < 13) // 3 byte object found
            {
                Debug.Assert(i < bytes.Length, "SideView data contains incomplete map command.");
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
            SideViewMapCommand o = new(objectBytes);
            o.AbsX = xcursor;
            Commands.Add(o);
        }
    }

    public SideViewMapCommand? Find(Predicate<SideViewMapCommand> match)
    {
        return Commands.Find(match);
    }

    public void Add(SideViewMapCommand command)
    {
        Commands.Add(command);
    }

    public void Remove(SideViewMapCommand item)
    {
        Commands.Remove(item);
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
            SideViewMapCommand o = Commands[i];
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
            SideViewMapCommand o = Commands[i];
            if (xCursor + o.RelX != o.AbsX)
            {
                int xDiff = o.AbsX - xCursor;
                if (xDiff > 15) // create new "x skip" command
                {
                    SideViewMapCommand xSkip = new SideViewMapCommand(xDiff / 16, 0xE, 0);
                    Commands.Insert(i, xSkip);
                    i++;
                    xDiff = xDiff & 0xF;
                }
                o.RelX = xDiff;
            }
            xCursor = o.AbsX;
            i++;
        }
        byte[] bytes = header.Concat(Commands.SelectMany(o => o.Bytes)).ToArray();
        bytes[0] = (byte)bytes.Length;
        return bytes;
    }
}
