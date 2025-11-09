using CutTheRope.ctr_commons;
using CutTheRope.iframework;
using CutTheRope.iframework.core;
using CutTheRope.iframework.media;
using CutTheRope.windows;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Globalization;
using System.Threading;
using System.Windows.Forms;

namespace CutTheRope
{
    public class Game1 : Game
    {
        // (get) Token: 0x06000022 RID: 34 RVA: 0x00002517 File Offset: 0x00000717
        private bool IsMinimized
        {
            get
            {
                return this.WindowAsForm().WindowState == FormWindowState.Minimized;
            }
        }

        public Game1()
        {
            Global.XnaGame = this;
            base.Content.RootDirectory = "content";
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
            Global.GraphicsDeviceManager.PreparingDeviceSettings += this.GraphicsDeviceManager_PreparingDeviceSettings;
            base.TargetElapsedTime = TimeSpan.FromTicks(166666L);
            base.IsFixedTimeStep = false;
            base.InactiveSleepTime = TimeSpan.FromTicks(500000L);
            base.IsMouseVisible = true;
            base.Activated += this.Game1_Activated;
            base.Deactivated += this.Game1_Deactivated;
            base.Exiting += this.Game1_Exiting;
            this.parentProcess = ParentProcessUtilities.GetParentProcess();
            Form form = this.WindowAsForm();
            form.MouseMove += this.form_MouseMove;
            form.MouseUp += this.form_MouseUp;
            form.MouseDown += this.form_MouseDown;
        }

        private void form_MouseDown(object sender, MouseEventArgs e)
        {
            this.mouseState_X = e.X;
            this.mouseState_Y = e.Y;
            MouseButtons button = e.Button;
            if (button <= MouseButtons.Right)
            {
                if (button != MouseButtons.Left)
                {
                    if (button == MouseButtons.Right)
                    {
                        this.mouseState_RightButton = Microsoft.Xna.Framework.Input.ButtonState.Pressed;
                    }
                }
                else
                {
                    this.mouseState_LeftButton = Microsoft.Xna.Framework.Input.ButtonState.Pressed;
                }
            }
            else if (button != MouseButtons.Middle)
            {
                if (button != MouseButtons.XButton1)
                {
                    if (button == MouseButtons.XButton2)
                    {
                        this.mouseState_XButton2 = Microsoft.Xna.Framework.Input.ButtonState.Pressed;
                    }
                }
                else
                {
                    this.mouseState_XButton1 = Microsoft.Xna.Framework.Input.ButtonState.Pressed;
                }
            }
            else
            {
                this.mouseState_MiddleButton = Microsoft.Xna.Framework.Input.ButtonState.Pressed;
            }
            if (this._DrawMovie && e.Button == MouseButtons.Left)
            {
                CutTheRope.iframework.core.Application.sharedMovieMgr().stop();
            }
            CtrRenderer.Java_com_zeptolab_ctr_CtrRenderer_nativeTouchProcess(Global.MouseCursor.GetTouchLocation());
        }

        private void form_MouseUp(object sender, MouseEventArgs e)
        {
            this.mouseState_X = e.X;
            this.mouseState_Y = e.Y;
            MouseButtons button = e.Button;
            if (button <= MouseButtons.Right)
            {
                if (button != MouseButtons.Left)
                {
                    if (button == MouseButtons.Right)
                    {
                        this.mouseState_RightButton = Microsoft.Xna.Framework.Input.ButtonState.Released;
                    }
                }
                else
                {
                    this.mouseState_LeftButton = Microsoft.Xna.Framework.Input.ButtonState.Released;
                }
            }
            else if (button != MouseButtons.Middle)
            {
                if (button != MouseButtons.XButton1)
                {
                    if (button == MouseButtons.XButton2)
                    {
                        this.mouseState_XButton2 = Microsoft.Xna.Framework.Input.ButtonState.Released;
                    }
                }
                else
                {
                    this.mouseState_XButton1 = Microsoft.Xna.Framework.Input.ButtonState.Released;
                }
            }
            else
            {
                this.mouseState_MiddleButton = Microsoft.Xna.Framework.Input.ButtonState.Released;
            }
            CtrRenderer.Java_com_zeptolab_ctr_CtrRenderer_nativeTouchProcess(Global.MouseCursor.GetTouchLocation());
        }

