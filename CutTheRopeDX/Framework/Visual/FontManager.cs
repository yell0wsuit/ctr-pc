using System;
using System.Collections.Generic;
using System.IO;

using CutTheRopeDX.Helpers;

using FontStashSharp;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace CutTheRopeDX.Framework.Visual
{
    /// <summary>
    /// Manages loading and caching of FontStashSharp fonts.
    /// </summary>
    internal static class FontManager
    {
        /// <summary>
        /// Loaded font systems keyed by font file path.
        /// </summary>
        private static readonly Dictionary<string, FontSystem> fontSystems = [];

        /// <summary>
        /// Cached font instances keyed by a composite cache key.
        /// </summary>
        private static readonly Dictionary<string, FontStashFont> fontCache = [];

        /// <summary>
        /// The graphics device used for font rendering.
        /// </summary>
        private static GraphicsDevice graphicsDevice;

        /// <summary>
        /// Initializes the font manager with the specified graphics <paramref name="device"/>.
        /// </summary>
        /// <param name="device">Graphics device for font rendering.</param>
        public static void Initialize(GraphicsDevice device)
        {
            graphicsDevice = device ?? throw new ArgumentNullException(nameof(device));
        }

        /// <summary>
        /// Loads a FontStashSharp font from a TTF/OTF file.
        /// </summary>
        /// <param name="fontPath">Path to the TTF/OTF font file.</param>
        /// <param name="fontSize">Font size in pixels.</param>
        /// <param name="color">Text color.</param>
        /// <param name="effects">Stroke and shadow effect settings.</param>
        /// <param name="lineSpacing">Extra spacing between lines.</param>
        /// <param name="topSpacing">Extra spacing above the first line.</param>
        /// <returns>A cached or newly created <see cref="FontStashFont"/> instance.</returns>
        public static FontStashFont LoadFont(string fontPath, float fontSize, Color color, FontEffectSettings effects, float lineSpacing = 0f, float topSpacing = 0f)
        {
            if (graphicsDevice == null)
            {
                throw new InvalidOperationException("FontManager not initialized. Call Initialize() first.");
            }

            // Create a cache key based on all parameters
            string cacheKey = $"{fontPath}_{fontSize}_{color.PackedValue}_{GetEffectHash(effects)}_{lineSpacing}_{topSpacing}";

            if (fontCache.TryGetValue(cacheKey, out FontStashFont cachedFont))
            {
                // Recreate the font if the cached instance was disposed by FreePack/FreeResource
                if (cachedFont.GetInternalFont() != null)
                {
                    return cachedFont;
                }

                _ = fontCache.Remove(cacheKey);
            }

            // Get or create FontSystem for this font file
            if (!fontSystems.TryGetValue(fontPath, out FontSystem fontSystem))
            {
                fontSystem = LoadFontSystem(fontPath);
                fontSystems[fontPath] = fontSystem;
            }

            // Get the dynamic font at the specified size
            DynamicSpriteFont dynamicFont = fontSystem.GetFont(fontSize);

            // Create and cache the font wrapper
            FontStashFont font = new FontStashFont().InitWithFont(dynamicFont, fontSize, color, effects, lineSpacing, topSpacing);
            fontCache[cacheKey] = font;

            return font;
        }

        /// <summary>
        /// Loads a <see cref="FontSystem"/> from the specified font file path.
        /// </summary>
        /// <param name="fontPath">Path to the TTF/OTF font file.</param>
        /// <returns>The loaded <see cref="FontSystem"/>.</returns>
        private static FontSystem LoadFontSystem(string fontPath)
        {
            string contentFontPath = ContentPaths.GetFontPath(fontPath);

            byte[] fontData;
            try
            {
                // Try loading from content directory using TitleContainer
                using Stream stream = TitleContainer.OpenStream(contentFontPath);
                using MemoryStream ms = new();
                stream.CopyTo(ms);
                fontData = ms.ToArray();
            }
            catch
            {
                // Fallback to direct file access if TitleContainer fails
                try
                {
                    using Stream stream = TitleContainer.OpenStream(fontPath);
                    using MemoryStream ms = new();
                    stream.CopyTo(ms);
                    fontData = ms.ToArray();
                }
                catch
                {
                    throw new FileNotFoundException($"Font file not found: {fontPath}");
                }
            }

            FontSystemSettings settings = new()
            {
                FontResolutionFactor = 2, // Higher quality rendering
                KernelWidth = 2,
                KernelHeight = 2
            };

            FontSystem fontSystem = new(settings);
            fontSystem.AddFont(fontData);

            return fontSystem;
        }

        /// <summary>
        /// Computes a hash code for the effect settings used in cache key generation.
        /// </summary>
        /// <param name="effects">Font effect settings to hash.</param>
        /// <returns>A deterministic hash value for the provided settings.</returns>
        private static int GetEffectHash(FontEffectSettings effects)
        {
            if (effects == null)
            {
                return 0;
            }

            int hash = 17;
            hash = (hash * 31) + (effects.HasStroke ? 1 : 0);
            hash = (hash * 31) + effects.StrokeAmount;
            hash = (hash * 31) + (int)effects.StrokeColor.PackedValue;
            hash = (hash * 31) + (effects.HasShadow ? 1 : 0);
            hash = (hash * 31) + effects.ShadowOffsetX;
            hash = (hash * 31) + effects.ShadowOffsetY;
            hash = (hash * 31) + (int)effects.ShadowColor.PackedValue;
            return hash;
        }

        /// <summary>
        /// Clears all cached fonts and font systems.
        /// </summary>
        public static void ClearCache()
        {
            foreach (FontStashFont font in fontCache.Values)
            {
                font?.Dispose();
            }
            fontCache.Clear();

            fontSystems.Clear();
        }
    }
}
