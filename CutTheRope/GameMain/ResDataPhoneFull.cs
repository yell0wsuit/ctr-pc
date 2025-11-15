using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Xml.Linq;

using CutTheRope.Framework;

using Microsoft.Xna.Framework;

namespace CutTheRope.GameMain
{
    internal class ResDataPhoneFull
    {
        private const string ResourceDataFileName = "res_data_phone_full.xml";

        public static string GetXml(string resName)
        {
            if (string.IsNullOrEmpty(resName))
            {
                return null;
            }

            EnsureXmlsLoaded();

            _ = xmls_.TryGetValue(resName, out string value);
            return value;
        }

        private static void EnsureXmlsLoaded()
        {
            if (xmls_ != null)
            {
                return;
            }

            lock (xmlsLock_)
            {
                xmls_ ??= LoadXmlDefinitions();
            }
        }

        private static Dictionary<string, string> LoadXmlDefinitions()
        {
            Dictionary<string, string> result = [];

            try
            {
                using Stream stream = OpenResourceDataStream();
                if (stream == null)
                {
                    return result;
                }

                XDocument document = XDocument.Load(stream);
                XElement root = document.Root;
                if (root == null)
                {
                    return result;
                }

                foreach (XElement resourceNode in root.Elements("resource"))
                {
                    string name = resourceNode.Attribute("name")?.Value;
                    if (string.IsNullOrEmpty(name))
                    {
                        continue;
                    }

                    XElement[] dataNodes = [.. resourceNode.Elements()];
                    if (dataNodes.Length == 0)
                    {
                        continue;
                    }

                    string xml = string.Concat(dataNodes.Select(node => node.ToString(SaveOptions.DisableFormatting)));
                    if (!string.IsNullOrEmpty(xml))
                    {
                        result[name] = xml;
                    }
                }
            }
            catch (Exception)
            {
            }

            return result;
        }

        private static Stream OpenResourceDataStream()
        {
            string[] candidates = string.IsNullOrEmpty(ContentFolder)
                ? [ResourceDataFileName]
                : [$"{ContentFolder}{ResourceDataFileName}", ResourceDataFileName];

            foreach (string candidate in candidates)
            {
                try
                {
                    return TitleContainer.OpenStream($"content/{candidate}");
                }
                catch (Exception)
                {
                }
            }

            return null;
        }

        public static bool IsSound(int id)
        {
            return id switch
            {
                9 or 11 or 12 or 13 or 14 or 15 or 16 or 17 or 18 or 19 or 20 or 21 or 22 or 23 or 24 or 25 or 26 or 27 or 28 or 29 or 30 or 31 or 32 or 33 or 34 or 35 or 36 or 37 or 38 or 39 or 40 or 41 or 42 or 43 or 44 or 45 or 46 or 47 => true,
                10 => false,
                _ => id is >= 145 and <= 148,
            };
        }

        public static bool IsFont(int id)
        {
            return id is 3 or 4 or 68;
        }

        public const int IMG_DEFAULT = 0;

        public const int IMG_LOADERBAR_FULL = 1;

        public const int IMG_MENU_BUTTON_DEFAULT = 2;

        public const int FNT_BIG_FONT = 3;

        public const int FNT_SMALL_FONT = 4;

        public const int IMG_MENU_LOADING = 5;

        public const int IMG_MENU_NOTIFICATION = 6;

        public const int IMG_MENU_ACHIEVEMENT = 7;

        public const int IMG_MENU_OPTIONS = 8;

        public const int SND_TAP = 9;

        public const int STR_MENU = 10;

        public const int SND_BUTTON = 11;

        public const int SND_BUBBLE_BREAK = 12;

        public const int SND_BUBBLE = 13;

        public const int SND_CANDY_BREAK = 14;

        public const int SND_MONSTER_CHEWING = 15;

        public const int SND_MONSTER_CLOSE = 16;

        public const int SND_MONSTER_OPEN = 17;

        public const int SND_MONSTER_SAD = 18;

        public const int SND_RING = 19;

        public const int SND_ROPE_BLEAK_1 = 20;

        public const int SND_ROPE_BLEAK_2 = 21;

        public const int SND_ROPE_BLEAK_3 = 22;

        public const int SND_ROPE_BLEAK_4 = 23;

        public const int SND_ROPE_GET = 24;

        public const int SND_STAR_1 = 25;

        public const int SND_STAR_2 = 26;

        public const int SND_STAR_3 = 27;

        public const int SND_ELECTRIC = 28;

        public const int SND_PUMP_1 = 29;

        public const int SND_PUMP_2 = 30;

        public const int SND_PUMP_3 = 31;

        public const int SND_PUMP_4 = 32;

        public const int SND_SPIDER_ACTIVATE = 33;

        public const int SND_SPIDER_FALL = 34;

        public const int SND_SPIDER_WIN = 35;

        public const int SND_WHEEL = 36;

        public const int SND_WIN = 37;

        public const int SND_GRAVITY_OFF = 38;

        public const int SND_GRAVITY_ON = 39;

        public const int SND_CANDY_LINK = 40;

        public const int SND_BOUNCER = 41;

        public const int SND_SPIKE_ROTATE_IN = 42;

        public const int SND_SPIKE_ROTATE_OUT = 43;

        public const int SND_BUZZ = 44;

        public const int SND_TELEPORT = 45;

        public const int SND_SCRATCH_IN = 46;

        public const int SND_SCRATCH_OUT = 47;

        public const int IMG_MENU_BGR = 48;

        public const int IMG_MENU_POPUP = 49;

        public const int IMG_MENU_LOGO = 50;

        public const int IMG_MENU_LEVEL_SELECTION = 51;

        public const int IMG_MENU_PACK_SELECTION = 52;

        public const int IMG_MENU_PACK_SELECTION2 = 53;

        public const int IMG_MENU_EXTRA_BUTTONS = 54;

        public const int IMG_MENU_SCROLLBAR = 55;

        public const int IMG_MENU_LEADERBOARD = 56;

        public const int IMG_MENU_PROCESSING = 57;

        public const int IMG_MENU_SCROLLBAR_CHANGENAME = 58;

        public const int IMG_MENU_BUTTON_ACHIV_CUP = 59;

        public const int IMG_MENU_BGR_SHADOW = 60;

        public const int IMG_MENU_BUTTON_SHORT = 61;

        public const int IMG_HUD_BUTTONS = 62;

        public const int IMG_OBJ_CANDY_01 = 63;

        public const int IMG_OBJ_SPIDER = 64;

        public const int IMG_CONFETTI_PARTICLES = 65;

        public const int IMG_MENU_PAUSE = 66;

        public const int IMG_MENU_RESULT = 67;

        public const int FNT_FONT_NUMBERS_BIG = 68;

        public const int IMG_HUD_BUTTONS_EN = 69;

        public const int IMG_MENU_RESULT_EN = 70;

        public const int IMG_OBJ_STAR_DISAPPEAR = 71;

        public const int IMG_OBJ_BUBBLE_FLIGHT = 72;

        public const int IMG_OBJ_BUBBLE_POP = 73;

        public const int IMG_OBJ_HOOK_AUTO = 74;

        public const int IMG_OBJ_BUBBLE_ATTACHED = 75;

        public const int IMG_OBJ_HOOK_01 = 76;

        public const int IMG_OBJ_HOOK_02 = 77;

        public const int IMG_OBJ_STAR_IDLE = 78;

        public const int IMG_HUD_STAR = 79;

        public const int IMG_CHAR_ANIMATIONS = 80;

        public const int IMG_OBJ_HOOK_REGULATED = 81;

        public const int IMG_OBJ_HOOK_MOVABLE = 82;

        public const int IMG_OBJ_PUMP = 83;

        public const int IMG_TUTORIAL_SIGNS = 84;

