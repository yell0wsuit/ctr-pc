using System;
using System.Linq;
using CutTheRope.iframework;
using CutTheRope.iframework.core;
using CutTheRope.iframework.helpers;
using CutTheRope.iframework.visual;
using CutTheRope.ios;
using CutTheRope.windows;

namespace CutTheRope.game
{
	// Token: 0x0200008B RID: 139
	internal class PollenDrawer : BaseElement
	{
		// Token: 0x060005A3 RID: 1443 RVA: 0x0002E248 File Offset: 0x0002C448
		public override NSObject init()
		{
			if (base.init() != null)
			{
				Image image = Image.Image_createWithResID(99);
				this.qw = (float)image.width * 1.5f;
				this.qh = (float)image.height * 1.5f;
				this.totalCapacity = 200;
				this.drawer = new ImageMultiDrawer().initWithImageandCapacity(image, this.totalCapacity);
				this.pollens = new Pollen[this.totalCapacity];
				this.colors = new RGBAColor[4 * this.totalCapacity];
				OpenGL.glGenBuffers(1, ref this.colorsID);
			}
			return this;
		}

		// Token: 0x060005A4 RID: 1444 RVA: 0x0002E2E4 File Offset: 0x0002C4E4
		public override void dealloc()
		{
			if (this.pollens != null)
			{
				this.pollens = null;
			}
			if (this.colors != null)
			{
				this.colors = null;
				OpenGL.glDeleteBuffers(1, ref this.colorsID);
			}
			if (this.vertices != null)
			{
				this.vertices = null;
				OpenGL.glDeleteBuffers(1, ref this.verticesID);
			}
			this.drawer = null;
			base.dealloc();
		}

		// Token: 0x060005A5 RID: 1445 RVA: 0x0002E344 File Offset: 0x0002C544
		public virtual void addPollenAtparentIndex(Vector v, int pi)
		{
			float num = 1f;
			float num2 = 1f;
			float[] array = new float[] { 0.3f, 0.3f, 0.5f, 0.5f, 0.6f };
			int num3 = array.Count<float>();
			float num4 = array[MathHelper.RND_RANGE(0, num3 - 1)];
			float num5 = num4;
			if (MathHelper.RND(1) == 1)
			{
				num4 *= 1f + (float)MathHelper.RND(1) / 10f;
			}
			else
			{
				num5 *= 1f + (float)MathHelper.RND(1) / 10f;
			}
			num *= num4;
			num2 *= num5;
			int num6 = (int)this.qw;
			int num7 = (int)this.qh;
			num6 *= (int)num;
			num7 *= (int)num2;
			Pollen pollen = default(Pollen);
			pollen.parentIndex = pi;
			pollen.x = v.x;
			pollen.y = v.y;
			float num8 = 1f;
			float num9 = Math.Min(num8 - num, num8 - num2);
			float rND_0_ = MathHelper.RND_0_1;
			pollen.startScaleX = num9 + num;
			pollen.startScaleY = num9 + num2;
			pollen.scaleX = pollen.startScaleX * rND_0_;
			pollen.scaleY = pollen.startScaleY * rND_0_;
			pollen.endScaleX = num;
			pollen.endScaleY = num2;
			pollen.endAlpha = 0.3f;
			pollen.startAlpha = 1f;
			pollen.alpha = 0.7f * rND_0_ + 0.3f;
			Quad2D qt = this.drawer.image.texture.quads[0];
			Quad3D qv = Quad3D.MakeQuad3D((double)(v.x - (float)(num6 / 2)), (double)(v.y - (float)(num7 / 2)), 0.0, (double)num6, (double)num7);
			this.drawer.setTextureQuadatVertexQuadatIndex(qt, qv, this.pollenCount);
			if (this.pollenCount >= this.totalCapacity)
			{
				this.totalCapacity = this.pollenCount;
				this.pollens = new Pollen[this.totalCapacity + 1];
				this.colors = new RGBAColor[4 * (this.totalCapacity + 1)];
			}
			for (int i = 0; i < 4; i++)
			{
				this.colors[this.pollenCount * 4 + i] = RGBAColor.whiteRGBA;
			}
			this.pollens[this.pollenCount] = pollen;
			this.pollenCount++;
		}

