namespace CutTheRopeDX.Framework.Visual
{
    /// <summary>
    /// A <see cref="BaseElement"/> that arranges children vertically with configurable spacing and horizontal alignment.
    /// </summary>
    internal sealed class VBox : BaseElement
    {
        /// <inheritdoc />
        public override int AddChildwithID(BaseElement c, int i)
        {
            int childId = base.AddChildwithID(c, i);
            if (align == 1)
            {
                c.anchor = c.parentAnchor = 9;
            }
            else if (align == 4)
            {
                c.anchor = c.parentAnchor = 12;
            }
            else if (align == 2)
            {
                c.anchor = c.parentAnchor = 10;
            }
            c.y = nextElementY;
            nextElementY += c.height + offset;
            height = (int)(nextElementY - offset);
            return childId;
        }

        /// <summary>
        /// Initializes the vertical box with spacing, alignment, and width.
        /// </summary>
        /// <param name="of">Spacing between children in pixels.</param>
        /// <param name="a">Horizontal alignment flag (LEFT, HCENTER, or RIGHT).</param>
        /// <param name="w">Width of the box.</param>
        /// <returns>The initialized vertical box instance.</returns>
        public VBox InitWithOffsetAlignWidth(float of, int a, float w)
        {
            offset = of;
            align = a;
            nextElementY = 0f;
            width = (int)w;
            return this;
        }

        /// <summary>
        /// Spacing between children in pixels.
        /// </summary>
        public float offset;

        /// <summary>
        /// Horizontal alignment flag for children.
        /// </summary>
        public int align;

        /// <summary>
        /// Y position where the next child will be placed.
        /// </summary>
        public float nextElementY;
    }
}