        public const int IMG_OBJ_SOCKS = 85;

        public const int IMG_OBJ_BOUNCER_01 = 86;

        public const int IMG_OBJ_BOUNCER_02 = 87;

        public const int IMG_OBJ_SPIKES_01 = 88;

        public const int IMG_OBJ_SPIKES_02 = 89;

        public const int IMG_OBJ_SPIKES_03 = 90;

        public const int IMG_OBJ_SPIKES_04 = 91;

        public const int IMG_OBJ_ELECTRODES = 92;

        public const int IMG_OBJ_ROTATABLE_SPIKES_01 = 93;

        public const int IMG_OBJ_ROTATABLE_SPIKES_02 = 94;

        public const int IMG_OBJ_ROTATABLE_SPIKES_03 = 95;

        public const int IMG_OBJ_ROTATABLE_SPIKES_04 = 96;

        public const int IMG_OBJ_ROTATABLE_SPIKES_BUTTON = 97;

        public const int IMG_OBJ_BEE_HD = 98;

        public const int IMG_OBJ_POLLEN_HD = 99;

        public const int IMG_CHAR_SUPPORTS = 100;

        public const int IMG_CHAR_ANIMATIONS2 = 101;

        public const int IMG_CHAR_ANIMATIONS3 = 102;

        public const int IMG_OBJ_VINIL = 103;

        public const int IMG_BGR_01_P1 = 104;

        public const int IMG_BGR_01_P2 = 105;

        public const int IMG_BGR_02_P1 = 106;

        public const int IMG_BGR_02_P2 = 107;

        public const int IMG_BGR_03_P1 = 108;

        public const int IMG_BGR_03_P2 = 109;

        public const int IMG_BGR_04_P1 = 110;

        public const int IMG_BGR_04_P2 = 111;

        public const int IMG_BGR_05_P1 = 112;

        public const int IMG_BGR_05_P2 = 113;

        public const int IMG_BGR_06_P1 = 114;

        public const int IMG_BGR_06_P2 = 115;

        public const int IMG_BGR_07_P1 = 116;

        public const int IMG_BGR_07_P2 = 117;

        public const int IMG_BGR_08_P1 = 118;

        public const int IMG_BGR_08_P2 = 119;

        public const int IMG_BGR_09_P1 = 120;

        public const int IMG_BGR_09_P2 = 121;

        public const int IMG_BGR_10_P1 = 122;

        public const int IMG_BGR_10_P2 = 123;

        public const int IMG_BGR_11_P1 = 124;

        public const int IMG_BGR_11_P2 = 125;

        public const int IMG_BGR_COVER_01 = 126;

        public const int IMG_BGR_COVER_02 = 127;

        public const int IMG_BGR_COVER_03 = 128;

        public const int IMG_BGR_COVER_04 = 129;

        public const int IMG_BGR_COVER_05 = 130;

        public const int IMG_BGR_COVER_06 = 131;

        public const int IMG_BGR_COVER_07 = 132;

        public const int IMG_BGR_COVER_08 = 133;

        public const int IMG_BGR_COVER_09 = 134;

        public const int IMG_BGR_COVER_10 = 135;

        public const int IMG_BGR_COVER_11 = 136;

        public const int IMG_MENU_EXTRA_BUTTONS_FR = 137;

        public const int IMG_MENU_EXTRA_BUTTONS_GR = 138;

        public const int IMG_MENU_EXTRA_BUTTONS_RU = 139;

        public const int IMG_HUD_BUTTONS_RU = 140;

        public const int IMG_HUD_BUTTONS_GR = 141;

        public const int IMG_MENU_RESULT_RU = 142;

        public const int IMG_MENU_RESULT_FR = 143;

        public const int IMG_MENU_RESULT_GR = 144;

        public const int SND_MENU_MUSIC = 145;

        public const int SND_GAME_MUSIC = 146;

        public const int SND_GAME_MUSIC2 = 147;

        public const int SND_GAME_MUSIC3 = 148;

        public const int IMG_MENU_EXTRA_BUTTONS_EN = 149;

        public const int RESOURCES_COUNT = 150;

        public const int SOUND_FIRST = 9;

        public const int SOUND_LAST = 47;

        internal const int IMG_MENU_BUTTON_DEFAULT_default_idle = 0;

        internal const int IMG_MENU_BUTTON_DEFAULT_default_pressed = 1;

        internal const int IMG_MENU_LOADING_tape_left = 0;

        internal const int IMG_MENU_LOADING_tape_right = 1;

        internal const int IMG_MENU_LOADING_scotch = 2;

        internal const int IMG_MENU_LOADING_knife = 3;

        internal const int IMG_MENU_NOTIFICATION_body = 0;

        internal const int IMG_MENU_NOTIFICATION__text = 1;

        internal const int IMG_MENU_NOTIFICATION__unlocked = 2;

        internal const int IMG_MENU_NOTIFICATION__achievement = 3;

        internal const int IMG_MENU_NOTIFICATION__name = 4;

        internal const int IMG_MENU_ACHIEVEMENT_achievement_bronze_scissors = 0;

        internal const int IMG_MENU_ACHIEVEMENT_achievement_silver_scissors = 1;

        internal const int IMG_MENU_ACHIEVEMENT_achievement_golden_scissors = 2;

        internal const int IMG_MENU_ACHIEVEMENT_achievement_rope_cutter = 3;

        internal const int IMG_MENU_ACHIEVEMENT_achievement_rope_cutter_maniac = 4;

        internal const int IMG_MENU_ACHIEVEMENT_achievement_bubble_popper = 5;

        internal const int IMG_MENU_ACHIEVEMENT_achievement_spider_buster = 6;

        internal const int IMG_MENU_ACHIEVEMENT_achievement_weight_loser = 7;

        internal const int IMG_MENU_ACHIEVEMENT_achievement_quick_finger = 8;

        internal const int IMG_MENU_ACHIEVEMENT_achievement_master_finger = 9;

        internal const int IMG_MENU_ACHIEVEMENT_achievement_cardboard_box = 10;

        internal const int IMG_MENU_ACHIEVEMENT_achievement_fabric_box = 11;

        internal const int IMG_MENU_ACHIEVEMENT_achievement_foil_box = 12;

        internal const int IMG_MENU_ACHIEVEMENT_achievement_gift_box = 13;

        internal const int IMG_MENU_ACHIEVEMENT_achievement_cosmic_box = 14;

        internal const int IMG_MENU_ACHIEVEMENT_achievement_valentine_box = 15;

        internal const int IMG_MENU_ACHIEVEMENT_achievement_magic_box = 16;

        internal const int IMG_MENU_ACHIEVEMENT_achievement_toy_box = 17;

        internal const int IMG_MENU_ACHIEVEMENT_achievement_tool_box = 18;

        internal const int IMG_MENU_ACHIEVEMENT_achievement_buzz_box = 19;

        internal const int IMG_MENU_ACHIEVEMENT_achievement_dj_box = 20;

        internal const int IMG_MENU_ACHIEVEMENT_achievement_cardboard_box_perfe = 21;

        internal const int IMG_MENU_ACHIEVEMENT_achievement_fabric_box_perfect = 22;

        internal const int IMG_MENU_ACHIEVEMENT_achievement_foil_box_perfect = 23;

        internal const int IMG_MENU_ACHIEVEMENT_achievement_magic_box_perfect = 24;

        internal const int IMG_MENU_ACHIEVEMENT_achievement_cosmic_box_perfect = 25;

        internal const int IMG_MENU_ACHIEVEMENT_achievement_valentine_box_perfe = 26;

        internal const int IMG_MENU_ACHIEVEMENT_achievement_gift_box_perfect = 27;

        internal const int IMG_MENU_ACHIEVEMENT_achievement_toy_box_perfect = 28;

