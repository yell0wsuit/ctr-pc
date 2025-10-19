using System;
using System.Collections.Generic;
using CutTheRope.iframework;
using CutTheRope.iframework.core;
using CutTheRope.iframework.helpers;
using CutTheRope.iframework.visual;
using CutTheRope.ios;
using CutTheRope.windows;
using Microsoft.Xna.Framework;

namespace CutTheRope.game
{
	// Token: 0x02000092 RID: 146
	internal class RotatedCircle2 : BaseElement
	{
		// Token: 0x060005D2 RID: 1490 RVA: 0x0003090C File Offset: 0x0002EB0C
		public virtual void setSize(float value)
		{
			this.size = value;
			float num = this.size / ((float)this.vinilTL.width + (float)this.vinilTR.width * (1f - this.vinilTL.scaleX));
			this.vinilHighlightL.scaleX = (this.vinilHighlightL.scaleY = (this.vinilHighlightR.scaleY = num));
			this.vinilHighlightR.scaleX = 0f - num;
			this.vinilBL.scaleX = (this.vinilBL.scaleY = (this.vinilBR.scaleY = num));
			this.vinilBR.scaleX = 0f - num;
			this.vinilTL.scaleX = num;
			this.vinilTL.scaleY = 0f - num;
			this.vinilTR.scaleX = (this.vinilTR.scaleY = 0f - num);
			float num2 = ((num >= 0.4f) ? num : 0.4f);
			this.vinilStickerL.scaleX = (this.vinilStickerL.scaleY = (this.vinilStickerR.scaleY = num2));
			this.vinilStickerR.scaleX = 0f - num2;
			float num3 = ((num >= 0.75f) ? num : 0.75f);
			this.vinilControllerL.scaleX = (this.vinilControllerL.scaleY = (this.vinilControllerR.scaleX = (this.vinilControllerR.scaleY = num3)));
			this.vinilActiveControllerL.scaleX = (this.vinilActiveControllerL.scaleY = (this.vinilActiveControllerR.scaleX = (this.vinilActiveControllerR.scaleY = num3)));
			this.vinilCenter.scaleX = 1f - (1f - this.vinilStickerL.scaleX) * 0.5f;
			this.vinilCenter.scaleY = this.vinilCenter.scaleX;
			this.sizeInPixels = (float)this.vinilHighlightL.width * this.vinilHighlightL.scaleX;
			this.updateChildPositions();
		}

		// Token: 0x060005D3 RID: 1491 RVA: 0x00030B2F File Offset: 0x0002ED2F
		public virtual void setHasOneHandle(bool value)
		{
			this.vinilControllerL.visible = !value;
		}

		// Token: 0x060005D4 RID: 1492 RVA: 0x00030B40 File Offset: 0x0002ED40
		public virtual bool hasOneHandle()
		{
			return !this.vinilControllerL.visible;
		}

		// Token: 0x060005D5 RID: 1493 RVA: 0x00030B50 File Offset: 0x0002ED50
		public virtual void setIsLeftControllerActive(bool value)
		{
			this.vinilActiveControllerL.visible = value;
		}

		// Token: 0x060005D6 RID: 1494 RVA: 0x00030B5E File Offset: 0x0002ED5E
		public virtual bool isLeftControllerActive()
		{
			return this.vinilActiveControllerL.visible;
		}

		// Token: 0x060005D7 RID: 1495 RVA: 0x00030B6B File Offset: 0x0002ED6B
		public virtual void setIsRightControllerActive(bool value)
		{
			this.vinilActiveControllerR.visible = value;
		}

		// Token: 0x060005D8 RID: 1496 RVA: 0x00030B79 File Offset: 0x0002ED79
		public virtual bool isRightControllerActive()
		{
			return this.vinilActiveControllerR.visible;
		}

		// Token: 0x060005D9 RID: 1497 RVA: 0x00030B88 File Offset: 0x0002ED88
		public virtual bool containsSameObjectWithAnotherCircle()
		{
			for (int i = 0; i < this.circlesArray.count(); i++)
			{
				RotatedCircle2 rotatedCircle = (RotatedCircle2)this.circlesArray[i];
				if (rotatedCircle != this && this.containsSameObjectWithCircle(rotatedCircle))
				{
					return true;
				}
			}
			return false;
		}

