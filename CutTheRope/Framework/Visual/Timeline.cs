using System;

namespace CutTheRope.Framework.Visual
{
    /// <summary>
    /// Manages playback of multi-track keyframe animations on a <see cref="BaseElement"/>.
    /// </summary>
    internal sealed class Timeline : FrameworkTypes
    {
        /// <summary>
        /// Stops playback and deactivates all tracks.
        /// </summary>
        public void StopTimeline()
        {
            state = TimelineState.TIMELINE_STOPPED;
            DeactivateTracks();
        }

        /// <summary>
        /// Resets all tracks to the inactive state.
        /// </summary>
        public void DeactivateTracks()
        {
            for (int i = 0; i < tracks.Length; i++)
            {
                _ = (tracks[i]?.state = Track.TrackState.TRACK_NOT_ACTIVE);
            }
        }

        /// <summary>
        /// Jumps the timeline to a specific keyframe on a specific track.
        /// </summary>
        /// <param name="t">Track index.</param>
        /// <param name="k">Keyframe index within the track.</param>
        public void JumpToTrackKeyFrame(int t, int k)
        {
            if (state == TimelineState.TIMELINE_STOPPED)
            {
                state = TimelineState.TIMELINE_PAUSED;
            }

            if (t < 0 || t >= tracks.Length)
            {
                return;
            }

            Track track = tracks[t];
            if (track == null || k < 0 || k >= track.keyFramesCount)
            {
                return;
            }

            time = track.GetFrameTime(k);
        }

        /// <summary>
        /// Starts or resumes timeline playback from the beginning or current pause position.
        /// </summary>
        public void PlayTimeline()
        {
            if (state != TimelineState.TIMELINE_PAUSED)
            {
                time = 0f;
                timelineDirReverse = false;
                length = 0f;
                int trackCount = (int)Track.TrackType.TRACKS_COUNT;
                for (int i = 0; i < trackCount; i++)
                {
                    if (tracks[i] != null)
                    {
                        tracks[i].UpdateRange();
                        if (tracks[i].endTime > length)
                        {
                            length = tracks[i].endTime;
                        }
                    }
                }
            }
            state = TimelineState.TIMELINE_PLAYING;
            UpdateTimeline(this, 0f);
        }

        /// <summary>
        /// Pauses timeline playback at the current position.
        /// </summary>
        public void PauseTimeline()
        {
            state = TimelineState.TIMELINE_PAUSED;
        }

