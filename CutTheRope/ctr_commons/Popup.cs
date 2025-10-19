using System;
using CutTheRope.iframework;
using CutTheRope.iframework.core;
using CutTheRope.iframework.visual;
using CutTheRope.ios;
using CutTheRope.windows;
using Microsoft.Xna.Framework;

namespace CutTheRope.ctr_commons
{
	// Token: 0x0200009F RID: 159
	internal class Popup : BaseElement, TimelineDelegate
	{
		// Token: 0x06000641 RID: 1601 RVA: 0x00033848 File Offset: 0x00031A48
		public override NSObject init()
		{
			if (base.init() != null)
			{
				Timeline timeline = new Timeline().initWithMaxKeyFramesOnTrack(4);
				timeline.addKeyFrame(KeyFrame.makeScale(0.0, 0.0, KeyFrame.TransitionType.FRAME_TRANSITION_LINEAR, 0.0));
				timeline.addKeyFrame(KeyFrame.makeScale(1.1, 1.1, KeyFrame.TransitionType.FRAME_TRANSITION_LINEAR, 0.3));
				timeline.addKeyFrame(KeyFrame.makeScale(0.9, 0.9, KeyFrame.TransitionType.FRAME_TRANSITION_LINEAR, 0.1));
				timeline.addKeyFrame(KeyFrame.makeScale(1.0, 1.0, KeyFrame.TransitionType.FRAME_TRANSITION_LINEAR, 0.2));
				this.addTimeline(timeline);
				timeline = new Timeline().initWithMaxKeyFramesOnTrack(2);
				timeline.addKeyFrame(KeyFrame.makeScale(1.0, 1.0, KeyFrame.TransitionType.FRAME_TRANSITION_LINEAR, 0.0));
				timeline.addKeyFrame(KeyFrame.makeScale(0.0, 0.0, KeyFrame.TransitionType.FRAME_TRANSITION_LINEAR, 0.3));
				this.width = (int)FrameworkTypes.SCREEN_WIDTH;
				this.height = (int)FrameworkTypes.SCREEN_HEIGHT;
				this.addTimeline(timeline);
				timeline.delegateTimelineDelegate = this;
			}
			return this;
		}

		// Token: 0x06000642 RID: 1602 RVA: 0x00033992 File Offset: 0x00031B92
		public virtual void timelinereachedKeyFramewithIndex(Timeline t, KeyFrame k, int i)
		{
		}

		// Token: 0x06000643 RID: 1603 RVA: 0x00033994 File Offset: 0x00031B94
		public virtual void timelineFinished(Timeline t)
		{
			View view = (View)this.parent;
			if (view != null)
			{
				view.removeChild(this);
			}
		}

		// Token: 0x06000644 RID: 1604 RVA: 0x000339B7 File Offset: 0x00031BB7
		public virtual void showPopup()
		{
			Application.sharedRootController().deactivateAllButtons();
			this.isShow = true;
			this.playTimeline(0);
		}

		// Token: 0x06000645 RID: 1605 RVA: 0x000339D1 File Offset: 0x00031BD1
		public virtual void hidePopup()
		{
			this.isShow = false;
			this.playTimeline(1);
		}

		// Token: 0x06000646 RID: 1606 RVA: 0x000339E1 File Offset: 0x00031BE1
		public override bool onTouchDownXY(float tx, float ty)
		{
			if (this.isShow)
			{
				base.onTouchDownXY(tx, ty);
			}
			return true;
		}

		// Token: 0x06000647 RID: 1607 RVA: 0x000339F5 File Offset: 0x00031BF5
		public override bool onTouchUpXY(float tx, float ty)
		{
			if (this.isShow)
			{
				base.onTouchUpXY(tx, ty);
			}
			return true;
		}

		// Token: 0x06000648 RID: 1608 RVA: 0x00033A09 File Offset: 0x00031C09
		public override bool onTouchMoveXY(float tx, float ty)
		{
			if (this.isShow)
			{
				base.onTouchMoveXY(tx, ty);
			}
			return true;
		}

		// Token: 0x06000649 RID: 1609 RVA: 0x00033A20 File Offset: 0x00031C20
		public override void draw()
		{
			OpenGL.glEnable(1);
			OpenGL.glDisable(0);
			OpenGL.glBlendFunc(BlendingFactor.GL_ONE, BlendingFactor.GL_ONE_MINUS_SRC_ALPHA);
			GLDrawer.drawSolidRectWOBorder(0f, 0f, FrameworkTypes.SCREEN_WIDTH, FrameworkTypes.SCREEN_HEIGHT, RGBAColor.MakeRGBA(0.0, 0.0, 0.0, 0.5));
			OpenGL.glEnable(0);
			OpenGL.glColor4f(Color.White);
			base.preDraw();
			base.postDraw();
			OpenGL.glDisable(1);
		}

		// Token: 0x04000868 RID: 2152
		private bool isShow;

		// Token: 0x020000D0 RID: 208
		private enum POPUP
		{
			// Token: 0x0400090B RID: 2315
			POPUP_SHOW_ANIM,
			// Token: 0x0400090C RID: 2316
			POPUP_HIDE_ANIM
		}
	}
}