        internal const int IMG_MENU_ACHIEVEMENT_achievement_tool_box_perfect = 29;

        internal const int IMG_MENU_ACHIEVEMENT_achievement_buzz_box_perfect = 30;

        internal const int IMG_MENU_ACHIEVEMENT_achievement_dj_box_perfect = 31;

        internal const int IMG_MENU_ACHIEVEMENT_achievement_ultimate_rope_cutte = 32;

        internal const int IMG_MENU_ACHIEVEMENT_achievement_bubble_master = 33;

        internal const int IMG_MENU_ACHIEVEMENT_achievement_spider_tamer = 34;

        internal const int IMG_MENU_ACHIEVEMENT_achievement_calorie_minimizer = 35;

        internal const int IMG_MENU_ACHIEVEMENT_achievement_tummy_teaser = 36;

        internal const int IMG_MENU_ACHIEVEMENT_achievement_romantic_soul = 37;

        internal const int IMG_MENU_ACHIEVEMENT_achievement_magician = 38;

        internal const int IMG_MENU_ACHIEVEMENT_achievement_spider_lover = 39;

        internal const int IMG_MENU_ACHIEVEMENT_achievement_candy_juggler = 40;

        internal const int IMG_MENU_ACHIEVEMENT_empty_achievement = 41;

        internal const int IMG_MENU_ACHIEVEMENT__scrollbar = 42;

        internal const int IMG_MENU_ACHIEVEMENT__ach_description = 43;

        internal const int IMG_MENU_ACHIEVEMENT__ach_name = 44;

        internal const int IMG_MENU_ACHIEVEMENT__block_spacing = 45;

        internal const int IMG_MENU_ACHIEVEMENT__container_size = 46;

        internal const int IMG_MENU_ACHIEVEMENT__container = 47;

        internal const int IMG_MENU_OPTIONS_short_button_idle = 0;

        internal const int IMG_MENU_OPTIONS_short_button_pressed = 1;

        internal const int IMG_MENU_OPTIONS_sound_ico = 2;

        internal const int IMG_MENU_OPTIONS_music_ico = 3;

        internal const int IMG_MENU_OPTIONS_cross = 4;

        internal const int IMG_MENU_OPTIONS_image_drag_to_cut = 5;

        internal const int IMG_MENU_OPTIONS_image_click_to_cut = 6;

        internal const int IMG_MENU_OPTIONS_check_left = 7;

        internal const int IMG_MENU_OPTIONS_check_right = 8;

        internal const int IMG_MENU_OPTIONS__title_click_to_cut = 9;

        internal const int IMG_MENU_OPTIONS__title_drag_to_cut = 10;

        internal const int IMG_MENU_OPTIONS__cross_sound = 11;

        internal const int IMG_MENU_OPTIONS__short_button_left = 12;

        internal const int STR_MENU_ABOUT_SPECIAL_THANKS = 655417;

        internal const int STR_MENU_ABOUT_TEXT = 655416;

        internal const int STR_MENU_ACHIEVEMENT_GAINED = 655396;

        internal const int STR_MENU_ACHIEVEMENT_UNLOCKED = 655504;

        internal const int STR_MENU_QUIT = 655505;

        internal const int STR_MENU_QUIT_BUTTON = 655506;

        internal const int STR_MENU_FACEBOOK_BUTTON = 655507;

        internal const int STR_MENU_AC_BRONZE_SCISSORS = 655426;

        internal const int STR_MENU_AC_BRONZE_SCISSORS_DESCR = 655443;

        internal const int STR_MENU_AC_BUBBLE_MASTER = 655433;

        internal const int STR_MENU_AC_BUBBLE_MASTER_DESCR = 655450;

        internal const int STR_MENU_AC_BUBBLE_POPPER = 655432;

        internal const int STR_MENU_AC_BUBBLE_POPPER_DESCR = 655449;

        internal const int STR_MENU_AC_BUZZ_BOX_COMPLETED = 655469;

        internal const int STR_MENU_AC_BUZZ_BOX_DESCR = 655491;

        internal const int STR_MENU_AC_BUZZ_BOX_PERFECT = 655480;

        internal const int STR_MENU_AC_BUZZ_BOX_PERFECT_DESCR = 655502;

        internal const int STR_MENU_AC_CALORIE_MINIMIZER = 655437;

        internal const int STR_MENU_AC_CALORIE_MINIMIZER_DESCR = 655454;

        internal const int STR_MENU_AC_CARDBOARD_BOX_COMPLETED = 655460;

        internal const int STR_MENU_AC_CARDBOARD_BOX_DESCR = 655482;

        internal const int STR_MENU_AC_CARDBOARD_BOX_PERFECT = 655471;

        internal const int STR_MENU_AC_CARDBOARD_BOX_PERFECT_DESCR = 655493;

        internal const int STR_MENU_AC_COSMIC_BOX_COMPLETED = 655467;

        internal const int STR_MENU_AC_COSMIC_BOX_DESCR = 655489;

        internal const int STR_MENU_AC_COSMIC_BOX_PERFECT = 655478;

        internal const int STR_MENU_AC_COSMIC_BOX_PERFECT_DESCR = 655500;

        internal const int STR_MENU_AC_DJ_BOX_COMPLETED = 655470;

        internal const int STR_MENU_AC_DJ_BOX_DESCR = 655492;

        internal const int STR_MENU_AC_DJ_BOX_PERFECT = 655481;

        internal const int STR_MENU_AC_DJ_BOX_PERFECT_DESCR = 655503;

        internal const int STR_MENU_AC_FABRIC_BOX_COMPLETED = 655461;

        internal const int STR_MENU_AC_FABRIC_BOX_DESCR = 655483;

        internal const int STR_MENU_AC_FABRIC_BOX_PERFECT = 655472;

        internal const int STR_MENU_AC_FABRIC_BOX_PERFECT_DESCR = 655494;

        internal const int STR_MENU_AC_FOIL_BOX_COMPLETED = 655462;

        internal const int STR_MENU_AC_FOIL_BOX_DESCR = 655484;

        internal const int STR_MENU_AC_FOIL_BOX_PERFECT = 655473;

        internal const int STR_MENU_AC_FOIL_BOX_PERFECT_DESCR = 655495;

        internal const int STR_MENU_AC_GIFT_BOX_COMPLETED = 655466;

        internal const int STR_MENU_AC_GIFT_BOX_DESCR = 655488;

        internal const int STR_MENU_AC_GIFT_BOX_PERFECT = 655477;

        internal const int STR_MENU_AC_GIFT_BOX_PERFECT_DESCR = 655499;

        internal const int STR_MENU_AC_GOLDEN_SCISSORS = 655428;

        internal const int STR_MENU_AC_GOLDEN_SCISSORS_DESCR = 655445;

        internal const int STR_MENU_AC_MAGICIAN = 655442;

        internal const int STR_MENU_AC_MAGICIAN_DESCR = 655459;

        internal const int STR_MENU_AC_MAGIC_BOX_COMPLETED = 655463;

        internal const int STR_MENU_AC_MAGIC_BOX_DESCR = 655485;

        internal const int STR_MENU_AC_MAGIC_BOX_PERFECT = 655474;

        internal const int STR_MENU_AC_MAGIC_BOX_PERFECT_DESCR = 655496;

        internal const int STR_MENU_AC_MASTER_FINGER = 655439;

        internal const int STR_MENU_AC_MASTER_FINGER_DESCR = 655456;

        internal const int STR_MENU_AC_QUICK_FINGER = 655438;

        internal const int STR_MENU_AC_QUICK_FINGER_DESCR = 655455;

        internal const int STR_MENU_AC_ROMANTIC_SOUL = 655441;

        internal const int STR_MENU_AC_ROMANTIC_SOUL_DESCR = 655458;

        internal const int STR_MENU_AC_ROPE_CUTTER = 655429;

