using System;

using CutTheRopeDX.Framework.Core;

namespace CutTheRopeDX.GameMain
{
    internal sealed partial class GameScene
    {
        /// <summary>
        /// Whether any candy body is still in play. Night-level sleep and lights-out loss must
        /// continue for split halves, and must not end the level while the only candy is briefly
        /// hidden inside a transporter.
        /// </summary>
        /// <returns>
        /// <see langword="true"/> while any eatable candy still has an active body or is hidden in
        /// transport; otherwise <see langword="false"/>.
        /// </returns>
        private bool AnyNightCandyBodyPresent()
        {
            for (int ci = 0; ci < candies.Count; ci++)
            {
                CandyContext ctx = candies[ci];
                if (!ctx.Capabilities.CanBeEaten)
                {
                    // A light bulb is a candy-like body that nobody is waiting to eat.
                    continue;
                }

                // One question per logical candy, however many bodies it has: a present candy and a
                // split candy with one surviving half both answer yes, and a candy inside a
                // transporter answers yes too because it is coming back out.
                if (ctx.Lifecycle.ActiveBodies.Count > 0
                    || ctx.Lifecycle.Presence == CandyPresence.Hidden)
                {
                    return true;
                }
            }

            return false;
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
        ///   <item><description>Collision between light emitters and the legacy split-candy halves</description></item>
        ///   <item><description>Removal of light emitters that fall off screen</description></item>
        ///   <item><description>Game over trigger when all light emitters are lost (night levels only)</description></item>
        /// </list>
        /// </remarks>
        private void UpdateLightEmitterPhysics()
        {
            // ResolveCandyCollisions pairs logical candies, so it never sees a split half. Each
            // surviving half still has to collide with every light emitter.
            foreach (CandyContext ctx in LightEmitters())
            {
                foreach (CandyBody body in ActiveCandyBodies(CandyInteraction.LightCollision))
                {
                    if (body.Role != CandyBodyRole.Whole)
                    {
                        HandleCandyIntersection(ctx.WholeBody.Point, body.Point, ctx.collisionDistanceOverride ?? LightBulbDefinition.CollisionDistance);
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
                if (!ctx.HasNoWholeBodyInPlay && PointOutOfScreen(ctx.WholeBody.Point))
                {
                    _ = TryRetireCandyBody(ctx.WholeBody, CandyRemovalReason.OffScreen);
                }
                // A bulb mid-teleport is Hidden for the brief transport window but is not lost: count
                // it as active so a lone emitter in a bamboo tube or hat does not trip the lights-out
                // loss the instant its light blinks out.
                hasActiveLightEmitter = hasActiveLightEmitter || !ctx.HasNoWholeBodyInPlay
                    || ctx.Lifecycle.Presence == CandyPresence.Hidden;
            }

            // Multi-candy/split-aware presence: the primary can be out of play while another candy
            // body is still around.
            if (nightLevel && !hasActiveLightEmitter && gameplayFlow.CanTriggerOutcome && AnyNightCandyBodyPresent())
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

                bool canUpdateSleepState = gameplayFlow.CanReactToCandy(t.Feeding.IsFed);

                bool isAwake = false;
                Vector targetPosition = Vect(t.targetObject.x, t.targetObject.y);
                foreach (CandyContext light in LightEmitters())
                {
                    if (LightProximity.IsWithinLight(targetPosition, light.WholeBody.Point.pos, light.lightRadius))
                    {
                        isAwake = true;
                        break;
                    }
                }

                if (hasCandyPresent && canUpdateSleepState)
                {
                    UpdateNightTargetAwake(t, isAwake);
                }

                bool isSleeping = !t.NightSleep.IsAwake && hasCandyPresent && canUpdateSleepState;
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
                    float pulseTime = t.NightSleep.PulseTime;
                    t.NightSleep.AdvancePulse(delta);

                    // Apply breathing pulse effect using sine wave (classic backend only;
                    // the Flash backend has its own sleeping timeline that includes the pulse).
                    if (t.NightSleep.Phase == NightSleepPhase.Pulsing
                        && t.controller?.HandlesOwnSleepPulse != true)
                    {
                        float sinValue = MathF.Sin(pulseTime * 2f);
                        float scaleY = 0.95f + ((sinValue + 1f) / 2f * 0.1f); // Scale between 0.95 and 1.05

                        if (t.controller?.IsSleepingAnimationPlaying() == true)
                        {
                            t.targetObject.rotationCenterY = 86f;
                            t.targetObject.scaleX = t.baseScaleX;
                            t.targetObject.scaleY = t.baseScaleY * scaleY;
                        }
                    }

                    if (t.NightSleep.AdvanceSound(delta, NightSleepSoundInterval))
                    {
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
                    if (LightProximity.IsWithinLight(Vect(star.x, star.y), light.WholeBody.Point.pos, light.lightRadius))
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
            float pulseDelay = t.controller?.GetSleepPulseDelaySeconds() ?? 0f;
            float pulseBaseY = t.targetObject == null
                ? 0f
                : GetSleepPulsePivotOffsetY(t.targetObject.height);
            NightSleepTransition transition = t.NightSleep.ObserveAwake(isAwake, pulseDelay, pulseBaseY);
            if (transition == NightSleepTransition.None)
            {
                return;
            }

            // Waking up: reset sleep state and play wake animation
            if (transition == NightSleepTransition.Woke)
            {
                if (t.targetObject != null && t.controller?.HandlesOwnSleepPulse != true)
                {
                    t.targetObject.scaleX = t.baseScaleX;
                    t.targetObject.scaleY = t.baseScaleY;
                    t.targetObject.rotationCenterX = 0f;
                    t.targetObject.rotationCenterY = 0f;
                }
                t.controller?.SetSleepOverlayVisible(false);
                t.controller?.PlayExcited();
                return;
            }

            bool hasCandyPresent = AnyNightCandyBodyPresent();
            if (!hasCandyPresent)
            {
                return;
            }

            // Falling asleep: start sleep animation and prepare pulse effect.
            t.controller?.SetSleepOverlayVisible(false);
            t.controller?.PlaySleeping();
            if (t.targetObject != null && t.controller?.HandlesOwnSleepPulse != true)
            {
                t.targetObject.rotationCenterY = t.NightSleep.PulseBaseY;
            }
        }

        /// <summary>
        /// Controls the visibility and playback of zzz animations.
        /// </summary>
        /// <param name="t">The Om Nom target context that owns the zzz animations.</param>
        /// <param name="visible">Whether the zzz animations should be visible.</param>
        private static void SetNightSleepVisibility(TargetContext t, bool visible)
        {
            if (!t.NightSleep.SetOverlayVisible(visible, t.Feeding.IsAsleep))
            {
                return;
            }

            t.controller?.SetSleepOverlayVisible(visible);
        }

    }
}
