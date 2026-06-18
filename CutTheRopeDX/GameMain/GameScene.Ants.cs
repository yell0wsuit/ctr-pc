using CutTheRopeDX.Framework.Core;
using CutTheRopeDX.Framework.Helpers;
using CutTheRopeDX.Framework.Physics;

namespace CutTheRopeDX.GameMain
{
    internal sealed partial class GameScene
    {
        /// <summary>
        /// Per-frame driver for the ant-conveyor system. Updates all paths, manages the
        /// wait-before-attach flag, drains segment cooldowns, handles detach when a candy
        /// leaves its segment's internal rectangle, and runs the priority search for new segments
        /// to carry each candy.
        /// </summary>
        /// <param name="delta">Elapsed time in seconds since the last frame.</param>
        private void UpdateAntConveyor(float delta)
        {
            if (antsPaths == null || antsPaths.Count == 0)
            {
                return;
            }

            foreach (AntsPath antsPath in antsPaths)
            {
                antsPath.Update(delta);
            }

            if (antsPathsSegments == null || antsPathsSegments.Count == 0)
            {
                return;
            }

            for (int ci = 0; ci < candies.Count; ci++)
            {
                CandyContext ctx = candies[ci];
                if ((ci != 0 && ctx.noCandy) || ctx.point == null || !ctx.Capabilities.CanAttachAnts)
                {
                    continue;
                }

                UpdateAntConveyorForCandy(ctx);
            }
        }