		// Token: 0x060005DA RID: 1498 RVA: 0x00030BD0 File Offset: 0x0002EDD0
		public override NSObject init()
		{
			if (base.init() != null)
			{
				this.circlesArray = null;
				this.containedObjects = new List<BaseElement>();
				this.soundPlaying = -1;
				this.vinilStickerL = Image.Image_createWithResIDQuad(103, 2);
				this.vinilStickerL.anchor = 20;
				this.vinilStickerL.rotationCenterX = (float)this.vinilStickerL.width / 2f;
				this.vinilStickerR = Image.Image_createWithResIDQuad(103, 2);
				this.vinilStickerR.scaleX = -1f;
				this.vinilStickerR.anchor = 20;
				this.vinilStickerR.rotationCenterX = (float)this.vinilStickerR.width / 2f;
				this.vinilCenter = Image.Image_createWithResIDQuad(103, 3);
				this.vinilCenter.anchor = 18;
				this.vinilHighlightL = Image.Image_createWithResIDQuad(103, 1);
				this.vinilHighlightL.anchor = 12;
				this.vinilHighlightR = Image.Image_createWithResIDQuad(103, 1);
				this.vinilHighlightR.scaleX = -1f;
				this.vinilHighlightR.anchor = 9;
				this.vinilControllerL = Image.Image_createWithResIDQuad(103, 5);
				this.vinilControllerL.anchor = 18;
				this.vinilControllerL.rotation = 90f;
				this.vinilControllerR = Image.Image_createWithResIDQuad(103, 5);
				this.vinilControllerR.anchor = 18;
				this.vinilControllerR.rotation = -90f;
				this.vinilActiveControllerL = Image.Image_createWithResIDQuad(103, 4);
				this.vinilActiveControllerL.anchor = this.vinilControllerL.anchor;
				this.vinilActiveControllerL.rotation = this.vinilControllerL.rotation;
				this.vinilActiveControllerL.visible = false;
				this.vinilActiveControllerR = Image.Image_createWithResIDQuad(103, 4);
				this.vinilActiveControllerR.anchor = this.vinilControllerR.anchor;
				this.vinilActiveControllerR.rotation = this.vinilControllerR.rotation;
				this.vinilActiveControllerR.visible = false;
				this.vinilBL = Image.Image_createWithResIDQuad(103, 0);
				this.vinilBL.anchor = 12;
				this.vinilBR = Image.Image_createWithResIDQuad(103, 0);
				this.vinilBR.scaleX = -1f;
				this.vinilBR.anchor = 9;
				this.vinilTL = Image.Image_createWithResIDQuad(103, 0);
				this.vinilTL.scaleY = -1f;
				this.vinilTL.anchor = 36;
				this.vinilTR = Image.Image_createWithResIDQuad(103, 0);
				this.vinilTR.scaleX = (this.vinilTR.scaleY = -1f);
				this.vinilTR.anchor = 33;
				this.passColorToChilds = false;
				this.addChild(this.vinilActiveControllerL);
				this.addChild(this.vinilActiveControllerR);
				this.addChild(this.vinilControllerL);
				this.addChild(this.vinilControllerR);
			}
			return this;
		}

		// Token: 0x060005DB RID: 1499 RVA: 0x00030EAC File Offset: 0x0002F0AC
		public virtual NSObject copy()
		{
			RotatedCircle2 rotatedCircle = (RotatedCircle2)new RotatedCircle2().init();
			rotatedCircle.x = this.x;
			rotatedCircle.y = this.y;
			rotatedCircle.rotation = this.rotation;
			rotatedCircle.circlesArray = this.circlesArray;
			rotatedCircle.containedObjects = this.containedObjects;
			rotatedCircle.operating = -1;
			rotatedCircle.handle1 = new Vector(rotatedCircle.x - this.size, rotatedCircle.y);
			rotatedCircle.handle2 = new Vector(rotatedCircle.x + this.size, rotatedCircle.y);
			rotatedCircle.handle1 = CutTheRope.iframework.helpers.MathHelper.vectRotateAround(rotatedCircle.handle1, (double)CutTheRope.iframework.helpers.MathHelper.DEGREES_TO_RADIANS(rotatedCircle.rotation), rotatedCircle.x, rotatedCircle.y);
			rotatedCircle.handle2 = CutTheRope.iframework.helpers.MathHelper.vectRotateAround(rotatedCircle.handle2, (double)CutTheRope.iframework.helpers.MathHelper.DEGREES_TO_RADIANS(rotatedCircle.rotation), rotatedCircle.x, rotatedCircle.y);
			rotatedCircle.setSize(this.size);
			rotatedCircle.setHasOneHandle(this.hasOneHandle());
			rotatedCircle.vinilControllerL.visible = false;
			rotatedCircle.vinilControllerR.visible = false;
			return rotatedCircle;
		}

