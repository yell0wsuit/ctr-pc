using System;
using System.Xml.Linq;

namespace CutTheRopeDX.Framework.Visual
{
    /// <summary>
    /// A <see cref="BaseElement"/> with up/down states, touch handling, and delegate-based press notification.
    /// </summary>
    internal class Button : BaseElement
    {
        /// <summary>
        /// Creates a button using separate <paramref name="up"/>/<paramref name="down"/> textures and assigns the provided identifier.
        /// </summary>
        /// <param name="up">Texture for the unpressed state.</param>
        /// <param name="down">Texture for the pressed state.</param>
        /// <param name="bID">Typed button identifier.</param>
        /// <returns>A new <see cref="Button"/> initialized with the given textures and identifier.</returns>
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
        /// <returns>This button instance for chaining.</returns>
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
        /// Initializes the button with separate elements for <paramref name="up"/>/<paramref name="down"/> states and an identifier.
        /// </summary>
        /// <param name="up">Element to render while the button is up.</param>
        /// <param name="down">Element to render while the button is pressed.</param>
        /// <param name="n">Typed button identifier.</param>
        /// <returns>This button instance for chaining.</returns>
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

        /// <summary>
        /// Expands the touch zone by the specified amounts on each side.
        /// </summary>
        /// <param name="l">Left expansion in pixels.</param>
        /// <param name="r">Right expansion in pixels.</param>
        /// <param name="t">Top expansion in pixels.</param>
        /// <param name="b">Bottom expansion in pixels.</param>
        public virtual void SetTouchIncreaseLeftRightTopBottom(float l, float r, float t, float b)
        {
            touchLeftInc = l;
            touchRightInc = r;
            touchTopInc = t;
            touchBottomInc = b;
        }

        /// <summary>
        /// Overrides the default touch zone with a fixed rectangle.
        /// </summary>
        /// <param name="r">Rectangle defining the forced touch zone.</param>
        public virtual void ForceTouchRect(CTRRectangle r)
        {
            forcedTouchZone = r;
        }

        /// <summary>
        /// Tests whether the touch point is within the button's touch zone.
        /// </summary>
        /// <param name="tx">Touch X coordinate.</param>
        /// <param name="ty">Touch Y coordinate.</param>
        /// <param name="td"><see langword="true"/> for touch-down (no padding); <see langword="false"/> for move/up (adds padding).</param>
        /// <returns><see langword="true"/> if the touch point lies within the button's touch zone.</returns>
        public virtual bool IsInTouchZoneXYforTouchDown(float tx, float ty, bool td)
        {
            float touchPadding = td ? 0f : 15f;
            return forcedTouchZone.w != -1f
                ? PointInRect(tx, ty, drawX + forcedTouchZone.x - touchPadding, drawY + forcedTouchZone.y - touchPadding, forcedTouchZone.w + (touchPadding * 2f), forcedTouchZone.h + (touchPadding * 2f))
                : PointInRect(tx, ty, drawX - touchLeftInc - touchPadding, drawY - touchTopInc - touchPadding, width + (touchLeftInc + touchRightInc) + (touchPadding * 2f), height + (touchTopInc + touchBottomInc) + (touchPadding * 2f));
        }

        /// <summary>
        /// Sets the button state and toggles visibility of up/down child elements.
        /// </summary>
        /// <param name="s">New button state.</param>
        public virtual void SetState(BUTTON_STATE s)
        {
            state = s;
            BaseElement child3 = GetChild(0);
            BaseElement child2 = GetChild(1);
            child3.SetEnabled(s == BUTTON_STATE.BUTTON_UP);
            child2.SetEnabled(s == BUTTON_STATE.BUTTON_DOWN);
        }

        /// <inheritdoc />
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

        /// <inheritdoc />
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

        /// <inheritdoc />
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

        /// <inheritdoc />
        public override int AddChildwithID(BaseElement c, int i)
        {
            int childId = base.AddChildwithID(c, i);
            c.parentAnchor = 9;
            if (i == 1)
            {
                width = c.width;
                height = c.height;
                SetState(BUTTON_STATE.BUTTON_UP);
            }
            return childId;
        }

        /// <summary>
        /// Creates a <see cref="BaseElement"/> from an XML definition. Not implemented in this class.
        /// </summary>
        /// <param name="xml">XML element to create from.</param>
        /// <returns>The created element.</returns>
        /// <exception cref="NotImplementedException">Always thrown by this base implementation.</exception>
        public virtual BaseElement CreateFromXML(XElement xml)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Extra padding in pixels added to the touch zone on move and up events.
        /// </summary>
        public const float TOUCH_MOVE_AND_UP_ZONE_INCREASE = 15f;

        /// <summary>
        /// Identifier forwarded to the delegate upon activation.
        /// </summary>
        public ButtonId buttonID;

        /// <summary>
        /// Current press state of the button.
        /// </summary>
        public BUTTON_STATE state;

        /// <summary>
        /// Delegate notified when the button is pressed.
        /// </summary>
        public IButtonDelegation delegateButtonDelegate;

        /// <summary>
        /// Touch zone expansion on the left side in pixels.
        /// </summary>
        public float touchLeftInc;

        /// <summary>
        /// Touch zone expansion on the right side in pixels.
        /// </summary>
        public float touchRightInc;

        /// <summary>
        /// Touch zone expansion on the top side in pixels.
        /// </summary>
        public float touchTopInc;

        /// <summary>
        /// Touch zone expansion on the bottom side in pixels.
        /// </summary>
        public float touchBottomInc;

        /// <summary>
        /// Forced touch zone rectangle, or (-1,-1,-1,-1) to use the default.
        /// </summary>
        public CTRRectangle forcedTouchZone;

        /// <summary>
        /// Represents the press state of a button.
        /// </summary>
        public enum BUTTON_STATE
        {
            /// <summary>
            /// Button is in the unpressed state.
            /// </summary>
            BUTTON_UP,

            /// <summary>
            /// Button is in the pressed state.
            /// </summary>
            BUTTON_DOWN
        }
    }
}
