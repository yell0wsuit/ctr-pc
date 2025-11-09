using System;

namespace CutTheRope.iframework.visual
{
    internal class ToggleButton : BaseElement, ButtonDelegate
    {
        public virtual void onButtonPressed(int n)
        {
            if (n <= 1)
            {
                this.toggle();
            }
            this.delegateButtonDelegate?.onButtonPressed(this.buttonID);
        }

        public ToggleButton initWithUpElement1DownElement1UpElement2DownElement2andID(BaseElement u1, BaseElement d1, BaseElement u2, BaseElement d2, int bid)
        {
            if (this.init() != null)
            {
                this.buttonID = bid;
                this.b1 = new Button().initWithUpElementDownElementandID(u1, d1, 0);
                this.b2 = new Button().initWithUpElementDownElementandID(u2, d2, 1);
                this.b1.parentAnchor = this.b2.parentAnchor = 9;
                this.width = this.b1.width;
                this.height = this.b1.height;
                this.addChildwithID(this.b1, 0);
                this.addChildwithID(this.b2, 1);
                this.b2.setEnabled(false);
                this.b1.delegateButtonDelegate = this;
                this.b2.delegateButtonDelegate = this;
            }
            return this;
        }

        public void setTouchIncreaseLeftRightTopBottom(double l, double r, double t, double b)
        {
            this.setTouchIncreaseLeftRightTopBottom((float)l, (float)r, (float)t, (float)b);
        }

        public void setTouchIncreaseLeftRightTopBottom(float l, float r, float t, float b)
        {
            this.b1.setTouchIncreaseLeftRightTopBottom(l, r, t, b);
            this.b2.setTouchIncreaseLeftRightTopBottom(l, r, t, b);
        }

        public void toggle()
        {
            this.b1.setEnabled(!this.b1.isEnabled());
            this.b2.setEnabled(!this.b2.isEnabled());
        }

        public bool on()
        {
            return this.b2.isEnabled();
        }

        public ButtonDelegate delegateButtonDelegate;

        private int buttonID;

        private Button b1;

        private Button b2;

        private enum TOGGLE_BUTTON
        {
            TOGGLE_BUTTON_FACE1,
            TOGGLE_BUTTON_FACE2
        }
    }
}
