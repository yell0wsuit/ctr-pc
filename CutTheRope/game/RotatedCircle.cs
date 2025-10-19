using System;
using CutTheRope.iframework;
using CutTheRope.iframework.core;
using CutTheRope.iframework.helpers;
using CutTheRope.iframework.visual;
using CutTheRope.ios;
using CutTheRope.windows;
using Microsoft.Xna.Framework;

namespace CutTheRope.game
{
	// Token: 0x02000091 RID: 145
	internal class RotatedCircle : BaseElement
	{
		// Token: 0x060005C3 RID: 1475 RVA: 0x0002FD7C File Offset: 0x0002DF7C
		public override NSObject init()
		{
			if (base.init() != null)
			{
				this.containedObjects = (DynamicArray)new DynamicArray().init();
				this.soundPlaying = -1;
				this.vinilStickerL = Image.Image_createWithResIDQuad(103, 2);
				this.vinilStickerL.anchor = 20;
				this.vinilStickerL.parentAnchor = 18;
				this.vinilStickerL.rotationCenterX = (float)this.vinilStickerL.width / 2f;
				this.vinilStickerR = Image.Image_createWithResIDQuad(103, 2);
				this.vinilStickerR.scaleX = -1f;
				this.vinilStickerR.anchor = 20;
				this.vinilStickerR.parentAnchor = 18;
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
				this.vinil = Image.Image_createWithResIDQuad(103, 0);
				this.vinil.anchor = 18;
				this.passColorToChilds = false;
				this.addChild(this.vinilStickerL);
				this.addChild(this.vinilStickerR);
				this.addChild(this.vinilActiveControllerL);
				this.addChild(this.vinilActiveControllerR);
				this.addChild(this.vinilControllerL);
				this.addChild(this.vinilControllerR);
			}
			return this;
		}

