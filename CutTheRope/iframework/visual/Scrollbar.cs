using System;

using CutTheRope.desktop;
using CutTheRope.iframework.core;

using Microsoft.Xna.Framework;

namespace CutTheRope.iframework.visual
{
    internal sealed class Scrollbar : BaseElement
    {
        public override void Update(float delta)
        {
            base.Update(delta);
            delegateProvider?.Invoke(ref sp, ref mp, ref sc);
        }

        public override void Draw()
        {
            PreDraw();
            if (VectEqual(sp, vectUndefined) && delegateProvider != null)
            {
                delegateProvider(ref sp, ref mp, ref sc);
            }
            OpenGL.GlDisable(0);
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
                float num4 = mp.y != 0f ? sp.y / mp.y : 1f;
                num5 = (float)(1.0 + ((height - 2.0 - (double)num3) * (double)num4));
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
                float num6 = mp.x != 0f ? sp.x / mp.x : 1f;
                num2 = (float)(1.0 + ((width - 2.0 - (double)num) * (double)num6));
                if (num > width)
                {
                    flag = true;
                }
            }
            if (!flag)
            {
                GLDrawer.DrawSolidRectWOBorder(drawX, drawY, width, height, backColor);
                GLDrawer.DrawSolidRectWOBorder(drawX + num2, drawY + num5, num, num3, scrollerColor);
            }
            OpenGL.GlEnable(0);
            OpenGL.GlColor4f(Color.White);
            PostDraw();
        }

        public Scrollbar InitWithWidthHeightVertical(float w, float h, bool v)
        {
            width = (int)w;
            height = (int)h;
            vertical = v;
            sp = vectUndefined;
            mp = vectUndefined;
            sc = vectUndefined;
            backColor = RGBAColor.MakeRGBA(1f, 1f, 1f, 0.5f);
            scrollerColor = RGBAColor.MakeRGBA(0f, 0f, 0f, 0.5f);
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
