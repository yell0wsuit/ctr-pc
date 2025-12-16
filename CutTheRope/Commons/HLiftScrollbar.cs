using CutTheRope.Framework.Core;
using CutTheRope.Framework.Visual;
using CutTheRope.GameMain;

namespace CutTheRope.Commons
{
    internal sealed class HLiftScrollbar : Image
    {
        public static HLiftScrollbar CreateWithResIDBackQuadLiftQuadLiftQuadPressed(int resID, int bq, int lq, int lqp)
        {
            return new HLiftScrollbar().InitWithResIDBackQuadLiftQuadLiftQuadPressed(resID, bq, lq, lqp);
        }

        public HLiftScrollbar InitWithResIDBackQuadLiftQuadLiftQuadPressed(int resID, int bq, int lq, int lqp)
        {
            string resourceName = ResourceNameTranslator.TranslateLegacyId(resID);
            if (InitWithTexture(Application.GetTexture(resourceName)) != null)
            {
                SetDrawQuad(bq);
                Image up = Image_createWithResIDQuad(resID, lq);
                Image image = Image_createWithResIDQuad(resID, lqp);
                Vector relativeQuadOffset = GetRelativeQuadOffset(resID, lq, lqp);
                image.x += relativeQuadOffset.x;
                image.y += relativeQuadOffset.y;
                lift = (Lift)new Lift().InitWithUpElementDownElementandID(up, image, 0);
                lift.parentAnchor = 17;
                lift.anchor = 18;
                lift.minX = 1f;
                lift.maxX = width - lift.minX;
                lift.liftDelegate = new Lift.PercentXY(PercentXY);
                int num = 45;
                lift.SetTouchIncreaseLeftRightTopBottom(num, num, -5f, 10f);
                _ = AddChild(lift);
                spointsNum = 0;
                spoints = null;
                activeSpoint = 0;
            }
            return this;
        }

        public Vector GetScrollPoint(int i)
        {
            return spoints[i];
        }

        public int GetTotalScrollPoints()
        {
            return spointsNum;
        }

        public void UpdateActiveSpoint()
        {
            int i = 0;
            while (i < spointsNum)
            {
                if (lift.x <= spointsLimits[i].x)
                {
                    activeSpoint = limitPoints[i];
                    if (delegateLiftScrollbarDelegate != null)
                    {
                        delegateLiftScrollbarDelegate.ChangedActiveSpointFromTo(0, activeSpoint);
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

        public override void Update(float delta)
        {
            base.Update(delta);
            UpdateLift();
            for (int i = 0; i < spointsNum; i++)
            {
                if (lift.x <= spointsLimits[i].x)
                {
                    int num = limitPoints[i];
                    if (activeSpoint != num)
                    {
                        delegateLiftScrollbarDelegate?.ChangedActiveSpointFromTo(activeSpoint, num);
                        activeSpoint = num;
                    }
                    return;
                }
            }
            if (lift.x >= spointsLimits[spointsNum - 1].x && activeSpoint != limitPoints[spointsNum - 1])
            {
                delegateLiftScrollbarDelegate?.ChangedActiveSpointFromTo(activeSpoint, limitPoints[spointsNum - 1]);
                activeSpoint = limitPoints[spointsNum - 1];
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                spoints = null;
                spointsLimits = null;
                limitPoints = null;
                container = null;
                delegateLiftScrollbarDelegate = null;
            }
            base.Dispose(disposing);
        }

        public override bool OnTouchDownXY(float tx, float ty)
        {
            return base.OnTouchDownXY(tx, ty);
        }

        public override bool OnTouchUpXY(float tx, float ty)
        {
            bool flag = base.OnTouchUpXY(tx, ty);
            container.StartMovingToSpointInDirection(vectZero);
            return flag;
        }

        public void PercentXY(float px, float py)
        {
            Vector maxScroll = container.GetMaxScroll();
            container.SetScroll(Vect(maxScroll.x * px, maxScroll.y * py));
        }

        public void UpdateLift()
        {
            Vector scroll = container.GetScroll();
            Vector maxScroll = container.GetMaxScroll();
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
            lift.x = ((lift.maxX - lift.minX) * num) + lift.minX;
            lift.y = ((lift.maxY - lift.minY) * num2) + lift.minY;
        }

        public void CalcScrollPoints()
        {
            Vector maxScroll = container.GetMaxScroll();
            spointsNum = container.GetTotalScrollPoints();
            spoints = null;
            spointsLimits = null;
            limitPoints = null;
            spoints = new Vector[spointsNum];
            spointsLimits = new Vector[spointsNum];
            limitPoints = new int[spointsNum];
            for (int i = 0; i < spointsNum; i++)
            {
                Vector vector = VectNeg(container.GetScrollPoint(i));
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
                float num3 = ((lift.maxX - lift.minX) * num) + lift.minX;
                float num4 = ((lift.maxY - lift.minY) * num2) + lift.minY;
                spoints[i] = Vect(num3, num4);
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
                        (spointsLimits[k + 1], spointsLimits[k]) = (spointsLimits[k], spointsLimits[k + 1]);
                        (limitPoints[k + 1], limitPoints[k]) = (limitPoints[k], limitPoints[k + 1]);
                    }
                }
            }
            for (int l = 0; l < spointsNum - 1; l++)
            {
                Vector vector3 = spointsLimits[l];
                Vector vector4 = spointsLimits[l + 1];
                Vector[] array = spointsLimits;
                int num6 = l;
                array[num6].x = array[num6].x + ((vector4.x - vector3.x) / 2f);
            }
        }

        public void SetContainer(ScrollableContainer c)
        {
            container = c;
            if (container != null)
            {
                CalcScrollPoints();
                UpdateLift();
            }
        }

        public Vector[] spoints;

        public Vector[] spointsLimits;

        public int[] limitPoints;

        public int spointsNum;

        public int activeSpoint;

        public Lift lift;

        public ScrollableContainer container;

        public ILiftScrollbarDelegate delegateLiftScrollbarDelegate;
    }
}
