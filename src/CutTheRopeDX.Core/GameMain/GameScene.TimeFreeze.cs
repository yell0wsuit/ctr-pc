using CutTheRopeDX.Framework.Core;
using CutTheRopeDX.Framework.Helpers;
using CutTheRopeDX.Framework.Physics;

namespace CutTheRopeDX.GameMain
{
    internal sealed partial class GameScene
    {
        /// <summary>Finds the switcher under a world point.</summary>
        /// <param name="worldX">World-space X.</param>
        /// <param name="worldY">World-space Y.</param>
        /// <returns>The switcher under the point, or <see langword="null"/>.</returns>
        private PauseSwitcher PauseSwitcherAt(float worldX, float worldY)
        {
            if (pauseSwitchers == null)
            {
                return null;
            }

            foreach (PauseSwitcher switcher in pauseSwitchers)
            {
                if (switcher != null && GameObject.PointInObject(new Vector(worldX, worldY), switcher))
                {
                    return switcher;
                }
            }

            return null;
        }

        /// <summary>
        /// Stops time if it is running, restarts it if it is stopped, and updates the button face.
        /// </summary>
        /// <param name="switcher">The switcher that was pressed.</param>
        private void ToggleTimeFreeze(PauseSwitcher switcher)
        {
            timeFrozen = !timeFrozen;
            if (timeFrozen)
            {
                switcher.ShowFrozen();
            }
            else
            {
                switcher.ShowRunning();
            }
        }

        /// <summary>
        /// Rewinds every candy point to where it started the step and clears its motion, so a
        /// stopped world stays exactly where it was. Integration still runs; this undoes it,
        /// which keeps forces and constraints consistent when time restarts.
        /// </summary>
        private void HoldFrozenPoints()
        {
            foreach (CandyBody body in ActiveCandyBodies())
            {
                ConstraintedPoint point = body.Point;
                if (point == null)
                {
                    continue;
                }

                point.a = vectZero;
                point.v = vectZero;
                point.posDelta = vectZero;
                point.pos = point.prevPos;
            }
        }
    }
}
