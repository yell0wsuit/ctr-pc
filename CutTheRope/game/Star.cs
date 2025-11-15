using CutTheRope.iframework;
using CutTheRope.iframework.core;
using CutTheRope.iframework.helpers;
using CutTheRope.iframework.visual;

namespace CutTheRope.game
{
    internal sealed class Star : CTRGameObject
    {
        public static Star Star_create(CTRTexture2D t)
        {
            return (Star)new Star().InitWithTexture(t);
        }

        public static Star Star_createWithResID(int r)
        {
            return Star_create(Application.GetTexture(r));
        }

        public static Star Star_createWithResIDQuad(int r, int q)
        {
            Star star = Star_create(Application.GetTexture(r));
            star.SetDrawQuad(q);
            return star;
        }

        public Star()
        {
            timedAnim = null;
        }

        public override void Update(float delta)
        {
            if (timeout > 0.0 && time > 0.0)
            {
                _ = Mover.MoveVariableToTarget(ref time, 0f, 1f, delta);
            }
            base.Update(delta);
        }

        public override void Draw()
        {
            timedAnim?.Draw();
            base.Draw();
        }

        public void CreateAnimations()
        {
            if (timeout > 0.0)
            {
                timedAnim = Animation_createWithResID(78);
                timedAnim.anchor = timedAnim.parentAnchor = 18;
                float d = timeout / 37f;
                timedAnim.AddAnimationWithIDDelayLoopFirstLast(0, d, Timeline.LoopType.TIMELINE_NO_LOOP, 19, 55);
                timedAnim.PlayTimeline(0);
                time = timeout;
                timedAnim.visible = false;
                _ = AddChild(timedAnim);
                Timeline timeline = new Timeline().InitWithMaxKeyFramesOnTrack(2);
                timeline.AddKeyFrame(KeyFrame.MakeColor(RGBAColor.solidOpaqueRGBA, KeyFrame.TransitionType.FRAME_TRANSITION_LINEAR, 0.0));
                timeline.AddKeyFrame(KeyFrame.MakeColor(RGBAColor.transparentRGBA, KeyFrame.TransitionType.FRAME_TRANSITION_LINEAR, 0.5));
                timedAnim.AddTimelinewithID(timeline, 1);
                Timeline timeline2 = new Timeline().InitWithMaxKeyFramesOnTrack(2);
                timeline2.AddKeyFrame(KeyFrame.MakeScale(1.0, 1.0, KeyFrame.TransitionType.FRAME_TRANSITION_LINEAR, 0.0));
                timeline2.AddKeyFrame(KeyFrame.MakeScale(0.0, 0.0, KeyFrame.TransitionType.FRAME_TRANSITION_LINEAR, 0.25));
                timeline2.AddKeyFrame(KeyFrame.MakeColor(RGBAColor.solidOpaqueRGBA, KeyFrame.TransitionType.FRAME_TRANSITION_LINEAR, 0.0));
                timeline2.AddKeyFrame(KeyFrame.MakeColor(RGBAColor.transparentRGBA, KeyFrame.TransitionType.FRAME_TRANSITION_LINEAR, 0.25));
                AddTimelinewithID(timeline2, 1);
            }
            bb = new CTRRectangle(22f, 20f, 30f, 30f);
            Timeline timeline3 = new Timeline().InitWithMaxKeyFramesOnTrack(5);
            timeline3.AddKeyFrame(KeyFrame.MakePos((int)x, (int)y, KeyFrame.TransitionType.FRAME_TRANSITION_EASE_IN, 0f));
            timeline3.AddKeyFrame(KeyFrame.MakePos((int)x, (int)y - 3, KeyFrame.TransitionType.FRAME_TRANSITION_EASE_OUT, 0.5f));
            timeline3.AddKeyFrame(KeyFrame.MakePos((int)x, (int)y, KeyFrame.TransitionType.FRAME_TRANSITION_EASE_IN, 0.5f));
            timeline3.AddKeyFrame(KeyFrame.MakePos((int)x, (int)y + 3, KeyFrame.TransitionType.FRAME_TRANSITION_EASE_OUT, 0.5f));
            timeline3.AddKeyFrame(KeyFrame.MakePos((int)x, (int)y, KeyFrame.TransitionType.FRAME_TRANSITION_EASE_IN, 0.5f));
            timeline3.SetTimelineLoopType(Timeline.LoopType.TIMELINE_REPLAY);
            AddTimelinewithID(timeline3, 0);
            PlayTimeline(0);
            Timeline.UpdateTimeline(timeline3, (float)(RND_RANGE(0, 20) / 10.0));
            Animation animation = Animation_createWithResID(78);
            animation.DoRestoreCutTransparency();
            _ = animation.AddAnimationDelayLoopFirstLast(0.05f, Timeline.LoopType.TIMELINE_REPLAY, 1, 18);
            animation.PlayTimeline(0);
            Timeline.UpdateTimeline(animation.GetTimeline(0), (float)(RND_RANGE(0, 20) / 10.0));
            animation.anchor = animation.parentAnchor = 18;
            _ = AddChild(animation);
        }

        public float time;

        public float timeout;

        public Animation timedAnim;
    }
}
