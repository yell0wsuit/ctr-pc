using CutTheRopeDX.Framework.Physics;
using CutTheRopeDX.GameMain;

using Xunit;

namespace CutTheRopeDX.Tests
{
    public class SplitCandyStateTests
    {
        private static CandyBody Body(CandyBodyRole role)
        {
            return new CandyBody(new ConstraintedPoint(), role);
        }

        private static CandyLifecycle SplitLifecycle()
        {
            CandyBody whole = Body(CandyBodyRole.Whole);
            CandyHalf left = new(Body(CandyBodyRole.LeftHalf));
            CandyHalf right = new(Body(CandyBodyRole.RightHalf));
            return CandyLifecycle.CreateSplit(whole, new SplitCandyState(left, right));
        }

        [Theory]
        [InlineData((int)CandyRemovalReason.Hazard)]
        [InlineData((int)CandyRemovalReason.Spider)]
        [InlineData((int)CandyRemovalReason.OffScreen)]
        public void RemovingEitherHalfMarksFailedRemovalAndCancelsMerge(int reasonValue)
        {
            CandyRemovalReason reason = (CandyRemovalReason)reasonValue;
            CandyLifecycle lifecycle = SplitLifecycle();
            Assert.True(lifecycle.Split.TryBeginMerge(100f));

            Assert.True(lifecycle.Split.Left.TryRemove(reason));

            Assert.Equal(SplitPhase.Separate, lifecycle.Split.Phase);
            Assert.True(lifecycle.HasFailedRemoval);
            _ = Assert.Single(lifecycle.ActiveBodies);
            Assert.False(lifecycle.WasEaten);
        }

        [Fact]
        public void RemovingBothHalvesNeverCountsAsEatenOrCompletesMerge()
        {
            CandyLifecycle lifecycle = SplitLifecycle();
            _ = lifecycle.Split.Left.TryRemove(CandyRemovalReason.OffScreen);
            _ = lifecycle.Split.Right.TryRemove(CandyRemovalReason.OffScreen);

            Assert.Empty(lifecycle.ActiveBodies);
            Assert.True(lifecycle.HasFailedRemoval);
            Assert.False(lifecycle.WasEaten);
            Assert.False(lifecycle.TryCompleteMerge());
        }

        [Fact]
        public void SplitHalfCannotBeRemovedAsEaten()
        {
            CandyLifecycle lifecycle = SplitLifecycle();

            Assert.False(lifecycle.Split.Left.TryRemove(CandyRemovalReason.Eaten));
            Assert.Equal([lifecycle.Split.Left.Body, lifecycle.Split.Right.Body], lifecycle.ActiveBodies);
        }

        [Fact]
        public void IntactMergingHalvesAtomicallyRestoreWholeCandy()
        {
            CandyLifecycle lifecycle = SplitLifecycle();
            Assert.True(lifecycle.Split.TryBeginMerge(25f));

            Assert.True(lifecycle.TryCompleteMerge());

            Assert.Equal(CandyPresence.Present, lifecycle.Presence);
            Assert.Null(lifecycle.Split);
            Assert.Equal([lifecycle.WholeBody], lifecycle.ActiveBodies);
        }
    }
}
