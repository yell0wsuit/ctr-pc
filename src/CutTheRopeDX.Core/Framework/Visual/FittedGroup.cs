namespace CutTheRopeDX.Framework.Visual
{
    /// <summary>
    /// Holds a scene's design-space content and reconciles the two spaces either side of itself.
    /// Drawing scales its children up into logical space; touches arrive in logical space and are
    /// scaled back down before the children see them, so a hit test lands on the element the
    /// player is actually looking at.
    /// </summary>
    /// <remarks>
    /// The inverse is taken about the same point the renderer scales about - this element's own
    /// center, integer shift included - because anything else would agree with the drawn position
    /// only where the scale happens to be one.
    /// </remarks>
    internal sealed class FittedGroup : BaseElement
    {
        /// <inheritdoc />
        public override bool OnTouchDownXY(float tx, float ty)
        {
            return base.OnTouchDownXY(ToDesignX(tx), ToDesignY(ty));
        }

        /// <inheritdoc />
        public override bool OnTouchUpXY(float tx, float ty)
        {
            return base.OnTouchUpXY(ToDesignX(tx), ToDesignY(ty));
        }

        /// <inheritdoc />
        public override bool OnTouchMoveXY(float tx, float ty)
        {
            return base.OnTouchMoveXY(ToDesignX(tx), ToDesignY(ty));
        }

        /// <summary>
        /// Maps a logical X coordinate into this group's design space.
        /// </summary>
        /// <param name="tx">Logical X coordinate.</param>
        /// <returns>The corresponding design-space X coordinate.</returns>
        private float ToDesignX(float tx)
        {
            if (scaleX == 0f)
            {
                return tx;
            }

            float center = drawX + (width >> 1);
            return center + ((tx - center) / scaleX);
        }

        /// <summary>
        /// Maps a logical Y coordinate into this group's design space.
        /// </summary>
        /// <param name="ty">Logical Y coordinate.</param>
        /// <returns>The corresponding design-space Y coordinate.</returns>
        private float ToDesignY(float ty)
        {
            if (scaleY == 0f)
            {
                return ty;
            }

            float center = drawY + (height >> 1);
            return center + ((ty - center) / scaleY);
        }
    }
}
