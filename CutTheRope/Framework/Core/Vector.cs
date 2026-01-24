using System.Globalization;

using Microsoft.Xna.Framework;

namespace CutTheRope.Framework.Core
{
    public struct Vector
    {
        public Vector(Vector2 v)
        {
            X = v.X;
            Y = v.Y;
        }

        public Vector(double xParam, double yParam)
        {
            X = (float)xParam;
            Y = (float)yParam;
        }

        public Vector(float xParam, float yParam)
        {
            X = xParam;
            Y = yParam;
        }

        public readonly Vector2 ToXNA()
        {
            return new Vector2(X, Y);
        }

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

        public float X { get; set; }

        public float Y { get; set; }
    }
}
