using CutTheRope.iframework.helpers;
using CutTheRope.ios;
using System;

namespace CutTheRope.iframework.visual
{
    internal class Button : BaseElement
    {
        public static Button createWithTextureUpDownID(CTRTexture2D up, CTRTexture2D down, int bID)
        {
            Image up2 = Image.Image_create(up);
            Image down2 = Image.Image_create(down);
            return new Button().initWithUpElementDownElementandID(up2, down2, bID);
        }

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
                this.forcedTouchZone = new CTRRectangle(-1f, -1f, -1f, -1f);
            }
            return this;
        }

        public virtual Button initWithUpElementDownElementandID(BaseElement up, BaseElement down, int n)
        {
            if (this.initWithID(n) != null)
            {
                up.parentAnchor = down.parentAnchor = 9;
                this.addChildwithID(up, 0);
                this.addChildwithID(down, 1);
                this.setState(Button.BUTTON_STATE.BUTTON_UP);
            }
            return this;
        }

        public void setTouchIncreaseLeftRightTopBottom(double l, double r, double t, double b)
        {
            this.setTouchIncreaseLeftRightTopBottom((float)l, (float)r, (float)t, (float)b);
        }

        public virtual void setTouchIncreaseLeftRightTopBottom(float l, float r, float t, float b)
        {
            this.touchLeftInc = l;
            this.touchRightInc = r;
            this.touchTopInc = t;
            this.touchBottomInc = b;
        }

        public virtual void forceTouchRect(CTRRectangle r)
        {
            this.forcedTouchZone = r;
        }

        public virtual bool isInTouchZoneXYforTouchDown(float tx, float ty, bool td)
        {
            float num = td ? 0f : 15f;
            if (this.forcedTouchZone.w != -1f)
            {
                return CTRMathHelper.pointInRect(tx, ty, this.drawX + this.forcedTouchZone.x - num, this.drawY + this.forcedTouchZone.y - num, this.forcedTouchZone.w + num * 2f, this.forcedTouchZone.h + num * 2f);
            }
            return CTRMathHelper.pointInRect(tx, ty, this.drawX - this.touchLeftInc - num, this.drawY - this.touchTopInc - num, (float)this.width + (this.touchLeftInc + this.touchRightInc) + num * 2f, (float)this.height + (this.touchTopInc + this.touchBottomInc) + num * 2f);
        }

        public virtual void setState(Button.BUTTON_STATE s)
        {
            this.state = s;
            BaseElement child3 = this.getChild(0);
            BaseElement child2 = this.getChild(1);
            child3.setEnabled(s == Button.BUTTON_STATE.BUTTON_UP);
            child2.setEnabled(s == Button.BUTTON_STATE.BUTTON_DOWN);
        }

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

        public override bool onTouchUpXY(float tx, float ty)
        {
            base.onTouchUpXY(tx, ty);
            if (this.state == Button.BUTTON_STATE.BUTTON_DOWN)
            {
                this.setState(Button.BUTTON_STATE.BUTTON_UP);
                if (this.isInTouchZoneXYforTouchDown(tx, ty, false))
                {
                    this.delegateButtonDelegate?.onButtonPressed(this.buttonID);
                    return true;
                }
            }
            return false;
        }

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

        public virtual BaseElement createFromXML(XMLNode xml)
        {
            throw new NotImplementedException();
        }

        public const float TOUCH_MOVE_AND_UP_ZONE_INCREASE = 15f;

        public int buttonID;

        public Button.BUTTON_STATE state;

        public ButtonDelegate delegateButtonDelegate;

        public float touchLeftInc;

        public float touchRightInc;

        public float touchTopInc;

        public float touchBottomInc;

        public CTRRectangle forcedTouchZone;

        public enum BUTTON_STATE
        {
            BUTTON_UP,
            BUTTON_DOWN
        }
    }
}
