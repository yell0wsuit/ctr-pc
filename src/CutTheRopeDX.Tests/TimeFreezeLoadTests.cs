using CutTheRopeDX.GameMain;
using CutTheRopeDX.Tests.Interactions;

using Xunit;

namespace CutTheRopeDX.Tests
{
    public class TimeFreezeLoadTests
    {
        [Fact]
        public void PauseSwitcherElementLoadsOneButtonAtTheAuthoredPosition()
        {
            Scenario scenario = Scenario.New().Candy(160, 100).OmNom(160, 400).PauseSwitcher(159, 368);
            GameScene scene = scenario.Build();

            _ = Assert.Single(scene.PauseSwitchers());
            Assert.Equal(scenario.WorldX(159), scene.PauseSwitchers()[0].x, 1);
            Assert.Equal(Scenario.WorldY(368), scene.PauseSwitchers()[0].y, 1);
        }

        [Fact]
        public void LevelWithoutAPauseSwitcherStartsUnfrozenAndHasNoButton()
        {
            GameScene scene = Scenario.New().Candy(160, 100).OmNom(160, 400).Build();

            Assert.Empty(scene.PauseSwitchers());
            Assert.False(scene.IsTimeFrozen());
        }
    }
}
