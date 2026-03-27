using System;
using System.Collections.Generic;
using System.Linq;

using CutTheRope.Desktop;

namespace CutTheRope.Framework.Visual
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
            float parentDrawX = e.HasParent ? e.parent.drawX : 0f;
            float parentDrawY = e.HasParent ? e.parent.drawY : 0f;
            int parentWidth = e.HasParent ? e.parent.width : 0;
            int parentHeight = e.HasParent ? e.parent.height : 0;
            if (e.parentAnchor != -1)
            {
                if ((e.parentAnchor & 1) != 0)
                {
                    e.drawX = parentDrawX + e.x;
                }
                else if ((e.parentAnchor & 2) != 0)
                {
                    e.drawX = parentDrawX + e.x + (parentWidth >> 1);
                }
                else if ((e.parentAnchor & 4) != 0)
                {
                    e.drawX = parentDrawX + e.x + parentWidth;
                }
                if ((e.parentAnchor & 8) != 0)
                {
                    e.drawY = parentDrawY + e.y;
                }
                else if ((e.parentAnchor & 16) != 0)
                {
                    e.drawY = parentDrawY + e.y + (parentHeight >> 1);
                }
                else if ((e.parentAnchor & 32) != 0)
                {
                    e.drawY = parentDrawY + e.y + parentHeight;
                }
            }
            else
            {
                e.drawX = e.x;
                e.drawY = e.y;
            }
            if (e.useCustomAnchor)
            {
                e.drawX -= e.customAnchorX;
                e.drawY -= e.customAnchorY;
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
            if (t.pushM
                || t.rotation != 0
                || t.scaleX != 1
                || t.scaleY != 1
                || t.translateX != 0
                || t.translateY != 0
                || t.skewX != 0
                || t.skewY != 0)
            {
                Renderer.PopMatrix();
                t.pushM = false;
            }
        }

        protected static void RestoreColor(BaseElement t)
        {
            if (!RGBAColor.RGBAEqual(t.color, RGBAColor.solidOpaqueRGBA))
            {
                Renderer.SetColor(RGBAColor.solidOpaqueRGBAXna);
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
            customAnchorX = 0f;
            customAnchorY = 0f;
            useCustomAnchor = false;
            scaleX = 1f;
            scaleY = 1f;
            skewX = 0f;
            skewY = 0f;
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
            bool changeScale = scaleX != 1 || scaleY != 1;
            bool changeRotation = rotation != 0;
            bool changeSkew = skewX != 0 || skewY != 0;
            bool changeTranslate = translateX != 0 || translateY != 0;
            if (changeScale || changeRotation || changeTranslate || changeSkew)
            {
                Renderer.PushMatrix();
                pushM = true;
                if (changeScale || changeRotation || changeSkew)
                {
                    float rotationOffsetX = drawX + (width >> 1) + rotationCenterX;
                    float rotationOffsetY = drawY + (height >> 1) + rotationCenterY;
                    Renderer.Translate(rotationOffsetX, rotationOffsetY, 0f);
                    if (changeRotation)
                    {
                        Renderer.Rotate(rotation, 0f, 0f, 1f);
                    }
                    if (changeSkew)
                    {
                        Renderer.Skew(skewX, skewY);
                    }
                    if (changeScale)
                    {
                        Renderer.Scale(scaleX, scaleY, 1f);
                    }
                    Renderer.Translate(0f - rotationOffsetX, 0f - rotationOffsetY, 0f);
                }
                if (changeTranslate)
                {
                    Renderer.Translate(translateX, translateY, 0f);
                }
            }
            if (!RGBAColor.RGBAEqual(color, RGBAColor.solidOpaqueRGBA))
            {
                Renderer.SetColor(color.ToWhiteAlphaXNA());
            }
            if (blendingMode != -1)
            {
                switch (blendingMode)
                {
                    case 0:
                        Renderer.SetBlendFunc(BlendingFactor.GLSRCALPHA, BlendingFactor.GLONEMINUSSRCALPHA);
                        return;
                    case 1:
                        Renderer.SetBlendFunc(BlendingFactor.GLONE, BlendingFactor.GLONEMINUSSRCALPHA);
                        return;
                    case 2:
                        Renderer.SetBlendFunc(BlendingFactor.GLSRCALPHA, BlendingFactor.GLONE);
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
            int processedChildren = 0;
            int childId = 0;
            while (processedChildren < childs.Count)
            {
                if (childs.TryGetValue(childId, out BaseElement value))
                {
                    if (value != null && value.visible)
                    {
                        value.Draw();
                    }
                    processedChildren++;
                }
                childId++;
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
            int processedChildren = 0;
            int childId = 0;
            while (processedChildren < childs.Count)
            {
                if (childs.TryGetValue(childId, out BaseElement value))
                {
                    if (value != null && value.updateable)
                    {
                        value.Update(delta);
                    }
                    processedChildren++;
                }
                childId++;
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
                    if (value.name != null && value.name == n)
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
            float minX = drawX;
            float minY = drawY;
            float maxX = drawX + width;
            float maxY = drawY + height;
            foreach (KeyValuePair<int, BaseElement> child in childs)
            {
                BaseElement value = child.Value;
                if (value != null)
                {
                    CalculateTopLeft(value);
                    if (value.drawX < minX)
                    {
                        minX = value.drawX;
                    }
                    if (value.drawY < minY)
                    {
                        minY = value.drawY;
                    }
                    if (value.drawX + value.width > maxX)
                    {
                        maxX = value.drawX + value.width;
                    }
                    if (value.drawX + value.height > maxY)
                    {
                        maxY = value.drawY + value.height;
                    }
                }
            }
            width = (int)(maxX - minX);
            height = (int)(maxY - minY);
        }

        public virtual bool HandleAction(ActionData a)
        {
            if (a.actionName == ACTION_SET_VISIBLE)
            {
                visible = a.actionSubParam != 0;
            }
            else if (a.actionName == ACTION_SET_UPDATEABLE)
            {
                updateable = a.actionSubParam != 0;
            }
            else if (a.actionName == ACTION_SET_TOUCHABLE)
            {
                touchable = a.actionSubParam != 0;
            }
            else if (a.actionName == ACTION_PLAY_TIMELINE)
            {
                PlayTimeline(a.actionSubParam);
            }
            else if (a.actionName == ACTION_PAUSE_TIMELINE)
            {
                PauseCurrentTimeline();
            }
            else if (a.actionName == ACTION_STOP_TIMELINE)
            {
                StopCurrentTimeline();
            }
            else if (a.actionName == ACTION_SET_CUSTOM_ANCHOR)
            {
                customAnchorX = a.actionParamFloat;
                customAnchorY = a.actionSubParamFloat;
                useCustomAnchor = true;
            }
            else if (a.actionName == ACTION_SET_ROTATION_CENTER)
            {
                rotationCenterX = a.actionParamFloat;
                rotationCenterY = a.actionSubParamFloat;
            }
            else
            {
                if (a.actionName != ACTION_JUMP_TO_TIMELINE_FRAME)
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
                _ = (value?.parent = null);
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
            bool handled = false;
            foreach (KeyValuePair<int, BaseElement> item in childs.Reverse())
            {
                BaseElement value = item.Value;
                if (value != null && value.touchable && value.OnTouchDownXY(tx, ty) && !handled)
                {
                    handled = true;
                    if (!passTouchEventsToAllChilds)
                    {
                        return handled;
                    }
                }
            }
            return handled;
        }

        public virtual bool OnTouchUpXY(float tx, float ty)
        {
            bool handled = false;
            foreach (KeyValuePair<int, BaseElement> item in childs.Reverse())
            {
                BaseElement value = item.Value;
                if (value != null && value.touchable && value.OnTouchUpXY(tx, ty) && !handled)
                {
                    handled = true;
                    if (!passTouchEventsToAllChilds)
                    {
                        return handled;
                    }
                }
            }
            return handled;
        }

        public virtual bool OnTouchMoveXY(float tx, float ty)
        {
            bool handled = false;
            foreach (KeyValuePair<int, BaseElement> item in childs.Reverse())
            {
                BaseElement value = item.Value;
                if (value != null && value.touchable && value.OnTouchMoveXY(tx, ty) && !handled)
                {
                    handled = true;
                    if (!passTouchEventsToAllChilds)
                    {
                        return handled;
                    }
                }
            }
            return handled;
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

        public const string ACTION_SET_CUSTOM_ANCHOR = "ACTION_SET_CUSTOM_ANCHOR";

        public const string ACTION_SET_ROTATION_CENTER = "ACTION_SET_ROTATION_CENTER";

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

        public float customAnchorX;

        public float customAnchorY;

        public bool useCustomAnchor;

        public float scaleX;

        public float scaleY;

        public float skewX;

        public float skewY;

        public RGBAColor color;

        private readonly float translateX;

        public float translateY;

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
