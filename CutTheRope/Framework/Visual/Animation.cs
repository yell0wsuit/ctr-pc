using System.Collections.Generic;

using CutTheRope.Framework.Core;
using CutTheRope.GameMain;

namespace CutTheRope.Framework.Visual
{
    internal class Animation : Image
    {
        public static Animation Animation_create(CTRTexture2D t)
        {
            return (Animation)new Animation().InitWithTexture(t);
        }

        public static Animation Animation_createWithResID(int r)
        {
            return Animation_create(Application.GetTexture(ResourceNameTranslator.TranslateLegacyId(r)));
        }

        /// <summary>
        /// Creates an animation using a texture resource name.
        /// </summary>
        public static Animation Animation_createWithResID(string resourceName)
        {
            return Animation_create(Application.GetTexture(resourceName));
        }

        public static Animation Animation_createWithResIDQuad(int r, int q)
        {
            Animation animation = Animation_createWithResID(r);
            animation?.SetDrawQuad(q);
            return animation;
        }

        public virtual void AddAnimationWithIDDelayLoopFirstLast(int aid, float d, Timeline.LoopType l, int s, int e)
        {
            int c = e - s + 1;
            AddAnimationWithIDDelayLoopCountFirstLastArgumentList(aid, d, l, c, s, e);
        }

        public virtual void AddAnimationWithIDDelayLoopCountFirstLastArgumentList(int aid, float d, Timeline.LoopType l, int c, int s, int e)
        {
            Timeline timeline = new Timeline().InitWithMaxKeyFramesOnTrack(c + 2);
            timeline.AddKeyFrame(KeyFrame.MakeAction([CTRAction.CreateAction(this, "ACTION_SET_DRAWQUAD", s, 0)], 0f));
            int num = s;
            for (int i = 1; i < c; i++)
            {
                num++;
                List<CTRAction> list = [CTRAction.CreateAction(this, "ACTION_SET_DRAWQUAD", num, 0)];
                timeline.AddKeyFrame(KeyFrame.MakeAction(list, d));
                if (i == c - 1 && l == Timeline.LoopType.TIMELINE_REPLAY)
                {
                    timeline.AddKeyFrame(KeyFrame.MakeAction(list, d));
                }
            }
            if (l != Timeline.LoopType.TIMELINE_NO_LOOP)
            {
                timeline.SetTimelineLoopType(l);
            }
            AddTimelinewithID(timeline, aid);
        }

        public virtual void AddAnimationWithIDDelayLoopCountSequence(int aid, float d, Timeline.LoopType l, int c, int s, List<int> al)
        {
            AddAnimationWithIDDelayLoopCountFirstLastArgumentList(aid, d, l, c, s, -1, al);
        }

        public virtual void AddAnimationWithIDDelayLoopCountFirstLastArgumentList(int aid, float d, Timeline.LoopType l, int c, int s, int e, List<int> al)
        {
            Timeline timeline = new Timeline().InitWithMaxKeyFramesOnTrack(c + 2);
            timeline.AddKeyFrame(KeyFrame.MakeAction([CTRAction.CreateAction(this, "ACTION_SET_DRAWQUAD", s, 0)], 0f));
            int num2 = 0;
            for (int i = 1; i < c; i++)
            {
                int num3 = al[num2++];
                List<CTRAction> list = [CTRAction.CreateAction(this, "ACTION_SET_DRAWQUAD", num3, 0)];
                timeline.AddKeyFrame(KeyFrame.MakeAction(list, d));
                if (i == c - 1 && l == Timeline.LoopType.TIMELINE_REPLAY)
                {
                    timeline.AddKeyFrame(KeyFrame.MakeAction(list, d));
                }
            }
            if (l != Timeline.LoopType.TIMELINE_NO_LOOP)
            {
                timeline.SetTimelineLoopType(l);
            }
            AddTimelinewithID(timeline, aid);
        }

        public virtual void SwitchToAnimationatEndOfAnimationDelay(int a2, int a1, float d)
        {
            GetTimeline(a1).AddKeyFrame(KeyFrame.MakeAction([CTRAction.CreateAction(this, "ACTION_PLAY_TIMELINE", 0, a2)], d));
        }

        public virtual void SetPauseAtIndexforAnimation(int i, int a)
        {
            SetActionTargetParamSubParamAtIndexforAnimation("ACTION_PAUSE_TIMELINE", this, 0, 0, i, a);
        }

        public virtual void SetActionTargetParamSubParamAtIndexforAnimation(string action, BaseElement target, int p, int sp, int i, int a)
        {
            GetTimeline(a).GetTrack(Track.TrackType.TRACK_ACTION).keyFrames[i].value.action.actionSet.Add(CTRAction.CreateAction(target, action, p, sp));
        }

        public virtual int AddAnimationWithDelayLoopedCountSequence(float d, Timeline.LoopType l, int c, int s, List<int> al)
        {
            int count = timelines.Count;
            AddAnimationWithIDDelayLoopCountFirstLastArgumentList(count, d, l, c, s, -1, al);
            return count;
        }

        public void SetDelayatIndexforAnimation(float d, int i, int a)
        {
            GetTimeline(a).GetTrack(Track.TrackType.TRACK_ACTION).keyFrames[i].timeOffset = d;
        }

        public int AddAnimationDelayLoopFirstLast(double d, Timeline.LoopType l, int s, int e)
        {
            return AddAnimationDelayLoopFirstLast((float)d, l, s, e);
        }

        public int AddAnimationDelayLoopFirstLast(float d, Timeline.LoopType l, int s, int e)
        {
            int count = timelines.Count;
            AddAnimationWithIDDelayLoopFirstLast(count, d, l, s, e);
            return count;
        }

        public void JumpTo(int i)
        {
            GetCurrentTimeline().JumpToTrackKeyFrame(4, i);
        }
    }
}
