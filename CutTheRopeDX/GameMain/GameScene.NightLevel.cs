using System;
using System.Collections.Generic;

using CutTheRopeDX.Framework.Core;

namespace CutTheRopeDX.GameMain
{
    internal sealed partial class GameScene
    {
        /// <summary>
        /// Whether any candy body is still in play. Night-level sleep and lights-out loss must
        /// continue for split halves, while ignoring the inactive whole-candy slot during a split.
        /// </summary>
        private bool AnyNightCandyBodyPresent()
        {
            List<CandyView> candyViews = [];
            for (int ci = 0; ci < candies.Count; ci++)
            {
                if (ci == 0 && twoParts != 2)
                {
                    continue;
                }
                candyViews.Add(candies[ci].ToView());
            }

            List<CandyView> splitCandyViews = [];
            if (twoParts != 2)
            {
                splitCandyViews.Add(new CandyView(starL.pos, noCandyL));
                splitCandyViews.Add(new CandyView(starR.pos, noCandyR));
            }

            return CandyDecisions.AnyCandyBodyPresent(candyViews, splitCandyViews);
        }

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
        /// Per-frame upkeep for light emitters that the shared candy path does not cover.
        /// </summary>
        /// <remarks>
        /// Integration, the bulb visual's own Update, and whole-body collision are handled by the
        /// shared candy path (main candy loop + <see cref="ResolveCandyCollisions"/>). This method
        /// only handles:
        /// <list type="bullet">
        ///   <item><description>Syncing capture/sock state from the context onto the bulb visual</description></item>
        ///   <item><description>Collision between light emitters and the legacy split-candy halves</description></item>
        ///   <item><description>Removal of light emitters that fall off screen</description></item>
        ///   <item><description>Game over trigger when all light emitters are lost (night levels only)</description></item>
        /// </list>
        /// </remarks>
        private void UpdateLightEmitterPhysics()
        {
            // Integration, whole-body collision, and the bulb visual's own Update() are all owned by
            // the shared candy path now: the main candy loop integrates the point and calls
            // ctx.candy.Update(delta) (ctx.candy IS the bulb), and ResolveCandyCollisions() handles
            // collision. Re-stepping the point here double-integrated it (erratic swing); calling
            // Update() here too double-advanced the bulb's animations (bubble/firefly ran ~2x fast).
            // This loop only syncs capture/sock state from the context onto the bulb visual.
            foreach (CandyContext ctx in LightEmitters())
            {
                ctx.lightBulb?.SyncFromContext(ctx);
            }

            // Split candy halves are still legacy singleton points, so they need explicit
            // collision with light emitters. Whole-body collisions use ResolveCandyCollisions().
            if (twoParts != 2)
            {
                foreach (CandyContext ctx in LightEmitters())
                {
                    if (!noCandyL)
                    {
                        HandleCandyIntersection(ctx.point, starL, ctx.collisionDistanceOverride ?? LightBulbDefinition.CollisionDistance);
                    }
                    if (!noCandyR)
                    {
                        HandleCandyIntersection(ctx.point, starR, ctx.collisionDistanceOverride ?? LightBulbDefinition.CollisionDistance);
                    }
                }
            }

            bool hasActiveLightEmitter = false;
            for (int i = 0; i < candies.Count; i++)
            {
                CandyContext ctx = candies[i];
                if (!ctx.emitsLight)
                {
                    continue;
                }
                if (!ctx.noCandy && PointOutOfScreen(ctx.point))
                {
                    ctx.noCandy = true;
                    // A light emitter leaving the screen is a non-candy object escaping: release its
                    // rope and exhaust its bound rocket, matching C's generic-object off-screen loop.
                    ExhaustRocketForCandy(ctx);
                    ReleaseRopesForPoint(ctx.point);
                    ctx.lightBulb?.SyncFromContext(ctx);
                }
                // A bulb mid-teleport has noCandy == true for the brief transport window but is not
                // lost: count it as active so a lone emitter in a bamboo tube or hat does not trip the
                // lights-out loss the instant its light blinks out.
                hasActiveLightEmitter = hasActiveLightEmitter || !ctx.noCandy || ctx.InTransport;
            }

            // Multi-candy/split-aware presence: the primary noCandy flag can be true while another
            // candy body is still in play.
            if (nightLevel && !hasActiveLightEmitter && restartState != 0 && AnyNightCandyBodyPresent())
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

            bool hasCandyPresent = AnyNightCandyBodyPresent();
            for (int ti = 0; ti < targets.Count; ti++)
            {
                TargetContext t = targets[ti];
                if (t.targetObject == null)
                {
                    continue;
                }

                bool canUpdateSleepState = GameOutcomeTransition.CanReactToCandyOrLight(outcomeTransitionActive, t.asleep);

                bool isAwake = false;
                Vector targetPosition = Vect(t.targetObject.x, t.targetObject.y);
                foreach (CandyContext light in LightEmitters())
                {
                    if (LightProximity.IsWithinLight(targetPosition, light.point.pos, light.lightRadius))
                    {
                        isAwake = true;
                        break;
                    }
                }

                if (hasCandyPresent && canUpdateSleepState)
                {
                    UpdateNightTargetAwake(t, isAwake);
                }

                bool isSleeping = t.isNightTargetAwake == false && hasCandyPresent && canUpdateSleepState;
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
                            t.controller?.SkinDefinition,
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
                foreach (CandyContext light in LightEmitters())
                {
                    if (LightProximity.IsWithinLight(Vect(star.x, star.y), light.point.pos, light.lightRadius))
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

            bool hasCandyPresent = AnyNightCandyBodyPresent();
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

    }
}
