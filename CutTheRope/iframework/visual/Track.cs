﻿using System;
using System.Collections.Generic;
using CutTheRope.ios;

namespace CutTheRope.iframework.visual
{
	// Token: 0x02000052 RID: 82
	internal class Track : NSObject
	{
		// Token: 0x060002BB RID: 699 RVA: 0x0000F581 File Offset: 0x0000D781
		public Track()
		{
			this.elementPrevState = new KeyFrame();
			this.currentStepPerSecond = new KeyFrame();
			this.currentStepAcceleration = new KeyFrame();
		}

		// Token: 0x060002BC RID: 700 RVA: 0x0000F5AC File Offset: 0x0000D7AC
		public virtual Track initWithTimelineTypeandMaxKeyFrames(Timeline timeline, Track.TrackType trackType, int m)
		{
			this.t = timeline;
			this.type = trackType;
			this.state = Track.TrackState.TRACK_NOT_ACTIVE;
			this.relative = false;
			this.nextKeyFrame = -1;
			this.keyFramesCount = 0;
			this.keyFramesCapacity = m;
			this.keyFrames = new KeyFrame[this.keyFramesCapacity];
			if (this.type == Track.TrackType.TRACK_ACTION)
			{
				this.actionSets = new List<List<Action>>();
			}
			return this;
		}

		// Token: 0x060002BD RID: 701 RVA: 0x0000F610 File Offset: 0x0000D810
		public virtual void initActionKeyFrameandTime(KeyFrame kf, float time)
		{
			this.keyFrameTimeLeft = time;
			this.setElementFromKeyFrame(kf);
			if (this.overrun > 0f)
			{
				Track.updateActionTrack(this, this.overrun);
				this.overrun = 0f;
			}
		}

		// Token: 0x060002BE RID: 702 RVA: 0x0000F644 File Offset: 0x0000D844
		public virtual void setKeyFrameAt(KeyFrame k, int i)
		{
			this.keyFrames[i] = k;
			if (i >= this.keyFramesCount)
			{
				this.keyFramesCount = i + 1;
			}
			if (this.type == Track.TrackType.TRACK_ACTION)
			{
				this.actionSets.Add(k.value.action.actionSet);
			}
		}

		// Token: 0x060002BF RID: 703 RVA: 0x0000F690 File Offset: 0x0000D890
		public virtual float getFrameTime(int f)
		{
			float num = 0f;
			for (int i = 0; i <= f; i++)
			{
				num += this.keyFrames[i].timeOffset;
			}
			return num;
		}

		// Token: 0x060002C0 RID: 704 RVA: 0x0000F6C0 File Offset: 0x0000D8C0
		public virtual void updateRange()
		{
			this.startTime = this.getFrameTime(0);
			this.endTime = this.getFrameTime(this.keyFramesCount - 1);
		}

