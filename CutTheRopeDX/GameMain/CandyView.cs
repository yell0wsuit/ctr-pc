using CutTheRopeDX.Framework.Core;

namespace CutTheRopeDX.GameMain
{
    /// <summary>
    /// Immutable snapshot of a candy used by pure decision helpers (no graphics dependencies).
    /// </summary>
    /// <param name="Position">World-space position of the candy body.</param>
    /// <param name="Consumed">Whether the candy has already been eaten/removed.</param>
    internal readonly record struct CandyView(Vector Position, bool Consumed);
}
