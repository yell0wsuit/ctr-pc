using System;
using System.Collections.Generic;

using CutTheRope.Framework;
using CutTheRope.Framework.Visual;

using Microsoft.Xna.Framework;

namespace CutTheRope.GameMain
{
    /// <summary>
    /// Centralized string constants for content assets grouped by type to simplify resource lookups.
    /// </summary>
    internal static class Resources
    {
        private static HashSet<string> soundNames_;
        private static HashSet<string> fontNames_;
        private static HashSet<string> imageNames_;

        /// <summary>
        /// Checks if a resource name is valid (exists in Resources.cs).
        /// </summary>
        public static bool IsValidResourceName(string resourceName)
        {
            if (imageNames_ == null)
            {
                InitializeImageNames();
            }
            if (soundNames_ == null)
            {
                InitializeSoundNames();
            }
            if (fontNames_ == null)
            {
                InitializeFontNames();
            }
            return imageNames_.Contains(resourceName) ||
                   soundNames_.Contains(resourceName) ||
                   fontNames_.Contains(resourceName);
        }

        /// <summary>
        /// Checks if a resource name is a sound.
        /// </summary>
        public static bool IsSound(string resourceName)
        {
            if (soundNames_ == null)
            {
                InitializeSoundNames();
            }
            return soundNames_.Contains(resourceName);
        }

        /// <summary>
        /// Checks if a resource name is a font.
        /// </summary>
        public static bool IsFont(string resourceName)
        {
            if (fontNames_ == null)
            {
                InitializeFontNames();
            }
            return fontNames_.Contains(resourceName);
        }