		// Token: 0x060002C1 RID: 705 RVA: 0x0000F6E4 File Offset: 0x0000D8E4
		private void initKeyFrameStepFromTowithTime(KeyFrame src, KeyFrame dst, float time)
		{
			this.keyFrameTimeLeft = time;
			this.setKeyFrameFromElement(this.elementPrevState);
			this.setElementFromKeyFrame(src);
			switch (this.type)
			{
			case Track.TrackType.TRACK_POSITION:
				this.currentStepPerSecond.value.pos.x = (dst.value.pos.x - src.value.pos.x) / this.keyFrameTimeLeft;
				this.currentStepPerSecond.value.pos.y = (dst.value.pos.y - src.value.pos.y) / this.keyFrameTimeLeft;
				break;
			case Track.TrackType.TRACK_SCALE:
				this.currentStepPerSecond.value.scale.scaleX = (dst.value.scale.scaleX - src.value.scale.scaleX) / this.keyFrameTimeLeft;
				this.currentStepPerSecond.value.scale.scaleY = (dst.value.scale.scaleY - src.value.scale.scaleY) / this.keyFrameTimeLeft;
				break;
			case Track.TrackType.TRACK_ROTATION:
				this.currentStepPerSecond.value.rotation.angle = (dst.value.rotation.angle - src.value.rotation.angle) / this.keyFrameTimeLeft;
				break;
			case Track.TrackType.TRACK_COLOR:
				this.currentStepPerSecond.value.color.rgba.r = (dst.value.color.rgba.r - src.value.color.rgba.r) / this.keyFrameTimeLeft;
				this.currentStepPerSecond.value.color.rgba.g = (dst.value.color.rgba.g - src.value.color.rgba.g) / this.keyFrameTimeLeft;
				this.currentStepPerSecond.value.color.rgba.b = (dst.value.color.rgba.b - src.value.color.rgba.b) / this.keyFrameTimeLeft;
				this.currentStepPerSecond.value.color.rgba.a = (dst.value.color.rgba.a - src.value.color.rgba.a) / this.keyFrameTimeLeft;
				break;
			}
			if (dst.transitionType == KeyFrame.TransitionType.FRAME_TRANSITION_EASE_IN || dst.transitionType == KeyFrame.TransitionType.FRAME_TRANSITION_EASE_OUT)
			{
				switch (this.type)
				{
				case Track.TrackType.TRACK_POSITION:
					this.currentStepPerSecond.value.pos.x *= 2f;
					this.currentStepPerSecond.value.pos.y *= 2f;
					this.currentStepAcceleration.value.pos.x = this.currentStepPerSecond.value.pos.x / this.keyFrameTimeLeft;
					this.currentStepAcceleration.value.pos.y = this.currentStepPerSecond.value.pos.y / this.keyFrameTimeLeft;
					if (dst.transitionType == KeyFrame.TransitionType.FRAME_TRANSITION_EASE_IN)
					{
						this.currentStepPerSecond.value.pos.x = 0f;
						this.currentStepPerSecond.value.pos.y = 0f;
					}
					else
					{
						this.currentStepAcceleration.value.pos.x *= -1f;
						this.currentStepAcceleration.value.pos.y *= -1f;
					}
					break;
				case Track.TrackType.TRACK_SCALE:
					this.currentStepPerSecond.value.scale.scaleX *= 2f;
					this.currentStepPerSecond.value.scale.scaleY *= 2f;
					this.currentStepAcceleration.value.scale.scaleX = this.currentStepPerSecond.value.scale.scaleX / this.keyFrameTimeLeft;
					this.currentStepAcceleration.value.scale.scaleY = this.currentStepPerSecond.value.scale.scaleY / this.keyFrameTimeLeft;
					if (dst.transitionType == KeyFrame.TransitionType.FRAME_TRANSITION_EASE_IN)
					{
						this.currentStepPerSecond.value.scale.scaleX = 0f;
						this.currentStepPerSecond.value.scale.scaleY = 0f;
					}
					else
					{
						this.currentStepAcceleration.value.scale.scaleX *= -1f;
						this.currentStepAcceleration.value.scale.scaleY *= -1f;
					}
					break;
				case Track.TrackType.TRACK_ROTATION:
					this.currentStepPerSecond.value.rotation.angle *= 2f;
					this.currentStepAcceleration.value.rotation.angle = this.currentStepPerSecond.value.rotation.angle / this.keyFrameTimeLeft;
					if (dst.transitionType == KeyFrame.TransitionType.FRAME_TRANSITION_EASE_IN)
					{
						this.currentStepPerSecond.value.rotation.angle = 0f;
					}
					else
					{
						this.currentStepAcceleration.value.rotation.angle *= -1f;
					}
					break;
				case Track.TrackType.TRACK_COLOR:
				{
					ColorParams color = this.currentStepPerSecond.value.color;
					color.rgba.r = color.rgba.r * 2f;
					ColorParams color2 = this.currentStepPerSecond.value.color;
					color2.rgba.g = color2.rgba.g * 2f;
					ColorParams color3 = this.currentStepPerSecond.value.color;
					color3.rgba.b = color3.rgba.b * 2f;
					ColorParams color4 = this.currentStepPerSecond.value.color;
					color4.rgba.a = color4.rgba.a * 2f;
					this.currentStepAcceleration.value.color.rgba.r = this.currentStepPerSecond.value.color.rgba.r / this.keyFrameTimeLeft;
					this.currentStepAcceleration.value.color.rgba.g = this.currentStepPerSecond.value.color.rgba.g / this.keyFrameTimeLeft;
					this.currentStepAcceleration.value.color.rgba.b = this.currentStepPerSecond.value.color.rgba.b / this.keyFrameTimeLeft;
					this.currentStepAcceleration.value.color.rgba.a = this.currentStepPerSecond.value.color.rgba.a / this.keyFrameTimeLeft;
					if (dst.transitionType == KeyFrame.TransitionType.FRAME_TRANSITION_EASE_IN)
					{
						this.currentStepPerSecond.value.color.rgba.r = 0f;
						this.currentStepPerSecond.value.color.rgba.g = 0f;
						this.currentStepPerSecond.value.color.rgba.b = 0f;
						this.currentStepPerSecond.value.color.rgba.a = 0f;
					}
					else
					{
						ColorParams color5 = this.currentStepAcceleration.value.color;
						color5.rgba.r = color5.rgba.r * -1f;
						ColorParams color6 = this.currentStepAcceleration.value.color;
						color6.rgba.g = color6.rgba.g * -1f;
						ColorParams color7 = this.currentStepAcceleration.value.color;
						color7.rgba.b = color7.rgba.b * -1f;
						ColorParams color8 = this.currentStepAcceleration.value.color;
						color8.rgba.a = color8.rgba.a * -1f;
					}
					break;
				}
				}
			}
			if (this.overrun > 0f)
			{
				Track.updateTrack(this, this.overrun);
				this.overrun = 0f;
			}
		}

