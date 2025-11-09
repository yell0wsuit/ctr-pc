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
            if (delegateProvider != null)
            {
                delegateProvider(ref sp, ref mp, ref sc);
            }
        }

        public override void draw()
        {
            base.preDraw();
            if (vectEqual(sp, vectUndefined) && delegateProvider != null)
            {
                delegateProvider(ref sp, ref mp, ref sc);
            }
            OpenGL.glDisable(0);
            bool flag = false;
            float num;
            float num2;
            float num3;
            float num5;
            if (vertical)
            {
                num = width - 2f;
                num2 = 1f;
                num3 = (float)Math.Round((height - 2.0) / sc.y);
                float num4 = (mp.y != 0f) ? (sp.y / mp.y) : 1f;
                num5 = (float)(1.0 + (height - 2.0 - (double)num3) * (double)num4);
                if (num3 > height)
                {
                    flag = true;
                }
            }
            else
            {
                num3 = height - 2f;
                num5 = 1f;
                num = (float)Math.Round((width - 2.0) / sc.x);
                float num6 = (mp.x != 0f) ? (sp.x / mp.x) : 1f;
                num2 = (float)(1.0 + (width - 2.0 - (double)num) * (double)num6);
                if (num > width)
                {
                    flag = true;
                }
            }
            if (!flag)
            {
                GLDrawer.drawSolidRectWOBorder(drawX, drawY, width, height, backColor);
                GLDrawer.drawSolidRectWOBorder(drawX + num2, drawY + num5, num, num3, scrollerColor);
            }
            OpenGL.glEnable(0);
            OpenGL.glColor4f(Color.White);
            base.postDraw();
        }

        public virtual Scrollbar initWithWidthHeightVertical(float w, float h, bool v)
        {
            if (init() != null)
            {
                width = (int)w;
                height = (int)h;
                vertical = v;
                sp = vectUndefined;
                mp = vectUndefined;
                sc = vectUndefined;
                backColor = RGBAColor.MakeRGBA(1f, 1f, 1f, 0.5f);
                scrollerColor = RGBAColor.MakeRGBA(0f, 0f, 0f, 0.5f);
            }
            return this;
        }

        public Vector sp;

        public Vector mp;

        public Vector sc;

        public ProvideScrollPosMaxScrollPosScrollCoeff delegateProvider;

        public bool vertical;

        public RGBAColor backColor;

        public RGBAColor scrollerColor;

        // (Invoke) Token: 0x0600066C RID: 1644
        public delegate void ProvideScrollPosMaxScrollPosScrollCoeff(ref Vector sp, ref Vector mp, ref Vector sc);
    }
}
