using CutTheRope.iframework;
using CutTheRope.iframework.core;
using CutTheRope.iframework.visual;
using CutTheRope.ios;
using System;

namespace CutTheRope.game
{
    internal class Processing : RectangleElement, TimelineDelegate
    {
        private static NSObject createWithLoading()
        {
            return new Processing().initWithLoading(true);
        }

        public virtual NSObject initWithLoading(bool loading)
        {
            if (init() != null)
            {
                width = (int)SCREEN_WIDTH_EXPANDED;
                height = (int)SCREEN_HEIGHT_EXPANDED + 1;
                x = 0f - SCREEN_OFFSET_X;
                y = 0f - SCREEN_OFFSET_Y;
                blendingMode = 0;
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
                    HBox hBox = new HBox().initWithOffsetAlignHeight(10f, 16, image.height);
                    hBox.parentAnchor = hBox.anchor = 18;
                    addChild(hBox);
                    hBox.addChild(image);
                    hBox.addChild(c);
                }
                Timeline timeline2 = new Timeline().initWithMaxKeyFramesOnTrack(2);
                timeline2.addKeyFrame(KeyFrame.makeColor(RGBAColor.transparentRGBA, KeyFrame.TransitionType.FRAME_TRANSITION_IMMEDIATE, 0f));
                timeline2.addKeyFrame(KeyFrame.makeColor(RGBAColor.MakeRGBA(0.0, 0.0, 0.0, 0.4), KeyFrame.TransitionType.FRAME_TRANSITION_LINEAR, 0.2));
                addTimeline(timeline2);
                timeline2 = new Timeline().initWithMaxKeyFramesOnTrack(2);
                timeline2.delegateTimelineDelegate = this;
                timeline2.addKeyFrame(KeyFrame.makeColor(RGBAColor.MakeRGBA(0.0, 0.0, 0.0, 0.4), KeyFrame.TransitionType.FRAME_TRANSITION_IMMEDIATE, 0f));
                timeline2.addKeyFrame(KeyFrame.makeColor(RGBAColor.transparentRGBA, KeyFrame.TransitionType.FRAME_TRANSITION_LINEAR, 0.2));
                addTimeline(timeline2);
                playTimeline(0);
            }
            return this;
        }

        public override bool onTouchDownXY(float tx, float ty)
        {
            base.onTouchDownXY(tx, ty);
            return true;
        }

        public override bool onTouchUpXY(float tx, float ty)
        {
            base.onTouchUpXY(tx, ty);
            return true;
        }

        public override bool onTouchMoveXY(float tx, float ty)
        {
            base.onTouchMoveXY(tx, ty);
            return true;
        }

        public override void playTimeline(int t)
        {
            if (t == 0)
            {
                setEnabled(true);
            }
            base.playTimeline(t);
        }

        public void timelineFinished(Timeline t)
        {
            setEnabled(false);
        }

        public void timelinereachedKeyFramewithIndex(Timeline t, KeyFrame k, int i)
        {
        }
    }
}
