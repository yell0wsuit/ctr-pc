using System;
using CutTheRope.ios;

namespace CutTheRope.iframework.helpers
{
	// Token: 0x02000060 RID: 96
	internal class Dispatch : NSObject
	{
		// Token: 0x06000337 RID: 823 RVA: 0x00012B19 File Offset: 0x00010D19
		public virtual Dispatch initWithObjectSelectorParamafterDelay(DelayedDispatcher.DispatchFunc callThisFunc, NSObject p, float d)
		{
			this.callThis = callThisFunc;
			this.param = p;
			this.delay = d;
			return this;
		}

		// Token: 0x06000338 RID: 824 RVA: 0x00012B31 File Offset: 0x00010D31
		public virtual void dispatch()
		{
			if (this.callThis != null)
			{
				this.callThis(this.param);
			}
		}

		// Token: 0x04000274 RID: 628
		public float delay;

		// Token: 0x04000275 RID: 629
		public DelayedDispatcher.DispatchFunc callThis;

		// Token: 0x04000276 RID: 630
		public NSObject param;
	}
}
