using CutTheRope.ios;
using System;

namespace CutTheRope.iframework.visual
{
    internal class FormattedString : NSObject
    {
        public virtual FormattedString initWithStringAndWidth(NSString str, float w)
        {
            if (base.init() != null)
            {
                this.string_ = (NSString)NSObject.NSRET(str);
                this.width = w;
            }
            return this;
        }

        public override void dealloc()
        {
            this.string_ = null;
            base.dealloc();
        }

        public NSString string_;

        public float width;
    }
}
