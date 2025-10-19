using System;
using System.Collections.Generic;
using System.Windows.Forms;
using CutTheRope.iframework;
using CutTheRope.iframework.core;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Input.Touch;

namespace CutTheRope.windows
{
	// Token: 0x0200000E RID: 14
	internal class MouseCursor
	{
		// Token: 0x0600005D RID: 93 RVA: 0x000037E1 File Offset: 0x000019E1
		public void Enable(bool b)
		{
			this._enabled = b;
		}

		// Token: 0x0600005E RID: 94 RVA: 0x000037EC File Offset: 0x000019EC
		public void ReleaseButtons()
		{
			this._mouseStateTranformed = new MouseState(this._mouseStateTranformed.X, this._mouseStateTranformed.Y, this._mouseStateTranformed.ScrollWheelValue, Microsoft.Xna.Framework.Input.ButtonState.Released, Microsoft.Xna.Framework.Input.ButtonState.Released, Microsoft.Xna.Framework.Input.ButtonState.Released, Microsoft.Xna.Framework.Input.ButtonState.Released, Microsoft.Xna.Framework.Input.ButtonState.Released);
		}

		// Token: 0x0600005F RID: 95 RVA: 0x0000382A File Offset: 0x00001A2A
		public void Load(ContentManager cm)
		{
			this._cursorWindows = NativeMethods.LoadCustomCursor("content/cursor_windows.cur");
			this._cursorActiveWindows = NativeMethods.LoadCustomCursor("content/cursor_active_windows.cur");
		}

		// Token: 0x06000060 RID: 96 RVA: 0x0000384C File Offset: 0x00001A4C
		public void Draw()
		{
			if (this._enabled && !Global.XnaGame.IsMouseVisible && this._mouseStateOriginal.X >= 0 && this._mouseStateOriginal.Y >= 0)
			{
				Texture2D texture2D = ((this._mouseStateTranformed.LeftButton == Microsoft.Xna.Framework.Input.ButtonState.Pressed) ? this._cursorActive : this._cursor);
				Microsoft.Xna.Framework.Rectangle scaledViewRect = Global.ScreenSizeManager.ScaledViewRect;
				float num = FrameworkTypes.SCREEN_WIDTH / (float)scaledViewRect.Width;
				float num2 = FrameworkTypes.SCREEN_HEIGHT / (float)scaledViewRect.Height;
				Global.SpriteBatch.Begin(SpriteSortMode.Deferred, null, null, null, null, null, null);
				Global.SpriteBatch.Draw(texture2D, new Microsoft.Xna.Framework.Rectangle(this._mouseStateTranformed.X, this._mouseStateTranformed.Y, (int)((double)((float)texture2D.Width / num) * 1.5), (int)((double)((float)texture2D.Height / num2) * 1.5)), Color.White);
				Global.SpriteBatch.End();
			}
		}

		// Token: 0x06000061 RID: 97 RVA: 0x00003952 File Offset: 0x00001B52
		public static MouseState GetMouseState()
		{
			return MouseCursor.TransformMouseState(Global.XnaGame.GetMouseState());
		}

		// Token: 0x06000062 RID: 98 RVA: 0x00003964 File Offset: 0x00001B64
		private static MouseState TransformMouseState(MouseState mouseState)
		{
			return new MouseState(Global.ScreenSizeManager.TransformWindowToViewX(mouseState.X), Global.ScreenSizeManager.TransformWindowToViewY(mouseState.Y), mouseState.ScrollWheelValue, mouseState.LeftButton, mouseState.MiddleButton, mouseState.RightButton, mouseState.XButton1, mouseState.XButton2);
		}

		// Token: 0x06000063 RID: 99 RVA: 0x000039C4 File Offset: 0x00001BC4
		public List<TouchLocation> GetTouchLocation()
		{
			List<TouchLocation> list = new List<TouchLocation>();
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

		// Token: 0x04000056 RID: 86
		private Cursor _cursorWindows;

		// Token: 0x04000057 RID: 87
		private Cursor _cursorActiveWindows;

		// Token: 0x04000058 RID: 88
		private Texture2D _cursor;

		// Token: 0x04000059 RID: 89
		private Texture2D _cursorActive;

		// Token: 0x0400005A RID: 90
		private MouseState _mouseStateTranformed;

		// Token: 0x0400005B RID: 91
		private MouseState _mouseStateOriginal;

		// Token: 0x0400005C RID: 92
		private int _touchID;

		// Token: 0x0400005D RID: 93
		private bool _enabled;
	}
}
