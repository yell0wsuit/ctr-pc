using System;
using System.Reflection;

using CutTheRopeDX.Commons;
using CutTheRopeDX.Framework.Helpers;
using CutTheRopeDX.Framework.Platform;
using CutTheRopeDX.GameMain;

using Xunit;

namespace CutTheRopeDX.Tests
{
    /// <summary>
    /// Covers a window resized while the game is paused. A paused scene stops updating, and the
    /// camera fit - which decides how much world a screen of this shape sees, and through that how
    /// far the background has to reach - was only ever applied from the update loop.
    /// </summary>
    public sealed class PausedResizeTests
    {
        [Fact]
        public void ResizingWhilePausedFitsTheCameraToTheNewViewport()
        {
            // A scene resized while paused must land where one built at that size does. It used to
            // keep the fit it was paused with: the world drawn for the old shape, offset from the
            // screen it is now on.
            _ = HeadlessGame.Boot();

            (float scale, float x, float y) = ResizedWhilePaused(Fit);
            (float scale, float x, float y) fresh = BuiltAtTheNewSize(Fit);

            Assert.Equal(fresh.scale, scale, 0.0001);
            Assert.Equal(fresh.x, x, 0.0001);
            Assert.Equal(fresh.y, y, 0.0001);
        }

        [Fact]
        public void TheBackgroundIsRecoveredForTheNewViewportToo()
        {
            // How far the background has to reach is measured through the camera's own scale, so a
            // stale fit left it short of the edges it no longer reached - a black band down the
            // side of the paused screen.
            _ = HeadlessGame.Boot();

            float resized = ResizedWhilePaused(BackgroundScale);
            float fresh = BuiltAtTheNewSize(BackgroundScale);

            Assert.Equal(fresh, resized, 0.0001);
        }

        /// <summary>
        /// Loads a level on a landscape surface, pauses, resizes to a portrait one and lays out,
        /// then reads something off the scene.
        /// </summary>
        /// <typeparam name="T">What is read.</typeparam>
        /// <param name="read">Reads the value from the controller.</param>
        /// <returns>The value after the resize.</returns>
        private static T ResizedWhilePaused<T>(Func<GameController, T> read)
        {
            T value = default;
            LayoutSurfaces.WithSurface(2572, 1080, () =>
            {
                GameController controller = HeadlessGame.LoadLevelWithController(0, 14);
                controller.OnButtonPressed(GameControllerButtonId.Pause);

                CtrRenderer.OnSurfaceChanged(720, 1280);
                controller.RelayoutTree(ScreenPresentation.Instance.Snapshot);

                value = read(controller);
            });
            return value;
        }

        /// <summary>
        /// Loads the same level on the surface the resize ends at, and reads the same thing.
        /// </summary>
        /// <typeparam name="T">What is read.</typeparam>
        /// <param name="read">Reads the value from the controller.</param>
        /// <returns>The value on a scene built at that size.</returns>
        private static T BuiltAtTheNewSize<T>(Func<GameController, T> read)
        {
            T value = default;
            LayoutSurfaces.WithSurface(720, 1280, () =>
                value = read(HeadlessGame.LoadLevelWithController(0, 14)));
            return value;
        }

        /// <summary>The fit the scene's camera is drawn at.</summary>
        /// <param name="controller">Controller owning the scene.</param>
        /// <returns>The camera's scale and rendered position.</returns>
        private static (float Scale, float X, float Y) Fit(GameController controller)
        {
            Camera2D camera = ReadCamera(Scene(controller));
            return (camera.Scale, camera.RenderPos.X, camera.RenderPos.Y);
        }

        /// <summary>The scale the scene's background is drawn at.</summary>
        /// <param name="controller">Controller owning the scene.</param>
        /// <returns>The background scale.</returns>
        private static float BackgroundScale(GameController controller)
        {
            GameScene scene = Scene(controller);
            FieldInfo field = typeof(GameScene).GetField(
                "backgroundScale",
                BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.NotNull(field);
            return (float)field.GetValue(scene);
        }

        /// <summary>The scene a controller is playing.</summary>
        /// <param name="controller">Controller owning the scene.</param>
        /// <returns>The scene.</returns>
        private static GameScene Scene(GameController controller)
        {
            return (GameScene)controller.GetView(0).GetChild(0);
        }

        /// <summary>Reads a scene's camera.</summary>
        /// <param name="scene">Scene to read.</param>
        /// <returns>The camera.</returns>
        private static Camera2D ReadCamera(GameScene scene)
        {
            FieldInfo field = typeof(GameScene).GetField(
                "camera",
                BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.NotNull(field);
            return (Camera2D)field.GetValue(scene);
        }
    }
}
