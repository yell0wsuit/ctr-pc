using System;
using CutTheRope.ios;

namespace CutTheRope.iframework.visual
{
	// Token: 0x02000032 RID: 50
	internal class FormattedString : NSObject
	{
		// Token: 0x060001BB RID: 443 RVA: 0x000088B6 File Offset: 0x00006AB6
		public virtual FormattedString initWithStringAndWidth(NSString str, float w)
		{
			if (base.init() != null)
			{
				this.string_ = (NSString)NSObject.NSRET(str);
				this.width = w;
			}
			return this;
		}

		// Token: 0x060001BC RID: 444 RVA: 0x000088D9 File Offset: 0x00006AD9
		public override void dealloc()
		{
			this.string_ = null;
			base.dealloc();
		}

		// Token: 0x04000137 RID: 311
		public NSString string_;

		// Token: 0x04000138 RID: 312
		public float width;
	}
}
