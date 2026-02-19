using System.Collections.Generic;

using CutTheRope.Framework;
using CutTheRope.Framework.Core;
using CutTheRope.Framework.Helpers;
using CutTheRope.Framework.Visual;

namespace CutTheRope.GameMain
{
    internal sealed class CharAnimations : GameObject
    {
        public static CharAnimations CharAnimations_createWithResID(string resourceName)
        {
            return CharAnimations_create(Application.GetTexture(resourceName));
        }

        private static CharAnimations CharAnimations_create(CTRTexture2D t)
        {
            CharAnimations charAnimations = new();
            _ = charAnimations.InitWithTexture(t);
            return charAnimations;
        }

        public void AddImage(string resourceName)
        {
            animations ??= new DynamicArray<Animation>();
            animationNameToIndex ??= [];

            CharAnimation charAnimation = CharAnimation.CharAnimation_createWithResID(resourceName);
            // Use the same anchor as the base animation (18) for proper centering
            charAnimation.parentAnchor = charAnimation.anchor = anchor;
            charAnimation.DoRestoreCutTransparency();

            int index = nextAnimationIndex++;
            animations.SetObjectAt(charAnimation, index);
            animationNameToIndex[resourceName] = index;
            _ = AddChild(charAnimation);
            charAnimation.SetEnabled(false);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (animations != null)
                {
                    foreach (Animation animation in animations)
                    {
                        animation?.Dispose();
                    }
                    animations.RemoveAllObjects();
                    animations = null;
                }
                animationNameToIndex?.Clear();
                animationNameToIndex = null;
            }
            base.Dispose(disposing);
        }

        public void AddAnimationWithIDDelayLoopFirstLast(string resourceName, int aid, float d, Timeline.LoopType l, int s, int e)
        {
            if (resourceName == Resources.Img.CharAnimations)
            {
                AddAnimationWithIDDelayLoopFirstLast(aid, d, l, s, e);
            }
            else if (animationNameToIndex != null && animationNameToIndex.TryGetValue(resourceName, out int index))
            {
                ((CharAnimation)animations.ObjectAtIndex(index)).AddAnimationWithIDDelayLoopFirstLast(aid, d, l, s, e);
            }
        }

        public Animation GetAnimation(string resourceName)
        {
            return resourceName == Resources.Img.CharAnimations
                ? this
                : animationNameToIndex != null && animationNameToIndex.TryGetValue(resourceName, out int index)
                ? animations.ObjectAtIndex(index)
                : null;
        }

        public void SwitchToAnimationatEndOfAnimationDelay(string resourceName2, int a2, string resourceName1, int a1, float d)
        {
            Animation animation = GetAnimation(resourceName1);
            Animation animation2 = GetAnimation(resourceName2);
            Timeline timeline = animation.GetTimeline(a1);
            DynamicArray<CTRAction> dynamicArray = new();
            // Check if resourceName1 refers to the base animation (CharAnimations)
            bool isBaseAnimation = resourceName1 == Resources.Img.CharAnimations;
            _ = dynamicArray.AddObject(CTRAction.CreateAction(animation2, "ACTION_PLAY_TIMELINE", isBaseAnimation ? 1 : 0, a2));
            if (animation != animation2)
            {
                _ = dynamicArray.AddObject(CTRAction.CreateAction(animation2, "ACTION_SET_UPDATEABLE", 1, 1));
                _ = dynamicArray.AddObject(CTRAction.CreateAction(animation2, "ACTION_SET_VISIBLE", 1, 1));
                _ = dynamicArray.AddObject(CTRAction.CreateAction(animation2, "ACTION_SET_TOUCHABLE", 1, 1));
                _ = dynamicArray.AddObject(CTRAction.CreateAction(animation, "ACTION_SET_UPDATEABLE", 0, 0));
                _ = dynamicArray.AddObject(CTRAction.CreateAction(animation, "ACTION_SET_VISIBLE", 0, 0));
                _ = dynamicArray.AddObject(CTRAction.CreateAction(animation, "ACTION_SET_TOUCHABLE", 0, 0));
            }
            timeline.AddKeyFrame(KeyFrame.MakeAction(dynamicArray, d));
        }

        public void PlayAnimationtimeline(string resourceName, int t)
        {
            if (GetCurrentTimeline() != null)
            {
                StopCurrentTimeline();
            }
            foreach (Animation anim in animations)
            {
                anim.SetEnabled(false);
            }
            Animation animation = GetAnimation(resourceName);
            animation.SetEnabled(true);
            color = animation == this ? RGBAColor.solidOpaqueRGBA : RGBAColor.transparentRGBA;
            animation.PlayTimeline(t);
        }

        public override void PlayTimeline(int t)
        {
            foreach (object obj in animations)
            {
                ((Animation)obj).SetEnabled(false);
            }
            color = RGBAColor.solidOpaqueRGBA;
            base.PlayTimeline(t);
        }

        private DynamicArray<Animation> animations;
        private Dictionary<string, int> animationNameToIndex;
        private int nextAnimationIndex;
    }
}
