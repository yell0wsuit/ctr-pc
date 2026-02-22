using System.Xml.Linq;

using CutTheRope.Framework.Visual;
using System.Globalization;

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
            pump.bb = MakeRectangle(300f, 300f, 175f, 175f);
            pump.initial_x = pump.x = ((string.IsNullOrEmpty(xmlNode.Attribute("x")?.Value ?? string.Empty) ? 0 : int.Parse(xmlNode.Attribute("x")?.Value ?? string.Empty)) * scale) + offsetX + mapOffsetX;
            pump.initial_y = pump.y = ((string.IsNullOrEmpty(xmlNode.Attribute("y")?.Value ?? string.Empty) ? 0 : int.Parse(xmlNode.Attribute("y")?.Value ?? string.Empty)) * scale) + offsetY + mapOffsetY;
            pump.initial_rotation = 0f;
            pump.initial_rotatedCircle = null;
            pump.rotation = (string.IsNullOrEmpty(xmlNode.Attribute("angle")?.Value ?? string.Empty) ? 0f : float.Parse(xmlNode.Attribute("angle")?.Value ?? string.Empty, CultureInfo.InvariantCulture)) + DEG_90;
            pump.UpdateRotation();
            pump.anchor = 18;
            pumps.Add(pump);
        }
    }
}
