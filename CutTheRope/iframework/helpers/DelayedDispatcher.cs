using CutTheRope.ios;
using System;
using System.Collections.Generic;

namespace CutTheRope.iframework.helpers
{
    internal class DelayedDispatcher : NSObject
    {
        public DelayedDispatcher()
        {
            dispatchers = new List<Dispatch>();
        }

        public override void dealloc()
        {
            dispatchers.Clear();
            dispatchers = null;
            base.dealloc();
        }

        public virtual void callObjectSelectorParamafterDelay(DispatchFunc s, NSObject p, double d)
        {
            callObjectSelectorParamafterDelay(s, p, (float)d);
        }

        public virtual void callObjectSelectorParamafterDelay(DispatchFunc s, NSObject p, float d)
        {
            Dispatch item = new Dispatch().initWithObjectSelectorParamafterDelay(s, p, d);
            dispatchers.Add(item);
        }

        public virtual void update(float d)
        {
            int count = dispatchers.Count;
            for (int i = 0; i < count; i++)
            {
                Dispatch dispatch = dispatchers[i];
                dispatch.delay -= d;
                if (dispatch.delay <= 0.0)
                {
                    dispatch.dispatch();
                    dispatchers.Remove(dispatch);
                    i--;
                    count = dispatchers.Count;
                }
            }
        }

        public virtual void cancelAllDispatches()
        {
            dispatchers.Clear();
        }

        public virtual void cancelDispatchWithObjectSelectorParam(DispatchFunc s, NSObject p)
        {
            throw new NotImplementedException();
        }

        private List<Dispatch> dispatchers;

        // (Invoke) Token: 0x06000670 RID: 1648
        public delegate void DispatchFunc(NSObject param);
    }
}
