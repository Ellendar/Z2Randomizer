using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Reactive.Disposables;
using System.Reactive.Threading.Tasks;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using Avalonia.Threading;
using CrossPlatformUI.Services;
using Microsoft.Extensions.DependencyInjection;
using ReactiveUI;
using Z2Randomizer.RandomizerCore;

namespace CrossPlatformUI.ViewModels.Tabs;

public class SpritePreviewViewModel : ReactiveObject, IActivatableViewModel
{
    private CancellationTokenSource backgroundUpdateTask = new ();
    private CancellationTokenSource backgroundLoadTask = new ();

    private string spriteName = "Link";

    public string? SpriteName { 
        get { return spriteName; }
        set { spriteName = value ?? "Link"; Main.Config.SpriteName = value ?? "Link"; }
    }
    public bool ChangeItemSprites
    {
        get => Main.Config.ChangeItemSprites;
        set { Main.Config.ChangeItemSprites = value; this.RaisePropertyChanged(); }
    }
    public CharacterColor TunicColor
    {
        get => Main.Config.Tunic;
        set { Main.Config.Tunic = value; this.RaisePropertyChanged(); }
    }
    public CharacterColor OutlineColor
    {
        get => Main.Config.TunicOutline;
        set { Main.Config.TunicOutline = value; this.RaisePropertyChanged(); }
    }
    public CharacterColor ShieldColor
    {
        get => Main.Config.ShieldTunic;
        set { Main.Config.ShieldTunic = value; this.RaisePropertyChanged(); }
    }
    public BeamSprites BeamSprite
    {
        get => Main.Config.BeamSprite;
        set { Main.Config.BeamSprite = value; this.RaisePropertyChanged(); }
    }
    
    [JsonConstructor]
#pragma warning disable CS8618
    public SpritePreviewViewModel() {}
#pragma warning restore CS8618
    public SpritePreviewViewModel(MainViewModel main)
    {
        Main = main;
        SpriteName = main.Config.SpriteName;
        Activator = new();
        this.WhenActivated(OnActivate);
    }

    internal void OnActivate(CompositeDisposable disposable)
    {
        this.WhenAnyValue(
            x => x.Main.Config.Sprite,
            x => x.Main.Config.Tunic,
            x => x.Main.Config.TunicOutline,
            // x => x.Main.Config.ShieldTunic,
            // x => x.Main.Config.BeamSprite,
            x => x.Main.RomFileViewModel.HasRomData
        ).Subscribe(tuple => {
            var (_, _, _, hasRom) = tuple;
            if (hasRom)
            {
                backgroundUpdateTask.CancelAsync().ToObservable().Subscribe(_ =>
                {
                    backgroundUpdateTask = new CancellationTokenSource();
                    Dispatcher.UIThread.Post(() => UpdateCharacterSprites(backgroundUpdateTask.Token), DispatcherPriority.Background);
                });
            }
        });

        Main.RomFileViewModel.ObservableForProperty(x => x.HasRomData, false, false).Subscribe(x =>
        {
            if (x.Value)
            {
                backgroundLoadTask.CancelAsync().ToObservable().Subscribe(_ =>
                {
                    backgroundLoadTask = new CancellationTokenSource();
                    Dispatcher.UIThread.Post(() => LoadCharacterSprites(backgroundLoadTask.Token), DispatcherPriority.Background);
                });
            }
        });
        return;

        async void UpdateCharacterSprites(CancellationToken token)
        {
            // Load the selected sprite first so that one updates fastest
            var current = Options.FirstOrDefault(loaded => loaded.Name == Main.Config.Sprite.DisplayName);
            if (current != null)
                await current.Update(Main.Config.Tunic, Main.Config.TunicOutline, Main.Config.ShieldTunic, Main.Config.BeamSprite);
            foreach (var loaded in Options)
            {
                if (token.IsCancellationRequested)
                    return;
                await loaded.Update(Main.Config.Tunic, Main.Config.TunicOutline, Main.Config.ShieldTunic, Main.Config.BeamSprite);
            }
        }

        async void LoadCharacterSprites(CancellationToken token)
        {
            Options.Clear();
            var link = new LoadedCharacterSprite(Main.RomFileViewModel.RomData!, CharacterSprite.LINK);
            await link.Update(Main.Config.Tunic, Main.Config.TunicOutline, Main.Config.ShieldTunic, Main.Config.BeamSprite);
            if (token.IsCancellationRequested)
                return;
            Options.Add(link);
            var fileservice = App.Current?.Services?.GetService<IFileSystemService>()!;
            var files = await fileservice.ListLocalFiles(IFileSystemService.RandomizerPath.Sprites);
            var spriteFiles = files.Where(x => x.EndsWith(".ips")).ToList();
            foreach (var spriteFile in spriteFiles)
            {
                var patch = await fileservice.OpenBinaryFile(IFileSystemService.RandomizerPath.Sprites, spriteFile);
                var parsedName = Path.GetFileNameWithoutExtension(spriteFile).Replace("_", " ");
                var ch = new CharacterSprite(parsedName, patch);
                var loaded = new LoadedCharacterSprite(Main.RomFileViewModel.RomData!, ch);
                await loaded.Update(Main.Config.Tunic, Main.Config.TunicOutline, Main.Config.ShieldTunic, Main.Config.BeamSprite);
                if (token.IsCancellationRequested)
                    return;
                Options.Add(loaded);
            }
            if (token.IsCancellationRequested)
                return;
            Options.Add(new LoadedCharacterSprite(Main.RomFileViewModel.RomData!, CharacterSprite.RANDOM));
            
            // Select the sprite on load based on the name
            Sprite = Options.FirstOrDefault(loaded => loaded.Name == SpriteName, link);
        }
    }

