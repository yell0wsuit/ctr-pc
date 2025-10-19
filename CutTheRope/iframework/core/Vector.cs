using System;
using Microsoft.Xna.Framework;

namespace CutTheRope.iframework.core
{
	// Token: 0x0200006A RID: 106
	public struct Vector
	{
		// Token: 0x06000413 RID: 1043 RVA: 0x00016198 File Offset: 0x00014398
		public Vector(Vector2 v)
		{
			this.x = v.X;
			this.y = v.Y;
		}

		// Token: 0x06000414 RID: 1044 RVA: 0x000161B2 File Offset: 0x000143B2
		public Vector(double xParam, double yParam)
		{
			this.x = (float)xParam;
			this.y = (float)yParam;
		}

		// Token: 0x06000415 RID: 1045 RVA: 0x000161C4 File Offset: 0x000143C4
		public Vector(float xParam, float yParam)
		{
			this.x = xParam;
			this.y = yParam;
		}

		// Token: 0x06000416 RID: 1046 RVA: 0x000161D4 File Offset: 0x000143D4
		public Vector2 toXNA()
		{
			return new Vector2(this.x, this.y);
		}

		// Token: 0x06000417 RID: 1047 RVA: 0x000161E8 File Offset: 0x000143E8
		public override string ToString()
		{
			return string.Concat(new string[]
			{
				"Vector(x=",
				this.x.ToString(),
				",y=",
				this.y.ToString(),
				")"
			});
		}

		// Token: 0x040002C4 RID: 708
		public float x;

		// Token: 0x040002C5 RID: 709
		public float y;
	}
}
