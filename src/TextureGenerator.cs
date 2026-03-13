using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using SkiaSharp;
using Vintagestory.API.Client;
using Vintagestory.API.Common;

namespace IceSkates
{
    /// <summary>
    /// Generates mottled, vanilla-style 32x32 textures for blades and straps
    /// at runtime via palette-swapped noise templates. Static PNGs in
    /// assets/iceskates/textures/item/ take priority when present.
    /// </summary>
    public static class TextureGenerator
    {
        private const int Size = 32;
        private const int NoiseSize = 16;

        #region Noise Templates (16x16, values 0-4)

        // Mottled metal — random hammered look, C2 most common
        private static readonly byte[] MetalNoise =
        {
            2,1,2,3,1,2,1,2,3,1,2,1,3,2,1,2,
            1,3,1,2,4,1,3,2,1,3,1,2,1,3,2,1,
            2,1,4,1,3,2,1,3,2,1,3,1,2,1,3,2,
            3,2,1,3,1,2,4,1,2,3,1,4,1,2,1,3,
            1,2,3,1,2,3,1,2,3,1,2,1,3,1,2,4,
            2,3,1,2,1,3,2,3,1,2,4,2,1,3,1,2,
            1,2,2,3,2,1,3,1,2,3,1,3,2,1,2,3,
            3,1,3,1,4,2,1,2,3,1,2,1,3,2,4,1,
            2,3,1,2,1,3,2,3,1,4,1,2,1,3,1,2,
            1,2,4,1,3,1,2,1,2,1,3,1,2,1,3,2,
            3,1,2,3,1,2,3,2,1,3,2,4,1,2,1,3,
            2,3,1,2,2,3,1,3,2,1,3,1,2,3,2,1,
            1,2,3,1,3,1,2,4,1,2,1,3,2,1,3,2,
            2,1,2,3,1,2,3,1,3,2,1,2,3,1,2,1,
            3,2,1,2,4,1,2,1,2,3,2,1,1,3,1,3,
            1,3,2,1,2,3,1,2,1,2,3,2,3,1,2,2,
        };

        // Bumpy/irregular rawhide — rough skin, mostly C2-C3
        private static readonly byte[] RawhideNoise =
        {
            2,3,2,2,3,2,3,2,2,3,2,2,3,2,3,2,
            3,2,3,3,2,3,2,3,3,2,3,3,2,3,2,3,
            2,2,3,2,4,2,3,2,2,4,2,2,3,2,2,4,
            3,3,2,3,2,3,2,3,2,2,3,2,2,3,3,2,
            2,3,2,2,3,2,3,2,3,2,2,4,2,3,2,3,
            3,2,4,2,2,3,2,3,2,3,2,2,3,2,3,2,
            2,2,2,3,2,2,4,2,2,2,3,2,2,4,2,2,
            3,3,2,2,3,2,2,3,3,2,2,3,2,2,3,3,
            2,3,3,2,2,3,2,2,4,2,3,2,3,2,2,4,
            3,2,2,3,2,2,3,2,2,3,2,4,2,3,2,2,
            2,2,3,2,4,2,2,3,2,2,3,2,2,2,3,2,
            3,3,2,3,2,3,2,2,3,2,2,3,3,2,2,3,
            2,2,4,2,2,3,2,4,2,3,2,2,4,2,3,2,
            3,2,2,3,2,2,3,2,3,2,3,2,2,3,2,3,
            2,3,2,2,3,2,2,3,2,2,4,2,2,2,4,2,
            3,2,3,2,2,3,2,2,3,2,2,3,2,3,2,3,
        };

        // Horizontal grain leather — smooth with striped bands
        private static readonly byte[] LeatherNoise =
        {
            2,2,2,3,2,2,2,3,2,2,3,2,2,2,3,2,
            1,1,2,1,1,1,2,1,1,2,1,1,1,2,1,1,
            3,3,2,3,3,3,2,3,3,2,3,3,3,2,3,3,
            2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,
            1,2,1,1,2,1,1,1,2,1,1,2,1,1,1,2,
            3,2,3,3,2,3,3,3,2,3,3,2,3,3,3,2,
            2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,
            2,3,2,2,3,2,2,2,3,2,2,3,2,2,2,3,
            1,1,1,2,1,1,1,2,1,1,2,1,1,1,2,1,
            2,2,3,2,2,2,3,2,2,3,2,2,2,3,2,2,
            2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,
            3,3,2,3,3,3,2,3,3,2,3,3,3,2,3,3,
            1,2,1,1,2,1,1,1,2,1,1,2,1,1,1,2,
            2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,
            3,2,3,3,2,3,3,2,3,3,2,3,3,2,3,2,
            2,2,1,2,2,2,1,2,2,1,2,2,2,1,2,2,
        };

        // Fuzzy fur — dark with dense clusters, mostly C1-C2
        private static readonly byte[] FurNoise =
        {
            1,2,1,1,2,1,1,2,1,1,1,2,1,1,2,1,
            2,1,0,1,1,2,1,1,2,1,0,1,2,1,1,2,
            1,1,2,1,2,1,2,1,1,2,1,1,1,2,1,1,
            2,1,1,2,1,1,0,2,1,1,2,1,2,1,0,2,
            1,2,1,1,2,1,1,1,2,1,1,2,1,1,1,1,
            1,1,2,1,1,2,1,2,1,2,1,1,0,2,1,2,
            2,1,1,0,1,1,2,1,1,1,2,1,1,1,2,1,
            1,2,1,1,2,1,1,2,1,2,1,0,2,1,1,2,
            1,1,2,1,1,2,1,1,2,1,1,1,1,2,1,1,
            2,1,1,2,0,1,2,1,1,2,1,2,1,1,0,1,
            1,2,1,1,1,2,1,2,1,1,2,1,2,1,1,2,
            1,1,0,2,1,1,2,1,2,1,1,2,1,2,1,1,
            2,1,1,1,2,1,1,2,1,1,0,1,1,1,2,1,
            1,2,1,2,1,2,1,1,2,1,1,2,1,0,1,2,
            1,1,2,1,1,1,2,1,1,2,1,1,2,1,1,1,
            2,1,1,2,1,0,1,2,1,1,2,1,1,2,0,1,
        };

