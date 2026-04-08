using System;
using System.Collections.Generic;
using System.Linq;

using CutTheRope.Desktop;

namespace CutTheRope.Framework.Visual
{
    /// <summary>
    /// Base class for all visual elements in the scene graph.
    /// Provides positioning, anchoring, transforms, child management, timeline playback, and touch dispatch.
    /// </summary>
    internal class BaseElement : FrameworkTypes
    {
        /// <summary>
        /// <see langword="true"/> if this element has a parent in the scene graph.
        /// </summary>
        public bool HasParent => parent != null;

        /// <summary>
        /// Returns <see langword="true"/> if <paramref name="f"/> is set in the element's anchor flags.
        /// </summary>
        /// <param name="f">Anchor flag bitmask to test.</param>
        /// <returns><see langword="true"/> if any of the bits in <paramref name="f"/> are set in <see cref="anchor"/>.</returns>
        public bool AnchorHas(int f)
        {
            return (anchor & f) != 0;
        }

        /// <summary>
        /// Returns <see langword="true"/> if <paramref name="f"/> is set in the parent-anchor flags.
        /// </summary>
        /// <param name="f">Parent-anchor flag bitmask to test.</param>
        /// <returns><see langword="true"/> if any of the bits in <paramref name="f"/> are set in <see cref="parentAnchor"/>.</returns>
        public bool ParentAnchorHas(int f)
        {
            return (parentAnchor & f) != 0;
        }

        /// <summary>
        /// Computes <see cref="drawX"/>/<see cref="drawY"/> for <paramref name="e"/> based on its anchor, parent-anchor, and parent position.
        /// </summary>
        /// <param name="e">Element to compute draw position for.</param>
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

        /// <summary>
        /// Pops the matrix stack if any transforms were applied to <paramref name="t"/>.
        /// </summary>
        /// <param name="t">Element whose transforms to restore.</param>
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

        /// <summary>
        /// Resets the renderer color to solid opaque if <paramref name="t"/> has a custom color.
        /// </summary>
        /// <param name="t">Element whose color to restore.</param>
        protected static void RestoreColor(BaseElement t)
        {
            if (!RGBAColor.RGBAEqual(t.color, RGBAColor.solidOpaqueRGBA))
            {
                Renderer.SetColor(RGBAColor.solidOpaqueRGBAXna);
            }
        }

        /// <summary>
        /// Initializes a new <see cref="BaseElement"/> with default values.
        /// </summary>
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

        /// <summary>
        /// Applies transforms, color, and blending before drawing.
        /// </summary>
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

        /// <summary>
        /// Draws this element by calling <see cref="PreDraw"/> and <see cref="PostDraw"/>.
        /// </summary>
        public virtual void Draw()
        {
            PreDraw();
            PostDraw();
        }

        /// <summary>
        /// Draws visible children and restores transforms/color.
        /// </summary>
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

        /// <summary>
        /// Updates children and advances the current timeline by <paramref name="delta"/> seconds.
        /// </summary>
        /// <param name="delta">Elapsed time in seconds.</param>
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

        /// <summary>
        /// Recursively searches for a child element with the given name.
        /// </summary>
        /// <param name="n">Name to search for.</param>
        /// <returns>The first matching child, or <see langword="null"/> if no match is found.</returns>
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

        /// <summary>
        /// Expands this element's size to encompass all children.
        /// </summary>
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

        /// <summary>
        /// Handles a timeline action. Returns <see langword="true"/> if the action was recognized.
        /// </summary>
        /// <param name="a">Action data to process.</param>
        /// <returns><see langword="true"/> if the action was recognized and handled.</returns>
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

        /// <summary>
        /// Adds a child element and returns its assigned ID.
        /// </summary>
        /// <param name="c">Child element to add.</param>
        /// <returns>The slot ID assigned to the child.</returns>
        public virtual int AddChild(BaseElement c)
        {
            return AddChildwithID(c, -1);
        }

        /// <summary>
        /// Adds a child element at the specified ID slot, disposing any existing child at that slot.
        /// </summary>
        /// <param name="c">Child element to add.</param>
        /// <param name="i">Slot ID, or -1 to auto-assign.</param>
        /// <returns>The slot ID at which the child was inserted.</returns>
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

