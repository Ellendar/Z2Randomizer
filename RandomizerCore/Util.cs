using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Z2Randomizer.RandomizerCore.Overworld;
using Z2Randomizer.RandomizerCore.Sidescroll;

namespace Z2Randomizer.RandomizerCore;

public class Util
{
    private const int textEndByte = 0xFF;
    public static byte ReverseByte(byte b)
    {
        return (byte)(((b * 0x80200802ul) & 0x0884422110ul) * 0x0101010101ul >> 32);
    }

    public static byte[] ToGameText(string rawText, bool endByte = false)
    {
        char[] rawTextChars = rawText.ToUpper().ToCharArray();
        List<byte> output = new List<byte>();

        for (int i = 0; i < rawTextChars.Length; i++)
        {
            char c = rawTextChars[i];
            if (c >= '0' && c <= '9')
            { 
                output.Add((byte)(c + 0xA0));
            }
            else if (c >= 'A' && c <= 'Z')
            { 
                output.Add((byte)(c + 0x99));
            }
            else
            {
                switch (c)
                {
                    case '.': output.Add(0xcf); break;
                    case '/': output.Add(0xce); break;
                    case ',': output.Add(0x9c); break;
                    case '!': output.Add(0x36); break;
                    case '?': output.Add(0x34); break;
                    case '*': output.Add(0x32); break;
                    case ' ': output.Add(0xf4); break;
                    case '\n': output.Add(0xfd); break;
                    case '$': output.Add(0xfd); break;
                    case '-': output.Add(0xf6); break;
                    case '_': output.Add(0xc5); break;
                    default: break; // ignore unknown characters
                }
            }
        }
        if (endByte)
        {
            output.Add((byte)textEndByte);
        }

        return output.ToArray();
    }

    public static string FromGameText(IEnumerable<byte> bytes)
    {
        StringBuilder output = new StringBuilder();
        foreach (byte b in bytes)
        {
            if (b >= 0xD0 && b <= 0xD9)
            {
                output.Append((char)(b - 0xA0));
            }
            else if (b >= 0xDA && b <= 0xF3)
            {
                output.Append((char)(b - 0x99));
            }
            else
            {
                switch (b)
                {
                    case 0xcf: output.Append('.'); break;
                    case 0xce: output.Append('/'); break;
                    case 0x9c: output.Append(','); break;
                    case 0x36: output.Append('!'); break;
                    case 0x34: output.Append('?'); break;
                    case 0x32: output.Append('*'); break;
                    case 0xf4: output.Append(' '); break;
                    case 0xfd: output.Append('$'); break;
                    case 0xf6: output.Append('-'); break;
                    case 0xc5: output.Append('_'); break;
                    default: break; // ignore unknown bytes
                }
            }
        }

        return output.ToString();
    }

    public static void Swap(Location p1, Location p2)
    {
        //(p2.Continent, p1.Continent) = (p1.Continent, p2.Continent);
        (p2.Map, p1.Map) = (p1.Map, p2.Map);
        (p2.PalaceNumber, p1.PalaceNumber) = (p1.PalaceNumber, p2.PalaceNumber);

        (p2.ActualTown, p1.ActualTown) = (p1.ActualTown, p2.ActualTown);
        (p2.Collectables, p1.Collectables) = (p1.Collectables, p2.Collectables);
        (p2.Name, p1.Name) = (p1.Name, p2.Name);
    }

    public static string ByteArrayToHexString(byte[] bytes)
    {
        string hex = BitConverter.ToString(bytes);
        return hex.Replace("-", "");
    }

    /// <summary>
    /// Cross platform safe method for finding files that defaults to current working directory with a fallback of the directory
    /// that the executable is running from.
    /// </summary>
    /// <param name="fileName">The </param>
    /// <returns>Whether the file exists in the current working directory or in the directory of the executable.</returns>
    public static bool FileExists(string fileName)
    {
        bool exists = File.Exists(fileName);
        if (!exists)
        {
            string absoluteFilePath = FilePathFromAssemblyLocation(fileName);
            exists = File.Exists(absoluteFilePath);
        }

        return exists;
    }

