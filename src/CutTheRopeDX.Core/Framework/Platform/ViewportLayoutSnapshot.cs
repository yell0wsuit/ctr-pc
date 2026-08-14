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
    internal readonly record struct ViewportLayoutSnapshot(
        int SurfaceWidth,
        int SurfaceHeight,
        CTRRectangle LegacyContentBounds,
        float LegacyScale);
}