        /// <summary>
        /// Removes the child at slot <paramref name="i"/> without disposing it.
        /// </summary>
        /// <param name="i">Slot ID of the child to remove.</param>
        public virtual void RemoveChildWithID(int i)
        {
            if (childs.TryGetValue(i, out BaseElement value))
            {
                _ = (value?.parent = null);
                _ = childs.Remove(i);
            }
        }

        /// <summary>
        /// Removes all children without disposing them.
        /// </summary>
        public void RemoveAllChilds()
        {
            childs.Clear();
        }

        /// <summary>
        /// Removes the specified child element by reference.
        /// </summary>
        /// <param name="c">Child element to remove.</param>
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

        /// <summary>
        /// Returns the child at slot <paramref name="i"/>, or <see langword="null"/>.
        /// </summary>
        /// <param name="i">Slot ID to look up.</param>
        /// <returns>The child at the slot, or <see langword="null"/> if no child exists there.</returns>
        public virtual BaseElement GetChild(int i)
        {
            _ = childs.TryGetValue(i, out BaseElement value);
            return value;
        }

        /// <summary>
        /// Returns the slot ID of the specified child, or -1 if not found.
        /// </summary>
        /// <param name="c">Child element to find.</param>
        /// <returns>The slot ID of <paramref name="c"/>, or -1 if it is not a child.</returns>
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

        /// <summary>
        /// Returns the number of children.
        /// </summary>
        /// <returns>The number of children currently attached to this element.</returns>
        public virtual int ChildsCount()
        {
            return childs.Count;
        }

        /// <summary>
        /// Returns the children dictionary.
        /// </summary>
        /// <returns>The dictionary mapping slot IDs to child elements.</returns>
        public virtual Dictionary<int, BaseElement> GetChilds()
        {
            return childs;
        }

        /// <summary>
        /// Adds a timeline and returns its auto-assigned ID.
        /// </summary>
        /// <param name="t">Timeline to add.</param>
        /// <returns>The slot ID assigned to the timeline.</returns>
        public virtual int AddTimeline(Timeline t)
        {
            int count = timelines.Count;
            AddTimelinewithID(t, count);
            return count;
        }

        /// <summary>
        /// Adds a timeline at the specified ID slot.
        /// </summary>
        /// <param name="t">Timeline to add.</param>
        /// <param name="i">Slot ID to assign.</param>
        public virtual void AddTimelinewithID(Timeline t, int i)
        {
            t.element = this;
            timelines[i] = t;
        }

        /// <summary>
        /// Removes the timeline at slot <paramref name="i"/>, stopping it if active.
        /// </summary>
        /// <param name="i">Slot ID of the timeline to remove.</param>
        public virtual void RemoveTimeline(int i)
        {
            if (currentTimelineIndex == i)
            {
                StopCurrentTimeline();
            }
            _ = timelines.Remove(i);
        }

        /// <summary>
        /// Starts playback of the timeline at slot <paramref name="t"/>, stopping any active timeline.
        /// </summary>
        /// <param name="t">Slot ID of the timeline to play.</param>
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

        /// <summary>
        /// Pauses the currently playing timeline.
        /// </summary>
        public virtual void PauseCurrentTimeline()
        {
            currentTimeline.PauseTimeline();
        }

        /// <summary>
        /// Stops the currently playing timeline and clears it.
        /// </summary>
        public virtual void StopCurrentTimeline()
        {
            currentTimeline.StopTimeline();
            currentTimeline = null;
            currentTimelineIndex = -1;
        }

        /// <summary>
        /// Returns the currently active timeline, or <see langword="null"/>.
        /// </summary>
        /// <returns>The currently active timeline, or <see langword="null"/> if none is playing.</returns>
        public virtual Timeline GetCurrentTimeline()
        {
            return currentTimeline;
        }

        /// <summary>
        /// Returns the ID of the currently active timeline, or -1.
        /// </summary>
        /// <returns>The slot ID of the active timeline, or -1 if none is playing.</returns>
        public int GetCurrentTimelineIndex()
        {
            return currentTimelineIndex;
        }

