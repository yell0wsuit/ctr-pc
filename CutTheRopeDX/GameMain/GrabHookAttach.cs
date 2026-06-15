namespace CutTheRopeDX.GameMain
{
    /// <summary>
    /// Pure attach-gate for the auto-attaching grab-radius hook. A hook attaches a candy only when it is
    /// a radius hook, has not yet created a rope (one-time use), the candy exists, and the candy is within
    /// range. The in-range value is precomputed by the caller
    /// (<c>VectDistance(hook, candy) &lt;= grab.radius + CandyGrabPadding</c>).
    /// </summary>
    internal static class GrabHookAttach
    {
        public static bool ShouldAttach(
            bool radiusEnabled,
            bool ropeAbsent,
            bool candyPresent,
            bool inRange)
        {
            return radiusEnabled
                && ropeAbsent
                && candyPresent
                && inRange;
        }
    }
}