        /// <summary>
        /// List all of texture resources.
        /// </summary>
        private static void InitializeImageNames()
        {
            imageNames_ =
            [
                Img.ZeptolabNoLink, Img.LoaderbarFull, Img.MenuButtonDefault,
                Img.MenuLoading, Img.MenuNotification, Img.MenuAchievement,
                Img.MenuOptions, Img.MenuBgr, Img.MenuPopup, Img.MenuLogo,
                Img.MenuLogoNew, Img.CandySelectionFx, Img.SkinSelection, Img.MenuLevelSelection,
                Img.MenuPackSelection, Img.MenuPackSelection2,
                Img.MenuExtraButtons, Img.MenuScrollbar, Img.MenuLeaderboard,
                Img.MenuProcessingHd, Img.MenuScrollbarChangename, Img.MenuButtonAchivCup,
                Img.MenuBgrShadow, Img.MenuButtonShort, Img.HudButtons, Img.ObjCandy01New,
                Img.ObjCandyFx, Img.ObjSpider, Img.ConfettiParticles, Img.MenuPause, Img.MenuResult,
                Img.HudButtonsEn, Img.MenuResultEn, Img.ObjStarDisappear,
                Img.ObjBubbleFlight, Img.ObjBubblePop, Img.ObjHookAuto,
                Img.ObjBubbleAttached, Img.ObjHook01, Img.ObjHook02, Img.ObjStarIdle,
                Img.HudStar, Img.CharAnimations, Img.ObjHookRegulated, Img.ObjHookMovable,
                Img.ObjPump, Img.TutorialSigns, Img.ObjHat, Img.ObjBouncer01,
                Img.ObjBouncer02, Img.ObjSpikes01, Img.ObjSpikes02, Img.ObjSpikes03,
                Img.ObjSpikes04, Img.ObjElectrodes, Img.ObjRotatableSpikes01,
                Img.ObjRotatableSpikes02, Img.ObjRotatableSpikes03, Img.ObjRotatableSpikes04,
                Img.ObjRotatableSpikesButton, Img.ObjBeeHd, Img.ObjPollenHd,
                Img.CharSupports, Img.CharAnimations2, Img.CharAnimations3, Img.ObjVinil,
                Img.Bgr01P1, Img.Bgr01P2, Img.Bgr02P1, Img.Bgr02P2,
                Img.Bgr03P1, Img.Bgr03P2, Img.Bgr04P1, Img.Bgr04P2,
                Img.Bgr05P1, Img.Bgr05P2, Img.Bgr06P1, Img.Bgr06P2,
                Img.Bgr07P1, Img.Bgr07P2, Img.Bgr08P1, Img.Bgr08P2,
                Img.Bgr09P1, Img.Bgr09P2, Img.Bgr10P1, Img.Bgr10P2,
                Img.Bgr11P1, Img.Bgr11P2, Img.Bgr01Cover, Img.Bgr02Cover,
                Img.Bgr03Cover, Img.Bgr04Cover, Img.Bgr05Cover, Img.Bgr06Cover,
                Img.Bgr07Cover, Img.Bgr08Cover, Img.Bgr09Cover, Img.Bgr10Cover,
                Img.Bgr11Cover, Img.MenuExtraButtonsFr, Img.MenuExtraButtonsGr,
                Img.MenuExtraButtonsRu, Img.HudButtonsRu, Img.HudButtonsGr,
                Img.MenuResultRu, Img.MenuResultFr, Img.MenuResultGr,
                Img.MenuExtraButtonsEn, Img.Bgr12Cover, Img.Bgr12P1, Img.Bgr12P2,
                Img.ObjGhost, Img.Bgr13Cover, Img.Bgr13P1, Img.ObjPipe, Img.XmasLights,
                Img.Snowflakes, Img.CharGreetingXmas,
                Img.Bgr14Cover, Img.Bgr14P1, Img.ObjLantern,
                Img.CharIdleXmas, Img.MenuBgrXmas, Img.MenuBgrXmasSecondary, Img.MenuLogoXmasHat,
                Img.ObjCandy02, Img.ObjCandy03, Img.ObjCandy04, Img.ObjCandy05, Img.ObjCandy06,
                Img.ObjCandy07, Img.ObjCandy08, Img.ObjCandy09, Img.ObjCandy10, Img.ObjCandy11,
                Img.ObjCandy12, Img.ObjCandy13, Img.ObjCandy14, Img.ObjCandy15, Img.ObjCandy16,
                Img.ObjCandy17, Img.ObjCandy18, Img.ObjCandy19, Img.ObjCandy20, Img.ObjCandy21,
                Img.ObjCandy22, Img.ObjCandy23, Img.ObjCandy24, Img.ObjCandy25, Img.ObjCandy26,
                Img.ObjCandy27, Img.ObjCandy28, Img.ObjCandy29, Img.ObjCandy30, Img.ObjCandy31,
                Img.ObjCandy32, Img.ObjCandy33, Img.ObjCandy34, Img.ObjCandy35, Img.ObjCandy36,
                Img.ObjCandy37, Img.ObjCandy38, Img.ObjCandy39, Img.ObjCandy40, Img.ObjCandy41,
                Img.ObjCandy42, Img.ObjCandy43, Img.ObjCandy44, Img.ObjCandy45, Img.ObjCandy46,
                Img.ObjCandy47, Img.ObjCandy48, Img.ObjCandy49, Img.ObjCandy50, Img.ObjCandy51
            ];
        }

        /// <summary>
        /// List all of audio resources.
        /// </summary>
        private static void InitializeSoundNames()
        {
            soundNames_ =
            [
                Snd.Tap, Snd.Button, Snd.BubbleBreak, Snd.Bubble, Snd.CandyBreak,
                Snd.MonsterChewing, Snd.MonsterClose, Snd.MonsterOpen, Snd.MonsterSad,
                Snd.Ring, Snd.RopeBleak1, Snd.RopeBleak2, Snd.RopeBleak3, Snd.RopeBleak4,
                Snd.RopeGet, Snd.Star1, Snd.Star2, Snd.Star3, Snd.Electric,
                Snd.Pump1, Snd.Pump2, Snd.Pump3, Snd.Pump4, Snd.SpiderActivate,
                Snd.SpiderFall, Snd.SpiderWin, Snd.Wheel, Snd.Win, Snd.GravityOff,
                Snd.GravityOn, Snd.CandyLink, Snd.Bouncer, Snd.SpikeRotateIn,
                Snd.SpikeRotateOut, Snd.Buzz, Snd.Teleport, Snd.ScratchIn,
                Snd.ScratchOut, Snd.MenuMusic, Snd.GameMusic, Snd.GameMusic2,
                Snd.GameMusic3, Snd.GameMusic4, Snd.GhostPuff, Snd.XmasBell,
                Snd.MenuMusicXmas, Snd.GameMusicXmas, Snd.SteamStart,
                Snd.SteamStart2, Snd.SteamEnd, Snd.LanternTeleportIn,
                Snd.LanternTeleportOut
            ];
        }

