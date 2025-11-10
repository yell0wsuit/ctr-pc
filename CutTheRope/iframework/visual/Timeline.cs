using CutTheRope.ios;
using System;

namespace CutTheRope.iframework.visual
{
    internal class Timeline : NSObject
    {
        public virtual void stopTimeline()
        {
            state = TimelineState.TIMELINE_STOPPED;
            deactivateTracks();
        }

        public virtual void deactivateTracks()
        {
            for (int i = 0; i < tracks.Length; i++)
            {
                if (tracks[i] != null)
                {
                    tracks[i].state = Track.TrackState.TRACK_NOT_ACTIVE;
                }
            }
        }

        public void jumpToTrackKeyFrame(int t, int k)
        {
            if (state == TimelineState.TIMELINE_STOPPED)
            {
                state = TimelineState.TIMELINE_PAUSED;
            }
            time = tracks[t].getFrameTime(k);
        }

        public virtual void playTimeline()
        {
            if (state != TimelineState.TIMELINE_PAUSED)
            {
                time = 0f;
                timelineDirReverse = false;
                length = 0f;
                for (int i = 0; i < 5; i++)
                {
                    if (tracks[i] != null)
                    {
                        tracks[i].updateRange();
                        if (tracks[i].endTime > length)
                        {
                            length = tracks[i].endTime;
                        }
                    }
                }
            }
            state = TimelineState.TIMELINE_PLAYING;
            updateTimeline(this, 0f);
        }

        public virtual void pauseTimeline()
        {
            state = TimelineState.TIMELINE_PAUSED;
        }

        public static void updateTimeline(Timeline thiss, float delta)
        {
            if (thiss.state != TimelineState.TIMELINE_PLAYING)
            {
                return;
            }
            if (!thiss.timelineDirReverse)
            {
                thiss.time += delta;
            }
            else
            {
                thiss.time -= delta;
            }
            for (int i = 0; i < 5; i++)
            {
                if (thiss.tracks[i] != null)
                {
                    if (thiss.tracks[i].type == Track.TrackType.TRACK_ACTION)
                    {
                        Track.updateActionTrack(thiss.tracks[i], delta);
                    }
                    else
                    {
                        Track.updateTrack(thiss.tracks[i], delta);
                    }
                }
            }
            switch (thiss.timelineLoopType)
            {
                case LoopType.TIMELINE_NO_LOOP:
                    if (thiss.time >= thiss.length - 1E-06f)
                    {
                        thiss.stopTimeline();
                        if (thiss != null && thiss.delegateTimelineDelegate != null)
                        {
                            thiss.delegateTimelineDelegate.timelineFinished(thiss);
                        }
                    }
                    break;
                case LoopType.TIMELINE_REPLAY:
                    if (thiss.time >= thiss.length - 1E-06f)
                    {
                        if (thiss.loopsLimit > 0)
                        {
                            thiss.loopsLimit--;
                            if (thiss.loopsLimit == 0)
                            {
                                thiss.stopTimeline();
                                thiss.delegateTimelineDelegate?.timelineFinished(thiss);
                            }
                        }
                        thiss.time = Math.Min(thiss.time - thiss.length, thiss.length);
                        return;
                    }
                    break;
                case LoopType.TIMELINE_PING_PONG:
                    {
                        bool flag3 = !thiss.timelineDirReverse && thiss.time >= thiss.length - 1E-06f;
                        bool flag2 = thiss.timelineDirReverse && thiss.time <= 1E-06f;
                        if (flag3)
                        {
                            thiss.time = Math.Max(0f, thiss.length - (thiss.time - thiss.length));
                            thiss.timelineDirReverse = true;
                            return;
                        }
                        if (flag2)
                        {
                            if (thiss.loopsLimit > 0)
                            {
                                thiss.loopsLimit--;
                                if (thiss.loopsLimit == 0)
                                {
                                    thiss.stopTimeline();
                                    thiss.delegateTimelineDelegate?.timelineFinished(thiss);
                                }
                            }
                            thiss.time = Math.Min(0f - thiss.time, thiss.length);
                            thiss.timelineDirReverse = false;
                            return;
                        }
                        break;
                    }
                default:
                    return;
            }
        }

        public virtual Timeline initWithMaxKeyFramesOnTrack(int m)
        {
            if (base.init() != null)
            {
                maxKeyFrames = m;
                time = 0f;
                length = 0f;
                state = TimelineState.TIMELINE_STOPPED;
                loopsLimit = -1;
                timelineLoopType = LoopType.TIMELINE_NO_LOOP;
            }
            return this;
        }

        public virtual void addKeyFrame(KeyFrame k)
        {
            int i = (tracks[(int)k.trackType] != null) ? tracks[(int)k.trackType].keyFramesCount : 0;
            setKeyFrameAt(k, i);
        }

        public virtual void setKeyFrameAt(KeyFrame k, int i)
        {
            if (tracks[(int)k.trackType] == null)
            {
                tracks[(int)k.trackType] = new Track().initWithTimelineTypeandMaxKeyFrames(this, k.trackType, maxKeyFrames);
            }
            tracks[(int)k.trackType].setKeyFrameAt(k, i);
        }

        public virtual void setTimelineLoopType(LoopType l)
        {
            timelineLoopType = l;
        }

        public virtual Track getTrack(Track.TrackType tt)
        {
            return tracks[(int)tt];
        }

        private const int TRACKS_COUNT = 5;

        public TimelineDelegate delegateTimelineDelegate;

        public BaseElement element;

        public TimelineState state;

        public float time;

        private float length;

        public bool timelineDirReverse;

        public int loopsLimit;

        private int maxKeyFrames;

        private LoopType timelineLoopType;

        private Track[] tracks = new Track[5];

        public enum TimelineState
        {
            TIMELINE_STOPPED,
            TIMELINE_PLAYING,
            TIMELINE_PAUSED
        }

        public enum LoopType
        {
            TIMELINE_NO_LOOP,
            TIMELINE_REPLAY,
            TIMELINE_PING_PONG
        }
    }
}
