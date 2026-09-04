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
    /// <remarks>
    /// The scan covers sound effects as well as textures. Both are loaded on demand by the
    /// resource manager, so anything a level's objects can reach but the scan misses is read
    /// off disk on the game thread at the moment the object first acts.
    /// </remarks>
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

            // Null entries stand for classic targets, so the post-loop passes can tell the
            // skins apart without depending on where gameDesign sits in document order.
            List<OmNomSkinDefinition> targetSkins = [];

            foreach (XElement node in map.Descendants())
            {
                switch (node.Name.LocalName)
                {
                    case "gameDesign":
                        nightLevel = ParseBool(node.Attribute("nightLevel")?.Value);
                        waterLevel = ParseFloatOrZero(node.Attribute("water")?.Value) > 0f;
                        if (ParseBool(node.Attribute("candiesConnected")?.Value))
                        {
                            _ = resources.Add(Resources.Snd.CandyLink);
                        }

                        break;
                    case "star":
                        if (nightLevel)
                        {
                            _ = resources.Add(Resources.Img.ObjStarNight);
                        }
                        break;
                    case "candyL":
                    case "candyR":
                        _ = resources.Add(Resources.Snd.CandyLink);
                        break;
                    case "grab":
                        AddGrabResources(resources, node);
                        break;
                    case "bubble":
                        _ = resources.Add(Resources.Snd.Bubble);
                        _ = resources.Add(Resources.Snd.BubbleBreak);
                        break;
                    case "spike1":
                    case "spike2":
                    case "spike3":
                    case "spike4":
                        _ = resources.Add(Resources.Img.ObjSpikes);
                        AddSpikeRotationSounds(resources);
                        break;
                    case "electro":
                        _ = resources.Add(Resources.Img.ObjElectrodes);
                        _ = resources.Add(Resources.Snd.Electric);
                        AddSpikeRotationSounds(resources);
                        break;
                    case "bouncer1":
                    case "bouncer2":
                        _ = resources.Add(Resources.Img.ObjBouncer);
                        _ = resources.Add(Resources.Snd.Bouncer);
                        break;
                    case "pump":
                        _ = resources.Add(Resources.Img.ObjPump);
                        _ = resources.Add(Resources.Snd.Pump1);
                        _ = resources.Add(Resources.Snd.Pump2);
                        _ = resources.Add(Resources.Snd.Pump3);
                        _ = resources.Add(Resources.Snd.Pump4);
                        break;
                    case "sock":
                        AddSockResources(resources, node);
                        break;
                    case "ghost":
                        _ = resources.Add(Resources.Img.ObjGhost);
                        _ = resources.Add(Resources.Snd.GhostPuff);
                        break;
                    case "rocket":
                        _ = resources.Add(Resources.Img.ObjRocket);
                        _ = resources.Add(Resources.Snd.ExpRocketStart);
                        _ = resources.Add(Resources.Snd.ExpRocketFlyLooped);
                        _ = resources.Add(Resources.Snd.ExpRocketInWater);
                        break;
                    case "axe":
                        _ = resources.Add(Resources.Img.ObjAxe);
                        _ = resources.Add(Resources.Img.FxCutChain);
                        _ = resources.Add(Resources.Snd.ChainCut);
                        break;
                    case "load":
                        _ = resources.Add(Resources.Img.ObjSnail);
                        _ = resources.Add(Resources.Snd.ExpSnailIn);
                        _ = resources.Add(Resources.Snd.ExpSnailOut);
                        break;
                    case "pipe":
                        _ = resources.Add(Resources.Img.ObjBambooTube);
                        _ = resources.Add(Resources.Snd.ExpBambooChute);
                        break;
                    case "ants":
                        _ = resources.Add(Resources.Img.ObjAnt);
                        _ = resources.Add(Resources.Snd.ExpAntsTakeCandy);
                        _ = resources.Add(Resources.Snd.ExpAntsDropCandy);
                        break;
                    case "lantern":
                        _ = resources.Add(Resources.Img.ObjLantern);
                        _ = resources.Add(Resources.Snd.LanternTeleportIn);
                        _ = resources.Add(Resources.Snd.LanternTeleportOut);
                        break;
                    case "gap":
                    case "mouse":
                        _ = resources.Add(Resources.Img.ObjMouse);
                        _ = resources.Add(Resources.Snd.MouseIdle);
                        _ = resources.Add(Resources.Snd.MouseRustle);
                        _ = resources.Add(Resources.Snd.MouseTap);
                        break;
                    case "conveyorBelt":
                    case "transporter":
                        _ = resources.Add(Resources.Img.ObjConveyor);
                        _ = resources.Add(Resources.Snd.TransporterMove);
                        _ = resources.Add(Resources.Snd.TransporterDrop);
                        _ = resources.Add(Resources.Snd.Conv01);
                        _ = resources.Add(Resources.Snd.Conv02);
                        _ = resources.Add(Resources.Snd.Conv03);
                        _ = resources.Add(Resources.Snd.Conv04);
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
                        _ = resources.Add(Resources.Snd.ExpHandCatch);
                        _ = resources.Add(Resources.Snd.ExpHandDrop);
                        _ = resources.Add(Resources.Snd.ExpHandRotate);
                        _ = resources.Add(Resources.Snd.ExpHandClap);
                        break;
                    case "gravitySwitch":
                        _ = resources.Add(Resources.Snd.GravityOn);
                        _ = resources.Add(Resources.Snd.GravityOff);
                        break;
                    case "pauseSwitcher":
                        _ = resources.Add(Resources.Img.ObjPause);
                        _ = resources.Add(Resources.Img.FxPause);
                        _ = resources.Add(Resources.Snd.PauseDown);
                        _ = resources.Add(Resources.Snd.PauseUp);
                        break;
                    case "target":
                        sawTarget = true;
                        targetSkins.Add(AddTargetResources(resources, node));
                        break;
                    case "steamTube":
                        _ = resources.Add(Resources.Img.ObjPipe);
                        _ = resources.Add(Resources.Snd.SteamStart);
                        _ = resources.Add(Resources.Snd.SteamStart2);
                        _ = resources.Add(Resources.Snd.SteamEnd);
                        break;
                    case "rotatedCircle":
                        _ = resources.Add(Resources.Img.ObjVinil);
                        _ = resources.Add(Resources.Snd.ScratchIn);
                        _ = resources.Add(Resources.Snd.ScratchOut);
                        break;
                    default:
                        break;
                }
            }

            // A classic target sleeps only on night levels; a themed one also sleeps after
            // being fed on a day level, so its sleep set is needed either way.
            foreach (OmNomSkinDefinition skin in targetSkins)
            {
                if (nightLevel || skin != null)
                {
                    AddOmNomSleepSounds(resources, skin);
                }
            }

            // Spritesheets owned by the classic animation backend are only needed when a classic
            // Om Nom is present. Use per-target resolution; if a level has no target node, fall
            // back to the player's selected skin to preserve prior behavior.
            bool hasClassicTarget = sawTarget
                ? targetSkins.Contains(null)
                : OmNomSkinRegistry.IsClassicSkin(OmNomSkinRegistry.GetSelectedSkinIndex());

            if (nightLevel)
            {
                _ = resources.Add(Resources.Img.ObjStarNight);

                if (hasClassicTarget)
                {
                    _ = resources.Add(Resources.Img.CharAnimationsSleeping);
                }

                _ = resources.Add(Resources.Img.FxSleep);
                _ = resources.Add(Resources.Snd.StarLight1);
                _ = resources.Add(Resources.Snd.StarLight2);
            }
            if (waterLevel)
            {
                _ = resources.Add(Resources.Img.WaterTile);
                _ = resources.Add(Resources.Snd.ExpWaterSplash);
            }
            if (SpecialEvents.IsXmas)
            {
                // Both Xmas character sheets belong to the classic animation backend; a themed
                // skin animates from its own Flash XML and never asks for them.
                if (hasClassicTarget)
                {
                    _ = resources.Add(Resources.Img.CharGreetingXmas);
                    _ = resources.Add(Resources.Img.CharIdleXmas);
                }

                _ = resources.Add(Resources.Img.XmasLights);
                _ = resources.Add(Resources.Img.Snowflakes);
                _ = resources.Add(Resources.Snd.XmasBell);
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

            // Every trace skin draws from these two, and nothing touches them until the
            // player's first swipe, which is exactly when a load must not happen.
            _ = resources.Add(Resources.Img.FingerTraces);
            _ = resources.Add(Resources.Img.FingerTraceGlow);

            _ = resources.Add(Resources.Snd.Tap);
            _ = resources.Add(Resources.Snd.CandyBreak);
            _ = resources.Add(Resources.Snd.RopeBleak1);
            _ = resources.Add(Resources.Snd.RopeBleak2);
            _ = resources.Add(Resources.Snd.RopeBleak3);
            _ = resources.Add(Resources.Snd.RopeBleak4);
            _ = resources.Add(Resources.Snd.RopeGet);
            _ = resources.Add(Resources.Snd.Star1);
            _ = resources.Add(Resources.Snd.Star2);
            _ = resources.Add(Resources.Snd.Star3);
            _ = resources.Add(Resources.Snd.Win);
        }

        /// <summary>
        /// Adds the sounds a spike button press plays, shared by plain and electrified spikes.
        /// </summary>
        /// <param name="resources">The destination set being accumulated.</param>
        private static void AddSpikeRotationSounds(HashSet<string> resources)
        {
            _ = resources.Add(Resources.Snd.SpikeRotateIn);
            _ = resources.Add(Resources.Snd.SpikeRotateOut);
        }

        /// <summary>
        /// Adds hook-related resources based on a grab node's attributes.
        /// </summary>
        /// <summary>
        /// Adds the art and sound one magic hat reaches. Which texture that is depends on the
        /// hat's group and the season, so the answer comes from <see cref="SockArt"/> - the same
        /// place the loader asks.
        /// </summary>
        /// <param name="resources">The destination set being accumulated.</param>
        /// <param name="node">The sock XML node being inspected.</param>
        private static void AddSockResources(HashSet<string> resources, XElement node)
        {
            int group = ParseIntOrZero(node.Attribute("group")?.Value);

            _ = resources.Add(SockArt.TextureFor(group, SpecialEvents.IsXmas));
            if (SockArt.WearsGeneratedBand(group))
            {
                _ = resources.Add(Resources.Img.ObjHatMaskable);
            }

            _ = resources.Add(SpecialEvents.IsXmas ? Resources.Snd.TeleportXmas : Resources.Snd.Teleport);
        }

        /// <param name="resources">The destination set being accumulated.</param>
        /// <param name="node">The grab XML node being inspected.</param>
        private static void AddGrabResources(HashSet<string> resources, XElement node)
        {
            bool gun = ParseBool(node.Attribute("gun")?.Value);
            bool kickable = ParseBool(node.Attribute("kickable")?.Value);
            bool bee = ParseBool(node.Attribute("bee")?.Value) || node.Attribute("path") != null;
            bool chain = node.Attribute("breakable") is XAttribute breakableAttr && !ParseBool(breakableAttr.Value);
            bool autoHook = node.Attribute("radius")?.Value is string radius && radius != "-1" && !gun;
            bool spider = ParseBool(node.Attribute("spider")?.Value);
            bool wheel = ParseBool(node.Attribute("wheel")?.Value);

            _ = resources.Add(chain
                ? autoHook ? Resources.Img.ObjHookAutoChain : Resources.Img.ObjHookChain
                : Resources.Img.ObjHook);

            if (bee)
            {
                _ = resources.Add(Resources.Img.ObjBee);
                _ = resources.Add(Resources.Snd.Buzz);
            }
            if (gun)
            {
                _ = resources.Add(Resources.Img.ObjGun);
                _ = resources.Add(Resources.Snd.ExpGun);
            }
            if (kickable)
            {
                _ = resources.Add(Resources.Img.ObjSticker);
                _ = resources.Add(Resources.Snd.ExpSuckerDrop);
                _ = resources.Add(Resources.Snd.ExpSuckerLand);
            }
            if (chain)
            {
                _ = resources.Add(Resources.Img.ObjExpChain);
            }
            if (spider)
            {
                _ = resources.Add(Resources.Img.ObjSpider);
                _ = resources.Add(Resources.Snd.SpiderActivate);
                _ = resources.Add(Resources.Snd.SpiderFall);
                _ = resources.Add(Resources.Snd.SpiderWin);
            }
            if (wheel)
            {
                _ = resources.Add(Resources.Snd.Wheel);
            }
        }

        /// <summary>
        /// Adds Om Nom animation and voice resources for a single target, using its resolved skin.
        /// </summary>
        /// <param name="resources">The destination set being accumulated.</param>
        /// <param name="node">The target XML node, whose <c>targetType</c> selects the skin.</param>
        /// <returns>The target's themed skin definition, or <see langword="null"/> for the classic skin.</returns>
        private static OmNomSkinDefinition AddTargetResources(HashSet<string> resources, XElement node)
        {
            int targetType = ParseIntOrZero(node.Attribute("targetType")?.Value ?? string.Empty);
            int skinIndex = OmNomSkinRegistry.ResolveTargetSkinIndex(
                targetType,
                OmNomSkinRegistry.GetSelectedSkinIndex(),
                OmNomSkinRegistry.TotalSkinCount);

            bool isClassic = OmNomSkinRegistry.IsClassicSkin(skinIndex);
            OmNomSkinDefinition skin = null;
            if (isClassic)
            {
                _ = resources.Add(Resources.Img.CharAnimations);
                _ = resources.Add(Resources.Img.CharAnimations2);
                _ = resources.Add(Resources.Img.CharAnimations3);
            }
            else
            {
                skin = OmNomSkinRegistry.GetXmlSkinDefinition(skinIndex);
                _ = string.Equals(skin.Id, "OM_NOM_PREHISTORIC", StringComparison.Ordinal)
                    ? resources.Add(Resources.Img.CharAnimationsPrehistoric)
                    : resources.Add(Resources.Img.CharAnimationsSmooth);
            }

            _ = resources.Add(Resources.Img.FxBubbles);
            _ = resources.Add(Resources.Img.CharSupports);

            AddOmNomSound(resources, skin, Resources.Snd.MonsterChewing);
            AddOmNomSound(resources, skin, Resources.Snd.MonsterClose);
            AddOmNomSound(resources, skin, Resources.Snd.MonsterOpen);
            AddOmNomSound(resources, skin, Resources.Snd.MonsterSad);
            AddOmNomSound(resources, skin, Resources.Snd.MonsterExcited);
            AddOmNomSound(resources, skin, Resources.Snd.MonsterGreeting);

            return skin;
        }

        /// <summary>
        /// Adds the three sleep voice clips a target can play, resolved against its skin.
        /// </summary>
        /// <param name="resources">The destination set being accumulated.</param>
        /// <param name="skin">The target's themed skin, or <see langword="null"/> for the classic skin.</param>
        private static void AddOmNomSleepSounds(HashSet<string> resources, OmNomSkinDefinition skin)
        {
            AddOmNomSound(resources, skin, Resources.Snd.MonsterSleep1);
            AddOmNomSound(resources, skin, Resources.Snd.MonsterSleep2);
            AddOmNomSound(resources, skin, Resources.Snd.MonsterSleep3);
        }

        /// <summary>
        /// Adds the sound a target actually plays for a classic clip, following the same skin
        /// resolution that playback uses so themed skins warm their own recordings.
        /// </summary>
        /// <param name="resources">The destination set being accumulated.</param>
        /// <param name="skin">The target's themed skin, or <see langword="null"/> for the classic skin.</param>
        /// <param name="classicSoundResourceName">The classic clip the target would ask for.</param>
        private static void AddOmNomSound(HashSet<string> resources, OmNomSkinDefinition skin, string classicSoundResourceName)
        {
            // Resolves to null for clips a skin opts out of, which the final filter drops.
            _ = resources.Add(OmNomSoundResolver.ResolveSoundResource(skin, classicSoundResourceName));
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