        /// <summary>
        /// Returns the timeline at slot <paramref name="n"/>, or <see langword="null"/>.
        /// </summary>
        /// <param name="n">Slot ID to look up.</param>
        /// <returns>The timeline at the slot, or <see langword="null"/> if no timeline exists there.</returns>
        public virtual Timeline GetTimeline(int n)
        {
            _ = timelines.TryGetValue(n, out Timeline value);
            return value;
        }

        /// <summary>
        /// Dispatches a touch-down event to children. Returns <see langword="true"/> if handled.
        /// </summary>
        /// <param name="tx">Touch X coordinate.</param>
        /// <param name="ty">Touch Y coordinate.</param>
        /// <returns><see langword="true"/> if the event was handled by a child.</returns>
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

        /// <summary>
        /// Dispatches a touch-up event to children. Returns <see langword="true"/> if handled.
        /// </summary>
        /// <param name="tx">Touch X coordinate.</param>
        /// <param name="ty">Touch Y coordinate.</param>
        /// <returns><see langword="true"/> if the event was handled by a child.</returns>
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

        /// <summary>
        /// Dispatches a touch-move event to children. Returns <see langword="true"/> if handled.
        /// </summary>
        /// <param name="tx">Touch X coordinate.</param>
        /// <param name="ty">Touch Y coordinate.</param>
        /// <returns><see langword="true"/> if the event was handled by a child.</returns>
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

        /// <summary>
        /// Sets visible, touchable, and updateable to <paramref name="e"/>.
        /// </summary>
        /// <param name="e">Whether to enable or disable.</param>
        public void SetEnabled(bool e)
        {
            visible = e;
            touchable = e;
            updateable = e;
        }

        /// <summary>
        /// Returns <see langword="true"/> if visible, touchable, and updateable are all <see langword="true"/>.
        /// </summary>
        /// <returns><see langword="true"/> if the element is fully enabled.</returns>
        public bool IsEnabled()
        {
            return visible && touchable && updateable;
        }

        /// <summary>
        /// Sets the element's name used by <see cref="GetChildWithName"/>.
        /// </summary>
        /// <param name="n">Name to assign.</param>
        public void SetName(string n)
        {
            name = n;
        }

        /// <summary>
        /// Recursively shows all visible children.
        /// </summary>
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

        /// <summary>
        /// Recursively hides all visible children.
        /// </summary>
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

        /// <inheritdoc />
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

        /// <summary>
        /// Timeline action name for setting visibility.
        /// </summary>
        public const string ACTION_SET_VISIBLE = "ACTION_SET_VISIBLE";

        /// <summary>
        /// Timeline action name for setting touchability.
        /// </summary>
        public const string ACTION_SET_TOUCHABLE = "ACTION_SET_TOUCHABLE";

        /// <summary>
        /// Timeline action name for setting updateability.
        /// </summary>
        public const string ACTION_SET_UPDATEABLE = "ACTION_SET_UPDATEABLE";

        /// <summary>
        /// Timeline action name for playing a timeline by index.
        /// </summary>
        public const string ACTION_PLAY_TIMELINE = "ACTION_PLAY_TIMELINE";

        /// <summary>
        /// Timeline action name for pausing the current timeline.
        /// </summary>
        public const string ACTION_PAUSE_TIMELINE = "ACTION_PAUSE_TIMELINE";

        /// <summary>
        /// Timeline action name for stopping the current timeline.
        /// </summary>
        public const string ACTION_STOP_TIMELINE = "ACTION_STOP_TIMELINE";

        /// <summary>
        /// Timeline action name for jumping to a specific keyframe.
        /// </summary>
        public const string ACTION_JUMP_TO_TIMELINE_FRAME = "ACTION_JUMP_TO_TIMELINE_FRAME";

        /// <summary>
        /// Timeline action name for setting a custom anchor point.
        /// </summary>
        public const string ACTION_SET_CUSTOM_ANCHOR = "ACTION_SET_CUSTOM_ANCHOR";

        /// <summary>
        /// Timeline action name for setting the rotation center offset.
        /// </summary>
        public const string ACTION_SET_ROTATION_CENTER = "ACTION_SET_ROTATION_CENTER";

        /// <summary>
        /// Whether a matrix push is pending and needs to be popped.
        /// </summary>
        private bool pushM;

