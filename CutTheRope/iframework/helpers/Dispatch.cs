using CutTheRope.ios;

namespace CutTheRope.iframework.helpers
{
    internal sealed class DispatchClass : NSObject
    {
        public DispatchClass InitWithObjectSelectorParamafterDelay(DelayedDispatcher.DispatchFunc callThisFunc, NSObject p, float d)
        {
            callThis = callThisFunc;
            param = p;
            delay = d;
            return this;
        }

        public void Dispatch()
        {
            callThis?.Invoke(param);
        }

        public float delay;

        public DelayedDispatcher.DispatchFunc callThis;

        public NSObject param;
    }
}
