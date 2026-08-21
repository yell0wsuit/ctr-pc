using System;

using CutTheRopeDX.Framework.Core;

namespace CutTheRopeDX.Framework.Platform
{
    /// <summary>
    /// Pure layout arithmetic. Every scene layout is expressed in terms of these, so a fit or
    /// anchor rule exists in exactly one place and can be tested without a renderer.
    /// </summary>
    internal static class LayoutMath
    {
        /// <summary>
        /// Returns where a design box of the given size lands in <paramref name="viewport"/> when
        /// drawn at <paramref name="scale"/> and centered on it.
        /// </summary>
        /// <remarks>
        /// The one placement rule every design-space composition resolves through, whether the
        /// scale came from containing the box, covering the viewport with it, or a scene's own
        /// choice. The returned rectangle is free to be larger than the viewport; what overflows
        /// is the caller's to crop or to let bleed.
        /// </remarks>
        /// <param name="designWidth">Width of the design box.</param>
        /// <param name="designHeight">Height of the design box.</param>
        /// <param name="viewport">Rectangle to center within.</param>
        /// <param name="scale">Uniform scale the box is drawn at.</param>
        /// <returns>The placed, centered rectangle.</returns>
        public static CTRRectangle PlaceBox(
            float designWidth,
            float designHeight,
            CTRRectangle viewport,
            float scale)
        {
            float width = designWidth * scale;
            float height = designHeight * scale;
            return new CTRRectangle(
                viewport.x + ((viewport.w - width) / 2f),
                viewport.y + ((viewport.h - height) / 2f),
                width,
                height);
        }

        /// <summary>
        /// Returns the uniform scale at which a design box of the given size fits entirely inside
        /// <paramref name="viewport"/>. The complement of <see cref="Cover"/>: the axis with less
        /// room drives it, and the other is left with slack.
        /// </summary>
        /// <param name="designWidth">Width of the design box.</param>
        /// <param name="designHeight">Height of the design box.</param>
        /// <param name="viewport">Rectangle to fit inside.</param>
        /// <returns>The containing scale.</returns>
        public static float Contain(float designWidth, float designHeight, CTRRectangle viewport)
        {
            return MathF.Min(viewport.w / designWidth, viewport.h / designHeight);
        }

        /// <summary>
        /// Returns the largest rectangle of the given aspect ratio that fits inside
        /// <paramref name="viewport"/>, centered.
        /// </summary>
        /// <param name="designWidth">Width of the design box.</param>
        /// <param name="designHeight">Height of the design box.</param>
        /// <param name="viewport">Rectangle to fit inside.</param>
        /// <returns>The fitted, centered rectangle.</returns>
        public static CTRRectangle FitInside(float designWidth, float designHeight, CTRRectangle viewport)
        {
            return PlaceBox(designWidth, designHeight, viewport, Contain(designWidth, designHeight, viewport));
        }

        /// <summary>
        /// Returns where a design box of the given size lands when scaled to cover
        /// <paramref name="viewport"/> completely and centered on it, so what overflows hangs off
        /// both ends of the driving axis equally.
        /// </summary>
        /// <param name="designWidth">Width of the design box.</param>
        /// <param name="designHeight">Height of the design box.</param>
        /// <param name="viewport">Rectangle to cover.</param>
        /// <returns>The covering, centered rectangle.</returns>
        public static CTRRectangle CoverInside(float designWidth, float designHeight, CTRRectangle viewport)
        {
            return PlaceBox(
                designWidth,
                designHeight,
                viewport,
                Cover(designWidth, designHeight, viewport).Scale);
        }

        /// <summary>
        /// Returns the uniform scale at which an image of the given size covers
        /// <paramref name="viewport"/> completely, and which axis determined it.
        /// </summary>
        /// <param name="imageWidth">Natural image width.</param>
        /// <param name="imageHeight">Natural image height.</param>
        /// <param name="viewport">Rectangle to cover.</param>
        /// <returns>The covering scale and its driving axis.</returns>
        public static CoverFit Cover(float imageWidth, float imageHeight, CTRRectangle viewport)
        {
            float horizontal = viewport.w / imageWidth;
            float vertical = viewport.h / imageHeight;
            return horizontal >= vertical
                ? new CoverFit(horizontal, LayoutAxis.Horizontal)
                : new CoverFit(vertical, LayoutAxis.Vertical);
        }

