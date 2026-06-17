using System;

using CutTheRopeDX.Framework.Core;
using CutTheRopeDX.Framework.Physics;

namespace CutTheRopeDX.GameMain
{
    internal sealed partial class GameScene
    {
        /// <summary>
        /// Calculates the Y offset for the sleep pulse animation pivot point.
        /// </summary>
        /// <param name="height">The height of the target object.</param>
        /// <returns>The Y offset from center for the rotation pivot.</returns>
        private static float GetSleepPulsePivotOffsetY(float height)
        {
            return (height * SleepPulsePivotYRatio) - (height / 2f);
        }

        /// <summary>
        /// Updates physics simulation for all light bulbs in the level.
        /// </summary>
        /// <param name="delta">Time elapsed since the last frame in seconds.</param>
        /// <remarks>
        /// This method handles:
        /// <list type="bullet">
        ///   <item><description>Constraint physics and relaxation for each light bulb</description></item>
        ///   <item><description>Collision between light bulbs and the candy</description></item>
        ///   <item><description>Collision between multiple light bulbs</description></item>
        ///   <item><description>Removal of light bulbs that fall off screen</description></item>
        ///   <item><description>Game over trigger when all light bulbs are lost (night levels only)</description></item>
        /// </list>
        /// </remarks>
        private void UpdateLightBulbPhysics(float delta)
        {
            if (lightBulbs.Count == 0)
            {
                return;
            }

            float timeStep = delta * ropePhysicsSpeed;
            foreach (LightBulb bulb in lightBulbs)
            {
                bulb.constraint.Update(timeStep);
                for (int i = 0; i < NightConstraintRelaxationSteps; i++)
                {
                    ConstraintedPoint.SatisfyConstraints(bulb.constraint);
                }
                bulb.SyncToConstraint();
                bulb.Update(delta);
            }

            // Light bulb collision with candy and other light bulbs
            float lightBulbCollisionDistance = 2.25f * STAR_RADIUS;
            for (int i = 0; i < lightBulbs.Count; i++)
            {
                LightBulb bulb = lightBulbs[i];
                if (bulb == null || bulb.attachedSock != null)
                {
                    continue;
                }
                // Resolve collision between light bulb and candy (skip a candy being teleported by its own sock)
                // Half candy mode: check collision with both candy halves. A split candy can't enter a
                // sock (TransportEntry blocks it), so there's no per-half sock gate here.
                if (twoParts != 2)
                {
                    if (!noCandyL)
                    {
                        HandleCandyIntersection(bulb.constraint, starL, lightBulbCollisionDistance);
                    }
                    if (!noCandyR)
                    {
                        HandleCandyIntersection(bulb.constraint, starR, lightBulbCollisionDistance);
                    }
                }
                // Full candy mode: check collision with every candy, each gated on its own sock
                else
                {
                    if (!noCandy && candies[0].targetSock == null)
                    {
                        HandleCandyIntersection(bulb.constraint, star, lightBulbCollisionDistance);
                    }
                    for (int ci = 1; ci < candies.Count; ci++)
                    {
                        CandyContext ctx = candies[ci];
                        if (!ctx.noCandy && ctx.targetSock == null)
                        {
                            HandleCandyIntersection(bulb.constraint, ctx.point, lightBulbCollisionDistance);
                        }
                    }
                }
                for (int j = i + 1; j < lightBulbs.Count; j++)
                {
                    LightBulb other = lightBulbs[j];
                    if (other == null || other.attachedSock != null)
                    {
                        continue;
                    }
                    HandleCandyIntersection(bulb.constraint, other.constraint, lightBulbCollisionDistance);
                }
            }

            foreach (LightBulb bulb in lightBulbs)
            {
                bulb.SyncToConstraint();
            }

            // Remove light bulbs that fall off screen
            for (int i = lightBulbs.Count - 1; i >= 0; i--)
            {
                LightBulb bulb = lightBulbs[i];
                if (bulb != null && PointOutOfScreen(bulb.constraint))
                {
                    _ = lightBulbs.Remove(bulb);
                }
            }

            if (nightLevel && lightBulbs.Count == 0 && restartState != 0 && !noCandy)
            {
                GameLost();
            }
        }

