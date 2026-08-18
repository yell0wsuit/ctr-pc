namespace CutTheRopeDX.Framework.Platform
{
    /// <summary>
    /// An immutable description of one surface size and everything derived from it.
    /// Every consumer reads these values rather than recomputing its own aspect or offset
    /// variant, so rendering and input can never disagree about where content sits.
    /// </summary>
    /// <param name="SurfaceWidth">Drawable surface width in pixels.</param>
    /// <param name="SurfaceHeight">Drawable surface height in pixels.</param>
    /// <param name="RenderViewport">
    /// Sub-rectangle of the surface the game draws into, in surface pixels. Equals the whole
    /// surface unless the aspect ratio falls outside the supported range, in which case it is
    /// the centered crop at the nearest supported limit.
    /// </param>
    /// <param name="VisibleBounds">
    /// <paramref name="RenderViewport"/> expressed in logical units, positioned at the origin.
    /// How much logical space the current viewport exposes.
    /// </param>
    /// <param name="Scale">Uniform scale from logical units to surface pixels.</param>
    /// <param name="DevicePixelRatio">
    /// Physical pixels per logical pixel on the host surface. Reported so chrome with a
    /// physical minimum size can honor it; no geometry in this record depends on it.
    /// </param>
    /// <param name="Orientation">Which way round the viewport is.</param>
    internal readonly record struct ViewportLayoutSnapshot(
        int SurfaceWidth,
        int SurfaceHeight,
        CTRRectangle RenderViewport,
        CTRRectangle VisibleBounds,
        float Scale,
        float DevicePixelRatio,
        LayoutOrientation Orientation)
    {
        /// <summary>
        /// Width-to-height ratio of the region the game draws into. Derived from
        /// <see cref="VisibleBounds"/> rather than the surface, so it is already inside the
        /// supported range and never describes a region that is cropped away.
        /// </summary>
        public float Aspect => VisibleBounds.w / VisibleBounds.h;

        /// <summary>
        /// Converts a rectangle in logical space to pixels in the render target a frame is drawn
        /// into.
        /// </summary>
        /// <remarks>
        /// The target is the drawn region's own size, with its own origin. Where that region sits
        /// on the surface - <see cref="RenderViewport"/>'s corner - is applied when the target is
        /// copied to the screen, so a rectangle measured in this space must not carry it as well:
        /// a scissor that did was pushed sideways by the width of the letterbox, and cut the edge
        /// off whatever it was meant to clip.
        /// </remarks>
        /// <param name="logical">Rectangle in logical space.</param>
        /// <returns>The same rectangle in render target pixels.</returns>
        public CTRRectangle ToRenderTarget(CTRRectangle logical)
        {
            return new CTRRectangle(
                logical.x * Scale,
                logical.y * Scale,
                logical.w * Scale,
                logical.h * Scale);
        }
    }
}
