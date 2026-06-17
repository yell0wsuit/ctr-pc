using System;

using CutTheRopeDX.Framework.Physics;

namespace CutTheRopeDX.GameMain
{
    internal sealed partial class GameScene
    {
        /// <summary>
        /// Candy body collision radius in pixels (engine: 32 for a normal candy, 46.4 for disco).
        /// </summary>
        private const float CandyCollisionRadius = 32f;

        /// <summary>
        /// Resolves candy-to-candy collisions for all independent candies, matching the engine's
        /// pairwise <c>handleCandyIntersection</c> loop. No-ops for single-candy levels.
        /// Candies that are eaten, carried by a bubble, or captured in a lantern do not collide.
        /// </summary>
        private void ResolveCandyCollisions()
        {
            int count = candies.Count;
            for (int i = 0; i < count; i++)
            {
                CandyContext ca = candies[i];
                if (!CandyCollision.ShouldParticipate(ca.noCandy, ca.bubble != null, ca.inLantern))
                {
                    continue;
                }
                for (int j = i + 1; j < count; j++)
                {
                    CandyContext cb = candies[j];
                    if (!CandyCollision.ShouldParticipate(cb.noCandy, cb.bubble != null, cb.inLantern))
                    {
                        continue;
                    }
                    // Handclap exemption: two hand-held candies pass through each other (spec §4).
                    if (ca.capturingHand != null && cb.capturingHand != null)
                    {
                        continue;
                    }
                    float collisionDist = CandyCollisionRadius + CandyCollisionRadius;
                    float dx = ca.point.pos.X - cb.point.pos.X;
                    float dy = ca.point.pos.Y - cb.point.pos.Y;
                    if (((dx * dx) + (dy * dy)) < (collisionDist * collisionDist))
                    {
                        HandleCandyIntersection(ca.point, cb.point, collisionDist);
                    }
                }
            }
        }

        /// <summary>
        /// Resolves a single elastic body overlap. Ported from the engine's
        /// <c>GameScene::handleCandyIntersection</c>, which the engine uses for every elastic
        /// body collision (candy↔candy and light-bulb collisions both route through here).
        /// </summary>
        /// <param name="a">First body point.</param>
        /// <param name="b">Second body point.</param>
        /// <param name="collisionDist">
        /// Collision distance threshold (the engine's <c>a5</c>). For candy↔candy this is the sum
        /// of the two candy radii; for light bulbs it is the bulb collision distance.
        /// </param>
        private static void HandleCandyIntersection(ConstraintedPoint a, ConstraintedPoint b, float collisionDist)
        {
            float dx = a.pos.X - b.pos.X;
            float dy = a.pos.Y - b.pos.Y;
            float distSq = (dx * dx) + (dy * dy);
            if (distSq >= (collisionDist * collisionDist))
            {
                return;
            }

            float dist = MathF.Sqrt(distSq);
            float penetration = collisionDist - dist;
            float half = penetration * 0.5f;

            // Unit normal pointing from b toward a (algebraic equivalent of the engine's
            // acos/sincos separation: each candy moves half the penetration along the normal).
            float invDist = dist > 0f ? 1f / dist : 0f;
            float nx = dx * invDist;
            float ny = dy * invDist;

            float speedA = MathF.Sqrt((a.v.X * a.v.X) + (a.v.Y * a.v.Y));
            float speedB = MathF.Sqrt((b.v.X * b.v.X) + (b.v.Y * b.v.Y));
            float combinedSpeed = speedA + speedB;

            // High closing speed -> full elastic response; low speed -> gentle separation only.
            // (Engine threshold: penetration >= 1000/speedA-ish + 1000/speedB-ish == 2000/combined.)
            bool fullResolve = combinedSpeed > 0f && penetration >= (2000f / combinedSpeed);

            if (fullResolve)
            {
                // Equal-mass elastic velocity exchange along the contact axis. The engine
                // normalizes the solve by collisionDist (the radius sum), not the live distance.
                float avX = a.v.X;
                float avY = a.v.Y;
                float bvX = b.v.X;

                float dxBA = -dx; // b.x - a.x
                float dyBA = -dy; // b.y - a.y
                float c = dxBA / collisionDist;
                float s = dyBA / collisionDist;

                float t28 = dxBA * avX;
                float t29 = (t28 + (dyBA * avY)) / collisionDist;
                float t30 = dyBA * avX;
                float t31 = (t30 + (dxBA * bvX)) / collisionDist;
                float t32 = ((dxBA * avY) - t30) / collisionDist;
                float t35 = (t28 - (bvX * dyBA)) / collisionDist;

                a.v.X = (t31 * c) - (t32 * s);
                a.v.Y = (t32 * c) + (t31 * s);
                b.v.X = (t29 * c) - (t35 * s);
                b.v.Y = (t35 * c) + (t29 * s);

                a.pos.X += nx * half;
                a.pos.Y += ny * half;
                b.pos.X -= nx * half;
                b.pos.Y -= ny * half;

                // Rebuild the verlet history from the new velocity (dt = 1/60s).
                a.posDelta.X = a.v.X / 60f;
                a.posDelta.Y = a.v.Y / 60f;
                a.prevPos.X = a.pos.X - a.posDelta.X;
                a.prevPos.Y = a.pos.Y - a.posDelta.Y;
                b.posDelta.X = b.v.X / 60f;
                b.posDelta.Y = b.v.Y / 60f;
                b.prevPos.X = b.pos.X - b.posDelta.X;
                b.prevPos.Y = b.pos.Y - b.posDelta.Y;
            }
            else
            {
                // Low closing speed: separate positions only, leave velocities untouched.
                a.pos.X += nx * half;
                a.pos.Y += ny * half;
                b.pos.X -= nx * half;
                b.pos.Y -= ny * half;
            }
        }
    }
}
