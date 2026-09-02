using CutTheRopeDX.Desktop;
using CutTheRopeDX.Framework.Visual;
using CutTheRopeDX.GameMain;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace CutTheRopeDX.Framework.Platform
{
    /// <summary>Asset loading backed by MonoGame's ContentManager and a live graphics device.</summary>
    internal sealed class DesktopAssetPlatform : IAssetPlatform
    {
        /// <inheritdoc />
        public (int W, int H)? ImageDimensions(string contentPath)
        {
            // Images caches one ContentManager per asset, so the ImageTexture call that follows
            // this one resolves from that cache rather than loading a second time.
            Texture2D texture = Images.Get(contentPath);
            return texture == null ? null : (texture.Width, texture.Height);
        }

        /// <inheritdoc />
        public ITextureHandle ImageTexture(string contentPath)
        {
            return Images.GetHandle(contentPath);
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
            if (source is not MonoGameTexture texture)
            {
                return null;
            }

            // Content is built premultiplied, which is what the shared tint expects.
            byte[] pixels = new byte[width * height * 4];
            texture.Texture.GetData(0, new Rectangle(x, y, width, height), pixels, 0, pixels.Length);
            PremultipliedTint.Apply(pixels, tint);

            Texture2D tinted = new(Global.GraphicsDevice, width, height, false, SurfaceFormat.Color);
            tinted.SetData(pixels);
            return new MonoGameTexture(tinted);
        }

        /// <inheritdoc />
        public void FreeImage(string contentPath)
        {
            Images.Free(contentPath);
        }

        /// <inheritdoc />
        public FontGeneric Font(string resourceName)
        {
            FontConfiguration config = Resources.FontConfig.GetConfiguration(
                resourceName,
                LanguageHelper.CurrentAsInt);
            return FontManager.LoadFont(
                config.FontFile,
                config.Size,
                new Color(config.Color.R, config.Color.G, config.Color.B, config.Color.A),
                config.Effects,
                config.LineSpacing,
                config.TopSpacing);
        }

        /// <inheritdoc />
        public void ClearFontCache()
        {
            FontManager.ClearCache();
        }
    }
}
