using System;
using System.Collections.Generic;

namespace CutTheRope.Framework.Visual
{
    internal sealed class Track : FrameworkTypes
    {
        public Track()
        {
            elementPrevState = new KeyFrame();
            currentStepPerSecond = new KeyFrame();
            currentStepAcceleration = new KeyFrame();
            currentStepSource = new KeyFrame();
            currentStepDestination = new KeyFrame();
        }

        public Track InitWithTimelineTypeandMaxKeyFrames(Timeline timeline, TrackType trackType, int m)
        {
            t = timeline;
            type = trackType;
            state = TrackState.TRACK_NOT_ACTIVE;
            relative = false;
            nextKeyFrame = -1;
            keyFramesCount = 0;
            keyFramesCapacity = m;
            keyFrames = new KeyFrame[keyFramesCapacity];
            if (type == TrackType.TRACK_ACTION)
            {
                actionSets = [];
            }
            return this;
        }

        public void InitActionKeyFrameandTime(KeyFrame kf, float time)
        {
            keyFrameTimeLeft = time;
            SetElementFromKeyFrame(kf);
            if (overrun > 0f)
            {
                UpdateActionTrack(this, overrun);
                overrun = 0f;
            }
        }

        public void SetKeyFrameAt(KeyFrame k, int i)
        {
            keyFrames[i] = k;
            if (i >= keyFramesCount)
            {
                keyFramesCount = i + 1;
            }
            if (type == TrackType.TRACK_ACTION)
            {
                actionSets.Add(k.value.action.actionSet);
            }
        }

        public float GetFrameTime(int f)
        {
            float totalTime = 0f;
            for (int i = 0; i <= f; i++)
            {
                totalTime += keyFrames[i].timeOffset;
            }
            return totalTime;
        }

        public void UpdateRange()
        {
            startTime = GetFrameTime(0);
            endTime = GetFrameTime(keyFramesCount - 1);
        }

