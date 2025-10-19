using System;
using System.Collections.Generic;
using CutTheRope.iframework.core;
using CutTheRope.iframework.helpers;
using CutTheRope.ios;
using CutTheRope.windows;

namespace CutTheRope.iframework.visual
{
	// Token: 0x02000049 RID: 73
	internal class Text : BaseElement
	{
		// Token: 0x06000264 RID: 612 RVA: 0x0000D674 File Offset: 0x0000B874
		public static Text createWithFontandString(int i, string str)
		{
			return Text.createWithFontandString(i, NSObject.NSS(str));
		}

		// Token: 0x06000265 RID: 613 RVA: 0x0000D682 File Offset: 0x0000B882
		public static Text createWithFontandString(int i, NSString str)
		{
			Text text = new Text().initWithFont(Application.getFont(i));
			text.setString(str);
			return text;
		}

		// Token: 0x06000266 RID: 614 RVA: 0x0000D69C File Offset: 0x0000B89C
		public virtual Text initWithFont(FontGeneric i)
		{
			if (this.init() != null)
			{
				this.font = (FontGeneric)NSObject.NSRET(i);
				this.formattedStrings = new List<FormattedString>();
				this.width = -1;
				this.height = -1;
				this.align = 1;
				this.multiDrawers = new List<ImageMultiDrawer>();
				this.wrapLongWords = false;
				this.maxHeight = -1f;
			}
			this.font.notifyTextCreated(this);
			return this;
		}

		// Token: 0x06000267 RID: 615 RVA: 0x0000D70C File Offset: 0x0000B90C
		public virtual void setString(string newString)
		{
			this.setString(NSObject.NSS(newString));
		}

		// Token: 0x06000268 RID: 616 RVA: 0x0000D71A File Offset: 0x0000B91A
		public virtual void setString(NSString newString)
		{
			this.setStringandWidth(newString, -1f);
		}

		// Token: 0x06000269 RID: 617 RVA: 0x0000D728 File Offset: 0x0000B928
		public virtual void setStringandWidth(NSString newString, double w)
		{
			this.setStringandWidth(newString, (float)w);
		}

		// Token: 0x0600026A RID: 618 RVA: 0x0000D734 File Offset: 0x0000B934
		public virtual void setStringandWidth(NSString newString, float w)
		{
			this.string_ = newString;
			if (this.string_ == null)
			{
				this.string_ = new NSString("");
			}
			this.font.notifyTextChanged(this);
			if (w == -1f)
			{
				float num = 0.1f;
				this.wrapWidth = this.font.stringWidth(this.string_) + num;
			}
			else
			{
				this.wrapWidth = w;
			}
			if (this.string_ != null)
			{
				this.formatText();
				this.updateDrawerValues();
				return;
			}
			this.stringLength = 0;
		}

