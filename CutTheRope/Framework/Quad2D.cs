namespace CutTheRope.Framework
{
    /// <summary>
    /// A 2D quad defined by four corner points (top-left, top-right, bottom-left, bottom-right).
    /// </summary>
    /// <param name="x">X position of the top-left corner.</param>
    /// <param name="y">Y position of the top-left corner.</param>
    /// <param name="w">Width of the quad.</param>
    /// <param name="h">Height of the quad.</param>
    internal struct Quad2D(float x, float y, float w, float h)
    {
        /// <summary>
        /// Returns the eight corner coordinates as a flat float array.
        /// </summary>
        /// <returns>A flat array containing all quad coordinates.</returns>
        public readonly float[] ToFloatArray()
        {
            return [tlX, tlY, trX, trY, blX, blY, brX, brY];
        }

        /// <summary>
        /// Creates a <see cref="Quad2D"/> from position and size.
        /// </summary>
        /// <param name="x">X position of the top-left corner.</param>
        /// <param name="y">Y position of the top-left corner.</param>
        /// <param name="w">Width of the quad.</param>
        /// <param name="h">Height of the quad.</param>
        /// <returns>The constructed quad.</returns>
        public static Quad2D MakeQuad2D(float x, float y, float w, float h)
        {
            return new Quad2D(x, y, w, h);
        }

        /// <summary>
        /// Top-left X coordinate.
        /// </summary>
        public float tlX = x;

        /// <summary>
        /// Top-left Y coordinate.
        /// </summary>
        public float tlY = y;

        /// <summary>
        /// Top-right X coordinate.
        /// </summary>
        public float trX = x + w;

        /// <summary>
        /// Top-right Y coordinate.
        /// </summary>
        public float trY = y;

        /// <summary>
        /// Bottom-left X coordinate.
        /// </summary>
        public float blX = x;

        /// <summary>
        /// Bottom-left Y coordinate.
        /// </summary>
        public float blY = y + h;

        /// <summary>
        /// Bottom-right X coordinate.
        /// </summary>
        public float brX = x + w;

        /// <summary>
        /// Bottom-right Y coordinate.
        /// </summary>
        public float brY = y + h;
    }
}