        /// <summary>
        /// Whether this element is drawn.
        /// </summary>
        public bool visible;

        /// <summary>
        /// Whether this element receives touch events.
        /// </summary>
        public bool touchable;

        /// <summary>
        /// Whether this element is updated each frame.
        /// </summary>
        public bool updateable;

        /// <summary>
        /// Optional name used for lookup via <see cref="GetChildWithName"/>.
        /// </summary>
        private string name;

        /// <summary>
        /// Local X position relative to the parent.
        /// </summary>
        public float x;

        /// <summary>
        /// Local Y position relative to the parent.
        /// </summary>
        public float y;

        /// <summary>
        /// Computed draw X position in screen coordinates.
        /// </summary>
        public float drawX;

        /// <summary>
        /// Computed draw Y position in screen coordinates.
        /// </summary>
        public float drawY;

        /// <summary>
        /// Width of this element in pixels.
        /// </summary>
        public int width;

        /// <summary>
        /// Height of this element in pixels.
        /// </summary>
        public int height;

        /// <summary>
        /// Rotation angle in degrees.
        /// </summary>
        public float rotation;

        /// <summary>
        /// X offset of the rotation center from the element's center.
        /// </summary>
        public float rotationCenterX;

        /// <summary>
        /// Y offset of the rotation center from the element's center.
        /// </summary>
        public float rotationCenterY;

        /// <summary>
        /// Custom anchor X offset applied when <see cref="useCustomAnchor"/> is <see langword="true"/>.
        /// </summary>
        public float customAnchorX;

        /// <summary>
        /// Custom anchor Y offset applied when <see cref="useCustomAnchor"/> is <see langword="true"/>.
        /// </summary>
        public float customAnchorY;

        /// <summary>
        /// Whether to apply <see cref="customAnchorX"/> and <see cref="customAnchorY"/>.
        /// </summary>
        public bool useCustomAnchor;

        /// <summary>
        /// Horizontal scale factor (1 = no scaling).
        /// </summary>
        public float scaleX;

        /// <summary>
        /// Vertical scale factor (1 = no scaling).
        /// </summary>
        public float scaleY;

        /// <summary>
        /// Horizontal skew factor.
        /// </summary>
        public float skewX;

        /// <summary>
        /// Vertical skew factor.
        /// </summary>
        public float skewY;

        /// <summary>
        /// Tint color applied when drawing.
        /// </summary>
        public RGBAColor color;

        /// <summary>
        /// Horizontal translation offset applied during drawing.
        /// </summary>
        private readonly float translateX;

        /// <summary>
        /// Vertical translation offset applied during drawing.
        /// </summary>
        public float translateY;

        /// <summary>
        /// Bitmask controlling how this element anchors within its own bounds.
        /// </summary>
        public sbyte anchor;

        /// <summary>
        /// Bitmask controlling how this element anchors relative to its parent.
        /// </summary>
        public sbyte parentAnchor;

        /// <summary>
        /// Whether transforms are passed down to children during drawing.
        /// </summary>
        public bool passTransformationsToChilds;

        /// <summary>
        /// Whether color tint is passed down to children during drawing.
        /// </summary>
        public bool passColorToChilds;

        /// <summary>
        /// Whether touch events are dispatched to all children instead of stopping at the first handler.
        /// </summary>
        private readonly bool passTouchEventsToAllChilds;

        /// <summary>
        /// Blending mode index (-1 = default, 0 = alpha, 1 = premultiplied, 2 = additive).
        /// </summary>
        public int blendingMode;

        /// <summary>
        /// Parent element in the scene graph, or <see langword="null"/> if this is a root element.
        /// </summary>
        public BaseElement parent;

        /// <summary>
        /// Child elements keyed by slot ID.
        /// </summary>
        protected Dictionary<int, BaseElement> childs;

        /// <summary>
        /// Timelines keyed by slot ID.
        /// </summary>
        protected Dictionary<int, Timeline> timelines;

        /// <summary>
        /// Index of the currently active timeline, or -1 if none.
        /// </summary>
        private int currentTimelineIndex;

        /// <summary>
        /// The currently active timeline, or <see langword="null"/> if none.
        /// </summary>
        private Timeline currentTimeline;
    }
}
