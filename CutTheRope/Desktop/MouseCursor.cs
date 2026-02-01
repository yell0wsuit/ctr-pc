using System;
using System.Collections.Generic;

using CutTheRope.Framework.Platform;
using CutTheRope.Helpers;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Input.Touch;

namespace CutTheRope.Desktop
{
    internal sealed class MouseCursor : IDisposable
    {
        public void Enable(bool b)
        {
            _enabled = b;
        }

        public void Dispose()
        {
            DisposeNativeCursors();
            _scaledCursor?.Dispose();
            _scaledCursorActive?.Dispose();
            _scaledCursor = null;
            _scaledCursorActive = null;
        }

        private void DisposeNativeCursors()
        {
            _nativeCursor?.Dispose();
            _nativeCursorActive?.Dispose();
            _nativeCursor = null;
            _nativeCursorActive = null;
        }

        public void ReleaseButtons()
        {
            _mouseStateTranformed = new MouseState(_mouseStateTranformed.X, _mouseStateTranformed.Y, _mouseStateTranformed.ScrollWheelValue, ButtonState.Released, ButtonState.Released, ButtonState.Released, ButtonState.Released, ButtonState.Released);
        }

        public void Load(ContentManager cm)
        {
            // Dispose old resources if reloading
            DisposeNativeCursors();
            _scaledCursor?.Dispose();
            _scaledCursorActive?.Dispose();
            _scaledCursor = null;
            _scaledCursorActive = null;

            _cursor = cm.Load<Texture2D>(ContentPaths.GetImageContentPath("cursor"));
            _cursorActive = cm.Load<Texture2D>(ContentPaths.GetImageContentPath("cursor_active"));

            // Force recreation of scaled cursors on next draw
            _currentScale = 0;
        }

        private void UpdateScaledCursors(double scale)
        {
            if (Math.Abs(_currentScale - scale) < 0.01)
            {
                return;
            }

            _currentScale = scale;

            // Dispose old scaled resources
            DisposeNativeCursors();
            _scaledCursor?.Dispose();
            _scaledCursorActive?.Dispose();

            // Create scaled textures
            _scaledCursor = ScaleTexture(_cursor, scale);
            _scaledCursorActive = ScaleTexture(_cursorActive, scale);

            // Create native cursors from scaled textures
            _nativeCursor = Microsoft.Xna.Framework.Input.MouseCursor.FromTexture2D(_scaledCursor, 0, 0);
            _nativeCursorActive = Microsoft.Xna.Framework.Input.MouseCursor.FromTexture2D(_scaledCursorActive, 0, 0);

            // Force cursor update
            _usingActiveCursor = !_usingActiveCursor;
        }

        private static Texture2D ScaleTexture(Texture2D source, double scale)
        {
            int newWidth = Math.Max(1, (int)(source.Width * scale));
            int newHeight = Math.Max(1, (int)(source.Height * scale));

            // Get source pixel data
            Color[] sourceData = new Color[source.Width * source.Height];
            source.GetData(sourceData);

            // Create scaled pixel data using bilinear interpolation
            Color[] scaledData = new Color[newWidth * newHeight];
            for (int y = 0; y < newHeight; y++)
            {
                for (int x = 0; x < newWidth; x++)
                {
                    // Map to source coordinates
                    float srcX = (float)x / newWidth * source.Width;
                    float srcY = (float)y / newHeight * source.Height;

                    // Bilinear interpolation
                    int x0 = Math.Min((int)srcX, source.Width - 1);
                    int y0 = Math.Min((int)srcY, source.Height - 1);
                    int x1 = Math.Min(x0 + 1, source.Width - 1);
                    int y1 = Math.Min(y0 + 1, source.Height - 1);

                    float xFrac = srcX - x0;
                    float yFrac = srcY - y0;

                    Color c00 = sourceData[(y0 * source.Width) + x0];
                    Color c10 = sourceData[(y0 * source.Width) + x1];
                    Color c01 = sourceData[(y1 * source.Width) + x0];
                    Color c11 = sourceData[(y1 * source.Width) + x1];

                    // Interpolate
                    float r = Lerp(Lerp(c00.R, c10.R, xFrac), Lerp(c01.R, c11.R, xFrac), yFrac);
                    float g = Lerp(Lerp(c00.G, c10.G, xFrac), Lerp(c01.G, c11.G, xFrac), yFrac);
                    float b = Lerp(Lerp(c00.B, c10.B, xFrac), Lerp(c01.B, c11.B, xFrac), yFrac);
                    float a = Lerp(Lerp(c00.A, c10.A, xFrac), Lerp(c01.A, c11.A, xFrac), yFrac);

                    scaledData[(y * newWidth) + x] = new Color((byte)r, (byte)g, (byte)b, (byte)a);
                }
            }

            Texture2D scaledTexture = new(Global.GraphicsDevice, newWidth, newHeight);
            scaledTexture.SetData(scaledData);
            return scaledTexture;
        }

