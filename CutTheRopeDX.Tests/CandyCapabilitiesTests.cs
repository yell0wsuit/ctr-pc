using CutTheRopeDX.GameMain;

using Xunit;

namespace CutTheRopeDX.Tests
{
    public class CandyCapabilitiesTests
    {
        [Fact]
        public void Candy_DefaultCapabilitiesMatchCurrentCandyBehavior()
        {
            CandyCapabilities candy = CandyCapabilities.Candy;

            Assert.True(candy.CanCollectStars);
            Assert.True(candy.CanOpenMouth);
            Assert.True(candy.CanBeEaten);
            Assert.True(candy.CanLoseLevelWhenOffScreen);
            Assert.True(candy.CanBeGrabbedBySpider);
            Assert.True(candy.CanBeGrabbedByMouse);
            Assert.True(candy.CanBeGrabbedByHand);
            Assert.True(candy.CanEnterTransport);
        }

        [Fact]
        public void LightBulb_IsPhysicalButNotCandyConsumable()
        {
            CandyCapabilities bulb = CandyCapabilities.LightBulb;

            Assert.False(bulb.CanCollectStars);
            Assert.False(bulb.CanOpenMouth);
            Assert.False(bulb.CanBeEaten);
            Assert.False(bulb.CanLoseLevelWhenOffScreen);
            Assert.False(bulb.CanBeGrabbedBySpider);
            Assert.False(bulb.CanBeGrabbedByMouse);
            Assert.False(bulb.CanBeGrabbedByHand);
            Assert.True(bulb.CanEnterTransport);
        }
    }
}
