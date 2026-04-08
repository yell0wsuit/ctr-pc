namespace CutTheRope.Framework.Visual
{
    /// <summary>
    /// A two-state toggle button composed of two <see cref="Button"/> instances that alternate visibility on press.
    /// </summary>
    internal sealed class ToggleButton : BaseElement, IButtonDelegation
    {
        /// <inheritdoc />
        public void OnButtonPressed(ButtonId n)
        {
            if (n <= 1)
            {
                Toggle();
            }
            delegateButtonDelegate?.OnButtonPressed(buttonID);
        }

        /// <summary>
        /// Initializes the toggle button with two pairs of up/down elements and an identifier.
        /// </summary>
        /// <param name="u1">Up element for state 1.</param>
        /// <param name="d1">Down element for state 1.</param>
        /// <param name="u2">Up element for state 2.</param>
        /// <param name="d2">Down element for state 2.</param>
        /// <param name="bid">Button identifier forwarded to the delegate.</param>
        /// <returns>The initialized toggle button instance.</returns>
        public ToggleButton InitWithUpElement1DownElement1UpElement2DownElement2andID(BaseElement u1, BaseElement d1, BaseElement u2, BaseElement d2, ButtonId bid)
        {
            buttonID = bid;
            b1 = new Button().InitWithUpElementDownElementandID(u1, d1, 0);
            b2 = new Button().InitWithUpElementDownElementandID(u2, d2, 1);
            b1.parentAnchor = b2.parentAnchor = 9;
            width = b1.width;
            height = b1.height;
            _ = AddChildwithID(b1, 0);
            _ = AddChildwithID(b2, 1);
            b2.SetEnabled(false);
            b1.delegateButtonDelegate = this;
            b2.delegateButtonDelegate = this;
            return this;
        }

        /// <summary>
        /// Expands the touch zone on both internal buttons.
        /// </summary>
        /// <param name="l">Left expansion in pixels.</param>
        /// <param name="r">Right expansion in pixels.</param>
        /// <param name="t">Top expansion in pixels.</param>
        /// <param name="b">Bottom expansion in pixels.</param>
        public void SetTouchIncreaseLeftRightTopBottom(float l, float r, float t, float b)
        {
            b1.SetTouchIncreaseLeftRightTopBottom(l, r, t, b);
            b2.SetTouchIncreaseLeftRightTopBottom(l, r, t, b);
        }

        /// <summary>
        /// Toggles between the two button states.
        /// </summary>
        public void Toggle()
        {
            b1.SetEnabled(!b1.IsEnabled());
            b2.SetEnabled(!b2.IsEnabled());
        }

        /// <summary>
        /// Returns <see langword="true"/> if the toggle is in its second (on) state.
        /// </summary>
        /// <returns><see langword="true"/> when the toggle is on; otherwise <see langword="false"/>.</returns>
        public bool On()
        {
            return b2.IsEnabled();
        }

        /// <summary>
        /// Delegate notified when the toggle button is pressed.
        /// </summary>
        public IButtonDelegation delegateButtonDelegate;

        /// <summary>
        /// Identifier forwarded to the delegate upon activation.
        /// </summary>
        private ButtonId buttonID;

        /// <summary>
        /// Button for the first toggle state.
        /// </summary>
        private Button b1;

        /// <summary>
        /// Button for the second toggle state.
        /// </summary>
        private Button b2;

        /// <summary>
        /// Internal toggle state identifiers.
        /// </summary>
        private enum TOGGLE_BUTTON
        {
            /// <summary>
            /// First button face (state 1).
            /// </summary>
            BUTTON_FACE1,

            /// <summary>
            /// Second button face (state 2).
            /// </summary>
            BUTTON_FACE2
        }
    }
}