        internal const int STR_MENU_AC_ROPE_CUTTER_DESCR = 655446;

        internal const int STR_MENU_AC_ROPE_CUTTER_MANIAC = 655430;

        internal const int STR_MENU_AC_ROPE_CUTTER_MANIAC_DESCR = 655447;

        internal const int STR_MENU_AC_SILVER_SCISSORS = 655427;

        internal const int STR_MENU_AC_SILVER_SCISSORS_DESCR = 655444;

        internal const int STR_MENU_AC_SPIDER_BUSTER = 655434;

        internal const int STR_MENU_AC_SPIDER_BUSTER_DESCR = 655451;

        internal const int STR_MENU_AC_SPIDER_TAMER = 655435;

        internal const int STR_MENU_AC_SPIDER_TAMER_DESCR = 655452;

        internal const int STR_MENU_AC_TOOLBOX_COMPLETED = 655468;

        internal const int STR_MENU_AC_TOOLBOX_DESCR = 655490;

        internal const int STR_MENU_AC_TOOLBOX_PERFECT = 655479;

        internal const int STR_MENU_AC_TOOLBOX_PERFECT_DESCR = 655501;

        internal const int STR_MENU_AC_TOY_BOX_COMPLETED = 655465;

        internal const int STR_MENU_AC_TOY_BOX_DESCR = 655487;

        internal const int STR_MENU_AC_TOY_BOX_PERFECT = 655476;

        internal const int STR_MENU_AC_TOY_BOX_PERFECT_DESCR = 655498;

        internal const int STR_MENU_AC_TUMMY_TEASER = 655440;

        internal const int STR_MENU_AC_TUMMY_TEASER_DESCR = 655457;

        internal const int STR_MENU_AC_ULTIMATE_ROPE_CUTTER = 655431;

        internal const int STR_MENU_AC_ULTIMATE_ROPE_CUTTER_DESCR = 655448;

        internal const int STR_MENU_AC_VALENTINE_BOX_COMPLETED = 655464;

        internal const int STR_MENU_AC_VALENTINE_BOX_DESCR = 655486;

        internal const int STR_MENU_AC_VALENTINE_BOX_PERFECT = 655475;

        internal const int STR_MENU_AC_VALENTINE_BOX_PERFECT_DESCR = 655497;

        internal const int STR_MENU_AC_WEIGHT_LOSER = 655436;

        internal const int STR_MENU_AC_WEIGHT_LOSER_DESCR = 655453;

        internal const int STR_MENU_BEST_SCORE = 655380;

        internal const int STR_MENU_BOX10_LABEL = 655413;

        internal const int STR_MENU_BOX11_LABEL = 655414;

        internal const int STR_MENU_BOX1_LABEL = 655404;

        internal const int STR_MENU_BOX2_LABEL = 655405;

        internal const int STR_MENU_BOX3_LABEL = 655406;

        internal const int STR_MENU_BOX4_LABEL = 655407;

        internal const int STR_MENU_BOX5_LABEL = 655408;

        internal const int STR_MENU_BOX6_LABEL = 655409;

        internal const int STR_MENU_BOX7_LABEL = 655410;

        internal const int STR_MENU_BOX8_LABEL = 655411;

        internal const int STR_MENU_BOX9_LABEL = 655412;

        internal const int STR_MENU_BOX_SOON_LABEL = 655415;

        internal const int STR_MENU_CANCEL = 655403;

        internal const int STR_MENU_CANT_UNLOCK_TEXT1 = 655391;

        internal const int STR_MENU_CANT_UNLOCK_TEXT2 = 655392;

        internal const int STR_MENU_CANT_UNLOCK_TEXT3 = 655393;

        internal const int STR_MENU_CHANGE_TITLE = 655424;

        internal const int STR_MENU_CLICK_TO_CUT = 655422;

        internal const int STR_MENU_CONTINUE = 655397;

        internal const int STR_MENU_CREDITS = 655365;

        internal const int STR_MENU_CRYSTAL = 655366;

        internal const int STR_MENU_DRAG_TO_CUT = 655423;

        internal const int STR_MENU_EXTRAS = 655362;

        internal const int STR_MENU_FINAL_SCORE = 655379;

        internal const int STR_MENU_FULL_VERSION = 655363;

        internal const int STR_MENU_GAME_FINISHED_TEXT = 655394;

        internal const int STR_MENU_GAME_FINISHED_TEXT2 = 655395;

        internal const int STR_MENU_LANGUAGE = 655370;

        internal const int STR_MENU_LEADERBOARD_EDIT = 655420;

        internal const int STR_MENU_LEADERBOARD_NAME = 655418;

        internal const int STR_MENU_LEADERBOARD_NO_DATA = 655421;

        internal const int STR_MENU_LEADERBOARD_SCORE = 655419;

        internal const int STR_MENU_LEVEL = 655376;

        internal const int STR_MENU_LEVEL_CLEARED1 = 655372;

        internal const int STR_MENU_LEVEL_CLEARED2 = 655373;

        internal const int STR_MENU_LEVEL_CLEARED3 = 655374;

        internal const int STR_MENU_LEVEL_CLEARED4 = 655375;

        internal const int STR_MENU_LEVEL_SELECT = 655399;

        internal const int STR_MENU_LOADING = 655387;

        internal const int STR_MENU_MAIN_MENU = 655400;

        internal const int STR_MENU_MENU = 655386;

        internal const int STR_MENU_MUSIC_OFF = 655367;

        internal const int STR_MENU_NEXT = 655385;

        internal const int STR_MENU_NO = 655383;

        internal const int STR_MENU_OK = 655389;

        internal const int STR_MENU_OPTIONS = 655361;

        internal const int STR_MENU_PLAY = 655360;

        internal const int STR_MENU_PROCESSING = 655425;

        internal const int STR_MENU_RATEME_TEXT = 655402;

        internal const int STR_MENU_RATEME_TITLE = 655401;

        internal const int STR_MENU_REPLAY = 655384;

        internal const int STR_MENU_RESET = 655364;

        internal const int STR_MENU_RESET_TEXT = 655371;

        internal const int STR_MENU_SCORE = 655381;

        internal const int STR_MENU_SKIP_LEVEL = 655398;

        internal const int STR_MENU_SOUNDS_OFF = 655369;

        internal const int STR_MENU_SOUNDS_ON = 655368;

        internal const int STR_MENU_STAR_BONUS = 655378;

        internal const int STR_MENU_TIME = 655377;

        internal const int STR_MENU_TOTAL_STARS = 655388;

        internal const int STR_MENU_UNLOCK_HINT = 655390;

        internal const int STR_MENU_YES = 655382;

        internal const int IMG_MENU_BGR_bgr_default = 0;

        internal const int IMG_MENU_BGR_bgr_main_menu = 1;

        internal const int IMG_MENU_POPUP_popup = 0;

        internal const int IMG_MENU_POPUP_text_1 = 1;

        internal const int IMG_MENU_POPUP_text_2 = 2;

        internal const int IMG_MENU_POPUP_text_3 = 3;

        internal const int IMG_MENU_POPUP_button = 4;

        internal const int IMG_MENU_POPUP_stars_value = 5;

        internal const int IMG_MENU_LOGO_logo = 0;

        internal const int IMG_MENU_LOGO_zeptolab_logo = 1;

        internal const int IMG_MENU_LOGO_sm_logo = 2;

        internal const int IMG_MENU_LEVEL_SELECTION_level_playable = 0;

        internal const int IMG_MENU_LEVEL_SELECTION_level_locked = 1;

        internal const int IMG_MENU_LEVEL_SELECTION_stars_0 = 2;

        internal const int IMG_MENU_LEVEL_SELECTION_stars_1 = 3;

        internal const int IMG_MENU_LEVEL_SELECTION_stars_2 = 4;

