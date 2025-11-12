using CutTheRope.desktop;
using CutTheRope.iframework;
using CutTheRope.iframework.core;
using CutTheRope.iframework.helpers;
using CutTheRope.iframework.sfe;
using CutTheRope.iframework.visual;
using CutTheRope.ios;

namespace CutTheRope.game
{
    /// <summary>
    /// Handles loading spike objects from XML level data
    /// Supports regular spikes (spike1-4) and electro spikes
    /// </summary>
    internal sealed partial class GameScene
    {
        private void LoadSpike(XMLNode item, float scale, float offsetX, float offsetY, int mapOffsetX, int mapOffsetY)
        {
            if (!item.Name.Contains("spike") && item.Name != "electro") return;

            float px = (item["x"].IntValue() * scale) + offsetX + mapOffsetX;
            float py = (item["y"].IntValue() * scale) + offsetY + mapOffsetY;
            int w = item["size"].IntValue();
            double an = item["angle"].IntValue();
            NSString nSString2 = item["toggled"];
            int num8 = -1;
            if (nSString2.Length() > 0)
            {
                num8 = nSString2.IsEqualToString("false") ? -1 : nSString2.IntValue();
            }
            Spikes spikes = (Spikes)new Spikes().InitWithPosXYWidthAndAngleToggled(px, py, w, an, num8);
            spikes.ParseMover(item);
            if (num8 != 0)
            {
                spikes.delegateRotateAllSpikesWithID = new Spikes.rotateAllSpikesWithID(RotateAllSpikesWithID);
            }
            if (item.Name == "electro")
            {
                spikes.electro = true;
                spikes.initialDelay = item["initialDelay"].FloatValue();
                spikes.onTime = item["onTime"].FloatValue();
                spikes.offTime = item["offTime"].FloatValue();
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
