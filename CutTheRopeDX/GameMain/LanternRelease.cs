using System.Collections.Generic;

using CutTheRopeDX.Framework;
using CutTheRopeDX.Framework.Physics;

namespace CutTheRopeDX.GameMain
{
    /// <summary>
    /// Restores per-candy state after a lantern releases its captured candy.
    /// </summary>
    internal static class LanternRelease
    {
        public static int RestoreReleasedCandy(List<CandyContext> candies, ConstraintedPoint releasedPoint)
        {
            for (int ci = 0; ci < candies.Count; ci++)
            {
                CandyContext ctx = candies[ci];
                if (releasedPoint != null && ctx.point != releasedPoint)
                {
                    continue;
                }

                ctx.inLantern = false;
                ctx.candy.color = RGBAColor.solidOpaqueRGBA;
                ctx.candy.passTransformationsToChilds = false;
                ctx.candy.scaleX = ctx.candy.scaleY = 0.71f;
                ctx.candyMain.scaleX = ctx.candyMain.scaleY = 0.71f;
                ctx.candyTop.scaleX = ctx.candyTop.scaleY = 0.71f;

                return ci;
            }

            return -1;
        }
    }
}
