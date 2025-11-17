using System;
using System.Collections;
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
            _cursorTexture = cm.Load<Texture2D>("cursor");
            _cursorActiveTexture = cm.Load<Texture2D>("cursor_active");
            _cursorTextureArray = [_cursorTexture, _cursorActiveTexture];
        }

        public void Draw()
        {
            if (_enabled && _mouseStateOriginal.X >= 0 && _mouseStateOriginal.Y >= 0)
            {
                if (_cursorTexture == null || _cursorActiveTexture == null)
                {
                    return;
                }
                if (Global.XnaGame.UseNativeMouse)
                {
                    Global.XnaGame.IsMouseVisible = true;
                }
                Rectangle scaledViewRect = Global.ScreenSizeManager.ScaledViewRect;
                float gameWidth = FrameworkTypes.SCREEN_WIDTH / scaledViewRect.Width;
                float gameHeight = FrameworkTypes.SCREEN_HEIGHT / scaledViewRect.Height;
                if (scaledViewRect != previousScaledViewRect)
                {
                    _cursorArray.Clear();
                    for (int i = 0; i < _cursorTextureArray.Count; i++)
                    {
                        Texture2D mouseTexture = _cursorTextureArray[i];
                        RenderTarget2D renderTarget = new RenderTarget2D(Global.GraphicsDevice, 64, 64, false, SurfaceFormat.Color, DepthFormat.None);
                        Global.GraphicsDevice.SetRenderTarget(renderTarget);
                        Global.GraphicsDevice.Clear(Color.Transparent);
                        SpriteBatch mouseSpriteBatch = new SpriteBatch(Global.GraphicsDevice);
                        mouseSpriteBatch.Begin();
                        mouseSpriteBatch.Draw(mouseTexture, new Rectangle(0, 0, (int)((double)(mouseTexture.Width / gameWidth) * 1), (int)((double)(mouseTexture.Height / gameHeight) * 1)), Color.White); // Make the max size 64x64.
                        mouseSpriteBatch.End();
                        Global.GraphicsDevice.SetRenderTarget(null);
                        Texture2D newTexture = new Texture2D(Global.GraphicsDevice, 64, 64);
                        Color[] data = new Color[64 * 64];
                        renderTarget.GetData(data);
                        newTexture.SetData(data);
                        renderTarget.Dispose();
                        _cursorArray.Add(Microsoft.Xna.Framework.Input.MouseCursor.FromTexture2D(newTexture, 0, 0));
                    }
                }
                if (Global.XnaGame.IsMouseVisible)
                {
                    Mouse.SetCursor(_mouseStateTranformed.LeftButton == ButtonState.Pressed ? _cursorArray[1] : _cursorArray[0]);
                }
                else
                {
                    Texture2D mouseTexture = _mouseStateTranformed.LeftButton == ButtonState.Pressed ? _cursorTextureArray[1] : _cursorTextureArray[0];
                    Global.SpriteBatch.Begin(SpriteSortMode.Deferred, null, null, null, null, null, null);
                    Global.SpriteBatch.Draw(mouseTexture, new Rectangle(_mouseStateTranformed.X, _mouseStateTranformed.Y, (int)((double)(mouseTexture.Width / gameWidth) * 1), (int)((double)(mouseTexture.Height / gameHeight) * 1)), Color.White);
                    Global.SpriteBatch.End();
                }
                previousScaledViewRect = scaledViewRect;
            }
            else
            {
                Global.XnaGame.IsMouseVisible = false;
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

        private Texture2D _cursorTexture;

        private Texture2D _cursorActiveTexture;

        private List<Microsoft.Xna.Framework.Input.MouseCursor> _cursorArray = new List<Microsoft.Xna.Framework.Input.MouseCursor>();

        private List<Texture2D> _cursorTextureArray;

        private MouseState _mouseStateTranformed;

        private MouseState _mouseStateOriginal;

        private int _touchID;

        private bool _enabled;

        private Rectangle previousScaledViewRect;
    }
}
