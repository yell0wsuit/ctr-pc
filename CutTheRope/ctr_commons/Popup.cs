using CutTheRope.iframework;
using CutTheRope.iframework.core;
using CutTheRope.iframework.visual;
using CutTheRope.ios;
using CutTheRope.windows;
using Microsoft.Xna.Framework;
using System;

namespace CutTheRope.ctr_commons
{
    internal class Popup : BaseElement, TimelineDelegate
    {
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

        public virtual void timelinereachedKeyFramewithIndex(Timeline t, KeyFrame k, int i)
        {
        }

        public virtual void timelineFinished(Timeline t)
        {
            View view = (View)this.parent;
            view?.removeChild(this);
        }

        public virtual void showPopup()
        {
            Application.sharedRootController().deactivateAllButtons();
            this.isShow = true;
            this.playTimeline(0);
        }

        public virtual void hidePopup()
        {
            this.isShow = false;
            this.playTimeline(1);
        }

        public override bool onTouchDownXY(float tx, float ty)
        {
            if (this.isShow)
            {
                base.onTouchDownXY(tx, ty);
            }
            return true;
        }

        public override bool onTouchUpXY(float tx, float ty)
        {
            if (this.isShow)
            {
                base.onTouchUpXY(tx, ty);
            }
            return true;
        }

        public override bool onTouchMoveXY(float tx, float ty)
        {
            if (this.isShow)
            {
                base.onTouchMoveXY(tx, ty);
            }
            return true;
        }

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

        private bool isShow;

        private enum POPUP
        {
            POPUP_SHOW_ANIM,
            POPUP_HIDE_ANIM
        }
    }
}
