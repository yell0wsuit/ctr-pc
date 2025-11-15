namespace CutTheRope.iframework.visual
{
    internal sealed class VBox : BaseElement
    {
        public override int AddChildwithID(BaseElement c, int i)
        {
            int num = base.AddChildwithID(c, i);
            if (align == 1)
            {
                c.anchor = c.parentAnchor = 9;
            }
            else if (align == 4)
            {
                c.anchor = c.parentAnchor = 12;
            }
            else if (align == 2)
            {
                c.anchor = c.parentAnchor = 10;
            }
            c.y = nextElementY;
            nextElementY += c.height + offset;
            height = (int)(nextElementY - offset);
            return num;
        }

        public VBox InitWithOffsetAlignWidth(double of, int a, double w)
        {
            return InitWithOffsetAlignWidth((float)of, a, (float)w);
        }

        public VBox InitWithOffsetAlignWidth(float of, int a, float w)
        {
            offset = of;
            align = a;
            nextElementY = 0f;
            width = (int)w;
            return this;
        }

        public float offset;

        public int align;

        public float nextElementY;
    }
}
