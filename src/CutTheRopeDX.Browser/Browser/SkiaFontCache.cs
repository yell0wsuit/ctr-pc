using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

using CutTheRopeDX.Framework.Platform;
using CutTheRopeDX.Framework.Visual;
using CutTheRopeDX.GameMain;

using SkiaSharp;

namespace CutTheRopeDX.Browser
{
    /// <summary>Loads and caches the subset typefaces the game renders text with.</summary>
    internal static class SkiaFontCache
    {
        private static readonly Dictionary<string, SKTypeface> Typefaces = [];
        private static readonly Dictionary<string, SkiaFont> Fonts = [];

        /// <summary>Returns the content-relative path of a pipeline font file.</summary>
        /// <param name="fontFile">Font file name from <see cref="FontConfiguration.FontFile"/>.</param>
        public static string PathFor(string fontFile)
        {
            return $"fonts/{Path.GetFileNameWithoutExtension(fontFile)}.ttf";
        }

        /// <summary>Fetches a typeface so <see cref="Load"/> can resolve it synchronously.</summary>
        /// <param name="fontFile">Font file name from <see cref="FontConfiguration.FontFile"/>.</param>
        public static Task EnsureTypefaceResidentAsync(string fontFile)
        {
            return PlatformServices.Content.EnsureResidentAsync([PathFor(fontFile)]);
        }

        /// <summary>Returns the font for a configuration, building it on first use.</summary>
        /// <param name="config">Resolved font configuration for the current language.</param>
        public static FontGeneric Load(FontConfiguration config)
        {
            string key = $"{config.FontFile}|{config.Size}|{config.LineSpacing}|{config.TopSpacing}";
            if (Fonts.TryGetValue(key, out SkiaFont cached))
            {
                return cached;
            }

            SkiaFont font = new(GetTypeface(config.FontFile), config);
            Fonts[key] = font;
            return font;
        }

        /// <summary>Drops every cached font and typeface.</summary>
        public static void Clear()
        {
            foreach (SkiaFont font in Fonts.Values)
            {
                font.Dispose();
            }
            Fonts.Clear();

            foreach (SKTypeface typeface in Typefaces.Values)
            {
                typeface.Dispose();
            }
            Typefaces.Clear();
        }

        private static SKTypeface GetTypeface(string fontFile)
        {
            if (Typefaces.TryGetValue(fontFile, out SKTypeface cached))
            {
                return cached;
            }

            byte[] bytes = PlatformServices.Content.Read(PathFor(fontFile));
            using SKData data = SKData.CreateCopy(bytes);
            SKTypeface typeface = SKTypeface.FromData(data)
                ?? throw new InvalidOperationException($"Skia could not decode font '{fontFile}'.");
            Typefaces[fontFile] = typeface;
            return typeface;
        }
    }
}
