using CutTheRopeDX.Framework.Core;

namespace CutTheRopeDX.GameMain
{
    /// <summary>
    /// Immutable snapshot of one active candy body, taken for the pure mouth-range decision (no
    /// graphics or scene dependencies). A body only exists while it is in play, so the snapshot
    /// carries no presence flag of its own: whether a candy is gone is the lifecycle's answer.
    /// </summary>
    /// <param name="Position">World-space position of the candy body.</param>
    /// <param name="Capabilities">Optional candy-like behavior flags. Null means regular candy.</param>
    internal readonly record struct CandyView(
        Vector Position,
        CandyCapabilities Capabilities = null)
    {
        /// <summary>Gets the flags to apply, treating a null <see cref="Capabilities"/> as regular candy.</summary>
        public CandyCapabilities EffectiveCapabilities => Capabilities ?? CandyCapabilities.Candy;

        /// <summary>Gets whether a body like this one opens a target's mouth.</summary>
        public bool CanOpenMouth => EffectiveCapabilities.CanOpenMouth;
    }
}