        /// <summary>
        /// Advances the timeline by <paramref name="delta"/> seconds, updating all tracks and handling looping.
        /// </summary>
        /// <param name="thiss">Timeline to update.</param>
        /// <param name="delta">Elapsed time in seconds.</param>
        public static void UpdateTimeline(Timeline thiss, float delta)
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
            int trackCount = (int)Track.TrackType.TRACKS_COUNT;
            for (int i = 0; i < trackCount; i++)
            {
                if (thiss.tracks[i] != null)
                {
                    if (thiss.tracks[i].type == Track.TrackType.TRACK_ACTION)
                    {
                        Track.UpdateActionTrack(thiss.tracks[i], delta);
                    }
                    else
                    {
                        Track.UpdateTrack(thiss.tracks[i], delta);
                    }
                }
            }
            switch (thiss.timelineLoopType)
            {
                case LoopType.TIMELINE_NO_LOOP:
                    if (thiss.time >= thiss.length - 1E-06f)
                    {
                        thiss.StopTimeline();
                        if (thiss != null && thiss.delegateTimelineDelegate != null)
                        {
                            thiss.delegateTimelineDelegate.TimelineFinished(thiss);
                        }
                        thiss.OnFinished?.Invoke();
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
                                thiss.StopTimeline();
                                thiss.delegateTimelineDelegate?.TimelineFinished(thiss);
                                thiss.OnFinished?.Invoke();
                            }
                        }
                        thiss.time = MathF.Min(thiss.time - thiss.length, thiss.length);
                        return;
                    }
                    break;
                case LoopType.TIMELINE_PING_PONG:
                    {
                        bool flag3 = !thiss.timelineDirReverse && thiss.time >= thiss.length - 1E-06f;
                        bool flag2 = thiss.timelineDirReverse && thiss.time <= 1E-06f;
                        if (flag3)
                        {
                            thiss.time = MathF.Max(0f, thiss.length - (thiss.time - thiss.length));
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
                                    thiss.StopTimeline();
                                    thiss.delegateTimelineDelegate?.TimelineFinished(thiss);
                                    thiss.OnFinished?.Invoke();
                                }
                            }
                            thiss.time = MathF.Min(0f - thiss.time, thiss.length);
                            thiss.timelineDirReverse = false;
                            return;
                        }
                        break;
                    }
                default:
                    return;
            }
        }

        /// <summary>
        /// Initializes the timeline with the specified maximum keyframes per track.
        /// </summary>
        /// <param name="m">Maximum number of keyframes per track.</param>
        /// <returns>The initialized timeline instance.</returns>
        public Timeline InitWithMaxKeyFramesOnTrack(int m)
        {
            maxKeyFrames = m;
            time = 0f;
            length = 0f;
            state = TimelineState.TIMELINE_STOPPED;
            loopsLimit = -1;
            timelineLoopType = LoopType.TIMELINE_NO_LOOP;
            return this;
        }

        /// <summary>
        /// Appends a keyframe to the appropriate track based on its type.
        /// </summary>
        /// <param name="k">Keyframe to add.</param>
        public void AddKeyFrame(KeyFrame k)
        {
            int i = tracks[(int)k.trackType] != null ? tracks[(int)k.trackType].keyFramesCount : 0;
            SetKeyFrameAt(k, i);
        }

        /// <summary>
        /// Sets a keyframe at a specific index on the appropriate track, creating the track if needed.
        /// </summary>
        /// <param name="k">Keyframe to set.</param>
        /// <param name="i">Index within the track.</param>
        public void SetKeyFrameAt(KeyFrame k, int i)
        {
            if (tracks[(int)k.trackType] == null)
            {
                tracks[(int)k.trackType] = new Track().InitWithTimelineTypeandMaxKeyFrames(this, k.trackType, maxKeyFrames);
            }
            tracks[(int)k.trackType].SetKeyFrameAt(k, i);
        }

        /// <summary>
        /// Sets the loop behavior for this timeline.
        /// </summary>
        /// <param name="l">Loop type.</param>
        public void SetTimelineLoopType(LoopType l)
        {
            timelineLoopType = l;
        }

        /// <summary>
        /// Returns the track for the specified type, or <see langword="null"/> if not created.
        /// </summary>
        /// <param name="tt">Track type to retrieve.</param>
        /// <returns>The matching track instance, or <see langword="null"/> when unavailable.</returns>
        public Track GetTrack(Track.TrackType tt)
        {
            return tracks[(int)tt];
        }

        /// <summary>
        /// Delegate notified on keyframe and timeline completion events.
        /// </summary>
        public ITimelineDelegate delegateTimelineDelegate;

        /// <summary>
        /// Optional callback invoked when the timeline finishes.
        /// </summary>
        public Action OnFinished;

        /// <summary>
        /// Element this timeline animates.
        /// </summary>
        public BaseElement element;

        /// <summary>
        /// Current playback state.
        /// </summary>
        public TimelineState state;

        /// <summary>
        /// Current playback time in seconds.
        /// </summary>
        public float time;

        /// <summary>
        /// Total duration of the timeline in seconds.
        /// </summary>
        private float length;

        /// <summary>
        /// Whether the timeline is currently playing in reverse.
        /// </summary>
        public bool timelineDirReverse;

        /// <summary>
        /// Number of remaining loops, or -1 for unlimited.
        /// </summary>
        public int loopsLimit;

        /// <summary>
        /// Maximum keyframes per track, set during initialization.
        /// </summary>
        private int maxKeyFrames;

        /// <summary>
        /// Loop behavior for this timeline.
        /// </summary>
        private LoopType timelineLoopType;

        /// <summary>
        /// Array of tracks indexed by <see cref="Track.TrackType"/>.
        /// </summary>
        private readonly Track[] tracks = new Track[(int)Track.TrackType.TRACKS_COUNT];

        /// <summary>
        /// Playback states for a timeline.
        /// </summary>
        public enum TimelineState
        {
            /// <summary>
            /// Timeline is stopped.
            /// </summary>
            TIMELINE_STOPPED,

            /// <summary>
            /// Timeline is actively playing.
            /// </summary>
            TIMELINE_PLAYING,

            /// <summary>
            /// Timeline is paused at the current position.
            /// </summary>
            TIMELINE_PAUSED
        }

        /// <summary>
        /// Loop behaviors for a timeline.
        /// </summary>
        public enum LoopType
        {
            /// <summary>
            /// No looping; plays once and stops.
            /// </summary>
            TIMELINE_NO_LOOP,

            /// <summary>
            /// Restarts from the beginning when finished.
            /// </summary>
            TIMELINE_REPLAY,

            /// <summary>
            /// Alternates between forward and reverse playback.
            /// </summary>
            TIMELINE_PING_PONG
        }
    }
}
