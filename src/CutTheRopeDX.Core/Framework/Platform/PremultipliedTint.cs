namespace CutTheRopeDX.Framework.Platform
{
    /// <summary>
    /// Recolors premultiplied RGBA pixels in place, keeping each pixel's alpha and giving it a new
    /// color. Art that is drawn as flat ink carries its whole shape and shading in alpha, so its
    /// own color channels hold nothing worth preserving - and being black, they cannot be tinted by
    /// the vertex-color multiply the renderer applies, which would leave them black whatever color
    /// was asked for.
    /// </summary>
    /// <remarks>
    /// The math lives here rather than in each backend because the two consume color differently:
    /// MonoGame hands out premultiplied bytes while Skia's public colors are straight. Both ask for
    /// premultiplied RGBA here, so both produce the same pixels.
    /// </remarks>
    internal static class PremultipliedTint
    {
        /// <summary>Rewrites premultiplied RGBA pixels so each keeps its alpha and wears <paramref name="tint"/>.</summary>
        /// <param name="pixels">Premultiplied RGBA bytes, four per pixel.</param>
        /// <param name="tint">Straight color the pixels take on; its own alpha is ignored.</param>
        internal static void Apply(byte[] pixels, RGBAColor tint)
        {
            for (int i = 0; i + 3 < pixels.Length; i += 4)
            {
                float alpha = pixels[i + 3];
                pixels[i] = Channel(tint.RedColor, alpha);
                pixels[i + 1] = Channel(tint.GreenColor, alpha);
                pixels[i + 2] = Channel(tint.BlueColor, alpha);
            }
        }

        private static byte Channel(float color, float alpha)
        {
            return (byte)((color * alpha) + 0.5f);
        }
    }
}
