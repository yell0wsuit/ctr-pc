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
            SetMoversHeld(timeFrozen);
            particlesAniPool.visible = !timeFrozen;
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
        /// Suspends or resumes path travel for objects whose iOS updates receive the shared mover
        /// gate. The original gate also checks for clock elements, which this port does not have.
        /// </summary>
        /// <param name="held">Whether path travel is suspended.</param>
        private void SetMoversHeld(bool held)
        {
            foreach (Spikes spike in spikes)
            {
                spike.moverHeld = held;
            }

            foreach (Bouncer bouncer in bouncers)
            {
                bouncer.moverHeld = held;
            }

            foreach (Sock sock in socks)
            {
                sock.moverHeld = held;
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
