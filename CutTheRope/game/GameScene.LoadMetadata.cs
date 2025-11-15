using System.Xml.Linq;

using CutTheRope.Helpers;
using CutTheRope.iframework.core;
using CutTheRope.iframework.helpers;
using CutTheRope.iframework.visual;

namespace CutTheRope.game
{
    /// <summary>
    /// GameScene.LoadMetadata - Partial class handling level metadata loading
    /// Loads map dimensions, game design settings, and candy positions from XML
    /// </summary>
    internal sealed partial class GameScene
    {
        /// <summary>
        /// Loads all level metadata from XML in a single pass
        /// Extracts map dimensions, game design settings, and candy positions
        /// </summary>
        private void LoadAllLevelMetadata(XElement mapNode, float scale, float offsetY, out float offsetX, out int mapOffsetX, out int mapOffsetY)
        {
            offsetX = 0f;
            mapOffsetX = 0;
            mapOffsetY = 0;

            CTRRootController cTRRootController = (CTRRootController)Application.SharedRootController();

            // Single pass through XML metadata nodes
            foreach (XElement xmlnode in mapNode.Elements())
            {
                foreach (XElement item2 in xmlnode.Elements())
                {
                    if (item2.Name.LocalName == "map")
                    {
                        mapWidth = item2.AttributeAsNSString("width").FloatValue();
                        mapHeight = item2.AttributeAsNSString("height").FloatValue();
                        offsetX = (2560f - (mapWidth * scale)) / 2f;
                        mapWidth *= scale;
                        mapHeight *= scale;

                        if (cTRRootController.GetPack() == 7)
                        {
                            earthAnims = new DynamicArray<Image>();
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
                    }
                    else if (item2.Name.LocalName == "gameDesign")
                    {
                        mapOffsetX = item2.AttributeAsNSString("mapOffsetX").IntValue();
                        mapOffsetY = item2.AttributeAsNSString("mapOffsetY").IntValue();
                        special = item2.AttributeAsNSString("special").IntValue();
                        ropePhysicsSpeed = item2.AttributeAsNSString("ropePhysicsSpeed").FloatValue();
                        nightLevel = item2.AttributeAsNSString("nightLevel").IsEqualToString("true");
                        twoParts = !item2.AttributeAsNSString("twoParts").IsEqualToString("true") ? 2 : 0;
                        ropePhysicsSpeed *= 1.4f;
                    }
                    else if (item2.Name.LocalName == "candyL")
                    {
                        starL.pos.x = (item2.AttributeAsNSString("x").IntValue() * scale) + offsetX + mapOffsetX;
                        starL.pos.y = (item2.AttributeAsNSString("y").IntValue() * scale) + offsetY + mapOffsetY;
                        candyL = GameObject.GameObject_createWithResIDQuad(63, 19);
                        candyL.scaleX = candyL.scaleY = 0.71f;
                        candyL.passTransformationsToChilds = false;
                        candyL.DoRestoreCutTransparency();
                        candyL.anchor = 18;
                        candyL.x = starL.pos.x;
                        candyL.y = starL.pos.y;
                        candyL.bb = MakeRectangle(155.0, 176.0, 88.0, 76.0);
                    }
                    else if (item2.Name.LocalName == "candyR")
                    {
                        starR.pos.x = (item2.AttributeAsNSString("x").IntValue() * scale) + offsetX + mapOffsetX;
                        starR.pos.y = (item2.AttributeAsNSString("y").IntValue() * scale) + offsetY + mapOffsetY;
                        candyR = GameObject.GameObject_createWithResIDQuad(63, 20);
                        candyR.scaleX = candyR.scaleY = 0.71f;
                        candyR.passTransformationsToChilds = false;
                        candyR.DoRestoreCutTransparency();
                        candyR.anchor = 18;
                        candyR.x = starR.pos.x;
                        candyR.y = starR.pos.y;
                        candyR.bb = MakeRectangle(155.0, 176.0, 88.0, 76.0);
                    }
                    else if (item2.Name.LocalName == "candy")
                    {
                        star.pos.x = (item2.AttributeAsNSString("x").IntValue() * scale) + offsetX + mapOffsetX;
                        star.pos.y = (item2.AttributeAsNSString("y").IntValue() * scale) + offsetY + mapOffsetY;
                    }
                }
            }
        }
    }
}
