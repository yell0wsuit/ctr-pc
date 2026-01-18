using CutTheRope.Framework.Core;

namespace CutTheRope.GameMain
{
    internal interface IConveyorItem
    {
        int ConveyorId { get; set; }
        float? ConveyorBaseScaleX { get; set; }
        float? ConveyorBaseScaleY { get; set; }
    }

    internal interface IConveyorSizeProvider
    {
        Vector GetConveyorSize();
    }

    internal interface IConveyorPaddingProvider
    {
        float GetConveyorPadding();
    }

    internal interface IConveyorPositionProvider
    {
        Vector GetConveyorPosition();
    }

    internal interface IConveyorPositionSetter
    {
        void SetConveyorPosition(Vector position);
    }

    internal interface IConveyorDropHandler
    {
        void OnConveyorDrop();
    }
}
