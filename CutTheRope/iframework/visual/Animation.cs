using CutTheRope.iframework.core;
using System;
using System.Collections.Generic;

namespace CutTheRope.iframework.visual
{
    internal class Animation : Image
    {
        public static Animation Animation_create(Texture2D t)
        {
            return (Animation)new Animation().initWithTexture(t);
        }

        public static Animation Animation_createWithResID(int r)
        {
            return Animation.Animation_create(Application.getTexture(r));
        }

        public static Animation Animation_createWithResIDQuad(int r, int q)
        {
            Animation animation = Animation.Animation_createWithResID(r);
            animation?.setDrawQuad(q);
            return animation;
        }

        public virtual void addAnimationWithIDDelayLoopFirstLast(int aid, float d, Timeline.LoopType l, int s, int e)
        {
            int c = e - s + 1;
            this.addAnimationWithIDDelayLoopCountFirstLastArgumentList(aid, d, l, c, s, e);
        }

        public virtual void addAnimationWithIDDelayLoopCountFirstLastArgumentList(int aid, float d, Timeline.LoopType l, int c, int s, int e)
        {
            Timeline timeline = new Timeline().initWithMaxKeyFramesOnTrack(c + 2);
            timeline.addKeyFrame(KeyFrame.makeAction(new List<Action> { Action.createAction(this, "ACTION_SET_DRAWQUAD", s, 0) }, 0f));
            int num = s;
            for (int i = 1; i < c; i++)
            {
                num++;
                List<Action> list = new();
                list.Add(Action.createAction(this, "ACTION_SET_DRAWQUAD", num, 0));
                timeline.addKeyFrame(KeyFrame.makeAction(list, d));
                if (i == c - 1 && l == Timeline.LoopType.TIMELINE_REPLAY)
                {
                    timeline.addKeyFrame(KeyFrame.makeAction(list, d));
                }
            }
            if (l != Timeline.LoopType.TIMELINE_NO_LOOP)
            {
                timeline.setTimelineLoopType(l);
            }
            this.addTimelinewithID(timeline, aid);
        }

        public virtual void addAnimationWithIDDelayLoopCountSequence(int aid, float d, Timeline.LoopType l, int c, int s, List<int> al)
        {
            this.addAnimationWithIDDelayLoopCountFirstLastArgumentList(aid, d, l, c, s, -1, al);
        }

        public virtual void addAnimationWithIDDelayLoopCountFirstLastArgumentList(int aid, float d, Timeline.LoopType l, int c, int s, int e, List<int> al)
        {
            Timeline timeline = new Timeline().initWithMaxKeyFramesOnTrack(c + 2);
            timeline.addKeyFrame(KeyFrame.makeAction(new List<Action> { Action.createAction(this, "ACTION_SET_DRAWQUAD", s, 0) }, 0f));
            int num2 = 0;
            for (int i = 1; i < c; i++)
            {
                int num3 = al[num2++];
                List<Action> list = new();
                list.Add(Action.createAction(this, "ACTION_SET_DRAWQUAD", num3, 0));
                timeline.addKeyFrame(KeyFrame.makeAction(list, d));
                if (i == c - 1 && l == Timeline.LoopType.TIMELINE_REPLAY)
                {
                    timeline.addKeyFrame(KeyFrame.makeAction(list, d));
                }
            }
            if (l != Timeline.LoopType.TIMELINE_NO_LOOP)
            {
                timeline.setTimelineLoopType(l);
            }
            this.addTimelinewithID(timeline, aid);
        }

        public virtual void switchToAnimationatEndOfAnimationDelay(int a2, int a1, float d)
        {
            this.getTimeline(a1).addKeyFrame(KeyFrame.makeAction(new List<Action> { Action.createAction(this, "ACTION_PLAY_TIMELINE", 0, a2) }, d));
        }

        public virtual void setPauseAtIndexforAnimation(int i, int a)
        {
            this.setActionTargetParamSubParamAtIndexforAnimation("ACTION_PAUSE_TIMELINE", this, 0, 0, i, a);
        }

        public virtual void setActionTargetParamSubParamAtIndexforAnimation(string action, BaseElement target, int p, int sp, int i, int a)
        {
            this.getTimeline(a).getTrack(Track.TrackType.TRACK_ACTION).keyFrames[i].value.action.actionSet.Add(Action.createAction(target, action, p, sp));
        }

        public virtual int addAnimationWithDelayLoopedCountSequence(float d, Timeline.LoopType l, int c, int s, List<int> al)
        {
            int count = this.timelines.Count;
            this.addAnimationWithIDDelayLoopCountFirstLastArgumentList(count, d, l, c, s, -1, al);
            return count;
        }

        public void setDelayatIndexforAnimation(float d, int i, int a)
        {
            this.getTimeline(a).getTrack(Track.TrackType.TRACK_ACTION).keyFrames[i].timeOffset = d;
        }

        public int addAnimationDelayLoopFirstLast(double d, Timeline.LoopType l, int s, int e)
        {
            return this.addAnimationDelayLoopFirstLast((float)d, l, s, e);
        }

        public int addAnimationDelayLoopFirstLast(float d, Timeline.LoopType l, int s, int e)
        {
            int count = this.timelines.Count;
            this.addAnimationWithIDDelayLoopFirstLast(count, d, l, s, e);
            return count;
        }

        public void jumpTo(int i)
        {
            this.getCurrentTimeline().jumpToTrackKeyFrame(4, i);
        }
    }
}
