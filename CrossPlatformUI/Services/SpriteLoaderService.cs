using System;
using System.Collections.Generic;
using System.IO;
using System.Reactive.Linq;
using System.Threading.Tasks;
using Z2Randomizer.RandomizerCore;

namespace CrossPlatformUI.Services;

public class SpriteLoaderService
{
    private readonly IFileSystemService _fileService;

    public IObservable<IReadOnlyList<CharacterSprite>> Sprites { get; }

    public SpriteLoaderService(IFileSystemService fileSystemService)
    {
        _fileService = fileSystemService;
        Sprites = Observable.Defer(() => Observable.FromAsync(LoadSpritesAsync)).Replay(1).AutoConnect();
    }

    private async Task<IReadOnlyList<CharacterSprite>> LoadSpritesAsync()
    {
        List<CharacterSprite> options = new();

        options.Add(CharacterSprite.LINK);

        var spriteFiles = await _fileService.ListLocalFiles(IFileSystemService.RandomizerPath.Sprites);
        if (spriteFiles != null)
        {
            foreach (var spriteFile in spriteFiles)
            {
                var patch = await _fileService.OpenBinaryFile(IFileSystemService.RandomizerPath.Sprites, spriteFile);
                var parsedName = Path.GetFileNameWithoutExtension(spriteFile).Replace("_", " ");
                var charSprite = new CharacterSprite(parsedName, patch);
                options.Add(charSprite);
            }
        }

        options.Add(CharacterSprite.RANDOM);

        return options;
    }
}
