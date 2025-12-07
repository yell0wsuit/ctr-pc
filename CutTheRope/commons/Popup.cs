using System.Collections.Generic;

using CutTheRope.Desktop;
using CutTheRope.Framework;
using CutTheRope.Framework.Core;
using CutTheRope.Framework.Visual;

using Microsoft.Xna.Framework;

namespace CutTheRope.Commons
{
    /// <summary>
    /// Represents a modal popup dialog with animated show/hide effects and text fade animations.
    /// </summary>
    internal sealed class Popup : BaseElement, ITimelineDelegate
    {
        public Popup()
        {
            // Timeline 0: Show animation - bounce effect (scale 0 → 1.1 → 0.9 → 1.0)
            Timeline timeline = new Timeline().InitWithMaxKeyFramesOnTrack(4);
            timeline.AddKeyFrame(KeyFrame.MakeScale(0.0, 0.0, KeyFrame.TransitionType.FRAME_TRANSITION_LINEAR, 0.0));
            timeline.AddKeyFrame(KeyFrame.MakeScale(1.1, 1.1, KeyFrame.TransitionType.FRAME_TRANSITION_LINEAR, 0.3));
            timeline.AddKeyFrame(KeyFrame.MakeScale(0.9, 0.9, KeyFrame.TransitionType.FRAME_TRANSITION_LINEAR, 0.1));
            timeline.AddKeyFrame(KeyFrame.MakeScale(1.0, 1.0, KeyFrame.TransitionType.FRAME_TRANSITION_LINEAR, 0.2));
            _ = AddTimeline(timeline);
            // Timeline 1: Hide animation - shrink to zero (scale 1.0 → 0.0)
            timeline = new Timeline().InitWithMaxKeyFramesOnTrack(2);
            timeline.AddKeyFrame(KeyFrame.MakeScale(1.0, 1.0, KeyFrame.TransitionType.FRAME_TRANSITION_LINEAR, 0.0));
            timeline.AddKeyFrame(KeyFrame.MakeScale(0.0, 0.0, KeyFrame.TransitionType.FRAME_TRANSITION_LINEAR, 0.3));
            width = (int)SCREEN_WIDTH;
            height = (int)SCREEN_HEIGHT;
            _ = AddTimeline(timeline);
            timeline.delegateTimelineDelegate = this;
        }

        public void TimelinereachedKeyFramewithIndex(Timeline t, KeyFrame k, int i)
        {
        }

        public void TimelineFinished(Timeline t)
        {
            View view = (View)parent;
            view?.RemoveChild(this);
        }

        /// <summary>
        /// Shows the popup with a bounce animation. Text elements will fade in after the popup appears.
        /// </summary>
        public void ShowPopup()
        {
            Application.SharedRootController().DeactivateAllButtons();
            isShow = true;
            PlayTimeline(0); // Play show animation
        }

        /// <summary>
        /// Hides the popup. Text elements fade out first, then the popup shrinks away.
        /// </summary>
        public void HidePopup()
        {
            isShow = false;
            FadeOutTextChildren(); // Fade out all text elements before popup closes
            PlayTimeline(1); // Play hide animation
        }

        /// <summary>
        /// Initiates fade-out animation for all text elements within the popup.
        /// </summary>
        private void FadeOutTextChildren()
        {
            // Recursively find and fade out all text elements
            FadeOutTextInElement(this);
        }

        /// <summary>
        /// Recursively fades out text elements within the given element and its children.
        /// </summary>
        /// <param name="element">The element to search for text elements.</param>
        private static void FadeOutTextInElement(BaseElement element)
        {
            if (element == null)
            {
                return;
            }

            // Check if this element is a Text element and fade it out
            if (element is Text textElement)
            {
                // Create fade-out timeline: opaque → transparent over 0.2s
                Timeline fadeOutTimeline = new Timeline().InitWithMaxKeyFramesOnTrack(2);
                fadeOutTimeline.AddKeyFrame(KeyFrame.MakeColor(RGBAColor.solidOpaqueRGBA, KeyFrame.TransitionType.FRAME_TRANSITION_LINEAR, 0.0));
                fadeOutTimeline.AddKeyFrame(KeyFrame.MakeColor(RGBAColor.transparentRGBA, KeyFrame.TransitionType.FRAME_TRANSITION_LINEAR, 0.2));
                textElement.AddTimelinewithID(fadeOutTimeline, 1);
                textElement.PlayTimeline(1);
            }

            // Recursively process all children to find nested text elements
            Dictionary<int, BaseElement> children = element.GetChilds();
            for (int i = 0; i < children.Count; i++)
            {
                BaseElement child = children[i];
                FadeOutTextInElement(child);
            }
        }

        public override bool OnTouchDownXY(float tx, float ty)
        {
            if (isShow)
            {
                _ = base.OnTouchDownXY(tx, ty);
            }
            return true;
        }

        public override bool OnTouchUpXY(float tx, float ty)
        {
            if (isShow)
            {
                _ = base.OnTouchUpXY(tx, ty);
            }
            return true;
        }

        public override bool OnTouchMoveXY(float tx, float ty)
        {
            if (isShow)
            {
                _ = base.OnTouchMoveXY(tx, ty);
            }
            return true;
        }

        public override void Draw()
        {
            OpenGL.GlEnable(1);
            OpenGL.GlDisable(0);
            OpenGL.GlBlendFunc(BlendingFactor.GLONE, BlendingFactor.GLONEMINUSSRCALPHA);
            GLDrawer.DrawSolidRectWOBorder(0f, 0f, SCREEN_WIDTH, SCREEN_HEIGHT, RGBAColor.MakeRGBA(0.0, 0.0, 0.0, 0.5));
            OpenGL.GlEnable(0);
            OpenGL.GlColor4f(Color.White);
            PreDraw();
            PostDraw();
            OpenGL.GlDisable(1);
        }

        private bool isShow;

        private enum POPUP
        {
            SHOW_ANIM,
            HIDE_ANIM
        }
    }
}
