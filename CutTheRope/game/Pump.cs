using System;
using CutTheRope.iframework.core;
using CutTheRope.iframework.helpers;
using CutTheRope.iframework.visual;

namespace CutTheRope.game
{
	// Token: 0x0200008D RID: 141
	internal class Pump : GameObject
	{
		// Token: 0x060005B3 RID: 1459 RVA: 0x0002EC24 File Offset: 0x0002CE24
		public static Pump Pump_create(Texture2D t)
		{
			return (Pump)new Pump().initWithTexture(t);
		}

		// Token: 0x060005B4 RID: 1460 RVA: 0x0002EC36 File Offset: 0x0002CE36
		public static Pump Pump_createWithResID(int r)
		{
			return Pump.Pump_create(Application.getTexture(r));
		}

		// Token: 0x060005B5 RID: 1461 RVA: 0x0002EC43 File Offset: 0x0002CE43
		public static Pump Pump_createWithResIDQuad(int r, int q)
		{
			Pump pump = Pump.Pump_create(Application.getTexture(r));
			pump.setDrawQuad(q);
			return pump;
		}

		// Token: 0x060005B6 RID: 1462 RVA: 0x0002EC58 File Offset: 0x0002CE58
		public virtual void updateRotation()
		{
			this.t1.x = this.x - this.bb.w / 2f;
			this.t2.x = this.x + this.bb.w / 2f;
			this.t1.y = (this.t2.y = this.y);
			this.angle = (double)MathHelper.DEGREES_TO_RADIANS(this.rotation);
			this.t1 = MathHelper.vectRotateAround(this.t1, this.angle, this.x, this.y);
			this.t2 = MathHelper.vectRotateAround(this.t2, this.angle, this.x, this.y);
		}

		// Token: 0x040004BC RID: 1212
		public double angle;

		// Token: 0x040004BD RID: 1213
		public Vector t1;

		// Token: 0x040004BE RID: 1214
		public Vector t2;

		// Token: 0x040004BF RID: 1215
		public float pumpTouchTimer;

		// Token: 0x040004C0 RID: 1216
		public int pumpTouch;

		// Token: 0x040004C1 RID: 1217
		public float initial_rotation;

		// Token: 0x040004C2 RID: 1218
		public float initial_x;

		// Token: 0x040004C3 RID: 1219
		public float initial_y;

		// Token: 0x040004C4 RID: 1220
		public RotatedCircle initial_rotatedCircle;
	}
}
