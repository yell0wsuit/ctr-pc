using CutTheRope.Framework.Core;

namespace CutTheRope.GameMain
{
    internal interface ITransporterItem
    {
        float PositionOnTransporter { get; set; }
        Vector BindPoint { get; }
        void SetBindPoint(Vector point);
        float CollisionRadius { get; }
        float MinScale { get; }
        float MaxScale { get; }
        float TransporterScale { get; set; }
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
