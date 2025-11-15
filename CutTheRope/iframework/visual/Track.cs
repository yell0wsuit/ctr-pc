using System.Collections.Generic;

namespace CutTheRope.iframework.visual
{
    internal sealed class Track : FrameworkTypes
    {
        public Track()
        {
            elementPrevState = new KeyFrame();
            currentStepPerSecond = new KeyFrame();
            currentStepAcceleration = new KeyFrame();
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
            float num = 0f;
            for (int i = 0; i <= f; i++)
            {
                num += keyFrames[i].timeOffset;
            }
            return num;
        }

        public void UpdateRange()
        {
            startTime = GetFrameTime(0);
            endTime = GetFrameTime(keyFramesCount - 1);
        }

        private void InitKeyFrameStepFromTowithTime(KeyFrame src, KeyFrame dst, float time)
        {
            keyFrameTimeLeft = time;
            SetKeyFrameFromElement(elementPrevState);
            SetElementFromKeyFrame(src);
            switch (type)
            {
                case TrackType.TRACK_POSITION:
                    currentStepPerSecond.value.pos.x = (dst.value.pos.x - src.value.pos.x) / keyFrameTimeLeft;
                    currentStepPerSecond.value.pos.y = (dst.value.pos.y - src.value.pos.y) / keyFrameTimeLeft;
                    break;
                case TrackType.TRACK_SCALE:
                    currentStepPerSecond.value.scale.scaleX = (dst.value.scale.scaleX - src.value.scale.scaleX) / keyFrameTimeLeft;
                    currentStepPerSecond.value.scale.scaleY = (dst.value.scale.scaleY - src.value.scale.scaleY) / keyFrameTimeLeft;
                    break;
                case TrackType.TRACK_ROTATION:
                    currentStepPerSecond.value.rotation.angle = (dst.value.rotation.angle - src.value.rotation.angle) / keyFrameTimeLeft;
                    break;
                case TrackType.TRACK_COLOR:
                    currentStepPerSecond.value.color.rgba.r = (dst.value.color.rgba.r - src.value.color.rgba.r) / keyFrameTimeLeft;
                    currentStepPerSecond.value.color.rgba.g = (dst.value.color.rgba.g - src.value.color.rgba.g) / keyFrameTimeLeft;
                    currentStepPerSecond.value.color.rgba.b = (dst.value.color.rgba.b - src.value.color.rgba.b) / keyFrameTimeLeft;
                    currentStepPerSecond.value.color.rgba.a = (dst.value.color.rgba.a - src.value.color.rgba.a) / keyFrameTimeLeft;
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
                        currentStepAcceleration.value.pos.x = currentStepPerSecond.value.pos.x / keyFrameTimeLeft;
                        currentStepAcceleration.value.pos.y = currentStepPerSecond.value.pos.y / keyFrameTimeLeft;
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
                        currentStepAcceleration.value.scale.scaleX = currentStepPerSecond.value.scale.scaleX / keyFrameTimeLeft;
                        currentStepAcceleration.value.scale.scaleY = currentStepPerSecond.value.scale.scaleY / keyFrameTimeLeft;
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
                        currentStepAcceleration.value.rotation.angle = currentStepPerSecond.value.rotation.angle / keyFrameTimeLeft;
                        if (dst.transitionType == KeyFrame.TransitionType.FRAME_TRANSITION_EASE_IN)
                        {
                            currentStepPerSecond.value.rotation.angle = 0f;
                        }
                        else
                        {
                            currentStepAcceleration.value.rotation.angle *= -1f;
                        }
                        break;
                    case TrackType.TRACK_COLOR:
                        {
                            ColorParams color = currentStepPerSecond.value.color;
                            color.rgba.r *= 2f;
                            ColorParams color2 = currentStepPerSecond.value.color;
                            color2.rgba.g *= 2f;
                            ColorParams color3 = currentStepPerSecond.value.color;
                            color3.rgba.b *= 2f;
                            ColorParams color4 = currentStepPerSecond.value.color;
                            color4.rgba.a *= 2f;
                            currentStepAcceleration.value.color.rgba.r = currentStepPerSecond.value.color.rgba.r / keyFrameTimeLeft;
                            currentStepAcceleration.value.color.rgba.g = currentStepPerSecond.value.color.rgba.g / keyFrameTimeLeft;
                            currentStepAcceleration.value.color.rgba.b = currentStepPerSecond.value.color.rgba.b / keyFrameTimeLeft;
                            currentStepAcceleration.value.color.rgba.a = currentStepPerSecond.value.color.rgba.a / keyFrameTimeLeft;
                            if (dst.transitionType == KeyFrame.TransitionType.FRAME_TRANSITION_EASE_IN)
                            {
                                currentStepPerSecond.value.color.rgba.r = 0f;
                                currentStepPerSecond.value.color.rgba.g = 0f;
                                currentStepPerSecond.value.color.rgba.b = 0f;
                                currentStepPerSecond.value.color.rgba.a = 0f;
                            }
                            else
                            {
                                ColorParams color5 = currentStepAcceleration.value.color;
                                color5.rgba.r *= -1f;
                                ColorParams color6 = currentStepAcceleration.value.color;
                                color6.rgba.g *= -1f;
                                ColorParams color7 = currentStepAcceleration.value.color;
                                color7.rgba.b *= -1f;
                                ColorParams color8 = currentStepAcceleration.value.color;
                                color8.rgba.a *= -1f;
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
                case TrackType.TRACK_COLOR:
                    if (!relative)
                    {
                        t.element.color = kf.value.color.rgba;
                        return;
                    }
                    t.element.color.r = elementPrevState.value.color.rgba.r + kf.value.color.rgba.r;
                    t.element.color.g = elementPrevState.value.color.rgba.g + kf.value.color.rgba.g;
                    t.element.color.b = elementPrevState.value.color.rgba.b + kf.value.color.rgba.b;
                    t.element.color.a = elementPrevState.value.color.rgba.a + kf.value.color.rgba.a;
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
            thiss.keyFrameTimeLeft -= delta;
            if (thiss.keyFrames[thiss.nextKeyFrame].transitionType is KeyFrame.TransitionType.FRAME_TRANSITION_EASE_IN or KeyFrame.TransitionType.FRAME_TRANSITION_EASE_OUT)
            {
                KeyFrame keyFrame = thiss.currentStepPerSecond;
                switch (thiss.type)
                {
                    case TrackType.TRACK_POSITION:
                        {
                            float num8 = thiss.currentStepAcceleration.value.pos.x * delta;
                            float num9 = thiss.currentStepAcceleration.value.pos.y * delta;
                            thiss.currentStepPerSecond.value.pos.x += num8;
                            thiss.currentStepPerSecond.value.pos.y += num9;
                            timeline.element.x += (keyFrame.value.pos.x + (num8 / 2f)) * delta;
                            timeline.element.y += (keyFrame.value.pos.y + (num9 / 2f)) * delta;
                            break;
                        }
                    case TrackType.TRACK_SCALE:
                        {
                            float num10 = thiss.currentStepAcceleration.value.scale.scaleX * delta;
                            float num11 = thiss.currentStepAcceleration.value.scale.scaleY * delta;
                            thiss.currentStepPerSecond.value.scale.scaleX += num10;
                            thiss.currentStepPerSecond.value.scale.scaleY += num11;
                            timeline.element.scaleX += (keyFrame.value.scale.scaleX + (num10 / 2f)) * delta;
                            timeline.element.scaleY += (keyFrame.value.scale.scaleY + (num11 / 2f)) * delta;
                            break;
                        }
                    case TrackType.TRACK_ROTATION:
                        {
                            float num12 = thiss.currentStepAcceleration.value.rotation.angle * delta;
                            thiss.currentStepPerSecond.value.rotation.angle += num12;
                            timeline.element.rotation += (keyFrame.value.rotation.angle + (num12 / 2f)) * delta;
                            break;
                        }
                    case TrackType.TRACK_COLOR:
                        {
                            ColorParams color = thiss.currentStepPerSecond.value.color;
                            color.rgba.r += thiss.currentStepAcceleration.value.color.rgba.r * delta;
                            ColorParams color2 = thiss.currentStepPerSecond.value.color;
                            color2.rgba.g += thiss.currentStepAcceleration.value.color.rgba.g * delta;
                            ColorParams color3 = thiss.currentStepPerSecond.value.color;
                            color3.rgba.b += thiss.currentStepAcceleration.value.color.rgba.b * delta;
                            ColorParams color4 = thiss.currentStepPerSecond.value.color;
                            color4.rgba.a += thiss.currentStepAcceleration.value.color.rgba.a * delta;
                            float num13 = thiss.currentStepAcceleration.value.color.rgba.r * delta;
                            float num14 = thiss.currentStepAcceleration.value.color.rgba.g * delta;
                            float num15 = thiss.currentStepAcceleration.value.color.rgba.b * delta;
                            float num16 = thiss.currentStepAcceleration.value.color.rgba.a * delta;
                            ColorParams color5 = thiss.currentStepPerSecond.value.color;
                            color5.rgba.r += num13;
                            ColorParams color6 = thiss.currentStepPerSecond.value.color;
                            color6.rgba.g += num14;
                            ColorParams color7 = thiss.currentStepPerSecond.value.color;
                            color7.rgba.b += num15;
                            ColorParams color8 = thiss.currentStepPerSecond.value.color;
                            color8.rgba.a += num16;
                            BaseElement element = timeline.element;
                            element.color.r += (keyFrame.value.color.rgba.r + (num13 / 2f)) * delta;
                            BaseElement element2 = timeline.element;
                            element2.color.g += (keyFrame.value.color.rgba.g + (num14 / 2f)) * delta;
                            BaseElement element3 = timeline.element;
                            element3.color.b += (keyFrame.value.color.rgba.b + (num15 / 2f)) * delta;
                            BaseElement element4 = timeline.element;
                            element4.color.a += (keyFrame.value.color.rgba.a + (num16 / 2f)) * delta;
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
            else if (thiss.keyFrames[thiss.nextKeyFrame].transitionType == KeyFrame.TransitionType.FRAME_TRANSITION_LINEAR)
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
                    case TrackType.TRACK_COLOR:
                        {
                            BaseElement element5 = timeline.element;
                            element5.color.r += thiss.currentStepPerSecond.value.color.rgba.r * delta;
                            BaseElement element6 = timeline.element;
                            element6.color.g += thiss.currentStepPerSecond.value.color.rgba.g * delta;
                            BaseElement element7 = timeline.element;
                            element7.color.b += thiss.currentStepPerSecond.value.color.rgba.b * delta;
                            BaseElement element8 = timeline.element;
                            element8.color.a += thiss.currentStepPerSecond.value.color.rgba.a * delta;
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

        public KeyFrame elementPrevState;

        public float overrun;

        public List<List<CTRAction>> actionSets;

        public enum TrackType
        {
            TRACK_POSITION,
            TRACK_SCALE,
            TRACK_ROTATION,
            TRACK_COLOR,
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
