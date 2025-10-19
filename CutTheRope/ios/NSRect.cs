using System;
using CutTheRope.iframework;
using Microsoft.Xna.Framework;

namespace CutTheRope.ios
{
	// Token: 0x02000016 RID: 22
	internal struct NSRect
	{
		// Token: 0x060000DA RID: 218 RVA: 0x000051F0 File Offset: 0x000033F0
		public NSRect(float _x, float _y, float _w, float _h)
		{
			this.origin = new NSPoint
			{
				x = _x,
				y = _y
			};
			this.size = new NSSize
			{
				width = _w,
				height = _h
			};
		}

		// Token: 0x060000DB RID: 219 RVA: 0x0000523C File Offset: 0x0000343C
		public NSRect(Microsoft.Xna.Framework.Rectangle xnaRect)
		{
			this.origin = new NSPoint
			{
				x = (float)xnaRect.X,
				y = (float)xnaRect.Y
			};
			this.size = new NSSize
			{
				width = (float)xnaRect.Width,
				height = (float)xnaRect.Height
			};
		}

		// Token: 0x060000DC RID: 220 RVA: 0x000052A0 File Offset: 0x000034A0
		public NSRect(CutTheRope.iframework.Rectangle ctrRect)
		{
			this.origin = new NSPoint
			{
				x = ctrRect.x,
				y = ctrRect.y
			};
			this.size = new NSSize
			{
				width = ctrRect.w,
				height = ctrRect.h
			};
		}

		// Token: 0x0400008E RID: 142
		public NSPoint origin;

		// Token: 0x0400008F RID: 143
		public NSSize size;
	}
}
