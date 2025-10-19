using System;

namespace CutTheRope.iframework
{
	// Token: 0x02000023 RID: 35
	internal struct Quad3D
	{
		// Token: 0x06000132 RID: 306 RVA: 0x00006962 File Offset: 0x00004B62
		public static Quad3D MakeQuad3D(double x, double y, double z, double w, double h)
		{
			return Quad3D.MakeQuad3D((float)x, (float)y, (float)z, (float)w, (float)h);
		}

		// Token: 0x06000133 RID: 307 RVA: 0x00006974 File Offset: 0x00004B74
		public static Quad3D MakeQuad3D(float x, float y, float z, float w, float h)
		{
			return new Quad3D
			{
				blX = x,
				blY = y,
				blZ = z,
				brX = x + w,
				brY = y,
				brZ = z,
				tlX = x,
				tlY = y + h,
				tlZ = z,
				trX = x + w,
				trY = y + h,
				trZ = z
			};
		}

		// Token: 0x06000134 RID: 308 RVA: 0x000069F4 File Offset: 0x00004BF4
		public static Quad3D MakeQuad3DEx(float x1, float y1, float x2, float y2, float x3, float y3, float x4, float y4)
		{
			return new Quad3D
			{
				blX = x1,
				blY = y1,
				blZ = 0f,
				brX = x2,
				brY = y2,
				brZ = 0f,
				tlX = x3,
				tlY = y3,
				tlZ = 0f,
				trX = x4,
				trY = y4,
				trZ = 0f
			};
		}

		// Token: 0x06000135 RID: 309 RVA: 0x00006A80 File Offset: 0x00004C80
		public float[] toFloatArray()
		{
			if (this._array == null)
			{
				this._array = new float[]
				{
					this.blX, this.blY, this.blZ, this.brX, this.brY, this.brZ, this.tlX, this.tlY, this.tlZ, this.trX,
					this.trY, this.trZ
				};
			}
			return this._array;
		}

		// Token: 0x040000D9 RID: 217
		private float blX;

		// Token: 0x040000DA RID: 218
		private float blY;

		// Token: 0x040000DB RID: 219
		private float blZ;

		// Token: 0x040000DC RID: 220
		private float brX;

		// Token: 0x040000DD RID: 221
		private float brY;

		// Token: 0x040000DE RID: 222
		private float brZ;

		// Token: 0x040000DF RID: 223
		private float tlX;

		// Token: 0x040000E0 RID: 224
		private float tlY;

		// Token: 0x040000E1 RID: 225
		private float tlZ;

		// Token: 0x040000E2 RID: 226
		private float trX;

		// Token: 0x040000E3 RID: 227
		private float trY;

		// Token: 0x040000E4 RID: 228
		private float trZ;

		// Token: 0x040000E5 RID: 229
		private float[] _array;
	}
}
