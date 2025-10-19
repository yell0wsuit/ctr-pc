using System;
using CutTheRope.iframework.core;
using CutTheRope.iframework.helpers;
using CutTheRope.windows;

namespace CutTheRope.iframework.visual
{
	// Token: 0x0200003B RID: 59
	internal class MultiParticles : Particles
	{
		// Token: 0x06000215 RID: 533 RVA: 0x0000A51C File Offset: 0x0000871C
		public virtual Particles initWithTotalParticlesandImageGrid(int numberOfParticles, Image image)
		{
			if (this.init() == null)
			{
				return null;
			}
			this.imageGrid = image;
			this.drawer = new ImageMultiDrawer().initWithImageandCapacity(this.imageGrid, numberOfParticles);
			this.width = (int)FrameworkTypes.SCREEN_WIDTH;
			this.height = (int)FrameworkTypes.SCREEN_HEIGHT;
			this.totalParticles = numberOfParticles;
			this.particles = new Particle[this.totalParticles];
			this.colors = new RGBAColor[4 * this.totalParticles];
			if (this.particles == null || this.colors == null)
			{
				this.particles = null;
				this.colors = null;
				return null;
			}
			this.active = false;
			this.blendAdditive = false;
			OpenGL.glGenBuffers(1, ref this.colorsID);
			return this;
		}

		// Token: 0x06000216 RID: 534 RVA: 0x0000A5D0 File Offset: 0x000087D0
		public override void initParticle(ref Particle particle)
		{
			Image image = this.imageGrid;
			int num = MathHelper.RND(image.texture.quadsCount - 1);
			Quad2D qt = image.texture.quads[num];
			Quad3D qv = Quad3D.MakeQuad3D(0f, 0f, 0f, 0f, 0f);
			Rectangle rectangle = image.texture.quadRects[num];
			this.drawer.setTextureQuadatVertexQuadatIndex(qt, qv, this.particleCount);
			base.initParticle(ref particle);
			particle.width = rectangle.w * particle.size;
			particle.height = rectangle.h * particle.size;
		}

		// Token: 0x06000217 RID: 535 RVA: 0x0000A678 File Offset: 0x00008878
		public override void updateParticle(ref Particle p, float delta)
		{
			if (p.life > 0f)
			{
				Vector vector = MathHelper.vectZero;
				if (p.pos.x != 0f || p.pos.y != 0f)
				{
					vector = MathHelper.vectNormalize(p.pos);
				}
				Vector v = vector;
				vector = MathHelper.vectMult(vector, p.radialAccel);
				float num = v.x;
				v.x = 0f - v.y;
				v.y = num;
				v = MathHelper.vectMult(v, p.tangentialAccel);
				Vector v2 = MathHelper.vectAdd(MathHelper.vectAdd(vector, v), this.gravity);
				v2 = MathHelper.vectMult(v2, delta);
				p.dir = MathHelper.vectAdd(p.dir, v2);
				v2 = MathHelper.vectMult(p.dir, delta);
				p.pos = MathHelper.vectAdd(p.pos, v2);
				p.color.r = p.color.r + p.deltaColor.r * delta;
				p.color.g = p.color.g + p.deltaColor.g * delta;
				p.color.b = p.color.b + p.deltaColor.b * delta;
				p.color.a = p.color.a + p.deltaColor.a * delta;
				p.life -= delta;
				this.drawer.vertices[this.particleIdx] = Quad3D.MakeQuad3D((double)(p.pos.x - p.width / 2f), (double)(p.pos.y - p.height / 2f), 0.0, (double)p.width, (double)p.height);
				for (int i = 0; i < 4; i++)
				{
					this.colors[this.particleIdx * 4 + i] = p.color;
				}
				this.particleIdx++;
				return;
			}
			if (this.particleIdx != this.particleCount - 1)
			{
				this.particles[this.particleIdx] = this.particles[this.particleCount - 1];
				this.drawer.vertices[this.particleIdx] = this.drawer.vertices[this.particleCount - 1];
				this.drawer.texCoordinates[this.particleIdx] = this.drawer.texCoordinates[this.particleCount - 1];
			}
			this.particleCount--;
		}

		// Token: 0x06000218 RID: 536 RVA: 0x0000A910 File Offset: 0x00008B10
		public override void update(float delta)
		{
			base.update(delta);
			if (this.active && this.emissionRate != 0f)
			{
				float num = 1f / this.emissionRate;
				this.emitCounter += delta;
				while (this.particleCount < this.totalParticles && this.emitCounter > num)
				{
					this.addParticle();
					this.emitCounter -= num;
				}
				this.elapsed += delta;
				if (this.duration != -1f && this.duration < this.elapsed)
				{
					this.stopSystem();
				}
			}
			this.particleIdx = 0;
			while (this.particleIdx < this.particleCount)
			{
				this.updateParticle(ref this.particles[this.particleIdx], delta);
			}
			OpenGL.glBindBuffer(2, this.colorsID);
			OpenGL.glBufferData(2, this.colors, 3);
			OpenGL.glBindBuffer(2, 0U);
		}

		// Token: 0x06000219 RID: 537 RVA: 0x0000AA04 File Offset: 0x00008C04
		public override void draw()
		{
			this.preDraw();
			if (this.blendAdditive)
			{
				OpenGL.glBlendFunc(BlendingFactor.GL_SRC_ALPHA, BlendingFactor.GL_ONE);
			}
			else
			{
				OpenGL.glBlendFunc(BlendingFactor.GL_ONE, BlendingFactor.GL_ONE_MINUS_SRC_ALPHA);
			}
			OpenGL.glEnable(0);
			OpenGL.glBindTexture(this.drawer.image.texture.name());
			OpenGL.glVertexPointer(3, 5, 0, FrameworkTypes.toFloatArray(this.drawer.vertices));
			OpenGL.glTexCoordPointer(2, 5, 0, FrameworkTypes.toFloatArray(this.drawer.texCoordinates));
			OpenGL.glEnableClientState(13);
			OpenGL.glBindBuffer(2, this.colorsID);
			OpenGL.glColorPointer(4, 5, 0, this.colors);
			OpenGL.glDrawElements(7, this.particleIdx * 6, this.drawer.indices);
			OpenGL.glBlendFunc(BlendingFactor.GL_ONE, BlendingFactor.GL_ONE_MINUS_SRC_ALPHA);
			OpenGL.glBindBuffer(2, 0U);
			OpenGL.glDisableClientState(13);
			this.postDraw();
		}

		// Token: 0x0600021A RID: 538 RVA: 0x0000AAE0 File Offset: 0x00008CE0
		public override void dealloc()
		{
			this.drawer = null;
			this.imageGrid = null;
			base.dealloc();
		}

		// Token: 0x04000154 RID: 340
		public ImageMultiDrawer drawer;

		// Token: 0x04000155 RID: 341
		public Image imageGrid;
	}
}
