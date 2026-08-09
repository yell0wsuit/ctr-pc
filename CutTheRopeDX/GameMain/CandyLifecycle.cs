using System.Collections.Generic;

namespace CutTheRopeDX.GameMain
{
    /// <summary>Owns the lifecycle state of one logical candy.</summary>
    internal sealed class CandyLifecycle
    {
        private CandyLifecycle(CandyPresence presence, CandyBody wholeBody, SplitCandyState split)
        {
            Presence = presence;
            WholeBody = wholeBody;
            Split = split;
            Attachments = new CandyAttachments();
        }

        /// <summary>Gets the candy's current lifecycle topology.</summary>
        public CandyPresence Presence { get; private set; }

        /// <summary>Gets the permanent removal reason, or <see langword="null"/> while the candy is not removed.</summary>
        public CandyRemovalReason? RemovalReason { get; private set; }

        /// <summary>Gets the dormant or active whole body owned by this lifecycle.</summary>
        public CandyBody WholeBody { get; }

        /// <summary>Gets the owned split payload, or <see langword="null"/> outside <see cref="CandyPresence.Split"/>.</summary>
        public SplitCandyState Split { get; private set; }

        /// <summary>Gets the current hidden transport session, or <see langword="null"/> outside transport.</summary>
        public CandyTransportSession Transport { get; private set; }

        /// <summary>Gets the authoritative whole-candy attachment state.</summary>
        public CandyAttachments Attachments { get; }

        /// <summary>Gets whether the present candy may start a transport operation.</summary>
        public bool CanEnterTransport => Presence == CandyPresence.Present && !Attachments.InLantern;

        /// <summary>Gets whether lifecycle or attachment ownership currently suppresses gravity.</summary>
        public bool IsGravitySuppressed => Presence == CandyPresence.Hidden || Attachments.SuppressGravity;

        /// <summary>Gets the physical bodies currently available to scene systems.</summary>
        public IReadOnlyList<CandyBody> ActiveBodies =>
            Presence == CandyPresence.Present ? [WholeBody] :
            Presence == CandyPresence.Split ? Split.SurvivingBodies : [];

        /// <summary>Gets whether the candy reached terminal removal by being eaten.</summary>
        public bool WasEaten => Presence == CandyPresence.Removed
            && RemovalReason == CandyRemovalReason.Eaten;

        /// <summary>
        /// Gets whether an owned split half was permanently removed. Split halves cannot be eaten, so
        /// any removed half is a loss even while the logical candy is still <see cref="CandyPresence.Split"/>.
        /// </summary>
        public bool HasFailedSplitHalf => Presence == CandyPresence.Split
            && (Split.Left.RemovalReason != null || Split.Right.RemovalReason != null);

        /// <summary>Gets whether the candy reached terminal removal for a loss reason.</summary>
        public bool HasFailedRemoval => (Presence == CandyPresence.Removed
                && RemovalReason is not CandyRemovalReason.Eaten)
            || HasFailedSplitHalf;

        /// <summary>Creates a lifecycle whose whole <paramref name="wholeBody"/> is present.</summary>
        /// <param name="wholeBody">The whole physical body owned by the lifecycle.</param>
        /// <returns>A new present lifecycle.</returns>
        public static CandyLifecycle CreatePresent(CandyBody wholeBody)
        {
            return new(CandyPresence.Present, wholeBody, null);
        }

        /// <summary>Creates a lifecycle whose whole body is dormant behind two owned split halves.</summary>
        /// <param name="wholeBody">The dormant whole body restored after a successful merge.</param>
        /// <param name="split">The owned split-half state.</param>
        /// <returns>A new split lifecycle.</returns>
        public static CandyLifecycle CreateSplit(CandyBody wholeBody, SplitCandyState split)
        {
            return new(CandyPresence.Split, wholeBody, split);
        }

        /// <summary>
        /// Replaces the present whole body with two owned halves. A level authors its candy as split,
        /// so this runs once while the map loads rather than in response to gameplay; the whole body
        /// stays dormant until <see cref="TryCompleteMerge"/> brings it back.
        /// </summary>
        /// <param name="split">The owned split-half state built from the map's halves.</param>
        /// <returns>
        /// <see langword="true"/> when a present candy becomes split; otherwise, <see langword="false"/>
        /// when <paramref name="split"/> is <see langword="null"/> or the candy is already removed,
        /// hidden, or split.
        /// </returns>
        public bool TryBeginSplit(SplitCandyState split)
        {
            if (split == null || Presence != CandyPresence.Present)
            {
                return false;
            }

            Split = split;
            Presence = CandyPresence.Split;
            return true;
        }

        /// <summary>
        /// Permanently removes a present candy and atomically detaches its whole-candy owners.
        /// </summary>
        /// <param name="reason">The reason the candy is removed.</param>
        /// <param name="detachedAttachments">Former attachment owners for scene-level cleanup.</param>
        /// <returns>
        /// <see langword="true"/> when the candy transitioned to removed; otherwise,
        /// <see langword="false"/> without changing lifecycle or attachment state.
        /// </returns>
        public bool TryRemove(CandyRemovalReason reason, out CandyAttachmentSnapshot detachedAttachments)
        {
            if (Presence != CandyPresence.Present)
            {
                detachedAttachments = null;
                return false;
            }

            Presence = CandyPresence.Removed;
            RemovalReason = reason;
            detachedAttachments = Attachments.DetachAll();
            return true;
        }

        /// <summary>Atomically replaces two intact merging halves with the owned whole body.</summary>
        /// <returns>
        /// <see langword="true"/> when a split lifecycle has two intact merging halves and returns to
        /// <see cref="CandyPresence.Present"/>; otherwise, <see langword="false"/> without changing state.
        /// </returns>
        public bool TryCompleteMerge()
        {
            if (Presence != CandyPresence.Split || !Split.TryCompleteMerge())
            {
                return false;
            }

            Split = null;
            Presence = CandyPresence.Present;
            return true;
        }

        /// <summary>Temporarily hides a present whole body inside the specified transport session.</summary>
        /// <param name="session">The exact transport session that will later complete this transition.</param>
        /// <param name="detachedAttachments">Former incompatible carriers for scene-level cleanup.</param>
        /// <returns>
        /// <see langword="true"/> when a present whole candy becomes hidden; otherwise,
        /// <see langword="false"/> when the lifecycle is removed, split, or already hidden.
        /// </returns>
        public bool TryHide(
            CandyTransportSession session,
            out CandyAttachmentSnapshot detachedAttachments)
        {
            if (session == null || !CanEnterTransport)
            {
                detachedAttachments = null;
                return false;
            }

            detachedAttachments = Attachments.DetachForTransport();
            Transport = session;
            Presence = CandyPresence.Hidden;
            return true;
        }

        /// <summary>Completes the current hidden transport session and restores the whole body.</summary>
        /// <param name="expectedSession">The exact session whose delayed completion is executing.</param>
        /// <returns>
        /// <see langword="true"/> when <paramref name="expectedSession"/> is the current hidden session and
        /// the whole body returns to <see cref="CandyPresence.Present"/>; otherwise, <see langword="false"/>.
        /// A stale or non-current session cannot mutate lifecycle state.
        /// </returns>
        public bool TryCompleteTransport(CandyTransportSession expectedSession)
        {
            if (Presence != CandyPresence.Hidden || !ReferenceEquals(Transport, expectedSession))
            {
                return false;
            }

            Transport = null;
            Presence = CandyPresence.Present;
            return true;
        }
    }
}
