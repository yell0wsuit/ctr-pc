using CutTheRope.Framework.Core;

namespace CutTheRope.GameMain
{
    /// <summary>
    /// Defines properties and methods required for an object to ride on a transporter (conveyor belt).
    /// </summary>
    internal interface ITransporterItem
    {
        /// <summary>
        /// Normalized position along the transporter path (0–1).
        /// </summary>
        float PositionOnTransporter { get; set; }

        /// <summary>
        /// World-space point where the item attaches to the transporter.
        /// </summary>
        Vector BindPoint { get; }

        /// <summary>
        /// Sets the bind point to <paramref name="point"/>.
        /// </summary>
        /// <param name="point">New bind point.</param>
        void SetBindPoint(Vector point);

        /// <summary>
        /// Collision radius used for transporter interaction.
        /// </summary>
        float CollisionRadius { get; }

        /// <summary>
        /// Minimum scale applied while on the transporter.
        /// </summary>
        float MinScale { get; }

        /// <summary>
        /// Maximum scale applied while on the transporter.
        /// </summary>
        float MaxScale { get; }

        /// <summary>
        /// Current scale factor applied by the transporter.
        /// </summary>
        float TransporterScale { get; set; }

        /// <summary>
        /// Whether the transporter is responsible for drawing this item.
        /// </summary>
        bool IsDrawnByTransporter { get; set; }
    }

    /// <summary>
    /// Optional callback invoked right before an object is bound to a transporter.
    /// </summary>
    internal interface ITransporterBindAware
    {
        /// <summary>Called right before the object is bound to a transporter.</summary>
        void WillBind();
    }

    /// <summary>
    /// Optional callback invoked when an object wraps to the opposite transporter side.
    /// </summary>
    internal interface ITransporterSideSwitchAware
    {
        /// <summary>Called when the object wraps to the opposite transporter side.</summary>
        void DidMoveToOtherSide();
    }

    /// <summary>
    /// Optional callback for custom transporter scaling.
    /// </summary>
    internal interface ITransporterScaleAware
    {
        /// <summary>Applies a custom transporter scale to the object.</summary>
        /// <param name="scale">Scale factor to apply.</param>
        void SetTransporterScale(float scale);
    }
}
