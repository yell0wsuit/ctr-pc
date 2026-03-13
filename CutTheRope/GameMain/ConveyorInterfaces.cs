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
}
