using System.Globalization;

using Microsoft.Xna.Framework;

namespace CutTheRope.Framework.Core
{
    /// <summary>
    /// A 2D vector with X and Y float components.
    /// </summary>
    public struct Vector
    {
        /// <summary>
        /// Initializes a new <see cref="Vector"/> from an XNA <see cref="Vector2"/>.
        /// </summary>
        /// <param name="v">Source XNA vector.</param>
        public Vector(Vector2 v)
        {
            X = v.X;
            Y = v.Y;
        }

        /// <summary>
        /// Initializes a new <see cref="Vector"/> with the specified components.
        /// </summary>
        /// <param name="xParam">X component.</param>
        /// <param name="yParam">Y component.</param>
        public Vector(float xParam, float yParam)
        {
            X = xParam;
            Y = yParam;
        }

        /// <summary>
        /// Converts this vector to an XNA <see cref="Vector2"/>.
        /// </summary>
        /// <returns>The converted XNA vector.</returns>
        public readonly Vector2 ToXNA()
        {
            return new Vector2(X, Y);
        }

        /// <inheritdoc />
        public override readonly string ToString()
        {
            return string.Concat(new string[]
            {
                "Vector(x=",
                X.ToString(CultureInfo.InvariantCulture),
                ",y=",
                Y.ToString(CultureInfo.InvariantCulture),
                ")"
            });
        }

        /// <summary>
        /// X component.
        /// </summary>
        public float X { get; set; }

        /// <summary>
        /// Y component.
        /// </summary>
        public float Y { get; set; }
    }
}
