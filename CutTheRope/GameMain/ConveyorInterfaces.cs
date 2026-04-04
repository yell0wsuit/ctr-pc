using CutTheRope.Framework.Core;

namespace CutTheRope.GameMain
{
    /// <summary>
    /// Defines properties and methods required for an object to ride on a transporter (conveyor belt).
    /// </summary>
    internal interface ITransporterItem
    {
        /// <summary>Normalized position along the transporter path (0–1).</summary>
        float PositionOnTransporter { get; set; }

        /// <summary>World-space point where the item attaches to the transporter.</summary>
        Vector BindPoint { get; }

        /// <summary>Sets the bind point to <paramref name="point"/>.</summary>
        void SetBindPoint(Vector point);

        /// <summary>Collision radius used for transporter interaction.</summary>
        float CollisionRadius { get; }

        /// <summary>Minimum scale applied while on the transporter.</summary>
        float MinScale { get; }

        /// <summary>Maximum scale applied while on the transporter.</summary>
        float MaxScale { get; }

        /// <summary>Current scale factor applied by the transporter.</summary>
        float TransporterScale { get; set; }

        /// <summary>Whether the transporter is responsible for drawing this item.</summary>
        bool IsDrawnByTransporter { get; set; }
    }

    /// <summary>
    /// Optional callback invoked right before an object is bound to a transporter.
    /// Mirrors iOS willBind selector semantics.
    /// </summary>
    internal interface ITransporterBindAware
    {
        void WillBind();
    }

    /// <summary>
    /// Optional callback invoked when an object wraps to the opposite transporter side.
    /// Mirrors iOS didMoveToOtherSide selector semantics.
    /// </summary>
    internal interface ITransporterSideSwitchAware
    {
        void DidMoveToOtherSide();
    }

    /// <summary>
    /// Optional callback for custom transporter scaling.
    /// Mirrors iOS classes that override setScale: (e.g., Grab, SteamTube).
    /// </summary>
    internal interface ITransporterScaleAware
    {
        void SetTransporterScale(float scale);
    }
}
