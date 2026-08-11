namespace CutTheRopeDX.Framework
{
    /// <summary>
    /// A rectangle defined by position and size.
    /// </summary>
    /// <param name="xParam">X position.</param>
    /// <param name="yParam">Y position.</param>
    /// <param name="width">Width.</param>
    /// <param name="height">Height.</param>
    internal struct CTRRectangle(float xParam, float yParam, float width, float height)
    {
        /// <summary>
        /// Returns <see langword="true"/> if any component is non-zero.
        /// </summary>
        /// <returns><see langword="true"/> when at least one rectangle component is non-zero; otherwise <see langword="false"/>.</returns>
        public readonly bool IsValid()
        {
            return x != 0f || y != 0f || w != 0f || h != 0f;
        }

        /// <summary>
        /// X position of the rectangle.
        /// </summary>
        public float x = xParam;

        /// <summary>
        /// Y position of the rectangle.
        /// </summary>
        public float y = yParam;

        /// <summary>
        /// Width of the rectangle.
        /// </summary>
        public float w = width;

        /// <summary>
        /// Height of the rectangle.
        /// </summary>
        public float h = height;
    }
}
