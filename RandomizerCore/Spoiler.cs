using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.InteropServices;
using Z2Randomizer.RandomizerCore.Overworld;

namespace Z2Randomizer.RandomizerCore;

public class Spoiler
{
    private ROM rom;
    private byte[] itemPalette;
    public Dictionary<Terrain, SKBitmap> terrainTiles;

    public Spoiler(ROM rom)
    {
        this.rom = rom;
        itemPalette = rom.GetBytes(Palettes.ORANGE, 4);
        itemPalette[0] = 0x00;

        Dictionary<Terrain, byte[]> palettes = new();
        terrainTiles = new();

        foreach (var kvp in CHR.TERRAIN_TILE_ADDRS)
        {
            Terrain t = kvp.Key;
            var chrAddr = kvp.Value;
            if (!palettes.TryGetValue(t, out var palette))
            {
                palette = rom.GetBytes(ROM.RomHdrSize + Palettes.TERRAIN_ADDRS[t], 4);
                palette[0] = 0x0f;
                palettes[t] = palette;
            }

            SKBitmap tileBitmap;
            switch(t)
            {
                case Terrain.CAVE:
                case Terrain.DESERT:
                case Terrain.GRASS:
                case Terrain.SWAMP:
                case Terrain.WALKABLEWATER:
                case Terrain.PREPLACED_WATER_WALKABLE:
                case Terrain.ROAD:
                case Terrain.LAVA:
                    // repeat 8x8 tile 4 times into a 16x16 tile
                    tileBitmap = LoadChrFillPattern(rom, chrAddr, 1, 1, 2, 2, palette);
                    break;
                case Terrain.PREPLACED_WATER:
                case Terrain.WATER:
                    palette = [.. palette[..3], 0x11]; // distinguish between walkable and unwalkable water
                    tileBitmap = LoadChrFillPattern(rom, chrAddr, 1, 1, 2, 2, palette);
                    break;
                case Terrain.BRIDGE:
                    // repeat 16x8 tile 2 times
                    tileBitmap = LoadChrFillPattern(rom, chrAddr, 1, 2, 2, 2, palette);
                    break;
                case Terrain.GRAVE:
                    // we need to combine the grave 8x16 tile and two 8x8 road tiles
                    byte[] tileData1 = rom.ReadSprite(ROM.ChrRomOffset + CHR.TERRAIN_TILE_ADDRS[Terrain.GRAVE], 1, 2, palette);
                    byte[] tileData2 = rom.ReadSprite(ROM.ChrRomOffset + CHR.TERRAIN_TILE_ADDRS[Terrain.ROAD], 1, 1, palette);
                    byte[] fullTileData = new byte[16 * 16 * 4];
                    InsertTile(fullTileData, 16, 16, tileData1, 8, 16, 0, 0);
                    InsertTile(fullTileData, 16, 16, tileData2, 8, 8, 8, 0);
                    InsertTile(fullTileData, 16, 16, tileData2, 8, 8, 8, 8);
                    tileBitmap = MakeSpriteBitmap(fullTileData, 16, 16);
                    break;
                default:
                    // the rest are normal 16x16 tiles
                    tileBitmap = LoadChr(rom, chrAddr, 2, 2, palette);
                    break;
            }
            terrainTiles[t] = tileBitmap;
        }
    }

    public byte[] CreateSpoilerImage(List<World> worlds)
    {
        int column1Width = Math.Max(worlds[0].MapColumns, worlds[1].MapColumns);
        int column2Width = Math.Max(worlds[2].MapColumns, worlds[3].MapColumns);
        int row1Height = Math.Max(worlds[0].MapRows, worlds[2].MapRows);
        int row2Height = Math.Max(worlds[1].MapRows, worlds[3].MapRows);
        int gapSize = 1;
        int tilesWide = column1Width + column2Width + gapSize;
        int tilesHigh = row1Height + row2Height + gapSize;
        int w = tilesWide * 16;
        int h = tilesHigh * 16;

        using SKBitmap bitmap = new SKBitmap(w, h, SKColorType.Rgba8888, SKAlphaType.Premul);
        using SKCanvas canvas = new SKCanvas(bitmap);
        canvas.Clear(new SKColor(0, 0, 0, 0));

        DrawWorld(canvas, worlds[0], 0, 0);
        DrawWorld(canvas, worlds[1], 0, row1Height + gapSize);
        DrawWorld(canvas, worlds[2], column1Width + gapSize, 0);
        DrawWorld(canvas, worlds[3], column1Width + gapSize, row1Height + gapSize);

        using var image = SKImage.FromBitmap(bitmap);
        using var data = image.Encode(SKEncodedImageFormat.Png, 100);
        byte[] returnData = data.ToArray();
        image.Dispose();
        canvas.Dispose();
        bitmap.Dispose();
        return returnData;
    }

