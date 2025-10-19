using System;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using CutTheRope.iframework.core;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace CutTheRope.windows
{
	// Token: 0x02000012 RID: 18
	internal class ScreenSizeManager
	{
		// Token: 0x17000010 RID: 16
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

		// Token: 0x17000011 RID: 17
		// (get) Token: 0x060000B0 RID: 176 RVA: 0x00004B92 File Offset: 0x00002D92
		public int WindowWidth
		{
			get
			{
				return this._windowRect.Width;
			}
		}

		// Token: 0x17000012 RID: 18
		// (get) Token: 0x060000B1 RID: 177 RVA: 0x00004B9F File Offset: 0x00002D9F
		public int WindowHeight
		{
			get
			{
				return this._windowRect.Height;
			}
		}

		// Token: 0x17000013 RID: 19
		// (get) Token: 0x060000B2 RID: 178 RVA: 0x00004BAC File Offset: 0x00002DAC
		public int ScreenWidth
		{
			get
			{
				return this._fullScreenRect.Width;
			}
		}

		// Token: 0x17000014 RID: 20
		// (get) Token: 0x060000B3 RID: 179 RVA: 0x00004BB9 File Offset: 0x00002DB9
		public int ScreenHeight
		{
			get
			{
				return this._fullScreenRect.Height;
			}
		}

		// Token: 0x17000015 RID: 21
		// (get) Token: 0x060000B4 RID: 180 RVA: 0x00004BC6 File Offset: 0x00002DC6
		public bool IsFullScreen
		{
			get
			{
				return this._isFullScreen;
			}
		}

		// Token: 0x17000016 RID: 22
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

		// Token: 0x17000017 RID: 23
		// (get) Token: 0x060000B6 RID: 182 RVA: 0x00004BE5 File Offset: 0x00002DE5
		public int GameWidth
		{
			get
			{
				return this._gameWidth;
			}
		}

		// Token: 0x17000018 RID: 24
		// (get) Token: 0x060000B7 RID: 183 RVA: 0x00004BED File Offset: 0x00002DED
		public int GameHeight
		{
			get
			{
				return this._gameHeight;
			}
		}

		// Token: 0x17000019 RID: 25
		// (get) Token: 0x060000B8 RID: 184 RVA: 0x00004BF5 File Offset: 0x00002DF5
		public Microsoft.Xna.Framework.Rectangle ScaledViewRect
		{
			get
			{
				return this._scaledViewRect;
			}
		}

		// Token: 0x1700001A RID: 26
		// (get) Token: 0x060000B9 RID: 185 RVA: 0x00004BFD File Offset: 0x00002DFD
		public bool SkipSizeChanges
		{
			get
			{
				return this._skipChanges;
			}
		}

		// Token: 0x1700001B RID: 27
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

		// Token: 0x1700001C RID: 28
		// (get) Token: 0x060000BB RID: 187 RVA: 0x00004C1D File Offset: 0x00002E1D
		public double WidthAspectRatio
		{
			get
			{
				return (double)this._scaledViewRect.Width / (double)this._gameWidth;
			}
		}

		// Token: 0x060000BC RID: 188
		[DllImport("Shell32.dll")]
		private static extern int SHChangeNotify(int eventId, int flags, IntPtr item1, IntPtr item2);

		// Token: 0x060000BD RID: 189 RVA: 0x00004C33 File Offset: 0x00002E33
		private void RefreshDesktop()
		{
			ScreenSizeManager.SHChangeNotify(134217728, 4096, IntPtr.Zero, IntPtr.Zero);
		}

		// Token: 0x060000BE RID: 190 RVA: 0x00004C4F File Offset: 0x00002E4F
		public void SetWindowMinimumSize(Form form)
		{
			form.MinimumSize = new Size(800, this.ScaledGameHeight(800));
		}

		// Token: 0x060000BF RID: 191 RVA: 0x00004C6C File Offset: 0x00002E6C
		public int TransformWindowToViewX(int x)
		{
			return x - this._scaledViewRect.X;
		}

		// Token: 0x060000C0 RID: 192 RVA: 0x00004C7B File Offset: 0x00002E7B
		public int TransformWindowToViewY(int y)
		{
			return y - this._scaledViewRect.Y;
		}

		// Token: 0x060000C1 RID: 193 RVA: 0x00004C8A File Offset: 0x00002E8A
		public float TransformViewToGameX(float x)
		{
			return x * (float)this._gameWidth / (float)this._scaledViewRect.Width;
		}

		// Token: 0x060000C2 RID: 194 RVA: 0x00004CA2 File Offset: 0x00002EA2
		public float TransformViewToGameY(float y)
		{
			return y * (float)this._gameHeight / (float)this._scaledViewRect.Height;
		}

		// Token: 0x060000C3 RID: 195 RVA: 0x00004CBA File Offset: 0x00002EBA
		public ScreenSizeManager(int gameWidth, int gameHeight)
		{
			this._gameWidth = gameWidth;
			this._gameHeight = gameHeight;
			this._gameAspectRatio = (double)gameHeight / (double)gameWidth;
		}

		// Token: 0x060000C4 RID: 196 RVA: 0x00004CE4 File Offset: 0x00002EE4
		public void Init(DisplayMode displayMode, int windowWidth, bool isFullScreen)
		{
			this.FullScreenRectChanged(displayMode);
			int num = ((windowWidth > 0) ? windowWidth : (displayMode.Width - 100));
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

		// Token: 0x060000C5 RID: 197 RVA: 0x00004D60 File Offset: 0x00002F60
		public int ScaledGameWidth(int scaledHeight)
		{
			return (int)((double)scaledHeight / this._gameAspectRatio + 0.5);
		}

		// Token: 0x060000C6 RID: 198 RVA: 0x00004D76 File Offset: 0x00002F76
		public int ScaledGameHeight(int scaledWidth)
		{
			return (int)((double)scaledWidth * this._gameAspectRatio + 0.5);
		}

		// Token: 0x060000C7 RID: 199 RVA: 0x00004D8C File Offset: 0x00002F8C
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
				int num = (this._fullScreenCropWidth ? this._fullScreenRect.Height : this.ScaledGameHeight(this._fullScreenRect.Width));
				int num2 = (this._fullScreenCropWidth ? this.ScaledGameWidth(num) : this._fullScreenRect.Width);
				this._scaledViewRect = new Microsoft.Xna.Framework.Rectangle((this._fullScreenRect.Width - num2) / 2, (this._fullScreenRect.Height - num) / 2, num2, num);
				return;
			}
			int num3 = (this._fullScreenCropWidth ? ((int)((float)this._fullScreenRect.Width / 5f * 4f)) : this.ScaledGameHeight(this._fullScreenRect.Width));
			int num4 = (this._fullScreenCropWidth ? this.ScaledGameWidth(num3) : this._fullScreenRect.Width);
			this._scaledViewRect = new Microsoft.Xna.Framework.Rectangle((this._fullScreenRect.Width - num4) / 2, (this._fullScreenRect.Height - num3) / 2, num4, num3);
		}

		// Token: 0x060000C8 RID: 200 RVA: 0x00004EC0 File Offset: 0x000030C0
		public void ApplyWindowSize(int width)
		{
			GraphicsDeviceManager graphicsDeviceManager = Global.GraphicsDeviceManager;
			graphicsDeviceManager.PreferredBackBufferWidth = width;
			graphicsDeviceManager.PreferredBackBufferHeight = this.ScaledGameHeight(width);
			graphicsDeviceManager.ApplyChanges();
			this.WindowRectChanged(new Microsoft.Xna.Framework.Rectangle(0, 0, graphicsDeviceManager.PreferredBackBufferWidth, graphicsDeviceManager.PreferredBackBufferHeight));
		}

		// Token: 0x060000C9 RID: 201 RVA: 0x00004F08 File Offset: 0x00003108
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

		// Token: 0x060000CA RID: 202 RVA: 0x00004FDC File Offset: 0x000031DC
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

		// Token: 0x060000CB RID: 203 RVA: 0x000050B8 File Offset: 0x000032B8
		public void ApplyViewportToDevice()
		{
			Microsoft.Xna.Framework.Rectangle bounds = ((!this._isFullScreen) ? Microsoft.Xna.Framework.Rectangle.Intersect(this._scaledViewRect, this._windowRect) : Microsoft.Xna.Framework.Rectangle.Intersect(this._scaledViewRect, this._fullScreenRect));
			try
			{
				Global.GraphicsDevice.Viewport = new Viewport(bounds);
			}
			catch (Exception)
			{
			}
		}

		// Token: 0x060000CC RID: 204 RVA: 0x00005118 File Offset: 0x00003318
		public void Save()
		{
			Preferences._setIntforKey(this._windowRect.Width, "PREFS_WINDOW_WIDTH", false);
			Preferences._setIntforKey(this._windowRect.Height, "PREFS_WINDOW_HEIGHT", false);
			Preferences._setBooleanforKey(this._isFullScreen, "PREFS_WINDOW_FULLSCREEN", true);
		}

		// Token: 0x060000CD RID: 205 RVA: 0x00005157 File Offset: 0x00003357
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

		// Token: 0x060000CE RID: 206 RVA: 0x00005186 File Offset: 0x00003386
		private void FullScreenRectChanged(DisplayMode d)
		{
			this.FullScreenRectChanged(new Microsoft.Xna.Framework.Rectangle(0, 0, d.Width, d.Height));
		}

		// Token: 0x060000CF RID: 207 RVA: 0x000051A1 File Offset: 0x000033A1
		private void FullScreenRectChanged(Microsoft.Xna.Framework.Rectangle r)
		{
			if (!this._skipChanges)
			{
				this._fullScreenRect = r;
				this.UpdateScaledView();
			}
		}

		// Token: 0x060000D0 RID: 208 RVA: 0x000051B8 File Offset: 0x000033B8
		private void EnableFullScreen(bool bFull)
		{
			if (!this._skipChanges)
			{
				this._isFullScreen = bFull;
				this.UpdateScaledView();
			}
		}

		// Token: 0x04000080 RID: 128
		public const int MIN_WINDOW_WIDTH = 800;

		// Token: 0x04000081 RID: 129
		private bool _isFullScreen;

		// Token: 0x04000082 RID: 130
		private Microsoft.Xna.Framework.Rectangle _windowRect;

		// Token: 0x04000083 RID: 131
		private Microsoft.Xna.Framework.Rectangle _fullScreenRect;

		// Token: 0x04000084 RID: 132
		private int _gameWidth;

		// Token: 0x04000085 RID: 133
		private int _gameHeight;

		// Token: 0x04000086 RID: 134
		private double _gameAspectRatio;

		// Token: 0x04000087 RID: 135
		private Microsoft.Xna.Framework.Rectangle _scaledViewRect;

		// Token: 0x04000088 RID: 136
		private bool _skipChanges;

		// Token: 0x04000089 RID: 137
		private bool _fullScreenCropWidth = true;
	}
}
