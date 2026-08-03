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

        /// <summary>Permanently removes a present candy for the specified reason.</summary>
        /// <param name="reason">The reason the candy is removed.</param>
        /// <returns>
        /// <see langword="true"/> when the candy transitions from present to removed;
        /// otherwise, <see langword="false"/> when the transition is illegal or already terminal.
        /// </returns>
        public bool TryRemove(CandyRemovalReason reason)
        {
            if (Presence != CandyPresence.Present)
            {
                return false;
            }

            Presence = CandyPresence.Removed;
            RemovalReason = reason;
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
        /// <returns>
        /// <see langword="true"/> when a present whole candy becomes hidden; otherwise,
        /// <see langword="false"/> when the lifecycle is removed, split, or already hidden.
        /// </returns>
        public bool TryHide(CandyTransportSession session)
        {
            if (Presence != CandyPresence.Present)
            {
                return false;
            }

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