        internal const int IMG_MENU_LEVEL_SELECTION_stars_3 = 5;

        internal const int IMG_MENU_PACK_SELECTION_box_back_01 = 0;

        internal const int IMG_MENU_PACK_SELECTION_monster_01 = 1;

        internal const int IMG_MENU_PACK_SELECTION_lock = 2;

        internal const int IMG_MENU_PACK_SELECTION_star = 3;

        internal const int IMG_MENU_PACK_SELECTION_box_01 = 4;

        internal const int IMG_MENU_PACK_SELECTION_box_02 = 5;

        internal const int IMG_MENU_PACK_SELECTION_box_03 = 6;

        internal const int IMG_MENU_PACK_SELECTION_box_04_ = 7;

        internal const int IMG_MENU_PACK_SELECTION_box_05 = 8;

        internal const int IMG_MENU_PACK_SELECTION_box_06 = 9;

        internal const int IMG_MENU_PACK_SELECTION_box_07 = 10;

        internal const int IMG_MENU_PACK_SELECTION_slon_down = 11;

        internal const int IMG_MENU_PACK_SELECTION_slot_up = 12;

        internal const int IMG_MENU_PACK_SELECTION_arrow_left = 13;

        internal const int IMG_MENU_PACK_SELECTION_arrow_left_actve = 14;

        internal const int IMG_MENU_PACK_SELECTION2_box_07 = 0;

        internal const int IMG_MENU_PACK_SELECTION2_box_08 = 1;

        internal const int IMG_MENU_PACK_SELECTION2_box_09 = 2;

        internal const int IMG_MENU_PACK_SELECTION2_box_10 = 3;

        internal const int IMG_MENU_PACK_SELECTION2_box_11 = 4;

        internal const int IMG_MENU_PACK_SELECTION2_box_soon = 5;

        internal const int IMG_MENU_EXTRA_BUTTONS_back_idle = 0;

        internal const int IMG_MENU_EXTRA_BUTTONS_back_pressed = 1;

        internal const int IMG_MENU_EXTRA_BUTTONS_button_facebook = 2;

        internal const int IMG_MENU_EXTRA_BUTTONS_button_twitter = 3;

        internal const int IMG_MENU_EXTRA_BUTTONS_lang_ru = 4;

        internal const int IMG_MENU_EXTRA_BUTTONS_lang_de = 5;

        internal const int IMG_MENU_EXTRA_BUTTONS_lang_fr = 6;

        internal const int IMG_MENU_EXTRA_BUTTONS_lang_en = 7;

        internal const int IMG_MENU_EXTRA_BUTTONS_lang_ja = 8;

        internal const int IMG_MENU_EXTRA_BUTTONS_lang_zh = 9;

        internal const int IMG_MENU_EXTRA_BUTTONS_lang_ko = 10;

        internal const int IMG_MENU_EXTRA_BUTTONS_lang_es = 11;

        internal const int IMG_MENU_EXTRA_BUTTONS_lang_br = 12;

        internal const int IMG_MENU_EXTRA_BUTTONS_lang_it = 13;

        internal const int IMG_MENU_EXTRA_BUTTONS_lang_nl = 14;

        internal const int IMG_MENU_SCROLLBAR_scroll_mini = 0;

        internal const int IMG_MENU_SCROLLBAR_scroll_big = 1;

        internal const int IMG_MENU_SCROLLBAR_candy_idle = 2;

        internal const int IMG_MENU_SCROLLBAR_candy_pressed = 3;

        internal const int IMG_MENU_LEADERBOARD_edit_button = 0;

        internal const int IMG_MENU_LEADERBOARD_place = 1;

        internal const int IMG_MENU_LEADERBOARD__scrollbar = 2;

        internal const int IMG_MENU_LEADERBOARD__place = 3;

        internal const int IMG_MENU_LEADERBOARD__number = 4;

        internal const int IMG_MENU_LEADERBOARD___block_spacing = 5;

        internal const int IMG_MENU_LEADERBOARD__name = 6;

        internal const int IMG_MENU_LEADERBOARD__center_title_box = 7;

        internal const int IMG_MENU_LEADERBOARD__center_control = 8;

        internal const int IMG_MENU_LEADERBOARD__title_name = 9;

        internal const int IMG_MENU_LEADERBOARD__title_score = 10;

        internal const int IMG_MENU_LEADERBOARD__score = 11;

        internal const int IMG_MENU_LEADERBOARD_title_decor = 12;

        internal const int IMG_MENU_LEADERBOARD__container = 13;

        internal const int IMG_MENU_LEADERBOARD__container_size = 14;

        internal const int IMG_MENU_PROCESSING_candy = 0;

        internal const int IMG_MENU_SCROLLBAR_CHANGENAME_back = 0;

        internal const int IMG_MENU_SCROLLBAR_CHANGENAME_cursor = 1;

        internal const int IMG_MENU_SCROLLBAR_CHANGENAME_short_pressed = 2;

        internal const int IMG_MENU_SCROLLBAR_CHANGENAME_short_idle = 3;

        internal const int IMG_MENU_SCROLLBAR_CHANGENAME__short_pressed2 = 4;

        internal const int IMG_MENU_SCROLLBAR_CHANGENAME__short_idle2 = 5;

        internal const int IMG_MENU_SCROLLBAR_CHANGENAME__name = 6;

        internal const int IMG_MENU_SCROLLBAR_CHANGENAME__description = 7;

        internal const int IMG_MENU_SCROLLBAR_CHANGENAME__title = 8;

        internal const int IMG_MENU_SCROLLBAR_CHANGENAME__cancel_button = 9;

        internal const int IMG_MENU_SCROLLBAR_CHANGENAME__accept_button = 10;

        internal const int IMG_MENU_SCROLLBAR_CHANGENAME__container_start = 11;

        internal const int IMG_MENU_SCROLLBAR_CHANGENAME__container_end = 12;

        internal const int IMG_MENU_BUTTON_ACHIV_CUP_button_idle = 0;

        internal const int IMG_MENU_BUTTON_ACHIV_CUP_button_pressed = 1;

        internal const int IMG_MENU_BUTTON_ACHIV_CUP_stars = 2;

        internal const int IMG_MENU_BUTTON_ACHIV_CUP_cup = 3;

        internal const int IMG_MENU_BGR_SHADOW_bgr_shadow = 0;

        internal const int IMG_MENU_BUTTON_SHORT_short_pressed = 0;

        internal const int IMG_MENU_BUTTON_SHORT_short_idle = 1;

        internal const int IMG_HUD_BUTTONS_restart = 0;

        internal const int IMG_HUD_BUTTONS_restart_touch = 1;

        internal const int IMG_OBJ_CANDY_01_candy_bottom = 0;

        internal const int IMG_OBJ_CANDY_01_candy_main = 1;

        internal const int IMG_OBJ_CANDY_01_candy_top = 2;

        internal const int IMG_OBJ_CANDY_01_piece_01 = 3;

        internal const int IMG_OBJ_CANDY_01_piece_02 = 4;

        internal const int IMG_OBJ_CANDY_01_piece_03 = 5;

        internal const int IMG_OBJ_CANDY_01_piece_04 = 6;

        internal const int IMG_OBJ_CANDY_01_piece_05 = 7;

        internal const int IMG_OBJ_CANDY_01_highlight_start = 8;

        internal const int IMG_OBJ_CANDY_01_highlight_end = 17;

        internal const int IMG_OBJ_CANDY_01_glow = 18;

        internal const int IMG_OBJ_CANDY_01_part_1 = 19;

        internal const int IMG_OBJ_CANDY_01_part_2 = 20;

        internal const int IMG_OBJ_CANDY_01_part_fx_start = 21;

        internal const int IMG_OBJ_CANDY_01_part_fx_end = 25;

