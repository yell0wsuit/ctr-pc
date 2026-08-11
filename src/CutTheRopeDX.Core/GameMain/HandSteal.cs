namespace CutTheRopeDX.GameMain
{
    /// <summary>
    /// Decides whether a rival mechanical hand must release a candy that another hand is now grabbing.
    /// Keyed to the specific candy so that, with several hands and candies in play, only the hand holding
    /// the contested candy lets go.
    /// </summary>
    internal static class HandSteal
    {
        /// <summary>
        /// True when <paramref name="otherHandHoldsThisCandy"/> should be forced to release: it is a
        /// different hand, it is currently holding a candy, and that candy is the one being grabbed.
        /// </summary>
        /// <param name="isDifferentHand">True when the candidate is not the grabbing hand.</param>
        /// <param name="otherHandHoldingCandy">True when the candidate hand is in its holding state.</param>
        /// <param name="otherHandHoldsThisCandy">True when the candidate hand holds the candy being grabbed.</param>
        public static bool ShouldReleaseOtherHand(
            bool isDifferentHand,
            bool otherHandHoldingCandy,
            bool otherHandHoldsThisCandy)
        {
            return isDifferentHand && otherHandHoldingCandy && otherHandHoldsThisCandy;
        }
    }
}
