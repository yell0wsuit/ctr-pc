using System;

using CutTheRope.Framework.Visual;

namespace CutTheRope.Commons
{
    internal sealed class Lift : Button
    {
        public override bool OnTouchDownXY(float tx, float ty)
        {
            startX = tx - x;
            return base.OnTouchDownXY(tx, ty);
        }

        public override bool OnTouchUpXY(float tx, float ty)
        {
            startX = 0f;
            return base.OnTouchUpXY(tx, ty);
        }

        public override bool OnTouchMoveXY(float tx, float ty)
        {
            if (state == BUTTON_STATE.BUTTON_DOWN)
            {
                x = Math.Max(Math.Min(tx - startX, maxX), minX);
                y = 0f;
                if (maxX != 0f)
                {
                    float num = (x - minX) / (maxX - minX);
                    if (num != xPercent)
                    {
                        xPercent = num;
                        liftDelegate?.Invoke(xPercent, 0f);
                    }
                }
                return true;
            }
            return base.OnTouchMoveXY(tx, ty);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                liftDelegate = null;
            }
            base.Dispose(disposing);
        }

        public float startX;

        public PercentXY liftDelegate;

        public float minX;

        public float maxX;

        public float xPercent;

        public delegate void PercentXY(float px, float py);
    }
}
