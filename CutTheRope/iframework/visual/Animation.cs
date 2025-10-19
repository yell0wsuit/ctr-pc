using System;
using System.Collections.Generic;
using CutTheRope.iframework.core;

namespace CutTheRope.iframework.visual
{
	// Token: 0x02000029 RID: 41
	internal class Animation : Image
	{
		// Token: 0x06000148 RID: 328 RVA: 0x00006F84 File Offset: 0x00005184
		public static Animation Animation_create(Texture2D t)
		{
			return (Animation)new Animation().initWithTexture(t);
		}

		// Token: 0x06000149 RID: 329 RVA: 0x00006F96 File Offset: 0x00005196
		public static Animation Animation_createWithResID(int r)
		{
			return Animation.Animation_create(Application.getTexture(r));
		}

		// Token: 0x0600014A RID: 330 RVA: 0x00006FA4 File Offset: 0x000051A4
		public static Animation Animation_createWithResIDQuad(int r, int q)
		{
			Animation animation = Animation.Animation_createWithResID(r);
			if (animation != null)
			{
				animation.setDrawQuad(q);
			}
			return animation;
		}

		// Token: 0x0600014B RID: 331 RVA: 0x00006FC4 File Offset: 0x000051C4
		public virtual void addAnimationWithIDDelayLoopFirstLast(int aid, float d, Timeline.LoopType l, int s, int e)
		{
			int c = e - s + 1;
			this.addAnimationWithIDDelayLoopCountFirstLastArgumentList(aid, d, l, c, s, e);
		}

		// Token: 0x0600014C RID: 332 RVA: 0x00006FE8 File Offset: 0x000051E8
		public virtual void addAnimationWithIDDelayLoopCountFirstLastArgumentList(int aid, float d, Timeline.LoopType l, int c, int s, int e)
		{
			Timeline timeline = new Timeline().initWithMaxKeyFramesOnTrack(c + 2);
			timeline.addKeyFrame(KeyFrame.makeAction(new List<Action> { Action.createAction(this, "ACTION_SET_DRAWQUAD", s, 0) }, 0f));
			int num = s;
			for (int i = 1; i < c; i++)
			{
				num++;
				List<Action> list = new List<Action>();
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

		// Token: 0x0600014D RID: 333 RVA: 0x00007093 File Offset: 0x00005293
		public virtual void addAnimationWithIDDelayLoopCountSequence(int aid, float d, Timeline.LoopType l, int c, int s, List<int> al)
		{
			this.addAnimationWithIDDelayLoopCountFirstLastArgumentList(aid, d, l, c, s, -1, al);
		}

		// Token: 0x0600014E RID: 334 RVA: 0x000070A8 File Offset: 0x000052A8
		public virtual void addAnimationWithIDDelayLoopCountFirstLastArgumentList(int aid, float d, Timeline.LoopType l, int c, int s, int e, List<int> al)
		{
			Timeline timeline = new Timeline().initWithMaxKeyFramesOnTrack(c + 2);
			timeline.addKeyFrame(KeyFrame.makeAction(new List<Action> { Action.createAction(this, "ACTION_SET_DRAWQUAD", s, 0) }, 0f));
			int num2 = 0;
			for (int i = 1; i < c; i++)
			{
				int num3 = al[num2++];
				List<Action> list = new List<Action>();
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

		// Token: 0x0600014F RID: 335 RVA: 0x00007164 File Offset: 0x00005364
		public virtual void switchToAnimationatEndOfAnimationDelay(int a2, int a1, float d)
		{
			this.getTimeline(a1).addKeyFrame(KeyFrame.makeAction(new List<Action> { Action.createAction(this, "ACTION_PLAY_TIMELINE", 0, a2) }, d));
		}

		// Token: 0x06000150 RID: 336 RVA: 0x0000719D File Offset: 0x0000539D
		public virtual void setPauseAtIndexforAnimation(int i, int a)
		{
			this.setActionTargetParamSubParamAtIndexforAnimation("ACTION_PAUSE_TIMELINE", this, 0, 0, i, a);
		}

		// Token: 0x06000151 RID: 337 RVA: 0x000071AF File Offset: 0x000053AF
		public virtual void setActionTargetParamSubParamAtIndexforAnimation(string action, BaseElement target, int p, int sp, int i, int a)
		{
			this.getTimeline(a).getTrack(Track.TrackType.TRACK_ACTION).keyFrames[i].value.action.actionSet.Add(Action.createAction(target, action, p, sp));
		}

		// Token: 0x06000152 RID: 338 RVA: 0x000071E8 File Offset: 0x000053E8
		public virtual int addAnimationWithDelayLoopedCountSequence(float d, Timeline.LoopType l, int c, int s, List<int> al)
		{
			int count = this.timelines.Count;
			this.addAnimationWithIDDelayLoopCountFirstLastArgumentList(count, d, l, c, s, -1, al);
			return count;
		}

		// Token: 0x06000153 RID: 339 RVA: 0x00007211 File Offset: 0x00005411
		public void setDelayatIndexforAnimation(float d, int i, int a)
		{
			this.getTimeline(a).getTrack(Track.TrackType.TRACK_ACTION).keyFrames[i].timeOffset = d;
		}

		// Token: 0x06000154 RID: 340 RVA: 0x0000722D File Offset: 0x0000542D
		public int addAnimationDelayLoopFirstLast(double d, Timeline.LoopType l, int s, int e)
		{
			return this.addAnimationDelayLoopFirstLast((float)d, l, s, e);
		}

		// Token: 0x06000155 RID: 341 RVA: 0x0000723C File Offset: 0x0000543C
		public int addAnimationDelayLoopFirstLast(float d, Timeline.LoopType l, int s, int e)
		{
			int count = this.timelines.Count;
			this.addAnimationWithIDDelayLoopFirstLast(count, d, l, s, e);
			return count;
		}

		// Token: 0x06000156 RID: 342 RVA: 0x00007262 File Offset: 0x00005462
		public void jumpTo(int i)
		{
			this.getCurrentTimeline().jumpToTrackKeyFrame(4, i);
		}
	}
}
