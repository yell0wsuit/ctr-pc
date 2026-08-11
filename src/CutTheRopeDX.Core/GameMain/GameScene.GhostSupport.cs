using CutTheRopeDX.Framework.Core;
using CutTheRopeDX.Framework.Physics;

namespace CutTheRopeDX.GameMain
{
    internal sealed partial class GameScene
    {
        /// <summary>
        /// Chooses the candy constraint point a ghost-created rope anchors on. A split candy offers
        /// its surviving halves, ranked by distance to the ghost; every other level anchors on the
        /// primary candy's own point, exactly as the single-candy engine did.
        /// </summary>
        /// <param name="ghostPosition">World position of the ghost creating the rope.</param>
        /// <returns>The closest split half, or the primary candy's point when none is offered.</returns>
        internal ConstraintedPoint GetGhostRopeAnchor(Vector ghostPosition)
        {
            ConstraintedPoint best = null;
            float bestDistance = float.MaxValue;

            foreach (CandyBody body in ActiveCandyBodies())
            {
                if (body.Role == CandyBodyRole.Whole)
                {
                    continue;
                }

                float distance = VectLength(VectSub(ghostPosition, body.Point.pos));
                if (distance < bestDistance)
                {
                    bestDistance = distance;
                    best = body.Point;
                }
            }

            return best ?? star;
        }
    }
}
