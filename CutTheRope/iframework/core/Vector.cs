using Microsoft.Xna.Framework;
using System.Globalization;

namespace CutTheRope.iframework.core
{
    public struct Vector
    {
        public Vector(Vector2 v)
        {
            x = v.X;
            y = v.Y;
        }

        public Vector(double xParam, double yParam)
        {
            x = (float)xParam;
            y = (float)yParam;
        }

        public Vector(float xParam, float yParam)
        {
            x = xParam;
            y = yParam;
        }

        public readonly Vector2 ToXNA()
        {
            return new Vector2(x, y);
        }

        public override readonly string ToString()
        {
            return string.Concat(new string[]
            {
                "Vector(x=",
                x.ToString(CultureInfo.InvariantCulture),
                ",y=",
                y.ToString(CultureInfo.InvariantCulture),
                ")"
            });
        }

        public float x;

        public float y;
    }
}