        /// <summary>
        /// List all of font resources.
        /// </summary>
        private static void InitializeFontNames()
        {
            fontNames_ =
            [
                Fnt.BigFont, Fnt.SmallFont, Fnt.FontNumbersBig
            ];
        }
        /// <summary>
        /// Image and atlas resource names.
        /// </summary>
        internal static class Img
        {
            public const string ZeptolabNoLink = "zeptolab_no_link";
            public const string LoaderbarFull = "loaderbar_full";
            public const string MenuButtonDefault = "menu_button_default";
            public const string MenuLoading = "menu_loading";
            public const string MenuNotification = "menu_notification";
            public const string MenuAchievement = "menu_achievement";
            public const string MenuOptions = "menu_options";
            public const string MenuBgr = "menu_bgr";
            public const string MenuPopup = "menu_popup";
            public const string MenuLogo = "menu_logo";
            public const string MenuLogoNew = "menu_logo_new";
            public const string CandySelectionFx = "candy_selection_fx";
            public const string SkinSelection = "skin_selection";
            public const string SkinBackground = "skin_bg";
            public const string MenuLevelSelection = "menu_level_selection";
            public const string MenuPackSelection = "menu_pack_selection";
            public const string MenuPackSelection2 = "menu_pack_selection2";
            public const string MenuExtraButtons = "menu_extra_buttons";
            public const string MenuScrollbar = "menu_scrollbar";
            public const string MenuLeaderboard = "menu_leaderboard";
            public const string MenuProcessingHd = "menu_processing_hd";
            public const string MenuScrollbarChangename = "menu_scrollbar_changename";
            public const string MenuButtonAchivCup = "menu_button_achiv_cup";
            public const string MenuBgrShadow = "menu_bgr_shadow";
            public const string MenuButtonShort = "menu_button_short";
            public const string HudButtons = "hud_buttons";
            public const string ObjSpider = "obj_spider";
            public const string ConfettiParticles = "confetti_particles";
            public const string MenuPause = "menu_pause";
            public const string MenuResult = "menu_result";
            public const string HudButtonsEn = "hud_buttons_en";
            public const string MenuResultEn = "menu_result_en";
            public const string ObjStarDisappear = "obj_star_disappear";
            public const string ObjBubbleFlight = "obj_bubble_flight";
            public const string ObjBubblePop = "obj_bubble_pop";
            public const string ObjHookAuto = "obj_hook_auto";
            public const string ObjBubbleAttached = "obj_bubble_attached";
            public const string ObjHook01 = "obj_hook_01";
            public const string ObjHook02 = "obj_hook_02";
            public const string ObjStarIdle = "obj_star_idle";
            public const string HudStar = "hud_star";
            public const string CharAnimations = "char_animations";
            public const string ObjHookRegulated = "obj_hook_regulated";
            public const string ObjHookMovable = "obj_hook_movable";
            public const string ObjPump = "obj_pump";
            public const string TutorialSigns = "tutorial_signs";
            public const string ObjHat = "obj_hat";
            public const string ObjBouncer01 = "obj_bouncer_01";
            public const string ObjBouncer02 = "obj_bouncer_02";
            public const string ObjSpikes01 = "obj_spikes_01";
            public const string ObjSpikes02 = "obj_spikes_02";
            public const string ObjSpikes03 = "obj_spikes_03";
            public const string ObjSpikes04 = "obj_spikes_04";
            public const string ObjElectrodes = "obj_electrodes";
            public const string ObjRotatableSpikes01 = "obj_rotatable_spikes_01";
            public const string ObjRotatableSpikes02 = "obj_rotatable_spikes_02";
            public const string ObjRotatableSpikes03 = "obj_rotatable_spikes_03";
            public const string ObjRotatableSpikes04 = "obj_rotatable_spikes_04";
            public const string ObjRotatableSpikesButton = "obj_rotatable_spikes_button";
            public const string ObjBeeHd = "obj_bee_hd";
            public const string ObjPollenHd = "obj_pollen_hd";
            public const string CharSupports = "char_supports";
            public const string CharAnimations2 = "char_animations2";
            public const string CharAnimations3 = "char_animations3";
            public const string ObjVinil = "obj_vinil";
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
            public const string HudButtonsRu = "hud_buttons_ru";
            public const string HudButtonsGr = "hud_buttons_gr";
            public const string MenuResultRu = "menu_result_ru";
            public const string MenuResultFr = "menu_result_fr";
            public const string MenuResultGr = "menu_result_gr";
            public const string MenuExtraButtonsEn = "menu_extra_buttons_en";
            public const string Bgr12Cover = "bgr_12_cover";
            public const string Bgr12P1 = "bgr_12_p1";
            public const string Bgr12P2 = "bgr_12_p2";
            public const string ObjGhost = "obj_ghost";
            public const string XmasLights = "christmas_lights";
            public const string Snowflakes = "snowflakes";
            public const string CharGreetingXmas = "char_greeting_xmas";
            public const string CharIdleXmas = "char_idle_xmas";
            public const string ObjSock = "obj_sock_xmas";
            public const string MenuBgrXmas = "menu_xmas_pc";
            public const string MenuBgrXmasSecondary = "menu_xmas_secondary_pc";
            public const string MenuLogoXmasHat = "xmas_hat_logo";
            public const string Bgr13Cover = "bgr_13_cover";
            public const string Bgr13P1 = "bgr_13_p1";
            public const string ObjPipe = "obj_pipe";
            public const string Bgr14Cover = "bgr_14_cover";
            public const string Bgr14P1 = "bgr_14_p1";
            public const string ObjLantern = "obj_lantern";

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
            private const string StandardFont = "gooddog_new-webfont.ttf";
            private const string ExtendedFont = "PlaypenSans-SemiBold.ttf";

