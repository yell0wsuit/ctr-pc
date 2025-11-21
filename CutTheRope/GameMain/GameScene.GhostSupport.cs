using CutTheRope.Framework.Core;
using CutTheRope.Framework.Sfe;

namespace CutTheRope.GameMain
{
    /// <summary>
    /// Ghost-specific helpers for GameScene.
    /// Provides utilities for auto-attaching ghost grabs to the active candy constraint.
    /// </summary>
    internal sealed partial class GameScene
    {
        internal ConstraintedPoint GetGhostRopeAnchor(Vector ghostPosition)
        {
            if (twoParts == 2)
            {
                return !noCandy && star != null ? star : star ?? starL ?? starR;
            }

            ConstraintedPoint best = null;
            float bestDistance = float.MaxValue;

            void Consider(ConstraintedPoint candidate, bool candyMissing)
            {
                if (candidate == null || candyMissing)
                {
                    return;
                }

                float distance = VectLength(VectSub(ghostPosition, candidate.pos));
                if (distance < bestDistance)
                {
                    bestDistance = distance;
                    best = candidate;
                }
            }

            Consider(starL, noCandyL);
            Consider(starR, noCandyR);

            return best ?? (!noCandy && star != null ? star : star ?? starL ?? starR);
        }
    }
}
