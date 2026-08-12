using System;

using CutTheRopeDX.Framework.Core;

namespace CutTheRopeDX.Framework.Platform
{
    /// <summary>
    /// Converts fixed-function vertex colors for backends whose color API accepts straight alpha.
    /// </summary>
    internal static class VertexColorEncoding
    {
        /// <summary>
        /// Returns the straight-alpha representation a backend such as Skia must receive to
        /// reproduce the requested GL blend factors.
        /// </summary>
        /// <remarks>
        /// <c>GL_ONE / GL_ONE_MINUS_SRC_ALPHA</c> consumes premultiplied source colors. Skia's
        /// public color values are straight and are premultiplied internally, so those vertex
        /// colors must be unpremultiplied exactly once. The other blend pairs used by Core consume
        /// straight source colors and therefore pass through unchanged.
        /// </remarks>
        /// <param name="color">The packed vertex color supplied by Core.</param>
        /// <param name="source">The requested source blend factor.</param>
        /// <param name="destination">The requested destination blend factor.</param>
        /// <returns>A color suitable for a straight-alpha backend.</returns>
        public static Color ForExplicitVertex(
            Color color,
            BlendingFactor source,
            BlendingFactor destination)
        {
            return (source, destination, color.A) switch
            {
                (BlendingFactor.GLONE, BlendingFactor.GLONEMINUSSRCALPHA, 0) =>
                    Color.Transparent,
                (BlendingFactor.GLONE, BlendingFactor.GLONEMINUSSRCALPHA, byte.MaxValue) =>
                    color,
                (BlendingFactor.GLONE, BlendingFactor.GLONEMINUSSRCALPHA, _) =>
                    new Color(
                        Unpremultiply(color.R, color.A),
                        Unpremultiply(color.G, color.A),
                        Unpremultiply(color.B, color.A),
                        color.A),
                _ => color,
            };
        }

        /// <summary>
        /// Returns a renderer tint unchanged because Skia itself converts straight colors to its
        /// premultiplied internal representation.
        /// </summary>
        /// <param name="color">The renderer's straight-alpha tint.</param>
        /// <returns><paramref name="color"/> unchanged.</returns>
        public static Color ForRendererTint(Color color)
        {
            return color;
        }

        private static byte Unpremultiply(byte channel, byte alpha)
        {
            int straight = ((channel * byte.MaxValue) + (alpha / 2)) / alpha;
            return (byte)Math.Min(byte.MaxValue, straight);
        }
    }
}
