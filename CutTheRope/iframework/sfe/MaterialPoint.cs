using System;
using CutTheRope.iframework.core;
using CutTheRope.iframework.helpers;
using CutTheRope.ios;

namespace CutTheRope.iframework.sfe
{
	// Token: 0x02000058 RID: 88
	internal class MaterialPoint : NSObject
	{
		// Token: 0x060002E6 RID: 742 RVA: 0x00011C89 File Offset: 0x0000FE89
		public override NSObject init()
		{
			if (base.init() != null)
			{
				this.forces = new Vector[10];
				this.setWeight(1f);
				this.resetAll();
			}
			return this;
		}

		// Token: 0x060002E7 RID: 743 RVA: 0x00011CB2 File Offset: 0x0000FEB2
		public virtual void setWeight(float w)
		{
			this.weight = w;
			this.invWeight = (float)(1.0 / (double)this.weight);
			this.gravity = MathHelper.vect(0f, 784f * this.weight);
		}

		// Token: 0x060002E8 RID: 744 RVA: 0x00011CEF File Offset: 0x0000FEEF
		public override void dealloc()
		{
			this.forces = null;
			base.dealloc();
		}

		// Token: 0x060002E9 RID: 745 RVA: 0x00011CFE File Offset: 0x0000FEFE
		public virtual void resetForces()
		{
			this.forces = new Vector[10];
			this.highestForceIndex = -1;
		}

		// Token: 0x060002EA RID: 746 RVA: 0x00011D14 File Offset: 0x0000FF14
		public virtual void resetAll()
		{
			this.resetForces();
			this.v = MathHelper.vectZero;
			this.a = MathHelper.vectZero;
			this.pos = MathHelper.vectZero;
			this.posDelta = MathHelper.vectZero;
			this.totalForce = MathHelper.vectZero;
		}

		// Token: 0x060002EB RID: 747 RVA: 0x00011D53 File Offset: 0x0000FF53
		public virtual void setForcewithID(Vector force, int n)
		{
			this.forces[n] = force;
			if (n > this.highestForceIndex)
			{
				this.highestForceIndex = n;
			}
		}

		// Token: 0x060002EC RID: 748 RVA: 0x00011D72 File Offset: 0x0000FF72
		public virtual void deleteForce(int n)
		{
			this.forces[n] = MathHelper.vectZero;
		}

		// Token: 0x060002ED RID: 749 RVA: 0x00011D85 File Offset: 0x0000FF85
		public virtual Vector getForce(int n)
		{
			return this.forces[n];
		}

		// Token: 0x060002EE RID: 750 RVA: 0x00011D94 File Offset: 0x0000FF94
		public virtual void applyImpulseDelta(Vector impulse, float delta)
		{
			if (!MathHelper.vectEqual(impulse, MathHelper.vectZero))
			{
				Vector v = MathHelper.vectMult(impulse, (float)((double)delta / 1.0));
				this.pos = MathHelper.vectAdd(this.pos, v);
			}
		}

		// Token: 0x060002EF RID: 751 RVA: 0x00011DD4 File Offset: 0x0000FFD4
		public virtual void updatewithPrecision(float delta, float p)
		{
			int num = (int)(delta / p) + 1;
			if (num != 0)
			{
				delta /= (float)num;
			}
			for (int i = 0; i < num; i++)
			{
				this.update(delta);
			}
		}

		// Token: 0x060002F0 RID: 752 RVA: 0x00011E04 File Offset: 0x00010004
		public virtual void update(float delta)
		{
			this.totalForce = MathHelper.vectZero;
			if (!this.disableGravity)
			{
				if (!MathHelper.vectEqual(MaterialPoint.globalGravity, MathHelper.vectZero))
				{
					this.totalForce = MathHelper.vectAdd(this.totalForce, MathHelper.vectMult(MaterialPoint.globalGravity, this.weight));
				}
				else
				{
					this.totalForce = MathHelper.vectAdd(this.totalForce, this.gravity);
				}
			}
			if (this.highestForceIndex != -1)
			{
				for (int i = 0; i <= this.highestForceIndex; i++)
				{
					this.totalForce = MathHelper.vectAdd(this.totalForce, this.forces[i]);
				}
			}
			this.totalForce = MathHelper.vectMult(this.totalForce, this.invWeight);
			this.a = MathHelper.vectMult(this.totalForce, (float)((double)delta / 1.0));
			this.v = MathHelper.vectAdd(this.v, this.a);
			this.posDelta = MathHelper.vectMult(this.v, (float)((double)delta / 1.0));
			this.pos = MathHelper.vectAdd(this.pos, this.posDelta);
		}

		// Token: 0x060002F1 RID: 753 RVA: 0x00011F27 File Offset: 0x00010127
		public virtual void drawForces()
		{
		}

		// Token: 0x0400023C RID: 572
		protected const double TIME_SCALE = 1.0;

		// Token: 0x0400023D RID: 573
		private const double PIXEL_TO_SI_METERS_K = 80.0;

		// Token: 0x0400023E RID: 574
		public const double GCONST = 784.0;

		// Token: 0x0400023F RID: 575
		private const int MAX_FORCES = 10;

		// Token: 0x04000240 RID: 576
		public static Vector globalGravity;

		// Token: 0x04000241 RID: 577
		public Vector pos;

		// Token: 0x04000242 RID: 578
		public Vector posDelta;

		// Token: 0x04000243 RID: 579
		public Vector v;

		// Token: 0x04000244 RID: 580
		public Vector a;

		// Token: 0x04000245 RID: 581
		public Vector totalForce;

		// Token: 0x04000246 RID: 582
		public float weight;

		// Token: 0x04000247 RID: 583
		public float invWeight;

		// Token: 0x04000248 RID: 584
		public Vector[] forces;

		// Token: 0x04000249 RID: 585
		public int highestForceIndex;

		// Token: 0x0400024A RID: 586
		public Vector gravity;

		// Token: 0x0400024B RID: 587
		public bool disableGravity;
	}
}
