using System;
using System.Collections.Generic;
using System.Linq;
using CutTheRope.ios;
using CutTheRope.windows;

namespace CutTheRope.iframework.visual
{
	// Token: 0x0200002B RID: 43
	internal class BaseElement : NSObject
	{
		// Token: 0x17000021 RID: 33
		// (get) Token: 0x0600015F RID: 351 RVA: 0x0000733B File Offset: 0x0000553B
		public bool HasParent
		{
			get
			{
				return this.parent != null;
			}
		}

		// Token: 0x06000160 RID: 352 RVA: 0x00007346 File Offset: 0x00005546
		public bool AnchorHas(int f)
		{
			return ((int)this.anchor & f) != 0;
		}

		// Token: 0x06000161 RID: 353 RVA: 0x00007353 File Offset: 0x00005553
		public bool ParentAnchorHas(int f)
		{
			return ((int)this.parentAnchor & f) != 0;
		}

		// Token: 0x06000162 RID: 354 RVA: 0x00007360 File Offset: 0x00005560
		public static void calculateTopLeft(BaseElement e)
		{
			float num = (e.HasParent ? e.parent.drawX : 0f);
			float num2 = (e.HasParent ? e.parent.drawY : 0f);
			int num3 = (e.HasParent ? e.parent.width : 0);
			int num4 = (e.HasParent ? e.parent.height : 0);
			if (e.parentAnchor != -1)
			{
				if ((e.parentAnchor & 1) != 0)
				{
					e.drawX = num + e.x;
				}
				else if ((e.parentAnchor & 2) != 0)
				{
					e.drawX = num + e.x + (float)(num3 >> 1);
				}
				else if ((e.parentAnchor & 4) != 0)
				{
					e.drawX = num + e.x + (float)num3;
				}
				if ((e.parentAnchor & 8) != 0)
				{
					e.drawY = num2 + e.y;
				}
				else if ((e.parentAnchor & 16) != 0)
				{
					e.drawY = num2 + e.y + (float)(num4 >> 1);
				}
				else if ((e.parentAnchor & 32) != 0)
				{
					e.drawY = num2 + e.y + (float)num4;
				}
			}
			else
			{
				e.drawX = e.x;
				e.drawY = e.y;
			}
			if ((e.anchor & 8) == 0)
			{
				if ((e.anchor & 16) != 0)
				{
					e.drawY -= (float)(e.height >> 1);
				}
				else if ((e.anchor & 32) != 0)
				{
					e.drawY -= (float)e.height;
				}
			}
			if ((e.anchor & 1) == 0)
			{
				if ((e.anchor & 2) != 0)
				{
					e.drawX -= (float)(e.width >> 1);
					return;
				}
				if ((e.anchor & 4) != 0)
				{
					e.drawX -= (float)e.width;
				}
			}
		}

		// Token: 0x06000163 RID: 355 RVA: 0x00007538 File Offset: 0x00005738
		protected static void restoreTransformations(BaseElement t)
		{
			if (t.pushM || (double)t.rotation != 0.0 || (double)t.scaleX != 1.0 || (double)t.scaleY != 1.0 || (double)t.translateX != 0.0 || (double)t.translateY != 0.0)
			{
				OpenGL.glPopMatrix();
				t.pushM = false;
			}
		}

		// Token: 0x06000164 RID: 356 RVA: 0x000075B3 File Offset: 0x000057B3
		private static void restoreColor(BaseElement t)
		{
			if (!RGBAColor.RGBAEqual(t.color, RGBAColor.solidOpaqueRGBA))
			{
				OpenGL.glColor4f(RGBAColor.solidOpaqueRGBA_Xna);
			}
		}

