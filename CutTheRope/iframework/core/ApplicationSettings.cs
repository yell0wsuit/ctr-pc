using CutTheRope.game;
using CutTheRope.ios;
using System;
using System.Collections.Generic;

namespace CutTheRope.iframework.core
{
    // Token: 0x02000065 RID: 101
    internal class ApplicationSettings : NSObject
    {
        // Token: 0x060003B7 RID: 951 RVA: 0x00014C3A File Offset: 0x00012E3A
        public virtual int getInt(int s)
        {
            if (s == 5)
            {
                return ApplicationSettings.fps;
            }
            if (s != 6)
            {
                throw new NotImplementedException();
            }
            return (int)this.orientation;
        }

        // Token: 0x060003B8 RID: 952 RVA: 0x00014C58 File Offset: 0x00012E58
        public virtual bool getBool(int s)
        {
            bool value = false;
            ApplicationSettings.DEFAULT_APP_SETTINGS.TryGetValue((ApplicationSettings.AppSettings)s, out value);
            return value;
        }

        // Token: 0x060003B9 RID: 953 RVA: 0x00014C78 File Offset: 0x00012E78
        public virtual NSString getString(int s)
        {
            if (s != 8)
            {
                return NSObject.NSS("");
            }
            if (this.locale != null)
            {
                return NSObject.NSS(this.locale);
            }
            switch (ResDataPhoneFull.LANGUAGE)
            {
                case Language.LANG_EN:
                    return NSObject.NSS("en");
                case Language.LANG_RU:
                    return NSObject.NSS("ru");
                case Language.LANG_DE:
                    return NSObject.NSS("de");
                case Language.LANG_FR:
                    return NSObject.NSS("fr");
                case Language.LANG_ZH:
                    return NSObject.NSS("zh");
                case Language.LANG_JA:
                    return NSObject.NSS("ja");
                default:
                    return NSObject.NSS("en");
            }
        }

        // Token: 0x060003BA RID: 954 RVA: 0x00014D20 File Offset: 0x00012F20
        public virtual void setString(int sid, NSString str)
        {
            if (sid == 8)
            {
                this.locale = str.ToString();
                ResDataPhoneFull.LANGUAGE = Language.LANG_EN;
                if (this.locale == "ru")
                {
                    ResDataPhoneFull.LANGUAGE = Language.LANG_RU;
                }
                else if (this.locale == "de")
                {
                    ResDataPhoneFull.LANGUAGE = Language.LANG_DE;
                }
                if (this.locale == "fr")
                {
                    ResDataPhoneFull.LANGUAGE = Language.LANG_FR;
                }
            }
        }

        // Token: 0x040002A1 RID: 673
        private static int fps = 60;

        // Token: 0x040002A2 RID: 674
        private ApplicationSettings.ORIENTATION orientation;

        // Token: 0x040002A3 RID: 675
        private string locale;

        // Token: 0x040002A4 RID: 676
        private static Dictionary<ApplicationSettings.AppSettings, bool> DEFAULT_APP_SETTINGS = new()
        {
            {
                ApplicationSettings.AppSettings.APP_SETTING_INTERACTION_ENABLED,
                true
            },
            {
                ApplicationSettings.AppSettings.APP_SETTING_MULTITOUCH_ENABLED,
                true
            },
            {
                ApplicationSettings.AppSettings.APP_SETTING_STATUSBAR_HIDDEN,
                true
            },
            {
                ApplicationSettings.AppSettings.APP_SETTING_MAIN_LOOP_TIMERED,
                true
            },
            {
                ApplicationSettings.AppSettings.APP_SETTING_FPS_METER_ENABLED,
                true
            },
            {
                ApplicationSettings.AppSettings.APP_SETTING_LOCALIZATION_ENABLED,
                true
            },
            {
                ApplicationSettings.AppSettings.APP_SETTING_RETINA_SUPPORT,
                false
            },
            {
                ApplicationSettings.AppSettings.APP_SETTING_IPAD_RETINA_SUPPORT,
                false
            }
        };

        // Token: 0x020000BB RID: 187
        public enum ORIENTATION
        {
            // Token: 0x040008C9 RID: 2249
            ORIENTATION_PORTRAIT,
            // Token: 0x040008CA RID: 2250
            ORIENTATION_PORTRAIT_UPSIDE_DOWN,
            // Token: 0x040008CB RID: 2251
            ORIENTATION_LANDSCAPE_LEFT,
            // Token: 0x040008CC RID: 2252
            ORIENTATION_LANDSCAPE_RIGHT
        }

        // Token: 0x020000BC RID: 188
        public enum AppSettings
        {
            // Token: 0x040008CE RID: 2254
            APP_SETTING_INTERACTION_ENABLED,
            // Token: 0x040008CF RID: 2255
            APP_SETTING_MULTITOUCH_ENABLED,
            // Token: 0x040008D0 RID: 2256
            APP_SETTING_STATUSBAR_HIDDEN,
            // Token: 0x040008D1 RID: 2257
            APP_SETTING_MAIN_LOOP_TIMERED,
            // Token: 0x040008D2 RID: 2258
            APP_SETTING_FPS_METER_ENABLED,
            // Token: 0x040008D3 RID: 2259
            APP_SETTING_FPS,
            // Token: 0x040008D4 RID: 2260
            APP_SETTING_ORIENTATION,
            // Token: 0x040008D5 RID: 2261
            APP_SETTING_LOCALIZATION_ENABLED,
            // Token: 0x040008D6 RID: 2262
            APP_SETTING_LOCALE,
            // Token: 0x040008D7 RID: 2263
            APP_SETTING_RETINA_SUPPORT,
            // Token: 0x040008D8 RID: 2264
            APP_SETTING_IPAD_RETINA_SUPPORT,
            // Token: 0x040008D9 RID: 2265
            APP_SETTINGS_CUSTOM
        }
    }
}
