using System;
using System.Collections.Generic;
using System.IO;
using FontStashSharp;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace CutTheRope.Framework.Visual
{
    /// <summary>
    /// Manages loading and caching of FontStashSharp fonts.
    /// </summary>
    internal static class FontManager
    {
        private static readonly Dictionary<string, FontSystem> fontSystems = new();
        private static readonly Dictionary<string, FontStashFont> fontCache = new();
        private static GraphicsDevice graphicsDevice;

        public static void Initialize(GraphicsDevice device)
        {
            graphicsDevice = device ?? throw new ArgumentNullException(nameof(device));
        }

        /// <summary>
        /// Loads a FontStashSharp font from a TTF file.
        /// </summary>
        public static FontStashFont LoadFont(string fontPath, float fontSize, Color color, FontEffectSettings effects)
        {
            if (graphicsDevice == null)
            {
                throw new InvalidOperationException("FontManager not initialized. Call Initialize() first.");
            }

            // Create a cache key based on all parameters
            string cacheKey = $"{fontPath}_{fontSize}_{color.PackedValue}_{GetEffectHash(effects)}";

            if (fontCache.TryGetValue(cacheKey, out FontStashFont cachedFont))
            {
                return cachedFont;
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
            FontStashFont font = new FontStashFont().InitWithFont(dynamicFont, fontSize, color, effects);
            fontCache[cacheKey] = font;

            return font;
        }

        private static FontSystem LoadFontSystem(string fontPath)
        {
            string fullPath;

            // Try content directory first
            if (File.Exists($"content/fonts/{fontPath}"))
            {
                fullPath = $"content/fonts/{fontPath}";
            }
            else if (File.Exists(fontPath))
            {
                fullPath = fontPath;
            }
            else
            {
                throw new FileNotFoundException($"Font file not found: {fontPath}");
            }

            byte[] fontData = File.ReadAllBytes(fullPath);

            FontSystemSettings settings = new FontSystemSettings
            {
                FontResolutionFactor = 2, // Higher quality rendering
                KernelWidth = 2,
                KernelHeight = 2
            };

            FontSystem fontSystem = new FontSystem(settings);
            fontSystem.AddFont(fontData);

            return fontSystem;
        }

        private static int GetEffectHash(FontEffectSettings effects)
        {
            if (effects == null)
            {
                return 0;
            }

            int hash = 17;
            hash = hash * 31 + (effects.HasStroke ? 1 : 0);
            hash = hash * 31 + effects.StrokeAmount;
            hash = hash * 31 + (int)effects.StrokeColor.PackedValue;
            hash = hash * 31 + (effects.HasShadow ? 1 : 0);
            hash = hash * 31 + effects.ShadowOffsetX;
            hash = hash * 31 + effects.ShadowOffsetY;
            hash = hash * 31 + (int)effects.ShadowColor.PackedValue;
            return hash;
        }

        /// <summary>
        /// Clears all cached fonts and font systems.
        /// </summary>
        public static void ClearCache()
        {
            foreach (var font in fontCache.Values)
            {
                font?.Dispose();
            }
            fontCache.Clear();

            fontSystems.Clear();
        }
    }
}