		// Token: 0x060002C2 RID: 706 RVA: 0x0000FF84 File Offset: 0x0000E184
		public virtual void setElementFromKeyFrame(KeyFrame kf)
		{
			switch (this.type)
			{
			case Track.TrackType.TRACK_POSITION:
				if (!this.relative)
				{
					this.t.element.x = kf.value.pos.x;
					this.t.element.y = kf.value.pos.y;
					return;
				}
				this.t.element.x = this.elementPrevState.value.pos.x + kf.value.pos.x;
				this.t.element.y = this.elementPrevState.value.pos.y + kf.value.pos.y;
				return;
			case Track.TrackType.TRACK_SCALE:
				if (!this.relative)
				{
					this.t.element.scaleX = kf.value.scale.scaleX;
					this.t.element.scaleY = kf.value.scale.scaleY;
					return;
				}
				this.t.element.scaleX = this.elementPrevState.value.scale.scaleX + kf.value.scale.scaleX;
				this.t.element.scaleY = this.elementPrevState.value.scale.scaleY + kf.value.scale.scaleY;
				return;
			case Track.TrackType.TRACK_ROTATION:
				if (!this.relative)
				{
					this.t.element.rotation = kf.value.rotation.angle;
					return;
				}
				this.t.element.rotation = this.elementPrevState.value.rotation.angle + kf.value.rotation.angle;
				return;
			case Track.TrackType.TRACK_COLOR:
				if (!this.relative)
				{
					this.t.element.color = kf.value.color.rgba;
					return;
				}
				this.t.element.color.r = this.elementPrevState.value.color.rgba.r + kf.value.color.rgba.r;
				this.t.element.color.g = this.elementPrevState.value.color.rgba.g + kf.value.color.rgba.g;
				this.t.element.color.b = this.elementPrevState.value.color.rgba.b + kf.value.color.rgba.b;
				this.t.element.color.a = this.elementPrevState.value.color.rgba.a + kf.value.color.rgba.a;
				return;
			case Track.TrackType.TRACK_ACTION:
			{
				for (int i = 0; i < kf.value.action.actionSet.Count; i++)
				{
					Action action = kf.value.action.actionSet[i];
					action.actionTarget.handleAction(action.data);
				}
				return;
			}
			default:
				return;
			}
		}

