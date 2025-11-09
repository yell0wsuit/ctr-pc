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
                return parent != null;
            }
        }

        public bool AnchorHas(int f)
        {
            return ((int)anchor & f) != 0;
        }

        public bool ParentAnchorHas(int f)
        {
            return ((int)parentAnchor & f) != 0;
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
            visible = true;
            touchable = true;
            updateable = true;
            name = null;
            x = 0f;
            y = 0f;
            drawX = 0f;
            drawY = 0f;
            width = 0;
            height = 0;
            rotation = 0f;
            rotationCenterX = 0f;
            rotationCenterY = 0f;
            scaleX = 1f;
            scaleY = 1f;
            color = RGBAColor.solidOpaqueRGBA;
            translateX = 0f;
            translateY = 0f;
            parentAnchor = -1;
            parent = null;
            anchor = 9;
            childs = new Dictionary<int, BaseElement>();
            timelines = new Dictionary<int, Timeline>();
            currentTimeline = null;
            currentTimelineIndex = -1;
            passTransformationsToChilds = true;
            passColorToChilds = true;
            passTouchEventsToAllChilds = false;
            blendingMode = -1;
            return this;
        }

        public virtual void preDraw()
        {
            calculateTopLeft(this);
            bool flag = (double)scaleX != 1.0 || (double)scaleY != 1.0;
            bool flag2 = (double)rotation != 0.0;
            bool flag3 = (double)translateX != 0.0 || (double)translateY != 0.0;
            if (flag || flag2 || flag3)
            {
                OpenGL.glPushMatrix();
                pushM = true;
                if (flag || flag2)
                {
                    float num = drawX + (float)(width >> 1) + rotationCenterX;
                    float num2 = drawY + (float)(height >> 1) + rotationCenterY;
                    OpenGL.glTranslatef(num, num2, 0f);
                    if (flag2)
                    {
                        OpenGL.glRotatef(rotation, 0f, 0f, 1f);
                    }
                    if (flag)
                    {
                        OpenGL.glScalef(scaleX, scaleY, 1f);
                    }
                    OpenGL.glTranslatef(0f - num, 0f - num2, 0f);
                }
                if (flag3)
                {
                    OpenGL.glTranslatef(translateX, translateY, 0f);
                }
            }
            if (!RGBAColor.RGBAEqual(color, RGBAColor.solidOpaqueRGBA))
            {
                OpenGL.glColor4f(color.toWhiteAlphaXNA());
            }
            if (blendingMode != -1)
            {
                switch (blendingMode)
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
            preDraw();
            postDraw();
        }

        public virtual void postDraw()
        {
            if (!passTransformationsToChilds)
            {
                restoreTransformations(this);
            }
            if (!passColorToChilds)
            {
                restoreColor(this);
            }
            int num = 0;
            int num2 = 0;
            while (num < childs.Count)
            {
                BaseElement value;
                if (childs.TryGetValue(num2, out value))
                {
                    if (value != null && value.visible)
                    {
                        value.draw();
                    }
                    num++;
                }
                num2++;
            }
            if (passTransformationsToChilds)
            {
                restoreTransformations(this);
            }
            if (passColorToChilds)
            {
                restoreColor(this);
            }
        }

        public virtual void update(float delta)
        {
            int num = 0;
            int num2 = 0;
            while (num < childs.Count)
            {
                BaseElement value;
                if (childs.TryGetValue(num2, out value))
                {
                    if (value != null && value.updateable)
                    {
                        value.update(delta);
                    }
                    num++;
                }
                num2++;
            }
            if (currentTimeline != null)
            {
                Timeline.updateTimeline(currentTimeline, delta);
            }
        }

        public BaseElement getChildWithName(NSString n)
        {
            return getChildWithName(n.ToString());
        }

        public BaseElement getChildWithName(string n)
        {
            foreach (KeyValuePair<int, BaseElement> child in childs)
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
            calculateTopLeft(this);
            float num = drawX;
            float num2 = drawY;
            float num3 = drawX + (float)width;
            float num4 = drawY + (float)height;
            foreach (KeyValuePair<int, BaseElement> child in childs)
            {
                BaseElement value = child.Value;
                if (value != null)
                {
                    calculateTopLeft(value);
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
            width = (int)(num3 - num);
            height = (int)(num4 - num2);
        }

        public virtual bool handleAction(ActionData a)
        {
            if (a.actionName == "ACTION_SET_VISIBLE")
            {
                visible = a.actionSubParam != 0;
            }
            else if (a.actionName == "ACTION_SET_UPDATEABLE")
            {
                updateable = a.actionSubParam != 0;
            }
            else if (a.actionName == "ACTION_SET_TOUCHABLE")
            {
                touchable = a.actionSubParam != 0;
            }
            else if (a.actionName == "ACTION_PLAY_TIMELINE")
            {
                playTimeline(a.actionSubParam);
            }
            else if (a.actionName == "ACTION_PAUSE_TIMELINE")
            {
                pauseCurrentTimeline();
            }
            else if (a.actionName == "ACTION_STOP_TIMELINE")
            {
                stopCurrentTimeline();
            }
            else
            {
                if (!(a.actionName == "ACTION_JUMP_TO_TIMELINE_FRAME"))
                {
                    return false;
                }
                getCurrentTimeline().jumpToTrackKeyFrame(a.actionParam, a.actionSubParam);
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
            return addChildwithID(c, -1);
        }

        public virtual int addChildwithID(BaseElement c, int i)
        {
            c.parent = this;
            BaseElement value2;
            if (i == -1)
            {
                i = 0;
                BaseElement value;
                while (childs.TryGetValue(i, out value))
                {
                    if (value == null)
                    {
                        childs[i] = c;
                        break;
                    }
                    i++;
                }
                childs.Add(i, c);
            }
            else if (childs.TryGetValue(i, out value2))
            {
                if (value2 != c)
                {
                    value2.dealloc();
                }
                childs[i] = c;
            }
            else
            {
                childs.Add(i, c);
            }
            return i;
        }

        public virtual void removeChildWithID(int i)
        {
            BaseElement value = null;
            if (childs.TryGetValue(i, out value))
            {
                if (value != null)
                {
                    value.parent = null;
                }
                childs.Remove(i);
            }
        }

        public void removeAllChilds()
        {
            childs.Clear();
        }

        public virtual void removeChild(BaseElement c)
        {
            foreach (KeyValuePair<int, BaseElement> child in childs)
            {
                if (c.Equals(child.Value))
                {
                    childs.Remove(child.Key);
                    break;
                }
            }
        }

        public virtual BaseElement getChild(int i)
        {
            BaseElement value = null;
            childs.TryGetValue(i, out value);
            return value;
        }

        public virtual int getChildId(BaseElement c)
        {
            int result = -1;
            foreach (KeyValuePair<int, BaseElement> child in childs)
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
            return childs.Count;
        }

        public virtual Dictionary<int, BaseElement> getChilds()
        {
            return childs;
        }

        public virtual int addTimeline(Timeline t)
        {
            int count = timelines.Count;
            addTimelinewithID(t, count);
            return count;
        }

        public virtual void addTimelinewithID(Timeline t, int i)
        {
            t.element = this;
            timelines[i] = t;
        }

        public virtual void removeTimeline(int i)
        {
            if (currentTimelineIndex == i)
            {
                stopCurrentTimeline();
            }
            timelines.Remove(i);
        }

        public virtual void playTimeline(int t)
        {
            Timeline value = null;
            timelines.TryGetValue(t, out value);
            if (value != null)
            {
                if (currentTimeline != null && currentTimeline.state != Timeline.TimelineState.TIMELINE_STOPPED)
                {
                    currentTimeline.stopTimeline();
                }
                currentTimelineIndex = t;
                currentTimeline = value;
                currentTimeline.playTimeline();
            }
        }

        public virtual void pauseCurrentTimeline()
        {
            currentTimeline.pauseTimeline();
        }

        public virtual void stopCurrentTimeline()
        {
            currentTimeline.stopTimeline();
            currentTimeline = null;
            currentTimelineIndex = -1;
        }

        public virtual Timeline getCurrentTimeline()
        {
            return currentTimeline;
        }

        public int getCurrentTimelineIndex()
        {
            return currentTimelineIndex;
        }

        public virtual Timeline getTimeline(int n)
        {
            Timeline value = null;
            timelines.TryGetValue(n, out value);
            return value;
        }

        public virtual bool onTouchDownXY(float tx, float ty)
        {
            bool flag = false;
            foreach (KeyValuePair<int, BaseElement> item in childs.Reverse())
            {
                BaseElement value = item.Value;
                if (value != null && value.touchable && value.onTouchDownXY(tx, ty) && !flag)
                {
                    flag = true;
                    if (!passTouchEventsToAllChilds)
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
            foreach (KeyValuePair<int, BaseElement> item in childs.Reverse())
            {
                BaseElement value = item.Value;
                if (value != null && value.touchable && value.onTouchUpXY(tx, ty) && !flag)
                {
                    flag = true;
                    if (!passTouchEventsToAllChilds)
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
            foreach (KeyValuePair<int, BaseElement> item in childs.Reverse())
            {
                BaseElement value = item.Value;
                if (value != null && value.touchable && value.onTouchMoveXY(tx, ty) && !flag)
                {
                    flag = true;
                    if (!passTouchEventsToAllChilds)
                    {
                        return flag;
                    }
                }
            }
            return flag;
        }

        public void setEnabled(bool e)
        {
            visible = e;
            touchable = e;
            updateable = e;
        }

        public bool isEnabled()
        {
            return visible && touchable && updateable;
        }

        public void setName(string n)
        {
            NSREL(name);
            name = new NSString(n);
        }

        public void setName(NSString n)
        {
            NSREL(name);
            name = n;
        }

        public virtual void show()
        {
            foreach (KeyValuePair<int, BaseElement> child in childs)
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
            foreach (KeyValuePair<int, BaseElement> child in childs)
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
            childs.Clear();
            childs = null;
            timelines.Clear();
            timelines = null;
            NSREL(name);
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
