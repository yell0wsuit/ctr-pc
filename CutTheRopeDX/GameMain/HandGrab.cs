namespace CutTheRopeDX.GameMain
{
    /// <summary>
    /// Pure grab-gate for the mechanical hand. An idle hand grabs a candy only when the candy exists,
    /// is not captured in a lantern, is not in sock transit, and is within grab range. Bamboo transit
    /// is covered by <c>candyPresent</c>: bamboo entry sets <c>noCandy</c> for the transit while sock
    /// entry does not, which is why only the sock needs an explicit parameter. <c>inRange</c>
    /// is precomputed by the caller (distance &lt; <c>MechanicalHand.MH_GRAB_DISTANCE</c>). One candy per hand.
    /// </summary>
    internal static class HandGrab
    {
        public static bool ShouldGrab(
            bool handIdle,
            bool candyPresent,
            bool candyInLantern,
            bool candyInSock,
            bool inRange)
        {
            return handIdle
                && candyPresent
                && !candyInLantern
                && !candyInSock
                && inRange;
        }
    }
}
