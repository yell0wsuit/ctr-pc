using System;
using CutTheRope.iframework;
using CutTheRope.iframework.core;
using CutTheRope.iframework.helpers;
using CutTheRope.iframework.visual;

namespace CutTheRope.game
{
	// Token: 0x02000074 RID: 116
	internal class CharAnimations : GameObject
	{
		// Token: 0x06000477 RID: 1143 RVA: 0x00019DE1 File Offset: 0x00017FE1
		public static CharAnimations CharAnimations_createWithResID(int r)
		{
			return CharAnimations.CharAnimations_create(Application.getTexture(r));
		}

		// Token: 0x06000478 RID: 1144 RVA: 0x00019DEE File Offset: 0x00017FEE
		private static CharAnimations CharAnimations_create(Texture2D t)
		{
			CharAnimations charAnimations = new CharAnimations();
			charAnimations.initWithTexture(t);
			return charAnimations;
		}

		// Token: 0x06000479 RID: 1145 RVA: 0x00019E00 File Offset: 0x00018000
		public virtual void addImage(int resId)
		{
			if (this.animations == null)
			{
				this.animations = (DynamicArray)new DynamicArray().init();
			}
			CharAnimation charAnimation = CharAnimation.CharAnimation_createWithResID(resId);
			charAnimation.parentAnchor = (charAnimation.anchor = 9);
			charAnimation.doRestoreCutTransparency();
			int i = resId - 101;
			this.animations.setObjectAt(charAnimation, i);
			this.addChild(charAnimation);
			charAnimation.setEnabled(false);
		}

		// Token: 0x0600047A RID: 1146 RVA: 0x00019E69 File Offset: 0x00018069
		public override void dealloc()
		{
			this.animations.removeAllObjects();
			this.animations = null;
			base.dealloc();
		}

		// Token: 0x0600047B RID: 1147 RVA: 0x00019E83 File Offset: 0x00018083
		public virtual void addAnimationWithIDDelayLoopFirstLast(int a, int aid, float d, Timeline.LoopType l, int s, int e)
		{
			((CharAnimation)this.animations.objectAtIndex(a - 101)).addAnimationWithIDDelayLoopFirstLast(aid, d, l, s, e);
		}

		// Token: 0x0600047C RID: 1148 RVA: 0x00019EA6 File Offset: 0x000180A6
		public virtual Animation getAnimation(int resID)
		{
			if (resID == 80)
			{
				return this;
			}
			return (Animation)this.animations.objectAtIndex(resID - 101);
		}

		// Token: 0x0600047D RID: 1149 RVA: 0x00019EC4 File Offset: 0x000180C4
		public virtual void switchToAnimationatEndOfAnimationDelay(int i2, int a2, int i1, int a1, float d)
		{
			Animation animation = this.getAnimation(i1);
			Animation animation2 = this.getAnimation(i2);
			Timeline timeline = animation.getTimeline(a1);
			DynamicArray dynamicArray = (DynamicArray)new DynamicArray().init();
			dynamicArray.addObject(Action.createAction(animation2, "ACTION_PLAY_TIMELINE", (i1 == 80) ? 1 : 0, a2));
			if (animation != animation2)
			{
				dynamicArray.addObject(Action.createAction(animation2, "ACTION_SET_UPDATEABLE", 1, 1));
				dynamicArray.addObject(Action.createAction(animation2, "ACTION_SET_VISIBLE", 1, 1));
				dynamicArray.addObject(Action.createAction(animation2, "ACTION_SET_TOUCHABLE", 1, 1));
				dynamicArray.addObject(Action.createAction(animation, "ACTION_SET_UPDATEABLE", 0, 0));
				dynamicArray.addObject(Action.createAction(animation, "ACTION_SET_VISIBLE", 0, 0));
				dynamicArray.addObject(Action.createAction(animation, "ACTION_SET_TOUCHABLE", 0, 0));
			}
			timeline.addKeyFrame(KeyFrame.makeAction(dynamicArray, d));
		}

		// Token: 0x0600047E RID: 1150 RVA: 0x00019F9C File Offset: 0x0001819C
		public virtual void playAnimationtimeline(int resID, int t)
		{
			if (this.getCurrentTimeline() != null)
			{
				this.stopCurrentTimeline();
			}
			foreach (object obj in this.animations)
			{
				((Animation)obj).setEnabled(false);
			}
			Animation animation = this.getAnimation(resID);
			animation.setEnabled(true);
			if (animation == this)
			{
				this.color = RGBAColor.solidOpaqueRGBA;
			}
			else
			{
				this.color = RGBAColor.transparentRGBA;
			}
			animation.playTimeline(t);
		}

		// Token: 0x0600047F RID: 1151 RVA: 0x0001A034 File Offset: 0x00018234
		public override void playTimeline(int t)
		{
			foreach (object obj in this.animations)
			{
				((Animation)obj).setEnabled(false);
			}
			this.color = RGBAColor.solidOpaqueRGBA;
			base.playTimeline(t);
		}

		// Token: 0x04000317 RID: 791
		public DynamicArray animations;
	}
}
