using System;
using CutTheRope.iframework;
using CutTheRope.iframework.core;
using CutTheRope.iframework.helpers;
using CutTheRope.iframework.visual;
using CutTheRope.ios;

namespace CutTheRope.game
{
	// Token: 0x02000096 RID: 150
	internal class Star : CTRGameObject
	{
		// Token: 0x060005FC RID: 1532 RVA: 0x00032148 File Offset: 0x00030348
		public static Star Star_create(Texture2D t)
		{
			return (Star)new Star().initWithTexture(t);
		}

		// Token: 0x060005FD RID: 1533 RVA: 0x0003215A File Offset: 0x0003035A
		public static Star Star_createWithResID(int r)
		{
			return Star.Star_create(Application.getTexture(r));
		}

		// Token: 0x060005FE RID: 1534 RVA: 0x00032167 File Offset: 0x00030367
		public static Star Star_createWithResIDQuad(int r, int q)
		{
			Star star = Star.Star_create(Application.getTexture(r));
			star.setDrawQuad(q);
			return star;
		}

		// Token: 0x060005FF RID: 1535 RVA: 0x0003217B File Offset: 0x0003037B
		public override NSObject init()
		{
			if (base.init() != null)
			{
				this.timedAnim = null;
			}
			return this;
		}

		// Token: 0x06000600 RID: 1536 RVA: 0x00032190 File Offset: 0x00030390
		public override void update(float delta)
		{
			if ((double)this.timeout > 0.0 && (double)this.time > 0.0)
			{
				Mover.moveVariableToTarget(ref this.time, 0f, 1f, delta);
			}
			base.update(delta);
		}

		// Token: 0x06000601 RID: 1537 RVA: 0x000321DF File Offset: 0x000303DF
		public override void draw()
		{
			if (this.timedAnim != null)
			{
				this.timedAnim.draw();
			}
			base.draw();
		}

		// Token: 0x06000602 RID: 1538 RVA: 0x000321FC File Offset: 0x000303FC
		public virtual void createAnimations()
		{
			if ((double)this.timeout > 0.0)
			{
				this.timedAnim = Animation.Animation_createWithResID(78);
				this.timedAnim.anchor = (this.timedAnim.parentAnchor = 18);
				float d = this.timeout / 37f;
				this.timedAnim.addAnimationWithIDDelayLoopFirstLast(0, d, Timeline.LoopType.TIMELINE_NO_LOOP, 19, 55);
				this.timedAnim.playTimeline(0);
				this.time = this.timeout;
				this.timedAnim.visible = false;
				this.addChild(this.timedAnim);
				Timeline timeline = new Timeline().initWithMaxKeyFramesOnTrack(2);
				timeline.addKeyFrame(KeyFrame.makeColor(RGBAColor.solidOpaqueRGBA, KeyFrame.TransitionType.FRAME_TRANSITION_LINEAR, 0.0));
				timeline.addKeyFrame(KeyFrame.makeColor(RGBAColor.transparentRGBA, KeyFrame.TransitionType.FRAME_TRANSITION_LINEAR, 0.5));
				this.timedAnim.addTimelinewithID(timeline, 1);
				Timeline timeline2 = new Timeline().initWithMaxKeyFramesOnTrack(2);
				timeline2.addKeyFrame(KeyFrame.makeScale(1.0, 1.0, KeyFrame.TransitionType.FRAME_TRANSITION_LINEAR, 0.0));
				timeline2.addKeyFrame(KeyFrame.makeScale(0.0, 0.0, KeyFrame.TransitionType.FRAME_TRANSITION_LINEAR, 0.25));
				timeline2.addKeyFrame(KeyFrame.makeColor(RGBAColor.solidOpaqueRGBA, KeyFrame.TransitionType.FRAME_TRANSITION_LINEAR, 0.0));
				timeline2.addKeyFrame(KeyFrame.makeColor(RGBAColor.transparentRGBA, KeyFrame.TransitionType.FRAME_TRANSITION_LINEAR, 0.25));
				this.addTimelinewithID(timeline2, 1);
			}
			this.bb = new Rectangle(22f, 20f, 30f, 30f);
			Timeline timeline3 = new Timeline().initWithMaxKeyFramesOnTrack(5);
			timeline3.addKeyFrame(KeyFrame.makePos((int)this.x, (int)this.y, KeyFrame.TransitionType.FRAME_TRANSITION_EASE_IN, 0f));
			timeline3.addKeyFrame(KeyFrame.makePos((int)this.x, (int)this.y - 3, KeyFrame.TransitionType.FRAME_TRANSITION_EASE_OUT, 0.5f));
			timeline3.addKeyFrame(KeyFrame.makePos((int)this.x, (int)this.y, KeyFrame.TransitionType.FRAME_TRANSITION_EASE_IN, 0.5f));
			timeline3.addKeyFrame(KeyFrame.makePos((int)this.x, (int)this.y + 3, KeyFrame.TransitionType.FRAME_TRANSITION_EASE_OUT, 0.5f));
			timeline3.addKeyFrame(KeyFrame.makePos((int)this.x, (int)this.y, KeyFrame.TransitionType.FRAME_TRANSITION_EASE_IN, 0.5f));
			timeline3.setTimelineLoopType(Timeline.LoopType.TIMELINE_REPLAY);
			this.addTimelinewithID(timeline3, 0);
			this.playTimeline(0);
			Timeline.updateTimeline(timeline3, (float)((double)MathHelper.RND_RANGE(0, 20) / 10.0));
			Animation animation = Animation.Animation_createWithResID(78);
			animation.doRestoreCutTransparency();
			animation.addAnimationDelayLoopFirstLast(0.05f, Timeline.LoopType.TIMELINE_REPLAY, 1, 18);
			animation.playTimeline(0);
			Timeline.updateTimeline(animation.getTimeline(0), (float)((double)MathHelper.RND_RANGE(0, 20) / 10.0));
			animation.anchor = (animation.parentAnchor = 18);
			this.addChild(animation);
		}

		// Token: 0x04000837 RID: 2103
		public float time;

		// Token: 0x04000838 RID: 2104
		public float timeout;

		// Token: 0x04000839 RID: 2105
		public Animation timedAnim;
	}
}
