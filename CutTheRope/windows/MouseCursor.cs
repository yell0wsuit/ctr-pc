using CutTheRope.iframework;
using CutTheRope.iframework.core;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Input.Touch;
using System;
using System.Collections.Generic;
using System.Windows.Forms;

namespace CutTheRope.windows
{
    internal class MouseCursor
    {
        public void Enable(bool b)
        {
            _enabled = b;
        }

        public void ReleaseButtons()
        {
            _mouseStateTranformed = new MouseState(_mouseStateTranformed.X, _mouseStateTranformed.Y, _mouseStateTranformed.ScrollWheelValue, Microsoft.Xna.Framework.Input.ButtonState.Released, Microsoft.Xna.Framework.Input.ButtonState.Released, Microsoft.Xna.Framework.Input.ButtonState.Released, Microsoft.Xna.Framework.Input.ButtonState.Released, Microsoft.Xna.Framework.Input.ButtonState.Released);
        }

        public void Load(ContentManager cm)
        {
            _cursorWindows = NativeMethods.LoadCustomCursor("content/cursor_windows.cur");
            _cursorActiveWindows = NativeMethods.LoadCustomCursor("content/cursor_active_windows.cur");
        }

        public void Draw()
        {
            if (_enabled && !Global.XnaGame.IsMouseVisible && _mouseStateOriginal.X >= 0 && _mouseStateOriginal.Y >= 0)
            {
                Texture2D texture2D = (_mouseStateTranformed.LeftButton == Microsoft.Xna.Framework.Input.ButtonState.Pressed) ? _cursorActive : _cursor;
                Rectangle scaledViewRect = Global.ScreenSizeManager.ScaledViewRect;
                float num = FrameworkTypes.SCREEN_WIDTH / (float)scaledViewRect.Width;
                float num2 = FrameworkTypes.SCREEN_HEIGHT / (float)scaledViewRect.Height;
                Global.SpriteBatch.Begin(SpriteSortMode.Deferred, null, null, null, null, null, null);
                Global.SpriteBatch.Draw(texture2D, new Rectangle(_mouseStateTranformed.X, _mouseStateTranformed.Y, (int)((double)((float)texture2D.Width / num) * 1.5), (int)((double)((float)texture2D.Height / num2) * 1.5)), Color.White);
                Global.SpriteBatch.End();
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
            List<TouchLocation> list = new();
            _mouseStateOriginal = Global.XnaGame.GetMouseState();
            if (_mouseStateOriginal.LeftButton == Microsoft.Xna.Framework.Input.ButtonState.Pressed)
            {
                Global.XnaGame.SetCursor(_cursorActiveWindows, _mouseStateOriginal);
            }
            else
            {
                Global.XnaGame.SetCursor(_cursorWindows, _mouseStateOriginal);
            }
            MouseState mouseStateTranformed = TransformMouseState(_mouseStateOriginal);
            TouchLocation item = default(TouchLocation);
            if (_touchID > 0)
            {
                if (mouseStateTranformed.LeftButton == Microsoft.Xna.Framework.Input.ButtonState.Pressed)
                {
                    TouchLocation touchLocation;
                    if (_mouseStateTranformed.LeftButton == Microsoft.Xna.Framework.Input.ButtonState.Pressed)
                    {
                        touchLocation = new TouchLocation(_touchID, TouchLocationState.Moved, new Vector2((float)mouseStateTranformed.X, (float)mouseStateTranformed.Y));
                    }
                    else
                    {
                        int num = _touchID + 1;
                        _touchID = num;
                        touchLocation = new TouchLocation(num, TouchLocationState.Pressed, new Vector2((float)mouseStateTranformed.X, (float)mouseStateTranformed.Y));
                    }
                    item = touchLocation;
                }
                else if (_mouseStateTranformed.LeftButton == Microsoft.Xna.Framework.Input.ButtonState.Pressed)
                {
                    item = new TouchLocation(_touchID, TouchLocationState.Released, new Vector2((float)_mouseStateTranformed.X, (float)_mouseStateTranformed.Y));
                }
            }
            else if (mouseStateTranformed.LeftButton == Microsoft.Xna.Framework.Input.ButtonState.Pressed)
            {
                int num = _touchID + 1;
                _touchID = num;
                item = new TouchLocation(num, TouchLocationState.Pressed, new Vector2((float)mouseStateTranformed.X, (float)mouseStateTranformed.Y));
            }
            if (item.State != TouchLocationState.Invalid)
            {
                list.Add(item);
            }
            _mouseStateTranformed = mouseStateTranformed;
            return iframework.core.Application.sharedCanvas().convertTouches(list);
        }

        private Cursor _cursorWindows;

        private Cursor _cursorActiveWindows;

        private Texture2D _cursor;

        private Texture2D _cursorActive;

        private MouseState _mouseStateTranformed;

        private MouseState _mouseStateOriginal;

        private int _touchID;

        private bool _enabled;
    }
}
