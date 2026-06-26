using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;

using CutTheRopeDX.Helpers;

using static CutTheRopeDX.Helpers.ParsingHelpers;

namespace CutTheRopeDX.GameMain
{
    /// <summary>
    /// Discovers gameplay resource dependencies from parsed level XML.
    /// </summary>
    internal static class LevelResourceScanner
    {
        /// <summary>
        /// Computes the gameplay resources required to instantiate a single parsed map.
        /// </summary>
        /// <param name="map">The parsed level XML.</param>
        /// <returns>A de-duplicated array of resource identifiers needed for the map.</returns>
        public static string[] GetRequiredResources(XElement map)
        {
            if (map == null)
            {
                return [];
            }

            HashSet<string> resources = [];

            AddAlwaysLoadedLevelResources(resources);

            bool nightLevel = false;
            bool waterLevel = false;
            bool sawTarget = false;
            bool hasClassicTarget = false;

            foreach (XElement node in map.Descendants())
            {
                switch (node.Name.LocalName)
                {
                    case "gameDesign":
                        nightLevel = ParseBool(node.Attribute("nightLevel")?.Value);
                        waterLevel = ParseFloatOrZero(node.Attribute("water")?.Value) > 0f;
                        break;
                    case "star":
                        if (nightLevel)
                        {
                            _ = resources.Add(Resources.Img.ObjStarNight);
                        }
                        break;
                    case "grab":
                        AddGrabResources(resources, node);
                        break;
                    case "bubble":
                        break;
                    case "spike1":
                    case "spike2":
                    case "spike3":
                    case "spike4":
                        _ = resources.Add(Resources.Img.ObjSpikes);
                        break;
                    case "electro":
                        _ = resources.Add(Resources.Img.ObjElectrodes);
                        break;
                    case "bouncer1":
                    case "bouncer2":
                        _ = resources.Add(Resources.Img.ObjBouncer);
                        break;
                    case "pump":
                        _ = resources.Add(Resources.Img.ObjPump);
                        break;
                    case "sock":
                        _ = resources.Add(SpecialEvents.IsXmas ? Resources.Img.ObjSock : Resources.Img.ObjHat);
                        break;
                    case "ghost":
                        _ = resources.Add(Resources.Img.ObjGhost);
                        break;
                    case "rocket":
                        _ = resources.Add(Resources.Img.ObjRocket);
                        break;
                    case "load":
                        _ = resources.Add(Resources.Img.ObjSnail);
                        break;
                    case "pipe":
                        _ = resources.Add(Resources.Img.ObjBambooTube);
                        break;
                    case "ants":
                        _ = resources.Add(Resources.Img.ObjAnt);
                        break;
                    case "lantern":
                        _ = resources.Add(Resources.Img.ObjLantern);
                        break;
                    case "gap":
                    case "mouse":
                        _ = resources.Add(Resources.Img.ObjMouse);
                        break;
                    case "conveyorBelt":
                    case "transporter":
                        _ = resources.Add(Resources.Img.ObjConveyor);
                        break;
                    case "tutorialText":
                        _ = resources.Add(Resources.Fnt.SmallFont);
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
                        _ = resources.Add(Resources.Img.TutorialSigns);
                        break;
                    case "lightBulb":
                    case "lightbulb":
                        _ = resources.Add(Resources.Img.ObjLighter);
                        _ = resources.Add(Resources.Img.ObjGhost);
                        break;
                    case "hand":
                        _ = resources.Add(Resources.Img.ObjRoboHand);
                        break;
                    case "target":
                        sawTarget = true;
                        if (AddTargetResources(resources, node))
                        {
                            hasClassicTarget = true;
                        }

                        break;
                    case "steamTube":
                        _ = resources.Add(Resources.Img.ObjPipe);
                        break;
                    case "rotatedCircle":
                        _ = resources.Add(Resources.Img.ObjVinil);
                        break;
                    default:
                        break;
                }
            }

            if (nightLevel)
            {
                _ = resources.Add(Resources.Img.ObjStarNight);

                // The classic sleeping spritesheet is only needed when a classic Om Nom is
                // present. Use per-target resolution; if a night level has no target node,
                // fall back to the player's selected skin to preserve prior behavior.
                bool needsClassicSleeping = sawTarget
                    ? hasClassicTarget
                    : OmNomSkinRegistry.IsClassicSkin(OmNomSkinRegistry.GetSelectedSkinIndex());
                if (needsClassicSleeping)
                {
                    _ = resources.Add(Resources.Img.CharAnimationsSleeping);
                }

                _ = resources.Add(Resources.Img.FxSleep);
            }
            if (waterLevel)
            {
                _ = resources.Add(Resources.Img.WaterTile);
            }
            if (SpecialEvents.IsXmas)
            {
                _ = resources.Add(Resources.Img.CharGreetingXmas);
                _ = resources.Add(Resources.Img.CharIdleXmas);
                _ = resources.Add(Resources.Img.XmasLights);
            }

            return [.. resources.Where(static resourceName => !string.IsNullOrWhiteSpace(resourceName))];
        }

