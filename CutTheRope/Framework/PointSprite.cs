namespace CutTheRope.Framework
{
    /// <summary>
    /// A 2D point with an associated sprite size.
    /// </summary>
    /// <param name="xx">X coordinate.</param>
    /// <param name="yy">Y coordinate.</param>
    /// <param name="s">Sprite size.</param>
    internal struct PointSprite(float xx, float yy, float s)
    {
        /// <summary>
        /// X coordinate.
        /// </summary>
        public float x = xx;

        /// <summary>
        /// Y coordinate.
        /// </summary>
        public float y = yy;

        /// <summary>
        /// Sprite size.
        /// </summary>
        public float size = s;
    }
}
