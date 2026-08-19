using CutTheRopeDX.Framework.Platform;

using SkiaSharp;

namespace CutTheRopeDX.Browser
{
    /// <summary>Platform texture backed by a GPU-resident Skia image.</summary>
    /// <param name="image">The Skia image; ownership transfers to this handle.</param>
    internal sealed class SkiaTexture(SKImage image) : ITextureHandle
    {
        /// <summary>
        /// Rewrites a color to opaque grey carrying its own alpha, so multiplying a texture by it
        /// weights the texture's color by its alpha and leaves its alpha untouched. Skia applies
        /// color matrices to straight colors, so the row that fixes alpha at one is what keeps the
        /// weighting off the alpha channel, and the destination factor still sees the source alpha
        /// the fixed-function pipeline would have produced.
        /// </summary>
        private static readonly float[] AlphaWeightMatrix =
        [
            0f, 0f, 0f, 1f, 0f,
            0f, 0f, 0f, 1f, 0f,
            0f, 0f, 0f, 1f, 0f,
            0f, 0f, 0f, 0f, 1f,
        ];

        private SKShader _shader;
        private SKShader _alphaWeightedShader;

        /// <summary>
        /// Filtering for every image the backend samples, both the sprite batches and the
        /// presented render target. It matches the <c>SamplerState.LinearClamp</c> the desktop
        /// backend draws quads and presents with, down to carrying no mipmaps: the textures are
        /// uploaded without them, and the game only ever scales by the modest factor between its
        /// internal resolution and the window. Skia defaults to nearest when a draw does not say
        /// otherwise, which would leave every scaled or rotated sprite aliased.
        /// </summary>
        internal static SKSamplingOptions LinearSampling { get; } =
            new(SKFilterMode.Linear, SKMipmapMode.None);

        /// <summary>The underlying Skia image.</summary>
        public SKImage Image { get; } = image;

        /// <inheritdoc />
        public int Width => Image.Width;

        /// <inheritdoc />
        public int Height => Image.Height;

        /// <summary>
        /// The shader a batch samples this texture through, optionally weighted by the texture's
        /// own alpha for the <c>GL_SRC_ALPHA</c> source factor.
        /// </summary>
        /// <remarks>
        /// Both variants depend on nothing but the image, so they are built once and kept for the
        /// texture's lifetime. Rebuilding them per batch put several native Skia objects on the
        /// allocation path of every draw the frame issued.
        /// </remarks>
        /// <param name="weightedByAlpha">Whether the source is weighted by its own alpha.</param>
        internal SKShader Shader(bool weightedByAlpha)
        {
            _shader ??= SKShader.CreateImage(
                Image, SKShaderTileMode.Clamp, SKShaderTileMode.Clamp, LinearSampling);
            if (!weightedByAlpha)
            {
                return _shader;
            }

            if (_alphaWeightedShader is null)
            {
                using SKColorFilter alphaWeight =
                    SKColorFilter.CreateColorMatrix(AlphaWeightMatrix);
                using SKShader alphaOnly = _shader.WithColorFilter(alphaWeight);
                _alphaWeightedShader =
                    SKShader.CreateBlend(SKBlendMode.Modulate, _shader, alphaOnly);
            }
            return _alphaWeightedShader;
        }

        /// <inheritdoc />
        public void Dispose()
        {
            _alphaWeightedShader?.Dispose();
            _alphaWeightedShader = null;
            _shader?.Dispose();
            _shader = null;
            Image.Dispose();
        }
    }
}
