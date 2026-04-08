using System.Collections.Generic;

using CutTheRope.Framework.Core;

namespace CutTheRope.Framework.Visual
{
    /// <summary>
    /// An <see cref="Image"/> that supports frame-based animation via timeline-driven quad switching.
    /// </summary>
    internal class Animation : Image
    {
        /// <summary>
        /// Creates an animation from the specified <paramref name="texture"/>.
        /// </summary>
        /// <param name="texture">Texture to create the animation from.</param>
        /// <returns>A new animation initialized with <paramref name="texture"/>.</returns>
        public static Animation Animation_create(CTRTexture2D texture)
        {
            return (Animation)new Animation().InitWithTexture(texture);
        }

        /// <summary>
        /// Creates an animation using a texture resource name.
        /// </summary>
        /// <param name="resourceName">Texture resource name.</param>
        /// <returns>A new animation initialized from the specified texture resource.</returns>
        public static Animation Animation_createWithResID(string resourceName)
        {
            return Animation_create(Application.GetTexture(resourceName));
        }

        /// <summary>
        /// Adds a sequential frame animation from quad <paramref name="start"/> to <paramref name="end"/>.
        /// </summary>
        /// <param name="animationId">Timeline slot ID for the animation.</param>
        /// <param name="delay">Delay in seconds between frames.</param>
        /// <param name="loopType">Loop behavior for the animation.</param>
        /// <param name="start">First quad index in the sequence.</param>
        /// <param name="end">Last quad index in the sequence.</param>
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

        /// <summary>
        /// Adds a sequential frame animation with explicit frame <paramref name="count"/>.
        /// </summary>
        /// <param name="animationId">Timeline slot ID for the animation.</param>
        /// <param name="delay">Delay in seconds between frames.</param>
        /// <param name="loopType">Loop behavior for the animation.</param>
        /// <param name="count">Number of frames in the animation.</param>
        /// <param name="start">First quad index in the sequence.</param>
        /// <param name="end">Last quad index in the sequence.</param>
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

        /// <summary>
        /// Adds an animation with an explicit frame sequence list.
        /// </summary>
        /// <param name="animationId">Timeline slot ID for the animation.</param>
        /// <param name="delay">Delay in seconds between frames.</param>
        /// <param name="loopType">Loop behavior for the animation.</param>
        /// <param name="count">Number of frames in the animation.</param>
        /// <param name="start">First quad index in the sequence.</param>
        /// <param name="argumentList">List of quad indices defining the frame order.</param>
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

        /// <summary>
        /// Adds a frame animation with an explicit frame sequence list and frame range.
        /// </summary>
        /// <param name="animationId">Timeline slot ID for the animation.</param>
        /// <param name="delay">Delay in seconds between frames.</param>
        /// <param name="loopType">Loop behavior for the animation.</param>
        /// <param name="count">Number of frames in the animation.</param>
        /// <param name="start">First quad index in the sequence.</param>
        /// <param name="end">Last quad index in the sequence.</param>
        /// <param name="argumentList">List of quad indices defining the frame order.</param>
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

        /// <summary>
        /// Appends a keyframe to <paramref name="sourceAnimationId"/> that switches to <paramref name="targetAnimationId"/> after <paramref name="delay"/>.
        /// </summary>
        /// <param name="targetAnimationId">Animation to switch to.</param>
        /// <param name="sourceAnimationId">Animation to append the switch keyframe to.</param>
        /// <param name="delay">Delay in seconds before switching.</param>
        public virtual void SwitchToAnimationatEndOfAnimationDelay(int targetAnimationId, int sourceAnimationId, float delay)
        {
            GetTimeline(sourceAnimationId).AddKeyFrame(
                KeyFrame.MakeAction([CTRAction.CreateAction(this, "ACTION_PLAY_TIMELINE", 0, targetAnimationId)], delay));
        }

        /// <summary>
        /// Inserts a pause action at the specified keyframe index in the given animation.
        /// </summary>
        /// <param name="keyframeIndex">Index of the keyframe to add the pause to.</param>
        /// <param name="animationId">Timeline slot ID of the animation.</param>
        public virtual void SetPauseAtIndexforAnimation(int keyframeIndex, int animationId)
        {
            SetActionTargetParamSubParamAtIndexforAnimation("ACTION_PAUSE_TIMELINE", this, 0, 0, keyframeIndex, animationId);
        }

        /// <summary>
        /// Appends an <paramref name="action"/> to an existing keyframe in the specified animation.
        /// </summary>
        /// <param name="action">Action name to add.</param>
        /// <param name="target">Target element for the action.</param>
        /// <param name="param">Primary action parameter.</param>
        /// <param name="subParam">Secondary action parameter.</param>
        /// <param name="keyframeIndex">Index of the keyframe to append to.</param>
        /// <param name="animationId">Timeline slot ID of the animation.</param>
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

        /// <summary>
        /// Adds an animation with a frame sequence and returns its auto-assigned ID.
        /// </summary>
        /// <param name="delay">Delay in seconds between frames.</param>
        /// <param name="loopType">Loop behavior for the animation.</param>
        /// <param name="count">Number of frames in the animation.</param>
        /// <param name="start">First quad index in the sequence.</param>
        /// <param name="argumentList">List of quad indices defining the frame order.</param>
        /// <returns>The auto-assigned timeline ID.</returns>
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

        /// <summary>
        /// Sets the time offset of a keyframe in the specified animation.
        /// </summary>
        /// <param name="delay">New delay in seconds.</param>
        /// <param name="keyframeIndex">Index of the keyframe to modify.</param>
        /// <param name="animationId">Timeline slot ID of the animation.</param>
        public void SetDelayatIndexforAnimation(float delay, int keyframeIndex, int animationId)
        {
            GetTimeline(animationId).GetTrack(Track.TrackType.TRACK_ACTION).keyFrames[keyframeIndex].timeOffset = delay;
        }

        /// <summary>
        /// Adds a sequential frame animation and returns its auto-assigned ID.
        /// </summary>
        /// <param name="delay">Delay in seconds between frames.</param>
        /// <param name="loopType">Loop behavior for the animation.</param>
        /// <param name="start">First quad index in the sequence.</param>
        /// <param name="end">Last quad index in the sequence.</param>
        /// <returns>The auto-assigned timeline ID.</returns>
        public int AddAnimationDelayLoopFirstLast(float delay, Timeline.LoopType loopType, int start, int end)
        {
            int animationId = timelines.Count;
            AddAnimationWithIDDelayLoopFirstLast(animationId, delay, loopType, start, end);
            return animationId;
        }

        /// <summary>
        /// Jumps the current timeline's action track to the specified keyframe <paramref name="index"/>.
        /// </summary>
        /// <param name="index">Keyframe index to jump to.</param>
        public void JumpTo(int index)
        {
            GetCurrentTimeline().JumpToTrackKeyFrame((int)Track.TrackType.TRACK_ACTION, index);
        }
    }
}
