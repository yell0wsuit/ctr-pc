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
            this._enabled = b;
        }

        public void ReleaseButtons()
        {
            this._mouseStateTranformed = new MouseState(this._mouseStateTranformed.X, this._mouseStateTranformed.Y, this._mouseStateTranformed.ScrollWheelValue, Microsoft.Xna.Framework.Input.ButtonState.Released, Microsoft.Xna.Framework.Input.ButtonState.Released, Microsoft.Xna.Framework.Input.ButtonState.Released, Microsoft.Xna.Framework.Input.ButtonState.Released, Microsoft.Xna.Framework.Input.ButtonState.Released);
        }

        public void Load(ContentManager cm)
        {
            this._cursorWindows = NativeMethods.LoadCustomCursor("content/cursor_windows.cur");
            this._cursorActiveWindows = NativeMethods.LoadCustomCursor("content/cursor_active_windows.cur");
        }

        public void Draw()
        {
            if (this._enabled && !Global.XnaGame.IsMouseVisible && this._mouseStateOriginal.X >= 0 && this._mouseStateOriginal.Y >= 0)
            {
                Texture2D texture2D = (this._mouseStateTranformed.LeftButton == Microsoft.Xna.Framework.Input.ButtonState.Pressed) ? this._cursorActive : this._cursor;
                Microsoft.Xna.Framework.Rectangle scaledViewRect = Global.ScreenSizeManager.ScaledViewRect;
                float num = FrameworkTypes.SCREEN_WIDTH / (float)scaledViewRect.Width;
                float num2 = FrameworkTypes.SCREEN_HEIGHT / (float)scaledViewRect.Height;
                Global.SpriteBatch.Begin(SpriteSortMode.Deferred, null, null, null, null, null, null);
                Global.SpriteBatch.Draw(texture2D, new Microsoft.Xna.Framework.Rectangle(this._mouseStateTranformed.X, this._mouseStateTranformed.Y, (int)((double)((float)texture2D.Width / num) * 1.5), (int)((double)((float)texture2D.Height / num2) * 1.5)), Color.White);
                Global.SpriteBatch.End();
            }
        }

        public static MouseState GetMouseState()
        {
            return MouseCursor.TransformMouseState(Global.XnaGame.GetMouseState());
        }

        private static MouseState TransformMouseState(MouseState mouseState)
        {
            return new MouseState(Global.ScreenSizeManager.TransformWindowToViewX(mouseState.X), Global.ScreenSizeManager.TransformWindowToViewY(mouseState.Y), mouseState.ScrollWheelValue, mouseState.LeftButton, mouseState.MiddleButton, mouseState.RightButton, mouseState.XButton1, mouseState.XButton2);
        }

        public List<TouchLocation> GetTouchLocation()
        {
            List<TouchLocation> list = new();
            this._mouseStateOriginal = Global.XnaGame.GetMouseState();
            if (this._mouseStateOriginal.LeftButton == Microsoft.Xna.Framework.Input.ButtonState.Pressed)
            {
                Global.XnaGame.SetCursor(this._cursorActiveWindows, this._mouseStateOriginal);
            }
            else
            {
                Global.XnaGame.SetCursor(this._cursorWindows, this._mouseStateOriginal);
            }
            MouseState mouseStateTranformed = MouseCursor.TransformMouseState(this._mouseStateOriginal);
            TouchLocation item = default(TouchLocation);
            if (this._touchID > 0)
            {
                if (mouseStateTranformed.LeftButton == Microsoft.Xna.Framework.Input.ButtonState.Pressed)
                {
                    TouchLocation touchLocation;
                    if (this._mouseStateTranformed.LeftButton == Microsoft.Xna.Framework.Input.ButtonState.Pressed)
                    {
                        touchLocation = new TouchLocation(this._touchID, TouchLocationState.Moved, new Vector2((float)mouseStateTranformed.X, (float)mouseStateTranformed.Y));
                    }
                    else
                    {
                        int num = this._touchID + 1;
                        this._touchID = num;
                        touchLocation = new TouchLocation(num, TouchLocationState.Pressed, new Vector2((float)mouseStateTranformed.X, (float)mouseStateTranformed.Y));
                    }
                    item = touchLocation;
                }
                else if (this._mouseStateTranformed.LeftButton == Microsoft.Xna.Framework.Input.ButtonState.Pressed)
                {
                    item = new TouchLocation(this._touchID, TouchLocationState.Released, new Vector2((float)this._mouseStateTranformed.X, (float)this._mouseStateTranformed.Y));
                }
            }
            else if (mouseStateTranformed.LeftButton == Microsoft.Xna.Framework.Input.ButtonState.Pressed)
            {
                int num = this._touchID + 1;
                this._touchID = num;
                item = new TouchLocation(num, TouchLocationState.Pressed, new Vector2((float)mouseStateTranformed.X, (float)mouseStateTranformed.Y));
            }
            if (item.State != TouchLocationState.Invalid)
            {
                list.Add(item);
            }
            this._mouseStateTranformed = mouseStateTranformed;
            return CutTheRope.iframework.core.Application.sharedCanvas().convertTouches(list);
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