		// Token: 0x060002C3 RID: 707 RVA: 0x00010308 File Offset: 0x0000E508
		private void setKeyFrameFromElement(KeyFrame kf)
		{
			switch (this.type)
			{
			case Track.TrackType.TRACK_POSITION:
				kf.value.pos.x = this.t.element.x;
				kf.value.pos.y = this.t.element.y;
				return;
			case Track.TrackType.TRACK_SCALE:
				kf.value.scale.scaleX = this.t.element.scaleX;
				kf.value.scale.scaleY = this.t.element.scaleY;
				return;
			case Track.TrackType.TRACK_ROTATION:
				kf.value.rotation.angle = this.t.element.rotation;
				return;
			case Track.TrackType.TRACK_COLOR:
				kf.value.color.rgba = this.t.element.color;
				break;
			case Track.TrackType.TRACK_ACTION:
				break;
			default:
				return;
			}
		}

		// Token: 0x060002C4 RID: 708 RVA: 0x000103FC File Offset: 0x0000E5FC
		public static void updateActionTrack(Track thiss, float delta)
		{
			if (thiss == null)
			{
				return;
			}
			if (thiss.state == Track.TrackState.TRACK_NOT_ACTIVE)
			{
				if (!thiss.t.timelineDirReverse)
				{
					if (thiss.t.time - delta <= thiss.endTime && thiss.t.time >= thiss.startTime)
					{
						if (thiss.keyFramesCount > 1)
						{
							thiss.state = Track.TrackState.TRACK_ACTIVE;
							thiss.nextKeyFrame = 0;
							thiss.overrun = thiss.t.time - thiss.startTime;
							thiss.nextKeyFrame++;
							thiss.initActionKeyFrameandTime(thiss.keyFrames[thiss.nextKeyFrame - 1], thiss.keyFrames[thiss.nextKeyFrame].timeOffset);
							return;
						}
						thiss.initActionKeyFrameandTime(thiss.keyFrames[0], 0f);
						return;
					}
				}
				else if (thiss.t.time + delta >= thiss.startTime && thiss.t.time <= thiss.endTime)
				{
					if (thiss.keyFramesCount > 1)
					{
						thiss.state = Track.TrackState.TRACK_ACTIVE;
						thiss.nextKeyFrame = thiss.keyFramesCount - 1;
						thiss.overrun = thiss.endTime - thiss.t.time;
						thiss.nextKeyFrame--;
						thiss.initActionKeyFrameandTime(thiss.keyFrames[thiss.nextKeyFrame + 1], thiss.keyFrames[thiss.nextKeyFrame + 1].timeOffset);
						return;
					}
					thiss.initActionKeyFrameandTime(thiss.keyFrames[0], 0f);
				}
				return;
			}
			thiss.keyFrameTimeLeft -= delta;
			if (thiss.keyFrameTimeLeft <= 1E-06f)
			{
				if (thiss.t != null && thiss.t.delegateTimelineDelegate != null)
				{
					thiss.t.delegateTimelineDelegate.timelinereachedKeyFramewithIndex(thiss.t, thiss.keyFrames[thiss.nextKeyFrame], thiss.nextKeyFrame);
				}
				thiss.overrun = 0f - thiss.keyFrameTimeLeft;
				if (thiss.nextKeyFrame == thiss.keyFramesCount - 1)
				{
					thiss.setElementFromKeyFrame(thiss.keyFrames[thiss.nextKeyFrame]);
					thiss.state = Track.TrackState.TRACK_NOT_ACTIVE;
					return;
				}
				if (thiss.nextKeyFrame == 0)
				{
					thiss.setElementFromKeyFrame(thiss.keyFrames[thiss.nextKeyFrame]);
					thiss.state = Track.TrackState.TRACK_NOT_ACTIVE;
					return;
				}
				if (!thiss.t.timelineDirReverse)
				{
					thiss.nextKeyFrame++;
					thiss.initActionKeyFrameandTime(thiss.keyFrames[thiss.nextKeyFrame - 1], thiss.keyFrames[thiss.nextKeyFrame].timeOffset);
					return;
				}
				thiss.nextKeyFrame--;
				thiss.initActionKeyFrameandTime(thiss.keyFrames[thiss.nextKeyFrame + 1], thiss.keyFrames[thiss.nextKeyFrame + 1].timeOffset);
			}
		}

