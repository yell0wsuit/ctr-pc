using CutTheRopeDX.Framework.Visual;

using static CutTheRopeDX.Framework.Helpers.CTRMathHelper;

namespace CutTheRopeDX.GameMain
{
    /// <summary>
    /// A hook the player drags along a straight rail. The rail is player-driven, so no platform may
    /// capture it. Owns the rail's geometry, its three images, and the drag itself, which used to be
    /// spread across three separate blocks in <c>GameScene.Input</c>.
    /// </summary>
    internal sealed class RailMotion : AnchorMotion
    {
        /// <summary>Half-width of the square tap zone that starts a rail drag.</summary>
        private const float DragTapRadius = 65f;

        /// <summary>Initializes a rail.</summary>
        /// <param name="length">Rail length in world units.</param>
        /// <param name="isVertical">Whether the rail runs vertically.</param>
        /// <param name="offset">The hook's offset along the rail from its authored end.</param>
        /// <param name="anchorX">The hook's authored X position.</param>
        /// <param name="anchorY">The hook's authored Y position.</param>
        public RailMotion(float length, bool isVertical, float offset, float anchorX, float anchorY)
        {
            Length = length;
            IsVertical = isVertical;
            Offset = offset;
            DraggingTouch = -1;

            if (isVertical)
            {
                MinValue = anchorY - offset;
                MaxValue = anchorY + (length - offset);
            }
            else
            {
                MinValue = anchorX - offset;
                MaxValue = anchorX + (length - offset);
            }
        }

        /// <summary>Gets the rail length.</summary>
        public float Length { get; }

        /// <summary>Gets whether the rail runs vertically.</summary>
        public bool IsVertical { get; }

        /// <summary>Gets the hook's offset along the rail from its authored end.</summary>
        public float Offset { get; }

        /// <summary>Gets the lowest coordinate the hook may slide to.</summary>
        public float MinValue { get; }

        /// <summary>Gets the highest coordinate the hook may slide to.</summary>
        public float MaxValue { get; }

        /// <summary>Gets the touch index currently dragging the hook, or -1 when idle.</summary>
        public int DraggingTouch { get; private set; }

        /// <summary>Gets or sets the tiled rail background image.</summary>
        public HorizontallyTiledImage Background { get; set; }

        /// <summary>Gets or sets the draggable hook image.</summary>
        public Image Mover { get; set; }

        /// <summary>Gets or sets the highlighted draggable hook image.</summary>
        public Image MoverHighlight { get; set; }

        /// <inheritdoc />
        public override bool CanBind => false;

        /// <summary>Starts a drag when a touch lands on the hook.</summary>
        /// <param name="grab">The hook being dragged.</param>
        /// <param name="worldX">Touch X in world space.</param>
        /// <param name="worldY">Touch Y in world space.</param>
        /// <param name="touchIndex">Touch index.</param>
        /// <returns><see langword="true"/> when this touch now owns the hook.</returns>
        public bool TryBeginDrag(Grab grab, float worldX, float worldY, int touchIndex)
        {
            if (!PointInRect(
                    worldX, worldY,
                    grab.x - DragTapRadius, grab.y - DragTapRadius,
                    DragTapRadius * 2f, DragTapRadius * 2f))
            {
                return false;
            }

            DraggingTouch = touchIndex;
            return true;
        }

        /// <summary>Slides the hook to follow an active drag.</summary>
        /// <param name="grab">The hook being dragged.</param>
        /// <param name="worldX">Touch X in world space.</param>
        /// <param name="worldY">Touch Y in world space.</param>
        public void DragTo(Grab grab, float worldX, float worldY)
        {
            if (IsVertical)
            {
                grab.y = FIT_TO_BOUNDARIES(worldY, MinValue, MaxValue);
            }
            else
            {
                grab.x = FIT_TO_BOUNDARIES(worldX, MinValue, MaxValue);
            }
        }

        /// <summary>Ends the drag if this touch owns it.</summary>
        /// <param name="touchIndex">Touch index being released.</param>
        public void EndDrag(int touchIndex)
        {
            if (DraggingTouch == touchIndex)
            {
                DraggingTouch = -1;
            }
        }
    }
}