        private void InitKeyFrameStepFromTowithTime(KeyFrame src, KeyFrame dst, float time)
        {
            keyFrameTimeLeft = time;
            keyFrameDuration = time;
            keyFrameElapsed = 0f;
            SetKeyFrameFromElement(elementPrevState);
            CopyTrackValue(src, currentStepSource);
            CopyTrackValue(dst, currentStepDestination);
            SetElementFromKeyFrame(src);
            float duration = keyFrameTimeLeft <= 1E-06f ? 1E-06f : keyFrameTimeLeft;
            switch (type)
            {
                case TrackType.TRACK_POSITION:
                    currentStepPerSecond.value.pos.x = (dst.value.pos.x - src.value.pos.x) / duration;
                    currentStepPerSecond.value.pos.y = (dst.value.pos.y - src.value.pos.y) / duration;
                    break;
                case TrackType.TRACK_SCALE:
                    currentStepPerSecond.value.scale.scaleX = (dst.value.scale.scaleX - src.value.scale.scaleX) / duration;
                    currentStepPerSecond.value.scale.scaleY = (dst.value.scale.scaleY - src.value.scale.scaleY) / duration;
                    break;
                case TrackType.TRACK_ROTATION:
                    currentStepPerSecond.value.rotation.angle = (dst.value.rotation.angle - src.value.rotation.angle) / duration;
                    break;
                case TrackType.TRACK_SKEW:
                    currentStepPerSecond.value.skew.skewX = (dst.value.skew.skewX - src.value.skew.skewX) / duration;
                    currentStepPerSecond.value.skew.skewY = (dst.value.skew.skewY - src.value.skew.skewY) / duration;
                    break;
                case TrackType.TRACK_COLOR:
                    currentStepPerSecond.value.color.rgba.RedColor = (dst.value.color.rgba.RedColor - src.value.color.rgba.RedColor) / duration;
                    currentStepPerSecond.value.color.rgba.GreenColor = (dst.value.color.rgba.GreenColor - src.value.color.rgba.GreenColor) / duration;
                    currentStepPerSecond.value.color.rgba.BlueColor = (dst.value.color.rgba.BlueColor - src.value.color.rgba.BlueColor) / duration;
                    currentStepPerSecond.value.color.rgba.AlphaChannel = (dst.value.color.rgba.AlphaChannel - src.value.color.rgba.AlphaChannel) / duration;
                    break;
                case TrackType.TRACK_ACTION:
                    break;
                case TrackType.TRACKS_COUNT:
                    break;
                default:
                    break;
            }
            if (dst.transitionType is KeyFrame.TransitionType.FRAME_TRANSITION_EASE_IN or KeyFrame.TransitionType.FRAME_TRANSITION_EASE_OUT)
            {
                switch (type)
                {
                    case TrackType.TRACK_POSITION:
                        currentStepPerSecond.value.pos.x *= 2f;
                        currentStepPerSecond.value.pos.y *= 2f;
                        currentStepAcceleration.value.pos.x = currentStepPerSecond.value.pos.x / duration;
                        currentStepAcceleration.value.pos.y = currentStepPerSecond.value.pos.y / duration;
                        if (dst.transitionType == KeyFrame.TransitionType.FRAME_TRANSITION_EASE_IN)
                        {
                            currentStepPerSecond.value.pos.x = 0f;
                            currentStepPerSecond.value.pos.y = 0f;
                        }
                        else
                        {
                            currentStepAcceleration.value.pos.x *= -1f;
                            currentStepAcceleration.value.pos.y *= -1f;
                        }
                        break;
                    case TrackType.TRACK_SCALE:
                        currentStepPerSecond.value.scale.scaleX *= 2f;
                        currentStepPerSecond.value.scale.scaleY *= 2f;
                        currentStepAcceleration.value.scale.scaleX = currentStepPerSecond.value.scale.scaleX / duration;
                        currentStepAcceleration.value.scale.scaleY = currentStepPerSecond.value.scale.scaleY / duration;
                        if (dst.transitionType == KeyFrame.TransitionType.FRAME_TRANSITION_EASE_IN)
                        {
                            currentStepPerSecond.value.scale.scaleX = 0f;
                            currentStepPerSecond.value.scale.scaleY = 0f;
                        }
                        else
                        {
                            currentStepAcceleration.value.scale.scaleX *= -1f;
                            currentStepAcceleration.value.scale.scaleY *= -1f;
                        }
                        break;
                    case TrackType.TRACK_ROTATION:
                        currentStepPerSecond.value.rotation.angle *= 2f;
                        currentStepAcceleration.value.rotation.angle = currentStepPerSecond.value.rotation.angle / duration;
                        if (dst.transitionType == KeyFrame.TransitionType.FRAME_TRANSITION_EASE_IN)
                        {
                            currentStepPerSecond.value.rotation.angle = 0f;
                        }
                        else
                        {
                            currentStepAcceleration.value.rotation.angle *= -1f;
                        }
                        break;
                    case TrackType.TRACK_SKEW:
                        currentStepPerSecond.value.skew.skewX *= 2f;
                        currentStepPerSecond.value.skew.skewY *= 2f;
                        currentStepAcceleration.value.skew.skewX = currentStepPerSecond.value.skew.skewX / duration;
                        currentStepAcceleration.value.skew.skewY = currentStepPerSecond.value.skew.skewY / duration;
                        if (dst.transitionType == KeyFrame.TransitionType.FRAME_TRANSITION_EASE_IN)
                        {
                            currentStepPerSecond.value.skew.skewX = 0f;
                            currentStepPerSecond.value.skew.skewY = 0f;
                        }
                        else
                        {
                            currentStepAcceleration.value.skew.skewX *= -1f;
                            currentStepAcceleration.value.skew.skewY *= -1f;
                        }
                        break;
                    case TrackType.TRACK_COLOR:
                        {
                            ColorParams color = currentStepPerSecond.value.color;
                            color.rgba.RedColor *= 2f;
                            ColorParams color2 = currentStepPerSecond.value.color;
                            color2.rgba.GreenColor *= 2f;
                            ColorParams color3 = currentStepPerSecond.value.color;
                            color3.rgba.BlueColor *= 2f;
                            ColorParams color4 = currentStepPerSecond.value.color;
                            color4.rgba.AlphaChannel *= 2f;
                            currentStepAcceleration.value.color.rgba.RedColor = currentStepPerSecond.value.color.rgba.RedColor / duration;
                            currentStepAcceleration.value.color.rgba.GreenColor = currentStepPerSecond.value.color.rgba.GreenColor / duration;
                            currentStepAcceleration.value.color.rgba.BlueColor = currentStepPerSecond.value.color.rgba.BlueColor / duration;
                            currentStepAcceleration.value.color.rgba.AlphaChannel = currentStepPerSecond.value.color.rgba.AlphaChannel / duration;
                            if (dst.transitionType == KeyFrame.TransitionType.FRAME_TRANSITION_EASE_IN)
                            {
                                currentStepPerSecond.value.color.rgba.RedColor = 0f;
                                currentStepPerSecond.value.color.rgba.GreenColor = 0f;
                                currentStepPerSecond.value.color.rgba.BlueColor = 0f;
                                currentStepPerSecond.value.color.rgba.AlphaChannel = 0f;
                            }
                            else
                            {
                                ColorParams color5 = currentStepAcceleration.value.color;
                                color5.rgba.RedColor *= -1f;
                                ColorParams color6 = currentStepAcceleration.value.color;
                                color6.rgba.GreenColor *= -1f;
                                ColorParams color7 = currentStepAcceleration.value.color;
                                color7.rgba.BlueColor *= -1f;
                                ColorParams color8 = currentStepAcceleration.value.color;
                                color8.rgba.AlphaChannel *= -1f;
                            }
                            break;
                        }

                    case TrackType.TRACK_ACTION:
                        break;
                    case TrackType.TRACKS_COUNT:
                        break;
                    default:
                        break;
                }
            }
            if (overrun > 0f)
            {
                UpdateTrack(this, overrun);
                overrun = 0f;
            }
        }

