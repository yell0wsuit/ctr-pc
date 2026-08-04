namespace CutTheRopeDX.GameMain
{
    /// <summary>
    /// Pure weight bookkeeping for snails riding a candy point. Each attached snail adds
    /// <see cref="PerSnailWeight"/> to the point so the candy is dragged down; force-detaching a snail
    /// (hand grab, capture, etc.) must remove that exact amount, otherwise the candy keeps falling as if
    /// the snail were still attached. iOS instead assigns a flat 1.0 at its two reset sites (boarding the
    /// ant conveyor and tapping the candy); subtracting reaches the same number here, because every candy
    /// point is created at <see cref="MinWeight"/> and a snail is the only thing that ever adds to it -
    /// a rocket carries its own point, not a heavier candy. The floor keeps that true if it ever changes.
    /// </summary>
    internal static class SnailWeight
    {
        /// <summary>Weight each attached snail adds to the candy point (see GameScene snail attach).</summary>
        public const float PerSnailWeight = 3f;

        /// <summary>Lowest weight a candy point retains after its snails are removed.</summary>
        public const float MinWeight = 1f;

        /// <summary>
        /// Weight a candy point should carry after <paramref name="detachedSnails"/> snails are removed.
        /// </summary>
        /// <param name="weight">Current point weight (base weight plus each attached snail's contribution).</param>
        /// <param name="detachedSnails">Number of snails being force-detached from the point.</param>
        /// <returns>The restored point weight, floored at <see cref="MinWeight"/>.</returns>
        public static float AfterForceDetach(float weight, int detachedSnails)
        {
            float restored = weight - (PerSnailWeight * detachedSnails);
            return restored < MinWeight ? MinWeight : restored;
        }
    }
}
