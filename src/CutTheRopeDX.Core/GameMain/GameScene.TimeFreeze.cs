using CutTheRopeDX.Framework.Core;
using CutTheRopeDX.Framework.Helpers;
using CutTheRopeDX.Framework.Physics;

namespace CutTheRopeDX.GameMain
{
    internal sealed partial class GameScene
    {
        /// <summary>One pointer's hold on a time-freeze button, from press to release.</summary>
        /// <param name="PointerIndex">Pointer that pressed the button.</param>
        /// <param name="Switcher">Button the pointer pressed.</param>
        private readonly record struct PauseSwitcherTouch(int PointerIndex, PauseSwitcher Switcher);

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
            particlesAniPool.updateable = !timeFrozen;
            if (timeFrozen)
            {
                switcher.ShowFrozen();
                pauseSwitcherWaves?.PlayFadeIn();
                StopLoopingMoverSounds();
                CTRSoundMgr.PlaySound(Resources.Snd.PauseDown);
            }
            else
            {
                switcher.ShowRunning();
                pauseSwitcherWaves?.PlayFadeOut();
                RestartLoopingMoverSounds();
                CTRSoundMgr.PlaySound(Resources.Snd.PauseUp);
            }
        }

        /// <summary>Silences looping sounds whose gameplay sources stop when time is frozen.</summary>
        private void StopLoopingMoverSounds()
        {
            foreach (Spikes spike in spikes)
            {
                spike.SuspendElectricLoop();
            }

            if (rockets == null)
            {
                return;
            }

            foreach (Rocket rocket in rockets)
            {
                if (rocket?.flyLoopSound == null)
                {
                    continue;
                }

                CTRSoundMgr.StopLoopedSound(rocket.flyLoopSound);
                rocket.flyLoopSound = null;
            }
        }

        /// <summary>Restarts looping sounds for sources that remain active when time resumes.</summary>
        private void RestartLoopingMoverSounds()
        {
            foreach (Spikes spike in spikes)
            {
                spike.ResumeElectricLoop();
            }

            if (rockets == null)
            {
                return;
            }

            foreach (Rocket rocket in rockets)
            {
                if (rocket != null
                    && rocket.flyLoopSound == null
                    && RocketBoundCandy(rocket) != null)
                {
                    rocket.flyLoopSound = CTRSoundMgr.PlaySoundLooped(Resources.Snd.ExpRocketFlyLooped);
                }
            }
        }

        /// <summary>
        /// Rewinds every candy point to its previous position and clears motion accumulated by
        /// constraints and other frozen-step interactions. Normal point integration is skipped
        /// earlier in the step; this final hold mirrors the iOS cleanup pass.
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
