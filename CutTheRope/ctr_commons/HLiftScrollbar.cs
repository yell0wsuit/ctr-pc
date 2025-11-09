using CutTheRope.iframework.core;
using CutTheRope.iframework.helpers;
using CutTheRope.iframework.visual;
using System;

namespace CutTheRope.ctr_commons
{
    internal class HLiftScrollbar : Image
    {
        public static HLiftScrollbar createWithResIDBackQuadLiftQuadLiftQuadPressed(int resID, int bq, int lq, int lqp)
        {
            return new HLiftScrollbar().initWithResIDBackQuadLiftQuadLiftQuadPressed(resID, bq, lq, lqp);
        }

        public virtual HLiftScrollbar initWithResIDBackQuadLiftQuadLiftQuadPressed(int resID, int bq, int lq, int lqp)
        {
            if (base.initWithTexture(Application.getTexture(resID)) != null)
            {
                this.setDrawQuad(bq);
                Image up = Image.Image_createWithResIDQuad(resID, lq);
                Image image = Image.Image_createWithResIDQuad(resID, lqp);
                Vector relativeQuadOffset = Image.getRelativeQuadOffset(resID, lq, lqp);
                image.x += relativeQuadOffset.x;
                image.y += relativeQuadOffset.y;
                this.lift = (Lift)new Lift().initWithUpElementDownElementandID(up, image, 0);
                this.lift.parentAnchor = 17;
                this.lift.anchor = 18;
                this.lift.minX = 1f;
                this.lift.maxX = (float)this.width - this.lift.minX;
                this.lift.liftDelegate = new Lift.PercentXY(this.percentXY);
                int num = 45;
                this.lift.setTouchIncreaseLeftRightTopBottom((float)num, (float)num, -5f, 10f);
                this.addChild(this.lift);
                this.spointsNum = 0;
                this.spoints = null;
                this.activeSpoint = 0;
            }
            return this;
        }

        public virtual Vector getScrollPoint(int i)
        {
            return this.spoints[i];
        }

        public virtual int getTotalScrollPoints()
        {
            return this.spointsNum;
        }

        public virtual void updateActiveSpoint()
        {
            int i = 0;
            while (i < this.spointsNum)
            {
                if (this.lift.x <= this.spointsLimits[i].x)
                {
                    this.activeSpoint = this.limitPoints[i];
                    if (this.delegateLiftScrollbarDelegate != null)
                    {
                        this.delegateLiftScrollbarDelegate.changedActiveSpointFromTo(0, this.activeSpoint);
                        return;
                    }
                    break;
                }
                else
                {
                    i++;
                }
            }
        }

        public override void update(float delta)
        {
            base.update(delta);
            this.updateLift();
            for (int i = 0; i < this.spointsNum; i++)
            {
                if (this.lift.x <= this.spointsLimits[i].x)
                {
                    int num = this.limitPoints[i];
                    if (this.activeSpoint != num)
                    {
                        this.delegateLiftScrollbarDelegate?.changedActiveSpointFromTo(this.activeSpoint, num);
                        this.activeSpoint = num;
                    }
                    return;
                }
            }
            if (this.lift.x >= this.spointsLimits[this.spointsNum - 1].x && this.activeSpoint != this.limitPoints[this.spointsNum - 1])
            {
                this.delegateLiftScrollbarDelegate?.changedActiveSpointFromTo(this.activeSpoint, this.limitPoints[this.spointsNum - 1]);
                this.activeSpoint = this.limitPoints[this.spointsNum - 1];
            }
        }

        public override void dealloc()
        {
            this.spoints = null;
            this.spointsLimits = null;
            this.limitPoints = null;
            this.container = null;
            this.delegateLiftScrollbarDelegate = null;
            base.dealloc();
        }

        public override bool onTouchDownXY(float tx, float ty)
        {
            return base.onTouchDownXY(tx, ty);
        }

        public override bool onTouchUpXY(float tx, float ty)
        {
            bool flag = base.onTouchUpXY(tx, ty);
            this.container.startMovingToSpointInDirection(MathHelper.vectZero);
            return flag;
        }

        public void percentXY(float px, float py)
        {
            Vector maxScroll = this.container.getMaxScroll();
            this.container.setScroll(MathHelper.vect(maxScroll.x * px, maxScroll.y * py));
        }

        public virtual void updateLift()
        {
            Vector scroll = this.container.getScroll();
            Vector maxScroll = this.container.getMaxScroll();
            float num = 0f;
            float num2 = 0f;
            if (maxScroll.x != 0f)
            {
                num = scroll.x / maxScroll.x;
            }
            if (maxScroll.y != 0f)
            {
                num2 = scroll.y / maxScroll.y;
            }
            this.lift.x = (this.lift.maxX - this.lift.minX) * num + this.lift.minX;
            this.lift.y = (this.lift.maxY - this.lift.minY) * num2 + this.lift.minY;
        }

        public virtual void calcScrollPoints()
        {
            Vector maxScroll = this.container.getMaxScroll();
            this.spointsNum = this.container.getTotalScrollPoints();
            this.spoints = null;
            this.spointsLimits = null;
            this.limitPoints = null;
            this.spoints = new Vector[this.spointsNum];
            this.spointsLimits = new Vector[this.spointsNum];
            this.limitPoints = new int[this.spointsNum];
            for (int i = 0; i < this.spointsNum; i++)
            {
                Vector vector = MathHelper.vectNeg(this.container.getScrollPoint(i));
                float num = 0f;
                float num2 = 0f;
                if (maxScroll.x != 0f)
                {
                    num = vector.x / maxScroll.x;
                }
                if (maxScroll.y != 0f)
                {
                    num2 = vector.y / maxScroll.y;
                }
                float num3 = (this.lift.maxX - this.lift.minX) * num + this.lift.minX;
                float num4 = (this.lift.maxY - this.lift.minY) * num2 + this.lift.minY;
                this.spoints[i] = MathHelper.vect(num3, num4);
            }
            for (int j = 0; j < this.spointsNum; j++)
            {
                this.spointsLimits[j] = this.spoints[j];
                this.limitPoints[j] = j;
            }
            bool flag = true;
            while (flag)
            {
                flag = false;
                for (int k = 0; k < this.spointsNum - 1; k++)
                {
                    if (this.spointsLimits[k].x > this.spointsLimits[k + 1].x)
                    {
                        flag = true;
                        Vector vector2 = this.spointsLimits[k];
                        this.spointsLimits[k] = this.spointsLimits[k + 1];
                        this.spointsLimits[k + 1] = vector2;
                        int num5 = this.limitPoints[k];
                        this.limitPoints[k] = this.limitPoints[k + 1];
                        this.limitPoints[k + 1] = num5;
                    }
                }
            }
            for (int l = 0; l < this.spointsNum - 1; l++)
            {
                Vector vector3 = this.spointsLimits[l];
                Vector vector4 = this.spointsLimits[l + 1];
                Vector[] array = this.spointsLimits;
                int num6 = l;
                array[num6].x = array[num6].x + (vector4.x - vector3.x) / 2f;
            }
        }

        public virtual void setContainer(ScrollableContainer c)
        {
            this.container = c;
            if (this.container != null)
            {
                this.calcScrollPoints();
                this.updateLift();
            }
        }

        public Vector[] spoints;

        public Vector[] spointsLimits;

        public int[] limitPoints;

        public int spointsNum;

        public int activeSpoint;

        public Lift lift;

        public ScrollableContainer container;

        public LiftScrollbarDelegate delegateLiftScrollbarDelegate;
    }
}
