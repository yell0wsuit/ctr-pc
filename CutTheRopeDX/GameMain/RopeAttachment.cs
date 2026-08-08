namespace CutTheRopeDX.GameMain
{
    /// <summary>Condition of the rope a hook holds, derived from the rope itself.</summary>
    internal enum RopeAttachmentState
    {
        /// <summary>No rope is held.</summary>
        Idle,

        /// <summary>A rope is held and has not been cut.</summary>
        Intact,

        /// <summary>The rope was cut and is still fading; it is still simulated and drawn.</summary>
        Severing,

        /// <summary>The rope was cut and has finished fading; it is no longer simulated.</summary>
        Inert,
    }

    /// <summary>
    /// The single authority for whether a hook holds a rope and what condition that rope is in.
    /// Only the rope reference is stored: the three non-idle states are computed from
    /// <see cref="Bungee.cut"/> and <see cref="Bungee.cutTime"/>, so this type cannot disagree with
    /// the rope it describes.
    /// </summary>
    internal sealed class RopeAttachment
    {
        /// <summary>Gets the held rope, or <see langword="null"/> while idle.</summary>
        public Bungee Rope { get; private set; }

        /// <summary>
        /// Gets whether the hook's rope source has been used up. Set by <see cref="TryAttach"/> and
        /// deliberately preserved across <see cref="Release"/>: a hook that already spent its one
        /// attach must not get another when its rope is destroyed.
        /// </summary>
        public bool SourceExhausted { get; private set; }

        /// <summary>Gets the current condition, derived from the held rope.</summary>
        public RopeAttachmentState State =>
            Rope == null ? RopeAttachmentState.Idle :
            Rope.cut == -1 ? RopeAttachmentState.Intact :
            Rope.cutTime != 0f ? RopeAttachmentState.Severing :
            RopeAttachmentState.Inert;

        /// <summary>Gets whether a rope is held and uncut.</summary>
        public bool IsIntact => State == RopeAttachmentState.Intact;

        /// <summary>
        /// Gets whether the held rope still takes part in physics. True while intact and while a cut
        /// rope is fading; this is the replacement for <c>rope.cut == -1 || rope.cutTime != 0</c>.
        /// </summary>
        public bool IsSimulated =>
            State is RopeAttachmentState.Intact or RopeAttachmentState.Severing;

        /// <summary>Takes ownership of a rope and marks the source used up.</summary>
        /// <param name="rope">The rope to hold.</param>
        /// <returns>
        /// <see langword="true"/> when an idle attachment takes the rope; otherwise,
        /// <see langword="false"/> when a rope is already held.
        /// </returns>
        public bool TryAttach(Bungee rope)
        {
            if (rope == null || Rope != null)
            {
                return false;
            }

            Rope = rope;
            SourceExhausted = true;
            return true;
        }

        /// <summary>
        /// Disposes the held rope and returns to <see cref="RopeAttachmentState.Idle"/> without
        /// clearing <see cref="SourceExhausted"/>. Used by ghost morphing and disposal.
        /// </summary>
        public void Release()
        {
            Rope?.Dispose();
            Rope = null;
        }

        /// <summary>
        /// Marks the source used up without attaching anything. Needed by a source that is spent by
        /// something other than a successful attach, such as a radius that finished fading out.
        /// </summary>
        public void MarkSourceExhausted()
        {
            SourceExhausted = true;
        }
    }
}