		// Token: 0x060005DC RID: 1500 RVA: 0x00030FCC File Offset: 0x0002F1CC
		public override void draw()
		{
			if (this.isRightControllerActive() || this.isLeftControllerActive())
			{
				OpenGL.glDisable(0);
				OpenGL.glBlendFunc(BlendingFactor.GL_ONE, BlendingFactor.GL_ONE_MINUS_SRC_ALPHA);
				GLDrawer.drawAntialiasedCurve2(this.x, this.y, this.sizeInPixels + 3f * Math.Abs(this.vinilTR.scaleX), 0f, 6.2831855f, 51, 2f, 1f * Math.Abs(this.vinilTR.scaleX), RGBAColor.whiteRGBA);
			}
			OpenGL.glEnable(0);
			OpenGL.glBlendFunc(BlendingFactor.GL_ONE, BlendingFactor.GL_ONE_MINUS_SRC_ALPHA);
			this.vinilTL.color = (this.vinilTR.color = (this.vinilBL.color = (this.vinilBR.color = RGBAColor.solidOpaqueRGBA)));
			this.vinilTL.draw();
			this.vinilTR.draw();
			this.vinilBL.draw();
			this.vinilBR.draw();
			OpenGL.glDisable(0);
			OpenGL.glBlendFunc(BlendingFactor.GL_SRC_ALPHA, BlendingFactor.GL_ONE_MINUS_SRC_ALPHA);
			if (this.isRightControllerActive() || this.isLeftControllerActive() || (double)this.color.a < 1.0)
			{
				RGBAColor whiteRGBA = RGBAColor.whiteRGBA;
				whiteRGBA.a = 1f - this.color.a;
				GLDrawer.drawAntialiasedCurve2(this.x, this.y, this.sizeInPixels + 1f, 0f, 6.2831855f, 51, 2f, 1f * Math.Abs(this.vinilTR.scaleX), whiteRGBA);
			}
			for (int i = 0; i < this.circlesArray.count(); i++)
			{
				RotatedCircle2 rotatedCircle = (RotatedCircle2)this.circlesArray[i];
				if (rotatedCircle != this && rotatedCircle.containsSameObjectWithAnotherCircle() && this.circlesArray.getObjectIndex(rotatedCircle) < this.circlesArray.getObjectIndex(this))
				{
					GLDrawer.drawCircleIntersection(this.x, this.y, this.sizeInPixels, rotatedCircle.x, rotatedCircle.y, rotatedCircle.sizeInPixels, 51, 7f * rotatedCircle.vinilHighlightL.scaleX * 0.5f, this.CONTOUR_COLOR);
				}
			}
			OpenGL.glEnable(0);
			OpenGL.glBlendFunc(BlendingFactor.GL_ONE, BlendingFactor.GL_ONE_MINUS_SRC_ALPHA);
			OpenGL.glColor4f(Color.White);
			OpenGL.glEnable(0);
			this.vinilHighlightL.color = this.color;
			this.vinilHighlightR.color = this.color;
			this.vinilHighlightL.draw();
			this.vinilHighlightR.draw();
			this.vinilStickerL.x = (this.vinilStickerR.x = this.x);
			this.vinilStickerL.y = (this.vinilStickerR.y = this.y);
			this.vinilStickerL.rotation = (this.vinilStickerR.rotation = this.rotation);
			this.vinilStickerL.draw();
			this.vinilStickerR.draw();
			OpenGL.glDisable(0);
			OpenGL.glBlendFunc(BlendingFactor.GL_ONE, BlendingFactor.GL_ONE_MINUS_SRC_ALPHA);
			GLDrawer.drawAntialiasedCurve2(this.x, this.y, (float)this.vinilStickerL.width * this.vinilStickerL.scaleX, 0f, 6.2831855f, 51, 1f, this.vinilStickerL.scaleX * 1.5f, this.INNER_CIRCLE_COLOR1);
			GLDrawer.drawAntialiasedCurve2(this.x, this.y, (float)(this.vinilStickerL.width - 2) * this.vinilStickerL.scaleX, 0f, 6.2831855f, 51, 0f, this.vinilStickerL.scaleX * 1f, this.INNER_CIRCLE_COLOR2);
			OpenGL.glColor4f(Color.White);
			OpenGL.glEnable(0);
			this.vinilControllerL.color = this.color;
			this.vinilControllerR.color = this.color;
			base.draw();
			this.vinilCenter.draw();
		}

