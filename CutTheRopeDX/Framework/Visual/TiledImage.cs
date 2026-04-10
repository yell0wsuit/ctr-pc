using CutTheRopeDX.Framework.Core;

namespace CutTheRopeDX.Framework.Visual
{
    /// <summary>
    /// An <see cref="Image"/> that tiles a single quad to fill its width and height.
    /// </summary>
    internal sealed class TiledImage : Image
    {
        /// <summary>
        /// Sets the quad index to tile.
        /// </summary>
        /// <param name="t">Quad index, or -1 for full image.</param>
        public void SetTile(int t)
        {
            q = t;
        }

        /// <inheritdoc />
        public override void Draw()
        {
            PreDraw();
            DrawHelper.DrawImageTiled(texture, q, drawX, drawY, width, height);
            PostDraw();
        }

        /// <summary>
        /// Creates a tiled image from the specified texture.
        /// </summary>
        /// <param name="t">Texture to tile.</param>
        /// <returns>A new tiled image instance.</returns>
        private static TiledImage TiledImage_create(CTRTexture2D t)
        {
            return (TiledImage)new TiledImage().InitWithTexture(t);
        }

        /// <summary>
        /// Creates a tiled image from the specified texture resource name.
        /// </summary>
        /// <param name="resourceName">Texture resource name.</param>
        /// <returns>A new tiled image initialized from the requested resource.</returns>
        public static TiledImage TiledImage_createWithResID(string resourceName)
        {
            return TiledImage_create(Application.GetTexture(resourceName));
        }

        /// <summary>
        /// Quad index to tile, or -1 for full image.
        /// </summary>
        private int q;
    }
}
