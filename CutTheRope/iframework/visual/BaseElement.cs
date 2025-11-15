using System;
using System.Collections.Generic;
using System.Linq;

using CutTheRope.desktop;
using CutTheRope.Helpers;

namespace CutTheRope.iframework.visual
{
    internal class BaseElement : FrameworkTypes
    {
        // (get) Token: 0x0600015F RID: 351 RVA: 0x0000733B File Offset: 0x0000553B
        public bool HasParent => parent != null;

        public bool AnchorHas(int f)
        {
            return (anchor & f) != 0;
        }

        public bool ParentAnchorHas(int f)
        {
            return (parentAnchor & f) != 0;
        }

        public static void CalculateTopLeft(BaseElement e)
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
                    e.drawX = num + e.x + (num3 >> 1);
                }
                else if ((e.parentAnchor & 4) != 0)
                {
                    e.drawX = num + e.x + num3;
                }
                if ((e.parentAnchor & 8) != 0)
                {
                    e.drawY = num2 + e.y;
                }
                else if ((e.parentAnchor & 16) != 0)
                {
                    e.drawY = num2 + e.y + (num4 >> 1);
                }
                else if ((e.parentAnchor & 32) != 0)
                {
                    e.drawY = num2 + e.y + num4;
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
                    e.drawY -= e.height >> 1;
                }
                else if ((e.anchor & 32) != 0)
                {
                    e.drawY -= e.height;
                }
            }
            if ((e.anchor & 1) == 0)
            {
                if ((e.anchor & 2) != 0)
                {
                    e.drawX -= e.width >> 1;
                    return;
                }
                if ((e.anchor & 4) != 0)
                {
                    e.drawX -= e.width;
                }
            }
        }

        protected static void RestoreTransformations(BaseElement t)
        {
            if (t.pushM || t.rotation != 0.0 || t.scaleX != 1.0 || t.scaleY != 1.0 || t.translateX != 0.0 || t.translateY != 0.0)
            {
                OpenGL.GlPopMatrix();
                t.pushM = false;
            }
        }

        private static void RestoreColor(BaseElement t)
        {
            if (!RGBAColor.RGBAEqual(t.color, RGBAColor.solidOpaqueRGBA))
            {
                OpenGL.GlColor4f(RGBAColor.solidOpaqueRGBAXna);
            }
        }

        public BaseElement()
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
            childs = [];
            timelines = [];
            currentTimeline = null;
            currentTimelineIndex = -1;
            passTransformationsToChilds = true;
            passColorToChilds = true;
            passTouchEventsToAllChilds = false;
            blendingMode = -1;
        }

        public virtual void PreDraw()
        {
            CalculateTopLeft(this);
            bool flag = scaleX != 1.0 || scaleY != 1.0;
            bool flag2 = rotation != 0.0;
            bool flag3 = translateX != 0.0 || translateY != 0.0;
            if (flag || flag2 || flag3)
            {
                OpenGL.GlPushMatrix();
                pushM = true;
                if (flag || flag2)
                {
                    float num = drawX + (width >> 1) + rotationCenterX;
                    float num2 = drawY + (height >> 1) + rotationCenterY;
                    OpenGL.GlTranslatef(num, num2, 0f);
                    if (flag2)
                    {
                        OpenGL.GlRotatef(rotation, 0f, 0f, 1f);
                    }
                    if (flag)
                    {
                        OpenGL.GlScalef(scaleX, scaleY, 1f);
                    }
                    OpenGL.GlTranslatef(0f - num, 0f - num2, 0f);
                }
                if (flag3)
                {
                    OpenGL.GlTranslatef(translateX, translateY, 0f);
                }
            }
            if (!RGBAColor.RGBAEqual(color, RGBAColor.solidOpaqueRGBA))
            {
                OpenGL.GlColor4f(color.ToWhiteAlphaXNA());
            }
            if (blendingMode != -1)
            {
                switch (blendingMode)
                {
                    case 0:
                        OpenGL.GlBlendFunc(BlendingFactor.GLSRCALPHA, BlendingFactor.GLONEMINUSSRCALPHA);
                        return;
                    case 1:
                        OpenGL.GlBlendFunc(BlendingFactor.GLONE, BlendingFactor.GLONEMINUSSRCALPHA);
                        return;
                    case 2:
                        OpenGL.GlBlendFunc(BlendingFactor.GLSRCALPHA, BlendingFactor.GLONE);
                        break;
                    default:
                        return;
                }
            }
        }

        public virtual void Draw()
        {
            PreDraw();
            PostDraw();
        }

        public virtual void PostDraw()
        {
            if (!passTransformationsToChilds)
            {
                RestoreTransformations(this);
            }
            if (!passColorToChilds)
            {
                RestoreColor(this);
            }
            int num = 0;
            int num2 = 0;
            while (num < childs.Count)
            {
                if (childs.TryGetValue(num2, out BaseElement value))
                {
                    if (value != null && value.visible)
                    {
                        value.Draw();
                    }
                    num++;
                }
                num2++;
            }
            if (passTransformationsToChilds)
            {
                RestoreTransformations(this);
            }
            if (passColorToChilds)
            {
                RestoreColor(this);
            }
        }

        public virtual void Update(float delta)
        {
            int num = 0;
            int num2 = 0;
            while (num < childs.Count)
            {
                if (childs.TryGetValue(num2, out BaseElement value))
                {
                    if (value != null && value.updateable)
                    {
                        value.Update(delta);
                    }
                    num++;
                }
                num2++;
            }
            if (currentTimeline != null)
            {
                Timeline.UpdateTimeline(currentTimeline, delta);
            }
        }

        public BaseElement GetChildWithName(string n)
        {
            foreach (KeyValuePair<int, BaseElement> child in childs)
            {
                BaseElement value = child.Value;
                if (value != null)
                {
                    if (value.name != null && value.name.IsEqualToString(n))
                    {
                        return value;
                    }
                    BaseElement childWithName = value.GetChildWithName(n);
                    if (childWithName != null)
                    {
                        return childWithName;
                    }
                }
            }
            return null;
        }

        public void SetSizeToChildsBounds()
        {
            CalculateTopLeft(this);
            float num = drawX;
            float num2 = drawY;
            float num3 = drawX + width;
            float num4 = drawY + height;
            foreach (KeyValuePair<int, BaseElement> child in childs)
            {
                BaseElement value = child.Value;
                if (value != null)
                {
                    CalculateTopLeft(value);
                    if (value.drawX < num)
                    {
                        num = value.drawX;
                    }
                    if (value.drawY < num2)
                    {
                        num2 = value.drawY;
                    }
                    if (value.drawX + value.width > num3)
                    {
                        num3 = value.drawX + value.width;
                    }
                    if (value.drawX + value.height > num4)
                    {
                        num4 = value.drawY + value.height;
                    }
                }
            }
            width = (int)(num3 - num);
            height = (int)(num4 - num2);
        }

        public virtual bool HandleAction(ActionData a)
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
                PlayTimeline(a.actionSubParam);
            }
            else if (a.actionName == "ACTION_PAUSE_TIMELINE")
            {
                PauseCurrentTimeline();
            }
            else if (a.actionName == "ACTION_STOP_TIMELINE")
            {
                StopCurrentTimeline();
            }
            else
            {
                if (!(a.actionName == "ACTION_JUMP_TO_TIMELINE_FRAME"))
                {
                    return false;
                }
                GetCurrentTimeline().JumpToTrackKeyFrame(a.actionParam, a.actionSubParam);
            }
            return true;
        }

        public virtual int AddChild(BaseElement c)
        {
            return AddChildwithID(c, -1);
        }

        public virtual int AddChildwithID(BaseElement c, int i)
        {
            c.parent = this;
            if (i == -1)
            {
                i = 0;
                while (childs.TryGetValue(i, out BaseElement value))
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
            else if (childs.TryGetValue(i, out BaseElement value2))
            {
                if (value2 != c)
                {
                    value2?.Dispose();
                }
                childs[i] = c;
            }
            else
            {
                childs.Add(i, c);
            }
            return i;
        }

        public virtual void RemoveChildWithID(int i)
        {
            if (childs.TryGetValue(i, out BaseElement value))
            {
                if (value != null)
                {
                    value.parent = null;
                }
                _ = childs.Remove(i);
            }
        }

        public void RemoveAllChilds()
        {
            childs.Clear();
        }

        public virtual void RemoveChild(BaseElement c)
        {
            foreach (KeyValuePair<int, BaseElement> child in childs)
            {
                if (c.Equals(child.Value))
                {
                    _ = childs.Remove(child.Key);
                    break;
                }
            }
        }

        public virtual BaseElement GetChild(int i)
        {
            _ = childs.TryGetValue(i, out BaseElement value);
            return value;
        }

        public virtual int GetChildId(BaseElement c)
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

        public virtual int ChildsCount()
        {
            return childs.Count;
        }

        public virtual Dictionary<int, BaseElement> GetChilds()
        {
            return childs;
        }

        public virtual int AddTimeline(Timeline t)
        {
            int count = timelines.Count;
            AddTimelinewithID(t, count);
            return count;
        }

        public virtual void AddTimelinewithID(Timeline t, int i)
        {
            t.element = this;
            timelines[i] = t;
        }

        public virtual void RemoveTimeline(int i)
        {
            if (currentTimelineIndex == i)
            {
                StopCurrentTimeline();
            }
            _ = timelines.Remove(i);
        }

        public virtual void PlayTimeline(int t)
        {
            _ = timelines.TryGetValue(t, out Timeline value);
            if (value != null)
            {
                if (currentTimeline != null && currentTimeline.state != Timeline.TimelineState.TIMELINE_STOPPED)
                {
                    currentTimeline.StopTimeline();
                }
                currentTimelineIndex = t;
                currentTimeline = value;
                currentTimeline.PlayTimeline();
            }
        }

        public virtual void PauseCurrentTimeline()
        {
            currentTimeline.PauseTimeline();
        }

        public virtual void StopCurrentTimeline()
        {
            currentTimeline.StopTimeline();
            currentTimeline = null;
            currentTimelineIndex = -1;
        }

        public virtual Timeline GetCurrentTimeline()
        {
            return currentTimeline;
        }

        public int GetCurrentTimelineIndex()
        {
            return currentTimelineIndex;
        }

        public virtual Timeline GetTimeline(int n)
        {
            _ = timelines.TryGetValue(n, out Timeline value);
            return value;
        }

        public virtual bool OnTouchDownXY(float tx, float ty)
        {
            bool flag = false;
            foreach (KeyValuePair<int, BaseElement> item in childs.Reverse())
            {
                BaseElement value = item.Value;
                if (value != null && value.touchable && value.OnTouchDownXY(tx, ty) && !flag)
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

        public virtual bool OnTouchUpXY(float tx, float ty)
        {
            bool flag = false;
            foreach (KeyValuePair<int, BaseElement> item in childs.Reverse())
            {
                BaseElement value = item.Value;
                if (value != null && value.touchable && value.OnTouchUpXY(tx, ty) && !flag)
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

        public virtual bool OnTouchMoveXY(float tx, float ty)
        {
            bool flag = false;
            foreach (KeyValuePair<int, BaseElement> item in childs.Reverse())
            {
                BaseElement value = item.Value;
                if (value != null && value.touchable && value.OnTouchMoveXY(tx, ty) && !flag)
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

        public void SetEnabled(bool e)
        {
            visible = e;
            touchable = e;
            updateable = e;
        }

        public bool IsEnabled()
        {
            return visible && touchable && updateable;
        }

        public void SetName(string n)
        {
            name = n;
        }

        public virtual void Show()
        {
            foreach (KeyValuePair<int, BaseElement> child in childs)
            {
                BaseElement value = child.Value;
                if (value != null && value.visible)
                {
                    value.Show();
                }
            }
        }

        public virtual void Hide()
        {
            foreach (KeyValuePair<int, BaseElement> child in childs)
            {
                BaseElement value = child.Value;
                if (value != null && value.visible)
                {
                    value.Hide();
                }
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                childs?.Clear();
                childs = null;
                timelines?.Clear();
                timelines = null;
            }
            base.Dispose(disposing);
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

        private string name;

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

        private readonly float translateX;

        private readonly float translateY;

        public sbyte anchor;

        public sbyte parentAnchor;

        public bool passTransformationsToChilds;

        public bool passColorToChilds;

        private readonly bool passTouchEventsToAllChilds;

        public int blendingMode;

        public BaseElement parent;

        protected Dictionary<int, BaseElement> childs;

        protected Dictionary<int, Timeline> timelines;

        private int currentTimelineIndex;

        private Timeline currentTimeline;
    }
}
