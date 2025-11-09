using CutTheRope.iframework;
using CutTheRope.iframework.core;
using CutTheRope.ios;
using System;
using System.Collections.Generic;

namespace CutTheRope.game
{
    internal class CTRResourceMgr : ResourceMgr
    {
        public override NSObject init()
        {
            base.init();
            return this;
        }

        public static int handleLocalizedResource(int r)
        {
            if (r != 69)
            {
                if (r != 70)
                {
                    if (r == 149)
                    {
                        if (LANGUAGE == Language.LANG_RU)
                        {
                            return 139;
                        }
                        if (LANGUAGE == Language.LANG_DE)
                        {
                            return 138;
                        }
                        if (LANGUAGE == Language.LANG_FR)
                        {
                            return 137;
                        }
                    }
                }
                else
                {
                    if (LANGUAGE == Language.LANG_RU)
                    {
                        return 142;
                    }
                    if (LANGUAGE == Language.LANG_DE)
                    {
                        return 144;
                    }
                    if (LANGUAGE == Language.LANG_FR)
                    {
                        return 143;
                    }
                }
            }
            else
            {
                if (LANGUAGE == Language.LANG_RU)
                {
                    return 140;
                }
                if (LANGUAGE == Language.LANG_DE)
                {
                    return 141;
                }
                if (LANGUAGE == Language.LANG_FR)
                {
                    return 69;
                }
            }
            return r;
        }

