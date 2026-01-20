namespace CutTheRope.Framework
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
                BlX = x,
                BlY = y,
                BlZ = z,
                BrX = x + w,
                BrY = y,
                BrZ = z,
                TlX = x,
                TlY = y + h,
                TlZ = z,
                TrX = x + w,
                TrY = y + h,
                TrZ = z
            };
        }

        public static Quad3D MakeQuad3DEx(float x1, float y1, float x2, float y2, float x3, float y3, float x4, float y4)
        {
            return new Quad3D
            {
                BlX = x1,
                BlY = y1,
                BlZ = 0f,
                BrX = x2,
                BrY = y2,
                BrZ = 0f,
                TlX = x3,
                TlY = y3,
                TlZ = 0f,
                TrX = x4,
                TrY = y4,
                TrZ = 0f
            };
        }

        public float[] ToFloatArray()
        {
            _array ??=
                [
                    BlX, BlY, BlZ, BrX, BrY, BrZ, TlX, TlY, TlZ, TrX,
                    TrY, TrZ
                ];
            return _array;
        }

        public float BlX { get; private set; }

        public float BlY { get; private set; }

        public float BlZ { get; private set; }

        public float BrX { get; private set; }

        public float BrY { get; private set; }

        public float BrZ { get; private set; }

        public float TlX { get; private set; }

        public float TlY { get; private set; }

        public float TlZ { get; private set; }

        public float TrX { get; private set; }

        public float TrY { get; private set; }

        public float TrZ { get; private set; }

        private float[] _array;
    }
}
