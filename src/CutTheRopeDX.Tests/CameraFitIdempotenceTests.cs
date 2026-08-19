using System.Reflection;

using CutTheRopeDX.Framework.Helpers;
using CutTheRopeDX.Framework.Platform;
using CutTheRopeDX.GameMain;

using Xunit;

namespace CutTheRopeDX.Tests
{
    /// <summary>
    /// The camera fit reads the tracked position and writes only what gets drawn. Applying it
    /// repeatedly must therefore land in the same place every time. When the fit took its anchor
    /// from its own previous result instead, any viewport with vertical slack walked the camera
    /// toward the top of the level a little further on each pass.
    /// </summary>
    public sealed class CameraFitIdempotenceTests
    {
        [Theory]
        [MemberData(nameof(Surfaces))]
        public void RepeatedFitsLandInTheSamePlace(string name, int width, int height)
        {
            _ = HeadlessGame.Boot();

            LayoutSurfaces.WithSurface(width, height, () =>
            {
                // A level twice the design height, so there is a scroll range to walk along.
                GameScene scene = HeadlessGame.LoadLevel(0, 14);
                Camera2D camera = ReadCamera(scene);
                MethodInfo apply = typeof(GameScene).GetMethod(
                    "ApplyCameraFit",
                    BindingFlags.Instance | BindingFlags.NonPublic);
                Assert.NotNull(apply);

                // Drive the tracked position to the bottom of the level's scroll range.
                camera.MoveToXYImmediate(0f, ReadFloat(scene, "mapHeight"), true);
                _ = apply.Invoke(scene, [ScreenPresentation.Instance.Snapshot]);

                float scale = camera.Scale;
                float x = camera.RenderPos.X;
                float y = camera.RenderPos.Y;

                for (int i = 0; i < 10; i++)
                {
                    _ = apply.Invoke(scene, [ScreenPresentation.Instance.Snapshot]);
                }

                Assert.Equal(scale, camera.Scale, 0.001);
                Assert.Equal(x, camera.RenderPos.X, 0.001);
                Assert.Equal(y, camera.RenderPos.Y, 0.001);
                Assert.False(string.IsNullOrEmpty(name));
            });
        }

        [Fact]
        public void TheFitNeverMovesTheTrackedPosition()
        {
            _ = HeadlessGame.Boot();

            LayoutSurfaces.WithSurface(720, 1280, () =>
            {
                GameScene scene = HeadlessGame.LoadLevel(0, 14);
                Camera2D camera = ReadCamera(scene);
                MethodInfo apply = typeof(GameScene).GetMethod(
                    "ApplyCameraFit",
                    BindingFlags.Instance | BindingFlags.NonPublic);

                camera.MoveToXYImmediate(0f, 900f, true);
                _ = apply.Invoke(scene, [ScreenPresentation.Instance.Snapshot]);

                Assert.Equal(900f, camera.pos.Y, 0.001);
            });
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
