namespace CutTheRopeDX.GameMain
{
    /// <summary>
    /// Pure helper: is the primary candy still in play?
    /// </summary>
    /// <remarks>
    /// While the candy is split, the singleton <c>noCandy</c> flag is always <see langword="true"/>
    /// and presence is tracked by the per-half flags instead.
    /// </remarks>
    internal static class PrimaryCandyPresence
    {
        public static bool AnyPresent(bool splitActive, bool noCandy, bool noCandyLeft, bool noCandyRight)
        {
            return splitActive ? (!noCandyLeft || !noCandyRight) : !noCandy;
        }
    }
}
