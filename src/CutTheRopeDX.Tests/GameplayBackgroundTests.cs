using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;

using CutTheRopeDX.Commons;
using CutTheRopeDX.Framework;
using CutTheRopeDX.Framework.Core;
using CutTheRopeDX.Framework.Helpers;
using CutTheRopeDX.Framework.Platform;
using CutTheRopeDX.Framework.Visual;
using CutTheRopeDX.GameMain;
using CutTheRopeDX.Tests.Interactions;

using Xunit;

namespace CutTheRopeDX.Tests
{
    /// <summary>
    /// Covers the gameplay background: the one piece of art behind the level, drawn through the
    /// camera and repeated vertically. It has to reach every edge of whatever window the level is
    /// played in, and the repeat's seam has to stay off the screen while it does.
    /// </summary>
    public sealed class GameplayBackgroundTests
    {
        /// <summary>A level whose map fits the screen, so the camera holds still.</summary>
        private const int StillPack = 0;

        /// <summary>Level index of that level.</summary>
        private const int StillLevel = 0;

        /// <summary>A level whose map is taller than the screen, so the camera scrolls.</summary>
        private const int ScrollingLevel = 14;

        /// <summary>
        /// How much of a world unit an edge may fall short by. A cover fit lands the art's edge on
        /// the screen's, and the two are computed from the same numbers in a different order, so
        /// they agree to rounding rather than exactly. A world unit here is under a screen pixel.
        /// </summary>
        private const float EdgeTolerance = 0.5f;

        [Theory]
        [MemberData(nameof(Surfaces))]
        public void BackgroundReachesEveryEdgeOfTheScreen(string name, int width, int height)
        {
            WithScene(width, height, StillLevel, (background, visibleWorld) =>
            {
                CTRRectangle drawn = background.WorldRect;

                Assert.True(
                    drawn.x <= visibleWorld.x + EdgeTolerance
                        && drawn.x + drawn.w >= visibleWorld.x + visibleWorld.w - EdgeTolerance,
                    $"{name}: background spans x [{drawn.x}, {drawn.x + drawn.w}], screen needs "
                        + $"[{visibleWorld.x}, {visibleWorld.x + visibleWorld.w}]");
                Assert.True(
                    drawn.y <= visibleWorld.y + EdgeTolerance
                        && drawn.y + drawn.h >= visibleWorld.y + visibleWorld.h - EdgeTolerance,
                    $"{name}: background spans y [{drawn.y}, {drawn.y + drawn.h}], screen needs "
                        + $"[{visibleWorld.y}, {visibleWorld.y + visibleWorld.h}]");
            });
        }

        [Theory]
        [MemberData(nameof(Surfaces))]
        public void NoRepeatSeamCrossesTheScreen(string name, int width, int height)
        {
            WithScene(width, height, StillLevel, (background, visibleWorld) =>
            {
                // The art repeats vertically, and its two ends do not match. Every seam therefore
                // has to fall on an edge of the screen or outside it, which happens only while a
                // single repeat covers the whole of it.
                float seam = background.FirstSeamBelow(visibleWorld.y + EdgeTolerance);

                Assert.True(
                    seam >= visibleWorld.y + visibleWorld.h - EdgeTolerance,
                    $"{name}: a seam crosses the screen at world y {seam}, inside "
                        + $"[{visibleWorld.y}, {visibleWorld.y + visibleWorld.h}]");
            });
        }

        [Theory]
        [MemberData(nameof(Surfaces))]
        public void ScreenFitsTheRegionTheTileMapFills(string name, int width, int height)
        {
            WithScene(width, height, StillLevel, (background, visibleWorld) =>
            {
                // The tile map fills a window of the design size around the camera and clips
                // whatever falls outside it, so a screen that reaches past that window is filled
                // short however large the art is scaled.
                Assert.True(
                    visibleWorld.w <= background.FilledWidth + EdgeTolerance
                        && visibleWorld.h <= background.FilledHeight + EdgeTolerance,
                    $"{name}: screen covers {visibleWorld.w}x{visibleWorld.h} of world, the tile "
                        + $"map fills {background.FilledWidth}x{background.FilledHeight}");
            });
        }