		// Token: 0x060005C4 RID: 1476 RVA: 0x00030000 File Offset: 0x0002E200
		public virtual void setSize(float value)
		{
			this.size = value;
			float num = this.size / 167f;
			this.vinilHighlightL.scaleX = (this.vinilHighlightL.scaleY = (this.vinilHighlightR.scaleY = num));
			this.vinilHighlightR.scaleX = 0f - num;
			this.vinil.scaleX = (this.vinil.scaleY = num);
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

		// Token: 0x060005C5 RID: 1477 RVA: 0x0003019F File Offset: 0x0002E39F
		public virtual bool hasOneHandle()
		{
			return !this.vinilControllerL.visible;
		}

		// Token: 0x060005C6 RID: 1478 RVA: 0x000301AF File Offset: 0x0002E3AF
		public virtual void setHasOneHandle(bool value)
		{
			this.vinilControllerL.visible = !value;
		}

		// Token: 0x060005C7 RID: 1479 RVA: 0x000301C0 File Offset: 0x0002E3C0
		public virtual bool isLeftControllerActive()
		{
			return this.vinilActiveControllerL.visible;
		}

		// Token: 0x060005C8 RID: 1480 RVA: 0x000301CD File Offset: 0x0002E3CD
		public virtual void setIsLeftControllerActive(bool value)
		{
			this.vinilActiveControllerL.visible = value;
		}

		// Token: 0x060005C9 RID: 1481 RVA: 0x000301DB File Offset: 0x0002E3DB
		public virtual bool isRightControllerActive()
		{
			return this.vinilActiveControllerR.visible;
		}

		// Token: 0x060005CA RID: 1482 RVA: 0x000301E8 File Offset: 0x0002E3E8
		public virtual void setIsRightControllerActive(bool value)
		{
			this.vinilActiveControllerR.visible = value;
		}

		// Token: 0x060005CB RID: 1483 RVA: 0x000301F8 File Offset: 0x0002E3F8
		public virtual bool containsSameObjectWithAnotherCircle()
		{
			foreach (object obj in this.circlesArray)
			{
				RotatedCircle item = (RotatedCircle)obj;
				if (item != this && this.containsSameObjectWithCircle(item))
				{
					return true;
				}
			}
			return false;
		}

		// Token: 0x060005CC RID: 1484 RVA: 0x00030260 File Offset: 0x0002E460
		public override void draw()
		{
			if (this.isRightControllerActive() || this.isLeftControllerActive())
			{
				OpenGL.glDisable(0);
				OpenGL.glBlendFunc(BlendingFactor.GL_ONE, BlendingFactor.GL_ONE_MINUS_SRC_ALPHA);
				RGBAColor whiteRGBA = RGBAColor.whiteRGBA;
				if ((double)this.color.a != 1.0)
				{
					whiteRGBA.a = this.color.a;
				}
				GLDrawer.drawAntialiasedCurve2(this.x, this.y, this.sizeInPixels + this.ACTIVE_CIRCLE_WIDTH * this.vinilControllerL.scaleX, 0f, 6.2831855f, 81, (this.ACTIVE_CIRCLE_WIDTH + FrameworkTypes.RTPD(1.0) * 3f) * this.vinilControllerL.scaleX, 5f, whiteRGBA);
				OpenGL.glColor4f(Color.White);
				OpenGL.glEnable(0);
			}
			this.vinilHighlightL.color = this.color;
			this.vinilHighlightR.color = this.color;
			this.vinilControllerL.color = this.color;
			this.vinilControllerR.color = this.color;
			this.vinil.color = this.color;
			this.vinil.draw();
			OpenGL.glDisable(0);
			OpenGL.glBlendFunc(BlendingFactor.GL_SRC_ALPHA, BlendingFactor.GL_ONE_MINUS_SRC_ALPHA);
			foreach (object obj in this.circlesArray)
			{
				RotatedCircle item = (RotatedCircle)obj;
				if (item != this && item.containsSameObjectWithAnotherCircle() && this.circlesArray.getObjectIndex(item) < this.circlesArray.getObjectIndex(this))
				{
					GLDrawer.drawCircleIntersection(this.x, this.y, this.sizeInPixels, item.x, item.y, item.sizeInPixels, 81, this.OUTER_CIRCLE_WIDTH * item.vinilHighlightL.scaleX * 0.5f, this.CONTOUR_COLOR);
				}
			}
			OpenGL.glBlendFunc(BlendingFactor.GL_ONE, BlendingFactor.GL_ONE_MINUS_SRC_ALPHA);
			OpenGL.glColor4f(Color.White);
			OpenGL.glEnable(0);
			this.vinilHighlightL.draw();
			this.vinilHighlightR.draw();
			base.draw();
			this.vinilCenter.draw();
		}

		// Token: 0x060005CD RID: 1485 RVA: 0x000304A4 File Offset: 0x0002E6A4
		public virtual void updateChildPositions()
		{
			this.vinil.x = (this.vinilCenter.x = this.x);
			this.vinil.y = (this.vinilCenter.y = this.y);
			float num = (float)(this.vinilHighlightL.width / 2) * (1f - this.vinilHighlightL.scaleX);
			float num2 = (float)(this.vinilHighlightL.height / 2) * (1f - this.vinilHighlightL.scaleY);
			float num3 = this.sizeInPixels - FrameworkTypes.RTPD((double)(this.CONTROLLER_SHIFT_PARAM1 - this.CONTROLLER_SHIFT_PARAM2 * this.size)) + (1f - this.vinilControllerL.scaleX) * (float)(this.vinilControllerL.width / 2);
			this.vinilHighlightL.x = this.x + num;
			this.vinilHighlightR.x = this.x - num;
			this.vinilHighlightL.y = (this.vinilHighlightR.y = this.y - num2);
			this.vinilControllerL.x = this.x - num3;
			this.vinilControllerR.x = this.x + num3;
			this.vinilControllerL.y = (this.vinilControllerR.y = this.y);
			this.vinilActiveControllerL.x = this.vinilControllerL.x;
			this.vinilActiveControllerL.y = this.vinilControllerL.y;
			this.vinilActiveControllerR.x = this.vinilControllerR.x;
			this.vinilActiveControllerR.y = this.vinilControllerR.y;
		}

		// Token: 0x060005CE RID: 1486 RVA: 0x00030658 File Offset: 0x0002E858
		public virtual bool containsSameObjectWithCircle(RotatedCircle anotherCircle)
		{
			if (this.x == anotherCircle.x && this.y == anotherCircle.y && this.size == anotherCircle.size)
			{
				return false;
			}
			foreach (object obj in this.containedObjects)
			{
				GameObject containedObject = (GameObject)obj;
				if (anotherCircle.containedObjects.getObjectIndex(containedObject) != -1)
				{
					return true;
				}
			}
			return false;
		}

		// Token: 0x060005CF RID: 1487 RVA: 0x000306EC File Offset: 0x0002E8EC
		public virtual NSObject copy()
		{
			RotatedCircle rotatedCircle = (RotatedCircle)new RotatedCircle().init();
			rotatedCircle.x = this.x;
			rotatedCircle.y = this.y;
			rotatedCircle.rotation = this.rotation;
			rotatedCircle.circlesArray = this.circlesArray;
			rotatedCircle.containedObjects = this.containedObjects;
			rotatedCircle.operating = -1;
			rotatedCircle.handle1 = CutTheRope.iframework.helpers.MathHelper.vect(rotatedCircle.x - FrameworkTypes.RTPD((double)(this.size * 3f)), rotatedCircle.y);
			rotatedCircle.handle2 = CutTheRope.iframework.helpers.MathHelper.vect(rotatedCircle.x + FrameworkTypes.RTPD((double)(this.size * 3f)), rotatedCircle.y);
			rotatedCircle.handle1 = CutTheRope.iframework.helpers.MathHelper.vectRotateAround(rotatedCircle.handle1, (double)CutTheRope.iframework.helpers.MathHelper.DEGREES_TO_RADIANS(rotatedCircle.rotation), rotatedCircle.x, rotatedCircle.y);
			rotatedCircle.handle2 = CutTheRope.iframework.helpers.MathHelper.vectRotateAround(rotatedCircle.handle2, (double)CutTheRope.iframework.helpers.MathHelper.DEGREES_TO_RADIANS(rotatedCircle.rotation), rotatedCircle.x, rotatedCircle.y);
			rotatedCircle.setSize(this.size);
			rotatedCircle.setHasOneHandle(this.hasOneHandle_);
			rotatedCircle.vinilControllerL.visible = false;
			rotatedCircle.vinilControllerR.visible = false;
			return rotatedCircle;
		}

		// Token: 0x060005D0 RID: 1488 RVA: 0x00030823 File Offset: 0x0002EA23
		public override void dealloc()
		{
			this.vinilCenter.release();
			this.vinilHighlightL.release();
			this.vinilHighlightR.release();
			this.vinil.release();
			this.containedObjects.release();
			base.dealloc();
		}

		// Token: 0x040007CB RID: 1995
		public const int PM = 3;

		// Token: 0x040007CC RID: 1996
		public const float CONTROLLER_MIN_SCALE = 0.75f;

		// Token: 0x040007CD RID: 1997
		public const float STICKER_MIN_SCALE = 0.4f;

		// Token: 0x040007CE RID: 1998
		public const float CENTER_SCALE_FACTOR = 0.5f;

		// Token: 0x040007CF RID: 1999
		public const float HUNDRED_PERCENT_SCALE_SIZE = 167f;

		// Token: 0x040007D0 RID: 2000
		public const int CIRCLE_VERTEX_COUNT = 80;

		// Token: 0x040007D1 RID: 2001
		public float size;

		// Token: 0x040007D2 RID: 2002
		public float sizeInPixels;

		// Token: 0x040007D3 RID: 2003
		public int operating;

		// Token: 0x040007D4 RID: 2004
		public int soundPlaying;

		// Token: 0x040007D5 RID: 2005
		public Vector lastTouch;

		// Token: 0x040007D6 RID: 2006
		public Vector handle1;

		// Token: 0x040007D7 RID: 2007
		public Vector handle2;

		// Token: 0x040007D8 RID: 2008
		public Vector inithanlde1;

		// Token: 0x040007D9 RID: 2009
		public Vector inithanlde2;

		// Token: 0x040007DA RID: 2010
		public DynamicArray circlesArray;

		// Token: 0x040007DB RID: 2011
		public DynamicArray containedObjects;

		// Token: 0x040007DC RID: 2012
		public bool removeOnNextUpdate;

		// Token: 0x040007DD RID: 2013
		private Image vinilStickerL;

		// Token: 0x040007DE RID: 2014
		private Image vinilStickerR;

		// Token: 0x040007DF RID: 2015
		private Image vinilHighlightL;

		// Token: 0x040007E0 RID: 2016
		private Image vinilHighlightR;

		// Token: 0x040007E1 RID: 2017
		private Image vinilControllerL;

		// Token: 0x040007E2 RID: 2018
		private Image vinilControllerR;

		// Token: 0x040007E3 RID: 2019
		private Image vinilActiveControllerL;

		// Token: 0x040007E4 RID: 2020
		private Image vinilActiveControllerR;

		// Token: 0x040007E5 RID: 2021
		private Image vinilCenter;

		// Token: 0x040007E6 RID: 2022
		private Image vinil;

		// Token: 0x040007E7 RID: 2023
		private bool hasOneHandle_;

		// Token: 0x040007E8 RID: 2024
		private RGBAColor CONTOUR_COLOR = RGBAColor.MakeRGBA(1.0, 1.0, 1.0, 0.2);

		// Token: 0x040007E9 RID: 2025
		private float INNER_CIRCLE_WIDTH = FrameworkTypes.RTPD(15.0) * 3f;

		// Token: 0x040007EA RID: 2026
		private float OUTER_CIRCLE_WIDTH = FrameworkTypes.RTPD(7.0) * 3f;

		// Token: 0x040007EB RID: 2027
		private float ACTIVE_CIRCLE_WIDTH = FrameworkTypes.RTPD(3.0) * 3f;

		// Token: 0x040007EC RID: 2028
		private float CONTROLLER_SHIFT_PARAM1 = 67.5f;

		// Token: 0x040007ED RID: 2029
		private float CONTROLLER_SHIFT_PARAM2 = 0.089999996f;
	}
}