        private void UpdateAntConveyorForCandy(CandyContext ctx)
        {
            if (ctx.antWaitForFly)
            {
                ctx.antWaitForFly = false;
                foreach (AntsPathSegment segment in antsPathsSegments)
                {
                    if (segment.ContainsPoint(ctx.point.pos, external: true))
                    {
                        ctx.antWaitForFly = true;
                        break;
                    }
                }
            }

            if (ctx.lastAntSegment != null
                && ctx.antSegment == null
                && Mover.MoveVariableToTarget(ref ctx.antCooldown, 0f, 1f, 0.01f))
            {
                ctx.lastAntSegment = null;
            }

            AntsPathSegment carrier = ctx.antSegment;
            if (AntCandyInteraction.ShouldDetach(
                candyCarriedBySegment: carrier != null,
                segmentInteracting: carrier?.interacting == true,
                interactionTime: carrier?.interactionTime ?? 0f,
                candyInsideInternalBounds: carrier?.ContainsPoint(ctx.point.pos) == true))
            {
                bool otherSegmentContainsCandyExternally = false;
                foreach (AntsPathSegment other in antsPathsSegments)
                {
                    if (other != carrier && other.ContainsPoint(ctx.point.pos, external: true))
                    {
                        otherSegmentContainsCandyExternally = true;
                        break;
                    }
                }

                bool shouldSlowStop = AntCandyInteraction.ShouldSlowStopAfterDetach(otherSegmentContainsCandyExternally);
                ctx.point.disableGravity = ctx.activeRocket != null;
                carrier.StopInteractionWithCandySlow(shouldSlowStop);
                ctx.antSegment = null;

                if (shouldSlowStop)
                {
                    PlayAntConveyorDetachSound();
                }
            }

            if (ctx.antSegment == null)
            {
                bool attached = false;
                foreach (AntsPathSegment segment in antsPathsSegments)
                {
                    if (TryStartAntInteraction(segment, ctx, useExternalBounds: false))
                    {
                        attached = true;
                        break;
                    }
                }

                if (!attached)
                {
                    foreach (AntsPathSegment segment in antsPathsSegments)
                    {
                        if (TryStartAntInteraction(segment, ctx, useExternalBounds: true))
                        {
                            break;
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Overwrites the candy's world position with the carrier follow position computed by
        /// <see cref="AntConveyorLogic.ComputeCarrierFollowPosition"/>. No-op if no segment is
        /// currently carrying the candy.
        /// </summary>
        private void ApplyAntCarryToCandyPosition()
        {
            float scale = GetAntConveyorScale();
            float snapDistance = AntConveyorLogic.GetCarrierSnapDistance(scale);

            for (int ci = 0; ci < candies.Count; ci++)
            {
                CandyContext ctx = candies[ci];
                if (ctx.antSegment == null || ctx.point == null)
                {
                    continue;
                }

                Vector nextPos = AntConveyorLogic.ComputeCarrierFollowPosition(
                    ctx.point.pos,
                    ctx.antSegment.interactionPoint,
                    ctx.antSegment.container?.InteractionTime ?? 0f,
                    snapDistance);

                ctx.point.pos = nextPos;

                if (ctx.activeRocket?.point != null)
                {
                    ctx.activeRocket.point.pos = nextPos;
                }
            }
        }

        /// <summary>
        /// Handles a touch event on the candy while it is being carried by the conveyor.
        /// If the touch lands inside the carrier touch zone the candy is detached and released to physics.
        /// </summary>
        /// <param name="point">The candy's constraint point.</param>
        /// <param name="tx">Touch X coordinate in screen space.</param>
        /// <param name="ty">Touch Y coordinate in screen space.</param>
        /// <returns><see langword="true"/> if the touch was consumed; otherwise, <see langword="false"/>.</returns>
        private bool HandleConveyorTouchConstraintedPointXY(ConstraintedPoint point, float tx, float ty)
        {
            if (point == null)
            {
                return false;
            }

            CandyContext ctx = CandyForPoint(point);
            if (ctx.point != point || ctx.antSegment == null)
            {
                return false;
            }

            Vector touchWorld = Vect(tx + camera.pos.X, ty + camera.pos.Y);
            float halfSize = AntConveyorLogic.GetCarrierTouchHalfSize(GetAntConveyorScale());

            if (!AntConveyorLogic.IsPointInCarrierTouchZone(touchWorld, point.pos, halfSize))
            {
                return false;
            }

            ctx.antWaitForFly = true;
            ctx.antSegment.StopInteractionWithCandySlow(true);
            ctx.antSegment = null;
            PlayAntConveyorDetachSound();
            point.disableGravity = ctx.activeRocket != null;
            return true;
        }

        /// <summary>
        /// Detaches a single candy from the segment currently carrying it (no-op if it is not being
        /// carried) and restores it to physics. Used when another mechanic (e.g. a mechanical hand)
        /// takes ownership of that candy. Other candies on the conveyor are unaffected; ants are kept
        /// off this candy while a hand holds it via the <c>candyHeldByHand</c> guard in
        /// <see cref="AntCandyInteraction.CanAttach"/>.
        /// </summary>
        /// <param name="ctx">The candy to detach from the conveyor.</param>
        private static void DetachCandyFromConveyor(CandyContext ctx)
        {
            AntsPathSegment segment = ctx?.antSegment;
            if (segment == null)
            {
                return;
            }

            if (segment.interacting)
            {
                segment.StopInteractionWithCandySlow(false);
                PlayAntConveyorDetachSound();
            }

            segment.canInteract = true;

            // A candy can only hold a segment if it had a point when it attached.
            ctx.point.disableGravity = ctx.activeRocket != null;

            ctx.antWaitForFly = false;
            ctx.antSegment = null;
            ctx.lastAntSegment = null;
            ctx.antCooldown = 0f;
        }

        /// <summary>
        /// Attempts to attach the candy to <paramref name="segment"/>. Returns <see langword="true"/> and starts the
        /// interaction if all preconditions pass: the segment is idle and interactable, the candy is
        /// not in the wait-before-attach state, and the candy lies inside the segment's bounding rectangle.
        /// </summary>
        /// <param name="segment">The segment to test.</param>
        /// <param name="ctx">The candy to attach.</param>
        /// <param name="useExternalBounds">Whether to use the wider external bounding rectangle.</param>
        /// <returns><see langword="true"/> if the candy was attached to the segment; otherwise, <see langword="false"/>.</returns>
        private bool TryStartAntInteraction(AntsPathSegment segment, CandyContext ctx, bool useExternalBounds)
        {
            if (segment == null || !CandyInteraction.CanAttachAnts(ctx))
            {
                return false;
            }

            bool contains = ctx.point != null && segment.ContainsPoint(ctx.point.pos, useExternalBounds);
            if (!AntCandyInteraction.CanAttach(
                candyPresent: ctx.point != null,
                segmentInteracting: segment.interacting,
                segmentCanInteract: segment.canInteract,
                candyWaitingForFly: ctx.antWaitForFly,
                isLastSegment: segment == ctx.lastAntSegment,
                candyInsideBounds: contains,
                candyHeldByHand: ctx.capturingHand != null))
            {
                return false;
            }

            ctx.point.disableGravity = true;
            ctx.antSegment = segment;
            ctx.lastAntSegment = segment;
            ctx.antCooldown = 0.3f;

            if ((segment.container?.InteractionTime ?? 0f) == 0f)
            {
                PlayAntConveyorAttachSound();
            }

            segment.StartInteractionWithConstraitedPoint(ctx.point);

            if (ctx == candies[0] && candyBubble != null)
            {
                PopCandyBubble(false);
            }
            else if (ctx.bubble != null)
            {
                PopCandyBubble(ctx);
            }

            if (ctx.point.weight > 1f)
            {
                ctx.point.SetWeight(1f);
                DetachSnailsForPoint(ctx.point);
            }

            return true;
        }

        /// <summary>Returns the scale factor for ant-conveyor sizing. Always 1 on PC.</summary>
        /// <returns>The device scale multiplier.</returns>
        private static float GetAntConveyorScale()
        {
            return 1f;
        }

        /// <summary>
        /// Calls <see cref="Bungee.Update(float)"/> for <paramref name="rope"/> while preventing rope tension
        /// from displacing the candy when it is being carried by the ant conveyor.
        /// The candy position is locked before the rope physics step and restored afterward.
        /// </summary>
        /// <param name="rope">The bungee rope to update.</param>
        /// <param name="delta">Elapsed time in seconds since the last frame.</param>
        private void UpdateRopeWithAntCarryOverride(Bungee rope, float delta)
        {
            if (rope == null)
            {
                return;
            }

            ConstraintedPoint tail = rope.tail;
            CandyContext ctx = tail != null ? CandyForPoint(tail) : null;
            bool carried = ctx != null && ctx.point == tail && ctx.antSegment != null;
            if (!carried)
            {
                rope.Update(delta * ropePhysicsSpeed);
                return;
            }

            // Keep rope simulation running, but don't let it displace candy while ants carry it.
            Vector lockedCandyPos = tail.pos;
            rope.Update(delta * ropePhysicsSpeed);
            tail.pos = lockedCandyPos;
        }

        /// <summary>Plays the sound effect for the candy attaching to the ant conveyor.</summary>
        private static void PlayAntConveyorAttachSound()
        {
            CTRSoundMgr.PlaySound(Resources.Snd.ExpAntsTakeCandy);
        }

        /// <summary>Plays the sound effect for the candy detaching from the ant conveyor.</summary>
        private static void PlayAntConveyorDetachSound()
        {
            CTRSoundMgr.PlaySound(Resources.Snd.ExpAntsDropCandy);
        }
    }
}
