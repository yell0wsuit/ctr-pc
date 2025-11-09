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
    // Token: 0x02000005 RID: 5
    public class Game1 : Game
    {
        // Token: 0x17000007 RID: 7
        // (get) Token: 0x06000022 RID: 34 RVA: 0x00002517 File Offset: 0x00000717
        private bool IsMinimized
        {
            get
            {
                return this.WindowAsForm().WindowState == FormWindowState.Minimized;
            }
        }

        // Token: 0x06000023 RID: 35 RVA: 0x00002528 File Offset: 0x00000728
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

        // Token: 0x06000024 RID: 36 RVA: 0x00002684 File Offset: 0x00000884
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

        // Token: 0x06000025 RID: 37 RVA: 0x00002740 File Offset: 0x00000940
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

        // Token: 0x06000026 RID: 38 RVA: 0x000027DA File Offset: 0x000009DA
        private void form_MouseMove(object sender, MouseEventArgs e)
        {
            this.mouseState_X = e.X;
            this.mouseState_Y = e.Y;
            CtrRenderer.Java_com_zeptolab_ctr_CtrRenderer_nativeTouchProcess(Global.MouseCursor.GetTouchLocation());
        }

        // Token: 0x06000027 RID: 39 RVA: 0x00002803 File Offset: 0x00000A03
        public MouseState GetMouseState()
        {
            return new MouseState(this.mouseState_X, this.mouseState_Y, this.mouseState_ScrollWheelValue, this.mouseState_LeftButton, this.mouseState_MiddleButton, this.mouseState_RightButton, this.mouseState_XButton1, this.mouseState_XButton2);
        }

        // Token: 0x06000028 RID: 40 RVA: 0x0000283C File Offset: 0x00000A3C
        private void GraphicsDeviceManager_PreparingDeviceSettings(object sender, PreparingDeviceSettingsEventArgs e)
        {
            e.GraphicsDeviceInformation.PresentationParameters.DepthStencilFormat = DepthFormat.None;
            if (e.GraphicsDeviceInformation.Adapter.CurrentDisplayMode.Width > ScreenSizeManager.MAX_WINDOW_WIDTH || e.GraphicsDeviceInformation.Adapter.CurrentDisplayMode.Height > ScreenSizeManager.MAX_WINDOW_WIDTH)
            {
                this.UseWindowMode_TODO_ChangeFullScreenResolution = true;
            }
        }

        // Token: 0x06000029 RID: 41 RVA: 0x0000289C File Offset: 0x00000A9C
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

        // Token: 0x0600002A RID: 42 RVA: 0x000028D8 File Offset: 0x00000AD8
        public void SetCursor(Cursor cursor, MouseState mouseState)
        {
            if (base.Window.ClientBounds.Contains(base.Window.ClientBounds.X + mouseState.X, base.Window.ClientBounds.Y + mouseState.Y) && this._cursorLast != cursor)
            {
                this.WindowAsForm().Cursor = cursor;
                this._cursorLast = cursor;
            }
        }

        // Token: 0x0600002B RID: 43 RVA: 0x0000294B File Offset: 0x00000B4B
        private Form WindowAsForm()
        {
            return (Form)Control.FromHandle(base.Window.Handle);
        }

        // Token: 0x0600002C RID: 44 RVA: 0x00002964 File Offset: 0x00000B64
        private void Window_ClientSizeChanged(object sender, EventArgs e)
        {
            base.Window.ClientSizeChanged -= this.Window_ClientSizeChanged;
            Global.ScreenSizeManager.FixWindowSize(base.Window.ClientBounds);
            base.Window.ClientSizeChanged += this.Window_ClientSizeChanged;
        }

        // Token: 0x0600002D RID: 45 RVA: 0x000029B4 File Offset: 0x00000BB4
        private void Game1_Exiting(object sender, EventArgs e)
        {
            Preferences._savePreferences();
            Preferences.Update();
        }

        // Token: 0x0600002E RID: 46 RVA: 0x000029C0 File Offset: 0x00000BC0
        private void Game1_Deactivated(object sender, EventArgs e)
        {
            this._ignoreMouseClick = 60;
            CtrRenderer.Java_com_zeptolab_ctr_CtrRenderer_nativePause();
        }

        // Token: 0x0600002F RID: 47 RVA: 0x000029CF File Offset: 0x00000BCF
        private void Game1_Activated(object sender, EventArgs e)
        {
            CtrRenderer.Java_com_zeptolab_ctr_CtrRenderer_nativeResume();
        }

        // Token: 0x06000030 RID: 48 RVA: 0x000029D6 File Offset: 0x00000BD6
        protected override void Initialize()
        {
            base.Initialize();
        }

        // Token: 0x06000031 RID: 49 RVA: 0x000029E0 File Offset: 0x00000BE0
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

        // Token: 0x06000032 RID: 50 RVA: 0x00002B1F File Offset: 0x00000D1F
        protected override void UnloadContent()
        {
        }

        // Token: 0x06000033 RID: 51 RVA: 0x00002B24 File Offset: 0x00000D24
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

        // Token: 0x06000034 RID: 52 RVA: 0x00002B7C File Offset: 0x00000D7C
        public bool IsKeyPressed(Microsoft.Xna.Framework.Input.Keys key)
        {
            bool value = false;
            this.keyState.TryGetValue(key, out value);
            bool flag = this.keyboardStateXna.IsKeyDown(key);
            this.keyState[key] = flag;
            return flag && value != flag;
        }

        // Token: 0x06000035 RID: 53 RVA: 0x00002BC0 File Offset: 0x00000DC0
        public bool IsKeyDown(Microsoft.Xna.Framework.Input.Keys key)
        {
            return this.keyboardStateXna.IsKeyDown(key);
        }

        // Token: 0x06000036 RID: 54 RVA: 0x00002BD0 File Offset: 0x00000DD0
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

        // Token: 0x06000037 RID: 55 RVA: 0x00002DDC File Offset: 0x00000FDC
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

        // Token: 0x06000038 RID: 56 RVA: 0x00002F00 File Offset: 0x00001100
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

        // Token: 0x0400000C RID: 12
        private Branding branding;

        // Token: 0x0400000D RID: 13
        private bool _altEnterPressed;

        // Token: 0x0400000E RID: 14
        private Process parentProcess;

        // Token: 0x0400000F RID: 15
        private int mouseState_X;

        // Token: 0x04000010 RID: 16
        private int mouseState_Y;

        // Token: 0x04000011 RID: 17
        private int mouseState_ScrollWheelValue;

        // Token: 0x04000012 RID: 18
        private Microsoft.Xna.Framework.Input.ButtonState mouseState_LeftButton;

        // Token: 0x04000013 RID: 19
        private Microsoft.Xna.Framework.Input.ButtonState mouseState_MiddleButton;

        // Token: 0x04000014 RID: 20
        private Microsoft.Xna.Framework.Input.ButtonState mouseState_RightButton;

        // Token: 0x04000015 RID: 21
        private Microsoft.Xna.Framework.Input.ButtonState mouseState_XButton1;

        // Token: 0x04000016 RID: 22
        private Microsoft.Xna.Framework.Input.ButtonState mouseState_XButton2;

        // Token: 0x04000017 RID: 23
        private bool UseWindowMode_TODO_ChangeFullScreenResolution = true;

        // Token: 0x04000018 RID: 24
        private Cursor _cursorLast;

        // Token: 0x04000019 RID: 25
        private Dictionary<Microsoft.Xna.Framework.Input.Keys, bool> keyState = new();

        // Token: 0x0400001A RID: 26
        private KeyboardState keyboardStateXna;

        // Token: 0x0400001B RID: 27
        private bool _DrawMovie;

        // Token: 0x0400001C RID: 28
        private int _ignoreMouseClick;

        // Token: 0x0400001D RID: 29
        private int frameRate;

        // Token: 0x0400001E RID: 30
        private int frameCounter;

        // Token: 0x0400001F RID: 31
        private TimeSpan elapsedTime = TimeSpan.Zero;

        // Token: 0x04000020 RID: 32
        private bool bFirstFrame = true;
    }
}
