using CutTheRope.Desktop;
using CutTheRope.Framework;
using CutTheRope.Framework.Core;
using CutTheRope.Framework.Visual;

using Microsoft.Xna.Framework;

namespace CutTheRope.GameMain
{
    internal sealed class GhostGrab : Grab
    {
        public GhostGrab InitWithPosition(float px, float py)
        {
            x = px;
            y = py;
            Image image = Image_createWithResIDQuad(Resources.Img.ObjGhost, 5);
            image.x = x - 60f;
            image.y = y + 2f;
            image.anchor = 18;
            // image.DoRestoreCutTransparency();
            _ = AddChild(image);
            Timeline timeline = new Timeline().InitWithMaxKeyFramesOnTrack(5);
            timeline.SetTimelineLoopType(Timeline.LoopType.TIMELINE_REPLAY);
            timeline.AddKeyFrame(KeyFrame.MakeScale(0.43f, 0.43f, KeyFrame.TransitionType.FRAME_TRANSITION_IMMEDIATE, 0.0f));
            timeline.AddKeyFrame(KeyFrame.MakeScale(0.465f, 0.465f, KeyFrame.TransitionType.FRAME_TRANSITION_EASE_IN, 0.65f));
            timeline.AddKeyFrame(KeyFrame.MakeScale(0.5f, 0.5f, KeyFrame.TransitionType.FRAME_TRANSITION_EASE_OUT, 0.65f));
            timeline.AddKeyFrame(KeyFrame.MakeScale(0.465f, 0.465f, KeyFrame.TransitionType.FRAME_TRANSITION_EASE_IN, 0.65f));
            timeline.AddKeyFrame(KeyFrame.MakeScale(0.43f, 0.43f, KeyFrame.TransitionType.FRAME_TRANSITION_EASE_OUT, 0.65f));
            timeline.AddKeyFrame(KeyFrame.MakePos(image.x - 1f, image.y + 1f, KeyFrame.TransitionType.FRAME_TRANSITION_IMMEDIATE, 0.0f));
            timeline.AddKeyFrame(KeyFrame.MakePos(image.x, image.y, KeyFrame.TransitionType.FRAME_TRANSITION_EASE_IN, 0.65f));
            timeline.AddKeyFrame(KeyFrame.MakePos(image.x + 1f, image.y - 1f, KeyFrame.TransitionType.FRAME_TRANSITION_EASE_OUT, 0.65f));
            timeline.AddKeyFrame(KeyFrame.MakePos(image.x, image.y, KeyFrame.TransitionType.FRAME_TRANSITION_EASE_IN, 0.65f));
            timeline.AddKeyFrame(KeyFrame.MakePos(image.x - 1f, image.y + 1f, KeyFrame.TransitionType.FRAME_TRANSITION_EASE_OUT, 0.65f));
            image.AddTimelinewithID(timeline, 0);
            image.PlayTimeline(0);

            Image image2 = Image_createWithResIDQuad(Resources.Img.ObjGhost, 4);
            image2.x = x + 58f;
            image2.y = y + 18f;
            image2.anchor = 18;
            // image2.DoRestoreCutTransparency();
            _ = AddChild(image2);
            Timeline timeline2 = new Timeline().InitWithMaxKeyFramesOnTrack(5);
            timeline2.SetTimelineLoopType(Timeline.LoopType.TIMELINE_REPLAY);
            timeline2.AddKeyFrame(KeyFrame.MakeScale(0.9f, 0.9f, KeyFrame.TransitionType.FRAME_TRANSITION_IMMEDIATE, 0.0f));
            timeline2.AddKeyFrame(KeyFrame.MakeScale(0.8f, 0.8f, KeyFrame.TransitionType.FRAME_TRANSITION_EASE_IN, 0.45f));
            timeline2.AddKeyFrame(KeyFrame.MakeScale(0.7f, 0.7f, KeyFrame.TransitionType.FRAME_TRANSITION_EASE_OUT, 0.45f));
            timeline2.AddKeyFrame(KeyFrame.MakeScale(0.8f, 0.8f, KeyFrame.TransitionType.FRAME_TRANSITION_EASE_IN, 0.45f));
            timeline2.AddKeyFrame(KeyFrame.MakeScale(0.9f, 0.9f, KeyFrame.TransitionType.FRAME_TRANSITION_EASE_OUT, 0.45f));
            timeline2.AddKeyFrame(KeyFrame.MakePos(image2.x + 1f, image2.y + 1f, KeyFrame.TransitionType.FRAME_TRANSITION_IMMEDIATE, 0.0f));
            timeline2.AddKeyFrame(KeyFrame.MakePos(image2.x, image2.y, KeyFrame.TransitionType.FRAME_TRANSITION_EASE_IN, 0.45f));
            timeline2.AddKeyFrame(KeyFrame.MakePos(image2.x - 1f, image2.y - 1f, KeyFrame.TransitionType.FRAME_TRANSITION_EASE_OUT, 0.45f));
            timeline2.AddKeyFrame(KeyFrame.MakePos(image2.x, image2.y, KeyFrame.TransitionType.FRAME_TRANSITION_EASE_IN, 0.45f));
            timeline2.AddKeyFrame(KeyFrame.MakePos(image2.x + 1f, image2.y + 1f, KeyFrame.TransitionType.FRAME_TRANSITION_EASE_OUT, 0.45f));
            image2.AddTimelinewithID(timeline2, 0);
            image2.PlayTimeline(0);

            Image image3 = Image_createWithResIDQuad(Resources.Img.ObjGhost, 2);
            image3.x = x - 15f;
            image3.y = y + 45f;
            image3.anchor = 18;
            // image3.DoRestoreCutTransparency();
            _ = AddChild(image3);
            Timeline timeline3 = new Timeline().InitWithMaxKeyFramesOnTrack(5);
            timeline3.SetTimelineLoopType(Timeline.LoopType.TIMELINE_REPLAY);
            timeline3.AddKeyFrame(KeyFrame.MakeScale(1.1f, 1.1f, KeyFrame.TransitionType.FRAME_TRANSITION_IMMEDIATE, 0.0f));
            timeline3.AddKeyFrame(KeyFrame.MakeScale(1f, 1f, KeyFrame.TransitionType.FRAME_TRANSITION_EASE_IN, 0.5f));
            timeline3.AddKeyFrame(KeyFrame.MakeScale(0.9f, 0.9f, KeyFrame.TransitionType.FRAME_TRANSITION_EASE_OUT, 0.5f));
            timeline3.AddKeyFrame(KeyFrame.MakeScale(1f, 1f, KeyFrame.TransitionType.FRAME_TRANSITION_EASE_IN, 0.5f));
            timeline3.AddKeyFrame(KeyFrame.MakeScale(1.1f, 1.1f, KeyFrame.TransitionType.FRAME_TRANSITION_EASE_OUT, 0.5f));
            timeline3.AddKeyFrame(KeyFrame.MakePos(image3.x - 1f, image3.y + 1f, KeyFrame.TransitionType.FRAME_TRANSITION_IMMEDIATE, 0.0f));
            timeline3.AddKeyFrame(KeyFrame.MakePos(image3.x, image3.y, KeyFrame.TransitionType.FRAME_TRANSITION_EASE_IN, 0.5f));
            timeline3.AddKeyFrame(KeyFrame.MakePos(image3.x + 1f, image3.y - 1f, KeyFrame.TransitionType.FRAME_TRANSITION_EASE_OUT, 0.5f));
            timeline3.AddKeyFrame(KeyFrame.MakePos(image3.x, image3.y, KeyFrame.TransitionType.FRAME_TRANSITION_EASE_IN, 0.5f));
            timeline3.AddKeyFrame(KeyFrame.MakePos(image3.x - 1f, image3.y + 1f, KeyFrame.TransitionType.FRAME_TRANSITION_EASE_OUT, 0.5f));
            image3.AddTimelinewithID(timeline3, 0);
            image3.PlayTimeline(0);
            return this;
        }

