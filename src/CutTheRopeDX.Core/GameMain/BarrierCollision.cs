using CutTheRopeDX.Framework.Helpers;

namespace CutTheRopeDX.GameMain
{
    /// <summary>
    /// Pure collision test shared by spike and bouncer.
    /// </summary>
    internal static class BarrierCollision
    {
        public static bool Hits(
            float t1x, float t1y, float t2x, float t2y,
            float b1x, float b1y, float b2x, float b2y,
            float px, float py, float prevX, float prevY, float radius)
        {
            float bbX = px - radius;
            float bbY = py - radius;
            float bbSize = radius * 2f;
            return CTRMathHelper.LineInRect(t1x, t1y, t2x, t2y, bbX, bbY, bbSize, bbSize)
                || CTRMathHelper.LineInRect(b1x, b1y, b2x, b2y, bbX, bbY, bbSize, bbSize)
                || CTRMathHelper.LineInLine(prevX, prevY, px, py, t1x, t1y, t2x, t2y)
                || CTRMathHelper.LineInLine(prevX, prevY, px, py, b1x, b1y, b2x, b2y);
        }
    }
}