		// Token: 0x06000165 RID: 357 RVA: 0x000075D4 File Offset: 0x000057D4
		public override NSObject init()
		{
			this.visible = true;
			this.touchable = true;
			this.updateable = true;
			this.name = null;
			this.x = 0f;
			this.y = 0f;
			this.drawX = 0f;
			this.drawY = 0f;
			this.width = 0;
			this.height = 0;
			this.rotation = 0f;
			this.rotationCenterX = 0f;
			this.rotationCenterY = 0f;
			this.scaleX = 1f;
			this.scaleY = 1f;
			this.color = RGBAColor.solidOpaqueRGBA;
			this.translateX = 0f;
			this.translateY = 0f;
			this.parentAnchor = -1;
			this.parent = null;
			this.anchor = 9;
			this.childs = new Dictionary<int, BaseElement>();
			this.timelines = new Dictionary<int, Timeline>();
			this.currentTimeline = null;
			this.currentTimelineIndex = -1;
			this.passTransformationsToChilds = true;
			this.passColorToChilds = true;
			this.passTouchEventsToAllChilds = false;
			this.blendingMode = -1;
			return this;
		}

		// Token: 0x06000166 RID: 358 RVA: 0x000076E8 File Offset: 0x000058E8
		public virtual void preDraw()
		{
			BaseElement.calculateTopLeft(this);
			bool flag = (double)this.scaleX != 1.0 || (double)this.scaleY != 1.0;
			bool flag2 = (double)this.rotation != 0.0;
			bool flag3 = (double)this.translateX != 0.0 || (double)this.translateY != 0.0;
			if (flag || flag2 || flag3)
			{
				OpenGL.glPushMatrix();
				this.pushM = true;
				if (flag || flag2)
				{
					float num = this.drawX + (float)(this.width >> 1) + this.rotationCenterX;
					float num2 = this.drawY + (float)(this.height >> 1) + this.rotationCenterY;
					OpenGL.glTranslatef(num, num2, 0f);
					if (flag2)
					{
						OpenGL.glRotatef(this.rotation, 0f, 0f, 1f);
					}
					if (flag)
					{
						OpenGL.glScalef(this.scaleX, this.scaleY, 1f);
					}
					OpenGL.glTranslatef(0f - num, 0f - num2, 0f);
				}
				if (flag3)
				{
					OpenGL.glTranslatef(this.translateX, this.translateY, 0f);
				}
			}
			if (!RGBAColor.RGBAEqual(this.color, RGBAColor.solidOpaqueRGBA))
			{
				OpenGL.glColor4f(this.color.toWhiteAlphaXNA());
			}
			if (this.blendingMode != -1)
			{
				switch (this.blendingMode)
				{
				case 0:
					OpenGL.glBlendFunc(BlendingFactor.GL_SRC_ALPHA, BlendingFactor.GL_ONE_MINUS_SRC_ALPHA);
					return;
				case 1:
					OpenGL.glBlendFunc(BlendingFactor.GL_ONE, BlendingFactor.GL_ONE_MINUS_SRC_ALPHA);
					return;
				case 2:
					OpenGL.glBlendFunc(BlendingFactor.GL_SRC_ALPHA, BlendingFactor.GL_ONE);
					break;
				default:
					return;
				}
			}
		}

		// Token: 0x06000167 RID: 359 RVA: 0x00007899 File Offset: 0x00005A99
		public virtual void draw()
		{
			this.preDraw();
			this.postDraw();
		}

		// Token: 0x06000168 RID: 360 RVA: 0x000078A8 File Offset: 0x00005AA8
		public virtual void postDraw()
		{
			if (!this.passTransformationsToChilds)
			{
				BaseElement.restoreTransformations(this);
			}
			if (!this.passColorToChilds)
			{
				BaseElement.restoreColor(this);
			}
			int num = 0;
			int num2 = 0;
			while (num < this.childs.Count)
			{
				BaseElement value;
				if (this.childs.TryGetValue(num2, out value))
				{
					if (value != null && value.visible)
					{
						value.draw();
					}
					num++;
				}
				num2++;
			}
			if (this.passTransformationsToChilds)
			{
				BaseElement.restoreTransformations(this);
			}
			if (this.passColorToChilds)
			{
				BaseElement.restoreColor(this);
			}
		}

		// Token: 0x06000169 RID: 361 RVA: 0x0000792C File Offset: 0x00005B2C
		public virtual void update(float delta)
		{
			int num = 0;
			int num2 = 0;
			while (num < this.childs.Count)
			{
				BaseElement value;
				if (this.childs.TryGetValue(num2, out value))
				{
					if (value != null && value.updateable)
					{
						value.update(delta);
					}
					num++;
				}
				num2++;
			}
			if (this.currentTimeline != null)
			{
				Timeline.updateTimeline(this.currentTimeline, delta);
			}
		}

