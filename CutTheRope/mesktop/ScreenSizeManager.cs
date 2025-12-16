using System;

using CutTheRope.Framework.Core;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace CutTheRope.Desktop
{
    internal sealed class ScreenSizeManager(int gameWidth, int gameHeight)
    {
        // (get) Token: 0x060000AF RID: 175 RVA: 0x00004B78 File Offset: 0x00002D78
        public static int MAX_WINDOW_WIDTH => Global.GraphicsDeviceManager.GraphicsProfile == GraphicsProfile.HiDef ? 4096 : 2048;

        // (get) Token: 0x060000B0 RID: 176 RVA: 0x00004B92 File Offset: 0x00002D92
        public int WindowWidth => _windowRect.Width;

        // (get) Token: 0x060000B1 RID: 177 RVA: 0x00004B9F File Offset: 0x00002D9F
        public int WindowHeight => _windowRect.Height;

        // (get) Token: 0x060000B2 RID: 178 RVA: 0x00004BAC File Offset: 0x00002DAC
        public int ScreenWidth => _fullScreenRect.Width;

        // (get) Token: 0x060000B3 RID: 179 RVA: 0x00004BB9 File Offset: 0x00002DB9
        public int ScreenHeight => _fullScreenRect.Height;

        // (get) Token: 0x060000B4 RID: 180 RVA: 0x00004BC6 File Offset: 0x00002DC6
        public bool IsFullScreen { get; private set; }

        // (get) Token: 0x060000B5 RID: 181 RVA: 0x00004BCE File Offset: 0x00002DCE
        public Rectangle CurrentSize => IsFullScreen ? _fullScreenRect : _windowRect;

        // (get) Token: 0x060000B6 RID: 182 RVA: 0x00004BE5 File Offset: 0x00002DE5
        public int GameWidth { get; } = gameWidth;

        // (get) Token: 0x060000B7 RID: 183 RVA: 0x00004BED File Offset: 0x00002DED
        public int GameHeight { get; } = gameHeight;

        // (get) Token: 0x060000B8 RID: 184 RVA: 0x00004BF5 File Offset: 0x00002DF5
        public Rectangle ScaledViewRect => _scaledViewRect;

        // (get) Token: 0x060000B9 RID: 185 RVA: 0x00004BFD File Offset: 0x00002DFD
        public bool SkipSizeChanges { get; private set; }

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
        public double WidthAspectRatio => _scaledViewRect.Width / (double)GameWidth;

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
            return x * GameWidth / _scaledViewRect.Width;
        }

        public float TransformViewToGameY(float y)
        {
            return y * GameHeight / _scaledViewRect.Height;
        }

        public void Init(DisplayMode displayMode, int windowWidth, bool isFullScreen)
        {
            FullScreenRectChanged(displayMode);
            int num = windowWidth > 0 ? windowWidth : displayMode.Width - 100;
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
            WindowRectChanged(new Rectangle(0, 0, num, ScaledGameHeight(num)));
            if (isFullScreen)
            {
                ToggleFullScreen();
                return;
            }
            ApplyWindowSize(WindowWidth);
        }

        public int ScaledGameWidth(int scaledHeight)
        {
            return (int)((scaledHeight / _gameAspectRatio) + 0.5);
        }

        public int ScaledGameHeight(int scaledWidth)
        {
            return (int)((scaledWidth * _gameAspectRatio) + 0.5);
        }

        private void UpdateScaledView()
        {
            if (SkipSizeChanges)
            {
                return;
            }
            if (!IsFullScreen)
            {
                _scaledViewRect = _windowRect;
                return;
            }
            if (_fullScreenRect.Width >= _fullScreenRect.Height)
            {
                int num = _fullScreenCropWidth ? _fullScreenRect.Height : ScaledGameHeight(_fullScreenRect.Width);
                int num2 = _fullScreenCropWidth ? ScaledGameWidth(num) : _fullScreenRect.Width;
                _scaledViewRect = new Rectangle((_fullScreenRect.Width - num2) / 2, (_fullScreenRect.Height - num) / 2, num2, num);
                return;
            }
            int num3 = _fullScreenCropWidth ? (int)(_fullScreenRect.Width / 5f * 4f) : ScaledGameHeight(_fullScreenRect.Width);
            int num4 = _fullScreenCropWidth ? ScaledGameWidth(num3) : _fullScreenRect.Width;
            _scaledViewRect = new Rectangle((_fullScreenRect.Width - num4) / 2, (_fullScreenRect.Height - num3) / 2, num4, num3);
        }

        public void ApplyWindowSize(int width)
        {
            GraphicsDeviceManager graphicsDeviceManager = Global.GraphicsDeviceManager;
            graphicsDeviceManager.PreferredBackBufferWidth = width;
            graphicsDeviceManager.PreferredBackBufferHeight = ScaledGameHeight(width);
            graphicsDeviceManager.ApplyChanges();
            WindowRectChanged(new Rectangle(0, 0, graphicsDeviceManager.PreferredBackBufferWidth, graphicsDeviceManager.PreferredBackBufferHeight));
        }

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
            Application.SharedCanvas().Reshape();
        }

        public void ApplyViewportToDevice()
        {
            Rectangle bounds = !IsFullScreen ? Rectangle.Intersect(_scaledViewRect, _windowRect) : Rectangle.Intersect(_scaledViewRect, _fullScreenRect);
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
            Preferences.SetIntForKey(_windowRect.Width, "PREFS_WINDOW_WIDTH", false);
            Preferences.SetIntForKey(_windowRect.Height, "PREFS_WINDOW_HEIGHT", false);
            Preferences.SetBooleanForKey(IsFullScreen, "PREFS_WINDOW_FULLSCREEN", true);
        }

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

        private void FullScreenRectChanged(DisplayMode d)
        {
            FullScreenRectChanged(new Rectangle(0, 0, d.Width, d.Height));
        }

        private void FullScreenRectChanged(Rectangle r)
        {
            if (!SkipSizeChanges)
            {
                _fullScreenRect = r;
                UpdateScaledView();
            }
        }

        private void EnableFullScreen(bool bFull)
        {
            if (!SkipSizeChanges)
            {
                IsFullScreen = bFull;
                UpdateScaledView();
            }
        }

        public const int MIN_WINDOW_WIDTH = 800;
        private Rectangle _windowRect;

        private Rectangle _fullScreenRect;
        private readonly double _gameAspectRatio = gameHeight / (double)gameWidth;

        private Rectangle _scaledViewRect;
        private bool _fullScreenCropWidth = true;
    }
}
