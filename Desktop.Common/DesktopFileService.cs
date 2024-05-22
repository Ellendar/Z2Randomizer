
using CrossPlatformUI.Services;

namespace Desktop.Common;

public class DesktopFileService : IFileSystemService
{
    private string SpriteBasePath { get; init; }
    private string SettingsBasePath { get; init; }
    private string PalacesBasePath { get; init; }
    private string OSBasePath { get; init; }

    public DesktopFileService()
    {
        if (OperatingSystem.IsWindows())
        {
            OSBasePath = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
        } 
        else if (OperatingSystem.IsMacOS() && Environment.GetEnvironmentVariable("HOME") is not null)
        {
            OSBasePath = Path.Combine(Environment.GetEnvironmentVariable("HOME")!,
                                      "Library", "Application Support");
        }
        else if (OperatingSystem.IsLinux() && Environment.GetEnvironmentVariable("HOME") is not null)
        {
            OSBasePath = Path.Combine(Environment.GetEnvironmentVariable("HOME")!, ".local", "share");
        }
        else
        {
            throw new NotImplementedException();
        }
        SettingsBasePath = Path.Combine(OSBasePath, "Z2Randomizer");
        SpriteBasePath = Path.Combine(SettingsBasePath, "Sprites");
        PalacesBasePath = Path.GetDirectoryName(AppContext.BaseDirectory)! + "/";
        Directory.CreateDirectory(SpriteBasePath);
        Directory.CreateDirectory(SettingsBasePath);
        Directory.CreateDirectory(PalacesBasePath);
    }

    private string FullPath(IFileSystemService.RandomizerPath path, string filename) =>
        path switch
        {
            IFileSystemService.RandomizerPath.Sprites => Path.Combine(SpriteBasePath, filename),
            IFileSystemService.RandomizerPath.Settings => Path.Combine(SettingsBasePath, filename),
            IFileSystemService.RandomizerPath.Palaces => Path.Combine(PalacesBasePath, filename),
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
