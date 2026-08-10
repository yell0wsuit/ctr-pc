using CutTheRopeDX.Framework.Visual;

namespace CutTheRopeDX.GameMain
{
    /// <summary>
    /// Builds the shared bubble-capture overlay animations (the generic <c>ObjBubble</c> sprite)
    /// used by every body a bubble can carry — candies, light bulbs, and the axe. Callers add the
    /// returned animation as a child of the captured body and position it.
    /// </summary>
    internal static class BubbleAnimationFactory
    {
        /// <summary>Per-frame delay of the looping bubble animation.</summary>
        private const float FrameDelaySeconds = 0.05f;

        /// <summary>First atlas frame of the bubble loop.</summary>
        private const int FirstFrame = 4;

        /// <summary>Last atlas frame of the bubble loop.</summary>
        private const int LastFrame = 16;

        /// <summary>Center anchor (and parent anchor) used by all bubble overlays.</summary>
        private const int CenterAnchor = 18;

        /// <summary>
        /// Creates a hidden, looping normal-bubble overlay animation.
        /// </summary>
        /// <returns>The configured bubble animation (not yet added to a parent).</returns>
        public static Animation CreateBubble()
        {
            Animation bubble = Animation.Animation_createWithResID(Resources.Img.ObjBubble);
            bubble.anchor = bubble.parentAnchor = CenterAnchor;
            _ = bubble.AddAnimationDelayLoopFirstLast(FrameDelaySeconds, Timeline.LoopType.TIMELINE_REPLAY, FirstFrame, LastFrame);
            bubble.PlayTimeline(0);
            bubble.visible = false;
            return bubble;
        }

        /// <summary>
        /// Creates a hidden, looping ghost-bubble overlay animation, including its supporting clouds.
        /// </summary>
        /// <returns>The configured ghost-bubble animation (not yet added to a parent).</returns>
        public static CandyInGhostBubbleAnimation CreateGhostBubble()
        {
            CandyInGhostBubbleAnimation ghostBubble = CandyInGhostBubbleAnimation.CIGBAnimation_createWithResID(Resources.Img.ObjBubble);
            ghostBubble.anchor = ghostBubble.parentAnchor = CenterAnchor;
            ghostBubble.visible = false;
            ghostBubble.AddSupportingCloudsTimelines();
            _ = ghostBubble.AddAnimationDelayLoopFirstLast(FrameDelaySeconds, Timeline.LoopType.TIMELINE_REPLAY, FirstFrame, LastFrame);
            ghostBubble.PlayTimeline(0);
            return ghostBubble;
        }
    }
}
