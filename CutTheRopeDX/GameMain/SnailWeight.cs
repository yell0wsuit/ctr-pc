namespace CutTheRopeDX.GameMain
{
    /// <summary>
    /// Pure weight bookkeeping for snails riding a candy point. Each attached snail adds
    /// <see cref="PerSnailWeight"/> to the point so the candy is dragged down; force-detaching a snail
    /// (hand grab, capture, etc.) must remove that exact amount, otherwise the candy keeps falling as if
    /// the snail were still attached. The result never drops below <see cref="MinWeight"/>, so a base
    /// candy weight (including a heavier rocket candy) is preserved rather than clobbered to a flat value.
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
