using System;
using System.Collections.Generic;

namespace CutTheRope.Framework.Core
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

        /// <summary>
        /// Gets a string application setting.
        /// </summary>
        /// <param name="s">The setting identifier (see <see cref="AppSettings"/>).</param>
        /// <returns>The locale code if <c>s</c> is <see cref="AppSettings.APP_SETTING_LOCALE"/>, otherwise an empty string.</returns>
        public string GetString(int s)
        {
            return s != (int)AppSettings.APP_SETTING_LOCALE ? "" : locale ?? LanguageHelper.CurrentCode;
        }

        /// <summary>
        /// Sets a string application setting.
        /// </summary>
        /// <param name="sid">The setting identifier (see <see cref="AppSettings"/>).</param>
        /// <param name="str">The string value to set.</param>
        /// <remarks>
        /// Currently only <see cref="AppSettings.APP_SETTING_LOCALE"/> is supported.
        /// Setting the locale also updates <see cref="LanguageHelper.Current"/>.
        /// </remarks>
        public void SetString(int sid, string str)
        {
            if (sid == (int)AppSettings.APP_SETTING_LOCALE)
            {
                locale = str;
                LanguageHelper.Current = LanguageHelper.FromCode(locale);
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
