using CutTheRope.ios;
using System;

namespace CutTheRope.iframework.helpers
{
    internal class Dispatch : NSObject
    {
        public virtual Dispatch initWithObjectSelectorParamafterDelay(DelayedDispatcher.DispatchFunc callThisFunc, NSObject p, float d)
        {
            this.callThis = callThisFunc;
            this.param = p;
            this.delay = d;
            return this;
        }

        public virtual void dispatch()
        {
            if (this.callThis != null)
            {
                this.callThis(this.param);
            }
        }

        public float delay;

        public DelayedDispatcher.DispatchFunc callThis;

        public NSObject param;
    }
}
