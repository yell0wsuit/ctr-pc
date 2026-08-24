using CutTheRopeDX.GameMain;
using CutTheRopeDX.Tests.Interactions;
using CutTheRopeDX.Framework.Core;

using Xunit;

namespace CutTheRopeDX.Tests
{
    public class TimeFreezeInputTests
    {
        private static GameScene SceneWithSwitcher(out float buttonX, out float buttonY)
        {
            Scenario scenario = Scenario.New().Candy(160, 100).OmNom(160, 400).PauseSwitcher(159, 368);
            GameScene scene = scenario.Build();
            PauseSwitcher switcher = scene.PauseSwitchers()[0];
            Vector screenPosition = scene.ScreenPositionOf(switcher);
            buttonX = screenPosition.X;
            buttonY = screenPosition.Y;
            return scene;
        }

        [Fact]
        public void PressAndReleaseOnTheButtonFreezesThenUnfreezes()
        {
            GameScene scene = SceneWithSwitcher(out float bx, out float by);

            _ = scene.TouchDownXYIndex(bx, by, 0);
            _ = scene.TouchUpXYIndex(bx, by, 0);
            Assert.True(scene.IsTimeFrozen());

            _ = scene.TouchDownXYIndex(bx, by, 0);
            _ = scene.TouchUpXYIndex(bx, by, 0);
            Assert.False(scene.IsTimeFrozen());
        }

        [Fact]
        public void ReleasingAwayFromTheButtonDoesNotToggle()
        {
            GameScene scene = SceneWithSwitcher(out float bx, out float by);

            _ = scene.TouchDownXYIndex(bx, by, 0);
            _ = scene.TouchUpXYIndex(bx + 600f, by + 600f, 0);

            Assert.False(scene.IsTimeFrozen());
        }

        [Fact]
        public void ReleasingOnTheButtonWithoutPressingItDoesNotToggle()
        {
            GameScene scene = SceneWithSwitcher(out float bx, out float by);

            _ = scene.TouchDownXYIndex(bx + 600f, by + 600f, 0);
            _ = scene.TouchUpXYIndex(bx, by, 0);

            Assert.False(scene.IsTimeFrozen());
        }

        [Fact]
        public void ADifferentPointerReleasingOnTheButtonDoesNotToggle()
        {
            GameScene scene = SceneWithSwitcher(out float bx, out float by);

            _ = scene.TouchDownXYIndex(bx, by, 0);
            _ = scene.TouchUpXYIndex(bx, by, 1);

            Assert.False(scene.IsTimeFrozen());

            _ = scene.TouchUpXYIndex(bx, by, 0);
            Assert.False(scene.IsTimeFrozen());
        }

        [Fact]
        public void ReleasingOnADifferentSwitcherDoesNotToggle()
        {
            GameScene scene = Scenario.New()
                .Candy(160, 100)
                .OmNom(160, 400)
                .PauseSwitcher(80, 368)
                .PauseSwitcher(240, 368)
                .Build();
            PauseSwitcher pressed = scene.PauseSwitchers()[0];
            PauseSwitcher released = scene.PauseSwitchers()[1];
            Vector pressedAt = scene.ScreenPositionOf(pressed);
            Vector releasedAt = scene.ScreenPositionOf(released);

            _ = scene.TouchDownXYIndex(pressedAt.X, pressedAt.Y, 0);
            _ = scene.TouchUpXYIndex(releasedAt.X, releasedAt.Y, 0);

            Assert.False(scene.IsTimeFrozen());
        }

        [Fact]
        public void OutcomeReleaseClearsTheCapturedSwitcherBeforeReturningEarly()
        {
            GameScene scene = SceneWithSwitcher(out float bx, out float by);

            _ = scene.TouchDownXYIndex(bx, by, 0);
            Assert.True(scene.gameplayFlow.TryBeginWin());
            _ = scene.TouchUpXYIndex(bx, by, 0);
            scene.gameplayFlow.ResetOutcome();

            _ = scene.TouchUpXYIndex(bx, by, 0);

            Assert.False(scene.IsTimeFrozen());
        }

        [Fact]
        public void SceneUpdateAdvancesThePauseSwitcherBurstAnimation()
        {
            GameScene scene = SceneWithSwitcher(out float bx, out float by);
            PauseSwitcher switcher = scene.PauseSwitchers()[0];
            _ = scene.TouchDownXYIndex(bx, by, 0);
            _ = scene.TouchUpXYIndex(bx, by, 0);
            float before = switcher.GetChild(0).GetCurrentTimeline().time;

            HeadlessGame.StepFrames(scene, 4);

            Assert.NotEqual(before, switcher.GetChild(0).GetCurrentTimeline().time);
        }
    }
}
