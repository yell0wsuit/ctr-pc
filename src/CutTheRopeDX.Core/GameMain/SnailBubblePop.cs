namespace CutTheRopeDX.GameMain
{
    /// <summary>
    /// A snail and a bubble cannot coexist on one candy. While a snail actively rides, the ridden
    /// candy's bubble pops — the snail wins (Experiments reference). Per candy: only the
    /// ridden candy's bubble is affected.
    /// </summary>
    internal static class SnailBubblePop
    {
        /// <param name="snailActive">True when the snail is in its riding state.</param>
        /// <param name="ridesACandy">True when the snail's attached point resolves to a candy.</param>
        /// <param name="candyHasBubble">True when that candy currently has a bubble.</param>
        public static bool ShouldPop(bool snailActive, bool ridesACandy, bool candyHasBubble)
        {
            return snailActive && ridesACandy && candyHasBubble;
        }
    }
}
