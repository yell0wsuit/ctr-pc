using CutTheRope.iframework.visual;
using System;

namespace CutTheRope.ctr_commons
{
    internal class Lift : Button
    {
        public override bool onTouchDownXY(float tx, float ty)
        {
            this.startX = tx - this.x;
            this.startY = ty - this.y;
            return base.onTouchDownXY(tx, ty);
        }

        public override bool onTouchUpXY(float tx, float ty)
        {
            this.startX = 0f;
            this.startY = 0f;
            return base.onTouchUpXY(tx, ty);
        }

        public override bool onTouchMoveXY(float tx, float ty)
        {
            if (this.state == Button.BUTTON_STATE.BUTTON_DOWN)
            {
                this.x = Math.Max(Math.Min(tx - this.startX, this.maxX), this.minX);
                this.y = Math.Max(Math.Min(ty - this.startY, this.maxY), this.minY);
                if (this.maxX != 0f)
                {
                    float num = (this.x - this.minX) / (this.maxX - this.minX);
                    if (num != this.xPercent)
                    {
                        this.xPercent = num;
                        if (this.liftDelegate != null)
                        {
                            this.liftDelegate(this.xPercent, this.yPercent);
                        }
                    }
                }
                if (this.maxY != 0f)
                {
                    float num2 = (this.y - this.minY) / (this.maxY - this.minY);
                    if (num2 != this.yPercent)
                    {
                        this.yPercent = num2;
                        if (this.liftDelegate != null)
                        {
                            this.liftDelegate(this.xPercent, this.yPercent);
                        }
                    }
                }
                return true;
            }
            return base.onTouchMoveXY(tx, ty);
        }

        public override void dealloc()
        {
            this.liftDelegate = null;
            base.dealloc();
        }

        public float startX;

        public float startY;

        public Lift.PercentXY liftDelegate;

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
