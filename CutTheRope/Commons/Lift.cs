using System;

using CutTheRope.Framework.Visual;

namespace CutTheRope.Commons
{
    internal sealed class Lift : Button
    {
        public override bool OnTouchDownXY(float tx, float ty)
        {
            startX = tx - x;
            startY = ty - y;
            return base.OnTouchDownXY(tx, ty);
        }

        public override bool OnTouchUpXY(float tx, float ty)
        {
            startX = 0f;
            startY = 0f;
            return base.OnTouchUpXY(tx, ty);
        }

        public override bool OnTouchMoveXY(float tx, float ty)
        {
            if (state == BUTTON_STATE.BUTTON_DOWN)
            {
                x = Math.Max(Math.Min(tx - startX, maxX), minX);
                y = Math.Max(Math.Min(ty - startY, maxY), minY);
                if (maxX != 0f)
                {
                    float num = (x - minX) / (maxX - minX);
                    if (num != xPercent)
                    {
                        xPercent = num;
                        liftDelegate?.Invoke(xPercent, yPercent);
                    }
                }
                if (maxY != 0f)
                {
                    float num2 = (y - minY) / (maxY - minY);
                    if (num2 != yPercent)
                    {
                        yPercent = num2;
                        liftDelegate?.Invoke(xPercent, yPercent);
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

        public float startY;

        public PercentXY liftDelegate;

        public float minX;

        public float maxX;

        public float minY;

        public float maxY;

        public float xPercent;

        public float yPercent;

        // (Invoke) Token: 0x0600068D RID: 1677
        public delegate void PercentXY(float px, float py);
    }
}
