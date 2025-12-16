using System.Collections.Generic;

using CutTheRope.Framework;
using CutTheRope.Framework.Platform;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Input.Touch;

namespace CutTheRope.Desktop
{
    internal sealed class MouseCursor
    {
        public void Enable(bool b)
        {
            _enabled = b;
        }

        public void ReleaseButtons()
        {
            _mouseStateTranformed = new MouseState(_mouseStateTranformed.X, _mouseStateTranformed.Y, _mouseStateTranformed.ScrollWheelValue, ButtonState.Released, ButtonState.Released, ButtonState.Released, ButtonState.Released, ButtonState.Released);
        }

        public void Load(ContentManager cm)
        {
            _cursor = cm.Load<Texture2D>("cursor");
            _cursorActive = cm.Load<Texture2D>("cursor_active");
        }

        public void Draw()
        {
            if (_enabled && !Global.XnaGame.IsMouseVisible && _mouseStateOriginal.X >= 0 && _mouseStateOriginal.Y >= 0)
            {
                if (_cursor == null || _cursorActive == null)
                {
                    return;
                }
                Texture2D texture2D = _mouseStateTranformed.LeftButton == ButtonState.Pressed ? _cursorActive : _cursor;
                Rectangle scaledViewRect = Global.ScreenSizeManager.ScaledViewRect;
                float num = FrameworkTypes.SCREEN_WIDTH / scaledViewRect.Width;
                float num2 = FrameworkTypes.SCREEN_HEIGHT / scaledViewRect.Height;
                Global.SpriteBatch.Begin(SpriteSortMode.Deferred, null, null, null, null, null, null);
                Global.SpriteBatch.Draw(texture2D, new Rectangle(_mouseStateTranformed.X, _mouseStateTranformed.Y, (int)((double)(texture2D.Width / num) * 1), (int)((double)(texture2D.Height / num2) * 1)), Color.White);
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

        private MouseState _mouseStateTranformed;

        private MouseState _mouseStateOriginal;

        private int _touchID;

        private bool _enabled;
    }
}
