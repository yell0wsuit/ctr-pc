using System;
using CutTheRope.ios;

namespace CutTheRope.iframework.visual
{
	// Token: 0x02000031 RID: 49
	internal abstract class FontGeneric : NSObject
	{
		// Token: 0x060001AC RID: 428 RVA: 0x0000884C File Offset: 0x00006A4C
		public virtual float stringWidth(NSString str)
		{
			float num = 0f;
			int num2 = str.length();
			char[] characters = str.getCharacters();
			float num3 = 0f;
			for (int i = 0; i < num2; i++)
			{
				num3 = this.getCharOffset(characters, i, num2);
				num += this.getCharWidth(characters[i]) + num3;
			}
			return num - num3;
		}

		// Token: 0x060001AD RID: 429
		public abstract void setCharOffsetLineOffsetSpaceWidth(float co, float lo, float sw);

		// Token: 0x060001AE RID: 430
		public abstract float fontHeight();

		// Token: 0x060001AF RID: 431
		public abstract bool canDraw(char c);

		// Token: 0x060001B0 RID: 432
		public abstract float getCharWidth(char c);

		// Token: 0x060001B1 RID: 433
		public abstract int getCharmapIndex(char c);

		// Token: 0x060001B2 RID: 434
		public abstract int getCharQuad(char c);

		// Token: 0x060001B3 RID: 435
		public abstract float getCharOffset(char[] s, int c, int len);

		// Token: 0x060001B4 RID: 436 RVA: 0x000088A0 File Offset: 0x00006AA0
		public virtual float getLineOffset()
		{
			return this.lineOffset;
		}

		// Token: 0x060001B5 RID: 437 RVA: 0x000088A8 File Offset: 0x00006AA8
		public virtual void notifyTextCreated(Text st)
		{
		}

		// Token: 0x060001B6 RID: 438 RVA: 0x000088AA File Offset: 0x00006AAA
		public virtual void notifyTextChanged(Text st)
		{
		}

		// Token: 0x060001B7 RID: 439 RVA: 0x000088AC File Offset: 0x00006AAC
		public virtual void notifyTextDeleted(Text st)
		{
		}

		// Token: 0x060001B8 RID: 440
		public abstract int totalCharmaps();

		// Token: 0x060001B9 RID: 441
		public abstract Image getCharmap(int i);

		// Token: 0x04000134 RID: 308
		protected float charOffset;

		// Token: 0x04000135 RID: 309
		protected float lineOffset;

		// Token: 0x04000136 RID: 310
		protected float spaceWidth;
	}
}