        public override void DrawBack()
        {
        }

        public override void Draw()
        {
            if (!visible)
            {
                return;
            }
            PreDraw();
            back.color = color;
            OpenGL.GlBlendFunc(BlendingFactor.GLONE, BlendingFactor.GLONEMINUSSRCALPHA);
            back.Draw();
            OpenGL.GlBlendFunc(BlendingFactor.GLSRCALPHA, BlendingFactor.GLONEMINUSSRCALPHA);
            OpenGL.GlDisable(0);
            if (radius != -1f || hideRadius)
            {
                CTRRootController rootController = (CTRRootController)Application.SharedRootController();
                int pack = rootController.GetPack();
                RGBAColor grabColor = pack == 6 ? RGBAColor.MakeRGBA(0.4, 0.7, 1.0, radiusAlpha * color.a) : RGBAColor.MakeRGBA(0.2, 0.5, 0.9, radiusAlpha * color.a);
                DrawGrabCircle(this, x, y, radius, vertexCount, grabColor);
            }
            OpenGL.GlColor4f(Color.White);
            OpenGL.GlEnable(0);
            OpenGL.GlDisable(0);
            rope?.Draw();
            OpenGL.GlColor4f(Color.White);
            OpenGL.GlEnable(0);
            OpenGL.GlBlendFunc(BlendingFactor.GLONE, BlendingFactor.GLONEMINUSSRCALPHA);
            front.color = color;
            front.Draw();
            PostDraw();
        }
    }
}
