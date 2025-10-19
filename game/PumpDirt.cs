using System;
using CutTheRope.iframework;
using CutTheRope.iframework.core;
using CutTheRope.iframework.helpers;
using CutTheRope.iframework.visual;
using CutTheRope.windows;

namespace CutTheRope.game
{
	// Token: 0x0200008E RID: 142
	internal class PumpDirt : MultiParticles
	{
		// Token: 0x060005B8 RID: 1464 RVA: 0x0002ED2C File Offset: 0x0002CF2C
		public virtual PumpDirt initWithTotalParticlesAngleandImageGrid(int p, float a, Image grid)
		{
			if (base.initWithTotalParticlesandImageGrid(p, grid) == null)
			{
				return null;
			}
			this.duration = 0.6f;
			this.gravity.x = 0f;
			this.gravity.y = 0f;
			this.angle = a;
			this.angleVar = 10f;
			this.speed = 1000f;
			this.speedVar = 100f;
			this.radialAccel = 0f;
			this.radialAccelVar = 0f;
			this.tangentialAccel = 0f;
			this.tangentialAccelVar = 0f;
			this.posVar.x = 0f;
			this.posVar.y = 0f;
			this.life = 0.6f;
			this.lifeVar = 0f;
			this.size = 2f;
			this.sizeVar = 0f;
			this.emissionRate = 100f;
			this.startColor.r = 1f;
			this.startColor.g = 1f;
			this.startColor.b = 1f;
			this.startColor.a = 0.6f;
			this.startColorVar.r = 0f;
			this.startColorVar.g = 0f;
			this.startColorVar.b = 0f;
			this.startColorVar.a = 0f;
			this.endColor.r = 1f;
			this.endColor.g = 1f;
			this.endColor.b = 1f;
			this.endColor.a = 0f;
			this.endColorVar.r = 0f;
			this.endColorVar.g = 0f;
			this.endColorVar.b = 0f;
			this.endColorVar.a = 0f;
			this.blendAdditive = true;
			return this;
		}

		// Token: 0x060005B9 RID: 1465 RVA: 0x0002EF24 File Offset: 0x0002D124
		public override void initParticle(ref Particle particle)
		{
			base.initParticle(ref particle);
			int num = MathHelper.RND_RANGE(6, 8);
			Quad2D qt = this.imageGrid.texture.quads[num];
			Quad3D qv = Quad3D.MakeQuad3D(0f, 0f, 0f, 0f, 0f);
			this.drawer.setTextureQuadatVertexQuadatIndex(qt, qv, this.particleCount);
			Rectangle rectangle = this.imageGrid.texture.quadRects[num];
			particle.width = rectangle.w * particle.size;
			particle.height = rectangle.h * particle.size;
		}

		// Token: 0x060005BA RID: 1466 RVA: 0x0002EFC8 File Offset: 0x0002D1C8
		public override void updateParticle(ref Particle p, float delta)
		{
			if (p.life > 0f)
			{
				p.dir = MathHelper.vectMult(p.dir, 0.9);
				Vector v = MathHelper.vectMult(p.dir, delta);
				v = MathHelper.vectAdd(v, this.gravity);
				p.pos = MathHelper.vectAdd(p.pos, v);
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

		// Token: 0x060005BB RID: 1467 RVA: 0x0002F1E4 File Offset: 0x0002D3E4
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
	}
}
