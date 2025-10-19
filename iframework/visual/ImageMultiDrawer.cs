using System;
using CutTheRope.ios;
using CutTheRope.windows;
using Microsoft.Xna.Framework.Graphics;

namespace CutTheRope.iframework.visual
{
	// Token: 0x02000038 RID: 56
	internal class ImageMultiDrawer : BaseElement
	{
		// Token: 0x060001FA RID: 506 RVA: 0x00009EC0 File Offset: 0x000080C0
		public virtual ImageMultiDrawer initWithImageandCapacity(Image i, int n)
		{
			if (this.init() == null)
			{
				return null;
			}
			this.image = (Image)NSObject.NSRET(i);
			this.numberOfQuadsToDraw = -1;
			this.totalQuads = n;
			this.texCoordinates = new Quad2D[this.totalQuads];
			this.vertices = new Quad3D[this.totalQuads];
			this.indices = new short[this.totalQuads * 6];
			this.initIndices();
			return this;
		}

		// Token: 0x060001FB RID: 507 RVA: 0x00009F32 File Offset: 0x00008132
		private void freeWithCheck()
		{
			this.texCoordinates = null;
			this.vertices = null;
			this.indices = null;
		}

		// Token: 0x060001FC RID: 508 RVA: 0x00009F49 File Offset: 0x00008149
		public override void dealloc()
		{
			this.freeWithCheck();
			this.image = null;
			base.dealloc();
		}

		// Token: 0x060001FD RID: 509 RVA: 0x00009F60 File Offset: 0x00008160
		private void initIndices()
		{
			for (int i = 0; i < this.totalQuads; i++)
			{
				this.indices[i * 6] = (short)(i * 4);
				this.indices[i * 6 + 1] = (short)(i * 4 + 1);
				this.indices[i * 6 + 2] = (short)(i * 4 + 2);
				this.indices[i * 6 + 3] = (short)(i * 4 + 3);
				this.indices[i * 6 + 4] = (short)(i * 4 + 2);
				this.indices[i * 6 + 5] = (short)(i * 4 + 1);
			}
		}

		// Token: 0x060001FE RID: 510 RVA: 0x00009FE6 File Offset: 0x000081E6
		public void setTextureQuadatVertexQuadatIndex(Quad2D qt, Quad3D qv, int n)
		{
			if (n >= this.totalQuads)
			{
				this.resizeCapacity(n + 1);
			}
			this.texCoordinates[n] = qt;
			this.vertices[n] = qv;
		}

		// Token: 0x060001FF RID: 511 RVA: 0x0000A014 File Offset: 0x00008214
		public void mapTextureQuadAtXYatIndex(int q, float dx, float dy, int n)
		{
			if (n >= this.totalQuads)
			{
				this.resizeCapacity(n + 1);
			}
			this.texCoordinates[n] = this.image.texture.quads[q];
			this.vertices[n] = Quad3D.MakeQuad3D((double)(dx + this.image.texture.quadOffsets[q].x), (double)(dy + this.image.texture.quadOffsets[q].y), 0.0, (double)this.image.texture.quadRects[q].w, (double)this.image.texture.quadRects[q].h);
		}

		// Token: 0x06000200 RID: 512 RVA: 0x0000A0E8 File Offset: 0x000082E8
		private void drawNumberOfQuads(int n)
		{
			OpenGL.glEnable(0);
			OpenGL.glBindTexture(this.image.texture.name());
			OpenGL.glVertexPointer(3, 5, 0, FrameworkTypes.toFloatArray(this.vertices));
			OpenGL.glTexCoordPointer(2, 5, 0, FrameworkTypes.toFloatArray(this.texCoordinates));
			OpenGL.glDrawElements(7, n * 6, this.indices);
		}

		// Token: 0x06000201 RID: 513 RVA: 0x0000A145 File Offset: 0x00008345
		private void drawNumberOfQuadsStartingFrom(int n, int s)
		{
			throw new NotImplementedException();
		}

		// Token: 0x06000202 RID: 514 RVA: 0x0000A14C File Offset: 0x0000834C
		public void optimize(VertexPositionNormalTexture[] v)
		{
			if (v != null && this.verticesOptimized == null)
			{
				this.verticesOptimized = v;
			}
		}

		// Token: 0x06000203 RID: 515 RVA: 0x0000A160 File Offset: 0x00008360
		public void drawAllQuads()
		{
			if (this.verticesOptimized == null)
			{
				this.drawNumberOfQuads(this.totalQuads);
				return;
			}
			OpenGL.glEnable(0);
			OpenGL.glBindTexture(this.image.texture.name());
			OpenGL.Optimized_DrawTriangleList(this.verticesOptimized, this.indices);
		}

		// Token: 0x06000204 RID: 516 RVA: 0x0000A1B0 File Offset: 0x000083B0
		public override void draw()
		{
			this.preDraw();
			OpenGL.glTranslatef(this.drawX, this.drawY, 0f);
			if (this.numberOfQuadsToDraw == -1)
			{
				this.drawAllQuads();
			}
			else if (this.numberOfQuadsToDraw > 0)
			{
				this.drawNumberOfQuads(this.numberOfQuadsToDraw);
			}
			OpenGL.glTranslatef(0f - this.drawX, 0f - this.drawY, 0f);
			this.postDraw();
		}

		// Token: 0x06000205 RID: 517 RVA: 0x0000A228 File Offset: 0x00008428
		private void resizeCapacity(int n)
		{
			if (n != this.totalQuads)
			{
				this.totalQuads = n;
				this.texCoordinates = new Quad2D[this.totalQuads];
				this.vertices = new Quad3D[this.totalQuads];
				this.indices = new short[this.totalQuads * 6];
				if (this.texCoordinates == null || this.vertices == null || this.indices == null)
				{
					this.freeWithCheck();
				}
				this.initIndices();
			}
		}

		// Token: 0x04000144 RID: 324
		public Image image;

		// Token: 0x04000145 RID: 325
		public int totalQuads;

		// Token: 0x04000146 RID: 326
		public Quad2D[] texCoordinates;

		// Token: 0x04000147 RID: 327
		public Quad3D[] vertices;

		// Token: 0x04000148 RID: 328
		public short[] indices;

		// Token: 0x04000149 RID: 329
		public int numberOfQuadsToDraw;

		// Token: 0x0400014A RID: 330
		private VertexPositionNormalTexture[] verticesOptimized;
	}
}
