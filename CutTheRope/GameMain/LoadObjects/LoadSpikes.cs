using System.Xml.Linq;


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
            float px = (ParseIntOrZero(xmlNode.Attribute("x")?.Value) * scale) + offsetX + mapOffsetX;
            float py = (ParseIntOrZero(xmlNode.Attribute("y")?.Value) * scale) + offsetY + mapOffsetY;
            int w = ParseIntOrZero(xmlNode.Attribute("size")?.Value);
            float an = ParseIntOrZero(xmlNode.Attribute("angle")?.Value);
            string toggledAttribute = xmlNode.Attribute("toggled")?.Value ?? string.Empty;
            int toggledState = -1;
            if (toggledAttribute.Length > 0)
            {
                toggledState = toggledAttribute == "false" ? -1 : ParseIntOrZero(toggledAttribute);
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
                spikes.initialDelay = ParseFloatOrZero(xmlNode.Attribute("initialDelay")?.Value);
                spikes.onTime = ParseFloatOrZero(xmlNode.Attribute("onTime")?.Value);
                spikes.offTime = ParseFloatOrZero(xmlNode.Attribute("offTime")?.Value);
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
