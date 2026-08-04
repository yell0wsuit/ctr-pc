using CutTheRopeDX.GameMain;

using Xunit;

namespace CutTheRopeDX.Tests
{
    public class SnailWeightTests
    {
        [Fact]
        public void AfterForceDetachRestoresBaseWeightWhenSingleSnailRemoved()
        {
            // Base candy (weight 1) + one snail (+3) -> back to 1 once the snail is force-detached.
            Assert.Equal(1f, SnailWeight.AfterForceDetach(4f, 1));
        }

        [Fact]
        public void AfterForceDetachRemovesEachSnailsContribution()
        {
            // Two snails stacked (+6) come off together.
            Assert.Equal(1f, SnailWeight.AfterForceDetach(7f, 2));
        }

        [Fact]
        public void AfterForceDetachPreservesHeavierBaseWeight()
        {
            // Rocket candy (base 2.5) + one snail (+3 = 5.5) must return to 2.5, not a flat minimum.
            Assert.Equal(2.5f, SnailWeight.AfterForceDetach(5.5f, 1));
        }

        [Fact]
        public void AfterForceDetachNeverDropsBelowMinimum()
        {
            Assert.Equal(SnailWeight.MinWeight, SnailWeight.AfterForceDetach(1f, 1));
        }
    }
}
