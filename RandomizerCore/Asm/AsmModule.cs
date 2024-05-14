using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.Serialization;
using System.Text;

namespace RandomizerCore.Asm;

[DebuggerDisplay("{" + nameof(GetDebuggerDisplay) + "(),nq}")]
[DataContract]
public class AsmModule
{
    [DataMember]
    public List<Dictionary<string, object>> Actions { get; } = [];
    
    public void Code(string asm, string name = "")
    {
        Actions.Add(new() {
            { "action", "code" },
            { "code", asm },
            { "name", name },
        });
    }

    public void Label(string lb)
    {
        Actions.Add(new() {
            { "action", "label" },
            { "label", lb },
        });
    }

    public void Byt(params byte[] bytes)
    {
        Actions.Add(new() {
            { "action", "byte" },
            { "bytes", bytes },
        });
    }
    public void Byt(params Dictionary<string, object>[] bytes)
    {
        Actions.Add(new() {
            { "action", "byte" },
            { "bytes", bytes },
        });
    }

    public void Word(params ushort[] words)
    {
        Actions.Add(new() {
            { "action", "word" },
            { "words", words },
        });
    }
    public void Word(params Dictionary<string, object>[] words)
    {
        Actions.Add(new() {
            { "action", "word" },
            { "words", words },
        });
    }

    public void Org(ushort addr, string name = "")
    {
        Actions.Add(new() {
            { "action", "org" },
            { "addr", addr },
            { "name", name },
        });
    }

    // Converts from file address space to CPU address space, just a helper function
    // since all the current addresses in the randomizer are in file address space
    public void RomOrg(int addr, string name = "")
    {
        // adjustment for the ines header
        int romaddr = addr - 0x10;
        byte segment = (byte)(romaddr / 0x4000);
        ushort cpuoffset = (ushort)(segment == 7 ? 0xc000 : 0x8000);
        ushort cpuaddr = (ushort)((romaddr % 0x4000) + cpuoffset);

        Actions.Add(new() {
            { "action", "org" },
            { "addr", cpuaddr },
            { "name", name },
        });
    }
    public void Segment(params string[] name)
    {
        Actions.Add(new() {
            { "action", "segment" },
            { "name", name },
        });
    }

    public void Reloc(string name = "")
    {
        Actions.Add(new()
        {
            { "action", "reloc" },
            { "name", name },
        });
    }

    public Dictionary<string, object> Symbol(string name)
    {
        // kinda jank, but instead of eating the overhead for creating this token,
        // just hardcode the symbol token
        return new Dictionary<string, object>
        {
            { "op", "sym" },
            { "sym", name },
        };
    }

    public void Export(string name)
    {
        Actions.Add(new()
        {
            { "action", "reloc" },
            { "name", name },
        });
    }

    public void RelocExportLabel(string name, params string[] segments)
    {
        if (segments.Length > 0)
        {
            Segment(segments);
        }
        Reloc();
        Label(name);
        Export(name);
    }

    // Assign defines a constant value or expression
    public void Assign(string name, int value)
    {
        Actions.Add(new() {
            { "action", "assign" },
            { "value", value },
            { "name", name },
        });
    }

    // Set defines a non-constant value (which can be redefined with a second set)
    public void Set(string name, int value)
    {
        Actions.Add(new() {
            { "action", "set" },
            { "value", value },
            { "name", name },
        });
    }

    public void Free(string segment, ushort startorg, ushort endorg) {
        if (endorg <= startorg) {
            throw new Exception($"Free called with bad range: Start {startorg:04X} End {endorg:04X}");
        }
        Segment(segment);
        Org(startorg);
        Actions.Add(new() {
            { "action", "free" },
            { "size", endorg - startorg },
        });
    }
    
    public string GetDebuggerDisplay()
    {
        StringBuilder sb = new StringBuilder();
        sb.Append('[');
        int count = 0;
        foreach (var dict in Actions) 
        {
            count++;
            sb.Append('{');
            int count2 = 0;
            foreach(KeyValuePair<string, object> property in dict)
            {
                sb.Append(property.Key + " : " + property.Value + ",");
                count2++;
            }
            if(count2 > 0)
            {
                sb.Remove(sb.Length - 1, 1);
            }
            sb.Append("},");
        }
        if (count > 0)
        {
            sb.Remove(sb.Length - 1, 1);
        }
        sb.Append(']');

        return sb.ToString();
    }
}
