using System;

using CutTheRopeDX.Framework;

namespace CutTheRopeDX.GameMain
{
    /// <summary>
    /// How the pack picker's strip of boxes divides a viewport: the size one box is drawn at, how
    /// many fit across, and the scroll that keeps the selected one in the middle of them.
    /// </summary>
    /// <remarks>
    /// Pure, and derived from the viewport and the artwork's own width, so every part of the strip
    /// - the boxes, the frames down its edges, the arrows beside it and the hole Om Nom shows
    /// through - is measured from one set of numbers rather than each site multiplying the scale
    /// through on its own.
    /// </remarks>
    /// <param name="Scale">Uniform scale the strip is drawn at.</param>
    /// <param name="VisibleBoxes">How many boxes the strip shows at once.</param>
    /// <param name="BoxWidth">Width one box is drawn at.</param>
    /// <param name="Spacing">Gap between two boxes, which is negative: they overlap slightly.</param>
    internal readonly record struct PackStripLayout(
        float Scale,
        int VisibleBoxes,
        float BoxWidth,
        float Spacing)
    {
        /// <summary>
        /// Divides a viewport between the boxes of the pack strip.
        /// </summary>
        /// <param name="visible">The logical region the viewport exposes.</param>
        /// <param name="scale">Uniform scale to draw the strip at.</param>
        /// <param name="boxWidth">Authored width of one box, including its quad offset padding.</param>
        /// <returns>The layout for that viewport.</returns>
        public static PackStripLayout For(CTRRectangle visible, float scale, float boxWidth)
        {
            float scaledBoxWidth = boxWidth * scale;
            int fit = scaledBoxWidth > 0f
                ? (int)((visible.w - StripMargin) / scaledBoxWidth)
                : MaxVisibleBoxes;

            return new PackStripLayout(
                scale,
                Math.Clamp(fit, 1, MaxVisibleBoxes),
                scaledBoxWidth,
                AuthoredSpacing * scale);
        }

        /// <summary>Width of the strip: the boxes it shows, side by side.</summary>
        public float StripWidth => VisibleBoxes * BoxWidth;

        /// <summary>Scroll distance from one box to the next.</summary>
        public float Step => BoxWidth + Spacing;

        /// <summary>
        /// Scroll offset that centers the selected box in the strip.
        /// </summary>
        /// <remarks>
        /// The scroll points are laid out for a strip <see cref="MaxVisibleBoxes"/> boxes wide,
        /// where the selected box sits in the middle slot on its own. A narrower strip drops slots
        /// from both sides at once, so half a box of scroll per slot dropped puts the selected box
        /// back in the middle of what is left. Never negative: the strip bounces back from a
        /// scroll before its own start, so what centers the box is the run-up in front of it
        /// rather than scrolling past where the content begins.
        /// </remarks>
        public float PackOffset => (MaxVisibleBoxes - VisibleBoxes) * BoxWidth / 2f;

        /// <summary>
        /// Width of the run-up in front of the first box: a box wide, plus the overlap the gap
        /// after it takes straight back.
        /// </summary>
        /// <remarks>
        /// A run-up of exactly one box left every box sitting one overlap left of where the scroll
        /// points expect it, so the selected box was drawn that far left of the middle of the
        /// strip at every width - invisible between two neighbours, plain on a phone where it is
        /// the only box the strip shows.
        /// </remarks>
        public float LeadingSpacer => BoxWidth - Spacing;

        /// <summary>
        /// Where the selected box sits inside the strip once it has come to rest, which is the
        /// middle of it.
        /// </summary>
        public float SelectedBoxLeft => LeadingSpacer + Spacing - PackOffset;

        /// <summary>Gap between the strip's edge and the frame drawn outside it.</summary>
        public float FrameGap => AuthoredFrameGap * Scale;

        /// <summary>Gap between the strip's edge and the shadow drawn inside it.</summary>
        public float SeamGap => AuthoredSeamGap * Scale;

        /// <summary>Gap between the strip's edge and the navigation arrow beside it.</summary>
        public float ArrowGap => AuthoredArrowGap * Scale;

        /// <summary>
        /// Most boxes the strip shows at once, which is what the landscape design shows.
        /// </summary>
        public const int MaxVisibleBoxes = 3;

        /// <summary>
        /// Width the strip leaves to the screen around it, so the boxes it shows are never pressed
        /// against the edges the navigation arrows sit in.
        /// </summary>
        private const float StripMargin = 200f;

        /// <summary>Authored gap between two boxes; they overlap slightly.</summary>
        private const float AuthoredSpacing = -20f;

        /// <summary>Authored gap between the strip's edge and the frame outside it.</summary>
        private const float AuthoredFrameGap = 2f;

        /// <summary>Authored gap between the strip's edge and the shadow inside it.</summary>
        private const float AuthoredSeamGap = 3f;

        /// <summary>Authored gap between the strip's edge and the arrow beside it.</summary>
        private const float AuthoredArrowGap = 40f;
    }
}
