using CutTheRope.Desktop;
using CutTheRope.Framework;
using CutTheRope.Framework.Core;
using CutTheRope.Framework.Visual;

using Microsoft.Xna.Framework;

namespace CutTheRope.Commons
{
    /// <summary>
    /// Represents a modal popup dialog with animated show/hide effects and an optional scrollable content area.
    /// </summary>
    internal sealed class Popup : BaseElement, ITimelineDelegate
    {
        /// <summary>
        /// Initializes a popup with default show/hide timelines and a centered content root.
        /// </summary>
        public Popup()
        {
            ContentRoot = new BaseElement
            {
                width = (int)SCREEN_WIDTH,
                height = (int)SCREEN_HEIGHT,
                anchor = CENTER,
                parentAnchor = CENTER
            };

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

            _ = AddChild(ContentRoot);
        }

        public void TimelinereachedKeyFramewithIndex(Timeline t, KeyFrame k, int i)
        {
        }

        /// <summary>
        /// Called when a popup timeline finishes; removes the popup from its parent view.
        /// </summary>
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
            PlayTimeline(1);
        }

        /// <summary>
        /// Gets the root element that hosts popup content (background, text, buttons, etc.).
        /// </summary>
        public BaseElement ContentRoot { get; }

        /// <summary>
        /// Applies a uniform or non-uniform scale to the popup content root.
        /// </summary>
        /// <param name="sx">Horizontal scale factor.</param>
        /// <param name="sy">Vertical scale factor.</param>
        public void SetContentScale(float sx, float sy)
        {
            ContentRoot.scaleX = sx;
            ContentRoot.scaleY = sy;
        }

        /// <summary>
        /// Registers a scrollable container to receive mouse-wheel scrolling while the popup is shown.
        /// </summary>
        /// <param name="container">Scrollable container hosting long text or content.</param>
        public void RegisterScrollableContainer(ScrollableContainer container)
        {
            scrollContainer = container;
        }

        /// <summary>
        /// Forwards mouse wheel input to the registered scroll container, if present.
        /// </summary>
        /// <param name="scrollDelta">Mouse wheel delta.</param>
        /// <returns><c>true</c> if the popup consumed the scroll input; otherwise <c>false</c>.</returns>
        public bool HandleMouseWheel(int scrollDelta)
        {
            if (!isShow || scrollContainer == null)
            {
                return false;
            }

            scrollContainer.HandleMouseWheel(scrollDelta);
            return true;
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
            Renderer.Enable(Renderer.GL_BLEND);
            Renderer.Disable(Renderer.GL_TEXTURE_2D);
            Renderer.SetBlendFunc(BlendingFactor.GLONE, BlendingFactor.GLONEMINUSSRCALPHA);
            DrawHelper.DrawSolidRectWOBorder(0f, 0f, SCREEN_WIDTH, SCREEN_HEIGHT, RGBAColor.MakeRGBA(0.0, 0.0, 0.0, 0.5));
            Renderer.Enable(Renderer.GL_TEXTURE_2D);
            Renderer.SetColor(Color.White);
            PreDraw();
            PostDraw();
            Renderer.Disable(Renderer.GL_BLEND);
        }

        private bool isShow;
        private ScrollableContainer scrollContainer;

        private enum POPUP
        {
            SHOW_ANIM,
            HIDE_ANIM
        }
    }
}
