namespace CutTheRope.Framework
{
    /// <summary>
    /// A 2D point with float coordinates.
    /// </summary>
    /// <param name="xx">X coordinate.</param>
    /// <param name="yy">Y coordinate.</param>
    internal struct Point(float xx, float yy)
    {
        /// <summary>
        /// X coordinate.
        /// </summary>
        public float x = xx;

        /// <summary>
        /// Y coordinate.
        /// </summary>
        public float y = yy;
    }
}
