
using System.Drawing.Imaging;
using RandomizerCore;

namespace WinFormUI.UI
{
    internal class SpritePreview
    {

        private byte[] _rom;

        public string? Credit { get; private set; }

        public Image? Preview { get; private set; }

        public SpritePreview(byte[] rawRom)
        {
            _rom = rawRom.ToArray();
        }
        public SpritePreview(string romPath)
        {
            _rom = File.ReadAllBytes(romPath);
        }

        private void LoadSpriteCredit(ROM rom)
        {
            // 0x10 iNES header, 0x16AAB = first empty line after the intro text scroll
            Credit = rom.Z2BytesToString(rom.GetBytes(0x10 + 0x16AAB, 0x1C)).Trim();
        }

        public void ReloadSpriteFromROM(CharacterSprite sprite, CharacterColor tunicColor, CharacterColor shieldColor, BeamSprites beamSprite)
        {
            // make a temporary copy of the original ROM file and apply the sprite to it
            var rom = new ROM(_rom.ToArray(), true);
            rom.UpdateSprites(sprite, tunicColor, shieldColor, beamSprite);

            // now the original rom has the sprite and palette data applied,
            // so load the specific CHR tiles and palette data that we want
            LoadPreviewFromRom(rom);
            LoadSpriteCredit(rom);
        }

        private void LoadPreviewFromRom(ROM rom)
        {
            var img = new Bitmap(16, 32, PixelFormat.Format32bppArgb);
            
            // Load the new palette for the sprite from the ROM
            var CHAR_PALETTE_ADDR = 0x10 + 0x1c46b;
            var palette = rom.GetBytes(CHAR_PALETTE_ADDR, 4);

            // 8 pixels in each 8x8 sprite :P
            var pixelsPerTileRow = 8;

            // 8 tile sprites in the main character data
            var tileCount = 8;

            // Location in the ROM where the sprite starts
            // 0x22000 = CHR page (bank $02) base, 0x10 = ines header, 0x80 for the tile offset
            // we are reading 8 sprites from [0x80, 0x100)
            var spriteBase = ROM.ChrRomOffs + 0x2000 + 0x80;

            // We only have one palette to read from, but should there be multipalette sprites someday
            // this could come in handy
            var paletteIdx = 0;

            // TODO jroweboy:
            // Its late at night, and I'm too tired to math this out properly
            int[] xpattern = new int[8] { 0, 0, 1, 1, 0, 0, 1, 1 };
            int[] ypattern = new int[8] { 0, 1, 0, 1, 2, 3, 2, 3 };
            for (var n = 0; n < tileCount; ++n)
            {
                var offset = n * 16;

                int tilex = xpattern[n] * 8;
                int tiley = ypattern[n] * 8;
                for (var j = 0; j < pixelsPerTileRow; ++j)
                {
                    var plane0 = rom.GetByte(spriteBase + offset + j);
                    var plane1 = rom.GetByte(spriteBase + offset + j + 8);
                    for (var i = 0; i < pixelsPerTileRow; ++i)
                    {
                        var pixelbit = 7 - i;
                        var bit0 = (plane0 >> pixelbit) & 1;
                        var bit1 = ((plane1 >> pixelbit) & 1) << 1;
                        var color = (bit0 | bit1) + (paletteIdx * 4);
                        var appliedColor = baseColors[palette[color]];
                        var withAlpha = Color.FromArgb((color == 0) ? 0 : 255, appliedColor);

                        var x = tilex + i;
                        var y = tiley + j;
                        img.SetPixel(x, y, withAlpha);
                    }
                }
            }

            Preview = EnlargeImage(img, 6);
        }
        private static Image EnlargeImage(Image original, int scale)
        {
            Bitmap newimg = new Bitmap(original.Width * scale, original.Height * scale);

            using (Graphics g = Graphics.FromImage(newimg))
            {
                // Here you set your interpolation mode
                g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.NearestNeighbor;

                // Scale the image, by drawing it on the larger bitmap
                g.DrawImage(original, new Rectangle(Point.Empty, newimg.Size));
            }

            return newimg;
        }

        // Basic look up table to convert from the original NES palette value to RGB
        private static readonly Color[] baseColors = new Color[]
        {
  Color.FromArgb( 84,  84,  84), Color.FromArgb(  0,  30, 116), Color.FromArgb(  8,  16, 144), Color.FromArgb( 48,   0, 136), Color.FromArgb( 68,   0, 100), Color.FromArgb( 92,   0,  48), Color.FromArgb( 84,   4,   0), Color.FromArgb( 60,  24,   0), Color.FromArgb( 32,  42,   0), Color.FromArgb(  8,  58,   0), Color.FromArgb(  0,  64,   0), Color.FromArgb(  0,  60,   0), Color.FromArgb(  0,  50,  60), Color.FromArgb(  0,   0,   0), Color.FromArgb(  0,   0,   0), Color.FromArgb(  0,   0,   0),
  Color.FromArgb(152, 150, 152), Color.FromArgb(  8,  76, 196), Color.FromArgb( 48,  50, 236), Color.FromArgb( 92,  30, 228), Color.FromArgb(136,  20, 176), Color.FromArgb(160,  20, 100), Color.FromArgb(152,  34,  32), Color.FromArgb(120,  60,   0), Color.FromArgb( 84,  90,   0), Color.FromArgb( 40, 114,   0), Color.FromArgb(  8, 124,   0), Color.FromArgb(  0, 118,  40), Color.FromArgb(  0, 102, 120), Color.FromArgb(  0,   0,   0), Color.FromArgb(  0,   0,   0), Color.FromArgb(  0,   0,   0),
  Color.FromArgb(236, 238, 236), Color.FromArgb( 76, 154, 236), Color.FromArgb(120, 124, 236), Color.FromArgb(176,  98, 236), Color.FromArgb(228,  84, 236), Color.FromArgb(236,  88, 180), Color.FromArgb(236, 106, 100), Color.FromArgb(212, 136,  32), Color.FromArgb(160, 170,   0), Color.FromArgb(116, 196,   0), Color.FromArgb( 76, 208,  32), Color.FromArgb( 56, 204, 108), Color.FromArgb( 56, 180, 204), Color.FromArgb( 60,  60,  60), Color.FromArgb(  0,   0,   0), Color.FromArgb(  0,   0,   0),
  Color.FromArgb(236, 238, 236), Color.FromArgb(168, 204, 236), Color.FromArgb(188, 188, 236), Color.FromArgb(212, 178, 236), Color.FromArgb(236, 174, 236), Color.FromArgb(236, 174, 212), Color.FromArgb(236, 180, 176), Color.FromArgb(228, 196, 144), Color.FromArgb(204, 210, 120), Color.FromArgb(180, 222, 120), Color.FromArgb(168, 226, 144), Color.FromArgb(152, 226, 180), Color.FromArgb(160, 214, 228), Color.FromArgb(160, 162, 160), Color.FromArgb(  0,   0,   0), Color.FromArgb(  0,   0,   0),
        };
    }
}
