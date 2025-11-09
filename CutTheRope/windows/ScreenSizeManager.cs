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
                return _windowRect.Width;
            }
        }

        // (get) Token: 0x060000B1 RID: 177 RVA: 0x00004B9F File Offset: 0x00002D9F
        public int WindowHeight
        {
            get
            {
                return _windowRect.Height;
            }
        }

        // (get) Token: 0x060000B2 RID: 178 RVA: 0x00004BAC File Offset: 0x00002DAC
        public int ScreenWidth
        {
            get
            {
                return _fullScreenRect.Width;
            }
        }

        // (get) Token: 0x060000B3 RID: 179 RVA: 0x00004BB9 File Offset: 0x00002DB9
        public int ScreenHeight
        {
            get
            {
                return _fullScreenRect.Height;
            }
        }

        // (get) Token: 0x060000B4 RID: 180 RVA: 0x00004BC6 File Offset: 0x00002DC6
        public bool IsFullScreen
        {
            get
            {
                return _isFullScreen;
            }
        }

        // (get) Token: 0x060000B5 RID: 181 RVA: 0x00004BCE File Offset: 0x00002DCE
        public Microsoft.Xna.Framework.Rectangle CurrentSize
        {
            get
            {
                if (IsFullScreen)
                {
                    return _fullScreenRect;
                }
                return _windowRect;
            }
        }

        // (get) Token: 0x060000B6 RID: 182 RVA: 0x00004BE5 File Offset: 0x00002DE5
        public int GameWidth
        {
            get
            {
                return _gameWidth;
            }
        }

        // (get) Token: 0x060000B7 RID: 183 RVA: 0x00004BED File Offset: 0x00002DED
        public int GameHeight
        {
            get
            {
                return _gameHeight;
            }
        }

        // (get) Token: 0x060000B8 RID: 184 RVA: 0x00004BF5 File Offset: 0x00002DF5
        public Microsoft.Xna.Framework.Rectangle ScaledViewRect
        {
            get
            {
                return _scaledViewRect;
            }
        }

        // (get) Token: 0x060000B9 RID: 185 RVA: 0x00004BFD File Offset: 0x00002DFD
        public bool SkipSizeChanges
        {
            get
            {
                return _skipChanges;
            }
        }

        // (set) Token: 0x060000BA RID: 186 RVA: 0x00004C05 File Offset: 0x00002E05
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

        // (get) Token: 0x060000BB RID: 187 RVA: 0x00004C1D File Offset: 0x00002E1D
        public double WidthAspectRatio
        {
            get
            {
                return _scaledViewRect.Width / (double)_gameWidth;
            }
        }

        [DllImport("Shell32.dll")]
        private static extern int SHChangeNotify(int eventId, int flags, IntPtr item1, IntPtr item2);

        private void RefreshDesktop()
        {
            SHChangeNotify(134217728, 4096, IntPtr.Zero, IntPtr.Zero);
        }

        public void SetWindowMinimumSize(Form form)
        {
            form.MinimumSize = new Size(800, ScaledGameHeight(800));
        }

        public int TransformWindowToViewX(int x)
        {
            return x - _scaledViewRect.X;
        }

        public int TransformWindowToViewY(int y)
        {
            return y - _scaledViewRect.Y;
        }

        public float TransformViewToGameX(float x)
        {
            return x * _gameWidth / _scaledViewRect.Width;
        }

        public float TransformViewToGameY(float y)
        {
            return y * _gameHeight / _scaledViewRect.Height;
        }

        public ScreenSizeManager(int gameWidth, int gameHeight)
        {
            _gameWidth = gameWidth;
            _gameHeight = gameHeight;
            _gameAspectRatio = gameHeight / (double)gameWidth;
        }

        public void Init(DisplayMode displayMode, int windowWidth, bool isFullScreen)
        {
            FullScreenRectChanged(displayMode);
            int num = (windowWidth > 0) ? windowWidth : (displayMode.Width - 100);
            if (num < 800)
            {
                num = 800;
            }
            if (num > MAX_WINDOW_WIDTH)
            {
                num = MAX_WINDOW_WIDTH;
            }
            if (num > displayMode.Width)
            {
                num = displayMode.Width;
            }
            WindowRectChanged(new Microsoft.Xna.Framework.Rectangle(0, 0, num, ScaledGameHeight(num)));
            if (isFullScreen)
            {
                ToggleFullScreen();
                return;
            }
            ApplyWindowSize(WindowWidth);
        }

        public int ScaledGameWidth(int scaledHeight)
        {
            return (int)(scaledHeight / _gameAspectRatio + 0.5);
        }

        public int ScaledGameHeight(int scaledWidth)
        {
            return (int)(scaledWidth * _gameAspectRatio + 0.5);
        }

        private void UpdateScaledView()
        {
            if (_skipChanges)
            {
                return;
            }
            if (!_isFullScreen)
            {
                _scaledViewRect = _windowRect;
                return;
            }
            if (_fullScreenRect.Width >= _fullScreenRect.Height)
            {
                int num = _fullScreenCropWidth ? _fullScreenRect.Height : ScaledGameHeight(_fullScreenRect.Width);
                int num2 = _fullScreenCropWidth ? ScaledGameWidth(num) : _fullScreenRect.Width;
                _scaledViewRect = new Microsoft.Xna.Framework.Rectangle((_fullScreenRect.Width - num2) / 2, (_fullScreenRect.Height - num) / 2, num2, num);
                return;
            }
            int num3 = _fullScreenCropWidth ? ((int)(_fullScreenRect.Width / 5f * 4f)) : ScaledGameHeight(_fullScreenRect.Width);
            int num4 = _fullScreenCropWidth ? ScaledGameWidth(num3) : _fullScreenRect.Width;
            _scaledViewRect = new Microsoft.Xna.Framework.Rectangle((_fullScreenRect.Width - num4) / 2, (_fullScreenRect.Height - num3) / 2, num4, num3);
        }

        public void ApplyWindowSize(int width)
        {
            GraphicsDeviceManager graphicsDeviceManager = Global.GraphicsDeviceManager;
            graphicsDeviceManager.PreferredBackBufferWidth = width;
            graphicsDeviceManager.PreferredBackBufferHeight = ScaledGameHeight(width);
            graphicsDeviceManager.ApplyChanges();
            WindowRectChanged(new Microsoft.Xna.Framework.Rectangle(0, 0, graphicsDeviceManager.PreferredBackBufferWidth, graphicsDeviceManager.PreferredBackBufferHeight));
        }

        public void ToggleFullScreen()
        {
            _skipChanges = true;
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
            _skipChanges = false;
            EnableFullScreen(!isFullScreen);
            Save();
            iframework.core.Application.sharedCanvas().reshape();
            iframework.core.Application.sharedRootController().fullscreenToggled(!isFullScreen);
            if (!graphicsDeviceManager.IsFullScreen)
            {
                RefreshDesktop();
            }
        }

        public void FixWindowSize(Microsoft.Xna.Framework.Rectangle newWindowRect)
        {
            if (_skipChanges)
            {
                return;
            }
            GraphicsDeviceManager graphicsDeviceManager = Global.GraphicsDeviceManager;
            FullScreenRectChanged(GraphicsAdapter.DefaultAdapter.CurrentDisplayMode);
            if (!IsFullScreen)
            {
                try
                {
                    int num = graphicsDeviceManager.PreferredBackBufferWidth;
                    if (newWindowRect.Width != WindowWidth)
                    {
                        num = newWindowRect.Width;
                    }
                    else if (newWindowRect.Height != WindowHeight)
                    {
                        num = ScaledGameWidth(newWindowRect.Height);
                    }
                    if (num < 800 || ScaledGameHeight(num) < ScaledGameHeight(800))
                    {
                        num = 800;
                    }
                    if (num > MAX_WINDOW_WIDTH)
                    {
                        num = MAX_WINDOW_WIDTH;
                    }
                    if (num > ScreenWidth)
                    {
                        num = ScreenWidth;
                    }
                    ApplyWindowSize(num);
                }
                catch (Exception)
                {
                }
            }
            Save();
            iframework.core.Application.sharedCanvas().reshape();
        }

        public void ApplyViewportToDevice()
        {
            Microsoft.Xna.Framework.Rectangle bounds = (!_isFullScreen) ? Microsoft.Xna.Framework.Rectangle.Intersect(_scaledViewRect, _windowRect) : Microsoft.Xna.Framework.Rectangle.Intersect(_scaledViewRect, _fullScreenRect);
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
            Preferences._setIntforKey(_windowRect.Width, "PREFS_WINDOW_WIDTH", false);
            Preferences._setIntforKey(_windowRect.Height, "PREFS_WINDOW_HEIGHT", false);
            Preferences._setBooleanforKey(_isFullScreen, "PREFS_WINDOW_FULLSCREEN", true);
        }

        private void WindowRectChanged(Microsoft.Xna.Framework.Rectangle newWindowRect)
        {
            if (!_skipChanges)
            {
                _windowRect = newWindowRect;
                _windowRect.X = 0;
                _windowRect.Y = 0;
                UpdateScaledView();
            }
        }

        private void FullScreenRectChanged(DisplayMode d)
        {
            FullScreenRectChanged(new Microsoft.Xna.Framework.Rectangle(0, 0, d.Width, d.Height));
        }

        private void FullScreenRectChanged(Microsoft.Xna.Framework.Rectangle r)
        {
            if (!_skipChanges)
            {
                _fullScreenRect = r;
                UpdateScaledView();
            }
        }

        private void EnableFullScreen(bool bFull)
        {
            if (!_skipChanges)
            {
                _isFullScreen = bFull;
                UpdateScaledView();
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
