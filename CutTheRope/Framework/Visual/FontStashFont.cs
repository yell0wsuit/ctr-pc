using System;
using System.Collections.Generic;
using CutTheRope.Helpers;
using FontStashSharp;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace CutTheRope.Framework.Visual
{
    /// <summary>
    /// FontStashSharp-based font implementation that replaces sprite/texture-based fonts.
    /// </summary>
    internal sealed class FontStashFont : FontGeneric
    {
        private DynamicSpriteFont font;
        private float fontSize;
        private Color textColor;
        private FontEffectSettings effectSettings;

        // Cache for rendered character textures
        private readonly Dictionary<char, Image> charImageCache = new();

        public FontStashFont InitWithFont(DynamicSpriteFont dynamicFont, float size, Color color, FontEffectSettings effects)
        {
            font = dynamicFont ?? throw new ArgumentNullException(nameof(dynamicFont));
            fontSize = size;
            textColor = color;
            effectSettings = effects;

            // Set default values
            charOffset = 0f;
            lineOffset = 0f;
            spaceWidth = MeasureCharWidth(' ');

            return this;
        }

        public void SetColor(Color color)
        {
            textColor = color;
        }

        public Color GetColor()
        {
            return textColor;
        }

        public DynamicSpriteFont GetInternalFont()
        {
            return font;
        }

        public FontEffectSettings GetEffectSettings()
        {
            return effectSettings;
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                // Clear cached images
                foreach (var cachedImage in charImageCache.Values)
                {
                    cachedImage?.Dispose();
                }
                charImageCache.Clear();

                font = null;
            }
            base.Dispose(disposing);
        }

        public override void SetCharOffsetLineOffsetSpaceWidth(float co, float lo, float sw)
        {
            charOffset = co;
            lineOffset = lo;
            spaceWidth = sw;
        }

        public override float FontHeight()
        {
            return font?.LineHeight ?? fontSize;
        }

        public override bool CanDraw(char c)
        {
            // FontStashSharp can draw most characters
            return font != null && !char.IsControl(c);
        }

        public override float GetCharWidth(char c)
        {
            if (c == ' ')
            {
                return spaceWidth;
            }

            if (c == '*')
            {
                return 0f;
            }

            return MeasureCharWidth(c);
        }

        private float MeasureCharWidth(char c)
        {
            if (font == null)
            {
                return 0f;
            }

            string charStr = c.ToString();
            Vector2 size = font.MeasureString(charStr);
            return size.X;
        }

        public override int GetCharmapIndex(char c)
        {
            // FontStashSharp uses a single texture atlas, so always return 0
            return 0;
        }

        public override int GetCharQuad(char c)
        {
            // For FontStashSharp, we don't use quad-based rendering
            // Return the character code as an identifier
            return CanDraw(c) ? c : -1;
        }

        public override float GetCharOffset(char[] s, int c, int len)
        {
            return c == len - 1 ? 0f : charOffset;
        }

        public override int TotalCharmaps()
        {
            // FontStashSharp uses a single texture atlas
            return 1;
        }

        public override Image GetCharmap(int i)
        {
            // Return a placeholder image for compatibility
            // The actual rendering is done differently with FontStashSharp
            return null;
        }
    }

    /// <summary>
    /// Configuration for font effects (stroke, shadow).
    /// </summary>
    internal class FontEffectSettings
    {
        public bool HasStroke { get; set; }
        public int StrokeAmount { get; set; } = 1;
        public Color StrokeColor { get; set; } = Color.Black;

        public bool HasShadow { get; set; }
        public int ShadowOffsetX { get; set; }
        public int ShadowOffsetY { get; set; }
        public Color ShadowColor { get; set; } = Color.Black;

        public static FontEffectSettings None => new FontEffectSettings();

        public static FontEffectSettings CreateStroke(int amount = 1, Color? color = null)
        {
            return new FontEffectSettings
            {
                HasStroke = true,
                StrokeAmount = amount,
                StrokeColor = color ?? Color.Black
            };
        }

        public static FontEffectSettings CreateShadow(int offsetX, int offsetY, Color? color = null)
        {
            return new FontEffectSettings
            {
                HasShadow = true,
                ShadowOffsetX = offsetX,
                ShadowOffsetY = offsetY,
                ShadowColor = color ?? Color.Black
            };
        }

        public static FontEffectSettings CreateStrokeAndShadow(int strokeAmount, int shadowX, int shadowY)
        {
            return new FontEffectSettings
            {
                HasStroke = true,
                StrokeAmount = strokeAmount,
                StrokeColor = Color.Black,
                HasShadow = true,
                ShadowOffsetX = shadowX,
                ShadowOffsetY = shadowY,
                ShadowColor = Color.Black
            };
        }
    }
}
