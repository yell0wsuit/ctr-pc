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
    }
}