        public void SetElementFromKeyFrame(KeyFrame kf)
        {
            switch (type)
            {
                case TrackType.TRACK_POSITION:
                    if (!relative)
                    {
                        t.element.x = kf.value.pos.x;
                        t.element.y = kf.value.pos.y;
                        return;
                    }
                    t.element.x = elementPrevState.value.pos.x + kf.value.pos.x;
                    t.element.y = elementPrevState.value.pos.y + kf.value.pos.y;
                    return;
                case TrackType.TRACK_SCALE:
                    if (!relative)
                    {
                        t.element.scaleX = kf.value.scale.scaleX;
                        t.element.scaleY = kf.value.scale.scaleY;
                        return;
                    }
                    t.element.scaleX = elementPrevState.value.scale.scaleX + kf.value.scale.scaleX;
                    t.element.scaleY = elementPrevState.value.scale.scaleY + kf.value.scale.scaleY;
                    return;
                case TrackType.TRACK_ROTATION:
                    if (!relative)
                    {
                        t.element.rotation = kf.value.rotation.angle;
                        return;
                    }
                    t.element.rotation = elementPrevState.value.rotation.angle + kf.value.rotation.angle;
                    return;
                case TrackType.TRACK_SKEW:
                    if (!relative)
                    {
                        t.element.skewX = kf.value.skew.skewX;
                        t.element.skewY = kf.value.skew.skewY;
                        return;
                    }
                    t.element.skewX = elementPrevState.value.skew.skewX + kf.value.skew.skewX;
                    t.element.skewY = elementPrevState.value.skew.skewY + kf.value.skew.skewY;
                    return;
                case TrackType.TRACK_COLOR:
                    if (!relative)
                    {
                        t.element.color = kf.value.color.rgba;
                        return;
                    }
                    t.element.color.RedColor = elementPrevState.value.color.rgba.RedColor + kf.value.color.rgba.RedColor;
                    t.element.color.GreenColor = elementPrevState.value.color.rgba.GreenColor + kf.value.color.rgba.GreenColor;
                    t.element.color.BlueColor = elementPrevState.value.color.rgba.BlueColor + kf.value.color.rgba.BlueColor;
                    t.element.color.AlphaChannel = elementPrevState.value.color.rgba.AlphaChannel + kf.value.color.rgba.AlphaChannel;
                    return;
                case TrackType.TRACK_ACTION:
                    {
                        for (int i = 0; i < kf.value.action.actionSet.Count; i++)
                        {
                            CTRAction action = kf.value.action.actionSet[i];
                            _ = action.actionTarget.HandleAction(action.data);
                        }
                        return;
                    }

                case TrackType.TRACKS_COUNT:
                    break;
                default:
                    return;
            }
        }

        private void SetKeyFrameFromElement(KeyFrame kf)
        {
            switch (type)
            {
                case TrackType.TRACK_POSITION:
                    kf.value.pos.x = t.element.x;
                    kf.value.pos.y = t.element.y;
                    return;
                case TrackType.TRACK_SCALE:
                    kf.value.scale.scaleX = t.element.scaleX;
                    kf.value.scale.scaleY = t.element.scaleY;
                    return;
                case TrackType.TRACK_ROTATION:
                    kf.value.rotation.angle = t.element.rotation;
                    return;
                case TrackType.TRACK_SKEW:
                    kf.value.skew.skewX = t.element.skewX;
                    kf.value.skew.skewY = t.element.skewY;
                    return;
                case TrackType.TRACK_COLOR:
                    kf.value.color.rgba = t.element.color;
                    break;
                case TrackType.TRACK_ACTION:
                    break;
                case TrackType.TRACKS_COUNT:
                    break;
                default:
                    return;
            }
        }

