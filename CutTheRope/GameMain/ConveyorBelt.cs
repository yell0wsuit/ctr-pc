using System;
using System.Collections.Generic;

using CutTheRope.Framework;
using CutTheRope.Framework.Core;
using CutTheRope.Framework.Helpers;
using CutTheRope.Framework.Visual;

namespace CutTheRope.GameMain
{
    /// <summary>
    /// Represents a conveyor belt game element that transports items along a linear path.
    /// Items placed on the belt are automatically moved in the belt's direction, with support
    /// for both automatic (constant velocity) and manual (user-draggable) operation modes.
    /// </summary>
    internal sealed class ConveyorBelt : BaseElement
    {
        private const int ImgObjConveyorEnd = 0;
        private const int ImgObjConveyorEndSide = 1;
        private const int ImgObjConveyorMiddle = 2;
        private const int ImgObjConveyorMiddleSide = 3;
        private const int ImgObjConveyorPlate = 4;
        private const int ImgObjConveyorPlateArrow = 5;
        private const int ImgObjConveyorHighlight = 6;

        /// <summary>
        /// Tracks the state of an item riding on the conveyor belt.
        /// </summary>
        /// <param name="initialOffset">The starting offset position along the belt.</param>
        private sealed class ConveyorItemState(float initialOffset)
        {
            private static int nextIndex;
            /// <summary>Whether the item should be removed from the belt.</summary>
            public bool markedForRemoval;
            /// <summary>Whether the item is still sliding perpendicular to settle onto the belt center.</summary>
            public bool isSettling = true;
            /// <summary>The target offset for the next frame.</summary>
            public float nextOffset = initialOffset;
            /// <summary>The current offset position along the belt length.</summary>
            public float offset = initialOffset;
            /// <summary>Unique index for ordering items.</summary>
            public int index = nextIndex++;
        }

        /// <summary>
        /// Handles the visual rendering of the conveyor belt's moving surface using tiled plate segments.
        /// </summary>
        private sealed class ConveyorBeltVisual : BaseElement
        {
            private readonly Image plateSection;
            private readonly Image plateArrow;
            private readonly float tileHeight;

            /// <summary>The current visual offset for the scrolling belt texture.</summary>
            public float offset;

            /// <summary>
            /// Creates a new conveyor belt visual surface.
            /// </summary>
            /// <param name="width">The width of the belt surface.</param>
            /// <param name="height">The height of the belt surface.</param>
            /// <param name="direction">The movement direction indicator: negative for left arrow, positive for right arrow, zero for no arrow.</param>
            public ConveyorBeltVisual(float width, float height, int direction)
            {
                this.width = (int)MathF.Ceiling(width);
                this.height = (int)MathF.Ceiling(height);
                anchor = 9;
                parentAnchor = 9;

                plateSection = Image.Image_createWithResIDQuad(Resources.Img.ObjConveyor, ImgObjConveyorPlate);
                plateSection.parent = this;
                plateSection.anchor = 10;
                plateSection.parentAnchor = 10;
                plateSection.scaleX = plateSection.width > 0 ? width / plateSection.width : 1f;
                tileHeight = plateSection.height;

                if (direction != 0)
                {
                    plateArrow = Image.Image_createWithResIDQuad(Resources.Img.ObjConveyor, ImgObjConveyorPlateArrow);
                    plateArrow.anchor = 18;
                    plateArrow.parentAnchor = 18;
                    if (direction < 0)
                    {
                        plateArrow.rotation = 180f;
                    }

                    _ = plateSection.AddChild(plateArrow);
                }
                else
                {
                    plateArrow = null;
                }
            }

            /// <summary>
            /// Moves the belt visual by the specified delta, wrapping around at the edges.
            /// </summary>
            /// <param name="delta">The distance to move the belt texture.</param>
            public void Move(float delta)
            {
                if (tileHeight <= 0f)
                {
                    return;
                }

                offset += delta;
                if (offset > height)
                {
                    do
                    {
                        offset -= tileHeight;
                    }
                    while (offset > height);
                }

                while (offset < 0f)
                {
                    offset += tileHeight;
                }
            }

