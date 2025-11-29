using CutTheRope.Framework.Core;
using CutTheRope.Framework.Visual;

namespace CutTheRope.GameMain
{
    internal sealed class GhostBubble : Bubble
    {
        public static GhostBubble Create(CTRTexture2D texture)
        {
            return (GhostBubble)new GhostBubble().InitWithTexture(texture);
        }

        public static GhostBubble CreateWithResID(int resId)
        {
            return Create(Application.GetTexture(ResourceNameTranslator.TranslateLegacyId(resId)));
        }

        public static GhostBubble CreateWithResIDQuad(string resourceName, int quad)
        {
            GhostBubble bubble = Create(Application.GetTexture(resourceName));
            bubble?.SetDrawQuad(quad);
            return bubble;
        }

        public static GhostBubble CreateWithResIDQuad(int resId, int quad)
        {
            GhostBubble bubble = Create(Application.GetTexture(ResourceNameTranslator.TranslateLegacyId(resId)));
            bubble?.SetDrawQuad(quad);
            return bubble;
        }

        public void AddSupportingCloudsTimelines()
        {
            // first right cloud
            backCloud = Image_createWithResIDQuad(Resources.Img.ObjGhost, 6);
            backCloud.x = x + 85f;
            backCloud.y = y + 25f;
            backCloud.anchor = 18;
            _ = AddChild(backCloud);
            Timeline timeline = new Timeline().InitWithMaxKeyFramesOnTrack(5);
            timeline.SetTimelineLoopType(Timeline.LoopType.TIMELINE_REPLAY);
            timeline.AddKeyFrame(KeyFrame.MakeScale(0.8f, 0.8f, KeyFrame.TransitionType.FRAME_TRANSITION_IMMEDIATE, 0.0f));
            timeline.AddKeyFrame(KeyFrame.MakeScale(0.78f, 0.78f, KeyFrame.TransitionType.FRAME_TRANSITION_EASE_IN, 0.48f));
            timeline.AddKeyFrame(KeyFrame.MakeScale(0.76f, 0.76f, KeyFrame.TransitionType.FRAME_TRANSITION_EASE_OUT, 0.48f));
            timeline.AddKeyFrame(KeyFrame.MakeScale(0.78f, 0.78f, KeyFrame.TransitionType.FRAME_TRANSITION_EASE_IN, 0.48f));
            timeline.AddKeyFrame(KeyFrame.MakeScale(0.8f, 0.8f, KeyFrame.TransitionType.FRAME_TRANSITION_EASE_OUT, 0.48f));
            timeline.AddKeyFrame(KeyFrame.MakePos(backCloud.x + 1f, backCloud.y + 1f, KeyFrame.TransitionType.FRAME_TRANSITION_IMMEDIATE, 0.0f));
            timeline.AddKeyFrame(KeyFrame.MakePos(backCloud.x, backCloud.y, KeyFrame.TransitionType.FRAME_TRANSITION_EASE_IN, 0.48f));
            timeline.AddKeyFrame(KeyFrame.MakePos(backCloud.x - 1f, backCloud.y - 1f, KeyFrame.TransitionType.FRAME_TRANSITION_EASE_OUT, 0.48f));
            timeline.AddKeyFrame(KeyFrame.MakePos(backCloud.x, backCloud.y, KeyFrame.TransitionType.FRAME_TRANSITION_EASE_IN, 0.48f));
            timeline.AddKeyFrame(KeyFrame.MakePos(backCloud.x + 1f, backCloud.y + 1f, KeyFrame.TransitionType.FRAME_TRANSITION_EASE_OUT, 0.48f));
            backCloud.AddTimelinewithID(timeline, 0);
            backCloud.PlayTimeline(0);

            backCloud2 = Image_createWithResIDQuad(Resources.Img.ObjGhost, 5);
            backCloud2.x = x + 65f;
            backCloud2.y = y + 55f;
            backCloud2.anchor = 18;
            _ = AddChild(backCloud2);
            Timeline timeline2 = new Timeline().InitWithMaxKeyFramesOnTrack(5);
            timeline2.SetTimelineLoopType(Timeline.LoopType.TIMELINE_REPLAY);
            timeline2.AddKeyFrame(KeyFrame.MakeScale(0.93f, 0.93f, KeyFrame.TransitionType.FRAME_TRANSITION_IMMEDIATE, 0.0f));
            timeline2.AddKeyFrame(KeyFrame.MakeScale(0.965f, 0.965f, KeyFrame.TransitionType.FRAME_TRANSITION_EASE_IN, 0.4f));
            timeline2.AddKeyFrame(KeyFrame.MakeScale(1f, 1f, KeyFrame.TransitionType.FRAME_TRANSITION_EASE_OUT, 0.4f));
            timeline2.AddKeyFrame(KeyFrame.MakeScale(0.965f, 0.965f, KeyFrame.TransitionType.FRAME_TRANSITION_EASE_IN, 0.4f));
            timeline2.AddKeyFrame(KeyFrame.MakeScale(0.93f, 0.93f, KeyFrame.TransitionType.FRAME_TRANSITION_EASE_OUT, 0.4f));
            timeline2.AddKeyFrame(KeyFrame.MakePos(backCloud2.x + 1f, backCloud2.y + 1f, KeyFrame.TransitionType.FRAME_TRANSITION_IMMEDIATE, 0.0f));
            timeline2.AddKeyFrame(KeyFrame.MakePos(backCloud2.x, backCloud2.y, KeyFrame.TransitionType.FRAME_TRANSITION_EASE_IN, 0.4f));
            timeline2.AddKeyFrame(KeyFrame.MakePos(backCloud2.x - 1f, backCloud2.y - 1f, KeyFrame.TransitionType.FRAME_TRANSITION_EASE_OUT, 0.4f));
            timeline2.AddKeyFrame(KeyFrame.MakePos(backCloud2.x, backCloud2.y, KeyFrame.TransitionType.FRAME_TRANSITION_EASE_IN, 0.4f));
            timeline2.AddKeyFrame(KeyFrame.MakePos(backCloud2.x + 1f, backCloud2.y + 1f, KeyFrame.TransitionType.FRAME_TRANSITION_EASE_OUT, 0.4f));
            backCloud2.AddTimelinewithID(timeline2, 0);
            backCloud2.PlayTimeline(0);

            // first left small cloud
            backCloud3 = Image_createWithResIDQuad(Resources.Img.ObjGhost, 5);
            backCloud3.x = x - 90f;
            backCloud3.y = y + 15f;
            backCloud3.anchor = 18;
            _ = AddChild(backCloud3);
            Timeline timeline3 = new Timeline().InitWithMaxKeyFramesOnTrack(5);
            timeline3.SetTimelineLoopType(Timeline.LoopType.TIMELINE_REPLAY);
            timeline3.AddKeyFrame(KeyFrame.MakeScale(0.33f, 0.33f, KeyFrame.TransitionType.FRAME_TRANSITION_IMMEDIATE, 0.0f));
            timeline3.AddKeyFrame(KeyFrame.MakeScale(0.365f, 0.365f, KeyFrame.TransitionType.FRAME_TRANSITION_EASE_IN, 0.43f));
            timeline3.AddKeyFrame(KeyFrame.MakeScale(0.4f, 0.4f, KeyFrame.TransitionType.FRAME_TRANSITION_EASE_OUT, 0.43f));
            timeline3.AddKeyFrame(KeyFrame.MakeScale(0.365f, 0.365f, KeyFrame.TransitionType.FRAME_TRANSITION_EASE_IN, 0.43f));
            timeline3.AddKeyFrame(KeyFrame.MakeScale(0.33f, 0.33f, KeyFrame.TransitionType.FRAME_TRANSITION_EASE_OUT, 0.43f));
            timeline3.AddKeyFrame(KeyFrame.MakePos(backCloud3.x + 1f, backCloud3.y + 1f, KeyFrame.TransitionType.FRAME_TRANSITION_IMMEDIATE, 0.0f));
            timeline3.AddKeyFrame(KeyFrame.MakePos(backCloud3.x, backCloud3.y, KeyFrame.TransitionType.FRAME_TRANSITION_EASE_IN, 0.43f));
            timeline3.AddKeyFrame(KeyFrame.MakePos(backCloud3.x - 1f, backCloud3.y - 1f, KeyFrame.TransitionType.FRAME_TRANSITION_EASE_OUT, 0.43f));
            timeline3.AddKeyFrame(KeyFrame.MakePos(backCloud3.x, backCloud3.y, KeyFrame.TransitionType.FRAME_TRANSITION_EASE_IN, 0.43f));
            timeline3.AddKeyFrame(KeyFrame.MakePos(backCloud3.x + 1f, backCloud3.y + 1f, KeyFrame.TransitionType.FRAME_TRANSITION_EASE_OUT, 0.43f));
            backCloud3.AddTimelinewithID(timeline3, 0);
            backCloud3.PlayTimeline(0);

            // second left small cloud
            Image image = Image_createWithResIDQuad(Resources.Img.ObjGhost, 6);
            image.x = x - 75f;
            image.y = y + 45f;
            image.anchor = 18;
            //image.DoRestoreCutTransparency();
            _ = AddChild(image);
            Timeline timeline4 = new Timeline().InitWithMaxKeyFramesOnTrack(5);
            timeline4.SetTimelineLoopType(Timeline.LoopType.TIMELINE_REPLAY);
            timeline4.AddKeyFrame(KeyFrame.MakeScale(0.6f, 0.6f, KeyFrame.TransitionType.FRAME_TRANSITION_IMMEDIATE, 0.0f));
            timeline4.AddKeyFrame(KeyFrame.MakeScale(0.565f, 0.565f, KeyFrame.TransitionType.FRAME_TRANSITION_EASE_IN, 0.42f));
            timeline4.AddKeyFrame(KeyFrame.MakeScale(0.53f, 0.53f, KeyFrame.TransitionType.FRAME_TRANSITION_EASE_OUT, 0.42f));
            timeline4.AddKeyFrame(KeyFrame.MakeScale(0.565f, 0.565f, KeyFrame.TransitionType.FRAME_TRANSITION_EASE_IN, 0.42f));
            timeline4.AddKeyFrame(KeyFrame.MakeScale(0.6f, 0.6f, KeyFrame.TransitionType.FRAME_TRANSITION_EASE_OUT, 0.42f));
            timeline4.AddKeyFrame(KeyFrame.MakePos(image.x - 1f, image.y + 1f, KeyFrame.TransitionType.FRAME_TRANSITION_IMMEDIATE, 0.0f));
            timeline4.AddKeyFrame(KeyFrame.MakePos(image.x, image.y, KeyFrame.TransitionType.FRAME_TRANSITION_EASE_IN, 0.42f));
            timeline4.AddKeyFrame(KeyFrame.MakePos(image.x + 1f, image.y - 1f, KeyFrame.TransitionType.FRAME_TRANSITION_EASE_OUT, 0.42f));
            timeline4.AddKeyFrame(KeyFrame.MakePos(image.x, image.y, KeyFrame.TransitionType.FRAME_TRANSITION_EASE_IN, 0.42f));
            timeline4.AddKeyFrame(KeyFrame.MakePos(image.x - 1f, image.y + 1f, KeyFrame.TransitionType.FRAME_TRANSITION_EASE_OUT, 0.42f));
            image.AddTimelinewithID(timeline4, 0);
            image.PlayTimeline(0);

            // big cloud
            Image image2 = Image_createWithResIDQuad(Resources.Img.ObjGhost, 2);
            image2.x = x - 20f;
            image2.y = y + 75f;
            image2.anchor = 18;
            //image2.DoRestoreCutTransparency();
            _ = AddChild(image2);
            Timeline timeline5 = new Timeline().InitWithMaxKeyFramesOnTrack(5);
            timeline5.SetTimelineLoopType(Timeline.LoopType.TIMELINE_REPLAY);
            timeline5.AddKeyFrame(KeyFrame.MakeScale(0.93f, 0.93f, KeyFrame.TransitionType.FRAME_TRANSITION_IMMEDIATE, 0.0f));
            timeline5.AddKeyFrame(KeyFrame.MakeScale(0.965f, 0.965f, KeyFrame.TransitionType.FRAME_TRANSITION_EASE_IN, 0.47f));
            timeline5.AddKeyFrame(KeyFrame.MakeScale(1f, 1f, KeyFrame.TransitionType.FRAME_TRANSITION_EASE_OUT, 0.47f));
            timeline5.AddKeyFrame(KeyFrame.MakeScale(0.965f, 0.965f, KeyFrame.TransitionType.FRAME_TRANSITION_EASE_IN, 0.47f));
            timeline5.AddKeyFrame(KeyFrame.MakeScale(0.93f, 0.93f, KeyFrame.TransitionType.FRAME_TRANSITION_EASE_OUT, 0.47f));
            timeline5.AddKeyFrame(KeyFrame.MakePos(image2.x + 1f, image2.y - 1f, KeyFrame.TransitionType.FRAME_TRANSITION_IMMEDIATE, 0.0f));
            timeline5.AddKeyFrame(KeyFrame.MakePos(image2.x, image2.y, KeyFrame.TransitionType.FRAME_TRANSITION_EASE_IN, 0.47f));
            timeline5.AddKeyFrame(KeyFrame.MakePos(image2.x - 1f, image2.y + 1f, KeyFrame.TransitionType.FRAME_TRANSITION_EASE_OUT, 0.47f));
            timeline5.AddKeyFrame(KeyFrame.MakePos(image2.x, image2.y, KeyFrame.TransitionType.FRAME_TRANSITION_EASE_IN, 0.47f));
            timeline5.AddKeyFrame(KeyFrame.MakePos(image2.x + 1f, image2.y - 1f, KeyFrame.TransitionType.FRAME_TRANSITION_EASE_OUT, 0.47f));
            timeline5.AddKeyFrame(KeyFrame.MakeRotation(350.0, KeyFrame.TransitionType.FRAME_TRANSITION_IMMEDIATE, 0.0));
            timeline5.AddKeyFrame(KeyFrame.MakeRotation(350.0, KeyFrame.TransitionType.FRAME_TRANSITION_IMMEDIATE, 0.0));
            timeline5.AddKeyFrame(KeyFrame.MakeRotation(350.0, KeyFrame.TransitionType.FRAME_TRANSITION_IMMEDIATE, 0.0));
            image2.AddTimelinewithID(timeline5, 0);
            image2.PlayTimeline(0);
            passTransformationsToChilds = true;
        }

        public override void Draw()
        {
            PreDraw();
            if (!withoutShadow)
            {
                if (quadToDraw == -1)
                {
                    GLDrawer.DrawImage(texture, drawX, drawY);
                }
                else
                {
                    DrawQuad(quadToDraw);
                }
            }
            if (!popped)
            {
                PostDraw();
                return;
            }
            RestoreColor(this);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                backCloud = null;
                backCloud2 = null;
                backCloud3 = null;
            }
            base.Dispose(disposing);
        }

        public Image backCloud;
        public Image backCloud2;
        public Image backCloud3;
    }
}
