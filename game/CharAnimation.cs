using System;
using CutTheRope.iframework;
using CutTheRope.iframework.core;
using CutTheRope.iframework.visual;

namespace CutTheRope.game
{
	// Token: 0x02000073 RID: 115
	internal class CharAnimation : Animation
	{
		// Token: 0x06000473 RID: 1139 RVA: 0x00019D6B File Offset: 0x00017F6B
		public static CharAnimation CharAnimation_create(Texture2D t)
		{
			return (CharAnimation)new CharAnimation().initWithTexture(t);
		}

		// Token: 0x06000474 RID: 1140 RVA: 0x00019D7D File Offset: 0x00017F7D
		public static CharAnimation CharAnimation_createWithResID(int r)
		{
			return CharAnimation.CharAnimation_create(Application.getTexture(r));
		}

		// Token: 0x06000475 RID: 1141 RVA: 0x00019D8C File Offset: 0x00017F8C
		public override bool handleAction(ActionData a)
		{
			if (a.actionName == "ACTION_PLAY_TIMELINE")
			{
				if (a.actionParam == 1)
				{
					this.parent.color = RGBAColor.transparentRGBA;
				}
				this.playTimeline(a.actionSubParam);
				return true;
			}
			return base.handleAction(a);
		}
	}
}
