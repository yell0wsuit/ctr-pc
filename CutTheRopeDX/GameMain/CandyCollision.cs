namespace CutTheRopeDX.GameMain
{
    /// <summary>
    /// Pure candy-collision eligibility checks.
    /// </summary>
    internal static class CandyCollision
    {
        public static bool ShouldParticipate(bool noCandy, bool inLantern)
        {
            return !noCandy && !inLantern;
        }

        public static float PairDistance(CandyContext a, CandyContext b)
        {
            return a.collisionDistanceOverride.HasValue || b.collisionDistanceOverride.HasValue
                ? System.MathF.Max(a.collisionDistanceOverride ?? 0f, b.collisionDistanceOverride ?? 0f)
                : a.collisionRadius + b.collisionRadius;
        }
    }
}
