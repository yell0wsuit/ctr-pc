using CutTheRope.desktop;
using CutTheRope.iframework;
using CutTheRope.iframework.core;
using CutTheRope.iframework.visual;

using Microsoft.Xna.Framework;

namespace CutTheRope.commons
{
    internal sealed class Popup : BaseElement, ITimelineDelegate
    {
        public Popup()
        {
            Timeline timeline = new Timeline().InitWithMaxKeyFramesOnTrack(4);
            timeline.AddKeyFrame(KeyFrame.MakeScale(0.0, 0.0, KeyFrame.TransitionType.FRAME_TRANSITION_LINEAR, 0.0));
            timeline.AddKeyFrame(KeyFrame.MakeScale(1.1, 1.1, KeyFrame.TransitionType.FRAME_TRANSITION_LINEAR, 0.3));
            timeline.AddKeyFrame(KeyFrame.MakeScale(0.9, 0.9, KeyFrame.TransitionType.FRAME_TRANSITION_LINEAR, 0.1));
            timeline.AddKeyFrame(KeyFrame.MakeScale(1.0, 1.0, KeyFrame.TransitionType.FRAME_TRANSITION_LINEAR, 0.2));
            _ = AddTimeline(timeline);
            timeline = new Timeline().InitWithMaxKeyFramesOnTrack(2);
            timeline.AddKeyFrame(KeyFrame.MakeScale(1.0, 1.0, KeyFrame.TransitionType.FRAME_TRANSITION_LINEAR, 0.0));
            timeline.AddKeyFrame(KeyFrame.MakeScale(0.0, 0.0, KeyFrame.TransitionType.FRAME_TRANSITION_LINEAR, 0.3));
            width = (int)SCREEN_WIDTH;
            height = (int)SCREEN_HEIGHT;
            _ = AddTimeline(timeline);
            timeline.delegateTimelineDelegate = this;
        }

        public void TimelinereachedKeyFramewithIndex(Timeline t, KeyFrame k, int i)
        {
        }

        public void TimelineFinished(Timeline t)
        {
            View view = (View)parent;
            view?.RemoveChild(this);
        }

        public void ShowPopup()
        {
            Application.SharedRootController().DeactivateAllButtons();
            isShow = true;
            PlayTimeline(0);
        }

        public void HidePopup()
        {
            isShow = false;
            PlayTimeline(1);
        }

        public override bool OnTouchDownXY(float tx, float ty)
        {
            if (isShow)
            {
                _ = base.OnTouchDownXY(tx, ty);
            }
            return true;
        }

        public override bool OnTouchUpXY(float tx, float ty)
        {
            if (isShow)
            {
                _ = base.OnTouchUpXY(tx, ty);
            }
            return true;
        }

        public override bool OnTouchMoveXY(float tx, float ty)
        {
            if (isShow)
            {
                _ = base.OnTouchMoveXY(tx, ty);
            }
            return true;
        }

        public override void Draw()
        {
            OpenGL.GlEnable(1);
            OpenGL.GlDisable(0);
            OpenGL.GlBlendFunc(BlendingFactor.GLONE, BlendingFactor.GLONEMINUSSRCALPHA);
            GLDrawer.DrawSolidRectWOBorder(0f, 0f, SCREEN_WIDTH, SCREEN_HEIGHT, RGBAColor.MakeRGBA(0.0, 0.0, 0.0, 0.5));
            OpenGL.GlEnable(0);
            OpenGL.GlColor4f(Color.White);
            PreDraw();
            PostDraw();
            OpenGL.GlDisable(1);
        }

        private bool isShow;

        private enum POPUP
        {
            SHOW_ANIM,
            HIDE_ANIM
        }
    }
}
