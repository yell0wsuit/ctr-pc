using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;

using CutTheRopeDX.Framework;
using CutTheRopeDX.Framework.Visual;

using Microsoft.Xna.Framework;

namespace CutTheRopeDX.GameMain
{
    /// <summary>
    /// Centralized string constants for content assets grouped by type to simplify resource lookups.
    /// </summary>
    internal static class Resources
    {
        /// <summary>Cached sound resource names.</summary>
        private static HashSet<string> soundNames_;

        /// <summary>Cached map from sound constant identifiers to sound resource names.</summary>
        private static Dictionary<string, string> soundFieldNames_;

        /// <summary>Cached music resource names.</summary>
        private static HashSet<string> musicNames_;

        /// <summary>Cached font resource names.</summary>
        private static HashSet<string> fontNames_;

        /// <summary>Cached atlas image resource names.</summary>
        private static HashSet<string> imageNames_;

        /// <summary>Cached background image resource names.</summary>
        private static HashSet<string> backgroundImgNames_;

        /// <summary>
        /// Checks if a resource name is valid (exists in Resources.cs).
        /// </summary>
        /// <param name="resourceName">Resource name to check.</param>
        /// <returns><see langword="true"/> when the resource name is known; otherwise, <see langword="false"/>.</returns>
        public static bool IsValidResourceName(string resourceName)
        {
            if (imageNames_ == null)
            {
                InitializeImageNames();
            }
            if (backgroundImgNames_ == null)
            {
                InitializeBackgroundImgNames();
            }
            if (soundNames_ == null)
            {
                InitializeSoundNames();
            }
            if (musicNames_ == null)
            {
                InitializeMusicNames();
            }
            if (fontNames_ == null)
            {
                InitializeFontNames();
            }
            return imageNames_.Contains(resourceName) ||
                   backgroundImgNames_.Contains(resourceName) ||
                   soundNames_.Contains(resourceName) ||
                   musicNames_.Contains(resourceName) ||
                   fontNames_.Contains(resourceName);
        }

        /// <summary>
        /// Checks if a resource name is a sound.
        /// </summary>
        /// <param name="resourceName">Resource name to check.</param>
        /// <returns><see langword="true"/> when the resource name is a known sound; otherwise, <see langword="false"/>.</returns>
        public static bool IsSound(string resourceName)
        {
            if (soundNames_ == null)
            {
                InitializeSoundNames();
            }
            return soundNames_.Contains(resourceName);
        }

        /// <summary>
        /// Resolves a sound identifier such as MonsterExcited to the underlying resource name.
        /// Accepts either the constant identifier or the resource value.
        /// </summary>
        /// <param name="soundIdentifier">Sound constant identifier or resource name to resolve.</param>
        /// <param name="soundResourceName">Resolved sound resource name when this method returns <see langword="true"/>.</param>
        /// <returns><see langword="true"/> when the identifier maps to a known sound; otherwise, <see langword="false"/>.</returns>
        public static bool TryResolveSoundIdentifier(string soundIdentifier, out string soundResourceName)
        {
            if (soundNames_ == null || soundFieldNames_ == null)
            {
                InitializeSoundNames();
            }

            if (string.IsNullOrWhiteSpace(soundIdentifier))
            {
                soundResourceName = null;
                return false;
            }

            if (soundFieldNames_.TryGetValue(soundIdentifier, out soundResourceName))
            {
                return true;
            }

            if (soundNames_.Contains(soundIdentifier))
            {
                soundResourceName = soundIdentifier;
                return true;
            }

            soundResourceName = null;
            return false;
        }

        /// <summary>
        /// Checks if a resource name is music.
        /// </summary>
        /// <param name="resourceName">Resource name to check.</param>
        /// <returns><see langword="true"/> when the resource name is known music; otherwise, <see langword="false"/>.</returns>
        public static bool IsMusic(string resourceName)
        {
            if (musicNames_ == null)
            {
                InitializeMusicNames();
            }
            return musicNames_.Contains(resourceName);
        }

        /// <summary>
        /// Checks if a resource name is a font.
        /// </summary>
        /// <param name="resourceName">Resource name to check.</param>
        /// <returns><see langword="true"/> when the resource name is a known font; otherwise, <see langword="false"/>.</returns>
        public static bool IsFont(string resourceName)
        {
            if (fontNames_ == null)
            {
                InitializeFontNames();
            }
            return fontNames_.Contains(resourceName);
        }