        private void form_MouseMove(object sender, MouseEventArgs e)
        {
            this.mouseState_X = e.X;
            this.mouseState_Y = e.Y;
            CtrRenderer.Java_com_zeptolab_ctr_CtrRenderer_nativeTouchProcess(Global.MouseCursor.GetTouchLocation());
        }

        public MouseState GetMouseState()
        {
            return new MouseState(this.mouseState_X, this.mouseState_Y, this.mouseState_ScrollWheelValue, this.mouseState_LeftButton, this.mouseState_MiddleButton, this.mouseState_RightButton, this.mouseState_XButton1, this.mouseState_XButton2);
        }

        private void GraphicsDeviceManager_PreparingDeviceSettings(object sender, PreparingDeviceSettingsEventArgs e)
        {
            e.GraphicsDeviceInformation.PresentationParameters.DepthStencilFormat = DepthFormat.None;
            if (e.GraphicsDeviceInformation.Adapter.CurrentDisplayMode.Width > ScreenSizeManager.MAX_WINDOW_WIDTH || e.GraphicsDeviceInformation.Adapter.CurrentDisplayMode.Height > ScreenSizeManager.MAX_WINDOW_WIDTH)
            {
                this.UseWindowMode_TODO_ChangeFullScreenResolution = true;
            }
        }

        private void form_Resize(object sender, EventArgs e)
        {
            if (Global.ScreenSizeManager.SkipSizeChanges)
            {
                return;
            }
            Form form = this.WindowAsForm();
            if (form.WindowState == FormWindowState.Maximized)
            {
                form.WindowState = FormWindowState.Normal;
                bool isFullScreen = Global.ScreenSizeManager.IsFullScreen;
            }
        }

        public void SetCursor(Cursor cursor, MouseState mouseState)
        {
            if (base.Window.ClientBounds.Contains(base.Window.ClientBounds.X + mouseState.X, base.Window.ClientBounds.Y + mouseState.Y) && this._cursorLast != cursor)
            {
                this.WindowAsForm().Cursor = cursor;
                this._cursorLast = cursor;
            }
        }

        private Form WindowAsForm()
        {
            return (Form)Control.FromHandle(base.Window.Handle);
        }

        private void Window_ClientSizeChanged(object sender, EventArgs e)
        {
            base.Window.ClientSizeChanged -= this.Window_ClientSizeChanged;
            Global.ScreenSizeManager.FixWindowSize(base.Window.ClientBounds);
            base.Window.ClientSizeChanged += this.Window_ClientSizeChanged;
        }

        private void Game1_Exiting(object sender, EventArgs e)
        {
            Preferences._savePreferences();
            Preferences.Update();
        }

        private void Game1_Deactivated(object sender, EventArgs e)
        {
            this._ignoreMouseClick = 60;
            CtrRenderer.Java_com_zeptolab_ctr_CtrRenderer_nativePause();
        }

        private void Game1_Activated(object sender, EventArgs e)
        {
            CtrRenderer.Java_com_zeptolab_ctr_CtrRenderer_nativeResume();
        }

        protected override void Initialize()
        {
            base.Initialize();
        }

        protected override void LoadContent()
        {
            Global.GraphicsDevice = base.GraphicsDevice;
            Global.SpriteBatch = new SpriteBatch(base.GraphicsDevice);
            SoundMgr.SetContentManager(base.Content);
            OpenGL.Init();
            Global.MouseCursor.Load(base.Content);
            Form form = this.WindowAsForm();
            if (this.UseWindowMode_TODO_ChangeFullScreenResolution)
            {
                base.Window.AllowUserResizing = true;
                if (form != null)
                {
                    form.MaximizeBox = false;
                }
            }
            else
            {
                base.Window.AllowUserResizing = true;
            }
            Preferences._loadPreferences();
            int num = Preferences._getIntForKey("PREFS_WINDOW_WIDTH");
            bool isFullScreen = !this.UseWindowMode_TODO_ChangeFullScreenResolution && (num <= 0 || Preferences._getBooleanForKey("PREFS_WINDOW_FULLSCREEN"));
            Global.ScreenSizeManager.Init(GraphicsAdapter.DefaultAdapter.CurrentDisplayMode, num, isFullScreen);
            base.Window.ClientSizeChanged += this.Window_ClientSizeChanged;
            if (form != null)
            {
                Global.ScreenSizeManager.SetWindowMinimumSize(form);
                form.BackColor = global::System.Drawing.Color.Black;
                form.Resize += this.form_Resize;
            }
            CtrRenderer.Java_com_zeptolab_ctr_CtrRenderer_nativeInit(this.GetSystemLanguage());
            CtrRenderer.onSurfaceCreated();
            CtrRenderer.onSurfaceChanged(Global.ScreenSizeManager.WindowWidth, Global.ScreenSizeManager.WindowHeight);
            this.branding = new Branding();
            this.branding.LoadSplashScreens();
        }

