using System;
using System.Collections.Generic;

namespace CutTheRope.iframework.core
{
    internal sealed class ApplicationSettings : FrameworkTypes
    {
        public int GetInt(int s)
        {
            return s == 5 ? fps : s != 6 ? throw new NotImplementedException() : (int)orientation;
        }

        public static bool GetBool(int s)
        {
            _ = DEFAULT_APP_SETTINGS.TryGetValue((AppSettings)s, out bool value);
            return value;
        }

        public string GetString(int s)
        {
            return s != 8
                ? ""
                : locale ?? LANGUAGE switch
                {
                    Language.LANGEN => "en",
                    Language.LANGRU => "ru",
                    Language.LANGDE => "de",
                    Language.LANGFR => "fr",
                    Language.LANGZH => "zh",
                    Language.LANGJA => "ja",
                    _ => "en",
                };
        }

        public void SetString(int sid, string str)
        {
            if (sid == 8)
            {
                locale = str.ToString();
                LANGUAGE = Language.LANGEN;
                if (locale == "ru")
                {
                    LANGUAGE = Language.LANGRU;
                }
                else if (locale == "de")
                {
                    LANGUAGE = Language.LANGDE;
                }
                if (locale == "fr")
                {
                    LANGUAGE = Language.LANGFR;
                }
            }
        }

        private static readonly int fps = 60;

        private readonly ORIENTATION orientation;

        private string locale;

        private static readonly Dictionary<AppSettings, bool> DEFAULT_APP_SETTINGS = new()
        {
            {
                AppSettings.APP_SETTING_INTERACTION_ENABLED,
                true
            },
            {
                AppSettings.APP_SETTING_MULTITOUCH_ENABLED,
                true
            },
            {
                AppSettings.APP_SETTING_STATUSBAR_HIDDEN,
                true
            },
            {
                AppSettings.APP_SETTING_MAIN_LOOP_TIMERED,
                true
            },
            {
                AppSettings.APP_SETTING_FPS_METER_ENABLED,
                true
            },
            {
                AppSettings.APP_SETTING_LOCALIZATION_ENABLED,
                true
            },
            {
                AppSettings.APP_SETTING_RETINA_SUPPORT,
                false
            },
            {
                AppSettings.APP_SETTING_IPAD_RETINA_SUPPORT,
                false
            }
        };

        public enum ORIENTATION
        {
            PORTRAIT,
            PORTRAIT_UPSIDE_DOWN,
            LANDSCAPE_LEFT,
            LANDSCAPE_RIGHT
        }

        public enum AppSettings
        {
            APP_SETTING_INTERACTION_ENABLED,
            APP_SETTING_MULTITOUCH_ENABLED,
            APP_SETTING_STATUSBAR_HIDDEN,
            APP_SETTING_MAIN_LOOP_TIMERED,
            APP_SETTING_FPS_METER_ENABLED,
            APP_SETTING_FPS,
            APP_SETTING_ORIENTATION,
            APP_SETTING_LOCALIZATION_ENABLED,
            APP_SETTING_LOCALE,
            APP_SETTING_RETINA_SUPPORT,
            APP_SETTING_IPAD_RETINA_SUPPORT,
            APP_SETTINGS_CUSTOM
        }
    }
}
