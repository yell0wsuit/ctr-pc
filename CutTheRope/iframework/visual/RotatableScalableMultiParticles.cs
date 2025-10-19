using System;
using CutTheRope.iframework.core;
using CutTheRope.iframework.helpers;
using CutTheRope.windows;

namespace CutTheRope.iframework.visual
{
	// Token: 0x02000041 RID: 65
	internal class RotatableScalableMultiParticles : ScalableMultiParticles
	{
		// Token: 0x0600022F RID: 559 RVA: 0x0000B3FC File Offset: 0x000095FC
		public override void initParticle(ref Particle particle)
		{
			base.initParticle(ref particle);
			particle.angle = this.initialAngle;
			particle.deltaAngle = MathHelper.DEGREES_TO_RADIANS(this.rotateSpeed + this.rotateSpeedVar * MathHelper.RND_MINUS1_1);
			particle.deltaSize = (this.endSize - this.size) / particle.life;
		}

		// Token: 0x06000230 RID: 560 RVA: 0x0000B454 File Offset: 0x00009654
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
				p.size += p.deltaSize * delta;
				p.life -= delta;
				float num2 = p.width * p.size;
				float num3 = p.height * p.size;
				float num4 = p.pos.x - num2 / 2f;
				float num5 = p.pos.y - num3 / 2f;
				float num6 = p.pos.x + num2 / 2f;
				float num7 = p.pos.y - num3 / 2f;
				float num8 = p.pos.x - num2 / 2f;
				float num9 = p.pos.y + num3 / 2f;
				float num11 = p.pos.x + num2 / 2f;
				float num10 = p.pos.y + num3 / 2f;
				float cx = p.pos.x;
				float cy = p.pos.y;
				Vector v3 = MathHelper.vect(num4, num5);
				Vector v4 = MathHelper.vect(num6, num7);
				Vector v5 = MathHelper.vect(num8, num9);
				Vector v6 = MathHelper.vect(num11, num10);
				p.angle += p.deltaAngle * delta;
				float cosA = MathHelper.cosf(p.angle);
				float sinA = MathHelper.sinf(p.angle);
				v3 = Particles.rotatePreCalc(v3, cosA, sinA, cx, cy);
				v4 = Particles.rotatePreCalc(v4, cosA, sinA, cx, cy);
				v5 = Particles.rotatePreCalc(v5, cosA, sinA, cx, cy);
				v6 = Particles.rotatePreCalc(v6, cosA, sinA, cx, cy);
				this.drawer.vertices[this.particleIdx] = Quad3D.MakeQuad3DEx(v3.x, v3.y, v4.x, v4.y, v5.x, v5.y, v6.x, v6.y);
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

		// Token: 0x06000231 RID: 561 RVA: 0x0000B870 File Offset: 0x00009A70
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

		// Token: 0x0400018B RID: 395
		public float initialAngle;

		// Token: 0x0400018C RID: 396
		public float rotateSpeed;

		// Token: 0x0400018D RID: 397
		public float rotateSpeedVar;
	}
}