    private void DrawWorld(SKCanvas canvas, World world, int startDrawX, int startDrawY)
    {
        int offsetX = (world is MazeIsland && world.biome.UsesVanillaMap()) ? 32 : 0;
        int width = ((world is DeathMountain || world is MazeIsland) && world.biome.UsesVanillaMap()) ? 32 : world.MapColumns;
        int height = ((world is DeathMountain || world is MazeIsland) && world.biome.UsesVanillaMap()) ? 48 : world.MapRows;

        var background = new SKPaint();
        background.Color = SKColors.Black;
        canvas.DrawRect(startDrawX * 16, startDrawY * 16, width * 16, height * 16, background);
        for (int y = 0; y < height; y++)
        {
            for (int x = offsetX; x < offsetX + width; x++)
            {
                Terrain t = world.map[y, x];
                var tile = terrainTiles[t];
                Debug.Assert(tile != null);
                canvas.DrawBitmap(tile, (startDrawX - offsetX) * 16 + x * 16, startDrawY * 16 + y * 16);
            }
        }

        int[,] itemCountAtPos = new int[world.MapColumns, world.MapRows];
        foreach (var loc in world.AllLocations)
        {
            foreach (var col in loc.Collectables)
            {
                var parts = CHR.COLLECTABLE_TILES[col];
                var itemCount = itemCountAtPos[loc.Xpos, loc.Y];
                DrawSpriteParts(rom, canvas, parts, (startDrawX - offsetX) * 16 + loc.Xpos * 16 + itemCount * 8, startDrawY * 16 + loc.Y * 16 + itemCount * 8, itemPalette);
                itemCountAtPos[loc.Xpos, loc.Y]++;
            }
        }
    }

    public static SKBitmap LoadChr(ROM rom, int chrAddr, int tilesWide, int tilesHigh, byte[] palette)
    {
        byte[] tileData = rom.ReadSprite(ROM.ChrRomOffset + chrAddr, tilesWide, tilesHigh, palette);
        return MakeSpriteBitmap(tileData, tilesWide * 8, tilesHigh * 8);
    }

    public static SKBitmap LoadChrTransparent(ROM rom, int chrAddr, int tilesWide, int tilesHigh, byte[] palette)
    {
        int stride = tilesWide * 8 * 4;
        byte[] tileData = rom!.ReadSprite(ROM.ChrRomOffset + chrAddr, tilesWide, tilesHigh, palette);
        for (var i = 3; i < tileData.Length; i += 4)
        {
            tileData[i] = 0xff;
        }
        return MakeSpriteBitmap(tileData, tilesWide * 8, tilesHigh * 8);
    }

    public static SKBitmap LoadChrFillPattern(ROM rom, int chrAddr,
                                              int tilesWide, int tilesHigh,
                                              int targetTileWidth, int targetTileHeight,
                                              byte[] palette)
    {
        const int tileSize = 8; // Each tile is 8x8 pixels
        const int colorDepthBytes = 4; // RGBA

        int srcWidth = tilesWide * tileSize;
        int srcHeight = tilesHigh * tileSize;
        byte[] originalTileData = rom.ReadSprite(ROM.ChrRomOffset + chrAddr, tilesWide, tilesHigh, palette);
        int fullWidth = targetTileWidth * tileSize;
        int fullHeight = targetTileHeight * tileSize;
        byte[] fullTileData = new byte[fullWidth * fullHeight * colorDepthBytes];

        for (int yTile = 0; yTile < targetTileHeight; yTile += tilesHigh)
        {
            for (int xTile = 0; xTile < targetTileWidth; xTile += tilesWide)
            {
                int offsetX = xTile * srcWidth;
                int offsetY = yTile * srcHeight;

                InsertTile(fullTileData, fullWidth, fullHeight,
                           originalTileData, srcWidth, srcHeight,
                           offsetX, offsetY);
            }
        }

        // Convert the final RGBA byte array into an SKBitmap
        return MakeSpriteBitmap(fullTileData, fullWidth, fullHeight);
    }

