using CutTheRopeDX.Framework;
using CutTheRopeDX.Framework.Helpers;
using CutTheRopeDX.Framework.Physics;

namespace CutTheRopeDX.GameMain
{
    internal sealed partial class GameScene
    {
        /// <summary>
        /// Atomically records one body's permanent removal and releases every live owner attached
        /// to that body. A failed lifecycle transition has no cleanup or presentation side effects.
        /// </summary>
        /// <param name="body">Body being permanently retired.</param>
        /// <param name="reason">Permanent removal reason.</param>
        /// <returns><see langword="true"/> only when the body transitioned to removed.</returns>
        private bool TryRetireCandyBody(CandyBody body, CandyRemovalReason reason)
        {
            if (body?.Owner == null || !TryCommitCandyRemoval(body, reason, out CandyAttachmentSnapshot detached))
            {
                return false;
            }

            ReleaseRemovalOwnership(body, reason, detached);
            return true;
        }

        private static bool TryCommitCandyRemoval(
            CandyBody body,
            CandyRemovalReason reason,
            out CandyAttachmentSnapshot detached)
        {
            SplitCandyState split = body.Owner.Lifecycle.Split;
            detached = null;
            return body.Role switch
            {
                CandyBodyRole.LeftHalf => split?.Left.TryRemove(reason) == true,
                CandyBodyRole.RightHalf => split?.Right.TryRemove(reason) == true,
                CandyBodyRole.Whole => body.Owner.Lifecycle.TryRemove(reason, out detached),
                _ => false,
            };
        }

        private void ReleaseRemovalOwnership(
            CandyBody body,
            CandyRemovalReason reason,
            CandyAttachmentSnapshot detached)
        {
            ReleaseRopesForBody(body);
            DetachRopeConstraintsForPoint(body.Point);
            if (body.Role != CandyBodyRole.Whole)
            {
                DetachRopeConstraintsForPoint(body.Owner.WholeBody.Point);
            }
            ReleaseBubbleForRemoval(body, reason);

            if (body.Role == CandyBodyRole.Whole)
            {
                ReleaseDetachedHand(detached?.Hand, body.Point);
                DetachSnailsForPoint(body.Point);
                DropMouseCandyForPoint(body.Point);
                if (detached?.AntSegment != null)
                {
                    PlayAntConveyorDetachSound();
                }
                ExhaustDetachedRocket(detached?.Rocket);
                CancelPendingLanternCaptureForRemoval(body.Point);
                _ = Lantern.CancelCandyCaptureForRemoval(body.Point);
            }

            // Carrier release ordering is deliberately closed here: no owner may leave a retired
            // point gravity-suppressed. Authored/device-specific weight is preserved.
            body.Point.disableGravity = false;
        }

        private void ReleaseDetachedHand(MechanicalHand hand, ConstraintedPoint point)
        {
            if (hand == null)
            {
                return;
            }

            hand.cPoint.RemoveConstraint(point);
            hand.ReleaseCandyAfterDropSound();
            hand.AnimateReleaseWithAnimationsPool(aniPool);
            CTRSoundMgr.PlaySound(Resources.Snd.ExpHandDrop);
        }

        private void ReleaseTransportAttachments(
            CandyAttachmentSnapshot detached,
            ConstraintedPoint point)
        {
            ReleaseDetachedHand(detached?.Hand, point);
            DropMouseCandyForPoint(point);
            if (detached?.AntSegment != null)
            {
                PlayAntConveyorDetachSound();
            }

            CandyContext ctx = CandyForPointOrNull(point);
            if (ctx != null)
            {
                point.disableGravity = IsCandyGravitySuppressed(ctx);
            }
        }

        private void ReleaseLanternCaptureAttachments(
            CandyAttachmentSnapshot detached,
            ConstraintedPoint point)
        {
            ReleaseDetachedHand(detached?.Hand, point);
            DropMouseCandyForPoint(point);
            if (detached?.AntSegment != null)
            {
                PlayAntConveyorDetachSound();
            }
            ExhaustDetachedRocket(detached?.Rocket);

            CandyContext ctx = CandyForPointOrNull(point);
            if (ctx != null)
            {
                point.disableGravity = IsCandyGravitySuppressed(ctx);
            }
        }

        private static void ExhaustDetachedRocket(Rocket rocket)
        {
            if (rocket == null)
            {
                return;
            }

            rocket.state = Rocket.STATE_ROCKET_EXAUST;
            rocket.StopAnimation();
        }

        private void CancelPendingLanternCaptureForRemoval(ConstraintedPoint point)
        {
            if (pendingLanternCapture?.Point == point)
            {
                pendingLanternCapture = null;
            }
        }

        private void CompletePendingLanternCapture(FrameworkTypes value)
        {
            if (value is not PendingLanternCapture capture
                || !ReferenceEquals(pendingLanternCapture, capture))
            {
                return;
            }

            pendingLanternCapture = null;
            capture.Complete(CandyForPointOrNull(capture.Point));
        }

        private void DetachRopeConstraintsForPoint(ConstraintedPoint point)
        {
            foreach (RopeEntry entry in ropes.All)
            {
                int? cutPart = entry.CutPartForCandy(point);
                if (cutPart == null)
                {
                    continue;
                }

                ConstraintedPoint ropeEnd = entry.Rope.parts[cutPart.Value];
                ConstraintedPoint attachedPoint = entry.Rope.parts[cutPart.Value + 1];
                if (attachedPoint.HasConstraintTo(ropeEnd))
                {
                    entry.Rope.RemovePart(cutPart.Value);
                }
            }
        }

        private void ReleaseBubbleForRemoval(CandyBody body, CandyRemovalReason reason)
        {
            if (body.Bubble == null)
            {
                return;
            }

            if (reason == CandyRemovalReason.Hazard)
            {
                PopCandyBubbleAt(body, body.Point.pos);
                return;
            }

            if (reason == CandyRemovalReason.Eaten)
            {
                PopCandyBubble(body);
                return;
            }

            GameObject released = body.Bubble;
            ReleaseGhostForBubble(released);
            ReleasePendingSecondGhostBubbleForBody(body);

            if (released is Bubble bubble)
            {
                bubble.capturedByBulb = false;
            }

            body.Bubble = null;
            body.BubbleHasGhost = false;
            _ = (body.BubbleAnimation?.visible = false);
            _ = (body.GhostBubbleAnimation?.visible = false);
        }

        private void ReleasePendingSecondGhostBubbleForBody(CandyBody body)
        {
            if (parkedGhostBubble?.Owner != body)
            {
                return;
            }

            CancelParkedGhostBubble();
        }

        private void CancelParkedGhostBubble()
        {
            parkedGhostBubble = parkedGhostBubble?.ReplaceWith(null, ReleaseGhostForBubble);
        }
    }
}
