using CutTheRope.ios;
using System;

namespace CutTheRope.iframework.visual
{
    internal class Timeline : NSObject
    {
        public virtual void stopTimeline()
        {
            this.state = Timeline.TimelineState.TIMELINE_STOPPED;
            this.deactivateTracks();
        }

        public virtual void deactivateTracks()
        {
            for (int i = 0; i < this.tracks.Length; i++)
            {
                if (this.tracks[i] != null)
                {
                    this.tracks[i].state = Track.TrackState.TRACK_NOT_ACTIVE;
                }
            }
        }

        public void jumpToTrackKeyFrame(int t, int k)
        {
            if (this.state == Timeline.TimelineState.TIMELINE_STOPPED)
            {
                this.state = Timeline.TimelineState.TIMELINE_PAUSED;
            }
            this.time = this.tracks[t].getFrameTime(k);
        }

        public virtual void playTimeline()
        {
            if (this.state != Timeline.TimelineState.TIMELINE_PAUSED)
            {
                this.time = 0f;
                this.timelineDirReverse = false;
                this.length = 0f;
                for (int i = 0; i < 5; i++)
                {
                    if (this.tracks[i] != null)
                    {
                        this.tracks[i].updateRange();
                        if (this.tracks[i].endTime > this.length)
                        {
                            this.length = this.tracks[i].endTime;
                        }
                    }
                }
            }
            this.state = Timeline.TimelineState.TIMELINE_PLAYING;
            Timeline.updateTimeline(this, 0f);
        }

        public virtual void pauseTimeline()
        {
            this.state = Timeline.TimelineState.TIMELINE_PAUSED;
        }

        public static void updateTimeline(Timeline thiss, float delta)
        {
            if (thiss.state != Timeline.TimelineState.TIMELINE_PLAYING)
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
                case Timeline.LoopType.TIMELINE_NO_LOOP:
                    if (thiss.time >= thiss.length - 1E-06f)
                    {
                        thiss.stopTimeline();
                        if (thiss != null && thiss.delegateTimelineDelegate != null)
                        {
                            thiss.delegateTimelineDelegate.timelineFinished(thiss);
                        }
                    }
                    break;
                case Timeline.LoopType.TIMELINE_REPLAY:
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
                case Timeline.LoopType.TIMELINE_PING_PONG:
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
                this.maxKeyFrames = m;
                this.time = 0f;
                this.length = 0f;
                this.state = Timeline.TimelineState.TIMELINE_STOPPED;
                this.loopsLimit = -1;
                this.timelineLoopType = Timeline.LoopType.TIMELINE_NO_LOOP;
            }
            return this;
        }

        public virtual void addKeyFrame(KeyFrame k)
        {
            int i = (this.tracks[(int)k.trackType] != null) ? this.tracks[(int)k.trackType].keyFramesCount : 0;
            this.setKeyFrameAt(k, i);
        }

        public virtual void setKeyFrameAt(KeyFrame k, int i)
        {
            if (this.tracks[(int)k.trackType] == null)
            {
                this.tracks[(int)k.trackType] = new Track().initWithTimelineTypeandMaxKeyFrames(this, k.trackType, this.maxKeyFrames);
            }
            this.tracks[(int)k.trackType].setKeyFrameAt(k, i);
        }

        public virtual void setTimelineLoopType(Timeline.LoopType l)
        {
            this.timelineLoopType = l;
        }

        public virtual Track getTrack(Track.TrackType tt)
        {
            return this.tracks[(int)tt];
        }

        private const int TRACKS_COUNT = 5;

        public TimelineDelegate delegateTimelineDelegate;

        public BaseElement element;

        public Timeline.TimelineState state;

        public float time;

        private float length;

        public bool timelineDirReverse;

        public int loopsLimit;

        private int maxKeyFrames;

        private Timeline.LoopType timelineLoopType;

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
