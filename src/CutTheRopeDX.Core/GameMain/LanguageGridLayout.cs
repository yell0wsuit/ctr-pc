using System;

using CutTheRopeDX.Framework;

namespace CutTheRopeDX.GameMain
{
    /// <summary>
    /// How many language buttons the picker puts in a row.
    /// </summary>
    /// <remarks>
    /// Pure. The buttons are composed in design space and the group holding them is drawn at the
    /// content scale, so how many fit across is measured in that space: a row of three is wider
    /// than a phone screen once the group has grown, which is what ran the outer two columns off
    /// both edges.
    /// </remarks>
    internal static class LanguageGridLayout
    {
        /// <summary>
        /// Gets how many buttons fit in a row on a given viewport.
        /// </summary>
        /// <param name="visible">The logical region the viewport exposes.</param>
        /// <param name="scale">Scale the group holding the buttons is drawn at.</param>
        /// <param name="buttonWidth">Authored width of one button.</param>
        /// <returns>The number of buttons per row.</returns>
        public static int ColumnsFor(CTRRectangle visible, float scale, float buttonWidth)
        {
            float pitch = buttonWidth + ButtonSpacing;
            if (scale <= 0f || pitch <= 0f)
            {
                return MaxColumns;
            }

            // In the group's own units, which is where the buttons are laid out: the room the
            // screen has, taken back through the scale the group is drawn at. A row is one button
            // plus a pitch for each one after it.
            float room = ((visible.w - (EdgeMargin * 2f)) / scale) - buttonWidth;
            return Math.Clamp(1 + (int)(room / pitch), 1, MaxColumns);
        }

        /// <summary>Most buttons the picker puts in one row, which is what the design shows.</summary>
        public const int MaxColumns = 3;

        /// <summary>Authored gap between two buttons in a row; they overlap slightly.</summary>
        public const float ButtonSpacing = -10f;

        /// <summary>Distance the buttons keep from the sides of the screen.</summary>
        private const float EdgeMargin = 20f;
    }
}
