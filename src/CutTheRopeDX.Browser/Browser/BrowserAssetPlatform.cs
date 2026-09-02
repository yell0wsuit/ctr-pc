using System.Collections.Generic;

using CutTheRopeDX.Framework;
using CutTheRopeDX.Framework.Platform;
using CutTheRopeDX.Framework.Visual;
using CutTheRopeDX.GameMain;

using SkiaSharp;

namespace CutTheRopeDX.Browser
{
    /// <summary>Asset loading backed by the browser content store and Skia decoding.</summary>
    /// <param name="surface">Supplies the GPU context textures are uploaded into.</param>
    internal sealed class BrowserAssetPlatform(SkiaSurface surface) : IAssetPlatform
    {
        /// <summary>Extension the web content pipeline writes images as.</summary>
        public const string ImageExtension = ".webp";

        private readonly Dictionary<string, SkiaTexture> _textures = [];

        private SkiaTexture Load(string contentPath)
        {
            if (_textures.TryGetValue(contentPath, out SkiaTexture cached))
            {
                return cached;
            }

            byte[] encoded = PlatformServices.Content.Read(contentPath + ImageExtension);
            using SKData data = SKData.CreateCopy(encoded);
            using SKImage decoded = SKImage.FromEncodedData(data);
            if (decoded is null)
            {
                return null;
            }

            SkiaTexture texture = new(decoded.ToTextureImage(surface.Context));
            _textures[contentPath] = texture;
            return texture;
        }

        /// <inheritdoc />
        public (int W, int H)? ImageDimensions(string contentPath)
        {
            SkiaTexture texture = Load(contentPath);
            return texture is null ? null : (texture.Width, texture.Height);
        }

        /// <inheritdoc />
        public ITextureHandle ImageTexture(string contentPath)
        {
            return Load(contentPath);
        }

        /// <inheritdoc />
        public ITextureHandle TintedRegion(
            ITextureHandle source,
            int x,
            int y,
            int width,
            int height,
            RGBAColor tint)
        {
            if (source is not SkiaTexture texture)
            {
                return null;
            }

            // Skia's public colors are straight, so the region is read back premultiplied here to
            // give the shared tint the same pixels the desktop backend hands it.
            SKImageInfo info = new(width, height, SKColorType.Rgba8888, SKAlphaType.Premul);
            using SKBitmap region = new(info);
            if (!texture.Image.ReadPixels(info, region.GetPixels(), region.RowBytes, x, y))
            {
                return null;
            }

            byte[] pixels = region.Bytes;
            PremultipliedTint.Apply(pixels, tint);
            using SKImage tinted = SKImage.FromPixelCopy(info, pixels);
            return new SkiaTexture(tinted.ToTextureImage(surface.Context));
        }

        /// <inheritdoc />
        public void FreeImage(string contentPath)
        {
            if (_textures.Remove(contentPath, out SkiaTexture texture))
            {
                texture.Dispose();
            }
        }

        /// <inheritdoc />
        public FontGeneric Font(string resourceName)
        {
            FontConfiguration config = Resources.FontConfig.GetConfiguration(
                resourceName, LanguageHelper.CurrentAsInt);
            return SkiaFontCache.Load(config);
        }

        /// <inheritdoc />
        public void ClearFontCache()
        {
            SkiaFontCache.Clear();
        }
    }
}
