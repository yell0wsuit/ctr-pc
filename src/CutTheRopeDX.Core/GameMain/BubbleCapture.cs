using CutTheRopeDX.Framework.Core;

namespace CutTheRopeDX.GameMain
{
    /// <summary>
    /// Pure geometry for bubble capture: whether a candy's center lies inside a bubble's
    /// square capture region. Mirrors <c>PointInRect(candy, bubble - r, 2r)</c> exactly
    /// (low edge inclusive, high edge exclusive).
    /// </summary>
    internal static class BubbleCapture
    {
        /// <summary>True when <paramref name="candyPos"/> is inside the square of half-size
        /// <paramref name="captureRadius"/> centered on <paramref name="bubblePos"/>.</summary>
        public static bool Captures(Vector candyPos, Vector bubblePos, float captureRadius)
        {
            float cx = bubblePos.X - captureRadius;
            float cy = bubblePos.Y - captureRadius;
            float size = captureRadius * 2f;
            return candyPos.X >= cx && candyPos.X < cx + size
                && candyPos.Y >= cy && candyPos.Y < cy + size;
        }
    }
}
