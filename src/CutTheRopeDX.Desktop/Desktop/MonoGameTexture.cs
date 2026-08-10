using CutTheRopeDX.Framework.Platform;

using Microsoft.Xna.Framework.Graphics;

namespace CutTheRopeDX.Desktop
{
    /// <summary>Wraps a MonoGame <see cref="Texture2D"/> as the platform texture handle.</summary>
    internal sealed class MonoGameTexture(Texture2D texture) : ITextureHandle
    {
        public Texture2D Texture { get; } = texture;
        public int Width => Texture.Width;
        public int Height => Texture.Height;

        /// <inheritdoc />
        public void Dispose()
        {
            Texture.Dispose();
        }
    }
}