        /// <summary>
        /// Updates night level specific game logic each frame.
        /// </summary>
        /// <param name="delta">Time elapsed since the last frame in seconds.</param>
        /// <remarks>
        /// This method handles:
        /// <list type="bullet">
        ///   <item><description>Determining if Om Nom is illuminated by any light bulb</description></item>
        ///   <item><description>Transitioning between awake and sleeping states</description></item>
        ///   <item><description>Sleep breathing animation (pulse effect)</description></item>
        ///   <item><description>Playing sleep sounds at regular intervals</description></item>
        ///   <item><description>Updating star lit states based on light bulb proximity</description></item>
        ///   <item><description>Positioning zzz animations on Om Nom</description></item>
        /// </list>
        /// </remarks>
        private void UpdateNightLevel(float delta)
        {
            if (!nightLevel)
            {
                return;
            }

            bool hasCandyPresent = twoParts == 2 ? !noCandy : (!noCandyL || !noCandyR);
            for (int ti = 0; ti < targets.Count; ti++)
            {
                TargetContext t = targets[ti];
                if (t.targetObject == null)
                {
                    continue;
                }

                bool isAwake = false;
                Vector targetPosition = Vect(t.targetObject.x, t.targetObject.y);
                foreach (LightBulb bulb in lightBulbs)
                {
                    if (LightProximity.IsWithinLight(targetPosition, bulb.constraint.pos, bulb.lightRadius))
                    {
                        isAwake = true;
                        break;
                    }
                }

                if (hasCandyPresent)
                {
                    UpdateNightTargetAwake(t, isAwake);
                }

                bool isSleeping = t.isNightTargetAwake == false && hasCandyPresent && !gameLostTriggered;
                bool shouldShowSleepOverlay = isSleeping
                    && t.controller?.IsSleepingAnimationPlaying() == true;
                SetNightSleepVisibility(t, shouldShowSleepOverlay);

                if (shouldShowSleepOverlay)
                {
                    t.controller?.UpdateSleepOverlays(delta);
                    t.controller?.SyncSleepOverlayPosition(t.targetObject.x, t.targetObject.y);
                }

                // Handle sleeping state animations and sounds
                if (isSleeping)
                {
                    // Wait for sleep animation to finish before starting pulse
                    if (!t.sleepPulseActive)
                    {
                        t.sleepPulseDelay = MathF.Max(0f, t.sleepPulseDelay - delta);
                        if (t.sleepPulseDelay == 0f)
                        {
                            t.sleepPulseActive = true;
                        }
                    }

                    // Apply breathing pulse effect using sine wave (classic backend only;
                    // the Flash backend has its own sleeping timeline that includes the pulse).
                    if (t.sleepPulseActive && t.controller?.HandlesOwnSleepPulse != true)
                    {
                        float sinValue = MathF.Sin(t.sleepPulseTime * 2f);
                        float scaleY = 0.95f + ((sinValue + 1f) / 2f * 0.1f); // Scale between 0.95 and 1.05

                        if (t.controller?.IsSleepingAnimationPlaying() == true)
                        {
                            t.targetObject.rotationCenterY = 86f;
                            t.targetObject.scaleX = t.baseScaleX;
                            t.targetObject.scaleY = t.baseScaleY * scaleY;
                        }
                        t.sleepPulseTime += delta;
                    }
                    else if (t.sleepPulseActive)
                    {
                        t.sleepPulseTime += delta;
                    }

                    t.sleepSoundTimer += delta;
                    if (t.sleepSoundTimer > NightSleepSoundInterval)
                    {
                        t.sleepSoundTimer = 0f;
                        CTRSoundMgr.PlayRandomOmNomSound(
                            Resources.Snd.MonsterSleep1,
                            Resources.Snd.MonsterSleep2,
                            Resources.Snd.MonsterSleep3);
                    }
                }
            }

            // Update star lit states based on proximity to light bulbs
            foreach (Star star in stars)
            {
                if (star == null)
                {
                    continue;
                }
                bool lit = false;
                foreach (LightBulb bulb in lightBulbs)
                {
                    if (LightProximity.IsWithinLight(Vect(star.x, star.y), bulb.constraint.pos, bulb.lightRadius))
                    {
                        lit = true;
                        break;
                    }
                }
                star.SetLitState(lit);
            }

        }

