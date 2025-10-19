using System;
using CutTheRope.ios;

namespace CutTheRope.iframework.visual
{
	// Token: 0x0200004E RID: 78
	internal class Timeline : NSObject
	{
		// Token: 0x060002A0 RID: 672 RVA: 0x0000F00F File Offset: 0x0000D20F
		public virtual void stopTimeline()
		{
			this.state = Timeline.TimelineState.TIMELINE_STOPPED;
			this.deactivateTracks();
		}

		// Token: 0x060002A1 RID: 673 RVA: 0x0000F020 File Offset: 0x0000D220
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

		// Token: 0x060002A2 RID: 674 RVA: 0x0000F058 File Offset: 0x0000D258
		public void jumpToTrackKeyFrame(int t, int k)
		{
			if (this.state == Timeline.TimelineState.TIMELINE_STOPPED)
			{
				this.state = Timeline.TimelineState.TIMELINE_PAUSED;
			}
			this.time = this.tracks[t].getFrameTime(k);
		}

		// Token: 0x060002A3 RID: 675 RVA: 0x0000F080 File Offset: 0x0000D280
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

		// Token: 0x060002A4 RID: 676 RVA: 0x0000F110 File Offset: 0x0000D310
		public virtual void pauseTimeline()
		{
			this.state = Timeline.TimelineState.TIMELINE_PAUSED;
		}

		// Token: 0x060002A5 RID: 677 RVA: 0x0000F11C File Offset: 0x0000D31C
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
							if (thiss.delegateTimelineDelegate != null)
							{
								thiss.delegateTimelineDelegate.timelineFinished(thiss);
							}
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
							if (thiss.delegateTimelineDelegate != null)
							{
								thiss.delegateTimelineDelegate.timelineFinished(thiss);
							}
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

		// Token: 0x060002A6 RID: 678 RVA: 0x0000F327 File Offset: 0x0000D527
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

		// Token: 0x060002A7 RID: 679 RVA: 0x0000F364 File Offset: 0x0000D564
		public virtual void addKeyFrame(KeyFrame k)
		{
			int i = ((this.tracks[(int)k.trackType] != null) ? this.tracks[(int)k.trackType].keyFramesCount : 0);
			this.setKeyFrameAt(k, i);
		}

		// Token: 0x060002A8 RID: 680 RVA: 0x0000F3A0 File Offset: 0x0000D5A0
		public virtual void setKeyFrameAt(KeyFrame k, int i)
		{
			if (this.tracks[(int)k.trackType] == null)
			{
				this.tracks[(int)k.trackType] = new Track().initWithTimelineTypeandMaxKeyFrames(this, k.trackType, this.maxKeyFrames);
			}
			this.tracks[(int)k.trackType].setKeyFrameAt(k, i);
		}

		// Token: 0x060002A9 RID: 681 RVA: 0x0000F3F4 File Offset: 0x0000D5F4
		public virtual void setTimelineLoopType(Timeline.LoopType l)
		{
			this.timelineLoopType = l;
		}

		// Token: 0x060002AA RID: 682 RVA: 0x0000F3FD File Offset: 0x0000D5FD
		public virtual Track getTrack(Track.TrackType tt)
		{
			return this.tracks[(int)tt];
		}

		// Token: 0x0400020F RID: 527
		private const int TRACKS_COUNT = 5;

		// Token: 0x04000210 RID: 528
		public TimelineDelegate delegateTimelineDelegate;

		// Token: 0x04000211 RID: 529
		public BaseElement element;

		// Token: 0x04000212 RID: 530
		public Timeline.TimelineState state;

		// Token: 0x04000213 RID: 531
		public float time;

		// Token: 0x04000214 RID: 532
		private float length;

		// Token: 0x04000215 RID: 533
		public bool timelineDirReverse;

		// Token: 0x04000216 RID: 534
		public int loopsLimit;

		// Token: 0x04000217 RID: 535
		private int maxKeyFrames;

		// Token: 0x04000218 RID: 536
		private Timeline.LoopType timelineLoopType;

		// Token: 0x04000219 RID: 537
		private Track[] tracks = new Track[5];

		// Token: 0x020000B4 RID: 180
		public enum TimelineState
		{
			// Token: 0x040008B0 RID: 2224
			TIMELINE_STOPPED,
			// Token: 0x040008B1 RID: 2225
			TIMELINE_PLAYING,
			// Token: 0x040008B2 RID: 2226
			TIMELINE_PAUSED
		}

		// Token: 0x020000B5 RID: 181
		public enum LoopType
		{
			// Token: 0x040008B4 RID: 2228
			TIMELINE_NO_LOOP,
			// Token: 0x040008B5 RID: 2229
			TIMELINE_REPLAY,
			// Token: 0x040008B6 RID: 2230
			TIMELINE_PING_PONG
		}
	}
}
