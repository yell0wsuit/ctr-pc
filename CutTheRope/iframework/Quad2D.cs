namespace CutTheRope.iframework
{
    internal struct Quad2D(float x, float y, float w, float h)
    {
        public readonly float[] ToFloatArray()
        {
            return [tlX, tlY, trX, trY, blX, blY, brX, brY];
        }

        public static Quad2D MakeQuad2D(float x, float y, float w, float h)
        {
            return new Quad2D(x, y, w, h);
        }

        public float tlX = x;

        public float tlY = y;

        public float trX = x + w;

        public float trY = y;

        public float blX = x;

        public float blY = y + h;

        public float brX = x + w;

        public float brY = y + h;
    }
}