		// Token: 0x0600016A RID: 362 RVA: 0x0000798B File Offset: 0x00005B8B
		public BaseElement getChildWithName(NSString n)
		{
			return this.getChildWithName(n.ToString());
		}

		// Token: 0x0600016B RID: 363 RVA: 0x0000799C File Offset: 0x00005B9C
		public BaseElement getChildWithName(string n)
		{
			foreach (KeyValuePair<int, BaseElement> child in this.childs)
			{
				BaseElement value = child.Value;
				if (value != null)
				{
					if (value.name != null && value.name.isEqualToString(n))
					{
						return value;
					}
					BaseElement childWithName = value.getChildWithName(n);
					if (childWithName != null)
					{
						return childWithName;
					}
				}
			}
			return null;
		}

		// Token: 0x0600016C RID: 364 RVA: 0x00007A24 File Offset: 0x00005C24
		public void setSizeToChildsBounds()
		{
			BaseElement.calculateTopLeft(this);
			float num = this.drawX;
			float num2 = this.drawY;
			float num3 = this.drawX + (float)this.width;
			float num4 = this.drawY + (float)this.height;
			foreach (KeyValuePair<int, BaseElement> child in this.childs)
			{
				BaseElement value = child.Value;
				if (value != null)
				{
					BaseElement.calculateTopLeft(value);
					if (value.drawX < num)
					{
						num = value.drawX;
					}
					if (value.drawY < num2)
					{
						num2 = value.drawY;
					}
					if (value.drawX + (float)value.width > num3)
					{
						num3 = value.drawX + (float)value.width;
					}
					if (value.drawX + (float)value.height > num4)
					{
						num4 = value.drawY + (float)value.height;
					}
				}
			}
			this.width = (int)(num3 - num);
			this.height = (int)(num4 - num2);
		}

		// Token: 0x0600016D RID: 365 RVA: 0x00007B40 File Offset: 0x00005D40
		public virtual bool handleAction(ActionData a)
		{
			if (a.actionName == "ACTION_SET_VISIBLE")
			{
				this.visible = a.actionSubParam != 0;
			}
			else if (a.actionName == "ACTION_SET_UPDATEABLE")
			{
				this.updateable = a.actionSubParam != 0;
			}
			else if (a.actionName == "ACTION_SET_TOUCHABLE")
			{
				this.touchable = a.actionSubParam != 0;
			}
			else if (a.actionName == "ACTION_PLAY_TIMELINE")
			{
				this.playTimeline(a.actionSubParam);
			}
			else if (a.actionName == "ACTION_PAUSE_TIMELINE")
			{
				this.pauseCurrentTimeline();
			}
			else if (a.actionName == "ACTION_STOP_TIMELINE")
			{
				this.stopCurrentTimeline();
			}
			else
			{
				if (!(a.actionName == "ACTION_JUMP_TO_TIMELINE_FRAME"))
				{
					return false;
				}
				this.getCurrentTimeline().jumpToTrackKeyFrame(a.actionParam, a.actionSubParam);
			}
			return true;
		}

		// Token: 0x0600016E RID: 366 RVA: 0x00007C3C File Offset: 0x00005E3C
		private BaseElement createFromXML(XMLNode xml)
		{
			return new BaseElement();
		}

		// Token: 0x0600016F RID: 367 RVA: 0x00007C44 File Offset: 0x00005E44
		private int parseAlignmentString(NSString s)
		{
			int num = 0;
			if (s.rangeOfString("LEFT").length != 0U)
			{
				num = 1;
			}
			else if (s.rangeOfString("HCENTER").length != 0U || s.isEqualToString("CENTER"))
			{
				num = 2;
			}
			else if (s.rangeOfString("RIGHT").length != 0U)
			{
				num = 4;
			}
			if (s.rangeOfString("TOP").length != 0U)
			{
				num |= 8;
			}
			else if (s.rangeOfString("VCENTER").length != 0U || s.isEqualToString("CENTER"))
			{
				num |= 16;
			}
			else if (s.rangeOfString("BOTTOM").length != 0U)
			{
				num |= 32;
			}
			return num;
		}