        protected override void UnloadContent()
        {
        }

        private Language GetSystemLanguage()
        {
            Language result = Language.LANG_EN;
            if (CultureInfo.CurrentCulture.TwoLetterISOLanguageName == "ru")
            {
                result = Language.LANG_RU;
            }
            if (CultureInfo.CurrentCulture.TwoLetterISOLanguageName == "de")
            {
                result = Language.LANG_DE;
            }
            if (CultureInfo.CurrentCulture.TwoLetterISOLanguageName == "fr")
            {
                result = Language.LANG_FR;
            }
            return result;
        }

        public bool IsKeyPressed(Microsoft.Xna.Framework.Input.Keys key)
        {
            bool value = false;
            this.keyState.TryGetValue(key, out value);
            bool flag = this.keyboardStateXna.IsKeyDown(key);
            this.keyState[key] = flag;
            return flag && value != flag;
        }

        public bool IsKeyDown(Microsoft.Xna.Framework.Input.Keys key)
        {
            return this.keyboardStateXna.IsKeyDown(key);
        }

        protected override void Update(GameTime gameTime)
        {
            KeyboardState keyboardState = Keyboard.GetState();
            bool flag = keyboardState.IsKeyDown(Microsoft.Xna.Framework.Input.Keys.LeftAlt) || keyboardState.IsKeyDown(Microsoft.Xna.Framework.Input.Keys.RightAlt);
            bool enterPressed = keyboardState.IsKeyDown(Microsoft.Xna.Framework.Input.Keys.Enter);
            if (flag && enterPressed)
            {
                if (!this._altEnterPressed)
                {
                    Global.ScreenSizeManager.ToggleFullScreen();
                    this._altEnterPressed = true;
                }
            }
            else
            {
                this._altEnterPressed = false;
            }
            this.elapsedTime += gameTime.ElapsedGameTime;
            if (this.elapsedTime > TimeSpan.FromSeconds(1.0))
            {
                this.elapsedTime -= TimeSpan.FromSeconds(1.0);
                this.frameRate = this.frameCounter;
                this.frameCounter = 0;
                Preferences.Update();
            }
            if (this.IsMinimized)
            {
                return;
            }
            if (this.frameRate > 0 && this.frameRate < 50)
            {
                base.IsFixedTimeStep = true;
            }
            else
            {
                base.IsFixedTimeStep = true;
            }
            this.keyboardStateXna = Keyboard.GetState();
            if ((this.IsKeyPressed(Microsoft.Xna.Framework.Input.Keys.F11) || ((this.IsKeyDown(Microsoft.Xna.Framework.Input.Keys.LeftAlt) || this.IsKeyDown(Microsoft.Xna.Framework.Input.Keys.RightAlt)) && this.IsKeyPressed(Microsoft.Xna.Framework.Input.Keys.Enter))) && !this.UseWindowMode_TODO_ChangeFullScreenResolution)
            {
                Global.ScreenSizeManager.ToggleFullScreen();
                Thread.Sleep(500);
                return;
            }
            if (this.branding != null)
            {
                if (base.IsActive && this.branding.IsLoaded)
                {
                    if (this.branding.IsFinished)
                    {
                        this.branding = null;
                        return;
                    }
                    this.branding.Update(gameTime);
                }
                return;
            }
            if (this.IsKeyPressed(Microsoft.Xna.Framework.Input.Keys.Escape) || GamePad.GetState(PlayerIndex.One).Buttons.Back == Microsoft.Xna.Framework.Input.ButtonState.Pressed)
            {
                CutTheRope.iframework.core.Application.sharedMovieMgr().stop();
                CtrRenderer.Java_com_zeptolab_ctr_CtrRenderer_nativeBackPressed();
            }
            MouseState mouseState = CutTheRope.windows.MouseCursor.GetMouseState();
            CutTheRope.iframework.core.Application.sharedRootController().mouseMoved(CtrRenderer.transformX((float)mouseState.X), CtrRenderer.transformY((float)mouseState.Y));
            CtrRenderer.update((float)gameTime.ElapsedGameTime.Milliseconds / 1000f);
            base.Update(gameTime);
        }

