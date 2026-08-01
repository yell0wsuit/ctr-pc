using CutTheRopeDX.Framework.Physics;
using CutTheRopeDX.GameMain;

using Xunit;

namespace CutTheRopeDX.Tests
{
    public class CandyLifecycleTests
    {
        private static CandyBody Body(CandyBodyRole role)
        {
            return new CandyBody(new ConstraintedPoint(), role);
        }

        private static CandyLifecycle PresentLifecycle()
        {
            return CandyLifecycle.CreatePresent(Body(CandyBodyRole.Whole));
        }

        [Fact]
        public void PresentCandy_CanBeRemovedAsEaten()
        {
            CandyLifecycle lifecycle = PresentLifecycle();

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
            CandyLifecycle lifecycle = PresentLifecycle();

            Assert.True(lifecycle.TryRemove(reason));
            Assert.False(lifecycle.WasEaten);
            Assert.True(lifecycle.HasFailedRemoval);
        }

        [Fact]
        public void RemovedCandy_IsTerminal()
        {
            CandyLifecycle lifecycle = PresentLifecycle();
            Assert.True(lifecycle.TryRemove(CandyRemovalReason.Hazard));

            Assert.False(lifecycle.TryRemove(CandyRemovalReason.Eaten));
            Assert.Equal(CandyRemovalReason.Hazard, lifecycle.RemovalReason);
        }

        [Fact]
        public void Split_ExposesBothPresentHalvesInsteadOfWholeBody()
        {
            CandyBody whole = Body(CandyBodyRole.Whole);
            CandyHalf left = new(Body(CandyBodyRole.LeftHalf));
            CandyHalf right = new(Body(CandyBodyRole.RightHalf));
            CandyLifecycle lifecycle = CandyLifecycle.CreateSplit(whole, new SplitCandyState(left, right));

            Assert.Equal(CandyPresence.Split, lifecycle.Presence);
            Assert.Equal([left.Body, right.Body], lifecycle.ActiveBodies);
        }
    }
}
