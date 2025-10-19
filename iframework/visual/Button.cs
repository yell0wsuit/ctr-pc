using System;
using CutTheRope.iframework.helpers;
using CutTheRope.ios;

namespace CutTheRope.iframework.visual
{
	// Token: 0x0200002C RID: 44
	internal class Button : BaseElement
	{
		// Token: 0x0600018D RID: 397 RVA: 0x000082E0 File Offset: 0x000064E0
		public static Button createWithTextureUpDownID(Texture2D up, Texture2D down, int bID)
		{
			Image up2 = Image.Image_create(up);
			Image down2 = Image.Image_create(down);
			return new Button().initWithUpElementDownElementandID(up2, down2, bID);
		}

		// Token: 0x0600018E RID: 398 RVA: 0x00008308 File Offset: 0x00006508
		public virtual Button initWithID(int n)
		{
			if (this.init() != null)
			{
				this.buttonID = n;
				this.state = Button.BUTTON_STATE.BUTTON_UP;
				this.touchLeftInc = 0f;
				this.touchRightInc = 0f;
				this.touchTopInc = 0f;
				this.touchBottomInc = 0f;
				this.forcedTouchZone = new Rectangle(-1f, -1f, -1f, -1f);
			}
			return this;
		}

		// Token: 0x0600018F RID: 399 RVA: 0x00008378 File Offset: 0x00006578
		public virtual Button initWithUpElementDownElementandID(BaseElement up, BaseElement down, int n)
		{
			if (this.initWithID(n) != null)
			{
				up.parentAnchor = (down.parentAnchor = 9);
				this.addChildwithID(up, 0);
				this.addChildwithID(down, 1);
				this.setState(Button.BUTTON_STATE.BUTTON_UP);
			}
			return this;
		}

		// Token: 0x06000190 RID: 400 RVA: 0x000083B9 File Offset: 0x000065B9
		public void setTouchIncreaseLeftRightTopBottom(double l, double r, double t, double b)
		{
			this.setTouchIncreaseLeftRightTopBottom((float)l, (float)r, (float)t, (float)b);
		}

		// Token: 0x06000191 RID: 401 RVA: 0x000083CA File Offset: 0x000065CA
		public virtual void setTouchIncreaseLeftRightTopBottom(float l, float r, float t, float b)
		{
			this.touchLeftInc = l;
			this.touchRightInc = r;
			this.touchTopInc = t;
			this.touchBottomInc = b;
		}

		// Token: 0x06000192 RID: 402 RVA: 0x000083E9 File Offset: 0x000065E9
		public virtual void forceTouchRect(Rectangle r)
		{
			this.forcedTouchZone = r;
		}

		// Token: 0x06000193 RID: 403 RVA: 0x000083F4 File Offset: 0x000065F4
		public virtual bool isInTouchZoneXYforTouchDown(float tx, float ty, bool td)
		{
			float num = (td ? 0f : 15f);
			if (this.forcedTouchZone.w != -1f)
			{
				return MathHelper.pointInRect(tx, ty, this.drawX + this.forcedTouchZone.x - num, this.drawY + this.forcedTouchZone.y - num, this.forcedTouchZone.w + num * 2f, this.forcedTouchZone.h + num * 2f);
			}
			return MathHelper.pointInRect(tx, ty, this.drawX - this.touchLeftInc - num, this.drawY - this.touchTopInc - num, (float)this.width + (this.touchLeftInc + this.touchRightInc) + num * 2f, (float)this.height + (this.touchTopInc + this.touchBottomInc) + num * 2f);
		}

		// Token: 0x06000194 RID: 404 RVA: 0x000084D8 File Offset: 0x000066D8
		public virtual void setState(Button.BUTTON_STATE s)
		{
			this.state = s;
			BaseElement child3 = this.getChild(0);
			BaseElement child2 = this.getChild(1);
			child3.setEnabled(s == Button.BUTTON_STATE.BUTTON_UP);
			child2.setEnabled(s == Button.BUTTON_STATE.BUTTON_DOWN);
		}

		// Token: 0x06000195 RID: 405 RVA: 0x0000850E File Offset: 0x0000670E
		public override bool onTouchDownXY(float tx, float ty)
		{
			base.onTouchDownXY(tx, ty);
			if (this.state == Button.BUTTON_STATE.BUTTON_UP && this.isInTouchZoneXYforTouchDown(tx, ty, true))
			{
				this.setState(Button.BUTTON_STATE.BUTTON_DOWN);
				return true;
			}
			return false;
		}

		// Token: 0x06000196 RID: 406 RVA: 0x00008538 File Offset: 0x00006738
		public override bool onTouchUpXY(float tx, float ty)
		{
			base.onTouchUpXY(tx, ty);
			if (this.state == Button.BUTTON_STATE.BUTTON_DOWN)
			{
				this.setState(Button.BUTTON_STATE.BUTTON_UP);
				if (this.isInTouchZoneXYforTouchDown(tx, ty, false))
				{
					if (this.delegateButtonDelegate != null)
					{
						this.delegateButtonDelegate.onButtonPressed(this.buttonID);
					}
					return true;
				}
			}
			return false;
		}

		// Token: 0x06000197 RID: 407 RVA: 0x00008585 File Offset: 0x00006785
		public override bool onTouchMoveXY(float tx, float ty)
		{
			base.onTouchMoveXY(tx, ty);
			if (this.state == Button.BUTTON_STATE.BUTTON_DOWN)
			{
				if (this.isInTouchZoneXYforTouchDown(tx, ty, false))
				{
					return true;
				}
				this.setState(Button.BUTTON_STATE.BUTTON_UP);
			}
			return false;
		}

		// Token: 0x06000198 RID: 408 RVA: 0x000085AE File Offset: 0x000067AE
		public override int addChildwithID(BaseElement c, int i)
		{
			int num = base.addChildwithID(c, i);
			c.parentAnchor = 9;
			if (i == 1)
			{
				this.width = c.width;
				this.height = c.height;
				this.setState(Button.BUTTON_STATE.BUTTON_UP);
			}
			return num;
		}

		// Token: 0x06000199 RID: 409 RVA: 0x000085E3 File Offset: 0x000067E3
		public virtual BaseElement createFromXML(XMLNode xml)
		{
			throw new NotImplementedException();
		}

		// Token: 0x04000122 RID: 290
		public const float TOUCH_MOVE_AND_UP_ZONE_INCREASE = 15f;

		// Token: 0x04000123 RID: 291
		public int buttonID;

		// Token: 0x04000124 RID: 292
		public Button.BUTTON_STATE state;

		// Token: 0x04000125 RID: 293
		public ButtonDelegate delegateButtonDelegate;

		// Token: 0x04000126 RID: 294
		public float touchLeftInc;

		// Token: 0x04000127 RID: 295
		public float touchRightInc;

		// Token: 0x04000128 RID: 296
		public float touchTopInc;

		// Token: 0x04000129 RID: 297
		public float touchBottomInc;

		// Token: 0x0400012A RID: 298
		public Rectangle forcedTouchZone;

		// Token: 0x020000AC RID: 172
		public enum BUTTON_STATE
		{
			// Token: 0x04000894 RID: 2196
			BUTTON_UP,
			// Token: 0x04000895 RID: 2197
			BUTTON_DOWN
		}
	}
}
