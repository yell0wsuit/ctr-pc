namespace CutTheRope.Framework.Visual
{
    /// <summary>
    /// A <see cref="BaseElement"/> that arranges children horizontally with configurable spacing and vertical alignment.
    /// </summary>
    internal sealed class HBox : BaseElement
    {
        /// <inheritdoc />
        public override int AddChildwithID(BaseElement c, int i)
        {
            int childId = base.AddChildwithID(c, i);
            if (align == 8)
            {
                c.anchor = c.parentAnchor = 9;
            }
            else if (align == 16)
            {
                c.anchor = c.parentAnchor = 17;
            }
            else if (align == 32)
            {
                c.anchor = c.parentAnchor = 33;
            }
            c.x = nextElementX;
            nextElementX += c.width + offset;
            width = (int)(nextElementX - offset);
            return childId;
        }

        /// <summary>
        /// Initializes the horizontal box with spacing, alignment, and height.
        /// </summary>
        /// <param name="of">Spacing between children in pixels.</param>
        /// <param name="a">Vertical alignment flag (TOP, VCENTER, or BOTTOM).</param>
        /// <param name="h">Height of the box.</param>
        /// <returns>The initialized horizontal box instance.</returns>
        public HBox InitWithOffsetAlignHeight(float of, int a, float h)
        {
            offset = of;
            align = a;
            nextElementX = 0f;
            height = (int)h;
            return this;
        }

        /// <summary>
        /// Spacing between children in pixels.
        /// </summary>
        public float offset;

        /// <summary>
        /// Vertical alignment flag for children.
        /// </summary>
        public int align;

        /// <summary>
        /// X position where the next child will be placed.
        /// </summary>
        public float nextElementX;
    }
}