		// Token: 0x060005DD RID: 1501 RVA: 0x000313D0 File Offset: 0x0002F5D0
		public override void dealloc()
		{
			this.vinilCenter = null;
			this.vinilHighlightL = null;
			this.vinilHighlightR = null;
			this.vinilBL = null;
			this.vinilBR = null;
			this.vinilTL = null;
			this.vinilTR = null;
			this.vinilStickerL = null;
			this.vinilStickerR = null;
			this.containedObjects.Clear();
			this.containedObjects = null;
			base.dealloc();
		}

		// Token: 0x060005DE RID: 1502 RVA: 0x00031434 File Offset: 0x0002F634
		public virtual void updateChildPositions()
		{
			this.vinilCenter.x = this.x;
			this.vinilCenter.y = this.y;
			float num = (float)(this.vinilHighlightL.width / 2) * (1f - this.vinilHighlightL.scaleX);
			float num2 = (float)(this.vinilHighlightL.height / 2) * (1f - this.vinilHighlightL.scaleY);
			float num3 = (float)(this.vinilBL.width + 4) / 2f * (1f - this.vinilBL.scaleX);
			float num4 = (float)(this.vinilBL.height + 4) / 2f * (1f - this.vinilBL.scaleY);
			float num5 = ((Math.Abs(this.vinilControllerR.scaleX) < 1f) ? ((1f - Math.Abs(this.vinilControllerR.scaleX)) * 10f) : 0f);
			float num6 = ((Math.Abs(this.vinilTL.scaleX) < 0.45f) ? ((0.45f - Math.Abs(this.vinilTL.scaleX)) * 10f + 1f) : 0f);
			float num7 = Math.Abs((float)this.vinilBL.height * this.vinilBL.scaleY) - Math.Abs((float)this.vinilControllerR.height * 0.58f * this.vinilControllerR.scaleY / 2f) - num5 - num6;
			this.vinilHighlightL.x = this.x + num;
			this.vinilHighlightR.x = this.x - num;
			this.vinilHighlightL.y = (this.vinilHighlightR.y = this.y - num2);
			this.vinilBL.x = (this.vinilTL.x = this.x + num3);
			this.vinilBL.y = (this.vinilBR.y = this.y - num4);
			this.vinilBR.x = (this.vinilTR.x = this.x - num3);
			this.vinilTL.y = (this.vinilTR.y = this.y + num4);
			this.vinilControllerL.x = this.x - num7;
			this.vinilControllerR.x = this.x + num7;
			this.vinilControllerL.y = (this.vinilControllerR.y = this.y);
			this.vinilActiveControllerL.x = this.vinilControllerL.x;
			this.vinilActiveControllerL.y = this.vinilControllerL.y;
			this.vinilActiveControllerR.x = this.vinilControllerR.x;
			this.vinilActiveControllerR.y = this.vinilControllerR.y;
		}

		// Token: 0x060005DF RID: 1503 RVA: 0x00031738 File Offset: 0x0002F938
		public virtual bool containsSameObjectWithCircle(RotatedCircle2 anotherCircle)
		{
			if (this.x == anotherCircle.x && this.y == anotherCircle.y && this.size == anotherCircle.size)
			{
				return false;
			}
			for (int i = 0; i < this.containedObjects.Count; i++)
			{
				GameObject item = (GameObject)this.containedObjects[i];
				if (anotherCircle.containedObjects.IndexOf(item) != -1)
				{
					return true;
				}
			}
			return false;
		}

		// Token: 0x040007EE RID: 2030
		private const float CONTROLLER_MIN_SCALE = 0.75f;