        private void CopyTrackValue(KeyFrame src, KeyFrame dst)
        {
            switch (type)
            {
                case TrackType.TRACK_POSITION:
                    dst.value.pos.x = src.value.pos.x;
                    dst.value.pos.y = src.value.pos.y;
                    return;
                case TrackType.TRACK_SCALE:
                    dst.value.scale.scaleX = src.value.scale.scaleX;
                    dst.value.scale.scaleY = src.value.scale.scaleY;
                    return;
                case TrackType.TRACK_ROTATION:
                    dst.value.rotation.angle = src.value.rotation.angle;
                    return;
                case TrackType.TRACK_SKEW:
                    dst.value.skew.skewX = src.value.skew.skewX;
                    dst.value.skew.skewY = src.value.skew.skewY;
                    return;
                case TrackType.TRACK_COLOR:
                    dst.value.color.rgba.RedColor = src.value.color.rgba.RedColor;
                    dst.value.color.rgba.GreenColor = src.value.color.rgba.GreenColor;
                    dst.value.color.rgba.BlueColor = src.value.color.rgba.BlueColor;
                    dst.value.color.rgba.AlphaChannel = src.value.color.rgba.AlphaChannel;
                    return;
                case TrackType.TRACK_ACTION:
                case TrackType.TRACKS_COUNT:
                    return;
                default:
                    return;
            }
        }

        private static bool IsFlashInterpolationTransition(KeyFrame.TransitionType transition)
        {
            return transition is KeyFrame.TransitionType.FRAME_TRANSITION_FLASH_LINEAR
                or KeyFrame.TransitionType.FRAME_TRANSITION_FLASH_EASE_IN
                or KeyFrame.TransitionType.FRAME_TRANSITION_FLASH_EASE_OUT
                or KeyFrame.TransitionType.FRAME_TRANSITION_FLASH_EASE_IN_OUT
                or KeyFrame.TransitionType.FRAME_TRANSITION_FLASH_EASE_MIRRORED
                or KeyFrame.TransitionType.FRAME_TRANSITION_FLASH_HOLD
                or KeyFrame.TransitionType.FRAME_TRANSITION_FLASH_IMMEDIATE;
        }

        private static float ComputeFlashInterpolationFactor(Track track, KeyFrame.TransitionType transition)
        {
            float clampedTimeLeft = MathF.Max(0f, track.keyFrameTimeLeft);
            float denominator = track.keyFrameElapsed + clampedTimeLeft;
            float progress = denominator <= 1E-06f ? 1f : track.keyFrameElapsed / denominator;

            float factor = transition switch
            {
                KeyFrame.TransitionType.FRAME_TRANSITION_FLASH_LINEAR => progress,
                KeyFrame.TransitionType.FRAME_TRANSITION_FLASH_IMMEDIATE => 1f,
                KeyFrame.TransitionType.FRAME_TRANSITION_FLASH_HOLD => 0f,
                KeyFrame.TransitionType.FRAME_TRANSITION_FLASH_EASE_IN => progress * progress,
                KeyFrame.TransitionType.FRAME_TRANSITION_FLASH_EASE_OUT => 1f - ((1f - progress) * (1f - progress)),
                KeyFrame.TransitionType.FRAME_TRANSITION_FLASH_EASE_IN_OUT => EvaluateFlashEaseInOut(progress),
                KeyFrame.TransitionType.FRAME_TRANSITION_FLASH_EASE_MIRRORED => EvaluateFlashEaseMirrored(progress),
                KeyFrame.TransitionType.FRAME_TRANSITION_LINEAR => progress,
                KeyFrame.TransitionType.FRAME_TRANSITION_IMMEDIATE => progress,
                KeyFrame.TransitionType.FRAME_TRANSITION_EASE_IN => progress,
                KeyFrame.TransitionType.FRAME_TRANSITION_EASE_OUT => progress,
                _ => progress
            };

            if (factor > 1f)
            {
                factor = 1f;
            }
            else if (factor < 0f)
            {
                factor = 0f;
            }

            return factor;
        }

        private static float EvaluateFlashEaseInOut(float progress)
        {
            float doubled = progress + progress;
            if (doubled < 1f)
            {
                return 0.5f * doubled * doubled;
            }

            float shifted = doubled - 2f;
            return -0.5f * ((shifted * shifted) - 2f);
        }

        private static float EvaluateFlashEaseMirrored(float progress)
        {
            float doubled = progress + progress;
            float shifted = doubled - 1f;
            float squared = shifted * shifted;
            return doubled < 1f
                ? 0.5f * (1f - squared)
                : 0.5f * (1f + squared);
        }

