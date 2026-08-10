using CutTheRopeDX.Framework.Core;
using CutTheRopeDX.Framework.Visual;

// FontConfiguration keeps its original CutTheRopeDX.GameMain namespace even though it now lives
// alongside FontEffectSettings in Framework/Visual — the decouple-core refactor plan holds
// namespaces stable across file moves, so the mismatch here is deliberate.
#pragma warning disable IDE0130 // Namespace does not match folder structure
namespace CutTheRopeDX.GameMain
{
    /// <summary>
    /// Configuration for a font including size, color, and effects.
    /// </summary>
    internal sealed class FontConfiguration
    {
        /// <summary>
        /// Gets or sets the font file name.
        /// </summary>
        public string FontFile { get; set; }

        /// <summary>
        /// Gets or sets the font size in pixels.
        /// </summary>
        public float Size { get; set; }

        /// <summary>
        /// Gets or sets the base text color.
        /// </summary>
        public Color Color { get; set; }

        /// <summary>
        /// Gets or sets stroke and shadow effects applied while drawing the font.
        /// </summary>
        public FontEffectSettings Effects { get; set; }

        /// <summary>
        /// Gets or sets the extra line spacing.
        /// </summary>
        public float LineSpacing { get; set; }

        /// <summary>
        /// Gets or sets the top spacing adjustment.
        /// </summary>
        public float TopSpacing { get; set; }
    }
}
#pragma warning restore IDE0130

namespace CutTheRopeDX.Framework.Visual
{
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
