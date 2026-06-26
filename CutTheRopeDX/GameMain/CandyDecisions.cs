using System;
using System.Collections.Generic;

using CutTheRopeDX.Framework.Core;

namespace CutTheRopeDX.GameMain
{
    /// <summary>
    /// Pure win/loss/mouth decisions for the multi-candy model. No graphics or scene state.
    /// </summary>
    internal static class CandyDecisions
    {
        /// <summary>Win condition: at least one candy exists and every candy is consumed.</summary>
        public static bool AllConsumed(IReadOnlyList<CandyView> candies)
        {
            if (candies == null || candies.Count == 0)
            {
                return false;
            }
            for (int i = 0; i < candies.Count; i++)
            {
                if (!candies[i].CanBeEaten)
                {
                    continue;
                }
                if (!candies[i].Consumed || candies[i].InTransport)
                {
                    return false;
                }
            }
            return true;
        }

        /// <summary>True when any edible candy body is still active or hidden in transport.</summary>
        public static bool AnyConsumablePresent(IReadOnlyList<CandyView> candies)
        {
            if (candies == null)
            {
                return false;
            }
            for (int i = 0; i < candies.Count; i++)
            {
                if (candies[i].CanBeEaten && (!candies[i].Consumed || candies[i].InTransport))
                {
                    return true;
                }
            }
            return false;
        }

        /// <summary>True when any candy body that participates in candy gameplay is still active.</summary>
        public static bool AnyCandyBodyPresent(IReadOnlyList<CandyView> candies, IReadOnlyList<CandyView> splitCandies)
        {
            return AnyCandyBodyPresent(candies) || AnyCandyBodyPresent(splitCandies);
        }

        private static bool AnyCandyBodyPresent(IReadOnlyList<CandyView> candies)
        {
            if (candies == null)
            {
                return false;
            }
            for (int i = 0; i < candies.Count; i++)
            {
                if (candies[i].CanBeEaten && (!candies[i].Consumed || candies[i].InTransport))
                {
                    return true;
                }
            }
            return false;
        }

        /// <summary>Loss condition: any not-yet-consumed candy is outside the play area.</summary>
        public static bool AnyUneatenOutOfScreen(IReadOnlyList<CandyView> candies, Func<Vector, bool> isOutOfScreen)
        {
            if (candies == null)
            {
                return false;
            }
            for (int i = 0; i < candies.Count; i++)
            {
                if (candies[i].CanLoseLevelWhenOffScreen
                    && !candies[i].Consumed
                    && isOutOfScreen(candies[i].Position))
                {
                    return true;
                }
            }
            return false;
        }

        /// <summary>Loss condition across independent candies and active split candy halves.</summary>
        public static bool AnyUneatenOutOfScreen(
            IReadOnlyList<CandyView> candies,
            IReadOnlyList<CandyView> splitCandies,
            Func<Vector, bool> isOutOfScreen)
        {
            return AnyUneatenOutOfScreen(candies, isOutOfScreen)
                || AnyUneatenOutOfScreen(splitCandies, isOutOfScreen);
        }

        /// <summary>True when any uneaten candy is within <paramref name="range"/> of the target.</summary>
        public static bool ShouldOpenMouth(Vector targetPos, IReadOnlyList<CandyView> candies, float range)
        {
            if (candies == null)
            {
                return false;
            }
            float rangeSq = range * range;
            for (int i = 0; i < candies.Count; i++)
            {
                if (candies[i].Consumed || !candies[i].CanOpenMouth)
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
    }
}