        internal const int IMG_OBJ_SPIDER_activation_start = 0;

        internal const int IMG_OBJ_SPIDER_activation_end = 6;

        internal const int IMG_OBJ_SPIDER_crawl_start = 7;

        internal const int IMG_OBJ_SPIDER_crawl_end = 10;

        internal const int IMG_OBJ_SPIDER_busted = 11;

        internal const int IMG_OBJ_SPIDER_stealing = 12;

        internal const int IMG_CONFETTI_PARTICLES_particle_3_start = 0;

        internal const int IMG_CONFETTI_PARTICLES_particle_3_end = 8;

        internal const int IMG_CONFETTI_PARTICLES_particle_2_start = 9;

        internal const int IMG_CONFETTI_PARTICLES_particle_2_end = 17;

        internal const int IMG_CONFETTI_PARTICLES_particle_1_start = 18;

        internal const int IMG_CONFETTI_PARTICLES_particle_1_end = 26;

        internal const int IMG_MENU_PAUSE_screen_top = 0;

        internal const int IMG_MENU_RESULT_p_star_1 = 0;

        internal const int IMG_MENU_RESULT_p_star_2 = 1;

        internal const int IMG_MENU_RESULT_p_star_3 = 2;

        internal const int IMG_MENU_RESULT_p_title = 3;

        internal const int IMG_MENU_RESULT_p_line_1 = 4;

        internal const int IMG_MENU_RESULT_p_data = 5;

        internal const int IMG_MENU_RESULT_p_data_value = 6;

        internal const int IMG_MENU_RESULT_p_final = 7;

        internal const int IMG_MENU_RESULT_p_score_value = 8;

        internal const int IMG_MENU_RESULT_p_button_3 = 9;

        internal const int IMG_MENU_RESULT_p_button_2 = 10;

        internal const int IMG_MENU_RESULT_p_button_1 = 11;

        internal const int IMG_MENU_RESULT_p_stamp = 12;

        internal const int IMG_MENU_RESULT_star = 13;

        internal const int IMG_MENU_RESULT_star_empty = 14;

        internal const int IMG_MENU_RESULT_line = 15;

        internal const int IMG_MENU_RESULT_dark_area = 16;

        internal const int IMG_HUD_BUTTONS_EN_menu = 0;

        internal const int IMG_HUD_BUTTONS_EN_menu_touch = 1;

        internal const int IMG_MENU_RESULT_EN_en = 0;

        internal const int IMG_OBJ_STAR_DISAPPEAR_Frame_1 = 0;

        internal const int IMG_OBJ_STAR_DISAPPEAR_Frame_2 = 1;

        internal const int IMG_OBJ_STAR_DISAPPEAR_Frame_3 = 2;

        internal const int IMG_OBJ_STAR_DISAPPEAR_Frame_4 = 3;

        internal const int IMG_OBJ_STAR_DISAPPEAR_Frame_5 = 4;

        internal const int IMG_OBJ_STAR_DISAPPEAR_Frame_6 = 5;

        internal const int IMG_OBJ_STAR_DISAPPEAR_Frame_7 = 6;

        internal const int IMG_OBJ_STAR_DISAPPEAR_Frame_8 = 7;

        internal const int IMG_OBJ_STAR_DISAPPEAR_Frame_9 = 8;

        internal const int IMG_OBJ_STAR_DISAPPEAR_Frame_10 = 9;

        internal const int IMG_OBJ_STAR_DISAPPEAR_Frame_11 = 10;

        internal const int IMG_OBJ_STAR_DISAPPEAR_Frame_12 = 11;

        internal const int IMG_OBJ_STAR_DISAPPEAR_Frame_13 = 12;

        internal const int IMG_OBJ_BUBBLE_FLIGHT_Frame_1 = 0;

        internal const int IMG_OBJ_BUBBLE_FLIGHT_Frame_2 = 1;

        internal const int IMG_OBJ_BUBBLE_FLIGHT_Frame_3 = 2;

        internal const int IMG_OBJ_BUBBLE_FLIGHT_Frame_4 = 3;

        internal const int IMG_OBJ_BUBBLE_FLIGHT_Frame_5 = 4;

        internal const int IMG_OBJ_BUBBLE_FLIGHT_Frame_6 = 5;

        internal const int IMG_OBJ_BUBBLE_FLIGHT_Frame_7 = 6;

        internal const int IMG_OBJ_BUBBLE_FLIGHT_Frame_8 = 7;

        internal const int IMG_OBJ_BUBBLE_FLIGHT_Frame_9 = 8;

        internal const int IMG_OBJ_BUBBLE_FLIGHT_Frame_10 = 9;

        internal const int IMG_OBJ_BUBBLE_FLIGHT_Frame_11 = 10;

        internal const int IMG_OBJ_BUBBLE_FLIGHT_Frame_12 = 11;

        internal const int IMG_OBJ_BUBBLE_FLIGHT_Frame_13 = 12;

        internal const int IMG_OBJ_BUBBLE_FLIGHT_Frame_14 = 13;

        internal const int IMG_OBJ_BUBBLE_POP_Frame_1 = 0;

        internal const int IMG_OBJ_BUBBLE_POP_Frame_2 = 1;

        internal const int IMG_OBJ_BUBBLE_POP_Frame_3 = 2;

        internal const int IMG_OBJ_BUBBLE_POP_Frame_4 = 3;

        internal const int IMG_OBJ_BUBBLE_POP_Frame_5 = 4;

        internal const int IMG_OBJ_BUBBLE_POP_Frame_6 = 5;

        internal const int IMG_OBJ_BUBBLE_POP_Frame_7 = 6;

        internal const int IMG_OBJ_BUBBLE_POP_Frame_8 = 7;

        internal const int IMG_OBJ_BUBBLE_POP_Frame_9 = 8;

        internal const int IMG_OBJ_BUBBLE_POP_Frame_10 = 9;

        internal const int IMG_OBJ_BUBBLE_POP_Frame_11 = 10;

        internal const int IMG_OBJ_BUBBLE_POP_Frame_12 = 11;

        internal const int IMG_OBJ_HOOK_AUTO_bottom = 0;

        internal const int IMG_OBJ_HOOK_AUTO_top = 1;

        internal const int IMG_OBJ_BUBBLE_ATTACHED_bubble = 0;

        internal const int IMG_OBJ_BUBBLE_ATTACHED_stain_01 = 1;

        internal const int IMG_OBJ_BUBBLE_ATTACHED_stain_02 = 2;

        internal const int IMG_OBJ_BUBBLE_ATTACHED_stain_03 = 3;

        internal const int IMG_OBJ_HOOK_01_bottom = 0;

        internal const int IMG_OBJ_HOOK_01_top = 1;

        internal const int IMG_OBJ_HOOK_02_bottom = 0;

        internal const int IMG_OBJ_HOOK_02_top = 1;

        internal const int IMG_OBJ_STAR_IDLE_glow = 0;

        internal const int IMG_OBJ_STAR_IDLE_idle_start = 1;

        internal const int IMG_OBJ_STAR_IDLE_idle_end = 18;

        internal const int IMG_OBJ_STAR_IDLE_timed_start = 19;

        internal const int IMG_OBJ_STAR_IDLE_timed_end = 55;

        internal const int IMG_OBJ_STAR_IDLE_gravity_down = 56;

        internal const int IMG_OBJ_STAR_IDLE_gravity_up = 57;

        internal const int IMG_OBJ_STAR_IDLE_window = 58;

        internal const int IMG_HUD_STAR_Frame_1 = 0;

        internal const int IMG_HUD_STAR_Frame_2 = 1;

        internal const int IMG_HUD_STAR_Frame_3 = 2;

        internal const int IMG_HUD_STAR_Frame_4 = 3;

        internal const int IMG_HUD_STAR_Frame_5 = 4;

