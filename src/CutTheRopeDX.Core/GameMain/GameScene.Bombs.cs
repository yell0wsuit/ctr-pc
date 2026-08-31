using System.Collections.Generic;
using System.Linq;

using CutTheRopeDX.Framework;
using CutTheRopeDX.Framework.Core;
using CutTheRopeDX.Framework.Helpers;
using CutTheRopeDX.Framework.Physics;
using CutTheRopeDX.Framework.Visual;

namespace CutTheRopeDX.GameMain
{
    /// <summary>
    /// Ticket handed to the delayed dispatcher so a bomb's debris burst can only ever finish the
    /// detonation it was scheduled for.
    /// </summary>
    internal sealed class PendingBombDebris(CandyContext bomb) : FrameworkTypes
    {
        /// <summary>Gets the bomb waiting for its debris burst.</summary>
        public CandyContext Bomb { get; } = bomb;
    }

    internal sealed partial class GameScene
    {
        /// <summary>Explosion animation, parsed once and replayed for every detonation.</summary>
        private static FlashXmlOneShotEffect s_explosionEffect;

        /// <summary>Every bomb that is still live: present, loaded, and not yet detonated.</summary>
        private IEnumerable<CandyContext> LiveBombs()
        {
            for (int i = 0; i < candies.Count; i++)
            {
                CandyContext ctx = candies[i];
                if (ctx.bomb != null && !ctx.bomb.Exploded && !ctx.HasNoWholeBodyInPlay)
                {
                    yield return ctx;
                }
            }
        }

        /// <summary>
        /// Detonates bombs that something has run into. Each bomb tests the other bodies first and
        /// only then the other bombs, so a bomb caught between a candy and a second bomb goes off
        /// on the candy, exactly as the original orders the two passes.
        /// </summary>
        /// <param name="delta">Elapsed time in seconds since the last update.</param>
        private void DetonateBombsOnContact(float delta)
        {
            foreach (CandyContext bombCtx in LiveBombs().ToList())
            {
                if (bombCtx.bomb.Exploded)
                {
                    continue;
                }

                _ = DetonateOnTouchingBody(bombCtx, delta) || DetonateOnTouchingBomb(bombCtx, delta);
            }
        }

        /// <summary>
        /// Detonates <paramref name="bombCtx"/> when a candy-like body other than a bomb touches it.
        /// The body is stopped dead first, so the blast pushes it from rest the way the original does.
        /// </summary>
        /// <returns><see langword="true"/> when the bomb went off.</returns>
        private bool DetonateOnTouchingBody(CandyContext bombCtx, float delta)
        {
            ConstraintedPoint bombPoint = bombCtx.WholeBody.Point;
            foreach (CandyBody body in ActiveCandyBodies(CandyInteraction.Hazard))
            {
                if (body.Owner.bomb != null)
                {
                    continue;
                }
                if (VectDistance(body.Point.pos, bombPoint.pos) > BombDefinition.ContactTriggerDistance)
                {
                    continue;
                }

                StopBodyAtImpact(body.Point);
                BoomBoomBomb(bombCtx, delta);
                return true;
            }

            return false;
        }

        /// <summary>
        /// Detonates <paramref name="bombCtx"/> and the first live bomb it is touching. Both go off:
        /// a pair of bombs that meet destroys each other.
        /// </summary>
        /// <returns><see langword="true"/> when the bombs went off.</returns>
        private bool DetonateOnTouchingBomb(CandyContext bombCtx, float delta)
        {
            ConstraintedPoint bombPoint = bombCtx.WholeBody.Point;
            foreach (CandyContext otherCtx in LiveBombs().ToList())
            {
                if (otherCtx == bombCtx)
                {
                    continue;
                }
                if (VectDistance(otherCtx.WholeBody.Point.pos, bombPoint.pos) > BombDefinition.BombPairTriggerDistance)
                {
                    continue;
                }

                BoomBoomBomb(bombCtx, delta);
                BoomBoomBomb(otherCtx, delta);
                return true;
            }

            return false;
        }

        /// <summary>
        /// Brings a body to a dead stop where it is, clearing the Verlet history so the blast that
        /// follows is the only thing moving it.
        /// </summary>
        private static void StopBodyAtImpact(ConstraintedPoint point)
        {
            point.v = vectZero;
            point.a = vectZero;
            point.posDelta = vectZero;
            point.prevPos = point.pos;
        }

        /// <summary>
        /// Detonates every live bomb a cut stroke passes over. The stroke has to cross the bomb's
        /// own small square, not merely come near it.
        /// </summary>
        /// <param name="v1">Stroke start.</param>
        /// <param name="v2">Stroke end.</param>
        /// <param name="delta">Elapsed time in seconds since the last update.</param>
        /// <returns>The number of bombs detonated.</returns>
        private int DetonateBombsCrossedByLine(Vector v1, Vector v2, float delta)
        {
            int detonated = 0;
            float extent = BombDefinition.SwipeHalfExtent;
            foreach (CandyContext bombCtx in LiveBombs().ToList())
            {
                Vector pos = bombCtx.WholeBody.Point.pos;
                if (!LineInRect(v1.X, v1.Y, v2.X, v2.Y, pos.X - extent, pos.Y - extent, extent * 2f, extent * 2f))
                {
                    continue;
                }

                BoomBoomBomb(bombCtx, delta);
                detonated++;
            }

            return detonated;
        }

