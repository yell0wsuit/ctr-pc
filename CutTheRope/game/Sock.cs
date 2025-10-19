using System;
using System.Collections.Generic;
using CutTheRope.iframework;
using CutTheRope.iframework.core;
using CutTheRope.iframework.helpers;
using CutTheRope.iframework.visual;

namespace CutTheRope.game
{
	// Token: 0x02000094 RID: 148
	internal class Sock : CTRGameObject
	{
		// Token: 0x060005E5 RID: 1509 RVA: 0x000318E7 File Offset: 0x0002FAE7
		public static Sock Sock_create(Texture2D t)
		{
			return (Sock)new Sock().initWithTexture(t);
		}

		// Token: 0x060005E6 RID: 1510 RVA: 0x000318F9 File Offset: 0x0002FAF9
		public static Sock Sock_createWithResID(int r)
		{
			return Sock.Sock_create(Application.getTexture(r));
		}

		// Token: 0x060005E7 RID: 1511 RVA: 0x00031906 File Offset: 0x0002FB06
		public static Sock Sock_createWithResIDQuad(int r, int q)
		{
			Sock sock = Sock.Sock_create(Application.getTexture(r));
			sock.setDrawQuad(q);
			return sock;
		}

		// Token: 0x060005E8 RID: 1512 RVA: 0x0003191C File Offset: 0x0002FB1C
		public virtual void createAnimations()
		{
			this.light = Animation.Animation_createWithResID(85);
			this.light.anchor = 34;
			this.light.parentAnchor = 10;
			this.light.y = 270f;
			this.light.x = FrameworkTypes.RTD(0.0);
			this.light.addAnimationWithIDDelayLoopCountSequence(0, 0.05f, Timeline.LoopType.TIMELINE_NO_LOOP, 4, 2, new List<int> { 3, 4, 4 });
			this.light.doRestoreCutTransparency();
			this.light.visible = false;
			this.addChild(this.light);
		}

		// Token: 0x060005E9 RID: 1513 RVA: 0x000319CC File Offset: 0x0002FBCC
		public virtual void updateRotation()
		{
			float num = 140f;
			this.t1.x = this.x - num / 2f - 20f;
			this.t2.x = this.x + num / 2f - 20f;
			this.t1.y = (this.t2.y = this.y);
			this.b1.x = this.t1.x;
			this.b2.x = this.t2.x;
			this.b1.y = (this.b2.y = this.y + 15f);
			this.angle = (double)MathHelper.DEGREES_TO_RADIANS(this.rotation);
			this.t1 = MathHelper.vectRotateAround(this.t1, this.angle, this.x, this.y);
			this.t2 = MathHelper.vectRotateAround(this.t2, this.angle, this.x, this.y);
			this.b1 = MathHelper.vectRotateAround(this.b1, this.angle, this.x, this.y);
			this.b2 = MathHelper.vectRotateAround(this.b2, this.angle, this.x, this.y);
		}

		// Token: 0x060005EA RID: 1514 RVA: 0x00031B2C File Offset: 0x0002FD2C
		public override void draw()
		{
			Timeline timeline = this.light.getCurrentTimeline();
			if (timeline != null && timeline.state == Timeline.TimelineState.TIMELINE_STOPPED)
			{
				this.light.visible = false;
			}
			base.draw();
		}

		// Token: 0x060005EB RID: 1515 RVA: 0x00031B62 File Offset: 0x0002FD62
		public override void drawBB()
		{
		}

		// Token: 0x060005EC RID: 1516 RVA: 0x00031B64 File Offset: 0x0002FD64
		public override void update(float delta)
		{
			base.update(delta);
			if (this.mover != null)
			{
				this.updateRotation();
			}
		}

		// Token: 0x04000817 RID: 2071
		public const float SOCK_IDLE_TIMOUT = 0.8f;

		// Token: 0x04000818 RID: 2072
		public static int SOCK_RECEIVING = 0;

		// Token: 0x04000819 RID: 2073
		public static int SOCK_THROWING = 1;

		// Token: 0x0400081A RID: 2074
		public static int SOCK_IDLE = 2;

		// Token: 0x0400081B RID: 2075
		public int group;

		// Token: 0x0400081C RID: 2076
		public double angle;

		// Token: 0x0400081D RID: 2077
		public Vector t1;

		// Token: 0x0400081E RID: 2078
		public Vector t2;

		// Token: 0x0400081F RID: 2079
		public Vector b1;

		// Token: 0x04000820 RID: 2080
		public Vector b2;

		// Token: 0x04000821 RID: 2081
		public float idleTimeout;

		// Token: 0x04000822 RID: 2082
		public Animation light;
	}
}
