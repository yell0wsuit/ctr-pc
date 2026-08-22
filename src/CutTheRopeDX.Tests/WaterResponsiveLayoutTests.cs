using System;
using System.Reflection;

using CutTheRopeDX.Commons;
using CutTheRopeDX.Framework;
using CutTheRopeDX.Framework.Core;
using CutTheRopeDX.Framework.Helpers;
using CutTheRopeDX.Framework.Platform;
using CutTheRopeDX.GameMain;
using CutTheRopeDX.Tests.Interactions;

using Xunit;

namespace CutTheRopeDX.Tests
{
    /// <summary>Responsive coverage for the world-space water layer.</summary>
    public sealed class WaterResponsiveLayoutTests
    {
        [Fact]
        public void WaterExtendsAcrossTheVisibleWorldAfterNonSixteenNineResizes()
        {
            LayoutSurfaces.WithSurface(2560, 1440, () =>
            {
                GameScene scene = Scenario.New()
                    .MapSize(320, 480)
                    .Design("water", "160")
                    .Candy(160, 120)
                    .OmNom(160, 400)
                    .Build();

                WaterElement water = Read<WaterElement>(scene, "waterLayer");
                float authoredWaterline = water.y;

                AssertWaterCoversVisibleWorld(scene, water, 2560, 1080, authoredWaterline);
                AssertWaterCoversVisibleWorld(scene, water, 1000, 1000, authoredWaterline);
            });
        }

        [Fact]
        public void BubbleClipTracksTheCameraScaledWaterSurfaceAfterPortraitResize()
        {
            LayoutSurfaces.WithSurface(2560, 1440, () =>
            {
                GameScene scene = BuildWaterScene();
                WaterElement water = Read<WaterElement>(scene, "waterLayer");

                CtrRenderer.OnSurfaceChanged(640, 1600);
                scene.RelayoutCamera();

                Camera2D camera = Read<Camera2D>(scene, "camera");
                ScissorElement clip = Read<ScissorElement>(water, "scissorElement");
                Vector topTileSize = Read<Vector>(water, "topTileSize");

                Assert.Equal((water.x - camera.RenderPos.X) * camera.Scale, clip.x, 0.01);
                Assert.Equal(
                    (water.y + topTileSize.Y - camera.RenderPos.Y) * camera.Scale,
                    clip.y,
                    0.01);
                Assert.Equal(MathF.Ceiling(water.width * camera.Scale), clip.width);
                Assert.Equal(MathF.Ceiling(water.height * camera.Scale), clip.height);
            });
        }

        [Fact]
        public void ClickBubblesOnlySpawnInsideTheResponsiveWaterBounds()
        {
            LayoutSurfaces.WithSurface(2560, 1440, () =>
            {
                GameScene scene = BuildWaterScene();
                WaterElement water = Read<WaterElement>(scene, "waterLayer");

                CtrRenderer.OnSurfaceChanged(640, 1600);
                scene.RelayoutCamera();

                WaterBubbles bubbles = Read<WaterBubbles>(water, "bubbles");
                int initialParticles = bubbles.particleCount;
                float middleX = water.x + (water.width / 2f);

                water.AddParticlesAtXY(middleX, water.y - 1f);
                Assert.Equal(initialParticles, bubbles.particleCount);

                water.AddParticlesAtXY(middleX, water.y + water.height - 1f);
                Assert.Equal(initialParticles + 3, bubbles.particleCount);

                water.AddParticlesAtXY(water.x - 1f, water.y + 1f);
                Assert.Equal(initialParticles + 3, bubbles.particleCount);
            });
        }

        private static GameScene BuildWaterScene()
        {
            return Scenario.New()
                .MapSize(320, 480)
                .Design("water", "160")
                .Candy(160, 120)
                .OmNom(160, 400)
                .Build();
        }

        private static void AssertWaterCoversVisibleWorld(
            GameScene scene,
            WaterElement water,
            int width,
            int height,
            float authoredWaterline)
        {
            CtrRenderer.OnSurfaceChanged(width, height);
            scene.RelayoutCamera();

            Camera2D camera = Read<Camera2D>(scene, "camera");
            CTRRectangle viewport = ScreenPresentation.Instance.Snapshot.VisibleBounds;
            float visibleLeft = camera.RenderPos.X;
            float visibleRight = visibleLeft + (viewport.w / camera.Scale);
            float visibleBottom = camera.RenderPos.Y + (viewport.h / camera.Scale);

            Assert.True(
                water.x <= visibleLeft && water.x + water.width >= visibleRight,
                $"{width}x{height}: water spans x [{water.x}, {water.x + water.width}], "
                    + $"visible world needs [{visibleLeft}, {visibleRight}]");
            Assert.True(
                water.y + water.height >= visibleBottom,
                $"{width}x{height}: water ends at {water.y + water.height}, visible world ends "
                    + $"at {visibleBottom}");
            Assert.Equal(authoredWaterline, water.y, 0.01);
        }

        private static T Read<T>(object target, string field)
        {
            object value = target.GetType()
                .GetField(field, BindingFlags.Instance | BindingFlags.NonPublic)
                ?.GetValue(target);
            return Assert.IsType<T>(value);
        }
    }
}
