using System;
using CutTheRope.iframework;

namespace CutTheRope.ios
{
	// Token: 0x02000013 RID: 19
	internal class NSObject : FrameworkTypes
	{
		// Token: 0x060000D1 RID: 209 RVA: 0x000051CF File Offset: 0x000033CF
		public static void NSREL(NSObject obj)
		{
		}

		// Token: 0x060000D2 RID: 210 RVA: 0x000051D1 File Offset: 0x000033D1
		public static object NSRET(object obj)
		{
			return obj;
		}

		// Token: 0x060000D3 RID: 211 RVA: 0x000051D4 File Offset: 0x000033D4
		public static NSString NSS(string s)
		{
			return new NSString(s);
		}

		// Token: 0x060000D4 RID: 212 RVA: 0x000051DC File Offset: 0x000033DC
		public virtual NSObject init()
		{
			return this;
		}

		// Token: 0x060000D5 RID: 213 RVA: 0x000051DF File Offset: 0x000033DF
		public virtual void dealloc()
		{
		}

		// Token: 0x060000D6 RID: 214 RVA: 0x000051E1 File Offset: 0x000033E1
		public virtual void release()
		{
		}

		// Token: 0x060000D7 RID: 215 RVA: 0x000051E3 File Offset: 0x000033E3
		public virtual void retain()
		{
		}

		// Token: 0x060000D8 RID: 216 RVA: 0x000051E5 File Offset: 0x000033E5
		public static void free(object o)
		{
		}
	}
}
