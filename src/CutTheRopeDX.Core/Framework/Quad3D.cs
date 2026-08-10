namespace CutTheRopeDX.Framework
{
    /// <summary>
    /// A 3D quad defined by four corner points with X, Y, and Z coordinates.
    /// </summary>
    internal struct Quad3D
    {
        /// <summary>
        /// Creates a <see cref="Quad3D"/> from position, size, and Z depth.
        /// </summary>
        /// <param name="x">X position of the bottom-left corner.</param>
        /// <param name="y">Y position of the bottom-left corner.</param>
        /// <param name="z">Z depth for all corners.</param>
        /// <param name="w">Width of the quad.</param>
        /// <param name="h">Height of the quad.</param>
        /// <returns>The constructed quad.</returns>
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

        /// <summary>
        /// Creates a <see cref="Quad3D"/> from four explicit 2D corner positions at Z=0.
        /// </summary>
        /// <param name="x1">Bottom-left X.</param>
        /// <param name="y1">Bottom-left Y.</param>
        /// <param name="x2">Bottom-right X.</param>
        /// <param name="y2">Bottom-right Y.</param>
        /// <param name="x3">Top-left X.</param>
        /// <param name="y3">Top-left Y.</param>
        /// <param name="x4">Top-right X.</param>
        /// <param name="y4">Top-right Y.</param>
        /// <returns>The constructed quad.</returns>
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

        /// <summary>
        /// Returns the twelve corner coordinates as a flat float array.
        /// </summary>
        /// <returns>A flat array containing all 4 corner coordinates in XYZ order.</returns>
        public float[] ToFloatArray()
        {
            _array ??=
                [
                    BlX, BlY, BlZ, BrX, BrY, BrZ, TlX, TlY, TlZ, TrX,
                    TrY, TrZ
                ];
            return _array;
        }

        /// <summary>
        /// Bottom-left X coordinate.
        /// </summary>
        public float BlX { get; private set; }

        /// <summary>
        /// Bottom-left Y coordinate.
        /// </summary>
        public float BlY { get; private set; }

        /// <summary>
        /// Bottom-left Z coordinate.
        /// </summary>
        public float BlZ { get; private set; }

        /// <summary>
        /// Bottom-right X coordinate.
        /// </summary>
        public float BrX { get; private set; }

        /// <summary>
        /// Bottom-right Y coordinate.
        /// </summary>
        public float BrY { get; private set; }

        /// <summary>
        /// Bottom-right Z coordinate.
        /// </summary>
        public float BrZ { get; private set; }

        /// <summary>
        /// Top-left X coordinate.
        /// </summary>
        public float TlX { get; private set; }

        /// <summary>
        /// Top-left Y coordinate.
        /// </summary>
        public float TlY { get; private set; }

        /// <summary>
        /// Top-left Z coordinate.
        /// </summary>
        public float TlZ { get; private set; }

        /// <summary>
        /// Top-right X coordinate.
        /// </summary>
        public float TrX { get; private set; }

        /// <summary>
        /// Top-right Y coordinate.
        /// </summary>
        public float TrY { get; private set; }

        /// <summary>
        /// Top-right Z coordinate.
        /// </summary>
        public float TrZ { get; private set; }

        /// <summary>
        /// Cached flat array returned by <see cref="ToFloatArray"/>.
        /// </summary>
        private float[] _array;
    }
}
