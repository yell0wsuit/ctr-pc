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
            // Dispose old native cursors if reloading
            _nativeCursor?.Dispose();
            _nativeCursorActive?.Dispose();

            _cursor = cm.Load<Texture2D>(ContentPaths.GetImageContentPath("cursor"));
            _cursorActive = cm.Load<Texture2D>(ContentPaths.GetImageContentPath("cursor_active"));
            _nativeCursor = Microsoft.Xna.Framework.Input.MouseCursor.FromTexture2D(_cursor, 0, 0);
            _nativeCursorActive = Microsoft.Xna.Framework.Input.MouseCursor.FromTexture2D(_cursorActive, 0, 0);
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

        private Microsoft.Xna.Framework.Input.MouseCursor _nativeCursor;

        private Microsoft.Xna.Framework.Input.MouseCursor _nativeCursorActive;

        private MouseState _mouseStateTranformed;

        private MouseState _mouseStateOriginal;

        private int _touchID;

        private bool _enabled;

        private bool _usingActiveCursor;

        private bool _cursorOverrideActive;
    }
}
