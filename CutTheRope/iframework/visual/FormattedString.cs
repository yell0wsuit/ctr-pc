using CutTheRope.ios;

namespace CutTheRope.iframework.visual
{
    internal class FormattedString : NSObject
    {
        public virtual FormattedString initWithStringAndWidth(NSString str, float w)
        {
            if (base.init() != null)
            {
                string_ = (NSString)NSRET(str);
                width = w;
            }
            return this;
        }

        public override void dealloc()
        {
            string_ = null;
            base.dealloc();
        }

        public NSString string_;

        public float width;
    }
}
