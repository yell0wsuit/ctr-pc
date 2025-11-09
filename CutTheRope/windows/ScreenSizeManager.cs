using CutTheRope.iframework.core;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace CutTheRope.windows
{
    internal class ScreenSizeManager
    {
        // (get) Token: 0x060000AF RID: 175 RVA: 0x00004B78 File Offset: 0x00002D78
        public static int MAX_WINDOW_WIDTH
        {
            get
            {
                if (Global.GraphicsDeviceManager.GraphicsProfile == GraphicsProfile.HiDef)
                {
                    return 4096;
                }
                return 2048;
            }
        }

        // (get) Token: 0x060000B0 RID: 176 RVA: 0x00004B92 File Offset: 0x00002D92
        public int WindowWidth
        {
            get
            {
                return this._windowRect.Width;
            }
        }

        // (get) Token: 0x060000B1 RID: 177 RVA: 0x00004B9F File Offset: 0x00002D9F
        public int WindowHeight
        {
            get
            {
                return this._windowRect.Height;
            }
        }

        // (get) Token: 0x060000B2 RID: 178 RVA: 0x00004BAC File Offset: 0x00002DAC
        public int ScreenWidth
        {
            get
            {
                return this._fullScreenRect.Width;
            }
        }

        // (get) Token: 0x060000B3 RID: 179 RVA: 0x00004BB9 File Offset: 0x00002DB9
        public int ScreenHeight
        {
            get
            {
                return this._fullScreenRect.Height;
            }
        }

        // (get) Token: 0x060000B4 RID: 180 RVA: 0x00004BC6 File Offset: 0x00002DC6
        public bool IsFullScreen
        {
            get
            {
                return this._isFullScreen;
            }
        }

        // (get) Token: 0x060000B5 RID: 181 RVA: 0x00004BCE File Offset: 0x00002DCE
        public Microsoft.Xna.Framework.Rectangle CurrentSize
        {
            get
            {
                if (this.IsFullScreen)
                {
                    return this._fullScreenRect;
                }
                return this._windowRect;
            }
        }

        // (get) Token: 0x060000B6 RID: 182 RVA: 0x00004BE5 File Offset: 0x00002DE5
        public int GameWidth
        {
            get
            {
                return this._gameWidth;
            }
        }

        // (get) Token: 0x060000B7 RID: 183 RVA: 0x00004BED File Offset: 0x00002DED
        public int GameHeight
        {
            get
            {
                return this._gameHeight;
            }
        }

        // (get) Token: 0x060000B8 RID: 184 RVA: 0x00004BF5 File Offset: 0x00002DF5
        public Microsoft.Xna.Framework.Rectangle ScaledViewRect
        {
            get
            {
                return this._scaledViewRect;
            }
        }

        // (get) Token: 0x060000B9 RID: 185 RVA: 0x00004BFD File Offset: 0x00002DFD
        public bool SkipSizeChanges
        {
            get
            {
                return this._skipChanges;
            }
        }

        // (set) Token: 0x060000BA RID: 186 RVA: 0x00004C05 File Offset: 0x00002E05
        public bool FullScreenCropWidth
        {
            set
            {
                if (this._fullScreenCropWidth != value)
                {
                    this._fullScreenCropWidth = value;
                    this.UpdateScaledView();
                }
            }
        }

        // (get) Token: 0x060000BB RID: 187 RVA: 0x00004C1D File Offset: 0x00002E1D
        public double WidthAspectRatio
        {
            get
            {
                return (double)this._scaledViewRect.Width / (double)this._gameWidth;
            }
        }

        [DllImport("Shell32.dll")]
        private static extern int SHChangeNotify(int eventId, int flags, IntPtr item1, IntPtr item2);

        private void RefreshDesktop()
        {
            ScreenSizeManager.SHChangeNotify(134217728, 4096, IntPtr.Zero, IntPtr.Zero);
        }

        public void SetWindowMinimumSize(Form form)
        {
            form.MinimumSize = new Size(800, this.ScaledGameHeight(800));
        }

        public int TransformWindowToViewX(int x)
        {
            return x - this._scaledViewRect.X;
        }

        public int TransformWindowToViewY(int y)
        {
            return y - this._scaledViewRect.Y;
        }

        public float TransformViewToGameX(float x)
        {
            return x * (float)this._gameWidth / (float)this._scaledViewRect.Width;
        }

        public float TransformViewToGameY(float y)
        {
            return y * (float)this._gameHeight / (float)this._scaledViewRect.Height;
        }

        public ScreenSizeManager(int gameWidth, int gameHeight)
        {
            this._gameWidth = gameWidth;
            this._gameHeight = gameHeight;
            this._gameAspectRatio = (double)gameHeight / (double)gameWidth;
        }

        public void Init(DisplayMode displayMode, int windowWidth, bool isFullScreen)
        {
            this.FullScreenRectChanged(displayMode);
            int num = (windowWidth > 0) ? windowWidth : (displayMode.Width - 100);
            if (num < 800)
            {
                num = 800;
            }
            if (num > ScreenSizeManager.MAX_WINDOW_WIDTH)
            {
                num = ScreenSizeManager.MAX_WINDOW_WIDTH;
            }
            if (num > displayMode.Width)
            {
                num = displayMode.Width;
            }
            this.WindowRectChanged(new Microsoft.Xna.Framework.Rectangle(0, 0, num, this.ScaledGameHeight(num)));
            if (isFullScreen)
            {
                this.ToggleFullScreen();
                return;
            }
            this.ApplyWindowSize(this.WindowWidth);
        }

        public int ScaledGameWidth(int scaledHeight)
        {
            return (int)((double)scaledHeight / this._gameAspectRatio + 0.5);
        }

        public int ScaledGameHeight(int scaledWidth)
        {
            return (int)((double)scaledWidth * this._gameAspectRatio + 0.5);
        }

        private void UpdateScaledView()
        {
            if (this._skipChanges)
            {
                return;
            }
            if (!this._isFullScreen)
            {
                this._scaledViewRect = this._windowRect;
                return;
            }
            if (this._fullScreenRect.Width >= this._fullScreenRect.Height)
            {
                int num = this._fullScreenCropWidth ? this._fullScreenRect.Height : this.ScaledGameHeight(this._fullScreenRect.Width);
                int num2 = this._fullScreenCropWidth ? this.ScaledGameWidth(num) : this._fullScreenRect.Width;
                this._scaledViewRect = new Microsoft.Xna.Framework.Rectangle((this._fullScreenRect.Width - num2) / 2, (this._fullScreenRect.Height - num) / 2, num2, num);
                return;
            }
            int num3 = this._fullScreenCropWidth ? ((int)((float)this._fullScreenRect.Width / 5f * 4f)) : this.ScaledGameHeight(this._fullScreenRect.Width);
            int num4 = this._fullScreenCropWidth ? this.ScaledGameWidth(num3) : this._fullScreenRect.Width;
            this._scaledViewRect = new Microsoft.Xna.Framework.Rectangle((this._fullScreenRect.Width - num4) / 2, (this._fullScreenRect.Height - num3) / 2, num4, num3);
        }

        public void ApplyWindowSize(int width)
        {
            GraphicsDeviceManager graphicsDeviceManager = Global.GraphicsDeviceManager;
            graphicsDeviceManager.PreferredBackBufferWidth = width;
            graphicsDeviceManager.PreferredBackBufferHeight = this.ScaledGameHeight(width);
            graphicsDeviceManager.ApplyChanges();
            this.WindowRectChanged(new Microsoft.Xna.Framework.Rectangle(0, 0, graphicsDeviceManager.PreferredBackBufferWidth, graphicsDeviceManager.PreferredBackBufferHeight));
        }

        public void ToggleFullScreen()
        {
            this._skipChanges = true;
            GraphicsDeviceManager graphicsDeviceManager = Global.GraphicsDeviceManager;
            bool isFullScreen = graphicsDeviceManager.IsFullScreen;
            bool fullScreenCropWidth = this._fullScreenCropWidth;
            this.FullScreenCropWidth = true;
            if (isFullScreen)
            {
                graphicsDeviceManager.PreferredBackBufferWidth = this._windowRect.Width;
                graphicsDeviceManager.PreferredBackBufferHeight = this._windowRect.Height;
            }
            else
            {
                graphicsDeviceManager.PreferredBackBufferWidth = this._fullScreenRect.Width;
                graphicsDeviceManager.PreferredBackBufferHeight = this._fullScreenRect.Height;
            }
            graphicsDeviceManager.IsFullScreen = !isFullScreen;
            graphicsDeviceManager.ApplyChanges();
            this.ApplyViewportToDevice();
            this.FullScreenCropWidth = fullScreenCropWidth;
            this._skipChanges = false;
            this.EnableFullScreen(!isFullScreen);
            this.Save();
            CutTheRope.iframework.core.Application.sharedCanvas().reshape();
            CutTheRope.iframework.core.Application.sharedRootController().fullscreenToggled(!isFullScreen);
            if (!graphicsDeviceManager.IsFullScreen)
            {
                this.RefreshDesktop();
            }
        }

        public void FixWindowSize(Microsoft.Xna.Framework.Rectangle newWindowRect)
        {
            if (this._skipChanges)
            {
                return;
            }
            GraphicsDeviceManager graphicsDeviceManager = Global.GraphicsDeviceManager;
            this.FullScreenRectChanged(GraphicsAdapter.DefaultAdapter.CurrentDisplayMode);
            if (!this.IsFullScreen)
            {
                try
                {
                    int num = graphicsDeviceManager.PreferredBackBufferWidth;
                    if (newWindowRect.Width != this.WindowWidth)
                    {
                        num = newWindowRect.Width;
                    }
                    else if (newWindowRect.Height != this.WindowHeight)
                    {
                        num = this.ScaledGameWidth(newWindowRect.Height);
                    }
                    if (num < 800 || this.ScaledGameHeight(num) < this.ScaledGameHeight(800))
                    {
                        num = 800;
                    }
                    if (num > ScreenSizeManager.MAX_WINDOW_WIDTH)
                    {
                        num = ScreenSizeManager.MAX_WINDOW_WIDTH;
                    }
                    if (num > this.ScreenWidth)
                    {
                        num = this.ScreenWidth;
                    }
                    this.ApplyWindowSize(num);
                }
                catch (Exception)
                {
                }
            }
            this.Save();
            CutTheRope.iframework.core.Application.sharedCanvas().reshape();
        }

        public void ApplyViewportToDevice()
        {
            Microsoft.Xna.Framework.Rectangle bounds = (!this._isFullScreen) ? Microsoft.Xna.Framework.Rectangle.Intersect(this._scaledViewRect, this._windowRect) : Microsoft.Xna.Framework.Rectangle.Intersect(this._scaledViewRect, this._fullScreenRect);
            try
            {
                Global.GraphicsDevice.Viewport = new Viewport(bounds);
            }
            catch (Exception)
            {
            }
        }

        public void Save()
        {
            Preferences._setIntforKey(this._windowRect.Width, "PREFS_WINDOW_WIDTH", false);
            Preferences._setIntforKey(this._windowRect.Height, "PREFS_WINDOW_HEIGHT", false);
            Preferences._setBooleanforKey(this._isFullScreen, "PREFS_WINDOW_FULLSCREEN", true);
        }

        private void WindowRectChanged(Microsoft.Xna.Framework.Rectangle newWindowRect)
        {
            if (!this._skipChanges)
            {
                this._windowRect = newWindowRect;
                this._windowRect.X = 0;
                this._windowRect.Y = 0;
                this.UpdateScaledView();
            }
        }

        private void FullScreenRectChanged(DisplayMode d)
        {
            this.FullScreenRectChanged(new Microsoft.Xna.Framework.Rectangle(0, 0, d.Width, d.Height));
        }

        private void FullScreenRectChanged(Microsoft.Xna.Framework.Rectangle r)
        {
            if (!this._skipChanges)
            {
                this._fullScreenRect = r;
                this.UpdateScaledView();
            }
        }

        private void EnableFullScreen(bool bFull)
        {
            if (!this._skipChanges)
            {
                this._isFullScreen = bFull;
                this.UpdateScaledView();
            }
        }

        public const int MIN_WINDOW_WIDTH = 800;

        private bool _isFullScreen;

        private Microsoft.Xna.Framework.Rectangle _windowRect;

        private Microsoft.Xna.Framework.Rectangle _fullScreenRect;

        private int _gameWidth;

        private int _gameHeight;

        private double _gameAspectRatio;

        private Microsoft.Xna.Framework.Rectangle _scaledViewRect;

        private bool _skipChanges;

        private bool _fullScreenCropWidth = true;
    }
}
