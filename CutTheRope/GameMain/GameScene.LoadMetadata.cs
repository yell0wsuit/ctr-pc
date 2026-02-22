using System.Xml.Linq;

using CutTheRope.Framework.Core;
using CutTheRope.Framework.Helpers;
using CutTheRope.Helpers;
using System.Globalization;

namespace CutTheRope.GameMain
{
    /// <summary>
    /// GameScene.LoadMetadata - Partial class handling level metadata loading
    /// Loads map dimensions, Game design settings, and candy positions from XML
    /// </summary>
    internal sealed partial class GameScene
    {
        /// <summary>
        /// Loads all level metadata from XML in a single pass
        /// Extracts map dimensions, Game design settings, and candy positions
        /// </summary>
        private void LoadAllLevelMetadata(XElement mapNode, float scale, float offsetY, out float offsetX, out int mapOffsetX, out int mapOffsetY)
        {
            offsetX = 0f;
            mapOffsetX = 0;
            mapOffsetY = 0;

            CTRRootController rc = (CTRRootController)Application.SharedRootController();

            // Single pass through XML metadata nodes
            foreach (XElement xmlnode in mapNode.Elements())
            {
                foreach (XElement item2 in xmlnode.Elements())
                {
                    switch (item2.Name.LocalName)
                    {
                        case "map":
                            mapWidth = string.IsNullOrEmpty(item2.AttributeAsNSString("width")) ? 0f : float.Parse(item2.AttributeAsNSString("width"), CultureInfo.InvariantCulture);
                            mapHeight = string.IsNullOrEmpty(item2.AttributeAsNSString("height")) ? 0f : float.Parse(item2.AttributeAsNSString("height"), CultureInfo.InvariantCulture);
                            offsetX = (2560f - (mapWidth * scale)) / 2f;
                            mapWidth *= scale;
                            mapHeight *= scale;

                            if (PackConfig.GetEarthBg(rc.GetPack()))
                            {
                                earthAnims = [];
                                if (mapWidth > SCREEN_WIDTH)
                                {
                                    CreateEarthImageWithOffsetXY(back.width, 0f);
                                }
                                if (mapHeight > SCREEN_HEIGHT)
                                {
                                    CreateEarthImageWithOffsetXY(0f, back.height);
                                }
                                CreateEarthImageWithOffsetXY(0f, 0f);
                            }
                            break;
                        case "gameDesign":
                            mapOffsetX = string.IsNullOrEmpty(item2.AttributeAsNSString("mapOffsetX")) ? 0 : int.Parse(item2.AttributeAsNSString("mapOffsetX"));
                            mapOffsetY = string.IsNullOrEmpty(item2.AttributeAsNSString("mapOffsetY")) ? 0 : int.Parse(item2.AttributeAsNSString("mapOffsetY"));
                            special = string.IsNullOrEmpty(item2.AttributeAsNSString("special")) ? 0 : int.Parse(item2.AttributeAsNSString("special"));
                            ropePhysicsSpeed = string.IsNullOrEmpty(item2.AttributeAsNSString("ropePhysicsSpeed")) ? 0f : float.Parse(item2.AttributeAsNSString("ropePhysicsSpeed"), CultureInfo.InvariantCulture);
                            nightLevel = item2.AttributeAsNSString("nightLevel") == "true";
                            twoParts = item2.AttributeAsNSString("twoParts") != "true" ? 2 : 0;
                            waterLevel = string.IsNullOrEmpty(item2.AttributeAsNSString("water")) ? 0f : float.Parse(item2.AttributeAsNSString("water"), CultureInfo.InvariantCulture);
                            if (waterLevel != 0f)
                            {
                                waterLevel *= scale;
                            }
                            waterSpeed = (string.IsNullOrEmpty(item2.AttributeAsNSString("waterSpeed")) ? 0f : float.Parse(item2.AttributeAsNSString("waterSpeed"), CultureInfo.InvariantCulture)) * scale;
                            if (waterLevel > 0f)
                            {
                                float waterWorldX = offsetX + mapOffsetX;
                                float waterWorldWidth = mapWidth;
                                if (waterWorldWidth < SCREEN_WIDTH)
                                {
                                    waterWorldX = 0f;
                                    waterWorldWidth = SCREEN_WIDTH;
                                }

                                waterLayer = WaterElement.CreateWithWidthHeight(waterWorldWidth, waterLevel);
                                if (waterLayer != null)
                                {
                                    waterLayer.x = waterWorldX;
                                    waterLayer.y = offsetY + mapOffsetY + mapHeight - waterLevel;
                                }
                                else
                                {
                                    // Disable water behavior when the texture atlas is not available.
                                    waterLevel = 0f;
                                    waterSpeed = 0f;
                                }
                            }
                            ropePhysicsSpeed *= 1.4f;
                            break;
                        case "candyL":
                            starL.pos.X = ((string.IsNullOrEmpty(item2.AttributeAsNSString("x")) ? 0 : int.Parse(item2.AttributeAsNSString("x"))) * scale) + offsetX + mapOffsetX;
                            starL.pos.Y = ((string.IsNullOrEmpty(item2.AttributeAsNSString("y")) ? 0 : int.Parse(item2.AttributeAsNSString("y"))) * scale) + offsetY + mapOffsetY;
                            {
                                int selectedCandySkin = Preferences.GetIntForKey(CTRPreferences.PREFS_SELECTED_CANDY);
                                string candyResource = CandySkinHelper.GetCandyResource(selectedCandySkin);
                                candyL = GameObject.GameObject_createWithResIDQuad(candyResource, 8);
                            }
                            candyL.scaleX = candyL.scaleY = 0.71f;
                            candyL.passTransformationsToChilds = false;
                            candyL.DoRestoreCutTransparency();
                            candyL.anchor = 18;
                            candyL.x = starL.pos.X;
                            candyL.y = starL.pos.Y;
                            candyL.bb = MakeRectangle(155f, 176f, 88f, 76f);
                            break;
                        case "candyR":
                            starR.pos.X = ((string.IsNullOrEmpty(item2.AttributeAsNSString("x")) ? 0 : int.Parse(item2.AttributeAsNSString("x"))) * scale) + offsetX + mapOffsetX;
                            starR.pos.Y = ((string.IsNullOrEmpty(item2.AttributeAsNSString("y")) ? 0 : int.Parse(item2.AttributeAsNSString("y"))) * scale) + offsetY + mapOffsetY;
                            {
                                int selectedCandySkin = Preferences.GetIntForKey(CTRPreferences.PREFS_SELECTED_CANDY);
                                string candyResource = CandySkinHelper.GetCandyResource(selectedCandySkin);
                                candyR = GameObject.GameObject_createWithResIDQuad(candyResource, 9);
                            }
                            candyR.scaleX = candyR.scaleY = 0.71f;
                            candyR.passTransformationsToChilds = false;
                            candyR.DoRestoreCutTransparency();
                            candyR.anchor = 18;
                            candyR.x = starR.pos.X;
                            candyR.y = starR.pos.Y;
                            candyR.bb = MakeRectangle(155f, 176f, 88f, 76f);
                            break;
                        case "candy":
                            star.pos.X = ((string.IsNullOrEmpty(item2.AttributeAsNSString("x")) ? 0 : int.Parse(item2.AttributeAsNSString("x"))) * scale) + offsetX + mapOffsetX;
                            star.pos.Y = ((string.IsNullOrEmpty(item2.AttributeAsNSString("y")) ? 0 : int.Parse(item2.AttributeAsNSString("y"))) * scale) + offsetY + mapOffsetY;
                            break;
                        default:
                            break;
                    }
                }
            }
        }
    }
}
