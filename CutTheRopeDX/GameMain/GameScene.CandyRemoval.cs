using CutTheRopeDX.Framework.Helpers;

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
            if (body?.Owner == null || !TryCommitCandyRemoval(body, reason))
            {
                return false;
            }

            ReleaseRemovalOwnership(body, reason);
            return true;
        }

        private static bool TryCommitCandyRemoval(CandyBody body, CandyRemovalReason reason)
        {
            SplitCandyState split = body.Owner.Lifecycle.Split;
            return body.Role switch
            {
                CandyBodyRole.LeftHalf => split?.Left.TryRemove(reason) == true,
                CandyBodyRole.RightHalf => split?.Right.TryRemove(reason) == true,
                CandyBodyRole.Whole => body.Owner.Lifecycle.TryRemove(reason),
                _ => false,
            };
        }

        // Temporary low-level compatibility for removal callers migrated by the later plan tasks.
        private static bool TryRemoveBody(CandyBody body, CandyRemovalReason reason)
        {
            return body?.Owner != null && TryCommitCandyRemoval(body, reason);
        }

        private void ReleaseRemovalOwnership(CandyBody body, CandyRemovalReason reason)
        {
            ReleaseRopesForBody(body);
            ReleaseBubbleForRemoval(body, reason);

            if (body.Role == CandyBodyRole.Whole)
            {
                CandyContext ctx = body.Owner;
                DetachHandsForPoint(body.Point);
                DetachSnailsForPoint(body.Point);
                DropMouseCandyForPoint(body.Point);
                DetachCandyFromConveyor(ctx);
                ExhaustRocketForCandy(ctx);
                if (Lantern.CancelCandyCaptureForRemoval(body.Point))
                {
                    ctx.inLantern = false;
                }
            }

            // Carrier release ordering is deliberately closed here: no owner may leave a retired
            // point gravity-suppressed. Authored/device-specific weight is preserved.
            body.Point.disableGravity = false;
        }

        private void ReleaseBubbleForRemoval(CandyBody body, CandyRemovalReason reason)
        {
            if (body.Bubble == null)
            {
                return;
            }

            if (reason is CandyRemovalReason.Eaten or CandyRemovalReason.Hazard)
            {
                PopCandyBubble(body);
                return;
            }

            GameObject released = body.Bubble;
            EnableGhostCycleForBubble(released);
            if (pendingSecondGhostBubble != null && body.Role == CandyBodyRole.Whole)
            {
                EnableGhostCycleForBubble(pendingSecondGhostBubble);
                pendingSecondGhostBubble = null;
            }

            if (released is Bubble bubble)
            {
                bubble.capturedByBulb = false;
            }

            body.Bubble = null;
            body.BubbleHasGhost = false;
            body.Owner.lightBulb?.SyncFromContext(body.Owner);
            _ = (body.BubbleAnimation?.visible = false);
            _ = (body.GhostBubbleAnimation?.visible = false);
        }
    }
}
