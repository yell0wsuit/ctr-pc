using System.Xml.Linq;

using CutTheRope.Helpers;

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
            float px = (xmlNode.AttributeAsNSString("x").IntValue() * scale) + offsetX + mapOffsetX;
            float py = (xmlNode.AttributeAsNSString("y").IntValue() * scale) + offsetY + mapOffsetY;
            int w = xmlNode.AttributeAsNSString("size").IntValue();
            float an = xmlNode.AttributeAsNSString("angle").IntValue();
            string toggledAttribute = xmlNode.AttributeAsNSString("toggled");
            int toggledState = -1;
            if (toggledAttribute.Length() > 0)
            {
                toggledState = toggledAttribute.IsEqualToString("false") ? -1 : toggledAttribute.IntValue();
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
                spikes.initialDelay = xmlNode.AttributeAsNSString("initialDelay").FloatValue();
                spikes.onTime = xmlNode.AttributeAsNSString("onTime").FloatValue();
                spikes.offTime = xmlNode.AttributeAsNSString("offTime").FloatValue();
                spikes.electroTimer = 0f;
                spikes.TurnElectroOff();
                spikes.electroTimer += spikes.initialDelay;
                spikes.UpdateRotation();
            }
            else
            {
                spikes.electro = false;
            }
            _ = this.spikes.AddObject(spikes);
        }
    }
}
