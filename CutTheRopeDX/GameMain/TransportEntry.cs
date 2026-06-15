namespace CutTheRopeDX.GameMain
{
    /// <summary>
    /// Pure entry-gate for candy transport (sock / bamboo tube). A candy may enter only when it
    /// exists, is not already in transit (sock or bamboo), is not captured in a lantern, is not in
    /// split-candy mode, and is within range. The range flag is precomputed by the caller
    /// (sock collision math / <c>BambooTube.TryCatchCandy</c>). Transport is multi-use: there is no
    /// group exclusivity, only the per-candy in-transit gate.
    /// </summary>
    internal static class TransportEntry
    {
        public static bool ShouldEnter(
            bool candyPresent,
            bool alreadyInSock,
            bool alreadyInBamboo,
            bool inLantern,
            bool splitActive,
            bool inRange)
        {
            return candyPresent
                && !alreadyInSock
                && !alreadyInBamboo
                && !inLantern
                && !splitActive
                && inRange;
        }
    }
}
