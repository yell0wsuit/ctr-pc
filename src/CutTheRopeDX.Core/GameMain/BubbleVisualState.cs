namespace CutTheRopeDX.GameMain
{
    internal readonly record struct BubbleVisualState(bool ShowNormalBubble, bool ShowGhostBubble)
    {
        public static BubbleVisualState ForCapture(bool capturedGhostBubble, bool hasGhostAnimation)
        {
            bool showGhost = capturedGhostBubble && hasGhostAnimation;
            return new BubbleVisualState(!showGhost, showGhost);
        }
    }
}
