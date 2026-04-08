namespace CutTheRope.GameMain
{
    /// <summary>
    /// Scale and vertical offset used when rendering an Om Nom slot preview.
    /// </summary>
    /// <param name="Scale">Scale applied to the preview object.</param>
    /// <param name="YOffset">Vertical offset applied to the preview object.</param>
    internal readonly record struct OmNomSlotPreviewLayoutInfo(float Scale, float YOffset);

    /// <summary>
    /// Resolves layout values for Om Nom slot preview modes.
    /// </summary>
    internal static class OmNomSlotPreviewLayout
    {
        /// <summary>
        /// Gets the layout values for an Om Nom preview mode.
        /// </summary>
        /// <param name="previewMode">Preview mode to lay out.</param>
        /// <returns>The scale and vertical offset for the preview mode.</returns>
        public static OmNomSlotPreviewLayoutInfo Resolve(OmNomSlotPreviewMode previewMode)
        {
            return previewMode switch
            {
                OmNomSlotPreviewMode.ClassicAnimated => new(0.75f, -10f),
                OmNomSlotPreviewMode.ClassicStatic => new(0.75f, -10f),
                OmNomSlotPreviewMode.Xml => new(1.25f, -10f),
                _ => new(1f, 0f),
            };
        }
    }
}
