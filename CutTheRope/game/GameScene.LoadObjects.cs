using System.Collections.Generic;

using CutTheRope.ios;

namespace CutTheRope.game
{
    /// <summary>
    /// GameScene.LoadObjects - Partial class coordinating object loading from XML
    /// Coordinates loading of all game objects and UI elements from XML level data
    /// </summary>
    internal sealed partial class GameScene
    {
        /// <summary>
        /// Loads all game objects from XML map data using a dispatch switch
        /// Iterates through XML nodes and calls appropriate Load* method for each object type
        /// </summary>
        private void LoadObjectsFromMap(XMLNode map, float scale, float offsetX, float offsetY, int mapOffsetX, int mapOffsetY)
        {
            List<XMLNode> list = map.Childs();
            foreach (XMLNode xmlnode2 in list)
            {
                foreach (XMLNode item3 in xmlnode2.Childs())
                {
                    switch (item3.Name)
                    {
                        case "gravitySwitch":
                            LoadGravityButton(item3, scale, offsetX + mapOffsetX, offsetY + mapOffsetY, 0, 0);
                            break;
                        case "star":
                            LoadStar(item3, scale, offsetX + mapOffsetX, offsetY + mapOffsetY, 0, 0);
                            break;
                        case "tutorialText":
                            LoadTutorialText(item3, scale, offsetX + mapOffsetX, offsetY + mapOffsetY, 0, 0);
                            break;
                        case "tutorial01":
                        case "tutorial02":
                        case "tutorial03":
                        case "tutorial04":
                        case "tutorial05":
                        case "tutorial06":
                        case "tutorial07":
                        case "tutorial08":
                        case "tutorial09":
                        case "tutorial10":
                        case "tutorial11":
                            LoadTutorialImage(item3, scale, offsetX + mapOffsetX, offsetY + mapOffsetY, 0, 0);
                            break;
                        case "bubble":
                            LoadBubble(item3, scale, offsetX + mapOffsetX, offsetY + mapOffsetY, 0, 0);
                            break;
                        case "pump":
                            LoadPump(item3, scale, offsetX + mapOffsetX, offsetY + mapOffsetY, 0, 0);
                            break;
                        case "sock":
                            LoadSock(item3, scale, offsetX + mapOffsetX, offsetY + mapOffsetY, 0, 0);
                            break;
                        case "spike1":
                        case "spike2":
                        case "spike3":
                        case "spike4":
                        case "electro":
                            LoadSpike(item3, scale, offsetX + mapOffsetX, offsetY + mapOffsetY, 0, 0);
                            break;
                        case "rotatedCircle":
                            LoadRotatedCircle(item3, scale, offsetX + mapOffsetX, offsetY + mapOffsetY, 0, 0);
                            break;
                        case "bouncer1":
                        case "bouncer2":
                            LoadBouncer(item3, scale, offsetX + mapOffsetX, offsetY + mapOffsetY, 0, 0);
                            break;
                        case "grab":
                            LoadGrab(item3, scale, offsetX + mapOffsetX, offsetY + mapOffsetY, 0, 0);
                            break;
                        case "target":
                            LoadTarget(item3, scale, offsetX + mapOffsetX, offsetY + mapOffsetY, 0, 0);
                            break;
                        default:
                            break;
                    }
                }
            }
        }
    }
}
