using System.Collections.Generic;

using CutTheRopeDX.Framework.Helpers;

namespace CutTheRopeDX.GameMain
{
    /// <summary>Merge phase of the two bodies owned by a split logical candy.</summary>
    internal enum SplitPhase
    {
        /// <summary>The two surviving halves move independently.</summary>
        Separate,

        /// <summary>The two intact halves are moving toward an atomic merge.</summary>
        Merging,
    }

    /// <summary>Owns one physical half and its permanent removal state.</summary>
    internal sealed class CandyHalf
    {
        private SplitCandyState owner;

        /// <summary>Initializes an intact half around its physical body.</summary>
        /// <param name="body">The physical body represented by this half.</param>
        internal CandyHalf(CandyBody body)
        {
            Body = body;
        }

        /// <summary>Gets the physical body represented by this half.</summary>
        public CandyBody Body { get; }

        /// <summary>Gets the permanent removal reason, or <see langword="null"/> while the half survives.</summary>
        public CandyRemovalReason? RemovalReason { get; private set; }

        /// <summary>Gets whether this half still has an active physical body.</summary>
        public bool IsPresent => RemovalReason == null;

        /// <summary>Permanently removes this half for a non-eating reason and cancels an active merge.</summary>
        /// <param name="reason">The non-eating reason the half is removed.</param>
        /// <returns>
        /// <see langword="true"/> when an intact half is removed; otherwise, <see langword="false"/>
        /// when <paramref name="reason"/> is <see cref="CandyRemovalReason.Eaten"/> or the half was already removed.
        /// </returns>
        public bool TryRemove(CandyRemovalReason reason)
        {
            if (reason == CandyRemovalReason.Eaten || RemovalReason != null)
            {
                return false;
            }

            RemovalReason = reason;
            owner?.CancelMerge();
            return true;
        }

        /// <summary>Assigns the split aggregate that owns this half.</summary>
        /// <param name="splitOwner">The owning split aggregate.</param>
        internal void AttachTo(SplitCandyState splitOwner)
        {
            owner = splitOwner;
        }
    }

    /// <summary>Owns the halves and merge state of one split <see cref="CandyLifecycle"/>.</summary>
    internal sealed class SplitCandyState
    {
        /// <summary>Initializes split state with its left and right halves.</summary>
        /// <param name="left">The owned left half.</param>
        /// <param name="right">The owned right half.</param>
        internal SplitCandyState(CandyHalf left, CandyHalf right)
        {
            Left = left;
            Right = right;
            Left.AttachTo(this);
            Right.AttachTo(this);
        }

        /// <summary>Gets the owned left half.</summary>
        public CandyHalf Left { get; }

        /// <summary>Gets the owned right half.</summary>
        public CandyHalf Right { get; }

        /// <summary>Gets the current merge phase.</summary>
        public SplitPhase Phase { get; private set; }

        /// <summary>Gets the distance recorded when the current merge began.</summary>
        public float MergeDistance { get; private set; }

        /// <summary>Gets the physical bodies of all halves that have not been removed.</summary>
        public IReadOnlyList<CandyBody> SurvivingBodies =>
            Left.IsPresent && Right.IsPresent ? [Left.Body, Right.Body] :
            Left.IsPresent ? [Left.Body] :
            Right.IsPresent ? [Right.Body] : [];

        /// <summary>Begins merging two intact, separate halves at the specified distance.</summary>
        /// <param name="mergeDistance">The distance between the halves when merging begins.</param>
        /// <returns>
        /// <see langword="true"/> when both halves are present and transition to merging;
        /// otherwise, <see langword="false"/> when a half is removed or a merge is already active.
        /// </returns>
        public bool TryBeginMerge(float mergeDistance)
        {
            if (!Left.IsPresent || !Right.IsPresent || Phase != SplitPhase.Separate)
            {
                return false;
            }

            MergeDistance = mergeDistance;
            Phase = SplitPhase.Merging;
            return true;
        }

        /// <summary>
        /// Closes an active merge by one frame, shrinking the distance the halves still have to cover.
        /// </summary>
        /// <param name="speed">Distance closed per second.</param>
        /// <param name="delta">Frame delta in seconds.</param>
        /// <returns>
        /// <see langword="true"/> when the halves have closed the whole distance and the merge is ready
        /// to complete; <see langword="false"/> while distance remains, or when no merge is active.
        /// </returns>
        public bool TryAdvanceMerge(float speed, float delta)
        {
            if (Phase != SplitPhase.Merging)
            {
                return false;
            }

            float remaining = MergeDistance;
            bool closed = Mover.MoveVariableToTarget(ref remaining, 0, speed, delta);
            MergeDistance = remaining;
            return closed;
        }

        /// <summary>Checks whether two intact merging halves can complete their lifecycle merge.</summary>
        /// <returns>
        /// <see langword="true"/> when both halves survive and are merging; otherwise,
        /// <see langword="false"/> when either half was removed or merging has not begun.
        /// </returns>
        public bool TryCompleteMerge()
        {
            return Left.IsPresent && Right.IsPresent && Phase == SplitPhase.Merging;
        }

        /// <summary>Cancels a merge because an owned half was permanently removed.</summary>
        internal void CancelMerge()
        {
            Phase = SplitPhase.Separate;
            MergeDistance = 0f;
        }
    }
}
