using System;

using Microsoft.Xna.Framework;

namespace CutTheRopeDX.Desktop
{
    internal static class BackingScaleMath
    {
        public static Rectangle LogicalToPixelRect(Rectangle logicalRect, double scale)
        {
            double normalizedScale = NormalizeScale(scale);

            static int ScaleValue(int value, double s)
            {
                return (int)Math.Round(value * s, MidpointRounding.AwayFromZero);
            }

            return new Rectangle(
                ScaleValue(logicalRect.X, normalizedScale),
                ScaleValue(logicalRect.Y, normalizedScale),
                Math.Max(1, ScaleValue(logicalRect.Width, normalizedScale)),
                Math.Max(1, ScaleValue(logicalRect.Height, normalizedScale)));
        }

        public static double NormalizeScale(double scale)
        {
            return scale > 0d ? scale : 1d;
        }

        public static Rectangle ResolvePresentDestinationRect(
            Rectangle logicalRect,
            Rectangle pixelRect,
            int reportedBackBufferWidth,
            int reportedBackBufferHeight,
            double backingScale)
        {
            double normalizedScale = NormalizeScale(backingScale);
            return normalizedScale > 1.01d
                ? pixelRect
                : pixelRect.Right > reportedBackBufferWidth || pixelRect.Bottom > reportedBackBufferHeight ? logicalRect : pixelRect;
        }
    }
}
