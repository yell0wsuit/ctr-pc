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
        /// Returns the renderer tint a straight-alpha backend must receive for the requested
        /// source blend factor.
        /// </summary>
        /// <remarks>
        /// Skia converts the straight tint to its premultiplied internal representation, which is
        /// what the fixed-function pipeline modulates the texture by, so the tint normally passes
        /// through. <c>GL_SRC_ALPHA</c> is the exception: see
        /// <see cref="ScalesSourceByAlpha"/> for why it needs the extra factor, and note that
        /// scaling the tint's RGB while leaving its alpha alone survives Skia's premultiply as one
        /// more multiplication by the tint alpha.
        /// </remarks>
        /// <param name="color">The renderer's straight-alpha tint.</param>
        /// <param name="source">The requested source blend factor.</param>
        /// <returns>A tint suitable for a straight-alpha backend.</returns>
        public static Color ForRendererTint(Color color, BlendingFactor source)
        {
            return ScalesSourceByAlpha(source)
                ? new Color(
                    Scale(color.R, color.A),
                    Scale(color.G, color.A),
                    Scale(color.B, color.A),
                    color.A)
                : color;
        }

        /// <summary>
        /// Whether the source blend factor makes the fixed-function pipeline multiply an
        /// already-premultiplied fragment by its own alpha a second time.
        /// </summary>
        /// <remarks>
        /// Both the built textures and the baked renderer tint are premultiplied, so the fragment
        /// leaving the sampler is premultiplied too. <c>GL_ONE</c> consumes that directly, which is
        /// what Skia's blend modes assume. <c>GL_SRC_ALPHA</c> instead multiplies it by source
        /// alpha once more; Skia has no blend mode that does the same, so a backend reproducing
        /// this pipeline has to fold that factor into the source it hands Skia. Skipping it leaves
        /// every partially transparent draw brighter than the fixed-function pipeline renders it.
        /// </remarks>
        /// <param name="source">The requested source blend factor.</param>
        /// <returns><see langword="true"/> when the source needs the extra alpha factor.</returns>
        public static bool ScalesSourceByAlpha(BlendingFactor source)
        {
            return source == BlendingFactor.GLSRCALPHA;
        }

        private static byte Scale(byte channel, byte alpha)
        {
            return (byte)(((channel * alpha) + (byte.MaxValue / 2)) / byte.MaxValue);
        }

        private static byte Unpremultiply(byte channel, byte alpha)
        {
            int straight = ((channel * byte.MaxValue) + (alpha / 2)) / alpha;
            return (byte)Math.Min(byte.MaxValue, straight);
        }
    }
}
