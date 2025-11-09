using System;

namespace CutTheRope.iframework.visual
{
    internal class HBox : BaseElement
    {
        public override int addChildwithID(BaseElement c, int i)
        {
            int num = base.addChildwithID(c, i);
            if (align == 8)
            {
                c.anchor = c.parentAnchor = 9;
            }
            else if (align == 16)
            {
                c.anchor = c.parentAnchor = 17;
            }
            else if (align == 32)
            {
                c.anchor = c.parentAnchor = 33;
            }
            c.x = nextElementX;
            nextElementX += c.width + offset;
            width = (int)(nextElementX - offset);
            return num;
        }

        public virtual HBox initWithOffsetAlignHeight(double of, int a, double h)
        {
            return initWithOffsetAlignHeight((float)of, a, (float)h);
        }

        public virtual HBox initWithOffsetAlignHeight(float of, int a, float h)
        {
            if (init() != null)
            {
                offset = of;
                align = a;
                nextElementX = 0f;
                height = (int)h;
            }
            return this;
        }

        public float offset;

        public int align;

        public float nextElementX;
    }
}