    /// <summary>
    /// Pastes a smaller RGBA image into a larger RGBA image at a specified offset.
    /// </summary>
    /// <param name="dest">Destination image byte array (RGBA)</param>
    /// <param name="destWidth">Width of the destination image in pixels</param>
    /// <param name="destHeight">Height of the destination image in pixels</param>
    /// <param name="src">Source image byte array (RGBA)</param>
    /// <param name="srcWidth">Width of the source image in pixels</param>
    /// <param name="srcHeight">Height of the source image in pixels</param>
    /// <param name="offsetX">X position in the destination image to start pasting</param>
    /// <param name="offsetY">Y position in the destination image to start pasting</param>
    public static void InsertTile(byte[] dest, int destWidth, int destHeight,
                                  byte[] src, int srcWidth, int srcHeight,
                                  int offsetX, int offsetY)
    {
        const int colorDepthBytes = 4; // RGBA

        for (int y = 0; y < srcHeight; y++)
        {
            int destY = y + offsetY;
            if (destY < 0 || destY >= destHeight) { continue; }

            for (int x = 0; x < srcWidth; x++)
            {
                int destX = x + offsetX;
                if (destX < 0 || destX >= destWidth) {  continue; }

                int destIndex = (destY * destWidth + destX) * colorDepthBytes;
                int srcIndex = (y * srcWidth + x) * colorDepthBytes;

                dest[destIndex] = src[srcIndex];
                dest[destIndex + 1] = src[srcIndex + 1];
                dest[destIndex + 2] = src[srcIndex + 2];
                dest[destIndex + 3] = src[srcIndex + 3];
            }
        }
    }

    public static void DrawSpriteParts(ROM rom, SKCanvas canvas, SpriteTile[] parts, int startX, int startY, byte[] palette)
    {
        foreach (var part in parts)
        {
            /*
            SKBitmap tile = part.Alpha ? LoadChrTransparent(rom, part.Addr, part.W, part.H, palette)
                                       : LoadChr(rom, part.Addr, part.W, part.H, palette);
            */
            SKBitmap tile = LoadChr(rom, part.Addr, part.W, part.H, palette);
            foreach (var p in part.Placement)
            {
                SKBitmap drawTile = p.FlipH ? FlipTileHorizontally(tile) : tile;
                canvas.DrawBitmap(drawTile, startX + p.X * 8, startY + p.Y * 8);
            }
        }
    }

    public static SKBitmap MakeSpriteBitmap(byte[] tileData, int w, int h)
    {
        SKBitmap tile = new SKBitmap(w, h, SKColorType.Rgba8888, SKAlphaType.Unpremul);
        GCHandle handle = GCHandle.Alloc(tileData, GCHandleType.Pinned);
        try
        {
            IntPtr ptr = handle.AddrOfPinnedObject();
            using (var pixmap = new SKPixmap(tile.Info, ptr, tile.Info.RowBytes))
            {
                tile.InstallPixels(pixmap);
            }
            // Copy to avoid problems with original memory being freed
            return tile.Copy();
        }
        finally { handle.Free(); }
    }

    public static SKBitmap FlipTileHorizontally(SKBitmap tile)
    {
        SKBitmap res = new SKBitmap(tile.Width, tile.Height);
        for (int x = 0; x < tile.Width; x++)
        {
            var mirrorX = tile.Width - x - 1;
            for (int y = 0; y < tile.Height; y++)
            {
                res.SetPixel(mirrorX, y, tile.GetPixel(x, y));
            }
        }
        return res;
    }
}
