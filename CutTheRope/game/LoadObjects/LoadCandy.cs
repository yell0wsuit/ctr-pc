using CutTheRope.iframework.visual;

namespace CutTheRope.game
{
    /// <summary>
    /// Handles loading split candy bubble animations
    /// </summary>
    internal sealed partial class GameScene
    {
        /// <summary>
        /// Initializes split candy bubble animations
        /// Called when level has split candy (left and right variants)
        /// </summary>
        private void LoadCandyBubbleAnimations()
        {
            if (twoParts != 2)
            {
                // Setup left candy bubble animation
                candyBubbleAnimationL = Animation.Animation_createWithResID(72);
                candyBubbleAnimationL.parentAnchor = candyBubbleAnimationL.anchor = 18;
                _ = candyBubbleAnimationL.AddAnimationDelayLoopFirstLast(0.05, Timeline.LoopType.TIMELINE_REPLAY, 0, 12);
                candyBubbleAnimationL.PlayTimeline(0);
                _ = candyL.AddChild(candyBubbleAnimationL);
                candyBubbleAnimationL.visible = false;

                // Setup right candy bubble animation
                candyBubbleAnimationR = Animation.Animation_createWithResID(72);
                candyBubbleAnimationR.parentAnchor = candyBubbleAnimationR.anchor = 18;
                _ = candyBubbleAnimationR.AddAnimationDelayLoopFirstLast(0.05, Timeline.LoopType.TIMELINE_REPLAY, 0, 12);
                candyBubbleAnimationR.PlayTimeline(0);
                _ = candyR.AddChild(candyBubbleAnimationR);
                candyBubbleAnimationR.visible = false;
            }
        }
    }
}
