using System;

using CutTheRope.Framework;
using CutTheRope.Framework.Visual;

namespace CutTheRope.GameMain
{
    internal sealed class GhostBouncer : Bouncer
    {
        public override Bouncer InitWithPosXYWidthAndAngle(float px, float py, int width, double angle)
        {
            if (base.InitWithPosXYWidthAndAngle(px, py, width, angle) != null)
            {
                backCloud2 = Image_createWithResIDQuad(Resources.Img.ObjGhost, 4);
                float radius = (float)Math.Sqrt(9000.0);
                backCloud2.x = x + (radius * Cosf(DEGREES_TO_RADIANS((float)(170.0 + angle))));
                backCloud2.y = y + (radius * Sinf(DEGREES_TO_RADIANS((float)(170.0 + angle))));
                backCloud2.anchor = 18;
                backCloud2.visible = false;
                _ = AddChild(backCloud2);
                Timeline timeline = new Timeline().InitWithMaxKeyFramesOnTrack(5);
                timeline.SetTimelineLoopType(Timeline.LoopType.TIMELINE_REPLAY);
                timeline.AddKeyFrame(KeyFrame.MakeScale(0.7f, 0.7f, KeyFrame.TransitionType.FRAME_TRANSITION_IMMEDIATE, 0.0f));
                timeline.AddKeyFrame(KeyFrame.MakeScale(0.55f, 0.55f, KeyFrame.TransitionType.FRAME_TRANSITION_EASE_IN, 0.35f));
                timeline.AddKeyFrame(KeyFrame.MakeScale(0.4f, 0.4f, KeyFrame.TransitionType.FRAME_TRANSITION_EASE_OUT, 0.35f));
                timeline.AddKeyFrame(KeyFrame.MakeScale(0.55f, 0.55f, KeyFrame.TransitionType.FRAME_TRANSITION_EASE_IN, 0.35f));
                timeline.AddKeyFrame(KeyFrame.MakeScale(0.7f, 0.7f, KeyFrame.TransitionType.FRAME_TRANSITION_EASE_OUT, 0.35f));
                timeline.AddKeyFrame(KeyFrame.MakePos(backCloud2.x + 1f, backCloud2.y + 1f, KeyFrame.TransitionType.FRAME_TRANSITION_IMMEDIATE, 0.0f));
                timeline.AddKeyFrame(KeyFrame.MakePos(backCloud2.x, backCloud2.y, KeyFrame.TransitionType.FRAME_TRANSITION_EASE_IN, 0.35f));
                timeline.AddKeyFrame(KeyFrame.MakePos(backCloud2.x - 1f, backCloud2.y - 1f, KeyFrame.TransitionType.FRAME_TRANSITION_EASE_OUT, 0.35f));
                timeline.AddKeyFrame(KeyFrame.MakePos(backCloud2.x, backCloud2.y, KeyFrame.TransitionType.FRAME_TRANSITION_EASE_IN, 0.35f));
                timeline.AddKeyFrame(KeyFrame.MakePos(backCloud2.x + 1f, backCloud2.y + 1f, KeyFrame.TransitionType.FRAME_TRANSITION_EASE_OUT, 0.35f));
                backCloud2.AddTimelinewithID(timeline, 0);
                backCloud2.PlayTimeline(0);

                backCloud = Image_createWithResIDQuad(Resources.Img.ObjGhost, 4);
                float radius2 = (float)Math.Sqrt(9000.0);
                backCloud.x = x + (radius2 * Cosf(DEGREES_TO_RADIANS((float)(10.0 + angle))));
                backCloud.y = y + (radius2 * Sinf(DEGREES_TO_RADIANS((float)(10.0 + angle))));
                backCloud.anchor = 18;
                backCloud.visible = false;
                _ = AddChild(backCloud);
                Timeline timeline2 = new Timeline().InitWithMaxKeyFramesOnTrack(5);
                timeline2.SetTimelineLoopType(Timeline.LoopType.TIMELINE_REPLAY);
                timeline2.AddKeyFrame(KeyFrame.MakeScale(0.9f, 0.9f, KeyFrame.TransitionType.FRAME_TRANSITION_IMMEDIATE, 0.0f));
                timeline2.AddKeyFrame(KeyFrame.MakeScale(0.8f, 0.8f, KeyFrame.TransitionType.FRAME_TRANSITION_EASE_IN, 0.39f));
                timeline2.AddKeyFrame(KeyFrame.MakeScale(0.7f, 0.7f, KeyFrame.TransitionType.FRAME_TRANSITION_EASE_OUT, 0.39f));
                timeline2.AddKeyFrame(KeyFrame.MakeScale(0.8f, 0.8f, KeyFrame.TransitionType.FRAME_TRANSITION_EASE_IN, 0.39f));
                timeline2.AddKeyFrame(KeyFrame.MakeScale(0.9f, 0.9f, KeyFrame.TransitionType.FRAME_TRANSITION_EASE_OUT, 0.39f));
                timeline2.AddKeyFrame(KeyFrame.MakePos(backCloud.x + 1f, backCloud.y + 1f, KeyFrame.TransitionType.FRAME_TRANSITION_IMMEDIATE, 0.0f));
                timeline2.AddKeyFrame(KeyFrame.MakePos(backCloud.x, backCloud.y, KeyFrame.TransitionType.FRAME_TRANSITION_EASE_IN, 0.39f));
                timeline2.AddKeyFrame(KeyFrame.MakePos(backCloud.x - 1f, backCloud.y - 1f, KeyFrame.TransitionType.FRAME_TRANSITION_EASE_OUT, 0.39f));
                timeline2.AddKeyFrame(KeyFrame.MakePos(backCloud.x, backCloud.y, KeyFrame.TransitionType.FRAME_TRANSITION_EASE_IN, 0.39f));
                timeline2.AddKeyFrame(KeyFrame.MakePos(backCloud.x + 1f, backCloud.y + 1f, KeyFrame.TransitionType.FRAME_TRANSITION_EASE_OUT, 0.39f));
                backCloud.AddTimelinewithID(timeline2, 0);
                backCloud.PlayTimeline(0);

                Image image = Image_createWithResIDQuad(Resources.Img.ObjGhost, 3);
                image.x = x + 60f;
                image.y = y + 55f;
                image.anchor = 18;
                //image.DoRestoreCutTransparency();
                _ = AddChild(image);
                Timeline timeline3 = new Timeline().InitWithMaxKeyFramesOnTrack(5);
                timeline3.SetTimelineLoopType(Timeline.LoopType.TIMELINE_REPLAY);
                timeline3.AddKeyFrame(KeyFrame.MakeScale(1.1f, 1.1f, KeyFrame.TransitionType.FRAME_TRANSITION_IMMEDIATE, 0.0f));
                timeline3.AddKeyFrame(KeyFrame.MakeScale(1f, 1f, KeyFrame.TransitionType.FRAME_TRANSITION_EASE_IN, 0.45f));
                timeline3.AddKeyFrame(KeyFrame.MakeScale(0.9f, 0.9f, KeyFrame.TransitionType.FRAME_TRANSITION_EASE_OUT, 0.45f));
                timeline3.AddKeyFrame(KeyFrame.MakeScale(1f, 1f, KeyFrame.TransitionType.FRAME_TRANSITION_EASE_IN, 0.45f));
                timeline3.AddKeyFrame(KeyFrame.MakeScale(1.1f, 1.1f, KeyFrame.TransitionType.FRAME_TRANSITION_EASE_OUT, 0.45f));
                timeline3.AddKeyFrame(KeyFrame.MakePos(image.x + 1f, image.y + 1f, KeyFrame.TransitionType.FRAME_TRANSITION_IMMEDIATE, 0.0f));
                timeline3.AddKeyFrame(KeyFrame.MakePos(image.x, image.y, KeyFrame.TransitionType.FRAME_TRANSITION_EASE_IN, 0.45f));
                timeline3.AddKeyFrame(KeyFrame.MakePos(image.x - 1f, image.y - 1f, KeyFrame.TransitionType.FRAME_TRANSITION_EASE_OUT, 0.45f));
                timeline3.AddKeyFrame(KeyFrame.MakePos(image.x, image.y, KeyFrame.TransitionType.FRAME_TRANSITION_EASE_IN, 0.45f));
                timeline3.AddKeyFrame(KeyFrame.MakePos(image.x + 1f, image.y + 1f, KeyFrame.TransitionType.FRAME_TRANSITION_EASE_OUT, 0.45f));
                image.AddTimelinewithID(timeline3, 0);
                image.PlayTimeline(0);

                Image image2 = Image_createWithResIDQuad(Resources.Img.ObjGhost, 2);
                image2.x = x - 50f;
                image2.y = y + 55f;
                image2.anchor = 18;
                //image2.DoRestoreCutTransparency();
                _ = AddChild(image2);
                Timeline timeline4 = new Timeline().InitWithMaxKeyFramesOnTrack(5);
                timeline4.SetTimelineLoopType(Timeline.LoopType.TIMELINE_REPLAY);
                timeline4.AddKeyFrame(KeyFrame.MakeScale(1.1f, 1.1f, KeyFrame.TransitionType.FRAME_TRANSITION_IMMEDIATE, 0.0f));
                timeline4.AddKeyFrame(KeyFrame.MakeScale(1f, 1f, KeyFrame.TransitionType.FRAME_TRANSITION_EASE_IN, 0.5f));
                timeline4.AddKeyFrame(KeyFrame.MakeScale(0.9f, 0.9f, KeyFrame.TransitionType.FRAME_TRANSITION_EASE_OUT, 0.5f));
                timeline4.AddKeyFrame(KeyFrame.MakeScale(1f, 1f, KeyFrame.TransitionType.FRAME_TRANSITION_EASE_IN, 0.5f));
                timeline4.AddKeyFrame(KeyFrame.MakeScale(1.1f, 1.1f, KeyFrame.TransitionType.FRAME_TRANSITION_EASE_OUT, 0.5f));
                timeline4.AddKeyFrame(KeyFrame.MakePos(image2.x - 1f, image2.y + 1f, KeyFrame.TransitionType.FRAME_TRANSITION_IMMEDIATE, 0.0f));
                timeline4.AddKeyFrame(KeyFrame.MakePos(image2.x, image2.y, KeyFrame.TransitionType.FRAME_TRANSITION_EASE_IN, 0.5f));
                timeline4.AddKeyFrame(KeyFrame.MakePos(image2.x + 1f, image2.y - 1f, KeyFrame.TransitionType.FRAME_TRANSITION_EASE_OUT, 0.5f));
                timeline4.AddKeyFrame(KeyFrame.MakePos(image2.x, image2.y, KeyFrame.TransitionType.FRAME_TRANSITION_EASE_IN, 0.5f));
                timeline4.AddKeyFrame(KeyFrame.MakePos(image2.x - 1f, image2.y + 1f, KeyFrame.TransitionType.FRAME_TRANSITION_EASE_OUT, 0.5f));
                image2.AddTimelinewithID(timeline4, 0);
                image2.PlayTimeline(0);
            }
            return this;
        }

        public override void PlayTimeline(int timelineIndex)
        {
            if (GetCurrentTimelineIndex() == 11)
            {
                return;
            }
            if (timelineIndex != 11 && GetCurrentTimelineIndex() == 10 && GetCurrentTimeline().state != Timeline.TimelineState.TIMELINE_STOPPED)
            {
                color = RGBAColor.solidOpaqueRGBA;
            }
            base.PlayTimeline(timelineIndex);
        }

        public override void Draw()
        {
            backCloud.Draw();
            backCloud2.Draw();
            base.Draw();
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                backCloud = null;
                backCloud2 = null;
            }
            base.Dispose(disposing);
        }

        public Image backCloud;
        public Image backCloud2;
    }
}
