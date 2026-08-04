using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;

using CutTheRopeDX.Commons;
using CutTheRopeDX.Desktop;
using CutTheRopeDX.Desktop.Graphics;
using CutTheRopeDX.Framework;
using CutTheRopeDX.Framework.Core;
using CutTheRopeDX.Framework.Media;
using CutTheRopeDX.Helpers;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace CutTheRopeDX
{
    /// <summary>
    /// Main game class that manages the MonoGame lifecycle, input handling, rendering, and Discord Rich Presence.
    /// </summary>
    public class Game1 : Game
    {
        /// <summary>
        /// Discord Rich Presence helper instance.
        /// </summary>
        public static RPCHelpers RPC { get; private set; }

        /// <summary>
        /// Initializes the game window, graphics device manager, and event handlers.
        /// </summary>
        public Game1()
        {
            Global.XnaGame = this;
            Content.Dispose();
            Content = new DesktopContentManager(Services);
            Global.GraphicsDeviceManager = new GraphicsDeviceManager(this);
            try
            {
                Global.GraphicsDeviceManager.GraphicsProfile = GraphicsProfile.HiDef;
                Global.GraphicsDeviceManager.ApplyChanges();
            }
            catch (Exception)
            {
                Global.GraphicsDeviceManager.GraphicsProfile = GraphicsProfile.Reach;
                Global.GraphicsDeviceManager.ApplyChanges();
            }
            // Use borderless fullscreen instead of hardware mode switch to prevent display resolution changes
            Global.GraphicsDeviceManager.HardwareModeSwitch = false;
            PresizeSwapchain();
            Global.GraphicsDeviceManager.PreparingDeviceSettings += GraphicsDeviceManager_PreparingDeviceSettings;
            TargetElapsedTime = TimeSpan.FromTicks(166666L);
            IsFixedTimeStep = false;
            InactiveSleepTime = TimeSpan.FromTicks(500000L);
            IsMouseVisible = false;
            Activated += Game1_Activated;
            Deactivated += Game1_Deactivated;
            Exiting += Game1_Exiting;
        }

        /// <summary>
        /// Sizes the preferred back buffer to the saved windowed dimensions before the graphics device is
        /// created, so the swapchain is born at its final size. Without this the device starts at MonoGame's
        /// default size and the first window sizing in <see cref="LoadContent"/> rebuilds the swapchain,
        /// flashing black during launch. Skipped when the last session was fullscreen, which is sized later.
        /// </summary>
        private static void PresizeSwapchain()
        {
            Preferences.LoadPreferences();
            if (Preferences.GetBooleanForKey("PREFS_WINDOW_FULLSCREEN"))
            {
                return;
            }
            int savedWidth = Preferences.GetIntForKey("PREFS_WINDOW_WIDTH");
            int displayWidth = GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Width;
            int width = ScreenSizeManager.ClampWindowWidth(savedWidth, displayWidth);
            Global.GraphicsDeviceManager.PreferredBackBufferWidth = width;
            Global.GraphicsDeviceManager.PreferredBackBufferHeight = Global.ScreenSizeManager.ScaledGameHeight(width);
        }

        /// <summary>
        /// Returns the current mouse state captured during the last update.
        /// </summary>
        /// <returns>The most recently polled <see cref="MouseState"/>.</returns>
        public MouseState GetMouseState()
        {
            return _currentMouseState;
        }

        /// <summary>
        /// Disables the depth-stencil buffer since the game is 2D only.
        /// </summary>
        /// <param name="sender">The graphics device manager raising the event.</param>
        /// <param name="e">Event arguments containing the device settings being prepared.</param>
        private void GraphicsDeviceManager_PreparingDeviceSettings(object sender, PreparingDeviceSettingsEventArgs e)
        {
            e.GraphicsDeviceInformation.PresentationParameters.DepthStencilFormat = DepthFormat.None;
        }

        /// <summary>
        /// Handles window resize events, adjusting the viewport while ignoring fullscreen and minimized states.
        /// </summary>
        /// <param name="sender">The window raising the event.</param>
        /// <param name="e">Event arguments for the size change.</param>
        private void Window_ClientSizeChanged(object sender, EventArgs e)
        {
            // Ignore size changes when in fullscreen mode
            if (Global.ScreenSizeManager != null && Global.ScreenSizeManager.IsFullScreen)
            {
                return;
            }

            // Ignore size changes when window is minimized
            const int MinimizedThreshold = 90;
            if (Window.ClientBounds.Width < MinimizedThreshold && Window.ClientBounds.Height < MinimizedThreshold)
            {
                return;
            }

            Window.ClientSizeChanged -= Window_ClientSizeChanged;
            Global.ScreenSizeManager.FixWindowSize(Window.ClientBounds);
            Window.ClientSizeChanged += Window_ClientSizeChanged;
        }

        /// <summary>
        /// Saves preferences and disposes resources when the game is closing.
        /// </summary>
        /// <param name="sender">The game instance raising the event.</param>
        /// <param name="e">Event arguments for the exit notification.</param>
        private void Game1_Exiting(object sender, EventArgs e)
        {
            UpdateChecker.Cancel();
            Preferences.RequestSave();
            Preferences.Update();
            //Dispose of RPC
            RPC?.Dispose();
            Global.MouseCursor?.Dispose();
        }

        /// <summary>
        /// Pauses the game when the window loses focus.
        /// </summary>
        /// <param name="sender">The game instance raising the event.</param>
        /// <param name="e">Event arguments for the deactivation notification.</param>
        private void Game1_Deactivated(object sender, EventArgs e)
        {
            _ignoreMouseClick = 60;
            CtrRenderer.Java_com_zeptolab_ctr_CtrRenderer_nativePause();
        }

        /// <summary>
        /// Resumes the game when the window regains focus.
        /// </summary>
        /// <param name="sender">The game instance raising the event.</param>
        /// <param name="e">Event arguments for the activation notification.</param>
        private void Game1_Activated(object sender, EventArgs e)
        {
            CtrRenderer.Java_com_zeptolab_ctr_CtrRenderer_nativeResume();
        }

        /// <inheritdoc />
        protected override void Initialize()
        {
            //Create RPC helper instance
            RPC = new RPCHelpers();
            string version =
                Assembly.GetExecutingAssembly()
                    .GetCustomAttribute<AssemblyInformationalVersionAttribute>()
                    ?.InformationalVersion
                ?? Assembly.GetExecutingAssembly().GetName().Version?.ToString()
                ?? "Unknown";
            Window.Title = $"Cut The Rope: DX v{version}";
            base.Initialize();
        }

        /// <inheritdoc />
        protected override void LoadContent()
        {
            Global.GraphicsDevice = GraphicsDevice;
            Global.SpriteBatch = new SpriteBatch(GraphicsDevice);

            // Initialize FontManager for FontStashSharp fonts
            Framework.Visual.FontManager.Initialize(GraphicsDevice);

            Renderer.Init();
            Global.MouseCursor.Load(Content);
            Window.AllowUserResizing = true;

            // Preferences are loaded again inside CtrBootstrap; LoadPreferences is idempotent and
            // the window-size read has to happen before ScreenSizeManager.Init.
            Preferences.LoadPreferences();
            int windowWidthPref = Preferences.GetIntForKey("PREFS_WINDOW_WIDTH");
            bool isFullScreen = Preferences.GetBooleanForKey("PREFS_WINDOW_FULLSCREEN");
            Global.ScreenSizeManager.Init(GraphicsAdapter.DefaultAdapter.CurrentDisplayMode, windowWidthPref, isFullScreen);
            Window.ClientSizeChanged += Window_ClientSizeChanged;

            CtrBootstrap.Initialize(
                new Framework.Platform.DesktopAssetPlatform(),
                Content,
                Global.ScreenSizeManager.WindowWidth,
                Global.ScreenSizeManager.WindowHeight,
                GetSystemLanguage());
        }

        /// <inheritdoc />
        protected override void UnloadContent()
        {
        }

        /// <summary>
        /// Returns the system language detected from the current culture.
        /// </summary>
        /// <returns>The <see cref="Language"/> matching the current system culture.</returns>
        private static Language GetSystemLanguage()
        {
            return LanguageHelper.FromSystemCulture();
        }

        /// <summary>
        /// Returns whether the specified key was just pressed this frame (not held).
        /// </summary>
        /// <param name="key">The key to check.</param>
        /// <returns><see langword="true"/> if <paramref name="key"/> transitioned from up to down this frame; otherwise <see langword="false"/>.</returns>
        public bool IsKeyPressed(Keys key)
        {
            _ = keyState.TryGetValue(key, out bool value);
            bool flag = keyboardStateXna.IsKeyDown(key);
            keyState[key] = flag;
            return flag && value != flag;
        }

        /// <summary>
        /// Returns whether the specified key is currently held down.
        /// </summary>
        /// <param name="key">The key to check.</param>
        /// <returns><see langword="true"/> if <paramref name="key"/> is currently held; otherwise <see langword="false"/>.</returns>
        public bool IsKeyDown(Keys key)
        {
            return keyboardStateXna.IsKeyDown(key);
        }

        /// <inheritdoc />
        protected override void Update(GameTime gameTime)
        {
            _frameWorkTimer.Start();
            try
            {
                UpdateFrame(gameTime);
            }
            finally
            {
                _frameWorkTimer.Stop();
            }
        }

        /// <summary>
        /// Advances input, audio and the game runtime by one fixed step.
        /// </summary>
        /// <param name="gameTime">Timing state for the step.</param>
        private void UpdateFrame(GameTime gameTime)
        {
            SoundMgr.Update(gameTime.ElapsedGameTime);
            KeyboardState keyboardState = Keyboard.GetState();
            HandleFullscreenToggle(keyboardState);
            elapsedTime += gameTime.ElapsedGameTime;
            if (elapsedTime > TimeSpan.FromSeconds(1))
            {
                elapsedTime -= TimeSpan.FromSeconds(1);
                Preferences.Update();
            }
            // Always fixed. The runtime advances a hardcoded 16ms per update (see CtrRenderer.Update), so
            // game time only tracks wall time because the fixed step runs the update again on any frame that
            // overran. Letting this go variable would leave the game running at whatever fraction of real
            // speed the frame rate happened to be.
            IsFixedTimeStep = true;
            keyboardStateXna = Keyboard.GetState();

            if (IsKeyPressed(Keys.F3))
            {
                RenderDiagnostics.Toggle();
            }

            if (IsKeyPressed(Keys.Escape) || GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed)
            {
                Application.SharedMovieMgr().Stop();
                _ = CtrRenderer.Java_com_zeptolab_ctr_CtrRenderer_nativeBackPressed();
            }
            MouseState newMouseState = Mouse.GetState();

            // Handle mouse wheel scrolling
            // Detects changes in scroll wheel position and forwards delta to root controller
            // ScrollWheelValue accumulates over time, so we calculate the delta between frames
            if (_currentMouseState.ScrollWheelValue != newMouseState.ScrollWheelValue)
            {
                int scrollDelta = newMouseState.ScrollWheelValue - _currentMouseState.ScrollWheelValue;
                _ = Application.SharedRootController().HandleMouseWheel(scrollDelta);
            }

            _currentMouseState = newMouseState;
            CtrRenderer.Java_com_zeptolab_ctr_CtrRenderer_nativeTouchProcess(Global.MouseCursor.GetTouchLocation());
            MouseState mouseState = Desktop.MouseCursor.GetMouseState();
            _ = Application.SharedRootController().MouseMoved(CtrRenderer.TransformX(mouseState.X), CtrRenderer.TransformY(mouseState.Y));
            CtrRenderer.Update();
            base.Update(gameTime);
        }

        /// <summary>
        /// Toggles fullscreen mode when <i>Alt</i> + <i>Enter</i> or <i>F11</i> is pressed.
        /// </summary>
        /// <param name="keyboardState">Current keyboard state.</param>
        private void HandleFullscreenToggle(KeyboardState keyboardState)
        {
            bool altDown = keyboardState.IsKeyDown(Keys.LeftAlt) || keyboardState.IsKeyDown(Keys.RightAlt);
            bool enterDown = keyboardState.IsKeyDown(Keys.Enter);
            bool f11Down = keyboardState.IsKeyDown(Keys.F11);
            bool altEnterDown = altDown && enterDown;

            bool shouldToggleFullscreen = (altEnterDown && !_altEnterPressed) || (f11Down && !_f11Pressed);
            _altEnterPressed = altEnterDown;
            _f11Pressed = f11Down;

            if (shouldToggleFullscreen)
            {
                Global.ScreenSizeManager.ToggleFullScreen();
            }
        }

        /// <summary>
        /// Renders the current video frame to the screen, stopping playback on mouse click.
        /// </summary>
        public void DrawMovie()
        {
            Renderer.FlushQuads();
            _DrawMovie = true;
            // A movie frame replaces the scene, so the render target is discarded for this frame and the
            // back buffer is drawn to directly. Bind and clear it up front: that leaves the early returns
            // below showing black instead of whatever the swapchain hands back, and costs one clear rather
            // than the two this used to do on the frames that reach the draw.
            Global.GraphicsDevice.SetRenderTarget(null);
            GraphicsDevice.Clear(Color.Black);
            if (!Application.SharedMovieMgr().IsTextureReady())
            {
                return;
            }
            Texture2D texture = Application.SharedMovieMgr().GetTexture();
            if (texture == null)
            {
                return;
            }
            if (_ignoreMouseClick > 0)
            {
                _ignoreMouseClick--;
            }
            else
            {
                MouseState mouseState = Global.XnaGame.GetMouseState();
                if (mouseState.LeftButton == ButtonState.Pressed && Global.ScreenSizeManager.CurrentSize.Contains(mouseState.X, mouseState.Y))
                {
                    Application.SharedMovieMgr().Stop();
                }
            }
            Global.ScreenSizeManager.FullScreenCropWidth = false;
            Global.ScreenSizeManager.ApplyViewportToDevice();
            Rectangle destinationRectangle = new(0, 0, GraphicsDevice.Viewport.Width, GraphicsDevice.Viewport.Height);
            Global.SpriteBatch.Begin(SpriteSortMode.Deferred, null, null, null, null, null, null);
            Global.SpriteBatch.Draw(texture, destinationRectangle, Color.White);
            Global.SpriteBatch.End();
            BlendParams.InvalidateDeviceCache();
        }

        /// <inheritdoc />
        protected override void Draw(GameTime gameTime)
        {
            _frameWorkTimer.Start();
            try
            {
                DrawFrame(gameTime);
            }
            finally
            {
                _frameWorkTimer.Stop();
                RecordFrameWork();
            }
        }

        /// <summary>
        /// Feeds the frame's measured work to the adaptive render scale and starts the next measurement.
        /// </summary>
        /// <remarks>
        /// The measurement spans the updates and the draw, and deliberately not the wait that follows. A
        /// frame that finishes early still blocks until the presentation interval lets it through, so timing
        /// anything that includes the present would read every cheap frame as a full one and walk the
        /// resolution down for no reason.
        /// <para>
        /// A movie replaces the scene, so its frames describe a cost the divisor has no bearing on; they are
        /// dropped rather than measured, along with whatever partial window preceded them.
        /// </para>
        /// </remarks>
        private void RecordFrameWork()
        {
            SoftwareRenderScale scale = SoftwareRenderScale.Shared;
            if (_DrawMovie || bFirstFrame)
            {
                scale.Reset();
            }
            else if (GraphicsFallback.IsSoftwareRendering)
            {
                scale.RecordFrame(_frameWorkTimer.Elapsed.TotalMilliseconds);
            }
            _frameWorkTimer.Reset();
        }

        /// <summary>
        /// Draws one frame of the game, or the movie that replaces it.
        /// </summary>
        /// <param name="gameTime">Timing state for the frame.</param>
        private void DrawFrame(GameTime gameTime)
        {
            Global.ScreenSizeManager.FullScreenCropWidth = true;
            Global.ScreenSizeManager.ApplyViewportToDevice();
            _DrawMovie = false;
            Renderer.BeginFrame();
            // Bind the scene target before the frame draws, so the engine's own clear lands on it rather
            // than on the back buffer. Nothing clears the back buffer here any more: every path below
            // paints all of it, either the opaque blit in CopyFromRenderTargetToScreen, the first-frame
            // clear, or DrawMovie.
            Renderer.BindSceneTarget();
            try
            {
                CtrRenderer.OnDrawFrame();
            }
            finally
            {
                Renderer.EndFrame();
            }
            Global.MouseCursor.Draw();
            Global.GraphicsDevice.SetRenderTarget(null);
            if (bFirstFrame)
            {
                GraphicsDevice.Clear(Color.Black);
            }
            else if (!_DrawMovie)
            {
                Renderer.CopyFromRenderTargetToScreen();
            }
            base.Draw(gameTime);
            bFirstFrame = false;
        }

        /// <summary>
        /// Whether <i>Alt</i> + <i>Enter</i> was held on the previous frame.
        /// </summary>
        private bool _altEnterPressed;

        /// <summary>
        /// Whether <i>F11</i> was held on the previous frame.
        /// </summary>
        private bool _f11Pressed;

        /// <summary>
        /// Mouse state captured during the current frame.
        /// </summary>
        private MouseState _currentMouseState;

        /// <summary>
        /// Tracks previous-frame key states for edge detection in <see cref="IsKeyPressed"/>.
        /// </summary>
        private readonly Dictionary<Keys, bool> keyState = [];

        /// <summary>
        /// Current keyboard state used for input polling.
        /// </summary>
        private KeyboardState keyboardStateXna;

        /// <summary>
        /// Whether a movie is currently being drawn instead of the game scene.
        /// </summary>
        private bool _DrawMovie;

        /// <summary>
        /// Accumulates the wall-clock time one displayed frame spends updating and drawing.
        /// </summary>
        /// <remarks>
        /// Accumulated rather than sampled once, because the fixed step runs the update more than once per
        /// drawn frame whenever the previous frame overran, and all of those updates are work that frame
        /// cost.
        /// </remarks>
        private readonly Stopwatch _frameWorkTimer = new();

        /// <summary>
        /// Remaining frames to ignore mouse clicks after the window regains focus.
        /// </summary>
        private int _ignoreMouseClick;

        /// <summary>
        /// Accumulated elapsed time used to drive the once-a-second preference flush.
        /// </summary>
        private TimeSpan elapsedTime = TimeSpan.Zero;

        /// <summary>
        /// Whether the first frame has not yet been rendered.
        /// </summary>
        private bool bFirstFrame = true;
    }
}
