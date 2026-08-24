using CutTheRopeDX.GameMain;
using CutTheRopeDX.Tests.Interactions;

using Xunit;

namespace CutTheRopeDX.Tests
{
    public class TimeFreezeWavesTests
    {
        [Fact]
        public void EachBurstSpawnsTwentySixWaves()
        {
            GameScene scene = Scenario.New()
                .Candy(160, 100)
                .OmNom(160, 400)
                .PauseSwitcher(60, 440)
                .Build();
            PauseSwitcherWaves waves = scene.WavesOverlay();

            waves.Update(0.9f);

            Assert.Equal(26, waves.ActiveWaveCount);
        }

        [Fact]
        public void BurstsRespawnOnTheEightHundredMillisecondCadence()
        {
            GameScene scene = Scenario.New()
                .Candy(160, 100)
                .OmNom(160, 400)
                .PauseSwitcher(60, 440)
                .Build();
            PauseSwitcherWaves waves = scene.WavesOverlay();

            waves.Update(0.5f);
            Assert.Equal(0, waves.ActiveWaveCount);

            waves.Update(0.4f);
            Assert.Equal(26, waves.ActiveWaveCount);
        }
    }
}
