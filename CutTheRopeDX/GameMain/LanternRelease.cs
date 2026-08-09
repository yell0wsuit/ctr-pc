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
                CandyBody body = ctx.WholeBody;
                if (releasedPoint != null && body.Point != releasedPoint)
                {
                    continue;
                }

                ctx.Lifecycle.Attachments.ReleaseFromLantern();
                body.Visual.color = RGBAColor.solidOpaqueRGBA;
                body.Visual.passTransformationsToChilds = false;
                body.Visual.scaleX = body.Visual.scaleY = 0.71f;
                body.Main.scaleX = body.Main.scaleY = 0.71f;
                body.Top.scaleX = body.Top.scaleY = 0.71f;

                return ci;
            }

            return -1;
        }
    }
}
