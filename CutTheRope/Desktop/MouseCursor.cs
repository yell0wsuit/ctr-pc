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
    /// <summary>
    /// Manages the desktop mouse cursor, including scaling and native cursor overrides.
    /// </summary>
    internal sealed class MouseCursor : IDisposable
    {
        /// <summary>
        /// Enables or disables cursor rendering and native cursor overrides.
        /// </summary>
        /// <param name="b"><see langword="true"/> to enable the custom cursor, <see langword="false"/> to hide it.</param>
        public void Enable(bool b)
        {
            _enabled = b;
        }

        /// <summary>
        /// Releases cursor-related native and managed resources.
        /// </summary>
        public void Dispose()
        {
            DisposeNativeCursors();
            _scaledCursor?.Dispose();
            _scaledCursorActive?.Dispose();
            _scaledCursor = null;
            _scaledCursorActive = null;
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Disposes any native cursor handles created from the scaled cursor textures.
        /// </summary>
        private void DisposeNativeCursors()
        {
            _nativeCursor?.Dispose();
            _nativeCursorActive?.Dispose();
            _nativeCursor = null;
            _nativeCursorActive = null;
        }

        /// <summary>
        /// Releases any pressed mouse buttons tracked by the cursor.
        /// </summary>
        public void ReleaseButtons()
        {
            _mouseStateTransformed = new MouseState(_mouseStateTransformed.X, _mouseStateTransformed.Y, _mouseStateTransformed.ScrollWheelValue, ButtonState.Released, ButtonState.Released, ButtonState.Released, ButtonState.Released, ButtonState.Released);
        }

        /// <summary>
        /// Loads cursor textures from content and resets scaled caches.
        /// </summary>
        /// <param name="cm">Content manager used to load cursor assets.</param>
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

        /// <summary>
        /// Rebuilds the scaled cursor textures and native cursor handles when the UI <paramref name="scale"/> changes.
        /// </summary>
        /// <param name="scale">The screen scale factor to apply to the cursor textures.</param>
        private void UpdateScaledCursors(double scale)
        {
            if (Math.Abs(_currentScale - scale) < 0.01)
            {
                return;
            }

            _currentScale = scale;

            // Create new scaled textures first (before disposing old ones for exception safety)
            Texture2D newScaledCursor = ScaleTexture(_cursor, scale);
            Texture2D newScaledCursorActive = ScaleTexture(_cursorActive, scale);

            // Create native cursors from scaled textures (hotspot at top-left corner: 0, 0)
            Microsoft.Xna.Framework.Input.MouseCursor newNativeCursor = Microsoft.Xna.Framework.Input.MouseCursor.FromTexture2D(newScaledCursor, 0, 0);
            Microsoft.Xna.Framework.Input.MouseCursor newNativeCursorActive = Microsoft.Xna.Framework.Input.MouseCursor.FromTexture2D(newScaledCursorActive, 0, 0);

            // Now dispose old resources (safe since new ones are ready)
            DisposeNativeCursors();
            _scaledCursor?.Dispose();
            _scaledCursorActive?.Dispose();

            // Assign new resources
            _scaledCursor = newScaledCursor;
            _scaledCursorActive = newScaledCursorActive;
            _nativeCursor = newNativeCursor;
            _nativeCursorActive = newNativeCursorActive;

            // Force cursor update
            _usingActiveCursor = !_usingActiveCursor;
        }

        /// <summary>
        /// Creates a scaled copy of a cursor texture using bilinear interpolation.
        /// </summary>
        /// <param name="source">The source texture to scale.</param>
        /// <param name="scale">The scale factor to apply.</param>
        /// <returns>A newly allocated scaled texture.</returns>
        private static Texture2D ScaleTexture(Texture2D source, double scale)
        {
            ArgumentNullException.ThrowIfNull(source);

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

        /// <summary>
        /// Linearly interpolates between two values.
        /// </summary>
        /// <param name="a">The starting value.</param>
        /// <param name="b">The ending value.</param>
        /// <param name="t">The interpolation factor.</param>
        /// <returns>The interpolated value.</returns>
        private static float Lerp(float a, float b, float t)
        {
            return a + ((b - a) * t);
        }

        /// <summary>
        /// Updates the native cursor based on current mouse state and screen scale.
        /// </summary>
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

        /// <summary>
        /// Gets the current mouse state transformed into view coordinates.
        /// </summary>
        /// <returns>The transformed mouse state.</returns>
        public static MouseState GetMouseState()
        {
            return TransformMouseState(Global.XnaGame.GetMouseState());
        }

        /// <summary>
        /// Converts a window-space mouse state into the game's logical view space.
        /// </summary>
        /// <param name="mouseState">The source mouse state in window coordinates.</param>
        /// <returns>The transformed mouse state.</returns>
        private static MouseState TransformMouseState(MouseState mouseState)
        {
            return new MouseState(Global.ScreenSizeManager.TransformWindowToViewX(mouseState.X), Global.ScreenSizeManager.TransformWindowToViewY(mouseState.Y), mouseState.ScrollWheelValue, mouseState.LeftButton, mouseState.MiddleButton, mouseState.RightButton, mouseState.XButton1, mouseState.XButton2);
        }

        /// <summary>
        /// Translates mouse button activity into touch events for desktop input emulation.
        /// </summary>
        /// <returns>The current touch locations derived from mouse input.</returns>
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
                    if (_mouseStateTransformed.LeftButton == ButtonState.Pressed)
                    {
                        touchLocation = new TouchLocation(_touchID, TouchLocationState.Moved, new Vector2(mouseStateTranformed.X, mouseStateTranformed.Y));
                    }
                    else
                    {
                        int nextTouchId = _touchID + 1;
                        _touchID = nextTouchId;
                        touchLocation = new TouchLocation(nextTouchId, TouchLocationState.Pressed, new Vector2(mouseStateTranformed.X, mouseStateTranformed.Y));
                    }
                    item = touchLocation;
                }
                else if (_mouseStateTransformed.LeftButton == ButtonState.Pressed)
                {
                    item = new TouchLocation(_touchID, TouchLocationState.Released, new Vector2(_mouseStateTransformed.X, _mouseStateTransformed.Y));
                }
            }
            else if (mouseStateTranformed.LeftButton == ButtonState.Pressed)
            {
                int nextTouchId = _touchID + 1;
                _touchID = nextTouchId;
                item = new TouchLocation(nextTouchId, TouchLocationState.Pressed, new Vector2(mouseStateTranformed.X, mouseStateTranformed.Y));
            }
            if (item.State != TouchLocationState.Invalid)
            {
                list.Add(item);
            }
            _mouseStateTransformed = mouseStateTranformed;
            return GLCanvas.ConvertTouches(list);
        }

        /// <summary>
        /// The base cursor texture used when the mouse button is not pressed.
        /// </summary>
        private Texture2D _cursor;

        /// <summary>
        /// The cursor texture used while the primary mouse button is pressed.
        /// </summary>
        private Texture2D _cursorActive;

        /// <summary>
        /// The cached scaled version of <see cref="_cursor"/>.
        /// </summary>
        private Texture2D _scaledCursor;

        /// <summary>
        /// The cached scaled version of <see cref="_cursorActive"/>.
        /// </summary>
        private Texture2D _scaledCursorActive;

        /// <summary>
        /// The native cursor handle created from <see cref="_scaledCursor"/>.
        /// </summary>
        private Microsoft.Xna.Framework.Input.MouseCursor _nativeCursor;

        /// <summary>
        /// The native cursor handle created from <see cref="_scaledCursorActive"/>.
        /// </summary>
        private Microsoft.Xna.Framework.Input.MouseCursor _nativeCursorActive;

        /// <summary>
        /// The last transformed mouse state used for touch emulation.
        /// </summary>
        private MouseState _mouseStateTransformed;

        /// <summary>
        /// The last raw mouse state read from the platform window.
        /// </summary>
        private MouseState _mouseStateOriginal;

        /// <summary>
        /// Tracks the current synthetic touch identifier for mouse-driven touch events.
        /// </summary>
        private int _touchID;

        /// <summary>
        /// Stores the screen scale used to build the current cursor textures.
        /// </summary>
        private double _currentScale;

        /// <summary>
        /// Indicates whether the custom cursor system is enabled.
        /// </summary>
        private bool _enabled;

        /// <summary>
        /// Indicates whether the active cursor variant is currently selected.
        /// </summary>
        private bool _usingActiveCursor;

        /// <summary>
        /// Indicates whether a native cursor override is currently applied.
        /// </summary>
        private bool _cursorOverrideActive;
    }
}
