using CutTheRope.ios;
using CutTheRope.windows;
using System;
using System.Collections.Generic;
using System.Linq;

namespace CutTheRope.iframework.visual
{
    internal class BaseElement : NSObject
    {
        // (get) Token: 0x0600015F RID: 351 RVA: 0x0000733B File Offset: 0x0000553B
        public bool HasParent
        {
            get
            {
                return this.parent != null;
            }
        }

        public bool AnchorHas(int f)
        {
            return ((int)this.anchor & f) != 0;
        }

        public bool ParentAnchorHas(int f)
        {
            return ((int)this.parentAnchor & f) != 0;
        }

        public static void calculateTopLeft(BaseElement e)
        {
            float num = e.HasParent ? e.parent.drawX : 0f;
            float num2 = e.HasParent ? e.parent.drawY : 0f;
            int num3 = e.HasParent ? e.parent.width : 0;
            int num4 = e.HasParent ? e.parent.height : 0;
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

        protected static void restoreTransformations(BaseElement t)
        {
            if (t.pushM || (double)t.rotation != 0.0 || (double)t.scaleX != 1.0 || (double)t.scaleY != 1.0 || (double)t.translateX != 0.0 || (double)t.translateY != 0.0)
            {
                OpenGL.glPopMatrix();
                t.pushM = false;
            }
        }

        private static void restoreColor(BaseElement t)
        {
            if (!RGBAColor.RGBAEqual(t.color, RGBAColor.solidOpaqueRGBA))
            {
                OpenGL.glColor4f(RGBAColor.solidOpaqueRGBA_Xna);
            }
        }

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

        public virtual void draw()
        {
            this.preDraw();
            this.postDraw();
        }

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

        public BaseElement getChildWithName(NSString n)
        {
            return this.getChildWithName(n.ToString());
        }

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

        private BaseElement createFromXML(XMLNode xml)
        {
            return new BaseElement();
        }

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

        public virtual int addChild(BaseElement c)
        {
            return this.addChildwithID(c, -1);
        }

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

        public void removeAllChilds()
        {
            this.childs.Clear();
        }

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

        public virtual BaseElement getChild(int i)
        {
            BaseElement value = null;
            this.childs.TryGetValue(i, out value);
            return value;
        }

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

        public virtual int childsCount()
        {
            return this.childs.Count;
        }

        public virtual Dictionary<int, BaseElement> getChilds()
        {
            return this.childs;
        }

        public virtual int addTimeline(Timeline t)
        {
            int count = this.timelines.Count;
            this.addTimelinewithID(t, count);
            return count;
        }

        public virtual void addTimelinewithID(Timeline t, int i)
        {
            t.element = this;
            this.timelines[i] = t;
        }

        public virtual void removeTimeline(int i)
        {
            if (this.currentTimelineIndex == i)
            {
                this.stopCurrentTimeline();
            }
            this.timelines.Remove(i);
        }

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

        public virtual void pauseCurrentTimeline()
        {
            this.currentTimeline.pauseTimeline();
        }

        public virtual void stopCurrentTimeline()
        {
            this.currentTimeline.stopTimeline();
            this.currentTimeline = null;
            this.currentTimelineIndex = -1;
        }

        public virtual Timeline getCurrentTimeline()
        {
            return this.currentTimeline;
        }

        public int getCurrentTimelineIndex()
        {
            return this.currentTimelineIndex;
        }

        public virtual Timeline getTimeline(int n)
        {
            Timeline value = null;
            this.timelines.TryGetValue(n, out value);
            return value;
        }

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

        public void setEnabled(bool e)
        {
            this.visible = e;
            this.touchable = e;
            this.updateable = e;
        }

        public bool isEnabled()
        {
            return this.visible && this.touchable && this.updateable;
        }

        public void setName(string n)
        {
            NSObject.NSREL(this.name);
            this.name = new NSString(n);
        }

        public void setName(NSString n)
        {
            NSObject.NSREL(this.name);
            this.name = n;
        }

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

        public override void dealloc()
        {
            this.childs.Clear();
            this.childs = null;
            this.timelines.Clear();
            this.timelines = null;
            NSObject.NSREL(this.name);
            base.dealloc();
        }

        public const string ACTION_SET_VISIBLE = "ACTION_SET_VISIBLE";

        public const string ACTION_SET_TOUCHABLE = "ACTION_SET_TOUCHABLE";

        public const string ACTION_SET_UPDATEABLE = "ACTION_SET_UPDATEABLE";

        public const string ACTION_PLAY_TIMELINE = "ACTION_PLAY_TIMELINE";

        public const string ACTION_PAUSE_TIMELINE = "ACTION_PAUSE_TIMELINE";

        public const string ACTION_STOP_TIMELINE = "ACTION_STOP_TIMELINE";

        public const string ACTION_JUMP_TO_TIMELINE_FRAME = "ACTION_JUMP_TO_TIMELINE_FRAME";

        private bool pushM;

        public bool visible;

        public bool touchable;

        public bool updateable;

        private NSString name;

        public float x;

        public float y;

        public float drawX;

        public float drawY;

        public int width;

        public int height;

        public float rotation;

        public float rotationCenterX;

        public float rotationCenterY;

        public float scaleX;

        public float scaleY;

        public RGBAColor color;

        private float translateX;

        private float translateY;

        public sbyte anchor;

        public sbyte parentAnchor;

        public bool passTransformationsToChilds;

        public bool passColorToChilds;

        private bool passTouchEventsToAllChilds;

        public int blendingMode;

        public BaseElement parent;

        protected Dictionary<int, BaseElement> childs;

        protected Dictionary<int, Timeline> timelines;

        private int currentTimelineIndex;

        private Timeline currentTimeline;
    }
}