        private void ApplyInterpolatedStep(float factor)
        {
            switch (type)
            {
                case TrackType.TRACK_POSITION:
                    {
                        float interpolatedX = currentStepSource.value.pos.x + ((currentStepDestination.value.pos.x - currentStepSource.value.pos.x) * factor);
                        float interpolatedY = currentStepSource.value.pos.y + ((currentStepDestination.value.pos.y - currentStepSource.value.pos.y) * factor);
                        if (relative)
                        {
                            t.element.x = elementPrevState.value.pos.x + interpolatedX;
                            t.element.y = elementPrevState.value.pos.y + interpolatedY;
                        }
                        else
                        {
                            t.element.x = interpolatedX;
                            t.element.y = interpolatedY;
                        }
                        return;
                    }
                case TrackType.TRACK_SCALE:
                    {
                        float interpolatedScaleX = currentStepSource.value.scale.scaleX
                            + ((currentStepDestination.value.scale.scaleX - currentStepSource.value.scale.scaleX) * factor);
                        float interpolatedScaleY = currentStepSource.value.scale.scaleY
                            + ((currentStepDestination.value.scale.scaleY - currentStepSource.value.scale.scaleY) * factor);
                        if (relative)
                        {
                            t.element.scaleX = elementPrevState.value.scale.scaleX + interpolatedScaleX;
                            t.element.scaleY = elementPrevState.value.scale.scaleY + interpolatedScaleY;
                        }
                        else
                        {
                            t.element.scaleX = interpolatedScaleX;
                            t.element.scaleY = interpolatedScaleY;
                        }
                        return;
                    }
                case TrackType.TRACK_ROTATION:
                    {
                        float interpolatedRotation = currentStepSource.value.rotation.angle
                            + ((currentStepDestination.value.rotation.angle - currentStepSource.value.rotation.angle) * factor);
                        t.element.rotation = relative
                            ? elementPrevState.value.rotation.angle + interpolatedRotation
                            : interpolatedRotation;
                        return;
                    }
                case TrackType.TRACK_SKEW:
                    {
                        float interpolatedSkewX = currentStepSource.value.skew.skewX
                            + ((currentStepDestination.value.skew.skewX - currentStepSource.value.skew.skewX) * factor);
                        float interpolatedSkewY = currentStepSource.value.skew.skewY
                            + ((currentStepDestination.value.skew.skewY - currentStepSource.value.skew.skewY) * factor);
                        if (relative)
                        {
                            t.element.skewX = elementPrevState.value.skew.skewX + interpolatedSkewX;
                            t.element.skewY = elementPrevState.value.skew.skewY + interpolatedSkewY;
                        }
                        else
                        {
                            t.element.skewX = interpolatedSkewX;
                            t.element.skewY = interpolatedSkewY;
                        }
                        return;
                    }
                case TrackType.TRACK_COLOR:
                    {
                        float interpolatedR = currentStepSource.value.color.rgba.RedColor
                            + ((currentStepDestination.value.color.rgba.RedColor - currentStepSource.value.color.rgba.RedColor) * factor);
                        float interpolatedG = currentStepSource.value.color.rgba.GreenColor
                            + ((currentStepDestination.value.color.rgba.GreenColor - currentStepSource.value.color.rgba.GreenColor) * factor);
                        float interpolatedB = currentStepSource.value.color.rgba.BlueColor
                            + ((currentStepDestination.value.color.rgba.BlueColor - currentStepSource.value.color.rgba.BlueColor) * factor);
                        float interpolatedA = currentStepSource.value.color.rgba.AlphaChannel
                            + ((currentStepDestination.value.color.rgba.AlphaChannel - currentStepSource.value.color.rgba.AlphaChannel) * factor);

                        if (relative)
                        {
                            t.element.color.RedColor = elementPrevState.value.color.rgba.RedColor + interpolatedR;
                            t.element.color.GreenColor = elementPrevState.value.color.rgba.GreenColor + interpolatedG;
                            t.element.color.BlueColor = elementPrevState.value.color.rgba.BlueColor + interpolatedB;
                            t.element.color.AlphaChannel = elementPrevState.value.color.rgba.AlphaChannel + interpolatedA;
                        }
                        else
                        {
                            t.element.color.RedColor = interpolatedR;
                            t.element.color.GreenColor = interpolatedG;
                            t.element.color.BlueColor = interpolatedB;
                            t.element.color.AlphaChannel = interpolatedA;
                        }
                        return;
                    }
                case TrackType.TRACK_ACTION:
                case TrackType.TRACKS_COUNT:
                    return;
                default:
                    return;
            }
        }

