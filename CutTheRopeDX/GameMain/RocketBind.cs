namespace CutTheRopeDX.GameMain
{
    /// <summary>
    /// Pure bind-gate for rockets. A rocket binds a candy only while the rocket is idle (one-time use:
    /// after it leaves idle it is permanently consumed), the candy exists, is not captured in a lantern,
    /// is not held by the active mouse, and physically intersects the rocket. Intersects is precomputed by the caller
    /// (<c>GameObject.ObjectsIntersectRotatedWithUnrotated</c>).
    /// </summary>
    internal static class RocketBind
    {
        public static bool ShouldBind(
            bool rocketIdle,
            bool candyPresent,
            bool candyInLantern,
            bool mouseHasCandy,
            bool intersects)
        {
            return rocketIdle
                && candyPresent
                && !candyInLantern
                && !mouseHasCandy
                && intersects;
        }
    }
}
