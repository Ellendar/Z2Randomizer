using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Reactive.Threading.Tasks;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using Avalonia.Threading;
using CrossPlatformUI.Services;
using DynamicData;
using DynamicData.Binding;
using Microsoft.Extensions.DependencyInjection;
using ReactiveUI;
using Z2Randomizer.RandomizerCore;

namespace CrossPlatformUI.ViewModels.Tabs;

public class SpritePreviewViewModel : ReactiveObject, IActivatableViewModel
{
    private CancellationTokenSource backgroundUpdateTask = new ();

    public string? SpriteName {
        get => Main.Config.SpriteName ?? "Link";
        set { Main.Config.SpriteName = value ?? "Link"; this.RaisePropertyChanged(); }
    }
    public bool ChangeItemSprites
    {
        get => Main.Config.ChangeItemSprites;
        set { Main.Config.ChangeItemSprites = value; this.RaisePropertyChanged(); }
    }
    public NesColor TunicColor
    {
        get => Main.Config.Tunic;
        set { Main.Config.Tunic = value; this.RaisePropertyChanged(); }
    }
    public NesColor SkinTone
    {
        get => Main.Config.SkinTone;
        set { Main.Config.SkinTone = value; this.RaisePropertyChanged(); }
    }
    public NesColor OutlineColor
    {
        get => Main.Config.TunicOutline;
        set { Main.Config.TunicOutline = value; this.RaisePropertyChanged(); }
    }
    public NesColor ShieldColor
    {
        get => Main.Config.ShieldTunic;
        set { Main.Config.ShieldTunic = value; this.RaisePropertyChanged(); }
    }
    public BeamSprites BeamSprite
    {
        get => Main.Config.BeamSprite;
        set { Main.Config.BeamSprite = value; this.RaisePropertyChanged(); }
    }

    public byte spriteTunicColor { get; private set; }
    public byte spriteSkinTone { get; private set; }
    public byte spriteOutlineColor { get; private set; }
    public byte spriteShieldColor { get; private set; }
    public byte SpriteTunicColor
    {
        get => spriteTunicColor;
        set { spriteTunicColor = value; this.RaisePropertyChanged(); }
    }
    public byte SpriteSkinTone
    {
        get => spriteSkinTone;
        set { spriteSkinTone = value; this.RaisePropertyChanged(); }
    }
    public byte SpriteOutlineColor
    {
        get => spriteOutlineColor;
        set { spriteOutlineColor = value; this.RaisePropertyChanged(); }
    }
    public byte SpriteShieldColor
    {
        get => spriteShieldColor;
        set { spriteShieldColor = value; this.RaisePropertyChanged(); }
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
            SpriteTunicColor = sprite.palette[3];
            SpriteSkinTone = sprite.palette[2];
            SpriteOutlineColor = sprite.palette[1];
            SpriteShieldColor = sprite.palette[0];
        }
    }

    private readonly SourceList<LoadedCharacterSprite> options = new();
    [JsonIgnore]
    public ObservableCollectionExtended<LoadedCharacterSprite> Options { get; } = new();

    private CompositeDisposable _disposables = new();

    [JsonIgnore]
    public ViewModelActivator Activator { get; }

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

        options
            .Connect()
            .Bind(Options)
            .Subscribe()
            .DisposeWith(_disposables);

        var spriteLoaderService = App.Current?.Services?.GetService<SpriteLoaderService>();
        Debug.Assert(spriteLoaderService != null);

        spriteLoaderService.Sprites
            .Where(sprites => sprites.Count > 0)
            .Take(1)
            .Subscribe(sprites =>
            {
                Dispatcher.UIThread.Post(() => LoadCharacterSprites(sprites), DispatcherPriority.Background);
            })
            .DisposeWith(_disposables);
    }

    ~SpritePreviewViewModel()
    {
        _disposables.Dispose();
    }

    internal void OnActivate(CompositeDisposable disposables)
    {
        IObservable<(string? DisplayName, NesColor, NesColor, NesColor)> settingsObservable = this.WhenAnyValue(
                x => x.Main.Config.Sprite,
                x => x.Main.Config.Tunic,
                x => x.Main.Config.SkinTone,
                x => x.Main.Config.TunicOutline
                // x => x.Main.Config.ShieldTunic,
                // x => x.Main.Config.BeamSprite,
            )
            .Select(t => (
                t.Item1?.DisplayName, // making selected Sprite a string compare instead of an object compare
                t.Item2,
                t.Item3,
                t.Item4
            ))
            .DistinctUntilChanged() // filter emits where nothing has changed
            .Throttle(TimeSpan.FromMilliseconds(20));

        var hasRomObservable = Main.RomFileViewModel.ObservableForProperty(x => x.HasRomData, false, false);
        var optionsObservable = options.Connect().ToCollection();

        Observable.CombineLatest(
            settingsObservable,
            hasRomObservable,
            optionsObservable,
            (settings, hasRom, options) => (settings, hasRom, options)
        )
            .Where(t => t.hasRom.Value && t.options.Count > 0)
            .Subscribe(t => {
                backgroundUpdateTask.CancelAsync().ToObservable().Subscribe(_ =>
                {
                    backgroundUpdateTask = new CancellationTokenSource();
                    Dispatcher.UIThread.Post(() => UpdateCharacterSprites(Main.RomFileViewModel.RomData, backgroundUpdateTask.Token), DispatcherPriority.Background);
                });
            })
            .DisposeWith(disposables);
    }

    void LoadCharacterSprites(IReadOnlyList<CharacterSprite> sprites)
    {
        List<LoadedCharacterSprite> optionsNew = new();

        foreach (var sprite in sprites)
        {
            var s = new LoadedCharacterSprite(sprite);
            optionsNew.Add(s);
        }
        options.Edit(inner =>
        {
            inner.Clear();
            inner.AddRange(optionsNew);
        });
    }

    async void UpdateCharacterSprites(byte[] romData, CancellationToken token)
    {
        var current = Options.FirstOrDefault(s => s.Name == Main.Config.Sprite.DisplayName);
        if (current != null)
        {
            await current.Update(romData, Main.Config.Tunic, Main.Config.SkinTone, Main.Config.TunicOutline, Main.Config.ShieldTunic, Main.Config.BeamSprite);
        }
        foreach (var sprite in Options)
        {
            if (sprite == current) { continue; }
            if (token.IsCancellationRequested) { return; }
            await sprite.Update(romData, Main.Config.Tunic, Main.Config.SkinTone, Main.Config.TunicOutline, Main.Config.ShieldTunic, Main.Config.BeamSprite);
        }
        Sprite = Options.FirstOrDefault(loaded => loaded.Name == SpriteName, Options[0]);
    }
}