        internal const int IMG_HUD_STAR_Frame_6 = 5;

        internal const int IMG_HUD_STAR_Frame_7 = 6;

        internal const int IMG_HUD_STAR_Frame_8 = 7;

        internal const int IMG_HUD_STAR_Frame_9 = 8;

        internal const int IMG_HUD_STAR_Frame_10 = 9;

        internal const int IMG_HUD_STAR_Frame_11 = 10;

        internal const int IMG_CHAR_ANIMATIONS_idle_start = 0;

        internal const int IMG_CHAR_ANIMATIONS_idle_end = 18;

        internal const int IMG_CHAR_ANIMATIONS_mouth_open_start = 19;

        internal const int IMG_CHAR_ANIMATIONS_mouth_open_end = 27;

        internal const int IMG_CHAR_ANIMATIONS_mouth_close_start = 28;

        internal const int IMG_CHAR_ANIMATIONS_mouth_close_end = 31;

        internal const int IMG_CHAR_ANIMATIONS_chew_start = 32;

        internal const int IMG_CHAR_ANIMATIONS_chew_end = 40;

        internal const int IMG_CHAR_ANIMATIONS_blink_start = 41;

        internal const int IMG_CHAR_ANIMATIONS_blink_end = 42;

        internal const int IMG_CHAR_ANIMATIONS_idle2_start = 43;

        internal const int IMG_CHAR_ANIMATIONS_idle2_end = 67;

        internal const int IMG_CHAR_ANIMATIONS_idle3_start = 68;

        internal const int IMG_CHAR_ANIMATIONS_idle3_end = 83;

        internal const int IMG_OBJ_HOOK_REGULATED_bottom = 0;

        internal const int IMG_OBJ_HOOK_REGULATED_rope = 1;

        internal const int IMG_OBJ_HOOK_REGULATED_active = 2;

        internal const int IMG_OBJ_HOOK_REGULATED_top = 3;

        internal const int IMG_OBJ_HOOK_MOVABLE_bottom_tile_left = 0;

        internal const int IMG_OBJ_HOOK_MOVABLE_bottom_tile_right = 1;

        internal const int IMG_OBJ_HOOK_MOVABLE_bottom_tile_middle = 2;

        internal const int IMG_OBJ_HOOK_MOVABLE_active = 3;

        internal const int IMG_OBJ_HOOK_MOVABLE_top = 4;

        internal const int IMG_OBJ_PUMP_pump_start = 0;

        internal const int IMG_OBJ_PUMP_pump_end = 5;

        internal const int IMG_OBJ_PUMP_particle_1 = 6;

        internal const int IMG_OBJ_PUMP_particle_2 = 7;

        internal const int IMG_OBJ_PUMP_particle_3 = 8;

        internal const int IMG_TUTORIAL_SIGNS_cut_line = 0;

        internal const int IMG_TUTORIAL_SIGNS_arrow_1 = 1;

        internal const int IMG_TUTORIAL_SIGNS_arrow_2 = 2;

        internal const int IMG_TUTORIAL_SIGNS_arrow_3 = 3;

        internal const int IMG_TUTORIAL_SIGNS_pop = 4;

        internal const int IMG_TUTORIAL_SIGNS_warning = 5;

        internal const int IMG_TUTORIAL_SIGNS_marks = 6;

        internal const int IMG_TUTORIAL_SIGNS_reset = 7;

        internal const int IMG_TUTORIAL_SIGNS_tip = 8;

        internal const int IMG_TUTORIAL_SIGNS_finger = 9;

        internal const int IMG_TUTORIAL_SIGNS_fingers = 10;

        internal const int IMG_OBJ_SOCKS_hat_01 = 0;

        internal const int IMG_OBJ_SOCKS_hat_02 = 1;

        internal const int IMG_OBJ_SOCKS_glow_start = 2;

        internal const int IMG_OBJ_SOCKS_level = 3;

        internal const int IMG_OBJ_SOCKS_glow_end = 4;

        internal const int IMG_OBJ_BOUNCER_01_start = 0;

        internal const int IMG_OBJ_BOUNCER_01_Frame_2 = 1;

        internal const int IMG_OBJ_BOUNCER_01_Frame_3 = 2;

        internal const int IMG_OBJ_BOUNCER_01_Frame_4 = 3;

        internal const int IMG_OBJ_BOUNCER_01_end = 4;

        internal const int IMG_OBJ_BOUNCER_02_start_ = 0;

        internal const int IMG_OBJ_BOUNCER_02_Frame_2 = 1;

        internal const int IMG_OBJ_BOUNCER_02_Frame_3 = 2;

        internal const int IMG_OBJ_BOUNCER_02_Frame_4 = 3;

        internal const int IMG_OBJ_BOUNCER_02_end = 4;

        internal const int IMG_OBJ_SPIKES_01_size_1 = 0;

        internal const int IMG_OBJ_SPIKES_02_size_2 = 0;

        internal const int IMG_OBJ_SPIKES_03_size_3 = 0;

        internal const int IMG_OBJ_SPIKES_04_size_4 = 0;

        internal const int IMG_OBJ_ELECTRODES_base = 0;

        internal const int IMG_OBJ_ELECTRODES_electric_start = 1;

        internal const int IMG_OBJ_ELECTRODES_electric_end = 4;

        internal const int IMG_OBJ_ROTATABLE_SPIKES_01_Shape_3 = 0;

        internal const int IMG_OBJ_ROTATABLE_SPIKES_02_size_2 = 0;

        internal const int IMG_OBJ_ROTATABLE_SPIKES_03_size_3 = 0;

        internal const int IMG_OBJ_ROTATABLE_SPIKES_04_size_4 = 0;

        internal const int IMG_OBJ_ROTATABLE_SPIKES_BUTTON_button_1 = 0;

        internal const int IMG_OBJ_ROTATABLE_SPIKES_BUTTON_button_1_pressed = 1;

        internal const int IMG_OBJ_ROTATABLE_SPIKES_BUTTON_button_2 = 2;

        internal const int IMG_OBJ_ROTATABLE_SPIKES_BUTTON_button_2_pressed = 3;

        internal const int IMG_OBJ_BEE_HD__rotation_center = 0;

        internal const int IMG_OBJ_BEE_HD_obj_bee = 1;

        internal const int IMG_OBJ_BEE_HD_wings_start = 2;

        internal const int IMG_OBJ_BEE_HD_wings_end = 4;

        internal const int IMG_OBJ_POLLEN_HD_obj_pollen = 0;

        internal const int IMG_CHAR_SUPPORTS_support_01 = 0;

        internal const int IMG_CHAR_SUPPORTS_support_02 = 1;

        internal const int IMG_CHAR_SUPPORTS_support_03 = 2;

        internal const int IMG_CHAR_SUPPORTS_support_07 = 3;

        internal const int IMG_CHAR_SUPPORTS_support_06 = 4;

        internal const int IMG_CHAR_SUPPORTS_support_08 = 5;

        internal const int IMG_CHAR_SUPPORTS_support_04 = 6;

        internal const int IMG_CHAR_SUPPORTS_support_05 = 7;

        internal const int IMG_CHAR_SUPPORTS_support_09 = 8;

        internal const int IMG_CHAR_SUPPORTS_support_10 = 9;

        internal const int IMG_CHAR_SUPPORTS_support_11 = 10;

        internal const int IMG_CHAR_ANIMATIONS2_excited_start = 0;

        internal const int IMG_CHAR_ANIMATIONS2_excited_end = 19;

        internal const int IMG_CHAR_ANIMATIONS2_puzzled_start = 20;

        internal const int IMG_CHAR_ANIMATIONS2_puzzled_end = 46;

        internal const int IMG_CHAR_ANIMATIONS2_greeting_start = 47;

