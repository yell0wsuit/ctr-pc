using CutTheRope.iframework;
using CutTheRope.iframework.core;
using CutTheRope.iframework.helpers;
using CutTheRope.iframework.visual;
using CutTheRope.ios;
using System;

namespace CutTheRope.game
{
    internal class Star : CTRGameObject
    {
        public static Star Star_create(CTRTexture2D t)
        {
            return (Star)new Star().initWithTexture(t);
        }

        public static Star Star_createWithResID(int r)
        {
            return Star.Star_create(Application.getTexture(r));
        }

        public static Star Star_createWithResIDQuad(int r, int q)
        {
            Star star = Star.Star_create(Application.getTexture(r));
            star.setDrawQuad(q);
            return star;
        }

        public override NSObject init()
        {
            if (base.init() != null)
            {
                this.timedAnim = null;
            }
            return this;
        }

        public override void update(float delta)
        {
            if ((double)this.timeout > 0.0 && (double)this.time > 0.0)
            {
                Mover.moveVariableToTarget(ref this.time, 0f, 1f, delta);
            }
            base.update(delta);
        }

        public override void draw()
        {
            this.timedAnim?.draw();
            base.draw();
        }

        public virtual void createAnimations()
        {
            if ((double)this.timeout > 0.0)
            {
                this.timedAnim = Animation.Animation_createWithResID(78);
                this.timedAnim.anchor = this.timedAnim.parentAnchor = 18;
                float d = this.timeout / 37f;
                this.timedAnim.addAnimationWithIDDelayLoopFirstLast(0, d, Timeline.LoopType.TIMELINE_NO_LOOP, 19, 55);
                this.timedAnim.playTimeline(0);
                this.time = this.timeout;
                this.timedAnim.visible = false;
                this.addChild(this.timedAnim);
                Timeline timeline = new Timeline().initWithMaxKeyFramesOnTrack(2);
                timeline.addKeyFrame(KeyFrame.makeColor(RGBAColor.solidOpaqueRGBA, KeyFrame.TransitionType.FRAME_TRANSITION_LINEAR, 0.0));
                timeline.addKeyFrame(KeyFrame.makeColor(RGBAColor.transparentRGBA, KeyFrame.TransitionType.FRAME_TRANSITION_LINEAR, 0.5));
                this.timedAnim.addTimelinewithID(timeline, 1);
                Timeline timeline2 = new Timeline().initWithMaxKeyFramesOnTrack(2);
                timeline2.addKeyFrame(KeyFrame.makeScale(1.0, 1.0, KeyFrame.TransitionType.FRAME_TRANSITION_LINEAR, 0.0));
                timeline2.addKeyFrame(KeyFrame.makeScale(0.0, 0.0, KeyFrame.TransitionType.FRAME_TRANSITION_LINEAR, 0.25));
                timeline2.addKeyFrame(KeyFrame.makeColor(RGBAColor.solidOpaqueRGBA, KeyFrame.TransitionType.FRAME_TRANSITION_LINEAR, 0.0));
                timeline2.addKeyFrame(KeyFrame.makeColor(RGBAColor.transparentRGBA, KeyFrame.TransitionType.FRAME_TRANSITION_LINEAR, 0.25));
                this.addTimelinewithID(timeline2, 1);
            }
            this.bb = new CTRRectangle(22f, 20f, 30f, 30f);
            Timeline timeline3 = new Timeline().initWithMaxKeyFramesOnTrack(5);
            timeline3.addKeyFrame(KeyFrame.makePos((int)this.x, (int)this.y, KeyFrame.TransitionType.FRAME_TRANSITION_EASE_IN, 0f));
            timeline3.addKeyFrame(KeyFrame.makePos((int)this.x, (int)this.y - 3, KeyFrame.TransitionType.FRAME_TRANSITION_EASE_OUT, 0.5f));
            timeline3.addKeyFrame(KeyFrame.makePos((int)this.x, (int)this.y, KeyFrame.TransitionType.FRAME_TRANSITION_EASE_IN, 0.5f));
            timeline3.addKeyFrame(KeyFrame.makePos((int)this.x, (int)this.y + 3, KeyFrame.TransitionType.FRAME_TRANSITION_EASE_OUT, 0.5f));
            timeline3.addKeyFrame(KeyFrame.makePos((int)this.x, (int)this.y, KeyFrame.TransitionType.FRAME_TRANSITION_EASE_IN, 0.5f));
            timeline3.setTimelineLoopType(Timeline.LoopType.TIMELINE_REPLAY);
            this.addTimelinewithID(timeline3, 0);
            this.playTimeline(0);
            Timeline.updateTimeline(timeline3, (float)((double)CTRMathHelper.RND_RANGE(0, 20) / 10.0));
            Animation animation = Animation.Animation_createWithResID(78);
            animation.doRestoreCutTransparency();
            animation.addAnimationDelayLoopFirstLast(0.05f, Timeline.LoopType.TIMELINE_REPLAY, 1, 18);
            animation.playTimeline(0);
            Timeline.updateTimeline(animation.getTimeline(0), (float)((double)CTRMathHelper.RND_RANGE(0, 20) / 10.0));
            animation.anchor = animation.parentAnchor = 18;
            this.addChild(animation);
        }

        public float time;

        public float timeout;

        public Animation timedAnim;
    }
}
