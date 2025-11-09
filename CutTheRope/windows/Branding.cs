using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.IO;

namespace CutTheRope.windows
{
    // Token: 0x02000009 RID: 9
    public class Branding
    {
        // Token: 0x17000008 RID: 8
        // (get) Token: 0x06000042 RID: 66 RVA: 0x000032D1 File Offset: 0x000014D1
        public bool IsLoaded
        {
            get
            {
                return this._isLoaded;
            }
        }

        // Token: 0x17000009 RID: 9
        // (get) Token: 0x06000043 RID: 67 RVA: 0x000032D9 File Offset: 0x000014D9
        public bool IsFinished
        {
            get
            {
                return this.IsLoaded && this._currentSplash >= this._listBitmap.Count;
            }
        }

        // Token: 0x06000044 RID: 68 RVA: 0x000032FC File Offset: 0x000014FC
        public void LoadSplashScreens()
        {
            List<string> list2 = new() { "BMP", "GIF", "EXIF", "JPG", "JPEG", "PNG", "TIFF" };
            int num = 1;
            for (; ; )
            {
                Texture2D texture2D = null;
                foreach (string item in list2)
                {
                    try
                    {
                        FileStream fileStream = new("branding/splash" + num.ToString() + "." + item, FileMode.Open, FileAccess.Read);
                        texture2D = Texture2D.FromStream(Global.GraphicsDevice, fileStream);
                        fileStream.Close();
                        this._listBitmap.Add(texture2D);
                        break;
                    }
                    catch (Exception)
                    {
                    }
                }
                if (texture2D == null)
                {
                    break;
                }
                num++;
            }
            this._isLoaded = true;
        }

        // Token: 0x06000045 RID: 69 RVA: 0x00003400 File Offset: 0x00001600
        public void Update(GameTime gameTime)
        {
            if (this._waitFirstDraw)
            {
                return;
            }
            try
            {
                this._currentSplashTime += gameTime.ElapsedGameTime;
                if (this._currentSplashTime.TotalMilliseconds >= 3700.0)
                {
                    this._currentSplash++;
                    this._currentSplashTime = default(TimeSpan);
                }
            }
            catch (Exception)
            {
            }
        }

        // Token: 0x06000046 RID: 70 RVA: 0x00003474 File Offset: 0x00001674
        public void Draw(GameTime gameTime)
        {
            try
            {
                if (this.IsLoaded && !this.IsFinished)
                {
                    if (this._waitFirstDraw)
                    {
                        this._waitFirstDraw = false;
                        this._currentSplash = 0;
                        this._currentSplashTime = default(TimeSpan);
                    }
                    Texture2D texture2D = this._listBitmap[this._currentSplash];
                    Rectangle currentSize = Global.ScreenSizeManager.CurrentSize;
                    Rectangle bounds = texture2D.Bounds;
                    double num5 = (double)currentSize.Width / (double)bounds.Width;
                    double val2 = (double)currentSize.Height / (double)bounds.Height;
                    double num = Math.Min(num5, val2);
                    bounds.Width = (int)((double)bounds.Width * num + 0.5);
                    bounds.Height = (int)((double)bounds.Height * num + 0.5);
                    bounds.X = (currentSize.Width - bounds.Width) / 2;
                    bounds.Y = (currentSize.Height - bounds.Height) / 2;
                    Color color = Color.White;
                    if (this._currentSplashTime.TotalMilliseconds < 200.0)
                    {
                        float num2 = (float)(this._currentSplashTime.TotalMilliseconds / 200.0);
                        color = new Color(num2, num2, num2, num2);
                    }
                    double num3 = 3000.0 - this._currentSplashTime.TotalMilliseconds;
                    if (num3 < 500.0)
                    {
                        if (num3 < 0.0)
                        {
                            num3 = 0.0;
                        }
                        float num4 = (float)(num3 / 500.0);
                        color = new Color(num4, num4, num4, num4);
                    }
                    Global.GraphicsDevice.Clear(Color.Black);
                    Global.SpriteBatch.Begin(SpriteSortMode.Deferred, null, null, null, null, null, null);
                    Global.SpriteBatch.Draw(texture2D, bounds, color);
                    Global.SpriteBatch.End();
                }
            }
            catch (Exception)
            {
            }
        }

        // Token: 0x04000033 RID: 51
        private const int SPLASH_TIME_MSEC = 3000;

        // Token: 0x04000034 RID: 52
        private const int SPLASH_FADE_IN_TIME_MSEC = 200;

        // Token: 0x04000035 RID: 53
        private const int SPLASH_FADE_OUT_TIME_MSEC = 500;

        // Token: 0x04000036 RID: 54
        private const int SPLASH_TIME_FULL_MSEC = 3700;

        // Token: 0x04000037 RID: 55
        private List<Texture2D> _listBitmap = new();

        // Token: 0x04000038 RID: 56
        private TimeSpan _currentSplashTime;

        // Token: 0x04000039 RID: 57
        private int _currentSplash;

        // Token: 0x0400003A RID: 58
        private bool _isLoaded;

        // Token: 0x0400003B RID: 59
        private bool _waitFirstDraw = true;
    }
}
