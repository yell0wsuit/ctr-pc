using System;

using CutTheRopeDX.Framework;
using CutTheRopeDX.Framework.Helpers;
using CutTheRopeDX.Framework.Physics;

namespace CutTheRopeDX.GameMain
{
    /// <summary>
    /// Immutable payload for a lantern capture that is waiting for its delayed completion.
    /// Passing the ticket itself through the dispatcher prevents an obsolete callback from
    /// completing a newer capture for the same point.
    /// </summary>
    internal sealed class PendingLanternCapture(ConstraintedPoint point, Lantern lantern) : FrameworkTypes
    {
        /// <summary>Gets the candy point being captured.</summary>
        public ConstraintedPoint Point { get; } = point ?? throw new ArgumentNullException(nameof(point));

        /// <summary>Gets the lantern that will complete the capture.</summary>
        public Lantern Lantern { get; } = lantern ?? throw new ArgumentNullException(nameof(lantern));

        /// <summary>Completes the capture only while the candy still belongs to a lantern.</summary>
        public void Complete(CandyContext candy)
        {
            if (candy?.Lifecycle.Attachments.InLantern == true)
            {
                Lantern.CaptureCandy(Point);
            }
        }
    }

    /// <summary>
    /// Immutable ownership ticket for the second ghost bubble retained across a candy merge.
    /// </summary>
    internal sealed class ParkedGhostBubble(CandyBody owner, GameObject bubble)
    {
        /// <summary>Gets the merged body whose active bubble hides this one.</summary>
        public CandyBody Owner { get; } = owner ?? throw new ArgumentNullException(nameof(owner));

        /// <summary>Gets the ghost bubble waiting to be released.</summary>
        public GameObject Bubble { get; } = bubble ?? throw new ArgumentNullException(nameof(bubble));

        /// <summary>Releases the parked bubble through the scene's ghost-cycle owner.</summary>
        public void Cancel(Action<GameObject> releaseBubble)
        {
            ArgumentNullException.ThrowIfNull(releaseBubble);
            releaseBubble(Bubble);
        }

        /// <summary>Cancels this ticket and returns the replacement to store atomically.</summary>
        public ParkedGhostBubble ReplaceWith(
            ParkedGhostBubble replacement,
            Action<GameObject> releaseBubble)
        {
            Cancel(releaseBubble);
            return replacement;
        }
    }
}
