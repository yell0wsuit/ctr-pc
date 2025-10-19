using System;
using CutTheRope.ios;

namespace CutTheRope.iframework.visual
{
	// Token: 0x02000030 RID: 48
	internal class Font : FontGeneric
	{
		// Token: 0x060001A0 RID: 416 RVA: 0x00008678 File Offset: 0x00006878
		public virtual Font initWithVariableSizeCharscharMapFileKerning(NSString strParam, Texture2D charmapfile, object k)
		{
			if (base.init() != null)
			{
				this._isWvga = charmapfile.isWvga();
				this.charmap = new Image().initWithTexture(charmapfile);
				this.quadsCount = charmapfile.quadsCount;
				this.height = charmapfile.quadRects[0].h;
				this.chars = strParam.copy();
				this.sortedChars = this.chars.getCharacters();
				Array.Sort<char>(this.sortedChars);
				this.charOffset = 0f;
				this.lineOffset = 0f;
			}
			return this;
		}

		// Token: 0x060001A1 RID: 417 RVA: 0x0000870C File Offset: 0x0000690C
		public override void dealloc()
		{
			this.chars = null;
			this.sortedChars = null;
			this.charmap = null;
			base.dealloc();
		}

		// Token: 0x060001A2 RID: 418 RVA: 0x0000872C File Offset: 0x0000692C
		public override void setCharOffsetLineOffsetSpaceWidth(float co, float lo, float sw)
		{
			this.charOffset = co;
			this.lineOffset = lo;
			this.spaceWidth = sw;
			if (this._isWvga)
			{
				this.charOffset = (float)((int)((double)this.charOffset / 1.5));
				this.lineOffset = (float)((int)((double)this.lineOffset / 1.5));
				this.spaceWidth = (float)((int)((double)this.spaceWidth / 1.5));
			}
		}

		// Token: 0x060001A3 RID: 419 RVA: 0x000087A1 File Offset: 0x000069A1
		public override float fontHeight()
		{
			return this.height;
		}

		// Token: 0x060001A4 RID: 420 RVA: 0x000087A9 File Offset: 0x000069A9
		public override bool canDraw(char c)
		{
			return c == ' ' || Array.BinarySearch<char>(this.sortedChars, c) >= 0;
		}

		// Token: 0x060001A5 RID: 421 RVA: 0x000087C4 File Offset: 0x000069C4
		public override float getCharWidth(char c)
		{
			if (c == ' ')
			{
				return this.spaceWidth;
			}
			if (c == '*')
			{
				return 0f;
			}
			return this.charmap.texture.quadRects[this.getCharQuad(c)].w;
		}

		// Token: 0x060001A6 RID: 422 RVA: 0x000087FE File Offset: 0x000069FE
		public override int getCharmapIndex(char c)
		{
			return 0;
		}

		// Token: 0x060001A7 RID: 423 RVA: 0x00008804 File Offset: 0x00006A04
		public override int getCharQuad(char c)
		{
			int num = this.chars.IndexOf(c);
			if (num >= 0)
			{
				return num;
			}
			return -1;
		}

		// Token: 0x060001A8 RID: 424 RVA: 0x00008825 File Offset: 0x00006A25
		public override float getCharOffset(char[] s, int c, int len)
		{
			if (c == len - 1)
			{
				return 0f;
			}
			return this.charOffset;
		}

		// Token: 0x060001A9 RID: 425 RVA: 0x00008839 File Offset: 0x00006A39
		public override int totalCharmaps()
		{
			return 1;
		}

		// Token: 0x060001AA RID: 426 RVA: 0x0000883C File Offset: 0x00006A3C
		public override Image getCharmap(int i)
		{
			return this.charmap;
		}

		// Token: 0x0400012E RID: 302
		private NSString chars;

		// Token: 0x0400012F RID: 303
		private char[] sortedChars;

		// Token: 0x04000130 RID: 304
		private bool _isWvga;

		// Token: 0x04000131 RID: 305
		private int quadsCount;

		// Token: 0x04000132 RID: 306
		private float height;

		// Token: 0x04000133 RID: 307
		private Image charmap;
	}
}
