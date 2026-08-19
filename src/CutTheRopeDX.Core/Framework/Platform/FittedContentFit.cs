using System;

namespace CutTheRopeDX.Framework.Platform
{
    /// <summary>
    /// The ceiling <see cref="ContentFit"/>'s scale is held under so a fitted composition keeps a
    /// margin to the screen edge.
    /// </summary>
    /// <remarks>
    /// <see cref="ContentFit"/> answers "how far from the design shape is this viewport" and
    /// spends the room that departure opens up. It measures the shape alone, never the content,
    /// which is sound only while the content stays well inside the design box on both axes - true
    /// across a button column, false down a composition that hangs from the top and bottom edges
    /// of the box at once. This is where the content gets a say.
    /// </remarks>
    internal static class FittedContentFit
    {
        /// <summary>
        /// Logical units of clearance a fitted composition keeps to each screen edge.
        /// </summary>
        /// <remarks>
        /// Under the smallest inset any scene authors against its design box, so that on the
        /// design shape itself this asks for nothing and every layout rule still reduces to the
        /// constant it was authored with.
        /// </remarks>
        public const float EdgeMargin = 48f;

        /// <summary>
        /// Returns the largest scale, no greater than the one asked for, at which centered content
        /// still clears the viewport's edges by <paramref name="margin"/>.
        /// </summary>
        /// <remarks>
        /// This never asks for less than the authored size: content authored past the margin on
        /// the design shape itself is drawn as large as it always was. Giving back growth the
        /// viewport cannot hold is this function's job; shrinking the shipped composition is not,
        /// and a scene whose content can outgrow its own box that way caps itself against its own
        /// measurements. A <paramref name="desired"/> already below the authored size is passed
        /// through untouched for the same reason.
        /// </remarks>
        /// <param name="visible">The logical region the viewport exposes.</param>
        /// <param name="designBox">The coordinate box the content is authored in.</param>
        /// <param name="content">What the content actually occupies, in design coordinates.</param>
        /// <param name="desired">The scale the content would be drawn at unhindered.</param>
        /// <param name="margin">Logical units to keep between the content and each edge.</param>
        /// <returns>The scale to draw the content at.</returns>
        public static float ScaleFor(
            CTRRectangle visible,
            CTRRectangle designBox,
            CTRRectangle content,
            float desired,
            float margin)
        {
            if (content.w <= 0f || content.h <= 0f)
            {
                return desired;
            }

            // The group is scaled about the design box's center and that center is put on the
            // viewport's, so what reaches an edge first is the content's far side from it - which
            // is not half the content's extent unless the content happens to be centered too.
            float reachX = FarthestFrom(designBox.x + (designBox.w / 2f), content.x, content.x + content.w);
            float reachY = FarthestFrom(designBox.y + (designBox.h / 2f), content.y, content.y + content.h);

            float scale = MathF.Min(
                Allowed((visible.w / 2f) - margin, reachX),
                Allowed((visible.h / 2f) - margin, reachY));

            // The floor goes under this function's own cap, not under the caller's scale: a
            // scene that has already held itself below the authored size did so for a reason of
            // its own, and raising it back is not this function's to do.
            return MathF.Min(desired, MathF.Max(1f, scale));
        }

        /// <summary>
        /// How far the farther of two edges lies from a center.
        /// </summary>
        /// <param name="center">The point the content is scaled about.</param>
        /// <param name="near">One edge of the content.</param>
        /// <param name="far">The other edge of the content.</param>
        /// <returns>The larger of the two distances.</returns>
        private static float FarthestFrom(float center, float near, float far)
        {
            return MathF.Max(MathF.Abs(near - center), MathF.Abs(far - center));
        }

        /// <summary>
        /// The scale at which a reach fills the room available to it.
        /// </summary>
        /// <param name="room">Distance from the viewport's center to the margin.</param>
        /// <param name="reach">Distance from the content's center to its far edge, unscaled.</param>
        /// <returns>The scale that puts the one on the other, or an unbounded scale when the
        /// content has no reach on that axis.</returns>
        private static float Allowed(float room, float reach)
        {
            return reach <= 0f ? float.MaxValue : room / reach;
        }
    }
}
