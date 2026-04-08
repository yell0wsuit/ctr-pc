using System;

using CutTheRope.Framework.Visual;

namespace CutTheRope.Commons
{
    /// <summary>
    /// A draggable <see cref="Button"/> that moves horizontally within a bounded range and reports its position as a percentage.
    /// </summary>
    internal sealed class Lift : Button
    {
        /// <inheritdoc />
        public override bool OnTouchDownXY(float tx, float ty)
        {
            startX = tx - x;
            return base.OnTouchDownXY(tx, ty);
        }

        /// <inheritdoc />
        public override bool OnTouchUpXY(float tx, float ty)
        {
            startX = 0f;
            return base.OnTouchUpXY(tx, ty);
        }

        /// <inheritdoc />
        public override bool OnTouchMoveXY(float tx, float ty)
        {
            if (state == BUTTON_STATE.BUTTON_DOWN)
            {
                x = MathF.Max(MathF.Min(tx - startX, maxX), minX);
                y = 0f;
                if (maxX != 0f)
                {
                    float xRatio = (x - minX) / (maxX - minX);
                    if (xRatio != xPercent)
                    {
                        xPercent = xRatio;
                        liftDelegate?.Invoke(xPercent, 0f);
                    }
                }
                return true;
            }
            return base.OnTouchMoveXY(tx, ty);
        }

        /// <inheritdoc />
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                liftDelegate = null;
            }
            base.Dispose(disposing);
        }

        /// <summary>
        /// Touch-down X offset used to keep the lift position relative to where the user grabbed it.
        /// </summary>
        public float startX;

        /// <summary>
        /// Callback invoked when the lift position changes, receiving the X and Y percentages.
        /// </summary>
        public PercentXY liftDelegate;

        /// <summary>
        /// Minimum allowed X position for the lift.
        /// </summary>
        public float minX;

        /// <summary>
        /// Maximum allowed X position for the lift.
        /// </summary>
        public float maxX;

        /// <summary>
        /// Current horizontal position as a percentage (0..1) between <see cref="minX"/> and <see cref="maxX"/>.
        /// </summary>
        public float xPercent;

        /// <summary>
        /// Delegate type for receiving lift position changes as X and Y percentages.
        /// </summary>
        /// <param name="px">Horizontal percentage (0..1).</param>
        /// <param name="py">Vertical percentage (0..1).</param>
        public delegate void PercentXY(float px, float py);
    }
}
