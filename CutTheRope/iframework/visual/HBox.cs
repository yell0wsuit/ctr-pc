namespace CutTheRope.iframework.visual
{
    internal sealed class HBox : BaseElement
    {
        public override int AddChildwithID(BaseElement c, int i)
        {
            int num = base.AddChildwithID(c, i);
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

        public HBox InitWithOffsetAlignHeight(double of, int a, double h)
        {
            return InitWithOffsetAlignHeight((float)of, a, (float)h);
        }

        public HBox InitWithOffsetAlignHeight(float of, int a, float h)
        {
            offset = of;
            align = a;
            nextElementX = 0f;
            height = (int)h;
            return this;
        }

        public float offset;

        public int align;

        public float nextElementX;
    }
}
