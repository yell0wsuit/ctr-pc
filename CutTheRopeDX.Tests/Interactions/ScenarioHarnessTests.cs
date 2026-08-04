using CutTheRopeDX.GameMain;

using Xunit;

namespace CutTheRopeDX.Tests.Interactions
{
    /// <summary>
    /// Guards the scenario harness itself. If these fail, every matrix test above them is
    /// asserting against a scene that was never wired the way the test described.
    /// </summary>
    public sealed class ScenarioHarnessTests
    {
        [Fact]
        public void CodeBuiltScenarioLoadsCandyRopeAndOmNom()
        {
            GameScene scene = Scenario.New()
                .Candy(160, 200)
                .Rope(160, 120, length: 40)
                .OmNom(160, 420)
                .Build();

            CandyContext candy = scene.Candy();
            _ = Assert.Single(scene.Grabs());
            Assert.Equal(1, scene.AttachedRopeCount(candy));
            Assert.False(candy.HasNoWholeBodyInPlay);
        }

        [Fact]
        public void StepFramesRunsTheRealUpdateLoop()
        {
            GameScene scene = Scenario.New()
                .Candy(160, 200)
                .Rope(160, 120, length: 40)
                .OmNom(160, 420)
                .Build();

            CandyContext candy = scene.Candy();
            float startY = candy.WholeBody.Point.pos.Y;
            HeadlessGame.StepFrames(scene, 30);

            // A roped candy swings; the point must actually have been integrated.
            Assert.NotEqual(startY, candy.WholeBody.Point.pos.Y);
        }

        [Fact]
        public void ObjectsAreBuiltFromTheScenarioNotFromAShippingLevel()
        {
            GameScene scene = Scenario.New()
                .Candy(160, 200)
                .OmNom(160, 420)
                .Rocket(160, 200)
                .Bubble(60, 200)
                .Snail(260, 200)
                .Hand(60, 60, segmentLength: 20, segmentAngle: 90f)
                .Build();

            _ = Assert.Single(scene.Rockets());
            _ = Assert.Single(scene.Bubbles());
            _ = Assert.Single(scene.Snails());
            _ = Assert.Single(scene.Hands());
            Assert.Empty(scene.Grabs());
        }
    }
}
