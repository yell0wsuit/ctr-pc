using CutTheRopeDX.Framework.Platform;
using CutTheRopeDX.Framework.Visual;
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

        [Fact]
        public void PausePlateScalesIndependentlyToCoverTheWholeOverlayAndResize()
        {
            _ = HeadlessGame.Boot();
            PauseSwitcherWaves waves = PauseSwitcherWaves.Create(800f, 600f);
            BaseElement plate = waves.GetChild(0);

            Assert.Equal(800f / plate.width, plate.scaleX, 3);
            Assert.Equal(600f / plate.height, plate.scaleY, 3);

            waves.Resize(1000f, 500f);

            Assert.Equal(1000f / plate.width, plate.scaleX, 3);
            Assert.Equal(500f / plate.height, plate.scaleY, 3);
        }

        [Fact]
        public void AdditivePauseOverlayDoesNotLeakBlendStatePastTheGameScene()
        {
            GameScene scene = Scenario.New()
                .Candy(160, 100)
                .OmNom(160, 400)
                .PauseSwitcher(60, 440)
                .Build();
            scene.WavesOverlay().PlayFadeIn();
            RecordingRenderBackend renderer = new();
            PlatformServices.Render = renderer;

            try
            {
                scene.Draw();

                Assert.Equal(BlendingFactor.GLONE, renderer.LastBlendSource);
                Assert.Equal(BlendingFactor.GLONEMINUSSRCALPHA, renderer.LastBlendDestination);
            }
            finally
            {
                PlatformServices.Render = new ThrowingRenderBackend();
            }
        }

        [Fact]
        public void GameplayParticlePoolDrawsEvenWhenItsVisibilityFlagIsFalse()
        {
            GameScene scene = Scenario.New()
                .Candy(160, 100)
                .OmNom(160, 400)
                .PauseSwitcher(60, 440)
                .Build();
            CountingElement particle = new();
            _ = scene.ParticleAnimations().AddChild(particle);
            Assert.False(scene.ParticleAnimations().visible);
            PlatformServices.Render = new RecordingRenderBackend();

            try
            {
                scene.Draw();

                Assert.Equal(1, particle.DrawCount);
            }
            finally
            {
                PlatformServices.Render = new ThrowingRenderBackend();
            }
        }

        private sealed class CountingElement : BaseElement
        {
            public int DrawCount { get; private set; }

            public override void Draw()
            {
                DrawCount++;
            }
        }
    }
}
