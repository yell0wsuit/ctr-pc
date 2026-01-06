namespace CutTheRope.Framework.Visual
{
    /// <summary>
    /// A <see cref="BaseElement"/> that draws a solid colored rectangle.
    /// </summary>
    internal class ColorRect : BaseElement
    {
        /// <summary>
        /// Gets or sets the fill color of the rectangle.
        /// </summary>
        public RGBAColor FillColor { get; set; }

        /// <summary>
        /// Creates a new <see cref="ColorRect"/> with the specified dimensions and color.
        /// </summary>
        /// <param name="w">Width of the rectangle in pixels.</param>
        /// <param name="h">Height of the rectangle in pixels.</param>
        /// <param name="color">Fill color of the rectangle.</param>
        /// <returns>A new <see cref="ColorRect"/> instance.</returns>
        public static ColorRect Create(float w, float h, RGBAColor color)
        {
            ColorRect rect = new()
            {
                width = (int)w,
                height = (int)h,
                FillColor = color
            };
            return rect;
        }

        /// <inheritdoc/>
        public override void Draw()
        {
            PreDraw();
            GLDrawer.DrawSolidRectWOBorder(drawX, drawY, width, height, FillColor);
            PostDraw();
        }
    }
}
