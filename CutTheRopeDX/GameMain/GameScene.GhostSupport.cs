using CutTheRopeDX.Framework.Core;
using CutTheRopeDX.Framework.Physics;

namespace CutTheRopeDX.GameMain
{
    internal sealed partial class GameScene
    {
        /// <summary>
        /// Chooses the best candy constraint point for a ghost-created rope anchor.
        /// </summary>
        /// <param name="ghostPosition">World position of the ghost creating the rope.</param>
        /// <returns>The closest available candy constraint point, or a fallback candy point when none are active.</returns>
        internal ConstraintedPoint GetGhostRopeAnchor(Vector ghostPosition)
        {
            if (twoParts == 2)
            {
                return !noCandy && star != null ? star : star ?? starL ?? starR;
            }

            ConstraintedPoint best = null;
            float bestDistance = float.MaxValue;

            // Local helper for ranking active candy points by distance to the ghost.
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
