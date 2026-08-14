namespace CutTheRopeDX.Framework.Platform
{
    /// <summary>
    /// An immutable description of one surface size and everything derived from it.
    /// Every consumer reads these values rather than recomputing its own aspect or offset
    /// variant, so rendering and input can never disagree about where content sits.
    /// </summary>
    /// <param name="SurfaceWidth">Drawable surface width in pixels.</param>
    /// <param name="SurfaceHeight">Drawable surface height in pixels.</param>
    /// <param name="LegacyContentBounds">
    /// Destination rectangle in surface pixels for a screen laid out against the fixed
    /// 2560x1440 design size.
    /// </param>
    /// <param name="LegacyScale">
    /// Uniform scale from design-size coordinates to <paramref name="LegacyContentBounds"/>.
    /// </param>
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
    /// <param name="Orientation">Which way round the viewport is.</param>
    internal readonly record struct ViewportLayoutSnapshot(
        int SurfaceWidth,
        int SurfaceHeight,
        CTRRectangle LegacyContentBounds,
        float LegacyScale,
        CTRRectangle RenderViewport,
        CTRRectangle VisibleBounds,
        float Scale,
        LayoutOrientation Orientation);
}