        private static float Lerp(float a, float b, float t)
        {
            return a + ((b - a) * t);
        }

        public void Draw()
        {
            if (!_enabled)
            {
                if (_cursorOverrideActive)
                {
                    Global.XnaGame.IsMouseVisible = false;
                    _cursorOverrideActive = false;
                    _usingActiveCursor = false;
                }
                return;
            }

            _mouseStateOriginal = Global.XnaGame.GetMouseState();
            if (_mouseStateOriginal.X < 0 || _mouseStateOriginal.Y < 0)
            {
                return;
            }

            if (_cursor == null || _cursorActive == null)
            {
                return;
            }

            // Update scaled cursors if game scale changed
            UpdateScaledCursors(Global.ScreenSizeManager.WidthAspectRatio);

            if (_nativeCursor == null || _nativeCursorActive == null)
            {
                return;
            }

            // Only update cursor when state changes to avoid per-frame overhead
            bool isActive = _mouseStateOriginal.LeftButton == ButtonState.Pressed;
            if (!_cursorOverrideActive || isActive != _usingActiveCursor)
            {
                if (!_cursorOverrideActive)
                {
                    Global.XnaGame.IsMouseVisible = true;
                }
                Mouse.SetCursor(isActive ? _nativeCursorActive : _nativeCursor);
                _cursorOverrideActive = true;
                _usingActiveCursor = isActive;
            }
        }

        public static MouseState GetMouseState()
        {
            return TransformMouseState(Global.XnaGame.GetMouseState());
        }

        private static MouseState TransformMouseState(MouseState mouseState)
        {
            return new MouseState(Global.ScreenSizeManager.TransformWindowToViewX(mouseState.X), Global.ScreenSizeManager.TransformWindowToViewY(mouseState.Y), mouseState.ScrollWheelValue, mouseState.LeftButton, mouseState.MiddleButton, mouseState.RightButton, mouseState.XButton1, mouseState.XButton2);
        }

        public List<TouchLocation> GetTouchLocation()
        {
            List<TouchLocation> list = [];
            _mouseStateOriginal = Global.XnaGame.GetMouseState();
            MouseState mouseStateTranformed = TransformMouseState(_mouseStateOriginal);
            TouchLocation item = default;
            if (_touchID > 0)
            {
                if (mouseStateTranformed.LeftButton == ButtonState.Pressed)
                {
                    TouchLocation touchLocation;
                    if (_mouseStateTranformed.LeftButton == ButtonState.Pressed)
                    {
                        touchLocation = new TouchLocation(_touchID, TouchLocationState.Moved, new Vector2(mouseStateTranformed.X, mouseStateTranformed.Y));
                    }
                    else
                    {
                        int num = _touchID + 1;
                        _touchID = num;
                        touchLocation = new TouchLocation(num, TouchLocationState.Pressed, new Vector2(mouseStateTranformed.X, mouseStateTranformed.Y));
                    }
                    item = touchLocation;
                }
                else if (_mouseStateTranformed.LeftButton == ButtonState.Pressed)
                {
                    item = new TouchLocation(_touchID, TouchLocationState.Released, new Vector2(_mouseStateTranformed.X, _mouseStateTranformed.Y));
                }
            }
            else if (mouseStateTranformed.LeftButton == ButtonState.Pressed)
            {
                int num = _touchID + 1;
                _touchID = num;
                item = new TouchLocation(num, TouchLocationState.Pressed, new Vector2(mouseStateTranformed.X, mouseStateTranformed.Y));
            }
            if (item.State != TouchLocationState.Invalid)
            {
                list.Add(item);
            }
            _mouseStateTranformed = mouseStateTranformed;
            return GLCanvas.ConvertTouches(list);
        }

        private Texture2D _cursor;

        private Texture2D _cursorActive;

        private Texture2D _scaledCursor;

        private Texture2D _scaledCursorActive;

        private Microsoft.Xna.Framework.Input.MouseCursor _nativeCursor;

        private Microsoft.Xna.Framework.Input.MouseCursor _nativeCursorActive;

        private MouseState _mouseStateTranformed;

        private MouseState _mouseStateOriginal;

        private int _touchID;

        private double _currentScale;

        private bool _enabled;

        private bool _usingActiveCursor;

        private bool _cursorOverrideActive;
    }
}