        public static string XNA_ResName(int resId)
        {
            if (resNames_ == null)
            {
                resNames_ = new Dictionary<int, string>
                {
                    { 0, "zeptolab_no_link" },
                    { 1, "loaderbar_full" },
                    { 2, "menu_button_default" },
                    { 3, "big_font" },
                    { 4, "small_font" },
                    { 5, "menu_loading" },
                    { 6, "menu_notification" },
                    { 7, "menu_achievement" },
                    { 8, "menu_options" },
                    { 9, "tap" },
                    { 10, "menu_strings" },
                    { 11, "button" },
                    { 12, "bubble_break" },
                    { 13, "bubble" },
                    { 14, "candy_break" },
                    { 15, "monster_chewing" },
                    { 16, "monster_close" },
                    { 17, "monster_open" },
                    { 18, "monster_sad" },
                    { 19, "ring" },
                    { 20, "rope_bleak_1" },
                    { 21, "rope_bleak_2" },
                    { 22, "rope_bleak_3" },
                    { 23, "rope_bleak_4" },
                    { 24, "rope_get" },
                    { 25, "star_1" },
                    { 26, "star_2" },
                    { 27, "star_3" },
                    { 28, "electric" },
                    { 29, "pump_1" },
                    { 30, "pump_2" },
                    { 31, "pump_3" },
                    { 32, "pump_4" },
                    { 33, "spider_activate" },
                    { 34, "spider_fall" },
                    { 35, "spider_win" },
                    { 36, "wheel" },
                    { 37, "win" },
                    { 38, "gravity_off" },
                    { 39, "gravity_on" },
                    { 40, "candy_link" },
                    { 41, "bouncer" },
                    { 42, "spike_rotate_in" },
                    { 43, "spike_rotate_out" },
                    { 44, "buzz" },
                    { 45, "teleport" },
                    { 46, "scratch_in" },
                    { 47, "scratch_out" },
                    { 48, "menu_bgr" },
                    { 49, "menu_popup" },
                    { 50, "menu_logo" },
                    { 51, "menu_level_selection" },
                    { 52, "menu_pack_selection" },
                    { 53, "menu_pack_selection2" },
                    { 54, "menu_extra_buttons" },
                    { 55, "menu_scrollbar" },
                    { 56, "menu_leaderboard" },
                    { 57, "menu_processing_hd" },
                    { 58, "menu_scrollbar_changename" },
                    { 59, "menu_button_achiv_cup" },
                    { 60, "menu_bgr_shadow" },
                    { 61, "menu_button_short" },
                    { 62, "hud_buttons" },
                    { 63, "obj_candy_01" },
                    { 64, "obj_spider" },
                    { 65, "confetti_particles" },
                    { 66, "menu_pause" },
                    { 67, "menu_result" },
                    { 68, "font_numbers_big" },
                    { 69, "hud_buttons_en" },
                    { 70, "menu_result_en" },
                    { 71, "obj_star_disappear" },
                    { 72, "obj_bubble_flight" },
                    { 73, "obj_bubble_pop" },
                    { 74, "obj_hook_auto" },
                    { 75, "obj_bubble_attached" },
                    { 76, "obj_hook_01" },
                    { 77, "obj_hook_02" },
                    { 78, "obj_star_idle" },
                    { 79, "hud_star" },
                    { 80, "char_animations" },
                    { 81, "obj_hook_regulated" },
                    { 82, "obj_hook_movable" },
                    { 83, "obj_pump" },
                    { 84, "tutorial_signs" },
                    { 85, "obj_hat" },
                    { 86, "obj_bouncer_01" },
                    { 87, "obj_bouncer_02" },
                    { 88, "obj_spikes_01" },
                    { 89, "obj_spikes_02" },
                    { 90, "obj_spikes_03" },
                    { 91, "obj_spikes_04" },
                    { 92, "obj_electrodes" },
                    { 93, "obj_rotatable_spikes_01" },
                    { 94, "obj_rotatable_spikes_02" },
                    { 95, "obj_rotatable_spikes_03" },
                    { 96, "obj_rotatable_spikes_04" },
                    { 97, "obj_rotatable_spikes_button" },
                    { 98, "obj_bee_hd" },
                    { 99, "obj_pollen_hd" },
                    { 100, "char_supports" },
                    { 101, "char_animations2" },
                    { 102, "char_animations3" },
                    { 103, "obj_vinil" },
                    { 104, "bgr_01_p1" },
                    { 105, "bgr_01_p2" },
                    { 106, "bgr_02_p1" },
                    { 107, "bgr_02_p2" },
                    { 108, "bgr_03_p1" },
                    { 109, "bgr_03_p2" },
                    { 110, "bgr_04_p1" },
                    { 111, "bgr_04_p2" },
                    { 112, "bgr_05_p1" },
                    { 113, "bgr_05_p2" },
                    { 114, "bgr_06_p1" },
                    { 115, "bgr_06_p2" },
                    { 116, "bgr_07_p1" },
                    { 117, "bgr_07_p2" },
                    { 118, "bgr_08_p1" },
                    { 119, "bgr_08_p2" },
                    { 120, "bgr_09_p1" },
                    { 121, "bgr_09_p2" },
                    { 122, "bgr_10_p1" },
                    { 123, "bgr_10_p2" },
                    { 124, "bgr_11_p1" },
                    { 125, "bgr_11_p2" },
                    { 126, "bgr_01_cover" },
                    { 127, "bgr_02_cover" },
                    { 128, "bgr_03_cover" },
                    { 129, "bgr_04_cover" },
                    { 130, "bgr_05_cover" },
                    { 131, "bgr_06_cover" },
                    { 132, "bgr_07_cover" },
                    { 133, "bgr_08_cover" },
                    { 134, "bgr_09_cover" },
                    { 135, "bgr_10_cover" },
                    { 136, "bgr_11_cover" },
                    { 137, "menu_extra_buttons_fr" },
                    { 138, "menu_extra_buttons_gr" },
                    { 139, "menu_extra_buttons_ru" },
                    { 140, "hud_buttons_ru" },
                    { 141, "hud_buttons_gr" },
                    { 142, "menu_result_ru" },
                    { 143, "menu_result_fr" },
                    { 144, "menu_result_gr" },
                    { 145, "menu_music" },
                    { 146, "game_music" },
                    { 147, "game_music2" },
                    { 148, "game_music3" },
                    { 149, "menu_extra_buttons_en" }
                };
            }
            string value;
            resNames_.TryGetValue(handleLocalizedResource(resId), out value);
            return value;
        }

        public override NSObject loadResource(int resID, ResourceType resType)
        {
            return base.loadResource(handleLocalizedResource(resID), resType);
        }

        public override void freeResource(int resID)
        {
            base.freeResource(handleLocalizedResource(resID));
        }

        private static Dictionary<int, string> resNames_;
    }
}
