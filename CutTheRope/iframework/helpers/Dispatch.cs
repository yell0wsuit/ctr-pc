namespace CutTheRope.iframework.helpers
{
    internal sealed class DispatchClass : FrameworkTypes
    {
        public DispatchClass InitWithObjectSelectorParamafterDelay(DelayedDispatcher.DispatchFunc callThisFunc, FrameworkTypes p, float d)
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

        public FrameworkTypes param;
    }
}
