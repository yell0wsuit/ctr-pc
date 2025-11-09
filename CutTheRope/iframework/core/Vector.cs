using Microsoft.Xna.Framework;
using System;

namespace CutTheRope.iframework.core
{
    public struct Vector
    {
        public Vector(Vector2 v)
        {
            this.x = v.X;
            this.y = v.Y;
        }

        public Vector(double xParam, double yParam)
        {
            this.x = (float)xParam;
            this.y = (float)yParam;
        }

        public Vector(float xParam, float yParam)
        {
            this.x = xParam;
            this.y = yParam;
        }

        public Vector2 toXNA()
        {
            return new Vector2(this.x, this.y);
        }

        public override string ToString()
        {
            return string.Concat(new string[]
            {
                "Vector(x=",
                this.x.ToString(),
                ",y=",
                this.y.ToString(),
                ")"
            });
        }

        public float x;

        public float y;
    }
}
