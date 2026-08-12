using System.Numerics;

using CutTheRopeDX.Framework.Core;

namespace CutTheRopeDX.Framework.Platform
{
    /// <summary>
    /// Pure CPU conversion of immediate-mode sprite draws into batchable vertices.
    /// </summary>
    /// <remarks>
    /// Baking on the CPU is what lets a canvas-based backend reproduce the original
    /// fixed-function pipeline exactly: the matrix stack, skew and tint are all resolved
    /// here, so the GPU layer only ever receives finished vertices.
    /// </remarks>
    internal static class QuadBaking
    {
        /// <summary>Bakes the renderer tint into a premultiplied vertex color.</summary>
        /// <param name="tint">The renderer's current draw color.</param>
        public static Color BakePremultipliedTint(Color tint)
        {
            return Color.FromNonPremultiplied(tint.R, tint.G, tint.B, tint.A);
        }

        /// <summary>Whether a draw with this tint is skipped entirely.</summary>
        /// <param name="tint">The renderer's current draw color.</param>
        public static bool IsInvisible(Color tint)
        {
            return tint.A == 0;
        }

        /// <summary>
        /// Transforms one sprite vertex by the model-view matrix and attaches the
        /// premultiplied tint, preserving UV coordinates.
        /// </summary>
        /// <param name="source">The untransformed sprite vertex.</param>
        /// <param name="modelView">The current model-view matrix.</param>
        /// <param name="premultipliedTint">The baked tint to attach.</param>
        public static VertexPositionColorTexture Bake(
            in VertexPositionNormalTexture source,
            in Matrix4x4 modelView,
            Color premultipliedTint)
        {
            return new VertexPositionColorTexture(
                Vector3.Transform(source.Position, modelView),
                premultipliedTint,
                source.TextureCoordinate);
        }
    }
}
