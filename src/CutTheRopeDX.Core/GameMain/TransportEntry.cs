namespace CutTheRopeDX.GameMain
{
    /// <summary>
    /// Pure entry-gate for candy transport (sock / bamboo tube). A candy may enter only when it
    /// exists, is not already in transit, is not captured in a lantern, is not in split-candy mode,
    /// and is within range. The range flag is precomputed by the caller (sock collision math /
    /// <c>BambooTube.TryCatchCandy</c>). Transport is multi-use: there is no group exclusivity, only
    /// the per-candy in-transit gate.
    /// </summary>
    internal static class TransportEntry
    {
        /// <summary>Decides whether a candy may enter a transporter this frame.</summary>
        /// <param name="candyPresent">Whether the candy still exists.</param>
        /// <param name="alreadyInTransit">
        /// Whether the candy is already hidden inside a transport session. Sock and bamboo transit
        /// are one state, so this is a single flag rather than one per transporter kind.
        /// </param>
        /// <param name="inLantern">Whether a lantern holds the candy.</param>
        /// <param name="splitActive">Whether the candy is the primary while it is split.</param>
        /// <param name="inRange">Whether the transporter's own range test passed.</param>
        /// <returns><see langword="true"/> when the candy may enter.</returns>
        public static bool ShouldEnter(
            bool candyPresent,
            bool alreadyInTransit,
            bool inLantern,
            bool splitActive,
            bool inRange)
        {
            return candyPresent
                && !alreadyInTransit
                && !inLantern
                && !splitActive
                && inRange;
        }
    }
}
