using CutTheRope.Framework.Core;

namespace CutTheRope.GameMain
{
    /// <summary>
    /// Implemented by elements that can ride on a conveyor belt and need to track their belt association and original scale.
    /// </summary>
    internal interface IConveyorItem
    {
        /// <summary>
        /// Gets or sets the ID of the conveyor belt this item is attached to, or -1 if not on any belt.
        /// </summary>
        int ConveyorId { get; set; }

        /// <summary>
        /// Gets or sets the cached original X scale before conveyor scaling was applied.
        /// </summary>
        float? ConveyorBaseScaleX { get; set; }

        /// <summary>
        /// Gets or sets the cached original Y scale before conveyor scaling was applied.
        /// </summary>
        float? ConveyorBaseScaleY { get; set; }
    }

    /// <summary>
    /// Implemented by elements that provide a custom size for conveyor collision and spacing calculations.
    /// </summary>
    internal interface IConveyorSizeProvider
    {
        /// <summary>
        /// Gets the size to use for conveyor belt collision and spacing calculations.
        /// </summary>
        /// <returns>The width and height as a vector.</returns>
        Vector GetConveyorSize();
    }

    /// <summary>
    /// Implemented by elements that provide a custom padding distance for belt proximity detection.
    /// </summary>
    internal interface IConveyorPaddingProvider
    {
        /// <summary>
        /// Gets the padding distance for detecting when this item is near a conveyor belt.
        /// </summary>
        /// <returns>The padding distance in world units.</returns>
        float GetConveyorPadding();
    }

    /// <summary>
    /// Implemented by elements that provide a custom position for conveyor belt calculations.
    /// </summary>
    internal interface IConveyorPositionProvider
    {
        /// <summary>
        /// Gets the position to use for conveyor belt offset calculations.
        /// </summary>
        /// <returns>The position in world coordinates.</returns>
        Vector GetConveyorPosition();
    }

    /// <summary>
    /// Implemented by elements that handle position updates from the conveyor belt in a custom way.
    /// </summary>
    internal interface IConveyorPositionSetter
    {
        /// <summary>
        /// Sets the position as determined by the conveyor belt movement.
        /// </summary>
        /// <param name="position">The new position in world coordinates.</param>
        void SetConveyorPosition(Vector position);
    }

    /// <summary>
    /// Implemented by elements that need to respond when they wrap around the conveyor belt edges.
    /// </summary>
    internal interface IConveyorDropHandler
    {
        /// <summary>
        /// Called when the item wraps around from one end of the conveyor belt to the other.
        /// </summary>
        void OnConveyorDrop();
    }
}
