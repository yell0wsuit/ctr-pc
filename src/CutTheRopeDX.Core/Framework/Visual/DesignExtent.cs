using System;

namespace CutTheRopeDX.Framework.Visual
{
    /// <summary>
    /// Measures what a group of design-space content actually paints, so a layout rule can be
    /// written against the content rather than against the box it was authored in.
    /// </summary>
    /// <remarks>
    /// Only elements that paint a box of their own are measured. The boxes between them are
    /// layout, not composition: a stack is authored the full width of the design box and centers
    /// buttons a third of it wide, and a button's own box is its pressed art's. Measuring those
    /// would report a composition several times the size of the one on screen.
    /// </remarks>
    internal static class DesignExtent
    {
        /// <summary>
        /// Returns the union of what <paramref name="group"/>'s descendants paint, in the group's
        /// own coordinates: the design box's origin is (0, 0), whatever the group's placement in
        /// logical space happens to be.
        /// </summary>
        /// <remarks>
        /// The group must already be sized to its design box, since that is what its children
        /// anchor against. Positions are resolved as the walk reaches them, parent before child,
        /// the way drawing resolves them - which also means hidden elements are measured
        /// correctly rather than skipped by a walk that only follows what is drawn.
        /// </remarks>
        /// <param name="group">Element holding the design-space content.</param>
        /// <returns>The painted extent, or an empty rectangle when nothing under it paints.</returns>
        public static CTRRectangle Measure(BaseElement group)
        {
            if (group == null)
            {
                return new CTRRectangle(0f, 0f, 0f, 0f);
            }

            float left = float.MaxValue;
            float top = float.MaxValue;
            float right = float.MinValue;
            float bottom = float.MinValue;

            foreach (BaseElement child in group.GetChilds().Values)
            {
                Union(child, ref left, ref top, ref right, ref bottom);
            }

            return right <= left || bottom <= top
                ? new CTRRectangle(0f, 0f, 0f, 0f)
                : new CTRRectangle(
                    left - group.drawX,
                    top - group.drawY,
                    right - left,
                    bottom - top);
        }

        /// <summary>
        /// Resolves one element's position, folds its painted box into the running union, and
        /// recurses into its children.
        /// </summary>
        /// <param name="element">Element to fold in, or <see langword="null"/>.</param>
        /// <param name="left">Running left edge.</param>
        /// <param name="top">Running top edge.</param>
        /// <param name="right">Running right edge.</param>
        /// <param name="bottom">Running bottom edge.</param>
        private static void Union(
            BaseElement element,
            ref float left,
            ref float top,
            ref float right,
            ref float bottom)
        {
            if (element == null || !element.visible)
            {
                return;
            }

            // Resolve before reading: a parent must be resolved before its children, because
            // CalculateTopLeft reads the parent's drawX and drawY.
            BaseElement.CalculateTopLeft(element);

            if (Paints(element))
            {
                // Elements scale about their own center, so a scaled one paints a box centered
                // where its unscaled one was.
                float centerX = element.drawX + (element.width / 2f);
                float centerY = element.drawY + (element.height / 2f);
                float halfWidth = element.width * MathF.Abs(element.scaleX) / 2f;
                float halfHeight = element.height * MathF.Abs(element.scaleY) / 2f;

                left = MathF.Min(left, centerX - halfWidth);
                top = MathF.Min(top, centerY - halfHeight);
                right = MathF.Max(right, centerX + halfWidth);
                bottom = MathF.Max(bottom, centerY + halfHeight);
            }

            foreach (BaseElement child in element.GetChilds().Values)
            {
                Union(child, ref left, ref top, ref right, ref bottom);
            }
        }

        /// <summary>
        /// Whether an element paints a box of its own, as opposed to only placing others.
        /// </summary>
        /// <param name="element">Element to classify.</param>
        /// <returns><see langword="true"/> when the element paints.</returns>
        private static bool Paints(BaseElement element)
        {
            return element is Image or Text or RectangleElement;
        }
    }
}