        public static void UpdateActionTrack(Track thiss, float delta)
        {
            if (thiss == null)
            {
                return;
            }
            if (thiss.state == TrackState.TRACK_NOT_ACTIVE)
            {
                if (!thiss.t.timelineDirReverse)
                {
                    if (thiss.t.time - delta <= thiss.endTime && thiss.t.time >= thiss.startTime)
                    {
                        if (thiss.keyFramesCount > 1)
                        {
                            thiss.state = TrackState.TRACK_ACTIVE;
                            thiss.nextKeyFrame = 0;
                            thiss.overrun = thiss.t.time - thiss.startTime;
                            thiss.nextKeyFrame++;
                            thiss.InitActionKeyFrameandTime(thiss.keyFrames[thiss.nextKeyFrame - 1], thiss.keyFrames[thiss.nextKeyFrame].timeOffset);
                            return;
                        }
                        thiss.InitActionKeyFrameandTime(thiss.keyFrames[0], 0f);
                        return;
                    }
                }
                else if (thiss.t.time + delta >= thiss.startTime && thiss.t.time <= thiss.endTime)
                {
                    if (thiss.keyFramesCount > 1)
                    {
                        thiss.state = TrackState.TRACK_ACTIVE;
                        thiss.nextKeyFrame = thiss.keyFramesCount - 1;
                        thiss.overrun = thiss.endTime - thiss.t.time;
                        thiss.nextKeyFrame--;
                        thiss.InitActionKeyFrameandTime(thiss.keyFrames[thiss.nextKeyFrame + 1], thiss.keyFrames[thiss.nextKeyFrame + 1].timeOffset);
                        return;
                    }
                    thiss.InitActionKeyFrameandTime(thiss.keyFrames[0], 0f);
                }
                return;
            }
            thiss.keyFrameTimeLeft -= delta;
            if (thiss.keyFrameTimeLeft <= 1E-06f)
            {
                if (thiss.t != null && thiss.t.delegateTimelineDelegate != null)
                {
                    thiss.t.delegateTimelineDelegate.TimelinereachedKeyFramewithIndex(thiss.t, thiss.keyFrames[thiss.nextKeyFrame], thiss.nextKeyFrame);
                }
                thiss.overrun = 0f - thiss.keyFrameTimeLeft;
                if (thiss.nextKeyFrame == thiss.keyFramesCount - 1)
                {
                    thiss.SetElementFromKeyFrame(thiss.keyFrames[thiss.nextKeyFrame]);
                    thiss.state = TrackState.TRACK_NOT_ACTIVE;
                    return;
                }
                if (thiss.nextKeyFrame == 0)
                {
                    thiss.SetElementFromKeyFrame(thiss.keyFrames[thiss.nextKeyFrame]);
                    thiss.state = TrackState.TRACK_NOT_ACTIVE;
                    return;
                }
                if (!thiss.t.timelineDirReverse)
                {
                    thiss.nextKeyFrame++;
                    thiss.InitActionKeyFrameandTime(thiss.keyFrames[thiss.nextKeyFrame - 1], thiss.keyFrames[thiss.nextKeyFrame].timeOffset);
                    return;
                }
                thiss.nextKeyFrame--;
                thiss.InitActionKeyFrameandTime(thiss.keyFrames[thiss.nextKeyFrame + 1], thiss.keyFrames[thiss.nextKeyFrame + 1].timeOffset);
            }
        }

