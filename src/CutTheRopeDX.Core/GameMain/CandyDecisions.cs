using System.Collections.Generic;
using System.Linq;

using CutTheRopeDX.Framework.Core;

namespace CutTheRopeDX.GameMain
{
    /// <summary>
    /// Pure win/loss/mouth decisions for the multi-candy model. No graphics or scene state.
    /// </summary>
    internal static class CandyDecisions
    {
        /// <summary>
        /// Determines whether any candy body in range should open a target's mouth. The caller
        /// snapshots only active bodies, so there is nothing here to skip as already gone.
        /// </summary>
        /// <param name="targetPos">World position of the target's mouth.</param>
        /// <param name="candies">Physical snapshots of the active bodies to test.</param>
        /// <param name="range">Mouth-opening radius.</param>
        /// <returns>
        /// <see langword="true"/> when at least one body that can open a mouth is within
        /// <paramref name="range"/>; otherwise <see langword="false"/>.
        /// </returns>
        public static bool ShouldOpenMouth(Vector targetPos, IReadOnlyList<CandyView> candies, float range)
        {
            if (candies == null)
            {
                return false;
            }
            float rangeSq = range * range;
            for (int i = 0; i < candies.Count; i++)
            {
                if (!candies[i].CanOpenMouth)
                {
                    continue;
                }
                float dx = candies[i].Position.X - targetPos.X;
                float dy = candies[i].Position.Y - targetPos.Y;
                if ((dx * dx) + (dy * dy) < rangeSq)
                {
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// Determines whether every candy that participates in the win condition
        /// was permanently removed as eaten.
        /// </summary>
        /// <param name="candies">The logical candy outcome snapshots to evaluate.</param>
        /// <returns>
        /// <see langword="true"/> when at least one candy exists and every eatable candy is
        /// <see cref="CandyPresence.Removed"/> with
        /// <see cref="CandyRemovalReason.Eaten"/>; otherwise <see langword="false"/>. This is the
        /// win gate, so a level with no candies at all never satisfies it.
        /// </returns>
        internal static bool AllEaten(IEnumerable<CandyOutcomeView> candies)
        {
            return candies.Any()
                && candies
                .Where(static candy => candy.CanBeEaten)
                .All(static candy =>
                    candy.Presence == CandyPresence.Removed
                    && candy.RemovalReason == CandyRemovalReason.Eaten);
        }

        /// <summary>
        /// Determines whether any candy has a failed removal caused by a hazard,
        /// spider, off-screen exit, or the loss of an owned split half.
        /// </summary>
        /// <param name="candies">The logical candy outcome snapshots to evaluate.</param>
        /// <returns>
        /// <see langword="true"/> when any candy records a failed removal;
        /// otherwise <see langword="false"/>.
        /// </returns>
        internal static bool AnyFailedRemoval(IEnumerable<CandyOutcomeView> candies)
        {
            return candies.Any(static candy =>
                candy.HasFailedSplitHalf
                || (candy.Presence == CandyPresence.Removed
                && candy.RemovalReason is CandyRemovalReason.Hazard
                    or CandyRemovalReason.Spider
                    or CandyRemovalReason.OffScreen));
        }
    }
}
