using System;
using CutTheRope.ios;

namespace CutTheRope.iframework.sfe
{
	// Token: 0x02000055 RID: 85
	internal class Constraint : NSObject
	{
		// Token: 0x04000234 RID: 564
		public ConstraintedPoint cp;

		// Token: 0x04000235 RID: 565
		public float restLength;

		// Token: 0x04000236 RID: 566
		public Constraint.CONSTRAINT type;

		// Token: 0x020000B9 RID: 185
		public enum CONSTRAINT
		{
			// Token: 0x040008C5 RID: 2245
			CONSTRAINT_DISTANCE,
			// Token: 0x040008C6 RID: 2246
			CONSTRAINT_NOT_MORE_THAN,
			// Token: 0x040008C7 RID: 2247
			CONSTRAINT_NOT_LESS_THAN
		}
	}
}
