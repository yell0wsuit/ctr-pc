using System;
using CutTheRope.iframework.core;
using CutTheRope.iframework.helpers;
using CutTheRope.iframework.visual;
using CutTheRope.ios;

namespace CutTheRope.game
{
	// Token: 0x0200006E RID: 110
	internal class Bouncer : CTRGameObject
	{
		// Token: 0x0600043F RID: 1087 RVA: 0x00016883 File Offset: 0x00014A83
		private static Bouncer Bouncer_create(Texture2D t)
		{
			return (Bouncer)new Bouncer().initWithTexture(t);
		}

		// Token: 0x06000440 RID: 1088 RVA: 0x00016895 File Offset: 0x00014A95
		private static Bouncer Bouncer_createWithResID(int r)
		{
			return Bouncer.Bouncer_create(Application.getTexture(r));
		}

		// Token: 0x06000441 RID: 1089 RVA: 0x000168A2 File Offset: 0x00014AA2
		private static Bouncer Bouncer_createWithResIDQuad(int r, int q)
		{
			Bouncer bouncer = Bouncer.Bouncer_create(Application.getTexture(r));
			bouncer.setDrawQuad(q);
			return bouncer;
		}

		// Token: 0x06000442 RID: 1090 RVA: 0x000168B8 File Offset: 0x00014AB8
		public virtual NSObject initWithPosXYWidthAndAngle(float px, float py, int w, double an)
		{
			int textureResID = -1;
			if (w != 1)
			{
				if (w == 2)
				{
					textureResID = 87;
				}
			}
			else
			{
				textureResID = 86;
			}
			if (this.initWithTexture(Application.getTexture(textureResID)) == null)
			{
				return null;
			}
			this.rotation = (float)an;
			this.x = px;
			this.y = py;
			this.updateRotation();
			int i = base.addAnimationDelayLoopFirstLast(0.04f, Timeline.LoopType.TIMELINE_NO_LOOP, 0, 4);
			this.getTimeline(i).addKeyFrame(KeyFrame.makeSingleAction(this, "ACTION_SET_DRAWQUAD", 0, 0, 0.04f));
			return this;
		}

		// Token: 0x06000443 RID: 1091 RVA: 0x00016934 File Offset: 0x00014B34
		public override void update(float delta)
		{
			base.update(delta);
			if (this.mover != null)
			{
				this.updateRotation();
			}
		}

		// Token: 0x06000444 RID: 1092 RVA: 0x0001694C File Offset: 0x00014B4C
		public virtual void updateRotation()
		{
			this.t1.x = this.x - (float)(this.width / 2);
			this.t2.x = this.x + (float)(this.width / 2);
			this.t1.y = (this.t2.y = (float)((double)this.y - 5.0));
			this.b1.x = this.t1.x;
			this.b2.x = this.t2.x;
			this.b1.y = (this.b2.y = (float)((double)this.y + 5.0));
			this.angle = MathHelper.DEGREES_TO_RADIANS(this.rotation);
			this.t1 = MathHelper.vectRotateAround(this.t1, (double)this.angle, this.x, this.y);
			this.t2 = MathHelper.vectRotateAround(this.t2, (double)this.angle, this.x, this.y);
			this.b1 = MathHelper.vectRotateAround(this.b1, (double)this.angle, this.x, this.y);
			this.b2 = MathHelper.vectRotateAround(this.b2, (double)this.angle, this.x, this.y);
		}

		// Token: 0x040002CF RID: 719
		private const float BOUNCER_HEIGHT = 10f;

		// Token: 0x040002D0 RID: 720
		public float angle;

		// Token: 0x040002D1 RID: 721
		public Vector t1;

		// Token: 0x040002D2 RID: 722
		public Vector t2;

		// Token: 0x040002D3 RID: 723
		public Vector b1;

		// Token: 0x040002D4 RID: 724
		public Vector b2;

		// Token: 0x040002D5 RID: 725
		public bool skip;
	}
}
