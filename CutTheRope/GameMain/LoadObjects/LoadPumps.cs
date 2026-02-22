using System.Xml.Linq;

using CutTheRope.Framework.Visual;
using CutTheRope.Helpers;
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
            pump.initial_x = pump.x = ((string.IsNullOrEmpty(xmlNode.AttributeAsNSString("x")) ? 0 : int.Parse(xmlNode.AttributeAsNSString("x"))) * scale) + offsetX + mapOffsetX;
            pump.initial_y = pump.y = ((string.IsNullOrEmpty(xmlNode.AttributeAsNSString("y")) ? 0 : int.Parse(xmlNode.AttributeAsNSString("y"))) * scale) + offsetY + mapOffsetY;
            pump.initial_rotation = 0f;
            pump.initial_rotatedCircle = null;
            pump.rotation = (string.IsNullOrEmpty(xmlNode.AttributeAsNSString("angle")) ? 0f : float.Parse(xmlNode.AttributeAsNSString("angle"), CultureInfo.InvariantCulture)) + DEG_90;
            pump.UpdateRotation();
            pump.anchor = 18;
            pumps.Add(pump);
        }
    }
}
