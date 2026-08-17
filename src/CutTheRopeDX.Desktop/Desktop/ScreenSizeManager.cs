using System;

using CutTheRopeDX.Commons;
using CutTheRopeDX.Framework.Core;
using CutTheRopeDX.Framework.Platform;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace CutTheRopeDX.Desktop
{
    /// <summary>
    /// Manages the OS window, fullscreen toggling, and persisted window settings for the desktop
    /// renderer. Device-free presentation math (render viewport, coordinate transforms) lives in
    /// <see cref="ScreenPresentation"/>; this class feeds it via <see cref="CtrRenderer.OnSurfaceChanged"/>
    /// whenever the window or fullscreen bounds change, and implements <see cref="IWindowService"/>
    /// so Core code can command window behavior through <see cref="PlatformServices.Window"/>.
    /// </summary>
    /// <param name="gameWidth">Logical game width.</param>
    /// <param name="gameHeight">Logical game height.</param>
    internal sealed class ScreenSizeManager(int gameWidth, int gameHeight) : IWindowService
    {
        /// <summary>
        /// Maximum allowed window width for the active graphics profile.
        /// </summary>
        public static int MAX_WINDOW_WIDTH => Global.GraphicsDeviceManager.GraphicsProfile == GraphicsProfile.HiDef ? 4096 : 2048;

        /// <summary>
        /// Gets the logical game width. Retained from the constructor for API symmetry with
        /// <see cref="ScreenPresentation"/>, which is the sole consumer of the aspect ratio it implies.
        /// </summary>
        public int GameWidth { get; } = gameWidth;

        /// <summary>
        /// Gets the logical game height. Retained from the constructor for API symmetry with
        /// <see cref="ScreenPresentation"/>, which is the sole consumer of the aspect ratio it implies.
        /// </summary>
        public int GameHeight { get; } = gameHeight;

        /// <summary>
        /// Gets the current window back-buffer width.
        /// </summary>
        public int WindowWidth => _windowRect.Width;

        /// <summary>
        /// Gets the current window back-buffer height.
        /// </summary>
        public int WindowHeight => _windowRect.Height;

        /// <summary>
        /// Gets the current fullscreen display width.
        /// </summary>
        public int ScreenWidth => _fullScreenRect.Width;

        /// <summary>
        /// Gets the current fullscreen display height.
        /// </summary>
        public int ScreenHeight => _fullScreenRect.Height;

        /// <summary>
        /// Gets a value indicating whether fullscreen mode is currently enabled.
        /// </summary>
        public bool IsFullScreen { get; private set; }

        /// <summary>
        /// Gets the active output rectangle, using fullscreen or window bounds as appropriate.
        /// </summary>
        public Rectangle CurrentSize => IsFullScreen ? _fullScreenRect : _windowRect;

        /// <summary>
        /// Gets a value indicating whether size-change reactions are temporarily disabled.
        /// </summary>
        public bool SkipSizeChanges { get; private set; }

        /// <summary>
        /// Initializes screen sizing from the current display mode, preferred window width, and fullscreen state.
        /// </summary>
        /// <param name="displayMode">Current display mode.</param>
        /// <param name="windowWidth">Preferred window width, or a non-positive value to derive one automatically.</param>
        /// <param name="windowHeight">Preferred window height, or a non-positive value to derive one automatically.</param>
        /// <param name="isFullScreen"><see langword="true" /> to start in fullscreen mode.</param>
        public void Init(DisplayMode displayMode, int windowWidth, int windowHeight, bool isFullScreen)
        {
            FullScreenRectChanged(displayMode);
            Point target = ClampWindowSize(windowWidth, windowHeight, displayMode.Width, displayMode.Height);
            WindowRectChanged(new Rectangle(0, 0, target.X, target.Y));
            if (isFullScreen)
            {
                ToggleFullScreen();
                return;
            }
            ApplyWindowSize(WindowWidth, WindowHeight);
            CenterWindow();
            // Size the canvas to the window that was just established.
            Application.SharedCanvas().Reshape();
        }

        /// <summary>
        /// Centers the game window on the primary display. A programmatic back-buffer resize keeps the
        /// window's top-left corner pinned, so this must be called after sizing to avoid the window
        /// hugging a screen corner. Repositioning also forces the window frame to re-layout, which
        /// restores the title bar after returning from borderless fullscreen.
        /// </summary>
        public void CenterWindow()
        {
            if (IsFullScreen)
            {
                return;
            }
            DisplayMode displayMode = GraphicsAdapter.DefaultAdapter.CurrentDisplayMode;
            int x = Math.Max(0, (displayMode.Width - _windowRect.Width) / 2);
            int y = Math.Max(0, (displayMode.Height - _windowRect.Height) / 2);
            Global.XnaGame.Window.Position = new Point(x, y);
        }

        /// <summary>
        /// Clamps a requested window size to the game's minimum, the graphics profile maximum, and
        /// the display, deriving a default from the display for either axis that is not supplied.
        /// Shared by startup swapchain sizing and <see cref="Init"/> so both agree on the target.
        /// </summary>
        /// <remarks>
        /// Each axis is clamped on its own. The window's shape is the user's to choose and the
        /// layout follows whatever they choose, so tying one axis to the other here would put the
        /// aspect ratio back under the game's control.
        /// </remarks>
        /// <param name="windowWidth">Requested window width, or a non-positive value to derive one from the display.</param>
        /// <param name="windowHeight">Requested window height, or a non-positive value to derive one from the display.</param>
        /// <param name="displayWidth">Current display width.</param>
        /// <param name="displayHeight">Current display height.</param>
        /// <returns>The clamped window size.</returns>
        public static Point ClampWindowSize(
            int windowWidth,
            int windowHeight,
            int displayWidth,
            int displayHeight)
        {
            return ClampWindowSize(
                windowWidth, windowHeight, displayWidth, displayHeight, MAX_WINDOW_WIDTH);
        }

        /// <summary>
        /// The arithmetic behind <see cref="ClampWindowSize(int, int, int, int)"/>, with the
        /// profile maximum passed in rather than read from the graphics device, so the clamping
        /// rules can be exercised without one.
        /// </summary>
        /// <param name="windowWidth">Requested window width, or a non-positive value to derive one from the display.</param>
        /// <param name="windowHeight">Requested window height, or a non-positive value to derive one from the display.</param>
        /// <param name="displayWidth">Current display width.</param>
        /// <param name="displayHeight">Current display height.</param>
        /// <param name="maximum">Largest length the graphics profile permits on either axis.</param>
        /// <returns>The clamped window size.</returns>
        public static Point ClampWindowSize(
            int windowWidth,
            int windowHeight,
            int displayWidth,
            int displayHeight,
            int maximum)
        {
            return new Point(
                ClampAxis(windowWidth, displayWidth, MIN_WINDOW_WIDTH, maximum),
                ClampAxis(windowHeight, displayHeight, MIN_WINDOW_HEIGHT, maximum));
        }

        /// <summary>
        /// Clamps one window axis to its floor, the graphics profile maximum and the display.
        /// </summary>
        /// <param name="requested">Requested length, or a non-positive value to derive one from the display.</param>
        /// <param name="displayLength">Display length on the same axis.</param>
        /// <param name="minimum">Smallest permitted length.</param>
        /// <param name="maximum">Largest permitted length.</param>
        /// <returns>The clamped length.</returns>
        private static int ClampAxis(int requested, int displayLength, int minimum, int maximum)
        {
            int target = requested > 0 ? requested : displayLength - 100;
            target = Math.Max(minimum, target);
            target = Math.Min(target, maximum);
            return Math.Min(target, displayLength);
        }

        /// <summary>
        /// Applies a new window back-buffer size and updates the tracked window rectangle.
        /// </summary>
        /// <param name="width">Target window width.</param>
        /// <param name="height">Target window height.</param>
        public void ApplyWindowSize(int width, int height)
        {
            GraphicsDeviceManager graphicsDeviceManager = Global.GraphicsDeviceManager;
            // Skip the swapchain rebuild when the back buffer already matches the requested size.
            // At startup the swapchain is created at this size (see Game1), so the first sizing here
            // would otherwise rebuild it needlessly and flash black.
            GraphicsDevice device = graphicsDeviceManager.GraphicsDevice;
            bool alreadySized = device != null
                && device.PresentationParameters.BackBufferWidth == width
                && device.PresentationParameters.BackBufferHeight == height;
            graphicsDeviceManager.PreferredBackBufferWidth = width;
            graphicsDeviceManager.PreferredBackBufferHeight = height;
            if (!alreadySized)
            {
                ApplyDesktopVkResize(graphicsDeviceManager);
            }
            WindowRectChanged(new Rectangle(0, 0, graphicsDeviceManager.PreferredBackBufferWidth, graphicsDeviceManager.PreferredBackBufferHeight));
        }

        /// <summary>
        /// Toggles between windowed and fullscreen mode, updates the viewport, persists settings,
        /// and notifies the canvas and root controller.
        /// </summary>
        public void ToggleFullScreen()
        {
            SkipSizeChanges = true;
            GraphicsDeviceManager graphicsDeviceManager = Global.GraphicsDeviceManager;
            bool isFullScreen = graphicsDeviceManager.IsFullScreen;
            if (isFullScreen)
            {
                graphicsDeviceManager.PreferredBackBufferWidth = _windowRect.Width;
                graphicsDeviceManager.PreferredBackBufferHeight = _windowRect.Height;
            }
            else
            {
                graphicsDeviceManager.PreferredBackBufferWidth = _fullScreenRect.Width;
                graphicsDeviceManager.PreferredBackBufferHeight = _fullScreenRect.Height;
            }
            graphicsDeviceManager.IsFullScreen = !isFullScreen;
            ApplyDesktopVkResize(graphicsDeviceManager);
            ApplyViewportToDevice();
            SkipSizeChanges = false;
            EnableFullScreen(!isFullScreen);
            // Returning to windowed mode: re-center so the restored window is not stuck in a corner
            // and to force the frame to re-layout, which repaints the title bar the borderless
            // fullscreen transition would otherwise leave missing until the next manual resize.
            if (isFullScreen)
            {
                CenterWindow();
            }
            Save();
            Application.SharedCanvas().Reshape();
            Application.SharedRootController().FullscreenToggled(!isFullScreen);
        }

        /// <summary>
        /// Adopts a window size change and persists the result.
        /// </summary>
        /// <remarks>
        /// The reported bounds are taken as they are, clamped only against the minimum and the
        /// display. Nothing here reshapes the window: the layout adapts to whatever the user drags
        /// it to, so correcting the size would only fight them for control of the frame.
        /// </remarks>
        /// <param name="newWindowRect">New window bounds reported by the host window.</param>
        public void FixWindowSize(Rectangle newWindowRect)
        {
            if (SkipSizeChanges)
            {
                return;
            }
            FullScreenRectChanged(GraphicsAdapter.DefaultAdapter.CurrentDisplayMode);
            if (!IsFullScreen)
            {
                try
                {
                    Point target = ClampWindowSize(
                        newWindowRect.Width,
                        newWindowRect.Height,
                        ScreenWidth,
                        ScreenHeight);
                    ApplyWindowSize(target.X, target.Y);
                }
                catch (Exception)
                {
                }
            }
            Save();
            Application.SharedCanvas().Reshape();
        }

        /// <summary>
        /// Applies the current render viewport to the graphics device viewport.
        /// </summary>
        public void ApplyViewportToDevice()
        {
            Framework.CTRRectangle render = ScreenPresentation.Instance.Snapshot.RenderViewport;
            Rectangle renderViewRect = new((int)render.x, (int)render.y, (int)render.w, (int)render.h);
            Rectangle bounds = !IsFullScreen ? Rectangle.Intersect(renderViewRect, _windowRect) : Rectangle.Intersect(renderViewRect, _fullScreenRect);
            try
            {
                Global.GraphicsDevice.Viewport = new Viewport(bounds);
            }
            catch (Exception)
            {
            }
        }

        /// <summary>
        /// Saves window dimensions and fullscreen state to preferences.
        /// </summary>
        public void Save()
        {
            Preferences.SetIntForKey(_windowRect.Width, "PREFS_WINDOW_WIDTH", false);
            Preferences.SetIntForKey(_windowRect.Height, "PREFS_WINDOW_HEIGHT", false);
            Preferences.SetBooleanForKey(IsFullScreen, "PREFS_WINDOW_FULLSCREEN", true);
        }

        /// <summary>
        /// Updates the stored window rectangle and recomputes the scaled view rectangle.
        /// </summary>
        /// <param name="newWindowRect">New window rectangle.</param>
        private void WindowRectChanged(Rectangle newWindowRect)
        {
            if (!SkipSizeChanges)
            {
                _windowRect = newWindowRect;
                _windowRect.X = 0;
                _windowRect.Y = 0;
                UpdateScaledView();
            }
        }

        /// <summary>
        /// Updates the stored fullscreen rectangle from a display mode.
        /// </summary>
        /// <param name="d">Display mode to copy.</param>
        private void FullScreenRectChanged(DisplayMode d)
        {
            FullScreenRectChanged(new Rectangle(0, 0, d.Width, d.Height));
        }

        /// <summary>
        /// Updates the stored fullscreen rectangle and recomputes the scaled view rectangle.
        /// </summary>
        /// <param name="r">New fullscreen rectangle.</param>
        private void FullScreenRectChanged(Rectangle r)
        {
            if (!SkipSizeChanges)
            {
                _fullScreenRect = r;
                UpdateScaledView();
            }
        }

        /// <summary>
        /// Updates the tracked fullscreen state and recomputes the scaled view rectangle.
        /// </summary>
        /// <param name="bFull"><see langword="true" /> to mark fullscreen as enabled; otherwise <see langword="false" />.</param>
        private void EnableFullScreen(bool bFull)
        {
            if (!SkipSizeChanges)
            {
                IsFullScreen = bFull;
                UpdateScaledView();
            }
        }

        /// <summary>
        /// Feeds the current active (window or fullscreen) bounds into
        /// <see cref="ScreenPresentation.Instance"/> so it recomputes the published viewport. A
        /// no-op while <see cref="SkipSizeChanges"/> is set, mirroring the batching
        /// <see cref="ToggleFullScreen"/> relies on to defer recomputation until every field it
        /// touches has settled.
        /// </summary>
        private void UpdateScaledView()
        {
            if (SkipSizeChanges)
            {
                return;
            }
            Rectangle sourceRect = IsFullScreen ? _fullScreenRect : _windowRect;

            // MonoGame exposes no portable DPI scale, so the surface size is reported as-is at a
            // unit ratio. If the backbuffer is ever allocated at a drawable size that differs from
            // the client size, this becomes that ratio.
            CtrRenderer.OnSurfaceChanged(
                sourceRect.Width,
                sourceRect.Height,
                1f);
        }

        private static void ApplyDesktopVkResize(
            GraphicsDeviceManager graphicsDeviceManager)
        {
            bool originalSynchronization =
                graphicsDeviceManager.SynchronizeWithVerticalRetrace;

            // MonoGame 3.8.5 ignores dimension-only DesktopVK swapchain resizes.
            // Changing the presentation interval forces the native resize path.
            graphicsDeviceManager.SynchronizeWithVerticalRetrace =
                !originalSynchronization;
            graphicsDeviceManager.ApplyChanges();

            graphicsDeviceManager.SynchronizeWithVerticalRetrace =
                originalSynchronization;
            graphicsDeviceManager.ApplyChanges();
        }

        /// <summary>
        /// Minimum allowed window width.
        /// </summary>
        public const int MIN_WINDOW_WIDTH = 800;

        /// <summary>
        /// Minimum allowed window height. The height the previous minimum width implied at the
        /// shipped aspect ratio, kept as the floor now that the two axes move independently.
        /// </summary>
        public const int MIN_WINDOW_HEIGHT = 450;

        /// <summary>
        /// Current window rectangle.
        /// </summary>
        private Rectangle _windowRect;

        /// <summary>
        /// Current fullscreen display rectangle.
        /// </summary>
        private Rectangle _fullScreenRect;
    }
}
