using CutTheRope.game;
using CutTheRope.ios;
using System;
using System.Collections.Generic;

namespace CutTheRope.iframework.core
{
    internal class ApplicationSettings : NSObject
    {
        public virtual int getInt(int s)
        {
            if (s == 5)
            {
                return fps;
            }
            if (s != 6)
            {
                throw new NotImplementedException();
            }
            return (int)orientation;
        }

        public virtual bool getBool(int s)
        {
            bool value = false;
            DEFAULT_APP_SETTINGS.TryGetValue((AppSettings)s, out value);
            return value;
        }

        public virtual NSString getString(int s)
        {
            if (s != 8)
            {
                return NSS("");
            }
            if (locale != null)
            {
                return NSS(locale);
            }
            switch (LANGUAGE)
            {
                case Language.LANG_EN:
                    return NSS("en");
                case Language.LANG_RU:
                    return NSS("ru");
                case Language.LANG_DE:
                    return NSS("de");
                case Language.LANG_FR:
                    return NSS("fr");
                case Language.LANG_ZH:
                    return NSS("zh");
                case Language.LANG_JA:
                    return NSS("ja");
                default:
                    return NSS("en");
            }
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

        private static int fps = 60;

        private ORIENTATION orientation;

        private string locale;

        private static Dictionary<AppSettings, bool> DEFAULT_APP_SETTINGS = new()
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