        [Fact]
        public void BackgroundIsDrawnAtItsAuthoredSizeOnTheDesignShape()
        {
            WithScene(2560, 1440, StillLevel, (background, visibleWorld) =>
            {
                // One background, one screen: the shape the art was drawn for scales it by the
                // ratio between the two, which its one-pixel-narrow width makes not quite one.
                Assert.Equal(2560f / 2559f, background.Scale, 0.001);
                Assert.Equal(2560f, visibleWorld.w, 0.5);
            });
        }

        [Fact]
        public void BackgroundHoldsItsPlaceInTheWorldWhileTheCameraScrolls()
        {
            LayoutSurfaces.WithSurface(1280, 720, () =>
            {
                _ = HeadlessGame.Boot();
                GameScene scene = HeadlessGame.LoadLevel(StillPack, ScrollingLevel);
                scene.Update(0.016f);
                CTRRectangle before = ReadBackground(scene).WorldRect;

                // The camera of a level this tall is still travelling at frame 60.
                HeadlessGame.StepFrames(scene, 60);
                CTRRectangle after = ReadBackground(scene).WorldRect;

                // Anchored to the world, not to the camera: a background that moved with the
                // camera would leave the level's second screen and its own second half apart.
                Assert.Equal(before.x, after.x, 0.01);
                Assert.Equal(before.y, after.y, 0.01);
                Assert.Equal(before.w, after.w, 0.01);
                Assert.Equal(before.h, after.h, 0.01);
            });
        }

        [Fact]
        public void WideLevelP1TilesCoverTheRightCameraWindow()
        {
            GameScene scene = Scenario.New()
                .MapSize(1280, 480)
                .Candy(1100, 120)
                .OmNom(1180, 360)
                .Build();

            Assert.True(Read<float>(scene, "mapWidth") > FrameworkTypes.SCREEN_WIDTH);
            TileMap background = Read<TileMap>(scene, "back");
            Assert.Equal(
                TileMap.Repeat.ALL,
                Read<TileMap.Repeat>(background, "repeatedHorizontally"));

            // Move the tile-map camera half a frame to the right, past the original P1's center.
            // The generated quads must still form one unbroken span across the whole camera.
            float cameraX = FrameworkTypes.SCREEN_WIDTH / 2f;
            background.UpdateWithCameraPos(new Vector(cameraX, 0f));

            List<ImageMultiDrawer> drawers = Read<List<ImageMultiDrawer>>(background, "drawers");
            ImageMultiDrawer drawer = Assert.Single(drawers);
            List<Quad3D> quads = [];
            for (int i = 0; i < drawer.numberOfQuadsToDraw; i++)
            {
                quads.Add(drawer.vertices[i]);
            }
            quads.Sort((left, right) => left.BlX.CompareTo(right.BlX));

            float coveredUntil = cameraX;
            foreach (Quad3D quad in quads)
            {
                Assert.True(
                    quad.BlX <= coveredUntil + EdgeTolerance,
                    $"P1 leaves a horizontal gap from {coveredUntil} to {quad.BlX}");
                coveredUntil = MathF.Max(coveredUntil, quad.BrX);
            }
            Assert.True(
                coveredUntil >= cameraX + FrameworkTypes.SCREEN_WIDTH - EdgeTolerance,
                $"P1 coverage ends at {coveredUntil}, before the camera edge at "
                    + $"{cameraX + FrameworkTypes.SCREEN_WIDTH}");
        }

        [Fact]
        public void EarthImageKeepsItsAuthoredOffsetFromP1AfterResize()
        {
            _ = HeadlessGame.Boot();

            LayoutSurfaces.WithSurface(2560, 1440, () =>
            {
                GameScene scene = HeadlessGame.LoadLevel(pack: 7, level: 0);

                CtrRenderer.OnSurfaceChanged(1868, 1674);
                scene.RelayoutCamera();
                scene.RelayoutHud();

                TileMap background = Read<TileMap>(scene, "back");
                Image earth = ReadFirstEarthImage(scene.gravityState);
                Assert.Equal(1284f, earth.x - background.x, 0.01);
                Assert.Equal(724f, earth.y - background.y, 0.01);
            });
        }