        internal const int IMG_CHAR_ANIMATIONS2_greeting_end = 76;

        internal const int IMG_CHAR_ANIMATIONS3_fail_start = 0;

        internal const int IMG_CHAR_ANIMATIONS3_fail_end = 12;

        internal const int IMG_OBJ_VINIL_obj_vinil = 0;

        internal const int IMG_OBJ_VINIL_obj_vinil_highlight = 1;

        internal const int IMG_OBJ_VINIL_odj_vinil_sticker = 2;

        internal const int IMG_OBJ_VINIL_obj_vinil_center = 3;

        internal const int IMG_OBJ_VINIL_obj_controller_active = 4;

        internal const int IMG_OBJ_VINIL_obj_controller = 5;

        internal const int IMG_BGR_01_P1_bgr = 0;

        internal const int IMG_BGR_01_P2_vert_transition = 0;

        internal const int IMG_BGR_02_P1_bgr = 0;

        internal const int IMG_BGR_02_P2_vert_transition = 0;

        internal const int IMG_BGR_03_P1_bgr = 0;

        internal const int IMG_BGR_03_P2_vert_transition = 0;

        internal const int IMG_BGR_04_P1_bgr = 0;

        internal const int IMG_BGR_04_P2_vert_transition = 0;

        internal const int IMG_BGR_05_P1_bgr = 0;

        internal const int IMG_BGR_05_P2_vert_transition = 0;

        internal const int IMG_BGR_06_P1_bgr = 0;

        internal const int IMG_BGR_06_P2_vert_transition = 0;

        internal const int IMG_BGR_07_P1_bgr = 0;

        internal const int IMG_BGR_07_P2_vert_transition = 0;

        internal const int IMG_BGR_08_P1_bgr = 0;

        internal const int IMG_BGR_08_P1__position_window = 1;

        internal const int IMG_BGR_08_P2_vert_transition = 0;

        internal const int IMG_BGR_08_P2__position_window = 1;

        internal const int IMG_BGR_09_P1_bgr = 0;

        internal const int IMG_BGR_09_P2_vert_transition = 0;

        internal const int IMG_BGR_10_P1_bgr = 0;

        internal const int IMG_BGR_10_P2_vert_transition = 0;

        internal const int IMG_BGR_11_P1_bgr = 0;

        internal const int IMG_BGR_11_P2_vert_transition = 0;

        internal const int IMG_BGR_COVER_01_bgr = 0;

        internal const int IMG_BGR_COVER_01_side = 1;

        internal const int IMG_BGR_COVER_02_top = 0;

        internal const int IMG_BGR_COVER_02_side = 1;

        internal const int IMG_BGR_COVER_03_top = 0;

        internal const int IMG_BGR_COVER_03_side = 1;

        internal const int IMG_BGR_COVER_04_bgr = 0;

        internal const int IMG_BGR_COVER_04_side = 1;

        internal const int IMG_BGR_COVER_05_top = 0;

        internal const int IMG_BGR_COVER_05_side = 1;

        internal const int IMG_BGR_COVER_06_top = 0;

        internal const int IMG_BGR_COVER_06_side = 1;

        internal const int IMG_BGR_COVER_07_top = 0;

        internal const int IMG_BGR_COVER_07_side = 1;

        internal const int IMG_BGR_COVER_08_top = 0;

        internal const int IMG_BGR_COVER_08_side = 1;

        internal const int IMG_BGR_COVER_09_top = 0;

        internal const int IMG_BGR_COVER_09_side = 1;

        internal const int IMG_BGR_COVER_10_top = 0;

        internal const int IMG_BGR_COVER_10_side = 1;

        internal const int IMG_BGR_COVER_11_top = 0;

        internal const int IMG_BGR_COVER_11_side = 1;

        internal const int IMG_MENU_EXTRA_BUTTONS_FR_follow_us = 0;

        internal const int IMG_MENU_EXTRA_BUTTONS_GR_follow_us = 0;

        internal const int IMG_MENU_EXTRA_BUTTONS_RU_follow_us = 0;

        internal const int IMG_HUD_BUTTONS_RU_menu = 0;

        internal const int IMG_HUD_BUTTONS_RU_menu_touch = 1;

        internal const int IMG_HUD_BUTTONS_GR_menu = 0;

        internal const int IMG_HUD_BUTTONS_GR_menu_touch = 1;

        internal const int IMG_MENU_RESULT_RU_stamp_ru = 0;

        internal const int IMG_MENU_RESULT_FR_stamp = 0;

        internal const int IMG_MENU_RESULT_GR_stamp_gr = 0;

        internal const int IMG_MENU_EXTRA_BUTTONS_EN_en = 0;

        public static string ContentFolder = "";

        internal static int[] PACK_STARTUP = [0, 1, -1];

        internal static int[] PACK_COMMON_IMAGES = [2, 3, 4, 5, 6, 7, 8, -1];

        internal static int[] PACK_COMMON =
[
    9, 10, 11, 12, 13, 14, 15, 16, 17, 18,
            19, 20, 21, 22, 23, 24, 25, 26, 27, 28,
            29, 30, 31, 32, 33, 34, 35, 36, 37, 38,
            39, 40, 41, 42, 43, 44, 45, 46, 47, -1
];

        internal static int[] PACK_MENU =
[
    48, 49, 50, 51, 52, 53, 54, 55, 56, 57,
            58, 59, 60, -1
];

        internal static int[] PACK_GAME =
[
    61, 62, 63, 64, 65, 66, 67, 68, 69, 70,
            -1
];

        internal static int[] PACK_GAME_NORMAL =
[
    71, 72, 73, 74, 75, 76, 77, 78, 79, 80,
            81, 82, 83, 84, 85, 86, 87, 88, 89, 90,
            91, 92, 93, 94, 95, 96, 97, 98, 99, 100,
            101, 102, 103, -1
];

        internal static int[] PACK_GAME_01 = [104, 105, -1];

        internal static int[] PACK_GAME_02 = [106, 107, -1];

        internal static int[] PACK_GAME_03 = [108, 109, -1];

        internal static int[] PACK_GAME_04 = [110, 111, -1];

        internal static int[] PACK_GAME_05 = [112, 113, -1];

        internal static int[] PACK_GAME_06 = [114, 115, -1];

        internal static int[] PACK_GAME_07 = [116, 117, -1];

        internal static int[] PACK_GAME_08 = [118, 119, -1];

        internal static int[] PACK_GAME_09 = [120, 121, -1];

        internal static int[] PACK_GAME_10 = [122, 123, -1];

        internal static int[] PACK_GAME_11 = [124, 125, -1];

        internal static int[] PACK_GAME_COVER_01 = [126, -1];

        internal static int[] PACK_GAME_COVER_02 = [127, -1];

        internal static int[] PACK_GAME_COVER_03 = [128, -1];

        internal static int[] PACK_GAME_COVER_04 = [129, -1];

        internal static int[] PACK_GAME_COVER_05 = [130, -1];

        internal static int[] PACK_GAME_COVER_06 = [131, -1];

        internal static int[] PACK_GAME_COVER_07 = [132, -1];

        internal static int[] PACK_GAME_COVER_08 = [133, -1];

        internal static int[] PACK_GAME_COVER_09 = [134, -1];

        internal static int[] PACK_GAME_COVER_10 = [135, -1];

        internal static int[] PACK_GAME_COVER_11 = [136, -1];

        internal static int[] PACK_LOCALIZATION = [137, 138, 139, 140, 141, 142, 143, 144, -1];

        internal static int[] PACK_MUSIC = [145, 146, 147, 148, -1];

        internal static int[] PACK_LOCALIZATION_MENU = [149, -1];

        public static Language LANGUAGE = Language.LANGEN;

        private static readonly Lock xmlsLock_ = new();

        private static Dictionary<string, string> xmls_;
    }
}