		// Token: 0x06000170 RID: 368 RVA: 0x00007CF6 File Offset: 0x00005EF6
		public virtual int addChild(BaseElement c)
		{
			return this.addChildwithID(c, -1);
		}

		// Token: 0x06000171 RID: 369 RVA: 0x00007D00 File Offset: 0x00005F00
		public virtual int addChildwithID(BaseElement c, int i)
		{
			c.parent = this;
			BaseElement value2;
			if (i == -1)
			{
				i = 0;
				BaseElement value;
				while (this.childs.TryGetValue(i, out value))
				{
					if (value == null)
					{
						this.childs[i] = c;
						break;
					}
					i++;
				}
				this.childs.Add(i, c);
			}
			else if (this.childs.TryGetValue(i, out value2))
			{
				if (value2 != c)
				{
					value2.dealloc();
				}
				this.childs[i] = c;
			}
			else
			{
				this.childs.Add(i, c);
			}
			return i;
		}

		// Token: 0x06000172 RID: 370 RVA: 0x00007D8C File Offset: 0x00005F8C
		public virtual void removeChildWithID(int i)
		{
			BaseElement value = null;
			if (this.childs.TryGetValue(i, out value))
			{
				if (value != null)
				{
					value.parent = null;
				}
				this.childs.Remove(i);
			}
		}

		// Token: 0x06000173 RID: 371 RVA: 0x00007DC2 File Offset: 0x00005FC2
		public void removeAllChilds()
		{
			this.childs.Clear();
		}

		// Token: 0x06000174 RID: 372 RVA: 0x00007DD0 File Offset: 0x00005FD0
		public virtual void removeChild(BaseElement c)
		{
			foreach (KeyValuePair<int, BaseElement> child in this.childs)
			{
				if (c.Equals(child.Value))
				{
					this.childs.Remove(child.Key);
					break;
				}
			}
		}

		// Token: 0x06000175 RID: 373 RVA: 0x00007E40 File Offset: 0x00006040
		public virtual BaseElement getChild(int i)
		{
			BaseElement value = null;
			this.childs.TryGetValue(i, out value);
			return value;
		}

		// Token: 0x06000176 RID: 374 RVA: 0x00007E60 File Offset: 0x00006060
		public virtual int getChildId(BaseElement c)
		{
			int result = -1;
			foreach (KeyValuePair<int, BaseElement> child in this.childs)
			{
				if (c.Equals(child.Value))
				{
					return child.Key;
				}
			}
			return result;
		}

		// Token: 0x06000177 RID: 375 RVA: 0x00007ECC File Offset: 0x000060CC
		public virtual int childsCount()
		{
			return this.childs.Count;
		}

		// Token: 0x06000178 RID: 376 RVA: 0x00007ED9 File Offset: 0x000060D9
		public virtual Dictionary<int, BaseElement> getChilds()
		{
			return this.childs;
		}

		// Token: 0x06000179 RID: 377 RVA: 0x00007EE4 File Offset: 0x000060E4
		public virtual int addTimeline(Timeline t)
		{
			int count = this.timelines.Count;
			this.addTimelinewithID(t, count);
			return count;
		}

		// Token: 0x0600017A RID: 378 RVA: 0x00007F06 File Offset: 0x00006106
		public virtual void addTimelinewithID(Timeline t, int i)
		{
			t.element = this;
			this.timelines[i] = t;
		}

		// Token: 0x0600017B RID: 379 RVA: 0x00007F1C File Offset: 0x0000611C
		public virtual void removeTimeline(int i)
		{
			if (this.currentTimelineIndex == i)
			{
				this.stopCurrentTimeline();
			}
			this.timelines.Remove(i);
		}

		// Token: 0x0600017C RID: 380 RVA: 0x00007F3C File Offset: 0x0000613C
		public virtual void playTimeline(int t)
		{
			Timeline value = null;
			this.timelines.TryGetValue(t, out value);
			if (value != null)
			{
				if (this.currentTimeline != null && this.currentTimeline.state != Timeline.TimelineState.TIMELINE_STOPPED)
				{
					this.currentTimeline.stopTimeline();
				}
				this.currentTimelineIndex = t;
				this.currentTimeline = value;
				this.currentTimeline.playTimeline();
			}
		}

