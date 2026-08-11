namespace CutTheRopeDX.GameMain
{
    /// <summary>
    /// Describes the logical outcome state of one candy independently of its active physical bodies.
    /// </summary>
    /// <param name="Presence">The candy's current lifecycle topology.</param>
    /// <param name="RemovalReason">
    /// The permanent removal reason when <paramref name="Presence"/> is
    /// <see cref="CandyPresence.Removed"/>; otherwise <see langword="null"/>.
    /// </param>
    /// <param name="CanBeEaten">Whether this logical candy participates in the level's win condition.</param>
    /// <param name="HasFailedSplitHalf">
    /// Whether an owned split half was permanently removed for a non-eaten reason.
    /// </param>
    internal readonly record struct CandyOutcomeView(
        CandyPresence Presence,
        CandyRemovalReason? RemovalReason,
        bool CanBeEaten,
        bool HasFailedSplitHalf);
}
