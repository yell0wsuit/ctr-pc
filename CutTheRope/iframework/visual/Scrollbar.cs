using CutTheRope.iframework.core;
using CutTheRope.iframework.helpers;
using CutTheRope.windows;
using Microsoft.Xna.Framework;
using System;

namespace CutTheRope.iframework.visual
{
    internal class Scrollbar : BaseElement
    {
        public override void update(float delta)
        {
            base.update(delta);
            if (this.delegateProvider != null)
            {
                this.delegateProvider(ref this.sp, ref this.mp, ref this.sc);
            }
        }

        public override void draw()
        {
            base.preDraw();
            if (CTRMathHelper.vectEqual(this.sp, CTRMathHelper.vectUndefined) && this.delegateProvider != null)
            {
                this.delegateProvider(ref this.sp, ref this.mp, ref this.sc);
            }
            OpenGL.glDisable(0);
            bool flag = false;
            float num;
            float num2;
            float num3;
            float num5;
            if (this.vertical)
            {
                num = (float)this.width - 2f;
                num2 = 1f;
                num3 = (float)Math.Round(((double)this.height - 2.0) / (double)this.sc.y);
                float num4 = (this.mp.y != 0f) ? (this.sp.y / this.mp.y) : 1f;
                num5 = (float)(1.0 + ((double)this.height - 2.0 - (double)num3) * (double)num4);
                if (num3 > (float)this.height)
                {
                    flag = true;
                }
            }
            else
            {
                num3 = (float)this.height - 2f;
                num5 = 1f;
                num = (float)Math.Round(((double)this.width - 2.0) / (double)this.sc.x);
                float num6 = (this.mp.x != 0f) ? (this.sp.x / this.mp.x) : 1f;
                num2 = (float)(1.0 + ((double)this.width - 2.0 - (double)num) * (double)num6);
                if (num > (float)this.width)
                {
                    flag = true;
                }
            }
            if (!flag)
            {
                GLDrawer.drawSolidRectWOBorder(this.drawX, this.drawY, (float)this.width, (float)this.height, this.backColor);
                GLDrawer.drawSolidRectWOBorder(this.drawX + num2, this.drawY + num5, num, num3, this.scrollerColor);
            }
            OpenGL.glEnable(0);
            OpenGL.glColor4f(Color.White);
            base.postDraw();
        }

        public virtual Scrollbar initWithWidthHeightVertical(float w, float h, bool v)
        {
            if (this.init() != null)
            {
                this.width = (int)w;
                this.height = (int)h;
                this.vertical = v;
                this.sp = CTRMathHelper.vectUndefined;
                this.mp = CTRMathHelper.vectUndefined;
                this.sc = CTRMathHelper.vectUndefined;
                this.backColor = RGBAColor.MakeRGBA(1f, 1f, 1f, 0.5f);
                this.scrollerColor = RGBAColor.MakeRGBA(0f, 0f, 0f, 0.5f);
            }
            return this;
        }

        public Vector sp;

        public Vector mp;

        public Vector sc;

        public Scrollbar.ProvideScrollPosMaxScrollPosScrollCoeff delegateProvider;

        public bool vertical;

        public RGBAColor backColor;

        public RGBAColor scrollerColor;

        // (Invoke) Token: 0x0600066C RID: 1644
        public delegate void ProvideScrollPosMaxScrollPosScrollCoeff(ref Vector sp, ref Vector mp, ref Vector sc);
    }
}