		// Token: 0x0600026B RID: 619 RVA: 0x0000D7B8 File Offset: 0x0000B9B8
		public virtual void updateDrawerValues()
		{
			this.multiDrawers.Clear();
			int num = this.font.totalCharmaps();
			int num2 = this.string_.length();
			char[] characters = this.string_.getCharacters();
			int[] array = new int[num];
			for (int i = 0; i < num2; i++)
			{
				if (characters[i] != ' ' && characters[i] != '*' && characters[i] != '\n')
				{
					array[this.font.getCharmapIndex(characters[i])]++;
				}
			}
			for (int j = 0; j < num; j++)
			{
				int num3 = array[j];
				if (num3 > 0)
				{
					ImageMultiDrawer item = new ImageMultiDrawer().initWithImageandCapacity(this.font.getCharmap(j), num3);
					this.multiDrawers.Add(item);
				}
			}
			float num4 = 0f;
			int num5 = (int)this.font.fontHeight();
			int num6 = 0;
			char[] characters2 = NSObject.NSS("..").getCharacters();
			int num7 = (int)this.font.getCharOffset(characters2, 0, 2);
			int num8 = (int)((this.maxHeight == -1f) ? ((float)this.formattedStrings.Count) : MathHelper.MIN((float)this.formattedStrings.Count, this.maxHeight / ((float)num5 + this.font.getLineOffset())));
			bool flag = num8 != this.formattedStrings.Count;
			int[] array2 = new int[num];
			for (int k = 0; k < num8; k++)
			{
				FormattedString formattedString = this.formattedStrings[k];
				int num9 = formattedString.string_.length();
				char[] characters3 = formattedString.string_.getCharacters();
				float num10 = ((this.align == 1) ? 0f : ((this.align != 2) ? (this.wrapWidth - formattedString.width) : ((this.wrapWidth - formattedString.width) / 2f)));
				for (int l = 0; l < num9; l++)
				{
					if (characters3[l] != '*')
					{
						if (characters3[l] == ' ')
						{
							num10 += this.font.getCharWidth(' ') + this.font.getCharOffset(characters3, l, num9);
						}
						else
						{
							int charmapIndex = this.font.getCharmapIndex(characters3[l]);
							int charQuad = this.font.getCharQuad(characters3[l]);
							ImageMultiDrawer imageMultiDrawer3 = this.multiDrawers[charmapIndex];
							int num12 = charQuad;
							float num13 = num10;
							float num14 = num4;
							int[] array3 = array2;
							int num15 = charmapIndex;
							int num16 = array3[num15];
							array3[num15] = num16 + 1;
							imageMultiDrawer3.mapTextureQuadAtXYatIndex(num12, num13, num14, num16);
							num6++;
							num10 += this.font.getCharWidth(characters3[l]) + this.font.getCharOffset(characters3, l, num9);
						}
						if (flag && k == num8 - 1)
						{
							int charmapIndex2 = this.font.getCharmapIndex('.');
							int charQuad2 = this.font.getCharQuad('.');
							ImageMultiDrawer imageMultiDrawer2 = this.multiDrawers[charmapIndex2];
							int num11 = (int)this.font.getCharWidth('.');
							if (l == num9 - 1 || (l == num9 - 2 && num10 + (float)(3 * (num11 + num7)) + this.font.getCharWidth(' ') > this.wrapWidth))
							{
								imageMultiDrawer2.mapTextureQuadAtXYatIndex(charQuad2, num10, num4, num6++);
								num10 += (float)(num11 + num7);
								imageMultiDrawer2.mapTextureQuadAtXYatIndex(charQuad2, num10, num4, num6++);
								num10 += (float)(num11 + num7);
								imageMultiDrawer2.mapTextureQuadAtXYatIndex(charQuad2, num10, num4, num6++);
								break;
							}
						}
					}
				}
				num4 += (float)num5 + this.font.getLineOffset();
			}
			this.stringLength = num6;
			if (this.formattedStrings.Count <= 1)
			{
				this.height = (int)this.font.fontHeight();
				this.width = (int)this.wrapWidth;
			}
			else
			{
				this.height = (int)((this.font.fontHeight() + this.font.getLineOffset()) * (float)this.formattedStrings.Count - this.font.getLineOffset());
				this.width = (int)this.wrapWidth;
			}
			if (this.maxHeight != -1f)
			{
				this.height = (int)MathHelper.MIN((float)this.height, this.maxHeight);
			}
		}

		// Token: 0x0600026C RID: 620 RVA: 0x0000DC01 File Offset: 0x0000BE01
		public virtual NSString getString()
		{
			return this.string_;
		}

		// Token: 0x0600026D RID: 621 RVA: 0x0000DC09 File Offset: 0x0000BE09
		public virtual void setAlignment(int a)
		{
			this.align = a;
		}

