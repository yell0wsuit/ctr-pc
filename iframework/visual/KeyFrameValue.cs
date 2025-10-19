using System;

namespace CutTheRope.iframework.visual
{
	// Token: 0x0200003A RID: 58
	internal class KeyFrameValue
	{
		// Token: 0x06000214 RID: 532 RVA: 0x0000A4DC File Offset: 0x000086DC
		public KeyFrameValue()
		{
			this.action = new ActionParams();
			this.scale = new ScaleParams();
			this.pos = new PosParams();
			this.rotation = new RotationParams();
			this.color = new ColorParams();
		}

		// Token: 0x0400014F RID: 335
		public PosParams pos;

		// Token: 0x04000150 RID: 336
		public ScaleParams scale;

		// Token: 0x04000151 RID: 337
		public RotationParams rotation;

		// Token: 0x04000152 RID: 338
		public ColorParams color;

		// Token: 0x04000153 RID: 339
		public ActionParams action;
	}
}
