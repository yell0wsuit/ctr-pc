using System.Globalization;

using Microsoft.Xna.Framework;

namespace CutTheRope.Framework.Core
{
    public struct Vector
    {
        public Vector(Vector2 v)
        {
            XAxis = v.X;
            YAxis = v.Y;
        }

        public Vector(double xParam, double yParam)
        {
            XAxis = (float)xParam;
            YAxis = (float)yParam;
        }

        public Vector(float xParam, float yParam)
        {
            XAxis = xParam;
            YAxis = yParam;
        }

        public readonly Vector2 ToXNA()
        {
            return new Vector2(XAxis, YAxis);
        }

        public override readonly string ToString()
        {
            return string.Concat(new string[]
            {
                "Vector(x=",
                XAxis.ToString(CultureInfo.InvariantCulture),
                ",y=",
                YAxis.ToString(CultureInfo.InvariantCulture),
                ")"
            });
        }

        public float XAxis { get; set; }

        public float YAxis { get; set; }
    }
}