		// Token: 0x0600026E RID: 622 RVA: 0x0000DC14 File Offset: 0x0000BE14
		public override void draw()
		{
			this.preDraw();
			if (this.stringLength > 0)
			{
				OpenGL.glTranslatef(this.drawX, this.drawY, 0f);
				int i = 0;
				int count = this.multiDrawers.Count;
				while (i < count)
				{
					ImageMultiDrawer imageMultiDrawer = this.multiDrawers[i];
					if (imageMultiDrawer != null)
					{
						imageMultiDrawer.drawAllQuads();
						imageMultiDrawer.optimize(OpenGL.GetLastVertices_PositionNormalTexture());
					}
					i++;
				}
				OpenGL.glTranslatef(0f - this.drawX, 0f - this.drawY, 0f);
			}
			this.postDraw();
		}

		// Token: 0x0600026F RID: 623 RVA: 0x0000DCA8 File Offset: 0x0000BEA8
		public virtual void formatText()
		{
			short[] array = new short[512];
			char[] characters = this.string_.getCharacters();
			int num = this.string_.length();
			int num2 = 0;
			int num3 = 0;
			float num4 = 0f;
			int num5 = 0;
			int num6 = 0;
			float num7 = 0f;
			int num8 = 0;
			while (num8 < num)
			{
				char c = characters[num8++];
				if (c == ' ' || c == '\n' || c == '*')
				{
					num7 += num4;
					num6 = num8 - 1;
					num4 = 0f;
					num3 = num8;
					if (c == ' ')
					{
						num3--;
						num4 = this.font.getCharWidth(' ') + this.font.getCharOffset(characters, num8 - 1, num);
					}
				}
				else
				{
					num4 += this.font.getCharWidth(c) + this.font.getCharOffset(characters, num8 - 1, num);
				}
				bool flag = num7 + num4 > this.wrapWidth;
				if (this.wrapLongWords && flag && num6 == num5)
				{
					num7 += num4;
					num6 = num8;
					num4 = 0f;
					num3 = num8;
				}
				if ((num7 + num4 > this.wrapWidth && num6 != num5) || c == '\n')
				{
					array[num2++] = (short)num5;
					array[num2++] = (short)num6;
					while (num3 < num && characters[num3] == ' ')
					{
						num3++;
						num4 -= this.font.getCharWidth(' ');
					}
					num5 = num3;
					num6 = num5;
					num7 = 0f;
				}
			}
			if (num4 != 0f)
			{
				array[num2++] = (short)num5;
				array[num2++] = (short)num8;
			}
			int num9 = num2 >> 1;
			this.formattedStrings.Clear();
			NSRange range = default(NSRange);
			for (int i = 0; i < num9; i++)
			{
				int num10 = (int)array[i << 1];
				int num11 = (int)array[(i << 1) + 1];
				range.location = (uint)num10;
				range.length = (uint)(num11 - num10);
				NSString str = this.string_.substringWithRange(range);
				float w = this.font.stringWidth(str);
				FormattedString item = new FormattedString().initWithStringAndWidth(str, w);
				this.formattedStrings.Add(item);
			}
		}

		// Token: 0x06000270 RID: 624 RVA: 0x0000DECA File Offset: 0x0000C0CA
		private BaseElement createFromXML(XMLNode xml)
		{
			return null;
		}

		// Token: 0x06000271 RID: 625 RVA: 0x0000DECD File Offset: 0x0000C0CD
		public override void dealloc()
		{
			this.font.notifyTextDeleted(this);
			this.string_ = null;
			this.font = null;
			this.formattedStrings = null;
			this.multiDrawers.Clear();
			this.multiDrawers = null;
			base.dealloc();
		}

		// Token: 0x040001CC RID: 460
		public int align;

		// Token: 0x040001CD RID: 461
		public NSString string_;

		// Token: 0x040001CE RID: 462
		public int stringLength;

		// Token: 0x040001CF RID: 463
		public FontGeneric font;

		// Token: 0x040001D0 RID: 464
		public float wrapWidth;

		// Token: 0x040001D1 RID: 465
		private List<FormattedString> formattedStrings;

		// Token: 0x040001D2 RID: 466
		private List<ImageMultiDrawer> multiDrawers;

		// Token: 0x040001D3 RID: 467
		public float maxHeight;

		// Token: 0x040001D4 RID: 468
		public bool wrapLongWords;
	}
}