        #endregion

        #region Palettes (0xRRGGBB, 5 colors: C0 darkest -> C4 brightest)

        private static readonly Dictionary<string, uint[]> MetalPalettes = new()
        {
            ["bone"]          = new uint[] { 0x8B7D60, 0xA89878, 0xC8B898, 0xE0D0B0, 0xF0E8D0 },
            ["copper"]        = new uint[] { 0x5C2810, 0x8B4A18, 0xB87333, 0xD89050, 0xE8A868 },
            ["tinbronze"]     = new uint[] { 0x584018, 0x7A5C28, 0xA88040, 0xC8A058, 0xD8B870 },
            ["bismuthbronze"] = new uint[] { 0x483818, 0x685828, 0x887840, 0xA89858, 0xC0B070 },
            ["blackbronze"]   = new uint[] { 0x181010, 0x302420, 0x483830, 0x584840, 0x685850 },
            ["iron"]          = new uint[] { 0x383840, 0x505860, 0x787880, 0x98A0A8, 0xB0B8C0 },
            ["blistersteel"]  = new uint[] { 0x303848, 0x485060, 0x687888, 0x8898A8, 0xA0B0C0 },
            ["meteoriciron"]  = new uint[] { 0x484030, 0x706848, 0x988860, 0xB0A078, 0xC8B890 },
            ["steel"]         = new uint[] { 0x404048, 0x606068, 0x888890, 0xA8A8B0, 0xC8C8D0 },
            ["silver"]        = new uint[] { 0x585860, 0x787880, 0xA0A0A8, 0xC0C0C8, 0xE0E0F0 },
            ["gold"]          = new uint[] { 0x705008, 0xA07810, 0xDAA520, 0xF0C830, 0xFFE040 },
        };

        private static readonly Dictionary<string, (uint[] Palette, byte[] Noise)> StrapData = new()
        {
            ["rawhide"] = (new uint[] { 0x5C4028, 0x7A5838, 0x988050, 0xB09868, 0xC8B080 }, RawhideNoise),
            ["leather"] = (new uint[] { 0x381C10, 0x583018, 0x784828, 0x906038, 0xA87848 }, LeatherNoise),
            ["fur"]     = (new uint[] { 0x181008, 0x302010, 0x483820, 0x604830, 0x786040 }, FurNoise),
        };

        #endregion

        public static void GenerateAndInject(ICoreAPI api)
        {
            foreach (var kvp in MetalPalettes)
            {
                var loc = new AssetLocation("iceskates", "textures/item/skateblade-" + kvp.Key + ".png");
                if (api.Assets.TryGet(loc) != null) continue;
                byte[] png = GeneratePng(MetalNoise, kvp.Value);
                api.Assets.Add(loc, new GeneratedAsset(png, loc));
            }

            foreach (var kvp in StrapData)
            {
                var loc = new AssetLocation("iceskates", "textures/item/skatestrap-" + kvp.Key + ".png");
                if (api.Assets.TryGet(loc) != null) continue;
                byte[] png = GeneratePng(kvp.Value.Noise, kvp.Value.Palette);
                api.Assets.Add(loc, new GeneratedAsset(png, loc));
            }
        }

        private static byte[] GeneratePng(byte[] noise, uint[] palette)
        {
            using var bmp = new SKBitmap(Size, Size, SKColorType.Rgba8888, SKAlphaType.Premul);
            for (int y = 0; y < Size; y++)
            {
                for (int x = 0; x < Size; x++)
                {
                    uint c = palette[noise[y * NoiseSize / Size * NoiseSize + x * NoiseSize / Size]];
                    byte r = (byte)((c >> 16) & 0xFF);
                    byte g = (byte)((c >> 8) & 0xFF);
                    byte b = (byte)(c & 0xFF);
                    bmp.SetPixel(x, y, new SKColor(r, g, b, 255));
                }
            }

            using var img = SKImage.FromBitmap(bmp);
            using var data = img.Encode(SKEncodedImageFormat.Png, 100);
            return data.ToArray();
        }

        /// <summary>
        /// Minimal IAsset implementation for runtime-generated PNG textures.
        /// </summary>
        private class GeneratedAsset : IAsset
        {
            public string Name { get; }
            public AssetLocation Location { get; }
            public IAssetOrigin Origin { get; set; }
            public byte[] Data { get; set; }
            public bool IsPatched { get; set; }

            public GeneratedAsset(byte[] data, AssetLocation location)
            {
                Data = data;
                Location = location;
                Name = System.IO.Path.GetFileName(location.Path);
            }

            public T ToObject<T>(JsonSerializerSettings settings = null)
                => throw new NotSupportedException("Generated texture asset has no JSON data");

            public string ToText()
                => throw new NotSupportedException("Generated texture asset has no text data");

            public BitmapRef ToBitmap(ICoreClientAPI capi)
            {
                return new BitmapExternal(SKBitmap.Decode(Data));
            }

            public bool IsLoaded() => Data != null;
        }
    }
}
