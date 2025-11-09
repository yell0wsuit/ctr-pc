using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.IO;

namespace CutTheRope.windows
{
    public class Branding
    {
        // (get) Token: 0x06000042 RID: 66 RVA: 0x000032D1 File Offset: 0x000014D1
        public bool IsLoaded
        {
            get
            {
                return _isLoaded;
            }
        }

        // (get) Token: 0x06000043 RID: 67 RVA: 0x000032D9 File Offset: 0x000014D9
        public bool IsFinished
        {
            get
            {
                return IsLoaded && _currentSplash >= _listBitmap.Count;
            }
        }

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
                        _listBitmap.Add(texture2D);
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
            _isLoaded = true;
        }

        public void Update(GameTime gameTime)
        {
            if (_waitFirstDraw)
            {
                return;
            }
            try
            {
                _currentSplashTime += gameTime.ElapsedGameTime;
                if (_currentSplashTime.TotalMilliseconds >= 3700.0)
                {
                    _currentSplash++;
                    _currentSplashTime = default(TimeSpan);
                }
            }
            catch (Exception)
            {
            }
        }

        public void Draw(GameTime gameTime)
        {
            try
            {
                if (IsLoaded && !IsFinished)
                {
                    if (_waitFirstDraw)
                    {
                        _waitFirstDraw = false;
                        _currentSplash = 0;
                        _currentSplashTime = default(TimeSpan);
                    }
                    Texture2D texture2D = _listBitmap[_currentSplash];
                    Rectangle currentSize = Global.ScreenSizeManager.CurrentSize;
                    Rectangle bounds = texture2D.Bounds;
                    double num5 = currentSize.Width / (double)bounds.Width;
                    double val2 = currentSize.Height / (double)bounds.Height;
                    double num = Math.Min(num5, val2);
                    bounds.Width = (int)(bounds.Width * num + 0.5);
                    bounds.Height = (int)(bounds.Height * num + 0.5);
                    bounds.X = (currentSize.Width - bounds.Width) / 2;
                    bounds.Y = (currentSize.Height - bounds.Height) / 2;
                    Color color = Color.White;
                    if (_currentSplashTime.TotalMilliseconds < 200.0)
                    {
                        float num2 = (float)(_currentSplashTime.TotalMilliseconds / 200.0);
                        color = new Color(num2, num2, num2, num2);
                    }
                    double num3 = 3000.0 - _currentSplashTime.TotalMilliseconds;
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

        private const int SPLASH_TIME_MSEC = 3000;

        private const int SPLASH_FADE_IN_TIME_MSEC = 200;

        private const int SPLASH_FADE_OUT_TIME_MSEC = 500;

        private const int SPLASH_TIME_FULL_MSEC = 3700;

        private List<Texture2D> _listBitmap = new();

        private TimeSpan _currentSplashTime;

        private int _currentSplash;

        private bool _isLoaded;

        private bool _waitFirstDraw = true;
    }
}
