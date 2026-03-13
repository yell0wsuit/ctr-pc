using System;
using System.Collections.Generic;

using CutTheRope.Framework.Core;
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
            _ = id;
            activePointerId = -1;
            this.x = x;
            this.y = y;
            beltWidth = length;
            beltHeight = height;
            width = (int)MathF.Ceiling(length);
            this.height = (int)MathF.Ceiling(height);

            this.rotation = -rotation;
            IsManual = isManual;
            rotationRad = DEGREES_TO_RADIANS(rotation);
            direction = Vect(Cosf(rotationRad), -Sinf(rotationRad));
            this.velocity = velocity;
            rotationCenterX = -length / 2f;
            rotationCenterY = 0f;
            transitionDist = height * 0.25f;

            RemoveAllChilds();
            BuildVisuals();
        }

        /// <summary>
        /// Transforms a local-space vector to world-space coordinates.
        /// </summary>
        /// <param name="localX">The local X coordinate (along belt length).</param>
        /// <param name="localY">The local Y coordinate (perpendicular to belt).</param>
        /// <returns>The world-space position.</returns>
        private Vector VecToWorldSpace(float localX, float localY)
        {
            float cosA = Cosf(rotationRad);
            float sinA = -Sinf(rotationRad);
            return Vect(
                x + (cosA * localX) - (sinA * localY),
                y + (sinA * localX) + (cosA * localY));
        }

        /// <summary>
        /// Checks whether a circle (center + radius) overlaps the belt's axis-aligned bounds.
        /// </summary>
        /// <param name="center">The center of the circle in world space.</param>
        /// <param name="radius">The radius of the circle.</param>
        /// <returns>True if the circle overlaps the belt bounds; false otherwise.</returns>
        public bool CollidesWithCircle(Vector center, float radius)
        {
            Vector local = ToLocalSpace(center);
            // local.X = along belt, local.Y = perpendicular
            if (local.X < -radius || local.X > beltWidth + radius)
            {
                return false;
            }

            float halfHeight = beltHeight * 0.5f;
            return local.Y >= -halfHeight - radius && local.Y <= halfHeight + radius;
        }

        /// <summary>
        /// Binds an item to this conveyor belt, setting its initial position along the belt.
        /// </summary>
        /// <param name="item">The transporter item to bind.</param>
        public void BindObject(ITransporterItem item)
        {
            if (boundObjects.Contains(item))
            {
                return;
            }

            boundObjects.Add(item);

            Vector local = ToLocalSpace(item.BindPoint);
            item.PositionOnTransporter = local.X;

            // Determine which end the item is near
            float pos = item.PositionOnTransporter;
            int side;
            float distFromEdge;
            if (pos < beltWidth * 0.5f)
            {
                side = 0;
                distFromEdge = pos;
            }
            else
            {
                side = 1;
                distFromEdge = beltWidth - pos;
            }

            // If past the edge, clamp to transitionDist from the opposite end
            if (distFromEdge < 0f)
            {
                item.PositionOnTransporter = side == 1 ? beltWidth - transitionDist : transitionDist;

                Vector worldPos = VecToWorldSpace(item.PositionOnTransporter, local.Y);
                item.SetBindPoint(worldPos);
            }
        }

        /// <summary>
        /// Checks whether an item is currently bound to this belt.
        /// </summary>
        /// <param name="item">The transporter item to check.</param>
        /// <returns>True if the item is on this belt; false otherwise.</returns>
        public bool HasItem(ITransporterItem item)
        {
            return boundObjects.Contains(item);
        }

        /// <summary>
        /// Removes a bound item from this belt.
        /// </summary>
        /// <param name="item">The transporter item to remove.</param>
        public void Remove(ITransporterItem item)
        {
            _ = boundObjects.Remove(item);
        }

        /// <summary>
        /// Updates the belt and all bound items each frame. Handles movement, wrapping,
        /// and scale transitions at belt edges.
        /// </summary>
        /// <param name="deltaTime">The time elapsed since the last frame in seconds.</param>
        public override void Update(float deltaTime)
        {
            base.Update(deltaTime);

            if (!IsManual)
            {
                offsetDelta = deltaTime * velocity;
                MoveBoundObjects(offsetDelta);
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

            if (IsManual)
            {
                offsetDelta = 0f;
            }
        }

        /// <summary>
        /// Moves all bound objects along the belt by the given delta, handling wrapping
        /// and scale transitions at belt edges.
        /// </summary>
        /// <param name="delta">The distance to move items along the belt.</param>
        private void MoveBoundObjects(float delta)
        {
            beltVisual?.Move(-delta);

            foreach (ITransporterItem item in boundObjects)
            {
                Vector local = ToLocalSpace(item.BindPoint);

                // Move along belt
                item.PositionOnTransporter -= delta;

                float pos = item.PositionOnTransporter;

                // Determine which end is closer
                int side;
                float distFromEdge;
                if (pos < beltWidth * 0.5f)
                {
                    side = 0;
                    distFromEdge = pos;
                }
                else
                {
                    side = 1;
                    distFromEdge = beltWidth - pos;
                }

                // Check if within transition zone
                if (distFromEdge < transitionDist)
                {
                    float newDist = (2f * transitionDist) - distFromEdge;
                    item.PositionOnTransporter = side == 0 ? beltWidth - newDist : newDist;

                    CTRSoundMgr.PlaySound(Resources.Snd.TransporterDrop);
                    side ^= 1;

                    pos = item.PositionOnTransporter;
                    distFromEdge = pos < beltWidth * 0.5f ? pos : beltWidth - pos;
                }

                // Compute scale
                float collisionRadius = item.CollisionRadius;
                float maxScale = item.MaxScale;
                float minScale = item.MinScale;

                if (distFromEdge >= collisionRadius * maxScale)
                {
                    item.TransporterScale = maxScale;
                    Vector worldPos = VecToWorldSpace(item.PositionOnTransporter, local.Y);
                    item.SetBindPoint(worldPos);
                }
                else
                {
                    float scaleRange = maxScale - minScale;
                    float scale = minScale + (distFromEdge * scaleRange / (collisionRadius * maxScale));
                    item.TransporterScale = scale;

                    float adjustedPos = side == 1 ? beltWidth - (scale * collisionRadius) : scale * collisionRadius;
                    Vector worldPos = VecToWorldSpace(adjustedPos, local.Y);
                    item.SetBindPoint(worldPos);
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
                MoveBoundObjects(offsetDelta);
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
            float perpAngle = -rotationRad - (MathF.PI / 2f);
            Vector perp = Vect(Cosf(perpAngle), Sinf(perpAngle));
            float dx = worldPoint.X - x;
            float dy = worldPoint.Y - y;
            return Vect((direction.X * dx) + (direction.Y * dy), (perp.X * dx) + (perp.Y * dy));
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
                rotation = 90f
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

            int beltDirection = IsManual ? 0 : velocity > 0f ? -1 : 1;
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
        /// Plays a random conveyor movement sound effect for manual dragging feedback.
        /// </summary>
        private static void PlayManualMoveSound()
        {
            CTRSoundMgr.PlayRandomSound(Resources.Snd.Conv01, Resources.Snd.Conv02, Resources.Snd.Conv03, Resources.Snd.Conv04);
        }

        private float velocity;
        private float manualTravelDistance;
        private float rotationRad;
        private float offsetDelta;
        private Vector direction;
        private bool active;
        private int activePointerId = -1;
        private Vector lastDragPosition;
        private ConveyorBeltVisual beltVisual;
        private readonly List<ITransporterItem> boundObjects = [];
        private float transitionDist;
        private float beltWidth;
        private float beltHeight;
    }
}
