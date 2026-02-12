using System;

using CutTheRopeDX.Framework.Core;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace CutTheRopeDX.Desktop
{
    /// <summary>
    /// Manages window, fullscreen, and scaled-view sizing for the desktop renderer.
    /// Handles aspect-ratio preservation, viewport updates, coordinate transforms, and persisted window settings.
    /// </summary>
    /// <param name="gameWidth">Logical game width.</param>
    /// <param name="gameHeight">Logical game height.</param>
    internal sealed class ScreenSizeManager(int gameWidth, int gameHeight)
    {
        /// <summary>
        /// Maximum allowed window width for the active graphics profile.
        /// </summary>
        public static int MAX_WINDOW_WIDTH => Global.GraphicsDeviceManager.GraphicsProfile == GraphicsProfile.HiDef ? 4096 : 2048;

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
        /// Gets the logical game width.
        /// </summary>
        public int GameWidth { get; } = gameWidth;

        /// <summary>
        /// Gets the logical game height.
        /// </summary>
        public int GameHeight { get; } = gameHeight;

        /// <summary>
        /// Gets the letterboxed or pillarboxed view rectangle used for rendering the game.
        /// </summary>
        public Rectangle ScaledViewRect => _scaledViewRect;

        /// <summary>
        /// Gets the scaled render rectangle in physical pixels for the current backing scale.
        /// </summary>
        public Rectangle ScaledViewRectPixels => _scaledViewRectPixels;

        /// <summary>
        /// Gets the current backing scale reported by the platform.
        /// </summary>
        public double BackingScale => _backingScaleState.CurrentScale;

        /// <summary>
        /// Gets the effective display scale after logical scaling and backing-scale conversion.
        /// </summary>
        public double EffectiveDisplayScale
        {
            get
            {
                double logicalScale = _scaledViewRect.Width > 0 ? _scaledViewRect.Width / (double)GameWidth : 1d;
                return logicalScale * BackingScale;
            }
        }

        /// <summary>
        /// Gets the current output surface width in physical pixels.
        /// </summary>
        public int SurfaceWidthPixels => Math.Max(1, (int)Math.Round(CurrentSize.Width * BackingScale, MidpointRounding.AwayFromZero));

        /// <summary>
        /// Gets the current output surface height in physical pixels.
        /// </summary>
        public int SurfaceHeightPixels => Math.Max(1, (int)Math.Round(CurrentSize.Height * BackingScale, MidpointRounding.AwayFromZero));

        /// <summary>
        /// Gets a value indicating whether size-change reactions are temporarily disabled.
        /// </summary>
        public bool SkipSizeChanges { get; private set; }

        /// <summary>
        /// Sets whether fullscreen view scaling should crop width instead of fitting the full game width.
        /// </summary>
        public bool FullScreenCropWidth
        {
            set
            {
                if (_fullScreenCropWidth != value)
                {
                    _fullScreenCropWidth = value;
                    UpdateScaledView();
                }
            }
        }

        /// <summary>
        /// Gets the horizontal scale factor from logical game width to the current scaled view width.
        /// </summary>
        public double WidthAspectRatio => _scaledViewRect.Width / (double)GameWidth;

        /// <summary>
        /// Converts a window-space X coordinate into scaled-view space.
        /// </summary>
        /// <param name="x">Window-space X coordinate.</param>
        /// <returns>Scaled-view X coordinate.</returns>
        public int TransformWindowToViewX(int x)
        {
            return x - _scaledViewRect.X;
        }

        /// <summary>
        /// Converts a window-space Y coordinate into scaled-view space.
        /// </summary>
        /// <param name="y">Window-space Y coordinate.</param>
        /// <returns>Scaled-view Y coordinate.</returns>
        public int TransformWindowToViewY(int y)
        {
            return y - _scaledViewRect.Y;
        }

        /// <summary>
        /// Converts a scaled-view X coordinate into logical game space.
        /// </summary>
        /// <param name="x">Scaled-view X coordinate.</param>
        /// <returns>Logical game-space X coordinate.</returns>
        public float TransformViewToGameX(float x)
        {
            return x * GameWidth / _scaledViewRect.Width;
        }

        /// <summary>
        /// Converts a scaled-view Y coordinate into logical game space.
        /// </summary>
        /// <param name="y">Scaled-view Y coordinate.</param>
        /// <returns>Logical game-space Y coordinate.</returns>
        public float TransformViewToGameY(float y)
        {
            return y * GameHeight / _scaledViewRect.Height;
        }

        /// <summary>
        /// Initializes screen sizing from the current display mode, preferred window width, and fullscreen state.
        /// </summary>
        /// <param name="displayMode">Current display mode.</param>
        /// <param name="windowWidth">Preferred window width, or a non-positive value to derive one automatically.</param>
        /// <param name="isFullScreen"><see langword="true" /> to start in fullscreen mode.</param>
        public void Init(DisplayMode displayMode, int windowWidth, bool isFullScreen)
        {
            _ = TryUpdateBackingScale();
            FullScreenRectChanged(displayMode);
            int targetWindowWidth = windowWidth > 0 ? windowWidth : displayMode.Width - 100;
            if (targetWindowWidth < 800)
            {
                targetWindowWidth = 800;
            }
            if (targetWindowWidth > MAX_WINDOW_WIDTH)
            {
                targetWindowWidth = MAX_WINDOW_WIDTH;
            }
            if (targetWindowWidth > displayMode.Width)
            {
                targetWindowWidth = displayMode.Width;
            }
            WindowRectChanged(new Rectangle(0, 0, targetWindowWidth, ScaledGameHeight(targetWindowWidth)));
            if (isFullScreen)
            {
                ToggleFullScreen();
                return;
            }
            ApplyWindowSize(WindowWidth);
        }

        /// <summary>
        /// Refreshes the backing scale and reapplies the viewport if the reported scale changed.
        /// </summary>
        /// <returns><see langword="true" /> when the scale changed and the viewport was updated.</returns>
        public bool RefreshBackingScaleIfChanged()
        {
            if (!TryUpdateBackingScale())
            {
                return false;
            }

            ApplyViewportToDevice();
            return true;
        }

        /// <summary>
        /// Returns the logical game width that preserves aspect ratio for the supplied scaled height.
        /// </summary>
        /// <param name="scaledHeight">Scaled view height.</param>
        /// <returns>Aspect-ratio-correct game width.</returns>
        public int ScaledGameWidth(int scaledHeight)
        {
            return (int)((scaledHeight / _gameAspectRatio) + 0.5);
        }

        /// <summary>
        /// Returns the logical game height that preserves aspect ratio for the supplied scaled width.
        /// </summary>
        /// <param name="scaledWidth">Scaled view width.</param>
        /// <returns>Aspect-ratio-correct game height.</returns>
        public int ScaledGameHeight(int scaledWidth)
        {
            return (int)((scaledWidth * _gameAspectRatio) + 0.5);
        }

        /// <summary>
        /// Recomputes the scaled render rectangle for the current window or fullscreen bounds.
        /// </summary>
        private void UpdateScaledView()
        {
            if (SkipSizeChanges)
            {
                return;
            }
            // Always use fullscreen-style letterboxing/pillarboxing for both modes
            Rectangle sourceRect = IsFullScreen ? _fullScreenRect : _windowRect;
            if (sourceRect.Width >= sourceRect.Height)
            {
                int scaledHeight = _fullScreenCropWidth ? sourceRect.Height : ScaledGameHeight(sourceRect.Width);
                int scaledWidth = _fullScreenCropWidth ? ScaledGameWidth(scaledHeight) : sourceRect.Width;
                _scaledViewRect = new Rectangle((sourceRect.Width - scaledWidth) / 2, (sourceRect.Height - scaledHeight) / 2, scaledWidth, scaledHeight);
                _scaledViewRectPixels = BackingScaleMath.LogicalToPixelRect(_scaledViewRect, BackingScale);
                return;
            }
            int portraitScaledHeight = _fullScreenCropWidth ? (int)(sourceRect.Width / 5f * 4f) : ScaledGameHeight(sourceRect.Width);
            int portraitScaledWidth = _fullScreenCropWidth ? ScaledGameWidth(portraitScaledHeight) : sourceRect.Width;
            _scaledViewRect = new Rectangle((sourceRect.Width - portraitScaledWidth) / 2, (sourceRect.Height - portraitScaledHeight) / 2, portraitScaledWidth, portraitScaledHeight);
            _scaledViewRectPixels = BackingScaleMath.LogicalToPixelRect(_scaledViewRect, BackingScale);
        }

        /// <summary>
        /// Applies a new window back-buffer <paramref name="width"/> and updates the tracked window rectangle.
        /// </summary>
        /// <param name="width">Target window width.</param>
        public void ApplyWindowSize(int width)
        {
            GraphicsDeviceManager graphicsDeviceManager = Global.GraphicsDeviceManager;
            graphicsDeviceManager.PreferredBackBufferWidth = width;
            graphicsDeviceManager.PreferredBackBufferHeight = ScaledGameHeight(width);
            graphicsDeviceManager.ApplyChanges();
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
            bool fullScreenCropWidth = _fullScreenCropWidth;
            FullScreenCropWidth = true;
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
            graphicsDeviceManager.ApplyChanges();
            ApplyViewportToDevice();
            FullScreenCropWidth = fullScreenCropWidth;
            SkipSizeChanges = false;
            EnableFullScreen(!isFullScreen);
            Save();
            Application.SharedCanvas().Reshape();
            Application.SharedRootController().FullscreenToggled(!isFullScreen);
        }

        /// <summary>
        /// Normalizes window size changes to the game's aspect-ratio constraints and persists the result.
        /// </summary>
        /// <param name="newWindowRect">New window bounds reported by the host window.</param>
        public void FixWindowSize(Rectangle newWindowRect)
        {
            if (SkipSizeChanges)
            {
                return;
            }
            GraphicsDeviceManager graphicsDeviceManager = Global.GraphicsDeviceManager;
            FullScreenRectChanged(GraphicsAdapter.DefaultAdapter.CurrentDisplayMode);
            if (!IsFullScreen)
            {
                try
                {
                    int targetWidth = graphicsDeviceManager.PreferredBackBufferWidth;
                    if (newWindowRect.Width != WindowWidth)
                    {
                        targetWidth = newWindowRect.Width;
                    }
                    else if (newWindowRect.Height != WindowHeight)
                    {
                        targetWidth = ScaledGameWidth(newWindowRect.Height);
                    }
                    if (targetWidth < 800 || ScaledGameHeight(targetWidth) < ScaledGameHeight(800))
                    {
                        targetWidth = 800;
                    }
                    if (targetWidth > MAX_WINDOW_WIDTH)
                    {
                        targetWidth = MAX_WINDOW_WIDTH;
                    }
                    if (targetWidth > ScreenWidth)
                    {
                        targetWidth = ScreenWidth;
                    }
                    ApplyWindowSize(targetWidth);
                }
                catch (Exception)
                {
                }
            }
            Save();
            Application.SharedCanvas().Reshape();
        }

        /// <summary>
        /// Applies the current scaled view rectangle to the graphics device viewport.
        /// </summary>
        public void ApplyViewportToDevice()
        {
            Rectangle boundsLogical = !IsFullScreen ? Rectangle.Intersect(_scaledViewRect, _windowRect) : Rectangle.Intersect(_scaledViewRect, _fullScreenRect);
            Rectangle boundsPixels = BackingScaleMath.LogicalToPixelRect(boundsLogical, BackingScale);
            try
            {
                if (Global.GraphicsDevice == null)
                {
                    return;
                }

                PresentationParameters presentationParameters = Global.GraphicsDevice.PresentationParameters;
                Rectangle bounds = BackingScaleMath.ResolvePresentDestinationRect(
                    boundsLogical,
                    boundsPixels,
                    presentationParameters.BackBufferWidth,
                    presentationParameters.BackBufferHeight,
                    BackingScale);

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
        /// Refreshes the cached backing scale from the current platform provider.
        /// </summary>
        /// <returns><see langword="true" /> when the scale changed.</returns>
        private bool TryUpdateBackingScale()
        {
            double candidateScale;
            try
            {
                if (!_backingScaleProvider.TryGetCurrentScale(out double reportedScale))
                {
                    return false;
                }

                candidateScale = reportedScale;
            }
            catch (Exception)
            {
                return false;
            }

            if (ShouldIgnoreFullscreenDownscaleToOne(
                    isFullScreen: IsFullScreen,
                    isDesktopGLProvider: _backingScaleProvider is DesktopGLBackingScaleProvider,
                    currentScale: _backingScaleState.CurrentScale,
                    candidateScale: candidateScale))
            {
                return false;
            }

            if (!_backingScaleState.TryUpdate(candidateScale))
            {
                return false;
            }

            UpdateScaledView();
            return true;
        }

        /// <summary>
        /// Ignores transient fullscreen DesktopGL reports that briefly drop from HiDPI to a scale of 1.
        /// </summary>
        internal static bool ShouldIgnoreFullscreenDownscaleToOne(
            bool isFullScreen,
            bool isDesktopGLProvider,
            double currentScale,
            double candidateScale)
        {
            const double epsilon = 0.01d;
            if (!isFullScreen || !isDesktopGLProvider)
            {
                return false;
            }

            if (currentScale <= (1d + epsilon))
            {
                return false;
            }

            double normalizedCandidate = BackingScaleMath.NormalizeScale(candidateScale);
            return Math.Abs(normalizedCandidate - 1d) <= epsilon;
        }

        /// <summary>
        /// Creates the platform-specific backing-scale provider for the current runtime.
        /// </summary>
        private static IBackingScaleProvider CreateBackingScaleProvider()
        {
            if (!OperatingSystem.IsMacOS())
            {
                return new FallbackBackingScaleProvider();
            }

#if MACOS_AVFOUNDATION
            return new MacBackingScaleProvider();
#else
            return new DesktopGLBackingScaleProvider();
#endif
        }

        /// <summary>
        /// Minimum allowed window width.
        /// </summary>
        public const int MIN_WINDOW_WIDTH = 800;

        /// <summary>
        /// Current window rectangle.
        /// </summary>
        private Rectangle _windowRect;

        /// <summary>
        /// Current fullscreen display rectangle.
        /// </summary>
        private Rectangle _fullScreenRect;

        /// <summary>
        /// Cached logical game aspect ratio.
        /// </summary>
        private readonly double _gameAspectRatio = gameHeight / (double)gameWidth;

        private readonly IBackingScaleProvider _backingScaleProvider = CreateBackingScaleProvider();
        private readonly BackingScaleState _backingScaleState = new(1d, epsilon: 0.01d, downscaleToOneConfirmationReadings: 3);

        /// <summary>
        /// Current scaled render rectangle after aspect-ratio fitting or cropping.
        /// </summary>
        private Rectangle _scaledViewRect;

        /// <summary>
        /// Current scaled render rectangle in physical pixels.
        /// </summary>
        private Rectangle _scaledViewRectPixels;

        /// <summary>
        /// Whether fullscreen scaling should crop width.
        /// </summary>
        private bool _fullScreenCropWidth = true;
    }
}
