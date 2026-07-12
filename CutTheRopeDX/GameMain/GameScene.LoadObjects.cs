using System.Collections.Generic;
using System.Xml.Linq;

namespace CutTheRopeDX.GameMain
{
    internal sealed partial class GameScene
    {
        /// <summary>
        /// Loads all game objects from XML map data using a dispatch switch
        /// Iterates through XML nodes and calls appropriate Load* method for each object type
        /// </summary>
        /// <param name="map">The XML map node containing object definitions.</param>
        /// <param name="scale">The level scale factor applied to object coordinates.</param>
        /// <param name="offsetX">The base X offset applied to loaded objects.</param>
        /// <param name="offsetY">The base Y offset applied to loaded objects.</param>
        /// <param name="mapOffsetX">The additional map X offset applied during loading.</param>
        /// <param name="mapOffsetY">The additional map Y offset applied during loading.</param>
        private void LoadObjectsFromMap(XElement map, float scale, float offsetX, float offsetY, int mapOffsetX, int mapOffsetY)
        {
            List<XElement> list = [.. map.Elements()];
            // Establish captured state before grabs are loaded so XML object order cannot attach a
            // fixed rope to candy that starts inside a lantern.
            candies[0].inLantern = NormalRopeLoad.CandyStartsInLantern(map);

            // Preload light bulbs so bindBulb grabs can resolve regardless of XML order.
            foreach (XElement xmlnode2 in list)
            {
                foreach (XElement item3 in xmlnode2.Elements())
                {
                    switch (item3.Name.LocalName)
                    {
                        case "lightBulb":
                        case "lightbulb":
                            LoadLightBulb(item3, scale, offsetX + mapOffsetX, offsetY + mapOffsetY, 0, 0);
                            break;
                        default:
                            break;
                    }
                }
            }
            foreach (XElement xmlnode2 in list)
            {
                foreach (XElement item3 in xmlnode2.Elements())
                {
                    switch (item3.Name.LocalName)
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
                        case "load":
                            LoadSnail(item3, scale, offsetX + mapOffsetX, offsetY + mapOffsetY, 0, 0);
                            break;
                        case "grab":
                            LoadGrab(item3, scale, offsetX + mapOffsetX, offsetY + mapOffsetY, 0, 0);
                            break;
                        case "target":
                            LoadTarget(item3, scale, offsetX + mapOffsetX, offsetY + mapOffsetY, 0, 0);
                            break;
                        case "steamTube":
                            LoadSteamTube(item3, scale, offsetX + mapOffsetX, offsetY + mapOffsetY, 0, 0);
                            break;
                        case "pipe":
                            LoadBambooTube(item3, scale, offsetX + mapOffsetX, offsetY + mapOffsetY, 0, 0);
                            break;
                        case "ants":
                            LoadAnts(item3, scale, offsetX + mapOffsetX, offsetY + mapOffsetY, 0, 0);
                            break;
                        case "ghost":
                            LoadGhost(item3, scale, offsetX + mapOffsetX, offsetY + mapOffsetY, 0, 0);
                            break;
                        case "rocket":
                            LoadRocket(item3, scale, offsetX + mapOffsetX, offsetY + mapOffsetY, 0, 0);
                            break;
                        case "hand":
                            LoadHand(item3, scale, offsetX + mapOffsetX, offsetY + mapOffsetY, 0, 0);
                            break;
                        case "conveyorBelt":
                        case "transporter":
                            LoadConveyorBelt(item3, scale, offsetX + mapOffsetX, offsetY + mapOffsetY, 0, 0);
                            break;
                        case "lantern":
                            LoadLantern(item3, scale, offsetX + mapOffsetX, offsetY + mapOffsetY, 0, 0);
                            break;
                        case "lightBulb":
                        case "lightbulb":
                            // Preloaded above.
                            break;
                        case "gap":
                        case "mouse":
                            LoadMouse(item3, scale, offsetX + mapOffsetX, offsetY + mapOffsetY, 0, 0);
                            break;
                        default:
                            break;
                    }
                }
            }
        }
    }
}
