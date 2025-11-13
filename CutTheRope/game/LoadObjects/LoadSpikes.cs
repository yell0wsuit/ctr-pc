using CutTheRope.ios;

namespace CutTheRope.game
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
        private void LoadSpike(XMLNode xmlNode, float scale, float offsetX, float offsetY, int mapOffsetX, int mapOffsetY)
        {
            float px = (xmlNode["x"].IntValue() * scale) + offsetX + mapOffsetX;
            float py = (xmlNode["y"].IntValue() * scale) + offsetY + mapOffsetY;
            int w = xmlNode["size"].IntValue();
            double an = xmlNode["angle"].IntValue();
            NSString nSString2 = xmlNode["toggled"];
            int num8 = -1;
            if (nSString2.Length() > 0)
            {
                num8 = nSString2.IsEqualToString("false") ? -1 : nSString2.IntValue();
            }
            Spikes spikes = (Spikes)new Spikes().InitWithPosXYWidthAndAngleToggled(px, py, w, an, num8);
            spikes.ParseMover(xmlNode);
            if (num8 != 0)
            {
                spikes.delegateRotateAllSpikesWithID = new Spikes.rotateAllSpikesWithID(RotateAllSpikesWithID);
            }
            if (xmlNode.Name == "electro")
            {
                spikes.electro = true;
                spikes.initialDelay = xmlNode["initialDelay"].FloatValue();
                spikes.onTime = xmlNode["onTime"].FloatValue();
                spikes.offTime = xmlNode["offTime"].FloatValue();
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
