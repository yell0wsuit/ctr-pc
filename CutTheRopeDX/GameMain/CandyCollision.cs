namespace CutTheRopeDX.GameMain
{
    /// <summary>
    /// Pure candy-collision eligibility checks.
    /// </summary>
    internal static class CandyCollision
    {
        public static bool ShouldParticipate(bool noCandy, bool inBubble, bool inLantern)
        {
            return !noCandy && !inBubble && !inLantern;
        }
    }
}
