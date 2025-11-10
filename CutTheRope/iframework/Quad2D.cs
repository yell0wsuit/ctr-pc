namespace CutTheRope.iframework
{
    internal struct Quad2D
    {
        public Quad2D(float x, float y, float w, float h)
        {
            tlX = x;
            tlY = y;
            trX = x + w;
            trY = y;
            blX = x;
            blY = y + h;
            brX = x + w;
            brY = y + h;
        }

        public float[] toFloatArray()
        {
            return [tlX, tlY, trX, trY, blX, blY, brX, brY];
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
