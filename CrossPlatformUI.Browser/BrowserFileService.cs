using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices.JavaScript;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using CrossPlatformUI.Services;
using Newtonsoft.Json;

namespace CrossPlatformUI.Browser;

public partial class BrowserFileService : IFileSystemService
{
    // [JSImport("globalThis.customFetchString")]
    // [return: JSMarshalAs<JSType.Promise<JSType.String>>]
    // private static partial Task<string> FetchString(string url);
    // [JSImport("globalThis.customFetchBinary")]
    // [return: JSMarshalAs<JSType.Promise<JSType.String>>]
    // private static partial Task<string> FetchBinary(string url);
    
    [JSImport("globalThis.window.localStorage.setItem")]
    private static partial void SetItem(string key, string value);
    [JSImport("globalThis.window.localStorage.getItem")]
    private static partial string? GetItem(string key);
    [JSImport("globalThis.window.localStorage.clear")]
    private static partial void Clear();

    [JSImport("globalThis.window.FetchPalaces")]
    private static partial Task<string> FetchPalaces();
    
    [JSImport("globalThis.window.FetchPreloadedSprites")]
    [return: JSMarshalAs<JSType.Promise<JSType.String>>]
    private static partial Task<string> FetchPreloadedSprites();
    
    [JSImport("globalThis.window.DownloadFile")]
    private static partial void DownloadFile(string data, string name);

    private readonly Task<SpriteFile[]> preloadedSprites;
    private readonly Task<string> preloadedPalaces;

    private struct SpriteFile
    {
        public string Filename;
        public string Patch;
    }
    public BrowserFileService()
    {
        var tsk  = FetchPreloadedSprites();
        preloadedSprites = tsk.ContinueWith(task =>
        {
            var res = task.Result;
            return JsonConvert.DeserializeObject<SpriteFile[]>(res)!;
        });

        preloadedPalaces = FetchPalaces();
    }

    // This is terrible. But I'm not about to make a full filesystem abstraction
    private async Task<string> OpenFileInternal(IFileSystemService.RandomizerPath path, string filename)
    {
        switch (path)
        {
        case IFileSystemService.RandomizerPath.Sprites:
            var sprites = await preloadedSprites;
            foreach (var spr in sprites)
            {
                if (spr.Filename == filename)
                {
                    return spr.Patch;
                }
            }
            return "";
        case IFileSystemService.RandomizerPath.Palaces:
            return await preloadedPalaces;
        case IFileSystemService.RandomizerPath.Settings:
            return GetItem(filename) ?? "";
        default:
            throw new ArgumentOutOfRangeException(nameof(path), path, null);
        }
    }

    public async Task<string> OpenFile(IFileSystemService.RandomizerPath path, string filename)
    {
        return await OpenFileInternal(path, filename);
    }

    public string OpenFileSync(IFileSystemService.RandomizerPath path, string filename)
    {
        if (path == IFileSystemService.RandomizerPath.Settings)
        {
            return GetItem(filename) ?? "";
        }
        throw new NotImplementedException();
    }

    public async Task<byte[]> OpenBinaryFile(IFileSystemService.RandomizerPath path, string filename)
    {
        var res = await OpenFileInternal(path, filename);
        return Convert.FromBase64String(res);
    }

    public Task SaveFile(IFileSystemService.RandomizerPath path, string filename, string data)
    {
        if (path == IFileSystemService.RandomizerPath.Settings)
        {
            return Task.Run(() => SetItem(filename, data));
        }
        throw new NotImplementedException();
    }

    public async Task<IEnumerable<string>> ListLocalFiles(IFileSystemService.RandomizerPath path)
    {
        if (path == IFileSystemService.RandomizerPath.Sprites)
        {
            var sprites = await preloadedSprites;
            return sprites.Select(spr => spr.Filename);
        }
        throw new NotImplementedException();
    }

    public Task SaveGeneratedBinaryFile(string filename, byte[] filedata, string? path = null)
    {
        return Task.Run(() =>
        {
            var data = Convert.ToBase64String(filedata);
            DownloadFile(data, filename);
        });
    }
}