    public static string ReadAllTextFromFile(string fileName)
    {
        string fileToLoad = fileName;
        if (!File.Exists(fileName))
        {
            fileToLoad = FilePathFromAssemblyLocation(fileName);
        }

        return File.ReadAllText(fileToLoad);
    }

    private static string FilePathFromAssemblyLocation(string fileName)
    {
        string executingDirectory = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) 
            ?? throw new ImpossibleException("Invalid executing assembly directory");

        return Path.Combine(executingDirectory, fileName);
    }

    /// <summary>
    /// Comparer where two byte arrays are equal iff their length is the same, and at each index, the arrays contain the same value
    /// </summary>
    public class StandardByteArrayEqualityComparer : IEqualityComparer<byte[]>
    {
        public bool Equals(byte[]? x, byte[]? y)
        {
            if(x == null)
            {
                return y == null;
            }
            if(y == null)
            {
                return false;
            }
            return x.Length == y.Length && x.SequenceEqual(y);
        }

        public int GetHashCode(byte[] obj)
        {
            int result = 17;
            foreach (var t in obj)
            {
                unchecked
                {
                    result = result * 23 + t;
                }
            }
            return result;
        }
    }

    public static IEqualityComparer<byte[]> byteArrayEqualityComparer = new StandardByteArrayEqualityComparer();

    public static string ReadResource(string path)
    {
        return Assembly.GetExecutingAssembly().ReadResource(path);
    }

    public static byte[] ReadBinaryResource(string path)
    {
        return Assembly.GetExecutingAssembly().ReadBinaryResource(path);
    }

    public static PalaceGrouping? AsPalaceGrouping(int? palaceNumber)
    {
        return palaceNumber switch
        {
            1 => PalaceGrouping.Palace125,
            2 => PalaceGrouping.Palace125,
            3 => PalaceGrouping.Palace346,
            4 => PalaceGrouping.Palace346,
            5 => PalaceGrouping.Palace125,
            6 => PalaceGrouping.Palace346,
            7 => PalaceGrouping.PalaceGp,
            _ => null
        };
    }

    /// <summary>
    /// Gets a PalaceGrouping based on the vanilla memory address of a room. Should only be used for actual vanilla rooms.
    /// </summary>
    /// <param name="address"></param>
    /// <returns></returns>
    /// <exception cref="Exception"></exception>
    public static PalaceGrouping GetPalaceGroupingByMemoryAddress(int address)
    {
        if(address >= 67371 && address <= 67619)
        {
            return PalaceGrouping.Palace125;
        }
        if (address >= 74248 && address <= 74496)
        {
            return PalaceGrouping.Palace346;
        }
        if (address >= 83755 && address <= 83971)
        {
            return PalaceGrouping.PalaceGp;
        }
        throw new Exception("Unrecognized memory range for PalaceGrouping assignment");
    }
}

public static class AssemblyExtensions
{
    public static string ReadResource(this Assembly assembly, string name)
    {
        // Format: "{Namespace}.{Folder}.{filename}.{Extension}"
        using var stream = assembly.GetManifestResourceStream(name);
        if(stream == null)
        {
            throw new Exception("Unable to read ManifestResourceStream: " + name);
        }
        using var reader = new StreamReader(stream);
        return reader.ReadToEnd();
    }
    public static async Task<string> ReadResourceAsync(this Assembly assembly, string name)
    {
        // Format: "{Namespace}.{Folder}.{filename}.{Extension}"
        await using var stream = assembly.GetManifestResourceStream(name)!;
        using StreamReader reader = new(stream);
        return await reader.ReadToEndAsync();
    }
    public static byte[] ReadBinaryResource(this Assembly assembly, string name)
    {
        // Format: "{Namespace}.{Folder}.{filename}.{Extension}"
        using var stream = assembly.GetManifestResourceStream(name);
        if(stream == null)
        {
            throw new Exception("Unable to read binary resource: " + name);
        }
        using var reader = new BinaryReader(stream);
        return reader.ReadBytes((int)stream.Length);
    }
}