        /// <summary>
        /// Handles transitions between Om Nom's awake and sleeping states.
        /// </summary>
        /// <param name="t">The Om Nom target context to update.</param>
        /// <param name="isAwake">Whether Om Nom should be awake (illuminated by a light bulb).</param>
        /// <remarks>
        /// When waking up, resets all sleep animation state and plays the wake animation.
        /// When falling asleep, starts the sleep animation and prepares the breathing pulse effect.
        /// </remarks>
        private void UpdateNightTargetAwake(TargetContext t, bool isAwake)
        {
            if (t.isNightTargetAwake == isAwake)
            {
                return;
            }

            t.isNightTargetAwake = isAwake;

            // Waking up: reset sleep state and play wake animation
            if (isAwake)
            {
                t.sleepPulseActive = false;
                t.sleepPulseTime = 0f;
                t.sleepPulseDelay = 0f;
                t.sleepSoundTimer = 0f;
                t.sleepPulseBaseY = 0f;
                if (t.targetObject != null && t.controller?.HandlesOwnSleepPulse != true)
                {
                    t.targetObject.scaleX = t.baseScaleX;
                    t.targetObject.scaleY = t.baseScaleY;
                    t.targetObject.rotationCenterX = 0f;
                    t.targetObject.rotationCenterY = 0f;
                }
                SetNightSleepVisibility(t, false);
                t.controller?.PlayExcited();
                return;
            }

            bool hasCandyPresent = twoParts == 2 ? !noCandy : (!noCandyL || !noCandyR);
            if (!hasCandyPresent)
            {
                return;
            }

            // Falling asleep: start sleep animation and prepare pulse effect
            t.sleepPulseActive = false;
            t.sleepPulseTime = 0f;
            t.sleepPulseDelay = t.controller?.GetSleepPulseDelaySeconds() ?? 0f;
            t.sleepSoundTimer = 0.9f;
            SetNightSleepVisibility(t, false);
            t.controller?.PlaySleeping();
            if (t.targetObject != null && t.controller?.HandlesOwnSleepPulse != true)
            {
                t.sleepPulseBaseY = GetSleepPulsePivotOffsetY(t.targetObject.height);
                t.targetObject.rotationCenterY = t.sleepPulseBaseY;
            }
        }

        /// <summary>
        /// Controls the visibility and playback of zzz animations.
        /// </summary>
        /// <param name="t">The Om Nom target context that owns the zzz animations.</param>
        /// <param name="visible">Whether the zzz animations should be visible.</param>
        private static void SetNightSleepVisibility(TargetContext t, bool visible)
        {
            if (t.nightSleepOverlayVisible == visible)
            {
                return;
            }

            t.nightSleepOverlayVisible = visible;
            t.controller?.SetSleepOverlayVisible(visible);
        }

        /// <summary>
        /// Controls the visibility and playback of zzz animations for every Om Nom.
        /// </summary>
        /// <param name="visible">Whether the zzz animations should be visible.</param>
        private void SetAllNightSleepVisibility(bool visible)
        {
            for (int ti = 0; ti < targets.Count; ti++)
            {
                SetNightSleepVisibility(targets[ti], visible);
            }
        }
    }
}