		// Token: 0x040007EF RID: 2031
		private const float STICKER_MIN_SCALE = 0.4f;

		// Token: 0x040007F0 RID: 2032
		private const float CENTER_SCALE_FACTOR = 0.5f;

		// Token: 0x040007F1 RID: 2033
		private const float HUNDRED_PERCENT_SCALE_SIZE = 160f;

		// Token: 0x040007F2 RID: 2034
		private const int CIRCLE_VERTEX_COUNT = 50;

		// Token: 0x040007F3 RID: 2035
		private const float INNER_CIRCLE_WIDTH = 15f;

		// Token: 0x040007F4 RID: 2036
		private const float OUTER_CIRCLE_WIDTH = 7f;

		// Token: 0x040007F5 RID: 2037
		private const float ACTIVE_CIRCLE_WIDTH = 2f;

		// Token: 0x040007F6 RID: 2038
		private const float CONTROLLER_RATIO_PARAM = 0.58f;

		// Token: 0x040007F7 RID: 2039
		private const float CIRCLE_CONTROLLER_OFFS = 10f;

		// Token: 0x040007F8 RID: 2040
		private RGBAColor CIRCLE_COLOR1 = RGBAColor.MakeRGBA(0.306, 0.298, 0.454, 1.0);

		// Token: 0x040007F9 RID: 2041
		private RGBAColor CIRCLE_COLOR2 = RGBAColor.MakeRGBA(0.239, 0.231, 0.356, 1.0);

		// Token: 0x040007FA RID: 2042
		private RGBAColor CIRCLE_COLOR3 = RGBAColor.MakeRGBA(0.29, 0.286, 0.419, 1.0);

		// Token: 0x040007FB RID: 2043
		private RGBAColor INNER_CIRCLE_COLOR1 = RGBAColor.MakeRGBA(0.6901960784313725, 0.4196078431372549, 0.07450980392156863, 1.0);

		// Token: 0x040007FC RID: 2044
		private RGBAColor INNER_CIRCLE_COLOR2 = RGBAColor.MakeRGBA(0.9294117647058824, 0.611764705882353, 0.07450980392156863, 1.0);

		// Token: 0x040007FD RID: 2045
		private RGBAColor CONTOUR_COLOR = RGBAColor.MakeRGBA(1.0, 1.0, 1.0, 0.2);

		// Token: 0x040007FE RID: 2046
		public float size;

		// Token: 0x040007FF RID: 2047
		public float sizeInPixels;

		// Token: 0x04000800 RID: 2048
		public int operating;

		// Token: 0x04000801 RID: 2049
		public int soundPlaying;

		// Token: 0x04000802 RID: 2050
		public Vector lastTouch;

		// Token: 0x04000803 RID: 2051
		public Vector handle1;

		// Token: 0x04000804 RID: 2052
		public Vector handle2;

		// Token: 0x04000805 RID: 2053
		public Vector inithanlde1;

		// Token: 0x04000806 RID: 2054
		public Vector inithanlde2;

		// Token: 0x04000807 RID: 2055
		public DynamicArray circlesArray;

		// Token: 0x04000808 RID: 2056
		public List<BaseElement> containedObjects;

		// Token: 0x04000809 RID: 2057
		public bool removeOnNextUpdate;

		// Token: 0x0400080A RID: 2058
		private Image vinilStickerL;

		// Token: 0x0400080B RID: 2059
		private Image vinilStickerR;

		// Token: 0x0400080C RID: 2060
		private Image vinilHighlightL;

		// Token: 0x0400080D RID: 2061
		private Image vinilHighlightR;

		// Token: 0x0400080E RID: 2062
		private Image vinilControllerL;

		// Token: 0x0400080F RID: 2063
		private Image vinilControllerR;

		// Token: 0x04000810 RID: 2064
		private Image vinilActiveControllerL;

		// Token: 0x04000811 RID: 2065
		private Image vinilActiveControllerR;

		// Token: 0x04000812 RID: 2066
		private Image vinilCenter;

		// Token: 0x04000813 RID: 2067
		private Image vinilTL;

		// Token: 0x04000814 RID: 2068
		private Image vinilTR;

		// Token: 0x04000815 RID: 2069
		private Image vinilBL;

		// Token: 0x04000816 RID: 2070
		private Image vinilBR;
	}
}
