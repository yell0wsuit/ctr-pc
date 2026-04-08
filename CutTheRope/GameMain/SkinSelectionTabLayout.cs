namespace CutTheRope.GameMain
{
    /// <summary>
    /// Layout helper for positioning skin selection tabs around the center of their parent.
    /// </summary>
    internal static class SkinSelectionTabLayout
    {
        /// <summary>
        /// Gets the centered x-position for a tab in an evenly spaced tab row.
        /// </summary>
        /// <param name="tabIndex">Zero-based tab index.</param>
        /// <param name="totalTabs">Total number of tabs in the row.</param>
        /// <param name="tabStride">Horizontal distance between adjacent tabs.</param>
        /// <returns>The centered x-position for the tab.</returns>
        public static float GetCenteredX(int tabIndex, int totalTabs, float tabStride)
        {
            float start = -((totalTabs - 1) * tabStride) / 2f;
            return start + (tabIndex * tabStride);
        }
    }
}