    [JsonIgnore]
    public MainViewModel Main { get; }

    private LoadedCharacterSprite? sprite;
    [JsonIgnore]
    public LoadedCharacterSprite? Sprite
    {
        get => sprite;
        set
        {
            this.RaiseAndSetIfChanged(ref sprite, value);
            if (sprite == null) return;
            Main.Config.Sprite = sprite.Sprite;
            SpriteName = sprite.Name;
        }
    }

    private readonly ObservableCollection<LoadedCharacterSprite> options = [];
    [JsonIgnore]
    public ObservableCollection<LoadedCharacterSprite> Options => options;
    [JsonIgnore]
    public ViewModelActivator Activator { get; }
}

public class LoadedCharacterSprite : ReactiveObject
{
    private readonly byte[] rom;
    public CharacterSprite Sprite { get; }
    public LoadedCharacterSprite(byte[] raw, CharacterSprite spr)
    {
        rom = raw.ToArray();
        Sprite = spr;
        Name = spr.DisplayName;
    }

    public async Task Update(CharacterColor tunicColor, CharacterColor outlineColor, CharacterColor shieldColor, BeamSprites beamSprite)
    {
        var tmp = new ROM(rom, true);
        // sanitizing will be slower, we don't need to do it for every sprite in the dropdown
        tmp.UpdateSprites(Sprite, tunicColor, outlineColor, shieldColor, beamSprite, false, false);
        var data = await LoadPreviewFromRom(tmp);
        unsafe
        {
            fixed (byte* b = data)
            {
                var bmp = new Bitmap(PixelFormat.Rgba8888, AlphaFormat.Premul, (nint)b, new PixelSize(16, 32), Vector.One, 16*4);
                SmallPreview = bmp.CreateScaledBitmap(new PixelSize(16, 32), BitmapInterpolationMode.None);
                LargePreview = bmp.CreateScaledBitmap(new PixelSize(16 * 6, 32 * 6), BitmapInterpolationMode.None);
            }
        }
        // 0x10 iNES header, 0x16AAB = first empty line after the intro text scroll
        var creditRaw = tmp.Z2BytesToString(tmp.GetBytes(0x10 + 0x16AAB, 0x1C)).Trim();
        Credit = creditRaw;
        if (creditRaw.StartsWith("SPRITE BY"))
        {
            creditRaw = creditRaw["SPRITE BY".Length ..].Trim();
        }
        AuthorName = creditRaw;
    }

    private Bitmap? largePreview;
    public Bitmap? LargePreview { get => largePreview; set => this.RaiseAndSetIfChanged(ref largePreview, value); }
    
    private Bitmap? smallPreview;
    public Bitmap? SmallPreview { get => smallPreview; set => this.RaiseAndSetIfChanged(ref smallPreview, value); }
    
    private string? authorName;
    public string? AuthorName { get => authorName; set => this.RaiseAndSetIfChanged(ref authorName, value); }
    private string? credit;
    public string? Credit { get => credit; set => this.RaiseAndSetIfChanged(ref credit, value); }
    
    private string? name;
    public string? Name { get => name; set => this.RaiseAndSetIfChanged(ref name, value); }

    private static Task<byte[]> LoadPreviewFromRom(ROM tmp)
    {
        return Task.Run(() =>
        {

            // var img = new WriteableBitmap(new PixelSize(16, 32), Vector.One, PixelFormat.Rgba8888);

            // Load the new palette for the sprite from the ROM
            const int charPaletteAddr = 0x10 + 0x1c46b;
            var palette = tmp.GetBytes(charPaletteAddr, 4);

            // Location in the ROM where the sprite starts
            // 0x22000 = CHR page (bank $02) base, 0x10 = ines header, 0x80 for the tile offset
            // we are reading 8 sprites from [0x80, 0x100)
            const int spriteBase = ROM.ChrRomOffset + 0x2000 + 0x80;

            return tmp.ReadSprite(spriteBase, 2, 4, palette);
        });
    }
}