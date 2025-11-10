using CutTheRope.ios;
using System;
using System.Collections.Generic;

namespace CutTheRope.iframework.core
{
    internal class ApplicationSettings : NSObject
    {
        public virtual int getInt(int s)
        {
            return s == 5 ? fps : s != 6 ? throw new NotImplementedException() : (int)orientation;
        }

        public virtual bool getBool(int s)
        {
            bool value = false;
            DEFAULT_APP_SETTINGS.TryGetValue((AppSettings)s, out value);
            return value;
        }

        public virtual NSString getString(int s)
        {
            return s != 8
                ? NSS("")
                : locale != null
                ? NSS(locale)
                : LANGUAGE switch
                {
                    Language.LANG_EN => NSS("en"),
                    Language.LANG_RU => NSS("ru"),
                    Language.LANG_DE => NSS("de"),
                    Language.LANG_FR => NSS("fr"),
                    Language.LANG_ZH => NSS("zh"),
                    Language.LANG_JA => NSS("ja"),
                    _ => NSS("en"),
                };
        }

        public virtual void setString(int sid, NSString str)
        {
            if (sid == 8)
            {
                locale = str.ToString();
                LANGUAGE = Language.LANG_EN;
                if (locale == "ru")
                {
                    LANGUAGE = Language.LANG_RU;
                }
                else if (locale == "de")
                {
                    LANGUAGE = Language.LANG_DE;
                }
                if (locale == "fr")
                {
                    LANGUAGE = Language.LANG_FR;
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
            ORIENTATION_PORTRAIT,
            ORIENTATION_PORTRAIT_UPSIDE_DOWN,
            ORIENTATION_LANDSCAPE_LEFT,
            ORIENTATION_LANDSCAPE_RIGHT
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
