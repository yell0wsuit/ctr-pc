using System;

namespace CutTheRope.iframework
{
	// Token: 0x02000024 RID: 36
	internal struct Rectangle
	{
		// Token: 0x06000136 RID: 310 RVA: 0x00006B17 File Offset: 0x00004D17
		public Rectangle(double xParam, double yParam, double width, double height)
		{
			this.x = (float)xParam;
			this.y = (float)yParam;
			this.w = (float)width;
			this.h = (float)height;
		}

		// Token: 0x06000137 RID: 311 RVA: 0x00006B3A File Offset: 0x00004D3A
		public Rectangle(float xParam, float yParam, float width, float height)
		{
			this.x = xParam;
			this.y = yParam;
			this.w = width;
			this.h = height;
		}

		// Token: 0x06000138 RID: 312 RVA: 0x00006B59 File Offset: 0x00004D59
		public bool isValid()
		{
			return this.x != 0f || this.y != 0f || this.w != 0f || this.h != 0f;
		}

		// Token: 0x040000E6 RID: 230
		public float x;

		// Token: 0x040000E7 RID: 231
		public float y;

		// Token: 0x040000E8 RID: 232
		public float w;

		// Token: 0x040000E9 RID: 233
		public float h;
	}
}
