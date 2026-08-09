using System;
using System.Collections.Generic;
using System.Xml.Linq;

using CutTheRopeDX.Framework;
using CutTheRopeDX.Framework.Core;
using CutTheRopeDX.Framework.Physics;

using static CutTheRopeDX.Helpers.ParsingHelpers;

namespace CutTheRopeDX.GameMain
{
    /// <summary>Selects the level layers consumed by <see cref="GameScene"/> metadata loading.</summary>
    internal static class LevelMetadataLayerSelection
    {
        /// <summary>
        /// Returns only the first case-insensitive <c>settings</c> layer while retaining every
        /// non-settings layer in document order.
        /// </summary>
        /// <param name="mapNode">Root XML node for the current map.</param>
        /// <returns>Layers to inspect for metadata.</returns>
        public static IEnumerable<XElement> SelectLayers(XElement mapNode)
        {
            bool settingsLayerSelected = false;
            foreach (XElement layer in mapNode.Elements())
            {
                bool isSettingsLayer = layer.Name.LocalName == "layer" && IsSettingsLayer(layer);
                if (isSettingsLayer)
                {
                    if (settingsLayerSelected)
                    {
                        continue;
                    }

                    settingsLayerSelected = true;
                }

                yield return layer;
            }
        }

        private static bool IsSettingsLayer(XElement layer)
        {
            return string.Equals(
                layer.Attribute("name")?.Value,
                "settings",
                StringComparison.OrdinalIgnoreCase);
        }
    }

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

            // Single pass through XML metadata nodes, ignoring duplicate settings layers.
            foreach (XElement xmlnode in LevelMetadataLayerSelection.SelectLayers(mapNode))
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
                            levelName = item2.Attribute("levelName")?.Value ?? null;

                            if (PackConfig.GetEarthBg(rc.GetPack()))
                            {
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
                            levelAuthorsSplitCandy = twoPartsBool;
                            if (levelAuthorsSplitCandy)
                            {
                                // A split level's primary candy is the split one, so candies[0] is
                                // reserved for the halves this pass is about to parse. Claiming it
                                // here - the settings layer is read before the object layers - is
                                // what lets a level author a split candy and ordinary candies at the
                                // same time: every <candy> element then builds its own whole context
                                // instead of the first one taking the split candy's place.
                                primaryCandyClaimed = true;
                            }
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
                            float globalGravityX = (item2.Attribute("globalGravityX") != null) ? ParseFloatOrZero(item2.Attribute("globalGravityX")?.Value) : 0f;
                            float globalGravityY = (item2.Attribute("globalGravityY") != null) ? ParseFloatOrZero(item2.Attribute("globalGravityY")?.Value) : ActivePhysicsConstants.GravityEarthY;
                            gravityState.ConfigureBase(new Vector(globalGravityX, globalGravityY));
                            _ = bool.TryParse(item2.Attribute("candiesConnected")?.Value, out candiesConnected);
                            candiesConnectedLength = ParseFloatOrZero(item2.Attribute("candiesConnectedLength")?.Value) * scale;
                            candiesConnectedBreakable = GetBoolAttribute(item2, "candiesConnectedBreakable", defaultValue: true);
                            break;
                        case "candyL":
                            pendingLeftHalf = CreateSplitHalfBody(
                                CandyBodyRole.LeftHalf,
                                (ParseCoordinateIntOrZero(item2.Attribute("x")?.Value) * scale) + offsetX + mapOffsetX,
                                (ParseCoordinateIntOrZero(item2.Attribute("y")?.Value) * scale) + offsetY + mapOffsetY);
                            break;
                        case "candyR":
                            pendingRightHalf = CreateSplitHalfBody(
                                CandyBodyRole.RightHalf,
                                (ParseCoordinateIntOrZero(item2.Attribute("x")?.Value) * scale) + offsetX + mapOffsetX,
                                (ParseCoordinateIntOrZero(item2.Attribute("y")?.Value) * scale) + offsetY + mapOffsetY);
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

            InstallSplitCandyState();

            // Re-apply per-level collision boxes after metadata is fully parsed, so XML order cannot leak stale mode.
            candy.bb = GetCandyBoundingBox();
            foreach (CandyBody body in ActiveCandyBodies())
            {
                if (body.Role != CandyBodyRole.Whole)
                {
                    body.Visual.bb = GetSplitCandyBoundingBox();
                }
            }

            // candiesConnected: join the two candies with a mutual elastic. Both candy points are
            // passed directly as head/tail; Bungee preserves their weights and skips integrating
            // non-owned endpoints.
            if (candiesConnected && candies.Count >= 2)
            {
                ConstraintedPoint connectorHead = candies[0].WholeBody.Point;
                ConstraintedPoint connectorTail = candies[1].WholeBody.Point;
                candyConnector = new Bungee().InitWithHeadAtXYTailAtTXTYandLength(
                    connectorHead, connectorHead.pos.X, connectorHead.pos.Y,
                    connectorTail, connectorTail.pos.X, connectorTail.pos.Y,
                    candiesConnectedLength);
                if (!candiesConnectedBreakable)
                {
                    // The connecting elastic is a chain: renders as a chain and is not finger-cuttable.
                    candyConnector.SetCutOnlyByAxe();
                }
                ropes.RegisterConnector(candyConnector);
            }
        }
    }
}
