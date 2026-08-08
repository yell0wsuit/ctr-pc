using System.Linq;

using CutTheRopeDX.GameMain;
using CutTheRopeDX.Tests.Interactions;

using Xunit;

namespace CutTheRopeDX.Tests
{
    /// <summary>Off-screen light emitters use the same retirement invariant as edible candy.</summary>
    public sealed class LightEmitterRemovalTests
    {
        [Fact]
        public void OffScreenLightEmitterReleasesItsRopeAndRocketBeforeLightsOut()
        {
            GameScene scene = Scenario.New()
                .Candy(60, 200)
                .LightBulb(240, 200)
                .Grab(240, 120, length: 100, bindBulb: true, bulbNumber: "first")
                .Rocket(240, 200, impulse: 0f)
                .OmNom(20, 460)
                .Design("nightLevel", "true")
                .Build();
            CandyContext bulb = scene.Candies().Single(candy => candy.emitsLight);
            Interaction.Hover(bulb);
            _ = Act.BindRocket(scene, bulb);
            Assert.Equal(1, scene.AttachedRopeCount(bulb));

            Act.LoseOffScreen(scene, bulb);

            Assert.Equal(CandyRemovalReason.OffScreen, bulb.Lifecycle.RemovalReason);
            scene.AssertNoLiveAttachments(bulb);
            Assert.Equal(1, scene.Outcomes().LostCount);
        }

        [Fact]
        public void OffScreenLightEmitterSilentlyClearsItsBubbleBeforeLightsOut()
        {
            GameScene scene = Scenario.New()
                .Candy(60, 200)
                .LightBulb(240, 200)
                .Bubble(240, 200)
                .OmNom(20, 460)
                .Design("nightLevel", "true")
                .Build();
            CandyContext bulb = scene.Candies().Single(candy => candy.emitsLight);
            Interaction.Hover(bulb);
            _ = Act.CaptureInBubble(scene, bulb);
            int bubblePopEffects = scene.BubblePopEffectCount();

            Act.LoseOffScreen(scene, bulb);

            Assert.Equal(CandyRemovalReason.OffScreen, bulb.Lifecycle.RemovalReason);
            Assert.Null(bulb.WholeBody.Bubble);
            Assert.Equal(bubblePopEffects, scene.BubblePopEffectCount());
            Assert.Equal(1, scene.Outcomes().LostCount);
        }
    }
}