            /// <summary>
            /// Draws the conveyor plate using the same slice/repeat/slice pattern as the iOS TransporterPlate.
            /// </summary>
            public override void Draw()
            {
                PreDraw();

                if (tileHeight <= 0f || height <= 0)
                {
                    PostDraw();
                    return;
                }

                float wrappedOffset = offset;
                while (wrappedOffset >= tileHeight)
                {
                    wrappedOffset -= tileHeight;
                }
                while (wrappedOffset < 0f)
                {
                    wrappedOffset += tileHeight;
                }

                float drawnHeight = 0f;
                if (wrappedOffset > 0f)
                {
                    DrawSegment(-0.5f * (tileHeight - wrappedOffset), wrappedOffset);
                    drawnHeight = wrappedOffset;
                }

                while (drawnHeight + tileHeight <= height)
                {
                    DrawSegment(drawnHeight, tileHeight);
                    drawnHeight += tileHeight;
                }

                float remainingHeight = height - drawnHeight;
                if (remainingHeight > 0f)
                {
                    float finalY = height - remainingHeight - (0.5f * (tileHeight - remainingHeight));
                    DrawSegment(finalY, remainingHeight);
                }

                PostDraw();
            }

            private void DrawSegment(float y, float visibleHeight)
            {
                plateSection.y = y;
                plateSection.scaleY = visibleHeight / tileHeight;
                plateSection.Draw();
                plateSection.scaleY = 1f;
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ConveyorBelt"/> class with default anchor settings.
        /// Use <see cref="Create"/> or <see cref="InitializeBelt"/> to fully configure the belt.
        /// </summary>
        public ConveyorBelt()
        {
            anchor = 17;
            parentAnchor = -1;
        }

        /// <summary>
        /// Creates and initializes a new conveyor belt instance.
        /// </summary>
        /// <param name="id">Unique identifier for this belt.</param>
        /// <param name="x">The x-coordinate of the belt's left edge origin.</param>
        /// <param name="y">The y-coordinate of the belt's left edge origin.</param>
        /// <param name="length">The length of the belt along its direction.</param>
        /// <param name="height">The height (thickness) of the belt.</param>
        /// <param name="rotation">The rotation angle in degrees.</param>
        /// <param name="isManual">If true, the belt is controlled by user drag; otherwise it moves automatically.</param>
        /// <param name="velocity">The automatic movement speed (used only when not manual).</param>
        /// <returns>A fully initialized conveyor belt.</returns>
        public static ConveyorBelt Create(
            int id,
            float x,
            float y,
            float length,
            float height,
            float rotation,
            bool isManual,
            float velocity)
        {
            ConveyorBelt belt = new();
            belt.InitializeBelt(id, x, y, length, height, rotation, isManual, velocity);
            return belt;
        }

        /// <summary>
        /// Configures the belt with the specified parameters and rebuilds its visuals.
        /// </summary>
        /// <param name="id">Unique identifier for this belt.</param>
        /// <param name="x">The x-coordinate of the belt's left edge origin.</param>
        /// <param name="y">The y-coordinate of the belt's left edge origin.</param>
        /// <param name="length">The length of the belt along its direction.</param>
        /// <param name="height">The height (thickness) of the belt.</param>
        /// <param name="rotation">The rotation angle in degrees.</param>
        /// <param name="isManual">If true, the belt is controlled by user drag; otherwise it moves automatically.</param>
        /// <param name="velocity">The automatic movement speed (used only when not manual).</param>
        public void InitializeBelt(
            int id,
            float x,
            float y,
            float length,
            float height,
            float rotation,
            bool isManual,
            float velocity)
        {
            activePointerId = -1;
            this.id = id;
            this.x = x;
            this.y = y;
            beltWidth = length;
            beltHeight = height;
            width = (int)MathF.Ceiling(length);
            this.height = (int)MathF.Ceiling(height);

            this.rotation = rotation;
            IsManual = isManual;
            rotationRad = DEGREES_TO_RADIANS(rotation);
            direction = Vect(Cosf(rotationRad), Sinf(rotationRad));
            this.velocity = velocity;
            rotationCenterX = -length / 2f;
            rotationCenterY = 0f;

            RemoveAllChilds();
            BuildVisuals();
        }

        /// <summary>
        /// Updates the belt and all items on it each frame. Handles movement, collision avoidance,
        /// wrapping, and settling of items onto the belt surface.
        /// </summary>
        /// <param name="deltaTime">The time elapsed since the last frame in seconds.</param>
        public override void Update(float deltaTime)
        {
            base.Update(deltaTime);

            if (!IsManual)
            {
                offsetDelta = deltaTime * velocity;
                offset += offsetDelta;
                offset = WrapOffset(offset, beltWidth);
            }

            active = MathF.Abs(offsetDelta) > 0.5f;

            if (IsManual && active)
            {
                manualTravelDistance += MathF.Abs(offsetDelta);
                if (manualTravelDistance >= 100f)
                {
                    PlayManualMoveSound();
                    manualTravelDistance = 0f;
                }
            }

            CleanupMarkedItems();

            BaseElement firstItem = null;
            BaseElement lastItem = null;

            foreach (KeyValuePair<BaseElement, ConveyorItemState> kvp in itemStates)
            {
                BaseElement item = kvp.Key;
                ConveyorItemState itemState = kvp.Value;
                if (itemState.markedForRemoval)
                {
                    continue;
                }

                float targetOffset = itemState.offset + offsetDelta;
                bool wrappedAround = true;

                if (targetOffset >= beltWidth)
                {
                    targetOffset -= beltWidth;
                }
                else if (targetOffset <= 0f)
                {
                    targetOffset += beltWidth;
                }
                else
                {
                    wrappedAround = false;
                }

                Vector size = GetItemSize(item);
                Vector position = GetItemPosition(item);
                Vector projectedSize = Vect(size.X * direction.X, size.Y * direction.Y);
                float halfLength = VectLength(projectedSize) / 2f;

                float scale = 1f;
                float projectedOffset = targetOffset;

                if (targetOffset < halfLength)
                {
                    scale = 0.5f + (0.5f * targetOffset / halfLength);
                    firstItem = item;
                    projectedOffset = halfLength * scale;
                }
                else if (beltWidth - targetOffset < halfLength)
                {
                    scale = 0.5f + (0.5f * (beltWidth - targetOffset) / halfLength);
                    lastItem = item;
                    projectedOffset = beltWidth - (halfLength * scale);
                }

                foreach (KeyValuePair<BaseElement, ConveyorItemState> neighborPair in itemStates)
                {
                    BaseElement neighbor = neighborPair.Key;
                    ConveyorItemState neighborState = neighborPair.Value;
                    if (neighbor == item || neighborState.markedForRemoval || scale != 1f)
                    {
                        continue;
                    }

                    float separation = neighborState.offset - itemState.offset;
                    Vector neighborSize = GetItemSize(neighbor);
                    Vector combined = Vect(size.X + neighborSize.X, size.Y + neighborSize.Y);
                    float combinedSq = (combined.X * combined.X) + (combined.Y * combined.Y);
                    if (0.25f * combinedSq > separation * separation)
                    {
                        if (MathF.Abs(separation) < 0.001f)
                        {
                            int deltaIndex = items.IndexOf(neighbor) - items.IndexOf(item);
                            separation = 600f * (deltaIndex > 0 ? 1f : deltaIndex < 0 ? -1f : 0f);
                        }
                        else if (MathF.Abs(separation) < 600f)
                        {
                            separation = MathF.Sign(separation) * 600f;
                        }
                        targetOffset -= separation * deltaTime;
                    }
                }

                ApplyItemScale(item, scale);

                Vector offsetVector = Vect(
                    x + (direction.X * projectedOffset) - position.X,
                    y + (direction.Y * projectedOffset) - position.Y);

                if (itemState.isSettling)
                {
                    Vector perpendicular = Vect(direction.Y, -direction.X);
                    float slideDistance = ((offsetVector.X * perpendicular.X) + (offsetVector.Y * perpendicular.Y)) / VectLength(direction);
                    Vector projectedSlide = Vect(perpendicular.X * slideDistance, perpendicular.Y * slideDistance);

                    float maxSlide = 800f * deltaTime;
                    float slideLengthSq = (projectedSlide.X * projectedSlide.X) + (projectedSlide.Y * projectedSlide.Y);
                    if (slideLengthSq >= maxSlide * maxSlide)
                    {
                        float slideLength = MathF.Sqrt(slideLengthSq);
                        float factor = (slideLength - maxSlide) / slideLength;
                        projectedSlide = Vect(projectedSlide.X * factor, projectedSlide.Y * factor);
                    }
                    else
                    {
                        itemState.isSettling = false;
                    }

                    offsetVector = VectSub(offsetVector, projectedSlide);
                    SetItemPosition(item, VectAdd(position, offsetVector));
                }
                else
                {
                    SetItemPosition(item, VectAdd(Vect(x, y), VectMult(direction, projectedOffset)));
                }

                itemState.nextOffset = targetOffset;
                if (wrappedAround)
                {
                    (item as IConveyorDropHandler)?.OnConveyorDrop();
                    CTRSoundMgr.PlaySound(Resources.Snd.TransporterDrop);
                }
            }

            foreach (ConveyorItemState state in itemStates.Values)
            {
                state.offset = WrapOffset(state.nextOffset, beltWidth);
            }

            beltVisual?.Move(offsetDelta);

            if (IsManual)
            {
                offsetDelta = 0f;
            }

            if (activePointerId == -1)
            {
                if (firstItem != null && lastItem != null)
                {
                    foreach (KeyValuePair<BaseElement, ConveyorItemState> kvp in itemStates)
                    {
                        if (kvp.Value.markedForRemoval)
                        {
                            continue;
                        }

                        if (kvp.Key == firstItem)
                        {
                            kvp.Value.offset += 1500f * deltaTime;
                        }

                        if (kvp.Key == lastItem)
                        {
                            kvp.Value.offset -= 1500f * deltaTime;
                        }
                    }
                }
                else if (firstItem != null)
                {
                    offsetDelta = 1500f * deltaTime;
                }
                else if (lastItem != null)
                {
                    offsetDelta = -1500f * deltaTime;
                }
            }
        }

        /// <summary>
        /// Handles pointer down events for manual belt dragging.
        /// </summary>
        /// <param name="pointerX">The x-coordinate of the pointer in world space.</param>
        /// <param name="pointerY">The y-coordinate of the pointer in world space.</param>
        /// <param name="pointerId">The unique identifier of the pointer.</param>
        /// <returns>True if the belt captured the pointer; false otherwise.</returns>
        public bool OnPointerDown(float pointerX, float pointerY, int pointerId)
        {
            if (!IsManual)
            {
                return false;
            }

            Vector local = ToLocalSpace(Vect(pointerX, pointerY));
            bool insideBounds =
                local.X >= 0f &&
                local.X <= beltWidth &&
                local.Y >= -0.5f * beltHeight &&
                local.Y <= 0.5f * beltHeight;

            if (insideBounds)
            {
                activePointerId = pointerId;
                lastDragPosition = local;
                return true;
            }

            return false;
        }

        /// <summary>
        /// Handles pointer up events to release manual belt dragging.
        /// </summary>
        /// <param name="pointerX">The x-coordinate of the pointer in world space.</param>
        /// <param name="pointerY">The y-coordinate of the pointer in world space.</param>
        /// <param name="pointerId">The unique identifier of the pointer.</param>
        /// <returns>True if the belt released its captured pointer; false otherwise.</returns>
        public bool OnPointerUp(float pointerX, float pointerY, int pointerId)
        {
            _ = pointerX;
            _ = pointerY;

            if (!IsManual)
            {
                return false;
            }

            if (activePointerId == pointerId)
            {
                activePointerId = -1;
                offsetDelta = 0f;

                foreach (KeyValuePair<BaseElement, ConveyorItemState> kvp in itemStates)
                {
                    if (kvp.Value.markedForRemoval)
                    {
                        Remove(kvp.Key);
                    }
                }

                return true;
            }

            return false;
        }

        /// <summary>
        /// Handles pointer move events to drag the manual belt.
        /// </summary>
        /// <param name="pointerX">The x-coordinate of the pointer in world space.</param>
        /// <param name="pointerY">The y-coordinate of the pointer in world space.</param>
        /// <param name="pointerId">The unique identifier of the pointer.</param>
        /// <returns>True if the belt handled the movement; false otherwise.</returns>
        public bool OnPointerMove(float pointerX, float pointerY, int pointerId)
        {
            if (!IsManual)
            {
                return false;
            }

            if (activePointerId == pointerId)
            {
                Vector local = ToLocalSpace(Vect(pointerX, pointerY));
                offsetDelta = local.X - lastDragPosition.X;
                offset += offsetDelta;
                offset = WrapOffset(offset, beltWidth);
                lastDragPosition = local;
                return true;
            }

            return false;
        }

        /// <summary>
        /// Determines whether a world-space point is within the belt's bounds.
        /// </summary>
        /// <param name="worldPoint">The point to test in world coordinates.</param>
        /// <returns>True if the point is inside the belt area; false otherwise.</returns>
        public bool Contains(Vector worldPoint)
        {
            Vector local = ToLocalSpace(worldPoint);
            return local.X >= 0f && local.X <= beltWidth && local.Y >= -0.5f * beltHeight && local.Y <= 0.5f * beltHeight;
        }

        /// <summary>
        /// Determines whether a world-space point is within the belt's bounds plus a padding margin.
        /// </summary>
        /// <param name="worldPoint">The point to test in world coordinates.</param>
        /// <param name="padding">The extra margin around the belt bounds.</param>
        /// <returns>True if the point is inside the padded belt area; false otherwise.</returns>
        public bool ContainsWithPadding(Vector worldPoint, float padding)
        {
            Vector local = ToLocalSpace(worldPoint);
            return local.X >= -padding && local.X <= beltWidth + padding && local.Y >= (-0.5f * beltHeight) - padding && local.Y <= (0.5f * beltHeight) + padding;
        }

        /// <summary>
        /// Transforms a world-space point into the belt's local coordinate space.
        /// Local X runs along the belt length; local Y is perpendicular to the belt.
        /// </summary>
        /// <param name="worldPoint">The point in world coordinates.</param>
        /// <returns>The point in belt-local coordinates.</returns>
        public Vector ToLocalSpace(Vector worldPoint)
        {
            float perpAngle = rotationRad - (MathF.PI / 2f);
            Vector perp = Vect(Cosf(perpAngle), Sinf(perpAngle));
            float dx = worldPoint.X - x;
            float dy = worldPoint.Y - y;
            return Vect((direction.X * dx) + (direction.Y * dy), (perp.X * dx) + (perp.Y * dy));
        }

        /// <summary>
        /// Attaches an item to the conveyor belt for transport.
        /// </summary>
        /// <param name="item">The element to attach to the belt.</param>
        public void AttachItem(BaseElement item)
        {
            RegisterItem(item);
        }

        /// <summary>
        /// Marks an item for removal from the belt. The item will be removed once it exits the belt bounds.
        /// </summary>
        /// <param name="item">The element to mark for removal.</param>
        public void MarkItemForRemoval(BaseElement item)
        {
            if (itemStates.TryGetValue(item, out ConveyorItemState state))
            {
                state.markedForRemoval = true;
            }

            if (item is IConveyorItem conveyorItem)
            {
                conveyorItem.ConveyorId = -1;
            }
        }

        /// <summary>
        /// Checks whether an item is currently attached to this belt.
        /// </summary>
        /// <param name="item">The element to check.</param>
        /// <returns>True if the item is on this belt; false otherwise.</returns>
        public bool HasItem(BaseElement item)
        {
            return itemStates.ContainsKey(item);
        }

        /// <summary>
        /// Immediately removes an item from the belt's tracking state.
        /// </summary>
        /// <param name="item">The element to remove.</param>
        public void Remove(BaseElement item)
        {
            _ = itemStates.Remove(item);
        }

        /// <summary>
        /// Checks whether an item has been marked for removal.
        /// </summary>
        /// <param name="item">The element to check.</param>
        /// <returns>True if the item is marked for removal; false otherwise.</returns>
        public bool IsItemMarkedForRemoval(BaseElement item)
        {
            return itemStates.TryGetValue(item, out ConveyorItemState state) && state.markedForRemoval;
        }

        /// <summary>
        /// Determines whether the belt is currently moving.
        /// </summary>
        /// <returns>True if the belt has non-zero movement delta; false otherwise.</returns>
        public bool IsActive()
        {
            return active;
        }

        /// <summary>
        /// Gets whether this belt is controlled manually by user drag input.
        /// </summary>
        public bool IsManual { get; private set; }

        /// <summary>
        /// Gets the normalized direction vector along the belt's length.
        /// </summary>
        public Vector Direction => direction;

        /// <summary>
        /// Wraps an offset value to stay within the belt width range.
        /// </summary>
        private static float WrapOffset(float value, float maxWidth)
        {
            float width = maxWidth;
            float wrapped = value;
            if (wrapped > maxWidth)
            {
                wrapped -= width;
            }
            if (wrapped < 0f)
            {
                wrapped += width;
            }
            return wrapped;
        }

        /// <summary>
        /// Registers an item with the belt and calculates its initial offset position.
        /// </summary>
        private void RegisterItem(BaseElement item)
        {
            if (itemStates.ContainsKey(item))
            {
                return;
            }

            Vector position = GetItemPosition(item);
            Vector offsetVector = Vect(position.X - x, position.Y - y);
            float initialOffset = MathF.Max(MathF.Min((offsetVector.X * direction.X) + (offsetVector.Y * direction.Y), beltWidth), 0f);
            itemStates[item] = new ConveyorItemState(initialOffset);
            items.Add(item);
            if (item is IConveyorItem conveyorItem)
            {
                conveyorItem.ConveyorId = id;
            }
            CacheBaseScale(item);
        }

        /// <summary>
        /// Constructs the belt's visual components including frame, pillars, and moving surface.
        /// </summary>
        private void BuildVisuals()
        {
            const float endScale = 0.6f;
            const float plateScale = 0.8f;
            BaseElement visualRoot = new()
            {
                width = (int)MathF.Ceiling(beltHeight),
                height = (int)MathF.Ceiling(beltWidth),
                anchor = 18,
                parentAnchor = 18,
                rotation = -90f
            };
            _ = AddChild(visualRoot);

            float transporterWidth = visualRoot.width;
            float transporterHeight = visualRoot.height;

            Image endTemplate = CreatePiece(ImgObjConveyorEnd, 34);
            float capOffset = endTemplate.height * 0.25f;

            Image middle = CreatePiece(ImgObjConveyorMiddle, 18);
            middle.scaleX = (transporterWidth - 10f) / middle.width;
            middle.scaleY = transporterHeight / middle.height;
            _ = visualRoot.AddChild(middle);

            Image bottomEnd = endTemplate;
            bottomEnd.y = capOffset;
            bottomEnd.scaleX = transporterWidth * endScale / bottomEnd.width;
            _ = visualRoot.AddChild(bottomEnd);

            Image topEnd = CreatePiece(ImgObjConveyorEnd, 10);
            topEnd.y = -capOffset;
            topEnd.scaleX = transporterWidth * endScale / topEnd.width;
            _ = visualRoot.AddChild(topEnd);

            Image leftSide = CreatePiece(ImgObjConveyorMiddleSide, 17);
            float sideScaleY = (transporterHeight - (2f * capOffset)) / leftSide.height;
            leftSide.scaleX = -1f;
            leftSide.scaleY = sideScaleY;
            _ = visualRoot.AddChild(leftSide);

            Image rightSide = CreatePiece(ImgObjConveyorMiddleSide, 20);
            rightSide.scaleY = sideScaleY;
            _ = visualRoot.AddChild(rightSide);

            Image bottomRightCorner = CreatePiece(ImgObjConveyorEndSide, 36);
            bottomRightCorner.y = capOffset;
            _ = visualRoot.AddChild(bottomRightCorner);

            Image bottomLeftCorner = CreatePiece(ImgObjConveyorEndSide, 33);
            bottomLeftCorner.y = capOffset;
            bottomLeftCorner.scaleX = -1f;
            _ = visualRoot.AddChild(bottomLeftCorner);

            Image topLeftCorner = CreatePiece(ImgObjConveyorEndSide, 9);
            topLeftCorner.y = -capOffset;
            topLeftCorner.scaleX = -1f;
            topLeftCorner.scaleY = -1f;
            _ = visualRoot.AddChild(topLeftCorner);

            Image topRightCorner = CreatePiece(ImgObjConveyorEndSide, 12);
            topRightCorner.y = -capOffset;
            topRightCorner.scaleY = -1f;
            _ = visualRoot.AddChild(topRightCorner);

            int beltDirection = IsManual ? 0 : velocity > 0f ? 1 : -1;
            beltVisual = new ConveyorBeltVisual(transporterWidth * plateScale, transporterHeight, beltDirection)
            {
                anchor = 10,
                parentAnchor = 10
            };
            _ = visualRoot.AddChild(beltVisual);

            Image bottomHighlight = CreatePiece(ImgObjConveyorHighlight, 34);
            bottomHighlight.scaleX = transporterWidth * plateScale / bottomHighlight.width;
            _ = visualRoot.AddChild(bottomHighlight);

            Image topHighlight = CreatePiece(ImgObjConveyorHighlight, 10);
            topHighlight.scaleX = transporterWidth * plateScale / topHighlight.width;
            topHighlight.scaleY = -1f;
            _ = visualRoot.AddChild(topHighlight);
        }

        /// <summary>
        /// Creates a visual piece for the belt frame from the transporter sprite sheet.
        /// </summary>
        private static Image CreatePiece(int quad, int anchors)
        {
            Image piece = Image.Image_createWithResIDQuad(Resources.Img.ObjConveyor, quad);
            piece.anchor = (sbyte)anchors;
            piece.parentAnchor = (sbyte)anchors;
            return piece;
        }

        /// <summary>
        /// Removes items that are marked for removal and have exited the belt bounds.
        /// </summary>
        private void CleanupMarkedItems()
        {
            List<BaseElement> toRemove = [];
            foreach (KeyValuePair<BaseElement, ConveyorItemState> kvp in itemStates)
            {
                if (kvp.Value.markedForRemoval && !Contains(GetItemPosition(kvp.Key)))
                {
                    toRemove.Add(kvp.Key);
                }
            }

            foreach (BaseElement item in toRemove)
            {
                _ = itemStates.Remove(item);
                _ = items.Remove(item);
                RestoreItemScale(item);
            }
        }

        /// <summary>
        /// Plays a random conveyor movement sound effect for manual dragging feedback.
        /// </summary>
        private static void PlayManualMoveSound()
        {
            CTRSoundMgr.PlayRandomSound(Resources.Snd.Conv01, Resources.Snd.Conv02, Resources.Snd.Conv03, Resources.Snd.Conv04);
        }

        /// <summary>
        /// Stores the item's original scale values for later restoration.
        /// </summary>
        private static void CacheBaseScale(BaseElement item)
        {
            if (item is not IConveyorItem conveyorItem)
            {
                return;
            }

            conveyorItem.ConveyorBaseScaleX ??= item.scaleX;
            conveyorItem.ConveyorBaseScaleY ??= item.scaleY;
        }

        /// <summary>
        /// Restores the item's scale to its original cached values.
        /// </summary>
        private static void RestoreItemScale(BaseElement item)
        {
            if (item is not IConveyorItem conveyorItem)
            {
                return;
            }

            if (conveyorItem.ConveyorBaseScaleX.HasValue)
            {
                item.scaleX = conveyorItem.ConveyorBaseScaleX.Value;
            }
            if (conveyorItem.ConveyorBaseScaleY.HasValue)
            {
                item.scaleY = conveyorItem.ConveyorBaseScaleY.Value;
            }
        }

        /// <summary>
        /// Applies a scale factor to the item relative to its cached base scale.
        /// </summary>
        private static void ApplyItemScale(BaseElement item, float scale)
        {
            if (item is not IConveyorItem conveyorItem)
            {
                return;
            }

            CacheBaseScale(item);
            float baseX = conveyorItem.ConveyorBaseScaleX ?? 1f;
            float baseY = conveyorItem.ConveyorBaseScaleY ?? 1f;
            item.scaleX = baseX * scale;
            item.scaleY = baseY * scale;
        }

        /// <summary>
        /// Gets the world-space position of an item, using its conveyor position provider if available.
        /// </summary>
        /// <param name="item">The element to get the position for.</param>
        /// <returns>The item's position in world coordinates.</returns>
        public static Vector GetItemPosition(BaseElement item)
        {
            return item is IConveyorPositionProvider provider ? provider.GetConveyorPosition() : Vect(item.x, item.y);
        }

        /// <summary>
        /// Sets the world-space position of an item, using its conveyor position setter if available.
        /// </summary>
        private static void SetItemPosition(BaseElement item, Vector position)
        {
            if (item is IConveyorPositionSetter setter)
            {
                setter.SetConveyorPosition(position);
                return;
            }
            item.x = position.X;
            item.y = position.Y;
        }

        /// <summary>
        /// Determines the effective size of an item for collision and spacing calculations.
        /// </summary>
        private static Vector GetItemSize(BaseElement item)
        {
            if (item is IConveyorSizeProvider provider)
            {
                return provider.GetConveyorSize();
            }

            float rawWidth = item.width;
            float rawHeight = item.height;
            float bbWidth = 0f;
            float bbHeight = 0f;

            if (item is GameObject gameObject)
            {
                bbWidth = gameObject.bb.w;
                bbHeight = gameObject.bb.h;
            }

            float fallbackWidth = bbWidth > 0f ? bbWidth : rawWidth;
            float fallbackHeight = bbHeight > 0f ? bbHeight : rawHeight;

            // When restoreCutTransparency is disabled and there's no bounding box,
            // use the texture quad rect dimensions as a fallback.
            // When restoreCutTransparency is enabled, Image.width/height are already
            // set to preCutSize, so use those values (rawWidth/rawHeight) instead.
            if (bbWidth <= 0f && bbHeight <= 0f &&
                item is Image image &&
                !image.restoreCutTransparency &&
                image.quadToDraw >= 0 &&
                image.texture?.quadRects != null &&
                image.quadToDraw < image.texture.quadRects.Length)
            {
                CTRRectangle rect = image.texture.quadRects[image.quadToDraw];
                fallbackWidth = rect.w;
                fallbackHeight = rect.h;
            }

            if (item is IConveyorItem conveyorItem)
            {
                float scaleX = MathF.Abs(conveyorItem.ConveyorBaseScaleX ?? 1f);
                float scaleY = MathF.Abs(conveyorItem.ConveyorBaseScaleY ?? 1f);
                return Vect(fallbackWidth * scaleX, fallbackHeight * scaleY);
            }

            return Vect(fallbackWidth, fallbackHeight);
        }

        /// <summary>
        /// Gets the padding distance for detecting when an item is near the belt.
        /// </summary>
        /// <param name="item">The element to get padding for.</param>
        /// <returns>The padding distance in world units.</returns>
        public static float GetItemPadding(BaseElement item)
        {
            if (item is IConveyorPaddingProvider provider)
            {
                return provider.GetConveyorPadding();
            }
            Vector size = GetItemSize(item);
            return (size.X + size.Y) / 4f;
        }

        private float velocity = 10f;
        private float offset;
        private int id = -1;
        private float manualTravelDistance;
        private float rotationRad;
        private float offsetDelta;
        private Vector direction;
        private bool active;
        private int activePointerId = -1;
        private Vector lastDragPosition;
        private ConveyorBeltVisual beltVisual;
        private readonly Dictionary<BaseElement, ConveyorItemState> itemStates = [];
        private readonly List<BaseElement> items = [];
        private float beltWidth;
        private float beltHeight;
    }
}
