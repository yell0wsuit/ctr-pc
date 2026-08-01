using CutTheRopeDX.GameMain;

using Xunit;

namespace CutTheRopeDX.Tests
{
    public class CandyLifecycleTests
    {
        [Fact]
        public void PresentCandy_CanBeRemovedAsEaten()
        {
            CandyLifecycle lifecycle = CandyLifecycle.CreatePresent();

            Assert.True(lifecycle.TryRemove(CandyRemovalReason.Eaten));
            Assert.Equal(CandyPresence.Removed, lifecycle.Presence);
            Assert.Equal(CandyRemovalReason.Eaten, lifecycle.RemovalReason);
            Assert.True(lifecycle.WasEaten);
            Assert.False(lifecycle.HasFailedRemoval);
        }

        [Theory]
        [InlineData((int)CandyRemovalReason.Hazard)]
        [InlineData((int)CandyRemovalReason.Spider)]
        [InlineData((int)CandyRemovalReason.OffScreen)]
        public void LossRemoval_NeverCountsAsEaten(int reasonValue)
        {
            CandyRemovalReason reason = (CandyRemovalReason)reasonValue;
            CandyLifecycle lifecycle = CandyLifecycle.CreatePresent();

            Assert.True(lifecycle.TryRemove(reason));
            Assert.False(lifecycle.WasEaten);
            Assert.True(lifecycle.HasFailedRemoval);
        }

        [Fact]
        public void RemovedCandy_IsTerminal()
        {
            CandyLifecycle lifecycle = CandyLifecycle.CreatePresent();
            Assert.True(lifecycle.TryRemove(CandyRemovalReason.Hazard));

            Assert.False(lifecycle.TryRemove(CandyRemovalReason.Eaten));
            Assert.Equal(CandyRemovalReason.Hazard, lifecycle.RemovalReason);
        }
    }
}
