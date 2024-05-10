using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Data.Converters;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using CrossPlatformUI.Services;
using Microsoft.Extensions.DependencyInjection;
using ReactiveUI;
using Z2Randomizer.Core;
using Size = Avalonia.Size;

namespace CrossPlatformUI.ViewModels.Tabs;

[DataContract]
public class SpritePreviewViewModel : ReactiveObject
{
    public MainViewModel Main { get; init; }
    public SpritePreviewViewModel(MainViewModel mainViewModel)
    {
        Main = mainViewModel;

        // this.WhenAnyValue(
        //     x => x.Sprite,
        //     x => x.TunicColor,
        //     x => x.ShieldColor,
        //     x => x.BeamSprite,
        //     (characterSprite, tunic, shield, beam) =>
        //     {
        //         
        //     }
        // );
        
        Main.RomFileViewModel.ObservableForProperty(x => x.HasRomData, false, false).Subscribe(x =>
        {
            if (x.Value)
            {
                Task.Run(LoadCharacterSprites);
            }
        });
        return;

        async void LoadCharacterSprites()
        {
            Options.Add(new LoadedCharacterSprite(Main.RomFileViewModel.RomData!, null, CharacterSprite.LINK));
            var fileservice = App.Current?.Services?.GetService<IFileService>()!;
            var files = await fileservice.ListLocalFiles("Sprites");
            var spriteFiles = files.Where(x => x.EndsWith(".ips")).ToList();
            foreach (var spriteFile in spriteFiles)
            {
                var patch = await fileservice.OpenLocalBinaryFile(spriteFile);
                string parsedName = Path.GetFileNameWithoutExtension(spriteFile).Replace("_", " ");
                var ch = new CharacterSprite(parsedName, patch);
                Options.Add(new LoadedCharacterSprite(Main.RomFileViewModel.RomData!, patch, ch));
            }
        }
    }

    // public Bitmap Preview { get; private set; }

    // private string? credit;
    // public string? Credit { get => credit; set => this.RaiseAndSetIfChanged(ref credit, value); }

    private LoadedCharacterSprite sprite;
    public LoadedCharacterSprite Sprite { get => sprite; set => this.RaiseAndSetIfChanged(ref sprite, value); }

    private ObservableCollection<LoadedCharacterSprite> options = new ();
    public ObservableCollection<LoadedCharacterSprite> Options { get => options; }

    private string tunicColor;
    public string TunicColor { get => tunicColor; set => this.RaiseAndSetIfChanged(ref tunicColor, value); }
    private string shieldColor;
    public string ShieldColor { get => shieldColor; set => this.RaiseAndSetIfChanged(ref shieldColor, value); }
    private string beamSprite;
    public string BeamSprite { get => beamSprite; set => this.RaiseAndSetIfChanged(ref beamSprite, value); }

    public static SpriteConverter SpriteConvert { get; } = new ();
    
}

public class SpriteConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is CharacterSprite)
        {
            
        }

        return null;
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}

public class LoadedCharacterSprite : ReactiveObject
{
    private readonly byte[] rom;
    private readonly byte[]? ips;
    private readonly CharacterSprite sprite;
    public LoadedCharacterSprite(byte[] raw, byte[]? patch, CharacterSprite spr)
    {
        rom = raw.ToArray();
        ips = patch;
        sprite = spr;
        Name = spr.DisplayName;
        Update("Default", "Default", "Default");
    }

    public unsafe void Update(string tunicColor, string shieldColor, string beamSprite)
    {
        var tmp = new ROM(rom);
        tmp.UpdateSprites(sprite, tunicColor, shieldColor, beamSprite);
        var data = LoadPreviewFromRom(tmp);
        
        fixed (byte* b = data)
        {
            var bmp = new Bitmap(PixelFormat.Rgba8888, AlphaFormat.Premul, (nint)b, new PixelSize(16, 32), Vector.One, 16);
            SmallPreview = bmp.CreateScaledBitmap(new PixelSize(16, 32), BitmapInterpolationMode.None);
            LargePreview = bmp.CreateScaledBitmap(new PixelSize(16 * 3, 32 * 3), BitmapInterpolationMode.None);
        }
        // 0x10 iNES header, 0x16AAB = first empty line after the intro text scroll
        Credit = tmp.Z2BytesToString(tmp.GetBytes(0x10 + 0x16AAB, 0x10 + 0x16AC7)).Trim();
    }

    private Bitmap largePreview;
    public Bitmap LargePreview { get => largePreview; set => this.RaiseAndSetIfChanged(ref largePreview, value); }
    
    private Bitmap smallPreview;
    public Bitmap SmallPreview { get => smallPreview; set => this.RaiseAndSetIfChanged(ref smallPreview, value); }
    
    private string credit;
    public string Credit { get => credit; set => this.RaiseAndSetIfChanged(ref credit, value); }
    
    private string name;
    public string Name { get => name; set => this.RaiseAndSetIfChanged(ref name, value); }

