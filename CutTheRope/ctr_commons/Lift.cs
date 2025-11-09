using CutTheRope.iframework.visual;
using System;

namespace CutTheRope.ctr_commons
{
    internal class Lift : Button
    {
        public override bool onTouchDownXY(float tx, float ty)
        {
            startX = tx - x;
            startY = ty - y;
            return base.onTouchDownXY(tx, ty);
        }

        public override bool onTouchUpXY(float tx, float ty)
        {
            startX = 0f;
            startY = 0f;
            return base.onTouchUpXY(tx, ty);
        }

        public override bool onTouchMoveXY(float tx, float ty)
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
                        if (liftDelegate != null)
                        {
                            liftDelegate(xPercent, yPercent);
                        }
                    }
                }
                if (maxY != 0f)
                {
                    float num2 = (y - minY) / (maxY - minY);
                    if (num2 != yPercent)
                    {
                        yPercent = num2;
                        if (liftDelegate != null)
                        {
                            liftDelegate(xPercent, yPercent);
                        }
                    }
                }
                return true;
            }
            return base.onTouchMoveXY(tx, ty);
        }

        public override void dealloc()
        {
            liftDelegate = null;
            base.dealloc();
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
