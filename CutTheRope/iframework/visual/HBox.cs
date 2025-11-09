using System;

namespace CutTheRope.iframework.visual
{
    internal class HBox : BaseElement
    {
        public override int addChildwithID(BaseElement c, int i)
        {
            int num = base.addChildwithID(c, i);
            if (this.align == 8)
            {
                c.anchor = c.parentAnchor = 9;
            }
            else if (this.align == 16)
            {
                c.anchor = c.parentAnchor = 17;
            }
            else if (this.align == 32)
            {
                c.anchor = c.parentAnchor = 33;
            }
            c.x = this.nextElementX;
            this.nextElementX += (float)c.width + this.offset;
            this.width = (int)(this.nextElementX - this.offset);
            return num;
        }

        public virtual HBox initWithOffsetAlignHeight(double of, int a, double h)
        {
            return this.initWithOffsetAlignHeight((float)of, a, (float)h);
        }

        public virtual HBox initWithOffsetAlignHeight(float of, int a, float h)
        {
            if (this.init() != null)
            {
                this.offset = of;
                this.align = a;
                this.nextElementX = 0f;
                this.height = (int)h;
            }
            return this;
        }

        public float offset;

        public int align;

        public float nextElementX;
    }
}
