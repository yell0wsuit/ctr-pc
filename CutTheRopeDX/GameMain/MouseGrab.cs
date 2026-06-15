namespace CutTheRopeDX.GameMain
{
    /// <summary>
    /// Pure grab-gate for the mouse. The active mouse grabs a candy only when it is not already
    /// carrying (single-occupancy), the candy exists, and it is within range. The in-range value is
    /// precomputed by the caller (<c>MiceObject.IsActiveMouseInRange</c>).
    /// </summary>
    internal static class MouseGrab
    {
        public static bool ShouldGrab(bool mouseHasCandy, bool candyPresent, bool inRange)
        {
            return !mouseHasCandy && candyPresent && inRange;
        }
    }
}
