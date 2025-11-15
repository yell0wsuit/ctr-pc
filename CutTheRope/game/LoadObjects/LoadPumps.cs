using System.Xml.Linq;

using CutTheRope.Helpers;
using CutTheRope.iframework.visual;

namespace CutTheRope.game
{
    /// <summary>
    /// Handles loading pump objects from XML level data
    /// </summary>
    internal sealed partial class GameScene
    {
        /// <summary>
        /// Loads a pump object from XML node data
        /// </summary>
        private void LoadPump(XElement xmlNode, float scale, float offsetX, float offsetY, int mapOffsetX, int mapOffsetY)
        {
            Pump pump = Pump.Pump_createWithResID(83);
            pump.DoRestoreCutTransparency();
            _ = pump.AddAnimationWithDelayLoopedCountSequence(0.05f, Timeline.LoopType.TIMELINE_NO_LOOP, 4, 1, [2, 3, 0]);
            pump.bb = MakeRectangle(300f, 300f, 175f, 175f);
            pump.initial_x = pump.x = (xmlNode.AttributeAsNSString("x").IntValue() * scale) + offsetX + mapOffsetX;
            pump.initial_y = pump.y = (xmlNode.AttributeAsNSString("y").IntValue() * scale) + offsetY + mapOffsetY;
            pump.initial_rotation = 0f;
            pump.initial_rotatedCircle = null;
            pump.rotation = xmlNode.AttributeAsNSString("angle").FloatValue() + 90f;
            pump.UpdateRotation();
            pump.anchor = 18;
            _ = pumps.AddObject(pump);
        }
    }
}
