using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using CrossPlatformUI.Services;

namespace CrossPlatformUI.Desktop;

public class DesktopFileService : IFileSystemService
{
    private string SpriteBasePath { get; init; }
    private string SettingsBasePath { get; init; }
    private string PalacesBasePath { get; init; }

    public DesktopFileService()
    {
        if (OperatingSystem.IsWindows())
        {
            SpriteBasePath = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) +
                             "/Z2Randomizer/Sprites/";
            SettingsBasePath = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) +
                               "/Z2Randomizer/";
            PalacesBasePath = Path.GetDirectoryName(AppContext.BaseDirectory)! + "/";
        } else // (OperatingSystem.IsMacOS())
        {
            // TODO
            SpriteBasePath = Path.GetDirectoryName(AppContext.BaseDirectory) + "/Sprites/";
            SettingsBasePath = Path.GetDirectoryName(AppContext.BaseDirectory)! + "/";
            PalacesBasePath = Path.GetDirectoryName(AppContext.BaseDirectory)! + "/";
        } 
        // else if (OperatingSystem.IsLinux())
        // {
        //     // TODO
        //     SpriteBasePath = "./";
        //     SettingsBasePath = "./";
        //     PalacesBasePath = "./";
        // }
        Directory.CreateDirectory(SpriteBasePath);
        Directory.CreateDirectory(SettingsBasePath);
        Directory.CreateDirectory(PalacesBasePath);
    }

    private string FullPath(IFileSystemService.RandomizerPath path, string filename) =>
        path switch
        {
            IFileSystemService.RandomizerPath.Sprites => SpriteBasePath + filename,
            IFileSystemService.RandomizerPath.Settings => SettingsBasePath + filename,
            IFileSystemService.RandomizerPath.Palaces => PalacesBasePath + filename,
            _ => throw new ArgumentOutOfRangeException(nameof(path), path, null)
        };
    public Task<string> OpenFile(IFileSystemService.RandomizerPath path, string filename)
    {
        return File.ReadAllTextAsync(FullPath(path, filename));
    }
    public string OpenFileSync(IFileSystemService.RandomizerPath path, string filename)
    {
        if (path == IFileSystemService.RandomizerPath.Settings)
        {
            return File.ReadAllText(FullPath(path, filename));
        }
        throw new NotImplementedException();
    }

    public Task<byte[]> OpenBinaryFile(IFileSystemService.RandomizerPath path, string filename)
    {
        return File.ReadAllBytesAsync(FullPath(path, filename));
    }

    public Task SaveFile(IFileSystemService.RandomizerPath path, string filename, string data)
    {
        return File.WriteAllTextAsync(FullPath(path, filename), data);
    }

    public Task<IEnumerable<string>> ListLocalFiles(IFileSystemService.RandomizerPath path)
    {
        return Task.FromResult(Directory.GetFiles(FullPath(path, "")).AsEnumerable());
    }

    public Task SaveGeneratedBinaryFile(string filename, byte[] filedata, string? path = null)
    {
        var file = Path.Join(path ?? "", filename);
        return File.WriteAllBytesAsync(file, filedata);
    }
}