        /// <summary>
        /// Sets a bomb off: plays the explosion, shoves every candy-like body inside the blast radius
        /// away from it, drops its ropes, and schedules the debris burst that removes it.
        /// </summary>
        /// <param name="bombCtx">The bomb going off.</param>
        /// <param name="delta">Elapsed time in seconds since the last update.</param>
        public void BoomBoomBomb(CandyContext bombCtx, float delta)
        {
            if (bombCtx?.bomb == null || bombCtx.bomb.Exploded)
            {
                return;
            }

            // Marked before the blast so a bomb caught in its own shockwave, or in a chain reaction
            // reaching back to it, cannot detonate twice.
            bombCtx.bomb.Exploded = true;

            Vector center = bombCtx.WholeBody.Point.pos;
            SpawnExplosionEffectAtXY(center.X, center.Y);
            CTRSoundMgr.PlaySound(Resources.Snd.Explosion);

            ApplyBlastAt(center, bombCtx, delta);
            ReleaseRopesForBody(bombCtx.WholeBody);
            // The bomb's own rocket cuts out at the blast, not when the wreck is cleared away, so it
            // cannot keep dragging the body through the delay before the debris burst.
            ExhaustBoundRocket(bombCtx);

            dd.CallObjectSelectorParamafterDelay(
                new DelayedDispatcher.DispatchFunc(Selector_bombDebris),
                new PendingBombDebris(bombCtx),
                BombDefinition.DebrisDelay);
        }

        /// <summary>Cuts out the rocket flying this bomb, if one is bound to it.</summary>
        private static void ExhaustBoundRocket(CandyContext bombCtx)
        {
            Rocket rocket = bombCtx.Lifecycle.Attachments.Rocket;
            if (rocket != null && bombCtx.Lifecycle.Attachments.TryReleaseRocket(rocket))
            {
                rocket.state = Rocket.STATE_ROCKET_EXAUST;
                rocket.StopAnimation();
            }
        }

        /// <summary>
        /// Pushes every candy-like body except the bomb itself away from the blast centre, with an
        /// impulse that falls off linearly to nothing at the blast radius.
        /// </summary>
        /// <param name="center">Blast centre.</param>
        /// <param name="source">The bomb that went off; it does not push itself.</param>
        /// <param name="delta">Elapsed time in seconds since the last update.</param>
        private void ApplyBlastAt(Vector center, CandyContext source, float delta)
        {
            foreach (CandyBody body in ActiveCandyBodies(CandyInteraction.Physics).ToList())
            {
                if (body.Owner == source || body.Owner.bomb?.Exploded is true)
                {
                    continue;
                }

                Vector offset = VectSub(body.Point.pos, center);
                float distance = VectLength(offset);
                if (distance is <= 0f or >= BombDefinition.BlastRadius)
                {
                    continue;
                }

                body.Point.ApplyImpulseDelta(BombDefinition.BlastImpulseFor(offset, distance), delta);
            }
        }

        /// <summary>Spawns the explosion animation at a world position.</summary>
        /// <param name="x">World-space X for the explosion.</param>
        /// <param name="y">World-space Y for the explosion.</param>
        private void SpawnExplosionEffectAtXY(float x, float y)
        {
            s_explosionEffect ??= new FlashXmlOneShotEffect("fx_explosion.xml", Resources.Img.FxExplosion, centerOnStage: true);
            s_explosionEffect.SpawnInto(aniPool, x, y, 0);
        }

        /// <summary>
        /// Finishes a detonation once the explosion has had time to cover the bomb: scatters the
        /// casing fragments and retires the body.
        /// </summary>
        private void Selector_bombDebris(FrameworkTypes param)
        {
            if (param is not PendingBombDebris pending || pending.Bomb.HasNoWholeBodyInPlay)
            {
                return;
            }

            Vector pos = pending.Bomb.WholeBody.Point.pos;
            SpawnBombDebrisAtXY(pos.X, pos.Y);
            _ = TryRetireCandyBody(pending.Bomb.WholeBody, CandyRemovalReason.Hazard);
        }

        /// <summary>Spawns the bomb's casing fragments at a world position.</summary>
        /// <param name="x">World-space X for the debris.</param>
        /// <param name="y">World-space Y for the debris.</param>
        private void SpawnBombDebrisAtXY(float x, float y)
        {
            Image grid = Image.Image_createWithResID(Resources.Img.ObjBomb);
            grid.DoRestoreCutTransparency();
            BombBreak debris = (BombBreak)new BombBreak().InitWithTotalParticlesandImageGrid(
                BombDefinition.DebrisParticleCount, grid);
            if (gravityState.IsInverted)
            {
                debris.gravity.Y = -debris.gravity.Y;
                debris.angle = -debris.angle;
            }
            debris.particlesDelegate = new Particles.ParticlesFinished(aniPool.ParticlesFinished);
            debris.x = x;
            debris.y = y;
            debris.StartSystem(BombDefinition.DebrisParticleCount);
            _ = aniPool.AddChild(debris);
        }
    }
}
