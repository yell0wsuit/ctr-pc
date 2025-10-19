using System;
using System.Collections.Generic;
using CutTheRope.ios;

namespace CutTheRope.iframework.helpers
{
	// Token: 0x0200005F RID: 95
	internal class DelayedDispatcher : NSObject
	{
		// Token: 0x06000330 RID: 816 RVA: 0x00012A2F File Offset: 0x00010C2F
		public DelayedDispatcher()
		{
			this.dispatchers = new List<Dispatch>();
		}

		// Token: 0x06000331 RID: 817 RVA: 0x00012A42 File Offset: 0x00010C42
		public override void dealloc()
		{
			this.dispatchers.Clear();
			this.dispatchers = null;
			base.dealloc();
		}

		// Token: 0x06000332 RID: 818 RVA: 0x00012A5C File Offset: 0x00010C5C
		public virtual void callObjectSelectorParamafterDelay(DelayedDispatcher.DispatchFunc s, NSObject p, double d)
		{
			this.callObjectSelectorParamafterDelay(s, p, (float)d);
		}

		// Token: 0x06000333 RID: 819 RVA: 0x00012A68 File Offset: 0x00010C68
		public virtual void callObjectSelectorParamafterDelay(DelayedDispatcher.DispatchFunc s, NSObject p, float d)
		{
			Dispatch item = new Dispatch().initWithObjectSelectorParamafterDelay(s, p, d);
			this.dispatchers.Add(item);
		}

		// Token: 0x06000334 RID: 820 RVA: 0x00012A90 File Offset: 0x00010C90
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

		// Token: 0x06000335 RID: 821 RVA: 0x00012B05 File Offset: 0x00010D05
		public virtual void cancelAllDispatches()
		{
			this.dispatchers.Clear();
		}

		// Token: 0x06000336 RID: 822 RVA: 0x00012B12 File Offset: 0x00010D12
		public virtual void cancelDispatchWithObjectSelectorParam(DelayedDispatcher.DispatchFunc s, NSObject p)
		{
			throw new NotImplementedException();
		}

		// Token: 0x04000273 RID: 627
		private List<Dispatch> dispatchers;

		// Token: 0x020000BA RID: 186
		// (Invoke) Token: 0x06000670 RID: 1648
		public delegate void DispatchFunc(NSObject param);
	}
}
