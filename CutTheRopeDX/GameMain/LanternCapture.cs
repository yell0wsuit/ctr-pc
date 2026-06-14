namespace CutTheRopeDX.GameMain
{
    /// <summary>
    /// Pure capture-gate for lanterns. A candy is captured only when the lantern group is free
    /// (single-occupancy), this lantern is inactive, the candy exists and is not already captured,
    /// and it is within capture range. The range check is precomputed by the caller.
    /// </summary>
    internal static class LanternCapture
    {
        public static bool ShouldCapture(
            bool lanternInactive,
            bool groupOccupied,
            bool candyPresent,
            bool candyAlreadyInLantern,
            bool inRange)
        {
            return lanternInactive
                && !groupOccupied
                && candyPresent
                && !candyAlreadyInLantern
                && inRange;
        }
    }
}
