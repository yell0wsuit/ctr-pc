using System;
using System.Xml.Linq;

namespace CutTheRope.Framework.Visual
{
    internal class Button : BaseElement
    {
        /// <summary>
        /// Creates a button using separate up/down textures and assigns the provided identifier.
        /// </summary>
        /// <param name="up">Texture for the unpressed state.</param>
        /// <param name="down">Texture for the pressed state.</param>
        /// <param name="bID">Typed button identifier.</param>
        public static Button CreateWithTextureUpDownID(CTRTexture2D up, CTRTexture2D down, ButtonId bID)
        {
            Image up2 = Image.Image_create(up);
            Image down2 = Image.Image_create(down);
            return new Button().InitWithUpElementDownElementandID(up2, down2, bID);
        }

        /// <summary>
        /// Initializes the button with its identifier.
        /// </summary>
        /// <param name="n">Typed button identifier.</param>
        public virtual Button InitWithID(ButtonId n)
        {
            buttonID = n;
            state = BUTTON_STATE.BUTTON_UP;
            touchLeftInc = 0f;
            touchRightInc = 0f;
            touchTopInc = 0f;
            touchBottomInc = 0f;
            forcedTouchZone = new CTRRectangle(-1f, -1f, -1f, -1f);
            return this;
        }

        /// <summary>
        /// Initializes the button with separate elements for up/down states and an identifier.
        /// </summary>
        /// <param name="up">Element to render while the button is up.</param>
        /// <param name="down">Element to render while the button is pressed.</param>
        /// <param name="n">Typed button identifier.</param>
        public virtual Button InitWithUpElementDownElementandID(BaseElement up, BaseElement down, ButtonId n)
        {
            if (InitWithID(n) != null)
            {
                up.parentAnchor = down.parentAnchor = 9;
                _ = AddChildwithID(up, 0);
                _ = AddChildwithID(down, 1);
                SetState(BUTTON_STATE.BUTTON_UP);
            }
            return this;
        }

        public void SetTouchIncreaseLeftRightTopBottom(double l, double r, double t, double b)
        {
            SetTouchIncreaseLeftRightTopBottom((float)l, (float)r, (float)t, (float)b);
        }

        public virtual void SetTouchIncreaseLeftRightTopBottom(float l, float r, float t, float b)
        {
            touchLeftInc = l;
            touchRightInc = r;
            touchTopInc = t;
            touchBottomInc = b;
        }

        public virtual void ForceTouchRect(CTRRectangle r)
        {
            forcedTouchZone = r;
        }

        public virtual bool IsInTouchZoneXYforTouchDown(float tx, float ty, bool td)
        {
            float num = td ? 0f : 15f;
            return forcedTouchZone.w != -1f
                ? PointInRect(tx, ty, drawX + forcedTouchZone.x - num, drawY + forcedTouchZone.y - num, forcedTouchZone.w + (num * 2f), forcedTouchZone.h + (num * 2f))
                : PointInRect(tx, ty, drawX - touchLeftInc - num, drawY - touchTopInc - num, width + (touchLeftInc + touchRightInc) + (num * 2f), height + (touchTopInc + touchBottomInc) + (num * 2f));
        }

        public virtual void SetState(BUTTON_STATE s)
        {
            state = s;
            BaseElement child3 = GetChild(0);
            BaseElement child2 = GetChild(1);
            child3.SetEnabled(s == BUTTON_STATE.BUTTON_UP);
            child2.SetEnabled(s == BUTTON_STATE.BUTTON_DOWN);
        }

        public override bool OnTouchDownXY(float tx, float ty)
        {
            _ = base.OnTouchDownXY(tx, ty);
            if (state == BUTTON_STATE.BUTTON_UP && IsInTouchZoneXYforTouchDown(tx, ty, true))
            {
                SetState(BUTTON_STATE.BUTTON_DOWN);
                return true;
            }
            return false;
        }

        public override bool OnTouchUpXY(float tx, float ty)
        {
            _ = base.OnTouchUpXY(tx, ty);
            if (state == BUTTON_STATE.BUTTON_DOWN)
            {
                SetState(BUTTON_STATE.BUTTON_UP);
                if (IsInTouchZoneXYforTouchDown(tx, ty, false))
                {
                    delegateButtonDelegate?.OnButtonPressed(buttonID);
                    return true;
                }
            }
            return false;
        }

        public override bool OnTouchMoveXY(float tx, float ty)
        {
            _ = base.OnTouchMoveXY(tx, ty);
            if (state == BUTTON_STATE.BUTTON_DOWN)
            {
                if (IsInTouchZoneXYforTouchDown(tx, ty, false))
                {
                    return true;
                }
                SetState(BUTTON_STATE.BUTTON_UP);
            }
            return false;
        }

        public override int AddChildwithID(BaseElement c, int i)
        {
            int num = base.AddChildwithID(c, i);
            c.parentAnchor = 9;
            if (i == 1)
            {
                width = c.width;
                height = c.height;
                SetState(BUTTON_STATE.BUTTON_UP);
            }
            return num;
        }

        public virtual BaseElement CreateFromXML(XElement xml)
        {
            throw new NotImplementedException();
        }

        public const float TOUCH_MOVE_AND_UP_ZONE_INCREASE = 15f;

        /// <summary>
        /// Identifier forwarded to the delegate upon activation.
        /// </summary>
        public ButtonId buttonID;

        public BUTTON_STATE state;

        public IButtonDelegation delegateButtonDelegate;

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
