using System;

using CutTheRope.Framework.Core;
using CutTheRope.Framework.Sfe;
using CutTheRope.Framework.Visual;

namespace CutTheRope.GameMain
{
    /// <summary>
    /// Partial class containing night level specific logic for the game scene.
    /// </summary>
    /// <remarks>
    /// Night levels feature a sleeping Om Nom that must be illuminated by light bulbs
    /// to wake up. Stars also require illumination to be collected.
    /// </remarks>
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
                LightBulb bulb = lightBulbs.ObjectAtIndex(i);
                if (bulb == null || bulb.attachedSock != null)
                {
                    continue;
                }
                if (!noCandy && targetSock == null)
                {
                    ResolveConstraintCollision(bulb.constraint, star, lightBulbCollisionDistance);
                }
                for (int j = i + 1; j < lightBulbs.Count; j++)
                {
                    LightBulb other = lightBulbs.ObjectAtIndex(j);
                    if (other == null || other.attachedSock != null)
                    {
                        continue;
                    }
                    ResolveConstraintCollision(bulb.constraint, other.constraint, lightBulbCollisionDistance);
                }
            }

            foreach (LightBulb bulb in lightBulbs)
            {
                bulb.SyncToConstraint();
            }

            // Remove light bulbs that fall off screen
            for (int i = lightBulbs.Count - 1; i >= 0; i--)
            {
                LightBulb bulb = lightBulbs.ObjectAtIndex(i);
                if (bulb != null && PointOutOfScreen(bulb.constraint))
                {
                    lightBulbs.RemoveObject(bulb);
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

            sleepAnimPrimary?.Update(delta);
            sleepAnimSecondary?.Update(delta);

            // Check if any light bulb is close enough to wake Om Nom
            bool isAwake = false;
            Vector targetPosition = Vect(target.x, target.y);
            foreach (LightBulb bulb in lightBulbs)
            {
                if (VectDistance(bulb.constraint.pos, targetPosition) < bulb.lightRadius)
                {
                    isAwake = true;
                    break;
                }
            }

            bool hasCandyPresent = twoParts == 2 ? !noCandy : (!noCandyL || !noCandyR);
            if (hasCandyPresent)
            {
                UpdateNightTargetAwake(isAwake);
            }

            // Handle sleeping state animations and sounds
            if (isNightTargetAwake == false && hasCandyPresent && !gameLostTriggered)
            {
                // Wait for sleep animation to finish before starting pulse
                if (!sleepPulseActive)
                {
                    sleepPulseDelay = MathF.Max(0f, sleepPulseDelay - delta);
                    if (sleepPulseDelay == 0f)
                    {
                        sleepPulseActive = true;
                    }
                }

                // Apply breathing pulse effect using sine wave
                if (sleepPulseActive)
                {
                    float sinValue = (float)Math.Sin(sleepPulseTime * 2f);
                    float scaleY = 0.95f + ((sinValue + 1f) / 2f * 0.1f); // Scale between 0.95 and 1.05

                    Animation sleepAnimation = target.GetAnimation(Resources.Img.CharAnimationsSleeping);
                    if (sleepAnimation != null && sleepAnimation.GetCurrentTimelineIndex() == CharAnimationSleeping)
                    {
                        target.rotationCenterY = 86f;
                        target.scaleY = scaleY;
                    }
                    sleepPulseTime += delta;
                }

                sleepSoundTimer += delta;
                if (sleepSoundTimer > NightSleepSoundInterval)
                {
                    sleepSoundTimer = 0f;
                    CTRSoundMgr.PlayRandomSound(
                        Resources.Snd.MonsterSleep1,
                        Resources.Snd.MonsterSleep2,
                        Resources.Snd.MonsterSleep3);
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
                    if (VectDistance(bulb.constraint.pos, Vect(star.x, star.y)) < bulb.lightRadius)
                    {
                        lit = true;
                        break;
                    }
                }
                star.SetLitState(lit);
            }

            // Keep zzz animations positioned on Om Nom
            if (sleepAnimPrimary != null)
            {
                sleepAnimPrimary.x = target.x;
                sleepAnimPrimary.y = target.y;
            }
            if (sleepAnimSecondary != null)
            {
                sleepAnimSecondary.x = target.x;
                sleepAnimSecondary.y = target.y;
            }
        }

        /// <summary>
        /// Handles transitions between Om Nom's awake and sleeping states.
        /// </summary>
        /// <param name="isAwake">Whether Om Nom should be awake (illuminated by a light bulb).</param>
        /// <remarks>
        /// When waking up, resets all sleep animation state and plays the wake animation.
        /// When falling asleep, starts the sleep animation and prepares the breathing pulse effect.
        /// </remarks>
        private void UpdateNightTargetAwake(bool isAwake)
        {
            if (isNightTargetAwake == isAwake)
            {
                return;
            }

            isNightTargetAwake = isAwake;

            // Waking up: reset sleep state and play wake animation
            if (isAwake)
            {
                sleepPulseActive = false;
                sleepPulseTime = 0f;
                sleepPulseDelay = 0f;
                sleepSoundTimer = 0f;
                sleepPulseBaseY = 0f;
                target.scaleX = 1f;
                target.scaleY = 1f;
                target.rotationCenterX = 0f;
                target.rotationCenterY = 0f;
                SetNightSleepVisibility(false);
                target.PlayAnimationtimeline(Resources.Img.CharAnimations2, 3);  // excited animation
                return;
            }

            bool hasCandyPresent = twoParts == 2 ? !noCandy : (!noCandyL || !noCandyR);
            if (!hasCandyPresent)
            {
                return;
            }

            // Falling asleep: start sleep animation and prepare pulse effect
            sleepPulseActive = false;
            sleepPulseTime = 0f;
            sleepPulseDelay = SleepAnimFrameDelay * (SleepAnimEnd - SleepAnimStart + 1);
            sleepSoundTimer = 0f;
            SetNightSleepVisibility(true);
            target.PlayAnimationtimeline(Resources.Img.CharAnimationsSleeping, CharAnimationSleeping);
            sleepPulseBaseY = GetSleepPulsePivotOffsetY(target.height);
            target.rotationCenterY = sleepPulseBaseY;
        }

        /// <summary>
        /// Controls the visibility and playback of zzz animations.
        /// </summary>
        /// <param name="visible">Whether the zzz animations should be visible.</param>
        private void SetNightSleepVisibility(bool visible)
        {
            if (sleepAnimPrimary != null)
            {
                sleepAnimPrimary.visible = visible;
                if (visible)
                {
                    sleepAnimPrimary.PlayTimeline(0);
                }
                else
                {
                    sleepAnimPrimary.GetTimeline(0)?.StopTimeline();
                }
            }
            if (sleepAnimSecondary != null)
            {
                sleepAnimSecondary.visible = visible;
                if (visible)
                {
                    sleepAnimSecondary.PlayTimeline(0);
                }
                else
                {
                    sleepAnimSecondary.GetTimeline(0)?.StopTimeline();
                }
            }
        }

        /// <summary>
        /// Resolves collision between two constraint points by separating them
        /// and exchanging velocities.
        /// </summary>
        /// <param name="a">The first constraint point.</param>
        /// <param name="b">The second constraint point.</param>
        /// <param name="minDistance">The minimum allowed distance between the points.</param>
        /// <remarks>
        /// Uses an elastic collision model that conserves momentum. For slow-moving
        /// or stationary objects, simply separates them without velocity exchange.
        /// For fast collisions, calculates proper velocity exchange based on
        /// collision normal and tangent components.
        /// </remarks>
        private static void ResolveConstraintCollision(ConstraintedPoint a, ConstraintedPoint b, float minDistance)
        {
            Vector delta = VectSub(a.pos, b.pos);
            float dist = VectLength(delta);

            if (dist >= minDistance)
            {
                return;
            }

            // Handle overlapping points by using arbitrary separation direction
            if (dist == 0f)
            {
                delta = Vect(1f, 0f);
                dist = 1f;
            }

            float overlap = minDistance - dist;
            float speedSum = VectLength(a.v) + VectLength(b.v);

            // For slow collisions, just separate without velocity exchange
            if (speedSum <= 0f || overlap < 1000f / speedSum * 2f)
            {
                float normX = delta.x / dist;
                float normY = delta.y / dist;
                float offset = overlap / 2f;
                a.pos.x += normX * offset;
                a.pos.y += normY * offset;
                b.pos.x -= normX * offset;
                b.pos.y -= normY * offset;
                return;
            }

            // Fast collision: calculate elastic velocity exchange
            Vector g = VectSub(b.pos, a.pos);
            float h = -g.y;
            float m = g.x;
            float f = ((a.v.x * g.x) + (a.v.y * g.y)) / minDistance;
            float e = ((a.v.x * h) + (a.v.y * m)) / minDistance;
            h = ((b.v.x * h) + (a.v.x * m)) / minDistance;
            m = f;
            f = ((b.v.x * g.x) + (b.v.y * g.y)) / minDistance;

            float nx = g.x / minDistance;
            float ny = g.y / minDistance;

            // Compute new velocities by exchanging normal components
            float aVx = (f * nx) - (e * ny);
            float aVy = (f * ny) + (e * nx);
            float bVx = (m * nx) - (h * ny);
            float bVy = (m * ny) + (h * nx);

            a.v.x = aVx;
            a.v.y = aVy;
            b.v.x = bVx;
            b.v.y = bVy;

            // Separate the points to eliminate overlap
            float sepX = overlap / 2f * (delta.x / dist);
            float sepY = overlap / 2f * (delta.y / dist);
            a.pos.x += sepX;
            a.pos.y += sepY;
            b.pos.x -= sepX;
            b.pos.y -= sepY;

            // Update previous positions to maintain velocity in Verlet integration
            a.prevPos.x = a.pos.x - (a.v.x / 60f);
            a.prevPos.y = a.pos.y - (a.v.y / 60f);
            b.prevPos.x = b.pos.x - (b.v.x / 60f);
            b.prevPos.y = b.pos.y - (b.v.y / 60f);
        }
    }
}
