using CutTheRopeDX.Framework.Core;

namespace CutTheRopeDX.GameMain
{
    /// <summary>
    /// Immutable snapshot of a candy used by pure decision helpers (no graphics dependencies).
    /// </summary>
    /// <param name="Position">World-space position of the candy body.</param>
    /// <param name="Consumed">Whether the candy has already been eaten/removed.</param>
    /// <param name="InTransport">Whether the candy is temporarily hidden while moving through transport.</param>
    /// <param name="Capabilities">Optional candy-like behavior flags. Null means regular candy.</param>
    internal readonly record struct CandyView(
        Vector Position,
        bool Consumed,
        bool InTransport = false,
        CandyCapabilities Capabilities = null)
    {
        public CandyCapabilities EffectiveCapabilities => Capabilities ?? CandyCapabilities.Candy;

        public bool CanOpenMouth => EffectiveCapabilities.CanOpenMouth;

        public bool CanBeEaten => EffectiveCapabilities.CanBeEaten;

        public bool CanLoseLevelWhenOffScreen => EffectiveCapabilities.CanLoseLevelWhenOffScreen;
    }
}