        public void DrawMovie()
        {
            this._DrawMovie = true;
            base.GraphicsDevice.Clear(Microsoft.Xna.Framework.Color.Black);
            Texture2D texture = CutTheRope.iframework.core.Application.sharedMovieMgr().getTexture();
            if (texture == null)
            {
                return;
            }
            if (this._ignoreMouseClick > 0)
            {
                this._ignoreMouseClick--;
            }
            else
            {
                MouseState mouseState = Global.XnaGame.GetMouseState();
                if (mouseState.LeftButton == Microsoft.Xna.Framework.Input.ButtonState.Pressed && Global.ScreenSizeManager.CurrentSize.Contains(mouseState.X, mouseState.Y))
                {
                    CutTheRope.iframework.core.Application.sharedMovieMgr().stop();
                }
            }
            Global.GraphicsDevice.SetRenderTarget(null);
            base.GraphicsDevice.Clear(Microsoft.Xna.Framework.Color.Black);
            Global.ScreenSizeManager.FullScreenCropWidth = false;
            Global.ScreenSizeManager.ApplyViewportToDevice();
            Microsoft.Xna.Framework.Rectangle destinationRectangle = new(0, 0, base.GraphicsDevice.Viewport.Width, base.GraphicsDevice.Viewport.Height);
            Global.SpriteBatch.Begin(SpriteSortMode.Deferred, null, null, null, null, null, null);
            Global.SpriteBatch.Draw(texture, destinationRectangle, Microsoft.Xna.Framework.Color.White);
            Global.SpriteBatch.End();
        }

        protected override void Draw(GameTime gameTime)
        {
            this.frameCounter++;
            base.GraphicsDevice.Clear(Microsoft.Xna.Framework.Color.Black);
            if (this.branding != null)
            {
                if (this.branding.IsLoaded)
                {
                    this.branding.Draw(gameTime);
                    Global.GraphicsDevice.SetRenderTarget(null);
                }
                return;
            }
            Global.ScreenSizeManager.FullScreenCropWidth = true;
            Global.ScreenSizeManager.ApplyViewportToDevice();
            this._DrawMovie = false;
            CtrRenderer.onDrawFrame();
            Global.MouseCursor.Draw();
            Global.GraphicsDevice.SetRenderTarget(null);
            if (this.bFirstFrame)
            {
                base.GraphicsDevice.Clear(Microsoft.Xna.Framework.Color.Black);
            }
            else if (!this._DrawMovie)
            {
                OpenGL.CopyFromRenderTargetToScreen();
            }
            base.Draw(gameTime);
            this.bFirstFrame = false;
        }

        private Branding branding;

        private bool _altEnterPressed;

        private Process parentProcess;

        private int mouseState_X;

        private int mouseState_Y;

        private int mouseState_ScrollWheelValue;

        private Microsoft.Xna.Framework.Input.ButtonState mouseState_LeftButton;

        private Microsoft.Xna.Framework.Input.ButtonState mouseState_MiddleButton;

        private Microsoft.Xna.Framework.Input.ButtonState mouseState_RightButton;

        private Microsoft.Xna.Framework.Input.ButtonState mouseState_XButton1;

        private Microsoft.Xna.Framework.Input.ButtonState mouseState_XButton2;

        private bool UseWindowMode_TODO_ChangeFullScreenResolution = true;

        private Cursor _cursorLast;

        private Dictionary<Microsoft.Xna.Framework.Input.Keys, bool> keyState = new();

        private KeyboardState keyboardStateXna;

        private bool _DrawMovie;

        private int _ignoreMouseClick;

        private int frameRate;

        private int frameCounter;

        private TimeSpan elapsedTime = TimeSpan.Zero;

        private bool bFirstFrame = true;
    }
}
