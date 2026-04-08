using System.Collections.Generic;

using CutTheRope.Framework;
using CutTheRope.Framework.Core;
using CutTheRope.Framework.Helpers;
using CutTheRope.Framework.Visual;

namespace CutTheRope.GameMain
{
    /// <summary>
    /// Om Nom character animation container that switches between the base texture and named child animations.
    /// </summary>
    internal sealed class CharAnimations : GameObject
    {
        /// <summary>
        /// Creates an Om Nom character animation container from a texture resource name.
        /// </summary>
        /// <param name="resourceName">Texture resource name to load.</param>
        /// <returns>The initialized Om Nom character animation container.</returns>
        public static CharAnimations CharAnimations_createWithResID(string resourceName)
        {
            return CharAnimations_create(Application.GetTexture(resourceName));
        }

        /// <summary>
        /// Creates an Om Nom character animation container from a texture.
        /// </summary>
        /// <param name="t">Texture used by the Om Nom character animation container.</param>
        /// <returns>The initialized Om Nom character animation container.</returns>
        private static CharAnimations CharAnimations_create(CTRTexture2D t)
        {
            CharAnimations charAnimations = new();
            _ = charAnimations.InitWithTexture(t);
            return charAnimations;
        }

        /// <summary>
        /// Adds a named child Om Nom character animation image to the container.
        /// </summary>
        /// <param name="resourceName">Texture resource name for the child animation.</param>
        public void AddImage(string resourceName)
        {
            animations ??= [];
            animationNameToIndex ??= [];

            CharAnimation charAnimation = CharAnimation.CharAnimation_createWithResID(resourceName);
            // Use the same anchor as the base animation (18) for proper centering
            charAnimation.parentAnchor = charAnimation.anchor = anchor;
            charAnimation.DoRestoreCutTransparency();

            int index = nextAnimationIndex++;
            animations.Add(charAnimation);
            animationNameToIndex[resourceName] = index;
            _ = AddChild(charAnimation);
            charAnimation.SetEnabled(false);
        }

        /// <inheritdoc />
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
                    animations.Clear();
                    animations = null;
                    nextAnimationIndex = 0;
                }
                animationNameToIndex?.Clear();
                animationNameToIndex = null;
            }
            base.Dispose(disposing);
        }

        /// <summary>
        /// Adds a frame animation to the base animation or a named child animation.
        /// </summary>
        /// <param name="resourceName">Texture resource name that identifies the target animation.</param>
        /// <param name="aid">Timeline ID to assign to the animation.</param>
        /// <param name="d">Delay between animation frames, in seconds.</param>
        /// <param name="l">Loop behavior for the timeline.</param>
        /// <param name="s">First frame index in the animation range.</param>
        /// <param name="e">Last frame index in the animation range.</param>
        public void AddAnimationWithIDDelayLoopFirstLast(string resourceName, int aid, float d, Timeline.LoopType l, int s, int e)
        {
            if (resourceName == Resources.Img.CharAnimations)
            {
                AddAnimationWithIDDelayLoopFirstLast(aid, d, l, s, e);
            }
            else if (animationNameToIndex != null && animationNameToIndex.TryGetValue(resourceName, out int index))
            {
                ((CharAnimation)animations[index]).AddAnimationWithIDDelayLoopFirstLast(aid, d, l, s, e);
            }
        }

        /// <summary>
        /// Gets the base animation or a named child animation by texture resource name.
        /// </summary>
        /// <param name="resourceName">Texture resource name that identifies the animation.</param>
        /// <returns>The matching animation, or <see langword="null"/> when no matching child animation exists.</returns>
        public Animation GetAnimation(string resourceName)
        {
            return resourceName == Resources.Img.CharAnimations
                ? this
                : animationNameToIndex != null && animationNameToIndex.TryGetValue(resourceName, out int index)
                ? animations[index]
                : null;
        }

        /// <summary>
        /// Adds an action keyframe that switches from one animation to another after a delay.
        /// </summary>
        /// <param name="resourceName2">Texture resource name for the destination animation.</param>
        /// <param name="a2">Timeline ID to play on the destination animation.</param>
        /// <param name="resourceName1">Texture resource name for the source animation.</param>
        /// <param name="a1">Timeline ID on the source animation that receives the switch action.</param>
        /// <param name="d">Delay before switching animations, in seconds.</param>
        public void SwitchToAnimationatEndOfAnimationDelay(string resourceName2, int a2, string resourceName1, int a1, float d)
        {
            Animation animation = GetAnimation(resourceName1);
            Animation animation2 = GetAnimation(resourceName2);
            Timeline timeline = animation.GetTimeline(a1);
            List<CTRAction> dynamicArray = [];
            // Check if resourceName1 refers to the base animation (CharAnimations)
            bool isBaseAnimation = resourceName1 == Resources.Img.CharAnimations;
            dynamicArray.Add(CTRAction.CreateAction(animation2, "ACTION_PLAY_TIMELINE", isBaseAnimation ? 1 : 0, a2));
            if (animation != animation2)
            {
                dynamicArray.Add(CTRAction.CreateAction(animation2, "ACTION_SET_UPDATEABLE", 1, 1));
                dynamicArray.Add(CTRAction.CreateAction(animation2, "ACTION_SET_VISIBLE", 1, 1));
                dynamicArray.Add(CTRAction.CreateAction(animation2, "ACTION_SET_TOUCHABLE", 1, 1));
                dynamicArray.Add(CTRAction.CreateAction(animation, "ACTION_SET_UPDATEABLE", 0, 0));
                dynamicArray.Add(CTRAction.CreateAction(animation, "ACTION_SET_VISIBLE", 0, 0));
                dynamicArray.Add(CTRAction.CreateAction(animation, "ACTION_SET_TOUCHABLE", 0, 0));
            }
            timeline.AddKeyFrame(KeyFrame.MakeAction(dynamicArray, d));
        }

        /// <summary>
        /// Plays a timeline on the base animation or a named child animation.
        /// </summary>
        /// <param name="resourceName">Texture resource name that identifies the animation to play.</param>
        /// <param name="t">Timeline ID to play.</param>
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

        /// <inheritdoc />
        public override void PlayTimeline(int t)
        {
            foreach (object obj in animations)
            {
                ((Animation)obj).SetEnabled(false);
            }
            color = RGBAColor.solidOpaqueRGBA;
            base.PlayTimeline(t);
        }

        /// <summary>
        /// Animation layers managed by this character animation container.
        /// </summary>
        private List<Animation> animations;

        /// <summary>
        /// Maps animation names to their layer indexes.
        /// </summary>
        private Dictionary<string, int> animationNameToIndex;

        /// <summary>
        /// Next available animation layer index.
        /// </summary>
        private int nextAnimationIndex;
    }
}
