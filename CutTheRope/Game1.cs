using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Reflection;
using System.Threading;

using CutTheRope.Commons;
using CutTheRope.Desktop;
using CutTheRope.Framework;
using CutTheRope.Framework.Core;
using CutTheRope.Framework.Media;
using CutTheRope.Helpers;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace CutTheRope
{
    public class Game1 : Game
    {
        // (get) Token: 0x06000022 RID: 34 RVA: 0x00002517 File Offset: 0x00000717
        public Game1()
        {
            Global.XnaGame = this;
            Content.RootDirectory = ContentPaths.RootDirectory;
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
            Global.GraphicsDeviceManager.PreparingDeviceSettings += GraphicsDeviceManager_PreparingDeviceSettings;
            TargetElapsedTime = TimeSpan.FromTicks(166666L);
            IsFixedTimeStep = false;
            InactiveSleepTime = TimeSpan.FromTicks(500000L);
            IsMouseVisible = false;
            Activated += Game1_Activated;
            Deactivated += Game1_Deactivated;
            Exiting += Game1_Exiting;
        }

        public MouseState GetMouseState()
        {
            return _currentMouseState;
        }

        private void GraphicsDeviceManager_PreparingDeviceSettings(object sender, PreparingDeviceSettingsEventArgs e)
        {
            e.GraphicsDeviceInformation.PresentationParameters.DepthStencilFormat = DepthFormat.None;
            if (e.GraphicsDeviceInformation.Adapter.CurrentDisplayMode.Width > ScreenSizeManager.MAX_WINDOW_WIDTH || e.GraphicsDeviceInformation.Adapter.CurrentDisplayMode.Height > ScreenSizeManager.MAX_WINDOW_WIDTH)
            {
                UseWindowMode_TODO_ChangeFullScreenResolution = true;
            }
        }

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

        private void Game1_Exiting(object sender, EventArgs e)
        {
            Preferences.RequestSave();
            Preferences.Update();
        }

        private void Game1_Deactivated(object sender, EventArgs e)
        {
            _ignoreMouseClick = 60;
            CtrRenderer.Java_com_zeptolab_ctr_CtrRenderer_nativePause();
        }

        private void Game1_Activated(object sender, EventArgs e)
        {
            CtrRenderer.Java_com_zeptolab_ctr_CtrRenderer_nativeResume();
        }

        protected override void Initialize()
        {
            string version = FileVersionInfo.GetVersionInfo(Assembly.GetExecutingAssembly().Location)
                .FileVersion ?? "Unknown";
            Window.Title = $"Cut The Rope: DX v{version}";
            base.Initialize();
        }

        protected override void LoadContent()
        {
            Global.GraphicsDevice = GraphicsDevice;
            Global.SpriteBatch = new SpriteBatch(GraphicsDevice);
            SoundMgr.SetContentManager(Content);

            // Initialize FontManager for FontStashSharp fonts
            Framework.Visual.FontManager.Initialize(GraphicsDevice);

            OpenGL.Init();
            Global.MouseCursor.Load(Content);
            Window.AllowUserResizing = UseWindowMode_TODO_ChangeFullScreenResolution || true;
            Preferences.LoadPreferences();
            int num = Preferences.GetIntForKey("PREFS_WINDOW_WIDTH");
            bool isFullScreen = !UseWindowMode_TODO_ChangeFullScreenResolution && (num <= 0 || Preferences.GetBooleanForKey("PREFS_WINDOW_FULLSCREEN"));
            Global.ScreenSizeManager.Init(GraphicsAdapter.DefaultAdapter.CurrentDisplayMode, num, isFullScreen);
            Window.ClientSizeChanged += Window_ClientSizeChanged;
            CtrRenderer.Java_com_zeptolab_ctr_CtrRenderer_nativeInit(GetSystemLanguage());
            CtrRenderer.OnSurfaceCreated();
            CtrRenderer.OnSurfaceChanged(Global.ScreenSizeManager.WindowWidth, Global.ScreenSizeManager.WindowHeight);
            branding = new Branding();
            branding.LoadSplashScreens();
        }

        protected override void UnloadContent()
        {
        }

        private static Language GetSystemLanguage()
        {
            Language result = Language.LANGEN;
            if (CultureInfo.CurrentCulture.TwoLetterISOLanguageName == "ru")
            {
                result = Language.LANGRU;
            }
            if (CultureInfo.CurrentCulture.TwoLetterISOLanguageName == "de")
            {
                result = Language.LANGDE;
            }
            if (CultureInfo.CurrentCulture.TwoLetterISOLanguageName == "fr")
            {
                result = Language.LANGFR;
            }
            return result;
        }

        public bool IsKeyPressed(Keys key)
        {
            _ = keyState.TryGetValue(key, out bool value);
            bool flag = keyboardStateXna.IsKeyDown(key);
            keyState[key] = flag;
            return flag && value != flag;
        }

        public bool IsKeyDown(Keys key)
        {
            return keyboardStateXna.IsKeyDown(key);
        }

        protected override void Update(GameTime gameTime)
        {
            KeyboardState keyboardState = Keyboard.GetState();
            bool flag = keyboardState.IsKeyDown(Keys.LeftAlt) || keyboardState.IsKeyDown(Keys.RightAlt);
            bool enterPressed = keyboardState.IsKeyDown(Keys.Enter);
            if (flag && enterPressed)
            {
                if (!_altEnterPressed)
                {
                    Global.ScreenSizeManager.ToggleFullScreen();
                    _altEnterPressed = true;
                }
            }
            else
            {
                _altEnterPressed = false;
            }
            elapsedTime += gameTime.ElapsedGameTime;
            if (elapsedTime > TimeSpan.FromSeconds(1.0))
            {
                elapsedTime -= TimeSpan.FromSeconds(1.0);
                frameRate = frameCounter;
                frameCounter = 0;
                Preferences.Update();
            }
            IsFixedTimeStep = (frameRate > 0 && frameRate < 50) || true;
            keyboardStateXna = Keyboard.GetState();
            if ((IsKeyPressed(Keys.F11) || ((IsKeyDown(Keys.LeftAlt) || IsKeyDown(Keys.RightAlt)) && IsKeyPressed(Keys.Enter))) && !UseWindowMode_TODO_ChangeFullScreenResolution)
            {
                Global.ScreenSizeManager.ToggleFullScreen();
                Thread.Sleep(500);
                return;
            }
            if (branding != null)
            {
                if (IsActive && branding.IsLoaded)
                {
                    if (branding.IsFinished)
                    {
                        branding = null;
                        return;
                    }
                    branding.Update(gameTime);
                }
                return;
            }
            if (IsKeyPressed(Keys.Escape) || GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed)
            {
                Application.SharedMovieMgr().Stop();
                _ = CtrRenderer.Java_com_zeptolab_ctr_CtrRenderer_nativeBackPressed();
            }
            _currentMouseState = Mouse.GetState();
            CtrRenderer.Java_com_zeptolab_ctr_CtrRenderer_nativeTouchProcess(Global.MouseCursor.GetTouchLocation());
            MouseState mouseState = Desktop.MouseCursor.GetMouseState();
            _ = Application.SharedRootController().MouseMoved(CtrRenderer.TransformX(mouseState.X), CtrRenderer.TransformY(mouseState.Y));
            CtrRenderer.Update(gameTime.ElapsedGameTime.Milliseconds / 1000f);
            base.Update(gameTime);
        }

        public void DrawMovie()
        {
            _DrawMovie = true;
            GraphicsDevice.Clear(Color.Black);
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
            Global.GraphicsDevice.SetRenderTarget(null);
            GraphicsDevice.Clear(Color.Black);
            Global.ScreenSizeManager.FullScreenCropWidth = false;
            Global.ScreenSizeManager.ApplyViewportToDevice();
            Rectangle destinationRectangle = new(0, 0, GraphicsDevice.Viewport.Width, GraphicsDevice.Viewport.Height);
            Global.SpriteBatch.Begin(SpriteSortMode.Deferred, null, null, null, null, null, null);
            Global.SpriteBatch.Draw(texture, destinationRectangle, Color.White);
            Global.SpriteBatch.End();
        }

        protected override void Draw(GameTime gameTime)
        {
            frameCounter++;
            GraphicsDevice.Clear(Color.Black);
            if (branding != null)
            {
                if (branding.IsLoaded)
                {
                    branding.Draw(gameTime);
                    Global.GraphicsDevice.SetRenderTarget(null);
                }
                return;
            }
            Global.ScreenSizeManager.FullScreenCropWidth = true;
            Global.ScreenSizeManager.ApplyViewportToDevice();
            _DrawMovie = false;
            CtrRenderer.OnDrawFrame();
            Global.MouseCursor.Draw();
            Global.GraphicsDevice.SetRenderTarget(null);
            if (bFirstFrame)
            {
                GraphicsDevice.Clear(Color.Black);
            }
            else if (!_DrawMovie)
            {
                OpenGL.CopyFromRenderTargetToScreen();
            }
            base.Draw(gameTime);
            bFirstFrame = false;
        }

        private Branding branding;

        private bool _altEnterPressed;

        private MouseState _currentMouseState;

        private bool UseWindowMode_TODO_ChangeFullScreenResolution = true;

        private readonly Dictionary<Keys, bool> keyState = [];

        private KeyboardState keyboardStateXna;

        private bool _DrawMovie;

        private int _ignoreMouseClick;

        private int frameRate;

        private int frameCounter;

        private TimeSpan elapsedTime = TimeSpan.Zero;

        private bool bFirstFrame = true;
    }
}
