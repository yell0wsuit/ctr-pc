using System;
using CutTheRope.iframework.core;
using CutTheRope.iframework.helpers;
using CutTheRope.ios;
using CutTheRope.windows;

namespace CutTheRope.iframework.visual
{
	// Token: 0x02000037 RID: 55
	internal class Image : BaseElement
	{
		// Token: 0x17000022 RID: 34
		// (get) Token: 0x060001E5 RID: 485 RVA: 0x00009A46 File Offset: 0x00007C46
		public string _ResName
		{
			get
			{
				if (this.texture != null)
				{
					return this.texture._resName;
				}
				return "ERROR: texture == null";
			}
		}

		// Token: 0x060001E6 RID: 486 RVA: 0x00009A64 File Offset: 0x00007C64
		public static Vector getQuadSize(int textureID, int quad)
		{
			Texture2D texture2D = Application.getTexture(textureID);
			return MathHelper.vect(texture2D.quadRects[quad].w, texture2D.quadRects[quad].h);
		}

		// Token: 0x060001E7 RID: 487 RVA: 0x00009A9F File Offset: 0x00007C9F
		public static Vector getQuadOffset(int textureID, int quad)
		{
			return Application.getTexture(textureID).quadOffsets[quad];
		}

		// Token: 0x060001E8 RID: 488 RVA: 0x00009AB4 File Offset: 0x00007CB4
		public static Vector getQuadCenter(int textureID, int quad)
		{
			Texture2D texture2D = Application.getTexture(textureID);
			return MathHelper.vectAdd(texture2D.quadOffsets[quad], MathHelper.vect(MathHelper.ceil((double)texture2D.quadRects[quad].w / 2.0), MathHelper.ceil((double)texture2D.quadRects[quad].h / 2.0)));
		}

		// Token: 0x060001E9 RID: 489 RVA: 0x00009B20 File Offset: 0x00007D20
		public static Vector getRelativeQuadOffset(int textureID, int quadToCountFrom, int quad)
		{
			Vector quadOffset = Image.getQuadOffset(textureID, quadToCountFrom);
			return MathHelper.vectSub(Image.getQuadOffset(textureID, quad), quadOffset);
		}

		// Token: 0x060001EA RID: 490 RVA: 0x00009B44 File Offset: 0x00007D44
		public static void setElementPositionWithQuadCenter(BaseElement e, int textureID, int quad)
		{
			Vector quadCenter = Image.getQuadCenter(textureID, quad);
			e.x = quadCenter.x;
			e.y = quadCenter.y;
			e.anchor = 18;
		}

		// Token: 0x060001EB RID: 491 RVA: 0x00009B7C File Offset: 0x00007D7C
		public static void setElementPositionWithQuadOffset(BaseElement e, int textureID, int quad)
		{
			Vector quadOffset = Image.getQuadOffset(textureID, quad);
			e.x = quadOffset.x;
			e.y = quadOffset.y;
		}

		// Token: 0x060001EC RID: 492 RVA: 0x00009BAC File Offset: 0x00007DAC
		public static void setElementPositionWithRelativeQuadOffset(BaseElement e, int textureID, int quadToCountFrom, int quad)
		{
			Vector relativeQuadOffset = Image.getRelativeQuadOffset(textureID, quadToCountFrom, quad);
			e.x = relativeQuadOffset.x;
			e.y = relativeQuadOffset.y;
		}

		// Token: 0x060001ED RID: 493 RVA: 0x00009BDA File Offset: 0x00007DDA
		public static Image Image_create(Texture2D t)
		{
			return new Image().initWithTexture(t);
		}

		// Token: 0x060001EE RID: 494 RVA: 0x00009BE7 File Offset: 0x00007DE7
		public static Image Image_createWithResID(int r)
		{
			return Image.Image_create(Application.getTexture(r));
		}

		// Token: 0x060001EF RID: 495 RVA: 0x00009BF4 File Offset: 0x00007DF4
		public static Image Image_createWithResIDQuad(int r, int q)
		{
			Image image = Image.Image_create(Application.getTexture(r));
			image.setDrawQuad(q);
			return image;
		}