            public static FontConfiguration GetConfiguration(string fontName, int language)
            {
                return fontName switch
                {
                    Fnt.BigFont => new FontConfiguration
                    {
                        FontFile = language == (int)Language.LANGRU ? ExtendedFont : StandardFont,
                        Size = 100f,
                        Color = Color.White,
                        Effects = FontEffectSettings.CreateStrokeAndShadow(2, 2, 3),
                        LineSpacing = 5f,
                        TopSpacing = -10f
                    },
                    Fnt.SmallFont => new FontConfiguration
                    {
                        FontFile = language == (int)Language.LANGRU ? ExtendedFont : StandardFont,
                        Size = 72f,
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
        /// Sound effect and music resource names.
        /// </summary>
        internal static class Snd
        {
            public const string Tap = "tap";
            public const string Button = "button";
            public const string BubbleBreak = "bubble_break";
            public const string Bubble = "bubble";
            public const string CandyBreak = "candy_break";
            public const string MonsterChewing = "monster_chewing";
            public const string MonsterClose = "monster_close";
            public const string MonsterOpen = "monster_open";
            public const string MonsterSad = "monster_sad";
            public const string Ring = "ring";
            public const string RopeBleak1 = "rope_bleak_1";
            public const string RopeBleak2 = "rope_bleak_2";
            public const string RopeBleak3 = "rope_bleak_3";
            public const string RopeBleak4 = "rope_bleak_4";
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
            public const string MenuMusic = "menu_music";
            public const string GameMusic = "game_music";
            public const string GameMusic2 = "game_music2";
            public const string GameMusic3 = "game_music3";
            public const string GameMusic4 = "game_music4";
            public const string GhostPuff = "ghost_puff";
            public const string XmasBell = "xmas_bell";
            public const string MenuMusicXmas = "menu_music_xmas";
            public const string GameMusicXmas = "game_music_xmas";
            public const string TeleportXmas = "teleport_xmas";
            public const string SteamStart = "steam_start";
            public const string SteamStart2 = "steam_start2";
            public const string SteamEnd = "steam_end";
            public const string LanternTeleportIn = "lantern_teleport_in";
            public const string LanternTeleportOut = "lantern_teleport_out";
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
        public string FontFile { get; set; }
        public float Size { get; set; }
        public Color Color { get; set; }
        public FontEffectSettings Effects { get; set; }
        public float LineSpacing { get; set; }
        public float TopSpacing { get; set; }
    }
}
