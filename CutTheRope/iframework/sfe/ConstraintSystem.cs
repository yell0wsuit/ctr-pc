using System;
using System.Collections.Generic;
using CutTheRope.ios;

namespace CutTheRope.iframework.sfe
{
	// Token: 0x02000057 RID: 87
	internal class ConstraintSystem : NSObject
	{
		// Token: 0x060002DF RID: 735 RVA: 0x00011BAF File Offset: 0x0000FDAF
		public override NSObject init()
		{
			if (base.init() != null)
			{
				this.relaxationTimes = 1;
				this.parts = new List<ConstraintedPoint>();
			}
			return this;
		}

		// Token: 0x060002E0 RID: 736 RVA: 0x00011BCC File Offset: 0x0000FDCC
		public virtual void addPart(ConstraintedPoint cp)
		{
			this.parts.Add(cp);
		}

		// Token: 0x060002E1 RID: 737 RVA: 0x00011BDA File Offset: 0x0000FDDA
		public virtual void addPartAt(ConstraintedPoint cp, int p)
		{
			this.parts.Insert(p, cp);
		}

		// Token: 0x060002E2 RID: 738 RVA: 0x00011BEC File Offset: 0x0000FDEC
		public virtual void update(float delta)
		{
			int count = this.parts.Count;
			for (int i = 0; i < count; i++)
			{
				ConstraintedPoint constraintedPoint = this.parts[i];
				if (constraintedPoint != null)
				{
					constraintedPoint.update(delta);
				}
			}
			int count2 = this.parts.Count;
			for (int j = 0; j < this.relaxationTimes; j++)
			{
				for (int k = 0; k < count2; k++)
				{
					ConstraintedPoint.satisfyConstraints(this.parts[k]);
				}
			}
		}

		// Token: 0x060002E3 RID: 739 RVA: 0x00011C6B File Offset: 0x0000FE6B
		public virtual void draw()
		{
			throw new NotImplementedException();
		}

		// Token: 0x060002E4 RID: 740 RVA: 0x00011C72 File Offset: 0x0000FE72
		public override void dealloc()
		{
			this.parts = null;
			base.dealloc();
		}

		// Token: 0x0400023A RID: 570
		public List<ConstraintedPoint> parts;

		// Token: 0x0400023B RID: 571
		public int relaxationTimes;
	}
}
