using System;
using CutTheRope.iframework;
using CutTheRope.iframework.core;
using CutTheRope.iframework.visual;
using CutTheRope.ios;

namespace CutTheRope.game
{
	// Token: 0x0200008C RID: 140
	internal class Processing : RectangleElement, TimelineDelegate
	{
		// Token: 0x060005AA RID: 1450 RVA: 0x0002E9DE File Offset: 0x0002CBDE
		private static NSObject createWithLoading()
		{
			return new Processing().initWithLoading(true);
		}

		// Token: 0x060005AB RID: 1451 RVA: 0x0002E9EC File Offset: 0x0002CBEC
		public virtual NSObject initWithLoading(bool loading)
		{
			if (this.init() != null)
			{
				this.width = (int)FrameworkTypes.SCREEN_WIDTH_EXPANDED;
				this.height = (int)FrameworkTypes.SCREEN_HEIGHT_EXPANDED + 1;
				this.x = 0f - FrameworkTypes.SCREEN_OFFSET_X;
				this.y = 0f - FrameworkTypes.SCREEN_OFFSET_Y;
				this.blendingMode = 0;
				if (loading)
				{
					Image image = Image.Image_createWithResIDQuad(57, 0);
					Timeline timeline = new Timeline().initWithMaxKeyFramesOnTrack(2);
					timeline.addKeyFrame(KeyFrame.makeRotation(0, KeyFrame.TransitionType.FRAME_TRANSITION_LINEAR, 0f));
					timeline.addKeyFrame(KeyFrame.makeRotation(360, KeyFrame.TransitionType.FRAME_TRANSITION_LINEAR, 1f));
					timeline.setTimelineLoopType(Timeline.LoopType.TIMELINE_REPLAY);
					image.addTimeline(timeline);
					image.playTimeline(0);
					Text c = Text.createWithFontandString(3, Application.getString(655425));
					HBox hBox = new HBox().initWithOffsetAlignHeight(10f, 16, (float)image.height);
					hBox.parentAnchor = (hBox.anchor = 18);
					this.addChild(hBox);
					hBox.addChild(image);
					hBox.addChild(c);
				}
				Timeline timeline2 = new Timeline().initWithMaxKeyFramesOnTrack(2);
				timeline2.addKeyFrame(KeyFrame.makeColor(RGBAColor.transparentRGBA, KeyFrame.TransitionType.FRAME_TRANSITION_IMMEDIATE, 0f));
				timeline2.addKeyFrame(KeyFrame.makeColor(RGBAColor.MakeRGBA(0.0, 0.0, 0.0, 0.4), KeyFrame.TransitionType.FRAME_TRANSITION_LINEAR, 0.2));
				this.addTimeline(timeline2);
				timeline2 = new Timeline().initWithMaxKeyFramesOnTrack(2);
				timeline2.delegateTimelineDelegate = this;
				timeline2.addKeyFrame(KeyFrame.makeColor(RGBAColor.MakeRGBA(0.0, 0.0, 0.0, 0.4), KeyFrame.TransitionType.FRAME_TRANSITION_IMMEDIATE, 0f));
				timeline2.addKeyFrame(KeyFrame.makeColor(RGBAColor.transparentRGBA, KeyFrame.TransitionType.FRAME_TRANSITION_LINEAR, 0.2));
				this.addTimeline(timeline2);
				this.playTimeline(0);
			}
			return this;
		}

		// Token: 0x060005AC RID: 1452 RVA: 0x0002EBDA File Offset: 0x0002CDDA
		public override bool onTouchDownXY(float tx, float ty)
		{
			base.onTouchDownXY(tx, ty);
			return true;
		}

		// Token: 0x060005AD RID: 1453 RVA: 0x0002EBE6 File Offset: 0x0002CDE6
		public override bool onTouchUpXY(float tx, float ty)
		{
			base.onTouchUpXY(tx, ty);
			return true;
		}

		// Token: 0x060005AE RID: 1454 RVA: 0x0002EBF2 File Offset: 0x0002CDF2
		public override bool onTouchMoveXY(float tx, float ty)
		{
			base.onTouchMoveXY(tx, ty);
			return true;
		}

		// Token: 0x060005AF RID: 1455 RVA: 0x0002EBFE File Offset: 0x0002CDFE
		public override void playTimeline(int t)
		{
			if (t == 0)
			{
				base.setEnabled(true);
			}
			base.playTimeline(t);
		}

		// Token: 0x060005B0 RID: 1456 RVA: 0x0002EC11 File Offset: 0x0002CE11
		public void timelineFinished(Timeline t)
		{
			base.setEnabled(false);
		}

		// Token: 0x060005B1 RID: 1457 RVA: 0x0002EC1A File Offset: 0x0002CE1A
		public void timelinereachedKeyFramewithIndex(Timeline t, KeyFrame k, int i)
		{
		}
	}
}