        /// <summary>
        /// Checks if a resource name is an image.
        /// </summary>
        /// <param name="resourceName">Resource name to check.</param>
        /// <returns><see langword="true"/> when the resource name is a known image or background image; otherwise, <see langword="false"/>.</returns>
        public static bool IsImage(string resourceName)
        {
            if (imageNames_ == null)
            {
                InitializeImageNames();
            }
            if (backgroundImgNames_ == null)
            {
                InitializeBackgroundImgNames();
            }
            return imageNames_.Contains(resourceName) || backgroundImgNames_.Contains(resourceName);
        }

        /// <summary>
        /// Builds a set of resource values from literal public fields on a catalog type.
        /// </summary>
        /// <param name="type">Catalog type to inspect.</param>
        /// <returns>The resource values declared on <paramref name="type"/>.</returns>
        private static HashSet<string> NamesFrom([DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicFields)] Type type)
        {
            return [.. type.GetFields(BindingFlags.Public | BindingFlags.Static)
                .Where(f => f.IsLiteral)
                .Select(f => (string)f.GetValue(null))];
        }

        /// <summary>
        /// Builds a map from literal public field names to resource values on a catalog type.
        /// </summary>
        /// <param name="type">Catalog type to inspect.</param>
        /// <returns>A map of field names to resource values declared on <paramref name="type"/>.</returns>
        private static Dictionary<string, string> NameMapFrom(
            [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicFields)] Type type)
        {
            return type.GetFields(BindingFlags.Public | BindingFlags.Static)
                .Where(f => f.IsLiteral)
                .ToDictionary(f => f.Name, f => (string)f.GetValue(null));
        }

        /// <summary>
        /// Initializes the cached atlas image resource name set.
        /// </summary>
        private static void InitializeImageNames() { imageNames_ = NamesFrom(typeof(Img)); }

        /// <summary>
        /// Initializes cached sound resource names and sound identifier mapping.
        /// </summary>
        private static void InitializeSoundNames()
        {
            soundNames_ = NamesFrom(typeof(Snd));
            soundFieldNames_ = NameMapFrom(typeof(Snd));
        }

        /// <summary>
        /// Initializes the cached music resource name set.
        /// </summary>
        private static void InitializeMusicNames() { musicNames_ = NamesFrom(typeof(Music)); }

        /// <summary>
        /// Initializes the cached font resource name set.
        /// </summary>
        private static void InitializeFontNames() { fontNames_ = NamesFrom(typeof(Fnt)); }

        /// <summary>
        /// Checks if a resource name is a background image.
        /// Background images are loaded without JSON atlas files.
        /// </summary>
        /// <param name="resourceName">Resource name to check.</param>
        /// <returns><see langword="true"/> when the resource name is a known background image; otherwise, <see langword="false"/>.</returns>
        public static bool IsBackgroundImg(string resourceName)
        {
            if (backgroundImgNames_ == null)
            {
                InitializeBackgroundImgNames();
            }
            return backgroundImgNames_.Contains(resourceName);
        }

        /// <summary>
        /// Initializes the cached background image resource name set.
        /// </summary>
        private static void InitializeBackgroundImgNames()
        {
            backgroundImgNames_ =
            [
                BackgroundImg.Bgr01P1, BackgroundImg.Bgr01P2, BackgroundImg.Bgr02P1, BackgroundImg.Bgr02P2,
                BackgroundImg.Bgr03P1, BackgroundImg.Bgr03P2, BackgroundImg.Bgr04P1, BackgroundImg.Bgr04P2,
                BackgroundImg.Bgr05P1, BackgroundImg.Bgr05P2, BackgroundImg.Bgr06P1, BackgroundImg.Bgr06P2,
                BackgroundImg.Bgr07P1, BackgroundImg.Bgr07P2, BackgroundImg.Bgr08P1, BackgroundImg.Bgr08P2,
                BackgroundImg.Bgr09P1, BackgroundImg.Bgr09P2, BackgroundImg.Bgr10P1, BackgroundImg.Bgr10P2,
                BackgroundImg.Bgr11P1, BackgroundImg.Bgr11P2, BackgroundImg.Bgr12P1, BackgroundImg.Bgr13P1,
                BackgroundImg.Bgr14P1, BackgroundImg.Bgr15P1, BackgroundImg.Bgr16P1, BackgroundImg.Bgr17P1,
                BackgroundImg.ZeptolabNoLink, BackgroundImg.SkinBackground
            ];
        }

        /// <summary>
        /// Background image resource names.
        /// </summary>
        internal static class BackgroundImg
        {
            public const string Bgr01P1 = "bgr_01_p1";
            public const string Bgr01P2 = "bgr_01_p2";
            public const string Bgr02P1 = "bgr_02_p1";
            public const string Bgr02P2 = "bgr_02_p2";
            public const string Bgr03P1 = "bgr_03_p1";
            public const string Bgr03P2 = "bgr_03_p2";
            public const string Bgr04P1 = "bgr_04_p1";
            public const string Bgr04P2 = "bgr_04_p2";
            public const string Bgr05P1 = "bgr_05_p1";
            public const string Bgr05P2 = "bgr_05_p2";
            public const string Bgr06P1 = "bgr_06_p1";
            public const string Bgr06P2 = "bgr_06_p2";
            public const string Bgr07P1 = "bgr_07_p1";
            public const string Bgr07P2 = "bgr_07_p2";
            public const string Bgr08P1 = "bgr_08_p1";
            public const string Bgr08P2 = "bgr_08_p2";
            public const string Bgr09P1 = "bgr_09_p1";
            public const string Bgr09P2 = "bgr_09_p2";
            public const string Bgr10P1 = "bgr_10_p1";
            public const string Bgr10P2 = "bgr_10_p2";
            public const string Bgr11P1 = "bgr_11_p1";
            public const string Bgr11P2 = "bgr_11_p2";
            public const string Bgr12P1 = "bgr_12_p1";
            public const string Bgr13P1 = "bgr_13_p1";
            public const string Bgr14P1 = "bgr_14_p1";
            public const string Bgr15P1 = "bgr_15_p1";
            public const string Bgr16P1 = "bgr_16_p1";
            public const string Bgr17P1 = "bgr_17_p1";
            public const string ZeptolabNoLink = "zeptolab_no_link";
            public const string SkinBackground = "skin_bg";
        }

        /// <summary>
        /// Image and atlas resource names.
        /// </summary>
        internal static class Img
        {
            public const string ZeptoLabLogoLoading = "zepto_loaderbar";
            public const string ZeptoLabLogoAnim = "zeptolab_logo_anim";
            public const string LoaderbarFull = "loaderbar_full";
            public const string MenuButtons = "menu_buttons";
            public const string MenuNotification = "menu_notification";
            public const string MenuAchievement = "menu_achievement";
            public const string MenuOptions = "menu_options_packed";
            public const string MenuBgr = "menu_bgr";
            public const string MenuPopup = "menu_popup";
            public const string MenuLogo = "menu_logo";
            public const string MenuLogoNew = "menu_logo_new";
            public const string CutTheRopeDXLogo = "CutTheRopeDXLogo";
            public const string CandySelectionFx = "candy_selection_fx";
            public const string SkinSelection = "skin_selection";
            public const string MenuPackUI = "menu_pack_ui";
            public const string MenuPackSelection = "menu_pack_selection";
            public const string MenuPackSelection2 = "menu_pack_selection2";
            public const string MenuExtraButtons = "menu_extra_buttons";
            public const string MenuScrollbar = "menu_scrollbar";
            public const string MenuLeaderboard = "menu_leaderboard";
            public const string MenuProcessingHd = "menu_processing_hd";
            public const string MenuScrollbarChangename = "menu_scrollbar_changename";
            public const string MenuButtonAchivCup = "menu_button_achiv_cup";
            public const string MenuBgrShadow = "menu_bgr_shadow";
            public const string MenuLevelUi = "menu_level_ui";
            public const string HudUi = "hud_ui";
            public const string ObjSpider = "obj_spider";
            public const string ConfettiParticles = "confetti_particles";
            public const string MenuPause = "menu_pause";
            public const string MenuResults = "menu_results";
            public const string ObjStarDisappear = "obj_star_disappear";
            public const string ObjBubble = "obj_bubble";
            public const string ObjHook = "obj_hook";
            public const string ObjExpChain = "obj_exp_chain";
            public const string ObjStarIdle = "obj_star_idle";
            public const string ObjStarNight = "obj_star_night";
            public const string CharAnimations = "char_animations";
            public const string CharAnimationsSmooth = "char_animations_smooth";
            public const string CharAnimationsSleeping = "char_animations_sleeping";
            public const string FxSleep = "fx_sleep";
            public const string FxBubbles = "fx_bubbles";
            public const string FxCutChain = "fx_cut_chain";
            public const string HatHalloween = "hat_halloween";
            public const string HatXmas = "hat_xmas";
            public const string CharAnimationsPrehistoric = "char_animations_body_prehistoric";
            public const string ObjGun = "obj_gun";
            public const string ObjSticker = "obj_sticker";
            public const string ObjPump = "obj_pump";
            public const string TutorialSigns = "tutorial_signs";
            public const string ObjHat = "obj_hat";
            public const string ObjBouncer = "obj_bouncer";
            public const string ObjSpikes = "obj_spikes";
            public const string ObjElectrodes = "obj_electrodes";
            public const string ObjAnt = "obj_ant";
            public const string ObjBee = "obj_bee";
            public const string CharSupports = "char_supports";
            public const string CharAnimations2 = "char_animations2";
            public const string CharAnimations3 = "char_animations3";
            public const string ObjVinil = "obj_vinil";
            public const string Bgr01Cover = "bgr_01_cover";
            public const string Bgr02Cover = "bgr_02_cover";
            public const string Bgr03Cover = "bgr_03_cover";
            public const string Bgr04Cover = "bgr_04_cover";
            public const string Bgr05Cover = "bgr_05_cover";
            public const string Bgr06Cover = "bgr_06_cover";
            public const string Bgr07Cover = "bgr_07_cover";
            public const string Bgr08Cover = "bgr_08_cover";
            public const string Bgr09Cover = "bgr_09_cover";
            public const string Bgr10Cover = "bgr_10_cover";
            public const string Bgr11Cover = "bgr_11_cover";
            public const string MenuExtraButtonsFr = "menu_extra_buttons_fr";
            public const string MenuExtraButtonsGr = "menu_extra_buttons_gr";
            public const string MenuExtraButtonsRu = "menu_extra_buttons_ru";
            public const string MenuExtraButtonsEn = "menu_extra_buttons_en";
            public const string Bgr12Cover = "bgr_12_cover";
            public const string ObjGhost = "obj_ghost";
            public const string XmasLights = "christmas_lights";
            public const string Snowflakes = "snowflakes";
            public const string CharGreetingXmas = "char_greeting_xmas";
            public const string CharIdleXmas = "char_idle_xmas";
            public const string ObjSock = "obj_sock_xmas";
            public const string MenuBgrXmas = "menu_bgr_xmas";
            public const string MenuLogoXmasHat = "xmas_hat_logo";
            public const string Bgr13Cover = "bgr_13_cover";
            public const string ObjPipe = "obj_pipe";
            public const string Bgr14Cover = "bgr_14_cover";
            public const string ObjLantern = "obj_lantern";
            public const string Bgr15Cover = "bgr_15_cover";
            public const string ObjMouse = "obj_mouse";
            public const string Bgr16Cover = "bgr_16_cover";
            public const string ObjLighter = "obj_lighter";
            public const string Bgr17Cover = "bgr_17_cover";
            public const string BoxLabel = "box_label";
            public const string ObjConveyor = "obj_conveyor";
            public const string ObjRocket = "obj_rocket";
            public const string WaterTile = "water_tile";
            public const string ObjSnail = "obj_snail";
            public const string ObjRoboHand = "obj_robohand";
            public const string ObjBambooTube = "obj_bamboo_tube";
            public const string FingerTraceSkinOptions = "fingertrace_skin";
            public const string FingerTraces = "traces_ctr2";
            public const string FingerTraceGlow = "traces_ctr2_glow";

            // Candies
            public const string ObjCandyFx = "candies/obj_candy_fx";
            public const string ObjCandy01New = "candies/obj_candy_01_new";
            public const string ObjCandy02 = "candies/obj_candy_02";
            public const string ObjCandy03 = "candies/obj_candy_03";
            public const string ObjCandy04 = "candies/obj_candy_04";
            public const string ObjCandy05 = "candies/obj_candy_05";
            public const string ObjCandy06 = "candies/obj_candy_06";
            public const string ObjCandy07 = "candies/obj_candy_07";
            public const string ObjCandy08 = "candies/obj_candy_08";
            public const string ObjCandy09 = "candies/obj_candy_09";
            public const string ObjCandy10 = "candies/obj_candy_10";
            public const string ObjCandy11 = "candies/obj_candy_11";
            public const string ObjCandy12 = "candies/obj_candy_12";
            public const string ObjCandy13 = "candies/obj_candy_13";
            public const string ObjCandy14 = "candies/obj_candy_14";
            public const string ObjCandy15 = "candies/obj_candy_15";
            public const string ObjCandy16 = "candies/obj_candy_16";
            public const string ObjCandy17 = "candies/obj_candy_17";
            public const string ObjCandy18 = "candies/obj_candy_18";
            public const string ObjCandy19 = "candies/obj_candy_19";
            public const string ObjCandy20 = "candies/obj_candy_20";
            public const string ObjCandy21 = "candies/obj_candy_21";
            public const string ObjCandy22 = "candies/obj_candy_22";
            public const string ObjCandy23 = "candies/obj_candy_23";
            public const string ObjCandy24 = "candies/obj_candy_24";
            public const string ObjCandy25 = "candies/obj_candy_25";
            public const string ObjCandy26 = "candies/obj_candy_26";
            public const string ObjCandy27 = "candies/obj_candy_27";
            public const string ObjCandy28 = "candies/obj_candy_28";
            public const string ObjCandy29 = "candies/obj_candy_29";
            public const string ObjCandy30 = "candies/obj_candy_30";
            public const string ObjCandy31 = "candies/obj_candy_31";
            public const string ObjCandy32 = "candies/obj_candy_32";
            public const string ObjCandy33 = "candies/obj_candy_33";
            public const string ObjCandy34 = "candies/obj_candy_34";
            public const string ObjCandy35 = "candies/obj_candy_35";
            public const string ObjCandy36 = "candies/obj_candy_36";
            public const string ObjCandy37 = "candies/obj_candy_37";
            public const string ObjCandy38 = "candies/obj_candy_38";
            public const string ObjCandy39 = "candies/obj_candy_39";
            public const string ObjCandy40 = "candies/obj_candy_40";
            public const string ObjCandy41 = "candies/obj_candy_41";
            public const string ObjCandy42 = "candies/obj_candy_42";
            public const string ObjCandy43 = "candies/obj_candy_43";
            public const string ObjCandy44 = "candies/obj_candy_44";
            public const string ObjCandy45 = "candies/obj_candy_45";
            public const string ObjCandy46 = "candies/obj_candy_46";
            public const string ObjCandy47 = "candies/obj_candy_47";
            public const string ObjCandy48 = "candies/obj_candy_48";
            public const string ObjCandy49 = "candies/obj_candy_49";
            public const string ObjCandy50 = "candies/obj_candy_50";
            public const string ObjCandy51 = "candies/obj_candy_51";
            public const string ObjCandy52 = "candies/obj_candy_52";
        }

        /// <summary>
        /// Font resource names (now using FontStashSharp).
        /// </summary>
        internal static class Fnt
        {
            public const string BigFont = "big_font";
            public const string SmallFont = "small_font";
            public const string FontNumbersBig = "font_numbers_big";
        }

        /// <summary>
        /// Font configuration for FontStashSharp fonts.
        /// </summary>
        internal static class FontConfig
        {
            /// <summary>Default Latin font file.</summary>
            private const string StandardFont = "gooddog_new-webfont.ttf";

            /// <summary>Font file used for Russian text.</summary>
            private const string RussianFont = "PlaypenSans-SemiBold.ttf";

            /// <summary>Font file used for Korean text.</summary>
            private const string KoreanFont = "Cafe24DongdongRegular.otf";

            /// <summary>Font file used for Simplified and Traditional Chinese text.</summary>
            private const string ChineseFont = "KNMaiyuan-Regular.ttf";

            /// <summary>Font file used for Japanese text.</summary>
            private const string JapaneseFont = "MPLUSRounded1c-Medium.ttf";

            /// <summary>
            /// Gets the font file for a language.
            /// </summary>
            /// <param name="language">Language ID from <see cref="Language"/>.</param>
            /// <returns>The font file name to use for the language.</returns>
            private static string GetFontFile(int language)
            {
                return language switch
                {
                    (int)Language.LANGRU => RussianFont,
                    (int)Language.LANGKO => KoreanFont,
                    (int)Language.LANGZH or (int)Language.LANGZHTW => ChineseFont,
                    (int)Language.LANGJA => JapaneseFont,
                    _ => StandardFont,
                };
            }

            /// <summary>
            /// Gets the font size scale for a language.
            /// </summary>
            /// <param name="language">Language ID from <see cref="Language"/>.</param>
            /// <returns>The font size multiplier for the language.</returns>
            private static float GetFontSizeScale(int language)
            {
                return language switch
                {
                    (int)Language.LANGZH or (int)Language.LANGZHTW => 0.75f,
                    (int)Language.LANGJA => 0.85f,
                    (int)Language.LANGRU => 0.9f,
                    (int)Language.LANGKO => 0.9f,
                    _ => 1f,
                };
            }

            /// <summary>
            /// Gets a font rendering configuration for a named game font.
            /// </summary>
            /// <param name="fontName">Font resource name from <see cref="Fnt"/>.</param>
            /// <param name="language">Language ID from <see cref="Language"/>.</param>
            /// <returns>The rendering configuration for <paramref name="fontName"/>.</returns>
            public static FontConfiguration GetConfiguration(string fontName, int language)
            {
                return fontName switch
                {
                    Fnt.BigFont => new FontConfiguration
                    {
                        FontFile = GetFontFile(language),
                        Size = 100f * GetFontSizeScale(language),
                        Color = Color.White,
                        Effects = FontEffectSettings.CreateStrokeAndShadow(2, 2, 3),
                        LineSpacing = 5f,
                        TopSpacing = -10f
                    },
                    Fnt.SmallFont => new FontConfiguration
                    {
                        FontFile = GetFontFile(language),
                        Size = 72f * GetFontSizeScale(language),
                        Color = Color.Black,
                        Effects = FontEffectSettings.None,
                        LineSpacing = 5f,
                        TopSpacing = 25f
                    },
                    Fnt.FontNumbersBig => new FontConfiguration
                    {
                        FontFile = StandardFont,
                        Size = 100f,
                        Color = Color.Black,
                        Effects = FontEffectSettings.None,
                        LineSpacing = 5f,
                        TopSpacing = 5f
                    },
                    _ => throw new ArgumentException($"Unknown font: {fontName}", nameof(fontName))
                };
            }
        }

        /// <summary>
        /// Sound effect resource names.
        /// </summary>
        internal static class Snd
        {
            public const string ZeptoLogoBubbles = "zepto_logo_bubbles";
            public const string Tap = "tap";
            public const string Button = "button";
            public const string BubbleBreak = "bubble_break";
            public const string Bubble = "bubble";
            public const string CandyBreak = "candy_break";
            public const string MonsterChewing = "monster_chewing";
            public const string MonsterClose = "monster_close";
            public const string MonsterOpen = "monster_open";
            public const string MonsterSad = "monster_sad";
            public const string MonsterExcited = "monster_excited";
            public const string MonsterGreeting = "monster_greeting";
            public const string Ring = "ring";
            public const string RopeBleak1 = "rope_bleak_1";
            public const string RopeBleak2 = "rope_bleak_2";
            public const string RopeBleak3 = "rope_bleak_3";
            public const string RopeBleak4 = "rope_bleak_4";
            public const string ChainCut = "chain_cut";
            public const string RopeGet = "rope_get";
            public const string Star1 = "star_1";
            public const string Star2 = "star_2";
            public const string Star3 = "star_3";
            public const string Electric = "electric";
            public const string Pump1 = "pump_1";
            public const string Pump2 = "pump_2";
            public const string Pump3 = "pump_3";
            public const string Pump4 = "pump_4";
            public const string SpiderActivate = "spider_activate";
            public const string SpiderFall = "spider_fall";
            public const string SpiderWin = "spider_win";
            public const string Wheel = "wheel";
            public const string Win = "win";
            public const string GravityOff = "gravity_off";
            public const string GravityOn = "gravity_on";
            public const string CandyLink = "candy_link";
            public const string Bouncer = "bouncer";
            public const string SpikeRotateIn = "spike_rotate_in";
            public const string SpikeRotateOut = "spike_rotate_out";
            public const string Buzz = "buzz";
            public const string Teleport = "teleport";
            public const string ScratchIn = "scratch_in";
            public const string ScratchOut = "scratch_out";
            public const string GhostPuff = "ghost_puff";
            public const string XmasBell = "xmas_bell";
            public const string TeleportXmas = "teleport_xmas";
            public const string SteamStart = "steam_start";
            public const string SteamStart2 = "steam_start2";
            public const string SteamEnd = "steam_end";
            public const string LanternTeleportIn = "lantern_teleport_in";
            public const string LanternTeleportOut = "lantern_teleport_out";
            public const string MouseRustle = "mouse_rustle";
            public const string MouseTap = "mouse_tap";
            public const string MouseIdle = "mouse_idle";
            public const string MonsterSleep1 = "monster_sleep01";
            public const string MonsterSleep2 = "monster_sleep02";
            public const string MonsterSleep3 = "monster_sleep03";
            public const string StarLight1 = "star_light01";
            public const string StarLight2 = "star_light02";
            public const string TransporterDrop = "transporter_drop";
            public const string TransporterMove = "transporter_move";
            public const string Conv01 = "con01";
            public const string Conv02 = "con02";
            public const string Conv03 = "con03";
            public const string Conv04 = "con04";

            // CTR Experiments sounds
            public const string ExpGun = "gun";
            public const string ExpSuckerDrop = "sucker_drop";
            public const string ExpSuckerLand = "sucker_land";
            public const string ExpRocketStart = "rocket_start";
            public const string ExpRocketFlyLooped = "rocket_fly_looped";
            public const string ExpRocketInWater = "rocket_in_water";
            public const string ExpWaterSplash = "water_splash";
            public const string ExpSnailIn = "snail_in";
            public const string ExpSnailOut = "snail_out";
            public const string ExpHandCatch = "hand_catch";
            public const string ExpHandDrop = "hand_drop";
            public const string ExpHandRotate = "hand_rotate";
            public const string ExpHandClap = "hand_clap";
            public const string ExpAntsTakeCandy = "ants_take_candy";
            public const string ExpAntsDropCandy = "ants_drop_candy";
            public const string ExpBambooChute = "bamboo_chutes_4_5";

            // CTR Time Travel sounds
            public const string TTArtistChewing = "Artist_chewing";
            public const string TTArtistExcited = "Artist_excited";
            public const string TTArtistGreeting = "Artist_greeting";
            public const string TTArtistMouthClose = "Artist_mouthClose";
            public const string TTArtistMouthOpen = "Artist_mouthOpen";
            public const string TTArtistSad = "Artist_sad";
            public const string TTArtistSleep01 = "Artist_sleep01";
            public const string TTArtistSleep02 = "Artist_sleep02";
            public const string TTArtistSleep03 = "Artist_sleep03";
            public const string TTCaesarChewing = "Caesar_chewing";
            public const string TTCaesarExcited = "Caesar_excited";
            public const string TTCaesarGreeting = "Caesar_greeting";
            public const string TTCaesarMouthClose = "Caesar_mouthClose";
            public const string TTCaesarMouthOpen = "Caesar_mouthOpen";
            public const string TTCaesarSad = "Caesar_sad";
            public const string TTCaesarSleep01 = "Caesar_sleep01";
            public const string TTCaesarSleep02 = "Caesar_sleep02";
            public const string TTCaesarSleep03 = "Caesar_sleep03";
            public const string TTDiscoChewing = "Disco_chewing";
            public const string TTDiscoExcited = "Disco_excited";
            public const string TTDiscoGreeting = "Disco_greeting";
            public const string TTDiscoMouthClose = "Disco_mouthClose";
            public const string TTDiscoMouthOpen = "Disco_mouthOpen";
            public const string TTDiscoSad = "Disco_sad";
            public const string TTDiscoSleep01 = "Disco_sleep01";
            public const string TTDiscoSleep02 = "Disco_sleep02";
            public const string TTDiscoSleep03 = "Disco_sleep03";
            public const string TTMedievalChewing = "Medieval_chewing";
            public const string TTMedievalExcited = "Medieval_excited";
            public const string TTMedievalGreeting = "Medieval_greeting";
            public const string TTMedievalMouthClose = "Medieval_mouthClose";
            public const string TTMedievalMouthOpen = "Medieval_mouthOpen";
            public const string TTMedievalSad = "Medieval_sad";
            public const string TTMedievalSleep01 = "Medieval_sleep01";
            public const string TTMedievalSleep02 = "Medieval_sleep02";
            public const string TTMedievalSleep03 = "Medieval_sleep03";
            public const string TTPharaohChewing = "Pharaoh_chewing";
            public const string TTPharaohExcited = "Pharaoh_excited";
            public const string TTPharaohGreeting = "Pharaoh_greeting";
            public const string TTPharaohMouthClose = "Pharaoh_mouthClose";
            public const string TTPharaohMouthOpen = "Pharaoh_mouthOpen";
            public const string TTPharaohSad = "Pharaoh_sad";
            public const string TTPharaohSleep01 = "Pharaoh_sleep01";
            public const string TTPharaohSleep02 = "Pharaoh_sleep02";
            public const string TTPharaohSleep03 = "Pharaoh_sleep03";
            public const string TTPirateChewing = "Pirate_chewing";
            public const string TTPirateExcited = "Pirate_excited";
            public const string TTPirateGreeting = "Pirate_greeting";
            public const string TTPirateMouthClose = "Pirate_mouthClose";
            public const string TTPirateMouthOpen = "Pirate_mouthOpen";
            public const string TTPirateSad = "Pirate_sad";
            public const string TTPirateSleep01 = "Pirate_sleep01";
            public const string TTPirateSleep02 = "Pirate_sleep02";
            public const string TTPirateSleep03 = "Pirate_sleep03";
            public const string TTPrehistoricChewing = "Prehistoric_chewing";
            public const string TTPrehistoricExcited = "Prehistoric_excited";
            public const string TTPrehistoricGreeting = "Prehistoric_greeting";
            public const string TTPrehistoricMouthClose = "Prehistoric_mouthClose";
            public const string TTPrehistoricMouthOpen = "Prehistoric_mouthOpen";
            public const string TTPrehistoricSad = "Prehistoric_sad";
            public const string TTPrehistoricSleep01 = "Prehistoric_sleep01";
            public const string TTPrehistoricSleep02 = "Prehistoric_sleep02";
            public const string TTPrehistoricSleep03 = "Prehistoric_sleep03";
            public const string TTWesternChewing = "Western_chewing";
            public const string TTWesternExcited = "Western_excited";
            public const string TTWesternGreeting = "Western_greeting";
            public const string TTWesternMouthClose = "Western_mouthClose";
            public const string TTWesternMouthOpen = "Western_mouthOpen";
            public const string TTWesternSad = "Western_sad";
            public const string TTWesternSleep01 = "Western_sleep01";
            public const string TTWesternSleep02 = "Western_sleep02";
            public const string TTWesternSleep03 = "Western_sleep03";
            public const string TTChinaChewing = "China_chewing";
            public const string TTChinaExcited = "China_excited";
            public const string TTChinaGreeting = "China_greeting";
            public const string TTChinaMouthClose = "China_mouthClose";
            public const string TTChinaMouthOpen = "China_mouthOpen";
            public const string TTChinaSad = "China_sad";
            public const string TTChinaSleep01 = "China_sleep01";
            public const string TTChinaSleep02 = "China_sleep02";
            public const string TTChinaSleep03 = "China_sleep03";
            public const string TTIndustrialChewing = "Industrial_chewing";
            public const string TTIndustrialExcited = "Industrial_excited";
            public const string TTIndustrialGreeting = "Industrial_greeting";
            public const string TTIndustrialMouthClose = "Industrial_mouthClose";
            public const string TTIndustrialMouthOpen = "Industrial_mouthOpen";
            public const string TTIndustrialSad = "Industrial_sad";
            public const string TTIndustrialSleep01 = "Industrial_sleep_01";
            public const string TTIndustrialSleep02 = "Industrial_sleep_02";
            public const string TTIndustrialSleep03 = "Industrial_sleep_03";
            public const string TTCyborgChewing = "Cyborg_chewing";
            public const string TTCyborgExcited = "Cyborg_excited";
            public const string TTCyborgGreeting = "Cyborg_greeting";
            public const string TTCyborgMouthClose = "Cyborg_mouthClose";
            public const string TTCyborgMouthOpen = "Cyborg_mouthOpen";
            public const string TTCyborgSad = "Cyborg_sad";
            public const string TTCyborgSleep01 = "Cyborg_sleep01";
            public const string TTCyborgSleep02 = "Cyborg_sleep02";
            public const string TTCyborgSleep03 = "Cyborg_sleep03";
        }

        /// <summary>
        /// Music resource names.
        /// </summary>
        internal static class Music
        {
            public const string MenuMusic = "menu_music";
            public const string GameMusic = "game_music";
            public const string GameMusic2 = "game_music2";
            public const string GameMusic3 = "game_music3";
            public const string GameMusic4 = "game_music4";
            public const string GameMusic5 = "game_music5";
            public const string MenuMusicXmas = "menu_music_xmas";
            public const string GameMusicXmas = "game_music_xmas";
        }

        /// <summary>
        /// String table resource names.
        /// </summary>
        internal static class Str
        {
            public const string MenuStrings = "menu_strings";
        }
    }

    /// <summary>
    /// Configuration for a font including size, color, and effects.
    /// </summary>
    internal sealed class FontConfiguration
    {
        /// <summary>
        /// Gets or sets the font file name.
        /// </summary>
        public string FontFile { get; set; }

        /// <summary>
        /// Gets or sets the font size in pixels.
        /// </summary>
        public float Size { get; set; }

        /// <summary>
        /// Gets or sets the base text color.
        /// </summary>
        public Color Color { get; set; }

        /// <summary>
        /// Gets or sets stroke and shadow effects applied while drawing the font.
        /// </summary>
        public FontEffectSettings Effects { get; set; }

        /// <summary>
        /// Gets or sets the extra line spacing.
        /// </summary>
        public float LineSpacing { get; set; }

        /// <summary>
        /// Gets or sets the top spacing adjustment.
        /// </summary>
        public float TopSpacing { get; set; }
    }
}
