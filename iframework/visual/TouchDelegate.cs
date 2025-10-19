using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework.Input.Touch;

namespace CutTheRope.iframework.visual
{
	// Token: 0x02000051 RID: 81
	internal interface TouchDelegate
	{
		// Token: 0x060002B5 RID: 693
		bool touchesBeganwithEvent(IList<TouchLocation> touches);

		// Token: 0x060002B6 RID: 694
		bool touchesEndedwithEvent(IList<TouchLocation> touches);

		// Token: 0x060002B7 RID: 695
		bool touchesMovedwithEvent(IList<TouchLocation> touches);

		// Token: 0x060002B8 RID: 696
		bool touchesCancelledwithEvent(IList<TouchLocation> touches);

		// Token: 0x060002B9 RID: 697
		bool backButtonPressed();

		// Token: 0x060002BA RID: 698
		bool menuButtonPressed();
	}
}
