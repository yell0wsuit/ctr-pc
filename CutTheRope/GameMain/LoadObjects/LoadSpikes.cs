using System.Xml.Linq;

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
            float px = ((string.IsNullOrEmpty(xmlNode.Attribute("x")?.Value ?? string.Empty) ? 0 : int.Parse(xmlNode.Attribute("x")?.Value ?? string.Empty)) * scale) + offsetX + mapOffsetX;
            float py = ((string.IsNullOrEmpty(xmlNode.Attribute("y")?.Value ?? string.Empty) ? 0 : int.Parse(xmlNode.Attribute("y")?.Value ?? string.Empty)) * scale) + offsetY + mapOffsetY;
            int w = string.IsNullOrEmpty(xmlNode.Attribute("size")?.Value ?? string.Empty) ? 0 : int.Parse(xmlNode.Attribute("size")?.Value ?? string.Empty);
            float an = string.IsNullOrEmpty(xmlNode.Attribute("angle")?.Value ?? string.Empty) ? 0 : int.Parse(xmlNode.Attribute("angle")?.Value ?? string.Empty);
            string toggledAttribute = xmlNode.Attribute("toggled")?.Value ?? string.Empty;
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
                spikes.initialDelay = string.IsNullOrEmpty(xmlNode.Attribute("initialDelay")?.Value ?? string.Empty) ? 0f : float.Parse(xmlNode.Attribute("initialDelay")?.Value ?? string.Empty, CultureInfo.InvariantCulture);
                spikes.onTime = string.IsNullOrEmpty(xmlNode.Attribute("onTime")?.Value ?? string.Empty) ? 0f : float.Parse(xmlNode.Attribute("onTime")?.Value ?? string.Empty, CultureInfo.InvariantCulture);
                spikes.offTime = string.IsNullOrEmpty(xmlNode.Attribute("offTime")?.Value ?? string.Empty) ? 0f : float.Parse(xmlNode.Attribute("offTime")?.Value ?? string.Empty, CultureInfo.InvariantCulture);
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
