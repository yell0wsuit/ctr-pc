using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace CutTheRopeDX.Desktop
{
    /// <summary>
    /// Pure CPU conversion of immediate-mode sprite draws into batchable vertices.
    /// </summary>
    internal static class QuadBaking
    {
        /// <summary>
        /// Bakes the renderer tint into a premultiplied vertex color.
        /// </summary>
        /// <param name="tint">The renderer's current draw color.</param>
        /// <returns>The premultiplied vertex color.</returns>
        public static Color BakePremultipliedTint(Color tint)
        {
            return Color.FromNonPremultiplied(tint.R, tint.G, tint.B, tint.A);
        }

        /// <summary>
        /// Whether a draw with this tint is skipped entirely.
        /// </summary>
        /// <param name="tint">The renderer's current draw color.</param>
        /// <returns><see langword="true"/> when the draw must be skipped.</returns>
        public static bool IsInvisible(Color tint)
        {
            return tint.A == 0;
        }

        /// <summary>
        /// Transforms one sprite vertex by the model-view matrix and attaches the
        /// premultiplied tint, preserving UV coordinates.
        /// </summary>
        public static VertexPositionColorTexture Bake(in VertexPositionNormalTexture source, in Matrix modelView, Color premultipliedTint)
        {
            return new VertexPositionColorTexture(
                Vector3.Transform(source.Position, modelView),
                premultipliedTint,
                source.TextureCoordinate);
        }
    }
}