        /// <summary>
        /// Scans all levels in a pack and returns every required image resource.
        /// </summary>
        /// <param name="pack">Pack index to scan.</param>
        /// <returns>Unique image resource names required by the pack's levels.</returns>
        public static HashSet<string> GetBoxResources(int pack)
        {
            HashSet<string> resources = [];
            int levelCount = PackConfig.GetLevelCount(pack);
            for (int level = 0; level < levelCount; level++)
            {
                string mapName = LevelsList.LEVEL_NAMES[pack, level];
                if (string.IsNullOrWhiteSpace(mapName))
                {
                    continue;
                }

                XElement map = ContentPaths.LoadXml(Path.Combine(ContentPaths.MapsDirectory, mapName));
                foreach (string resourceName in GetRequiredResources(map))
                {
                    _ = resources.Add(resourceName);
                }
            }

            return resources;
        }

        /// <summary>
        /// Adds resources that are expected in every gameplay map regardless of XML contents.
        /// </summary>
        /// <param name="resources">The destination set being accumulated.</param>
        private static void AddAlwaysLoadedLevelResources(HashSet<string> resources)
        {
            _ = resources.Add(Resources.Img.HudUi);
            _ = resources.Add(Resources.Img.ObjStarIdle);
            _ = resources.Add(Resources.Img.ObjStarDisappear);
            _ = resources.Add(Resources.Img.ObjBubble);
        }

        /// <summary>
        /// Adds hook-related resources based on a grab node's attributes.
        /// </summary>
        /// <param name="resources">The destination set being accumulated.</param>
        /// <param name="node">The grab XML node being inspected.</param>
        private static void AddGrabResources(HashSet<string> resources, XElement node)
        {
            _ = resources.Add(Resources.Img.ObjHook);

            bool gun = ParseBool(node.Attribute("gun")?.Value);
            bool kickable = ParseBool(node.Attribute("kickable")?.Value);
            bool bee = ParseBool(node.Attribute("bee")?.Value) || node.Attribute("path") != null;

            if (bee)
            {
                _ = resources.Add(Resources.Img.ObjBee);
            }
            if (gun)
            {
                _ = resources.Add(Resources.Img.ObjGun);
            }
            if (kickable)
            {
                _ = resources.Add(Resources.Img.ObjSticker);
            }
        }


        /// <summary>
        /// Adds Om Nom animation resources for a single target, using its resolved skin.
        /// </summary>
        /// <param name="resources">The destination set being accumulated.</param>
        /// <param name="node">The target XML node, whose <c>targetType</c> selects the skin.</param>
        /// <returns><see langword="true"/> when the target resolved to the classic skin; otherwise, <see langword="false"/>.</returns>
        private static bool AddTargetResources(HashSet<string> resources, XElement node)
        {
            int targetType = ParseIntOrZero(node.Attribute("targetType")?.Value ?? string.Empty);
            int skinIndex = OmNomSkinRegistry.ResolveTargetSkinIndex(
                targetType,
                OmNomSkinRegistry.GetSelectedSkinIndex(),
                OmNomSkinRegistry.TotalSkinCount);

            bool isClassic = OmNomSkinRegistry.IsClassicSkin(skinIndex);
            if (isClassic)
            {
                _ = resources.Add(Resources.Img.CharAnimations);
                _ = resources.Add(Resources.Img.CharAnimations2);
                _ = resources.Add(Resources.Img.CharAnimations3);
            }
            else
            {
                OmNomSkinDefinition skin = OmNomSkinRegistry.GetXmlSkinDefinition(skinIndex);
                _ = string.Equals(skin.Id, "OM_NOM_PREHISTORIC", StringComparison.Ordinal)
                    ? resources.Add(Resources.Img.CharAnimationsPrehistoric)
                    : resources.Add(Resources.Img.CharAnimationsSmooth);
            }

            _ = resources.Add(Resources.Img.FxBubbles);
            _ = resources.Add(Resources.Img.CharSupports);
            return isClassic;
        }

        /// <summary>
        /// Parses a boolean XML attribute value, defaulting to <see langword="false"/> when absent or invalid.
        /// </summary>
        /// <param name="value">The attribute text to parse.</param>
        /// <returns>The parsed boolean value.</returns>
        private static bool ParseBool(string value)
        {
            return bool.TryParse(value, out bool parsed) && parsed;
        }
    }
}
