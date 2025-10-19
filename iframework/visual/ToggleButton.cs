using System;

namespace CutTheRope.iframework.visual
{
	// Token: 0x02000050 RID: 80
	internal class ToggleButton : BaseElement, ButtonDelegate
	{
		// Token: 0x060002AE RID: 686 RVA: 0x0000F41B File Offset: 0x0000D61B
		public virtual void onButtonPressed(int n)
		{
			if (n <= 1)
			{
				this.toggle();
			}
			if (this.delegateButtonDelegate != null)
			{
				this.delegateButtonDelegate.onButtonPressed(this.buttonID);
			}
		}

		// Token: 0x060002AF RID: 687 RVA: 0x0000F440 File Offset: 0x0000D640
		public ToggleButton initWithUpElement1DownElement1UpElement2DownElement2andID(BaseElement u1, BaseElement d1, BaseElement u2, BaseElement d2, int bid)
		{
			if (this.init() != null)
			{
				this.buttonID = bid;
				this.b1 = new Button().initWithUpElementDownElementandID(u1, d1, 0);
				this.b2 = new Button().initWithUpElementDownElementandID(u2, d2, 1);
				this.b1.parentAnchor = (this.b2.parentAnchor = 9);
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

		// Token: 0x060002B0 RID: 688 RVA: 0x0000F505 File Offset: 0x0000D705
		public void setTouchIncreaseLeftRightTopBottom(double l, double r, double t, double b)
		{
			this.setTouchIncreaseLeftRightTopBottom((float)l, (float)r, (float)t, (float)b);
		}

		// Token: 0x060002B1 RID: 689 RVA: 0x0000F516 File Offset: 0x0000D716
		public void setTouchIncreaseLeftRightTopBottom(float l, float r, float t, float b)
		{
			this.b1.setTouchIncreaseLeftRightTopBottom(l, r, t, b);
			this.b2.setTouchIncreaseLeftRightTopBottom(l, r, t, b);
		}

		// Token: 0x060002B2 RID: 690 RVA: 0x0000F538 File Offset: 0x0000D738
		public void toggle()
		{
			this.b1.setEnabled(!this.b1.isEnabled());
			this.b2.setEnabled(!this.b2.isEnabled());
		}

		// Token: 0x060002B3 RID: 691 RVA: 0x0000F56C File Offset: 0x0000D76C
		public bool on()
		{
			return this.b2.isEnabled();
		}

		// Token: 0x0400021A RID: 538
		public ButtonDelegate delegateButtonDelegate;

		// Token: 0x0400021B RID: 539
		private int buttonID;

		// Token: 0x0400021C RID: 540
		private Button b1;

		// Token: 0x0400021D RID: 541
		private Button b2;

		// Token: 0x020000B6 RID: 182
		private enum TOGGLE_BUTTON
		{
			// Token: 0x040008B8 RID: 2232
			TOGGLE_BUTTON_FACE1,
			// Token: 0x040008B9 RID: 2233
			TOGGLE_BUTTON_FACE2
		}
	}
}
