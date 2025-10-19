using System;
using CutTheRope.ios;

namespace CutTheRope.iframework.visual
{
	// Token: 0x02000026 RID: 38
	internal class Action : NSObject
	{
		// Token: 0x06000144 RID: 324 RVA: 0x00006F25 File Offset: 0x00005125
		public Action()
		{
			this.data = new ActionData();
		}

		// Token: 0x06000145 RID: 325 RVA: 0x00006F38 File Offset: 0x00005138
		public static Action createAction(BaseElement target, string action, int p, int sp)
		{
			Action action2 = (Action)new Action().init();
			action2.actionTarget = target;
			action2.data.actionName = action;
			action2.data.actionParam = p;
			action2.data.actionSubParam = sp;
			return action2;
		}

		// Token: 0x040000F6 RID: 246
		public BaseElement actionTarget;

		// Token: 0x040000F7 RID: 247
		public ActionData data;
	}
}
