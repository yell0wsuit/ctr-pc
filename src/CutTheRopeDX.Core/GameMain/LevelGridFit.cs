using System;

using CutTheRopeDX.Framework;

namespace CutTheRopeDX.GameMain
{
    /// <summary>
    /// How large the level picker's grid may be drawn before it runs under the chrome in the
    /// screen's corners.
    /// </summary>
    /// 
    internal static class LevelGridFit
    {
        /// <summary>
        /// Returns the largest scale, no greater than the one asked for, at which a grid centered
        /// on the viewport clears every given piece of chrome.
        /// </summary>
        /// <param name="visible">The logical region the viewport exposes.</param>
        /// <param name="desired">The scale the grid would be drawn at unhindered.</param>
        /// <param name="gridWidth">Authored width of the grid's widest row.</param>
        /// <param name="gridHeight">Authored height of the grid.</param>
        /// <param name="chrome">Rectangles the grid must not overlap, in logical space.</param>
        /// <returns>The scale to draw the grid at.</returns>
        public static float ScaleFor(
            CTRRectangle visible,
            float desired,
            float gridWidth,
            float gridHeight,
            params CTRRectangle[] chrome)
        {
            if (gridWidth <= 0f || gridHeight <= 0f || chrome == null)
            {
                return desired;
            }

            float scale = desired;
            foreach (CTRRectangle rectangle in chrome)
            {
                if (rectangle.w <= 0f || rectangle.h <= 0f)
                {
                    continue;
                }

                scale = MathF.Min(scale, Clearing(visible, gridWidth, gridHeight, rectangle));
            }

            return MathF.Max(0f, scale);
        }

        /// <summary>
        /// The largest scale at which a centered grid clears one rectangle, by standing clear of it
        /// on whichever axis allows the grid to be larger.
        /// </summary>
        /// <param name="visible">The logical region the viewport exposes.</param>
        /// <param name="gridWidth">Authored width of the grid's widest row.</param>
        /// <param name="gridHeight">Authored height of the grid.</param>
        /// <param name="rectangle">Rectangle to clear.</param>
        /// <returns>The scale that clears it.</returns>
        private static float Clearing(
            CTRRectangle visible,
            float gridWidth,
            float gridHeight,
            CTRRectangle rectangle)
        {
            float centerX = visible.w / 2f;
            float centerY = visible.h / 2f;

            // How far the grid's own edge may reach before it meets the rectangle's near edge, on
            // the side of center the rectangle is on.
            float horizontal = rectangle.x + (rectangle.w / 2f) < centerX
                ? centerX - (rectangle.x + rectangle.w)
                : rectangle.x - centerX;
            float vertical = rectangle.y + (rectangle.h / 2f) < centerY
                ? centerY - (rectangle.y + rectangle.h)
                : rectangle.y - centerY;

            return MathF.Max(2f * horizontal / gridWidth, 2f * vertical / gridHeight);
        }
    }
}