        public static void UpdateTrack(Track thiss, float delta)
        {
            Timeline timeline = thiss.t;
            if (thiss.state == TrackState.TRACK_NOT_ACTIVE)
            {
                if (timeline.time >= thiss.startTime && timeline.time <= thiss.endTime)
                {
                    thiss.state = TrackState.TRACK_ACTIVE;
                    if (!timeline.timelineDirReverse)
                    {
                        thiss.nextKeyFrame = 0;
                        thiss.overrun = timeline.time - thiss.startTime;
                        thiss.nextKeyFrame++;
                        thiss.InitKeyFrameStepFromTowithTime(thiss.keyFrames[thiss.nextKeyFrame - 1], thiss.keyFrames[thiss.nextKeyFrame], thiss.keyFrames[thiss.nextKeyFrame].timeOffset);
                        return;
                    }
                    thiss.nextKeyFrame = thiss.keyFramesCount - 1;
                    thiss.overrun = thiss.endTime - timeline.time;
                    thiss.nextKeyFrame--;
                    thiss.InitKeyFrameStepFromTowithTime(thiss.keyFrames[thiss.nextKeyFrame + 1], thiss.keyFrames[thiss.nextKeyFrame], thiss.keyFrames[thiss.nextKeyFrame + 1].timeOffset);
                }
                return;
            }
            thiss.keyFrameElapsed += delta;
            thiss.keyFrameTimeLeft -= delta;
            KeyFrame.TransitionType transition = thiss.keyFrames[thiss.nextKeyFrame].transitionType;

            if (transition is KeyFrame.TransitionType.FRAME_TRANSITION_EASE_IN or KeyFrame.TransitionType.FRAME_TRANSITION_EASE_OUT)
            {
                KeyFrame keyFrame = thiss.currentStepPerSecond;
                switch (thiss.type)
                {
                    case TrackType.TRACK_POSITION:
                        {
                            float accelDeltaX = thiss.currentStepAcceleration.value.pos.x * delta;
                            float accelDeltaY = thiss.currentStepAcceleration.value.pos.y * delta;
                            thiss.currentStepPerSecond.value.pos.x += accelDeltaX;
                            thiss.currentStepPerSecond.value.pos.y += accelDeltaY;
                            timeline.element.x += (keyFrame.value.pos.x + (accelDeltaX / 2f)) * delta;
                            timeline.element.y += (keyFrame.value.pos.y + (accelDeltaY / 2f)) * delta;
                            break;
                        }
                    case TrackType.TRACK_SCALE:
                        {
                            float accelDeltaScaleX = thiss.currentStepAcceleration.value.scale.scaleX * delta;
                            float accelDeltaScaleY = thiss.currentStepAcceleration.value.scale.scaleY * delta;
                            thiss.currentStepPerSecond.value.scale.scaleX += accelDeltaScaleX;
                            thiss.currentStepPerSecond.value.scale.scaleY += accelDeltaScaleY;
                            timeline.element.scaleX += (keyFrame.value.scale.scaleX + (accelDeltaScaleX / 2f)) * delta;
                            timeline.element.scaleY += (keyFrame.value.scale.scaleY + (accelDeltaScaleY / 2f)) * delta;
                            break;
                        }
                    case TrackType.TRACK_ROTATION:
                        {
                            float accelDeltaRotation = thiss.currentStepAcceleration.value.rotation.angle * delta;
                            thiss.currentStepPerSecond.value.rotation.angle += accelDeltaRotation;
                            timeline.element.rotation += (keyFrame.value.rotation.angle + (accelDeltaRotation / 2f)) * delta;
                            break;
                        }
                    case TrackType.TRACK_SKEW:
                        {
                            float accelDeltaSkewX = thiss.currentStepAcceleration.value.skew.skewX * delta;
                            float accelDeltaSkewY = thiss.currentStepAcceleration.value.skew.skewY * delta;
                            thiss.currentStepPerSecond.value.skew.skewX += accelDeltaSkewX;
                            thiss.currentStepPerSecond.value.skew.skewY += accelDeltaSkewY;
                            timeline.element.skewX += (keyFrame.value.skew.skewX + (accelDeltaSkewX / 2f)) * delta;
                            timeline.element.skewY += (keyFrame.value.skew.skewY + (accelDeltaSkewY / 2f)) * delta;
                            break;
                        }
                    case TrackType.TRACK_COLOR:
                        {
                            ColorParams color = thiss.currentStepPerSecond.value.color;
                            color.rgba.RedColor += thiss.currentStepAcceleration.value.color.rgba.RedColor * delta;
                            ColorParams color2 = thiss.currentStepPerSecond.value.color;
                            color2.rgba.GreenColor += thiss.currentStepAcceleration.value.color.rgba.GreenColor * delta;
                            ColorParams color3 = thiss.currentStepPerSecond.value.color;
                            color3.rgba.BlueColor += thiss.currentStepAcceleration.value.color.rgba.BlueColor * delta;
                            ColorParams color4 = thiss.currentStepPerSecond.value.color;
                            color4.rgba.AlphaChannel += thiss.currentStepAcceleration.value.color.rgba.AlphaChannel * delta;
                            float accelDeltaRed = thiss.currentStepAcceleration.value.color.rgba.RedColor * delta;
                            float accelDeltaGreen = thiss.currentStepAcceleration.value.color.rgba.GreenColor * delta;
                            float accelDeltaBlue = thiss.currentStepAcceleration.value.color.rgba.BlueColor * delta;
                            float accelDeltaAlpha = thiss.currentStepAcceleration.value.color.rgba.AlphaChannel * delta;
                            ColorParams color5 = thiss.currentStepPerSecond.value.color;
                            color5.rgba.RedColor += accelDeltaRed;
                            ColorParams color6 = thiss.currentStepPerSecond.value.color;
                            color6.rgba.GreenColor += accelDeltaGreen;
                            ColorParams color7 = thiss.currentStepPerSecond.value.color;
                            color7.rgba.BlueColor += accelDeltaBlue;
                            ColorParams color8 = thiss.currentStepPerSecond.value.color;
                            color8.rgba.AlphaChannel += accelDeltaAlpha;
                            BaseElement element = timeline.element;
                            element.color.RedColor += (keyFrame.value.color.rgba.RedColor + (accelDeltaRed / 2f)) * delta;
                            BaseElement element2 = timeline.element;
                            element2.color.GreenColor += (keyFrame.value.color.rgba.GreenColor + (accelDeltaGreen / 2f)) * delta;
                            BaseElement element3 = timeline.element;
                            element3.color.BlueColor += (keyFrame.value.color.rgba.BlueColor + (accelDeltaBlue / 2f)) * delta;
                            BaseElement element4 = timeline.element;
                            element4.color.AlphaChannel += (keyFrame.value.color.rgba.AlphaChannel + (accelDeltaAlpha / 2f)) * delta;
                            break;
                        }

                    case TrackType.TRACK_ACTION:
                        break;
                    case TrackType.TRACKS_COUNT:
                        break;
                    default:
                        break;
                }
            }
            else if (transition == KeyFrame.TransitionType.FRAME_TRANSITION_LINEAR)
            {
                switch (thiss.type)
                {
                    case TrackType.TRACK_POSITION:
                        timeline.element.x += thiss.currentStepPerSecond.value.pos.x * delta;
                        timeline.element.y += thiss.currentStepPerSecond.value.pos.y * delta;
                        break;
                    case TrackType.TRACK_SCALE:
                        timeline.element.scaleX += thiss.currentStepPerSecond.value.scale.scaleX * delta;
                        timeline.element.scaleY += thiss.currentStepPerSecond.value.scale.scaleY * delta;
                        break;
                    case TrackType.TRACK_ROTATION:
                        timeline.element.rotation += thiss.currentStepPerSecond.value.rotation.angle * delta;
                        break;
                    case TrackType.TRACK_SKEW:
                        timeline.element.skewX += thiss.currentStepPerSecond.value.skew.skewX * delta;
                        timeline.element.skewY += thiss.currentStepPerSecond.value.skew.skewY * delta;
                        break;
                    case TrackType.TRACK_COLOR:
                        {
                            BaseElement element5 = timeline.element;
                            element5.color.RedColor += thiss.currentStepPerSecond.value.color.rgba.RedColor * delta;
                            BaseElement element6 = timeline.element;
                            element6.color.GreenColor += thiss.currentStepPerSecond.value.color.rgba.GreenColor * delta;
                            BaseElement element7 = timeline.element;
                            element7.color.BlueColor += thiss.currentStepPerSecond.value.color.rgba.BlueColor * delta;
                            BaseElement element8 = timeline.element;
                            element8.color.AlphaChannel += thiss.currentStepPerSecond.value.color.rgba.AlphaChannel * delta;
                            break;
                        }

                    case TrackType.TRACK_ACTION:
                        break;
                    case TrackType.TRACKS_COUNT:
                        break;
                    default:
                        break;
                }
            }
            else if (IsFlashInterpolationTransition(transition))
            {
                float factor = ComputeFlashInterpolationFactor(thiss, transition);
                thiss.ApplyInterpolatedStep(factor);
            }
            if (thiss.keyFrameTimeLeft <= 1E-06f)
            {
                timeline.delegateTimelineDelegate?.TimelinereachedKeyFramewithIndex(timeline, thiss.keyFrames[thiss.nextKeyFrame], thiss.nextKeyFrame);
                thiss.overrun = 0f - thiss.keyFrameTimeLeft;
                if (thiss.nextKeyFrame == thiss.keyFramesCount - 1)
                {
                    thiss.SetElementFromKeyFrame(thiss.keyFrames[thiss.nextKeyFrame]);
                    thiss.state = TrackState.TRACK_NOT_ACTIVE;
                    return;
                }
                if (thiss.nextKeyFrame == 0)
                {
                    thiss.SetElementFromKeyFrame(thiss.keyFrames[thiss.nextKeyFrame]);
                    thiss.state = TrackState.TRACK_NOT_ACTIVE;
                    return;
                }
                if (!timeline.timelineDirReverse)
                {
                    thiss.nextKeyFrame++;
                    thiss.InitKeyFrameStepFromTowithTime(thiss.keyFrames[thiss.nextKeyFrame - 1], thiss.keyFrames[thiss.nextKeyFrame], thiss.keyFrames[thiss.nextKeyFrame].timeOffset);
                    return;
                }
                thiss.nextKeyFrame--;
                thiss.InitKeyFrameStepFromTowithTime(thiss.keyFrames[thiss.nextKeyFrame + 1], thiss.keyFrames[thiss.nextKeyFrame], thiss.keyFrames[thiss.nextKeyFrame + 1].timeOffset);
            }
        }

        public TrackType type;

        public TrackState state;

        public bool relative;

        public float startTime;

        public float endTime;

        public int keyFramesCount;

        public KeyFrame[] keyFrames;

        public Timeline t;

        public int nextKeyFrame;

        public int keyFramesCapacity;

        public KeyFrame currentStepPerSecond;

        public KeyFrame currentStepAcceleration;

        public float keyFrameTimeLeft;

        public float keyFrameDuration;

        public float keyFrameElapsed;

        public KeyFrame elementPrevState;

        public KeyFrame currentStepSource;

        public KeyFrame currentStepDestination;

        public float overrun;

        public List<List<CTRAction>> actionSets;

        public enum TrackType
        {
            TRACK_POSITION,
            TRACK_SCALE,
            TRACK_ROTATION,
            TRACK_COLOR,
            TRACK_SKEW,
            TRACK_ACTION,
            TRACKS_COUNT
        }

        public enum TrackState
        {
            TRACK_NOT_ACTIVE,
            TRACK_ACTIVE
        }
    }
}
