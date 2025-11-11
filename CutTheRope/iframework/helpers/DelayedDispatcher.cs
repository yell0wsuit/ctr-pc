using CutTheRope.ios;
using System;
using System.Collections.Generic;

namespace CutTheRope.iframework.helpers
{
    internal sealed class DelayedDispatcher : NSObject
    {
        public DelayedDispatcher()
        {
            dispatchers = [];
        }

        public override void Dealloc()
        {
            dispatchers.Clear();
            dispatchers = null;
            base.Dealloc();
        }

        public void CallObjectSelectorParamafterDelay(DispatchFunc s, NSObject p, double d)
        {
            CallObjectSelectorParamafterDelay(s, p, (float)d);
        }

        public void CallObjectSelectorParamafterDelay(DispatchFunc s, NSObject p, float d)
        {
            DispatchClass item = new DispatchClass().InitWithObjectSelectorParamafterDelay(s, p, d);
            dispatchers.Add(item);
        }

        public void Update(float d)
        {
            int count = dispatchers.Count;
            for (int i = 0; i < count; i++)
            {
                DispatchClass dispatch = dispatchers[i];
                dispatch.delay -= d;
                if (dispatch.delay <= 0.0)
                {
                    dispatch.Dispatch();
                    _ = dispatchers.Remove(dispatch);
                    i--;
                    count = dispatchers.Count;
                }
            }
        }

        public void CancelAllDispatches()
        {
            dispatchers.Clear();
        }

        public void CancelDispatchWithObjectSelectorParam(DispatchFunc s, NSObject p)
        {
            throw new NotImplementedException();
        }

        private List<DispatchClass> dispatchers;

        // (Invoke) Token: 0x06000670 RID: 1648
        public delegate void DispatchFunc(NSObject param);
    }
}
