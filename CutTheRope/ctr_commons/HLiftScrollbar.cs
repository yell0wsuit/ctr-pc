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
                setDrawQuad(bq);
                Image up = Image_createWithResIDQuad(resID, lq);
                Image image = Image_createWithResIDQuad(resID, lqp);
                Vector relativeQuadOffset = getRelativeQuadOffset(resID, lq, lqp);
                image.x += relativeQuadOffset.x;
                image.y += relativeQuadOffset.y;
                lift = (Lift)new Lift().initWithUpElementDownElementandID(up, image, 0);
                lift.parentAnchor = 17;
                lift.anchor = 18;
                lift.minX = 1f;
                lift.maxX = width - lift.minX;
                lift.liftDelegate = new Lift.PercentXY(percentXY);
                int num = 45;
                lift.setTouchIncreaseLeftRightTopBottom(num, num, -5f, 10f);
                addChild(lift);
                spointsNum = 0;
                spoints = null;
                activeSpoint = 0;
            }
            return this;
        }

        public virtual Vector getScrollPoint(int i)
        {
            return spoints[i];
        }

        public virtual int getTotalScrollPoints()
        {
            return spointsNum;
        }

        public virtual void updateActiveSpoint()
        {
            int i = 0;
            while (i < spointsNum)
            {
                if (lift.x <= spointsLimits[i].x)
                {
                    activeSpoint = limitPoints[i];
                    if (delegateLiftScrollbarDelegate != null)
                    {
                        delegateLiftScrollbarDelegate.changedActiveSpointFromTo(0, activeSpoint);
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
            updateLift();
            for (int i = 0; i < spointsNum; i++)
            {
                if (lift.x <= spointsLimits[i].x)
                {
                    int num = limitPoints[i];
                    if (activeSpoint != num)
                    {
                        delegateLiftScrollbarDelegate?.changedActiveSpointFromTo(activeSpoint, num);
                        activeSpoint = num;
                    }
                    return;
                }
            }
            if (lift.x >= spointsLimits[spointsNum - 1].x && activeSpoint != limitPoints[spointsNum - 1])
            {
                delegateLiftScrollbarDelegate?.changedActiveSpointFromTo(activeSpoint, limitPoints[spointsNum - 1]);
                activeSpoint = limitPoints[spointsNum - 1];
            }
        }

        public override void dealloc()
        {
            spoints = null;
            spointsLimits = null;
            limitPoints = null;
            container = null;
            delegateLiftScrollbarDelegate = null;
            base.dealloc();
        }

        public override bool onTouchDownXY(float tx, float ty)
        {
            return base.onTouchDownXY(tx, ty);
        }

        public override bool onTouchUpXY(float tx, float ty)
        {
            bool flag = base.onTouchUpXY(tx, ty);
            container.startMovingToSpointInDirection(vectZero);
            return flag;
        }

        public void percentXY(float px, float py)
        {
            Vector maxScroll = container.getMaxScroll();
            container.setScroll(vect(maxScroll.x * px, maxScroll.y * py));
        }

        public virtual void updateLift()
        {
            Vector scroll = container.getScroll();
            Vector maxScroll = container.getMaxScroll();
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
            lift.x = (lift.maxX - lift.minX) * num + lift.minX;
            lift.y = (lift.maxY - lift.minY) * num2 + lift.minY;
        }

        public virtual void calcScrollPoints()
        {
            Vector maxScroll = container.getMaxScroll();
            spointsNum = container.getTotalScrollPoints();
            spoints = null;
            spointsLimits = null;
            limitPoints = null;
            spoints = new Vector[spointsNum];
            spointsLimits = new Vector[spointsNum];
            limitPoints = new int[spointsNum];
            for (int i = 0; i < spointsNum; i++)
            {
                Vector vector = vectNeg(container.getScrollPoint(i));
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
                float num3 = (lift.maxX - lift.minX) * num + lift.minX;
                float num4 = (lift.maxY - lift.minY) * num2 + lift.minY;
                spoints[i] = vect(num3, num4);
            }
            for (int j = 0; j < spointsNum; j++)
            {
                spointsLimits[j] = spoints[j];
                limitPoints[j] = j;
            }
            bool flag = true;
            while (flag)
            {
                flag = false;
                for (int k = 0; k < spointsNum - 1; k++)
                {
                    if (spointsLimits[k].x > spointsLimits[k + 1].x)
                    {
                        flag = true;
                        Vector vector2 = spointsLimits[k];
                        spointsLimits[k] = spointsLimits[k + 1];
                        spointsLimits[k + 1] = vector2;
                        int num5 = limitPoints[k];
                        limitPoints[k] = limitPoints[k + 1];
                        limitPoints[k + 1] = num5;
                    }
                }
            }
            for (int l = 0; l < spointsNum - 1; l++)
            {
                Vector vector3 = spointsLimits[l];
                Vector vector4 = spointsLimits[l + 1];
                Vector[] array = spointsLimits;
                int num6 = l;
                array[num6].x = array[num6].x + (vector4.x - vector3.x) / 2f;
            }
        }

        public virtual void setContainer(ScrollableContainer c)
        {
            container = c;
            if (container != null)
            {
                calcScrollPoints();
                updateLift();
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
