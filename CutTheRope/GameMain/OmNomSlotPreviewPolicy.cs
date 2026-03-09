namespace CutTheRope.GameMain
{
    internal enum OmNomSlotPreviewMode
    {
        ClassicStatic,
        ClassicAnimated,
        Xml,
    }

    internal static class OmNomSlotPreviewPolicy
    {
        public static OmNomSlotPreviewMode Resolve(int skinIndex, int selectedIndex)
        {
            return skinIndex == 0
                ? selectedIndex == 0
                    ? OmNomSlotPreviewMode.ClassicAnimated
                    : OmNomSlotPreviewMode.ClassicStatic
                : OmNomSlotPreviewMode.Xml;
        }
    }
}
