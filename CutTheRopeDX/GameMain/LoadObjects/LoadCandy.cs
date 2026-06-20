namespace CutTheRopeDX.GameMain
{
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
                candyBubbleAnimationL = BubbleAnimationFactory.CreateBubble();
                _ = candyL.AddChild(candyBubbleAnimationL);

                // Setup right candy bubble animation
                candyBubbleAnimationR = BubbleAnimationFactory.CreateBubble();
                _ = candyR.AddChild(candyBubbleAnimationR);
            }
        }
    }
}
