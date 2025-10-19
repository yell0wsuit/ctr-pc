using System;

namespace CutTheRope.iframework
{
	// Token: 0x02000021 RID: 33
	internal struct PointSprite
	{
		// Token: 0x0600012E RID: 302 RVA: 0x00006893 File Offset: 0x00004A93
		public PointSprite(float xx, float yy, float s)
		{
			this.x = xx;
			this.y = yy;
			this.size = s;
		}

		// Token: 0x040000CE RID: 206
		public float x;

		// Token: 0x040000CF RID: 207
		public float y;

		// Token: 0x040000D0 RID: 208
		public float size;
	}
}