		// Token: 0x060002C5 RID: 709 RVA: 0x000106B8 File Offset: 0x0000E8B8
		public static void updateTrack(Track thiss, float delta)
		{
			Timeline timeline = thiss.t;
			if (thiss.state == Track.TrackState.TRACK_NOT_ACTIVE)
			{
				if (timeline.time >= thiss.startTime && timeline.time <= thiss.endTime)
				{
					thiss.state = Track.TrackState.TRACK_ACTIVE;
					if (!timeline.timelineDirReverse)
					{
						thiss.nextKeyFrame = 0;
						thiss.overrun = timeline.time - thiss.startTime;
						thiss.nextKeyFrame++;
						thiss.initKeyFrameStepFromTowithTime(thiss.keyFrames[thiss.nextKeyFrame - 1], thiss.keyFrames[thiss.nextKeyFrame], thiss.keyFrames[thiss.nextKeyFrame].timeOffset);
						return;
					}
					thiss.nextKeyFrame = thiss.keyFramesCount - 1;
					thiss.overrun = thiss.endTime - timeline.time;
					thiss.nextKeyFrame--;
					thiss.initKeyFrameStepFromTowithTime(thiss.keyFrames[thiss.nextKeyFrame + 1], thiss.keyFrames[thiss.nextKeyFrame], thiss.keyFrames[thiss.nextKeyFrame + 1].timeOffset);
				}
				return;
			}
			thiss.keyFrameTimeLeft -= delta;
			if (thiss.keyFrames[thiss.nextKeyFrame].transitionType == KeyFrame.TransitionType.FRAME_TRANSITION_EASE_IN || thiss.keyFrames[thiss.nextKeyFrame].transitionType == KeyFrame.TransitionType.FRAME_TRANSITION_EASE_OUT)
			{
				KeyFrame keyFrame = thiss.currentStepPerSecond;
				switch (thiss.type)
				{
				case Track.TrackType.TRACK_POSITION:
				{
					float num8 = thiss.currentStepAcceleration.value.pos.x * delta;
					float num9 = thiss.currentStepAcceleration.value.pos.y * delta;
					thiss.currentStepPerSecond.value.pos.x += num8;
					thiss.currentStepPerSecond.value.pos.y += num9;
					timeline.element.x += (keyFrame.value.pos.x + num8 / 2f) * delta;
					timeline.element.y += (keyFrame.value.pos.y + num9 / 2f) * delta;
					break;
				}
				case Track.TrackType.TRACK_SCALE:
				{
					float num10 = thiss.currentStepAcceleration.value.scale.scaleX * delta;
					float num11 = thiss.currentStepAcceleration.value.scale.scaleY * delta;
					thiss.currentStepPerSecond.value.scale.scaleX += num10;
					thiss.currentStepPerSecond.value.scale.scaleY += num11;
					timeline.element.scaleX += (keyFrame.value.scale.scaleX + num10 / 2f) * delta;
					timeline.element.scaleY += (keyFrame.value.scale.scaleY + num11 / 2f) * delta;
					break;
				}
				case Track.TrackType.TRACK_ROTATION:
				{
					float num12 = thiss.currentStepAcceleration.value.rotation.angle * delta;
					thiss.currentStepPerSecond.value.rotation.angle += num12;
					timeline.element.rotation += (keyFrame.value.rotation.angle + num12 / 2f) * delta;
					break;
				}
				case Track.TrackType.TRACK_COLOR:
				{
					ColorParams color = thiss.currentStepPerSecond.value.color;
					color.rgba.r = color.rgba.r + thiss.currentStepAcceleration.value.color.rgba.r * delta;
					ColorParams color2 = thiss.currentStepPerSecond.value.color;
					color2.rgba.g = color2.rgba.g + thiss.currentStepAcceleration.value.color.rgba.g * delta;
					ColorParams color3 = thiss.currentStepPerSecond.value.color;
					color3.rgba.b = color3.rgba.b + thiss.currentStepAcceleration.value.color.rgba.b * delta;
					ColorParams color4 = thiss.currentStepPerSecond.value.color;
					color4.rgba.a = color4.rgba.a + thiss.currentStepAcceleration.value.color.rgba.a * delta;
					float num13 = thiss.currentStepAcceleration.value.color.rgba.r * delta;
					float num14 = thiss.currentStepAcceleration.value.color.rgba.g * delta;
					float num15 = thiss.currentStepAcceleration.value.color.rgba.b * delta;
					float num16 = thiss.currentStepAcceleration.value.color.rgba.a * delta;
					ColorParams color5 = thiss.currentStepPerSecond.value.color;
					color5.rgba.r = color5.rgba.r + num13;
					ColorParams color6 = thiss.currentStepPerSecond.value.color;
					color6.rgba.g = color6.rgba.g + num14;
					ColorParams color7 = thiss.currentStepPerSecond.value.color;
					color7.rgba.b = color7.rgba.b + num15;
					ColorParams color8 = thiss.currentStepPerSecond.value.color;
					color8.rgba.a = color8.rgba.a + num16;
					BaseElement element = timeline.element;
					element.color.r = element.color.r + (keyFrame.value.color.rgba.r + num13 / 2f) * delta;
					BaseElement element2 = timeline.element;
					element2.color.g = element2.color.g + (keyFrame.value.color.rgba.g + num14 / 2f) * delta;
					BaseElement element3 = timeline.element;
					element3.color.b = element3.color.b + (keyFrame.value.color.rgba.b + num15 / 2f) * delta;
					BaseElement element4 = timeline.element;
					element4.color.a = element4.color.a + (keyFrame.value.color.rgba.a + num16 / 2f) * delta;
					break;
				}
				}
			}
			else if (thiss.keyFrames[thiss.nextKeyFrame].transitionType == KeyFrame.TransitionType.FRAME_TRANSITION_LINEAR)
			{
				switch (thiss.type)
				{
				case Track.TrackType.TRACK_POSITION:
					timeline.element.x += thiss.currentStepPerSecond.value.pos.x * delta;
					timeline.element.y += thiss.currentStepPerSecond.value.pos.y * delta;
					break;
				case Track.TrackType.TRACK_SCALE:
					timeline.element.scaleX += thiss.currentStepPerSecond.value.scale.scaleX * delta;
					timeline.element.scaleY += thiss.currentStepPerSecond.value.scale.scaleY * delta;
					break;
				case Track.TrackType.TRACK_ROTATION:
					timeline.element.rotation += thiss.currentStepPerSecond.value.rotation.angle * delta;
					break;
				case Track.TrackType.TRACK_COLOR:
				{
					BaseElement element5 = timeline.element;
					element5.color.r = element5.color.r + thiss.currentStepPerSecond.value.color.rgba.r * delta;
					BaseElement element6 = timeline.element;
					element6.color.g = element6.color.g + thiss.currentStepPerSecond.value.color.rgba.g * delta;
					BaseElement element7 = timeline.element;
					element7.color.b = element7.color.b + thiss.currentStepPerSecond.value.color.rgba.b * delta;
					BaseElement element8 = timeline.element;
					element8.color.a = element8.color.a + thiss.currentStepPerSecond.value.color.rgba.a * delta;
					break;
				}
				}
			}
			if (thiss.keyFrameTimeLeft <= 1E-06f)
			{
				if (timeline.delegateTimelineDelegate != null)
				{
					timeline.delegateTimelineDelegate.timelinereachedKeyFramewithIndex(timeline, thiss.keyFrames[thiss.nextKeyFrame], thiss.nextKeyFrame);
				}
				thiss.overrun = 0f - thiss.keyFrameTimeLeft;
				if (thiss.nextKeyFrame == thiss.keyFramesCount - 1)
				{
					thiss.setElementFromKeyFrame(thiss.keyFrames[thiss.nextKeyFrame]);
					thiss.state = Track.TrackState.TRACK_NOT_ACTIVE;
					return;
				}
				if (thiss.nextKeyFrame == 0)
				{
					thiss.setElementFromKeyFrame(thiss.keyFrames[thiss.nextKeyFrame]);
					thiss.state = Track.TrackState.TRACK_NOT_ACTIVE;
					return;
				}
				if (!timeline.timelineDirReverse)
				{
					thiss.nextKeyFrame++;
					thiss.initKeyFrameStepFromTowithTime(thiss.keyFrames[thiss.nextKeyFrame - 1], thiss.keyFrames[thiss.nextKeyFrame], thiss.keyFrames[thiss.nextKeyFrame].timeOffset);
					return;
				}
				thiss.nextKeyFrame--;
				thiss.initKeyFrameStepFromTowithTime(thiss.keyFrames[thiss.nextKeyFrame + 1], thiss.keyFrames[thiss.nextKeyFrame], thiss.keyFrames[thiss.nextKeyFrame + 1].timeOffset);
			}
		}

