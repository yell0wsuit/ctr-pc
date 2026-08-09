using CutTheRopeDX.Framework.Core;
using CutTheRopeDX.Framework.Helpers;

namespace CutTheRopeDX.GameMain
{
    /// <summary>Immutable device owners released by an authoritative attachment transition.</summary>
    internal sealed class CandyAttachmentSnapshot
    {
        internal CandyAttachmentSnapshot(
            bool inLantern,
            Rocket rocket,
            AntsPathSegment antSegment,
            MechanicalHand hand)
        {
            InLantern = inLantern;
            Rocket = rocket;
            AntSegment = antSegment;
            Hand = hand;
        }

        /// <summary>Gets whether a lantern owned the candy.</summary>
        public bool InLantern { get; }

        /// <summary>Gets the rocket that owned the candy, if any.</summary>
        public Rocket Rocket { get; }

        /// <summary>Gets the ant segment that actively carried the candy, if any.</summary>
        public AntsPathSegment AntSegment { get; }

        /// <summary>Gets the mechanical hand that owned the candy, if any.</summary>
        public MechanicalHand Hand { get; }

    }

    /// <summary>
    /// Authoritative runtime attachment state for one logical candy. Mutations are expressed as
    /// complete transitions so device ownership and ant-conveyor bookkeeping cannot drift apart.
    /// </summary>
    internal sealed class CandyAttachments
    {
        private float antCooldown;

        /// <summary>Gets whether a lantern currently contains the candy.</summary>
        public bool InLantern { get; private set; }

        /// <summary>Gets the rocket currently bound to the candy, if any.</summary>
        public Rocket Rocket { get; private set; }

        /// <summary>Gets whether a rocket is currently bound to the candy.</summary>
        public bool HasActiveRocket => Rocket != null;

        /// <summary>Gets the ant segment currently carrying the candy, if any.</summary>
        public AntsPathSegment AntSegment { get; private set; }

        /// <summary>Gets the last ant segment, retained during the reattachment cooldown.</summary>
        public AntsPathSegment LastAntSegment { get; private set; }

        /// <summary>Gets the remaining ant reattachment cooldown.</summary>
        public float AntCooldown => antCooldown;

        /// <summary>Gets whether the candy must leave the ant lane before reattaching.</summary>
        public bool AntWaitingForExit { get; private set; }

        /// <summary>Gets the candy's independent marker on its ant segment.</summary>
        public Vector AntInteractionPoint { get; private set; }

        /// <summary>Gets the elapsed time of the current ant interaction.</summary>
        public float AntInteractionTime { get; private set; }

        /// <summary>Gets the mechanical hand currently holding the candy, if any.</summary>
        public MechanicalHand Hand { get; private set; }

        /// <summary>Gets whether the aggregate contains a live attachment or reattachment guard.</summary>
        public bool HasAny => InLantern
            || Rocket != null
            || AntSegment != null
            || LastAntSegment != null
            || AntWaitingForExit
            || Hand != null;

        /// <summary>Gets whether an attachment currently owns gravity for the candy point.</summary>
        public bool SuppressGravity => InLantern
            || Rocket != null
            || AntSegment != null;

        /// <summary>
        /// Captures the candy in a lantern and atomically releases every incompatible carrier.
        /// </summary>
        /// <returns>The former rocket, hand, and ant owners for scene-level cleanup.</returns>
        public CandyAttachmentSnapshot CaptureInLantern()
        {
            CandyAttachmentSnapshot snapshot = new(
                inLantern: false,
                rocket: Rocket,
                antSegment: AntSegment,
                hand: Hand);
            InLantern = true;
            Rocket = null;
            Hand = null;
            ResetAnts();
            return snapshot;
        }

        /// <summary>Records that the candy has left its lantern.</summary>
        public void ReleaseFromLantern()
        {
            InLantern = false;
        }

        /// <summary>Binds a non-null rocket to the candy.</summary>
        /// <returns><see langword="true"/> when the binding was recorded.</returns>
        public bool BindRocket(Rocket rocket)
        {
            if (rocket == null)
            {
                return false;
            }

            Rocket = rocket;
            return true;
        }

