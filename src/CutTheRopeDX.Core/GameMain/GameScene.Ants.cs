using CutTheRopeDX.Framework.Core;
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

            // Ants only ever carry a whole candy, which the body-role table already enforces.
            foreach (CandyBody body in ActiveCandyBodies(CandyInteraction.Ants))
            {
                if (body.Point == null || !body.Owner.Capabilities.CanAttachAnts)
                {
                    continue;
                }

                UpdateAntConveyorForCandy(body.Owner, delta);
            }
        }

        private void UpdateAntConveyorForCandy(CandyContext ctx, float delta)
        {
            // The caller only offers whole bodies, so this candy rides the conveyor on one point.
            ConstraintedPoint point = ctx.WholeBody.Point;
            CandyAttachments attachments = ctx.Lifecycle.Attachments;

            // Advance this candy's own carrier marker along its segment (replaces the segment-level
            // marker; lets several candies ride one lane, each keeping the spacing it entered with).
            attachments.AdvanceAntCarry(delta);

            if (attachments.AntWaitingForExit)
            {
                bool stillInside = false;
                foreach (AntsPathSegment segment in antsPathsSegments)
                {
                    if (segment.ContainsPoint(point.pos, external: true))
                    {
                        stillInside = true;
                        break;
                    }
                }

                attachments.SetAntWaitingForExit(stillInside);
            }

            _ = attachments.AdvanceAntCooldown(0.01f);

            AntsPathSegment carrier = attachments.AntSegment;
            if (AntCandyInteraction.ShouldDetach(
                candyCarriedBySegment: carrier != null,
                segmentInteracting: carrier != null,
                interactionTime: attachments.AntInteractionTime,
                candyInsideInternalBounds: carrier?.ContainsPoint(point.pos) == true))
            {
                bool otherSegmentContainsCandyExternally = false;
                foreach (AntsPathSegment other in antsPathsSegments)
                {
                    if (other != carrier && other.ContainsPoint(point.pos, external: true))
                    {
                        otherSegmentContainsCandyExternally = true;
                        break;
                    }
                }

                bool shouldSlowStop = AntCandyInteraction.ShouldSlowStopAfterDetach(otherSegmentContainsCandyExternally);
                attachments.EndAntCarry(waitForExit: false);
                point.disableGravity = IsCandyGravitySuppressed(ctx);

                if (shouldSlowStop)
                {
                    ApplyConveyorBrake(ctx);
                    PlayAntConveyorDetachSound();
                }
            }

            if (attachments.AntSegment == null)
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
                ConstraintedPoint point = ctx.WholeBody.Point;
                if (ctx.Lifecycle.Attachments.AntSegment == null || point == null)
                {
                    continue;
                }

                Vector nextPos = AntConveyorLogic.ComputeCarrierFollowPosition(
                    point.pos,
                    ctx.Lifecycle.Attachments.AntInteractionPoint,
                    ctx.Lifecycle.Attachments.AntInteractionTime,
                    snapDistance);

                point.pos = nextPos;

                if (ctx.Lifecycle.Attachments.Rocket?.point != null)
                {
                    ctx.Lifecycle.Attachments.Rocket.point.pos = nextPos;
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

            CandyContext ctx = CandyForPointOrNull(point);
            if (ctx == null || ctx.WholeBody.Point != point || ctx.Lifecycle.Attachments.AntSegment == null)
            {
                return false;
            }

            Vector touchWorld = camera.ScreenToWorld(tx, ty);
            float halfSize = AntConveyorLogic.GetCarrierTouchHalfSize(GetAntConveyorScale());

            if (!AntConveyorLogic.IsPointInCarrierTouchZone(touchWorld, point.pos, halfSize))
            {
                return false;
            }

            ctx.Lifecycle.Attachments.SetAntWaitingForExit(true);
            ApplyConveyorBrake(ctx);
            ctx.Lifecycle.Attachments.EndAntCarry(waitForExit: true);
            PlayAntConveyorDetachSound();
            point.disableGravity = IsCandyGravitySuppressed(ctx);
            return true;
        }

        /// <summary>
        /// Applies the iOS deceleration brake to a candy as it leaves the conveyor
        /// (horizontal velocity × −0.7 over 0.01 s).
        /// </summary>
        /// <param name="ctx">The candy to brake.</param>
        private static void ApplyConveyorBrake(CandyContext ctx)
        {
            ConstraintedPoint point = ctx?.WholeBody.Point;
            point?.ApplyImpulseDelta(new Vector(point.v.X * -0.7f, 0f), 0.01f);
        }

        /// <summary>
        /// Detaches a single candy from the segment currently carrying it (no-op if it is not being
        /// carried) and restores it to physics. Used when another mechanic (e.g. a mechanical hand)
        /// takes ownership of that candy. Other candies on the conveyor are unaffected; ants are kept
        /// off this candy while a hand holds it via the <c>candyHeldByHand</c> guard in
        /// <see cref="AntCandyInteraction.CanAttach"/>.
        /// </summary>
        /// <param name="ctx">The candy to detach from the conveyor.</param>
        private void DetachCandyFromConveyor(CandyContext ctx)
        {
            if (ctx == null)
            {
                return;
            }

            bool wasCarried = ctx.Lifecycle.Attachments.AntSegment != null;
            ctx.Lifecycle.Attachments.ResetAnts();
            if (wasCarried)
            {
                PlayAntConveyorDetachSound();

                // A candy can only hold a segment if it had a point when it attached.
                ctx.WholeBody.Point.disableGravity = IsCandyGravitySuppressed(ctx);
            }
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
            if (segment == null || !ctx.IsAntAttachable)
            {
                return false;
            }

            ConstraintedPoint point = ctx.WholeBody.Point;
            bool contains = point != null && segment.ContainsPoint(point.pos, useExternalBounds);
            if (!AntCandyInteraction.CanAttach(
                candyPresent: point != null,
                segmentCanInteract: segment.canInteract,
                candyWaitingForFly: ctx.Lifecycle.Attachments.AntWaitingForExit,
                isLastSegment: segment == ctx.Lifecycle.Attachments.LastAntSegment,
                candyInsideBounds: contains,
                candyHeldByHand: ctx.Lifecycle.Attachments.Hand != null,
                candyInLantern: ctx.Lifecycle.Attachments.InLantern,
                candyInTransport: ctx.Lifecycle.Presence == CandyPresence.Hidden,
                candyCarriedByMouse: MouseCarries(ctx)))
            {
                return false;
            }

            // Sound the pickup only when this candy first boards the conveyor, not on the internal
            // segment-to-segment hops (lastAntSegment is still set while it hops within a path).
            bool freshPickup = ctx.Lifecycle.Attachments.LastAntSegment == null;

            // Seed this candy's marker at its projection onto the segment, then nudge it one tick
            // (mirrors the segment's old StartInteraction + Update(0.01) on attach).
            Vector interactionPoint = AntsPathSegment.GetPointOnSegmentFromPointtoPointnearestToPoint(
                segment.startPoint, segment.endPoint, ctx.WholeBody.Point.pos);
            interactionPoint = new Vector(
                interactionPoint.X + (segment.speed.X * 0.01f),
                interactionPoint.Y + (segment.speed.Y * 0.01f));
            _ = ctx.Lifecycle.Attachments.BeginAntCarry(segment, interactionPoint, 0.3f, 0.01f);
            ctx.WholeBody.Point.disableGravity = IsCandyGravitySuppressed(ctx);

            if (freshPickup)
            {
                PlayAntConveyorAttachSound();
            }

            PopCandyBubble(ctx.WholeBody);

            if (point.weight > 1f)
            {
                point.SetWeight(1f);
                DetachSnailsForPoint(point);
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
            CandyContext ctx = CandyForPointOrNull(tail);
            bool carried = ctx != null && ctx.WholeBody.Point == tail && ctx.Lifecycle.Attachments.AntSegment != null;
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
