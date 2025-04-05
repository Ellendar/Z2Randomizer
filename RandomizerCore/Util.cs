using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using RandomizerCore.Overworld;
using RandomizerCore.Sidescroll;

namespace RandomizerCore;

public class Util
{
    private const int textEndByte = 0xFF;
    public static byte ReverseByte(byte b)
    {
        return (byte)(((b * 0x80200802ul) & 0x0884422110ul) * 0x0101010101ul >> 32);
    }

    public static List<char> ToGameText(string rawText, bool endByte = false)
    {
        List<char> rawTextChars = rawText.ToUpper().ToCharArray().ToList();
        StringBuilder output = new StringBuilder();
        for (int i = 0; i < rawTextChars.Count; i++)
        {
            if (rawTextChars[i] >= '0' && rawTextChars[i] <= '9')
                output.Append((char)(rawTextChars[i] + 0xA0));
            else if (rawTextChars[i] >= 'A' && rawTextChars[i] <= 'Z')
                output.Append((char)(rawTextChars[i] + 0x99));
            else output.Append(rawTextChars[i] switch
            {
                '.' => (char)0xcf,
                '/' => (char)0xce,
                ',' => (char)0x9c,
                '!' => (char)0x36,
                '?' => (char)0x34,
                '*' => (char)0x32,
                ' ' => (char)0xf4,
                '\n' => (char)0xfd,
                '$' => (char)0xfd,
                '-' => (char)0xf6,
                '_' => (char)0xc5,
                _ => ""
            });
        }
        if (endByte)
        {
            output.Append((char)textEndByte);
        }

        return output.ToString().ToCharArray().ToList();
    }

    public static string FromGameText(IEnumerable<char> rawText)
    {
        StringBuilder output = new StringBuilder();
        foreach(char rawChar in rawText)
        {
            if (rawChar >= 0xD0 && rawChar <= 0xD9)
                output.Append((char)(rawChar - 0xA0));
            else if (rawChar >= 0xda && rawChar <= 0xF3)
                output.Append((char)(rawChar - 0x99));
            else output.Append((int)rawChar switch
            {
                0xcf => '.',
                0xce => '/',
                0x9c => ',',
                0x36 => '!',
                0x34 => '?',
                0x32 => '*',
                0xf4 => ' ',
                0xfd => '$',
                0xf6 => '-',
                0xc5 => '_',
                _ => ""
            });
        }

        return output.ToString();
    }
    public static void Swap(Location p1, Location p2)
    {
        //(p2.Continent, p1.Continent) = (p1.Continent, p2.Continent);
        (p2.Map, p1.Map) = (p1.Map, p2.Map);
        (p2.PalaceNumber, p1.PalaceNumber) = (p1.PalaceNumber, p2.PalaceNumber);

        (p2.ActualTown, p1.ActualTown) = (p1.ActualTown, p2.ActualTown);
        (p2.Collectable, p1.Collectable) = (p1.Collectable, p2.Collectable);
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

    public static PalaceGrouping AsPalaceGrouping(int? palaceNumber)
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
            _ => PalaceGrouping.Unintialized,
        };
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
