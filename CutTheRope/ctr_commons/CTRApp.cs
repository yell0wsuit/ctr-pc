using System;
using CutTheRope.iframework.core;
using CutTheRope.ios;

namespace CutTheRope.ctr_commons
{
	// Token: 0x0200009A RID: 154
	internal class CTRApp : Application
	{
		// Token: 0x0600060E RID: 1550 RVA: 0x00032992 File Offset: 0x00030B92
		public override void dealloc()
		{
			throw new NotImplementedException();
		}

		// Token: 0x0600060F RID: 1551 RVA: 0x00032999 File Offset: 0x00030B99
		public virtual void applicationWillTerminate(UIApplication application)
		{
			Application.sharedPreferences().savePreferences();
		}

		// Token: 0x06000610 RID: 1552 RVA: 0x000329A5 File Offset: 0x00030BA5
		public virtual void applicationDidReceiveMemoryWarning(UIApplication application)
		{
			throw new NotImplementedException();
		}

		// Token: 0x06000611 RID: 1553 RVA: 0x000329AC File Offset: 0x00030BAC
		public virtual void challengeStartedWithGameConfig(NSString gameConfig)
		{
			throw new NotImplementedException();
		}

		// Token: 0x06000612 RID: 1554 RVA: 0x000329B3 File Offset: 0x00030BB3
		public virtual void applicationWillResignActive(UIApplication application)
		{
			Application.sharedPreferences().savePreferences();
			if (Application.root != null && !Application.root.isSuspended())
			{
				Application.root.suspend();
			}
		}

		// Token: 0x06000613 RID: 1555 RVA: 0x000329DC File Offset: 0x00030BDC
		public virtual void applicationDidBecomeActive(UIApplication application)
		{
			if (Application.root != null && Application.root.isSuspended())
			{
				Application.root.resume();
			}
		}
	}
}
