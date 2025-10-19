using System;
using CutTheRope.iframework.core;
using CutTheRope.iframework.helpers;
using CutTheRope.windows;

namespace CutTheRope.iframework.visual
{
	// Token: 0x0200003D RID: 61
	internal class Particles : BaseElement
	{
		// Token: 0x0600021C RID: 540 RVA: 0x0000AB00 File Offset: 0x00008D00
		public static Vector rotatePreCalc(Vector v, float cosA, float sinA, float cx, float cy)
		{
			Vector result = v;
			result.x -= cx;
			result.y -= cy;
			float num = result.x * cosA - result.y * sinA;
			float num2 = result.x * sinA + result.y * cosA;
			result.x = num + cx;
			result.y = num2 + cy;
			return result;
		}

		// Token: 0x0600021D RID: 541 RVA: 0x0000AB64 File Offset: 0x00008D64
		public virtual void updateParticle(ref Particle p, float delta)
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
				this.vertices[this.particleIdx].x = p.pos.x;
				this.vertices[this.particleIdx].y = p.pos.y;
				this.vertices[this.particleIdx].size = p.size;
				this.colors[this.particleIdx] = p.color;
				this.particleIdx++;
				return;
			}
			if (this.particleIdx != this.particleCount - 1)
			{
				this.particles[this.particleIdx] = this.particles[this.particleCount - 1];
			}
			this.particleCount--;
		}

		// Token: 0x0600021E RID: 542 RVA: 0x0000AD84 File Offset: 0x00008F84
		public override void update(float delta)
		{
			base.update(delta);
			if (this.particlesDelegate != null && this.particleCount == 0 && !this.active)
			{
				this.particlesDelegate(this);
				return;
			}
			if (this.vertices == null)
			{
				return;
			}
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
			OpenGL.glBindBuffer(2, this.verticesID);
			OpenGL.glBufferData(2, this.vertices, 3);
			OpenGL.glBindBuffer(2, this.colorsID);
			OpenGL.glBufferData(2, this.colors, 3);
			OpenGL.glBindBuffer(2, 0U);
		}

		// Token: 0x0600021F RID: 543 RVA: 0x0000AEBE File Offset: 0x000090BE
		public override void dealloc()
		{
			this.particles = null;
			this.vertices = null;
			this.colors = null;
			OpenGL.glDeleteBuffers(1, ref this.verticesID);
			OpenGL.glDeleteBuffers(1, ref this.colorsID);
			this.texture = null;
			base.dealloc();
		}

		// Token: 0x06000220 RID: 544 RVA: 0x0000AEFA File Offset: 0x000090FA
		public override void draw()
		{
			this.preDraw();
			this.postDraw();
		}

		// Token: 0x06000221 RID: 545 RVA: 0x0000AF08 File Offset: 0x00009108
		public virtual Particles initWithTotalParticles(int numberOfParticles)
		{
			if (this.init() == null)
			{
				return null;
			}
			this.width = (int)FrameworkTypes.SCREEN_WIDTH;
			this.height = (int)FrameworkTypes.SCREEN_HEIGHT;
			this.totalParticles = numberOfParticles;
			this.particles = new Particle[this.totalParticles];
			this.vertices = new PointSprite[this.totalParticles];
			this.colors = new RGBAColor[this.totalParticles];
			if (this.particles == null || this.vertices == null || this.colors == null)
			{
				this.particles = null;
				this.vertices = null;
				this.colors = null;
				return null;
			}
			this.active = false;
			this.blendAdditive = false;
			OpenGL.glGenBuffers(1, ref this.verticesID);
			OpenGL.glGenBuffers(1, ref this.colorsID);
			return this;
		}

		// Token: 0x06000222 RID: 546 RVA: 0x0000AFC7 File Offset: 0x000091C7
		public virtual bool addParticle()
		{
			if (this.isFull())
			{
				return false;
			}
			this.initParticle(ref this.particles[this.particleCount]);
			this.particleCount++;
			return true;
		}

		// Token: 0x06000223 RID: 547 RVA: 0x0000AFFC File Offset: 0x000091FC
		public virtual void initParticle(ref Particle particle)
		{
			particle.pos.x = this.x + this.posVar.x * MathHelper.RND_MINUS1_1;
			particle.pos.y = this.y + this.posVar.y * MathHelper.RND_MINUS1_1;
			particle.startPos = particle.pos;
			float num = MathHelper.DEGREES_TO_RADIANS(this.angle + this.angleVar * MathHelper.RND_MINUS1_1);
			Vector v = default(Vector);
			v.y = MathHelper.sinf(num);
			v.x = MathHelper.cosf(num);
			float s = this.speed + this.speedVar * MathHelper.RND_MINUS1_1;
			particle.dir = MathHelper.vectMult(v, s);
			particle.radialAccel = this.radialAccel + this.radialAccelVar * MathHelper.RND_MINUS1_1;
			particle.tangentialAccel = this.tangentialAccel + this.tangentialAccelVar * MathHelper.RND_MINUS1_1;
			particle.life = this.life + this.lifeVar * MathHelper.RND_MINUS1_1;
			RGBAColor rGBAColor = default(RGBAColor);
			rGBAColor.r = this.startColor.r + this.startColorVar.r * MathHelper.RND_MINUS1_1;
			rGBAColor.g = this.startColor.g + this.startColorVar.g * MathHelper.RND_MINUS1_1;
			rGBAColor.b = this.startColor.b + this.startColorVar.b * MathHelper.RND_MINUS1_1;
			rGBAColor.a = this.startColor.a + this.startColorVar.a * MathHelper.RND_MINUS1_1;
			RGBAColor rGBAColor2 = default(RGBAColor);
			rGBAColor2.r = this.endColor.r + this.endColorVar.r * MathHelper.RND_MINUS1_1;
			rGBAColor2.g = this.endColor.g + this.endColorVar.g * MathHelper.RND_MINUS1_1;
			rGBAColor2.b = this.endColor.b + this.endColorVar.b * MathHelper.RND_MINUS1_1;
			rGBAColor2.a = this.endColor.a + this.endColorVar.a * MathHelper.RND_MINUS1_1;
			particle.color = rGBAColor;
			particle.deltaColor.r = (rGBAColor2.r - rGBAColor.r) / particle.life;
			particle.deltaColor.g = (rGBAColor2.g - rGBAColor.g) / particle.life;
			particle.deltaColor.b = (rGBAColor2.b - rGBAColor.b) / particle.life;
			particle.deltaColor.a = (rGBAColor2.a - rGBAColor.a) / particle.life;
			particle.size = this.size + this.sizeVar * MathHelper.RND_MINUS1_1;
		}

		// Token: 0x06000224 RID: 548 RVA: 0x0000B2D2 File Offset: 0x000094D2
		public virtual void startSystem(int initialParticles)
		{
			this.particleCount = 0;
			while (this.particleCount < initialParticles)
			{
				this.addParticle();
			}
			this.active = true;
		}

		// Token: 0x06000225 RID: 549 RVA: 0x0000B2F4 File Offset: 0x000094F4
		public virtual void stopSystem()
		{
			this.active = false;
			this.elapsed = this.duration;
			this.emitCounter = 0f;
		}

		// Token: 0x06000226 RID: 550 RVA: 0x0000B314 File Offset: 0x00009514
		public virtual void resetSystem()
		{
			this.elapsed = 0f;
			this.emitCounter = 0f;
		}

		// Token: 0x06000227 RID: 551 RVA: 0x0000B32C File Offset: 0x0000952C
		public virtual bool isFull()
		{
			return this.particleCount == this.totalParticles;
		}

		// Token: 0x06000228 RID: 552 RVA: 0x0000B33C File Offset: 0x0000953C
		public virtual void setBlendAdditive(bool b)
		{
			this.blendAdditive = b;
		}

		// Token: 0x04000164 RID: 356
		public bool active;

		// Token: 0x04000165 RID: 357
		public float duration;

		// Token: 0x04000166 RID: 358
		public float elapsed;

		// Token: 0x04000167 RID: 359
		public Vector gravity;

		// Token: 0x04000168 RID: 360
		public Vector posVar;

		// Token: 0x04000169 RID: 361
		public float angle;

		// Token: 0x0400016A RID: 362
		public float angleVar;

		// Token: 0x0400016B RID: 363
		public float speed;

		// Token: 0x0400016C RID: 364
		public float speedVar;

		// Token: 0x0400016D RID: 365
		public float tangentialAccel;

		// Token: 0x0400016E RID: 366
		public float tangentialAccelVar;

		// Token: 0x0400016F RID: 367
		public float radialAccel;

		// Token: 0x04000170 RID: 368
		public float radialAccelVar;

		// Token: 0x04000171 RID: 369
		public float size;

		// Token: 0x04000172 RID: 370
		public float endSize;

		// Token: 0x04000173 RID: 371
		public float sizeVar;

		// Token: 0x04000174 RID: 372
		public float life;

		// Token: 0x04000175 RID: 373
		public float lifeVar;

		// Token: 0x04000176 RID: 374
		public RGBAColor startColor;

		// Token: 0x04000177 RID: 375
		public RGBAColor startColorVar;

		// Token: 0x04000178 RID: 376
		public RGBAColor endColor;

		// Token: 0x04000179 RID: 377
		public RGBAColor endColorVar;

		// Token: 0x0400017A RID: 378
		public Particle[] particles;

		// Token: 0x0400017B RID: 379
		public int totalParticles;

		// Token: 0x0400017C RID: 380
		public int particleCount;

		// Token: 0x0400017D RID: 381
		public bool blendAdditive;

		// Token: 0x0400017E RID: 382
		public bool colorModulate;

		// Token: 0x0400017F RID: 383
		public float emissionRate;

		// Token: 0x04000180 RID: 384
		public float emitCounter;

		// Token: 0x04000181 RID: 385
		public Texture2D texture;

		// Token: 0x04000182 RID: 386
		public PointSprite[] vertices;

		// Token: 0x04000183 RID: 387
		public RGBAColor[] colors;

		// Token: 0x04000184 RID: 388
		private uint verticesID;

		// Token: 0x04000185 RID: 389
		public uint colorsID;

		// Token: 0x04000186 RID: 390
		public int particleIdx;

		// Token: 0x04000187 RID: 391
		public Particles.ParticlesFinished particlesDelegate;

		// Token: 0x020000AE RID: 174
		// (Invoke) Token: 0x06000668 RID: 1640
		public delegate void ParticlesFinished(Particles p);
	}
}
