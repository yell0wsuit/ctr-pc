using CutTheRopeDX.Framework.Platform;

using SkiaSharp;

namespace CutTheRopeDX.Browser
{
    /// <summary>Platform texture backed by a GPU-resident Skia image.</summary>
    /// <param name="image">The Skia image; ownership transfers to this handle.</param>
    internal sealed class SkiaTexture(SKImage image) : ITextureHandle
    {
        /// <summary>The underlying Skia image.</summary>
        public SKImage Image { get; } = image;

        /// <inheritdoc />
        public int Width => Image.Width;

        /// <inheritdoc />
        public int Height => Image.Height;

        /// <inheritdoc />
        public void Dispose()
        {
            Image.Dispose();
        }
    }
}
