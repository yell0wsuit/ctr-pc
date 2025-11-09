using CutTheRope.ios;
using System;
using System.Collections.Generic;

namespace CutTheRope.iframework.helpers
{
    internal class DelayedDispatcher : NSObject
    {
        public DelayedDispatcher()
        {
            this.dispatchers = new List<Dispatch>();
        }

        public override void dealloc()
        {
            this.dispatchers.Clear();
            this.dispatchers = null;
            base.dealloc();
        }

        public virtual void callObjectSelectorParamafterDelay(DelayedDispatcher.DispatchFunc s, NSObject p, double d)
        {
            this.callObjectSelectorParamafterDelay(s, p, (float)d);
        }

        public virtual void callObjectSelectorParamafterDelay(DelayedDispatcher.DispatchFunc s, NSObject p, float d)
        {
            Dispatch item = new Dispatch().initWithObjectSelectorParamafterDelay(s, p, d);
            this.dispatchers.Add(item);
        }

        public virtual void update(float d)
        {
            int count = this.dispatchers.Count;
            for (int i = 0; i < count; i++)
            {
                Dispatch dispatch = this.dispatchers[i];
                dispatch.delay -= d;
                if ((double)dispatch.delay <= 0.0)
                {
                    dispatch.dispatch();
                    this.dispatchers.Remove(dispatch);
                    i--;
                    count = this.dispatchers.Count;
                }
            }
        }

        public virtual void cancelAllDispatches()
        {
            this.dispatchers.Clear();
        }

        public virtual void cancelDispatchWithObjectSelectorParam(DelayedDispatcher.DispatchFunc s, NSObject p)
        {
            throw new NotImplementedException();
        }

        private List<Dispatch> dispatchers;

        // (Invoke) Token: 0x06000670 RID: 1648
        public delegate void DispatchFunc(NSObject param);
    }
}
