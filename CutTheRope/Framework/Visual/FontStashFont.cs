using System;
using System.Collections.Generic;

using FontStashSharp;

using Microsoft.Xna.Framework;

namespace CutTheRope.Framework.Visual
{
    /// <summary>
    /// FontStashSharp-based font implementation that replaces sprite/texture-based fonts.
    /// </summary>
    internal sealed class FontStashFont : FontGeneric
    {
        /// <summary>
        /// The underlying FontStashSharp dynamic font.
        /// </summary>
        private DynamicSpriteFont font;

        /// <summary>
        /// Font size in pixels.
        /// </summary>
        private float fontSize;

        /// <summary>
        /// Text rendering color.
        /// </summary>
        private Color textColor;

        /// <summary>
        /// Stroke and shadow effect settings.
        /// </summary>
        private FontEffectSettings effectSettings;

        /// <summary>
        /// Cache for rendered character images.
        /// </summary>
        private readonly Dictionary<char, Image> charImageCache = [];

        /// <summary>
        /// Initializes the font with the specified dynamic font, <paramref name="size"/>, <paramref name="color"/>, and <paramref name="effects"/>.
        /// </summary>
        /// <param name="dynamicFont">FontStashSharp dynamic font instance.</param>
        /// <param name="size">Font size in pixels.</param>
        /// <param name="color">Text rendering color.</param>
        /// <param name="effects">Stroke and shadow effect settings.</param>
        /// <param name="lineSpacing">Extra spacing between lines.</param>
        /// <param name="topSpacing">Extra spacing above the first line.</param>
        /// <returns>The initialized <see cref="FontStashFont"/> instance.</returns>
        public FontStashFont InitWithFont(DynamicSpriteFont dynamicFont, float size, Color color, FontEffectSettings effects, float lineSpacing = 0f, float topSpacing = 0f)
        {
            font = dynamicFont ?? throw new ArgumentNullException(nameof(dynamicFont));
            fontSize = size;
            textColor = color;
            effectSettings = effects;

            // Set default values
            charOffset = 0f;
            lineOffset = lineSpacing;
            spaceWidth = MeasureCharWidth(' ');
            this.topSpacing = topSpacing;

            return this;
        }

        /// <summary>
        /// Sets the text rendering <paramref name="color"/>.
        /// </summary>
        /// <param name="color">New text color.</param>
        public void SetColor(Color color)
        {
            textColor = color;
        }

        /// <summary>
        /// Returns the current text rendering color.
        /// </summary>
        /// <returns>The color currently used for text rendering.</returns>
        public Color GetColor()
        {
            return textColor;
        }

        /// <summary>
        /// Returns the underlying FontStashSharp dynamic font, or <see langword="null"/> if disposed.
        /// </summary>
        /// <returns>The internal dynamic font instance, or <see langword="null"/>.</returns>
        public DynamicSpriteFont GetInternalFont()
        {
            return font;
        }

        /// <summary>
        /// Returns the current effect settings.
        /// </summary>
        /// <returns>The active stroke and shadow effect settings.</returns>
        public FontEffectSettings GetEffectSettings()
        {
            return effectSettings;
        }

        /// <inheritdoc />
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                // Clear cached images
                foreach (Image cachedImage in charImageCache.Values)
                {
                    cachedImage?.Dispose();
                }
                charImageCache.Clear();

                font = null;
            }
            base.Dispose(disposing);
        }

        /// <inheritdoc />
        public override void SetCharOffsetLineOffsetSpaceWidth(float co, float lo, float sw)
        {
            charOffset = co;
            lineOffset = lo;
            spaceWidth = sw;
        }

        /// <inheritdoc />
        public override float FontHeight()
        {
            return font?.LineHeight ?? fontSize;
        }

        /// <inheritdoc />
        public override bool CanDraw(char c)
        {
            // FontStashSharp can draw most characters
            return font != null && !char.IsControl(c);
        }

        /// <inheritdoc />
        public override float GetCharWidth(char c)
        {
            return c == ' ' ? spaceWidth : c == '*' ? 0f : MeasureCharWidth(c);
        }

        /// <summary>
        /// Measures the pixel width of a single character using FontStashSharp.
        /// </summary>
        /// <param name="c">Character to measure.</param>
        /// <returns>The measured width in pixels.</returns>
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

        /// <inheritdoc />
        public override int GetCharmapIndex(char c)
        {
            // FontStashSharp uses a single texture atlas, so always return 0
            return 0;
        }

        /// <inheritdoc />
        public override int GetCharQuad(char c)
        {
            // For FontStashSharp, we don't use quad-based rendering
            // Return the character code as an identifier
            return CanDraw(c) ? c : -1;
        }

        /// <inheritdoc />
        public override float GetCharOffset(char[] s, int c, int len)
        {
            return c == len - 1 ? 0f : charOffset;
        }

        /// <inheritdoc />
        public override int TotalCharmaps()
        {
            // FontStashSharp uses a single texture atlas
            return 1;
        }

        /// <inheritdoc />
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
    internal sealed class FontEffectSettings
    {
        /// <summary>
        /// Whether stroke is enabled.
        /// </summary>
        public bool HasStroke { get; set; }

        /// <summary>
        /// Stroke thickness in pixels.
        /// </summary>
        public int StrokeAmount { get; set; } = 1;

        /// <summary>
        /// Stroke color.
        /// </summary>
        public Color StrokeColor { get; set; } = Color.Black;

        /// <summary>
        /// Whether shadow is enabled.
        /// </summary>
        public bool HasShadow { get; set; }

        /// <summary>
        /// Shadow horizontal offset in pixels.
        /// </summary>
        public int ShadowOffsetX { get; set; }

        /// <summary>
        /// Shadow vertical offset in pixels.
        /// </summary>
        public int ShadowOffsetY { get; set; }

        /// <summary>
        /// Shadow color.
        /// </summary>
        public Color ShadowColor { get; set; } = Color.Black;

        /// <summary>
        /// Returns a settings instance with no effects.
        /// </summary>
        public static FontEffectSettings None => new();

        /// <summary>
        /// Creates settings with stroke only.
        /// </summary>
        /// <param name="amount">Stroke thickness in pixels.</param>
        /// <param name="color">Stroke color, defaults to black.</param>
        /// <returns>A <see cref="FontEffectSettings"/> instance configured with stroke only.</returns>
        public static FontEffectSettings CreateStroke(int amount = 1, Color? color = null)
        {
            return new FontEffectSettings
            {
                HasStroke = true,
                StrokeAmount = amount,
                StrokeColor = color ?? Color.Black
            };
        }

        /// <summary>
        /// Creates settings with shadow only.
        /// </summary>
        /// <param name="offsetX">Shadow horizontal offset.</param>
        /// <param name="offsetY">Shadow vertical offset.</param>
        /// <param name="color">Shadow color, defaults to black.</param>
        /// <returns>A <see cref="FontEffectSettings"/> instance configured with shadow only.</returns>
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

        /// <summary>
        /// Creates settings with both stroke and shadow using black color.
        /// </summary>
        /// <param name="strokeAmount">Stroke thickness in pixels.</param>
        /// <param name="shadowX">Shadow horizontal offset.</param>
        /// <param name="shadowY">Shadow vertical offset.</param>
        /// <returns>A <see cref="FontEffectSettings"/> instance configured with both stroke and shadow.</returns>
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
