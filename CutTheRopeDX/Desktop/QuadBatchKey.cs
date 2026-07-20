using System;

using Microsoft.Xna.Framework;

namespace CutTheRopeDX.Desktop
{
    /// <summary>
    /// GPU-visible state that must match for two sprite quads to share a batch.
    /// </summary>
    /// <param name="texture">Texture identity, compared by reference.</param>
    /// <param name="blend">Effective blend snapshot.</param>
    /// <param name="scissor">Device scissor rectangle.</param>
    /// <param name="projection">Projection matrix.</param>
    internal readonly struct QuadBatchKey(object texture, BlendParams.BlendType blend, Rectangle scissor, Matrix projection) : IEquatable<QuadBatchKey>
    {
        /// <summary>Texture identity, compared by reference.</summary>
        public readonly object Texture = texture;

        /// <summary>Effective blend snapshot at submission.</summary>
        public readonly BlendParams.BlendType Blend = blend;

        /// <summary>Device scissor rectangle at submission.</summary>
        public readonly Rectangle Scissor = scissor;

        /// <summary>Projection matrix at submission.</summary>
        public readonly Matrix Projection = projection;

        /// <inheritdoc />
        public bool Equals(QuadBatchKey other)
        {
            return ReferenceEquals(Texture, other.Texture)
                && Blend == other.Blend
                && Scissor == other.Scissor
                && Projection == other.Projection;
        }

        /// <inheritdoc />
        public override bool Equals(object obj)
        {
            return obj is QuadBatchKey other && Equals(other);
        }

        /// <inheritdoc />
        public override int GetHashCode()
        {
            return HashCode.Combine(Texture, Blend, Scissor);
        }
    }
}
