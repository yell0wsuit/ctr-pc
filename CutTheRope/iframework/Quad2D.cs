using System;

namespace CutTheRope.iframework
{
	// Token: 0x02000022 RID: 34
	internal struct Quad2D
	{
		// Token: 0x0600012F RID: 303 RVA: 0x000068AC File Offset: 0x00004AAC
		public Quad2D(float x, float y, float w, float h)
		{
			this.tlX = x;
			this.tlY = y;
			this.trX = x + w;
			this.trY = y;
			this.blX = x;
			this.blY = y + h;
			this.brX = x + w;
			this.brY = y + h;
		}

		// Token: 0x06000130 RID: 304 RVA: 0x000068FC File Offset: 0x00004AFC
		public float[] toFloatArray()
		{
			return new float[] { this.tlX, this.tlY, this.trX, this.trY, this.blX, this.blY, this.brX, this.brY };
		}

		// Token: 0x06000131 RID: 305 RVA: 0x00006957 File Offset: 0x00004B57
		public static Quad2D MakeQuad2D(float x, float y, float w, float h)
		{
			return new Quad2D(x, y, w, h);
		}

		// Token: 0x040000D1 RID: 209
		public float tlX;

		// Token: 0x040000D2 RID: 210
		public float tlY;

		// Token: 0x040000D3 RID: 211
		public float trX;

		// Token: 0x040000D4 RID: 212
		public float trY;

		// Token: 0x040000D5 RID: 213
		public float blX;

		// Token: 0x040000D6 RID: 214
		public float blY;

		// Token: 0x040000D7 RID: 215
		public float brX;

		// Token: 0x040000D8 RID: 216
		public float brY;
	}
}
