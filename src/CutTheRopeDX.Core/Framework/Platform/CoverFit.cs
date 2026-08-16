namespace CutTheRopeDX.Framework.Platform
{
    /// <summary>
    /// The result of covering a viewport with an image.
    /// </summary>
    /// <param name="Scale">Uniform scale at which the image covers the viewport completely.</param>
    /// <param name="DrivingAxis">
    /// The axis that determined <paramref name="Scale"/>, and therefore the axis along which the
    /// image exactly fits while the other overflows. Layout that anchors to a background edge
    /// needs this, because only the driving axis has an edge that coincides with the viewport's.
    /// </param>
    internal readonly record struct CoverFit(float Scale, LayoutAxis DrivingAxis);
}
