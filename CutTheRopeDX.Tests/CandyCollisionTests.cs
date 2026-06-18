using CutTheRopeDX.GameMain;

using Xunit;

namespace CutTheRopeDX.Tests
{
    public class CandyCollisionTests
    {
        [Fact]
        public void ShouldParticipate_FalseWhenCandyIsInLantern()
        {
            Assert.False(CandyCollision.ShouldParticipate(noCandy: false, inBubble: false, inLantern: true));
        }

        [Fact]
        public void ShouldParticipate_TrueOnlyForFreeUneatenCandy()
        {
            Assert.True(CandyCollision.ShouldParticipate(noCandy: false, inBubble: false, inLantern: false));
            Assert.False(CandyCollision.ShouldParticipate(noCandy: true, inBubble: false, inLantern: false));
            Assert.False(CandyCollision.ShouldParticipate(noCandy: false, inBubble: true, inLantern: false));
        }

        [Fact]
        public void PairDistance_UsesAdditiveRadiiForNormalCandy()
        {
            CandyContext a = new() { collisionRadius = 32f };
            CandyContext b = new() { collisionRadius = 32f };

            Assert.Equal(64f, CandyCollision.PairDistance(a, b));
        }

        [Fact]
        public void PairDistance_UsesLargestAbsoluteOverride()
        {
            CandyContext candy = new() { collisionRadius = 32f };
            CandyContext bulb = new() { collisionDistanceOverride = 94.5f };

            Assert.Equal(94.5f, CandyCollision.PairDistance(candy, bulb));
            Assert.Equal(94.5f, CandyCollision.PairDistance(bulb, bulb));
        }
    }
}