		// Token: 0x0600017D RID: 381 RVA: 0x00007F96 File Offset: 0x00006196
		public virtual void pauseCurrentTimeline()
		{
			this.currentTimeline.pauseTimeline();
		}

		// Token: 0x0600017E RID: 382 RVA: 0x00007FA3 File Offset: 0x000061A3
		public virtual void stopCurrentTimeline()
		{
			this.currentTimeline.stopTimeline();
			this.currentTimeline = null;
			this.currentTimelineIndex = -1;
		}

		// Token: 0x0600017F RID: 383 RVA: 0x00007FBE File Offset: 0x000061BE
		public virtual Timeline getCurrentTimeline()
		{
			return this.currentTimeline;
		}

		// Token: 0x06000180 RID: 384 RVA: 0x00007FC6 File Offset: 0x000061C6
		public int getCurrentTimelineIndex()
		{
			return this.currentTimelineIndex;
		}

		// Token: 0x06000181 RID: 385 RVA: 0x00007FD0 File Offset: 0x000061D0
		public virtual Timeline getTimeline(int n)
		{
			Timeline value = null;
			this.timelines.TryGetValue(n, out value);
			return value;
		}

		// Token: 0x06000182 RID: 386 RVA: 0x00007FF0 File Offset: 0x000061F0
		public virtual bool onTouchDownXY(float tx, float ty)
		{
			bool flag = false;
			foreach (KeyValuePair<int, BaseElement> item in this.childs.Reverse<KeyValuePair<int, BaseElement>>())
			{
				BaseElement value = item.Value;
				if (value != null && value.touchable && value.onTouchDownXY(tx, ty) && !flag)
				{
					flag = true;
					if (!this.passTouchEventsToAllChilds)
					{
						return flag;
					}
				}
			}
			return flag;
		}

		// Token: 0x06000183 RID: 387 RVA: 0x00008070 File Offset: 0x00006270
		public virtual bool onTouchUpXY(float tx, float ty)
		{
			bool flag = false;
			foreach (KeyValuePair<int, BaseElement> item in this.childs.Reverse<KeyValuePair<int, BaseElement>>())
			{
				BaseElement value = item.Value;
				if (value != null && value.touchable && value.onTouchUpXY(tx, ty) && !flag)
				{
					flag = true;
					if (!this.passTouchEventsToAllChilds)
					{
						return flag;
					}
				}
			}
			return flag;
		}

		// Token: 0x06000184 RID: 388 RVA: 0x000080F0 File Offset: 0x000062F0
		public virtual bool onTouchMoveXY(float tx, float ty)
		{
			bool flag = false;
			foreach (KeyValuePair<int, BaseElement> item in this.childs.Reverse<KeyValuePair<int, BaseElement>>())
			{
				BaseElement value = item.Value;
				if (value != null && value.touchable && value.onTouchMoveXY(tx, ty) && !flag)
				{
					flag = true;
					if (!this.passTouchEventsToAllChilds)
					{
						return flag;
					}
				}
			}
			return flag;
		}

		// Token: 0x06000185 RID: 389 RVA: 0x00008170 File Offset: 0x00006370
		public void setEnabled(bool e)
		{
			this.visible = e;
			this.touchable = e;
			this.updateable = e;
		}

		// Token: 0x06000186 RID: 390 RVA: 0x00008187 File Offset: 0x00006387
		public bool isEnabled()
		{
			return this.visible && this.touchable && this.updateable;
		}

		// Token: 0x06000187 RID: 391 RVA: 0x000081A1 File Offset: 0x000063A1
		public void setName(string n)
		{
			NSObject.NSREL(this.name);
			this.name = new NSString(n);
		}

		// Token: 0x06000188 RID: 392 RVA: 0x000081BA File Offset: 0x000063BA
		public void setName(NSString n)
		{
			NSObject.NSREL(this.name);
			this.name = n;
		}

