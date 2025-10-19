using System;
using CutTheRope.iframework.core;

namespace CutTheRope.iframework.visual
{
	// Token: 0x0200003C RID: 60
	internal struct Particle
	{
		// Token: 0x04000156 RID: 342
		public Vector startPos;

		// Token: 0x04000157 RID: 343
		public Vector pos;

		// Token: 0x04000158 RID: 344
		public Vector dir;

		// Token: 0x04000159 RID: 345
		public float radialAccel;

		// Token: 0x0400015A RID: 346
		public float tangentialAccel;

		// Token: 0x0400015B RID: 347
		public RGBAColor color;

		// Token: 0x0400015C RID: 348
		public RGBAColor deltaColor;

		// Token: 0x0400015D RID: 349
		public float size;

		// Token: 0x0400015E RID: 350
		public float deltaSize;

		// Token: 0x0400015F RID: 351
		public float life;

		// Token: 0x04000160 RID: 352
		public float deltaAngle;

		// Token: 0x04000161 RID: 353
		public float angle;

		// Token: 0x04000162 RID: 354
		public float width;

		// Token: 0x04000163 RID: 355
		public float height;
	}
}