		// Token: 0x060001F0 RID: 496 RVA: 0x00009C08 File Offset: 0x00007E08
		public virtual Image initWithTexture(Texture2D t)
		{
			if (this.init() != null)
			{
				this.texture = t;
				NSObject.NSRET(this.texture);
				this.restoreCutTransparency = false;
				if (this.texture.quadsCount > 0)
				{
					this.setDrawQuad(0);
				}
				else
				{
					this.setDrawFullImage();
				}
			}
			return this;
		}

		// Token: 0x060001F1 RID: 497 RVA: 0x00009C55 File Offset: 0x00007E55
		public virtual void setDrawFullImage()
		{
			this.quadToDraw = -1;
			this.width = this.texture._realWidth;
			this.height = this.texture._realHeight;
		}

		// Token: 0x060001F2 RID: 498 RVA: 0x00009C80 File Offset: 0x00007E80
		public virtual void setDrawQuad(int n)
		{
			this.quadToDraw = n;
			if (!this.restoreCutTransparency)
			{
				this.width = (int)this.texture.quadRects[n].w;
				this.height = (int)this.texture.quadRects[n].h;
			}
		}

		// Token: 0x060001F3 RID: 499 RVA: 0x00009CD8 File Offset: 0x00007ED8
		public virtual void doRestoreCutTransparency()
		{
			if (this.texture.preCutSize.x != MathHelper.vectUndefined.x)
			{
				this.restoreCutTransparency = true;
				this.width = (int)this.texture.preCutSize.x;
				this.height = (int)this.texture.preCutSize.y;
			}
		}

		// Token: 0x060001F4 RID: 500 RVA: 0x00009D36 File Offset: 0x00007F36
		public override void draw()
		{
			this.preDraw();
			if (this.quadToDraw == -1)
			{
				GLDrawer.drawImage(this.texture, this.drawX, this.drawY);
			}
			else
			{
				this.drawQuad(this.quadToDraw);
			}
			this.postDraw();
		}

		// Token: 0x060001F5 RID: 501 RVA: 0x00009D74 File Offset: 0x00007F74
		public virtual void drawQuad(int n)
		{
			float w = this.texture.quadRects[n].w;
			float h = this.texture.quadRects[n].h;
			float num = this.drawX;
			float num2 = this.drawY;
			if (this.restoreCutTransparency)
			{
				num += this.texture.quadOffsets[n].x;
				num2 += this.texture.quadOffsets[n].y;
			}
			float[] pointer = new float[]
			{
				num,
				num2,
				num + w,
				num2,
				num,
				num2 + h,
				num + w,
				num2 + h
			};
			OpenGL.glEnable(0);
			OpenGL.glBindTexture(this.texture.name());
			OpenGL.glVertexPointer(2, 5, 0, pointer);
			OpenGL.glTexCoordPointer(2, 5, 0, this.texture.quads[n].toFloatArray());
			OpenGL.glDrawArrays(8, 0, 4);
		}

		// Token: 0x060001F6 RID: 502 RVA: 0x00009E6D File Offset: 0x0000806D
		public override bool handleAction(ActionData a)
		{
			if (base.handleAction(a))
			{
				return true;
			}
			if (a.actionName == "ACTION_SET_DRAWQUAD")
			{
				this.setDrawQuad(a.actionParam);
				return true;
			}
			return false;
		}

		// Token: 0x060001F7 RID: 503 RVA: 0x00009E9B File Offset: 0x0000809B
		public virtual BaseElement createFromXML(XMLNode xml)
		{
			throw new NotImplementedException();
		}

		// Token: 0x060001F8 RID: 504 RVA: 0x00009EA2 File Offset: 0x000080A2
		public override void dealloc()
		{
			NSObject.NSREL(this.texture);
			base.dealloc();
		}

		// Token: 0x04000140 RID: 320
		public const string ACTION_SET_DRAWQUAD = "ACTION_SET_DRAWQUAD";

		// Token: 0x04000141 RID: 321
		public Texture2D texture;

		// Token: 0x04000142 RID: 322
		public bool restoreCutTransparency;

		// Token: 0x04000143 RID: 323
		public int quadToDraw;
	}
}
