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
                if (!candies[i].Consumed)
                {
                    return false;
                }
            }
            return true;
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
                if (!candies[i].Consumed && isOutOfScreen(candies[i].Position))
                {
                    return true;
                }
            }
            return false;
        }
    }
}
