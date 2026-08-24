using System.Collections.Generic;
using System.Linq;

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
            BaseElement innerPlate = plate.GetChild(0);

            Assert.Equal(800f, (plate.width - 6f) * plate.scaleX, 3);
            Assert.Equal(600f, (plate.height - 6f) * plate.scaleY, 3);
            Assert.Equal(800f, (innerPlate.width - 6f) * innerPlate.scaleX * plate.scaleX, 3);
            Assert.Equal(600f, (innerPlate.height - 6f) * innerPlate.scaleY * plate.scaleY, 3);

            waves.Resize(1000f, 500f);

            Assert.Equal(1000f, (plate.width - 6f) * plate.scaleX, 3);
            Assert.Equal(500f, (plate.height - 6f) * plate.scaleY, 3);
            Assert.Equal(1000f, (innerPlate.width - 6f) * innerPlate.scaleX * plate.scaleX, 3);
            Assert.Equal(500f, (innerPlate.height - 6f) * innerPlate.scaleY * plate.scaleY, 3);
        }

        [Fact]
        public void WaveBurstsUseTheIosRotationForEachScreenEdge()
        {
            _ = HeadlessGame.Boot();
            PauseSwitcherWaves waves = PauseSwitcherWaves.Create(800f, 600f);

            waves.Update(0.9f);

            BaseElement pool = waves.GetChild(1);
            BaseElement[] effects = [.. pool.GetChilds().Values];
            Assert.All(effects, effect =>
            {
                Assert.Equal(18, effect.anchor);
                Assert.Equal(9, effect.parentAnchor);
            });
            Assert.Equal(5, effects.Count(effect => effect.y == 600f && effect.rotation == 0f));
            Assert.Equal(5, effects.Count(effect => effect.y == 0f && effect.rotation == 180f));
            Assert.Equal(8, effects.Count(effect => effect.x == 0f && effect.rotation == 90f));
            Assert.Equal(8, effects.Count(effect => effect.x == 800f && effect.rotation == -90f));
        }

        [Fact]
        public void PauseOverlayUsesTheIosAdditiveBlendMode()
        {
            _ = HeadlessGame.Boot();

            PauseSwitcherWaves waves = PauseSwitcherWaves.Create(800f, 600f);

            Assert.Equal(2, waves.blendingMode);
        }

        [Fact]
        public void LevelLabelLayerDrawsAboveThePauseOverlay()
        {
            GameScene scene = Scenario.New()
                .Candy(160, 100)
                .OmNom(160, 400)
                .PauseSwitcher(60, 440)
                .Build();
            List<string> drawOrder = [];
            _ = scene.WavesOverlay().AddChild(new OrderedElement("overlay", drawOrder));
            _ = scene.StaticAnimations().AddChild(new OrderedElement("level label", drawOrder));
            scene.WavesOverlay().PlayFadeIn();
            PlatformServices.Render = new RecordingRenderBackend();

            try
            {
                scene.Draw();

                Assert.True(
                    drawOrder.IndexOf("overlay") < drawOrder.IndexOf("level label"),
                    $"Actual draw order: {string.Join(", ", drawOrder)}");
            }
            finally
            {
                PlatformServices.Render = new ThrowingRenderBackend();
            }
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

        private sealed class OrderedElement(string name, List<string> drawOrder) : BaseElement
        {
            public override void Draw()
            {
                drawOrder.Add(name);
            }
        }
    }
}
