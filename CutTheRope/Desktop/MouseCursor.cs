using System;
using System.Collections.Generic;

using CutTheRope.Framework;
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
    /// Manages native OS mouse cursor with custom texture support.
    /// Creates scaled native cursors from game textures that adapt to window size.
    /// </summary>
    internal sealed class MouseCursor : IDisposable
    {
        /// <summary>
        /// Maximum cursor size in pixels (OS limitation).
        /// </summary>
        private const int MaxCursorSize = 128;

        /// <summary>
        /// Minimum cursor size in pixels for visibility.
        /// </summary>
        private const int MinCursorSize = 16;

        /// <summary>
        /// Enables or disables the custom cursor display.
        /// </summary>
        /// <param name="b">True to show the custom cursor, false to hide it.</param>
        public void Enable(bool b)
        {
            _enabled = b;
        }

        /// <summary>
        /// Disposes all native cursor and texture resources.
        /// </summary>
        public void Dispose()
        {
            _nativeCursor?.Dispose();
            _nativeCursorActive?.Dispose();
            _scaledCursor?.Dispose();
            _scaledCursorActive?.Dispose();
            _nativeCursor = null;
            _nativeCursorActive = null;
            _scaledCursor = null;
            _scaledCursorActive = null;
        }

        /// <summary>
        /// Resets all mouse button states to released.
        /// </summary>
        public void ReleaseButtons()
        {
            _mouseStateTranformed = new MouseState(_mouseStateTranformed.X, _mouseStateTranformed.Y, _mouseStateTranformed.ScrollWheelValue, ButtonState.Released, ButtonState.Released, ButtonState.Released, ButtonState.Released, ButtonState.Released);
        }

        /// <summary>
        /// Loads cursor textures from content and prepares for native cursor creation.
        /// </summary>
        /// <param name="cm">The content manager to load textures from.</param>
        public void Load(ContentManager cm)
        {
            // Dispose old native cursors if reloading
            _nativeCursor?.Dispose();
            _nativeCursorActive?.Dispose();
            _scaledCursor?.Dispose();
            _scaledCursorActive?.Dispose();

<<<<<<< HEAD
            _cursor = cm.Load<Texture2D>(ContentPaths.GetImageContentPath("cursor"));
            _cursorActive = cm.Load<Texture2D>(ContentPaths.GetImageContentPath("cursor_active"));
            _nativeCursor = Microsoft.Xna.Framework.Input.MouseCursor.FromTexture2D(_cursor, 0, 0);
            _nativeCursorActive = Microsoft.Xna.Framework.Input.MouseCursor.FromTexture2D(_cursorActive, 0, 0);
=======
            _cursor = cm.Load<Texture2D>("cursor");
            _cursorActive = cm.Load<Texture2D>("cursor_active");

            // Create initial native cursors (will be recreated with proper scale in Draw)
            _lastViewWidth = 0;
            _lastViewHeight = 0;
            _nativeCursor = null;
            _nativeCursorActive = null;
        }

        /// <summary>
        /// Updates scaled cursor textures when the view size changes.
        /// Creates new native cursors sized proportionally to the game view.
        /// </summary>
        private void UpdateScaledCursors()
        {
            if (_cursor == null || _cursorActive == null)
            {
                return;
            }

            Rectangle viewRect = Global.ScreenSizeManager.ScaledViewRect;
            if (viewRect.Width <= 0 || viewRect.Height <= 0)
            {
                return;
            }

            if (viewRect.Width == _lastViewWidth && viewRect.Height == _lastViewHeight && _nativeCursor != null)
            {
                return;
            }

            _lastViewWidth = viewRect.Width;
            _lastViewHeight = viewRect.Height;

            // Calculate scale factor based on view size relative to game logical size
            float scaleX = viewRect.Width / FrameworkTypes.SCREEN_WIDTH;
            float scaleY = viewRect.Height / FrameworkTypes.SCREEN_HEIGHT;
            float scale = Math.Min(scaleX, scaleY);
            if (scale <= 0f)
            {
                return;
            }

            // Scale cursor to match game content scaling, but clamp to reasonable cursor sizes
            int scaledWidth = (int)(_cursor.Width * scale);
            int scaledHeight = (int)(_cursor.Height * scale);

            // Clamp to Windows cursor size limits
            if (scaledWidth > MaxCursorSize || scaledHeight > MaxCursorSize)
            {
                float clampScale = MaxCursorSize / (float)Math.Max(scaledWidth, scaledHeight);
                scaledWidth = (int)(scaledWidth * clampScale);
                scaledHeight = (int)(scaledHeight * clampScale);
            }
            if (scaledWidth < MinCursorSize || scaledHeight < MinCursorSize)
            {
                float clampScale = MinCursorSize / (float)Math.Min(scaledWidth, scaledHeight);
                scaledWidth = (int)(scaledWidth * clampScale);
                scaledHeight = (int)(scaledHeight * clampScale);
            }

            // Dispose old native cursors
            _nativeCursor?.Dispose();
            _nativeCursorActive?.Dispose();
            _scaledCursor?.Dispose();
            _scaledCursorActive?.Dispose();

            // Create scaled textures and native cursors
            _scaledCursor = ScaleTexture(_cursor, scaledWidth, scaledHeight);
            float activeWidthRatio = _cursorActive.Width / (float)_cursor.Width;
            float activeHeightRatio = _cursorActive.Height / (float)_cursor.Height;
            int scaledActiveWidth = Math.Max(1, (int)(scaledWidth * activeWidthRatio));
            int scaledActiveHeight = Math.Max(1, (int)(scaledHeight * activeHeightRatio));
            _scaledCursorActive = ScaleTexture(_cursorActive, scaledActiveWidth, scaledActiveHeight);

            _nativeCursor = Microsoft.Xna.Framework.Input.MouseCursor.FromTexture2D(_scaledCursor, 0, 0);
            _nativeCursorActive = Microsoft.Xna.Framework.Input.MouseCursor.FromTexture2D(_scaledCursorActive, 0, 0);

            // Force cursor update on next Draw
            _cursorOverrideActive = false;
        }

        /// <summary>
        /// Scales a texture to the specified dimensions using a render target.
        /// </summary>
        /// <param name="source">The source texture to scale.</param>
        /// <param name="targetWidth">The target width in pixels.</param>
        /// <param name="targetHeight">The target height in pixels.</param>
        /// <returns>A new Texture2D with the scaled content.</returns>
        private static Texture2D ScaleTexture(Texture2D source, int targetWidth, int targetHeight)
        {
            // Ensure minimum size
            targetWidth = Math.Max(1, targetWidth);
            targetHeight = Math.Max(1, targetHeight);

            RenderTarget2D renderTarget = new(Global.GraphicsDevice, targetWidth, targetHeight, false,
                SurfaceFormat.Color, DepthFormat.None, 0, RenderTargetUsage.PreserveContents);

            // Save current render target
            RenderTargetBinding[] previousTargets = Global.GraphicsDevice.GetRenderTargets();
            Viewport previousViewport = Global.GraphicsDevice.Viewport;

            Global.GraphicsDevice.SetRenderTarget(renderTarget);
            Global.GraphicsDevice.Viewport = new Viewport(0, 0, targetWidth, targetHeight);
            Global.GraphicsDevice.Clear(Color.Transparent);

            SpriteBatch spriteBatch = new(Global.GraphicsDevice);
            spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, SamplerState.LinearClamp,
                null, null, null, null);
            spriteBatch.Draw(source, new Rectangle(0, 0, targetWidth, targetHeight), Color.White);
            spriteBatch.End();
            spriteBatch.Dispose();

            // Restore previous render target
            Global.GraphicsDevice.SetRenderTargets(previousTargets);
            Global.GraphicsDevice.Viewport = previousViewport;

            // Copy to regular Texture2D
            // RenderTarget2D might cause issues with cursors
            Color[] data = new Color[targetWidth * targetHeight];
            renderTarget.GetData(data);

            Texture2D result = new(Global.GraphicsDevice, targetWidth, targetHeight);
            result.SetData(data);

            renderTarget.Dispose();
            return result;
>>>>>>> a9dfbc5 (scale mouse cursor with game view size)
        }

        /// <summary>
        /// Updates cursor state and switches between normal/active cursors based on mouse button state.
        /// Should be called each frame.
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

            // Update scaled cursors if view size changed
            UpdateScaledCursors();

            _mouseStateOriginal = Global.XnaGame.GetMouseState();
            if (_mouseStateOriginal.X < 0 || _mouseStateOriginal.Y < 0)
            {
                return;
            }

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
        /// Gets the current mouse state transformed to game view coordinates.
        /// </summary>
        /// <returns>The transformed mouse state.</returns>
        public static MouseState GetMouseState()
        {
            return TransformMouseState(Global.XnaGame.GetMouseState());
        }

        /// <summary>
        /// Transforms a mouse state from window coordinates to game view coordinates.
        /// </summary>
        /// <param name="mouseState">The mouse state in window coordinates.</param>
        /// <returns>The mouse state in game view coordinates.</returns>
        private static MouseState TransformMouseState(MouseState mouseState)
        {
            return new MouseState(Global.ScreenSizeManager.TransformWindowToViewX(mouseState.X), Global.ScreenSizeManager.TransformWindowToViewY(mouseState.Y), mouseState.ScrollWheelValue, mouseState.LeftButton, mouseState.MiddleButton, mouseState.RightButton, mouseState.XButton1, mouseState.XButton2);
        }

        /// <summary>
        /// Converts mouse input to touch locations for compatibility with touch-based game logic.
        /// </summary>
        /// <returns>A list of touch locations representing the current mouse state.</returns>
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

        private Microsoft.Xna.Framework.Input.MouseCursor _nativeCursor;

        private Microsoft.Xna.Framework.Input.MouseCursor _nativeCursorActive;

        private Texture2D _scaledCursor;

        private Texture2D _scaledCursorActive;

        private int _lastViewWidth;

        private int _lastViewHeight;

        private MouseState _mouseStateTranformed;

        private MouseState _mouseStateOriginal;

        private int _touchID;

        private bool _enabled;

        private bool _usingActiveCursor;

        private bool _cursorOverrideActive;
    }
}
