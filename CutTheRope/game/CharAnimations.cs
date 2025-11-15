using CutTheRope.iframework;
using CutTheRope.iframework.core;
using CutTheRope.iframework.helpers;
using CutTheRope.iframework.visual;

namespace CutTheRope.game
{
    internal sealed class CharAnimations : GameObject
    {
        public static CharAnimations CharAnimations_createWithResID(int r)
        {
            return CharAnimations_create(Application.GetTexture(r));
        }

        private static CharAnimations CharAnimations_create(CTRTexture2D t)
        {
            CharAnimations charAnimations = new();
            _ = charAnimations.InitWithTexture(t);
            return charAnimations;
        }

        public void AddImage(int resId)
        {
            animations ??= new DynamicArray<Animation>();
            CharAnimation charAnimation = CharAnimation.CharAnimation_createWithResID(resId);
            charAnimation.parentAnchor = charAnimation.anchor = 9;
            charAnimation.DoRestoreCutTransparency();
            int i = resId - 101;
            animations.SetObjectAt(charAnimation, i);
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
            }
            base.Dispose(disposing);
        }

        public void AddAnimationWithIDDelayLoopFirstLast(int a, int aid, float d, Timeline.LoopType l, int s, int e)
        {
            ((CharAnimation)animations.ObjectAtIndex(a - 101)).AddAnimationWithIDDelayLoopFirstLast(aid, d, l, s, e);
        }

        public Animation GetAnimation(int resID)
        {
            return resID == 80 ? this : animations.ObjectAtIndex(resID - 101);
        }

        public void SwitchToAnimationatEndOfAnimationDelay(int i2, int a2, int i1, int a1, float d)
        {
            Animation animation = GetAnimation(i1);
            Animation animation2 = GetAnimation(i2);
            Timeline timeline = animation.GetTimeline(a1);
            DynamicArray<CTRAction> dynamicArray = new();
            _ = dynamicArray.AddObject(CTRAction.CreateAction(animation2, "ACTION_PLAY_TIMELINE", i1 == 80 ? 1 : 0, a2));
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

        public void PlayAnimationtimeline(int resID, int t)
        {
            if (GetCurrentTimeline() != null)
            {
                StopCurrentTimeline();
            }
            foreach (Animation anim in animations)
            {
                anim.SetEnabled(false);
            }
            Animation animation = GetAnimation(resID);
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
    }
}