        /// <summary>Releases the rocket only when it is still the current owner.</summary>
        /// <returns><see langword="true"/> when the expected rocket was released.</returns>
        public bool TryReleaseRocket(Rocket expectedRocket)
        {
            if (expectedRocket == null || !ReferenceEquals(Rocket, expectedRocket))
            {
                return false;
            }

            Rocket = null;
            return true;
        }

        /// <summary>Records a non-null mechanical hand as the current holder.</summary>
        /// <returns><see langword="true"/> when the holder was recorded.</returns>
        public bool CaptureByHand(MechanicalHand hand)
        {
            if (hand == null)
            {
                return false;
            }

            Hand = hand;
            return true;
        }

        /// <summary>Releases the hand only when it is still the current owner.</summary>
        /// <returns><see langword="true"/> when the expected hand was released.</returns>
        public bool TryReleaseHand(MechanicalHand expectedHand)
        {
            if (expectedHand == null || !ReferenceEquals(Hand, expectedHand))
            {
                return false;
            }

            Hand = null;
            return true;
        }

        /// <summary>Starts a complete ant-conveyor carry interaction.</summary>
        /// <returns><see langword="true"/> when a non-null segment was attached.</returns>
        public bool BeginAntCarry(
            AntsPathSegment segment,
            Vector interactionPoint,
            float cooldown,
            float interactionTime)
        {
            if (segment == null)
            {
                return false;
            }

            AntSegment = segment;
            LastAntSegment = segment;
            antCooldown = cooldown;
            AntInteractionPoint = interactionPoint;
            AntInteractionTime = interactionTime;
            return true;
        }

        /// <summary>Advances the marker and elapsed time of the current ant carry.</summary>
        public void AdvanceAntCarry(float delta)
        {
            if (AntSegment == null)
            {
                return;
            }

            AntInteractionTime += delta;
            AntInteractionPoint = new Vector(
                AntInteractionPoint.X + (AntSegment.speed.X * delta),
                AntInteractionPoint.Y + (AntSegment.speed.Y * delta));
        }

        /// <summary>Records whether the candy must leave the ant lane before it can reattach.</summary>
        public void SetAntWaitingForExit(bool waiting)
        {
            AntWaitingForExit = waiting;
        }

        /// <summary>Ends the active ant carry while retaining cooldown state for reattachment rules.</summary>
        public void EndAntCarry(bool waitForExit)
        {
            AntSegment = null;
            AntWaitingForExit = waitForExit;
        }

        /// <summary>Advances the inactive ant cooldown and forgets the last segment at zero.</summary>
        /// <returns><see langword="true"/> when the cooldown reached zero during this call.</returns>
        public bool AdvanceAntCooldown(float delta)
        {
            if (LastAntSegment == null || AntSegment != null)
            {
                return false;
            }

            bool expired = Mover.MoveVariableToTarget(ref antCooldown, 0f, 1f, delta);
            if (expired)
            {
                LastAntSegment = null;
            }

            return expired;
        }

        /// <summary>Clears active and residual ant-conveyor state together.</summary>
        public void ResetAnts()
        {
            AntWaitingForExit = false;
            AntSegment = null;
            LastAntSegment = null;
            antCooldown = 0f;
            AntInteractionPoint = default;
            AntInteractionTime = 0f;
        }

        /// <summary>
        /// Clears carriers that cannot survive transport while preserving a bound rocket.
        /// </summary>
        /// <returns>The former hand and ant owners for scene-level cleanup.</returns>
        public CandyAttachmentSnapshot DetachForTransport()
        {
            CandyAttachmentSnapshot snapshot = new(
                inLantern: false,
                rocket: null,
                antSegment: AntSegment,
                hand: Hand);
            Hand = null;
            ResetAnts();
            return snapshot;
        }

        /// <summary>Atomically clears every attachment and returns the former owners.</summary>
        public CandyAttachmentSnapshot DetachAll()
        {
            CandyAttachmentSnapshot snapshot = new(InLantern, Rocket, AntSegment, Hand);
            InLantern = false;
            Rocket = null;
            Hand = null;
            ResetAnts();
            return snapshot;
        }
    }
}
