using CutTheRopeDX.GameMain;

using Xunit;

namespace CutTheRopeDX.Tests
{
    /// <summary>
    /// When a mechanical hand grabs a candy, any OTHER hand already holding that SAME candy must let go.
    /// The decision is per candy: a hand holding a different candy keeps it, so multi-hand, multi-candy
    /// levels don't drop unrelated candies when one hand grabs.
    /// </summary>
    public class HandStealTests
    {
        [Fact]
        public void ShouldReleaseOtherHandTrueWhenAnotherHandHoldsThisCandy()
        {
            Assert.True(HandSteal.ShouldReleaseOtherHand(
                isDifferentHand: true, otherHandHoldingCandy: true, otherHandHoldsThisCandy: true));
        }

        [Fact]
        public void ShouldReleaseOtherHandFalseWhenTheOtherHandHoldsADifferentCandy()
        {
            // Multi-candy isolation: grabbing candy A must not release a hand holding candy B.
            Assert.False(HandSteal.ShouldReleaseOtherHand(
                isDifferentHand: true, otherHandHoldingCandy: true, otherHandHoldsThisCandy: false));
        }

        [Fact]
        public void ShouldReleaseOtherHandFalseForTheGrabbingHandItself()
        {
            Assert.False(HandSteal.ShouldReleaseOtherHand(
                isDifferentHand: false, otherHandHoldingCandy: true, otherHandHoldsThisCandy: true));
        }

        [Fact]
        public void ShouldReleaseOtherHandFalseWhenTheOtherHandHoldsNothing()
        {
            Assert.False(HandSteal.ShouldReleaseOtherHand(
                isDifferentHand: true, otherHandHoldingCandy: false, otherHandHoldsThisCandy: false));
        }
    }
}
