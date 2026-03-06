using System.Collections.Generic;

using CutTheRope.Framework.Core;

namespace CutTheRope.Framework.Visual
{
    internal class Animation : Image
    {
        public static Animation Animation_create(CTRTexture2D texture)
        {
            return (Animation)new Animation().InitWithTexture(texture);
        }

        /// <summary>
        /// Creates an animation using a texture resource name.
        /// </summary>
        public static Animation Animation_createWithResID(string resourceName)
        {
            return Animation_create(Application.GetTexture(resourceName));
        }

        public virtual void AddAnimationWithIDDelayLoopFirstLast(
            int animationId,
            float delay,
            Timeline.LoopType loopType,
            int start,
            int end)
        {
            int count = end - start + 1;
            AddAnimationWithIDDelayLoopCountFirstLastArgumentList(animationId, delay, loopType, count, start, end);
        }

        public virtual void AddAnimationWithIDDelayLoopCountFirstLastArgumentList(
            int animationId,
            float delay,
            Timeline.LoopType loopType,
            int count,
            int start,
            int end)
        {
            Timeline timeline = new Timeline().InitWithMaxKeyFramesOnTrack(count + 2);
            timeline.AddKeyFrame(KeyFrame.MakeAction([CTRAction.CreateAction(this, "ACTION_SET_DRAWQUAD", start, 0)], 0f));
            int sequenceIndex = start;
            for (int i = 1; i < count; i++)
            {
                sequenceIndex++;
                List<CTRAction> actions = [CTRAction.CreateAction(this, "ACTION_SET_DRAWQUAD", sequenceIndex, 0)];
                timeline.AddKeyFrame(KeyFrame.MakeAction(actions, delay));
                if (i == count - 1 && loopType == Timeline.LoopType.TIMELINE_REPLAY)
                {
                    timeline.AddKeyFrame(KeyFrame.MakeAction(actions, delay));
                }
            }
            if (loopType != Timeline.LoopType.TIMELINE_NO_LOOP)
            {
                timeline.SetTimelineLoopType(loopType);
            }
            AddTimelinewithID(timeline, animationId);
        }

        public virtual void AddAnimationWithIDDelayLoopCountSequence(
            int animationId,
            float delay,
            Timeline.LoopType loopType,
            int count,
            int start,
            List<int> argumentList)
        {
            AddAnimationWithIDDelayLoopCountFirstLastArgumentList(animationId, delay, loopType, count, start, -1, argumentList);
        }

        public virtual void AddAnimationWithIDDelayLoopCountFirstLastArgumentList(
            int animationId,
            float delay,
            Timeline.LoopType loopType,
            int count,
            int start,
            int end,
            List<int> argumentList)
        {
            Timeline timeline = new Timeline().InitWithMaxKeyFramesOnTrack(count + 2);
            timeline.AddKeyFrame(KeyFrame.MakeAction([CTRAction.CreateAction(this, "ACTION_SET_DRAWQUAD", start, 0)], 0f));
            int argumentIndex = 0;
            for (int i = 1; i < count; i++)
            {
                int sequenceIndex = argumentList[argumentIndex++];
                List<CTRAction> actions = [CTRAction.CreateAction(this, "ACTION_SET_DRAWQUAD", sequenceIndex, 0)];
                timeline.AddKeyFrame(KeyFrame.MakeAction(actions, delay));
                if (i == count - 1 && loopType == Timeline.LoopType.TIMELINE_REPLAY)
                {
                    timeline.AddKeyFrame(KeyFrame.MakeAction(actions, delay));
                }
            }
            if (loopType != Timeline.LoopType.TIMELINE_NO_LOOP)
            {
                timeline.SetTimelineLoopType(loopType);
            }
            AddTimelinewithID(timeline, animationId);
        }

        public virtual void SwitchToAnimationatEndOfAnimationDelay(int targetAnimationId, int sourceAnimationId, float delay)
        {
            GetTimeline(sourceAnimationId).AddKeyFrame(
                KeyFrame.MakeAction([CTRAction.CreateAction(this, "ACTION_PLAY_TIMELINE", 0, targetAnimationId)], delay));
        }

        public virtual void SetPauseAtIndexforAnimation(int keyframeIndex, int animationId)
        {
            SetActionTargetParamSubParamAtIndexforAnimation("ACTION_PAUSE_TIMELINE", this, 0, 0, keyframeIndex, animationId);
        }

        public virtual void SetActionTargetParamSubParamAtIndexforAnimation(
            string action,
            BaseElement target,
            int param,
            int subParam,
            int keyframeIndex,
            int animationId)
        {
            GetTimeline(animationId)
                .GetTrack(Track.TrackType.TRACK_ACTION)
                .keyFrames[keyframeIndex]
                .value
                .action
                .actionSet
                .Add(CTRAction.CreateAction(target, action, param, subParam));
        }

        public virtual int AddAnimationWithDelayLoopedCountSequence(
            float delay,
            Timeline.LoopType loopType,
            int count,
            int start,
            List<int> argumentList)
        {
            int animationId = timelines.Count;
            AddAnimationWithIDDelayLoopCountFirstLastArgumentList(animationId, delay, loopType, count, start, -1, argumentList);
            return animationId;
        }

        public void SetDelayatIndexforAnimation(float delay, int keyframeIndex, int animationId)
        {
            GetTimeline(animationId).GetTrack(Track.TrackType.TRACK_ACTION).keyFrames[keyframeIndex].timeOffset = delay;
        }

        public int AddAnimationDelayLoopFirstLast(float delay, Timeline.LoopType loopType, int start, int end)
        {
            int animationId = timelines.Count;
            AddAnimationWithIDDelayLoopFirstLast(animationId, delay, loopType, start, end);
            return animationId;
        }

        public void JumpTo(int index)
        {
            GetCurrentTimeline().JumpToTrackKeyFrame((int)Track.TrackType.TRACK_ACTION, index);
        }
    }
}
