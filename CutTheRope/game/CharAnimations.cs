using CutTheRope.iframework;
using CutTheRope.iframework.core;
using CutTheRope.iframework.helpers;
using CutTheRope.iframework.visual;
using System;

namespace CutTheRope.game
{
    internal class CharAnimations : GameObject
    {
        public static CharAnimations CharAnimations_createWithResID(int r)
        {
            return CharAnimations.CharAnimations_create(Application.getTexture(r));
        }

        private static CharAnimations CharAnimations_create(CTRTexture2D t)
        {
            CharAnimations charAnimations = new();
            charAnimations.initWithTexture(t);
            return charAnimations;
        }

        public virtual void addImage(int resId)
        {
            if (this.animations == null)
            {
                this.animations = (DynamicArray)new DynamicArray().init();
            }
            CharAnimation charAnimation = CharAnimation.CharAnimation_createWithResID(resId);
            charAnimation.parentAnchor = charAnimation.anchor = 9;
            charAnimation.doRestoreCutTransparency();
            int i = resId - 101;
            this.animations.setObjectAt(charAnimation, i);
            this.addChild(charAnimation);
            charAnimation.setEnabled(false);
        }

        public override void dealloc()
        {
            this.animations.removeAllObjects();
            this.animations = null;
            base.dealloc();
        }

        public virtual void addAnimationWithIDDelayLoopFirstLast(int a, int aid, float d, Timeline.LoopType l, int s, int e)
        {
            ((CharAnimation)this.animations.objectAtIndex(a - 101)).addAnimationWithIDDelayLoopFirstLast(aid, d, l, s, e);
        }

        public virtual Animation getAnimation(int resID)
        {
            if (resID == 80)
            {
                return this;
            }
            return (Animation)this.animations.objectAtIndex(resID - 101);
        }

        public virtual void switchToAnimationatEndOfAnimationDelay(int i2, int a2, int i1, int a1, float d)
        {
            Animation animation = this.getAnimation(i1);
            Animation animation2 = this.getAnimation(i2);
            Timeline timeline = animation.getTimeline(a1);
            DynamicArray dynamicArray = (DynamicArray)new DynamicArray().init();
            dynamicArray.addObject(CTRAction.createAction(animation2, "ACTION_PLAY_TIMELINE", (i1 == 80) ? 1 : 0, a2));
            if (animation != animation2)
            {
                dynamicArray.addObject(CTRAction.createAction(animation2, "ACTION_SET_UPDATEABLE", 1, 1));
                dynamicArray.addObject(CTRAction.createAction(animation2, "ACTION_SET_VISIBLE", 1, 1));
                dynamicArray.addObject(CTRAction.createAction(animation2, "ACTION_SET_TOUCHABLE", 1, 1));
                dynamicArray.addObject(CTRAction.createAction(animation, "ACTION_SET_UPDATEABLE", 0, 0));
                dynamicArray.addObject(CTRAction.createAction(animation, "ACTION_SET_VISIBLE", 0, 0));
                dynamicArray.addObject(CTRAction.createAction(animation, "ACTION_SET_TOUCHABLE", 0, 0));
            }
            timeline.addKeyFrame(KeyFrame.makeAction(dynamicArray, d));
        }

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

        public override void playTimeline(int t)
        {
            foreach (object obj in this.animations)
            {
                ((Animation)obj).setEnabled(false);
            }
            this.color = RGBAColor.solidOpaqueRGBA;
            base.playTimeline(t);
        }

        public DynamicArray animations;
    }
}
