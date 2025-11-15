namespace CutTheRope.iframework.visual
{
    internal sealed class ToggleButton : BaseElement, IButtonDelegation
    {
        public void OnButtonPressed(int n)
        {
            if (n <= 1)
            {
                Toggle();
            }
            delegateButtonDelegate?.OnButtonPressed(buttonID);
        }

        public ToggleButton InitWithUpElement1DownElement1UpElement2DownElement2andID(BaseElement u1, BaseElement d1, BaseElement u2, BaseElement d2, int bid)
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

        public void SetTouchIncreaseLeftRightTopBottom(double l, double r, double t, double b)
        {
            SetTouchIncreaseLeftRightTopBottom((float)l, (float)r, (float)t, (float)b);
        }

        public void SetTouchIncreaseLeftRightTopBottom(float l, float r, float t, float b)
        {
            b1.SetTouchIncreaseLeftRightTopBottom(l, r, t, b);
            b2.SetTouchIncreaseLeftRightTopBottom(l, r, t, b);
        }

        public void Toggle()
        {
            b1.SetEnabled(!b1.IsEnabled());
            b2.SetEnabled(!b2.IsEnabled());
        }

        public bool On()
        {
            return b2.IsEnabled();
        }

        public IButtonDelegation delegateButtonDelegate;

        private int buttonID;

        private Button b1;

        private Button b2;

        private enum TOGGLE_BUTTON
        {
            BUTTON_FACE1,
            BUTTON_FACE2
        }
    }
}
