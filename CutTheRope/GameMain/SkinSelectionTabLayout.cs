namespace CutTheRope.GameMain
{
    internal static class SkinSelectionTabLayout
    {
        public static float GetCenteredX(int tabIndex, int totalTabs, float tabStride)
        {
            float start = -((totalTabs - 1) * tabStride) / 2f;
            return start + (tabIndex * tabStride);
        }
    }
}
