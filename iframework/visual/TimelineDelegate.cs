using System;

namespace CutTheRope.iframework.visual
{
	// Token: 0x0200004F RID: 79
	internal interface TimelineDelegate
	{
		// Token: 0x060002AC RID: 684
		void timelinereachedKeyFramewithIndex(Timeline t, KeyFrame k, int i);

		// Token: 0x060002AD RID: 685
		void timelineFinished(Timeline t);
	}
}
