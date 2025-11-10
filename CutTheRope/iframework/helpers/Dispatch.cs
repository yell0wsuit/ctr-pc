using CutTheRope.ios;

namespace CutTheRope.iframework.helpers
{
    internal class Dispatch : NSObject
    {
        public virtual Dispatch initWithObjectSelectorParamafterDelay(DelayedDispatcher.DispatchFunc callThisFunc, NSObject p, float d)
        {
            callThis = callThisFunc;
            param = p;
            delay = d;
            return this;
        }

        public virtual void dispatch()
        {
            if (callThis != null)
            {
                callThis(param);
            }
        }

        public float delay;

        public DelayedDispatcher.DispatchFunc callThis;

        public NSObject param;
    }
}
