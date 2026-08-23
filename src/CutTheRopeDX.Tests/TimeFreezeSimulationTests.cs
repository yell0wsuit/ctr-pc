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
    }
}