		// Token: 0x0400021E RID: 542
		public Track.TrackType type;

		// Token: 0x0400021F RID: 543
		public Track.TrackState state;

		// Token: 0x04000220 RID: 544
		public bool relative;

		// Token: 0x04000221 RID: 545
		public float startTime;

		// Token: 0x04000222 RID: 546
		public float endTime;

		// Token: 0x04000223 RID: 547
		public int keyFramesCount;

		// Token: 0x04000224 RID: 548
		public KeyFrame[] keyFrames;

		// Token: 0x04000225 RID: 549
		public Timeline t;

		// Token: 0x04000226 RID: 550
		public int nextKeyFrame;

		// Token: 0x04000227 RID: 551
		public int keyFramesCapacity;

		// Token: 0x04000228 RID: 552
		public KeyFrame currentStepPerSecond;

		// Token: 0x04000229 RID: 553
		public KeyFrame currentStepAcceleration;

		// Token: 0x0400022A RID: 554
		public float keyFrameTimeLeft;

		// Token: 0x0400022B RID: 555
		public KeyFrame elementPrevState;

		// Token: 0x0400022C RID: 556
		public float overrun;

		// Token: 0x0400022D RID: 557
		public List<List<Action>> actionSets;

		// Token: 0x020000B7 RID: 183
		public enum TrackType
		{
			// Token: 0x040008BB RID: 2235
			TRACK_POSITION,
			// Token: 0x040008BC RID: 2236
			TRACK_SCALE,
			// Token: 0x040008BD RID: 2237
			TRACK_ROTATION,
			// Token: 0x040008BE RID: 2238
			TRACK_COLOR,
			// Token: 0x040008BF RID: 2239
			TRACK_ACTION,
			// Token: 0x040008C0 RID: 2240
			TRACKS_COUNT
		}

		// Token: 0x020000B8 RID: 184
		public enum TrackState
		{
			// Token: 0x040008C2 RID: 2242
			TRACK_NOT_ACTIVE,
			// Token: 0x040008C3 RID: 2243
			TRACK_ACTIVE
		}
	}
}
