namespace CutTheRopeDX.GameMain
{
    /// <summary>Owns the lifecycle state of one logical candy.</summary>
    internal sealed class CandyLifecycle
    {
        private CandyLifecycle(CandyPresence presence)
        {
            Presence = presence;
        }

        /// <summary>Gets the candy's current lifecycle topology.</summary>
        public CandyPresence Presence { get; private set; }

        /// <summary>Gets the permanent removal reason, or <see langword="null"/> while the candy is not removed.</summary>
        public CandyRemovalReason? RemovalReason { get; private set; }

        /// <summary>Gets whether the candy reached terminal removal by being eaten.</summary>
        public bool WasEaten => Presence == CandyPresence.Removed
            && RemovalReason == CandyRemovalReason.Eaten;

        /// <summary>Gets whether the candy reached terminal removal for a loss reason.</summary>
        public bool HasFailedRemoval => Presence == CandyPresence.Removed
            && RemovalReason is not CandyRemovalReason.Eaten;

        /// <summary>Creates a lifecycle whose whole candy is present.</summary>
        /// <returns>A new present lifecycle.</returns>
        public static CandyLifecycle CreatePresent()
        {
            return new(CandyPresence.Present);
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
    }
}
