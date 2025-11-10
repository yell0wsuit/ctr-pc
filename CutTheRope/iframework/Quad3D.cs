namespace CutTheRope.iframework
{
    internal struct Quad3D
    {
        public static Quad3D MakeQuad3D(double x, double y, double z, double w, double h)
        {
            return MakeQuad3D((float)x, (float)y, (float)z, (float)w, (float)h);
        }

        public static Quad3D MakeQuad3D(float x, float y, float z, float w, float h)
        {
            return new Quad3D
            {
                blX = x,
                blY = y,
                blZ = z,
                brX = x + w,
                brY = y,
                brZ = z,
                tlX = x,
                tlY = y + h,
                tlZ = z,
                trX = x + w,
                trY = y + h,
                trZ = z
            };
        }

        public static Quad3D MakeQuad3DEx(float x1, float y1, float x2, float y2, float x3, float y3, float x4, float y4)
        {
            return new Quad3D
            {
                blX = x1,
                blY = y1,
                blZ = 0f,
                brX = x2,
                brY = y2,
                brZ = 0f,
                tlX = x3,
                tlY = y3,
                tlZ = 0f,
                trX = x4,
                trY = y4,
                trZ = 0f
            };
        }

        public float[] toFloatArray()
        {
            if (_array == null)
            {
                _array =
                [
                    blX, blY, blZ, brX, brY, brZ, tlX, tlY, tlZ, trX,
                    trY, trZ
                ];
            }
            return _array;
        }

        private float blX;

        private float blY;

        private float blZ;

        private float brX;

        private float brY;

        private float brZ;

        private float tlX;

        private float tlY;

        private float tlZ;

        private float trX;

        private float trY;

        private float trZ;

        private float[] _array;
    }
}
