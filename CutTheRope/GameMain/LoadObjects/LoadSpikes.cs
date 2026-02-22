using System.Xml.Linq;

using CutTheRope.Helpers;
using System.Globalization;

namespace CutTheRope.GameMain
{
    /// <summary>
    /// Handles loading spike objects from XML level data
    /// Supports regular spikes (spike1-4) and electro spikes
    /// </summary>
    internal sealed partial class GameScene
    {
        /// <summary>
        /// Loads a spike object from XML node data
        /// Supports regular spikes (spike1-4) and electro spikes
        /// </summary>
        private void LoadSpike(XElement xmlNode, float scale, float offsetX, float offsetY, int mapOffsetX, int mapOffsetY)
        {
            float px = ((string.IsNullOrEmpty(xmlNode.AttributeAsNSString("x")) ? 0 : int.Parse(xmlNode.AttributeAsNSString("x"))) * scale) + offsetX + mapOffsetX;
            float py = ((string.IsNullOrEmpty(xmlNode.AttributeAsNSString("y")) ? 0 : int.Parse(xmlNode.AttributeAsNSString("y"))) * scale) + offsetY + mapOffsetY;
            int w = string.IsNullOrEmpty(xmlNode.AttributeAsNSString("size")) ? 0 : int.Parse(xmlNode.AttributeAsNSString("size"));
            float an = string.IsNullOrEmpty(xmlNode.AttributeAsNSString("angle")) ? 0 : int.Parse(xmlNode.AttributeAsNSString("angle"));
            string toggledAttribute = xmlNode.AttributeAsNSString("toggled");
            int toggledState = -1;
            if (toggledAttribute.Length > 0)
            {
                toggledState = toggledAttribute == "false" ? -1 : (string.IsNullOrEmpty(toggledAttribute) ? 0 : int.Parse(toggledAttribute));
            }
            Spikes spikes = new Spikes().InitWithPosXYWidthAndAngleToggled(px, py, w, an, toggledState);
            spikes.ParseMover(xmlNode);
            if (toggledState != 0)
            {
                spikes.delegateRotateAllSpikesWithID = new Spikes.rotateAllSpikesWithID(RotateAllSpikesWithID);
            }
            if (xmlNode.Name.LocalName == "electro")
            {
                spikes.electro = true;
                spikes.initialDelay = string.IsNullOrEmpty(xmlNode.AttributeAsNSString("initialDelay")) ? 0f : float.Parse(xmlNode.AttributeAsNSString("initialDelay"), CultureInfo.InvariantCulture);
                spikes.onTime = string.IsNullOrEmpty(xmlNode.AttributeAsNSString("onTime")) ? 0f : float.Parse(xmlNode.AttributeAsNSString("onTime"), CultureInfo.InvariantCulture);
                spikes.offTime = string.IsNullOrEmpty(xmlNode.AttributeAsNSString("offTime")) ? 0f : float.Parse(xmlNode.AttributeAsNSString("offTime"), CultureInfo.InvariantCulture);
                spikes.electroTimer = 0f;
                spikes.TurnElectroOff();
                spikes.electroTimer += spikes.initialDelay;
                spikes.UpdateRotation();
            }
            else
            {
                spikes.electro = false;
            }
            this.spikes.Add(spikes);
        }
    }
}
