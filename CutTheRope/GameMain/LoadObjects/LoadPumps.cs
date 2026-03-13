using System.Xml.Linq;

using CutTheRope.Framework.Visual;

using static CutTheRope.Helpers.ParsingHelpers;

namespace CutTheRope.GameMain
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
            Pump pump = Pump.Pump_createWithResID(Resources.Img.ObjPump);
            pump.DoRestoreCutTransparency();
            _ = pump.AddAnimationWithDelayLoopedCountSequence(0.05f, Timeline.LoopType.TIMELINE_NO_LOOP, 4, 1, [2, 3, 0]);
            pump.bb = GetPumpBoundingBox();
            pump.initial_x = pump.x = (ParseIntOrZero(xmlNode.Attribute("x")?.Value) * scale) + offsetX + mapOffsetX;
            pump.initial_y = pump.y = (ParseIntOrZero(xmlNode.Attribute("y")?.Value) * scale) + offsetY + mapOffsetY;
            pump.initial_rotation = 0f;
            pump.initial_rotatedCircle = null;
            pump.rotation = ParseFloatOrZero(xmlNode.Attribute("angle")?.Value) + DEG_90;
            pump.UpdateRotation();
            pump.anchor = 18;
            pumps.Add(pump);
        }
    }
}
