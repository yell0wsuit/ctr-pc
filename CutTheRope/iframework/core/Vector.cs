using Microsoft.Xna.Framework;

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

        public Vector2 toXNA()
        {
            return new Vector2(x, y);
        }

        public override string ToString()
        {
            return string.Concat(new string[]
            {
                "Vector(x=",
                x.ToString(),
                ",y=",
                y.ToString(),
                ")"
            });
        }

        public float x;

        public float y;
    }
}
