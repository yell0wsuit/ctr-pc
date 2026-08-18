using System.Reflection;

using CutTheRopeDX.Commons;
using CutTheRopeDX.Framework.Helpers;
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
        public void CameraStartsAtTheOriginForANonScrollingLevel()
        {
            _ = HeadlessGame.Boot();
            GameScene scene = HeadlessGame.LoadLevel(0, 0);

            Camera2D camera = ReadCamera(scene);

            Assert.Equal(0f, camera.pos.X, 0.01);
            Assert.Equal(0f, camera.pos.Y, 0.01);
            Assert.Equal(0f, camera.target.X, 0.01);
            Assert.Equal(0f, camera.target.Y, 0.01);
        }

        [Fact]
        public void ScreenToWorldAddsTheCameraPositionToday()
        {
            _ = HeadlessGame.Boot();
            GameScene scene = HeadlessGame.LoadLevel(0, 0);

            Camera2D camera = ReadCamera(scene);

            Assert.Equal(640f, 640f + camera.pos.X, 0.01);
            Assert.Equal(360f, 360f + camera.pos.Y, 0.01);
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
        [InlineData(0, 0, 0f, 0f, 0f, 0f)]
        // The tall level is still moving toward the bottom of its 2880-unit map at frame 60.
        [InlineData(0, 14, 0f, 381.6959f, 0f, 1440f)]
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

        public static TheoryData<string, int, int> Surfaces()
        {
            return LayoutSurfaces.Theory();
        }
    }
}
