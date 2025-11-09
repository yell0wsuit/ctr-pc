using System;

namespace CutTheRope.iframework.visual
{
    internal class VBox : BaseElement
    {
        public override int addChildwithID(BaseElement c, int i)
        {
            int num = base.addChildwithID(c, i);
            if (this.align == 1)
            {
                c.anchor = c.parentAnchor = 9;
            }
            else if (this.align == 4)
            {
                c.anchor = c.parentAnchor = 12;
            }
            else if (this.align == 2)
            {
                c.anchor = c.parentAnchor = 10;
            }
            c.y = this.nextElementY;
            this.nextElementY += (float)c.height + this.offset;
            this.height = (int)(this.nextElementY - this.offset);
            return num;
        }

        public virtual VBox initWithOffsetAlignWidth(double of, int a, double w)
        {
            return this.initWithOffsetAlignWidth((float)of, a, (float)w);
        }

        public virtual VBox initWithOffsetAlignWidth(float of, int a, float w)
        {
            if (this.init() != null)
            {
                this.offset = of;
                this.align = a;
                this.nextElementY = 0f;
                this.width = (int)w;
            }
            return this;
        }

        public float offset;

        public int align;

        public float nextElementY;
    }
}