public class LoadedCharacterSprite : ReactiveObject
{
    static int charPaletteAddr = 0x10 + 0x1c46b;

    public CharacterSprite Sprite { get; }
    public byte[] palette = [0, 0, 0, 0];
    public LoadedCharacterSprite(CharacterSprite spr)
    {
        Sprite = spr;
        Name = spr.DisplayName;
    }

    public async Task Update(byte[] romData, NesColor tunicColor, NesColor skinTone, NesColor outlineColor, NesColor shieldColor, BeamSprites beamSprite)
    {
        var tmp = new ROM(romData, true);
        // sanitizing will be slower, we don't need to do it for every sprite in the dropdown
        tmp.UpdateSprite(Sprite, false, false);
        palette = tmp.GetBytes(charPaletteAddr, 4);
        palette[0] = tmp.GetByte(ROM.LinkShieldPaletteAddr);
        tmp.UpdateSpritePalette(tunicColor, skinTone, outlineColor, shieldColor, beamSprite);

        var data = await LoadPreviewFromRom(tmp);
        unsafe
        {
            fixed (byte* b = data)
            {
                var bmp = new Bitmap(PixelFormat.Rgba8888, AlphaFormat.Premul, (nint)b, new PixelSize(16, 32), Vector.One, 16*4);
                Preview = bmp.CreateScaledBitmap(new PixelSize(16, 32), BitmapInterpolationMode.None);
            }
        }
        // 0x10 iNES header, 0x16AAB = first empty line after the intro text scroll
        var creditRaw = ROM.Z2BytesToString(tmp.GetBytes(0x10 + 0x16AAB, 0x1C)).Trim();
        var credit2Raw = ROM.Z2BytesToString(tmp.GetBytes(0x10 + 0x16AC8, 0x1C)).Trim();
        if (creditRaw.Length > 0 && !creditRaw.StartsWith("SPRITE BY"))
        {
            creditRaw = $"SPRITE BY {creditRaw}";
        }
        creditRaw = creditRaw.Trim();
        credit2Raw = credit2Raw.Trim();
        Credit = (creditRaw + "\n" + credit2Raw).Trim();
    }

    private Bitmap? preview;
    public Bitmap? Preview { get => preview; set => this.RaiseAndSetIfChanged(ref preview, value); }
    
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
            var palette = tmp.GetBytes(charPaletteAddr, 4);

            // Location in the ROM where the sprite starts
            // 0x22000 = CHR page (bank $02) base, 0x10 = ines header, 0x80 for the tile offset
            // we are reading 8 sprites from [0x80, 0x100)
            const int spriteBase = ROM.ChrRomOffset + 0x2000 + 0x80;

            return tmp.ReadSprite(spriteBase, 2, 4, palette);
        });
    }
}
