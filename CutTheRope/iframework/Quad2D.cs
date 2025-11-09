using System;

namespace CutTheRope.iframework
{
    internal struct Quad2D
    {
        public Quad2D(float x, float y, float w, float h)
        {
            this.tlX = x;
            this.tlY = y;
            this.trX = x + w;
            this.trY = y;
            this.blX = x;
            this.blY = y + h;
            this.brX = x + w;
            this.brY = y + h;
        }

        public float[] toFloatArray()
        {
            return [this.tlX, this.tlY, this.trX, this.trY, this.blX, this.blY, this.brX, this.brY];
        }

        public static Quad2D MakeQuad2D(float x, float y, float w, float h)
        {
            return new Quad2D(x, y, w, h);
        }

        public float tlX;

        public float tlY;

        public float trX;

        public float trY;

        public float blX;

        public float blY;

        public float brX;

        public float brY;
    }
}