    private static byte[] LoadPreviewFromRom(ROM tmp)
    {
        // var img = new WriteableBitmap(new PixelSize(16, 32), Vector.One, PixelFormat.Rgba8888);
    
        // Load the new palette for the sprite from the ROM
        const int charPaletteAddr = 0x10 + 0x1c46b;
        var palette = tmp.GetBytes(charPaletteAddr, charPaletteAddr + 4);

        // 8 pixels in each 8x8 sprite :P
        var pixelsPerTileRow = 8;

        // 8 tile sprites in the main character data
        const int tileCount = 8;

        // Location in the ROM where the sprite starts
        // 0x22000 = CHR page (bank $02) base, 0x10 = ines header, 0x80 for the tile offset
        // we are reading 8 sprites from [0x80, 0x100)
        const int spriteBase = 0x22000 + 0x10 + 0x80;

        // We only have one palette to read from, but should there be multipalette sprites someday
        // this could come in handy
        const int paletteIdx = 0;

        // TODO jroweboy:
        // Its late at night, and I'm too tired to math this out properly
        int[] xpattern = [ 0, 0, 1, 1, 0, 0, 1, 1 ];
        int[] ypattern = [ 0, 1, 0, 1, 2, 3, 2, 3 ];

        // using var framebuffer = img.Lock();
        // var buffer = new Span<byte>(framebuffer.Address.ToPointer(), 16 * 32 * 4); // assume each pixel is 1 byte in size and my image has 4 channels
        var buffer = new byte[4 * 16 * 32];
        for (var n = 0; n < tileCount; ++n)
        {
            var offset = n * 16;

            int tilex = xpattern[n] * 8;
            int tiley = ypattern[n] * 8;
            for (var j = 0; j < pixelsPerTileRow; ++j)
            {
                var plane0 = tmp.GetByte(spriteBase + offset + j);
                var plane1 = tmp.GetByte(spriteBase + offset + j + 8);
                for (var i = 0; i < pixelsPerTileRow; ++i)
                {
                    var pixelbit = 7 - i;
                    var bit0 = (plane0 >> pixelbit) & 1;
                    var bit1 = ((plane1 >> pixelbit) & 1) << 1;
                    var color = (bit0 | bit1) + (paletteIdx * 4);
                    var appliedColor = BaseColors[palette[color]];
                    var withAlpha = Color.FromArgb((color == 0) ? 0 : 255, appliedColor);

                    var x = tilex + i;
                    var y = tiley + j;
                    SetPixel(buffer, x, y, withAlpha);
                }
            }
        }

        // Preview = EnlargeImage(img, 6);
        // Preview = img.CreateScaledBitmap(new PixelSize(16 * 6, 32 * 6), BitmapInterpolationMode.None);
        return buffer;
    }

    private static void SetPixel(Span<byte> buffer, int x, int y, Color c)
    {
        buffer[0 + 4 * x + 4 * 16 * y] = c.R;
        buffer[1 + 4 * x + 4 * 16 * y] = c.G;
        buffer[2 + 4 * x + 4 * 16 * y] = c.B;
        buffer[3 + 4 * x + 4 * 16 * y] = c.A;
    }

    // Basic look up table to convert from the original NES palette value to RGB
    private static readonly Color[] BaseColors = new Color[]
    {
Color.FromArgb( 84,  84,  84), Color.FromArgb(  0,  30, 116), Color.FromArgb(  8,  16, 144), Color.FromArgb( 48,   0, 136), Color.FromArgb( 68,   0, 100), Color.FromArgb( 92,   0,  48), Color.FromArgb( 84,   4,   0), Color.FromArgb( 60,  24,   0), Color.FromArgb( 32,  42,   0), Color.FromArgb(  8,  58,   0), Color.FromArgb(  0,  64,   0), Color.FromArgb(  0,  60,   0), Color.FromArgb(  0,  50,  60), Color.FromArgb(  0,   0,   0), Color.FromArgb(  0,   0,   0), Color.FromArgb(  0,   0,   0),
Color.FromArgb(152, 150, 152), Color.FromArgb(  8,  76, 196), Color.FromArgb( 48,  50, 236), Color.FromArgb( 92,  30, 228), Color.FromArgb(136,  20, 176), Color.FromArgb(160,  20, 100), Color.FromArgb(152,  34,  32), Color.FromArgb(120,  60,   0), Color.FromArgb( 84,  90,   0), Color.FromArgb( 40, 114,   0), Color.FromArgb(  8, 124,   0), Color.FromArgb(  0, 118,  40), Color.FromArgb(  0, 102, 120), Color.FromArgb(  0,   0,   0), Color.FromArgb(  0,   0,   0), Color.FromArgb(  0,   0,   0),
Color.FromArgb(236, 238, 236), Color.FromArgb( 76, 154, 236), Color.FromArgb(120, 124, 236), Color.FromArgb(176,  98, 236), Color.FromArgb(228,  84, 236), Color.FromArgb(236,  88, 180), Color.FromArgb(236, 106, 100), Color.FromArgb(212, 136,  32), Color.FromArgb(160, 170,   0), Color.FromArgb(116, 196,   0), Color.FromArgb( 76, 208,  32), Color.FromArgb( 56, 204, 108), Color.FromArgb( 56, 180, 204), Color.FromArgb( 60,  60,  60), Color.FromArgb(  0,   0,   0), Color.FromArgb(  0,   0,   0),
Color.FromArgb(236, 238, 236), Color.FromArgb(168, 204, 236), Color.FromArgb(188, 188, 236), Color.FromArgb(212, 178, 236), Color.FromArgb(236, 174, 236), Color.FromArgb(236, 174, 212), Color.FromArgb(236, 180, 176), Color.FromArgb(228, 196, 144), Color.FromArgb(204, 210, 120), Color.FromArgb(180, 222, 120), Color.FromArgb(168, 226, 144), Color.FromArgb(152, 226, 180), Color.FromArgb(160, 214, 228), Color.FromArgb(160, 162, 160), Color.FromArgb(  0,   0,   0), Color.FromArgb(  0,   0,   0),
    };

}