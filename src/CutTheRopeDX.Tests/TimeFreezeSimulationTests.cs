using CutTheRopeDX.Framework.Core;
using CutTheRopeDX.GameMain;
using CutTheRopeDX.Tests.Interactions;

using Xunit;

namespace CutTheRopeDX.Tests
{
    public class TimeFreezeSimulationTests
    {
        private static GameScene FrozenSceneWithFallingCandy()
        {
            Scenario scenario = Scenario.New().Candy(160, 100).OmNom(160, 400).PauseSwitcher(60, 400);
            GameScene scene = scenario.Build();
            HeadlessGame.StepFrames(scene, 5);
            Freeze(scene);
            return scene;
        }

        private static void Freeze(GameScene scene)
        {
            Vector button = scene.ScreenPositionOf(scene.PauseSwitchers()[0]);
            _ = scene.TouchDownXYIndex(button.X, button.Y, 0);
            _ = scene.TouchUpXYIndex(button.X, button.Y, 0);
        }

        [Fact]
        public void CandyDoesNotDriftWhileFrozen()
        {
            GameScene scene = FrozenSceneWithFallingCandy();
            Vector before = scene.Candy().WholeBody.Point.pos;

            HeadlessGame.StepFrames(scene, 120);

            Vector after = scene.Candy().WholeBody.Point.pos;
            Assert.Equal(before.X, after.X, 3);
            Assert.Equal(before.Y, after.Y, 3);
        }

        [Fact]
        public void CandyFallsAgainAfterUnfreezing()
        {
            GameScene scene = FrozenSceneWithFallingCandy();
            HeadlessGame.StepFrames(scene, 30);
            Vector frozenAt = scene.Candy().WholeBody.Point.pos;

            Freeze(scene);
            HeadlessGame.StepFrames(scene, 30);

            Assert.True(scene.Candy().WholeBody.Point.pos.Y > frozenAt.Y);
        }

        [Fact]
        public void MovingSpikesHoldStillWhileFrozen()
        {
            Scenario scenario = Scenario.New()
                .Candy(160, 100)
                .OmNom(160, 440)
                .MovingSpikes(160, 300)
                .PauseSwitcher(60, 440);
            GameScene scene = scenario.Build();
            Spikes moving = scene.SpikeStrips()[0];
            HeadlessGame.StepFrames(scene, 20);
            Assert.NotNull(moving.mover);
            Freeze(scene);
            float x = moving.x;
            float y = moving.y;

            HeadlessGame.StepFrames(scene, 120);

            Assert.Equal(x, moving.x, 3);
            Assert.Equal(y, moving.y, 3);
        }

        [Fact]
        public void OmNomDoesNotOpenHisMouthWhileFrozen()
        {
            GameScene scene = Scenario.New()
                .Candy(160, 100)
                .OmNom(160, 400)
                .PauseSwitcher(60, 440)
                .Build();
            TargetContext target = scene.Targets()[0];
            Freeze(scene);
            Interaction.PlaceCandyAt(
                scene.Candy(),
                new Vector(target.targetObject.x, target.targetObject.y - 100f));

            HeadlessGame.StepFrames(scene, 2);

            Assert.Equal(TargetFeedingPhase.Idle, target.Feeding.Phase);
        }

        [Fact]
        public void IdleRocketDoesNotBindCandyWhileFrozen()
        {
            GameScene scene = Scenario.New()
                .Candy(160, 200)
                .OmNom(160, 440)
                .Rocket(160, 200)
                .PauseSwitcher(60, 440)
                .Build();
            CandyContext candy = scene.Candy();
            Rocket rocket = scene.Rockets()[0];
            Interaction.Hover(candy);
            Interaction.PlaceCandyAt(candy, Interaction.At(rocket.x, rocket.y));
            Freeze(scene);

            HeadlessGame.StepFrames(scene, 2);

            Assert.False(candy.Lifecycle.Attachments.HasActiveRocket);
        }

        [Fact]
        public void MovingRocketContinuesItsAuthoredPathWhileFrozen()
        {
            GameScene scene = Scenario.New()
                .Candy(60, 100)
                .OmNom(160, 440)
                .Rocket(220, 200, path: "80,0", moveSpeed: 30f)
                .PauseSwitcher(60, 440)
                .Build();
            Rocket rocket = scene.Rockets()[0];
            Freeze(scene);
            Vector before = new(rocket.x, rocket.y);

            HeadlessGame.StepFrames(scene, 60);

            Assert.NotEqual(before, new Vector(rocket.x, rocket.y));
        }

        [Fact]
        public void FlyingRocketDoesNotConsumeFuelWhileFrozen()
        {
            GameScene scene = Scenario.New()
                .Candy(160, 200)
                .OmNom(160, 440)
                .Rocket(160, 200, time: 2f)
                .PauseSwitcher(60, 440)
                .Build();
            Rocket rocket = Act.BindRocket(scene, scene.Candy());
            Freeze(scene);
            float before = rocket.time;

            HeadlessGame.StepFrames(scene, 60);

            Assert.Equal(before, rocket.time);
        }
    }
}
