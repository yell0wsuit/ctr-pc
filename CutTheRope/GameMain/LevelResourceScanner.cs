using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;

using CutTheRope.Helpers;

using static CutTheRope.Helpers.ParsingHelpers;

namespace CutTheRope.GameMain
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
        /// <param name="pack">The active pack index for pack-specific resources.</param>
        /// <returns>A de-duplicated array of resource identifiers needed for the map.</returns>
        public static string[] GetRequiredResources(XElement map, int pack)
        {
            if (map == null)
            {
                return [];
            }

            HashSet<string> resources = [];

            AddAlwaysLoadedLevelResources(resources);

            bool nightLevel = false;
            bool waterLevel = false;

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
                        AddSpikeResources(resources, node, Resources.Img.ObjSpikes01, Resources.Img.ObjRotatableSpikes01);
                        break;
                    case "spike2":
                        AddSpikeResources(resources, node, Resources.Img.ObjSpikes02, Resources.Img.ObjRotatableSpikes02);
                        break;
                    case "spike3":
                        AddSpikeResources(resources, node, Resources.Img.ObjSpikes03, Resources.Img.ObjRotatableSpikes03);
                        break;
                    case "spike4":
                        AddSpikeResources(resources, node, Resources.Img.ObjSpikes04, Resources.Img.ObjRotatableSpikes04);
                        break;
                    case "electro":
                        AddSpikeResources(resources, node, Resources.Img.ObjElectrodes, Resources.Img.ObjRotatableSpikesButton);
                        break;
                    case "bouncer1":
                    case "bouncer2":
                        _ = resources.Add(Resources.Img.ObjBouncer01);
                        _ = resources.Add(Resources.Img.ObjBouncer02);
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
                        _ = resources.Add(Resources.Img.AntHole);
                        break;
                    case "lantern":
                        _ = resources.Add(Resources.Img.ObjLantern);
                        break;
                    case "gap":
                    case "mouse":
                        _ = resources.Add(Resources.Img.ObjGap);
                        break;
                    case "conveyorBelt":
                    case "transporter":
                        _ = resources.Add(Resources.Img.ObjTransporter);
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
                        AddTargetResources(resources, pack);
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
                _ = resources.Add(Resources.Img.CharAnimationsSleeping);
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
                foreach (string resourceName in GetRequiredResources(map, pack))
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
            _ = resources.Add(Resources.Img.HudStar);
            _ = resources.Add(Resources.Img.ObjStarIdle);
            _ = resources.Add(Resources.Img.ObjStarDisappear);
            _ = resources.Add(Resources.Img.ObjBubbleAttached);
            _ = resources.Add(Resources.Img.ObjBubbleFlight);
            _ = resources.Add(Resources.Img.ObjBubblePop);
        }

        /// <summary>
        /// Adds hook-related resources based on a grab node's attributes.
        /// </summary>
        /// <param name="resources">The destination set being accumulated.</param>
        /// <param name="node">The grab XML node being inspected.</param>
        private static void AddGrabResources(HashSet<string> resources, XElement node)
        {
            _ = resources.Add(Resources.Img.ObjHook01);
            _ = resources.Add(Resources.Img.ObjHook02);

            bool gun = ParseBool(node.Attribute("gun")?.Value);
            bool kickable = ParseBool(node.Attribute("kickable")?.Value);
            bool wheel = ParseBool(node.Attribute("wheel")?.Value);
            bool bee = ParseBool(node.Attribute("bee")?.Value) || node.Attribute("path") != null;
            float radius = ParseFloatOrZero(node.Attribute("radius")?.Value);
            float moveLength = ParseFloatOrZero(node.Attribute("moveLength")?.Value);

            if (radius != -1f && !gun && !kickable)
            {
                _ = resources.Add(Resources.Img.ObjHookAuto);
            }
            if (wheel)
            {
                _ = resources.Add(Resources.Img.ObjHookRegulated);
            }
            if (moveLength > 0f)
            {
                _ = resources.Add(Resources.Img.ObjHookMovable);
            }
            if (bee)
            {
                _ = resources.Add(Resources.Img.ObjBeeHd);
                _ = resources.Add(Resources.Img.ObjPollenHd);
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
        /// Adds spike resources, including the toggle button when the spike is controlled by a switch.
        /// </summary>
        /// <param name="resources">The destination set being accumulated.</param>
        /// <param name="node">The spike XML node being inspected.</param>
        /// <param name="baseResourceName">The base spike sprite resource.</param>
        /// <param name="rotatedResourceName">The rotated spike sprite resource.</param>
        private static void AddSpikeResources(HashSet<string> resources, XElement node, string baseResourceName, string rotatedResourceName)
        {
            _ = resources.Add(baseResourceName);
            if (node.Attribute("toggled") is not null)
            {
                _ = resources.Add(rotatedResourceName);
                _ = resources.Add(Resources.Img.ObjRotatableSpikesButton);
            }
        }

        /// <summary>
        /// Adds Om Nom animation resources, including the pack-specific support sprite.
        /// </summary>
        /// <param name="resources">The destination set being accumulated.</param>
        /// <param name="pack">The active pack index.</param>
        private static void AddTargetResources(HashSet<string> resources, int pack)
        {
            _ = resources.Add(Resources.Img.CharAnimations);
            _ = resources.Add(Resources.Img.CharAnimations2);
            _ = resources.Add(Resources.Img.CharAnimations3);
            _ = resources.Add(Resources.Img.CharSupports);
            _ = resources.Add(PackConfig.GetSupportResourceName(pack));
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
