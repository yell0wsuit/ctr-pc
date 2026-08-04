using CutTheRopeDX.GameMain;

using Xunit;

namespace CutTheRopeDX.Tests
{
    public sealed class BubbleVisualStateTests
    {
        [Fact]
        public void ForCaptureShowsGhostVisualWhenGhostBubbleAndAnimationExists()
        {
            BubbleVisualState state = BubbleVisualState.ForCapture(capturedGhostBubble: true, hasGhostAnimation: true);

            Assert.False(state.ShowNormalBubble);
            Assert.True(state.ShowGhostBubble);
        }

        [Fact]
        public void ForCaptureFallsBackToNormalVisualWhenGhostAnimationMissing()
        {
            BubbleVisualState state = BubbleVisualState.ForCapture(capturedGhostBubble: true, hasGhostAnimation: false);

            Assert.True(state.ShowNormalBubble);
            Assert.False(state.ShowGhostBubble);
        }

        [Fact]
        public void ForCaptureShowsNormalVisualForNormalBubble()
        {
            BubbleVisualState state = BubbleVisualState.ForCapture(capturedGhostBubble: false, hasGhostAnimation: true);

            Assert.True(state.ShowNormalBubble);
            Assert.False(state.ShowGhostBubble);
        }
    }
}