		// Token: 0x060005A6 RID: 1446 RVA: 0x0002E594 File Offset: 0x0002C794
		public virtual void fillWithPolenFromPathIndexToPathIndexGrab(int p1, int p2, Grab g)
		{
			int num = 44;
			Vector vector = g.mover.path[p1];
			Vector vector2 = MathHelper.vectSub(g.mover.path[p2], vector);
			int num2 = (int)(MathHelper.vectLength(vector2) / (float)num);
			Vector v3 = MathHelper.vectNormalize(vector2);
			for (int i = 0; i <= num2; i++)
			{
				Vector v4 = MathHelper.vectAdd(vector, MathHelper.vectMult(v3, (float)(i * num)));
				v4.x += (float)MathHelper.RND_RANGE((int)FrameworkTypes.RTPD(-2.0), (int)FrameworkTypes.RTPD(2.0));
				v4.y += (float)MathHelper.RND_RANGE((int)FrameworkTypes.RTPD(-2.0), (int)FrameworkTypes.RTPD(2.0));
				this.addPollenAtparentIndex(v4, p1);
			}
		}

		// Token: 0x060005A7 RID: 1447 RVA: 0x0002E670 File Offset: 0x0002C870
		public override void update(float delta)
		{
			base.update(delta);
			this.drawer.update(delta);
			for (int i = 0; i < this.pollenCount; i++)
			{
				if (Mover.moveVariableToTarget(ref this.pollens[i].scaleX, this.pollens[i].endScaleX, 1f, delta))
				{
					float startScaleX = this.pollens[i].startScaleX;
					this.pollens[i].startScaleX = this.pollens[i].endScaleX;
					this.pollens[i].endScaleX = startScaleX;
				}
				if (Mover.moveVariableToTarget(ref this.pollens[i].scaleY, this.pollens[i].endScaleY, 1f, delta))
				{
					float startScaleY = this.pollens[i].startScaleY;
					this.pollens[i].startScaleY = this.pollens[i].endScaleY;
					this.pollens[i].endScaleY = startScaleY;
				}
				float num = this.qw * this.pollens[i].scaleX;
				float num2 = this.qh * this.pollens[i].scaleY;
				this.drawer.vertices[i] = Quad3D.MakeQuad3D((double)(this.pollens[i].x - num / 2f), (double)(this.pollens[i].y - num2 / 2f), 0.0, (double)num, (double)num2);
				if (Mover.moveVariableToTarget(ref this.pollens[i].alpha, this.pollens[i].endAlpha, 1f, delta))
				{
					float startAlpha = this.pollens[i].startAlpha;
					this.pollens[i].startAlpha = this.pollens[i].endAlpha;
					this.pollens[i].endAlpha = startAlpha;
				}
				float alpha = this.pollens[i].alpha;
				for (int j = 0; j < 4; j++)
				{
					this.colors[i * 4 + j] = RGBAColor.MakeRGBA(alpha, alpha, alpha, alpha);
				}
			}
			OpenGL.glBindBuffer(2, this.colorsID);
			OpenGL.glBufferData(2, this.colors, 3);
			OpenGL.glBindBuffer(2, 0U);
		}

		// Token: 0x060005A8 RID: 1448 RVA: 0x0002E8F4 File Offset: 0x0002CAF4
		public override void draw()
		{
			if (this.pollenCount >= 2)
			{
				this.preDraw();
				OpenGL.glBlendFunc(BlendingFactor.GL_SRC_ALPHA, BlendingFactor.GL_ONE);
				OpenGL.glEnable(0);
				OpenGL.glBindTexture(this.drawer.image.texture.name());
				OpenGL.glVertexPointer(3, 5, 0, FrameworkTypes.toFloatArray(this.drawer.vertices));
				OpenGL.glTexCoordPointer(2, 5, 0, FrameworkTypes.toFloatArray(this.drawer.texCoordinates));
				OpenGL.glEnableClientState(13);
				OpenGL.glBindBuffer(2, this.colorsID);
				OpenGL.glBufferData(2, this.colors, 3);
				OpenGL.glColorPointer(4, 5, 0, this.colors);
				OpenGL.glDrawElements(7, (this.pollenCount - 1) * 6, this.drawer.indices);
				OpenGL.glBlendFunc(BlendingFactor.GL_ONE, BlendingFactor.GL_ONE_MINUS_SRC_ALPHA);
				OpenGL.glBindBuffer(2, 0U);
				OpenGL.glDisableClientState(13);
				this.postDraw();
			}
		}

		// Token: 0x040004B2 RID: 1202
		private ImageMultiDrawer drawer;

		// Token: 0x040004B3 RID: 1203
		private int pollenCount;

		// Token: 0x040004B4 RID: 1204
		private int totalCapacity;

		// Token: 0x040004B5 RID: 1205
		private Pollen[] pollens;

		// Token: 0x040004B6 RID: 1206
		private float qw;

		// Token: 0x040004B7 RID: 1207
		private float qh;

		// Token: 0x040004B8 RID: 1208
		private RGBAColor[] colors;

		// Token: 0x040004B9 RID: 1209
		private uint colorsID;

		// Token: 0x040004BA RID: 1210
		private PointSprite[] vertices;

		// Token: 0x040004BB RID: 1211
		private uint verticesID;
	}
}
