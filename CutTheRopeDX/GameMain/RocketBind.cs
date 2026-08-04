namespace CutTheRopeDX.GameMain
{
    /// <summary>
    /// Pure bind-gate for rockets. A rocket binds a candy only while the rocket is idle (one-time
    /// use: after it leaves idle it is permanently consumed), the candy exists, is not captured in
    /// a lantern, and physically intersects the rocket. A mouse-held candy IS bindable — the
    /// rocket coexists with the mouse via <see cref="RocketBindPath"/> (it steals from nobody).
    /// Intersects is precomputed by the caller.
    /// </summary>
    internal static class RocketBind
    {
        public static bool ShouldBind(
            bool rocketIdle,
            bool candyPresent,
            bool candyInLantern,
            bool intersects)
        {
            return rocketIdle
                && candyPresent
                && !candyInLantern
                && intersects;
        }
    }
}