        /// <summary>
        /// Returns the top-left position at which an element of the given size sits against
        /// <paramref name="edge"/> of <paramref name="viewport"/>, inset from it.
        /// </summary>
        /// <param name="viewport">Rectangle to anchor against.</param>
        /// <param name="edge">Which point of the viewport to anchor to.</param>
        /// <param name="elementWidth">Width of the element being placed.</param>
        /// <param name="elementHeight">Height of the element being placed.</param>
        /// <param name="insetX">Horizontal distance from the anchored edge.</param>
        /// <param name="insetY">Vertical distance from the anchored edge.</param>
        /// <returns>The element's top-left position.</returns>
        public static Vector AnchorPosition(
            CTRRectangle viewport,
            LayoutEdge edge,
            float elementWidth,
            float elementHeight,
            float insetX,
            float insetY)
        {
            float x = edge switch
            {
                LayoutEdge.TopLeft or LayoutEdge.MiddleLeft or LayoutEdge.BottomLeft
                    => viewport.x + insetX,
                LayoutEdge.TopCenter or LayoutEdge.MiddleCenter or LayoutEdge.BottomCenter
                    => viewport.x + ((viewport.w - elementWidth) / 2f),
                LayoutEdge.TopRight or LayoutEdge.MiddleRight or LayoutEdge.BottomRight
                    => viewport.x + viewport.w - elementWidth - insetX,
                _ => throw new ArgumentOutOfRangeException(nameof(edge), edge, null),
            };
            float y = edge switch
            {
                LayoutEdge.TopLeft or LayoutEdge.TopCenter or LayoutEdge.TopRight
                    => viewport.y + insetY,
                LayoutEdge.MiddleLeft or LayoutEdge.MiddleCenter or LayoutEdge.MiddleRight
                    => viewport.y + ((viewport.h - elementHeight) / 2f),
                LayoutEdge.BottomLeft or LayoutEdge.BottomCenter or LayoutEdge.BottomRight
                    => viewport.y + viewport.h - elementHeight - insetY,
                _ => throw new ArgumentOutOfRangeException(nameof(edge), edge, null),
            };
            return new Vector(x, y);
        }

        /// <summary>
        /// Corrects a corner-relative offset so growing an element by <paramref name="scale"/>
        /// keeps its distance from the anchored corner scaling right along with it, instead of
        /// the element just enlarging in place.
        /// </summary>
        /// <remarks>
        /// <see cref="Visual.BaseElement"/> always scales about its own
        /// center regardless of anchor, so an edge-anchored offset needs a correction term to
        /// compensate. The direction depends on which edge: a left/top anchor's offset already
        /// measures distance from the origin directly, while a right/bottom anchor's offset is
        /// measured backwards from that far edge, which is what <paramref name="farEdge"/>
        /// selects between.
        /// </remarks>
        /// <param name="baseOffset">Authored offset from the anchored edge, at scale one.</param>
        /// <param name="dimension">The element's own width or height along this axis.</param>
        /// <param name="scale">Uniform scale the element is drawn at.</param>
        /// <param name="farEdge">
        /// Whether the anchored edge is the far one on this axis (right for X, bottom for Y)
        /// rather than the near one (left for X, top for Y).
        /// </param>
        /// <returns>The offset to assign so the element grows from its anchored corner.</returns>
        public static float CornerAnchoredOffset(float baseOffset, float dimension, float scale, bool farEdge)
        {
            float correction = dimension / 2f * (1f - scale);
            return (baseOffset * scale) + (farEdge ? correction : -correction);
        }

        /// <summary>
        /// Linearly remaps <paramref name="value"/> from one range to another. Used for the
        /// aspect-ratio breakpoints scene layouts interpolate across.
        /// </summary>
        /// <param name="value">Value to remap.</param>
        /// <param name="inMin">Start of the input range.</param>
        /// <param name="inMax">End of the input range.</param>
        /// <param name="outMin">Value returned at <paramref name="inMin"/>.</param>
        /// <param name="outMax">Value returned at <paramref name="inMax"/>.</param>
        /// <returns>The remapped value, unclamped.</returns>
        public static float Remap(float value, float inMin, float inMax, float outMin, float outMax)
        {
            return outMin + ((value - inMin) / (inMax - inMin) * (outMax - outMin));
        }
    }
}
