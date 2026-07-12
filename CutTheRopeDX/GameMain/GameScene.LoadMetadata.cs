using System.Xml.Linq;

using CutTheRopeDX.Framework;
using CutTheRopeDX.Framework.Core;
using CutTheRopeDX.Framework.Helpers;
using CutTheRopeDX.Framework.Physics;

using static CutTheRopeDX.Helpers.ParsingHelpers;

namespace CutTheRopeDX.GameMain
{
    internal sealed partial class GameScene
    {
        /// <summary>
        /// Loads all level metadata from XML in a single pass
        /// Extracts map dimensions, Game design settings, and candy positions
        /// </summary>
        /// <param name="mapNode">Root XML node for the current map.</param>
        /// <param name="scale">Level scale factor.</param>
        /// <param name="offsetY">Vertical offset applied to map coordinates.</param>
        /// <param name="offsetX">Computed horizontal offset for map coordinates.</param>
        /// <param name="mapOffsetX">Computed integer map X offset.</param>
        /// <param name="mapOffsetY">Computed integer map Y offset.</param>
        private void LoadAllLevelMetadata(XElement mapNode, float scale, float offsetY, out float offsetX, out int mapOffsetX, out int mapOffsetY)
        {
            offsetX = 0f;
            mapOffsetX = 0;
            mapOffsetY = 0;
            ActivePhysicsConstants.UseMobilePhysicsModel = false;
            Bungee.BUNGEE_REST_LEN = ActivePhysicsConstants.BungeeRestLength;

            CTRRootController rc = (CTRRootController)Application.SharedRootController();

            // Single pass through XML metadata nodes
            foreach (XElement xmlnode in mapNode.Elements())
            {
                foreach (XElement item2 in xmlnode.Elements())
                {
                    switch (item2.Name.LocalName)
                    {
                        case "map":
                            mapWidth = ParseFloatOrZero(item2.Attribute("width")?.Value);
                            mapHeight = ParseFloatOrZero(item2.Attribute("height")?.Value);
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
                            mapOffsetX = ParseCoordinateIntOrZero(item2.Attribute("mapOffsetX")?.Value);
                            mapOffsetY = ParseCoordinateIntOrZero(item2.Attribute("mapOffsetY")?.Value);
                            special = ParseIntOrZero(item2.Attribute("special")?.Value);
                            ropePhysicsSpeed = ParseFloatOrZero(item2.Attribute("ropePhysicsSpeed")?.Value);
                            _ = bool.TryParse(item2.Attribute("useMobilePhysics")?.Value, out bool useMobilePhysics);
                            ActivePhysicsConstants.UseMobilePhysicsModel = useMobilePhysics;
                            Bungee.BUNGEE_REST_LEN = ActivePhysicsConstants.BungeeRestLength;
                            _ = bool.TryParse(item2.Attribute("nightLevel")?.Value, out nightLevel);
                            _ = bool.TryParse(item2.Attribute("twoParts")?.Value, out bool twoPartsBool);
                            twoParts = twoPartsBool ? 0 : 2;
                            waterLevel = ParseFloatOrZero(item2.Attribute("water")?.Value);
                            if (waterLevel != 0f)
                            {
                                waterLevel *= scale;
                            }
                            waterSpeed = ParseFloatOrZero(item2.Attribute("waterSpeed")?.Value) * scale;
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
                            ropePhysicsSpeed *= ActivePhysicsConstants.RopePhysicsSpeedMultiplier;
                            globalGravityX = (item2.Attribute("globalGravityX") != null) ? ParseFloatOrZero(item2.Attribute("globalGravityX")?.Value) : 0f;
                            globalGravityY = (item2.Attribute("globalGravityY") != null) ? ParseFloatOrZero(item2.Attribute("globalGravityY")?.Value) : ActivePhysicsConstants.GravityEarthY;
                            _ = bool.TryParse(item2.Attribute("candiesConnected")?.Value, out candiesConnected);
                            candiesConnectedLength = ParseFloatOrZero(item2.Attribute("candiesConnectedLength")?.Value) * scale;
                            break;
                        case "candyL":
                            starL.pos.X = (ParseCoordinateIntOrZero(item2.Attribute("x")?.Value) * scale) + offsetX + mapOffsetX;
                            starL.pos.Y = (ParseCoordinateIntOrZero(item2.Attribute("y")?.Value) * scale) + offsetY + mapOffsetY;
                            {
                                int selectedCandySkin = Preferences.GetIntForKey("PREFS_SELECTED_CANDY");
                                string candyResource = CandySkinHelper.GetCandyResource(selectedCandySkin);
                                candyL = GameObject.GameObject_createWithResIDQuad(candyResource, 8);
                            }
                            candyL.scaleX = candyL.scaleY = 0.71f;
                            candyL.passTransformationsToChilds = false;
                            candyL.DoRestoreCutTransparency();
                            candyL.anchor = 18;
                            candyL.x = starL.pos.X;
                            candyL.y = starL.pos.Y;
                            candyL.bb = GetSplitCandyBoundingBox();
                            break;
                        case "candyR":
                            starR.pos.X = (ParseCoordinateIntOrZero(item2.Attribute("x")?.Value) * scale) + offsetX + mapOffsetX;
                            starR.pos.Y = (ParseCoordinateIntOrZero(item2.Attribute("y")?.Value) * scale) + offsetY + mapOffsetY;
                            {
                                int selectedCandySkin = Preferences.GetIntForKey("PREFS_SELECTED_CANDY");
                                string candyResource = CandySkinHelper.GetCandyResource(selectedCandySkin);
                                candyR = GameObject.GameObject_createWithResIDQuad(candyResource, 9);
                            }
                            candyR.scaleX = candyR.scaleY = 0.71f;
                            candyR.passTransformationsToChilds = false;
                            candyR.DoRestoreCutTransparency();
                            candyR.anchor = 18;
                            candyR.x = starR.pos.X;
                            candyR.y = starR.pos.Y;
                            candyR.bb = GetSplitCandyBoundingBox();
                            break;
                        case "candy":
                            {
                                float cx = (ParseCoordinateIntOrZero(item2.Attribute("x")?.Value) * scale) + offsetX + mapOffsetX;
                                float cy = (ParseCoordinateIntOrZero(item2.Attribute("y")?.Value) * scale) + offsetY + mapOffsetY;
                                // Key comes straight from XML; null for legacy single-candy packs (never matched).
                                string number = item2.Attribute("candyNumber")?.Value;

                                // The first <candy> parsed claims the pre-built primary candy (candies[0])
                                // and takes its key from XML; later <candy> elements are built fresh.
                                if (!primaryCandyClaimed)
                                {
                                    primaryCandyClaimed = true;
                                    candies[0].candyNumber = number;
                                    star.pos.X = cx;
                                    star.pos.Y = cy;
                                    star.prevPos = star.pos;
                                    candy.x = cx;
                                    candy.y = cy;
                                }
                                else
                                {
                                    _ = CreateCandyContext(number, cx, cy);
                                }
                            }
                            break;
                        default:
                            break;
                    }
                }
            }

            // Re-apply per-level collision boxes after metadata is fully parsed, so XML order cannot leak stale mode.
            candy.bb = GetCandyBoundingBox();
            _ = (candyL?.bb = GetSplitCandyBoundingBox());
            _ = (candyR?.bb = GetSplitCandyBoundingBox());

            // candiesConnected: join the two candies with a mutual elastic. Both candy points are
            // passed directly as head/tail; Bungee preserves their weights and skips integrating
            // non-owned endpoints.
            if (candiesConnected && candies.Count >= 2)
            {
                ConstraintedPoint connectorHead = candies[0].point;
                ConstraintedPoint connectorTail = candies[1].point;
                candyConnector = new Bungee().InitWithHeadAtXYTailAtTXTYandLength(
                    connectorHead, connectorHead.pos.X, connectorHead.pos.Y,
                    connectorTail, connectorTail.pos.X, connectorTail.pos.Y,
                    candiesConnectedLength);
            }
        }
    }
}
