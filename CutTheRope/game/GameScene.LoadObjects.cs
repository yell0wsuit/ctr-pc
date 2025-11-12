using CutTheRope.desktop;
using CutTheRope.iframework;
using CutTheRope.iframework.core;
using CutTheRope.ios;
using System.Collections.Generic;

namespace CutTheRope.game
{
    /// <summary>
    /// GameScene.LoadObjects - Partial class coordinating object loading from XML
    /// Coordinates loading of all game objects and UI elements from XML level data
    /// </summary>
    internal sealed partial class GameScene
    {
        /// <summary>
        /// Loads all game objects from XML map data
        /// Delegates to specialized loading methods for each object type
        /// </summary>
        private void LoadGameObjects(XMLNode map, float scale, float offsetX, float offsetY, int mapOffsetX, int mapOffsetY)
        {
            List<XMLNode> list = map.Childs();
            foreach (XMLNode xmlnode2 in list)
            {
                foreach (XMLNode item3 in xmlnode2.Childs())
                {
                    LoadGravitySwitch(item3, scale, offsetX, offsetY, mapOffsetX, mapOffsetY);
                    LoadStar(item3, scale, offsetX, offsetY, mapOffsetX, mapOffsetY);
                    LoadTutorialText(item3, scale, offsetX, offsetY, mapOffsetX, mapOffsetY);
                    LoadTutorialImage(item3, scale, offsetX, offsetY, mapOffsetX, mapOffsetY);
                    LoadBubble(item3, scale, offsetX, offsetY, mapOffsetX, mapOffsetY);
                    LoadPump(item3, scale, offsetX, offsetY, mapOffsetX, mapOffsetY);
                    LoadSock(item3, scale, offsetX, offsetY, mapOffsetX, mapOffsetY);
                    LoadSpike(item3, scale, offsetX, offsetY, mapOffsetX, mapOffsetY);
                    LoadRotatedCircle(item3, scale, offsetX, offsetY, mapOffsetX, mapOffsetY);
                    LoadBouncer(item3, scale, offsetX, offsetY, mapOffsetX, mapOffsetY);
                    LoadGrab(item3, scale, offsetX, offsetY, mapOffsetX, mapOffsetY);
                    LoadTarget(item3, scale, offsetX, offsetY, mapOffsetX, mapOffsetY);
                }
            }
        }
    }
}