		// Token: 0x06000189 RID: 393 RVA: 0x000081D0 File Offset: 0x000063D0
		public virtual void show()
		{
			foreach (KeyValuePair<int, BaseElement> child in this.childs)
			{
				BaseElement value = child.Value;
				if (value != null && value.visible)
				{
					value.show();
				}
			}
		}

		// Token: 0x0600018A RID: 394 RVA: 0x00008238 File Offset: 0x00006438
		public virtual void hide()
		{
			foreach (KeyValuePair<int, BaseElement> child in this.childs)
			{
				BaseElement value = child.Value;
				if (value != null && value.visible)
				{
					value.hide();
				}
			}
		}

		// Token: 0x0600018B RID: 395 RVA: 0x000082A0 File Offset: 0x000064A0
		public override void dealloc()
		{
			this.childs.Clear();
			this.childs = null;
			this.timelines.Clear();
			this.timelines = null;
			NSObject.NSREL(this.name);
			base.dealloc();
		}

		// Token: 0x040000FD RID: 253
		public const string ACTION_SET_VISIBLE = "ACTION_SET_VISIBLE";

		// Token: 0x040000FE RID: 254
		public const string ACTION_SET_TOUCHABLE = "ACTION_SET_TOUCHABLE";

		// Token: 0x040000FF RID: 255
		public const string ACTION_SET_UPDATEABLE = "ACTION_SET_UPDATEABLE";

		// Token: 0x04000100 RID: 256
		public const string ACTION_PLAY_TIMELINE = "ACTION_PLAY_TIMELINE";

		// Token: 0x04000101 RID: 257
		public const string ACTION_PAUSE_TIMELINE = "ACTION_PAUSE_TIMELINE";

		// Token: 0x04000102 RID: 258
		public const string ACTION_STOP_TIMELINE = "ACTION_STOP_TIMELINE";

		// Token: 0x04000103 RID: 259
		public const string ACTION_JUMP_TO_TIMELINE_FRAME = "ACTION_JUMP_TO_TIMELINE_FRAME";

		// Token: 0x04000104 RID: 260
		private bool pushM;

		// Token: 0x04000105 RID: 261
		public bool visible;

		// Token: 0x04000106 RID: 262
		public bool touchable;

		// Token: 0x04000107 RID: 263
		public bool updateable;

		// Token: 0x04000108 RID: 264
		private NSString name;

		// Token: 0x04000109 RID: 265
		public float x;

		// Token: 0x0400010A RID: 266
		public float y;

		// Token: 0x0400010B RID: 267
		public float drawX;

		// Token: 0x0400010C RID: 268
		public float drawY;

		// Token: 0x0400010D RID: 269
		public int width;

		// Token: 0x0400010E RID: 270
		public int height;

		// Token: 0x0400010F RID: 271
		public float rotation;

		// Token: 0x04000110 RID: 272
		public float rotationCenterX;

		// Token: 0x04000111 RID: 273
		public float rotationCenterY;

		// Token: 0x04000112 RID: 274
		public float scaleX;

		// Token: 0x04000113 RID: 275
		public float scaleY;

		// Token: 0x04000114 RID: 276
		public RGBAColor color;

		// Token: 0x04000115 RID: 277
		private float translateX;

		// Token: 0x04000116 RID: 278
		private float translateY;

		// Token: 0x04000117 RID: 279
		public sbyte anchor;

		// Token: 0x04000118 RID: 280
		public sbyte parentAnchor;

		// Token: 0x04000119 RID: 281
		public bool passTransformationsToChilds;

		// Token: 0x0400011A RID: 282
		public bool passColorToChilds;

		// Token: 0x0400011B RID: 283
		private bool passTouchEventsToAllChilds;

		// Token: 0x0400011C RID: 284
		public int blendingMode;

		// Token: 0x0400011D RID: 285
		public BaseElement parent;

		// Token: 0x0400011E RID: 286
		protected Dictionary<int, BaseElement> childs;

		// Token: 0x0400011F RID: 287
		protected Dictionary<int, Timeline> timelines;

		// Token: 0x04000120 RID: 288
		private int currentTimelineIndex;

		// Token: 0x04000121 RID: 289
		private Timeline currentTimeline;
	}
}
