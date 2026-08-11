using CutTheRopeDX.Desktop;
using CutTheRopeDX.Framework.Visual;
using CutTheRopeDX.GameMain;

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
                new Microsoft.Xna.Framework.Color(config.Color.R, config.Color.G, config.Color.B, config.Color.A),
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
