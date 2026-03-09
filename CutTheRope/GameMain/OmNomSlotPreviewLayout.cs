namespace CutTheRope.GameMain
{
    internal readonly record struct OmNomSlotPreviewLayoutInfo(float Scale, float YOffset);

    internal static class OmNomSlotPreviewLayout
    {
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