        /// <summary>
        /// Loads a level laid out for the given surface and hands its background to
        /// <paramref name="body"/> along with the region of world the screen exposes.
        /// </summary>
        /// <param name="width">Surface width to lay out for.</param>
        /// <param name="height">Surface height to lay out for.</param>
        /// <param name="level">Zero-based level index within the first pack.</param>
        /// <param name="body">Work to run against the background and the visible world.</param>
        private static void WithScene(
            int width,
            int height,
            int level,
            Action<Background, CTRRectangle> body)
        {
            _ = HeadlessGame.Boot();

            LayoutSurfaces.WithSurface(width, height, () =>
            {
                GameScene scene = HeadlessGame.LoadLevel(StillPack, level);

                // One frame is what settles the camera onto its fit, and the background is
                // measured against where the camera actually ends up.
                scene.Update(0.016f);

                Camera2D camera = Read<Camera2D>(scene, "camera");
                CTRRectangle visible = ScreenPresentation.Instance.Snapshot.VisibleBounds;
                CTRRectangle visibleWorld = new(
                    camera.RenderPos.X,
                    camera.RenderPos.Y,
                    visible.w / camera.Scale,
                    visible.h / camera.Scale);

                body(ReadBackground(scene), visibleWorld);
            });
        }

        /// <summary>Reads the scene's background as world-space geometry.</summary>
        /// <param name="scene">Scene to read.</param>
        /// <returns>The background's placement.</returns>
        private static Background ReadBackground(GameScene scene)
        {
            TileMap back = Read<TileMap>(scene, "back");
            CTRTexture2D texture = Read<CTRTexture2D>(scene, "backTexture");
            float scale = Read<float>(scene, "backgroundScale");
            Camera2D camera = Read<Camera2D>(scene, "camera");

            return new Background(
                new CTRRectangle(
                    back.x * scale,
                    back.y * scale,
                    texture._realWidth * scale,
                    texture._realHeight * scale),
                scale,
                FrameworkTypes.SCREEN_WIDTH * scale / camera.Scale,
                FrameworkTypes.SCREEN_HEIGHT * scale / camera.Scale);
        }

        private static T Read<T>(object target, string field)
        {
            object value = target.GetType()
                .GetField(field, BindingFlags.Instance | BindingFlags.NonPublic)
                ?.GetValue(target);
            return Assert.IsType<T>(value);
        }

        private static Image ReadFirstEarthImage(GravityState gravityState)
        {
            object value = gravityState.GetType()
                .GetField("earthAnimations", BindingFlags.Instance | BindingFlags.NonPublic)
                ?.GetValue(gravityState);
            IList earthAnimations = Assert.IsAssignableFrom<IList>(value);
            _ = Assert.Single(earthAnimations);
            object first = earthAnimations[0];
            if (first is Image image)
            {
                return image;
            }

            PropertyInfo imageProperty = first.GetType().GetProperty(
                "Image",
                BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            Assert.NotNull(imageProperty);
            return Assert.IsType<Image>(imageProperty.GetValue(first));
        }

        public static TheoryData<string, int, int> Surfaces()
        {
            return LayoutSurfaces.Theory();
        }

        /// <summary>Where the background lands in world space, and how far its fill reaches.</summary>
        /// <param name="WorldRect">The single repeat of the art, in world units.</param>
        /// <param name="Scale">Scale from the art's own pixels to world units.</param>
        /// <param name="FilledWidth">Width of world the tile map fills before it clips.</param>
        /// <param name="FilledHeight">Height of world the tile map fills before it clips.</param>
        private sealed record Background(
            CTRRectangle WorldRect,
            float Scale,
            float FilledWidth,
            float FilledHeight)
        {
            /// <summary>
            /// The first repeat seam strictly below <paramref name="worldY"/>. Seams recur every
            /// repeat of the art, in both directions from where it is anchored; one landing on the
            /// screen's own top edge is the aligned case, not a seam in view.
            /// </summary>
            /// <param name="worldY">World position to search from.</param>
            /// <returns>World Y of that seam.</returns>
            public float FirstSeamBelow(float worldY)
            {
                float step = WorldRect.h;
                float repeats = MathF.Floor((worldY - WorldRect.y) / step) + 1f;
                return WorldRect.y + (repeats * step);
            }
        }
    }
}
