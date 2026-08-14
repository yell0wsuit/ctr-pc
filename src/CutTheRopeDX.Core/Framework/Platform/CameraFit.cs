namespace CutTheRopeDX.Framework.Platform
{
    /// <summary>
    /// The result of fitting a camera to a viewport: how much to scale the world, and which
    /// region of it ends up visible.
    /// </summary>
    /// <param name="Scale">Uniform world-to-viewport scale.</param>
    /// <param name="VisibleWorld">Region of world space the viewport exposes.</param>
    internal readonly record struct CameraFit(float Scale, CTRRectangle VisibleWorld);
}
