using System.Reflection;

using CutTheRopeDX.Commons;
using CutTheRopeDX.Framework;
using CutTheRopeDX.Framework.Helpers;
using CutTheRopeDX.Framework.Platform;
using CutTheRopeDX.GameMain;

using Xunit;

namespace CutTheRopeDX.Tests
{
    /// <summary>
    /// Pins gameplay framing: map placement, camera position and screen-to-world conversion.
    /// A change here moves the game rather than its presentation, so these values must either be
    /// preserved or deliberately changed by a camera refactor.
    /// </summary>
    public sealed class GameplayLayoutCharacterizationTests
    {
        [Fact]
        public void MapPlacementIsPinnedForTheFirstLevel()
        {
            _ = HeadlessGame.Boot();
            GameScene scene = HeadlessGame.LoadLevel(0, 0);

            Assert.Equal(960f, ReadFloat(scene, "mapWidth"));
            Assert.Equal(1440f, ReadFloat(scene, "mapHeight"));
            Assert.Equal(800f, ReadFloat(scene, "mapOriginX"));
            Assert.Equal(0f, ReadFloat(scene, "mapOriginY"));
        }

        [Fact]
        public void CameraStartsAtTheLevelOriginForANonScrollingLevel()
        {
            _ = HeadlessGame.Boot();
            GameScene scene = HeadlessGame.LoadLevel(0, 0);

            Camera2D camera = ReadCamera(scene);

            // The tracked position is a world coordinate, so a level narrower than the design box
            // starts it at the level's own left edge rather than at world zero. The region that
            // ends up drawn is unaffected: the fit spends the horizontal slack about that edge,
            // which is what CameraStaysAtTheDesignOriginForANonScrollingLevel pins.
            Assert.Equal(800f, camera.pos.X, 0.01);
            Assert.Equal(0f, camera.pos.Y, 0.01);
            Assert.Equal(800f, camera.target.X, 0.01);
            Assert.Equal(0f, camera.target.Y, 0.01);
        }

        [Fact]
        public void CameraStaysAtTheDesignOriginForANonScrollingLevel()
        {
            _ = HeadlessGame.Boot();
            GameScene scene = HeadlessGame.LoadLevel(0, 0);

            Camera2D camera = ReadCamera(scene);

            Assert.Equal(0f, camera.RenderPos.X, 0.01);
            Assert.Equal(0f, camera.RenderPos.Y, 0.01);
        }

        [Fact]
        public void ScreenToWorldIsTheIdentityForANonScrollingLevel()
        {
            _ = HeadlessGame.Boot();
            GameScene scene = HeadlessGame.LoadLevel(0, 0);

            Camera2D camera = ReadCamera(scene);

            Assert.Equal(640f, camera.ScreenToWorldX(640f), 0.01);
            Assert.Equal(360f, camera.ScreenToWorldY(360f), 0.01);
        }

        [Theory]
        [MemberData(nameof(Surfaces))]
        public void FirstLevelMapPlacementDoesNotTrackTheSurfaceSize(
            string name,
            int width,
            int height)
        {
            _ = HeadlessGame.Boot();

            LayoutSurfaces.WithSurface(width, height, () =>
            {
                CtrRenderer.OnSurfaceChanged(width, height);
                GameScene scene = HeadlessGame.LoadLevel(0, 0);

                Assert.Equal(960f, ReadFloat(scene, "mapWidth"));
                Assert.Equal(1440f, ReadFloat(scene, "mapHeight"));
                Assert.Equal(800f, ReadFloat(scene, "mapOriginX"));
                Assert.Equal(0f, ReadFloat(scene, "mapOriginY"));
                Assert.False(string.IsNullOrEmpty(name));
            });
        }

        [Theory]
        // X is the level's own left edge, not world zero: these maps are narrower than the design
        // box and sit centered on it, and the tracked position is a world coordinate.
        [InlineData(0, 0, 800f, 0f, 800f, 0f)]
        // The tall level is still moving toward the bottom of its 2880-unit map at frame 60.
        [InlineData(0, 14, 800f, 381.6959f, 800f, 1440f)]
        public void CameraPositionAfterSixtyFramesIsPinned(
            int pack,
            int level,
            float expectedX,
            float expectedY,
            float expectedTargetX,
            float expectedTargetY)
        {
            _ = HeadlessGame.Boot();
            GameScene scene = HeadlessGame.LoadLevel(pack, level);
            HeadlessGame.StepFrames(scene, 60);

            Camera2D camera = ReadCamera(scene);

            Assert.Equal(expectedX, camera.pos.X, 0.01);
            Assert.Equal(expectedY, camera.pos.Y, 0.01);
            Assert.Equal(expectedTargetX, camera.target.X, 0.01);
            Assert.Equal(expectedTargetY, camera.target.Y, 0.01);
        }

        [Fact]
        public void CameraBoundsAreTheWholeMapForANonScrollingLevel()
        {
            _ = HeadlessGame.Boot();
            GameScene scene = HeadlessGame.LoadLevel(0, 0);

            // The pinned map for this level is 960x1440 at origin (800, 0).
            CTRRectangle bounds = ReadRectangle(scene, "cameraBounds");

            Assert.Equal(800f, bounds.x, 0.01);
            Assert.Equal(0f, bounds.y, 0.01);
            Assert.Equal(960f, bounds.w, 0.01);
            Assert.Equal(1440f, bounds.h, 0.01);
        }

        [Fact]
        public void CameraFitReproducesTodaysCenteringAtSixteenNine()
        {
            // FitCamera over a 960x1440 level in a 2560x1440 viewport must land the level exactly
            // where offsetX = (2560 - 960) / 2 put it.
            CameraFit fit = LayoutMath.FitCamera(
                new CTRRectangle(0f, 0f, 960f, 1440f),
                new CTRRectangle(0f, 0f, 2560f, 1440f),
                0.5f,
                0.5f);

            Assert.Equal(1f, fit.Scale, 0.001);
            Assert.Equal(-800f, fit.VisibleWorld.x, 0.01);
            Assert.Equal(2560f, fit.VisibleWorld.w, 0.01);
        }

        private static Camera2D ReadCamera(GameScene scene)
        {
            FieldInfo field = typeof(GameScene).GetField(
                "camera",
                BindingFlags.Instance | BindingFlags.NonPublic);
            return Assert.IsType<Camera2D>(field?.GetValue(scene));
        }

        private static float ReadFloat(GameScene scene, string fieldName)
        {
            FieldInfo field = typeof(GameScene).GetField(
                fieldName,
                BindingFlags.Instance | BindingFlags.NonPublic);
            return Assert.IsType<float>(field?.GetValue(scene));
        }

        private static CTRRectangle ReadRectangle(GameScene scene, string fieldName)
        {
            FieldInfo field = typeof(GameScene).GetField(
                fieldName,
                BindingFlags.Instance | BindingFlags.NonPublic);
            return Assert.IsType<CTRRectangle>(field?.GetValue(scene));
        }

        public static TheoryData<string, int, int> Surfaces()
        {
            return LayoutSurfaces.Theory();
        }
    }
}
